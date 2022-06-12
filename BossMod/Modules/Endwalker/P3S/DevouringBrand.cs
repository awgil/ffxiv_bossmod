using System;
using System.Numerics;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to devouring brand mechanic
    class DevouringBrand : Component
    {
        private static float _halfWidth = 5;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var offset = actor.Position - module.Arena.WorldCenter;
            if (MathF.Abs(offset.X) <= _halfWidth || MathF.Abs(offset.Z) <= _halfWidth)
            {
                hints.Add("GTFO from brand!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.ZoneQuad(arena.WorldCenter, new WDir(1,  0), arena.WorldHalfSize, arena.WorldHalfSize, _halfWidth, arena.ColorAOE);
            arena.ZoneQuad(arena.WorldCenter, new WDir(0,  1), arena.WorldHalfSize, -_halfWidth, _halfWidth, arena.ColorAOE);
            arena.ZoneQuad(arena.WorldCenter, new WDir(0, -1), arena.WorldHalfSize, -_halfWidth, _halfWidth, arena.ColorAOE);
        }
    }
}
