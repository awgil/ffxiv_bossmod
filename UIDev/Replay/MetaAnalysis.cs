using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIDev
{
    class MetaAnalysis : IDisposable
    {
        private List<Replay> _replays = new();

        private SortedDictionary<string, List<(Replay, Replay.Action, Replay.ActionTarget, ActionEffect)>> _unknownActionEffects = new();

        public MetaAnalysis(string rootPath)
        {
            try
            {
                var di = new DirectoryInfo(rootPath);
                foreach (var fi in di.EnumerateFiles("World_*.log", new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    Service.Log($"Parsing {fi.FullName}...");
                    var replay = ReplayParserLog.Parse(fi.FullName);
                    AnalyzeReplay(replay);
                    _replays.Add(replay);
                }
            }
            catch (Exception e)
            {
                Service.Log($"Failed to read {rootPath}: {e}");
            }
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            ImGui.Text($"{_replays.Count} logs found");

            if (ImGui.TreeNode("Unknown action effects"))
            {
                foreach (var (type, effects) in _unknownActionEffects)
                {
                    if (ImGui.TreeNode($"{type} ({effects.Count} entries)"))
                    {
                        foreach (var (replay, action, target, effect) in effects)
                        {
                            if (ImGui.TreeNodeEx($"{effect.Param0:X2} {effect.Param1:X2} {effect.Param2:X2} {effect.Param3:X2} {effect.Param4:X2} {effect.Value:X4} = {ActionEffectParser.DescribeFields(effect)}: {replay.Path} {action.Time:O} {action.ID} {ParticipantString(action.Source)} -> {ParticipantString(action.MainTarget)} @ {ParticipantString(target.Target)}", ImGuiTreeNodeFlags.Leaf))
                                ImGui.TreePop();
                        }
                        ImGui.TreePop();
                    }
                }
                ImGui.TreePop();
            }
        }

        private void AnalyzeReplay(Replay replay)
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
                            _unknownActionEffects.TryAdd(catName, new());
                            _unknownActionEffects[catName].Add((replay, action, target, effect));
                        }
                    }
                }
            }
        }

        private string ParticipantString(Replay.Participant? p)
        {
            return p != null ? $"{p.Type} {p.InstanceID:X} ({p.OID:X}) '{p.Name}'" : "<none>";
        }
    }
}
