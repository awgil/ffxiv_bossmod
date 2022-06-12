using ImGuiNET;
using System.Numerics;
using BossMod;
using System;
using System.Collections.Generic;

namespace UIDev
{
    class MiniArenaTest : ITest
    {
        private MiniArena _arena = new(new(), new ArenaBoundsSquare(new(100, 100), 20));
        private bool _arenaIsCircle = false;
        private float _azimuth = -72;
        private float _altitude = 90;
        private bool _lineEnabled;
        private bool _coneEnabled = true;
        private List<Vector3> _shapeVertices = new();
        private Vector4 _lineEnds = new(90, 90, 110, 110);
        private Vector2 _playerPos = new(100, 90);
        private Vector2 _conePos = new(100, 80);
        private Vector2 _coneRadius = new(0, 100);
        private Vector2 _coneAngles = new(185, 161);

        public void Dispose()
        {
        }

        public void Draw()
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
            _arena.Border();
            if (_lineEnabled)
                _arena.AddLine(new(_lineEnds.X, _lineEnds.Y), new(_lineEnds.Z, _lineEnds.W), 0xffff0000);
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
    }
}
