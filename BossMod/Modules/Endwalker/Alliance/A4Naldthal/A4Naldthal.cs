namespace BossMod.Endwalker.Alliance.A4Naldthal
{
    class GoldenTenet : CommonComponents.SharedTankbuster
    {
        public GoldenTenet() : base(ActionID.MakeSpell(AID.GoldenTenetAOE), 6) { }
    }

    class StygianTenet : CommonComponents.SpreadFromCastTargets
    {
        public StygianTenet() : base(ActionID.MakeSpell(AID.StygianTenetAOE), 6) { }
    }

    class FlamesOfTheDead : Components.SelfTargetedAOEs
    {
        public FlamesOfTheDead() : base(ActionID.MakeSpell(AID.FlamesOfTheDeadReal), new AOEShapeDonut(8, 30), true) { }
    }

    class LivingHeat : Components.SelfTargetedAOEs
    {
        public LivingHeat() : base(ActionID.MakeSpell(AID.LivingHeatReal), new AOEShapeCircle(8), true) { }
    }

    class HellOfFireFront : Components.SelfTargetedAOEs
    {
        public HellOfFireFront() : base(ActionID.MakeSpell(AID.HellOfFireFrontAOE), new AOEShapeCone(60, 90.Degrees()), true) { }
    }

    class HellOfFireBack : Components.SelfTargetedAOEs
    {
        public HellOfFireBack() : base(ActionID.MakeSpell(AID.HellOfFireBackAOE), new AOEShapeCone(60, 90.Degrees()), true) { }
    }

    class WaywardSoul : Components.SelfTargetedAOEs
    {
        public WaywardSoul() : base(ActionID.MakeSpell(AID.WaywardSoulAOE), new AOEShapeCircle(18), true, 3) { }
    }

    class Twingaze : Components.SelfTargetedAOEs
    {
        public Twingaze() : base(ActionID.MakeSpell(AID.Twingaze), new AOEShapeCone(60, 15.Degrees()), true) { }
    }

    public class A4NaldthalStates : StateMachineBuilder
    {
        public A4NaldthalStates(BossModule module) : base(module)
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
    public class A4Naldthal : BossModule
    {
        public A4Naldthal(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(750, -750), 25)) { }
    }
}
