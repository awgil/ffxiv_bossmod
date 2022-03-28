using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev.Analysis
{
    class AbilityInfo
    {
        class ActionInfo
        {
            public Replay Replay;
            public Replay.Action Action;
            public Replay.ActionTarget Target;
            public int Damage;

            public ActionInfo(Replay r, Replay.Action a, Replay.ActionTarget t, int d)
            {
                Replay = r;
                Action = a;
                Target = t;
                Damage = d;
            }
        }

        class ActionData
        {
            public double MinDamage = Double.MaxValue;
            public double MaxDamage;
            public double AvgDamage;
            public double StdDev;
            public List<ActionInfo> Instances = new();
        }

        private Dictionary<uint, Dictionary<ActionID, Dictionary<uint, ActionData>>> _data = new(); // [encounter-oid][aid][source-oid]

        public AbilityInfo(List<Replay> replays)
        {
            foreach (var replay in replays)
            {
                foreach (var enc in replay.Encounters)
                {
                    foreach (var action in replay.EncounterActions(enc))
                    {
                        if (action.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)
                            continue;

                        foreach (var target in action.Targets)
                        {
                            int damage = 0;
                            foreach (var eff in target.Effects.Where(eff => eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage && (eff.Param4 & 0x80) == 0))
                            {
                                damage += eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0);
                            }
                            if (damage > 0)
                                _data.GetOrAdd(enc.OID).GetOrAdd(action.ID).GetOrAdd(action.Source?.OID ?? 0).Instances.Add(new(replay, action, target, damage));
                        }
                    }
                }
            }

            foreach (var enc in _data.Values)
            {
                foreach (var action in enc.Values)
                {
                    foreach (var src in action.Values)
                    {
                        src.Instances.Sort((l, r) => l.Damage.CompareTo(r.Damage));
                        src.MinDamage = src.Instances.First().Damage;
                        src.MaxDamage = src.Instances.Last().Damage;
                        double sum = 0, sumSq = 0;
                        foreach (var inst in src.Instances)
                        {
                            sum += inst.Damage;
                            sumSq += inst.Damage * inst.Damage;
                        }
                        src.AvgDamage = sum / src.Instances.Count;
                        src.StdDev = src.Instances.Count > 0 ? Math.Sqrt((sumSq - sum * sum / src.Instances.Count) / (src.Instances.Count - 1)) : 0;
                    }
                }
            }
        }

        public void Draw()
        {
            foreach (var (encOID, perEnc) in _data)
            {
                var moduleType = ModuleRegistry.TypeForOID(encOID);
                var oidType = moduleType?.Module.GetType($"{moduleType.Namespace}.OID");
                var aidType = moduleType?.Module.GetType($"{moduleType.Namespace}.AID");
                if (ImGui.TreeNode($"{encOID:X} ({moduleType?.Name})"))
                {
                    foreach (var (aid, perSource) in perEnc)
                    {
                        if (ImGui.TreeNode($"{aid} ({aidType?.GetEnumName(aid.ID)})"))
                        {
                            foreach (var (srcOID, action) in perSource)
                            {
                                if (ImGui.TreeNode($"{srcOID:X} ({oidType?.GetEnumName(srcOID)}): avg={action.AvgDamage:f0} +- {action.StdDev:f0}, [{action.MinDamage:f0}, {action.MaxDamage:f0}] range, {action.Instances.Count} seen"))
                                {
                                    foreach (var inst in action.Instances)
                                    {
                                        var delta = inst.Target.PosRot - inst.Action.MainTargetPosRot;
                                        if (ImGui.TreeNode($"{inst.Damage}: {inst.Replay.Path} {inst.Action.Timestamp} -> {ReplayUtils.ParticipantString(inst.Target.Target)}, dpos=[{delta.X:f2}, {delta.Z:f2}]={MathF.Sqrt(delta.X * delta.X + delta.Z * delta.Z):f2}"))
                                        {
                                            foreach (var eff in inst.Target.Effects)
                                            {
                                                if (ImGui.TreeNodeEx(ReplayUtils.ActionEffectString(eff), ImGuiTreeNodeFlags.Leaf))
                                                    ImGui.TreePop();
                                            }
                                            ImGui.TreePop();
                                        }
                                    }
                                    ImGui.TreePop();
                                }
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
