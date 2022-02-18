using System.Linq;
using System.Numerics;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to pinax mechanics
    // note: for now, we assume Lava Mekhane always targets 2 healers
    class Pinax : Component
    {
        public int NumFinished { get; private set; } = 0;
        private P4S _module;
        private WorldState.Actor? _acid;
        private WorldState.Actor? _fire;
        private WorldState.Actor? _water;
        private WorldState.Actor? _lighting;

        private static float _acidAOERadius = 5;
        private static float _fireAOERadius = 6;
        private static float _knockbackRadius = 13;
        private static float _lightingSafeDistance = 15; // not sure about this, what is real safe distance?

        public Pinax(P4S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_acid != null)
            {
                if (GeometryUtils.PointInRect(actor.Position - _acid.Position, Vector3.UnitX, 10, 10, 10))
                {
                    hints.Add("GTFO from acid square!");
                }
                hints.Add("Spread!", _module.RaidMembers.WithoutSlot().InRadiusExcluding(actor, _acidAOERadius).Any());
            }
            if (_fire != null)
            {
                if (GeometryUtils.PointInRect(actor.Position - _fire.Position, Vector3.UnitX, 10, 10, 10))
                {
                    hints.Add("GTFO from fire square!");
                }
                hints.Add("Stack in fours!", _module.RaidMembers.WithoutSlot().Where(x => x.Role == Role.Healer).InRadius(actor.Position, _fireAOERadius).Count() != 1);
            }
            if (_water != null)
            {
                if (GeometryUtils.PointInRect(actor.Position - _water.Position, Vector3.UnitX, 10, 10, 10))
                {
                    hints.Add("GTFO from water square!");
                }
                if (!_module.Arena.InBounds(AdjustPositionForKnockback(actor.Position, _module.Arena.WorldCenter, _knockbackRadius)))
                {
                    hints.Add("About to be knocked into wall!");
                }
            }
            if (_lighting != null)
            {
                if (GeometryUtils.PointInRect(actor.Position - _lighting.Position, Vector3.UnitX, 10, 10, 10))
                {
                    hints.Add("GTFO from lighting square!");
                }
                hints.Add("GTFO from center!", GeometryUtils.PointInRect(actor.Position - _module.Arena.WorldCenter, Vector3.UnitX, _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance));
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            if (_acid != null)
            {
                arena.ZoneQuad(_acid.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
            }
            if (_fire != null)
            {
                arena.ZoneQuad(_fire.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
            }
            if (_water != null)
            {
                arena.ZoneQuad(_water.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
            }
            if (_lighting != null)
            {
                arena.ZoneQuad(_lighting.Position, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
                arena.ZoneQuad(arena.WorldCenter, Vector3.UnitX, _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance, arena.ColorAOE);
            }
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            var pc = _module.Player();
            if (pc == null)
                return;

            if (_acid != null)
            {
                arena.AddCircle(pc.Position, _acidAOERadius, arena.ColorDanger);
                foreach (var player in _module.RaidMembers.WithoutSlot().Exclude(pc))
                    arena.Actor(player, GeometryUtils.PointInCircle(player.Position - pc.Position, _acidAOERadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
            if (_fire != null)
            {
                foreach (var player in _module.RaidMembers.WithoutSlot())
                {
                    if (player.Role == Role.Healer)
                    {
                        arena.Actor(player, arena.ColorDanger);
                        arena.AddCircle(pc.Position, _fireAOERadius, arena.ColorDanger);
                    }
                    else
                    {
                        arena.Actor(player, arena.ColorPlayerGeneric);
                    }
                }
            }
            if (_water != null)
            {
                var adjPos = AdjustPositionForKnockback(pc.Position, _module.Arena.WorldCenter, _knockbackRadius);
                if (adjPos != pc.Position)
                {
                    arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                    arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
                }
            }
        }

        public override void OnCastStarted(WorldState.Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.PinaxAcid:
                    _acid = actor;
                    break;
                case AID.PinaxLava:
                    _fire = actor;
                    break;
                case AID.PinaxWell:
                    _water = actor;
                    break;
                case AID.PinaxLevinstrike:
                    _lighting = actor;
                    break;
            }
        }

        public override void OnCastFinished(WorldState.Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.PinaxAcid:
                    _acid = null;
                    ++NumFinished;
                    break;
                case AID.PinaxLava:
                    _fire = null;
                    ++NumFinished;
                    break;
                case AID.PinaxWell:
                    _water = null;
                    ++NumFinished;
                    break;
                case AID.PinaxLevinstrike:
                    _lighting = null;
                    ++NumFinished;
                    break;
            }
        }
    }
}
