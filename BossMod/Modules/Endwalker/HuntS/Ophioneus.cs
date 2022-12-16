namespace BossMod.Endwalker.HuntS.Ophioneus
{
    public enum OID : uint
    {
        Boss = 0x35DC, // R5.875, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        RightMaw = 27350, // Boss->self, 5.0s cast, range 30 180-degree cone
        LeftMaw = 27351, // Boss->self, 5.0s cast, range 30 180-degree cone
        PyricCircle = 27352, // Boss->self, 5.0s cast, range 5-40 donut
        PyricBurst = 27353, // Boss->self, 5.0s cast, range 40 circle with ? falloff
        LeapingPyricCircle = 27341, // Boss->location, 6.0s cast, width 0 rect charge, visual
        LeapingPyricBurst = 27342, // Boss->location, 6.0s cast, width 0 rect charge, visual
        LeapingPyricCircleAOE = 27346, // Boss->self, 1.0s cast, range 5-40 donut
        LeapingPyricBurstAOE = 27347, // Boss->self, 1.0s cast, range 40 circle with ? falloff
        Scratch = 27348, // Boss->player, 5.0s cast, single-target
    };

    class RightMaw : Components.SelfTargetedAOEs
    {
        public RightMaw() : base(ActionID.MakeSpell(AID.RightMaw), new AOEShapeCone(30, 90.Degrees())) { }
    }

    class LeftMaw : Components.SelfTargetedAOEs
    {
        public LeftMaw() : base(ActionID.MakeSpell(AID.LeftMaw), new AOEShapeCone(30, 90.Degrees())) { }
    }

    class PyricCircle : Components.SelfTargetedAOEs
    {
        public PyricCircle() : base(ActionID.MakeSpell(AID.PyricCircle), new AOEShapeDonut(5, 40)) { }
    }

    class PyricBurst : Components.SelfTargetedAOEs
    {
        public PyricBurst() : base(ActionID.MakeSpell(AID.PyricBurst), new AOEShapeCircle(10)) { } // TODO: verify falloff
    }

    // TODO: consider showing early hint during teleport cast
    class LeapingPyricCircle : Components.SelfTargetedAOEs
    {
        public LeapingPyricCircle() : base(ActionID.MakeSpell(AID.LeapingPyricCircleAOE), new AOEShapeDonut(5, 40)) { }
    }

    // TODO: consider showing early hint during teleport cast
    class LeapingPyricBurst : Components.SelfTargetedAOEs
    {
        public LeapingPyricBurst() : base(ActionID.MakeSpell(AID.LeapingPyricBurstAOE), new AOEShapeCircle(10)) { } // TODO: verify falloff
    }

    class Scratch : Components.SingleTargetCast
    {
        public Scratch() : base(ActionID.MakeSpell(AID.Scratch)) { }
    }

    class OphioneusStates : StateMachineBuilder
    {
        public OphioneusStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<RightMaw>()
                .ActivateOnEnter<LeftMaw>()
                .ActivateOnEnter<PyricCircle>()
                .ActivateOnEnter<PyricBurst>()
                .ActivateOnEnter<LeapingPyricCircle>()
                .ActivateOnEnter<LeapingPyricBurst>()
                .ActivateOnEnter<Scratch>();
        }
    }

    public class Ophioneus : SimpleBossModule
    {
        public Ophioneus(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
