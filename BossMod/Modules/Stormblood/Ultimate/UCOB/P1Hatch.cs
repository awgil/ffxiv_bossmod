namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P1Hatch : Hatch
    {
        public int NumNeurolinkSpawns { get; private set; }

        public P1Hatch() { KeepOnPhaseChange = true; }

        public override void Update(BossModule module)
        {
            Active = module.PrimaryActor.IsTargetable; // don't care about standing in neurolinks when twintania flies away
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.Twintania && id == 0x94)
                ++NumNeurolinkSpawns;
        }
    }
}
