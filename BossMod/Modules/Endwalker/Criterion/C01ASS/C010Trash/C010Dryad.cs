namespace BossMod.Endwalker.Criterion.C01ASS.C010Dryad
{
    class ArborealStorm : Components.SelfTargetedAOEs
    {
        public ArborealStorm(ActionID aid) : base(aid, new AOEShapeCircle(12)) { }
    }

    class AcornBomb : Components.LocationTargetedAOEs
    {
        public AcornBomb(ActionID aid) : base(aid, 6) { }
    }

    class GelidGale : Components.LocationTargetedAOEs
    {
        public GelidGale(ActionID aid) : base(aid, 6) { }
    }

    class Uproot : Components.SelfTargetedAOEs
    {
        public Uproot(ActionID aid) : base(aid, new AOEShapeCircle(6)) { }
    }

    namespace Normal
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

        class NArborealStorm : ArborealStorm { public NArborealStorm() : base(ActionID.MakeSpell(AID.ArborealStorm)) { } }
        class NAcornBomb : AcornBomb { public NAcornBomb() : base(ActionID.MakeSpell(AID.AcornBomb)) { } }
        class NGelidGale : GelidGale { public NGelidGale() : base(ActionID.MakeSpell(AID.GelidGale)) { } }
        class NUproot : Uproot { public NUproot() : base(ActionID.MakeSpell(AID.Uproot)) { } }

        class C010NDryadStates : StateMachineBuilder
        {
            public C010NDryadStates(BossModule module) : base(module)
            {
                TrivialPhase()
                    .ActivateOnEnter<NArborealStorm>()
                    .ActivateOnEnter<NAcornBomb>()
                    .ActivateOnEnter<NGelidGale>()
                    .ActivateOnEnter<NUproot>();
            }
        }

        public class C010NDryad : SimpleBossModule
        {
            public C010NDryad(WorldState ws, Actor primary) : base(ws, primary) { }

            protected override void DrawEnemies(int pcSlot, Actor pc)
            {
                Arena.Actor(PrimaryActor, ArenaColor.Enemy);
                Arena.Actors(Enemies(OID.Odqan), ArenaColor.Enemy);
            }
        }
    }

    namespace Savage
    {
        public enum OID : uint
        {
            Boss = 0x3ADA, // R3.000, x1
            Odqan = 0x3ADB, // R1.050, x2
        };

        public enum AID : uint
        {
            AutoAttack = 31320, // Boss/Odqan->player, no cast, single-target
            ArborealStorm = 31087, // Boss->self, 5.0s cast, range 12 circle
            AcornBomb = 31088, // Boss->location, 3.0s cast, range 6 circle
            GelidGale = 31089, // Odqan->location, 3.0s cast, range 6 circle
            Uproot = 31090, // Odqan->self, 3.0s cast, range 6 circle
        };

        class NArborealStorm : ArborealStorm { public NArborealStorm() : base(ActionID.MakeSpell(AID.ArborealStorm)) { } }
        class NAcornBomb : AcornBomb { public NAcornBomb() : base(ActionID.MakeSpell(AID.AcornBomb)) { } }
        class NGelidGale : GelidGale { public NGelidGale() : base(ActionID.MakeSpell(AID.GelidGale)) { } }
        class NUproot : Uproot { public NUproot() : base(ActionID.MakeSpell(AID.Uproot)) { } }

        class C010NDryadStates : StateMachineBuilder
        {
            public C010NDryadStates(BossModule module) : base(module)
            {
                TrivialPhase()
                    .ActivateOnEnter<NArborealStorm>()
                    .ActivateOnEnter<NAcornBomb>()
                    .ActivateOnEnter<NGelidGale>()
                    .ActivateOnEnter<NUproot>();
            }
        }

        public class C010NDryad : SimpleBossModule
        {
            public C010NDryad(WorldState ws, Actor primary) : base(ws, primary) { }

            protected override void DrawEnemies(int pcSlot, Actor pc)
            {
                Arena.Actor(PrimaryActor, ArenaColor.Enemy);
                Arena.Actors(Enemies(OID.Odqan), ArenaColor.Enemy);
            }
        }
    }
}
