namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

abstract class SplittingCry(BossModule module, uint aid) : Components.BaitAwayCast(module, aid, new AOEShapeRect(60f, 7f));
sealed class NSplittingCry(BossModule module) : SplittingCry(module, (uint)AID.NSplittingCry);
sealed class SSplittingCry(BossModule module) : SplittingCry(module, (uint)AID.SSplittingCry);

abstract class ThunderVortex(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(8f, 30f));
sealed class NThunderVortex(BossModule module) : ThunderVortex(module, (uint)AID.NThunderVortex);
sealed class SThunderVortex(BossModule module) : ThunderVortex(module, (uint)AID.SThunderVortex);

public abstract class C021Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -100f), new ArenaBoundsSquare(24.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946u, NameID = 12428u, SortOrder = 2, PlanLevel = 90)]
public sealed class C021NShishio(WorldState ws, Actor primary) : C021Shishio(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947u, NameID = 12428u, SortOrder = 2, PlanLevel = 90)]
public sealed class C021SShishio(WorldState ws, Actor primary) : C021Shishio(ws, primary);
