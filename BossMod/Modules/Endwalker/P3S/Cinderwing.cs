using System;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to cinderwing
    class Cinderwing : Component
    {
        private P3S _module;
        private float _rot;

        public Cinderwing(P3S module, bool left)
        {
            _module = module;
            _rot = left ? MathF.PI / 2 : -MathF.PI / 2;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (GeometryUtils.PointInCone(actor.Position - _module.PrimaryActor.Position, _module.PrimaryActor.Rotation + _rot, MathF.PI / 2))
            {
                hints.Add("GTFO from wing!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            arena.ZoneQuad(_module.PrimaryActor.Position, _module.PrimaryActor.Rotation + _rot, 50, 0, 50, arena.ColorAOE);
        }
    }
}
