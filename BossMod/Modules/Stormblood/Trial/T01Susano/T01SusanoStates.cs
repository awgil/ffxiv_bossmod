namespace BossMod.Stormblood.Trial.T01Susano;

class T01SusanoStates : StateMachineBuilder
{
    public T01SusanoStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
