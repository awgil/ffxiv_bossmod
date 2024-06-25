namespace BossMod.Shadowbringers.Alliance.A22FlightUnits;

class A22FlightUnitsStates : StateMachineBuilder
{
    public A22FlightUnitsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IncendiaryBarrage>()
            .ActivateOnEnter<StandardSurfaceMissile1>()
            .ActivateOnEnter<StandardSurfaceMissile2>()
            .ActivateOnEnter<LethalRevolution>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<IncendiaryBombing>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<PrecisionGuidedMissile>();
    }
}
