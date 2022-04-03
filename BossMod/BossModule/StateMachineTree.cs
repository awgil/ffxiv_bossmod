using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // tree describing states and transitions
    public class StateMachineTree
    {
        public class Node
        {
            public float Time;
            public int BranchID;
            public bool InGroup;
            public bool BossIsCasting;
            public bool IsDowntime;
            public bool IsPositioning;
            public StateMachine.State State;
            public Node? Predecessor;
            public List<Node> Successors = new();

            internal Node(float t, int branchID, StateMachine.State state, Node? pred)
            {
                Time = t;
                BranchID = branchID;
                if (pred == null)
                {
                    InGroup = true;
                    BossIsCasting = IsDowntime = IsPositioning = false;
                }
                else
                {
                    var predEndHint = pred.State.EndHint;
                    InGroup = predEndHint.HasFlag(StateMachine.StateHint.GroupWithNext);
                    BossIsCasting = (pred.BossIsCasting || predEndHint.HasFlag(StateMachine.StateHint.BossCastStart)) && !predEndHint.HasFlag(StateMachine.StateHint.BossCastEnd);
                    IsDowntime = (pred.IsDowntime || predEndHint.HasFlag(StateMachine.StateHint.DowntimeStart)) && !predEndHint.HasFlag(StateMachine.StateHint.DowntimeEnd);
                    IsPositioning = (pred.IsPositioning || predEndHint.HasFlag(StateMachine.StateHint.PositioningStart)) && !predEndHint.HasFlag(StateMachine.StateHint.PositioningEnd);
                }
                State = state;
                Predecessor = pred;
            }
        }

        private Dictionary<uint, Node> _nodes = new();
        public IReadOnlyDictionary<uint, Node> Nodes => _nodes;

        public Node? StartingNode { get; private set; }
        public float MaxTime { get; private set; }
        public int NumBranches { get; private set; }

        public StateMachineTree(StateMachine.State? initial)
        {
            if (initial != null)
                (StartingNode, MaxTime, NumBranches) = LayoutNodeAndSuccessors(0, 0, initial, null);
        }

        // return sequential list of nodes belonging to the single branch
        public IEnumerable<Node> BranchNodes(int branchID)
        {
            if (StartingNode == null)
                yield break;

            yield return StartingNode;
            var n = StartingNode;
            while (n.Successors.Count > 0)
            {
                int nextIndex = n.Successors.FindIndex(n => n.BranchID > branchID);
                if (nextIndex == -1)
                    nextIndex = n.Successors.Count;
                n = n.Successors[nextIndex - 1];
                yield return n;
            }
        }

        public Node? TimeToBranchNode(int branchID, float t)
        {
            return BranchNodes(branchID).FirstOrDefault(n => n.Time >= t);
        }

        private (Node, float, int) LayoutNodeAndSuccessors(float t, int branchID, StateMachine.State state, Node? pred)
        {
            var node = _nodes[state.ID] = new Node(t + state.Duration, branchID, state, pred);
            float succDuration = 0;

            // first layout default state, if any
            if (state.Next != null)
            {
                (var succ, var dur, var nextBranch) = LayoutNodeAndSuccessors(t + state.Duration, branchID, state.Next, node);
                node.Successors.Add(succ);
                succDuration = dur;
                branchID = nextBranch;
            }

            // now layout extra successors, if any
            if (state.PotentialSuccessors != null)
            {
                foreach (var s in state.PotentialSuccessors)
                {
                    if (state.Next == s)
                        continue; // this is already processed

                    (var succ, var dur, var nextBranch) = LayoutNodeAndSuccessors(t + state.Duration, branchID, s, node);
                    node.Successors.Add(succ);
                    succDuration = Math.Max(succDuration, dur);
                    branchID = nextBranch;
                }
            }

            if (state.Next == null && state.PotentialSuccessors == null)
            {
                branchID++; // leaf
            }

            return (node, state.Duration + succDuration, branchID);
        }
    }
}
