namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

class BodySlamKB(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private Source? _knockback;
    private float LeviathanZ;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void Update()
    {
        if (LeviathanZ == default)
            LeviathanZ = Module.PrimaryActor.Position.Z;
        if (Module.PrimaryActor.Position.Z != LeviathanZ && Module.PrimaryActor.Position.Z != 0)
        {
            LeviathanZ = Module.PrimaryActor.Position.Z;
            _knockback = new(Module.Center, 25, WorldState.FutureTime(4.8f), Direction: Module.PrimaryActor.Position.Z <= 0 ? 180.Degrees() : 0.Degrees(), Kind: Kind.DirForward);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BodySlamNorth or AID.BodySlamSouth)
            _knockback = null;
    }
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Hydroshot>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (Module.FindComponent<Dreadstorm>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false);

}

class BodySlamAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private float LeviathanZ;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void Update()
    {
        if (LeviathanZ == default)
            LeviathanZ = Module.PrimaryActor.Position.Z;
        if (Module.PrimaryActor.Position.Z != LeviathanZ && Module.PrimaryActor.Position.Z != 0)
        {
            LeviathanZ = Module.PrimaryActor.Position.Z;
            _aoe = new(new AOEShapeRect(30, 5), Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, WorldState.FutureTime(2.6f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BodySlamRectAOE)
            _aoe = null;
    }
}
