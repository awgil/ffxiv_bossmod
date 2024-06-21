namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class Border(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos BoundsCenter = new(-416, -184);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsCircle(34.5f);
    private const float _innerRingRadius = 14.5f;
    private const float _outerRingRadius = 27.5f;
    private const float _ringHalfWidth = 2.5f;
    private const float _alcoveDepth = 1;
    private const float _alcoveWidth = 2;
    private bool Active;
    private static readonly List<Shape> labyrinth = [new PolygonCustom(InDanger()), new PolygonCustom(MidDanger()), new PolygonCustom(OutDanger())];
    public static readonly AOEShapeCustom customShape = new(labyrinth);
    public static readonly ArenaBounds labPhase = new ArenaBoundsComplex([new Circle(BoundsCenter, 34.5f)], labyrinth);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield return new(customShape, Module.Arena.Center);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MemoryOfTheLabyrinth)
        {
            Active = true;
            Module.Arena.Bounds = labPhase;
        }
    }

    private static IEnumerable<WPos> RingBorder(Angle centerOffset, float ringRadius, bool innerBorder)
    {
        float offsetMultiplier = innerBorder ? -1 : 1;
        var halfWidth = (_alcoveWidth / ringRadius).Radians();
        for (var i = 0; i < 8; ++i)
        {
            var centerAlcove = centerOffset + i * 45.Degrees();
            foreach (var p in CurveApprox.CircleArc(BoundsCenter, ringRadius + offsetMultiplier * (_ringHalfWidth + _alcoveDepth), centerAlcove - halfWidth, centerAlcove + halfWidth, Shape.MaxApproxError))
                yield return p;
            foreach (var p in CurveApprox.CircleArc(BoundsCenter, ringRadius + offsetMultiplier * _ringHalfWidth, centerAlcove + halfWidth, centerAlcove + 45.Degrees() - halfWidth, Shape.MaxApproxError))
                yield return p;
        }
    }

    private static IEnumerable<WPos> RepeatFirst(IEnumerable<WPos> pts)
    {
        WPos? first = null;
        foreach (var p in pts)
        {
            first ??= p;
            yield return p;
        }
        if (first != null)
            yield return first.Value;
    }

    private static IEnumerable<WPos> InDanger() => RingBorder(22.5f.Degrees(), _innerRingRadius, true);

    private static IEnumerable<WPos> MidDanger()
    {
        var outerRing = RepeatFirst(RingBorder(0.Degrees(), _outerRingRadius, true));
        var innerRing = RepeatFirst(RingBorder(22.5f.Degrees(), _innerRingRadius, false)).Reverse();
        return outerRing.Concat(innerRing);
    }

    private static IEnumerable<WPos> OutDanger()
    {
        var outerBoundary = RepeatFirst(CurveApprox.Circle(BoundsCenter, 34.6f, Shape.MaxApproxError));
        var innerRing = RepeatFirst(RingBorder(0.Degrees(), _outerRingRadius, false)).Reverse();
        return outerBoundary.Concat(innerRing);
    }
}
