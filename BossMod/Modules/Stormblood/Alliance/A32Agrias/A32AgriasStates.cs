namespace BossMod.Stormblood.Alliance.A32Agrias;

class A32AgriasStates : StateMachineBuilder
{
    public A32AgriasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DivineLight>()
            .ActivateOnEnter<NorthswainsStrikeEphemeralKnight>()
            .ActivateOnEnter<CleansingFlameSpread>()
            .ActivateOnEnter<HallowedBoltAOE>();
    }
}
