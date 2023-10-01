namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary
{
    class DestructiveStatic : Components.SelfTargetedAOEs
    {
        public DestructiveStatic() : base(ActionID.MakeSpell(AID.DestructiveStatic), new AOEShapeCone(50, 90.Degrees())) { }
    }

    class LightningBolt : Components.LocationTargetedAOEs
    {
        public LightningBolt() : base(ActionID.MakeSpell(AID.LightningBoltAOE), 6) { }
    }

    class BoltsFromTheBlue : Components.CastCounter
    {
        public BoltsFromTheBlue() : base(ActionID.MakeSpell(AID.BoltsFromTheBlueAOE)) { }
    }

    class DestructiveStrike : Components.BaitAwayCast
    {
        public DestructiveStrike() : base(ActionID.MakeSpell(AID.DestructiveStrike), new AOEShapeCone(13, 60.Degrees())) { } // TODO: verify angle
    }

    public class A10RhalgrEmissary : BossModule
    {
        public A10RhalgrEmissary(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(74, 516), 25)) { }
    }
}
