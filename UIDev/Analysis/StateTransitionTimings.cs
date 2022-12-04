using BossMod;
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

        private SortedDictionary<uint, StateMetrics> _metrics = new();
        private List<(Replay, Replay.EncounterError)> _errors = new();
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
                        _metrics[from.ID].Transitions.GetOrAdd(i < enc.States.Count - 1 ? enc.States[i + 1].ID : uint.MaxValue).Instances.Add(new TransitionMetric((from.Exit - enter).TotalSeconds, replay, enter));
                        enter = from.Exit;
                    }

                    foreach (var e in enc.Errors)
                    {
                        _errors.Add((replay, e));
                    }
                }
            }

            foreach (var (_, state) in _metrics)
            {
                foreach (var (_, trans) in state.Transitions)
                {
                    trans.Instances.SortBy(e => e.Duration);
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
                tree.LeafNodes(_errors, error => $"{error.Item1.Path} @ {error.Item2.Timestamp:O} [{error.Item2.CompType}] {error.Item2.Message}");
            }

            foreach (var from in _metrics.Values)
            {
                Func<KeyValuePair<uint, TransitionMetrics>, UITree.NodeProperties> map = kv =>
                {
                    var destName = kv.Key != uint.MaxValue ? _metrics[kv.Key].Name : "<end>";
                    string name = $"{from.Name} -> {destName}: avg={kv.Value.AvgTime:f2}-{from.ExpectedTime:f2}={kv.Value.AvgTime - from.ExpectedTime:f2} +- {kv.Value.StdDev:f2}, [{kv.Value.MinTime:f2}, {kv.Value.MaxTime:f2}] range, {kv.Value.Instances.Count} seen";
                    //bool warn = from.ExpectedTime < Math.Round(m.MinTime, 1) || from.ExpectedTime > Math.Round(m.MaxTime, 1);
                    bool warn = Math.Abs(from.ExpectedTime - kv.Value.AvgTime) > Math.Ceiling(kv.Value.StdDev * 10) / 10;
                    return new(name, false, warn ? 0xff00ffff : 0xffffffff);
                };
                foreach (var (toID, m) in tree.Nodes(from.Transitions, map))
                {
                    foreach (var inst in m.Instances)
                    {
                        bool warn = Math.Abs(inst.Duration - m.AvgTime) > m.StdDev;
                        tree.LeafNode($"{inst.Duration:f2}: {inst.Replay.Path} @ {inst.Time:O}", warn ? 0xff00ffff : 0xffffffff);
                    }
                }
            }
        }
    }
}
