using System;
using System.Collections.Generic;

namespace BossMod.Components
{
    // generic attract/pull to source component
    public class AttractBetweenHitboxes : Knockback
    {
        public float MaxRange;
        public float MaxDistance;
        public float MinSafeDistance;
        public AOEShape? Shape;
        public Kind KnockbackKind;
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        private float PullDistance;
        private Angle Direction;
        private float DistanceToCaster;
        private bool casting;
        public AttractBetweenHitboxes(ActionID aid, float maxRange, float maxDistance, float minSafeDistance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null)
            : base(aid, ignoreImmunes, maxCasts)
        {
            MaxRange = maxRange;
            MaxDistance = maxDistance;
            MinSafeDistance = minSafeDistance;
            Shape = shape;
        }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
            {
                {
                    DistanceToCaster = (actor.Position - c.Position).Length();
                    PullDistance = Math.Clamp(MaxRange - (c.HitboxRadius + actor.HitboxRadius + (MaxRange - DistanceToCaster)), 0, MaxDistance);
                    Direction = Angle.FromDirection(actor.Position - c.Position);
                }
            if (DistanceToCaster <= MaxRange)
                yield return new(new(), PullDistance, default, Shape, Direction, Kind.TowardsOrigin);
            }
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        { //TODO: find a way to check if destination is unsafe instead of hardcoding this
            if (casting && DistanceToCaster <= MaxRange && DistanceToCaster - PullDistance < MinSafeDistance && Shape == null)
                hints.Add("About to be pulled into danger!");
            foreach (var s in Sources(module, slot, actor))
            {
                if (casting && DistanceToCaster - PullDistance < MinSafeDistance && Shape != null && Shape.Check(actor.Position, s.Origin, s.Direction))
                    hints.Add("About to be pulled into danger!");
            }
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                {
                _casters.Add(caster);
                casting = true;
                }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Remove(caster);
                casting = false;
            }
        }
    }
    // generic attract/pull to source (ignores hitboxes) component
    public class AttractToSource : Knockback
    {
        public float MaxRange;
        public float MaxDistance;
        public float MinSafeDistance;
        public AOEShape? Shape;
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        private float PullDistance;
        private Angle Direction;
        private float DistanceToCaster;
        private bool casting;
        public AttractToSource(ActionID aid, float maxRange, float maxDistance, float minSafeDistance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null)
            : base(aid, ignoreImmunes, maxCasts)
        {
            MaxRange = maxRange;
            MaxDistance = maxDistance;
            MinSafeDistance = minSafeDistance;
            Shape = shape;
        }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
            {
                {
                    DistanceToCaster = (actor.Position - c.Position).Length();
                    PullDistance = Math.Clamp(MaxRange - (MaxRange - DistanceToCaster), 0, MaxDistance);
                    Direction = Angle.FromDirection(actor.Position - c.Position);
                }
                if (DistanceToCaster <= MaxRange)
                    yield return new(new(), PullDistance, default, Shape, Direction, Kind.TowardsOrigin);
            }
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        { //TODO: find a way to check if destination is unsafe instead of hardcoding this
            if (casting && DistanceToCaster <= MaxRange && DistanceToCaster - PullDistance < MinSafeDistance && Shape == null)
                hints.Add("About to be pulled into danger!");
            foreach (var s in Sources(module, slot, actor))
            {
                if (casting && DistanceToCaster - PullDistance < MinSafeDistance && Shape != null && Shape.Check(actor.Position, s.Origin, s.Direction))
                    hints.Add("About to be pulled into danger!");
            }
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                {
                _casters.Add(caster);
                casting = true;
                }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Remove(caster);
                casting = false;
            }
        }
    }
}
