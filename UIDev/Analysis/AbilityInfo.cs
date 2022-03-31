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
            public List<(Actor, Vector4)> PositionSnapshot;

            public ActionInfo(Replay r, Replay.Action a, List<(Actor, Vector4)> snaps)
            {
                Replay = r;
                Action = a;
                PositionSnapshot = snaps;
            }
        }

        class ConeAnalysis
        {
            private Plot _plot = new();
            private List<(ActionInfo Info, Actor Target, float Angle, float Range, bool Hit)> _points = new();

            public ConeAnalysis(List<ActionInfo> infos)
            {
                _plot.DataMin = new(-180, 0);
                _plot.DataMax = new(180, 60);
                _plot.TickAdvance = new(45, 5);
                foreach (var info in infos)
                {
                    var origin = info.Action.MainTargetPosRot.XYZ();
                    var dir = GeometryUtils.DirectionToVec3(info.Action.SourcePosRot.W);
                    var left = new Vector3(dir.Z, 0, -dir.X);
                    foreach (var (actor, posRot) in info.PositionSnapshot)
                    {
                        var toTarget = posRot.XYZ() - origin;
                        var dist = toTarget.Length();
                        toTarget /= dist;
                        var angle = MathF.Acos(Vector3.Dot(toTarget, dir));
                        if (Vector3.Dot(toTarget, left) < 0)
                            angle = -angle;
                        bool hit = info.Action.Targets.Any(t => t.Target?.InstanceID == actor.InstanceID);
                        _points.Add((info, actor, angle / MathF.PI * 180, dist, hit));
                    }
                }
            }

            public void Draw()
            {
                _plot.Begin();
                foreach (var i in _points)
                    _plot.Point(new(i.Angle, i.Range), i.Hit ? 0xff00ffff : 0xff808080, () => $"{(i.Hit ? "hit" : "miss")} {i.Target.Name} {i.Target.InstanceID:X} {i.Info.Replay.Path} @ {i.Info.Action.Timestamp:O}");
                _plot.End();
            }
        }

        class DamageFalloffAnalysis
        {
            private Plot _plot = new();
            private List<(ActionInfo Info, Actor Target, float Range, int Damage)> _points = new();

            public DamageFalloffAnalysis(List<ActionInfo> infos, bool useMaxComp)
            {
                _plot.DataMin = new(0, 0);
                _plot.DataMax = new(100, 200000);
                _plot.TickAdvance = new(5, 10000);
                foreach (var info in infos)
                {
                    var origin = info.Action.MainTargetPosRot.XYZ();
                    foreach (var (actor, posRot) in info.PositionSnapshot)
                    {
                        var offset = posRot.XYZ() - origin;
                        var dist = useMaxComp ? MathF.Max(Math.Abs(offset.X), Math.Abs(offset.Z)): offset.Length();
                        var hit = info.Action.Targets.Find(t => t.Target?.InstanceID == actor.InstanceID);
                        int damage = 0;
                        if (hit != null)
                            foreach (var eff in hit.Effects.Where(eff => eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage && (eff.Param4 & 0x80) == 0))
                                damage += eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0);
                        _points.Add((info, actor, dist, damage));
                    }
                }
            }

            public void Draw()
            {
                _plot.Begin();
                foreach (var i in _points)
                    _plot.Point(new(i.Range, i.Damage), i.Damage > 0 ? 0xff00ffff : 0xff808080, () => $"{i.Damage} {i.Target.Name} {i.Target.InstanceID:X} {i.Info.Replay.Path} @ {i.Info.Action.Timestamp:O}");
                _plot.End();
            }
        }

        class ActionData
        {
            public List<ActionInfo> Instances = new();
            public ConeAnalysis? ConeAnalysis;
            public DamageFalloffAnalysis? DamageFalloffAnalysisDist;
            public DamageFalloffAnalysis? DamageFalloffAnalysisMinCoord;
        }

        private Tree _tree;
        private Dictionary<uint, Dictionary<ActionID, ActionData>> _data = new(); // [encounter-oid][aid]

        public AbilityInfo(List<Replay> replays, Tree tree)
        {
            _tree = tree;

            foreach (var replay in replays.Where(r => r.Encounters.Count > 0))
            {
                List<List<(Actor, Vector4)>> snaps = new();
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
                    foreach (var an in _tree.Node("Cone analysis"))
                    {
                        if (data.ConeAnalysis == null)
                            data.ConeAnalysis = new(data.Instances);
                        data.ConeAnalysis.Draw();
                    }
                    foreach (var an in _tree.Node("Damage falloff analysis (by distance)"))
                    {
                        if (data.DamageFalloffAnalysisDist == null)
                            data.DamageFalloffAnalysisDist = new(data.Instances, false);
                        data.DamageFalloffAnalysisDist.Draw();
                    }
                    foreach (var an in _tree.Node("Damage falloff analysis (by max coord)"))
                    {
                        if (data.DamageFalloffAnalysisMinCoord == null)
                            data.DamageFalloffAnalysisMinCoord = new(data.Instances, true);
                        data.DamageFalloffAnalysisMinCoord.Draw();
                    }
                }
            }
        }

        private List<(Actor, Vector4)> BuildPositionSnapshot(WorldState ws)
        {
            List<(Actor, Vector4)> res = new();
            foreach (var actor in ws.Actors.Where(a => !a.IsDead && a.Type is ActorType.Player or ActorType.Chocobo))
                res.Add((actor, actor.PosRot));
            return res;
        }
    }
}
