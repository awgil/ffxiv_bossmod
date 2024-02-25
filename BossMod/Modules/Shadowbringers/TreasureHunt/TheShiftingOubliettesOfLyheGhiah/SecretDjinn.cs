// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretDjinn
{
    public enum OID : uint
    {
        Boss = 0x300F, //R=3.48
        BossAdd = 0x3010, //R=1.32
        BossHelper = 0x233C,
    };

    public enum AID : uint
    {
        AutoAttack = 23185, // Boss->player, no cast, single-target
        AutoAttack2 = 872, // BossAdd->player, no cast, single-target
        Gust = 21655, // Boss->location, 3,0s cast, range 6 circle
        ChangelessWinds = 21657, // Boss->self, 3,0s cast, range 40 width 8 rect
        WhirlingGaol = 21654, // Boss->self, 4,0s cast, range 40 circle
        Whipwind = 21656, // Boss->self, 5,0s cast, range 55 width 40 rect
        GentleBreeze = 21653, // BossAdd->self, 3,0s cast, range 15 width 4 rect
    };

    class Gust : Components.LocationTargetedAOEs
    {
        public Gust() : base(ActionID.MakeSpell(AID.Gust), 6) { }
    }

    class ChangelessWinds : Components.SelfTargetedAOEs
    {
        public ChangelessWinds() : base(ActionID.MakeSpell(AID.ChangelessWinds), new AOEShapeRect(40, 4)) { }
    }

    class Whipwind : Components.SelfTargetedAOEs
    {
        public Whipwind() : base(ActionID.MakeSpell(AID.Whipwind), new AOEShapeRect(55, 20)) { }
    }

    class GentleBreeze : Components.SelfTargetedAOEs
    {
        public GentleBreeze() : base(ActionID.MakeSpell(AID.GentleBreeze), new AOEShapeRect(15, 2)) { }
    }

    class WhirlingGaol : Components.RaidwideCast
    {
        public WhirlingGaol() : base(ActionID.MakeSpell(AID.WhirlingGaol)) { }
    }

    class DjinnStates : StateMachineBuilder
    {
        public DjinnStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Gust>()
                .ActivateOnEnter<ChangelessWinds>()
                .ActivateOnEnter<Whipwind>()
                .ActivateOnEnter<GentleBreeze>()
                .ActivateOnEnter<WhirlingGaol>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 745, NameID = 9788)]
    public class Djinn : BossModule
    {
        public Djinn(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.BossAdd))
                Arena.Actor(s, ArenaColor.Object);
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
