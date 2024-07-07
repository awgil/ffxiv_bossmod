namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D022Kahderyor;

public enum OID : uint
{
    Boss = 0x415D, // R7.000, x1
    Helper = 0x233C, // R0.500, x20, 523 type
    CrystallineDebris = 0x415E, // R1.400, x0 (spawn during fight)
    UnknownActor = 0x412E, // R3.960, x1
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2 (spawn during fight), EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WindUnbound = 36282, // Boss->self, 5.0s cast, range 60 circle

    CrystallineCrush1 = 36285, // Boss->location, 5.0+1.0s cast, single-target
    CrystallineCrush2 = 36153, // Helper->self, 6.3s cast, range 6 circle

    WindShot1 = 36284, // Boss->self, 5.5s cast, single-target
    WindShot2 = 36296, // Helper->players, 6.0s cast, range ?-10 donut
    Windshot3 = 36300, // Helper->player, no cast, single-target

    EarthenShot1 = 36283, // Boss->self, 5.0+0.5s cast, single-target
    EarthenShot2 = 36295, // Helper->player, 6.0s cast, range 6 circle
    EarthenShot3 = 36299, // Helper->player, no cast, single-target

    CrystallineStorm1 = 36286, // Boss->self, 3.0+1.0s cast, single-target
    CrystallineStorm2 = 36290, // Helper->self, 4.0s cast, range 50 width 2 rect

    SeedCrystals1 = 36291, // Boss->self, 4.5+0.5s cast, single-target
    SeedCrystals2 = 36298, // Helper->player, 5.0s cast, range 6 circle

    SharpenedSights = 36287, // Boss->self, 3.0s cast, single-target
    EyeOfTheFierce = 36297, // Helper->self, 5.0s cast, range 60 circle

    StalagmiteCircle1 = 36293, // Helper->self, 5.0s cast, range 15 circle
    StalagmiteCircle2 = 36288, // Boss->self, 5.0s cast, single-target

    CyclonicRing1 = 36294, // Helper->self, 5.0s cast, range ?-40 donut
    CyclonicRing2 = 36289, // Boss->self, 5.0s cast, single-target
}

public enum SID : uint
{
    SeedCrystals = 3809, // Helper->player, extra=0x0
    SharpenedSights = 3801, // Boss->Boss, extra=0x2/0x1
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2/0x3
    CrystalBurden = 3810, // none->player, extra=0x0
    Confused = 11, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Stackmarker = 62, // Helper
    Icon511 = 511, // player
    Icon169 = 169, // player
    Icon311 = 311, // player
}

class WindUnbound(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WindUnbound));
class CrystallineCrush1(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.CrystallineCrush1), 6);
class CrystallineCrush2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.CrystallineCrush2), 6, 4);
class EarthenShot2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EarthenShot2), 6);
class StalagmiteCircle1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StalagmiteCircle1), new AOEShapeCircle(15));
class CyclonicRing1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CyclonicRing1), new AOEShapeDonut(6, 40));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFierce));
class SeedCrystals2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SeedCrystals2), 6);

class D022KahderyorStates : StateMachineBuilder
{
    public D022KahderyorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<CrystallineCrush1>()
            .ActivateOnEnter<CrystallineCrush2>()
            .ActivateOnEnter<EarthenShot2>()
            .ActivateOnEnter<StalagmiteCircle1>()
            .ActivateOnEnter<CyclonicRing1>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<SeedCrystals2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12703)]
public class D022Kahderyor(WorldState ws, Actor primary) : BossModule(ws, primary, new(-53, -57), new ArenaBoundsCircle(20));
