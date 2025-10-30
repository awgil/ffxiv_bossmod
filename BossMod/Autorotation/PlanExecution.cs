namespace BossMod.Autorotation;

// to execute a concrete plan for a concrete encounter, we build a "timeline" - for each state we assign a time/duration (which depend both on state machine definition and plan's phase timings)
// for each action defined by a plan, we define start/end times on that timeline (plus a range of branches in which it might be executed)
public sealed class PlanExecution
{
    public record struct StateFlag(bool Active, float TransitionIn);

    public record class StateData
    {
        public float EnterTime;
        public float Duration;
        public int BranchID;
        public int NumBranches;
        public StateFlag Downtime = new(false, float.MaxValue);
        public StateFlag Positioning = new(false, float.MaxValue);
        public StateFlag Vulnerable = new(false, float.MaxValue);
    }

    public record class EntryData(float WindowStart, float WindowEnd, int BranchID, int NumBranches, StrategyValue Value)
    {
        public bool IntersectBranchRange(int branchID, int numBranches) => BranchID < branchID + numBranches && branchID < BranchID + NumBranches;
        public bool IsActive(float t, StateData s) => t >= WindowStart && t <= WindowEnd && IntersectBranchRange(s.BranchID, s.NumBranches);
    }

    public readonly record struct ModuleData(Type Type, RotationModuleDefinition Definition, List<List<EntryData>> Tracks, List<StrategyValueTrack> Defaults);

    public readonly BossModule Module;
    public readonly Plan? Plan;
    private readonly StateData Pull;
    private readonly Dictionary<uint, StateData> States = [];
    private readonly List<ModuleData> Strategies = [];
    private readonly List<EntryData> ForcedTargets = [];

    public PlanExecution(BossModule module, Plan? plan)
    {
        Module = module;
        Plan = plan;
        Pull = new() { EnterTime = -30, Duration = 30, NumBranches = 1 };

        var tree = new StateMachineTree(module.StateMachine);
        tree.ApplyTimings(plan?.PhaseDurations);

        StateData? nextPhaseStart = null;
        for (int i = tree.Phases.Count - 1; i >= 0; i--)
            nextPhaseStart = ProcessState(tree, tree.Phases[i].StartingNode, null, nextPhaseStart);
        UpdateTransitions(Pull, nextPhaseStart);

        if (plan != null)
        {
            Strategies = [.. plan.Modules.Select(m => new ModuleData(m.Type, m.Definition, [.. m.Tracks.Select(BuildEntries)], m.Defaults))];
            ForcedTargets = BuildEntries(plan.Targeting);
        }
    }

    public StateData FindCurrentStateData() => Module.StateMachine.ActiveState != null ? States[Module.StateMachine.ActiveState.ID] : Pull;

    // get current virtual time on the timeline corresponding to current state
    // it could be different if fight proceeds differently from what was planned (eg different phase durations)
    public float GetVirtualTime(StateData currentState) => currentState != Pull
        ? currentState.EnterTime + Math.Min(Module.StateMachine.TimeSinceTransition, currentState.Duration)
        : -Math.Max(Module.WorldState.Client.CountdownRemaining ?? 10000, 0.001f);

    // all such functions return whether flag is currently active + estimated time to transition
    public (bool, float) EstimateTimeToNextDowntime()
    {
        var s = FindCurrentStateData();
        return (s.Downtime.Active, s.Downtime.TransitionIn - Math.Min(Module.StateMachine.TimeSinceTransition, s.Duration));
    }

    public (bool, float) EstimateTimeToNextPositioning()
    {
        var s = FindCurrentStateData();
        return (s.Positioning.Active, s.Positioning.TransitionIn - Math.Min(Module.StateMachine.TimeSinceTransition, s.Duration));
    }

    public (bool, float) EstimateTimeToNextVulnerable()
    {
        var s = FindCurrentStateData();
        return (s.Vulnerable.Active, s.Vulnerable.TransitionIn - Math.Min(Module.StateMachine.TimeSinceTransition, s.Duration));
    }

    public StrategyValues ActiveStrategyOverrides(int moduleIndex)
    {
        var s = FindCurrentStateData();
        var t = GetVirtualTime(s);
        var data = Strategies[moduleIndex];
        var res = new StrategyValues(data.Definition.Configs);
        for (int i = 0; i < data.Tracks.Count; ++i)
        {
            // set global default
            res.Values[i] = data.Defaults[i];
            // then override with entries
            var entry = GetEntryAt(data.Tracks[i], t, s);
            if (entry != null)
                res.Values[i] = entry.Value with { ExpireIn = entry.WindowEnd - t };
        }
        return res;
    }

    public StrategyValueTrack? ActiveForcedTarget()
    {
        var s = FindCurrentStateData();
        var t = GetVirtualTime(s);
        return GetEntryAt(ForcedTargets, t, s)?.Value as StrategyValueTrack;
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

    private List<EntryData> BuildEntries(List<Plan.Entry> entries)
    {
        List<EntryData> res = [];
        foreach (var entry in entries)
        {
            if (!entry.Disabled)
            {
                var s = States.GetValueOrDefault(entry.StateID);
                if (s != null)
                {
                    var windowStart = s.EnterTime + Math.Min(s.Duration, entry.TimeSinceActivation);
                    res.Add(new(windowStart, windowStart + entry.WindowLength, s.BranchID, s.NumBranches, entry.Value));
                }
                else
                {
                    Service.Log($"Failed to find state {entry.StateID:X} for plan {Plan?.Guid}");
                }
            }
        }
        return res;
    }

    // if there are several intersecting entries, select one with biggest windowstart
    private EntryData? GetEntryAt(List<EntryData> entries, float t, StateData s) => entries.Where(e => e.IsActive(t, s)).MaxBy(s => s.WindowStart);
}
