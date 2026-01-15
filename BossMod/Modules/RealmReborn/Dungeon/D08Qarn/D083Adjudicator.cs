namespace BossMod.RealmReborn.Dungeon.D08Qarn.D083Adjudicator;

public enum OID : uint
{
    Boss = 0x477E, // R1.500, x1
    MythrilVergeLine = 0x477F, // R0.600, x0 (spawn during fight)
    MythrilVergePulse = 0x4780, // R0.600, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    LoomingJudgement = 42245, // Boss->player, 5.0s cast, single-target tankbuster
    Dark = 42246, // Boss->location, 3.0s cast, range 5 circle aoe
    DarkII = 42248, // Boss->self, 6.0s cast, range 40 120-degree cone aoe
    CreepingDarkness = 42247, // Boss->self, 2.5s cast, range 50 raidwide circle aoe
    SummonVL = 42243, //Boss->self, 3.0s cast, single-target, spawns MythrilVergeLine
    SummonVP = 42239, //Boss->self, 3.0s cast, single-target, spawns MythrilVergePulse

    VergeLine = 42244, // MythrilVergeLine->self, 4.0s cast, range 60+R width 4 rect aoe

    Stun = 30506, // MythrilVergePulse->player, no cast, single-target, applies status Stun/3408
    MythrilChains = 42240, //MythrilVergePulse->player, no cast, single-target, applies status Bind/3625 and tether 31
    VergePulse = 42241, // MythrilVergePulse->self, 20.0s cast, range 60+R width 4 rect aoe
}

public enum SID : uint
{
    Stun = 3408, // MythrilVergePulse->player, extra=0x0
    Bind = 3625, // MythrilVergePulse->player, extra=0x0
}

public enum IconID : uint
{
    Tankbuster = 218, // Player->self
}

public enum TetherID : uint
{
    MythrilChains = 31, // MythrilVergePulse->player
}

class LoomingJudgement(BossModule module) : Components.SingleTargetCast(module, AID.LoomingJudgement);
class CreepingDarkness(BossModule module) : Components.RaidwideCast(module, AID.CreepingDarkness);
class Dark(BossModule module) : Components.StandardAOEs(module, AID.Dark, new AOEShapeCircle(5));
class DarkII(BossModule module) : Components.StandardAOEs(module, AID.DarkII, new AOEShapeCone(40, 60.Degrees()));
class VergeLine(BossModule module) : Components.StandardAOEs(module, AID.VergeLine, new AOEShapeRect(60.6f, 2));
class VergePulse(BossModule module) : Components.StandardAOEs(module, AID.VergePulse, new AOEShapeRect(60.6f, 2));

class D083AdjudicatorStates : StateMachineBuilder
{
    public D083AdjudicatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LoomingJudgement>()
            .ActivateOnEnter<CreepingDarkness>()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<VergeLine>()
            .ActivateOnEnter<VergePulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1570)]
public class D083Adjudicator(WorldState ws, Actor primary) : BossModule(ws, primary, new(238, 0), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                // note: could do 'when x => 3' for tether source to guarantee you get out fast, but that seems overkill.
                // only relevant for non-melee, plus it all dies easy enough anyway.
                OID.MythrilVergePulse or OID.MythrilVergeLine => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.MythrilVergeLine))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.MythrilVergePulse))
            Arena.Actor(e, ArenaColor.Enemy);
    }
}
