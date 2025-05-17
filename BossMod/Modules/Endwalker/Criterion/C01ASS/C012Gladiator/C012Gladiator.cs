namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator;

class RushOfMightFront(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(60, 90.Degrees()));
class NRushOfMightFront(BossModule module) : RushOfMightFront(module, AID.NRushOfMightFront);
class SRushOfMightFront(BossModule module) : RushOfMightFront(module, AID.SRushOfMightFront);

class RushOfMightBack(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(60, 90.Degrees()));
class NRushOfMightBack(BossModule module) : RushOfMightBack(module, AID.NRushOfMightBack);
class SRushOfMightBack(BossModule module) : RushOfMightBack(module, AID.SRushOfMightBack);

public abstract class C012Gladiator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35, -271), new ArenaBoundsSquare(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11387, SortOrder = 8, PlanLevel = 90)]
public class C012NGladiator(WorldState ws, Actor primary) : C012Gladiator(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11387, SortOrder = 8, PlanLevel = 90)]
public class C012SGladiator(WorldState ws, Actor primary) : C012Gladiator(ws, primary);
