using ImGuiNET;
using System.Numerics;
using BossMod;
using System;
using System.Collections.Generic;

namespace UIDev
{
    class MiniArenaTest : TestWindow
    {
        private MiniArena _arena = new(new(), new ArenaBoundsSquare(new(100, 100), 20));
        private bool _arenaIsCircle = false;
        private float _azimuth = -72;
        private float _altitude = 90;
        private bool _lineEnabled;
        private bool _coneEnabled = true;
        private bool _kbContourEnabled = true;
        private List<Vector3> _shapeVertices = new();
        private Vector4 _lineEnds = new(90, 90, 110, 110);
        private Vector2 _playerPos = new(100, 90);
        private Vector2 _conePos = new(100, 80);
        private Vector2 _coneRadius = new(0, 100);
        private Vector2 _coneAngles = new(185, 161);
        private Vector2 _kbCenter = new(110, 100);
        private float _kbDistance = 5;

        public MiniArenaTest() : base("Arena test", new(400, 400), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) { }

        public override void Draw()
        {
            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, +180);
            ImGui.DragFloat("Camera altitude", ref _altitude, 1, -90, +90);
            if (ImGui.Checkbox("Circle shape", ref _arenaIsCircle))
            {
                _arena.Bounds = _arenaIsCircle ? new ArenaBoundsCircle(_arena.Bounds.Center, _arena.Bounds.HalfSize) : new ArenaBoundsSquare(_arena.Bounds.Center, _arena.Bounds.HalfSize);
            }

            _arena.Begin((float)(Math.PI * _azimuth / 180));
            if (_coneEnabled)
                _arena.ZoneCone(new(_conePos), _coneRadius.X, _coneRadius.Y, _coneAngles.X.Degrees(), _coneAngles.Y.Degrees(), ArenaColor.Safe);
            _arena.Border(ArenaColor.Border);
            if (_lineEnabled)
                _arena.AddLine(new(_lineEnds.X, _lineEnds.Y), new(_lineEnds.Z, _lineEnds.W), 0xffff0000);
            if (_kbContourEnabled)
            {
                foreach (var p in KBContour())
                    _arena.PathLineTo(p);
                _arena.PathStroke(true, 0xffff00ff);
            }
            _arena.Actor(new(_playerPos), 0.Degrees(), 0xff00ff00);
            _arena.End();

            // arena config
            ImGui.DragFloat2("Player pos", ref _playerPos);
            ImGui.Checkbox("Draw cone", ref _coneEnabled);
            if (_coneEnabled)
            {
                ImGui.DragFloat2("Center", ref _conePos);
                ImGui.DragFloat2("Radius", ref _coneRadius, 1, 0);
                ImGui.DragFloat2("Angles", ref _coneAngles);
            }
            ImGui.Checkbox("Knockback contour", ref _kbContourEnabled);
            if (_kbContourEnabled)
            {
                ImGui.DragFloat2("KB center", ref _kbCenter);
                ImGui.DragFloat("KB distance", ref _kbDistance);
            }

            if (ImGui.TreeNode("Clipped shape"))
            {
                for (int i = 0; i < _shapeVertices.Count; ++i)
                {
                    var v = _shapeVertices[i];
                    if (ImGui.DragFloat3($"Vertex {i}", ref v))
                        _shapeVertices[i] = v;
                    ImGui.SameLine();
                    if (ImGui.Button($"Delete##{i}"))
                        _shapeVertices.RemoveAt(i--);
                }
                if (ImGui.Button("Add"))
                    _shapeVertices.Add(new());
                ImGui.TreePop();
            }

            //var clippedShape = MiniArena.ClipPolygonToRect(_shapeVertices, _arena.WorldNW, _arena.WorldSE);
            //if (clippedShape.Count > 2)
            //{
            //    foreach (var v in clippedShape)
            //        _arena.PathLineTo(v);
            //    _arena.PathStroke(true, 0xff808080, 2);
            //}

            ImGui.Checkbox("Draw line", ref _lineEnabled);
            if (_lineEnabled)
                ImGui.DragFloat4("Endpoints", ref _lineEnds);
        }

        private IEnumerable<WPos> KBContour()
        {
            int cnt = 256;
            float coeff = 2 * MathF.PI / cnt;
            WPos kbCenter = new(_kbCenter);
            WDir centerOffset = kbCenter - _arena.Bounds.Center;
            var c = centerOffset.LengthSq() - _arena.Bounds.HalfSize * _arena.Bounds.HalfSize;
            for (int i = 0; i < cnt; ++i)
            {
                Angle phi = (i * coeff).Radians();
                var dir = phi.ToDirection();
                var offDotDir = dir.Dot(centerOffset);
                var d = -offDotDir + MathF.Sqrt(offDotDir * offDotDir - c);
                yield return kbCenter + (d - _kbDistance) * dir;
            }
        }
    }
}
