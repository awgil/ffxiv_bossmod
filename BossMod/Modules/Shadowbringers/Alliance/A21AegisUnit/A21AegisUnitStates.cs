namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

class A21AegisUnitStates : StateMachineBuilder
{
    public A21AegisUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlightPath>()
            .ActivateOnEnter<AntiPersonnelLaser>()
            .ActivateOnEnter<HighPoweredLaser>()
            .ActivateOnEnter<LifesLastSong>();
    }
}
