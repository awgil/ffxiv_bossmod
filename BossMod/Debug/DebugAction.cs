using Dalamud.Game.Gui;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Dalamud.Bindings.ImGui;

namespace BossMod;

sealed unsafe class DebugAction : IDisposable
{
    private int _customAction;
    private readonly UITree _tree = new();
    private readonly WorldState _ws;
    private readonly ActionManagerEx _amex;

    private bool _autoAttack;
    //private delegate byte SetAutoAttackDelegate(byte* self, byte value, byte sendPacket, byte isInstant);
    //private readonly HookAddress<SetAutoAttackDelegate> _hook;

    public DebugAction(WorldState ws, ActionManagerEx amex)
    {
        _ws = ws;
        _amex = amex;
        //_hook = new(Service.SigScanner.Module.BaseAddress + 0xAD3740, SetAutoAttackDetour);
        Service.Log("---");
    }

    public void Dispose()
    {
        //_hook.Dispose();
    }

    public void DrawActionManagerExtensions()
    {
        var pc = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var pcCast = pc != null ? ((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)pc)->GetCastInfo() : null;
        var amr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        var aidCastAction = _amex.CastAction;
        var aidCastSpell = _amex.CastSpell;
        var aidCombo = new ActionID(ActionType.Spell, amr->Combo.Action);
        var aidQueued = _amex.QueuedAction;
        var aidGTAction = new ActionID((ActionType)amr->AreaTargetingActionType, amr->AreaTargetingActionId);
        var aidGTSpell = new ActionID(ActionType.Spell, amr->AreaTargetingSpellId);
        ImGui.TextUnformatted($"ActionManager singleton address: 0x{(ulong)amr:X}");
        ImGui.TextUnformatted($"Anim lock: {amr->AnimationLock:f3}");
        ImGui.TextUnformatted($"Cast: {aidCastAction} / {aidCastSpell}, progress={amr->CastTimeElapsed:f3}/{amr->CastTimeTotal:f3}, target={amr->CastTargetId:X}/{Utils.Vec3String(amr->CastTargetPosition)}");
        if (pcCast != null)
            ImGui.TextUnformatted($"Cast (obj): {pcCast->IsCasting} {new ActionID((ActionType)pcCast->ActionType, pcCast->ActionId)}, progress={pcCast->CurrentCastTime:f3}/{pcCast->BaseCastTime:f3}/{pcCast->TotalCastTime:f3}");
        ImGui.TextUnformatted($"Combo: {aidCombo}, {_amex.ComboTimeLeft:f3}");
        ImGui.TextUnformatted($"Queue: {(amr->ActionQueued ? "active" : "inactive")}, {aidQueued} @ {(ulong)amr->QueuedTargetId:X} [{amr->QueueType}], combo={amr->QueuedComboRouteId}");
        ImGui.TextUnformatted($"GT: {aidGTAction} / {aidGTSpell}, arg={Utils.ReadField<uint>(amr, 0x94)}, targ={amr->AreaTargetingExecuteAtObject:X}/{amr->AreaTargetingExecuteAtCursor}, a0={Utils.ReadField<byte>(amr, 0xA0):X2}, bc={Utils.ReadField<byte>(amr, 0xBC):X}");
        ImGui.TextUnformatted($"Ballista: {amr->BallistaActive} {amr->BallistaRowId} @ {amr->BallistaOrigin} a={amr->BallistaRefAngle.Radians()} r={amr->BallistaRadius} t={amr->BallistaEntityId:X8}");
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
            _amex.FaceDirection((_ws.Party.Player()?.Rotation ?? 0.Degrees()) + 30.Degrees());
        }
    }

    public void DrawActionData()
    {
        var mgr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        ImGui.InputInt("Action to show details for", ref _customAction);
        if (_customAction != 0)
        {
            var data = Service.LuminaRow<Lumina.Excel.Sheets.Action>((uint)_customAction);
            if (data != null)
            {
                ImGui.TextUnformatted($"Name: {data.Value.Name}");
                ImGui.TextUnformatted($"Cast time: {data.Value.Cast100ms * 0.1f:f1} + {data.Value.ExtraCastTime100ms * 0.1f:f1}");
                ImGui.TextUnformatted($"Range: {data.Value.Range}");
                ImGui.TextUnformatted($"Effect range: {data.Value.EffectRange}");
                ImGui.TextUnformatted($"Cooldown group: {data.Value.CooldownGroup}");
                ImGui.TextUnformatted($"Max charges: {data.Value.MaxCharges}");
                ImGui.TextUnformatted($"Category: {data.Value.ActionCategory.RowId} ({data.Value.ActionCategory.ValueNullable?.Name})");

                if (data.Value.CooldownGroup > 0 && data.Value.CooldownGroup <= mgr->Cooldowns.Length)
                {
                    ref var cd = ref mgr->Cooldowns[data.Value.CooldownGroup - 1];
                    ImGui.TextUnformatted($"Cooldown: active={cd.IsActive}, {cd.Elapsed:f3}/{cd.Total:f3}");
                }
            }
        }

        var hover = Service.GameGui.HoveredAction;
        if (hover.ActionID != 0)
        {
            var mnemonic = Service.ClientState.LocalPlayer?.ClassJob.ValueNullable?.Abbreviation.ToString();
            var rotationType = mnemonic != null ? Type.GetType($"BossMod.{mnemonic}Rotation")?.GetNestedType("AID") : null;
            ImGui.TextUnformatted($"Hover action: {hover.ActionKind} {hover.ActionID} (base={hover.BaseActionID}) ({mnemonic}: {rotationType?.GetEnumName(hover.ActionID)})");

            string name = "";
            var type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.None;
            uint unlockLink = 0;
            if ((int)hover.ActionKind == 24) // action
            {
                var data = Service.LuminaRow<Lumina.Excel.Sheets.Action>(hover.ActionID);
                name = data?.Name.ToString() ?? "";
                type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action;
                unlockLink = data?.UnlockLink.RowId ?? 0;
            }
            else if (hover.ActionKind == HoverActionKind.GeneralAction)
            {
                var data = Service.LuminaRow<Lumina.Excel.Sheets.GeneralAction>(hover.ActionID);
                name = data?.Name.ToString() ?? "";
                type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.GeneralAction;
                unlockLink = data?.UnlockLink ?? 0;
            }
            else if (hover.ActionKind == HoverActionKind.Trait)
            {
                var data = Service.LuminaRow<Lumina.Excel.Sheets.Trait>(hover.ActionID);
                name = data?.Name.ToString() ?? "";
                unlockLink = data?.Quest.RowId ?? 0;
            }

            ImGui.TextUnformatted($"Name: {name}");
            ImGui.TextUnformatted($"Unlock: {unlockLink} ({Service.LuminaRow<Lumina.Excel.Sheets.Quest>(unlockLink)?.Name}) = {FFXIVClientStructs.FFXIV.Client.Game.QuestManager.IsQuestComplete(unlockLink)}");
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
            ImGui.TextUnformatted($"Name: {Service.LuminaRow<Lumina.Excel.Sheets.Item>(itemID)?.Name}{(isHQ ? " (HQ)" : "")}");
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
            DrawFilteredActions("Hostile + friendly", a => a.IsPlayerAction && a.CanTargetHostile && (a.CanTargetSelf || a.CanTargetParty || a.CanTargetAlliance || a.CanTargetAlly || a.CanTargetOwnPet || a.CanTargetPartyPet));
        }
    }

    public void DrawDutyActions()
    {
        var cd = EventFramework.Instance()->DirectorModule.ActiveContentDirector;
        if (cd == null)
        {
            ImGui.TextUnformatted("Content director is unavailable");
            return;
        }
        ImGui.TextUnformatted($"Excel rows: pending={cd->DutyActionManager.PendingContentExActionRowId}, current={cd->DutyActionManager.CurrentContentExActionRowId}");
        ImGui.TextUnformatted($"Num valid slots: {cd->DutyActionManager.NumValidSlots}, actions present={cd->DutyActionManager.ActionActive[0] && cd->DutyActionManager.NumValidSlots > 0}");
        for (int i = 0; i < cd->DutyActionManager.NumValidSlots; ++i)
        {
            var chargeText = i < 2 ? $"{cd->DutyActionManager.CurCharges[i]}/{cd->DutyActionManager.MaxCharges[i]}" : "?/?";
            ImGui.TextUnformatted($"[{i}]: action={new ActionID(ActionType.Spell, cd->DutyActionManager.ActionId[i])}, active={cd->DutyActionManager.ActionActive[i]}, charges={chargeText}");
        }
    }

    public void DrawAutoAttack()
    {
        var aa = UIState.Instance()->WeaponState.AutoAttackState.IsAutoAttacking;
        if (_autoAttack != aa)
            Service.Log($"AA state changed: {_autoAttack} -> {aa}");
        _autoAttack = aa;
        ImGui.TextUnformatted($"Auto-attack: {aa}");
    }

    private void DrawStatus(string prompt, ActionID action, bool checkRecast, bool checkCasting)
    {
        uint extra;
        var status = _amex.GetActionStatus(action, Service.ClientState.LocalPlayer?.TargetObjectId ?? 0xE0000000, checkRecast, checkCasting, &extra);
        ImGui.TextUnformatted($"{prompt}: {status} [{extra}] '{Service.LuminaRow<Lumina.Excel.Sheets.LogMessage>(status)?.Text}'");
    }

    private void DrawFilteredActions(string tag, Func<Lumina.Excel.Sheets.Action, bool> filter)
    {
        foreach (var nr in _tree.Node(tag))
        {
            foreach (var a in Service.LuminaSheet<Lumina.Excel.Sheets.Action>()!.Where(filter))
            {
                _tree.LeafNode($"#{a.RowId} {a.Name}");
            }
        }
    }

    //private byte SetAutoAttackDetour(byte* self, byte value, byte sendPacket, byte isInstant)
    //{
    //    if (*self != 0 || value != 0)
    //        Service.Log($"SAA: {*self} -> {value} ({sendPacket}, {isInstant})");
    //    return _hook.Original(self, value, sendPacket, isInstant);
    //}
}
