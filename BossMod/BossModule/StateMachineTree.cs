namespace BossMod;

// tree describing all phases, states and transitions
public class StateMachineTree
{
    public class Node
    {
        public float Time; // time from phase start to state transition, assuming all states last exactly for expected duration
        public int PhaseID;
        public int BranchID;
        public int NumBranches; // how many branches are reachable from this node
        public bool InGroup;
        public bool BossIsCasting;
        public bool IsDowntime;
        public bool IsPositioning;
        public bool IsVulnerable;
        public StateMachine.State State;
        public Node? Predecessor;
        public List<Node> Successors = [];

        internal Node(float t, int phaseID, int branchID, StateMachine.State state, Node? pred)
        {
            Time = t;
            PhaseID = phaseID;
            BranchID = branchID;
            if (pred == null)
            {
                InGroup = true;
                BossIsCasting = IsDowntime = IsPositioning = IsVulnerable = false;
            }
            else
            {
                var predEndHint = pred.State.EndHint;
                InGroup = predEndHint.HasFlag(StateMachine.StateHint.GroupWithNext);
                BossIsCasting = (pred.BossIsCasting || predEndHint.HasFlag(StateMachine.StateHint.BossCastStart)) && !predEndHint.HasFlag(StateMachine.StateHint.BossCastEnd);
                IsDowntime = (pred.IsDowntime || predEndHint.HasFlag(StateMachine.StateHint.DowntimeStart)) && !predEndHint.HasFlag(StateMachine.StateHint.DowntimeEnd);
                IsPositioning = (pred.IsPositioning || predEndHint.HasFlag(StateMachine.StateHint.PositioningStart)) && !predEndHint.HasFlag(StateMachine.StateHint.PositioningEnd);
                IsVulnerable = (pred.IsVulnerable || predEndHint.HasFlag(StateMachine.StateHint.VulnerableStart)) && !predEndHint.HasFlag(StateMachine.StateHint.VulnerableEnd);
            }
            State = state;
            Predecessor = pred;
        }
    }

    public class Phase
    {
        public string Name;
        public Node StartingNode;
        public float StartTime; // time from pull to phase start
        public float MaxTime; // max state machine duration
        public float Duration; // expected duration

        internal Phase(StateMachine.Phase phase, Node startingNode, float maxTime)
        {
            Name = phase.Name;
            StartingNode = startingNode;
            MaxTime = maxTime;
            Duration = phase.ExpectedDuration >= 0 ? phase.ExpectedDuration : maxTime;
        }

        // return sequential list of nodes belonging to the single branch
        public IEnumerable<Node> BranchNodes(int branchOffset)
        {
            if (branchOffset < 0 || branchOffset >= StartingNode.NumBranches)
                yield break;

            yield return StartingNode;
            var n = StartingNode;
            while (n.Successors.Count > 0)
            {
                int nextIndex = n.Successors.FindIndex(n => n.BranchID > StartingNode.BranchID + branchOffset);
                if (nextIndex == -1)
                    nextIndex = n.Successors.Count;
                n = n.Successors[nextIndex - 1];
                yield return n;
            }
        }

        public Node TimeToBranchNode(int branchOffset, float t)
        {
            Node? last = null;
            foreach (var n in BranchNodes(branchOffset))
            {
                if (n.Time >= t)
                    return n;
                last = n;
            }
            return last!;
        }
    }

    private readonly Dictionary<uint, Node> _nodes = [];
    public IReadOnlyDictionary<uint, Node> Nodes => _nodes;

    private readonly List<Phase> _phases = [];
    public IReadOnlyList<Phase> Phases => _phases;

    public int TotalBranches { get; private set; }
    public float TotalMaxTime { get; private set; }

    public StateMachineTree(StateMachine sm)
    {
        for (int i = 0; i < sm.Phases.Count; ++i)
        {
            var (startingNode, maxTime) = LayoutNodeAndSuccessors(0, i, TotalBranches, sm.Phases[i].InitialState, null);
            _phases.Add(new(sm.Phases[i], startingNode, maxTime));
            TotalBranches += startingNode.NumBranches;
            TotalMaxTime = Math.Max(TotalMaxTime, maxTime);
        }
    }

    public void ApplyTimings(List<float>? phaseDurations)
    {
        if (Phases.Count == 0)
            return;

        if (phaseDurations != null)
            foreach (var (p, t) in Phases.Zip(phaseDurations))
                p.Duration = Math.Min(t, p.MaxTime);

        for (int i = 1; i < Phases.Count; ++i)
            Phases[i].StartTime = Phases[i - 1].StartTime + Phases[i - 1].Duration;

        var lastPhase = Phases[^1];
        TotalMaxTime = lastPhase.StartTime + lastPhase.Duration;
    }

    // find phase index that corresponds to specified time; assumes ApplyTimings was called before
    public int FindPhaseAtTime(float t)
    {
        int next = _phases.FindIndex(p => p.StartTime > t);
        return next switch
        {
            < 0 => _phases.Count - 1,
            0 => 0,
            _ => next - 1
        };
    }

    public (Node, float) PhaseTimeToNodeAndDelay(float t, int phaseIndex, List<int> phaseBranches)
    {
        var node = Phases[phaseIndex].TimeToBranchNode(phaseBranches[phaseIndex], t);
        return (node, t - (node.Predecessor?.Time ?? 0));
    }

    public (Node, float) AbsoluteTimeToNodeAndDelay(float t, List<int> phaseBranches)
    {
        int phaseIndex = FindPhaseAtTime(t);
        return PhaseTimeToNodeAndDelay(t - Phases[phaseIndex].StartTime, phaseIndex, phaseBranches);
    }

    private (Node, float) LayoutNodeAndSuccessors(float t, int phaseID, int branchID, StateMachine.State state, Node? pred)
    {
        var node = _nodes[state.ID] = new Node(t + state.Duration, phaseID, branchID, state, pred);
        float succDuration = 0;

        if (state.NextStates?.Length > 0)
        {
            foreach (var s in state.NextStates)
            {
                var (succ, dur) = LayoutNodeAndSuccessors(t + state.Duration, phaseID, branchID + node.NumBranches, s, node);
                node.Successors.Add(succ);
                succDuration = Math.Max(succDuration, dur);
                node.NumBranches += succ.NumBranches;
            }
        }
        else
        {
            node.NumBranches++; // leaf
        }

        return (node, state.Duration + succDuration);
    }
}
