namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko;

class LateralSlice : Components.BaitAwayCast
{
    public LateralSlice(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(40, 45.Degrees())) { } // TODO: verify angle
}
class NLateralSlice : LateralSlice { public NLateralSlice() : base(AID.NLateralSlice) { } }
class SLateralSlice : LateralSlice { public SLateralSlice() : base(AID.SLateralSlice) { } }

public abstract class C023Moko : BossModule
{
    public C023Moko(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-200, 0), 20)) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12357, SortOrder = 8)]
public class C023NMoko : C023Moko { public C023NMoko(WorldState ws, Actor primary) : base(ws, primary) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12357, SortOrder = 8)]
public class C023SMoko : C023Moko { public C023SMoko(WorldState ws, Actor primary) : base(ws, primary) { } }
