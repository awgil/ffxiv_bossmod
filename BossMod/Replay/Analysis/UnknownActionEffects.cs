namespace BossMod.ReplayAnalysis;

class UnknownActionEffects
{
    class Entry(Replay replay, Replay.Action action, Replay.ActionTarget target, ActionEffect effect)
    {
        public Replay Replay = replay;
        public Replay.Action Action = action;
        public Replay.ActionTarget Target = target;
        public ActionEffect Effect = effect;
    }

    private readonly SortedDictionary<string, Dictionary<ActionID, List<Entry>>> _unknownActionEffects = [];

    public UnknownActionEffects(List<Replay> replays)
    {
        foreach (var replay in replays)
        {
            foreach (var action in replay.Actions)
            {
                foreach (var target in action.Targets)
                {
                    foreach (var effect in target.Effects)
                    {
                        var cat = ActionEffectParser.DescribeUnknown(effect);
                        if (cat.Length > 0)
                        {
                            var catName = $"{(byte)effect.Type:X2} {effect.Type}: {cat}";
                            _unknownActionEffects.GetOrAdd(catName).GetOrAdd(action.ID).Add(new(replay, action, target, effect));
                        }
                    }
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        foreach (var (type, actions) in tree.Nodes(_unknownActionEffects, kv => new($"{kv.Key} ({kv.Value.Count} actions)")))
        {
            foreach (var (action, entries) in tree.Nodes(actions, kv => new($"{kv.Key} ({kv.Value.Count} entries)")))
            {
                foreach (var entry in tree.Nodes(entries, entry => new($"{ReplayUtils.ActionEffectString(entry.Effect)}: {entry.Replay.Path} {entry.Action.Timestamp:O} {ReplayUtils.ParticipantString(entry.Action.Source, entry.Action.Timestamp)} -> {ReplayUtils.ParticipantString(entry.Action.MainTarget, entry.Action.Timestamp)} @ {ReplayUtils.ParticipantString(entry.Target.Target, entry.Action.Timestamp)}")))
                {
                    foreach (var target in tree.Nodes(entry.Action.Targets, target => new(ReplayUtils.ActionTargetString(target, entry.Action.Timestamp))))
                    {
                        tree.LeafNodes(target.Effects, ReplayUtils.ActionEffectString);
                    }
                }
            }
        }
    }
}
