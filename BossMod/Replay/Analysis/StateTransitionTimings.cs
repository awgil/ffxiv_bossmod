using ImGuiNET;

namespace BossMod.ReplayAnalysis;

class StateTransitionTimings
{
    class TransitionMetric(double duration, Replay replay, Replay.Encounter encounter, DateTime time)
    {
        public double Duration = duration;
        public Replay Replay = replay;
        public Replay.Encounter Encounter = encounter;
        public DateTime Time = time;
    }

    class TransitionMetrics
    {
        public double MinTime = double.MaxValue;
        public double MaxTime;
        public double AvgTime;
        public double StdDev;
        public List<TransitionMetric> Instances = [];
    }

    class StateMetrics(string name, double expectedTime)
    {
        public string Name = name;
        public double ExpectedTime = expectedTime;
        public SortedDictionary<uint, TransitionMetrics> Transitions = [];
    }

    private readonly SortedDictionary<uint, StateMetrics> _metrics = [];
    private readonly List<(Replay, Replay.Encounter, Replay.EncounterError)> _errors = [];
    private readonly List<(Replay, Replay.Encounter)> _encounters = [];
    private float _lastSecondsToIgnore;
    private object? _selected;

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
            ImGui.InputFloat("Last seconds to ignore", ref _lastSecondsToIgnore);
            tree.LeafNodes(_errors.Where(e => (e.Item2.Time.End - e.Item3.Timestamp).TotalSeconds >= _lastSecondsToIgnore), error => $"{LocationString(error.Item1, error.Item2, error.Item3.Timestamp)} [{error.Item3.CompType}] {error.Item3.Message}");
        }

        Action? actions = null;
        foreach (var from in _metrics.Values)
        {
            UITree.NodeProperties map(KeyValuePair<uint, TransitionMetrics> kv)
            {
                var destName = kv.Key != uint.MaxValue ? _metrics[kv.Key].Name : "<end>";
                var name = $"{from.Name} -> {destName}";
                var value = $"avg={kv.Value.AvgTime:f2}-{from.ExpectedTime:f2}={kv.Value.AvgTime - from.ExpectedTime:f2} +- {kv.Value.StdDev:f2}, [{kv.Value.MinTime:f2}, {kv.Value.MaxTime:f2}] range, {kv.Value.Instances.Count} seen";
                //bool warn = from.ExpectedTime < Math.Round(m.MinTime, 1) || from.ExpectedTime > Math.Round(m.MaxTime, 1);
                bool warn = Math.Abs(from.ExpectedTime - kv.Value.AvgTime) > Math.Ceiling(kv.Value.StdDev * 10) / 10;
                return new($"{name}: {value}###{name}", false, warn ? 0xff00ffff : 0xffffffff);
            }
            foreach (var (toID, m) in tree.Nodes(from.Transitions, map, kv => TransitionContextMenu(from, kv.Key, kv.Value, tree, ref actions), select: kv => _selected = kv.Value))
            {
                foreach (var inst in m.Instances)
                {
                    bool warn = Math.Abs(inst.Duration - m.AvgTime) > m.StdDev;
                    tree.LeafNode($"{inst.Duration:f2}: {LocationString(inst.Replay, inst.Encounter, inst.Time)}", warn ? 0xff00ffff : 0xffffffff, () => TransitionInstanceContextMenu(from, toID, m, inst, ref actions), select: () => _selected = inst);
                }
            }
        }
        actions?.Invoke();
    }

    private string LocationString(Replay replay, Replay.Encounter enc, DateTime timestamp) => $"{replay.Path} @ {enc.Time.Start:O} + {(timestamp - enc.Time.Start).TotalSeconds:f3} / {enc.Time.Duration:f3}";

    private void RecalculateMetrics(TransitionMetrics trans)
    {
        trans.Instances.SortBy(e => e.Duration);
        trans.MinTime = trans.Instances[0].Duration;
        trans.MaxTime = trans.Instances[^1].Duration;
        double sum = 0, sumSq = 0;
        foreach (var inst in trans.Instances)
        {
            sum += inst.Duration;
            sumSq += inst.Duration * inst.Duration;
        }
        trans.AvgTime = sum / trans.Instances.Count;
        trans.StdDev = trans.Instances.Count > 0 ? Math.Sqrt((sumSq - sum * sum / trans.Instances.Count) / (trans.Instances.Count - 1)) : 0;
    }

    private void TransitionContextMenu(StateMetrics state, uint to, TransitionMetrics trans, UITree tree, ref Action? actions)
    {
        if (ImGui.MenuItem("Ignore this transition"))
        {
            actions += () =>
            {
                if (_selected == trans)
                    _selected = null;
                state.Transitions.Remove(to);
            };
        }
        if (_selected is TransitionMetrics dest && trans != dest && ImGui.MenuItem("Merge to selected transition"))
        {
            dest.Instances.AddRange(trans.Instances);
            RecalculateMetrics(dest);
            state.Transitions.Remove(to);
        }
    }

    private void TransitionInstanceContextMenu(StateMetrics state, uint to, TransitionMetrics trans, TransitionMetric metric, ref Action? actions)
    {
        if (ImGui.MenuItem("Ignore this instance"))
        {
            actions += () =>
            {
                if (_selected == metric)
                    _selected = null;
                trans.Instances.Remove(metric);
                if (trans.Instances.Count > 0)
                    RecalculateMetrics(trans);
                else
                    state.Transitions.Remove(to);
            };
        }
    }
}
