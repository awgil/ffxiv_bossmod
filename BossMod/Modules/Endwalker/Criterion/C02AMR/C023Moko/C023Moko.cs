namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko
{
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

    [ModuleInfo(PrimaryActorOID = (uint)OID.NBoss, CFCID = 946, NameID = 12357)]
    public class C023NMoko : C023Moko { public C023NMoko(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SBoss, CFCID = 947, NameID = 12357)]
    public class C023SMoko : C023Moko { public C023SMoko(WorldState ws, Actor primary) : base(ws, primary) { } }
}
