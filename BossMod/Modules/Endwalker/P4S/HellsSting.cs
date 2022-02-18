using System;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to hell's sting mechanic (part of curtain call sequence)
    class HellsSting : Component
    {
        public int NumCasts { get; private set; } = 0;

        private P4S _module;
        private float _direction;

        private static float _coneHalfAngle = MathF.PI / 12; // not sure about this...

        public HellsSting(P4S module, float dir)
        {
            _module = module;
            _direction = dir;
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var boss = _module.Boss2();
            if (NumCasts >= 2 || boss == null)
                return;

            for (int i = 0; i < 8; ++i)
            {
                float dir = _direction + NumCasts * MathF.PI / 8 + i * MathF.PI / 4;
                if (GeometryUtils.PointInCone(actor.Position - boss.Position, dir, _coneHalfAngle))
                {
                    hints.Add("GTFO from cone!");
                    break;
                }
            }
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            var boss = _module.Boss2();
            if (NumCasts >= 2 || boss == null)
                return;

            for (int i = 0; i < 8; ++i)
            {
                float dir = _direction + NumCasts * MathF.PI / 8 + i * MathF.PI / 4;
                arena.ZoneCone(boss.Position, 0, 50, dir - _coneHalfAngle, dir + _coneHalfAngle, arena.ColorAOE);
            }
        }

        public override void OnCastFinished(WorldState.Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.HellsStingAOE1) || actor.CastInfo!.IsSpell(AID.HellsStingAOE2))
                ++NumCasts;
        }
    }
}
