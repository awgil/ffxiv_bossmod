namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class TwistNDrop(BossModule module) : Components.GroupedAOEs(module, [.. BossCasts, .. HelperCasts], new AOEShapeRect(20, 20))
{
    public bool Side1 { get; private set; }
    public bool Side2 { get; private set; }

    public static readonly AID[] BossCasts = [
        AID._Weaponskill_2SnapTwistDropTheNeedle, AID._Weaponskill_3SnapTwistDropTheNeedle, AID._Weaponskill_4SnapTwistDropTheNeedle, AID._Weaponskill_2SnapTwistDropTheNeedle6, AID._Weaponskill_2SnapTwistDropTheNeedle5, AID._Weaponskill_4SnapTwistDropTheNeedle8, AID._Weaponskill_4SnapTwistDropTheNeedle7, AID._Weaponskill_2SnapTwistDropTheNeedle7, AID._Weaponskill_2SnapTwistDropTheNeedle8, AID._Weaponskill_3SnapTwistDropTheNeedle6, AID._Weaponskill_3SnapTwistDropTheNeedle7, AID._Weaponskill_4SnapTwistDropTheNeedle6, AID._Weaponskill_3SnapTwistDropTheNeedle4, AID._Weaponskill_4SnapTwistDropTheNeedle9, AID._Weaponskill_3SnapTwistDropTheNeedle8, AID._Weaponskill_3SnapTwistDropTheNeedle9, AID._Weaponskill_3SnapTwistDropTheNeedle5, AID._Weaponskill_4SnapTwistDropTheNeedle5, AID._Weaponskill_2SnapTwistDropTheNeedle3, AID._Weaponskill_2SnapTwistDropTheNeedle4
    ];

    public static readonly AID[] HelperCasts = [
        AID._Weaponskill_2SnapTwistDropTheNeedle1, AID._Weaponskill_2SnapTwistDropTheNeedle2, AID._Weaponskill_3SnapTwistDropTheNeedle1, AID._Weaponskill_3SnapTwistDropTheNeedle2, AID._Weaponskill_3SnapTwistDropTheNeedle3, AID._Weaponskill_4SnapTwistDropTheNeedle1, AID._Weaponskill_4SnapTwistDropTheNeedle2, AID._Weaponskill_4SnapTwistDropTheNeedle3, AID._Weaponskill_4SnapTwistDropTheNeedle4,
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.OrderBy(c => Module.CastFinishAt(c.CastInfo)).Take(1).Select(csr => new AOEInstance(Shape, csr.CastInfo!.LocXZ, csr.CastInfo.Rotation, Module.CastFinishAt(csr.CastInfo), Color, Risky));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID is AID._Weaponskill_2SnapTwistDropTheNeedle1 or AID._Weaponskill_3SnapTwistDropTheNeedle2 or AID._Weaponskill_4SnapTwistDropTheNeedle3)
            Side1 = true;

        if ((AID)spell.Action.ID is AID._Weaponskill_2SnapTwistDropTheNeedle2 or AID._Weaponskill_3SnapTwistDropTheNeedle3 or AID._Weaponskill_4SnapTwistDropTheNeedle4)
            Side2 = true;
    }
}

