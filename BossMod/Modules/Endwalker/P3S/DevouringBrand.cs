using System;
using System.Numerics;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to devouring brand mechanic
    class DevouringBrand : Component
    {
        private P3S _module;

        private static float _halfWidth = 5;

        public DevouringBrand(P3S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var offset = actor.Position - _module.Arena.WorldCenter;
            if (MathF.Abs(offset.X) <= _halfWidth || MathF.Abs(offset.Z) <= _halfWidth)
            {
                hints.Add("GTFO from brand!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            arena.ZoneQuad(arena.WorldCenter, Vector3.UnitX, arena.WorldHalfSize, arena.WorldHalfSize, _halfWidth, arena.ColorAOE);
            arena.ZoneQuad(arena.WorldCenter, Vector3.UnitZ, arena.WorldHalfSize, -_halfWidth, _halfWidth, arena.ColorAOE);
            arena.ZoneQuad(arena.WorldCenter, -Vector3.UnitZ, arena.WorldHalfSize, -_halfWidth, _halfWidth, arena.ColorAOE);
        }
    }
}
