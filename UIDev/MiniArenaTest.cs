using ImGuiNET;
using System.Numerics;
using BossMod;
using System;
using System.Collections.Generic;

namespace UIDev
{
    class MiniArenaTest : ITest
    {
        private MiniArena _arena = new();
        private float _azimuth;
        private float _altitude = 90;
        //private bool _lineEnabled;
        private bool _coneEnabled = true;
        private List<Vector3> _shapeVertices = new();
        private Vector4 _lineEnds = new(90, 90, 110, 110);
        private Vector3 _playerPos = new(100, 0, 90);
        private Vector3 _conePos = new(100, 0, 100);
        private Vector2 _coneRadius = new(0, 62);
        private Vector2 _coneAngles = new(135, 397);

        public void Dispose()
        {
        }

        public void Draw()
        {
            ImGui.DragFloat("Camera azimuth", ref _azimuth, 1, -180, +180);
            ImGui.DragFloat("Camera altitude", ref _altitude, 1, -90, +90);
            ImGui.Checkbox("Circle shape", ref _arena.IsCircle);

            _arena.Begin((float)(Math.PI * _azimuth / 180));
            if (_coneEnabled)
                _arena.ZoneCone(_conePos, _coneRadius.X, _coneRadius.Y, _coneAngles.X / 180 * MathF.PI, _coneAngles.Y / 180 * MathF.PI, _arena.ColorSafe);
            _arena.Border();
            _arena.Actor(_playerPos, 0, 0xff00ff00);
            _arena.End();

            // arena config
            ImGui.DragFloat3("Player pos", ref _playerPos);
            ImGui.Checkbox("Draw cone", ref _coneEnabled);
            if (_coneEnabled)
            {
                ImGui.DragFloat3("Center", ref _conePos);
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

            //ImGui.Checkbox("Draw line", ref _lineEnabled);
            //if (_lineEnabled)
            //{
            //    ImGui.DragFloat4("Endpoints", ref _lineEnds);
            //    var a = new Vector3(_lineEnds.X, 0, _lineEnds.Y);
            //    var b = new Vector3(_lineEnds.Z, 0, _lineEnds.W);
            //    var clipped = _circle
            //        ? MiniArena.ClipLineToCircle(ref a, ref b, _arena.WorldCenter, _arena.WorldHalfSize)
            //        : MiniArena.ClipLineToRect(ref a, ref b, _arena.WorldNW, _arena.WorldSE);
            //    _arena.AddLine(a, b, 0xffff0000);
            //}
        }
    }
}
