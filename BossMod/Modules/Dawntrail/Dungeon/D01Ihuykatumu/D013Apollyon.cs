namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D013Apollyon;

public enum OID : uint
{
    Boss = 0x4165, // R7.000, x1
    Helper = 0x233C, // R0.500, x20 (spawn during fight), 523 type
    IhuykatumuOcelot = 0x4166, // R3.570, x0 (spawn during fight)
    IhuykatumuPuma = 0x4167, // R2.520, x0 (spawn during fight)
    IhuykatumuSandworm1 = 0x4169, // R3.300, x0 (spawn during fight)
    IhuykatumuSandworm2 = 0x4168, // R3.300, x0 (spawn during fight)
    Whirlwind = 0x416C, // R1.000, x0 (spawn during fight)
    Actor1eba21 = 0x1EBA21, // R0.500, x0 (spawn during fight), EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // IhuykatumuOcelot/IhuykatumuPuma/IhuykatumuSandworm2/IhuykatumuSandworm1->Boss, no cast, single-target

    RazorZephyr = 36340, // Boss->self, 4.0s cast, range 50 width 12 rect

    Blade = 36347, // Boss->player, 4.5s cast, single-target

    UnknownAbility1 = 36344, // Boss->location, no cast, single-target
    HighWind = 36341, // Boss->self, 5.0s cast, range 60 circle
    Devour = 36342, // Boss->self, no cast, single-target
    SwarmingLocust = 36343, // Boss->self, 3.0s cast, single-targe

    BladesOfFamine1 = 36345, // Boss->self, 2.2+0.8s cast, single-target
    BladesOfFamine2 = 36346, // Helper->self, 3.0s cast, range 50 width 12 rect

    Levinsickle1 = 36348, // Boss->self, 4.5+0.5s cast, single-target
    Levinsickle2 = 36350, // Helper->location, 5.0s cast, range 4 circle

    LevinsickleSpark = 36349, // Helper->location, 5.0s cast, range 4 circle

    WingOfLightning = 36351, // Helper->self, 8.0s cast, range 40 ?-degree cone

    ThunderIII1 = 36352, // Boss->self, 4.0+1.0s cast, single-target
    ThunderIII2 = 36353, // Helper->player, 5.0s cast, range 6 circle // spread

    UnknownAbility2 = 36354, // IhuykatumuSandworm1/IhuykatumuSandworm2->self, no cast, single-target

    Blade1 = 36356, // Boss->player, 4.5s cast, single-target
    Blade2 = 36357, // Helper->player, 5.0s cast, range 6 circle // AOE Tankbuster

    WindSickle = 36358, // Helper->self, 4.0s cast, range ?-60 donut
    RazorStorm = 36355, // Boss->self, 5.0s cast, range 40 width 40 rect
    Windwhistle = 36359, // Boss->self, 4.0s cast, single-target
    CuttingWind = 36360, // Helper->self, no cast, range 72 width 8 rect
    BitingWind = 36761, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    ApexWings = 3803, // none->Boss, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2/0x3
    Electrocution1 = 3073, // none->player, extra=0x0
    Electrocution2 = 3074, // none->player, extra=0x0
    ApexBlades = 3804, // none->Boss, extra=0x0
    Windburn = 2084, // Helper->player, extra=0x0
    UnknownStatus1 = 2234, // none->Whirlwind, extra=0xA
    UnknownStatus2 = 3517, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Icon108 = 108, // player
    Icon344 = 344, // player
    Icon506 = 506, // Whirlwind
}

class RazorZephyr(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RazorZephyr), new AOEShapeRect(50, 6));
class Blade(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Blade));
class HighWind(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HighWind));
class BladesOfFamine2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BladesOfFamine2), new AOEShapeRect(50, 6));
class Levinsickle2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Levinsickle2), 4);
class LevinsickleSpark(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LevinsickleSpark), 4);

//class WingOfLightning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WingOfLightning), new AOEShapeCone(40, 60.Degrees())); degrees unknown

class ThunderIII2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ThunderIII2), 6);
class Blade2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Blade2), 6);
class WindSickle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindSickle), new AOEShapeDonut(6, 60));
class RazorStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RazorStorm), new AOEShapeRect(40, 20));
class CuttingWind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CuttingWind), new AOEShapeRect(72, 4));

class D013ApollyonStates : StateMachineBuilder
{
    public D013ApollyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RazorZephyr>()
            .ActivateOnEnter<Blade>()
            .ActivateOnEnter<HighWind>()
            .ActivateOnEnter<BladesOfFamine2>()
            .ActivateOnEnter<Levinsickle2>()
            .ActivateOnEnter<LevinsickleSpark>()
            //.ActivateOnEnter<WingOfLightning>()
            .ActivateOnEnter<ThunderIII2>()
            .ActivateOnEnter<Blade2>()
            .ActivateOnEnter<WindSickle>()
            .ActivateOnEnter<RazorStorm>()
            .ActivateOnEnter<CuttingWind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12711)]
public class D013Apollyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-107, 265), new ArenaBoundsCircle(19.5f));
