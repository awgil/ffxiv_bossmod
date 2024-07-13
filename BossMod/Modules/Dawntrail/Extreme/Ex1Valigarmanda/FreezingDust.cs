namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class FreezingDust : Components.StayMove
{
    public int NumActiveFreezes;

    public FreezingDust(BossModule module) : base(module)
    {
        foreach (var (i, _) in module.Raid.WithSlot(true))
            Requirements[i] = Requirement.Move;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FreezingUp)
            ++NumActiveFreezes;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FreezingUp)
            --NumActiveFreezes;
    }
}
