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
                            if (ImGui.TreeNodeEx($"{effect.param0:X2} {effect.param1:X2} {effect.param2:X2} {effect.param3:X2} {effect.param4:X2} {effect.value:X4}: {replay.Path} {action.Time:O} {action.ID} {ParticipantString(action.Source)} -> {ParticipantString(action.MainTarget)} @ {ParticipantString(target.Target)}", ImGuiTreeNodeFlags.Leaf))
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
                        var cat = CategorizeUnknownEffect(effect);
                        if (cat.Length > 0)
                        {
                            var catName = $"{(byte)effect.effectType:X2} {effect.effectType}: {cat}";
                            _unknownActionEffects.TryAdd(catName, new());
                            _unknownActionEffects[catName].Add((replay, action, target, effect));
                        }
                    }
                }
            }
        }

        private string CategorizeUnknownEffect(ActionEffect eff)
        {
            switch (eff.effectType)
            {
                case ActionEffectType.Miss:
                case ActionEffectType.FullResist:
                    return eff.param0 != 0 || eff.param1 != 0 || eff.param2 != 0 || eff.param3 != 0 || eff.param4 != 0 || eff.value != 0 ? "non-zero params" : "";
                case ActionEffectType.Damage:
                case ActionEffectType.BlockedDamage:
                case ActionEffectType.ParriedDamage:
                    if ((eff.param0 & ~3) != 0)
                        return "unknown bits in param0";
                    else if (eff.param2 != 0)
                        return "non-zero param2";
                    else if (eff.param3 != 0 && (eff.param4 & 0x40) == 0)
                        return "non-zero param3 while large-value bit is unset";
                    else if ((eff.param4 & ~0xC0) != 0)
                        return "unknown bits in param4";
                    else
                        return "";
                case ActionEffectType.Heal:
                    if (eff.param0 != 0)
                        return "non-zero param0";
                    else if ((eff.param1 & ~1) != 0)
                        return "unknown bits in param1";
                    else if (eff.param2 != 0)
                        return "non-zero param2";
                    else if (eff.param3 != 0 && (eff.param4 & 0x40) == 0)
                        return "non-zero param3 while large-value bit is unset";
                    else if ((eff.param4 & ~0xC0) != 0)
                        return "unknown bits in param4";
                    else
                        return "";
                default:
                    return $"unknown type";
            }
        }

        private string ParticipantString(Replay.Participant? p)
        {
            return p != null ? $"{p.Type} {p.InstanceID:X} ({p.OID:X}) '{p.Name}'" : "<none>";
        }
    }
}
