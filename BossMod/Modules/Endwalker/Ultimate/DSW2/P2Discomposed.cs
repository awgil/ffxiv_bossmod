namespace BossMod.Endwalker.Ultimate.DSW2;

class P2Discomposed : BossComponent
{
    public bool Applied { get; private set; }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Discomposed)
            Applied = true;
    }
}
