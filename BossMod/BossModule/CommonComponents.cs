using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public static class CommonComponents
    {
        // generic component that counts specified casts
        public class CastCounter : BossModule.Component
        {
            public int NumCasts { get; private set; } = 0;

            private ActionID _watchedCastID;

            public CastCounter(ActionID aid)
            {
                _watchedCastID = aid;
            }

            public override void OnEventCast(BossModule module, CastEvent info)
            {
                if (info.Action == _watchedCastID)
                {
                    ++NumCasts;
                }
            }
        }

        // generic 'shared tankbuster' component that shows radius around cast target; assumes only 1 concurrent cast is active
        public class SharedTankbuster : BossModule.Component
        {
            private float _radius;
            private ActionID _watchedAction;
            private Actor? _target;

            public SharedTankbuster(ActionID aid, float radius)
            {
                _radius = radius;
                _watchedAction = aid;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
            {
                if (_target == null)
                    return;
                if (_target == actor)
                {
                    hints.Add("Stack with other tanks or press invuln!");
                }
                else if (actor.Role == Role.Tank)
                {
                    hints.Add("Stack with tank!", !GeometryUtils.PointInCircle(actor.Position - _target.Position, _radius));
                }
                else
                {
                    hints.Add("GTFO from tank!", GeometryUtils.PointInCircle(actor.Position - _target.Position, _radius));
                }
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                if (_target != null)
                {
                    arena.Actor(_target, arena.ColorDanger);
                    arena.AddCircle(_target.Position, _radius, arena.ColorDanger);
                }
            }

            public override void OnCastStarted(BossModule module, Actor actor)
            {
                if (actor.CastInfo!.Action == _watchedAction)
                    _target = module.WorldState.Actors.Find(actor.CastInfo.TargetID);
            }

            public override void OnCastFinished(BossModule module, Actor actor)
            {
                if (actor.CastInfo!.Action == _watchedAction)
                    _target = null;
            }
        }

        // generic 'stack to target' component that shows radius around specific actor; derived class should set actor as needed
        // it is also a cast counter
        public class FullPartyStack : CastCounter
        {
            protected float StackRadius;
            protected Actor? Target;

            public FullPartyStack(ActionID aid, float stackRadius)
                : base(aid)
            {
                StackRadius = stackRadius;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
            {
                if (Target != null && Target != actor && !GeometryUtils.PointInCircle(actor.Position - Target.Position, StackRadius))
                    hints.Add("Stack!");
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                if (Target == null)
                    return;

                arena.AddCircle(Target.Position, StackRadius, arena.ColorDanger);
                if (Target == pc)
                {
                    // draw other players to simplify stacking
                    foreach (var a in module.Raid.WithoutSlot().Exclude(pc))
                        arena.Actor(a, GeometryUtils.PointInCircle(a.Position - Target.Position, StackRadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
                else
                {
                    // draw target next to which pc is to stack
                    arena.Actor(Target, arena.ColorDanger);
                }
            }
        }

        // generic 'spread from target' component that shows circles around actors that are target of a specific cast
        public class SpreadFromCastTargets : BossModule.Component
        {
            private float _spreadRadius;
            private ActionID _watchedAction;
            private List<(Actor Caster, Actor Target)> _active = new();

            public SpreadFromCastTargets(ActionID aid, float spreadRadius)
            {
                _spreadRadius = spreadRadius;
                _watchedAction = aid;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
            {
                if (_active.Any(e => e.Target != actor && GeometryUtils.PointInCircle(e.Target.Position - actor.Position, _spreadRadius)))
                    hints.Add("GTFO from aoe target!");
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                foreach (var e in _active)
                {
                    arena.Actor(e.Target, arena.ColorDanger);
                    arena.AddCircle(e.Target.Position, _spreadRadius, arena.ColorDanger);
                }
            }

            public override void OnCastStarted(BossModule module, Actor actor)
            {
                if (actor.CastInfo!.Action == _watchedAction)
                {
                    var target = module.WorldState.Actors.Find(actor.CastInfo.TargetID);
                    if (target != null)
                    {
                        _active.Add((actor, target));
                    }
                }
            }

            public override void OnCastFinished(BossModule module, Actor actor)
            {
                if (actor.CastInfo!.Action == _watchedAction)
                {
                    _active.RemoveAll(e => e.Caster == actor);
                }
            }
        }

        // generic 'puddles' component that shows circle aoes for casters that target specific location
        public class Puddles : BossModule.Component
        {
            public bool Done { get; private set; } // set when currently there are no casters, but at least 1 cast was started before
            private ActionID _watchedAction;
            private AOEShapeCircle _aoe;
            private List<Actor> _casters = new();

            public Puddles(ActionID aid, float radius)
            {
                _watchedAction = aid;
                _aoe = new(radius);
            }

            public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
            {
                if (_casters.Any(c => _aoe.Check(actor.Position, c.CastInfo!.Location, 0)))
                    hints.Add("GTFO from puddle!");
            }

            public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                foreach (var c in _casters)
                    _aoe.Draw(arena, c.CastInfo!.Location, 0);
            }

            public override void OnCastStarted(BossModule module, Actor actor)
            {
                if (actor.CastInfo!.Action == _watchedAction)
                {
                    _casters.Add(actor);
                    Done = false;
                }
            }

            public override void OnCastFinished(BossModule module, Actor actor)
            {
                if (actor.CastInfo!.Action == _watchedAction)
                {
                    _casters.Remove(actor);
                    Done = _casters.Count == 0;
                }
            }
        }

        // generic component that shows user-defined shape for any number of active casters with self-targeted casts
        public class SelfTargetedAOE : BossModule.Component
        {
            private ActionID _watchedAction;
            private AOEShape _shape;
            private int _maxCasts;
            private List<Actor> _casters = new();

            public SelfTargetedAOE(ActionID aid, AOEShape shape, int maxCasts = int.MaxValue)
            {
                _watchedAction = aid;
                _shape = shape;
                _maxCasts = maxCasts;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
            {
                if (_casters.Take(_maxCasts).Any(c => _shape.Check(actor.Position, c)))
                    hints.Add("GTFO from aoe!");
            }

            public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                foreach (var c in _casters.Take(_maxCasts))
                    _shape.Draw(arena, c);
            }

            public override void OnCastStarted(BossModule module, Actor actor)
            {
                if (actor.CastInfo!.Action == _watchedAction)
                {
                    _casters.Add(actor);
                }
            }

            public override void OnCastFinished(BossModule module, Actor actor)
            {
                if (actor.CastInfo!.Action == _watchedAction)
                {
                    _casters.Remove(actor);
                }
            }
        }

        // generic component that is 'active' during specific primary target's cast
        // useful for simple bosses - outdoor, dungeons, etc.
        public class CastHint : BossModule.Component
        {
            protected ActionID _action;
            protected string _hint;

            public CastHint(ActionID action, string hint)
            {
                _action = action;
                _hint = hint;
            }

            public bool Active(BossModule module) => module.PrimaryActor.CastInfo?.Action == _action;

            public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
            {
                if (Active(module))
                    hints.Add(_hint);
            }
        }

        // generic avoidable aoe
        public class CastHintAvoidable : CastHint
        {
            protected AOEShape _shape;

            public CastHintAvoidable(ActionID action, AOEShape shape)
                : base(action, "Avoidable AOE")
            {
                _shape = shape;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
            {
                if (Active(module) && _shape.Check(actor.Position, module.PrimaryActor))
                    hints.Add("GTFO from aoe!");
            }

            public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                if (Active(module))
                    _shape.Draw(arena, module.PrimaryActor);
            }
        }
    }
}
