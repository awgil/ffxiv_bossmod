namespace BossMod.Endwalker.TreasureHunt.Pithekos
{
    public enum OID : uint
    {
        Boss = 0x3D2B, //R=6
        BallOfLevin = 0x3E90,
        BossMikros = 0x3D2C, //R=4.2
        BossHelper = 0x233C,
    };

public enum AID : uint
{
    Attack = 872, // Boss->player, no cast, single-target
    Thundercall = 32212, // Boss->location, 2,5s cast, range 3 circle
    LightningBolt = 32214, // Boss->self, 3,0s cast, single-target
    LightningBolt2 = 32215, // BossHelper->location, 3,0s cast, range 6 circle
    ThunderIV = 32213, // BallOfLevin->self, 7,0s cast, range 18 circle
    Spark = 32216, // Boss->self, 4,0s cast, range 14-30 donut --> TODO: confirm inner circle size
    Attack2 = 870, // BossMikros->player, no cast, single-target
    RockThrow = 32217, // BossMikros->location, 3,0s cast, range 6 circle
    SweepingGouge = 32211, // Boss->player, 5,0s cast, single-target
};

public enum IconID : uint
{    
    AOE = 111, // player
    TankBuster = 218, // player
};

    class Spark : Components.SelfTargetedAOEs
    {
        public Spark() : base(ActionID.MakeSpell(AID.Spark), new AOEShapeDonut(13,30)) { } 
    }
    class SweepingGouge : Components.SingleTargetCast
    {
        public SweepingGouge() : base(ActionID.MakeSpell(AID.SweepingGouge)) { }
    }
    class Thundercall : Components.LocationTargetedAOEs
    {
        private int targeted;
        public Thundercall() : base(ActionID.MakeSpell(AID.Thundercall), 3) {}
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var player = module.Raid.Player();
            if(player == actor)
             switch ((IconID)iconID)
            {
                case IconID.AOE:
                    targeted = 1;
                    break;
            }
        }
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.BallOfLevin)
             targeted = 0;
        }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
             if (targeted > 0)
            {
                hints.Add("Drop the AOE at the edge of the arena, will spawn a huge AOE");
            }
        }

    }
        class RockThrow : Components.LocationTargetedAOEs
    {
        public RockThrow() : base(ActionID.MakeSpell(AID.RockThrow), 6) { }
    }
    class LightningBolt2 : Components.LocationTargetedAOEs
    {
        public LightningBolt2() : base(ActionID.MakeSpell(AID.LightningBolt2), 6) { }
    }
    class ThunderIV : Components.SelfTargetedAOEs
    {
        public ThunderIV() : base(ActionID.MakeSpell(AID.ThunderIV), new AOEShapeCircle(18)) { }
    }

    class PithekosStates : StateMachineBuilder
    {
        public PithekosStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<Spark>()
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<RockThrow>()
            .ActivateOnEnter<LightningBolt2>()
            .ActivateOnEnter<SweepingGouge>()
            .ActivateOnEnter<ThunderIV>();
        }
    }

    public class Pithekos : BossModule
    {
        public Pithekos(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) {}

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.BossMikros))
                Arena.Actor(s, ArenaColor.Object, false);
        }
    
    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BossMikros => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
