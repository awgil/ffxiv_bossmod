namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class WeaponTracker(BossModule module) : Components.GenericAOEs(module)
{
    public bool AOEImminent { get; private set; }
    private AOEInstance? _aoe;
    public enum Stance { None, Sword, Staff, Chakram }
    public Stance CurStance { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B4)
        {
            _aoe = new(new AOEShapeCircle(10), Module.PrimaryActor.Position, default, WorldState.FutureTime(6));
            CurStance = Stance.Staff;
            AOEImminent = true;
        }

        if ((SID)status.ID == SID.HydaelynsWeapon && status.Extra == 0x1B5)
        {
            _aoe = new(new AOEShapeDonut(5, 40), Module.PrimaryActor.Position, default, WorldState.FutureTime(6));
            AOEImminent = true;
            CurStance = Stance.Chakram;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HydaelynsWeapon)
        {
            _aoe = new(new AOEShapeCross(40, 5), Module.PrimaryActor.Position, default, WorldState.FutureTime(6.9f));
            AOEImminent = true;
            CurStance = Stance.Sword;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WeaponChangeAOEChakram or AID.WeaponChangeAOEStaff or AID.WeaponChangeAOESword)
        {
            AOEImminent = false;
            _aoe = null;
        }
    }
}
