namespace BossMod.RealmReborn.Dungeon.D08Qarn.D083Adjudicator;

public enum OID : uint
{
    Boss = 0x477E, // x1
    MythrilVergeLine = 0x477F, // spawn during fight
    MythrilVergePulse = 0x4780, // spawn during fight
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast
    //Darkness = 928, // Boss->self, 2.5s cast, range 7.5 120-degree cone aoe
    LoomingJudgement = 42245, // Boss->player, 5.0s cast, tankbuster
    Dark = 42246, // Boss->none(? or is that just self?), [details]
    DarkII = 42248, // Boss->self, [aoe?]
    CreepingDarkness = 42247, // Boss->self, 2.5s cast, raidwide[?]
    SummonVL = 42243, //Boss->self, 3.0s cast, single-target, summons MythrilVergeLine
    SummonVP = 42239, //Boss->self, 3.0s cast, single-target, summons MythrilVergePulse

    VergeLine = 42244, // MythrilVergeLine->self, 4.0s cast, range 60.6 width 4 rect aoe

    Stun = 30506, // MythrilVergePulse->player, no cast, single-target, applies status 3408
    MythrilChains = 42240, //MythrilVergePulse->player, no cast, single-target, applies Bind and tether
    VergePulse = 42241, // MythrilVergePulse->self, 20.0s cast, range 60.6 width 4 rect aoe
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

//class Darkness(BossModule module) : Components.StandardAOEs(module, AID.Darkness, new AOEShapeCone(7.5f, 60.Degrees()));
class VergeLine(BossModule module) : Components.StandardAOEs(module, AID.VergeLine, new AOEShapeRect(60.6f, 2));
class VergePulse(BossModule module) : Components.StandardAOEs(module, AID.VergePulse, new AOEShapeRect(60.6f, 2));

class D083AdjudicatorStates : StateMachineBuilder
{
    public D083AdjudicatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //.ActivateOnEnter<Darkness>()
            .ActivateOnEnter<VergeLine>()
            .ActivateOnEnter<VergePulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1570)]
public class D083Adjudicator(WorldState ws, Actor primary) : BossModule(ws, primary, new(238, 0), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                // figure out [if tether from vergepulse == on player, target priority 3]; yanked from P10S for now
                //if ((TetherID)tether.ID is TetherID.MythrilChains)
                //    WorldState.Actors.Find(tether.Target) => 3,
                // maybe below is just fine? shouldn't spawn concurrently; Duty Support seems to kill them all fine in the allotted time
                OID.MythrilVergePulse or OID.MythrilVergeLine => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
