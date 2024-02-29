// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarChimera
{
    public enum OID : uint
    {
        Boss = 0x2539, //R=5.92
        BossAdd = 0x256A, //R=2.07
        BossHelper = 0x233C,
        IceVoidzone = 0x1E8D9C,
    };

    public enum AID : uint
    {
        AutoAttack = 870, // 2539->player, no cast, single-target
        AutoAttack2 = 6499, // 256A->player, no cast, single-target
        TheScorpionsSting = 13393, // 2539->self, 3,5s cast, range 6+R 90-degree cone
        TheRamsVoice = 13394, // 2539->self, 5,0s cast, range 4+R circle, interruptible, deep freeze + frostbite
        TheLionsBreath = 13392, // 2539->self, 3,5s cast, range 6+R 120-degree cone, burn
        LanguorousGaze = 13742, // 256A->self, 3,0s cast, range 6+R 90-degree cone
        TheRamsKeeper = 13396, // 2539->location, 3,5s cast, range 6 circle, voidzone
        TheDragonsVoice = 13395, // 2539->self, 5,0s cast, range 8-30 donut, interruptible, paralaysis
    };

    public enum IconID : uint
    {
        Baitaway = 23, // player
    };

    class TheScorpionsSting : Components.SelfTargetedAOEs
    {
        public TheScorpionsSting() : base(ActionID.MakeSpell(AID.TheScorpionsSting), new AOEShapeCone(11.92f, 45.Degrees())) { }
    }

    class TheRamsVoice : Components.SelfTargetedAOEs
    {
        public TheRamsVoice() : base(ActionID.MakeSpell(AID.TheRamsVoice), new AOEShapeCircle(9.92f)) { }
    }

    class TheRamsVoiceHint : Components.CastHint
    {
        public TheRamsVoiceHint() : base(ActionID.MakeSpell(AID.TheRamsVoice), "Interrupt!") { }
    }

    class TheLionsBreath : Components.SelfTargetedAOEs
    {
        public TheLionsBreath() : base(ActionID.MakeSpell(AID.TheLionsBreath), new AOEShapeCone(11.92f, 60.Degrees())) { }
    }

    class LanguorousGaze : Components.SelfTargetedAOEs
    {
        public LanguorousGaze() : base(ActionID.MakeSpell(AID.LanguorousGaze), new AOEShapeCone(8.07f, 45.Degrees())) { }
    }

    class TheDragonsVoice : Components.SelfTargetedAOEs
    {
        public TheDragonsVoice() : base(ActionID.MakeSpell(AID.TheDragonsVoice), new AOEShapeDonut(8, 30)) { }
    }

    class TheDragonsVoiceHint : Components.CastHint
    {
        public TheDragonsVoiceHint() : base(ActionID.MakeSpell(AID.TheDragonsVoice), "Interrupt!") { }
    }

    class TheRamsKeeper : Components.UniformStackSpread
    {
        public TheRamsKeeper() : base(0, 6, alwaysShowSpreads: true) { }
        private bool targeted;
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
            if ((AID)spell.Action.ID == AID.TheRamsKeeper)
            {
                Spreads.Clear();
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
                hints.Add("Bait away!");
        }
    }

    class IceVoidzone : Components.PersistentVoidzoneAtCastTarget
    {
        public IceVoidzone() : base(6, ActionID.MakeSpell(AID.TheRamsKeeper), m => m.Enemies(OID.IceVoidzone), 0) { }
    }

    class ChimeraStates : StateMachineBuilder
    {
        public ChimeraStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<TheScorpionsSting>()
                .ActivateOnEnter<TheRamsVoice>()
                .ActivateOnEnter<TheRamsVoiceHint>()
                .ActivateOnEnter<TheDragonsVoice>()
                .ActivateOnEnter<TheDragonsVoiceHint>()
                .ActivateOnEnter<TheLionsBreath>()
                .ActivateOnEnter<LanguorousGaze>()
                .ActivateOnEnter<TheRamsKeeper>()
                .ActivateOnEnter<IceVoidzone>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 586, NameID = 7591)]
    public class Chimera : BossModule
    {
        public Chimera(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
