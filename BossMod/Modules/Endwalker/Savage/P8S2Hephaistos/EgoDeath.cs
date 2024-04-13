namespace BossMod.Endwalker.Savage.P8S2;

class EgoDeath(BossModule module) : BossComponent(module)
{
    public BitMask InEventMask;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.InEvent)
            InEventMask.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.InEvent)
            InEventMask.Clear(Raid.FindSlot(actor.InstanceID));
    }
}
