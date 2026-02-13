namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class Unenlightenment(BossModule module, AID aid) : Components.CastCounter(module, aid);
class NUnenlightenment(BossModule module) : Unenlightenment(module, AID.NUnenlightenmentAOE);
class SUnenlightenment(BossModule module) : Unenlightenment(module, AID.SUnenlightenmentAOE);

public abstract class C022Gorai(ModuleArgs init) : BossModule(init, new(300, -120), new ArenaBoundsSquare(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12373, SortOrder = 7, PlanLevel = 90)]
public class C022NGorai(ModuleArgs args) : C022Gorai(args);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12373, SortOrder = 7, PlanLevel = 90)]
public class C022SGorai(ModuleArgs args) : C022Gorai(args);
