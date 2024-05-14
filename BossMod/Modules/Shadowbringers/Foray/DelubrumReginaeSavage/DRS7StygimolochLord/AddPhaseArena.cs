namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class AddPhaseArena(BossModule module) : BossComponent(module)
{
    private const float _innerRingRadius = 14.5f;
    private const float _outerRingRadius = 27.5f;
    private const float _ringHalfWidth = 2.5f;
    private const float _alcoveDepth = 1;
    private const float _alcoveWidth = 2;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Arena.ZoneRelPoly((GetType(), 0), InDanger(), ArenaColor.AOE);
        Arena.ZoneRelPoly((GetType(), 1), MidDanger(), ArenaColor.AOE);
        Arena.ZoneRelPoly((GetType(), 2), OutDanger(), ArenaColor.AOE);
    }

    private IEnumerable<WDir> RingBorder(Angle centerOffset, float ringRadius, bool innerBorder)
    {
        float offsetMultiplier = innerBorder ? -1 : 1;
        Angle halfWidth = (_alcoveWidth / ringRadius).Radians();
        for (int i = 0; i < 8; ++i)
        {
            var centerAlcove = centerOffset + i * 45.Degrees();
            foreach (var p in CurveApprox.CircleArc(ringRadius + offsetMultiplier * (_ringHalfWidth + _alcoveDepth), centerAlcove - halfWidth, centerAlcove + halfWidth, Module.Bounds.MaxApproxError))
                yield return p;
            foreach (var p in CurveApprox.CircleArc(ringRadius + offsetMultiplier * _ringHalfWidth, centerAlcove + halfWidth, centerAlcove + 45.Degrees() - halfWidth, Module.Bounds.MaxApproxError))
                yield return p;
        }
    }

    private IEnumerable<WDir> RepeatFirst(IEnumerable<WDir> pts)
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

    private IEnumerable<WDir> InDanger() => RingBorder(22.5f.Degrees(), _innerRingRadius, true);

    private IEnumerable<WDir> MidDanger()
    {
        foreach (var p in RepeatFirst(RingBorder(0.Degrees(), _outerRingRadius, true)))
            yield return p;
        foreach (var p in RepeatFirst(RingBorder(22.5f.Degrees(), _innerRingRadius, false)))
            yield return p;
    }

    private IEnumerable<WDir> OutDanger()
    {
        foreach (var p in RepeatFirst(CurveApprox.Circle(Module.Bounds.Radius, Module.Bounds.MaxApproxError)))
            yield return p;
        foreach (var p in RepeatFirst(RingBorder(0.Degrees(), _outerRingRadius, false)))
            yield return p;
    }
}
