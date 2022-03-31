using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UIDev.Analysis
{
    class AbilityInfo
    {
        class ActionInfo
        {
            public Replay Replay;
            public Replay.Action Action;
            public List<(uint, Vector4)> PositionSnapshot;

            public ActionInfo(Replay r, Replay.Action a, List<(uint, Vector4)> snaps)
            {
                Replay = r;
                Action = a;
                PositionSnapshot = snaps;
            }
        }

        class ConeAnalysis
        {
            public List<(ActionInfo Info, uint Target, float Angle, float Range, bool Hit)> InstancesByAngle = new();
            public List<(ActionInfo Info, uint Target, float Angle, float Range, bool Hit)> InstancesByRange;
            public Vector2 RangeLimit = new(0, 1000);
            public Vector2 AngleLimit = new(-180, 180);

            public ConeAnalysis(List<ActionInfo> infos)
            {
                foreach (var info in infos)
                {
                    var src = info.Action.SourcePosRot.XYZ();
                    var dir = GeometryUtils.DirectionToVec3(info.Action.SourcePosRot.W);
                    var left = new Vector3(dir.Z, 0, -dir.X);
                    foreach (var (id, posRot) in info.PositionSnapshot)
                    {
                        var toTarget = posRot.XYZ() - src;
                        var dist = toTarget.Length();
                        toTarget /= dist;
                        var angle = MathF.Acos(Vector3.Dot(toTarget, dir));
                        if (Vector3.Dot(toTarget, left) < 0)
                            angle = -angle;
                        bool hit = info.Action.Targets.Any(t => t.Target?.InstanceID == id);
                        InstancesByAngle.Add((info, id, angle, dist, hit));
                    }
                }
                InstancesByAngle.Sort((l, r) => l.Angle.CompareTo(r.Angle));

                InstancesByRange = InstancesByAngle.ToList();
                InstancesByRange.Sort((l, r) => l.Range.CompareTo(r.Range));
            }
        }

        class ActionData
        {
            public List<ActionInfo> Instances = new();
            public ConeAnalysis? ConeAnalysis;
        }

        private Tree _tree;
        private Dictionary<uint, Dictionary<ActionID, ActionData>> _data = new(); // [encounter-oid][aid]

        public AbilityInfo(List<Replay> replays, Tree tree)
        {
            _tree = tree;

            foreach (var replay in replays.Where(r => r.Encounters.Count > 0))
            {
                List<List<(uint, Vector4)>> snaps = new();
                WorldState ws = new();
                int nextOp = 0;
                foreach (var action in replay.Actions)
                {
                    while (nextOp < replay.Ops.Count && replay.Ops[nextOp].Timestamp < action.Timestamp)
                    {
                        ws.CurrentTime = replay.Ops[nextOp].Timestamp;
                        replay.Ops[nextOp++].Redo(ws);
                    }
                    snaps.Add(BuildPositionSnapshot(ws));
                }

                foreach (var enc in replay.Encounters)
                {
                    int index = enc.FirstAction;
                    foreach (var action in replay.EncounterActions(enc))
                    {
                        bool crap = action.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo;
                        if (!crap)
                        {
                            var data = _data.GetOrAdd(enc.OID).GetOrAdd(action.ID);
                            //int damage = 0;
                            //foreach (var eff in target.Effects.Where(eff => eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage && (eff.Param4 & 0x80) == 0))
                            //{
                            //    damage += eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0);
                            //}
                            data.Instances.Add(new(replay, action, snaps[index]));
                        }
                        ++index;
                    }
                }
            }
        }

        public void Draw()
        {
            foreach (var (encOID, perEnc) in _tree.Nodes(_data, kv => ($"{kv.Key:X} ({ModuleRegistry.TypeForOID(kv.Key)?.Name})", false)))
            {
                var moduleType = ModuleRegistry.TypeForOID(encOID);
                var oidType = moduleType?.Module.GetType($"{moduleType.Namespace}.OID");
                var aidType = moduleType?.Module.GetType($"{moduleType.Namespace}.AID");
                foreach (var (aid, data) in _tree.Nodes(perEnc, kv => ($"{kv.Key} ({aidType?.GetEnumName(kv.Key.ID)})", false)))
                {
                    foreach (var an in _tree.Node("Cone range"))
                    {
                        if (data.ConeAnalysis == null)
                            data.ConeAnalysis = new(data.Instances);
                        ImGui.InputFloat2("Angle limit", ref data.ConeAnalysis.AngleLimit);
                        var radLim = data.ConeAnalysis.AngleLimit * MathF.PI / 180;
                        _tree.LeafNodes(data.ConeAnalysis.InstancesByRange.Where(x => x.Angle >= radLim.X && x.Angle <= radLim.Y),
                            x => $"{x.Range:f1} = {(x.Hit ? "hit" : "miss")}: {x.Info.Replay.Path} {x.Info.Action.Timestamp:O} {x.Target:X}");
                    }

                    foreach (var an in _tree.Node("Cone angles"))
                    {
                        if (data.ConeAnalysis == null)
                            data.ConeAnalysis = new(data.Instances);
                        ImGui.InputFloat2("Range limit", ref data.ConeAnalysis.RangeLimit);
                        _tree.LeafNodes(data.ConeAnalysis.InstancesByAngle.Where(x => x.Range >= data.ConeAnalysis.RangeLimit.X && x.Range <= data.ConeAnalysis.RangeLimit.Y),
                            x => $"{Utils.RadianString(x.Angle)} ({x.Range:f1}) = {(x.Hit ? "hit" : "miss")}: {x.Info.Replay.Path} {x.Info.Action.Timestamp:O} {x.Target:X}");
                    }
                }
            }
        }

        private List<(uint, Vector4)> BuildPositionSnapshot(WorldState ws)
        {
            List<(uint, Vector4)> res = new();
            foreach (var actor in ws.Actors.Where(a => !a.IsDead && a.Type is ActorType.Player or ActorType.Chocobo))
                res.Add((actor.InstanceID, actor.PosRot));
            return res;
        }

        private void AnalyzeConeAOE(ActionData data)
        {
        }
    }
}
