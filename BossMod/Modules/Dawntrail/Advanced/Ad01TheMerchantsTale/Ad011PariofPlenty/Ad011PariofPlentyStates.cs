namespace BossMod.Modules.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

sealed class PariOfPlentyStates : StateMachineBuilder
{
    public PariOfPlentyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeatBurst>()
            .ActivateOnEnter<FireFlight>()
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<CharmedChains>()
            .ActivateOnEnter<SimpleFableFlight>()
            .ActivateOnEnter<FireOfVictory>()
            .ActivateOnEnter<LeftRightFireflight>()
            .ActivateOnEnter<WheelOfFireflight>()
            .ActivateOnEnter<FellSpark>()
            .ActivateOnEnter<CurseOfCompanionshipSolitude>()
            .ActivateOnEnter<DoubleFableFlight>()
            .ActivateOnEnter<SpurningFlames>()
            .ActivateOnEnter<ImpassionedSpark>()
            .ActivateOnEnter<SparkPuddle>()
            .ActivateOnEnter<BurningPillar>()
            .ActivateOnEnter<FireWell>()
            .ActivateOnEnter<ScouringScorn>()
            .ActivateOnEnter<FireFlightFactOrFiction>()
            .ActivateOnEnter<FalseFlameDisplay>();
    }
}
