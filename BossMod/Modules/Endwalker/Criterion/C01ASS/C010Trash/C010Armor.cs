namespace BossMod.Endwalker.Criterion.C01ASS.C010Armor
{
    public enum OID : uint
    {
        NBoss = 0x3AD8, // R2.500, x1
        SBoss = 0x3AE1, // R2.500, x1
    };

    public enum AID : uint
    {
        AutoAttack = 31109, // Boss->player, no cast, single-target
        NDominionSlash = 31082, // Boss->self, 3.5s cast, range 12 90-degree cone aoe
        NInfernalWeight = 31083, // Boss->self, 5.0s cast, raidwide
        NHellsNebula = 31084, // Boss->self, 4.0s cast, raidwide set hp to 1
        SDominionSlash = 31106, // Boss->self, 3.5s cast, range 12 90-degree cone aoe
        SInfernalWeight = 31107, // Boss->self, 5.0s cast, raidwide
        SHellsNebula = 31108, // Boss->self, 4.0s cast, raidwide set hp to 1
    };

    class DominionSlash : Components.SelfTargetedAOEs
    {
        public DominionSlash(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(12, 45.Degrees())) { }
    }
    class NDominionSlash : DominionSlash { public NDominionSlash() : base(AID.NDominionSlash) { } }
    class SDominionSlash : DominionSlash { public SDominionSlash() : base(AID.SDominionSlash) { } }

    class InfernalWeight : Components.RaidwideCast
    {
        public InfernalWeight(AID aid) : base(ActionID.MakeSpell(aid), "Raidwide with slow") { }
    }
    class NInfernalWeight : InfernalWeight { public NInfernalWeight() : base(AID.NInfernalWeight) { } }
    class SInfernalWeight : InfernalWeight { public SInfernalWeight() : base(AID.SInfernalWeight) { } }

    class HellsNebula : Components.CastHint
    {
        public HellsNebula(AID aid) : base(ActionID.MakeSpell(aid), "Reduce hp to 1") { }
    }
    class NHellsNebula : HellsNebula { public NHellsNebula() : base(AID.NHellsNebula) { } }
    class SHellsNebula : HellsNebula { public SHellsNebula() : base(AID.SHellsNebula) { } }

    class C010ArmorStates : StateMachineBuilder
    {
        public C010ArmorStates(BossModule module, bool savage) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<NDominionSlash>(!savage)
                .ActivateOnEnter<NInfernalWeight>(!savage)
                .ActivateOnEnter<NHellsNebula>(!savage)
                .ActivateOnEnter<SDominionSlash>(savage)
                .ActivateOnEnter<SInfernalWeight>(savage)
                .ActivateOnEnter<SHellsNebula>(savage);
        }
    }
    class C010NArmorStates : C010ArmorStates { public C010NArmorStates(BossModule module) : base(module, false) { } }
    class C010SArmorStates : C010ArmorStates { public C010SArmorStates(BossModule module) : base(module, true) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NBoss)]
    public class C010NArmor : SimpleBossModule { public C010NArmor(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SBoss)]
    public class C010SArmor : SimpleBossModule { public C010SArmor(WorldState ws, Actor primary) : base(ws, primary) { } }
}
