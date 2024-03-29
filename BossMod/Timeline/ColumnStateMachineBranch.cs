namespace BossMod;

public class ColumnStateMachineBranch : ColumnStateMachine
{
    private List<int> _phaseBranches;

    public ColumnStateMachineBranch(Timeline timeline, StateMachineTree tree, List<int> phaseBranches)
        : base(timeline, tree)
    {
        _phaseBranches = phaseBranches;
    }

    public override void Update()
    {
        Width = PixelsPerBranch;
    }

    public override void Draw()
    {
        foreach (var (phase, branch) in Tree.Phases.Zip(_phaseBranches))
        {
            foreach (var node in phase.BranchNodes(branch).TakeWhile(node => node.Time < phase.Duration))
            {
                DrawNode(node, true);
            }
        }
    }
}
