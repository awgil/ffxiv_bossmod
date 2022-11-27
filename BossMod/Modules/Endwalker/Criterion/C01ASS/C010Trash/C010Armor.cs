namespace BossMod.Endwalker.Criterion.C01ASS.C010Armor
{
    public enum OID : uint
    {
        Boss = 0x3AD8, // R2.500, x1
    };

    public enum AID : uint
    {
        AutoAttack = 31109, // Boss->player, no cast, single-target
        DominionSlash = 31082, // Boss->self, 3.5s cast, range 12 90-degree cone aoe
        InfernalWeight = 31083, // Boss->self, 5.0s cast, raidwide
        HellsNebula = 31084, // Boss->self, 4.0s cast, raidwide set hp to 1
    };

    class DominionSlash : Components.SelfTargetedAOEs
    {
        public DominionSlash() : base(ActionID.MakeSpell(AID.DominionSlash), new AOEShapeCone(12, 45.Degrees())) { }
    }

    class InfernalWeight : Components.RaidwideCast
    {
        public InfernalWeight() : base(ActionID.MakeSpell(AID.InfernalWeight), "Raidwide with slow") { }
    }

    class HellsNebula : Components.CastHint
    {
        public HellsNebula() : base(ActionID.MakeSpell(AID.HellsNebula), "Reduce hp to 1") { }
    }

    class C010ArmorStates : StateMachineBuilder
    {
        public C010ArmorStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<DominionSlash>()
                .ActivateOnEnter<InfernalWeight>()
                .ActivateOnEnter<HellsNebula>();
        }
    }

    public class C010Armor : SimpleBossModule
    {
        public C010Armor(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
