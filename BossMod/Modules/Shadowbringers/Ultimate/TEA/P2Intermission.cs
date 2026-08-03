namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P2IntermissionLimitCut(BossModule module) : LimitCut(module, 3.2d);

[SkipLocalsInit]
sealed class P2IntermissionHawkBlaster(BossModule module) : Components.GenericAOEs(module, (uint)AID.HawkBlasterIntermission)
{
    private Angle _blasterStartingDirection;
    private readonly TEAConfig _config = Service.Config.Get<TEAConfig>();
    private const float _blasterOffset = 14f;
    private readonly AOEShapeCircle _blasterShape = new(10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var future = FutureBlasterCenters();
        var imminent = ImminentBlasterCenters();
        var lenF = future.Length;
        var lenI = imminent.Length;
        var aoes = new AOEInstance[lenF + lenI];
        for (var i = 0; i < lenF; ++i)
        {
            aoes[i] = new(_blasterShape, future[i], risky: false);
        }
        for (var i = 0; i < lenI; ++i)
        {
            aoes[i + lenF] = new(_blasterShape, imminent[i], color: Colors.Danger);
        }
        return aoes;
    }

    // TODO: reconsider
    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (SafeSpotHint(slot) is var safespot && safespot != null)
            movementHints.Add(actor.Position, safespot.Value, Colors.Safe);
    }

    // TODO: reconsider
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (SafeSpotHint(pcSlot) is var safespot && safespot != null)
            Arena.ZoneCircleOutline(safespot.Value, 1f, Colors.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            if (NumCasts == 0)
            {
                var offset = spell.TargetXZ - Arena.Center;
                // a bit of a hack: most strats (lpdu etc) select a half between W and NE inclusive to the 'first' group; ensure 'starting' direction is one of these
                var invert = Math.Abs(offset.Z) < 2f ? offset.X > 0f : offset.Z > 0f;
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

    private WPos[] BlasterCenters(int index)
    {
        switch (index)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                {
                    var dir = (_blasterStartingDirection - index * 45f.Degrees()).ToDirection();
                    return
                        [
                            Arena.Center + _blasterOffset * dir,
                            Arena.Center - _blasterOffset * dir
                        ];
                }
            case 5:
            case 6:
            case 7:
            case 8:
                {
                    var dir = (_blasterStartingDirection - (index - 5) * 45f.Degrees()).ToDirection();
                    return
                        [
                            Arena.Center + _blasterOffset * dir,
                            Arena.Center - _blasterOffset * dir
                        ];
                }
            case 4:
            case 9:
                return [Arena.Center];
            default:
                return [];
        }
    }

    private WPos[] ImminentBlasterCenters() => NumCasts > 0 ? BlasterCenters(NextBlasterIndex) : [];
    private WPos[] FutureBlasterCenters() => NumCasts > 0 ? BlasterCenters(NextBlasterIndex + 1) : [];

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

        var strategy = _config.P2IntermissionHints;
        if (strategy == TEAConfig.P2Intermission.None)
            return null;

        var invert = strategy == TEAConfig.P2Intermission.FirstForOddPairs && Module.FindComponent<LimitCut>()?.PlayerOrder[slot] is 3 or 4 or 7 or 8;
        var offset = _blasterOffset * _blasterStartingDirection.ToDirection();
        return Arena.Center + (invert ? -offset : offset);
    }
}
