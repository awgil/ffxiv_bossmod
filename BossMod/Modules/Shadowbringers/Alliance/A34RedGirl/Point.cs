namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

class Point(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Barrier _barriers = module.FindComponent<Barrier>()!;
    private readonly List<(Actor Caster, DateTime Activation, bool IsBlocked)> _casters = [];

    public bool Active => _casters.Count > 0;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.BlackLance or OID.WhiteLance && !_casters.Any(c => c.Caster.Position.AlmostEqual(actor.Position, 1)))
        {
            var shade = (OID)actor.OID == OID.BlackLance ? Shade.Black : Shade.White;
            var isBlocked = _barriers.BarrierPositions.Any(b => b.Center.InRect(actor.Position, actor.Rotation, 50, 0, 3) && b.Shade != shade);
            _casters.Add((actor, WorldState.FutureTime(7.7f), isBlocked));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PointBlackLong or AID.PointBlackShort or AID.PointWhiteShort or AID.PointWhiteLong)
        {
            NumCasts++;
            _casters.Clear();
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID is OID.BlackLance or OID.WhiteLance && id is 0x11D1 or 0x11D2)
        {
            var shade = (OID)actor.OID == OID.BlackLance ? Shade.Black : Shade.White;
            var isBlocked = _barriers.BarrierPositions.Any(b => b.Center.InRect(actor.Position, actor.Rotation, 50, 0, 3) && b.Shade != shade);
            _casters.Add((actor, WorldState.FutureTime(8), isBlocked));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(new AOEShapeRect(c.IsBlocked ? 24 : 50, 3), c.Caster.Position, c.Caster.Rotation, c.Activation);
    }
}
