// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarTotem
{
    public enum OID : uint
    {
        Boss = 0x2534, //R=5.06
        BossAdd = 0x2566, //R=2.2
        BossHelper = 0x233C,
        FireVoidzone = 0x1EA8BB,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        AutoAttack2 = 6499, // BossAdd->player, no cast, single-target
        FlurryOfRage = 13451, // Boss->self, 3,0s cast, range 8+R 120-degree cone
        WhorlOfFrenzy = 13453, // Boss->self, 3,0s cast, range 6+R circle
        WaveOfMalice = 13454, // Boss->location, 2,5s cast, range 5 circle
        TheWardensVerdict = 13739, // BossAdd->self, 3,0s cast, range 40+R width 4 rect
        FlamesOfFury = 13452, // Boss->location, 4,0s cast, range 10 circle
    };

    public enum IconID : uint
    {
        Baitaway = 23, // player
    };

    class FlurryOfRage : Components.SelfTargetedAOEs
    {
        public FlurryOfRage() : base(ActionID.MakeSpell(AID.FlurryOfRage), new AOEShapeCone(13.06f, 60.Degrees())) { }
    }

    class WaveOfMalice : Components.LocationTargetedAOEs
    {
        public WaveOfMalice() : base(ActionID.MakeSpell(AID.WaveOfMalice), 5) { }
    }

    class WhorlOfFrenzy : Components.SelfTargetedAOEs
    {
        public WhorlOfFrenzy() : base(ActionID.MakeSpell(AID.WhorlOfFrenzy), new AOEShapeCircle(11.06f)) { }
    }

    class TheWardensVerdict : Components.SelfTargetedAOEs
    {
        public TheWardensVerdict() : base(ActionID.MakeSpell(AID.TheWardensVerdict), new AOEShapeRect(45.06f, 2)) { }
    }

    class FlamesOfFury : Components.UniformStackSpread
    {
        public FlamesOfFury() : base(0, 10, alwaysShowSpreads: true) { }
        private bool targeted;
        private int numcasts;
        private Actor? target;
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Baitaway)
            {
                AddSpread(actor);
                targeted = true;
                target = actor;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.FlamesOfFury)
                ++numcasts;
            if (numcasts == 3)
            {
                Spreads.Clear();
                numcasts = 0;
                targeted = false;
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (target == actor && targeted)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, 18));
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (target == actor && targeted)
                hints.Add("Bait away (3 times!)");
        }
    }

    class FlamesOfFuryVoidzone : Components.PersistentVoidzoneAtCastTarget
    {
        public FlamesOfFuryVoidzone() : base(10, ActionID.MakeSpell(AID.FlamesOfFury), m => m.Enemies(OID.FireVoidzone).Where(z => z.EventState != 7), 0) { }
    }

    class HatiStates : StateMachineBuilder
    {
        public HatiStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<FlurryOfRage>()
                .ActivateOnEnter<WaveOfMalice>()
                .ActivateOnEnter<WhorlOfFrenzy>()
                .ActivateOnEnter<TheWardensVerdict>()
                .ActivateOnEnter<FlamesOfFury>()
                .ActivateOnEnter<FlamesOfFuryVoidzone>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 586, NameID = 7586)]
    public class Hati : BossModule
    {
        public Hati(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
