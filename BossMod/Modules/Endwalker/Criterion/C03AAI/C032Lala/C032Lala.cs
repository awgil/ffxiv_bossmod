namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala
{
    class ArcaneBlight : Components.SelfTargetedAOEs
    {
        public ArcaneBlight(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(60, 135.Degrees())) { }
    }
    class NArcaneBlight : ArcaneBlight { public NArcaneBlight() : base(AID.NArcaneBlightAOE) { } }
    class SArcaneBlight : ArcaneBlight { public SArcaneBlight() : base(AID.SArcaneBlightAOE) { } }

    public abstract class C032Lala : BossModule
    {
        public C032Lala(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(200, 0), 20)) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NBoss, CFCID = 979, NameID = 12639)]
    public class C032NLala : C032Lala { public C032NLala(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SBoss, CFCID = 980, NameID = 12639)]
    public class C032SLala : C032Lala { public C032SLala(WorldState ws, Actor primary) : base(ws, primary) { } }
}
