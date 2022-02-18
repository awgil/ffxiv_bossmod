using System;

namespace BossMod.P2S
{
    using static BossModule;

    // state related to cataract mechanic
    class Cataract : Component
    {
        private P2S _module;
        private bool _isWinged;

        private static float _halfWidth = 7.5f;

        public Cataract(P2S module, bool winged)
        {
            _module = module;
            _isWinged = winged;
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var boss = _module.Boss();
            var head = _module.CataractHead();
            float headRot = _isWinged ? MathF.PI : 0;
            if ((boss != null && GeometryUtils.PointInRect(actor.Position - boss.Position, boss.Rotation, _module.Arena.WorldHalfSize, _module.Arena.WorldHalfSize, _halfWidth)) ||
                (head != null && GeometryUtils.PointInRect(actor.Position - head.Position, head.Rotation + headRot, _module.Arena.WorldHalfSize, 0, _module.Arena.WorldHalfSize)))
            {
                hints.Add("GTFO from cataract!");
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            var boss = _module.Boss();
            if (boss != null)
            {
                arena.ZoneQuad(boss.Position, boss.Rotation, arena.WorldHalfSize, arena.WorldHalfSize, _halfWidth, arena.ColorAOE);
            }

            var head = _module.CataractHead();
            if (head != null)
            {
                float headRot = _isWinged ? MathF.PI : 0;
                arena.ZoneQuad(head.Position, head.Rotation + headRot, arena.WorldHalfSize, 0, arena.WorldHalfSize, arena.ColorAOE);
            }
        }
    }
}
