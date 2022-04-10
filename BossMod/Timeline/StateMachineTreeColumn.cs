namespace BossMod
{
    public class StateMachineTreeColumn : StateMachineColumn
    {
        public StateMachine? ControlledSM;

        public StateMachineTreeColumn(Timeline timeline, StateMachineTree tree, StateMachine? controlledSM)
            : base(timeline, tree)
        {
            ControlledSM = controlledSM;
        }

        public override void Draw()
        {
            foreach (var node in Tree.Nodes.Values)
            {
                DrawNode(node, node.State == ControlledSM?.ActiveState ? ControlledSM.TimeSinceTransition : null);
            }
        }

        protected override void OnNodeActivated(StateMachineTree.Node node)
        {
            if (ControlledSM != null)
            {
                ControlledSM.ActiveState = node.State;
            }
        }
    }
}
