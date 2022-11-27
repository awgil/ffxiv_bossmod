namespace BossMod.Endwalker.Criterion.C01ASS.C010Udumbara
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

    class HoneyedLeft : Components.SelfTargetedAOEs
    {
        public HoneyedLeft() : base(ActionID.MakeSpell(AID.HoneyedLeft), new AOEShapeCone(30, 90.Degrees())) { }
    }

    class HoneyedRight : Components.SelfTargetedAOEs
    {
        public HoneyedRight() : base(ActionID.MakeSpell(AID.HoneyedRight), new AOEShapeCone(30, 90.Degrees())) { }
    }

    class HoneyedFront : Components.SelfTargetedAOEs
    {
        public HoneyedFront() : base(ActionID.MakeSpell(AID.HoneyedFront), new AOEShapeCone(30, 60.Degrees())) { }
    }

    class BloodyCaress : Components.SelfTargetedAOEs
    {
        public BloodyCaress() : base(ActionID.MakeSpell(AID.BloodyCaress), new AOEShapeCone(12, 60.Degrees())) { }
    }

    class C010UdumbaraStates : StateMachineBuilder
    {
        public C010UdumbaraStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<HoneyedLeft>()
                .ActivateOnEnter<HoneyedRight>()
                .ActivateOnEnter<HoneyedFront>()
                .ActivateOnEnter<BloodyCaress>();
        }
    }

    public class C010Udumbara : SimpleBossModule
    {
        public C010Udumbara(WorldState ws, Actor primary) : base(ws, primary) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actors(Enemies(OID.Sapria), ArenaColor.Enemy);
        }
    }
}
