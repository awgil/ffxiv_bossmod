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

        private Map.Coverage _blockCircle = Map.Coverage.Outside;
        private Vector2 _blockCircleCenter = new(0, 1);
        private float _blockCircleRadius = 2;
        private float _blockCircleG = 0;

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

                rebuild |= DrawCoverage("Block circle", ref _blockCircle);
                if (_blockCircle != Map.Coverage.None)
                {
                    rebuild |= ImGui.DragFloat2("Block circle: center", ref _blockCircleCenter, 0.25f);
                    rebuild |= ImGui.DragFloat("Block circle: radius", ref _blockCircleRadius, 0.25f, 0, 30);
                    rebuild |= ImGui.DragFloat("Block circle: max-g", ref _blockCircleG, 1, 0, 100);
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
            if (_blockCircle != Map.Coverage.None)
            {
                visu.Map.BlockPixels(visu.Map.RasterizeCircle(new(_blockCircleCenter), _blockCircleRadius, _blockCircle), _blockCircleG);
                visu.Circles.Add((new(_blockCircleCenter), _blockCircleRadius));
            }
            return visu;
        }
    }
}
