namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

// TODO: how does it really work?
class DominionSlash(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.DominionSlashHelper && state == 0x00010002)
            AOEs.Add(new(_shape, actor.Position, default, WorldState.FutureTime(6.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DivineDominion)
        {
            ++NumCasts;
            AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}
