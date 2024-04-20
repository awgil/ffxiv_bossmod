namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class RhalgrBeaconAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RhalgrsBeaconAOE), new AOEShapeCircle(10));

class RhalgrBeaconShock(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Shock))
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LightningOrb)
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(13)));
    }
}

// TODO: this is a knockback 50, ignores immunities - but need to clamp to correct fingers
// there are two possible source locations ([-10.12, 268.50] and [-24.12, 266.50]), two potential fingers for each - one of them is sometimes covered by lightning aoes
class RhalgrBeaconKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.RhalgrsBeaconKnockback), 50, true, stopAfterWall: true, safeWalls: safewalls)
{
    private static readonly List<SafeWall> safewalls = [new (new(9.09f, 293.91f), new(3.31f, 297.2f)), new(new(-6.23f, 304.72f), new(-13.9f, 303.98f)), new(new(-22.35f, 306.16f), new(-31.3f, 304.94f)), new(new(-40.96f, 300.2f), new(-49.39f, 296.73f)), new(new(-46.3f, 276.04f), new(-52.64f, 274.2f))];
}
