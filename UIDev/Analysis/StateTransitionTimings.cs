using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev.Analysis
{
    class StateTransitionTimings
    {
        class TransitionMetric
        {
            public double Duration;
            public Replay Replay;
            public DateTime Time;

            public TransitionMetric(double duration, Replay replay, DateTime time)
            {
                Duration = duration;
                Replay = replay;
                Time = time;
            }
        }

        class TransitionMetrics
        {
            public double MinTime = Double.MaxValue;
            public double MaxTime;
            public double AvgTime;
            public double StdDev;
            public List<TransitionMetric> Instances = new();
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

            public void RecordTransition(int from, int to, TransitionMetric metric)
            {
                Metrics[from].Transitions.GetOrAdd(to).Instances.Add(metric);
            }
        }

        class ActiveModuleState
        {
            public Replay Replay;
            public StateMachine.State? CurState;
            public DateTime StateEnter;
            public Dictionary<StateMachine.State, int> StateMap = new();
            public EncounterMetrics? Metrics;

            public ActiveModuleState(Replay replay, BossModule m)
            {
                Replay = replay;
                if (m.InitialState != null)
                    EnumerateStates(m.InitialState, 1);
            }

            public void Transition(StateMachine.State? state, DateTime time)
            {
                var stateIndex = CurState != null ? StateMap[CurState] : 0;
                var destIndex = state != null ? StateMap[state] : 0;
                Metrics?.RecordTransition(stateIndex, destIndex, new((time - StateEnter).TotalSeconds, Replay, time));
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
                            states[m] = s = new(replay, m);
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

                foreach (var (_, metrics) in _metrics)
                {
                    foreach (var state in metrics.Metrics)
                    {
                        foreach (var (_, trans) in state.Transitions)
                        {
                            trans.Instances.Sort((l, r) => l.Duration.CompareTo(r.Duration));
                            trans.MinTime = trans.Instances.First().Duration;
                            trans.MaxTime = trans.Instances.Last().Duration;
                            double sum = 0, sumSq = 0;
                            foreach (var inst in trans.Instances)
                            {
                                sum += inst.Duration;
                                sumSq += inst.Duration * inst.Duration;
                            }
                            trans.AvgTime = sum / trans.Instances.Count;
                            trans.StdDev = trans.Instances.Count > 0 ? Math.Sqrt((sumSq - sum * sum / trans.Instances.Count) / (trans.Instances.Count - 1)) : 0;
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
                                //bool warn = from.ExpectedTime < Math.Round(m.MinTime, 1) || from.ExpectedTime > Math.Round(m.MaxTime, 1);
                                bool warn = Math.Abs(from.ExpectedTime - m.AvgTime) > Math.Ceiling(m.StdDev * 10) / 10;
                                ImGui.PushStyleColor(ImGuiCol.Text, warn ? 0xff00ffff : 0xffffffff);
                                if (ImGui.TreeNode($"{from.Name} -> {to.Name}: avg={m.AvgTime:f2}-{from.ExpectedTime:f2}={m.AvgTime - from.ExpectedTime:f2} +- {m.StdDev:f2}, [{m.MinTime:f2}, {m.MaxTime:f2}] range, {m.Instances.Count} seen"))
                                {
                                    foreach (var inst in m.Instances)
                                    {
                                        warn = Math.Abs(inst.Duration - m.AvgTime) > m.StdDev;
                                        ImGui.PushStyleColor(ImGuiCol.Text, warn ? 0xff00ffff : 0xffffffff);
                                        if (ImGui.TreeNodeEx($"{inst.Duration:f2}: {inst.Replay.Path} @ {inst.Time:O}", ImGuiTreeNodeFlags.Leaf))
                                            ImGui.TreePop();
                                        ImGui.PopStyleColor();
                                    }
                                    ImGui.TreePop();
                                }
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
