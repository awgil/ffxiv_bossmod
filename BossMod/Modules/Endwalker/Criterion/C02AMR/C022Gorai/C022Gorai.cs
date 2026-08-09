namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

abstract class Unenlightenment(BossModule module, uint aid) : Components.CastCounter(module, aid);
sealed class NUnenlightenment(BossModule module) : Unenlightenment(module, (uint)AID.NUnenlightenmentAOE);
sealed class SUnenlightenment(BossModule module) : Unenlightenment(module, (uint)AID.SUnenlightenmentAOE);

public abstract class C022Gorai(WorldState ws, Actor primary) : BossModule(ws, primary, new(300f, -120f), new ArenaBoundsSquare(22.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946u, NameID = 12373u, SortOrder = 4, PlanLevel = 90)]
public sealed class C022NGorai(WorldState ws, Actor primary) : C022Gorai(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947u, NameID = 12373u, SortOrder = 4, PlanLevel = 90)]
public sealed class C022SGorai(WorldState ws, Actor primary) : C022Gorai(ws, primary);
