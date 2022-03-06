using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.P3S
{
    using static BossModule;

    // state related to darkblaze twister mechanics
    class DarkblazeTwister : Component
    {
        private P3S _module;
        private List<Actor> _twisters;

        private static float _knockbackRange = 17;
        private static float _aoeInnerRadius = 5; // not sure about this...
        private static float _aoeMiddleRadius = 7; // not sure about this...
        private static float _aoeOuterRadius = 20;

        public IEnumerable<Actor> BurningTwisters => _twisters.Where(twister => twister.CastInfo?.IsSpell(AID.BurningTwister) ?? false);
        public Actor? DarkTwister => _twisters.Find(twister => twister.CastInfo?.IsSpell(AID.DarkTwister) ?? false);

        public DarkblazeTwister(P3S module)
        {
            _module = module;
            _twisters = module.Enemies(OID.DarkblazeTwister);
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var adjPos = AdjustPositionForKnockback(actor.Position, DarkTwister, _knockbackRange);
            if (actor.Position != adjPos && !_module.Arena.InBounds(adjPos))
            {
                hints.Add("About to be knocked back into wall!");
            }

            foreach (var twister in BurningTwisters)
            {
                var offset = adjPos - twister.Position;
                if (GeometryUtils.PointInCircle(offset, _aoeInnerRadius) || GeometryUtils.PointInCircle(offset, _aoeOuterRadius) && !GeometryUtils.PointInCircle(offset, _aoeMiddleRadius))
                {
                    hints.Add("GTFO from aoe!");
                    break;
                }
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            foreach (var twister in BurningTwisters)
            {
                arena.ZoneCircle(twister.Position, _aoeInnerRadius, arena.ColorAOE);
                arena.ZoneCone(twister.Position, _aoeMiddleRadius, _aoeOuterRadius, 0, 2 * MathF.PI, arena.ColorAOE);
            }
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            var pc = _module.Player();
            var darkTwister = DarkTwister;
            if (pc == null || darkTwister == null)
                return;

            var adjPos = AdjustPositionForKnockback(pc.Position, darkTwister, _knockbackRange);
            if (adjPos != pc.Position)
            {
                arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
                arena.Actor(adjPos, pc.Rotation, arena.ColorDanger);
            }

            var safeOffset = _knockbackRange + (_aoeInnerRadius + _aoeMiddleRadius) / 2;
            var safeRadius = (_aoeMiddleRadius - _aoeInnerRadius) / 2;
            foreach (var burningTwister in BurningTwisters)
            {
                var dir = burningTwister.Position - darkTwister.Position;
                var len = dir.Length();
                dir /= len;
                arena.AddCircle(darkTwister.Position + dir * (len - safeOffset), safeRadius, arena.ColorSafe);
            }
        }
    }
}
