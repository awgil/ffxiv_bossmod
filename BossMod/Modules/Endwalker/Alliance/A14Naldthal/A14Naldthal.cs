namespace BossMod.Endwalker.Alliance.A14Naldthal
{
    class GoldenTenet : Components.CastSharedTankbuster
    {
        public GoldenTenet() : base(ActionID.MakeSpell(AID.GoldenTenetAOE), 6) { }
    }

    class StygianTenet : Components.SpreadFromCastTargets
    {
        public StygianTenet() : base(ActionID.MakeSpell(AID.StygianTenetAOE), 6) { }
    }

    class FlamesOfTheDead : Components.SelfTargetedAOEs
    {
        public FlamesOfTheDead() : base(ActionID.MakeSpell(AID.FlamesOfTheDeadReal), new AOEShapeDonut(8, 30)) { }
    }

    class LivingHeat : Components.SelfTargetedAOEs
    {
        public LivingHeat() : base(ActionID.MakeSpell(AID.LivingHeatReal), new AOEShapeCircle(8)) { }
    }

    class HellOfFireFront : Components.SelfTargetedLegacyRotationAOEs
    {
        public HellOfFireFront() : base(ActionID.MakeSpell(AID.HellOfFireFrontAOE), new AOEShapeCone(60, 90.Degrees())) { }
    }

    class HellOfFireBack : Components.SelfTargetedLegacyRotationAOEs
    {
        public HellOfFireBack() : base(ActionID.MakeSpell(AID.HellOfFireBackAOE), new AOEShapeCone(60, 90.Degrees())) { }
    }

    class WaywardSoul : Components.SelfTargetedAOEs
    {
        public WaywardSoul() : base(ActionID.MakeSpell(AID.WaywardSoulAOE), new AOEShapeCircle(18), 3) { }
    }

    class Twingaze : Components.SelfTargetedLegacyRotationAOEs
    {
        public Twingaze() : base(ActionID.MakeSpell(AID.Twingaze), new AOEShapeCone(60, 15.Degrees())) { }
    }

    public class A14NaldthalStates : StateMachineBuilder
    {
        public A14NaldthalStates(BossModule module) : base(module)
        {
            // TODO: reconsider
            TrivialPhase()
                .ActivateOnEnter<GoldenTenet>()
                .ActivateOnEnter<StygianTenet>()
                .ActivateOnEnter<FlamesOfTheDead>()
                .ActivateOnEnter<LivingHeat>()
                .ActivateOnEnter<HeavensTrial>()
                .ActivateOnEnter<DeepestPit>()
                .ActivateOnEnter<OnceAboveEverBelow>()
                .ActivateOnEnter<HellOfFireFront>()
                .ActivateOnEnter<HellOfFireBack>()
                .ActivateOnEnter<WaywardSoul>()
                .ActivateOnEnter<FortuneFlux>()
                .ActivateOnEnter<Twingaze>();
        }
    }

    // TODO: balancing counter, magmatic spell raid stack
    public class A14Naldthal : BossModule
    {
        public A14Naldthal(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(750, -750), 25)) { }
    }
}
