namespace BossMod.Shadowbringers.Ultimate.TEA;

class P2IntermissionLimitCut(BossModule module) : LimitCut(module, 3.2f);

class P2IntermissionHawkBlaster(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.HawkBlasterIntermission))
{
    private Angle _blasterStartingDirection;

    private const float _blasterOffset = 14;
    private static readonly AOEShapeCircle _blasterShape = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in FutureBlasterCenters())
            yield return new(_blasterShape, c, Risky: false);
        foreach (var c in ImminentBlasterCenters())
            yield return new(_blasterShape, c, Color: ArenaColor.Danger);
    }

    // TODO: reconsider
    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (SafeSpotHint(slot) is var safespot && safespot != null)
            movementHints.Add(actor.Position, safespot.Value, ArenaColor.Safe);
    }

    // TODO: reconsider
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (SafeSpotHint(pcSlot) is var safespot && safespot != null)
            Arena.AddCircle(safespot.Value, 1, ArenaColor.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (NumCasts == 0)
            {
                var offset = spell.TargetXZ - Module.Center;
                // a bit of a hack: most strats (lpdu etc) select a half between W and NE inclusive to the 'first' group; ensure 'starting' direction is one of these
                bool invert = Math.Abs(offset.Z) < 2 ? offset.X > 0 : offset.Z > 0;
                if (invert)
                    offset = -offset;
                _blasterStartingDirection = Angle.FromDirection(offset);
            }
            ++NumCasts;
        }
    }

    // 0,1,2,3 - offset aoes, 4 - center aoe
    private int NextBlasterIndex => NumCasts switch
    {
        0 or 1 => 0,
        2 or 3 => 1,
        4 or 5 => 2,
        6 or 7 => 3,
        8 => 4,
        9 or 10 => 5,
        11 or 12 => 6,
        13 or 14 => 7,
        15 or 16 => 8,
        17 => 9,
        _ => 10
    };

    private IEnumerable<WPos> BlasterCenters(int index)
    {
        switch (index)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                {
                    var dir = (_blasterStartingDirection - index * 45.Degrees()).ToDirection();
                    yield return Module.Center + _blasterOffset * dir;
                    yield return Module.Center - _blasterOffset * dir;
                }
                break;
            case 5:
            case 6:
            case 7:
            case 8:
                {
                    var dir = (_blasterStartingDirection - (index - 5) * 45.Degrees()).ToDirection();
                    yield return Module.Center + _blasterOffset * dir;
                    yield return Module.Center - _blasterOffset * dir;
                }
                break;
            case 4:
            case 9:
                yield return Module.Center;
                break;
        }
    }

    private IEnumerable<WPos> ImminentBlasterCenters() => NumCasts > 0 ? BlasterCenters(NextBlasterIndex) : [];
    private IEnumerable<WPos> FutureBlasterCenters() => NumCasts > 0 ? BlasterCenters(NextBlasterIndex + 1) : [];

    // TODO: reconsider
    private WPos? SafeSpotHint(int slot)
    {
        //var safespots = NextBlasterIndex switch
        //{
        //    1 or 2 or 3 or 4 => BlasterCenters(module, NextBlasterIndex - 1),
        //    5 => BlasterCenters(module, 3),
        //    6 or 7 or 8 => BlasterCenters(module, NextBlasterIndex - 1),
        //    _ => Enumerable.Empty<WPos>()
        //};
        if (NextBlasterIndex != 1)
            return null;

        var strategy = Service.Config.Get<TEAConfig>().P2IntermissionHints;
        if (strategy == TEAConfig.P2Intermission.None)
            return null;

        bool invert = strategy == TEAConfig.P2Intermission.FirstForOddPairs && (Module.FindComponent<LimitCut>()?.PlayerOrder[slot] is 3 or 4 or 7 or 8);
        var offset = _blasterOffset * _blasterStartingDirection.ToDirection();
        return Module.Center + (invert ? -offset : offset);
    }
}
