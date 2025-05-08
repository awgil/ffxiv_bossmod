namespace BossMod.RealmReborn.Raid.T01ADS;

public enum OID : uint
{
    Boss = 0x887, // x1
    Helper = 0x8EC, // x1
    PatrolNode = 0x888, // spawn during fight
    AttackNode = 0x889, // spawn during fight
    DefenseNode = 0x88A, // spawn during fight
    GravityField = 0x1E8728, // EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 1215, // Boss/PatrolNode/AttackNode/DefenseNode->player, no cast, range 6+R ?-degree cone cleave
    HighVoltage = 1447, // Boss->self, 2.5s cast (interruptible), raidwide
    RepellingCannons = 1448, // Boss/PatrolNode/AttackNode/DefenseNode->self, 2.5s cast, range 6+R circle aoe
    PiercingLaser = 1450, // Boss->self, 2.2s cast, range 30+R width 6 rect aoe
    DirtyCannons = 1378, // PatrolNode->self, 1.0s cast, range 4+R circle aoe
    GravityField = 1220, // AttackNode->location, 1.0s cast, range 6 circle, spawns voidzone
    ChainLightning = 1225, // DefenseNode->self, 1.5s cast, single-target, visual
    ChainLightningAOE = 1449, // Helper->player, no cast, single-target, ???
    NodeRetrieval = 1228, // Boss->PatrolNode/AttackNode/DefenseNode, no cast, single-target, happens if add is not killed in ~27s and gives boss damage up
    Object199 = 1229, // Boss->self, no cast, enrage
}

class HighVoltage(BossModule module) : Components.CastHint(module, AID.HighVoltage, "Interruptible");
class RepellingCannons(BossModule module) : Components.StandardAOEs(module, AID.RepellingCannons, new AOEShapeCircle(8.3f));
class PiercingLaser(BossModule module) : Components.StandardAOEs(module, AID.PiercingLaser, new AOEShapeRect(32.3f, 3));
class DirtyCannons(BossModule module) : Components.StandardAOEs(module, AID.DirtyCannons, new AOEShapeCircle(5.15f));
class GravityField(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.GravityField, m => m.Enemies(OID.GravityField), 1);

// TODO: chain lightning?..

class T01ADSStates : StateMachineBuilder
{
    public T01ADSStates(BossModule module) : base(module)
    {
        // adds spawn: at ~40.3, ~80.3, ~120.3, ~160.3, ~200.3 (2x)
        // enrage: first cast at ~245.2, then repeat every 5s
        TrivialPhase(0, 245)
            .ActivateOnEnter<HighVoltage>()
            .ActivateOnEnter<RepellingCannons>()
            .ActivateOnEnter<PiercingLaser>()
            .ActivateOnEnter<DirtyCannons>()
            .ActivateOnEnter<GravityField>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 93, NameID = 1459, SortOrder = 1)]
public class T01ADS(WorldState ws, Actor primary) : BossModule(ws, primary, new(-3, 27), new ArenaBoundsRect(7, 28))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var enemy in hints.PotentialTargets)
        {
            if (enemy.Actor == PrimaryActor)
            {
                enemy.Priority = 1;
                enemy.ShouldBeInterrupted = true; // only interruptible spell (high voltage) should be interrupted every time (TODO: consider physranged even/mt odd)
            }
            else if ((OID)enemy.Actor.OID is OID.PatrolNode or OID.AttackNode or OID.DefenseNode)
            {
                // these always spawn at south entrance, let OT tank them facing away from raid
                // even at MINE first add spawns when boss has ~25% hp left, so it makes sense just to offtank it and zerg boss
                enemy.Priority = assignment == PartyRolesConfig.Assignment.OT ? 2 : 0;
                enemy.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.OT;
                enemy.DesiredRotation = 0.Degrees();
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.PatrolNode))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.AttackNode))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.DefenseNode))
            Arena.Actor(e, ArenaColor.Enemy);
    }
}
