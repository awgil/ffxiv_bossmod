namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class AddPhaseArena(BossModule module) : BossComponent(module)
{
    private const float _innerRingRadius = 14.5f;
    private const float _outerRingRadius = 27.5f;
    private const float _ringHalfWidth = 2.5f;
    private const float _alcoveDepth = 1;
    private const float _alcoveWidth = 2;
    private bool Active;
    private static readonly List<Func<WPos, float>> _labyrinthDistances;

    static AddPhaseArena()
    {
        _labyrinthDistances =
        [
            ShapeDistance.ConcavePolygon(ConvertToWPos(InDanger())),
            ShapeDistance.ConcavePolygon(ConvertToWPos(MidDanger())),
            ShapeDistance.ConcavePolygon(ConvertToWPos(OutDanger()))
        ];
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
        {
            Arena.ZoneRelPoly((GetType(), 0), InDanger(), ArenaColor.AOE);
            Arena.ZoneRelPoly((GetType(), 1), MidDanger(), ArenaColor.AOE);
            Arena.ZoneRelPoly((GetType(), 2), OutDanger(), ArenaColor.AOE);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MemoryOfTheLabyrinth)
        {
            Active = true;
            Module.Arena.Bounds = labPhase;
        }
    }

    private static IEnumerable<WDir> RingBorder(Angle centerOffset, float ringRadius, bool innerBorder)
    {
        float offsetMultiplier = innerBorder ? -1 : 1;
        var halfWidth = (_alcoveWidth / ringRadius).Radians();
        for (var i = 0; i < 8; ++i)
        {
            var centerAlcove = centerOffset + i * 45.Degrees();
            foreach (var p in CurveApprox.CircleArc(ringRadius + offsetMultiplier * (_ringHalfWidth + _alcoveDepth), centerAlcove - halfWidth, centerAlcove + halfWidth, Shape.MaxApproxError))
                yield return p;
            foreach (var p in CurveApprox.CircleArc(ringRadius + offsetMultiplier * _ringHalfWidth, centerAlcove + halfWidth, centerAlcove + 45.Degrees() - halfWidth, Shape.MaxApproxError))
                yield return p;
        }
    }

    private static IEnumerable<WDir> RepeatFirst(IEnumerable<WDir> pts)
    {
        WDir? first = null;
        foreach (var p in pts)
        {
            first ??= p;
            yield return p;
        }
        if (first != null)
            yield return first.Value;
    }

    private static IEnumerable<WDir> InDanger() => RingBorder(22.5f.Degrees(), _innerRingRadius, true);

    private static IEnumerable<WDir> MidDanger()
    {
        var outerRing = RepeatFirst(RingBorder(0.Degrees(), _outerRingRadius, true));
        var innerRing = RepeatFirst(RingBorder(22.5f.Degrees(), _innerRingRadius, false)).Reverse();
        return outerRing.Concat(innerRing);
    }

    private static IEnumerable<WDir> OutDanger()
    {
        var outerBoundary = RepeatFirst(CurveApprox.Circle(34.6f, Shape.MaxApproxError));
        var innerRing = RepeatFirst(RingBorder(0.Degrees(), _outerRingRadius, false)).Reverse();
        return outerBoundary.Concat(innerRing);
    }

    private static IEnumerable<WPos> ConvertToWPos(IEnumerable<WDir> directions)
    {
        return directions.Select(dir => new WPos(DRS7.BoundsCenter.X + dir.X, DRS7.BoundsCenter.Z + dir.Z));
    }

    private static readonly List<Shape> labyrinth = [new PolygonCustom(ConvertToWPos(InDanger())),
        new PolygonCustom(ConvertToWPos(MidDanger())), new PolygonCustom(ConvertToWPos(OutDanger()))];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!Active)
        {
            hints.AddForbiddenZone(p => _labyrinthDistances.Select(f => f(p)).Min());
        }
    }

    public static readonly ArenaBounds labPhase = new ArenaBoundsComplex([new Circle(DRS7.BoundsCenter, 34.5f)], labyrinth);
}
