using BossMod.Dawntrail.Dungeon.D08Deadwalk.D081Leonogg;

namespace BossMod.Dawntrail.Dungeon.D08Deadwalk.D083Traumerei;

public enum OID : uint
{
    Boss = 0x421F, // R26.000, x1
    Helper = 0x233C, // R0.500, x6, 523 type

    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type

    StrayGeist = 0x4221, // R2.000, x0 (spawn during fight)
    StrayPhantagenitrix = 0x4220, // R1.500, x20
    StrayTableSet = 0x42C1, // R0.500, x1
}

public enum AID : uint
{
    AutoAttack = 16764, // Boss->player, no cast, single-target

    BitterRegret1 = 37139, // Boss->self, 6.0+0.7s cast, range 40 width 16 rect
    BitterRegret2 = 37140, // Boss->self, 6.0s cast, single-target
    BitterRegret3 = 37147, // Helper->self, 6.7s cast, range 50 width 12 rect
    BitterRegret4 = 37340, // StrayPhantagenitrix->self, 6.0s cast, range 40 width 4 rect

    Poltergeist = 37132, // Boss->self, 3.0s cast, single-target

    MemorialMarch1 = 37136, // Boss->self, 3.0s cast, single-target
    MemorialMarch2 = 37065, // Boss->self, 6.0s cast, single-target

    Impact = 37133, // Helper->self, 6.0s cast, range 40 width 4 rect
    IllIntent = 39607, // StrayGeist->player, 10.0s cast, single-target

    GhostdusterSpread = 37146, // Helper->player, 8.0s cast, range 8 circle // spread
    GhostdusterSpreadAOE = 37145, // Boss->self, 8.0s cast, single-target

    MaliciousMist1 = 37168, // Boss->self, 5.0s cast, range 60 circle
    MaliciousMist2 = 37138, // StrayGeist->player, 10.0s cast, single-target

    Fleshbuster = 37148, // Boss->self, 8.0s cast, range 60 circle
    UnknownAbility = 37144, // Helper->player, no cast, single-target

    GhostcrusherStack = 37142, // Boss->self, 5.0s cast, single-target // stackmarker
    GhostcrusherStackAOE = 37143, // Helper->self, no cast, range 80 width 8 rect
}

public enum SID : uint
{
    GhostlyGuise = 3949, // none->player, extra=0x0
    Bleeding = 2088, // Boss->player, extra=0x0
    VulnerabilityUp = 1789, // StrayPhantagenitrix->player, extra=0x1
}

public enum IconID : uint
{
    Stackmarker = 196, // player
    PhysicalVulnerabilityDown = 136, // player
}
public enum TetherID : uint
{
    Tether57 = 57, // StrayGeist->player
    Tether1 = 1, // StrayGeist->player
    Tether277 = 277, // StrayPhantagenitrix->Boss
}

class BitterRegret1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BitterRegret1), new AOEShapeRect(40, 8));
class BitterRegret3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BitterRegret3), new AOEShapeRect(50, 6));
class BitterRegret4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BitterRegret4), new AOEShapeRect(40, 2));

class GhostdusterSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.GhostdusterSpread), 8);

class D083TraumereiStates : StateMachineBuilder
{
    public D083TraumereiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BitterRegret1>()
            .ActivateOnEnter<BitterRegret3>()
            .ActivateOnEnter<BitterRegret4>()
            .ActivateOnEnter<GhostdusterSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12763)]
public class D083Traumerei(WorldState ws, Actor primary) : BossModule(ws, primary, new(148, -433), new ArenaBoundsCircle(20));

