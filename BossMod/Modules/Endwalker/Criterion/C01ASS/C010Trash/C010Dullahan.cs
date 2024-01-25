namespace BossMod.Endwalker.Criterion.C01ASS.C010Dullahan
{
    public enum OID : uint
    {
        NBoss = 0x3AD7, // R2.500, x1
        SBoss = 0x3AE0, // R2.500, x1
    };

    public enum AID : uint
    {
        AutoAttack = 31318, // Boss->player, no cast, single-target
        NBlightedGloom = 31078, // Boss->self, 4.0s cast, range 10 circle aoe
        NKingsWill = 31080, // Boss->self, 2.5s cast, single-target damage up
        NInfernalPain = 31081, // Boss->self, 5.0s cast, raidwide
        SBlightedGloom = 31102, // Boss->self, 4.0s cast, range 10 circle aoe
        SKingsWill = 31104, // Boss->self, 2.5s cast, single-target damage up
        SInfernalPain = 31105, // Boss->self, 5.0s cast, raidwide
    };

    class BlightedGloom : Components.SelfTargetedAOEs
    {
        public BlightedGloom(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCircle(10)) { }
    }
    class NBlightedGloom : BlightedGloom { public NBlightedGloom() : base(AID.NBlightedGloom) { } }
    class SBlightedGloom : BlightedGloom { public SBlightedGloom() : base(AID.SBlightedGloom) { } }

    class KingsWill : Components.CastHint
    {
        public KingsWill(AID aid) : base(ActionID.MakeSpell(aid), "Damage increase buff") { }
    }
    class NKingsWill : KingsWill { public NKingsWill() : base(AID.NKingsWill) { } }
    class SKingsWill : KingsWill { public SKingsWill() : base(AID.SKingsWill) { } }

    class InfernalPain : Components.RaidwideCast
    {
        public InfernalPain(AID aid) : base(ActionID.MakeSpell(aid)) { }
    }
    class NInfernalPain : InfernalPain { public NInfernalPain() : base(AID.NInfernalPain) { } }
    class SInfernalPain : InfernalPain { public SInfernalPain() : base(AID.SInfernalPain) { } }

    class C010DullahanStates : StateMachineBuilder
    {
        public C010DullahanStates(BossModule module, bool savage) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<NBlightedGloom>(!savage)
                .ActivateOnEnter<NKingsWill>(!savage)
                .ActivateOnEnter<NInfernalPain>(!savage)
                .ActivateOnEnter<SBlightedGloom>(savage)
                .ActivateOnEnter<SKingsWill>(savage)
                .ActivateOnEnter<SInfernalPain>(savage);
        }
    }
    class C010NDullahanStates : C010DullahanStates { public C010NDullahanStates(BossModule module) : base(module, false) { } }
    class C010SDullahanStates : C010DullahanStates { public C010SDullahanStates(BossModule module) : base(module, true) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NBoss, CFCID = 878, NameID = 11506)]
    public class C010NDullahan : SimpleBossModule { public C010NDullahan(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SBoss, CFCID = 879, NameID = 11506)]
    public class C010SDullahan : SimpleBossModule { public C010SDullahan(WorldState ws, Actor primary) : base(ws, primary) { } }
}
