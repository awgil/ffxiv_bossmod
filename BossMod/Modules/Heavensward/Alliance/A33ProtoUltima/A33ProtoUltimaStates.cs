namespace BossMod.Heavensward.Alliance.A33ProtoUltima;

class A33ProtoUltimaStates : StateMachineBuilder
{
    public A33ProtoUltimaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WreckingBall>()
            .ActivateOnEnter<AetherochemicalFlare>()
            .ActivateOnEnter<AetherochemicalLaser1>()
            .ActivateOnEnter<AetherochemicalLaser2>()
            .ActivateOnEnter<AetherochemicalLaser3>()
            .ActivateOnEnter<CitadelBuster2>()
            .ActivateOnEnter<FlareStar>()
            .ActivateOnEnter<Rotoswipe>();
    }
}
