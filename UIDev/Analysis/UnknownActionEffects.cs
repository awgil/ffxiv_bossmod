using BossMod;
using System.Collections.Generic;

namespace UIDev.Analysis
{
    class UnknownActionEffects
    {
        class Entry
        {
            public Replay Replay;
            public Replay.Action Action;
            public Replay.ActionTarget Target;
            public ActionEffect Effect;

            public Entry(Replay replay, Replay.Action action, Replay.ActionTarget target, ActionEffect effect)
            {
                Replay = replay;
                Action = action;
                Target = target;
                Effect = effect;
            }
        }

        private SortedDictionary<string, Dictionary<ActionID, List<Entry>>> _unknownActionEffects = new();

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
                    tree.LeafNodes(entries, entry => $"{ReplayUtils.ActionEffectString(entry.Effect)}: {entry.Replay.Path} {entry.Action.Timestamp:O} {ReplayUtils.ParticipantPosRotString(entry.Action.Source, entry.Action.Timestamp)} -> {ReplayUtils.ParticipantString(entry.Action.MainTarget)} {Utils.Vec3String(entry.Action.TargetPos)} @ {ReplayUtils.ParticipantPosRotString(entry.Target.Target, entry.Action.Timestamp)}");
                }
            }
        }
    }
}
