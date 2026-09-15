namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

class SpinningDive(BossModule module) : Components.GenericAOEs(module) //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorCreated(Actor actor)
    {
        var SpinningDiveHelper = Module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();
        if ((OID)actor.OID == OID.SpinningDiveHelper)
            _aoe = new(new AOEShapeRect(46, 8), SpinningDiveHelper!.Position, SpinningDiveHelper.Rotation, WorldState.FutureTime(0.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpinningDiveSnapshot)
            _aoe = null;
    }
}

class SpinningDiveKB(BossModule module) : Components.Knockback(module, stopAtWall: true) //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnActorCreated(Actor actor)
    {
        var SpinningDiveHelper = Module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();
        if ((OID)actor.OID == OID.SpinningDiveHelper)
            _knockback = new(SpinningDiveHelper!.Position, 10, WorldState.FutureTime(1.4f), new AOEShapeRect(46, 8), SpinningDiveHelper!.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpinningDiveEffect)
            _knockback = null;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Hydroshot>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (Module.FindComponent<Dreadstorm>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false);
}
