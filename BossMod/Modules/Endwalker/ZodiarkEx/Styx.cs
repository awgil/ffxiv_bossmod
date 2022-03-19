using System;

namespace BossMod.Endwalker.ZodiarkEx
{
    using static BossModule;

    // permanently active component that tracks styx targets - they are applied slightly before styx cast start
    class StyxTargetTracker : Component
    {
        private Actor? _target;
        private DateTime _assignTime;
        private ZodiarkEx _module;

        public Actor? Target => (_module.WorldState.CurrentTime - _assignTime).TotalSeconds < 20 ? _target : null; // that's a slight hack, since we don't know when icons disappear

        public StyxTargetTracker(ZodiarkEx module)
        {
            _module = module;
        }

        public override void OnEventIcon(uint actorID, uint iconID)
        {
            if (iconID == (uint)IconID.Styx)
            {
                _target = _module.WorldState.Actors.Find(actorID);
                _assignTime = _module.WorldState.CurrentTime;
            }
        }
    }

    // state related to styx mechanic
    class Styx : CommonComponents.CastCounter
    {
        private ZodiarkEx _module;
        private StyxTargetTracker? _tracker;

        private static float _stackRadius = 5;

        public Styx(ZodiarkEx module)
            : base(ActionID.MakeSpell(AID.StyxAOE))
        {
            _module = module;
            _tracker = module.FindComponent<StyxTargetTracker>();
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var target = _tracker?.Target;
            if (target != null && target != actor && !GeometryUtils.PointInCircle(actor.Position - target.Position, _stackRadius))
                hints.Add("Stack!");
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            var target = _tracker?.Target;
            if (target == null)
                return;

            arena.AddCircle(target.Position, _stackRadius, arena.ColorDanger);
            if (target == pc)
            {
                // draw other players to simplify stacking
                foreach (var a in _module.Raid.WithoutSlot().Exclude(pc))
                    arena.Actor(a, GeometryUtils.PointInCircle(a.Position - target.Position, _stackRadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
            else
            {
                // draw target next to which pc is to stack
                arena.Actor(target, arena.ColorDanger);
            }
        }
    }
}
