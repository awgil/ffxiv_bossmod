namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

class DAL3SaunionDawonStates : StateMachineBuilder
{
    public DAL3SaunionDawonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<FrigidPulseAOE>()
            .ActivateOnEnter<MagitekHalo>()
            .ActivateOnEnter<MagitekCrossray>()
            .ActivateOnEnter<MissileSalvo>()
            .ActivateOnEnter<Touchdown3>()
            .ActivateOnEnter<SwoopingFrenzyAOE>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<RawHeat>()
            .ActivateOnEnter<PentagustAOE>()
            .ActivateOnEnter<ToothAndTalon>()
            .ActivateOnEnter<HighPoweredMagitekRay>();
    }
}
