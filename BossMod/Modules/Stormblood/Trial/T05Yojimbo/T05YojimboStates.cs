namespace BossMod.Stormblood.Trial.T05Yojimbo;

class T05YojimboStates : StateMachineBuilder
{
    public T05YojimboStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
