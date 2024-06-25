namespace BossMod.Heavensward.Alliance.A13Cuchulainn;

class A13CuchulainnStates : StateMachineBuilder
{
    public A13CuchulainnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CorrosiveBile1>()
            .ActivateOnEnter<FlailingTentacles2>()
            //.ActivateOnEnter<FlailingTentacles2Knockback>()
            .ActivateOnEnter<Beckon>()
            .ActivateOnEnter<BileBelow>()
            .ActivateOnEnter<Pestilence>()
            .ActivateOnEnter<BlackLung>()
            .ActivateOnEnter<GrandCorruption>();
    }
}
