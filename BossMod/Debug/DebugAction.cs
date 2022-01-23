using Dalamud.Game.Gui;
using ImGuiNET;

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

            var hover = Service.GameGui.HoveredAction;
            if (hover.ActionID != 0)
            {
                ImGui.Text($"Hover action: {hover.ActionKind} {hover.ActionID} (base={hover.BaseActionID}) (WAR: {(WARRotation.AID)hover.ActionID})");
                if (hover.ActionKind != HoverActionKind.Action)
                    return;

                ImGui.Text($"Name: {Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(hover.ActionID)?.Name}");
                var mgr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
                ImGui.Text($"Range: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(hover.ActionID)}");
                ImGui.Text($"Stacks: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(hover.ActionID, 0)}");
                //ImGui.Text($"Cost: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionCost(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, hover.ActionID, 0, 0, 0, 0)}");
                ImGui.Text($"Status: {mgr->GetActionStatus(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, hover.ActionID)}");
                ImGui.Text($"Adjusted ID: {mgr->GetAdjustedActionId(hover.ActionID)}");
                ImGui.Text($"Adjusted recast: {mgr->GetAdjustedRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, hover.ActionID):f2}");
                ImGui.Text($"Adjusted cast: {mgr->GetAdjustedCastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, hover.ActionID):f2}");
                ImGui.Text($"Recast: {mgr->GetRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, hover.ActionID):f2}");
                ImGui.Text($"Recast elapsed: {mgr->GetRecastTimeElapsed(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, hover.ActionID):f2}");
                ImGui.Text($"Recast active: {mgr->IsRecastTimerActive(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell, hover.ActionID)}");

                var groupID = mgr->GetRecastGroup(1, hover.ActionID);
                ImGui.Text($"Recast group: {groupID}");
                var group = mgr->GetRecastGroupDetail(groupID);
                if (group != null)
                    ImGui.Text($"Recast group details: active={group->IsActive}, action={group->ActionID}, elapsed={group->Elapsed}, total={group->Total}");
            }
            else if (Service.GameGui.HoveredItem != 0)
            {
                ImGui.Text($"Hover item: {Service.GameGui.HoveredItem}");
                var row = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()?.GetRow(hover.ActionID % 1000000);
                if (row != null)
                {
                    ImGui.Text($"Name: {row.Name}{(hover.ActionID / 1000000 > 0 ? " (HQ)" : "")}");
                }
            }
            else
            {
                ImGui.Text("Hover: none");
            }
        }
    }
}
