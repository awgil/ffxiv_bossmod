using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Extreme.Ex1Zodiark
{
    // state related to astral eclipse mechanic
    // 'pattern' is a mask containing explosion spots; index is 4 bits, with low 2 bits describing world X position and 2 high bits describing world Z position
    // so NE corner (X=+1, Z=-1) corresponds to index 0b0010 = 2; S corner (X=0, Z=+1) corresponds to index 0b1001 = 9 and so on (0b11 index is unused)
    // 'completed' or 'not started' is represented as fully safe (all 0) mask, 'unknown' pattern is represented as fully dangerous (all 1) mask
    class AstralEclipse : BossComponent
    {
        private int[] _patterns = new int[3]; // W -> S -> E

        private static AOEShapeCircle _aoe = new(10);

        // transform from 'pattern space' (X goes to the right, Y goes to the bottom) to world space
        private static float _centerOffset = 14;
        private static Vector3[] _basisX = new Vector3[3] { -Vector3.UnitZ, -Vector3.UnitX, Vector3.UnitZ };
        private static Vector3[] _basisY = new Vector3[3] { -Vector3.UnitX,  Vector3.UnitZ, Vector3.UnitX };

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            int nextPattern = _patterns.SkipWhile(p => p == 0).FirstOrDefault();
            if (PatternSpots(module, nextPattern).Any(p => _aoe.Check(actor.Position, p)))
                hints.Add("GTFO from explosion!");

            if (movementHints != null)
                foreach (var (from, to) in EnumMovementHints(module, actor.Position))
                    movementHints.Add(from, to, ArenaColor.Safe);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            int nextPattern = _patterns.SkipWhile(p => p == 0).FirstOrDefault();
            foreach (var p in PatternSpots(module, nextPattern))
                _aoe.Draw(arena, p);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (from, to) in EnumMovementHints(module, pc.Position))
                arena.AddLine(from, to, ArenaColor.Safe);
        }

        public override void OnEventEnvControl(BossModule module, byte index, uint state)
        {
            if (index < 6 || index > 8)
                return;
            var seq = index - 6;
            _patterns[seq] = state switch
            {
                0x00080004 => 0, // done
                0x00020001 => BuildPattern(seq, new(0, 1), new(-1, 0)), // safe B & L
                0x00200010 => BuildPattern(seq, new(1, 1), new(0, 0)), // safe BR & C
                0x00800040 => BuildPattern(seq, new(-1, 1), new(1, 0)), // safe BL & R
                0x10000800 => BuildPattern(seq, new(1, 1), new(0, -1)), // safe BR & T
                _ => 0xFFFF, // unknown
            };
        }

        private int BuildMask(int seq, Vector2 patternCoords)
        {
            Vector3 w = _basisX[seq] * patternCoords.X + _basisY[seq] * patternCoords.Y;
            int xPos = w.X > 0 ? 2 : (w.X < 0 ? 0 : 1);
            int zPos = w.Z > 0 ? 2 : (w.Z < 0 ? 0 : 1);
            return 1 << (zPos * 4 + xPos);
        }

        private int BuildPattern(int seq, Vector2 safe1, Vector2 safe2)
        {
            return 0x777 & ~BuildMask(seq, safe1) & ~BuildMask(seq, safe2);
        }

        private IEnumerable<WPos> PatternSpots(BossModule module, int pattern)
        {
            if (pattern != 0)
                for (int z = -1; z <= 1; ++z)
                    for (int x = -1; x <= 1; ++x)
                        if ((pattern & (1 << ((z + 1) * 4 + (x + 1)))) != 0)
                            yield return module.Bounds.Center + _centerOffset * new WDir(x, z);
        }

        private IEnumerable<(WPos, WPos)> EnumMovementHints(BossModule module, WPos startingPosition)
        {
            WPos prev = startingPosition;
            foreach (var p in _patterns.Where(p => p != 0))
            {
                if (p == 0xFFFF)
                    break;

                var next = PatternSpots(module, ~p).MinBy(pos => (pos - prev).LengthSq() + (pos - module.PrimaryActor.Position).LengthSq() * 0.2f); // slightly penalize far positions...
                yield return (prev, next);
                prev = next;
            }
        }
    }
}
