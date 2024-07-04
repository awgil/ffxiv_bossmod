namespace BossMod.Heavensward.Alliance.A14Echidna;

class A14EchidnaStates : StateMachineBuilder
{
    public A14EchidnaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SickleStrike>()
            .ActivateOnEnter<SickleSlash2>()
            .ActivateOnEnter<SickleSlash3>()
            .ActivateOnEnter<AbyssalReaper>()
            .ActivateOnEnter<AbyssalReaperKnockback>()
            .ActivateOnEnter<Petrifaction1>()
            .ActivateOnEnter<Petrifaction2>()
            .ActivateOnEnter<Gehenna>()
            .ActivateOnEnter<BloodyHarvest>()
            .ActivateOnEnter<Deathstrike>()
            .ActivateOnEnter<FlameWreath>()
            .ActivateOnEnter<SerpentineStrike>();
    }
}
