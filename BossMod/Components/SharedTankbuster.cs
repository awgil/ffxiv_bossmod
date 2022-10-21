namespace BossMod.Components
{
    // generic 'shared tankbuster' component; assumes only 1 concurrent cast is active
    // TODO: revise and improve (track invuln, ai hints, num stacked tanks?)
    public class SharedTankbuster : CastCounter
    {
        public AOEShape Shape { get; private init; }
        public bool OriginAtTarget { get; private init; }
        private Actor? _caster;
        private Actor? _target;

        public bool CastActive => _caster != null;

        // oriented shapes (cones/rects) typically have origin at caster
        public SharedTankbuster(ActionID aid, AOEShape shape, bool originAtTarget = false) : base(aid)
        {
            Shape = shape;
            OriginAtTarget = originAtTarget;
        }

        // circle shapes typically have origin at target
        public SharedTankbuster(ActionID aid, float radius) : this(aid, new AOEShapeCircle(radius), true) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_target == null)
                return;
            if (_target == actor)
            {
                hints.Add("Stack with other tanks or press invuln!");
            }
            else if (actor.Role == Role.Tank)
            {
                hints.Add("Stack with tank!", !InAOE(actor));
            }
            else
            {
                hints.Add("GTFO from tank!", InAOE(actor));
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (_caster != null && _target != null && _target != actor)
            {
                var shape = OriginAtTarget ? Shape.Distance(_target.Position, _target.Rotation) : Shape.Distance(_caster.Position, Angle.FromDirection(_target.Position - _caster.Position));
                if (actor.Role == Role.Tank)
                    hints.AddForbiddenZone(p => -shape(p), _caster.CastInfo!.FinishAt);
                else
                    hints.AddForbiddenZone(shape, _caster.CastInfo!.FinishAt);
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _target == player ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_caster != null && _target != null)
            {
                if (OriginAtTarget)
                    Shape.Outline(arena, _target);
                else
                    Shape.Outline(arena, _caster.Position, Angle.FromDirection(_target.Position - _caster.Position));
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _caster = caster;
                _target = module.WorldState.Actors.Find(spell.TargetID);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (caster == _caster)
                _caster = _target = null;
        }

        private bool InAOE(Actor actor) => _caster != null && _target != null && (OriginAtTarget ? Shape.Check(actor.Position, _target) : Shape.Check(actor.Position, _caster.Position, Angle.FromDirection(_target.Position - _caster.Position)));
    }
}
