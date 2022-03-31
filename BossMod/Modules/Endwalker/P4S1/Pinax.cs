using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P4S1
{
    using static BossModule;

    // state related to pinax mechanics
    class Pinax : Component
    {
        private enum Order { Unknown, LUWU, WULU, LFWA, LAWF, WFLA, WALF }

        public int NumFinished { get; private set; } = 0;
        private Order _order;
        private Actor? _acid;
        private Actor? _fire;
        private Actor? _water;
        private Actor? _lighting;

        private static float _acidAOERadius = 5;
        private static float _fireAOERadius = 6;
        private static float _knockbackRadius = 13;
        private static float _lightingSafeDistance = 16; // linear falloff until 16, then constant (not sure whether it is true distance-based or max-coord-based)

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_acid != null)
            {
                if (GeometryUtils.PointInRect(actor.Position - _acid.Position, Vector3.UnitX, 10, 10, 10))
                {
                    hints.Add("GTFO from acid square!");
                }
                hints.Add("Spread!", module.Raid.WithoutSlot().InRadiusExcluding(actor, _acidAOERadius).Any());
            }
            if (_fire != null)
            {
                if (GeometryUtils.PointInRect(actor.Position - _fire.Position, Vector3.UnitX, 10, 10, 10))
                {
                    hints.Add("GTFO from fire square!");
                }
                hints.Add("Stack in fours!", module.Raid.WithoutSlot().Where(x => x.Role == Role.Healer).InRadius(actor.Position, _fireAOERadius).Count() != 1);
            }
            if (_water != null)
            {
                if (GeometryUtils.PointInRect(actor.Position - _water.Position, Vector3.UnitX, 10, 10, 10))
                {
                    hints.Add("GTFO from water square!");
                }
                if (!module.Arena.InBounds(AdjustPositionForKnockback(actor.Position, module.Arena.WorldCenter, _knockbackRadius)))
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
                hints.Add("GTFO from center!", GeometryUtils.PointInRect(actor.Position - module.Arena.WorldCenter, Vector3.UnitX, _lightingSafeDistance, _lightingSafeDistance, _lightingSafeDistance));
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            string order = _order switch
            {
                Order.LUWU => "Lightning - ??? - Water - ???",
                Order.WULU => "Water - ??? - Lightning - ???",
                Order.LFWA => "Lightning - Fire - Water - Acid",
                Order.LAWF => "Lightning - Acid - Water - Fire",
                Order.WFLA => "Water - Fire - Lightning - Acid",
                Order.WALF => "Water - Acid - Lightning - Fire",
                _ => "???"
            };
            hints.Add($"Pinax order: {order}");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
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

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_acid != null)
            {
                arena.AddCircle(pc.Position, _acidAOERadius, arena.ColorDanger);
                foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
                    arena.Actor(player, GeometryUtils.PointInCircle(player.Position - pc.Position, _acidAOERadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
            if (_fire != null)
            {
                foreach (var player in module.Raid.WithoutSlot())
                {
                    if (player.Role == Role.Healer)
                    {
                        arena.Actor(player, arena.ColorDanger);
                        arena.AddCircle(player.Position, _fireAOERadius, arena.ColorDanger);
                    }
                    else
                    {
                        arena.Actor(player, arena.ColorPlayerGeneric);
                    }
                }
            }
            if (_water != null)
            {
                var adjPos = AdjustPositionForKnockback(pc.Position, module.Arena.WorldCenter, _knockbackRadius);
                if (adjPos != pc.Position)
                {
                    arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                    arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.PinaxAcid:
                    _acid = actor;
                    if (_order == Order.WULU)
                        _order = Order.WALF;
                    else if (_order == Order.LUWU)
                        _order = Order.LAWF;
                    break;
                case AID.PinaxLava:
                    _fire = actor;
                    if (_order == Order.WULU)
                        _order = Order.WFLA;
                    else if (_order == Order.LUWU)
                        _order = Order.LFWA;
                    break;
                case AID.PinaxWell:
                    _water = actor;
                    if (_order == Order.Unknown)
                        _order = Order.WULU;
                    break;
                case AID.PinaxLevinstrike:
                    _lighting = actor;
                    if (_order == Order.Unknown)
                        _order = Order.LUWU;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
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
