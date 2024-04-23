namespace BossMod;

public class ColumnStateMachineTree(Timeline timeline, StateMachineTree tree, StateMachine? controlledSM) : ColumnStateMachine(timeline, tree)
{
    public StateMachine? ControlledSM = controlledSM;

    public override void Update()
    {
        Width = Tree.TotalBranches * PixelsPerBranch;
    }

    public override void Draw()
    {
        foreach (var node in Tree.Nodes.Values)
        {
            DrawNode(node, false, node.State == ControlledSM?.ActiveState ? ControlledSM.TimeSinceTransition : null);
        }
    }
}
