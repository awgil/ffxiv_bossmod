using BossMod.Components;
using ImGuiNET;
using System.Globalization;
using System.Text;

namespace BossMod.ReplayAnalysis;

class AbilityInfo : CommonEnumInfo
{
    public readonly record struct Instance(Replay Replay, Replay.Encounter? Enc, Replay.Action Action)
    {
        public string TimestampString() => Enc != null
            ? $"{Replay.Path} @ {Enc.Time.Start:O}+{(Action.Timestamp - Enc.Time.Start).TotalSeconds:f4}"
            : $"{Replay.Path} @ {Action.Timestamp:O}";
    }

    class SourcePositionAnalysis
    {
        private readonly UIPlot _plot = new();
        private readonly List<(Instance Inst, Vector2 SourcePos)> _points = [];

        public SourcePositionAnalysis(List<Instance> infos)
        {
            _plot.DataMin = new(float.MaxValue, float.MaxValue);
            _plot.DataMax = new(float.MinValue, float.MinValue);
            _plot.TickAdvance = new(5, 5);
            foreach (var inst in infos)
            {
                var pos = inst.Action.Source.PosRotAt(inst.Action.Timestamp).XZ();
                _plot.DataMin.X = Math.Min(_plot.DataMin.X, pos.X);
                _plot.DataMin.Y = Math.Min(_plot.DataMin.Y, pos.Y);
                _plot.DataMax.X = Math.Max(_plot.DataMax.X, pos.X);
                _plot.DataMax.Y = Math.Max(_plot.DataMax.Y, pos.Y);
                _points.Add((inst, pos));
            }
            _plot.DataMin.X -= 1;
            _plot.DataMin.Y -= 1;
            _plot.DataMax.X += 1;
            _plot.DataMax.Y += 1;
        }

        public void Draw()
        {
            _plot.Begin();
            foreach (var i in _points)
                _plot.Point(i.SourcePos, 0xff808080, i.Inst.TimestampString);
            _plot.End();
        }
    }

    class ConeAnalysis
    {
        public enum Targeting { SourcePosRot, TargetPosSourceRot, SourcePosDirToTarget }

        private readonly UIPlot _plot = new();
        private readonly List<(Instance Inst, Replay.Participant Target, float Angle, float Range, bool Hit)> _points = [];

        public ConeAnalysis(List<Instance> infos, Targeting targeting)
        {
            _plot.DataMin = new(-180, 0);
            _plot.DataMax = new(180, 60);
            _plot.TickAdvance = new(45, 5);
            foreach (var i in infos)
            {
                var cast = i.Action.Source.Casts.LastOrDefault(c => c.ID == i.Action.ID && c.Time.Start < i.Action.Timestamp);
                var sourcePosRot = cast == null ? i.Action.Source.PosRotAt(i.Action.Timestamp) : new Vector4(cast.Location, cast.Rotation.Rad);
                var sourcePos = new WPos(sourcePosRot.XZ());
                var targetPos = new WPos((cast?.Location ?? i.Action.TargetPos).XZ());
                if (targetPos == sourcePos && i.Action.Targets.Count > 0)
                    targetPos = new(i.Action.Targets[0].Target.PosRotAt(i.Action.Timestamp).XZ());
                var origin = targeting != Targeting.TargetPosSourceRot ? sourcePos : targetPos;
                var dir = targeting != Targeting.SourcePosDirToTarget ? sourcePosRot.W.Radians().ToDirection() : (targetPos - origin).Normalized();
                var left = dir.OrthoL();
                foreach (var target in AlivePlayersAt(i.Replay, i.Action.Timestamp))
                {
                    // TODO: take target hitbox size into account...
                    var pos = new WPos(target.PosRotAt(i.Action.Timestamp).XZ());
                    var toTarget = pos - origin;
                    var dist = toTarget.Length();
                    toTarget /= dist;
                    var angle = MathF.Acos(toTarget.Dot(dir));
                    if (toTarget.Dot(left) < 0)
                        angle = -angle;
                    bool hit = i.Action.Targets.Any(t => t.Target.InstanceID == target.InstanceID);
                    _points.Add((i, target, angle / MathF.PI * 180, dist, hit));
                }
            }
        }

        public void Draw()
        {
            _plot.Begin();
            foreach (var i in _points)
                _plot.Point(new(i.Angle, i.Range), i.Hit ? 0xff00ffff : 0xff808080, () => $"{(i.Hit ? "hit" : "miss")} {i.Target.NameAt(i.Inst.Action.Timestamp)} {i.Target.InstanceID:X} {i.Inst.TimestampString()}");
            _plot.End();
        }
    }

    class RectAnalysis
    {
        private readonly UIPlot _plot = new();
        private readonly List<(Instance Inst, Replay.Participant Target, float Normal, float Length, bool Hit)> _points = [];

        public RectAnalysis(List<Instance> infos, bool useActionRotation)
        {
            _plot.DataMin = new(-50, -50);
            _plot.DataMax = new(50, 50);
            _plot.TickAdvance = new(5, 5);
            foreach (var i in infos)
            {
                var sourcePosRot = i.Action.Source.PosRotAt(i.Action.Timestamp);
                var origin = new WPos(sourcePosRot.XZ());
                var dir = (useActionRotation ? i.Action.Rotation : sourcePosRot.W.Radians()).ToDirection();
                var left = dir.OrthoL();
                foreach (var target in AlivePlayersAt(i.Replay, i.Action.Timestamp))
                {
                    // TODO: take target hitbox size into account...
                    var pos = new WPos(target.PosRotAt(i.Action.Timestamp).XZ());
                    var toTarget = pos - origin;
                    bool hit = i.Action.Targets.Any(t => t.Target.InstanceID == target.InstanceID);
                    _points.Add((i, target, toTarget.Dot(left), toTarget.Dot(dir), hit));
                }
            }
        }

        public void Draw()
        {
            _plot.Begin();
            foreach (var i in _points)
                _plot.Point(new(i.Normal, i.Length), i.Hit ? 0xff00ffff : 0xff808080, () => $"{(i.Hit ? "hit" : "miss")} {i.Target.NameAt(i.Inst.Action.Timestamp)} {i.Target.InstanceID:X} {i.Inst.TimestampString()}");
            _plot.End();
        }
    }

    class DamageFalloffAnalysis
    {
        private readonly UIPlot _plot = new();
        private readonly List<(Instance Inst, Replay.Participant Target, float Range, int Damage)> _points = [];

        public DamageFalloffAnalysis(List<Instance> infos, bool useMaxComp, bool fromSource)
        {
            _plot.DataMin = new(0, 0);
            _plot.DataMax = new(100, 200000);
            _plot.TickAdvance = new(5, 10000);
            foreach (var i in infos)
            {
                var origin = fromSource ? i.Action.Source.PosRotAt(i.Action.Timestamp).XYZ() : i.Action.TargetPos;
                foreach (var target in i.Action.Targets)
                {
                    var offset = target.Target.PosRotAt(i.Action.Timestamp).XYZ() - origin;
                    var dist = useMaxComp ? MathF.Max(Math.Abs(offset.X), Math.Abs(offset.Z)) : offset.Length();
                    _points.Add((i, target.Target, dist, ReplayUtils.ActionDamage(target)));
                }
            }
        }

        public void Draw()
        {
            _plot.Begin();
            foreach (var i in _points)
                _plot.Point(new(i.Range, i.Damage), i.Damage > 0 ? 0xff00ffff : 0xff808080, () => $"{i.Damage} {i.Target.NameAt(i.Inst.Action.Timestamp)} {i.Target.InstanceID:X} {i.Inst.TimestampString()}");
            _plot.End();
        }
    }

    class GazeAnalysis
    {
        private readonly UIPlot _plot = new();
        private readonly List<(Instance Inst, Replay.Participant Target, Angle Angle, bool Hit)> _points = [];

        public GazeAnalysis(List<Instance> infos)
        {
            _plot.DataMin = new(-180, 0);
            _plot.DataMax = new(180, 2);
            _plot.TickAdvance = new(45, 1);
            foreach (var i in infos)
            {
                var src = new WPos(i.Action.Source.PosRotAt(i.Action.Timestamp).XZ());
                foreach (var target in i.Action.Targets)
                {
                    var posRot = target.Target.PosRotAt(i.Action.Timestamp);
                    var toSource = Angle.FromDirection(src - new WPos(posRot.XZ()));
                    var angle = toSource - posRot.W.Radians();
                    if (angle.Rad > MathF.PI)
                        angle.Rad -= 2 * MathF.PI;
                    if (angle.Rad < -MathF.PI)
                        angle.Rad += 2 * MathF.PI;
                    bool hit = !target.Effects.All(eff => eff.Type is ActionEffectType.Miss or ActionEffectType.StartActionCombo);
                    _points.Add((i, target.Target, angle, hit));
                }
            }
        }

        public void Draw()
        {
            _plot.Begin();
            foreach (var i in _points)
                _plot.Point(new(i.Angle.Deg, 1), i.Hit ? 0xff00ffff : 0xff808080, () => $"{(i.Hit ? "hit" : "miss")} {i.Target.NameAt(i.Inst.Action.Timestamp)} {i.Target.InstanceID:X} {i.Inst.TimestampString()}");
            _plot.End();
        }
    }

    class KnockbackAnalysis
    {
        private record struct Point(Instance Inst, Replay.ActionTarget Target);

        private readonly Dictionary<int, List<Point>> _byDistance = [];
        private readonly Dictionary<Knockback.Kind, List<Point>> _byKind = [];
        private readonly List<Point> _immuneIgnores = [];
        private readonly List<Point> _immuneMisses = [];
        private readonly List<Point> _transcendentIgnores = [];
        private readonly List<Point> _transcendentMisses = [];
        private readonly List<Point> _otherMisses = [];

        public KnockbackAnalysis(List<Instance> infos)
        {
            foreach (var i in infos)
            {
                foreach (var target in i.Action.Targets)
                {
                    bool hasKnockbacks = false;
                    foreach (var eff in target.Effects)
                    {
                        switch (eff.Type)
                        {
                            case ActionEffectType.Knockback:
                                var kbData = Service.LuminaRow<Lumina.Excel.Sheets.Knockback>(eff.Value);
                                var kind = kbData != null ? (KnockbackDirection)kbData.Value.Direction switch
                                {
                                    KnockbackDirection.AwayFromSource => Knockback.Kind.AwayFromOrigin,
                                    KnockbackDirection.SourceForward => Knockback.Kind.DirForward,
                                    KnockbackDirection.SourceRight => Knockback.Kind.DirRight,
                                    KnockbackDirection.SourceLeft => Knockback.Kind.DirLeft,
                                    _ => Knockback.Kind.None
                                } : Knockback.Kind.None;
                                AddPoint(i, target, (kbData?.Distance ?? 0) + eff.Param0, kind);
                                hasKnockbacks = true;
                                break;
                            case ActionEffectType.Attract1:
                            case ActionEffectType.Attract2:
                                var attrData = Service.LuminaRow<Lumina.Excel.Sheets.Attract>(eff.Value);
                                AddPoint(i, target, attrData?.MaxDistance ?? 0, Knockback.Kind.TowardsOrigin);
                                hasKnockbacks = true;
                                break;
                            case ActionEffectType.AttractCustom1:
                            case ActionEffectType.AttractCustom2:
                            case ActionEffectType.AttractCustom3:
                                AddPoint(i, target, eff.Value, Knockback.Kind.TowardsOrigin);
                                hasKnockbacks = true;
                                break;
                        }
                    }

                    if (!hasKnockbacks)
                    {
                        if (IsImmune(i.Replay, target.Target, i.Action.Timestamp))
                            _immuneMisses.Add(new(i, target));
                        else if (IsTranscendent(i.Replay, target.Target, i.Action.Timestamp))
                            _transcendentMisses.Add(new(i, target));
                        else
                            _otherMisses.Add(new(i, target));
                    }
                }
            }
        }

        public void Draw(UITree tree)
        {
            foreach (var (dist, points) in _byDistance)
                DrawPoints(tree, $"Distance {dist}", points);
            foreach (var (kind, points) in _byKind)
                DrawPoints(tree, $"Type {kind}", points);
            DrawPoints(tree, "Ignore immunity", _immuneIgnores);
            DrawPoints(tree, "Ignore transcendent", _transcendentIgnores);
            DrawPoints(tree, "Misses while immune", _immuneMisses);
            DrawPoints(tree, "Misses while transcendent", _transcendentMisses);
            DrawPoints(tree, "Misses in other states", _otherMisses);
        }

        private void AddPoint(Instance inst, Replay.ActionTarget target, int distance, Knockback.Kind kind)
        {
            _byDistance.GetOrAdd(distance).Add(new(inst, target));
            _byKind.GetOrAdd(kind).Add(new(inst, target));
            if (IsImmune(inst.Replay, target.Target, inst.Action.Timestamp))
                _immuneIgnores.Add(new(inst, target));
            if (IsTranscendent(inst.Replay, target.Target, inst.Action.Timestamp))
                _transcendentIgnores.Add(new(inst, target));
        }

        private void DrawPoints(UITree tree, string tag, List<Point> points)
        {
            foreach (var n in tree.Node($"{tag} ({points.Count} instances)", points.Count == 0))
            {
                foreach (var an in tree.Nodes(points, p => new($"{p.Inst.TimestampString()}: {ReplayUtils.ParticipantPosRotString(p.Inst.Action.Source, p.Inst.Action.Timestamp)} -> {ReplayUtils.ParticipantString(p.Target.Target, p.Inst.Action.Timestamp)}")))
                {
                    tree.LeafNodes(an.Target.Effects, ReplayUtils.ActionEffectString);
                }
            }
        }

        private static bool IsImmune(uint sid) => sid is 3054 or (uint)WHM.SID.Surecast or (uint)WAR.SID.ArmsLength or 1722 or (uint)WAR.SID.InnerStrength or 2345; // see Knockback component
        private static bool IsImmune(Replay replay, Replay.Participant participant, DateTime timestamp) => replay.Statuses.Any(status => status.Target == participant && status.Time.Contains(timestamp) && IsImmune(status.ID));

        // transcendent (after rez) is kind of immune too
        private static bool IsTranscendent(uint sid) => sid is 418;
        private static bool IsTranscendent(Replay replay, Replay.Participant participant, DateTime timestamp) => replay.Statuses.Any(status => status.Target == participant && status.Time.Contains(timestamp) && IsTranscendent(status.ID));
    }

    class CasterLinkAnalysis
    {
        private readonly List<(Instance Inst, float MinDistance)> _points = [];

        public CasterLinkAnalysis(List<Instance> infos)
        {
            foreach (var i in infos)
            {
                var pos = i.Action.Source.PosRotAt(i.Action.Timestamp).XYZ();

                float minDistance = float.MaxValue;
                foreach (var other in i.Replay.Participants.Where(p => p != i.Action.Source && p.OID == i.Action.Source.OID && p.ExistsInWorldAt(i.Action.Timestamp)))
                {
                    var otherPos = other.PosRotAt(i.Action.Timestamp).XYZ();
                    minDistance = MathF.Min(minDistance, (otherPos - pos).Length());
                }

                _points.Add((i, minDistance));
            }
            _points.SortByReverse(e => e.MinDistance);
        }

        public void Draw(UITree tree) => tree.LeafNodes(_points, p => $"{p.MinDistance:f3}: {p.Inst.TimestampString()}");
    }

    class ActionData
    {
        public List<Instance> Instances = [];
        public List<(Replay, Replay.Participant, Replay.Cast)> Casts = [];
        public HashSet<uint> CasterOIDs = [];
        public HashSet<uint> TargetOIDs = [];
        public bool SeenTargetSelf;
        public bool SeenTargetOtherEnemy;
        public bool SeenTargetPlayer;
        public bool SeenTargetLocation;
        public bool SeenAOE;
        public float CastTime;
        public SourcePositionAnalysis? SrcPosAnalysis;
        public ConeAnalysis? ConeAnalysisSourcePosRot;
        public ConeAnalysis? ConeAnalysisTargetPosSourceRot;
        public ConeAnalysis? ConeAnalysisSourcePosDirToTarget;
        public RectAnalysis? RectAnalysisActionRot;
        public RectAnalysis? RectAnalysisSourceRot;
        public DamageFalloffAnalysis? DamageFalloffAnalysisDist;
        public DamageFalloffAnalysis? DamageFalloffAnalysisDistFromSource;
        public DamageFalloffAnalysis? DamageFalloffAnalysisMinCoord;
        public GazeAnalysis? GazeAnalysis;
        public KnockbackAnalysis? KnockbackAnalysis;
        public CasterLinkAnalysis? CasterLinkAnalysis;
    }

    private readonly Type? _aidType;
    private readonly Dictionary<ActionID, ActionData> _data = [];

    public AbilityInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = BossModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        _aidType = moduleInfo?.ActionIDType;
        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var action in replay.EncounterActions(enc))
                    AddActionData(replay, enc, action);
                foreach (var (_, participants) in enc.ParticipantsByOID)
                    foreach (var p in participants)
                        foreach (var c in p.Casts.Where(c => enc.Time.Contains(c.Time.Start)))
                            AddCastData(replay, p, c);
            }
        }
    }

    public AbilityInfo(List<Replay> replays)
    {
        foreach (var replay in replays)
        {
            foreach (var action in replay.Actions)
                AddActionData(replay, null, action);
            foreach (var p in replay.Participants)
                foreach (var c in p.Casts)
                    AddCastData(replay, p, c);
        }
    }

    public void Draw(UITree tree)
    {
        UITree.NodeProperties map(KeyValuePair<ActionID, ActionData> kv)
        {
            var name = kv.Key.Type == ActionType.Spell ? _aidType?.GetEnumName(kv.Key.ID) : null;
            return new($"{kv.Key:X} ({name})", false, name == null ? 0xff00ffff : 0xffffffff);
        }
        foreach (var (aid, data) in tree.Nodes(_data, map))
        {
            tree.LeafNode($"Caster IDs: {OIDListString(data.CasterOIDs)}");
            tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
            tree.LeafNode($"Targets:{JoinStrings(ActionTargetStrings(data))}");
            tree.LeafNode($"Cast time: {data.CastTime:f1}");
            if (aid.Type == ActionType.Spell)
            {
                foreach (var n in tree.Node("Lumina data"))
                {
                    var row = Service.LuminaRow<Lumina.Excel.Sheets.Action>(aid.ID);
                    tree.LeafNode($"Category: {row?.ActionCategory.ValueNullable?.Name}");
                    tree.LeafNode($"Cast time: {row?.Cast100ms * 0.1f:f1} + {row?.ExtraCastTime100ms * 0.1f:f1}");
                    tree.LeafNode($"Target range: {row?.Range}");
                    tree.LeafNode($"Effect shape: {row?.CastType} ({(row != null ? DescribeShape(row.Value) : "")})");
                    tree.LeafNode($"Effect range: {row?.EffectRange}");
                    tree.LeafNode($"Effect width: {row?.XAxisModifier}");
                    tree.LeafNode($"Omen: {row?.Omen.ValueNullable?.Path} / {row?.Omen.ValueNullable?.PathAlly}");
                    var omenAlt = row != null ? Service.LuminaRow<Lumina.Excel.Sheets.Omen>(row.Value.OmenAlt.RowId) : null;
                    tree.LeafNode($"Omen alt: {omenAlt?.Path} / {omenAlt?.PathAlly}");
                }
            }
            foreach (var n in tree.Node("Instances", data.Instances.Count == 0))
            {
                foreach (var an in tree.Nodes(data.Instances, inst => new($"{inst.TimestampString()}: {ReplayUtils.ParticipantPosRotString(inst.Action.Source, inst.Action.Timestamp)} -> {ReplayUtils.ParticipantString(inst.Action.MainTarget, inst.Action.Timestamp)} {Utils.Vec3String(inst.Action.TargetPos)} / {inst.Action.Rotation} ({inst.Action.Targets.Count} affected)", inst.Action.Targets.Count == 0)))
                {
                    foreach (var tn in tree.Nodes(an.Action.Targets, t => new(ReplayUtils.ActionTargetString(t, an.Action.Timestamp))))
                    {
                        tree.LeafNodes(tn.Effects, ReplayUtils.ActionEffectString);
                    }
                }
            }
            foreach (var n in tree.Node("Casts", data.Casts.Count == 0))
            {
                tree.LeafNodes(data.Casts, c => $"{c.Item1.Path} @ {c.Item3.Time.Start:O} + {c.Item3.Time.Duration:f3}/{c.Item3.ExpectedCastTime:f3}: {ReplayUtils.ParticipantString(c.Item2, c.Item3.Time.Start)} / {c.Item3.Rotation} -> {ReplayUtils.ParticipantPosRotString(c.Item3.Target, c.Item3.Time.Start)} / {Utils.Vec3String(c.Item3.Location)}");
            }
            foreach (var an in tree.Node("Source position analysis"))
            {
                data.SrcPosAnalysis ??= new(data.Instances);
                data.SrcPosAnalysis.Draw();
            }
            foreach (var an in tree.Node("Cone analysis (origin & rotation at source)"))
            {
                data.ConeAnalysisSourcePosRot ??= new(data.Instances, ConeAnalysis.Targeting.SourcePosRot);
                data.ConeAnalysisSourcePosRot.Draw();
            }
            foreach (var an in tree.Node("Cone analysis (origin at target, rotation from source)"))
            {
                data.ConeAnalysisTargetPosSourceRot ??= new(data.Instances, ConeAnalysis.Targeting.TargetPosSourceRot);
                data.ConeAnalysisTargetPosSourceRot.Draw();
            }
            foreach (var an in tree.Node("Cone analysis (origin at source, directed at target)"))
            {
                data.ConeAnalysisSourcePosDirToTarget ??= new(data.Instances, ConeAnalysis.Targeting.SourcePosDirToTarget);
                data.ConeAnalysisSourcePosDirToTarget.Draw();
            }
            foreach (var an in tree.Node("Rect analysis (rotation from action)"))
            {
                data.RectAnalysisActionRot ??= new(data.Instances, true);
                data.RectAnalysisActionRot.Draw();
            }
            foreach (var an in tree.Node("Rect analysis (rotation from source)"))
            {
                data.RectAnalysisSourceRot ??= new(data.Instances, false);
                data.RectAnalysisSourceRot.Draw();
            }
            foreach (var an in tree.Node("Damage falloff analysis (by distance)"))
            {
                data.DamageFalloffAnalysisDist ??= new(data.Instances, false, false);
                data.DamageFalloffAnalysisDist.Draw();
            }
            foreach (var an in tree.Node("Damage falloff analysis (by distance from source)"))
            {
                data.DamageFalloffAnalysisDistFromSource ??= new(data.Instances, false, true);
                data.DamageFalloffAnalysisDistFromSource.Draw();
            }
            foreach (var an in tree.Node("Damage falloff analysis (by max coord)"))
            {
                data.DamageFalloffAnalysisMinCoord ??= new(data.Instances, true, false);
                data.DamageFalloffAnalysisMinCoord.Draw();
            }
            foreach (var an in tree.Node("Gaze analysis"))
            {
                data.GazeAnalysis ??= new(data.Instances);
                data.GazeAnalysis.Draw();
            }
            foreach (var an in tree.Node("Knockback analysis"))
            {
                data.KnockbackAnalysis ??= new(data.Instances);
                data.KnockbackAnalysis.Draw(tree);
            }
            foreach (var an in tree.Node("Caster link analysis"))
            {
                data.CasterLinkAnalysis ??= new(data.Instances);
                data.CasterLinkAnalysis.Draw(tree);
            }
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate enum for boss module"))
        {
            var sb = new StringBuilder("public enum AID : uint\n{\n");
            foreach (var (key, value) in Utils.DedupKeys(_data.Select(d => EnumMemberString(d.Key, d.Value))))
                sb.AppendLine($"    {key} = {value}");
            sb.AppendLine("}");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Generate missing enum values for boss module"))
        {
            var sb = new StringBuilder();
            foreach (var (key, value) in Utils.DedupKeys(_data.Where(kv => kv.Key.Type != ActionType.Spell || _aidType?.GetEnumName(kv.Key.ID) == null).Select(d => EnumMemberString(d.Key, d.Value))))
                sb.AppendLine($"    {key} = {value}");
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private void AddActionData(Replay replay, Replay.Encounter? enc, Replay.Action action)
    {
        if (action.Source.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.DutySupport || action.Source.AllyAt(action.Timestamp))
            return;

        var data = _data.GetOrAdd(action.ID);
        data.CasterOIDs.Add(action.Source.OID);
        if (action.MainTarget != null && action.MainTarget.Type is not ActorType.Player and not ActorType.DutySupport)
            data.TargetOIDs.Add(action.MainTarget.OID);
        data.SeenTargetSelf |= action.Source == action.MainTarget;
        data.SeenTargetOtherEnemy |= action.MainTarget != action.Source && action.MainTarget?.Type == ActorType.Enemy;
        data.SeenTargetPlayer |= action.MainTarget?.Type is ActorType.Player or ActorType.DutySupport;
        data.SeenTargetLocation |= action.MainTarget == null;
        data.SeenAOE |= action.Targets.Count > 1;

        var cast = action.Source.Casts.Find(c => c.ID == action.ID && Math.Abs((c.Time.End - action.Timestamp).TotalSeconds) < 1);
        data.CastTime = cast?.ExpectedCastTime + 0.3f ?? 0;

        data.Instances.Add(new(replay, enc, action));
    }

    private void AddCastData(Replay replay, Replay.Participant caster, Replay.Cast cast)
    {
        if (caster.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.DutySupport || caster.AllyAt(cast.Time.Start))
            return;

        var data = _data.GetOrAdd(cast.ID);
        data.CasterOIDs.Add(caster.OID);
        if (cast.Target != null && cast.Target.Type is not ActorType.Player and not ActorType.DutySupport)
            data.TargetOIDs.Add(cast.Target.OID);
        data.SeenTargetSelf |= caster == cast.Target;
        data.SeenTargetOtherEnemy |= cast.Target != caster && cast.Target?.Type == ActorType.Enemy;
        data.SeenTargetPlayer |= cast.Target?.Type is ActorType.Player or ActorType.DutySupport;
        data.SeenTargetLocation |= cast.Target == null;
        data.CastTime = cast.ExpectedCastTime + 0.3f;

        data.Casts.Add((replay, caster, cast));
    }

    private static IEnumerable<Replay.Participant> AlivePlayersAt(Replay r, DateTime t)
        => r.Participants.Where(p => p.Type is ActorType.Player or ActorType.Chocobo or ActorType.DutySupport && p.ExistsInWorldAt(t) && !p.DeadAt(t));

    private IEnumerable<string> ActionTargetStrings(ActionData data)
    {
        if (data.SeenTargetSelf)
            yield return "self";
        if (data.SeenTargetPlayer)
            yield return data.SeenAOE ? "players" : "player";
        if (data.SeenTargetLocation)
            yield return "location";
        if (data.SeenTargetOtherEnemy)
            foreach (var oid in data.TargetOIDs)
                yield return OIDString(oid);
    }

    private string CastTimeString(ActionData data, Lumina.Excel.Sheets.Action? ldata)
        => data.CastTime > 0 ? string.Create(CultureInfo.InvariantCulture, $"{data.CastTime:f1}{(ldata?.ExtraCastTime100ms > 0 ? $"+{ldata?.ExtraCastTime100ms * 0.1f:f1}" : "")}s cast") : "no cast";

    private (string Name, string Value) EnumMemberString(ActionID aid, ActionData data)
    {
        var ldata = aid.Type == ActionType.Spell ? Service.LuminaRow<Lumina.Excel.Sheets.Action>(aid.ID) : null;
        string name = aid.Type != ActionType.Spell ? $"// {aid}" : _aidType?.GetEnumName(aid.ID) ?? $"_{Utils.StringToIdentifier(ldata?.ActionCategory.ValueNullable?.Name.ToString() ?? "")}_{Utils.StringToIdentifier(ldata?.Name.ToString() ?? $"Ability{aid.ID}")}";
        return (name, $"{aid.ID}, // {OIDListString(data.CasterOIDs)}->{JoinStrings(ActionTargetStrings(data))}, {CastTimeString(data, ldata)}, {(ldata != null ? DescribeShape(ldata.Value) : "????")}");
    }

    private string DescribeShape(Lumina.Excel.Sheets.Action data) => data.CastType switch
    {
        1 => "single-target",
        2 => $"range {data.EffectRange} circle",
        3 => $"range {data.EffectRange}+R {DetermineConeAngle(data)?.ToString() ?? "?"}-degree cone",
        4 => $"range {data.EffectRange}+R width {data.XAxisModifier} rect",
        5 => $"range {data.EffectRange}+R circle",
        8 => $"width {data.XAxisModifier} rect charge",
        10 => $"range {DetermineDonutInner(data)?.ToString() ?? "?"}-{data.EffectRange} donut",
        11 => $"range {data.EffectRange} width {data.XAxisModifier} cross",
        12 => $"range {data.EffectRange} width {data.XAxisModifier} rect",
        13 => $"range {data.EffectRange} {DetermineConeAngle(data)?.ToString() ?? "?"}-degree cone",
        _ => "???"
    };

    private Angle? DetermineConeAngle(Lumina.Excel.Sheets.Action data)
    {
        var omen = data.Omen.ValueNullable;
        if (omen == null)
            return null;

        var path = omen.Value.Path.ToString();
        var pos = path.IndexOf("fan", StringComparison.Ordinal);
        return pos >= 0 && pos + 6 <= path.Length && int.TryParse(path.AsSpan(pos + 3, 3), out var angle) ? angle.Degrees() : null;
    }

    private float? DetermineDonutInner(Lumina.Excel.Sheets.Action data)
    {
        var omen = data.Omen.ValueNullable;
        if (omen == null)
            return null;

        var path = omen.Value.Path.ToString();
        var pos = path.IndexOf("sircle_", StringComparison.Ordinal);
        if (pos >= 0 && pos + 11 <= path.Length && int.TryParse(path.AsSpan(pos + 9, 2), out var inner))
            return inner;

        pos = path.IndexOf("circle", StringComparison.Ordinal);
        if (pos >= 0 && pos + 10 <= path.Length && int.TryParse(path.AsSpan(pos + 8, 2), out inner))
            return inner;

        return null;
    }
}
