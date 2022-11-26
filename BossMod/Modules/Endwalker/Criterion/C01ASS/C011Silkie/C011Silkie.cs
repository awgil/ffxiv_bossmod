namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie
{
    class FizzlingDuster : Components.SelfTargetedAOEs
    {
        public FizzlingDuster() : base(ActionID.MakeSpell(AID.FizzlingDusterAOE), C011Silkie.ShapeYellow) { }
    }

    class DustBluster : Components.KnockbackFromCastTarget
    {
        public DustBluster() : base(ActionID.MakeSpell(AID.DustBluster), 16) { }
    }

    class SqueakyCleanE : Components.SelfTargetedAOEs
    {
        public SqueakyCleanE() : base(ActionID.MakeSpell(AID.SqueakyCleanAOE3E), new AOEShapeCone(60, 112.5f.Degrees())) { }
    }

    class SqueakyCleanW : Components.SelfTargetedAOEs
    {
        public SqueakyCleanW() : base(ActionID.MakeSpell(AID.SqueakyCleanAOE3W), new AOEShapeCone(60, 112.5f.Degrees())) { }
    }

    class ChillingDusterPuff : Components.SelfTargetedAOEs
    {
        public ChillingDusterPuff() : base(ActionID.MakeSpell(AID.ChillingDusterPuff), C011Silkie.ShapeBlue) { }
    }

    class BracingDusterPuff : Components.SelfTargetedAOEs
    {
        public BracingDusterPuff() : base(ActionID.MakeSpell(AID.BracingDusterPuff), C011Silkie.ShapeGreen) { }
    }

    class FizzlingDusterPuff : Components.SelfTargetedAOEs
    {
        public FizzlingDusterPuff() : base(ActionID.MakeSpell(AID.FizzlingDusterPuff), C011Silkie.ShapeYellow) { }
    }

    public class C011Silkie : BossModule
    {
        public static AOEShapeCross ShapeBlue = new(60, 5);
        public static AOEShapeDonut ShapeGreen = new(5, 60);
        public static AOEShapeCone ShapeYellow = new(60, 22.5f.Degrees());

        public C011Silkie(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-335, -155), 20)) { }
    }
}
