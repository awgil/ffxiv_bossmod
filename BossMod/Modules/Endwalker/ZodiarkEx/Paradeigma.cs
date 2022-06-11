using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.ZodiarkEx
{
    using static BossModule;

    // state related to paradeigma and astral flow mechanics
    class Paradeigma : Component
    {
        public enum FlowDirection { None, CW, CCW };

        private FlowDirection _flow;
        private List<Vector3> _birds = new();
        private List<Vector3> _behemoths = new();
        private List<Vector4> _snakes = new();
        private List<Vector3> _fireLine = new();

        private static float _birdBehemothOffset = 10.5f;
        private static float _snakeNearOffset = 5.5f;
        private static float _snakeFarOffset = 15.5f;
        private static float _snakeOrthoOffset = 21;
        private static AOEShapeDonut _birdAOE = new(5, 15);
        private static AOEShapeCircle _behemothAOE = new(15);
        private static AOEShapeRect _snakeAOE = new(42, 5.5f);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (RotatedBirds(module).Any(b => _birdAOE.Check(actor.Position, b, new())) || RotatedBehemoths(module).Any(b => _behemothAOE.Check(actor.Position, b, new())))
                hints.Add("GTFO from bird/behemoth aoe!");
            if (RotatedSnakes(module).Any(s => _snakeAOE.Check(actor.Position, new(s.X, s.Y, s.Z), Angle.Radians(s.W))))
                hints.Add("GTFO from snake aoe!");
            if (_fireLine.Any(c => InFireAOE(module, c, actor.Position)))
                hints.Add("GTFO from fire aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var b in RotatedBirds(module))
                _birdAOE.Draw(arena, b, new());
            foreach (var b in RotatedBehemoths(module))
                _behemothAOE.Draw(arena, b, new());
            foreach (var s in RotatedSnakes(module))
                _snakeAOE.Draw(arena, new(s.X, s.Y, s.Z), new(s.W));
            foreach (var c in _fireLine)
                arena.ZoneTri(module.Arena.WorldCenter + c, RotatedPosition(module, c), arena.WorldCenter, arena.ColorAOE);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_fireLine.Count == 2)
                arena.AddLine(module.Arena.WorldCenter + _fireLine[0], module.Arena.WorldCenter + _fireLine[1], arena.ColorDanger);
        }

        public override void OnEventEnvControl(BossModule module, uint featureID, byte index, uint state)
        {
            if (featureID != 0x80034E71)
                return;

            // notable env controls that we don't care too much about:
            // 1: common for all flows, 00020001 = activate, 00080004 = deactivate
            // 3: common for all flows, happens a bit after cast start, always 00010001
            if (index == 2)
            {
                // flow rotation arrows (note that we could also rely on cast id for them...)
                if (state == 0x00020001)
                    _flow = FlowDirection.CW;
                else if (state == 0x00200010)
                    _flow = FlowDirection.CCW;
                // other states: 00080004, 00400004 - deactivation
            }
            else if (index == 5)
            {
                switch (state)
                {
                    case 0x00020001:
                        _fireLine.Add(new(+module.Arena.WorldHalfSize, 0, -module.Arena.WorldHalfSize));
                        _fireLine.Add(new(-module.Arena.WorldHalfSize, 0, +module.Arena.WorldHalfSize));
                        break;
                    case 0x00400020:
                        _fireLine.Add(new(-module.Arena.WorldHalfSize, 0, -module.Arena.WorldHalfSize));
                        _fireLine.Add(new(+module.Arena.WorldHalfSize, 0, +module.Arena.WorldHalfSize));
                        break;
                }
            }
            else if (index >= 9 && index <= 24 && state == 0x00200010)
            {
                // birds, behemoths and snakes; other states: 20001000 = color change, 40000004 = disappear
                switch (index)
                {
                    case  9: _behemoths.Add(new(-_birdBehemothOffset, 0, -_birdBehemothOffset)); break;
                    case 10: _behemoths.Add(new(+_birdBehemothOffset, 0, -_birdBehemothOffset)); break;
                    case 11: _behemoths.Add(new(-_birdBehemothOffset, 0, +_birdBehemothOffset)); break;
                    case 12: _behemoths.Add(new(+_birdBehemothOffset, 0, +_birdBehemothOffset)); break;
                    case 13:
                        _snakes.Add(new(-_snakeFarOffset,  0, -_snakeOrthoOffset, 0));
                        _snakes.Add(new(+_snakeNearOffset, 0, -_snakeOrthoOffset, 0));
                        break;
                    case 14:
                        _snakes.Add(new(-_snakeNearOffset, 0, -_snakeOrthoOffset, 0));
                        _snakes.Add(new(+_snakeFarOffset,  0, -_snakeOrthoOffset, 0));
                        break;
                    case 15:
                        _snakes.Add(new(-_snakeFarOffset,  0,  _snakeOrthoOffset, MathF.PI));
                        _snakes.Add(new(+_snakeNearOffset, 0,  _snakeOrthoOffset, MathF.PI));
                        break;
                    case 16:
                        _snakes.Add(new(-_snakeNearOffset, 0,  _snakeOrthoOffset, MathF.PI));
                        _snakes.Add(new(+_snakeFarOffset,  0,  _snakeOrthoOffset, MathF.PI));
                        break;
                    case 17:
                        _snakes.Add(new(-_snakeOrthoOffset, 0, -_snakeFarOffset,  MathF.PI / 2));
                        _snakes.Add(new(-_snakeOrthoOffset, 0, +_snakeNearOffset, MathF.PI / 2));
                        break;
                    case 18:
                        _snakes.Add(new(-_snakeOrthoOffset, 0, -_snakeNearOffset, MathF.PI / 2));
                        _snakes.Add(new(-_snakeOrthoOffset, 0, +_snakeFarOffset,  MathF.PI / 2));
                        break;
                    case 19:
                        _snakes.Add(new( _snakeOrthoOffset, 0, -_snakeFarOffset,  -MathF.PI / 2));
                        _snakes.Add(new( _snakeOrthoOffset, 0, +_snakeNearOffset, -MathF.PI / 2));
                        break;
                    case 20:
                        _snakes.Add(new( _snakeOrthoOffset, 0, -_snakeNearOffset, -MathF.PI / 2));
                        _snakes.Add(new( _snakeOrthoOffset, 0, +_snakeFarOffset,  -MathF.PI / 2));
                        break;
                    case 21: _birds.Add(new(-_birdBehemothOffset, 0, -_birdBehemothOffset)); break;
                    case 22: _birds.Add(new(+_birdBehemothOffset, 0, -_birdBehemothOffset)); break;
                    case 23: _birds.Add(new(-_birdBehemothOffset, 0, +_birdBehemothOffset)); break;
                    case 24: _birds.Add(new(+_birdBehemothOffset, 0, +_birdBehemothOffset)); break;
                }
            }
        }

        private Vector3 RotatedPosition(BossModule module, Vector3 pos)
        {
            return _flow switch
            {
                FlowDirection.CW  => module.Arena.WorldCenter + new Vector3(-pos.Z, 0, pos.X),
                FlowDirection.CCW => module.Arena.WorldCenter + new Vector3(pos.Z, 0, -pos.X),
                _ => module.Arena.WorldCenter + pos
            };
        }

        private Vector4 RotatedPosRot(BossModule module, Vector4 posRot)
        {
            return _flow switch
            {
                FlowDirection.CW  => new(module.Arena.WorldCenter.X - posRot.Z, 0, module.Arena.WorldCenter.Z + posRot.X, posRot.W - MathF.PI / 2),
                FlowDirection.CCW => new(module.Arena.WorldCenter.X + posRot.Z, 0, module.Arena.WorldCenter.Z - posRot.X, posRot.W + MathF.PI / 2),
                _ => new Vector4(module.Arena.WorldCenter, 0) + posRot
            };
        }

        private IEnumerable<Vector3> RotatedBirds(BossModule module) => _birds.Select(x => RotatedPosition(module, x));
        private IEnumerable<Vector3> RotatedBehemoths(BossModule module) => _behemoths.Select(x => RotatedPosition(module, x));
        private IEnumerable<Vector4> RotatedSnakes(BossModule module) => _snakes.Select(x => RotatedPosRot(module, x));

        private bool InFireAOE(BossModule module, Vector3 corner, Vector3 pos)
        {
            var p1 = module.Arena.WorldCenter + corner;
            var p2 = RotatedPosition(module, corner);
            var pMid = (p1 + p2) / 2;
            var dirMid = Vector3.Normalize(pMid - module.Arena.WorldCenter);
            return GeometryUtils.PointInCone(pos - module.Arena.WorldCenter, dirMid, Angle.Radians(MathF.PI / 4));
        }
    }
}
