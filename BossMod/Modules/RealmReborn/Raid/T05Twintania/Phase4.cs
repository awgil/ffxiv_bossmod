using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Raid.T05Twintania
{
    // P4 mechanics
    class P4Twisters : BossComponent
    {
        private List<Actor> _twisters = new();
        private List<WPos> _predictedPositions = new();
        private IEnumerable<Actor> ActiveTwisters => _twisters.Where(t => t.EventState != 7);

        private const float PredictBeforeCastFinish = 0; // 0.5f
        private const float PredictAvoidRadius = 2; // 5
        private const float TwisterCushion = 1; // 1

        public override void Init(BossModule module)
        {
            _twisters = module.Enemies(OID.Twister);
        }

        public override void Update(BossModule module)
        {
            if (_predictedPositions.Count == 0 && (module.PrimaryActor.CastInfo?.IsSpell(AID.Twister) ?? false) && (module.PrimaryActor.CastInfo.FinishAt - module.WorldState.CurrentTime).TotalSeconds <= PredictBeforeCastFinish)
                _predictedPositions.AddRange(module.Raid.WithoutSlot().Select(a => a.Position));
            if (_twisters.Count > 0)
                _predictedPositions.Clear();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.Twister) ?? false)
                hints.Add("Move!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var p in _predictedPositions)
                hints.AddForbiddenZone(ShapeDistance.Circle(p, PredictAvoidRadius), module.PrimaryActor.CastInfo?.FinishAt ?? new());
            foreach (var t in ActiveTwisters)
                hints.AddForbiddenZone(ShapeDistance.Circle(t.Position, t.HitboxRadius + TwisterCushion));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var twister in ActiveTwisters)
                arena.AddCircle(twister.Position, twister.HitboxRadius, ArenaColor.Danger);
        }
    }

    class P4Dreadknights : BossComponent
    {
        private Actor? _target;
        private List<Actor> _dreadknights = new();
        public IEnumerable<Actor> ActiveDreadknights => _dreadknights.Where(a => !a.IsDead);

        public override void Init(BossModule module)
        {
            _dreadknights = module.Enemies(OID.Dreadknight);
        }

        public override void Update(BossModule module)
        {
            if (!ActiveDreadknights.Any())
                _target = null;
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (_target == null)
            {
                // until target is selected, stay away from any dreadknights
                foreach (var dk in ActiveDreadknights)
                {
                    hints.AddForbiddenZone(ShapeDistance.Circle(dk.Position, 15));
                }
            }
            else
            {
                // stun/slow dreadknight if possible
                foreach (var dk in ActiveDreadknights)
                {
                    hints.PlannedActions.Add((ActionID.MakeSpell(BRD.AID.LegGraze), dk, 5, false));
                    hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.LowBlow), dk, 5, false));
                    hints.PlannedActions.Add((ActionID.MakeSpell(DRG.AID.LegSweep), dk, 5, false));
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var a in ActiveDreadknights)
            {
                arena.Actor(a, ArenaColor.Enemy);
                if (_target != null)
                    arena.AddLine(a.Position, _target.Position, ArenaColor.Danger);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.UnwovenWill)
                _target = module.WorldState.Actors.Find(spell.MainTargetID);
        }
    }

    class P4AI : BossComponent
    {
        private DeathSentence? _deathSentence;

        public override void Init(BossModule module)
        {
            _deathSentence = module.FindComponent<DeathSentence>();
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var e in hints.PotentialTargets)
            {
                switch ((OID)e.Actor.OID)
                {
                    case OID.Boss:
                        e.Priority = 1;
                        e.DesiredPosition = new(-7, -15);
                        e.DesiredRotation = 180.Degrees();
                        break;
                    case OID.Dreadknight:
                        e.Priority = assignment != _deathSentence?.TankRole ? 2 : 0; // for current mt it is not worth switching to dreadknight, since it risks plummeting raid
                        e.ShouldBeTanked = false;
                        break;
                }
            }
        }
    }
}
