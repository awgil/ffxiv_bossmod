using System;

namespace BossMod.Endwalker.ZodiarkEx
{
    using static BossModule;

    // permanently active component that tracks styx targets - they are applied slightly before styx cast start
    class StyxTargetTracker : Component
    {
        private Actor? _target;
        private DateTime _assignTime;

        public Actor? Target(BossModule module) => (module.WorldState.CurrentTime - _assignTime).TotalSeconds < 20 ? _target : null; // that's a slight hack, since we don't know when icons disappear

        public override void OnEventIcon(BossModule module, uint actorID, uint iconID)
        {
            if (iconID == (uint)IconID.Styx)
            {
                _target = module.WorldState.Actors.Find(actorID);
                _assignTime = module.WorldState.CurrentTime;
            }
        }
    }

    // state related to styx mechanic
    class Styx : CommonComponents.CastCounter
    {
        private static float _stackRadius = 5;

        public Styx() : base(ActionID.MakeSpell(AID.StyxAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var target = module.FindComponent<StyxTargetTracker>()?.Target(module);
            if (target != null && target != actor && !GeometryUtils.PointInCircle(actor.Position - target.Position, _stackRadius))
                hints.Add("Stack!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var target = module.FindComponent<StyxTargetTracker>()?.Target(module);
            if (target == null)
                return;

            arena.AddCircle(target.Position, _stackRadius, arena.ColorDanger);
            if (target == pc)
            {
                // draw other players to simplify stacking
                foreach (var a in module.Raid.WithoutSlot().Exclude(pc))
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
