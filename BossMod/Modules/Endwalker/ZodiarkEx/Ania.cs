using System;
using System.Linq;

namespace BossMod.Endwalker.ZodiarkEx
{
    using static BossModule;

    // state related to ania mechanic
    class Ania : Component
    {
        private ZodiarkEx _module;
        private Actor? _target;

        private static float _aoeRadius = 3;

        public bool Done => _target == null;

        public Ania(ZodiarkEx module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_target == null)
                return;

            if (actor == _target)
            {
                if (_module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
                    hints.Add("GTFO from raid!");
                if (_module.PrimaryActor.TargetID == _target.InstanceID)
                    hints.Add("Pass aggro!");
            }
            else
            {
                if (GeometryUtils.PointInCircle(actor.Position - _target.Position, _aoeRadius))
                    hints.Add("GTFO from tank!");
                if (actor.Role == Role.Tank && _module.PrimaryActor.TargetID != actor.InstanceID)
                    hints.Add("Taunt!");
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_target == null)
                return;

            arena.AddCircle(_target.Position, _aoeRadius, arena.ColorDanger);
            if (pc == _target)
            {
                foreach (var a in _module.Raid.WithoutSlot().Exclude(pc))
                    arena.Actor(a, GeometryUtils.PointInCircle(a.Position - _target.Position, _aoeRadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
            else
            {
                arena.Actor(_target, arena.ColorDanger);
            }
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AniaAOE))
                _target = _module.WorldState.Actors.Find(actor.CastInfo.TargetID);
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.AniaAOE))
                _target = null;
        }
    }
}
