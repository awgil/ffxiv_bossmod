using BossMod;
using ImGuiNET;
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

        private Tree _tree;
        private SortedDictionary<string, Dictionary<ActionID, List<Entry>>> _unknownActionEffects = new();

        public UnknownActionEffects(List<Replay> replays, Tree tree)
        {
            _tree = tree;

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

        public void Draw()
        {
            foreach (var (type, actions) in _tree.Nodes(_unknownActionEffects, kv => ($"{kv.Key} ({kv.Value.Count} actions)", false)))
            {
                foreach (var (action, entries) in _tree.Nodes(actions, kv => ($"{kv.Key} ({kv.Value.Count} entries)", false)))
                {
                    _tree.LeafNodes(entries, entry => $"{ReplayUtils.ActionEffectString(entry.Effect)}: {entry.Replay.Path} {entry.Action.Timestamp:O} {ReplayUtils.ParticipantPosRotString(entry.Action.Source, entry.Action.Timestamp)} -> {ReplayUtils.ParticipantString(entry.Action.MainTarget)} {Utils.Vec3String(entry.Action.TargetPos)} @ {ReplayUtils.ParticipantPosRotString(entry.Target.Target, entry.Action.Timestamp)}");
                }
            }
        }
    }
}
