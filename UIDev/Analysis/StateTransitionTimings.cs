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
            public Replay.Encounter Encounter;
            public DateTime Time;

            public TransitionMetric(double duration, Replay replay, Replay.Encounter encounter, DateTime time)
            {
                Duration = duration;
                Replay = replay;
                Encounter = encounter;
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

        class MetricsToRemove
        {
            public List<(StateMetrics state, uint to, TransitionMetrics trans, TransitionMetric metric)> Instances = new();
            public List<(StateMetrics state, uint to)> Transitions = new();
        }

        private SortedDictionary<uint, StateMetrics> _metrics = new();
        private List<(Replay, Replay.Encounter, Replay.EncounterError)> _errors = new();
        private List<(Replay, Replay.Encounter)> _encounters = new();

        public StateTransitionTimings(List<Replay> replays, uint oid)
        {
            foreach (var replay in replays)
            {
                foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
                {
                    _encounters.Add((replay, enc));

                    foreach (var s in enc.States)
                    {
                        if (!_metrics.ContainsKey(s.ID))
                        {
                            _metrics[s.ID] = new StateMetrics(s.FullName, Math.Round(s.ExpectedDuration, 1));
                        }
                    }

                    var enter = enc.Time.Start;
                    for (int i = 0; i < enc.States.Count; ++i)
                    {
                        var from = enc.States[i];
                        _metrics[from.ID].Transitions.GetOrAdd(i < enc.States.Count - 1 ? enc.States[i + 1].ID : uint.MaxValue).Instances.Add(new TransitionMetric((from.Exit - enter).TotalSeconds, replay, enc, enter));
                        enter = from.Exit;
                    }

                    foreach (var e in enc.Errors)
                    {
                        _errors.Add((replay, enc, e));
                    }
                }
            }

            foreach (var (_, state) in _metrics)
            {
                foreach (var (_, trans) in state.Transitions)
                {
                    trans.Instances.SortBy(e => e.Duration);
                    RecalculateMetrics(trans);
                }
            }

            _encounters.SortByReverse(e => e.Item2.Time.Duration);
        }

        public void Draw(UITree tree)
        {
            foreach (var n in tree.Node("Encounters", _encounters.Count == 0))
            {
                tree.LeafNodes(_encounters, e => $"{e.Item1.Path} @ {e.Item2.Time.Start:O} ({e.Item2.Time.Duration:f3}s)");
            }

            foreach (var n in tree.Node("Errors", _errors.Count == 0))
            {
                tree.LeafNodes(_errors, error => $"{LocationString(error.Item1, error.Item2, error.Item3.Timestamp)} [{error.Item3.CompType}] {error.Item3.Message}");
            }

            MetricsToRemove delContext = new();
            foreach (var from in _metrics.Values)
            {
                Func<KeyValuePair<uint, TransitionMetrics>, UITree.NodeProperties> map = kv =>
                {
                    var destName = kv.Key != uint.MaxValue ? _metrics[kv.Key].Name : "<end>";
                    var name = $"{from.Name} -> {destName}";
                    var value = $"avg={kv.Value.AvgTime:f2}-{from.ExpectedTime:f2}={kv.Value.AvgTime - from.ExpectedTime:f2} +- {kv.Value.StdDev:f2}, [{kv.Value.MinTime:f2}, {kv.Value.MaxTime:f2}] range, {kv.Value.Instances.Count} seen";
                    //bool warn = from.ExpectedTime < Math.Round(m.MinTime, 1) || from.ExpectedTime > Math.Round(m.MaxTime, 1);
                    bool warn = Math.Abs(from.ExpectedTime - kv.Value.AvgTime) > Math.Ceiling(kv.Value.StdDev * 10) / 10;
                    return new($"{name}: {value}###{name}", false, warn ? 0xff00ffff : 0xffffffff);
                };
                foreach (var (toID, m) in tree.Nodes(from.Transitions, map, kv => TransitionContextMenu(from, kv.Key, delContext)))
                {
                    foreach (var inst in m.Instances)
                    {
                        bool warn = Math.Abs(inst.Duration - m.AvgTime) > m.StdDev;
                        tree.LeafNode($"{inst.Duration:f2}: {LocationString(inst.Replay, inst.Encounter, inst.Time)}", warn ? 0xff00ffff : 0xffffffff, () => TransitionInstanceContextMenu(from, toID, m, inst, delContext));
                    }
                }
            }
            ExecuteDeletes(delContext);
        }

        private string LocationString(Replay replay, Replay.Encounter enc, DateTime timestamp) => $"{replay.Path} @ {enc.Time.Start:O} + {(timestamp - enc.Time.Start).TotalSeconds:f3} / {enc.Time.Duration:f3}";

        private void RecalculateMetrics(TransitionMetrics trans)
        {
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

        private void ExecuteDeletes(MetricsToRemove delContext)
        {
            // note: we can't delete collection elements while iterating
            foreach (var e in delContext.Instances)
            {
                e.trans.Instances.Remove(e.metric);
                if (e.trans.Instances.Count > 0)
                    RecalculateMetrics(e.trans);
                else
                    e.state.Transitions.Remove(e.to);
            }
            foreach (var e in delContext.Transitions)
            {
                e.state.Transitions.Remove(e.to);
            }
        }

        private void TransitionContextMenu(StateMetrics state, uint to, MetricsToRemove delContext)
        {
            if (ImGui.MenuItem("Ignore this transition"))
            {
                delContext.Transitions.Add((state, to));
            }
        }

        private void TransitionInstanceContextMenu(StateMetrics state, uint to, TransitionMetrics trans, TransitionMetric metric, MetricsToRemove delContext)
        {
            if (ImGui.MenuItem("Ignore this instance"))
            {
                delContext.Instances.Add((state, to, trans, metric));
            }
        }
    }
}
