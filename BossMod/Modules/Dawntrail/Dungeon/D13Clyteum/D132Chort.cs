namespace BossMod.Dawntrail.Dungeon.D13Clyteum.D132Chort;

public enum OID : uint
{
    Boss = 0x4C3F, // R5.000, x1
    Helper = 0x233C, // R0.500, x36, Helper type
}

public enum AID : uint
{
    AutoAttack = 45308, // Boss->player, no cast, single-target
    RipplesOfGloomCast = 48884, // Boss->self, 5.0s cast, single-target
    RipplesOfGloom = 50408, // Helper->self, 5.7s cast, range 40 circle
    Jump = 48888, // Boss->location, no cast, single-target
    MortifyingFleshBossCast = 48870, // Boss->self, 5.0s cast, single-target
    MortifyingFleshBossInstant = 48872, // Boss->self, no cast, single-target
    MortifyingFlesh1 = 50400, // Helper->self, 3.0s cast, range 40 width 16 rect
    MortifyingFlesh2 = 48876, // Helper->self, 3.0s cast, range 40 width 16 rect
    BodyweightExorcismKBBoss = 48877, // Boss->self, no cast, single-target
    BodyweightExorcismKB = 48878, // Helper->self, 2.8s cast, range 20 circle
    BasicVomit = 50361, // Boss->self, 5.0s cast, range 50 120-degree cone
    BodyweightExorcismTowerBoss1 = 48879, // Boss->location, 5.0s cast, single-target
    BodyweightExorcismTowerBoss2 = 48880, // Boss->location, no cast, single-target
    BodyweightExorcismTowerBoss3 = 48881, // Boss->self, no cast, single-target
    BodyweightExorcismTower = 48882, // Helper->location, 6.0s cast, range 4 circle
    EvilEmissionCast = 50417, // Boss->self, 4.5s cast, single-target
    EvilEmission = 48885, // Helper->player, 5.0s cast, range 5 circle
    ProfanePressureCast = 48886, // Boss->self, 4.5s cast, single-target
    ProfanePressure = 48887, // Helper->players, 5.0s cast, range 6 circle
}

public enum IconID : uint
{
    Spread = 558, // player->self
    Stack = 161, // player->self
}

class RipplesOfGloom(BossModule module) : Components.RaidwideCast(module, AID.RipplesOfGloom);
class MortifyingFlesh(BossModule module) : Components.GroupedAOEs(module, [AID.MortifyingFlesh1, AID.MortifyingFlesh2], new AOEShapeRect(40, 8));
class BasicVomit(BossModule module) : Components.StandardAOEs(module, AID.BasicVomit, new AOEShapeCone(50f, 60.Degrees()));
class BodyweightExorcismKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.BodyweightExorcismKB, 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(src.Origin, 7), src.Activation);
    }
}
class BodyweightExorcismTower(BossModule module) : Components.CastTowers(module, AID.BodyweightExorcismTower, 4, minSoakers: 4, maxSoakers: 4);
class EvilEmission(BossModule module) : Components.SpreadFromCastTargets(module, AID.EvilEmission, 5);
class ProfanePressure(BossModule module) : Components.StackWithCastTargets(module, AID.ProfanePressure, 5);

class D132ChortStates : StateMachineBuilder
{
    public D132ChortStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RipplesOfGloom>()
            .ActivateOnEnter<MortifyingFlesh>()
            .ActivateOnEnter<BasicVomit>()
            .ActivateOnEnter<BodyweightExorcismKnockback>()
            .ActivateOnEnter<BodyweightExorcismTower>()
            .ActivateOnEnter<EvilEmission>()
            .ActivateOnEnter<ProfanePressure>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1011, NameID = 14734)]
public class D132Chort(WorldState ws, Actor primary) : BossModule(ws, primary, new(660, -141), new ArenaBoundsCircle(15));
