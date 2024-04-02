namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class WeaponTracker : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B4)
            _aoe = new(new AOEShapeCircle(10), module.PrimaryActor.Position, activation: module.WorldState.CurrentTime.AddSeconds(6));
        if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B5)
            _aoe = new(new AOEShapeDonut(5, 40), module.PrimaryActor.Position, activation: module.WorldState.CurrentTime.AddSeconds(6));
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HydaelynsWeapon)
            _aoe = new(new AOEShapeCross(40, 5), module.PrimaryActor.Position, activation: module.WorldState.CurrentTime.AddSeconds(6.9f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Equinox2 or AID.HighestHoly or AID.Anthelion)
            _aoe = null;
    }
}
