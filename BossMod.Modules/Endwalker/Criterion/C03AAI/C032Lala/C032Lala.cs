namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class ArcaneBlight(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(60, 135.Degrees()));
class NArcaneBlight(BossModule module) : ArcaneBlight(module, AID.NArcaneBlightAOE);
class SArcaneBlight(BossModule module) : ArcaneBlight(module, AID.SArcaneBlightAOE);

public abstract class C032Lala(ModuleInit init) : BossModule(init, new(200, 0), new ArenaBoundsSquare(20));

[ModuleInfo(PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12639, SortOrder = 8, PlanLevel = 90)]
public class C032NLala(ModuleInit init) : C032Lala(init);

[ModuleInfo(PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12639, SortOrder = 8, PlanLevel = 90)]
public class C032SLala(ModuleInit init) : C032Lala(init);
