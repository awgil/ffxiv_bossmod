namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class TwistNDrop(BossModule module) : Components.GroupedAOEs(module, [.. BossCasts, .. HelperCasts], new AOEShapeRect(20, 20))
{
    public bool Side1 { get; private set; }
    public bool Side2 { get; private set; }

    // he REALLY likes casting these
    public static readonly AID[] BossCasts = [
        AID.W2SnapBoss1, AID.W3SnapBoss1, AID.W4SnapBoss1, AID.W2SnapBoss2, AID.W2SnapBoss3, AID.W4SnapBoss2, AID.W4SnapBoss3, AID.W2SnapBoss4, AID.W2SnapBoss5, AID.W3SnapBoss2, AID.W3SnapBoss3, AID.W4SnapBoss4, AID.W3SnapBoss4, AID.W4SnapBoss5, AID.W3SnapBoss5, AID.W3SnapBoss6, AID.W3SnapBoss7, AID.W4SnapBoss6, AID.W2SnapBoss6, AID.W2SnapBoss7, AID.W4SnapBoss7, AID.W2SnapBoss8, AID.W3SnapBoss8, AID.W4SnapBoss8
    ];

    public static readonly AID[] HelperCasts = [
        AID.W2SnapAOE1, AID.W2SnapAOELast, AID.W3SnapAOE1, AID.W3SnapAOE2, AID.W3SnapAOELast, AID.W4SnapAOE1, AID.W4SnapAOE2, AID.W4SnapAOE3, AID.W4SnapAOELast,
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters
        .OrderBy(c => Module.CastFinishAt(c.CastInfo)) // only show AOE that will resolve last so we don't clog up the minimap with 4 of the same rect
        .Take(1)
        .Select(csr => new AOEInstance(Shape, csr.CastInfo!.LocXZ, csr.CastInfo.Rotation, Module.CastFinishAt(csr.CastInfo), Color, Risky));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID is AID.W2SnapAOE1 or AID.W3SnapAOE2 or AID.W4SnapAOE3)
            Side1 = true;

        if ((AID)spell.Action.ID is AID.W2SnapAOELast or AID.W3SnapAOELast or AID.W4SnapAOELast)
            Side2 = true;
    }
}

