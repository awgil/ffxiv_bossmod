namespace BossMod.Endwalker.Alliance.A13Azeyma
{
    class WardensWarmth : Components.SpreadFromCastTargets
    {
        public WardensWarmth() : base(ActionID.MakeSpell(AID.WardensWarmthAOE), 6) { }
    }

    class SolarWingsL : Components.SelfTargetedLegacyRotationAOEs
    {
        public SolarWingsL() : base(ActionID.MakeSpell(AID.SolarWingsL), new AOEShapeCone(30, 67.5f.Degrees(), 90.Degrees())) { }
    }

    class SolarWingsR : Components.SelfTargetedLegacyRotationAOEs
    {
        public SolarWingsR() : base(ActionID.MakeSpell(AID.SolarWingsR), new AOEShapeCone(30, 67.5f.Degrees(), -90.Degrees())) { }
    }

    class FleetingSpark : Components.SelfTargetedLegacyRotationAOEs
    {
        public FleetingSpark() : base(ActionID.MakeSpell(AID.FleetingSpark), new AOEShapeCone(60, 135.Degrees())) { }
    }

    class SolarFold : Components.SelfTargetedLegacyRotationAOEs
    {
        public SolarFold() : base(ActionID.MakeSpell(AID.SolarFoldAOE), new AOEShapeCross(30, 5)) { }
    }

    class Sunbeam : Components.SelfTargetedAOEs
    {
        public Sunbeam() : base(ActionID.MakeSpell(AID.Sunbeam), new AOEShapeCircle(9), 14) { }
    }

    class SublimeSunset : Components.LocationTargetedAOEs
    {
        public SublimeSunset() : base(ActionID.MakeSpell(AID.SublimeSunsetAOE), 40) { } // TODO: check falloff
    }

    public class A13AzeymaStates : StateMachineBuilder
    {
        public A13AzeymaStates(BossModule module) : base(module)
        {
            // TODO: reconsider
            TrivialPhase()
                .ActivateOnEnter<WardensWarmth>()
                .ActivateOnEnter<SolarWingsL>()
                .ActivateOnEnter<SolarWingsR>()
                .ActivateOnEnter<SolarFlair>()
                .ActivateOnEnter<SolarFans>()
                .ActivateOnEnter<FleetingSpark>()
                .ActivateOnEnter<SolarFold>()
                .ActivateOnEnter<DancingFlame>()
                .ActivateOnEnter<WildfireWard>()
                .ActivateOnEnter<Sunbeam>()
                .ActivateOnEnter<SublimeSunset>();
        }
    }

    // TODO: FarFlungFire mechanic - sometimes (on first cast?) we get visual & stack marker, but no aoe...
    public class A13Azeyma : BossModule
    {
        public A13Azeyma(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-750, -750), 30)) { }
    }
}
