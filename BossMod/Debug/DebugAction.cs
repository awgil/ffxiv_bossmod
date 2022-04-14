using Dalamud.Game.Gui;
using ImGuiNET;
using System;

namespace BossMod
{
    public class DebugAction
    {
        private int _customAction = 0;

        public unsafe void DrawActionData()
        {
            ImGui.InputInt("Action to show details for", ref _customAction);
            if (_customAction != 0)
            {
                var data = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow((uint)_customAction);
                if (data != null)
                {
                    ImGui.Text($"Name: {data.Name}");
                    ImGui.Text($"Cast time: {data.Cast100ms / 10.0:f1}");
                    ImGui.Text($"Range: {data.Range}");
                    ImGui.Text($"Effect range: {data.EffectRange}");
                }
            }

            var mgr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
            var hover = Service.GameGui.HoveredAction;
            if (hover.ActionID != 0)
            {
                var mnemonic = Service.ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation.ToString();
                var rotationType = mnemonic != null ? Type.GetType($"BossMod.{mnemonic}Rotation")?.GetNestedType("AID") : null;
                ImGui.Text($"Hover action: {hover.ActionKind} {hover.ActionID} (base={hover.BaseActionID}) ({mnemonic}: {rotationType?.GetEnumName(hover.ActionID)})");

                var (name, type) = hover.ActionKind switch
                {
                    HoverActionKind.Action => (Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(hover.ActionID)?.Name, FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell),
                    HoverActionKind.GeneralAction => (Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GeneralAction>()?.GetRow(hover.ActionID)?.Name, FFXIVClientStructs.FFXIV.Client.Game.ActionType.General),
                    _ => (null, FFXIVClientStructs.FFXIV.Client.Game.ActionType.None)
                };
                ImGui.Text($"Name: {name}");

                if (hover.ActionKind == HoverActionKind.Action)
                {
                    ImGui.Text($"Range: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(hover.ActionID)}");
                    ImGui.Text($"Stacks: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(hover.ActionID, 0)}");
                    ImGui.Text($"Adjusted ID: {mgr->GetAdjustedActionId(hover.ActionID)}");
                }

                if (type != FFXIVClientStructs.FFXIV.Client.Game.ActionType.None)
                {
                    //ImGui.Text($"Cost: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionCost(type, hover.ActionID, 0, 0, 0, 0)}");
                    ImGui.Text($"Status: {mgr->GetActionStatus(type, hover.ActionID)}");
                    ImGui.Text($"Adjusted recast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedRecastTime(type, hover.ActionID):f2}");
                    ImGui.Text($"Adjusted cast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedCastTime(type, hover.ActionID):f2}");
                    ImGui.Text($"Recast: {mgr->GetRecastTime(type, hover.ActionID):f2}");
                    ImGui.Text($"Recast elapsed: {mgr->GetRecastTimeElapsed(type, hover.ActionID):f2}");
                    ImGui.Text($"Recast active: {mgr->IsRecastTimerActive(type, hover.ActionID)}");
                    var groupID = mgr->GetRecastGroup((int)type, hover.ActionID);
                    ImGui.Text($"Recast group: {groupID}");
                    var group = mgr->GetRecastGroupDetail(groupID);
                    if (group != null)
                        ImGui.Text($"Recast group details: active={group->IsActive}, action={group->ActionID}, elapsed={group->Elapsed}, total={group->Total}");
                }
            }
            else if (Service.GameGui.HoveredItem != 0)
            {
                uint itemID = (uint)Service.GameGui.HoveredItem % 1000000;
                bool isHQ = Service.GameGui.HoveredItem / 1000000 > 0;
                ImGui.Text($"Hover item: {Service.GameGui.HoveredItem}");
                ImGui.Text($"Name: {Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()?.GetRow(itemID)?.Name}{(isHQ ? " (HQ)" : "")}");
                ImGui.Text($"Count: {FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(itemID, isHQ, false, false)}");
                ImGui.Text($"Status: {mgr->GetActionStatus(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID)}");
                ImGui.Text($"Adjusted recast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
                ImGui.Text($"Adjusted cast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedCastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
                ImGui.Text($"Recast: {mgr->GetRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
                ImGui.Text($"Recast elapsed: {mgr->GetRecastTimeElapsed(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
                ImGui.Text($"Recast active: {mgr->IsRecastTimerActive(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID)}");
                var groupID = mgr->GetRecastGroup(2, itemID);
                ImGui.Text($"Recast group: {groupID}");
                var group = mgr->GetRecastGroupDetail(groupID);
                if (group != null)
                    ImGui.Text($"Recast group details: active={group->IsActive}, action={group->ActionID}, elapsed={group->Elapsed}, total={group->Total}");
            }
            else
            {
                ImGui.Text("Hover: none");
            }
        }
    }
}
