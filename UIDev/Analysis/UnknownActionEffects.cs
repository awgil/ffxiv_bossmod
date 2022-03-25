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

        public void Draw()
        {
            foreach (var (type, actions) in _unknownActionEffects)
            {
                if (ImGui.TreeNode($"{type} ({actions.Count} actions)"))
                {
                    foreach (var (action, entries) in actions)
                    {
                        if (ImGui.TreeNode($"{action} ({entries.Count} entries)"))
                        {
                            foreach (var entry in entries)
                            {
                                if (ImGui.TreeNodeEx($"{ReplayUtils.ActionEffectString(entry.Effect)}: {entry.Replay.Path} {entry.Action.Timestamp:O} {ReplayUtils.ParticipantPosRotString(entry.Action.Source, entry.Action.SourcePosRot)} -> {ReplayUtils.ParticipantPosRotString(entry.Action.MainTarget, entry.Action.MainTargetPosRot)} @ {ReplayUtils.ParticipantPosRotString(entry.Target.Target, entry.Target.PosRot)}", ImGuiTreeNodeFlags.Leaf))
                                    ImGui.TreePop();
                            }
                            ImGui.TreePop();
                        }
                    }
                    ImGui.TreePop();
                }
            }
        }
    }
}
