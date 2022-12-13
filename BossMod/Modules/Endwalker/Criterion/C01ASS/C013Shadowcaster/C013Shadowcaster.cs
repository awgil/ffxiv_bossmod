namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    class FiresteelFracture : Components.Cleave
    {
        public FiresteelFracture(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(40, 30.Degrees())) { }
    }
    class NFiresteelFracture : FiresteelFracture { public NFiresteelFracture() : base(AID.NFiresteelFracture) { } }
    class SFiresteelFracture : FiresteelFracture { public SFiresteelFracture() : base(AID.SFiresteelFracture) { } }

    // TODO: show AOEs
    class BlazingBenifice : Components.CastCounter
    {
        public BlazingBenifice(AID aid) : base(ActionID.MakeSpell(aid)) { }
    }
    class NBlazingBenifice : BlazingBenifice { public NBlazingBenifice() : base(AID.NBlazingBenifice) { } }
    class SBlazingBenifice : BlazingBenifice { public SBlazingBenifice() : base(AID.SBlazingBenifice) { } }

    class PureFire : Components.LocationTargetedAOEs
    {
        public PureFire(AID aid) : base(ActionID.MakeSpell(aid), 6) { }
    }
    class NPureFire : PureFire { public NPureFire() : base(AID.NPureFireAOE) { } }
    class SPureFire : PureFire { public SPureFire() : base(AID.SPureFireAOE) { } }

    public abstract class C013Shadowcaster : BossModule
    {
        public C013Shadowcaster(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(289, -105), 15, 20)) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NBoss)]
    public class C013NShadowcaster : C013Shadowcaster { public C013NShadowcaster(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SBoss)]
    public class C013SShadowcaster : C013Shadowcaster { public C013SShadowcaster(WorldState ws, Actor primary) : base(ws, primary) { } }
}
