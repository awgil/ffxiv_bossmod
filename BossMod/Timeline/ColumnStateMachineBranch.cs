namespace BossMod;

public class ColumnStateMachineBranch(Timeline timeline, StateMachineTree tree, List<int> phaseBranches) : ColumnStateMachine(timeline, tree)
{
    public override void Update()
    {
        Width = PixelsPerBranch;
    }

    public override void Draw()
    {
        foreach (var (phase, branch) in Tree.Phases.Zip(phaseBranches))
        {
            foreach (var node in phase.BranchNodes(branch).TakeWhile(node => node.Time < phase.Duration))
            {
                DrawNode(node, true);
            }
        }
    }
}
