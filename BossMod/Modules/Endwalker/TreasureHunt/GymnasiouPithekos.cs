// CONTRIB: made by malediktus, not checked
namespace BossMod.Endwalker.TreasureHunt.GymnasiouPithekos
{
    public enum OID : uint
    {
        Boss = 0x3D2B, //R=6
        BallOfLevin = 0x3E90,
        BossAdd = 0x3D2C, //R=4.2
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
        AutoAttack2 = 870, // BossAdds->player, no cast, single-target
        RockThrow = 32217, // BossAdds->location, 3,0s cast, range 6 circle
        SweepingGouge = 32211, // Boss->player, 5,0s cast, single-target
    };

    public enum IconID : uint
    {
        Thundercall = 111, // Thundercall marker
    };

    class Spark : Components.SelfTargetedAOEs
    {
        public Spark() : base(ActionID.MakeSpell(AID.Spark), new AOEShapeDonut(14,30)) { } 
    }

    class SweepingGouge : Components.SingleTargetCast
    {
        public SweepingGouge() : base(ActionID.MakeSpell(AID.SweepingGouge)) { }
    }

   class Thundercall : Components.LocationTargetedAOEs
    {
        public Thundercall() : base(ActionID.MakeSpell(AID.Thundercall), 3) {}
    }

    class Thundercall2 : Components.UniformStackSpread
    {
        public Thundercall2() : base(0, 18, alwaysShowSpreads: true) { }
        private bool targeted;
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if(iconID == (uint)IconID.Thundercall)
                AddSpread(actor);
                targeted = true;
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Thundercall)
                Spreads.Clear();
                targeted = false;
        }
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
           var player = module.Raid.Player();
            if(player == actor && targeted)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, 18));
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
            .ActivateOnEnter<Thundercall2>()
            .ActivateOnEnter<RockThrow>()
            .ActivateOnEnter<LightningBolt2>()
            .ActivateOnEnter<SweepingGouge>()
            .ActivateOnEnter<ThunderIV>();
        }
    }

    [ModuleInfo(CFCID = 909, NameID = 12001)]
    public class Pithekos : BossModule
    {
        public Pithekos(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) {}

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.BossAdd => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
        }
    }
}
