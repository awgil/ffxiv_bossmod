using System;
using System.Numerics;

namespace BossMod.P1S
{
    using static BossModule;

    // state related to aether explosion mechanics, done as part of aetherflails, aetherchain and shackles of time abilities
    class AetherExplosion : Component
    {
        private enum Cell { None, Red, Blue }

        private P1S _module;
        private Actor? _memberWithSOT = null; // if not null, then every update exploding cells are recalculated based on this raid member's position
        private Cell _explodingCells = Cell.None;

        private static uint _colorSOTActor = 0xff8080ff;

        public AetherExplosion(P1S module)
        {
            _module = module;
        }

        public bool SOTActive => _memberWithSOT != null;

        public override void Update()
        {
            if (_memberWithSOT != null)
                _explodingCells = CellFromOffset(_memberWithSOT.Position - _module.Arena.WorldCenter);
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (actor != _memberWithSOT && _explodingCells != Cell.None && _explodingCells == CellFromOffset(actor.Position - _module.Arena.WorldCenter))
            {
                hints.Add("Hit by aether explosion!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_explodingCells == Cell.None || pc == _memberWithSOT)
                return; // nothing to draw

            if (!_module.Arena.IsCircle)
            {
                Service.Log("[P1S] Trying to draw aether AOE when cells mode is not active...");
                return;
            }

            float start = _explodingCells == Cell.Blue ? 0 : MathF.PI / 4;
            for (int i = 0; i < 4; ++i)
            {
                arena.ZoneCone(arena.WorldCenter, 0, P1S.InnerCircleRadius, start, start + MathF.PI / 4, arena.ColorAOE);
                arena.ZoneCone(arena.WorldCenter, P1S.InnerCircleRadius, arena.WorldHalfSize, start + MathF.PI / 4, start + MathF.PI / 2, arena.ColorAOE);
                start += MathF.PI / 2;
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_memberWithSOT != pc)
                arena.Actor(_memberWithSOT, _colorSOTActor);
        }

        public override void OnStatusGain(Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.AetherExplosion:
                    // we rely on parameter of an invisible status on boss to detect red/blue
                    _explodingCells = actor.Statuses[index].Extra switch
                    {
                        0x14C => Cell.Red,
                        0x14D => Cell.Blue,
                        _ => Cell.None
                    };
                    if (_explodingCells == Cell.None)
                        Service.Log($"[P1S] Unexpected aether explosion param {actor.Statuses[index].Extra:X2}");
                    if (_memberWithSOT != null)
                    {
                        Service.Log($"[P1S] Unexpected forced explosion while SOT is active");
                        _memberWithSOT = null;
                    }
                    break;

                case SID.ShacklesOfTime:
                    if (_memberWithSOT != null)
                        Service.Log($"[P1S] Unexpected ShacklesOfTime: another is already active!");
                    _memberWithSOT = actor;
                    _explodingCells = Cell.None;
                    break;
            }
        }

        public override void OnStatusLose(Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.ShacklesOfTime:
                    if (_memberWithSOT == actor)
                        _memberWithSOT = null;
                    _explodingCells = Cell.None;
                    break;
            }
        }

        private static Cell CellFromOffset(Vector3 offsetFromCenter)
        {
            var phi = GeometryUtils.DirectionFromVec3(offsetFromCenter) + MathF.PI;
            int coneIndex = (int)(4 * phi / MathF.PI); // phi / (pi/4); range [0, 8]
            bool oddCone = (coneIndex & 1) != 0;
            bool outerCone = !GeometryUtils.PointInCircle(offsetFromCenter, P1S.InnerCircleRadius);
            return (oddCone == outerCone) ? Cell.Blue : Cell.Red; // outer odd = inner even = blue
        }
    }
}
