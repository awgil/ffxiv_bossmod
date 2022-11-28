namespace BossMod.Endwalker.Criterion.C01ASS.C010Udumbara
{
    class HoneyedLeft : Components.SelfTargetedAOEs
    {
        public HoneyedLeft(ActionID aid) : base(aid, new AOEShapeCone(30, 90.Degrees())) { }
    }

    class HoneyedRight : Components.SelfTargetedAOEs
    {
        public HoneyedRight(ActionID aid) : base(aid, new AOEShapeCone(30, 90.Degrees())) { }
    }

    class HoneyedFront : Components.SelfTargetedAOEs
    {
        public HoneyedFront(ActionID aid) : base(aid, new AOEShapeCone(30, 60.Degrees())) { }
    }

    class BloodyCaress : Components.SelfTargetedAOEs
    {
        public BloodyCaress(ActionID aid) : base(aid, new AOEShapeCone(12, 60.Degrees())) { }
    }

    namespace Normal
    {
        public enum OID : uint
        {
            Boss = 0x3AD3, // R4.000, x1
            Sapria = 0x3AD4, // R1.440, x2
        };

        public enum AID : uint
        {
            AutoAttack = 31320, // Boss/Sapria->player, no cast, single-target
            HoneyedLeft = 31067, // Boss->self, 4.0s cast, range 30 180-degree cone
            HoneyedRight = 31068, // Boss->self, 4.0s cast, range 30 180-degree cone
            HoneyedFront = 31069, // Boss->self, 4.0s cast, range 30 120-degree cone
            BloodyCaress = 31071, // Sapria->self, 3.0s cast, range 12 120-degree cone
        };

        class NHoneyedLeft : HoneyedLeft { public NHoneyedLeft() : base(ActionID.MakeSpell(AID.HoneyedLeft)) { } }
        class NHoneyedRight : HoneyedRight { public NHoneyedRight() : base(ActionID.MakeSpell(AID.HoneyedRight)) { } }
        class NHoneyedFront : HoneyedFront { public NHoneyedFront() : base(ActionID.MakeSpell(AID.HoneyedFront)) { } }
        class NBloodyCaress : BloodyCaress { public NBloodyCaress() : base(ActionID.MakeSpell(AID.BloodyCaress)) { } }

        class C010NUdumbaraStates : StateMachineBuilder
        {
            public C010NUdumbaraStates(BossModule module) : base(module)
            {
                TrivialPhase()
                    .ActivateOnEnter<NHoneyedLeft>()
                    .ActivateOnEnter<NHoneyedRight>()
                    .ActivateOnEnter<NHoneyedFront>()
                    .ActivateOnEnter<NBloodyCaress>();
            }
        }

        public class C010NUdumbara : SimpleBossModule
        {
            public C010NUdumbara(WorldState ws, Actor primary) : base(ws, primary) { }

            protected override void DrawEnemies(int pcSlot, Actor pc)
            {
                Arena.Actor(PrimaryActor, ArenaColor.Enemy);
                Arena.Actors(Enemies(OID.Sapria), ArenaColor.Enemy);
            }
        }
    }

    namespace Savage
    {
        public enum OID : uint
        {
            Boss = 0x3ADC, // R4.000, x1
            Sapria = 0x3ADD, // R1.440, x2
        };

        public enum AID : uint
        {
            AutoAttack = 31320, // Boss/Sapria->player, no cast, single-target
            HoneyedLeft = 31091, // Boss->self, 4.0s cast, range 30 180-degree cone
            HoneyedRight = 31092, // Boss->self, 4.0s cast, range 30 180-degree cone
            HoneyedFront = 31093, // Boss->self, 4.0s cast, range 30 120-degree cone
            BloodyCaress = 31095, // Sapria->self, 3.0s cast, range 12 120-degree cone
        };

        class SHoneyedLeft : HoneyedLeft { public SHoneyedLeft() : base(ActionID.MakeSpell(AID.HoneyedLeft)) { } }
        class SHoneyedRight : HoneyedRight { public SHoneyedRight() : base(ActionID.MakeSpell(AID.HoneyedRight)) { } }
        class SHoneyedFront : HoneyedFront { public SHoneyedFront() : base(ActionID.MakeSpell(AID.HoneyedFront)) { } }
        class SBloodyCaress : BloodyCaress { public SBloodyCaress() : base(ActionID.MakeSpell(AID.BloodyCaress)) { } }

        class C010SUdumbaraStates : StateMachineBuilder
        {
            public C010SUdumbaraStates(BossModule module) : base(module)
            {
                TrivialPhase()
                    .ActivateOnEnter<SHoneyedLeft>()
                    .ActivateOnEnter<SHoneyedRight>()
                    .ActivateOnEnter<SHoneyedFront>()
                    .ActivateOnEnter<SBloodyCaress>();
            }
        }

        public class C010SUdumbara : SimpleBossModule
        {
            public C010SUdumbara(WorldState ws, Actor primary) : base(ws, primary) { }

            protected override void DrawEnemies(int pcSlot, Actor pc)
            {
                Arena.Actor(PrimaryActor, ArenaColor.Enemy);
                Arena.Actors(Enemies(OID.Sapria), ArenaColor.Enemy);
            }
        }
    }
}
