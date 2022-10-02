using BossMod;
using BossMod.Pathfinding;
using ImGuiNET;
using System.Numerics;

namespace UIDev
{
    class PathfindingTest : ITest
    {
        private MapVisualizer _visu;
        private float _mapResolution = 1;
        private Vector2 _mapCenter;
        private Vector2 _mapHalfSize = new(20, 20);
        private float _mapRotationDeg;

        private Map.Coverage _blockCone = Map.Coverage.Outside;
        private Vector2 _blockConeCenter = new(0, 1);
        private Vector2 _blockConeRadius = new(0, 10);
        private float _blockConeRotationDeg;
        private float _blockConeHalfAngle = 180;
        private float _blockConeG = 0;

        private Map.Coverage _blockRect = Map.Coverage.None;
        private Vector2 _blockRectCenter = new(0, 0);
        private Vector2 _blockRectLen = new(20, 20);
        private float _blockRectHalfWidth = 20;
        private float _blockRectRotationDeg;
        private float _blockRectG = 0;

        public PathfindingTest()
        {
            _visu = RebuildMap();
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            _visu.Draw();

            bool rebuild = false;
            if (ImGui.CollapsingHeader("Map setup"))
            {
                rebuild |= ImGui.DragFloat("Resolution", ref _mapResolution, 0.1f, 0.1f, 10, "%.1f", ImGuiSliderFlags.Logarithmic);
                rebuild |= ImGui.DragFloat2("Center", ref _mapCenter);
                rebuild |= ImGui.DragFloat2("Half-size", ref _mapHalfSize, 1, 0, 30);
                rebuild |= ImGui.DragFloat("Rotation", ref _mapRotationDeg, 5, -180, 180);

                rebuild |= DrawCoverage("Block cone", ref _blockCone);
                if (_blockCone != Map.Coverage.None)
                {
                    rebuild |= ImGui.DragFloat2("Block cone: center", ref _blockConeCenter, 0.25f);
                    rebuild |= ImGui.DragFloat2("Block cone: radius", ref _blockConeRadius, 0.25f, 0, 30);
                    rebuild |= ImGui.DragFloat("Block cone: direction", ref _blockConeRotationDeg, 5, -180, 180);
                    rebuild |= ImGui.DragFloat("Block cone: half-angle", ref _blockConeHalfAngle, 5, 0, 180);
                    rebuild |= ImGui.DragFloat("Block cone: max-g", ref _blockConeG, 1, 0, 100);
                }

                rebuild |= DrawCoverage("Block rect", ref _blockRect);
                if (_blockRect != Map.Coverage.None)
                {
                    rebuild |= ImGui.DragFloat2("Block rect: center", ref _blockRectCenter, 0.25f);
                    rebuild |= ImGui.DragFloat2("Block rect: length", ref _blockRectLen, 0.25f, 0, 30);
                    rebuild |= ImGui.DragFloat("Block rect: half width", ref _blockRectHalfWidth, 1, 0, 30);
                    rebuild |= ImGui.DragFloat("Block rect: direction", ref _blockRectRotationDeg, 5, -180, 180);
                    rebuild |= ImGui.DragFloat("Block rect: max-g", ref _blockRectG, 1, 0, 100);
                }
            }

            if (rebuild)
                _visu = RebuildMap();
        }

        public ImGuiWindowFlags WindowFlags() { return ImGuiWindowFlags.None; }

        private bool DrawCoverage(string label, ref Map.Coverage v)
        {
            var castV = (int)v;
            bool changed = false;
            ImGui.TextUnformatted(label);
            ImGui.SameLine();
            changed |= ImGui.CheckboxFlags($"Inside##{label}", ref castV, (int)Map.Coverage.Inside);
            ImGui.SameLine();
            changed |= ImGui.CheckboxFlags($"Border##{label}", ref castV, (int)Map.Coverage.Border);
            ImGui.SameLine();
            changed |= ImGui.CheckboxFlags($"Outside##{label}", ref castV, (int)Map.Coverage.Outside);
            if (changed)
                v = (Map.Coverage)castV;
            return changed;
        }

        private MapVisualizer RebuildMap()
        {
            MapVisualizer visu = new(new(_mapResolution, new(_mapCenter), _mapHalfSize.X, _mapHalfSize.Y, _mapRotationDeg.Degrees()));
            if (_blockCone != Map.Coverage.None)
            {
                visu.Map.BlockPixels(visu.Map.RasterizeDonutSector(new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees()), _blockConeG, _blockCone);
                visu.Sectors.Add((new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees()));
            }
            if (_blockRect != Map.Coverage.None)
            {
                visu.Map.BlockPixels(visu.Map.RasterizeRect(new(_blockRectCenter), _blockRectRotationDeg.Degrees(), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth), _blockRectG, _blockRect);
                visu.Rects.Add((new(_blockRectCenter), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth, _blockRectRotationDeg.Degrees()));
            }
            return visu;
        }
    }
}
