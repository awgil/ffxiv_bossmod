using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UIDev.Analysis
{
    class AbilityInfo
    {
        class ConeAnalysis
        {
            private Plot _plot = new();
            private List<(Replay Replay, Replay.Action Action, Replay.Participant Target, float Angle, float Range, bool Hit)> _points = new();

            public ConeAnalysis(List<(Replay, Replay.Action)> infos)
            {
                _plot.DataMin = new(-180, 0);
                _plot.DataMax = new(180, 60);
                _plot.TickAdvance = new(45, 5);
                foreach (var (r, a) in infos)
                {
                    var origin = a.TargetPos;
                    var dir = GeometryUtils.DirectionToVec3(a.Source?.PosRotAt(a.Timestamp).W ?? 0);
                    var left = new Vector3(dir.Z, 0, -dir.X);
                    foreach (var target in AlivePlayersAt(r, a.Timestamp))
                    {
                        // TODO: take target hitbox size into account...
                        var pos = target.PosRotAt(a.Timestamp).XYZ();
                        var toTarget = pos - origin;
                        var dist = toTarget.Length();
                        toTarget /= dist;
                        var angle = MathF.Acos(Vector3.Dot(toTarget, dir));
                        if (Vector3.Dot(toTarget, left) < 0)
                            angle = -angle;
                        bool hit = a.Targets.Any(t => t.Target?.InstanceID == target.InstanceID);
                        _points.Add((r, a, target, angle / MathF.PI * 180, dist, hit));
                    }
                }
            }

            public void Draw()
            {
                _plot.Begin();
                foreach (var i in _points)
                    _plot.Point(new(i.Angle, i.Range), i.Hit ? 0xff00ffff : 0xff808080, () => $"{(i.Hit ? "hit" : "miss")} {i.Target.Name} {i.Target.InstanceID:X} {i.Replay.Path} @ {i.Action.Timestamp:O}");
                _plot.End();
            }
        }

        class DamageFalloffAnalysis
        {
            private Plot _plot = new();
            private List<(Replay Replay, Replay.Action Action, Replay.Participant Target, float Range, int Damage)> _points = new();

            public DamageFalloffAnalysis(List<(Replay, Replay.Action)> infos, bool useMaxComp)
            {
                _plot.DataMin = new(0, 0);
                _plot.DataMax = new(100, 200000);
                _plot.TickAdvance = new(5, 10000);
                foreach (var (r, a) in infos)
                {
                    var origin = a.TargetPos;
                    foreach (var target in a.Targets)
                    {
                        if (target.Target == null)
                            continue;

                        var offset = target.Target.PosRotAt(a.Timestamp).XYZ() - origin;
                        var dist = useMaxComp ? MathF.Max(Math.Abs(offset.X), Math.Abs(offset.Z)) : offset.Length();
                        _points.Add((r, a, target.Target, dist, ReplayUtils.ActionDamage(target)));
                    }
                }
            }

            public void Draw()
            {
                _plot.Begin();
                foreach (var i in _points)
                    _plot.Point(new(i.Range, i.Damage), i.Damage > 0 ? 0xff00ffff : 0xff808080, () => $"{i.Damage} {i.Target.Name} {i.Target.InstanceID:X} {i.Replay.Path} @ {i.Action.Timestamp:O}");
                _plot.End();
            }
        }

        class ActionData
        {
            public List<(Replay, Replay.Action)> Instances = new();
            public HashSet<uint> CasterOIDs = new();
            public HashSet<uint> TargetOIDs = new();
            public bool SeenTargetSelf;
            public bool SeenTargetOtherEnemy;
            public bool SeenTargetPlayer;
            public bool SeenTargetLocation;
            public bool SeenAOE;
            public float CastTime;
            public ConeAnalysis? ConeAnalysis;
            public DamageFalloffAnalysis? DamageFalloffAnalysisDist;
            public DamageFalloffAnalysis? DamageFalloffAnalysisMinCoord;
        }

        private Type? _oidType;
        private Type? _aidType;
        private Dictionary<ActionID, ActionData> _data = new();

        public AbilityInfo(List<Replay> replays, uint oid)
        {
            var moduleType = ModuleRegistry.TypeForOID(oid);
            _oidType = moduleType?.Module.GetType($"{moduleType.Namespace}.OID");
            _aidType = moduleType?.Module.GetType($"{moduleType.Namespace}.AID");
            foreach (var replay in replays)
            {
                foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
                {
                    foreach (var action in replay.EncounterActions(enc).Where(a => !(a.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)))
                    {
                        var data = _data.GetOrAdd(action.ID);
                        if (action.Source != null)
                            data.CasterOIDs.Add(action.Source.OID);
                        if (action.MainTarget != null)
                            data.TargetOIDs.Add(action.MainTarget.OID);
                        data.SeenTargetSelf |= action.Source == action.MainTarget;
                        data.SeenTargetOtherEnemy |= action.MainTarget != action.Source && action.MainTarget?.Type == ActorType.Enemy;
                        data.SeenTargetPlayer |= action.MainTarget?.Type == ActorType.Player;
                        data.SeenTargetLocation |= action.MainTarget == null;
                        data.SeenAOE |= action.Targets.Count > 1;

                        var cast = action.Source?.Casts.Find(c => c.ID == action.ID && Math.Abs((c.Time.End - action.Timestamp).TotalSeconds) < 1);
                        data.CastTime = cast?.ExpectedCastTime + 0.3f ?? 0;

                        data.Instances.Add((replay, action));
                    }
                }
            }
        }

        public void Draw(Tree tree)
        {
            foreach (var (aid, data) in tree.Nodes(_data, kv => ($"{kv.Key} ({_aidType?.GetEnumName(kv.Key.ID)})", false)))
            {
                tree.LeafNode($"Caster IDs: {string.Join(", ", data.CasterOIDs.Select(oid => $"${oid:X} ({_oidType?.GetEnumName(oid)})"))}");
                tree.LeafNode($"Target IDs: {string.Join(", ", data.TargetOIDs.Select(oid => $"${oid:X} ({_oidType?.GetEnumName(oid)})"))}");
                tree.LeafNode($"Targets:{(data.SeenTargetSelf ? " self" : "")}{(data.SeenTargetOtherEnemy ? " enemy" : "")}{(data.SeenTargetPlayer ? " player" : "")}{(data.SeenTargetLocation ? " location" : "")}{(data.SeenAOE ? " aoe" : "")}");
                tree.LeafNode($"Cast time: {data.CastTime:f1}");
                foreach (var an in tree.Node("Cone analysis"))
                {
                    if (data.ConeAnalysis == null)
                        data.ConeAnalysis = new(data.Instances);
                    data.ConeAnalysis.Draw();
                }
                foreach (var an in tree.Node("Damage falloff analysis (by distance)"))
                {
                    if (data.DamageFalloffAnalysisDist == null)
                        data.DamageFalloffAnalysisDist = new(data.Instances, false);
                    data.DamageFalloffAnalysisDist.Draw();
                }
                foreach (var an in tree.Node("Damage falloff analysis (by max coord)"))
                {
                    if (data.DamageFalloffAnalysisMinCoord == null)
                        data.DamageFalloffAnalysisMinCoord = new(data.Instances, true);
                    data.DamageFalloffAnalysisMinCoord.Draw();
                }
            }
        }

        private static IEnumerable<Replay.Participant> AlivePlayersAt(Replay r, DateTime t)
        {
            return r.Participants.Where(p => p.Type is ActorType.Player or ActorType.Chocobo && p.Existence.Contains(t) && !p.DeadAt(t));
        }
    }
}
