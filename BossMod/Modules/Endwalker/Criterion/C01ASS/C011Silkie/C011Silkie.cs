namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

abstract class FizzlingDuster(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60f, 22.5f.Degrees()));
sealed class NFizzlingDuster(BossModule module) : FizzlingDuster(module, (uint)AID.NFizzlingDusterAOE);
sealed class SFizzlingDuster(BossModule module) : FizzlingDuster(module, (uint)AID.SFizzlingDusterAOE);
sealed class NFizzlingDusterPuff(BossModule module) : FizzlingDuster(module, (uint)AID.NFizzlingDusterPuff);
sealed class SFizzlingDusterPuff(BossModule module) : FizzlingDuster(module, (uint)AID.SFizzlingDusterPuff);

abstract class DustBluster(BossModule module, uint aid) : Components.SimpleKnockbacks(module, aid, 16f);
sealed class NDustBluster(BossModule module) : DustBluster(module, (uint)AID.NDustBluster);
sealed class SDustBluster(BossModule module) : DustBluster(module, (uint)AID.SDustBluster);

abstract class SqueakyCleanE(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60f, 112.5f.Degrees()));
sealed class NSqueakyCleanE(BossModule module) : SqueakyCleanE(module, (uint)AID.NSqueakyCleanAOE3E);
sealed class SSqueakyCleanE(BossModule module) : SqueakyCleanE(module, (uint)AID.SSqueakyCleanAOE3E);

abstract class SqueakyCleanW(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60f, 112.5f.Degrees()));
sealed class NSqueakyCleanW(BossModule module) : SqueakyCleanW(module, (uint)AID.NSqueakyCleanAOE3W);
sealed class SSqueakyCleanW(BossModule module) : SqueakyCleanW(module, (uint)AID.SSqueakyCleanAOE3W);

abstract class ChillingDusterPuff(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCross(60f, 5f));
sealed class NChillingDusterPuff(BossModule module) : ChillingDusterPuff(module, (uint)AID.NChillingDusterPuff);
sealed class SChillingDusterPuff(BossModule module) : ChillingDusterPuff(module, (uint)AID.SChillingDusterPuff);

abstract class BracingDusterPuff(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(5f, 60f));
sealed class NBracingDusterPuff(BossModule module) : BracingDusterPuff(module, (uint)AID.NBracingDusterPuff);
sealed class SBracingDusterPuff(BossModule module) : BracingDusterPuff(module, (uint)AID.SBracingDusterPuff);

public abstract class C011Silkie(WorldState ws, Actor primary) : BossModule(ws, primary, new(-335f, -155f), new ArenaBoundsSquare(29.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878u, NameID = 11369u, SortOrder = 2, PlanLevel = 90)]
public sealed class C011NSilkie(WorldState ws, Actor primary) : C011Silkie(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879u, NameID = 11369u, SortOrder = 2, PlanLevel = 90)]
public sealed class C011SSilkie(WorldState ws, Actor primary) : C011Silkie(ws, primary);
