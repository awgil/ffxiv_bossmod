namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D072Anthracite;

public enum OID : uint
{
    Boss = 0x41BE, // R4.000, x1
    Helper = 0x233C, // R0.500, x26-50 (spawn during fight), 523 type
    ValleyCliffkite = 0x448C, // R3.000, x1
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x0-2 (spawn during fight), EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x0-1 (spawn during fight), EventObj type
    Actor1eba63 = 0x1EBA63, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eba62 = 0x1EBA62, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eba64 = 0x1EBA64, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x0-1 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Anthrabomb1 = 36542, // Boss->self, 6.5s cast, single-target
    Anthrabomb2 = 36543, // Helper->location, 7.5s cast, range 10 circle
    Anthrabomb3 = 36401, // Helper->location, 11.5s cast, range 10 circle
    Anthrabomb4 = 36549, // Helper->location, 4.5s cast, range 10 circle
    Anthrabomb5 = 36402, // Helper->location, 8.5s cast, range 10 circle
    Anthrabomb6 = 36553, // Helper->player, 6.0s cast, range 6 circle

    UnknownSpell1 = 36544, // Helper->location, 7.5s cast, single-target
    UnknownSpell2 = 36546, // Helper->location, 11.5s cast, single-target

    HotBlast1 = 36545, // Helper->self, 11.5s cast, range 40 width 6 rect
    HotBlast2 = 36551, // Helper->self, 8.5s cast, range 40 width 6 rect

    CarbonaceousCombustion1 = 36556, // Boss->self, 5.0s cast, single-target
    CarbonaceousCombustion2 = 36557, // Helper->self, 5.5s cast, range 80 circle

    Carniflagration1 = 36547, // Boss->self, 6.5s cast, single-target
    Carniflagration2 = 36548, // Boss->self, 3.5s cast, single-target

    UnknownSpell3 = 36550, // Helper->location, 4.5s cast, single-target
    UnknownSpell4 = 36552, // Helper->location, 8.5s cast, single-target

    BurningCoals1 = 36554, // Boss->self, 5.5s cast, single-target
    BurningCoals2 = 36555, // Helper->players, 6.0s cast, range 6 circle

    ChimneySmack1 = 38467, // Boss->self, 4.5s cast, single-target
    ChimneySmack2 = 38468, // Helper->player, 5.0s cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2/0x3/0x4
}

public enum IconID : uint
{
    Icon169 = 169, // player
    Icon318 = 318, // player
    Tankbuster = 218, // player
}

class Anthrabomb2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Anthrabomb2), 10);
class Anthrabomb3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Anthrabomb3), 10);
class Anthrabomb4(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Anthrabomb4), 10);
class Anthrabomb5(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Anthrabomb5), 10);
class Anthrabomb6(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Anthrabomb6), 6);
class HotBlast1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HotBlast1), new AOEShapeRect(40, 3));
class HotBlast2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HotBlast2), new AOEShapeRect(40, 3));
class CarbonaceousCombustion2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CarbonaceousCombustion2));
class BurningCoals2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BurningCoals2), 6, 8);

class D072AnthraciteStates : StateMachineBuilder
{
    public D072AnthraciteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Anthrabomb2>()
            .ActivateOnEnter<Anthrabomb3>()
            .ActivateOnEnter<Anthrabomb4>()
            .ActivateOnEnter<Anthrabomb5>()
            .ActivateOnEnter<Anthrabomb6>()
            .ActivateOnEnter<HotBlast1>()
            .ActivateOnEnter<HotBlast2>()
            .ActivateOnEnter<CarbonaceousCombustion2>()
            .ActivateOnEnter<BurningCoals2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12853)]
public class D072Anthracite(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130, -51), new ArenaBoundsSquare(18));
