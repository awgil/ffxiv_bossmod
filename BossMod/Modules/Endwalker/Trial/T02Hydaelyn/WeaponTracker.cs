namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class WeaponTracker(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B4)
            _aoe = new(new AOEShapeCircle(10), Module.PrimaryActor.Position, default, WorldState.FutureTime(6));
        if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B5)
            _aoe = new(new AOEShapeDonut(5, 40), Module.PrimaryActor.Position, default, WorldState.FutureTime(6));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HydaelynsWeapon)
            _aoe = new(new AOEShapeCross(40, 5), Module.PrimaryActor.Position, default, WorldState.FutureTime(6.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Equinox2 or AID.HighestHoly or AID.Anthelion)
            _aoe = null;
    }
}
