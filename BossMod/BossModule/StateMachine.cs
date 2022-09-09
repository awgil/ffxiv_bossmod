using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BossMod
{
    // a lot of boss fights can be modeled as state machines
    // by far the most common state has a single transition to a neighbouring state, and by far the most common transition is spell cast/finish by boss
    // some bosses have multiple "phases"; when phase condition is triggered, initial state of the next phase is activated
    // typical phase condition is boss reaching specific hp %
    public class StateMachine
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

        public class State
        {
            public string Name = ""; // if name is empty, state is "hidden" from UI
            public float Duration = 0; // estimated state duration
            public uint ID = 0;
            public string Comment = "";
            public List<Action> Enter = new(); // callbacks executed when state is activated
            public List<Action> Exit = new(); // callbacks executed when state is deactivated; note that this can happen unexpectedly, e.g. due to external reset or phase change
            public Func<float, State?>? Update = null; // callback executed every frame when state is active; should return target state for transition or null to remain in current state; argument = time since activation

            // fields below are used for visualization, autorotations, etc.
            public State? Next = null; // expected next state; note that actual next state is determined by update function
            public State[]? PotentialSuccessors = null; // if null, we consider the only potential successor to be Next; otherwise we use this list instead when building timeline
            public StateHint EndHint = StateHint.None; // special flags for state end
        }

        public class Phase
        {
            public State InitialState;
            public string Name;
            public float ExpectedDuration; // if >= 0, this is 'expected' phase duration (for use by CD planner etc); <0 means 'calculate from state tree max time'
            public List<Action> Enter = new(); // callbacks executed when phase is activated
            public List<Action> Exit = new(); // callbacks executed when phase is deactivated; note that this can happen unexpectedly, e.g. due to external reset
            public Func<bool>? Update; // callback executed every frame when phase is active; should return whether transition to next phase should happen

            public Phase(State initialState, string name, float expectedDuration = -1)
            {
                InitialState = initialState;
                Name = name;
                ExpectedDuration = expectedDuration;
            }
        }

        public List<Phase> Phases { get; private init; }

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

        public StateMachine(List<Phase> phases)
        {
            Phases = phases;
        }

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
                var transition = ActiveState.Update?.Invoke(TimeSinceTransition);
                if (transition == null)
                    break;
                Service.Log($"[StateMachine] State transition from {ActiveState.ID:X} '{ActiveState.Name}' to {transition.ID:X} '{transition.Name}', overdue={TimeSinceTransition:f2}-{ActiveState.Duration:f2}={TimeSinceTransition - ActiveState.Duration:f2}");
                TransitionToState(transition);
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

        private (string, State?) BuildComplexStateNameAndDuration(State start, float timeActive, bool writeTime)
        {
            var res = new StringBuilder(start.Name);
            var timeLeft = MathF.Max(0, start.Duration - timeActive);
            if (writeTime && res.Length > 0)
            {
                res.Append($" in {timeLeft:f1}s");
                timeLeft = 0;
            }

            while (start.EndHint.HasFlag(StateHint.GroupWithNext) && start.Next != null)
            {
                start = start.Next;
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

            return (res.ToString(), start.Next);
        }

        private void TransitionToPhase(int nextIndex)
        {
            if (ActivePhase != null)
            {
                TransitionToState(null);
                foreach (var cb in ActivePhase.Exit)
                    cb();
            }

            ActivePhaseIndex = nextIndex;
            _phaseEnter = _curTime;

            if (ActivePhase != null)
            {
                foreach (var cb in ActivePhase.Enter)
                    cb();
                TransitionToState(ActivePhase.InitialState);
            }
        }

        private void TransitionToState(State? nextState)
        {
            if (ActiveState != null)
                foreach (var cb in ActiveState.Exit)
                    cb();

            ActiveState = nextState;
            _lastTransition = _curTime;

            if (ActiveState != null)
                foreach (var cb in ActiveState.Enter)
                    cb();
        }
    }
}
