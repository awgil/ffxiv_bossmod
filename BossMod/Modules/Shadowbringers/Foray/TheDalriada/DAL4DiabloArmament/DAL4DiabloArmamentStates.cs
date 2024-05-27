namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

class DAL4DiabloArmamentStates : StateMachineBuilder
{
    public DAL4DiabloArmamentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AdvancedDeathIVAOE>()
            //.ActivateOnEnter<AdvancedNox>() not displaying properly
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<DeadlyDealingAOE>()
            .ActivateOnEnter<Explosion1>() // explosions need to be staggered
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<Explosion3>()
            .ActivateOnEnter<Explosion4>()
            .ActivateOnEnter<Explosion5>()
            .ActivateOnEnter<Explosion6>()
            .ActivateOnEnter<Explosion7>()
            .ActivateOnEnter<Explosion8>()
            .ActivateOnEnter<Explosion9>()
            .ActivateOnEnter<LightPseudopillarAOE>()
            .ActivateOnEnter<PillarOfShamash1>() // need to be staggered
            .ActivateOnEnter<PillarOfShamash2>()
            .ActivateOnEnter<PillarOfShamash3>()
            .ActivateOnEnter<UltimatePseudoterror>()
            .ActivateOnEnter<AccelerationBomb>();
    }
}
