using Dalamud.Game.Gui;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ImGuiNET;

namespace BossMod;

public class DebugAction(WorldState ws) : IDisposable
{
    private int _customAction;
    private readonly UITree _tree = new();

    public void Dispose()
    {
    }

    public unsafe void DrawActionManagerExtensions()
    {
        var am = ActionManagerEx.Instance!;
        var amr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        var aidCastAction = am.CastAction;
        var aidCastSpell = am.CastSpell;
        var aidCombo = new ActionID(ActionType.Spell, amr->Combo.Action);
        var aidQueued = am.QueuedAction;
        var aidGTAction = new ActionID((ActionType)amr->AreaTargetingActionType, amr->AreaTargetingActionId);
        var aidGTSpell = new ActionID(ActionType.Spell, amr->AreaTargetingSpellId);
        ImGui.TextUnformatted($"ActionManager singleton address: 0x{(ulong)amr:X}");
        ImGui.TextUnformatted($"Anim lock: {amr->AnimationLock:f3}");
        ImGui.TextUnformatted($"Cast: {aidCastAction} / {aidCastSpell}, progress={amr->CastTimeElapsed:f3}/{amr->CastTimeTotal:f3}, target={amr->CastTargetId:X}/{Utils.Vec3String(amr->CastTargetPosition)}");
        ImGui.TextUnformatted($"Combo: {aidCombo}, {am.ComboTimeLeft:f3}");
        ImGui.TextUnformatted($"Queue: {(amr->ActionQueued ? "active" : "inactive")}, {aidQueued} @ {(ulong)amr->QueuedTargetId:X} [{amr->QueueType}], combo={amr->QueuedComboRouteId}");
        ImGui.TextUnformatted($"GT: {aidGTAction} / {aidGTSpell}, arg={Utils.ReadField<uint>(amr, 0x94)}, targ={amr->AreaTargetingExecuteAtObject:X}/{amr->AreaTargetingExecuteAtCursor}, a0={Utils.ReadField<byte>(amr, 0xA0):X2}, bc={Utils.ReadField<byte>(amr, 0xBC):X}");
        ImGui.TextUnformatted($"Last used action sequence: {amr->LastUsedActionSequence}");
        //ImGui.TextUnformatted($"GT config: 298={Framework.Instance()->SystemConfig.GetConfigOption(298)->Value.UInt}");
        if (ImGui.Button("GT complete"))
        {
            amr->AreaTargetingExecuteAtCursor = true;
        }
        ImGui.SameLine();
        if (ImGui.Button("GT set target"))
        {
            var target = TargetSystem.Instance()->Target;
            amr->AreaTargetingExecuteAtObject = target != null ? target->GetGameObjectId() : 0xE0000000;
        }

        if (ImGui.Button("Rotate 30 CCW"))
        {
            am.FaceDirection((ws.Party.Player()?.Rotation ?? 0.Degrees() + 30.Degrees()).ToDirection());
        }
    }

    public unsafe void DrawActionData()
    {
        ImGui.InputInt("Action to show details for", ref _customAction);
        if (_customAction != 0)
        {
            var data = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow((uint)_customAction);
            if (data != null)
            {
                ImGui.TextUnformatted($"Name: {data.Name}");
                ImGui.TextUnformatted($"Cast time: {data.Cast100ms * 0.1f:f1} + {data.Unknown38 * 0.1f:f1}");
                ImGui.TextUnformatted($"Range: {data.Range}");
                ImGui.TextUnformatted($"Effect range: {data.EffectRange}");
                ImGui.TextUnformatted($"Cooldown group: {data.CooldownGroup}");
                ImGui.TextUnformatted($"Max charges: {data.MaxCharges}");
                ImGui.TextUnformatted($"Category: {data.ActionCategory.Row} ({data.ActionCategory.Value?.Name})");
            }
        }

        var mgr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        var hover = Service.GameGui.HoveredAction;
        if (hover.ActionID != 0)
        {
            var mnemonic = Service.ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation.ToString();
            var rotationType = mnemonic != null ? Type.GetType($"BossMod.{mnemonic}Rotation")?.GetNestedType("AID") : null;
            ImGui.TextUnformatted($"Hover action: {hover.ActionKind} {hover.ActionID} (base={hover.BaseActionID}) ({mnemonic}: {rotationType?.GetEnumName(hover.ActionID)})");

            Lumina.Text.SeString? name = null;
            var type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.None;
            uint unlockLink = 0;
            if ((int)hover.ActionKind == 24) // action
            {
                var data = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(hover.ActionID);
                name = data?.Name;
                type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action;
                unlockLink = data?.UnlockLink ?? 0;
            }
            else if (hover.ActionKind == HoverActionKind.GeneralAction)
            {
                var data = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GeneralAction>()?.GetRow(hover.ActionID);
                name = data?.Name;
                type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.GeneralAction;
                unlockLink = data?.UnlockLink ?? 0;
            }
            else if (hover.ActionKind == HoverActionKind.Trait)
            {
                var data = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Trait>()?.GetRow(hover.ActionID);
                name = data?.Name;
                unlockLink = data?.Quest.Row ?? 0;
            }

            ImGui.TextUnformatted($"Name: {name}");
            ImGui.TextUnformatted($"Unlock: {unlockLink} ({Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Quest>()?.GetRow(unlockLink)?.Name}) = {FFXIVClientStructs.FFXIV.Client.Game.QuestManager.IsQuestComplete(unlockLink)}");
            if (hover.ActionKind == HoverActionKind.Action)
            {
                ImGui.TextUnformatted($"Range: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(hover.ActionID)}");
                ImGui.TextUnformatted($"Stacks: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(hover.ActionID, 0)}");
                ImGui.TextUnformatted($"Adjusted ID: {mgr->GetAdjustedActionId(hover.ActionID)}");
            }

            if (type != FFXIVClientStructs.FFXIV.Client.Game.ActionType.None)
            {
                //ImGui.TextUnformatted($"Cost: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionCost(type, hover.ActionID, 0, 0, 0, 0)}");
                var action = new ActionID((ActionType)type, hover.ActionID);
                DrawStatus("Status RC", action, true, true);
                DrawStatus("Status R-", action, true, false);
                DrawStatus("Status -C", action, false, true);
                DrawStatus("Status --", action, false, false);
                ImGui.TextUnformatted($"Adjusted recast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedRecastTime(type, hover.ActionID):f2}");
                ImGui.TextUnformatted($"Adjusted cast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedCastTime(type, hover.ActionID):f2}");
                ImGui.TextUnformatted($"Recast: {mgr->GetRecastTime(type, hover.ActionID):f2}");
                ImGui.TextUnformatted($"Recast elapsed: {mgr->GetRecastTimeElapsed(type, hover.ActionID):f2}");
                ImGui.TextUnformatted($"Recast active: {mgr->IsRecastTimerActive(type, hover.ActionID)}");
                var groupID = mgr->GetRecastGroup((int)type, hover.ActionID);
                ImGui.TextUnformatted($"Recast group: {groupID}");
                var group = mgr->GetRecastGroupDetail(groupID);
                if (group != null)
                    ImGui.TextUnformatted($"Recast group details: active={group->IsActive}, action={group->ActionId}, elapsed={group->Elapsed:f3}, total={group->Total:f3}, cooldown={group->Total - group->Elapsed:f3}");
            }
        }
        else if (Service.GameGui.HoveredItem != 0)
        {
            uint itemID = (uint)Service.GameGui.HoveredItem % 1000000;
            bool isHQ = Service.GameGui.HoveredItem / 1000000 > 0;
            ImGui.TextUnformatted($"Hover item: {Service.GameGui.HoveredItem}");
            ImGui.TextUnformatted($"Name: {Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()?.GetRow(itemID)?.Name}{(isHQ ? " (HQ)" : "")}");
            ImGui.TextUnformatted($"Count: {FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(itemID, isHQ, false, false)}");
            ImGui.TextUnformatted($"Status: {mgr->GetActionStatus(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID)}");
            ImGui.TextUnformatted($"Adjusted recast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
            ImGui.TextUnformatted($"Adjusted cast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedCastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
            ImGui.TextUnformatted($"Recast: {mgr->GetRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
            ImGui.TextUnformatted($"Recast elapsed: {mgr->GetRecastTimeElapsed(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
            ImGui.TextUnformatted($"Recast active: {mgr->IsRecastTimerActive(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID)}");
            var groupID = mgr->GetRecastGroup(2, itemID);
            ImGui.TextUnformatted($"Recast group: {groupID}");
            var group = mgr->GetRecastGroupDetail(groupID);
            if (group != null)
                ImGui.TextUnformatted($"Recast group details: active={group->IsActive}, action={group->ActionId}, elapsed={group->Elapsed}, total={group->Total}");
        }
        else
        {
            ImGui.TextUnformatted("Hover: none");
        }

        foreach (var nr in _tree.Node("Player actions"))
        {
            DrawFilteredActions("Hostile + friendly", a => a.IsPlayerAction && a.CanTargetHostile && (a.CanTargetSelf || a.CanTargetParty || a.CanTargetFriendly || a.Unknown19 || a.Unknown22 || a.Unknown23));
        }
    }

    private unsafe void DrawStatus(string prompt, ActionID action, bool checkRecast, bool checkCasting)
    {
        uint extra;
        var status = ActionManagerEx.Instance!.GetActionStatus(action, Service.ClientState.LocalPlayer?.TargetObjectId ?? 0xE0000000, checkRecast, checkCasting, &extra);
        ImGui.TextUnformatted($"{prompt}: {status} [{extra}] '{Service.LuminaRow<Lumina.Excel.GeneratedSheets.LogMessage>(status)?.Text}'");
    }

    private void DrawFilteredActions(string tag, Func<Lumina.Excel.GeneratedSheets.Action, bool> filter)
    {
        foreach (var nr in _tree.Node(tag))
        {
            foreach (var a in Service.LuminaGameData!.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()!.Where(filter))
            {
                _tree.LeafNode($"#{a.RowId} {a.Name}");
            }
        }
    }
}
