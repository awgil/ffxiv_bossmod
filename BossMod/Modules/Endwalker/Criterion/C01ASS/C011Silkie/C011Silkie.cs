namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie
{
    class FizzlingDuster : Components.SelfTargetedAOEs
    {
        public FizzlingDuster(AID aid) : base(ActionID.MakeSpell(aid), C011Silkie.ShapeYellow) { }
    }
    class NFizzlingDuster : FizzlingDuster { public NFizzlingDuster() : base(AID.NFizzlingDusterAOE) { } }
    class SFizzlingDuster : FizzlingDuster { public SFizzlingDuster() : base(AID.SFizzlingDusterAOE) { } }

    class DustBluster : Components.KnockbackFromCastTarget
    {
        public DustBluster(AID aid) : base(ActionID.MakeSpell(aid), 16) { }
    }
    class NDustBluster : DustBluster { public NDustBluster() : base(AID.NDustBluster) { } }
    class SDustBluster : DustBluster { public SDustBluster() : base(AID.SDustBluster) { } }

    class SqueakyCleanE : Components.SelfTargetedAOEs
    {
        public SqueakyCleanE(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(60, 112.5f.Degrees())) { }
    }
    class NSqueakyCleanE : SqueakyCleanE { public NSqueakyCleanE() : base(AID.NSqueakyCleanAOE3E) { } }
    class SSqueakyCleanE : SqueakyCleanE { public SSqueakyCleanE() : base(AID.SSqueakyCleanAOE3E) { } }

    class SqueakyCleanW : Components.SelfTargetedAOEs
    {
        public SqueakyCleanW(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(60, 112.5f.Degrees())) { }
    }
    class NSqueakyCleanW : SqueakyCleanW { public NSqueakyCleanW() : base(AID.NSqueakyCleanAOE3W) { } }
    class SSqueakyCleanW : SqueakyCleanW { public SSqueakyCleanW() : base(AID.SSqueakyCleanAOE3W) { } }

    class ChillingDusterPuff : Components.SelfTargetedAOEs
    {
        public ChillingDusterPuff(AID aid) : base(ActionID.MakeSpell(aid), C011Silkie.ShapeBlue) { }
    }
    class NChillingDusterPuff : ChillingDusterPuff { public NChillingDusterPuff() : base(AID.NChillingDusterPuff) { } }
    class SChillingDusterPuff : ChillingDusterPuff { public SChillingDusterPuff() : base(AID.SChillingDusterPuff) { } }

    class BracingDusterPuff : Components.SelfTargetedAOEs
    {
        public BracingDusterPuff(AID aid) : base(ActionID.MakeSpell(aid), C011Silkie.ShapeGreen) { }
    }
    class NBracingDusterPuff : BracingDusterPuff { public NBracingDusterPuff() : base(AID.NBracingDusterPuff) { } }
    class SBracingDusterPuff : BracingDusterPuff { public SBracingDusterPuff() : base(AID.SBracingDusterPuff) { } }

    class FizzlingDusterPuff : Components.SelfTargetedAOEs
    {
        public FizzlingDusterPuff(AID aid) : base(ActionID.MakeSpell(aid), C011Silkie.ShapeYellow) { }
    }
    class NFizzlingDusterPuff : FizzlingDusterPuff { public NFizzlingDusterPuff() : base(AID.NFizzlingDusterPuff) { } }
    class SFizzlingDusterPuff : FizzlingDusterPuff { public SFizzlingDusterPuff() : base(AID.SFizzlingDusterPuff) { } }

    public abstract class C011Silkie : BossModule
    {
        public static AOEShapeCross ShapeBlue = new(60, 5);
        public static AOEShapeDonut ShapeGreen = new(5, 60);
        public static AOEShapeCone ShapeYellow = new(60, 22.5f.Degrees());

        public C011Silkie(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-335, -155), 20)) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NBoss, CFCID = 878, NameID = 11369)]
    public class C011NSilkie : C011Silkie { public C011NSilkie(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SBoss, CFCID = 879, NameID = 11369)]
    public class C011SSilkie : C011Silkie { public C011SSilkie(WorldState ws, Actor primary) : base(ws, primary) { } }
}
