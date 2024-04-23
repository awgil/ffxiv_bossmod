namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1Throttle(BossModule module) : BossComponent(module)
{
    public bool Applied { get; private set; }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Throttle)
            Applied = true;
    }
}
