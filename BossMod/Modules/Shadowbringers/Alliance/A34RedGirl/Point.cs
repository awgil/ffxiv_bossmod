namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class Point(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Barrier _barriers = module.FindComponent<Barrier>()!;
    private readonly List<(Actor Caster, DateTime Activation, bool IsBlocked)> _casters = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID._Gen_BlackLance or OID._Gen_WhiteLance)
        {
            var isBlocked = _barriers.BarrierPositions.Any(b => b.Center.InRect(actor.Position, actor.Rotation, 50, 0, 3));
            _casters.Add((actor, WorldState.FutureTime(7.7f), isBlocked));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_PointBlack or AID._Weaponskill_PointBlack1 or AID._Weaponskill_PointWhite or AID._Weaponskill_PointWhite1)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Caster == caster);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(new AOEShapeRect(c.IsBlocked ? 24 : 50, 3), c.Caster.Position, c.Caster.Rotation, c.Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (c, _, _) in _casters)
            Arena.ActorOutsideBounds(c.Position, c.Rotation, ArenaColor.Object);
    }
}
