using Dalamud.Bindings.ImGui;
using System.Text;

namespace BossMod;

// a lot of boss fights can be modeled as state machines
// by far the most common state has a single transition to a neighbouring state, and by far the most common transition is spell cast/finish by boss
// some bosses have multiple "phases"; when phase condition is triggered, initial state of the next phase is activated
// typical phase condition is boss reaching specific hp %
public class StateMachine(List<StateMachine.Phase> phases)
{
    [Flags]
    public enum StateHint
    {
        None = 0,
        GroupWithNext = 1 << 0, // this state is a logical 'substate' - it should be grouped together with next one for visualization
        BossCastStart = 1 << 1, // state ends when boss starts some cast
        BossCastEnd = 1 << 2, // state ends when boss ends a cast
        Tankbuster = 1 << 3, // state end is a tankbuster event - tank has to press some save
        Raidwide = 1 << 4, // state end is a raidwide damage event - someone better press some save
        Knockback = 1 << 5, // state end is a knockback event - it's a good time to use arm's length, or otherwise avoid being knocked into voidzones/walls/etc.
        DowntimeStart = 1 << 6, // at state end downtime starts - there are no targets to damage
        DowntimeEnd = 1 << 7, // at state end downtime ends
        PositioningStart = 1 << 8, // at state end a phase with movement or precise positioning requirements starts - we should be careful with charges etc.
        PositioningEnd = 1 << 9, // at state end positioning requirements are relaxed
        VulnerableStart = 1 << 10, // at state end some target becomes vulnerable and takes extra damage
        VulnerableEnd = 1 << 11, // at state end vulnerability phase ends
    }

    [Flags]
    public enum PhaseHint
    {
        None = 0,
        StartWithDowntime = 1 << 0, // the phase starts with downtime
    }

    public class State
    {
        public uint ID;
        public float Duration; // estimated state duration
        public string Name = ""; // if name is empty, state is "hidden" from UI
        public string Comment = "";
        public Action? Enter; // callbacks executed when state is activated
        public Action? Exit; // callbacks executed when state is deactivated; note that this can happen unexpectedly, e.g. due to external reset or phase change
        public Func<float, int>? Update; // callback executed every frame when state is active; should return target state index for transition or -1 to remain in current state; argument = time since activation
        public State[]? NextStates; // potential successor states
        public StateHint EndHint = StateHint.None; // special flags for state end (used for visualization, autorotation, etc.)
    }

    public class Phase(State initialState, string name, float expectedDuration = -1)
    {
        public State InitialState = initialState;
        public string Name = name;
        public float ExpectedDuration = expectedDuration; // if >= 0, this is 'expected' phase duration (for use by CD planner etc); <0 means 'calculate from state tree max time'
        public Action? Enter; // callbacks executed when phase is activated
        public Action? Exit; // callbacks executed when phase is deactivated; note that this can happen unexpectedly, e.g. due to external reset
        public Func<bool>? Update; // callback executed every frame when phase is active; should return whether transition to next phase should happen
        public PhaseHint Hint = PhaseHint.None; // special flags for phase
    }

    public List<Phase> Phases { get; private init; } = phases;

    private DateTime _curTime;
    private DateTime _activation;
    private DateTime _phaseEnter;
    private DateTime _lastTransition;
    public float TimeSinceActivation => (float)(_curTime - _activation).TotalSeconds;
    public float TimeSincePhaseEnter => (float)(_curTime - _phaseEnter).TotalSeconds;
    public float TimeSinceTransition => (float)(_curTime - _lastTransition).TotalSeconds;
    public float TimeSinceTransitionClamped => MathF.Min(TimeSinceTransition, ActiveState?.Duration ?? 0);

    public int ActivePhaseIndex { get; private set; } = -1;
    public Phase? ActivePhase => Phases.ElementAtOrDefault(ActivePhaseIndex);
    public State? ActiveState { get; private set; }

    public void Start(DateTime now)
    {
        _activation = _curTime = now;
        if (Phases.Count > 0)
            TransitionToPhase(0);
    }

    public void Reset()
    {
        TransitionToPhase(-1);
    }

    public void Update(DateTime now)
    {
        _curTime = now;
        while (ActivePhase != null)
        {
            bool transition = ActivePhase.Update?.Invoke() ?? false;
            if (!transition)
                break;
            Service.Log($"[StateMachine] Phase transition from {ActivePhaseIndex} '{ActivePhase.Name}', time={TimeSincePhaseEnter:f2}");
            TransitionToPhase(ActivePhaseIndex + 1);
        }
        while (ActiveState != null)
        {
            var transition = ActiveState.Update?.Invoke(TimeSinceTransition) ?? -1;
            var nextState = ActiveState.NextStates != null && transition >= 0 && transition < ActiveState.NextStates.Length ? ActiveState.NextStates[transition] : null;
            if (nextState == null)
                break;
            Service.Log($"[StateMachine] State transition from {ActiveState.ID:X} '{ActiveState.Name}' to {nextState.ID:X} '{nextState.Name}', overdue={TimeSinceTransition:f2}-{ActiveState.Duration:f2}={TimeSinceTransition - ActiveState.Duration:f2}");
            TransitionToState(nextState);
        }
    }

    public void Draw()
    {
        (var activeName, var next) = ActiveState != null ? BuildComplexStateNameAndDuration(ActiveState, TimeSinceTransition, true) : ("Inactive", null);
        ImGui.TextUnformatted($"Cur: {activeName}");

        var future = BuildStateChain(next, " ---> ");
        if (future.Length == 0)
        {
            ImGui.TextUnformatted("");
        }
        else
        {
            ImGui.TextUnformatted($"Then: {future}");
        }
    }

    public string BuildStateChain(State? start, string sep, int maxCount = 5)
    {
        int count = 0;
        var res = new StringBuilder();
        while (start != null && count < maxCount)
        {
            (var name, var next) = BuildComplexStateNameAndDuration(start, 0, false);
            if (name.Length > 0)
            {
                if (res.Length > 0)
                    res.Append(sep);
                res.Append(name);
                ++count;
            }
            start = next;
        }
        return res.ToString();
    }

    public DateTime NextTransitionWithFlag(StateHint flag)
    {
        var time = _lastTransition;
        var next = ActiveState;
        while (next != null)
        {
            time = time.AddSeconds(next.Duration);
            if (next.EndHint.HasFlag(flag))
                return time;
            next = next.NextStates?.Length == 1 ? next.NextStates[0] : null;
        }
        return DateTime.MaxValue;
    }

    private (string, State?) BuildComplexStateNameAndDuration(State start, float timeActive, bool writeTime)
    {
        var res = new StringBuilder(start.Name);
        var timeLeft = MathF.Max(0, start.Duration - timeActive);
        if (writeTime && res.Length > 0)
        {
            res.Append($" in {timeLeft:f1}s");
            timeLeft = 0;
        }

        while (start.EndHint.HasFlag(StateHint.GroupWithNext) && start.NextStates?.Length == 1)
        {
            start = start.NextStates[0];
            timeLeft += MathF.Max(0, start.Duration);
            if (start.Name.Length > 0)
            {
                if (res.Length > 0)
                    res.Append(" + ");
                res.Append(start.Name);

                if (writeTime && timeLeft > 0)
                {
                    res.Append($" in {timeLeft:f1}s");
                    timeLeft = 0;
                }
            }
        }

        if (writeTime && timeLeft > 0)
        {
            if (res.Length == 0)
                res.Append("???");
            res.Append($" in {timeLeft:f1}s");
        }

        return (res.ToString(), start.NextStates?.Length == 1 ? start.NextStates[0] : null);
    }

    private void TransitionToPhase(int nextIndex)
    {
        if (ActivePhase != null)
            TransitionToState(null);

        ActivePhase?.Exit?.Invoke();
        ActivePhaseIndex = nextIndex;
        _phaseEnter = _curTime;
        ActivePhase?.Enter?.Invoke();

        if (ActivePhase != null)
            TransitionToState(ActivePhase.InitialState);
    }

    private void TransitionToState(State? nextState)
    {
        ActiveState?.Exit?.Invoke();
        ActiveState = nextState;
        _lastTransition = _curTime;
        ActiveState?.Enter?.Invoke();
    }
}
