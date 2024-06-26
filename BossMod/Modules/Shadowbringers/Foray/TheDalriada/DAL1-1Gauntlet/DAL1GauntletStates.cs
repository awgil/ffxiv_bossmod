namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

class DAL1GauntletStates : StateMachineBuilder
{
    public DAL1GauntletStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
