namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

class SpinningDive : Components.GenericAOEs //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        var SpinningDiveHelper = module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();
        if ((OID)actor.OID == OID.SpinningDiveHelper)
            _aoe = new(new AOEShapeRect(46, 8), SpinningDiveHelper!.Position, SpinningDiveHelper.Rotation, module.WorldState.CurrentTime.AddSeconds(0.6f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpinningDiveSnapshot)
            _aoe = null;
    }
}

class SpinningDiveKB : Components.Knockback //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
{
    private Source? _knockback;

    public SpinningDiveKB()
    {
        StopAtWall = true;
    }

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        var SpinningDiveHelper = module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();
        if ((OID)actor.OID == OID.SpinningDiveHelper)
            _knockback = new(SpinningDiveHelper!.Position, 10, module.WorldState.CurrentTime.AddSeconds(1.4f), new AOEShapeRect(46, 8), SpinningDiveHelper!.Rotation);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpinningDiveEffect)
            _knockback = null;
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => (module.FindComponent<Hydroshot>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (module.FindComponent<Dreadstorm>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false);
}
