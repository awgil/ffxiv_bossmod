namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke
{
    class TidalRoar : Components.CastCounter
    {
        public TidalRoar(AID aid) : base(ActionID.MakeSpell(aid)) { }
    }
    class NTidalRoar : TidalRoar { public NTidalRoar() : base(AID.NTidalRoarAOE) { } }
    class STidalRoar : TidalRoar { public STidalRoar() : base(AID.STidalRoarAOE) { } }

    class BubbleNet : Components.CastCounter
    {
        public BubbleNet(AID aid) : base(ActionID.MakeSpell(aid)) { }
    }
    class NBubbleNet1 : BubbleNet { public NBubbleNet1() : base(AID.NBubbleNet1AOE) { } }
    class SBubbleNet1 : BubbleNet { public SBubbleNet1() : base(AID.SBubbleNet1AOE) { } }
    class NBubbleNet2 : BubbleNet { public NBubbleNet2() : base(AID.NBubbleNet2AOE) { } }
    class SBubbleNet2 : BubbleNet { public SBubbleNet2() : base(AID.SBubbleNet2AOE) { } }

    class Hydrobomb : Components.LocationTargetedAOEs
    {
        public Hydrobomb(AID aid) : base(ActionID.MakeSpell(aid), 5) { }
    }
    class NHydrobomb : Hydrobomb { public NHydrobomb() : base(AID.NHydrobombAOE) { } }
    class SHydrobomb : Hydrobomb { public SHydrobomb() : base(AID.SHydrobombAOE) { } }

    public abstract class C031Ketuduke : BossModule
    {
        public C031Ketuduke(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, 0), 20)) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NBoss, CFCID = 979, NameID = 12605)]
    public class C031NKetuduke : C031Ketuduke { public C031NKetuduke(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SBoss, CFCID = 980, NameID = 12605)]
    public class C031SKetuduke : C031Ketuduke { public C031SKetuduke(WorldState ws, Actor primary) : base(ws, primary) { } }
}
