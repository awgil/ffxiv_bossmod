namespace BossMod.Endwalker.Hunt.RankA.LunatenderQueen;

public enum OID : uint
{
    Boss = 0x35DF, // R5.320, x1
}

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
}

class AvertYourEyes(BossModule module) : Components.CastGaze(module, AID.AvertYourEyes);
class YouMayApproach(BossModule module) : Components.StandardAOEs(module, AID.YouMayApproach, new AOEShapeDonut(6, 40));
class AwayWithYou(BossModule module) : Components.StandardAOEs(module, AID.AwayWithYou, new AOEShapeCircle(15));
class Needles(BossModule module) : Components.StandardAOEs(module, AID.Needles, new AOEShapeCircle(6));
class WickedWhim(BossModule module) : Components.CastHint(module, AID.WickedWhim, "Invert next cast");
class AvertYourEyesInverted(BossModule module) : Components.CastGaze(module, AID.AvertYourEyesInverted, true);
class YouMayApproachInverted(BossModule module) : Components.StandardAOEs(module, AID.YouMayApproachInverted, new AOEShapeCircle(15));
class AwayWithYouInverted(BossModule module) : Components.StandardAOEs(module, AID.AwayWithYouInverted, new AOEShapeDonut(6, 40));

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

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10629)]
public class LunatenderQueen(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
