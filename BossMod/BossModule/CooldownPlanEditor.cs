using ImGuiNET;
using System;
using System.Linq;

namespace BossMod
{
    public class CooldownPlanEditor
    {
        private CooldownPlan _plan;
        private Action _onModified;
        private Timeline _timeline = new();
        private StateMachineBranchColumn _colStates;
        private CooldownPlannerColumns _planner;
        private bool _modified = false;

        public CooldownPlanEditor(CooldownPlan plan, StateMachine sm, Action onModified)
        {
            _plan = plan;
            _onModified = onModified;

            var tree = new StateMachineTree(sm);
            var phaseBranches = Enumerable.Repeat(0, tree.Phases.Count).ToList();
            _colStates = _timeline.AddColumn(new StateMachineBranchColumn(_timeline, tree, phaseBranches));
            _planner = new(plan, OnPlanModified, _timeline, tree, phaseBranches);

            _timeline.MaxTime = tree.TotalMaxTime;
        }

        public void Draw()
        {
            if (ImGui.Button(_modified ? "Save" : "No changes") && _modified)
                Save();
            ImGui.SameLine();
            _planner.DrawControls(true);

            _timeline.Draw();
        }

        private void Save()
        {
            _planner.UpdateEditedPlan();
            _onModified();
            _modified = false;
        }

        private void OnPlanModified()
        {
            _timeline.MaxTime = _colStates.Tree.TotalMaxTime;
            _modified = true;
        }
    }
}
