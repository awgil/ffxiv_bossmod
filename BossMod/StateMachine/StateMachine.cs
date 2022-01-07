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
        public interface IState
        {
            public int? Update(); // should return null if it wants to remain active, otherwise ID of the next state
            public string Name();
            public double EstimateTimeToTransition(); // we might call it for future state that is not yet active...
            public int? PredictedNextState(); // called to predict future states, return null if it's impossible
            public bool DrawHint(); // we always first try drawing hint for active state, if it returns false - try drawing hint for next state
            public void Reset(StateMachine sm) {}
        }

        public class Desc
        {
            public Dictionary<int, IState> States = new();
            public int NextID = 0;

            public int ReserveIDRange(int size)
            {
                int start = NextID;
                NextID += size;
                return start;
            }

            // when building typical state, it will be assigned to NextID, and its transition to NextID + 1
            public int NextTransitionID => NextID + 1;

            public T Add<T>(T state) where T : IState
            {
                States[NextID++] = state;
                return state;
            }
        }

        // initial state should have id 0, we automatically transition to it on pull
        private Desc _desc;
        public IReadOnlyDictionary<int, IState> States => _desc.States;

        public IState? ActiveState { get; private set; }
        public IState? NextState => ActiveState != null ? FindState(ActiveState.PredictedNextState()) : null;

        public IState? FindState(int? id)
        {
            IState? s = null;
            if (id != null)
                States.TryGetValue(id.Value, out s);
            return s;
        }

        public StateMachine(Desc desc)
        {
            _desc = desc;
            Reset();
        }

        public void Reset()
        {
            ActiveState = null;
            foreach (var s in States)
            {
                s.Value.Reset(this);
            }
        }

        public void Start(int initialState = 0)
        {
            if (ActiveState != null)
                return;
            ActiveState = FindState(initialState);
        }

        public void Update()
        {
            while (ActiveState != null)
            {
                int? transition = ActiveState.Update();
                if (transition == null)
                    return; // nothing more to update...

                // perform state transition
                ActiveState = FindState(transition.Value);
                if (ActiveState == null)
                {
                    Reset();
                }
            }
        }

        public void Draw()
        {
            ImGui.Text($"Cur: {ActiveString()}");

            var future = BuildStateChain(NextState, " ---> ");
            if (future.Length == 0)
            {
                ImGui.Text("");
            }
            else
            {
                ImGui.Text($"Then: {future}");
            }

            var hintState = ActiveState;
            while (hintState != null && !hintState.DrawHint())
            {
                hintState = FindState(hintState.PredictedNextState());
            }
        }

        public string ActiveString()
        {
            if (ActiveState == null)
                return "Inactive";

            var activeName = ActiveState.Name();
            var timeLeft = ActiveState.EstimateTimeToTransition();
            return timeLeft >= 0 ? $"{activeName} in {timeLeft:f1}s" : activeName;
        }

        public string BuildStateChain(IState? start, string sep, int maxCount = 5)
        {
            int count = 0;
            var res = new StringBuilder();
            while (start != null && count < maxCount)
            {
                var name = start.Name();
                if (name.Length > 0)
                {
                    if (res.Length > 0)
                        res.Append(sep);
                    res.Append(name);
                    ++count;
                }
                start = FindState(start.PredictedNextState());
            }
            return res.ToString();
        }
    }
}
