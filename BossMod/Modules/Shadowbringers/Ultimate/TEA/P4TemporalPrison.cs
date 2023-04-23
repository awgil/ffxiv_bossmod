namespace BossMod.Shadowbringers.Ultimate.TEA
{
    // TODO: show prison spots, warn if not taken...
    class P4TemporalPrison : BossComponent
    {
        public int NumPrisons { get; private set; }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.TemporalPrison)
                ++NumPrisons;
        }
    }
}
