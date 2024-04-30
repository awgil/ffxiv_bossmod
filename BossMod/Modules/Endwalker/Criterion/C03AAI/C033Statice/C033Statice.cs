namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

class SurpriseBalloon(BossModule module, AID aid) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(aid), 13);
class NSurpriseBalloon(BossModule module) : SurpriseBalloon(module, AID.NPop);
class SSurpriseBalloon(BossModule module) : SurpriseBalloon(module, AID.SPop);

class BeguilingGlitter(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);

class FaerieRing(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(6, 12)); // TODO: verify inner radius
class NFaerieRing(BossModule module) : FaerieRing(module, AID.NFaerieRing);
class SFaerieRing(BossModule module) : FaerieRing(module, AID.SFaerieRing);

public abstract class C033Statice(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200, 0), new ArenaBoundsCircle(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12506, SortOrder = 9)]
public class C033NStatice(WorldState ws, Actor primary) : C033Statice(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12506, SortOrder = 9)]
public class C033SStatice(WorldState ws, Actor primary) : C033Statice(ws, primary);
