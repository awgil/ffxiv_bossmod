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
            public string Name;
            public double ExpectedTime;
            public SortedDictionary<uint, TransitionMetrics> Transitions = new();

            public StateMetrics(string name, double expectedTime)
            {
                Name = name;
                ExpectedTime = expectedTime;
            }
        }

        class EncounterError
        {
            public Replay Replay;
            public DateTime Timestamp;
            public Type? CompType;
            public string Message;

            public EncounterError(Replay replay, DateTime timestamp, Type? compType, string message)
            {
                Replay = replay;
                Timestamp = timestamp;
                CompType = compType;
                Message = message;
            }
        }

        class BossModuleManagerWrapper : BossModuleManager
        {
            private StateTransitionTimings _self;
            private Replay _replay;

            public BossModuleManagerWrapper(StateTransitionTimings self, Replay replay, WorldState ws)
                : base(ws, new())
            {
                _self = self;
                _replay = replay;
            }

            public override void HandleError(BossModule module, BossModule.Component? comp, string message)
            {
                _self._errors.Add(new(_replay, module.WorldState.CurrentTime, comp?.GetType(), message));
            }
        }

        private SortedDictionary<uint, StateMetrics> _metrics = new();
        private List<EncounterError> _errors = new();

        public StateTransitionTimings(List<Replay> replays, uint oid)
        {
            foreach (var replay in replays)
            {
                ReplayPlayer player = new(replay);
                foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
                {
                    if (player.WorldState.CurrentTime > enc.Time.Start)
                        player.Reset();
                    player.AdvanceTo(enc.Time.Start, () => { });

                    var bmm = new BossModuleManagerWrapper(this, replay, player.WorldState);
                    var m = bmm.ActiveModules.FirstOrDefault(m => m.PrimaryActor.InstanceID == enc.InstanceID);
                    if (m == null)
                        continue;

                    StateMachine.State? prevState = m.StateMachine?.ActiveState;
                    DateTime prevStateEnter = player.WorldState.CurrentTime;
                    while (player.TickForward() && player.WorldState.CurrentTime <= enc.Time.End)
                    {
                        m.Update();
                        if (m.StateMachine?.ActiveState == null)
                            break;

                        if (prevState != m.StateMachine.ActiveState)
                        {
                            if (prevState != null)
                            {
                                RecordTransition(replay, prevState, prevStateEnter, m.StateMachine.ActiveState, player.WorldState.CurrentTime);
                            }

                            prevState = m.StateMachine.ActiveState;
                            prevStateEnter = player.WorldState.CurrentTime;
                        }
                    }
                }
            }

            foreach (var (_, state) in _metrics)
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

        public void Draw(Tree tree)
        {
            foreach (var n in tree.Node("Errors", _errors.Count == 0))
            {
                tree.LeafNodes(_errors, error => $"{error.Replay.Path} @ {error.Timestamp:O} [{error.CompType}] {error.Message}");
            }

            foreach (var from in _metrics.Values)
            {
                foreach (var (toID, m) in from.Transitions)
                {
                    //bool warn = from.ExpectedTime < Math.Round(m.MinTime, 1) || from.ExpectedTime > Math.Round(m.MaxTime, 1);
                    bool warn = Math.Abs(from.ExpectedTime - m.AvgTime) > Math.Ceiling(m.StdDev * 10) / 10;
                    ImGui.PushStyleColor(ImGuiCol.Text, warn ? 0xff00ffff : 0xffffffff);
                    foreach (var tn in tree.Node($"{from.Name} -> {_metrics[toID].Name}: avg={m.AvgTime:f2}-{from.ExpectedTime:f2}={m.AvgTime - from.ExpectedTime:f2} +- {m.StdDev:f2}, [{m.MinTime:f2}, {m.MaxTime:f2}] range, {m.Instances.Count} seen"))
                    {
                        foreach (var inst in m.Instances)
                        {
                            warn = Math.Abs(inst.Duration - m.AvgTime) > m.StdDev;
                            ImGui.PushStyleColor(ImGuiCol.Text, warn ? 0xff00ffff : 0xffffffff);
                            tree.LeafNode($"{inst.Duration:f2}: {inst.Replay.Path} @ {inst.Time:O}");
                            ImGui.PopStyleColor();
                        }
                    }
                    ImGui.PopStyleColor();
                }
            }
        }

        private void RecordTransition(Replay replay, StateMachine.State prev, DateTime prevEnter, StateMachine.State cur, DateTime curEnter)
        {
            var m = MetricsForState(prev);
            MetricsForState(cur); // ensure state entry exists, even if there are no transitions from it
            m.Transitions.GetOrAdd(cur.ID).Instances.Add(new TransitionMetric((curEnter - prevEnter).TotalSeconds, replay, curEnter));
        }

        private StateMetrics MetricsForState(StateMachine.State s)
        {
            var m = _metrics.GetValueOrDefault(s.ID);
            if (m == null)
                m = _metrics[s.ID] = new StateMetrics($"{s.ID:X} '{s.Name}' ({s.Comment})", Math.Round(s.Duration, 1));
            return m;
        }
    }
}
