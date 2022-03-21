using System.Linq;

namespace BossMod.Endwalker.ZodiarkEx
{
    using static BossModule;

    // state related to ania mechanic
    class Ania : Component
    {
        private Actor? _target;

        private static float _aoeRadius = 3;

        public bool Done => _target == null;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_target == null)
                return;

            if (actor == _target)
            {
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
                    hints.Add("GTFO from raid!");
                if (module.PrimaryActor.TargetID == _target.InstanceID)
                    hints.Add("Pass aggro!");
            }
            else
            {
                if (GeometryUtils.PointInCircle(actor.Position - _target.Position, _aoeRadius))
                    hints.Add("GTFO from tank!");
                if (actor.Role == Role.Tank && module.PrimaryActor.TargetID != actor.InstanceID)
                    hints.Add("Taunt!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_target == null)
                return;

            arena.AddCircle(_target.Position, _aoeRadius, arena.ColorDanger);
            if (pc == _target)
            {
                foreach (var a in module.Raid.WithoutSlot().Exclude(pc))
                    arena.Actor(a, GeometryUtils.PointInCircle(a.Position - _target.Position, _aoeRadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
            else
            {
                arena.Actor(_target, arena.ColorDanger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AniaAOE))
                _target = module.WorldState.Actors.Find(actor.CastInfo.TargetID);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AniaAOE))
                _target = null;
        }
    }
}
