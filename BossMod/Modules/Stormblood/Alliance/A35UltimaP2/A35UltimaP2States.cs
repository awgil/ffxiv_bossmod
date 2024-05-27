namespace BossMod.Stormblood.Alliance.A35UltimaP2;

class A35UltimaP2States : StateMachineBuilder
{
    public A35UltimaP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HolyIVBait>()
            .ActivateOnEnter<HolyIVSpread>()
            .ActivateOnEnter<Redemption>()
            .ActivateOnEnter<Auralight1>()
            .ActivateOnEnter<Auralight2>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<Embrace2>()
            .ActivateOnEnter<GrandCrossAOE>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<HolyIVBait>()
            .ActivateOnEnter<HolyIVSpread>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<Cataclysm>();
    }
}
