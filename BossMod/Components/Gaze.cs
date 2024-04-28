using ImGuiNET;

namespace BossMod.Components;

// generic gaze/weakpoint component, allows customized 'eye' position
public abstract class GenericGaze(BossModule module, ActionID aid = new(), bool inverted = false) : CastCounter(module, aid)
{
    public record struct Eye(
        WPos Position,
        DateTime Activation = new(),
        Angle Forward = new(), // if non-zero, treat specified side as 'forward' for hit calculations
        bool Risky = true);

    public bool Inverted = inverted; // if inverted, player should face eyes instead of averting

    private const float _eyeOuterH = 10;
    private const float _eyeOuterV = 6;
    private const float _eyeInnerR = 4;
    private const float _eyeOuterR = (_eyeOuterH * _eyeOuterH + _eyeOuterV * _eyeOuterV) / (2 * _eyeOuterV);
    private const float _eyeOffsetV = _eyeOuterR - _eyeOuterV;
    private static readonly float _eyeHalfAngle = MathF.Asin(_eyeOuterH / _eyeOuterR);

    public abstract IEnumerable<Eye> ActiveEyes(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveEyes(slot, actor).Any(eye => eye.Risky && HitByEye(actor, eye) != Inverted))
            hints.Add(Inverted ? "Face the eye!" : "Turn away from gaze!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Inverted)
        {
            foreach (var eye in ActiveEyes(slot, actor).Where(eye => eye.Risky))
                hints.ForbiddenDirections.Add((Angle.FromDirection(actor.Position - eye.Position) - eye.Forward, 135.Degrees(), eye.Activation));
        }
        else
        {
            foreach (var eye in ActiveEyes(slot, actor).Where(eye => eye.Risky))
                hints.ForbiddenDirections.Add((Angle.FromDirection(eye.Position - actor.Position) - eye.Forward, 45.Degrees(), eye.Activation));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var eye in ActiveEyes(pcSlot, pc))
        {
            bool danger = HitByEye(pc, eye) != Inverted;
            var eyeCenter = IndicatorScreenPos(eye.Position);
            var dl = ImGui.GetWindowDrawList();
            dl.PathArcTo(eyeCenter - new Vector2(0, _eyeOffsetV), _eyeOuterR, MathF.PI / 2 + _eyeHalfAngle, MathF.PI / 2 - _eyeHalfAngle);
            dl.PathArcTo(eyeCenter + new Vector2(0, _eyeOffsetV), _eyeOuterR, -MathF.PI / 2 + _eyeHalfAngle, -MathF.PI / 2 - _eyeHalfAngle);
            dl.PathFillConvex(danger ? ArenaColor.Enemy : ArenaColor.PC);
            dl.AddCircleFilled(eyeCenter, _eyeInnerR, ArenaColor.Border);

            if (eye.Risky)
            {
                var (min, max) = Inverted ? (45, 315) : (-45, 45);
                Arena.PathArcTo(pc.Position, 1, (pc.Rotation + eye.Forward + min.Degrees()).Rad, (pc.Rotation + eye.Forward + max.Degrees()).Rad);
                Arena.PathStroke(false, ArenaColor.Enemy);
            }
        }
    }

    private bool HitByEye(Actor actor, Eye eye) => (actor.Rotation + eye.Forward).ToDirection().Dot((eye.Position - actor.Position).Normalized()) >= 0.707107f; // 45-degree

    private Vector2 IndicatorScreenPos(WPos eye)
    {
        if (Module.InBounds(eye))
        {
            return Arena.WorldPositionToScreenPosition(eye);
        }
        else
        {
            var dir = (eye - Module.Center).Normalized();
            return Arena.ScreenCenter + Arena.RotatedCoords(dir.ToVec2()) * (Arena.ScreenHalfSize + Arena.ScreenMarginSize / 2);
        }
    }
}

// gaze that happens on cast end
public class CastGaze(BossModule module, ActionID aid, bool inverted = false) : GenericGaze(module, aid, inverted)
{
    private readonly List<Actor> _casters = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => _casters.Select(c => new Eye(c.Position, c.CastInfo!.NPCFinishAt));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }
}

// cast weakpoint component: a number of casts (with supposedly non-intersecting shapes), player should face specific side determined by active status to the caster for aoe he's in
public class CastWeakpoint(BossModule module, ActionID aid, AOEShape shape, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight) : GenericGaze(module, aid, true)
{
    public AOEShape Shape = shape;
    public readonly uint[] Statuses = [statusForward, statusLeft, statusBackward, statusRight]; // 4 elements: fwd, left, back, right
    private readonly List<Actor> _casters = [];
    private readonly Dictionary<ulong, Angle> _playerWeakpoints = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        // if there are multiple casters, take one that finishes first
        var caster = _casters.Where(a => Shape.Check(actor.Position, a.Position, a.CastInfo!.Rotation)).MinBy(a => a.CastInfo!.NPCFinishAt);
        if (caster != null && _playerWeakpoints.TryGetValue(actor.InstanceID, out var angle))
            yield return new(caster.Position, caster.CastInfo!.NPCFinishAt, angle);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
            _playerWeakpoints[actor.InstanceID] = statusKind * 90.Degrees();
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
            _playerWeakpoints.Remove(actor.InstanceID);
    }
}
