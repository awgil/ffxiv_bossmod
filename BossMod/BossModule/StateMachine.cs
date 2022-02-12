using ImGuiNET;
using System;
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
            public State? Next = null; // next state; note that this could be changed freely when needed
            public bool Active = false; // this is updated automatically by the framework when state is entered or exited
            public bool Done = false; // when set, on next update framework will automatically transition to next state
            public Action? Enter = null; // callback executed when state is activated
            public Action? Exit = null; // callback executed when state is deactivated; note that this can happen unexpectedly, e.g. due to external reset
            public Action<float>? Update = null; // callback executed every frame when state is active; should return whether transition to next state should happen; argument = time since activation

            // fields below are used for visualization, autorotations, etc.
            public State[]? PotentialSuccessors = null; // if null, we consider the only potential successor to be Next; otherwise we use this list instead when building timelie
            public StateHint EndHint = StateHint.None; // special flags for state end
        }

        private DateTime _curTime;
        private DateTime _lastTransition;
        private float? _pauseTime;
        public float TimeSinceTransition => _pauseTime ?? (float)(_curTime - _lastTransition).TotalSeconds;
        public bool Paused
        {
            get => _pauseTime != null;
            set
            {
                if (value && _pauseTime == null)
                {
                    _pauseTime = TimeSinceTransition;
                }
                else if (!value && _pauseTime != null)
                {
                    _lastTransition = _curTime - TimeSpan.FromSeconds(_pauseTime.Value);
                    _pauseTime = null;
                }
            }
        }

        private State? _activeState = null;
        public State? ActiveState
        {
            get => _activeState;
            set
            {
                if (_activeState != null)
                {
                    _activeState.Exit?.Invoke();
                    _activeState.Active = false;
                    _activeState.Done = false;
                }
                _activeState = value;
                if (_activeState != null)
                {
                    _activeState.Active = true;
                    _activeState.Done = false;
                    _activeState.Enter?.Invoke();
                }
                _lastTransition = _curTime;
                if (Paused)
                    _pauseTime = 0;
            }
        }

        public void Update(DateTime now)
        {
            _curTime = now;
            if (Paused)
                return;

            while (ActiveState != null)
            {
                ActiveState.Update?.Invoke(TimeSinceTransition);
                if (!ActiveState.Done)
                    break;
                Service.Log($"[StateMachine] Transition from '{ActiveState.Name}' to '{(ActiveState.Next?.Name ?? "null")}', overdue={TimeSinceTransition:f2}-{ActiveState.Duration:f2}={TimeSinceTransition - ActiveState.Duration:f2}");
                ActiveState = ActiveState.Next;
            }
        }

        public void Draw()
        {
            (var activeName, var next) = BuildComplexStateNameAndDuration(ActiveState, TimeSinceTransition, true);
            ImGui.Text($"Cur: {activeName}");

            var future = BuildStateChain(next, " ---> ");
            if (future.Length == 0)
            {
                ImGui.Text("");
            }
            else
            {
                ImGui.Text($"Then: {future}");
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
