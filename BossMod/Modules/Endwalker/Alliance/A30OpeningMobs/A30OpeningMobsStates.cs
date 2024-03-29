namespace BossMod.Endwalker.Alliance.A30OpeningMobs
{
    public class A30OpeningMobsStates : StateMachineBuilder
    {
        public A30OpeningMobsStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<WaterIII>()
                .ActivateOnEnter<PelagicCleaver1>()
                .ActivateOnEnter<PelagicCleaver2>()
                .ActivateOnEnter<WaterFlood>()
                .ActivateOnEnter<DivineFlood>();
        }
    }
}
