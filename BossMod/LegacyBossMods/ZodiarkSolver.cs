using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    public class ZodiarkSolver
    {
        [Flags]
        public enum Control
        {
            None = 0,
            ExoSides = 1 << 0,
            ExoFront = 1 << 1,
            ExoBack = 1 << 2,
            SnakesH = 1 << 3,
            SnakesV = 1 << 4,
            Lines = 1 << 5,
            Rot = 1 << 6,
            Birds = 1 << 7,
            Behemoths = 1 << 8,
            SideSmash = 1 << 9,
            Diagonal = 1 << 10,

            All = (1 << 11) - 1
        }

        public enum ExoType { None, Tri, Sq };
        public enum BirdPos { TL, TR, BR, BL }; // CW: +1, CCW: -1
        public enum SnakePos { HTop, VRight, HBottom, VLeft, None }; // CW: +1, CCW: -1
        public enum LinePos { None, TL, TR };
        public enum RotDir { None, CW, CCW };
        public enum ExoSide { Left, Right, Top, Bottom };

        // control state + accessors
        private ExoType[] _activeExo = { ExoType.None, ExoType.None, ExoType.None, ExoType.None };
        private bool[] _activeBehemoth = { false, false, false, false };
        public SnakePos ActiveSnakes = SnakePos.None;
        public LinePos ActiveLine = LinePos.None;
        public RotDir ActiveRot = RotDir.None;

        public ref ExoType ActiveExo(ExoSide side)
        {
            return ref _activeExo[(int)side];
        }

        public ref bool ActiveBehemoth(BirdPos pos)
        {
            return ref _activeBehemoth[(int)pos];
        }

        public ref bool ActiveRotatedBehemoth(BirdPos pos)
        {
            var rotatedPos = (uint)pos;
            if (ActiveRot == RotDir.CCW)
                rotatedPos = (rotatedPos + 1) % 4;
            else if (ActiveRot == RotDir.CW)
                rotatedPos = (rotatedPos - 1) % 4;
            return ref _activeBehemoth[rotatedPos];
        }

        public SnakePos ActiveRotatedSnakes()
        {
            if (ActiveSnakes == SnakePos.None)
                return SnakePos.None;
            var rotatedSnakes = (uint)ActiveSnakes;
            if (ActiveRot == RotDir.CW)
                rotatedSnakes = (rotatedSnakes + 1) % (int)SnakePos.None;
            else if (ActiveRot == RotDir.CCW)
                rotatedSnakes = (rotatedSnakes - 1) % (int)SnakePos.None;
            return (SnakePos)rotatedSnakes;
        }

        public float Scale = 1F;

        private float ArenaSize => 300 * Scale;
        private float MarginSize => 30 * Scale;
        private float ExoHalfSize => 20 * Scale / 2;
        private float SnakeRadius => 20 * Scale / 2;
        private float LineLen => 9 * Scale;
        private float RotHalfSize => 20 * Scale / 2;

        // colors are ABGR
        private uint _colorAOE = 0xff00ffff;
        private uint _colorArenaBorder = 0xffffffff;
        private uint _colorExoTri = 0xffff80d0;
        private uint _colorExoSq = 0xff5e34d3;
        private uint _colorParaBird = 0xff00ff00;
        private uint _colorParaBehemoth = 0xff00ffff;
        private uint _colorParaSnake = 0xffefad33;
        private uint _colorFireLine = 0xff0000ff;
        private uint _colorRotMarker = 0xffffff00;

        public void Draw(Control controls)
        {
            var topLeft = ImGui.GetCursorScreenPos();
            var drawList = ImGui.GetWindowDrawList();

            var marginSize = MarginSize;
            var arenaSize = ArenaSize;
            var arenaTL = topLeft + new Vector2(marginSize);
            var arenaBR = arenaTL + new Vector2(arenaSize);

            // draw birds/behemoths
            if (controls.HasFlag(Control.Birds))
            {
                bool allowBehemoths = controls.HasFlag(Control.Behemoths);
                DrawBird(drawList, arenaTL + new Vector2(arenaSize * 1 / 4, arenaSize * 1 / 4), allowBehemoths, ref ActiveRotatedBehemoth(BirdPos.TL));
                DrawBird(drawList, arenaTL + new Vector2(arenaSize * 3 / 4, arenaSize * 1 / 4), allowBehemoths, ref ActiveRotatedBehemoth(BirdPos.TR));
                DrawBird(drawList, arenaTL + new Vector2(arenaSize * 3 / 4, arenaSize * 3 / 4), allowBehemoths, ref ActiveRotatedBehemoth(BirdPos.BR));
                DrawBird(drawList, arenaTL + new Vector2(arenaSize * 1 / 4, arenaSize * 3 / 4), allowBehemoths, ref ActiveRotatedBehemoth(BirdPos.BL));
            }

            // draw arena with danger zones
            DrawArena(drawList, arenaTL, arenaBR, controls.HasFlag(Control.SideSmash), controls.HasFlag(Control.Diagonal));

            // draw controls
            if (controls.HasFlag(Control.ExoSides))
            {
                DrawExo(drawList, arenaTL + new Vector2(-marginSize / 2, arenaSize / 2), Vector2.UnitY, ref ActiveExo(ExoSide.Left));
                DrawExo(drawList, arenaBR - new Vector2(-marginSize / 2, arenaSize / 2), Vector2.UnitY, ref ActiveExo(ExoSide.Right));
            }
            if (controls.HasFlag(Control.ExoFront))
            {
                DrawExo(drawList, arenaTL + new Vector2(arenaSize / 2, -marginSize / 2), Vector2.UnitX, ref ActiveExo(ExoSide.Top));
            }
            if (controls.HasFlag(Control.ExoBack))
            {
                DrawExo(drawList, arenaBR - new Vector2(arenaSize / 2, -marginSize / 2), Vector2.UnitX, ref ActiveExo(ExoSide.Bottom));
            }
            if (controls.HasFlag(Control.SnakesH))
            {
                DrawSnakes(drawList, arenaTL, Vector2.UnitY, Vector2.UnitX, SnakePos.HTop, SnakePos.HBottom);
                DrawSnakes(drawList, arenaBR, -Vector2.UnitY, -Vector2.UnitX, SnakePos.HBottom, SnakePos.HTop);
            }
            if (controls.HasFlag(Control.SnakesV))
            {
                DrawSnakes(drawList, arenaTL, Vector2.UnitX, Vector2.UnitY, SnakePos.VLeft, SnakePos.VRight);
                DrawSnakes(drawList, arenaBR, -Vector2.UnitX, -Vector2.UnitY, SnakePos.VRight, SnakePos.VLeft);
            }
            if (controls.HasFlag(Control.Lines))
            {
                DrawFireLine(drawList, arenaTL, LinePos.TL);
                DrawFireLine(drawList, arenaTL + new Vector2(arenaSize, 0), LinePos.TR);
            }
            if (controls.HasFlag(Control.Rot))
            {
                DrawRotationMarker(drawList, topLeft + new Vector2(marginSize + arenaSize * 1 / 4, marginSize / 2), RotDir.CCW);
                DrawRotationMarker(drawList, topLeft + new Vector2(marginSize + arenaSize * 3 / 4, marginSize / 2), RotDir.CW);
            }

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + arenaSize + marginSize * 2);
        }

        public void Clear()
        {
            for (int i = 0; i < _activeExo.Length; ++i)
                _activeExo[i] = ExoType.None;
            for (int i = 0; i < _activeBehemoth.Length; ++i)
                _activeBehemoth[i] = false;
            ActiveSnakes = SnakePos.None;
            ActiveLine = LinePos.None;
            ActiveRot = RotDir.None;
        }

        private void DrawBird(ImDrawListPtr drawList, Vector2 centerPos, bool allowBehemoths, ref bool isBehemoth)
        {
            var r = SnakeRadius;
            drawList.AddCircleFilled(centerPos, r, isBehemoth ? _colorParaBehemoth : _colorParaBird);

            if (allowBehemoths && IsSquareClicked(centerPos, r))
            {
                isBehemoth = !isBehemoth;
            }
        }

        private void DrawArena(ImDrawListPtr drawList, Vector2 tl, Vector2 br, bool sideSmash, bool diagonalCharge)
        {
            var arenaHalfSize = ArenaSize / 2;
            var center = tl + new Vector2(arenaHalfSize);
            DrawExoAOE(drawList, tl + Vector2.UnitY * arenaHalfSize,  Vector2.UnitX, Vector2.UnitY * arenaHalfSize, ActiveExo(ExoSide.Left));
            DrawExoAOE(drawList, br - Vector2.UnitY * arenaHalfSize, -Vector2.UnitX, Vector2.UnitY * arenaHalfSize, ActiveExo(ExoSide.Right));
            DrawExoAOE(drawList, tl + Vector2.UnitX * arenaHalfSize,  Vector2.UnitY, Vector2.UnitX * arenaHalfSize, ActiveExo(ExoSide.Top));
            DrawExoAOE(drawList, br - Vector2.UnitX * arenaHalfSize, -Vector2.UnitY, Vector2.UnitX * arenaHalfSize, ActiveExo(ExoSide.Bottom));
            DrawSnakesAOE(drawList, tl, ActiveRotatedSnakes());
            DrawFireAOE(drawList, center);

            if (sideSmash)
                DrawSideSmashAOE(drawList, tl, br);
            if (diagonalCharge)
                DrawDiagonalAOE(drawList, center);

            drawList.AddRect(tl, br, _colorArenaBorder);
        }

        private void DrawExoAOE(ImDrawListPtr drawList, Vector2 origin, Vector2 direction, Vector2 originToCorner, ExoType type)
        {
            switch (type)
            {
                case ExoType.Tri:
                    DrawIsoscelesTriangle(drawList, origin, direction * ArenaSize, originToCorner, _colorAOE);
                    break;
                case ExoType.Sq:
                    drawList.AddRectFilled(origin - originToCorner, origin + originToCorner + direction * ArenaSize / 2, _colorAOE);
                    break;
            }
        }

        private void DrawSnakesAOE(ImDrawListPtr drawList, Vector2 tl, SnakePos pos)
        {
            var s = ArenaSize;
            switch (pos)
            {
                case SnakePos.HTop:
                    DrawSnakeAOERects(drawList, tl + new Vector2(0, 0), new Vector2(0,  s / 4), new Vector2( s, 0));
                    break;
                case SnakePos.VRight:
                    DrawSnakeAOERects(drawList, tl + new Vector2(s, 0), new Vector2(-s / 4, 0), new Vector2(0,  s));
                    break;
                case SnakePos.HBottom:
                    DrawSnakeAOERects(drawList, tl + new Vector2(s, s), new Vector2(0, -s / 4), new Vector2(-s, 0));
                    break;
                case SnakePos.VLeft:
                    DrawSnakeAOERects(drawList, tl + new Vector2(0, s), new Vector2( s / 4, 0), new Vector2(0, -s));
                    break;
            }
        }

        private void DrawSnakeAOERects(ImDrawListPtr drawList, Vector2 origin, Vector2 toNextSnake, Vector2 snakeDir)
        {
            drawList.AddRectFilled(origin, origin + toNextSnake + snakeDir, _colorAOE);
            drawList.AddRectFilled(origin + 2 * toNextSnake, origin + 3 * toNextSnake + snakeDir, _colorAOE);
        }

        private void DrawFireAOE(ImDrawListPtr drawList, Vector2 arenaCenter)
        {
            if (ActiveLine == LinePos.None || ActiveRot == RotDir.None)
                return;

            var height = new Vector2();
            var halfBase = new Vector2();
            if ((ActiveLine == LinePos.TL) ^ (ActiveRot == RotDir.CW))
                height.X = halfBase.Y = ArenaSize / 2; // horizontal
            else
                height.Y = halfBase.X = ArenaSize / 2; // vertical

            DrawIsoscelesTriangle(drawList, arenaCenter,  height, halfBase, _colorAOE);
            DrawIsoscelesTriangle(drawList, arenaCenter, -height, halfBase, _colorAOE);
        }

        private void DrawSideSmashAOE(ImDrawListPtr drawList, Vector2 tl, Vector2 br)
        {
            var r = ArenaSize / 2;
            drawList.PushClipRect(tl, br);
            drawList.AddCircleFilled(tl + new Vector2(0, r), r, _colorAOE);
            drawList.AddCircleFilled(br - new Vector2(0, r), r, _colorAOE);
            drawList.PopClipRect();
        }

        private void DrawDiagonalAOE(ImDrawListPtr drawList, Vector2 centerPos)
        {
            if (ActiveLine == LinePos.None || ActiveRot != RotDir.None)
                return;

            var h = ArenaSize / 2;
            var x = ActiveLine == LinePos.TR ? h : -h;
            var points = new Vector2[] {
                centerPos + new Vector2( 0, -h),
                centerPos + new Vector2( x, -h),
                centerPos + new Vector2( x,  0),
                centerPos + new Vector2( 0,  h),
                centerPos + new Vector2(-x,  h),
                centerPos + new Vector2(-x,  0),
            };
            drawList.AddConvexPolyFilled(ref points[0], points.Length, _colorAOE);
        }

        private void DrawExo(ImDrawListPtr drawList, Vector2 centerPos, Vector2 offsetDir, ref ExoType active)
        {
            var h = ExoHalfSize;
            var triCenter = centerPos - offsetDir * (h + 2);
            var sqCenter = centerPos + offsetDir * (h + 2);

            if (active == ExoType.Tri)
                drawList.AddTriangleFilled(new Vector2(triCenter.X, triCenter.Y - h), new Vector2(triCenter.X + h, triCenter.Y + h), new Vector2(triCenter.X - h, triCenter.Y + h), _colorExoTri);
            else
                drawList.AddTriangle(new Vector2(triCenter.X, triCenter.Y - h), new Vector2(triCenter.X + h, triCenter.Y + h), new Vector2(triCenter.X - h, triCenter.Y + h), _colorExoTri, 2);
            if (active == ExoType.Sq)
                drawList.AddRectFilled(new Vector2(sqCenter.X - h, sqCenter.Y - h), new Vector2(sqCenter.X + h, sqCenter.Y + h), _colorExoSq);
            else
                drawList.AddRect(new Vector2(sqCenter.X - h, sqCenter.Y - h), new Vector2(sqCenter.X + h, sqCenter.Y + h), _colorExoSq, 0, ImDrawFlags.None, 2);

            if (IsSquareClicked(triCenter, h))
            {
                active = active == ExoType.Tri ? ExoType.None : ExoType.Tri;
            }
            else if (IsSquareClicked(sqCenter, h))
            {
                active = active == ExoType.Sq ? ExoType.None : ExoType.Sq;
            }
        }

        private void DrawSnakes(ImDrawListPtr drawList, Vector2 cornerPos, Vector2 dir, Vector2 ortho, SnakePos snake13, SnakePos snake24)
        {
            var p1 = cornerPos + dir * ArenaSize / 8 + ortho * (SnakeRadius + 2);
            var offset = dir * (ArenaSize / 4);
            DrawSnake(drawList, p1 + 0 * offset, snake13);
            DrawSnake(drawList, p1 + 1 * offset, snake24);
            DrawSnake(drawList, p1 + 2 * offset, snake13);
            DrawSnake(drawList, p1 + 3 * offset, snake24);
        }

        private void DrawSnake(ImDrawListPtr drawList, Vector2 centerPos, SnakePos active)
        {
            var r = SnakeRadius;
            if (ActiveSnakes == active)
                drawList.AddCircleFilled(centerPos, r, _colorParaSnake);
            else
                drawList.AddCircle(centerPos, r, _colorParaSnake, 0, 2);

            if (IsSquareClicked(centerPos, r))
            {
                ActiveSnakes = ActiveSnakes == active ? SnakePos.None : active;
            }
        }

        private void DrawFireLine(ImDrawListPtr drawList, Vector2 cornerPos, LinePos active)
        {
            var l = LineLen;
            float sign = active == LinePos.TL ? 1 : -1;
            float len = ActiveLine == active ? ArenaSize : l + 1;
            drawList.AddLine(cornerPos - new Vector2(sign * l, l), cornerPos + new Vector2(sign * len, len), _colorFireLine, 2);

            if (IsSquareClicked(cornerPos, l))
            {
                ActiveLine = ActiveLine == active ? LinePos.None : active;
            }
        }

        private void DrawRotationMarker(ImDrawListPtr drawList, Vector2 centerPos, RotDir active)
        {
            var l = RotHalfSize;
            float sign = active == RotDir.CW ? 1 : -1;
            var p1 = centerPos + sign * new Vector2(-l, -l);
            var p2 = centerPos + sign * new Vector2(0, -l);
            var p3 = centerPos + sign * new Vector2(l, 0);
            var p4 = centerPos + sign * new Vector2(0, l);
            var p5 = centerPos + sign * new Vector2(-l, l);
            if (ActiveRot == active)
            {
                drawList.AddQuadFilled(p1, p2, p3, centerPos, _colorRotMarker);
                drawList.AddQuadFilled(centerPos, p3, p4, p5, _colorRotMarker);
            }
            else
            {
                var points = new Vector2[] { p1, p2, p3, p4, p5, centerPos };
                drawList.AddPolyline(ref points[0], points.Length, _colorRotMarker, ImDrawFlags.Closed, 2);
            }

            if (IsSquareClicked(centerPos, l))
            {
                ActiveRot = ActiveRot == active ? RotDir.None : active;
            }
        }

        private void DrawIsoscelesTriangle(ImDrawListPtr drawList, Vector2 apex, Vector2 height, Vector2 halfBase, uint color)
        {
            drawList.AddTriangleFilled(apex, apex + height + halfBase, apex + height - halfBase, color);
        }

        private bool IsSquareClicked(Vector2 centerPos, float halfSize)
        {
            if (!ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                return false;
            var pos = ImGui.GetMousePos();
            return pos.X >= centerPos.X - halfSize && pos.X <= centerPos.X + halfSize && pos.Y >= centerPos.Y - halfSize && pos.Y <= centerPos.Y + halfSize;
        }
    }
}
