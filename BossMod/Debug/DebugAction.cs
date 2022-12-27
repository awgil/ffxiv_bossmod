using Dalamud.Game.Gui;
using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    public class DebugAction
    {
        private WorldState _ws;
        private int _customAction = 0;

        public DebugAction(WorldState ws)
        {
            _ws = ws;
        }

        public unsafe void DrawActionManagerEx()
        {
            var am = ActionManagerEx.Instance!;
            var amr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
            ImGui.TextUnformatted($"ActionManager singleton address: 0x{(ulong)amr:X}");
            ImGui.TextUnformatted($"Anim lock: {am.AnimationLock:f3}");
            ImGui.TextUnformatted($"Cast: {am.CastAction} / {am.CastSpell}, progress={am.CastTimeElapsed:f3}/{am.CastTimeTotal:f3}, target={am.CastTargetID:X}/{Utils.Vec3String(am.CastTargetPos)}");
            ImGui.TextUnformatted($"Combo: {new ActionID(ActionType.Spell, am.ComboLastMove)}, {am.ComboTimeLeft:f3}");
            ImGui.TextUnformatted($"Queue: {(am.QueueActive ? "active" : "inactive")}, {am.QueueAction} @ {am.QueueTargetID:X} [{am.QueueCallType}], combo={am.QueueComboRouteID}");
            ImGui.TextUnformatted($"GT: {am.GTAction} / {am.GTSpell}, arg={am.GTUnkArg}, obj={am.GTUnkObj:X}, a0={am.GT_uA0:X2}, b8={am.GT_uB8:X2}, bc={am.GT_uBC:X}");
            if (ImGui.Button("GT complete"))
            {
                Utils.WriteField(amr, 0xB8, (byte)1);
            }
            ImGui.SameLine();
            if (ImGui.Button("GT set target"))
            {
                Utils.WriteField(amr, 0x98, (ulong)(Service.TargetManager.Target?.ObjectId ?? 0));
            }

            if (ImGui.Button("Rotate 30 CCW"))
            {
                am.FaceDirection((_ws.Party.Player()?.Rotation ?? 0.Degrees() + 30.Degrees()).ToDirection());
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
                    ImGui.TextUnformatted($"Cast time: {data.Cast100ms / 10.0:f1}");
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
                FFXIVClientStructs.FFXIV.Client.Game.ActionType type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.None;
                uint unlockLink = 0;
                if (hover.ActionKind == HoverActionKind.Action)
                {
                    var data = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(hover.ActionID);
                    name = data?.Name;
                    type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell;
                    unlockLink = data?.UnlockLink ?? 0;
                }
                else if (hover.ActionKind == HoverActionKind.GeneralAction)
                {
                    var data = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GeneralAction>()?.GetRow(hover.ActionID);
                    name = data?.Name;
                    type = FFXIVClientStructs.FFXIV.Client.Game.ActionType.General;
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
                    ImGui.TextUnformatted($"Status: {mgr->GetActionStatus(type, hover.ActionID, Service.ClientState.LocalPlayer?.TargetObjectId ?? 0xE0000000, 1, 1)}");
                    ImGui.TextUnformatted($"Adjusted recast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedRecastTime(type, hover.ActionID):f2}");
                    ImGui.TextUnformatted($"Adjusted cast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedCastTime(type, hover.ActionID):f2}");
                    ImGui.TextUnformatted($"Recast: {mgr->GetRecastTime(type, hover.ActionID):f2}");
                    ImGui.TextUnformatted($"Recast elapsed: {mgr->GetRecastTimeElapsed(type, hover.ActionID):f2}");
                    ImGui.TextUnformatted($"Recast active: {mgr->IsRecastTimerActive(type, hover.ActionID)}");
                    var groupID = mgr->GetRecastGroup((int)type, hover.ActionID);
                    ImGui.TextUnformatted($"Recast group: {groupID}");
                    var group = mgr->GetRecastGroupDetail(groupID);
                    if (group != null)
                        ImGui.TextUnformatted($"Recast group details: active={group->IsActive}, action={group->ActionID}, elapsed={group->Elapsed}, total={group->Total}");
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
                    ImGui.TextUnformatted($"Recast group details: active={group->IsActive}, action={group->ActionID}, elapsed={group->Elapsed}, total={group->Total}");
            }
            else
            {
                ImGui.TextUnformatted("Hover: none");
            }
        }
    }
}
