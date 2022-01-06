using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // a lot of boss fights can be modeled as state machines
    // by far the most common state has a single transition to a neighbouring state, and by far the most common transition is spell cast by boss
    // state represents single meaningful mechanic in a boss fight - simplest example could be an aoe or a tankbuster
    // more complex states (e.g. representing complex unique overlap of abilities) can e.g. have internal substates and transitions - this level knows nothing about it
    // state (particularly complex one) could draw custom state-specific hints - but take care not to create layout surprises e.g. during state transitions
    public class StateMachine
    {
        public interface ITransition
        {
            public bool CanActivate() { return true; } // when state is entered, it tries activating all transitions in order
            public bool CanTransition(double timeSinceActivation); // when transition is active, we check this every frame
            public double EstimateTimeToTransition(double timeSinceActivation); // if transition is active but not ready, we call this to get time estimate
            public int DestinationState(); // this can be called even for inactive transition, e.g. if we're building 'next' hint; return -1 if it can't be determined; if transition was activated, this should always be valid
            public void Reset() {} // called when state machine is reset, this is a chance to clean up any internal state
        }

        public class State
        {
            public string Name = "";
            public List<ITransition> Transitions = new();

            public virtual void DrawHint(double timeSinceActivation) {}
            public virtual void Reset() {}
        }

        // initial state should have id 0, we automatically transition to it on pull
        public Dictionary<int, State> States = new();
        public State? ActiveState { get; private set; }
        public ITransition? ActiveTransition { get; private set; }
        private DateTime _stateActivation = DateTime.Now;
        private DateTime _transActivation = DateTime.Now;

        public void Reset()
        {
            ActiveState = null;
            ActiveTransition = null;
            foreach (var s in States)
            {
                s.Value.Reset();
                foreach (var t in s.Value.Transitions)
                    t.Reset();
            }
        }

        public void Start(int initialState = 0)
        {
            if (ActiveState != null)
                return;
            ActiveState = States[initialState];
            _stateActivation = DateTime.Now;
        }

        public void Update()
        {
            while (ActiveState != null)
            {
                if (ActiveTransition == null)
                {
                    foreach (var trans in ActiveState.Transitions)
                    {
                        if (trans.CanActivate())
                        {
                            ActiveTransition = trans;
                            _transActivation = DateTime.Now;
                            break;
                        }
                    }
                }

                if (ActiveTransition == null || !ActiveTransition.CanTransition((DateTime.Now - _transActivation).TotalSeconds))
                    return; // nothing more to update...

                // perform state transition
                ActiveState = States[ActiveTransition.DestinationState()];
                _stateActivation = DateTime.Now;
                ActiveTransition = null;
            }
        }

        public void Draw()
        {
            if (ActiveState == null)
            {
                ImGui.Text("Inactive");
                return;
            }

            if (ActiveTransition == null)
            {
                ImGui.Text($"Next: {BuildForkName(ActiveState)}");
                ImGui.Text("");
            }
            else
            {
                var destState = States[ActiveTransition.DestinationState()];
                var timeLeft = Math.Max(0, ActiveTransition.EstimateTimeToTransition((DateTime.Now - _transActivation).TotalSeconds));
                ImGui.Text($"Next: {destState.Name} in {timeLeft:f1}s");

                int futureCount = 0;
                (var futureName, var futureState) = GuessNextState(destState);
                var futureText = new StringBuilder(futureName);
                while (futureState != null && futureCount < 5)
                {
                    (futureName, futureState) = GuessNextState(futureState);
                    if (futureName.Length > 0)
                        futureText.Append($" ---> {futureName}");
                    ++futureCount;
                }
                ImGui.Text($"Then: {futureText}");
            }

            ActiveState.DrawHint((DateTime.Now - _stateActivation).TotalSeconds);
        }

        private (string, State?) GuessNextState(State prev)
        {
            if (prev.Transitions.Count != 1)
                return (BuildForkName(prev), null);
            State? next;
            States.TryGetValue(prev.Transitions.First().DestinationState(), out next);
            return (next?.Name ?? "", next);
        }

        private string BuildForkName(State fork)
        {
            var res = new StringBuilder();
            foreach (var t in fork.Transitions)
            {
                State? next;
                States.TryGetValue(t.DestinationState(), out next);

                if (res.Length > 0)
                    res.Append(" -or- ");
                res.Append(next?.Name ?? "???");
            }
            return res.ToString();
        }
    }
}
