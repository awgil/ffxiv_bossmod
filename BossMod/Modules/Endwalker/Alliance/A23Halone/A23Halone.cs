namespace BossMod.Endwalker.Alliance.A23Halone
{
    class RainOfSpearsFirst : Components.CastCounter
    {
        public RainOfSpearsFirst() : base(ActionID.MakeSpell(AID.RainOfSpearsFirst)) { }
    }

    class RainOfSpearsRest : Components.CastCounter
    {
        public RainOfSpearsRest() : base(ActionID.MakeSpell(AID.RainOfSpearsRest)) { }
    }

    class SpearsThree : Components.BaitAwayCast
    {
        public SpearsThree() : base(ActionID.MakeSpell(AID.SpearsThreeAOE), new AOEShapeCircle(5), true) { }
    }

    class WrathOfHalone : Components.SelfTargetedAOEs
    {
        public WrathOfHalone() : base(ActionID.MakeSpell(AID.WrathOfHaloneAOE), new AOEShapeCircle(25)) { } // TODO: verify falloff
    }

    class GlacialSpearSmall : Components.Adds
    {
        public GlacialSpearSmall() : base((uint)OID.GlacialSpearSmall) { }
    }

    class GlacialSpearLarge : Components.Adds
    {
        public GlacialSpearLarge() : base((uint)OID.GlacialSpearLarge) { }
    }

    class IceDart : Components.SpreadFromCastTargets
    {
        public IceDart() : base(ActionID.MakeSpell(AID.IceDart), 6) { }
    }

    class IceRondel : Components.StackWithCastTargets
    {
        public IceRondel() : base(ActionID.MakeSpell(AID.IceRondel), 6) { }
    }

    class Niphas : Components.SelfTargetedAOEs
    {
        public Niphas() : base(ActionID.MakeSpell(AID.Niphas), new AOEShapeCircle(9)) { }
    }

    public class A23Halone : BossModule
    {
        public A23Halone(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-700, 600), 30)) { }
    }
}
