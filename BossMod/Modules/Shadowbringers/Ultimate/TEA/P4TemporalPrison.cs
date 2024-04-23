namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: show prison spots, warn if not taken...
class P4TemporalPrison(BossModule module) : BossComponent(module)
{
    public int NumPrisons { get; private set; }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.TemporalPrison)
            ++NumPrisons;
    }
}
