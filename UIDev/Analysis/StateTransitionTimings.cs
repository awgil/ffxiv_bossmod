using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev.Analysis
{
    class StateTransitionTimings
    {
        class TransitionMetrics
        {
            public int NumTransitions;
            public double MinTime = Double.MaxValue;
            public double MaxTime;
            public double SumTimes;
        }

        class StateMetrics
        {
            public string Name = "";
            public double ExpectedTime;
            public SortedDictionary<int, TransitionMetrics> Transitions = new();
        }

        class EncounterMetrics
        {
            public List<StateMetrics> Metrics;

            public EncounterMetrics(ActiveModuleState referenceState)
            {
                Metrics = new(referenceState.StateMap.Count + 1);
                Metrics.Add(new() { Name = "(null)" });
                for (int i = 0; i < referenceState.StateMap.Count; ++i)
                    Metrics.Add(new());

                foreach (var (state, index) in referenceState.StateMap)
                {
                    var m = Metrics[index];
                    m.Name = $"{state.ID:X} '{state.Name}' ({state.Comment})";
                    m.ExpectedTime = Math.Round(state.Duration, 1);
                }
            }

            public void RecordTransition(int from, int to, double time)
            {
                var m = Metrics[from].Transitions.GetOrAdd(to);
                ++m.NumTransitions;
                m.MinTime = Math.Min(m.MinTime, time);
                m.MaxTime = Math.Max(m.MaxTime, time);
                m.SumTimes += time;
            }
        }

        class ActiveModuleState
        {
            public StateMachine.State? CurState;
            public DateTime StateEnter;
            public Dictionary<StateMachine.State, int> StateMap = new();
            public EncounterMetrics? Metrics;

            public ActiveModuleState(BossModule m)
            {
                if (m.InitialState != null)
                    EnumerateStates(m.InitialState, 1);
            }

            public void Transition(StateMachine.State? state, DateTime time)
            {
                var stateIndex = CurState != null ? StateMap[CurState] : 0;
                var destIndex = state != null ? StateMap[state] : 0;
                Metrics?.RecordTransition(stateIndex, destIndex, (time - StateEnter).TotalSeconds);
                CurState = state;
                StateEnter = time;
            }

            private int EnumerateStates(StateMachine.State state, int index)
            {
                StateMap[state] = index++;

                if (state.Next != null)
                    index = EnumerateStates(state.Next, index);

                if (state.PotentialSuccessors != null)
                    foreach (var s in state.PotentialSuccessors.Where(s => s != state.Next))
                        index = EnumerateStates(s, index);

                return index;
            }
        }

        private Dictionary<uint, EncounterMetrics> _metrics = new();

        public StateTransitionTimings(List<Replay> replays)
        {
            foreach (var replay in replays.Where(r => r.Encounters.Count > 0))
            {
                WorldState ws = new();
                BossModuleManager mgr = new(ws, new());
                Dictionary<BossModule, ActiveModuleState> states = new();
                int nextOp = 0;
                while (nextOp < replay.Ops.Count)
                {
                    ws.CurrentTime = replay.Ops[nextOp].Timestamp;
                    while (nextOp < replay.Ops.Count && replay.Ops[nextOp].Timestamp == ws.CurrentTime)
                        replay.Ops[nextOp++].Redo(ws);
                    mgr.Update();

                    foreach (var m in mgr.ActiveModules)
                    {
                        var s = states.GetValueOrDefault(m);
                        if (s == null)
                        {
                            states[m] = s = new(m);
                            s.Metrics = _metrics.GetValueOrDefault(m.PrimaryActor.OID);
                            if (s.Metrics == null)
                            {
                                _metrics[m.PrimaryActor.OID] = s.Metrics = new(s);
                            }
                        }

                        if (s.CurState != m.StateMachine.ActiveState)
                        {
                            s.Transition(m.StateMachine.ActiveState, ws.CurrentTime);
                        }
                    }
                }
            }
        }

        public void Draw()
        {
            foreach (var (oid, metrics) in _metrics)
            {
                var moduleType = ModuleRegistry.TypeForOID(oid);
                if (ImGui.TreeNode($"{oid:X} ({moduleType?.Name})"))
                {
                    foreach (var from in metrics.Metrics.Skip(1))
                    {
                        foreach (var (toIndex, m) in from.Transitions)
                        {
                            if (toIndex != 0)
                            {
                                var to = metrics.Metrics[toIndex];
                                var avg = m.SumTimes / m.NumTransitions;
                                bool warn = from.ExpectedTime < Math.Round(m.MinTime, 1) || from.ExpectedTime > Math.Round(m.MaxTime, 1);
                                ImGui.PushStyleColor(ImGuiCol.Text, warn ? 0xff00ffff : 0xffffffff);
                                if (ImGui.TreeNodeEx($"{from.Name} -> {to.Name}: avg={avg:f2}-{from.ExpectedTime:f2}={avg - from.ExpectedTime:f2}, [{m.MinTime:f2}, {m.MaxTime:f2}] range, {m.NumTransitions} seen", ImGuiTreeNodeFlags.Leaf))
                                    ImGui.TreePop();
                                ImGui.PopStyleColor();
                            }
                        }
                    }
                    ImGui.TreePop();
                }
            }
        }
    }
}
