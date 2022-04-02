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
                var mgr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
                ImGui.Text($"Hover action: {hover.ActionKind} {hover.ActionID} (base={hover.BaseActionID}) (WAR: {(WARRotation.AID)hover.ActionID})");

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
                    ImGui.Text($"Adjusted recast: {mgr->GetAdjustedRecastTime(type, hover.ActionID):f2}");
                    ImGui.Text($"Adjusted cast: {mgr->GetAdjustedCastTime(type, hover.ActionID):f2}");
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
