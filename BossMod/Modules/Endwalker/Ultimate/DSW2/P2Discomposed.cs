namespace BossMod.Endwalker.Ultimate.DSW2;

class P2Discomposed(BossModule module) : BossComponent(module)
{
    public bool Applied { get; private set; }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Discomposed)
            Applied = true;
    }
}
