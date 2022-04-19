using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace BossMod
{
    // a lot of boss fights can be modeled as state machines
    // by far the most common state has a single transition to a neighbouring state, and by far the most common transition is spell cast/finish by boss
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
        }

        public class State
        {
            public string Name = ""; // if name is empty, state is "hidden" from UI
            public float Duration = 0; // estimated state duration
            public uint ID = 0;
            public string Comment = "";
            public List<Action> Enter = new(); // callbacks executed when state is activated
            public List<Action> Exit = new(); // callbacks executed when state is deactivated; note that this can happen unexpectedly, e.g. due to external reset
            public Func<float, State?>? Update = null; // callback executed every frame when state is active; should return target state for transition or null to remain in current state; argument = time since activation

            // fields below are used for visualization, autorotations, etc.
            public State? Next = null; // expected next state; note that actual next state is determined by update function
            public State[]? PotentialSuccessors = null; // if null, we consider the only potential successor to be Next; otherwise we use this list instead when building timeline
            public StateHint EndHint = StateHint.None; // special flags for state end
        }

        private DateTime _curTime;
        private DateTime _lastTransition;
        public float TimeSinceTransition => (float)(_curTime - _lastTransition).TotalSeconds;
        public float TimeSinceTransitionClamped => MathF.Min(TimeSinceTransition, ActiveState?.Duration ?? 0);

        private State? _activeState = null;
        public State? ActiveState
        {
            get => _activeState;
            set
            {
                if (_activeState != null)
                {
                    foreach (var cb in _activeState.Exit)
                        cb();
                }

                _activeState = value;

                if (_activeState != null)
                {
                    foreach (var cb in _activeState.Enter)
                        cb();
                }

                _lastTransition = _curTime;
            }
        }

        public void Update(DateTime now)
        {
            _curTime = now;
            while (ActiveState != null)
            {
                var transition = ActiveState.Update?.Invoke(TimeSinceTransition);
                if (transition == null)
                    break;
                Service.Log($"[StateMachine] Transition from {ActiveState.ID:X} '{ActiveState.Name}' to {transition.ID:X} '{transition.Name}', overdue={TimeSinceTransition:f2}-{ActiveState.Duration:f2}={TimeSinceTransition - ActiveState.Duration:f2}");
                ActiveState = transition;
            }
        }

        public void Draw()
        {
            (var activeName, var next) = BuildComplexStateNameAndDuration(ActiveState, TimeSinceTransition, true);
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

        private (string, State?) BuildComplexStateNameAndDuration(State? start, float timeActive, bool writeTime)
        {
            if (start == null)
                return ("Inactive", null);

            var res = new StringBuilder(start.Name);
            var timeLeft = MathF.Max(0, start.Duration - timeActive);
            if (writeTime && res.Length > 0)
            {
                writeTime = false;
                res.Append($" in {timeLeft:f1}s");
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

                    if (writeTime)
                    {
                        writeTime = false;
                        res.Append($" in {timeLeft:f1}s");
                    }
                }
            }

            if (writeTime)
            {
                if (res.Length == 0)
                    res.Append("???");
                res.Append($" in {timeLeft:f1}s");
            }

            return (res.ToString(), start.Next);
        }
    }
}
