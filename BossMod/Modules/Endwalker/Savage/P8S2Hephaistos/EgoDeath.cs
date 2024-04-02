namespace BossMod.Endwalker.Savage.P8S2;

class EgoDeath : BossComponent
{
    public BitMask InEventMask;

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.InEvent)
            InEventMask.Set(module.Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.InEvent)
            InEventMask.Clear(module.Raid.FindSlot(actor.InstanceID));
    }
}
