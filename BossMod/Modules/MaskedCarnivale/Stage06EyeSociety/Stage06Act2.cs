using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage06.Act2
{
    public enum OID : uint
    {
        Boss = 0x26FF, //R=2.53
        Eye = 0x25CE, //R=1.35
        Mandragora = 0x2701, //R=0.3
    };

    public enum AID : uint
    {
        TearyTwirl = 14693, // 2701->self, 3,0s cast, range 6+R circle
        DemonEye = 14691, // 26FF->self, 5,0s cast, range 50+R circle
        Attack = 6499, // /26FF/2701->player, no cast, single-target
        ColdStare = 14692, // 26FF->self, 2,5s cast, range 40+R 90-degree cone
        Stone = 14695, // 25CE->player, 1,0s cast, single-target
        DreadGaze = 14694, // 25CE->self, 3,0s cast, range 6+R ?-degree cone

    };
    public enum SID : uint
    {
        Blind = 571, // Mandragora->player, extra=0x0

    };
    class DemonEye : CastGaze
    {
        private BitMask _blinded;

        public DemonEye() : base(ActionID.MakeSpell(AID.DemonEye)) { }

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

    class ColdStare : SelfTargetedAOEs //TODO: cone based gaze
    {
        private BitMask _blinded;

        public ColdStare() : base(ActionID.MakeSpell(AID.ColdStare), new AOEShapeCone(42.53f, 45.Degrees())) { }

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

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _blinded[slot] ? Enumerable.Empty<AOEInstance>() : base.ActiveAOEs(module, slot, actor);
        }
    }

    class TearyTwirl : StackWithCastTargets
    {
        private BitMask _blinded;

        public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), 6.3f) { }

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

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_blinded[slot])
                hints.Add("Kill mandragoras last incase you need to get blinded again.", false);
            if (!_blinded[slot])
                hints.Add("Stack to get blinded!", false);
        }
    }

    class DreadGaze : SelfTargetedAOEs //TODO: cone based gaze
    {
        private BitMask _blinded;

        public DreadGaze() : base(ActionID.MakeSpell(AID.DreadGaze), new AOEShapeCone(7.35f, 45.Degrees())) { }

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

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _blinded[slot] ? Enumerable.Empty<AOEInstance>() : base.ActiveAOEs(module, slot, actor);
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("The eyes are weak to lightning spells.");
        }
    }

    class Stage06Act2States : StateMachineBuilder
    {
        public Stage06Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<ColdStare>()
                .ActivateOnEnter<DreadGaze>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Mandragora).All(e => e.IsDead) && module.Enemies(OID.Eye).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 616, NameID = 8092)]
    public class Stage06Act2 : BossModule
    {
        public Stage06Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<DemonEye>();
            ActivateComponent<TearyTwirl>();
            ActivateComponent<Hints>();
        }

        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Mandragora).Any(e => e.InCombat) || Enemies(OID.Eye).Any(e => e.InCombat); }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Eye))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Mandragora))
                Arena.Actor(s, ArenaColor.Object, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss or OID.Eye => 1,
                    OID.Mandragora => 0,
                    _ => 0
                };
            }
        }
    }
}
