using ImGuiNET;

namespace BossMod;

// to execute a concrete plan for a concrete encounter, we build a "timeline" - for each state we assign a time/duration (which depend both on state machine definition and plan's phase timings)
// for each action defined by a plan, we define start/end times on that timeline (plus a range of branches in which it might be executed)
public class CooldownPlanExecution
{
    public record struct StateFlag(bool Active, float TransitionIn);

    public class StateData
    {
        public float EnterTime;
        public float Duration;
        public int BranchID;
        public int NumBranches;
        public StateFlag Downtime = new(false, float.MaxValue);
        public StateFlag Positioning = new(false, float.MaxValue);
        public StateFlag Vulnerable = new(false, float.MaxValue);
    }

    public class ElementData(float windowStart, float windowEnd, int branchID, int numBranches)
    {
        public float WindowStart = windowStart;
        public float WindowEnd = windowEnd;
        public int BranchID = branchID;
        public int NumBranches = numBranches;

        public bool IntersectBranchRange(int branchID, int numBranches) => BranchID < branchID + numBranches && branchID < BranchID + NumBranches;
        public bool IsActive(float t, StateData s) => t >= WindowStart && t <= WindowEnd && IntersectBranchRange(s.BranchID, s.NumBranches);
    }

    public class ActionData(ActionID id, float windowStart, float windowEnd, int branchID, int numBranches, bool lowPriority, PlanTarget.ISelector target)
        : ElementData(windowStart, windowEnd, branchID, numBranches)
    {
        public ActionID ID = id;
        public bool LowPriority = lowPriority;
        public bool Executed;
        public PlanTarget.ISelector Target = target;
    }

    public class StrategyData(int index, uint value, float windowStart, float windowEnd, int branchID, int numBranches)
        : ElementData(windowStart, windowEnd, branchID, numBranches)
    {
        public int Index = index;
        public uint Value = value;
    }

    public class ForcedTargetData(uint oid, float windowStart, float windowEnd, int branchID, int numBranches)
        : ElementData(windowStart, windowEnd, branchID, numBranches)
    {
        public uint OID = oid;
    }

    public CooldownPlan? Plan { get; private init; }
    private readonly StateData Pull;
    private readonly Dictionary<uint, StateData> States = [];
    private readonly List<ActionData> Actions = [];
    private readonly List<StrategyData> Strategies = [];
    private readonly List<ForcedTargetData> ForcedTargets = [];
    private readonly int _numStrategyTracks;

    public IReadOnlyList<StrategyData> Strats => Strategies;

    public CooldownPlanExecution(StateMachine sm, CooldownPlan? plan)
    {
        Plan = plan;
        Pull = new() { EnterTime = -30, Duration = 30, NumBranches = 1 };

        var tree = new StateMachineTree(sm);
        tree.ApplyTimings(plan?.Timings);

        StateData? nextPhaseStart = null;
        for (int i = tree.Phases.Count - 1; i >= 0; i--)
            nextPhaseStart = ProcessState(tree, tree.Phases[i].StartingNode, null, nextPhaseStart);
        UpdateTransitions(Pull, nextPhaseStart);

        if (plan != null)
        {
            foreach (var a in plan.Actions)
            {
                var s = States.GetValueOrDefault(a.StateID);
                if (s != null)
                {
                    var windowStart = s.EnterTime + Math.Min(s.Duration, a.TimeSinceActivation);
                    Actions.Add(new(a.ID, windowStart, windowStart + a.WindowLength, s.BranchID, s.NumBranches, a.LowPriority, a.Target));
                }
            }

            _numStrategyTracks = plan.StrategyOverrides.Count;
            for (int i = 0; i < _numStrategyTracks; ++i)
            {
                foreach (var o in plan.StrategyOverrides[i])
                {
                    var s = States.GetValueOrDefault(o.StateID);
                    if (s != null)
                    {
                        var windowStart = s.EnterTime + Math.Min(s.Duration, o.TimeSinceActivation);
                        Strategies.Add(new(i, o.Value, windowStart, windowStart + o.WindowLength, s.BranchID, s.NumBranches));
                    }
                }
            }

            foreach (var t in plan.TargetOverrides)
            {
                var s = States.GetValueOrDefault(t.StateID);
                if (s != null)
                {
                    var windowStart = s.EnterTime + Math.Min(s.Duration, t.TimeSinceActivation);
                    ForcedTargets.Add(new(t.OID, windowStart, windowStart + t.WindowLength, s.BranchID, s.NumBranches));
                }
            }
        }
    }

    public StateData FindStateData(StateMachine.State? s)
    {
        var state = s != null ? States.GetValueOrDefault(s.ID) : null;
        return state ?? Pull;
    }

    // all such functions return whether flag is currently active + estimated time to transition
    public (bool, float) EstimateTimeToNextDowntime(StateMachine sm)
    {
        var s = FindStateData(sm.ActiveState);
        return (s.Downtime.Active, s.Downtime.TransitionIn - Math.Min(sm.TimeSinceTransition, s.Duration));
    }

    public (bool, float) EstimateTimeToNextPositioning(StateMachine sm)
    {
        var s = FindStateData(sm.ActiveState);
        return (s.Positioning.Active, s.Positioning.TransitionIn - Math.Min(sm.TimeSinceTransition, s.Duration));
    }

    public (bool, float) EstimateTimeToNextVulnerable(StateMachine sm)
    {
        var s = FindStateData(sm.ActiveState);
        return (s.Vulnerable.Active, s.Vulnerable.TransitionIn - Math.Min(sm.TimeSinceTransition, s.Duration));
    }

    public uint[] ActiveStrategyOverrides(StateMachine sm)
    {
        var res = new uint[_numStrategyTracks];
        var max = Utils.MakeArray(_numStrategyTracks, float.MinValue);
        var s = FindStateData(sm.ActiveState);
        var t = s != Pull ? s.EnterTime + Math.Min(sm.TimeSinceTransition, s.Duration) : -sm.PrepullTimer;
        foreach (var st in Strategies.Where(st => st.IsActive(t, s) && st.WindowStart >= max[st.Index]))
        {
            res[st.Index] = st.Value;
            max[st.Index] = st.WindowStart;
        }
        return res;
    }

    public IEnumerable<(ActionID Action, float TimeLeft, PlanTarget.ISelector Target, bool LowPriority)> ActiveActions(StateMachine sm)
    {
        var s = FindStateData(sm.ActiveState);
        var t = s != Pull ? s.EnterTime + Math.Min(sm.TimeSinceTransition, s.Duration) : -sm.PrepullTimer;
        return Actions.Where(a => !a.Executed && a.IsActive(t, s)).Select(a => (a.ID, a.WindowEnd - t, a.Target, a.LowPriority));
    }

    public void NotifyActionExecuted(StateMachine sm, ActionID action)
    {
        // TODO: not sure what to do if we have several overlapping requests for same action, do we really mark all of them as executed?..
        var s = FindStateData(sm.ActiveState);
        var t = s != Pull ? s.EnterTime + Math.Min(sm.TimeSinceTransition, s.Duration) : -sm.PrepullTimer;
        foreach (var a in Actions.Where(a => a.ID == action && a.IsActive(t, s)))
            a.Executed = true;
    }

    public uint? ActiveForcedTarget(StateMachine sm)
    {
        var s = FindStateData(sm.ActiveState);
        var t = s != Pull ? s.EnterTime + Math.Min(sm.TimeSinceTransition, s.Duration) : -sm.PrepullTimer;
        return ForcedTargets.Where(o => o.IsActive(t, s)).MaxBy(o => o.WindowStart)?.OID;
    }

    public void Draw(StateMachine sm)
    {
        if (Plan == null)
            return;
        var s = FindStateData(sm.ActiveState);
        var t = s.EnterTime + Math.Min(sm.TimeSinceTransition, s.Duration);
        var classDef = PlanDefinitions.Classes[Plan.Class];
        foreach (var track in classDef.CooldownTracks)
        {
            var next = FindNextActionInTrack(track.Actions.Select(a => a.aid), t, s.BranchID, s.NumBranches);
            if (next == null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, 0x80808080);
                ImGui.TextUnformatted(track.Name);
                ImGui.PopStyleColor();
            }
            else if (next.WindowStart <= t)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ffff);
                ImGui.TextUnformatted($"{track.Name}: use now! ({next.WindowEnd - t:f1}s left)");
                ImGui.PopStyleColor();
            }
            else
            {
                var left = next.WindowStart - t;
                ImGui.PushStyleColor(ImGuiCol.Text, left < classDef.Abilities[track.Actions[0].aid].Cooldown ? 0xffffffff : 0x80808080);
                ImGui.TextUnformatted($"{track.Name}: in {left:f1}s");
                ImGui.PopStyleColor();
            }
        }
    }

    private StateData ProcessState(StateMachineTree tree, StateMachineTree.Node curState, StateData? prev, StateData? nextPhaseStart)
    {
        var curPhase = tree.Phases[curState.PhaseID];

        // phaseLeft < 0 if cur state exit is after expected phase exit
        var phaseLeft = curPhase.Duration - curState.Time;

        var s = States[curState.State.ID] = new();
        s.EnterTime = prev != null ? prev.EnterTime + prev.Duration : curPhase.StartTime;
        s.Duration = Math.Clamp(curState.State.Duration + phaseLeft, 0, curState.State.Duration);
        s.BranchID = curState.BranchID;
        s.NumBranches = curState.NumBranches;
        s.Downtime.Active = curState.IsDowntime;
        s.Positioning.Active = curState.IsPositioning;
        s.Vulnerable.Active = curState.IsVulnerable;

        // process successor states of the same phase
        // note that we might not expect to reach them due to phase timings, but they still might be reached in practice (if we're going slower than intended)
        // in such case all successors will have identical enter-time (equal to enter-time of initial state of next phase) and identical duration (zero)
        if (curState.Successors.Count == 0)
        {
            UpdateTransitions(s, nextPhaseStart);
        }
        else
        {
            foreach (var succ in curState.Successors)
            {
                var succState = ProcessState(tree, succ, s, nextPhaseStart);
                UpdateTransitions(s, succState);
            }
        }

        return s;
    }

    private void UpdateTransitions(StateData s, StateData? next)
    {
        UpdateFlagTransition(ref s.Downtime, next?.Downtime ?? new(true, 10000), s.Duration);
        UpdateFlagTransition(ref s.Positioning, next?.Positioning ?? new(false, 10000), s.Duration);
        UpdateFlagTransition(ref s.Vulnerable, next?.Vulnerable ?? new(false, 10000), s.Duration);
    }

    private void UpdateFlagTransition(ref StateFlag curFlag, StateFlag nextFlag, float curDuration)
    {
        var transition = (curFlag.Active == nextFlag.Active ? nextFlag.TransitionIn : 0) + curDuration;
        curFlag.TransitionIn = Math.Min(curFlag.TransitionIn, transition); // in case state has multiple successors, take minimal time to transition (TODO: is that right?..)
    }

    // note: current implementation won't work well with overlapping windows
    private ActionData? FindNextActionInTrack(IEnumerable<ActionID> filter, float time, int branchID, int numBranches)
    {
        ActionData? res = null;
        foreach (var a in Actions.Where(a => !a.Executed && a.IntersectBranchRange(branchID, numBranches) && a.WindowEnd > time && filter.Contains(a.ID)))
            if (res == null || a.WindowEnd < res.WindowEnd)
                res = a;
        return res;
    }
}
