namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class ArcaneBlight(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(60, 135.Degrees()));
class NArcaneBlight(BossModule module) : ArcaneBlight(module, AID.NArcaneBlightAOE);
class SArcaneBlight(BossModule module) : ArcaneBlight(module, AID.SArcaneBlightAOE);

public abstract class C032Lala(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, 0), new ArenaBoundsSquare(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12639, SortOrder = 8)]
public class C032NLala(WorldState ws, Actor primary) : C032Lala(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12639, SortOrder = 8)]
public class C032SLala(WorldState ws, Actor primary) : C032Lala(ws, primary);
