namespace BossMod.Endwalker.HuntA.LunatenderQueen
{
    public enum OID : uint
    {
        Boss = 0x35DF, // R5.320, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        AvertYourEyes = 27363, // Boss->self, 7.0s cast, range 40 circle
        YouMayApproach = 27364, // Boss->self, 7.0s cast, range 6-40 donut
        AwayWithYou = 27365, // Boss->self, 7.0s cast, range 15 circle
        Needles = 27366, // Boss->self, 3.0s cast, range 6 circle
        WickedWhim = 27367, // Boss->self, 4.0s cast, single-target
        AvertYourEyesInverted = 27369, // Boss->self, 7.0s cast, range 40 circle
        YouMayApproachInverted = 27370, // Boss->self, 7.0s cast, range 15 circle
        AwayWithYouInverted = 27371, // Boss->self, 7.0s cast, range 6-40 donut
    };

    class AvertYourEyes : Components.CastGaze
    {
        public AvertYourEyes() : base(ActionID.MakeSpell(AID.AvertYourEyes)) { }
    }

    class YouMayApproach : Components.SelfTargetedAOEs
    {
        public YouMayApproach() : base(ActionID.MakeSpell(AID.YouMayApproach), new AOEShapeDonut(6, 40)) { }
    }

    class AwayWithYou : Components.SelfTargetedAOEs
    {
        public AwayWithYou() : base(ActionID.MakeSpell(AID.AwayWithYou), new AOEShapeCircle(15)) { }
    }

    class Needles : Components.SelfTargetedAOEs
    {
        public Needles() : base(ActionID.MakeSpell(AID.Needles), new AOEShapeCircle(6)) { }
    }

    class WickedWhim : Components.CastHint
    {
        public WickedWhim() : base(ActionID.MakeSpell(AID.WickedWhim), "Invert next cast") { }
    }

    class AvertYourEyesInverted : Components.CastGaze
    {
        public AvertYourEyesInverted() : base(ActionID.MakeSpell(AID.AvertYourEyesInverted), true) { }
    }

    class YouMayApproachInverted : Components.SelfTargetedAOEs
    {
        public YouMayApproachInverted() : base(ActionID.MakeSpell(AID.YouMayApproachInverted), new AOEShapeCircle(15)) { }
    }

    class AwayWithYouInverted : Components.SelfTargetedAOEs
    {
        public AwayWithYouInverted() : base(ActionID.MakeSpell(AID.AwayWithYouInverted), new AOEShapeDonut(6, 40)) { }
    }

    class LunatenderQueenStates : StateMachineBuilder
    {
        public LunatenderQueenStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<AvertYourEyes>()
                .ActivateOnEnter<YouMayApproach>()
                .ActivateOnEnter<AwayWithYou>()
                .ActivateOnEnter<Needles>()
                .ActivateOnEnter<WickedWhim>()
                .ActivateOnEnter<AvertYourEyesInverted>()
                .ActivateOnEnter<YouMayApproachInverted>()
                .ActivateOnEnter<AwayWithYouInverted>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 205)]
    public class LunatenderQueen : SimpleBossModule
    {
        public LunatenderQueen(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
