// CONTRIB: made by malediktus, not checked
namespace BossMod.Events.FF15Collab.MAx
{
    public enum OID : uint
    {
        Boss = 0x252B, //R=6.75
        MagitekTrooper = 0x252A, //R=0.9
        Helper = 0x233C,
        Noctis = 0x2529,
    }
    public enum AID : uint
    {
        AutoAttack = 14675, // 252A->player/Noctis, no cast, single-target
        AutoAttack2 = 872, // Boss->player, no cast, single-target
        Chainsaw = 14601, // Boss->self, 3,0s cast, range 10 90-degree cone
        MagitekMissile = 14598, // Boss->self, 1,0s cast, single-target
        MagitekMissile2 = 14599, // Helper->location, 3,0s cast, range 5 circle
        Shock = 14600, // Boss->self, 3,0s cast, range 10 circle
        unknown = 14531, // Boss->self, no cast, single-target
        unknown2 = 14533, // Boss->self, no cast, single-target
    };

    class Chainsaw : Components.SelfTargetedAOEs
    {
        public Chainsaw() : base(ActionID.MakeSpell(AID.Chainsaw), new AOEShapeCone(10, 45.Degrees())) { }
    }

    class Shock : Components.SelfTargetedAOEs
    {
        public Shock() : base(ActionID.MakeSpell(AID.Shock), new AOEShapeCircle(10)) { }
    }

    class MagitekMissile : Components.LocationTargetedAOEs
    {
        public MagitekMissile() : base(ActionID.MakeSpell(AID.MagitekMissile2), 5) { }
    }

    class MAxStates : StateMachineBuilder
    {
        public MAxStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Chainsaw>()
                .ActivateOnEnter<Shock>()
                .ActivateOnEnter<MagitekMissile>();
        }
    }

    [ModuleInfo(FateID = 1409, NameID = 7898)]
    public class MAx : BossModule
    {
        public MAx(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(295, -22), 25)) { }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.Noctis))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var s in Enemies(OID.MagitekTrooper))
                Arena.Actor(s, ArenaColor.Object);
        }
    }
}
