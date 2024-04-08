namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class RhalgrBeaconAOE : Components.SelfTargetedAOEs
{
    public RhalgrBeaconAOE() : base(ActionID.MakeSpell(AID.RhalgrsBeaconAOE), new AOEShapeCircle(10)) { }
}

class RhalgrBeaconShock : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(8);

    public RhalgrBeaconShock() : base(ActionID.MakeSpell(AID.Shock)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.LightningOrb)
            _aoes.Add(new(_shape, actor.Position, default, module.WorldState.CurrentTime.AddSeconds(13)));
    }
}

// TODO: this is a knockback 50, ignores immunities - but need to clamp to correct fingers
// there are two possible source locations ([-10.12, 268.50] and [-24.12, 266.50]), two potential fingers for each - one of them is sometimes covered by lightning aoes
class RhalgrBeaconKnockback : Components.KnockbackFromCastTarget
{
    public RhalgrBeaconKnockback() : base(ActionID.MakeSpell(AID.RhalgrsBeaconKnockback), 50, true) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (Sources(module, slot, actor).Any())
            hints.Add("Get knocked to a correct finger!");
    }
}
