using ImGuiNET;
using System;

namespace BossMod
{
    public class CooldownPlanEditor
    {
        private Action _onModified;
        private Timeline _timeline = new();
        private StateMachineBranchColumn _colStates;
        private CooldownPlannerColumns _planner;
        private bool _modified = false;

        public CooldownPlanEditor(CooldownPlan plan, StateMachineTree stateTree, Action onModified, int initialBranch = 0)
        {
            _onModified = onModified;
            _timeline.MaxTime = stateTree.MaxTime;

            _colStates = _timeline.AddColumn(new StateMachineBranchColumn(_timeline, stateTree));
            _colStates.Branch = initialBranch;

            _planner = new(plan, () => _modified = true, _timeline, stateTree, initialBranch);
        }

        public void Draw()
        {
            if (_colStates.DrawControls())
                _planner.SelectBranch(_colStates.Branch);
            ImGui.SameLine();
            if (ImGui.Button(_modified ? "Save" : "No changes") && _modified)
                Save();
            ImGui.SameLine();
            _planner.DrawControls();

            _timeline.Draw();
        }

        private void Save()
        {
            _planner.UpdateEditedPlan();
            _onModified();
            _modified = false;
        }
    }
}
