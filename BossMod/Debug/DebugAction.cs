using Dalamud.Game.Gui;
using ImGuiNET;

namespace BossMod
{
    public class DebugAction
    {
        public unsafe void DrawActionData()
        {
            var hover = Service.GameGui.HoveredAction;
            if (hover.ActionID == 0)
            {
                ImGui.Text("Hover action: none");
                return;
            }

            ImGui.Text($"Hover action: {hover.ActionKind} {hover.ActionID} (base={hover.BaseActionID}) (WAR: {(WARRotation.AID)hover.ActionID})");
            if (hover.ActionKind != HoverActionKind.Action)
                return;

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
    }
}
