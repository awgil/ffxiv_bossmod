namespace BossMod.Endwalker.Extreme.Ex6Golbez
{
    class Terrastorm : Components.SelfTargetedAOEs
    {
        public Terrastorm() : base(ActionID.MakeSpell(AID.TerrastormAOE), new AOEShapeCircle(16)) { }
    }

    class LingeringSpark : Components.LocationTargetedAOEs
    {
        public LingeringSpark() : base(ActionID.MakeSpell(AID.LingeringSparkAOE), 5) { }
    }

    class PhasesOfTheBladeFront : Components.SelfTargetedAOEs
    {
        public PhasesOfTheBladeFront() : base(ActionID.MakeSpell(AID.PhasesOfTheBlade), new AOEShapeCone(22, 90.Degrees())) { }
    }

    class PhasesOfTheBladeBack : Components.SelfTargetedAOEs
    {
        public PhasesOfTheBladeBack() : base(ActionID.MakeSpell(AID.PhasesOfTheBladeBack), new AOEShapeCone(22, 90.Degrees())) { }
    }

    class PhasesOfTheShadowFront : Components.SelfTargetedAOEs
    {
        public PhasesOfTheShadowFront() : base(ActionID.MakeSpell(AID.PhasesOfTheShadow), new AOEShapeCone(22, 90.Degrees())) { }
    }

    class PhasesOfTheShadowBack : Components.SelfTargetedAOEs
    {
        public PhasesOfTheShadowBack() : base(ActionID.MakeSpell(AID.PhasesOfTheShadowBack), new AOEShapeCone(22, 90.Degrees())) { }
    }

    class ArcticAssault : Components.SelfTargetedAOEs
    {
        public ArcticAssault() : base(ActionID.MakeSpell(AID.ArcticAssaultAOE), new AOEShapeRect(15, 7.5f)) { }
    }

    class RisingBeacon : Components.SelfTargetedAOEs
    {
        public RisingBeacon() : base(ActionID.MakeSpell(AID.RisingBeaconAOE), new AOEShapeCircle(10)) { }
    }

    class RisingRing : Components.SelfTargetedAOEs
    {
        public RisingRing() : base(ActionID.MakeSpell(AID.RisingRingAOE), new AOEShapeDonut(6, 22)) { }
    }

    class BurningShade : Components.SpreadFromCastTargets
    {
        public BurningShade() : base(ActionID.MakeSpell(AID.BurningShade), 5) { }
    }

    class ImmolatingShade : Components.StackWithCastTargets
    {
        public ImmolatingShade() : base(ActionID.MakeSpell(AID.ImmolatingShade), 6, 4) { }
    }

    class VoidBlizzard : Components.StackWithCastTargets
    {
        public VoidBlizzard() : base(ActionID.MakeSpell(AID.VoidBlizzard), 6, 4) { }
    }

    class VoidAero : Components.StackWithCastTargets
    {
        public VoidAero() : base(ActionID.MakeSpell(AID.VoidAero), 3, 2) { }
    }

    class VoidTornado : Components.StackWithCastTargets
    {
        public VoidTornado() : base(ActionID.MakeSpell(AID.VoidTornado), 6, 4) { }
    }

    [ConfigDisplay(Order = 0x060, Parent = typeof(EndwalkerConfig))]
    public class Ex6GolbezConfig : CooldownPlanningConfigNode
    {
        public Ex6GolbezConfig() : base(90) { }
    }

    public class Ex6Golbez : BossModule
    {
        public Ex6Golbez(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 15)) { }
    }
}
