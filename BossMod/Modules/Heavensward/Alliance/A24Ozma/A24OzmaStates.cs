namespace BossMod.Heavensward.Alliance.A24Ozma;

class A24OzmaStates : StateMachineBuilder
{
    public A24OzmaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<HolyKB>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<ExecrationAOE>()
            .ActivateOnEnter<AccelerationBomb>();
    }
}
