using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // legacy, these components should be reviewed and moved into Components dir/ns
    public static class CommonComponents
    {
        // generic 'shared tankbuster' component that shows radius around cast target; assumes only 1 concurrent cast is active
        public class SharedTankbuster : BossComponent
        {
            private float _radius;
            private ActionID _watchedAction;
            private Actor? _target;

            public SharedTankbuster(ActionID aid, float radius)
            {
                _radius = radius;
                _watchedAction = aid;
            }

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
                    hints.Add("Stack with tank!", !actor.Position.InCircle(_target.Position, _radius));
                }
                else
                {
                    hints.Add("GTFO from tank!", actor.Position.InCircle(_target.Position, _radius));
                }
            }

            public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
            {
                return _target == player ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                if (_target != null)
                    arena.AddCircle(_target.Position, _radius, ArenaColor.Danger);
            }

            public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if (spell.Action == _watchedAction)
                    _target = module.WorldState.Actors.Find(spell.TargetID);
            }

            public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if (spell.Action == _watchedAction)
                    _target = null;
            }
        }

        // generic 'stack to target' component that shows radius around specific actor; derived class should set actor as needed
        // it is also a cast counter
        public class FullPartyStack : Components.CastCounter
        {
            protected float StackRadius;
            protected Actor? Target;

            public FullPartyStack(ActionID aid, float stackRadius)
                : base(aid)
            {
                StackRadius = stackRadius;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (Target != null && Target != actor && !actor.Position.InCircle(Target.Position, StackRadius))
                    hints.Add("Stack!");
            }

            public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
            {
                if (Target == null)
                {
                    return PlayerPriority.Irrelevant;
                }
                else if (Target == pc)
                {
                    // other players are somewhat interesting to simplify stacking
                    return player.Position.InCircle(pc.Position, StackRadius) ? PlayerPriority.Normal : PlayerPriority.Interesting;
                }
                else
                {
                    // draw target next to which pc is to stack
                    return Target == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
                }
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                if (Target != null)
                    arena.AddCircle(Target.Position, StackRadius, ArenaColor.Danger);
            }
        }

        // generic 'spread from target' component that shows circles around actors that are target of a specific cast
        public class SpreadFromCastTargets : BossComponent
        {
            private float _spreadRadius;
            private ActionID _watchedAction;
            private List<(Actor Caster, Actor Target)> _active = new();

            public SpreadFromCastTargets(ActionID aid, float spreadRadius)
            {
                _spreadRadius = spreadRadius;
                _watchedAction = aid;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (_active.Any(e => e.Target != actor && e.Target.Position.InCircle(actor.Position, _spreadRadius)))
                    hints.Add("GTFO from aoe target!");
            }

            public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
            {
                return _active.Any(ct => ct.Target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                foreach (var e in _active)
                {
                    arena.AddCircle(e.Target.Position, _spreadRadius, ArenaColor.Danger);
                }
            }

            public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if (spell.Action == _watchedAction)
                {
                    var target = module.WorldState.Actors.Find(spell.TargetID);
                    if (target != null)
                    {
                        _active.Add((caster, target));
                    }
                }
            }

            public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if (spell.Action == _watchedAction)
                {
                    _active.RemoveAll(e => e.Caster == caster);
                }
            }
        }

        // generic knockback from caster component (TODO: detect knockback immunity, generalize...)
        public class KnockbackFromCaster : Components.CastCounter
        {
            public float Distance { get; private init; }
            public int MaxCasts { get; private init; } // used for staggered knockbacks, when showing all active would be pointless
            private List<Actor> _casters = new();
            public IReadOnlyList<Actor> Casters => _casters;
            public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

            public KnockbackFromCaster(ActionID aid, float distance, int maxCasts = 1)
                : base(aid)
            {
                Distance = distance;
                MaxCasts = maxCasts;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
            {
                if (ActiveCasters.Any(c => !module.Bounds.Contains(BossModule.AdjustPositionForKnockback(actor.Position, c, Distance))))
                    hints.Add("About to be knocked into wall!");
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                foreach (var c in ActiveCasters)
                {
                    var adjPos = BossModule.AdjustPositionForKnockback(pc.Position, c, Distance);
                    arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
                    arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
                }
            }

            public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if (spell.Action == WatchedAction)
                    _casters.Add(caster);
            }

            public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if (spell.Action == WatchedAction)
                    _casters.Remove(caster);
            }
        }

        // generic interrupt hint component
        public class Interruptible : BossComponent
        {
            private ActionID _watchedAction;
            private List<Actor> _casters = new();

            public Interruptible(ActionID aid)
            {
                _watchedAction = aid;
            }

            public override void AddGlobalHints(BossModule module, GlobalHints hints)
            {
                if (_casters.Count > 0)
                {
                    hints.Add("Interrupt!");
                }
            }

            public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if (spell.Action == _watchedAction)
                {
                    _casters.Add(caster);
                }
            }

            public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if (spell.Action == _watchedAction)
                {
                    _casters.Remove(caster);
                }
            }
        }
    }
}
