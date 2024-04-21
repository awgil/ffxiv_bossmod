namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component tracking [lateral] aureole mechanic, only exists for the timeline anymore
class Aureole(BossModule module) : BossComponent(module)
{
    public bool Done { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Aureole1AOE or AID.Aureole2AOE or AID.LateralAureole1AOE or AID.LateralAureole2AOE)
            Done = true;
    }
}
