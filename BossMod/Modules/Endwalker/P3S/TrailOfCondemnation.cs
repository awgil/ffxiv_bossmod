using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to trail of condemnation mechanic
    class TrailOfCondemnation : Component
    {
        public bool Done { get; private set; } = false;
        private bool _isCenter;

        private static float _halfWidth = 7.5f;
        private static float _sidesOffset = 12.5f;
        private static float _aoeRadius = 6;

        public override void Init(BossModule module)
        {
            _isCenter = module.PrimaryActor.CastInfo?.IsSpell(AID.TrailOfCondemnationCenter) ?? false;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (module.PrimaryActor.Position == module.Arena.WorldCenter)
                return;

            var dir = Vector3.Normalize(module.Arena.WorldCenter - module.PrimaryActor.Position);
            if (_isCenter)
            {
                if (GeometryUtils.PointInRect(actor.Position - module.PrimaryActor.Position, dir, 2 * module.Arena.WorldHalfSize, 0, _halfWidth))
                {
                    hints.Add("GTFO from aoe!");
                }
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
                {
                    hints.Add("Spread!");
                }
            }
            else
            {
                var offset = _sidesOffset * new Vector3(-dir.Z, 0, dir.X);
                if (GeometryUtils.PointInRect(actor.Position - module.PrimaryActor.Position + offset, dir, 2 * module.Arena.WorldHalfSize, 0, _halfWidth) ||
                    GeometryUtils.PointInRect(actor.Position - module.PrimaryActor.Position - offset, dir, 2 * module.Arena.WorldHalfSize, 0, _halfWidth))
                {
                    hints.Add("GTFO from aoe!");
                }
                // note: sparks either target all tanks & healers or all dds - so correct pairings are always dd+tank/healer
                int numStacked = 0;
                bool goodPair = false;
                foreach (var pair in module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius))
                {
                    ++numStacked;
                    goodPair = (actor.Role == Role.Tank || actor.Role == Role.Healer) != (pair.Role == Role.Tank || pair.Role == Role.Healer);
                }
                if (numStacked != 1)
                {
                    hints.Add("Stack in pairs!");
                }
                else if (!goodPair)
                {
                    hints.Add("Incorrect pairing!");
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (module.PrimaryActor.Position == arena.WorldCenter)
                return;

            var dir = Vector3.Normalize(arena.WorldCenter - module.PrimaryActor.Position);
            if (_isCenter)
            {
                arena.ZoneQuad(module.PrimaryActor.Position, dir, 2 * arena.WorldHalfSize, 0, _halfWidth, arena.ColorAOE);
            }
            else
            {
                var offset = _sidesOffset * new Vector3(-dir.Z, 0, dir.X);
                arena.ZoneQuad(module.PrimaryActor.Position + offset, dir, 2 * arena.WorldHalfSize, 0, _halfWidth, arena.ColorAOE);
                arena.ZoneQuad(module.PrimaryActor.Position - offset, dir, 2 * arena.WorldHalfSize, 0, _halfWidth, arena.ColorAOE);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            // draw all raid members, to simplify positioning
            foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
            {
                bool inRange = GeometryUtils.PointInCircle(player.Position - pc.Position, _aoeRadius);
                arena.Actor(player, inRange ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }

            // draw circle around pc
            arena.AddCircle(pc.Position, _aoeRadius, arena.ColorDanger);
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.FlareOfCondemnation) || info.IsSpell(AID.SparksOfCondemnation))
                Done = true;
        }
    }
}
