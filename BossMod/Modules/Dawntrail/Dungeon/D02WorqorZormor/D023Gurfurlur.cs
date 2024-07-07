namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D023Gurfurlur;

public enum OID : uint
{
    Boss = 0x415F, // R7.000, x1
    Helper = 0x233C, // R0.500, x32, 523 type
    AuraSphere = 0x4162, // R1.000, x0 (spawn during fight)
    BitingWind = 0x4160, // R1.000, x0 (spawn during fight)
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    HeavingHaymaker1 = 36269, // Boss->self, 5.0s cast, single-target
    HeavingHaymaker2 = 36375, // Helper->self, 5.3s cast, range 60 circle

    Stonework = 36301, // Boss->self, 3.0s cast, single-target
    LithicImpact = 36302, // Helper->self, 6.8s cast, range 4 width 4 rect
    GreatFlood = 36307, // Helper->self, 7.0s cast, range 80 width 60 rect

    Allfire1 = 36303, // Helper->self, 7.0s cast, range 10 width 10 rect
    Allfire2 = 36304, // Helper->self, 8.5s cast, range 10 width 10 rect
    Allfire3 = 36305, // Helper->self, 10.0s cast, range 10 width 10 rect

    VolcanicDrop = 36306, // Helper->player, 5.0s cast, range 6 circle
    UnknownSpell = 36315, // Helper->player, no cast, single-target

    Sledgehammer1 = 36313, // Boss->self/players, 5.0s cast, range 60 width 8 rect // party stack line aoe
    Sledgehammer2 = 36314, // Boss->self, no cast, range 60 width 8 rect
    Sledgehammer3 = 39260, // Boss->self, no cast, range 60 width 8 rect

    ArcaneStomp = 36319, // Boss->self, 3.0s cast, single-target

    ShroudOfEons1 = 36321, // AuraSphere->player, no cast, single-target
    ShroudOfEons2 = 36322, // AuraSphere->Boss, no cast, single-target

    EnduringGlory = 36320, // Boss->self, 6.0s cast, range 60 circle

    Windswrath1 = 36310, // Helper->self, 7.0s cast, range 40
    Windswrath2 = 39074, // Helper->self, 15.0s cast, range 40 circle

    Whirlwind = 36311, // Helper->self, no cast, range 5 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper->player, extra=0x1
    Heavy = 1141, // none->AuraSphere, extra=0x11
    UnknownStatus = 2552, // Boss->Boss, extra=0x2CF
    DamageUp1 = 3975, // AuraSphere->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8
    DamageUp2 = 443, // AuraSphere->Boss, extra=0x1
}

public enum IconID : uint
{
    Spreadmarker = 139, // player
}

class HeavingHaymaker2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HeavingHaymaker2));
class LithicImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LithicImpact), new AOEShapeRect(4, 2));
class GreatFlood(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GreatFlood), 25, stopAtWall: true, kind: Kind.DirForward);
class Allfire1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Allfire1), new AOEShapeRect(10, 5));
class Allfire2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Allfire2), new AOEShapeRect(10, 5));
class Allfire3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Allfire3), new AOEShapeRect(10, 5));
class VolcanicDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VolcanicDrop), 6);
class EnduringGlory(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EnduringGlory));
class Windswrath1(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Windswrath1), 15, stopAtWall: true);
class Windswrath2(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Windswrath2), 15, stopAtWall: true);

class D023GurfurlurStates : StateMachineBuilder
{
    public D023GurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavingHaymaker2>()
            .ActivateOnEnter<LithicImpact>()
            .ActivateOnEnter<GreatFlood>()
            .ActivateOnEnter<Allfire1>()
            .ActivateOnEnter<Allfire2>()
            .ActivateOnEnter<Allfire3>()
            .ActivateOnEnter<VolcanicDrop>()
            .ActivateOnEnter<EnduringGlory>()
            .ActivateOnEnter<Windswrath1>()
            .ActivateOnEnter<Windswrath2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12705)]
public class D023Gurfurlur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-54, -195), new ArenaBoundsSquare(20));
