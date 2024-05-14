namespace BossMod.Stormblood.Trial.T04Tsukuyomi;

class T04TsukuyomiStates : StateMachineBuilder
{
    public T04TsukuyomiStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
