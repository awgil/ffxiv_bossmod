namespace BossMod.Stormblood.Ultimate.UWU;

// select best safespot for all predation patterns
class P4UltimatePredation(BossModule module) : BossComponent(module)
{
    public enum State { Inactive, Predicted, First, Second, Done }

    public State CurState { get; private set; }
    private readonly List<WPos> _hints = [];
    private readonly ArcList _first = new(new(), _dodgeRadius);
    private readonly ArcList _second = new(new(), _dodgeRadius);

    private const float _dodgeRadius = 19;
    private static readonly Angle _dodgeCushion = 2.5f.Degrees();

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var h in EnumerateHints(actor.Position))
            movementHints.Add(h);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var h in EnumerateHints(pc.Position))
            Arena.AddLine(h.from, h.to, h.color);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (CurState == State.Inactive && id == 0x1E43)
        {
            RecalculateHints();
            CurState = State.Predicted;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (CurState == State.Predicted && (AID)spell.Action.ID == AID.CrimsonCyclone)
            CurState = State.First;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CrimsonCyclone:
                if (CurState == State.First)
                {
                    CurState = State.Second;
                    if (_hints.Count > 0)
                        _hints.RemoveAt(0);
                }
                break;
            case AID.CrimsonCycloneCross:
                if (CurState == State.Second)
                {
                    CurState = State.Done;
                    _hints.Clear();
                }
                break;
        }
    }

    private IEnumerable<(WPos from, WPos to, uint color)> EnumerateHints(WPos starting)
    {
        uint color = ArenaColor.Safe;
        foreach (var p in _hints)
        {
            yield return (starting, p, color);
            starting = p;
            color = ArenaColor.Danger;
        }
    }

    private void RecalculateHints()
    {
        _first.Center = _second.Center = Module.Center;
        _first.Forbidden.Clear();
        _second.Forbidden.Clear();
        _hints.Clear();

        var castModule = (UWU)Module;
        var garuda = castModule.Garuda();
        var titan = castModule.Titan();
        var ifrit = castModule.Ifrit();
        var ultima = castModule.Ultima();
        if (garuda == null || titan == null || ifrit == null || ultima == null)
            return;

        _first.ForbidInfiniteRect(titan.Position, titan.Rotation, 3);
        _first.ForbidInfiniteRect(titan.Position, titan.Rotation + 45.Degrees(), 3);
        _first.ForbidInfiniteRect(titan.Position, titan.Rotation - 45.Degrees(), 3);
        _second.ForbidInfiniteRect(titan.Position, titan.Rotation + 22.5f.Degrees(), 3);
        _second.ForbidInfiniteRect(titan.Position, titan.Rotation - 22.5f.Degrees(), 3);
        _second.ForbidInfiniteRect(titan.Position, titan.Rotation + 90.Degrees(), 3);
        _first.ForbidInfiniteRect(ifrit.Position, ifrit.Rotation, 9);
        _second.ForbidInfiniteRect(Module.Center - new WDir(Module.Bounds.Radius, 0), 90.Degrees(), 5);
        _second.ForbidInfiniteRect(Module.Center - new WDir(0, Module.Bounds.Radius), 0.Degrees(), 5);
        _first.ForbidCircle(garuda.Position, 20);
        _second.ForbidCircle(garuda.Position, 20);
        _second.ForbidCircle(ultima.Position, 14);

        var safespots = EnumeratePotentialSafespots();
        var (a1, a2) = safespots.MinBy(AngularDistance);
        _hints.Add(GetSafePositionAtAngle(a1));
        _hints.Add(GetSafePositionAtAngle(a2));
    }

    private WPos GetSafePositionAtAngle(Angle angle) => Module.Center + _dodgeRadius * angle.ToDirection();

    private IEnumerable<(Angle, Angle)> EnumeratePotentialSafespots()
    {
        var safeFirst = _first.Allowed(_dodgeCushion);
        var safeSecond = _second.Allowed(_dodgeCushion);
        foreach (var (min1, max1) in safeFirst)
        {
            foreach (var (min2, max2) in safeSecond)
            {
                var intersectMin = MathF.Max(min1.Rad, min2.Rad).Radians();
                var intersectMax = MathF.Min(max1.Rad, max2.Rad).Radians();
                if (intersectMin.Rad < intersectMax.Rad)
                {
                    var midpoint = ((intersectMin + intersectMax) * 0.5f).Normalized();
                    yield return (midpoint, midpoint);
                }
                else
                {
                    yield return (max1.Normalized(), min2.Normalized());
                    yield return (min1.Normalized(), max2.Normalized());
                }
            }
        }
    }

    private float AngularDistance((Angle, Angle) p)
    {
        var dist = MathF.Abs(p.Item1.Rad - p.Item2.Rad);
        return dist < MathF.PI ? dist : 2 * MathF.PI - dist;
    }
}
