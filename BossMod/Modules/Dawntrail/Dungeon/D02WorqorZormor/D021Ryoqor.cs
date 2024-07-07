namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D021Ryoqor;

public enum OID : uint
{
    Boss = 0x4159, // R5.280, x1
    Helper = 0x233C, // R0.500, x4, 523 type
    RorrlohTeh = 0x415B, // R1.500, x0 (spawn during fight)
    QorrlohTeh1 = 0x415A, // R3.000, x0 (spawn during fight)
    Snowball = 0x415C, // R2.500, x0 (spawn during fight)
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    QorrlohTeh2 = 0x43A2, // R0.500, x4
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    FrostingFracas1 = 36280, // Helper->self, 5.0s cast, range 60 circle
    FrostingFracas2 = 36279, // Boss->self, 5.0s cast, single-target

    FluffleUp = 36265, // Boss->self, 4.0s cast, single-target
    ColdFeat = 36266, // Boss->self, 4.0s cast, single-target
    IceScream = 36270, // RorrlohTeh->self, 12.0s cast, range 20 width 20 rect

    FrozenSwirl1 = 36272, // QorrlohTeh2->self, 12.0s cast, range 15 circle
    FrozenSwirl2 = 36271, // QorrlohTeh1->self, 12.0s cast, single-target

    Snowscoop = 36275, // Boss->self, 4.0s cast, single-target
    SnowBoulder = 36278, // Snowball->self, 4.0s cast, range 50 width 6 rect

    SparklingSprinkling1 = 36713, // Boss->self, 5.0s cast, single-target
    SparklingSprinkling2 = 36281, // Helper->player, 5.0s cast, range 5 circle
}

public enum SID : uint
{
    UnknownStatus1 = 2056, // none->RorrlohTeh, extra=0x2C4
    UnknownStatus2 = 3944, // none->RorrlohTeh/QorrlohTeh1, extra=0x0
    UnknownStatus3 = 3445, // none->RorrlohTeh/QorrlohTeh2/QorrlohTeh1, extra=0xFFF6
    VulnerabilityUp = 1789, // Snowball/QorrlohTeh2/RorrlohTeh->player, extra=0x1/0x2
}

public enum IconID : uint
{
    GazeMaybe = 376, // player //used for dog gaze in V02
}

public enum TetherID : uint
{
    Tether272 = 272, // RorrlohTeh/QorrlohTeh1->Boss
}

class FrostingFracas1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FrostingFracas1));
class IceScream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IceScream), new AOEShapeRect(20, 10));
class FrozenSwirl1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FrozenSwirl1), new AOEShapeCircle(15));
class SnowBoulder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SnowBoulder), new AOEShapeRect(50, 3));
class SparklingSprinkling2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SparklingSprinkling2), 5);

class D021RyoqorStates : StateMachineBuilder
{
    public D021RyoqorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FrostingFracas1>()
            .ActivateOnEnter<IceScream>()
            .ActivateOnEnter<FrozenSwirl1>()
            .ActivateOnEnter<SnowBoulder>()
            .ActivateOnEnter<SparklingSprinkling2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12699)]
public class D021Ryoqor(WorldState ws, Actor primary) : BossModule(ws, primary, new(-108, 119), new ArenaBoundsCircle(20));
