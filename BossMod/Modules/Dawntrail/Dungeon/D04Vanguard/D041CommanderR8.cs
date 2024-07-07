namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D041CommanderR8;

public enum OID : uint
{
    Boss = 0x411D, // R3.240, x1
    Helper = 0x233C, // R0.500, x7, 523 type
    VanguardSentryR8 = 0x41BC, // R3.240, x0 (spawn during fight)
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 36403, // Boss->player, no cast, single-target

    Electrowave = 36571, // Boss->self, 5.0s cast, range 60 circle

    EnhancedMobility1 = 39140, // Boss->location, 10.0s cast, range 14 width 6 rect
    EnhancedMobility2 = 39141, // Boss->location, 10.0s cast, range 14 width 6 rect
    EnhancedMobility3 = 36559, // Boss->location, 10.0s cast, range 14 width 6 rect
    EnhancedMobility4 = 36560, // Boss->location, 10.0s cast, range 14 width 6 rect

    EnhancedMobility5 = 36563, // Helper->self, 10.5s cast, range 10 width 14 rect
    EnhancedMobility6 = 36564, // Helper->self, 10.5s cast, range 10 width 14 rect
    EnhancedMobility7 = 37184, // Helper->self, 10.5s cast, range 20 width 14 rect
    EnhancedMobility8 = 37191, // Helper->self, 10.5s cast, range 20 width 14 rect

    RapidRotary1 = 36561, // Boss->self, no cast, single-target
    RapidRotary2 = 36566, // Helper->self, no cast, range 14 ?-degree cone
    RapidRotary3 = 36567, // Helper->self, no cast, range ?-28 donut
    RapidRotary4 = 36565, // Helper->self, no cast, range ?-17 donut
    RapidRotary5 = 39142, // Boss->self, no cast, single-target
    RapidRotary6 = 39143, // Boss->self, no cast, single-target

    Dispatch = 36568, // Boss->self, 4.0s cast, single-target
    Rush = 36569, // VanguardSentryR8->location, 6.0s cast, width 5 rect charge
    AerialOffensive = 36570, // VanguardSentryR8->location, 9.0s cast, range 4 circle

    Electrosurge1 = 36572, // Boss->self, 4.0+1.0s cast, single-target
    Electrosurge2 = 36573, // Helper->player, 5.0s cast, range 5 circle
}

public enum SID : uint
{
    UnknownStatus = 2970, // none->Boss/VanguardSentryR8, extra=0x2AF/0x2CA/0x2B0
    VulnerabilityUp = 1789, // VanguardSentryR8->player, extra=0x1/0x2
    AreaOfInfluenceUp = 1749, // none->VanguardSentryR8, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA
    Electrocution1 = 3073, // none->player, extra=0x0
    Electrocution2 = 3074, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon315 = 315, // player
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));

class EnhancedMobility1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility1), 6);
class EnhancedMobility2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility1), 6);
class EnhancedMobility3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility1), 6);
class EnhancedMobility4(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility4), 6);

class EnhancedMobility5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility2), new AOEShapeRect(10, 6));
class EnhancedMobility6(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility3), new AOEShapeRect(10, 6));
class EnhancedMobility7(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility2), new AOEShapeRect(20, 6));
class EnhancedMobility8(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility3), new AOEShapeRect(20, 6));

class Rush(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Rush), 2.5f);
class AerialOffensive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AerialOffensive), new AOEShapeCircle(5));
class Electrosurge2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Electrosurge2), new AOEShapeCircle(5));

class D041CommanderR8States : StateMachineBuilder
{
    public D041CommanderR8States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<EnhancedMobility1>()
            .ActivateOnEnter<EnhancedMobility2>()
            .ActivateOnEnter<EnhancedMobility3>()
            .ActivateOnEnter<EnhancedMobility4>()
            .ActivateOnEnter<EnhancedMobility5>()
            .ActivateOnEnter<EnhancedMobility6>()
            .ActivateOnEnter<EnhancedMobility7>()
            .ActivateOnEnter<EnhancedMobility8>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<AerialOffensive>()
            .ActivateOnEnter<Electrosurge2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12750)]
public class D041CommanderR8(WorldState ws, Actor primary) : BossModule(ws, primary, new(-100, 207), new ArenaBoundsSquare(17));
