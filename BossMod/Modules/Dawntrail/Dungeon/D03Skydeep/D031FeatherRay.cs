namespace BossMod.Dawntrail.Dungeon.D03Skydeep.D031FeatherRay;

public enum OID : uint
{
    Boss = 0x41D3, // R5.000, x1
    Helper = 0x233C, // R0.500, x5, 523 type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x4, EventObj type
    Actor1ea2ef = 0x1EA2EF, // R0.500, x1, EventObj type
    AiryBubble = 0x41D4, // R1.100-2.200, x36
    SkydeepSpecter = 0x414C, // R2.200, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Immersion = 36739, // Boss->self, 5.0s cast, range 24 circle
    TroublesomeTail = 36727, // Boss->self, 4.0s cast, range 24 circle 

    WorrisomeWave1 = 36728, // Boss->self, 4.0s cast, range 24 30.000-degree cone
    WorrisomeWave2 = 36729, // Helper->self, no cast, range 24 ?-degree cone // Transmission from P6S Hegemone aka snakes

    HydroRing = 36733, // Boss->self, 5.0s cast, range ?-24 donut
    BlowingBubbles = 36732, // Boss->self, 3.0s cast, single-target
    Pop = 36734, // AiryBubble->player, no cast, single-target
    BubbleBomb = 36735, // Boss->self, 3.0s cast, single-target

    RollingCurrent1 = 36737, // Boss->self, 5.0s cast, single-target
    RollingCurrent2 = 36736, // Boss->self, 5.0s cast, single-target

    UnknownWeaponskill = 38185, // Helper->self, 5.0s cast, range 68 width 32 rect
    Burst = 36738, // AiryBubble->self, 1.5s cast, range 6 circle
    TroubleBubbles = 38787, // Boss->self, 3.0s cast, single-target // Transmission from P6S Hegemone aka snakes, but as bubbles this time
}

public enum SID : uint
{
    Nuisance = 3950, // Boss->player, extra=0x0
    VulnerabilityUp = 1789, // Helper/41D4->player, extra=0x2/0x1/0x3
    Dropsy1 = 3075, // none->player, extra=0x0
    Dropsy2 = 3076, // none->player, extra=0x0
    UnknownStatus = 2234, // none->41D4, extra=0xA
}

public enum IconID : uint
{
    Icon514 = 514, // player
}

class Immersion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Immersion));
class Burst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burst), new AOEShapeCircle(6));
class HydroRing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HydroRing), new AOEShapeDonut(6, 24));
class WorrisomeWave1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WorrisomeWave1), new AOEShapeCone(24, 15.Degrees()));

class D031FeatherRayStates : StateMachineBuilder
{
    public D031FeatherRayStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Immersion>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<HydroRing>()
            .ActivateOnEnter<WorrisomeWave1>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12755)]
public class D031FeatherRay(WorldState ws, Actor primary) : BossModule(ws, primary, new(-105, -160), new ArenaBoundsSquare(14));
