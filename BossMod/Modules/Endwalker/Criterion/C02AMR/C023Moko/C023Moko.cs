namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

abstract class LateralSlice(BossModule module, uint aid) : Components.BaitAwayCast(module, aid, new AOEShapeCone(40f, 45f.Degrees())); // TODO: verify angle
sealed class NLateralSlice(BossModule module) : LateralSlice(module, (uint)AID.NLateralSlice);
sealed class SLateralSlice(BossModule module) : LateralSlice(module, (uint)AID.SLateralSlice);

public abstract class C023Moko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200f, 0f), new ArenaBoundsSquare(24.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946u, NameID = 12357u, SortOrder = 5, PlanLevel = 90)]
public sealed class C023NMoko(WorldState ws, Actor primary) : C023Moko(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947u, NameID = 12357u, SortOrder = 5, PlanLevel = 90)]
public sealed class C023SMoko(WorldState ws, Actor primary) : C023Moko(ws, primary);
