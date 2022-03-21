using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to darkblaze twister mechanics
    class DarkblazeTwister : Component
    {
        private static float _knockbackRange = 17;
        private static float _aoeInnerRadius = 5; // not sure about this...
        private static float _aoeMiddleRadius = 7; // not sure about this...
        private static float _aoeOuterRadius = 20;

        public IEnumerable<Actor> BurningTwisters(BossModule module) => module.Enemies(OID.DarkblazeTwister).Where(twister => twister.CastInfo?.IsSpell(AID.BurningTwister) ?? false);
        public Actor? DarkTwister(BossModule module) => module.Enemies(OID.DarkblazeTwister).Find(twister => twister.CastInfo?.IsSpell(AID.DarkTwister) ?? false);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var adjPos = AdjustPositionForKnockback(actor.Position, DarkTwister(module), _knockbackRange);
            if (actor.Position != adjPos && !module.Arena.InBounds(adjPos))
            {
                hints.Add("About to be knocked back into wall!");
            }

            foreach (var twister in BurningTwisters(module))
            {
                var offset = adjPos - twister.Position;
                if (GeometryUtils.PointInCircle(offset, _aoeInnerRadius) || GeometryUtils.PointInCircle(offset, _aoeOuterRadius) && !GeometryUtils.PointInCircle(offset, _aoeMiddleRadius))
                {
                    hints.Add("GTFO from aoe!");
                    break;
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var twister in BurningTwisters(module))
            {
                arena.ZoneCircle(twister.Position, _aoeInnerRadius, arena.ColorAOE);
                arena.ZoneDonut(twister.Position, _aoeMiddleRadius, _aoeOuterRadius, arena.ColorAOE);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var darkTwister = DarkTwister(module);
            if (darkTwister == null)
                return;

            var adjPos = AdjustPositionForKnockback(pc.Position, darkTwister, _knockbackRange);
            if (adjPos != pc.Position)
            {
                arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
            }

            var safeOffset = _knockbackRange + (_aoeInnerRadius + _aoeMiddleRadius) / 2;
            var safeRadius = (_aoeMiddleRadius - _aoeInnerRadius) / 2;
            foreach (var burningTwister in BurningTwisters(module))
            {
                var dir = burningTwister.Position - darkTwister.Position;
                var len = dir.Length();
                dir /= len;
                arena.AddCircle(darkTwister.Position + dir * (len - safeOffset), safeRadius, arena.ColorSafe);
            }
        }
    }
}
