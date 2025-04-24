namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko;

class LateralSlice(BossModule module, AID aid) : Components.BaitAwayCast(module, aid, new AOEShapeCone(40, 45.Degrees())); // TODO: verify angle
class NLateralSlice(BossModule module) : LateralSlice(module, AID.NLateralSlice);
class SLateralSlice(BossModule module) : LateralSlice(module, AID.SLateralSlice);

public abstract class C023Moko(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200, 0), new ArenaBoundsSquare(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12357, SortOrder = 8, PlanLevel = 90)]
public class C023NMoko(WorldState ws, Actor primary) : C023Moko(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12357, SortOrder = 8, PlanLevel = 90)]
public class C023SMoko(WorldState ws, Actor primary) : C023Moko(ws, primary);
