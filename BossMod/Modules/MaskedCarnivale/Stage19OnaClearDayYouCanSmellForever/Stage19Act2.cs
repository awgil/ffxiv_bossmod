using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage19.Act2
{
    public enum OID : uint
    {
        Boss = 0x2728, //R=5.775
        HotHip = 0x2779, //R=1.50
        voidzone = 0x1EA9F9, //R=0.5
    };

    public enum AID : uint
    {
        Reflect = 15073, // 2728->self, 3,0s cast, single-target, boss starts reflecting all melee attacks
        AutoAttack = 6499, // 2728->player, no cast, single-target
        VineProbe = 15075, // 2728->self, 2,5s cast, range 6+R width 8 rect
        OffalBreath = 15076, // 2728->location, 3,5s cast, range 6 circle
        Schizocarps = 15077, // 2728->self, 5,0s cast, single-target
        ExplosiveDehiscence = 15078, // 2729->self, 6,0s cast, range 50 circle, gaze
        BadBreath = 15074, // 2728->self, 3,5s cast, range 12+R 120-degree cone, interruptible, voidzone
    };

    public enum SID : uint
    {
        Reflect = 518, // Boss->Boss, extra=0x0
        Paralysis = 17, // Boss->player, extra=0x0
        Silence = 7, // Boss->player, extra=0x0
        Blind = 15, // Boss->player, extra=0x0
        Slow = 9, // Boss->player, extra=0x0
        Heavy = 14, // Boss->player, extra=0x32
        Nausea = 2388, // Boss->player, extra=0x0
        Poison = 18, // Boss->player, extra=0x0
        Leaden = 67, // none->player, extra=0x3C
        Pollen = 19, // none->player, extra=0x0
        Stun = 149, // 2729->player, extra=0x0
    };

    class ExplosiveDehiscence : CastGaze
    {
        public static BitMask _blinded;

        public ExplosiveDehiscence() : base(ActionID.MakeSpell(AID.ExplosiveDehiscence)) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Blind)
                _blinded.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Blind)
                _blinded.Clear(module.Raid.FindSlot(actor.InstanceID));
        }

        public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor)
        {
            return _blinded[slot] ? Enumerable.Empty<Eye>() : base.ActiveEyes(module, slot, actor);
        }
    }

    class GazeHint : BossComponent
    {
        public static bool casting;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!ExplosiveDehiscence._blinded[slot] && casting)
                hints.Add("Cast Ink Jet on boss to get blinded!");
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Schizocarps)
                casting = true;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.ExplosiveDehiscence)
                casting = false;
        }
    }

    class Reflect : BossComponent
    {
        private bool reflect;
        private bool casting;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Reflect)
                casting = true;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Reflect)
            {
                reflect = true;
                casting = false;
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (casting)
                hints.Add("Boss will reflect all magic damage!");
            if (reflect)
                hints.Add("Boss reflects all magic damage!");//TODO: could use an AI hint to never use magic abilities after this is casted
        }
    }

    class BadBreath : SelfTargetedAOEs
    {
        public BadBreath() : base(ActionID.MakeSpell(AID.BadBreath), new AOEShapeCone(17.775f, 60.Degrees())) { }
    }

    class VineProbe : SelfTargetedAOEs
    {
        public VineProbe() : base(ActionID.MakeSpell(AID.VineProbe), new AOEShapeRect(11.775f, 4)) { }
    }

    class OffalBreath : PersistentVoidzoneAtCastTarget
    {
        public OffalBreath() : base(6, ActionID.MakeSpell(AID.OffalBreath), m => m.Enemies(OID.voidzone), 0) { }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Same as first act, but this time the boss will cast a gaze from all directions.\nThe easiest counter for this is to blind yourself by casting Ink Jet on the\nboss after it casted Schizocarps.\nThe Final Sting combo window opens at around 75% health.\n(Off-guard->Bristle->Moonflute->Final Sting)");
        }
    }

    class Stage19Act1States : StateMachineBuilder
    {
        public Stage19Act1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<Reflect>()
                .ActivateOnEnter<BadBreath>()
                .ActivateOnEnter<VineProbe>()
                .ActivateOnEnter<ExplosiveDehiscence>()
                .ActivateOnEnter<GazeHint>()
                .ActivateOnEnter<OffalBreath>();
        }
    }

    [ModuleInfo(CFCID = 629, NameID = 8117)]
    public class Stage19Act1 : BossModule
    {
        public Stage19Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
        {
            ActivateComponent<Hints>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.HotHip))
                Arena.Actor(s, ArenaColor.Enemy, true);
        }
    }
}
