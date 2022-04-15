using ImGuiNET;

namespace BossMod
{
    public class StateMachineBranchColumn : StateMachineColumn
    {
        public int Branch = 0;

        public StateMachineBranchColumn(Timeline timeline, StateMachineTree tree)
            : base(timeline, tree)
        {
            SingleBranchMode = true;
        }

        public override void Draw()
        {
            foreach (var node in Tree.BranchNodes(Branch))
            {
                DrawNode(node);
            }
        }

        // returns whether branch was changed
        public bool DrawControls()
        {
            bool res = false;
            if (ImGui.Button("<") && Branch > 0)
            {
                --Branch;
                res = true;
            }
            ImGui.SameLine();
            ImGui.TextUnformatted($"Current branch: {Branch + 1}/{Tree.NumBranches}");
            ImGui.SameLine();
            if (ImGui.Button(">") && Branch < Tree.NumBranches - 1)
            {
                ++Branch;
                res = true;
            }
            return res;
        }
    }
}
