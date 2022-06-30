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

    class FlamesOfTheDead : CommonComponents.SelfTargetedAOE
    {
        public FlamesOfTheDead() : base(ActionID.MakeSpell(AID.FlamesOfTheDeadReal), new AOEShapeDonut(8, 30)) { }
    }

    class LivingHeat : CommonComponents.SelfTargetedAOE
    {
        public LivingHeat() : base(ActionID.MakeSpell(AID.LivingHeatReal), new AOEShapeCircle(8)) { }
    }

    class HellOfFireFront : CommonComponents.SelfTargetedAOE
    {
        public HellOfFireFront() : base(ActionID.MakeSpell(AID.HellOfFireFrontAOE), new AOEShapeCone(60, 90.Degrees())) { }
    }

    class HellOfFireBack : CommonComponents.SelfTargetedAOE
    {
        public HellOfFireBack() : base(ActionID.MakeSpell(AID.HellOfFireBackAOE), new AOEShapeCone(60, 90.Degrees())) { }
    }

    class WaywardSoul : CommonComponents.SelfTargetedAOE
    {
        public WaywardSoul() : base(ActionID.MakeSpell(AID.WaywardSoulAOE), new AOEShapeCircle(18), 3) { }
    }

    class Twingaze : CommonComponents.SelfTargetedAOE
    {
        public Twingaze() : base(ActionID.MakeSpell(AID.Twingaze), new AOEShapeCone(60, 15.Degrees())) { }
    }

    // TODO: balancing counter, magmatic spell raid stack
    public class A4Naldthal : BossModule
    {
        public A4Naldthal(WorldState ws, Actor primary)
            : base(ws, primary, new ArenaBoundsCircle(new(750, -750), 25))
        {
            var sb = new StateMachineBuilder(this);
            sb.TrivialPhase()
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
            StateMachine = sb.Build();
            //StateMachine = new A4NaldthalStates(this).Initial;
        }
    }
}
