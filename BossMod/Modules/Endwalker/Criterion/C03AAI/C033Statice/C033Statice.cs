namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

class SurpriseBalloon : Components.KnockbackFromCastTarget
{
    public SurpriseBalloon(AID aid) : base(ActionID.MakeSpell(aid), 13) { }
}
class NSurpriseBalloon : SurpriseBalloon { public NSurpriseBalloon() : base(AID.NPop) { } }
class SSurpriseBalloon : SurpriseBalloon { public SSurpriseBalloon() : base(AID.SPop) { } }

class BeguilingGlitter : Components.StatusDrivenForcedMarch
{
    public BeguilingGlitter() : base(2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { ActivationLimit = 8; }
}

class FaerieRing : Components.SelfTargetedAOEs
{
    public FaerieRing(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeDonut(6, 12)) { } // TODO: verify inner radius
}
class NFaerieRing : FaerieRing { public NFaerieRing() : base(AID.NFaerieRing) { } }
class SFaerieRing : FaerieRing { public SFaerieRing() : base(AID.SFaerieRing) { } }

public abstract class C033Statice : BossModule
{
    public C033Statice(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-200, 0), 20)) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12506, SortOrder = 9)]
public class C033NStatice : C033Statice { public C033NStatice(WorldState ws, Actor primary) : base(ws, primary) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12506, SortOrder = 9)]
public class C033SStatice : C033Statice { public C033SStatice(WorldState ws, Actor primary) : base(ws, primary) { } }
