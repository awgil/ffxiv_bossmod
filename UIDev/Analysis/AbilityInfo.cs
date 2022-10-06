using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIDev.Analysis
{
    class AbilityInfo
    {
        class ConeAnalysis
        {
            private UIPlot _plot = new();
            private List<(Replay Replay, Replay.Action Action, Replay.Participant Target, float Angle, float Range, bool Hit)> _points = new();

            public ConeAnalysis(List<(Replay, Replay.Action)> infos)
            {
                _plot.DataMin = new(-180, 0);
                _plot.DataMax = new(180, 60);
                _plot.TickAdvance = new(45, 5);
                foreach (var (r, a) in infos)
                {
                    var origin = new WPos(a.TargetPos.XZ());
                    var dir = (a.Source?.PosRotAt(a.Timestamp).W ?? 0).Radians().ToDirection();
                    var left = dir.OrthoL();
                    foreach (var target in AlivePlayersAt(r, a.Timestamp))
                    {
                        // TODO: take target hitbox size into account...
                        var pos = new WPos(target.PosRotAt(a.Timestamp).XZ());
                        var toTarget = pos - origin;
                        var dist = toTarget.Length();
                        toTarget /= dist;
                        var angle = MathF.Acos(toTarget.Dot(dir));
                        if (toTarget.Dot(left) < 0)
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
            private UIPlot _plot = new();
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

        class GazeAnalysis
        {
            private UIPlot _plot = new();
            private List<(Replay Replay, Replay.Action Action, Replay.Participant Target, Angle Angle, bool Hit)> _points = new();

            public GazeAnalysis(List<(Replay, Replay.Action)> infos)
            {
                _plot.DataMin = new(-180, 0);
                _plot.DataMax = new(180, 2);
                _plot.TickAdvance = new(45, 1);
                foreach (var (r, a) in infos)
                {
                    if (a.Source == null)
                        continue;
                    var src = new WPos(a.Source.PosRotAt(a.Timestamp).XZ());
                    foreach (var target in a.Targets)
                    {
                        if (target.Target == null)
                            continue;
                        var posRot = target.Target.PosRotAt(a.Timestamp);
                        var toSource = Angle.FromDirection(src - new WPos(posRot.XZ()));
                        var angle = toSource - posRot.W.Radians();
                        if (angle.Rad > MathF.PI)
                            angle.Rad -= 2 * MathF.PI;
                        if (angle.Rad < -MathF.PI)
                            angle.Rad += 2 * MathF.PI;
                        bool hit = !target.Effects.All(eff => eff.Type is ActionEffectType.Miss or ActionEffectType.StartActionCombo);
                        _points.Add((r, a, target.Target, angle, hit));
                    }
                }
            }

            public void Draw()
            {
                _plot.Begin();
                foreach (var i in _points)
                    _plot.Point(new(i.Angle.Deg, 1), i.Hit ? 0xff00ffff : 0xff808080, () => $"{(i.Hit ? "hit" : "miss")} {i.Target.Name} {i.Target.InstanceID:X} {i.Replay.Path} @ {i.Action.Timestamp:O}");
                _plot.End();
            }
        }

        class CasterLinkAnalysis
        {
            private List<(Replay Replay, Replay.Action Action, float MinDistance)> _points = new();

            public CasterLinkAnalysis(List<(Replay, Replay.Action)> infos)
            {
                foreach (var (r, a) in infos)
                {
                    if (a.Source == null)
                        continue;
                    var pos = a.Source.PosRotAt(a.Timestamp).XYZ();

                    float minDistance = float.MaxValue;
                    foreach (var other in r.Participants.Where(p => p != a.Source && p.OID == a.Source.OID && p.Existence.Contains(a.Timestamp)))
                    {
                        var otherPos = other.PosRotAt(a.Timestamp).XYZ();
                        minDistance = MathF.Min(minDistance, (otherPos - pos).Length());
                    }

                    _points.Add((r, a, minDistance));
                }
                _points.SortByReverse(e => e.MinDistance);
            }

            public void Draw(UITree tree)
            {
                tree.LeafNodes(_points, p => $"{p.MinDistance:f3}: {p.Replay.Path} @ {p.Action.Timestamp:O}");
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
            public GazeAnalysis? GazeAnalysis;
            public CasterLinkAnalysis? CasterLinkAnalysis;
        }

        private Type? _oidType;
        private Type? _aidType;
        private Dictionary<ActionID, ActionData> _data = new();

        public AbilityInfo(List<Replay> replays, uint oid)
        {
            var moduleInfo = ModuleRegistry.FindByOID(oid);
            _oidType = moduleInfo?.ObjectIDType;
            _aidType = moduleInfo?.ActionIDType;
            foreach (var replay in replays)
                foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
                    foreach (var action in replay.EncounterActions(enc))
                        AddActionData(replay, action);
        }

        public AbilityInfo(List<Replay> replays)
        {
            foreach (var replay in replays)
                foreach (var action in replay.Actions)
                    AddActionData(replay, action);
        }

        public void Draw(UITree tree)
        {
            Func<KeyValuePair<ActionID, ActionData>, UITree.NodeProperties> map = kv =>
            {
                var name = kv.Key.Type == ActionType.Spell ? _aidType?.GetEnumName(kv.Key.ID) : null;
                return new($"{kv.Key:X} ({name})", false, name == null ? 0xff00ffff : 0xffffffff);
            };
            foreach (var (aid, data) in tree.Nodes(_data, map))
            {
                tree.LeafNode($"Caster IDs: {OIDListString(data.CasterOIDs)}");
                tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
                tree.LeafNode($"Targets:{(data.SeenTargetSelf ? " self" : "")}{(data.SeenTargetOtherEnemy ? " enemy" : "")}{(data.SeenTargetPlayer ? " player" : "")}{(data.SeenTargetLocation ? " location" : "")}{(data.SeenAOE ? " aoe" : "")}");
                tree.LeafNode($"Cast time: {data.CastTime:f1}");
                if (aid.Type == ActionType.Spell)
                {
                    foreach (var n in tree.Node("Lumina data"))
                    {
                        var row = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(aid.ID);
                        tree.LeafNode($"Category: {row?.ActionCategory?.Value?.Name}");
                        tree.LeafNode($"Cast time: {row?.Cast100ms * 0.1f:f1}");
                        tree.LeafNode($"Target range: {row?.Range}");
                        tree.LeafNode($"Effect shape: {row?.CastType} ({(row != null ? DescribeShape(row) : "")})");
                        tree.LeafNode($"Effect range: {row?.EffectRange}");
                        tree.LeafNode($"Effect width: {row?.XAxisModifier}");
                        tree.LeafNode($"Omen: {row?.Omen.Value?.Path} / {row?.Omen.Value?.PathAlly}");
                        var omenAlt = row != null ? Service.LuminaRow<Lumina.Excel.GeneratedSheets.Omen>(row.Unknown54) : null;
                        tree.LeafNode($"Omen alt: {omenAlt?.Path} / {omenAlt?.PathAlly}");
                    }
                }
                foreach (var n in tree.Node("Instances"))
                {
                    foreach (var an in tree.Nodes(data.Instances, a => new($"{a.Item1.Path} @ {a.Item2.Timestamp:O}: {ReplayUtils.ParticipantPosRotString(a.Item2.Source, a.Item2.Timestamp)} -> {ReplayUtils.ParticipantString(a.Item2.MainTarget)} {Utils.Vec3String(a.Item2.TargetPos)} ({a.Item2.Targets.Count} affected)", a.Item2.Targets.Count == 0)))
                    {
                        foreach (var tn in tree.Nodes(an.Item2.Targets, t => new(ReplayUtils.ParticipantPosRotString(t.Target, an.Item2.Timestamp))))
                        {
                            tree.LeafNodes(tn.Effects, ReplayUtils.ActionEffectString);
                        }
                    }
                }
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
                foreach (var an in tree.Node("Gaze analysis"))
                {
                    if (data.GazeAnalysis == null)
                        data.GazeAnalysis = new(data.Instances);
                    data.GazeAnalysis.Draw();
                }
                foreach (var an in tree.Node("Caster link analysis"))
                {
                    if (data.CasterLinkAnalysis == null)
                        data.CasterLinkAnalysis = new(data.Instances);
                    data.CasterLinkAnalysis.Draw(tree);
                }
            }
        }

        public void DrawContextMenu()
        {
            if (ImGui.MenuItem("Generate enum for boss module"))
            {
                var sb = new StringBuilder("public enum AID : uint\n{");
                foreach (var (aid, data) in _data)
                {
                    var ldata = aid.Type == ActionType.Spell ? Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(aid.ID) : null;
                    string name = aid.Type != ActionType.Spell ? $"// {aid}" : _aidType?.GetEnumName(aid.ID) ?? $"_{Utils.StringToIdentifier(ldata?.ActionCategory?.Value?.Name ?? "")}_{Utils.StringToIdentifier(ldata?.Name ?? $"Ability{aid.ID}")}";
                    sb.Append($"\n    {name} = {aid.ID}, // {OIDListString(data.CasterOIDs)}->");

                    var tarSB = new StringBuilder();
                    if (data.SeenTargetSelf)
                        tarSB.Append("self");
                    if (data.SeenTargetPlayer)
                    {
                        if (tarSB.Length > 0)
                            tarSB.Append('/');
                        tarSB.Append(data.SeenAOE ? "players" : "player");
                    }
                    if (data.SeenTargetLocation)
                    {
                        if (tarSB.Length > 0)
                            tarSB.Append('/');
                        tarSB.Append("location");
                    }
                    if (data.SeenTargetOtherEnemy)
                    {
                        if (tarSB.Length > 0)
                            tarSB.Append('/');
                        tarSB.Append(OIDListString(data.TargetOIDs.Where(oid => oid != 0)));
                    }
                    if (tarSB.Length == 0)
                        tarSB.Append("none");

                    sb.Append(tarSB);
                    sb.Append($", {(data.CastTime > 0 ? $"{data.CastTime:f1}s" : "no")} cast");
                    if (ldata != null)
                        sb.Append($", {DescribeShape(ldata)}");
                }
                sb.Append("\n};\n");
                ImGui.SetClipboardText(sb.ToString());
            }
        }

        private void AddActionData(Replay replay, Replay.Action action)
        {
            if (action.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)
                return;

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

        private static IEnumerable<Replay.Participant> AlivePlayersAt(Replay r, DateTime t)
        {
            return r.Participants.Where(p => p.Type is ActorType.Player or ActorType.Chocobo && p.Existence.Contains(t) && !p.DeadAt(t));
        }

        private string OIDListString(IEnumerable<uint> oids)
        {
            var s = string.Join('/', oids.Select(oid => oid == 0 ? "player" : _oidType?.GetEnumName(oid) ?? $"{oid:X}"));
            return s.Length > 0 ? s : "none";
        }

        private string DescribeShape(Lumina.Excel.GeneratedSheets.Action data)
        {
            return data.CastType switch
            {
                1 => "single-target",
                2 => $"range {data.EffectRange} circle",
                3 => $"range {data.EffectRange}+R {DetermineConeAngle(data)?.ToString() ?? "?"}-degree cone",
                4 => $"range {data.EffectRange}+R width {data.XAxisModifier} rect",
                5 => $"range {data.EffectRange}+R circle",
                8 => $"width {data.XAxisModifier} rect charge",
                10 => $"range ?-{data.EffectRange} donut",
                11 => $"range {data.EffectRange} width {data.XAxisModifier} cross",
                12 => $"range {data.EffectRange} width {data.XAxisModifier} rect",
                13 => $"range {data.EffectRange} {DetermineConeAngle(data)?.ToString() ?? "?"}-degree cone",
                _ => "???"
            };
        }

        private Angle? DetermineConeAngle(Lumina.Excel.GeneratedSheets.Action data)
        {
            var omen = data.Omen.Value;
            if (omen == null)
                return null;

            var path = omen.Path.ToString();
            var pos = path.IndexOf("fan");
            if (pos < 0 || pos + 6 > path.Length)
                return null;

            return int.Parse(path.Substring(pos + 3, 3)).Degrees();
        }
    }
}
