namespace BossMod.Endwalker.Criterion.C01ASS.C010Dryad
{
    public enum OID : uint
    {
        Boss = 0x3AD1, // R3.000, x1
        Odqan = 0x3AD2, // R1.050, x2
    };

    public enum AID : uint
    {
        AutoAttack = 31320, // Boss/Odqan->player, no cast, single-target
        ArborealStorm = 31063, // Boss->self, 5.0s cast, range 12 circle
        AcornBomb = 31064, // Boss->location, 3.0s cast, range 6 circle
        GelidGale = 31065, // Odqan->location, 3.0s cast, range 6 circle
        Uproot = 31066, // Odqan->self, 3.0s cast, range 6 circle
    };

    class ArborealStorm : Components.SelfTargetedAOEs
    {
        public ArborealStorm() : base(ActionID.MakeSpell(AID.ArborealStorm), new AOEShapeCircle(12)) { }
    }

    class AcornBomb : Components.LocationTargetedAOEs
    {
        public AcornBomb() : base(ActionID.MakeSpell(AID.AcornBomb), 6) { }
    }

    class GelidGale : Components.LocationTargetedAOEs
    {
        public GelidGale() : base(ActionID.MakeSpell(AID.GelidGale), 6) { }
    }

    class Uproot : Components.SelfTargetedAOEs
    {
        public Uproot() : base(ActionID.MakeSpell(AID.Uproot), new AOEShapeCircle(6)) { }
    }

    class C010DryadStates : StateMachineBuilder
    {
        public C010DryadStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<ArborealStorm>()
                .ActivateOnEnter<AcornBomb>()
                .ActivateOnEnter<GelidGale>()
                .ActivateOnEnter<Uproot>();
        }
    }

    public class C010Dryad : SimpleBossModule
    {
        public C010Dryad(WorldState ws, Actor primary) : base(ws, primary) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.Odqan), ArenaColor.Enemy);
        }
    }
}
