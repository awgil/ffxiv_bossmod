namespace BossMod.Shadowbringers.Alliance.A23ArtilleryUnit;

class A23ArtilleryUnitStates : StateMachineBuilder
{
    public A23ArtilleryUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ManeuverVoltArray>()
            .ActivateOnEnter<LowerLaser1>()
            //.ActivateOnEnter<UpperLaser1>()
            //.ActivateOnEnter<UpperLaser2>()
            //.ActivateOnEnter<UpperLaser3>()
            .ActivateOnEnter<UpperLaser>()
            .ActivateOnEnter<EnergyBombardment2>()
            .ActivateOnEnter<UnknownWeaponskill>()
            .ActivateOnEnter<ManeuverRevolvingLaser>()
            .ActivateOnEnter<R010Laser>()
            .ActivateOnEnter<R030Hammer>();
    }
}
