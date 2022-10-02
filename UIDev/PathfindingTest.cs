using BossMod;
using BossMod.Pathfinding;
using ImGuiNET;
using System;
using System.Linq;
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

        private Vector2 _startingPos = new(-15, 0);
        private Vector2 _targetPos = new(15, 0);
        private float _targetRadius = 10;
        private float _targetFacingDeg;
        private int _goalPrio = 15;

        private Map.Coverage _blockCone = Map.Coverage.Inside | Map.Coverage.Border;
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

                rebuild |= ImGui.DragFloat2("Starting position", ref _startingPos, 1, -30, 30);
                rebuild |= ImGui.DragFloat2("Target position", ref _targetPos, 1, -30, 30);
                rebuild |= ImGui.DragFloat("Target radius", ref _targetRadius, 1, 0, 30);
                rebuild |= ImGui.DragFloat("Target direction", ref _targetFacingDeg, 5, -180, 180);

                rebuild |= ImGui.DragInt("Goal priority", ref _goalPrio, 1, 0, 100);

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
            Map map = new(_mapResolution, new(_mapCenter), _mapHalfSize.X, _mapHalfSize.Y, _mapRotationDeg.Degrees());
            if (_blockCone != Map.Coverage.None)
                map.BlockPixels(map.RasterizeDonutSector(new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees()), _blockConeG, _blockCone);
            if (_blockRect != Map.Coverage.None)
                map.BlockPixels(map.RasterizeRect(new(_blockRectCenter), _blockRectRotationDeg.Degrees(), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth), _blockRectG, _blockRect);
            map.AddGoal(map.RasterizeCircle(new(_targetPos), _targetRadius), Map.Coverage.Inside, 0, 10);
            map.AddGoal(map.RasterizeCone(new(_targetPos), _targetRadius, _targetFacingDeg.Degrees(), 45.Degrees()), Map.Coverage.Inside, 10, 5);

            var visu = new MapVisualizer(map, _goalPrio, new(_startingPos));

            if (_blockCone != Map.Coverage.None)
                visu.Sectors.Add((new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees()));
            if (_blockRect != Map.Coverage.None)
                visu.Rects.Add((new(_blockRectCenter), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth, _blockRectRotationDeg.Degrees()));

            return visu;
        }

        //private (ThetaStar, int) RebuildPathfinding(Map map)
        //{
        //    var start = map.WorldToGrid(new(_startingPos));
        //    if (start.x < 0 || start.x >= map.Width || start.y < 0 || start.y >= map.Height)
        //    {
        //        var pathfind = new ThetaStar(map, Enumerable.Empty<(int, int)>(), 0, 0);
        //        var pathfindResult = pathfind.Execute();
        //        return (pathfind, pathfindResult);
        //    }

        //    var goals = map.Goals().ToList();
        //    goals.SortByReverse(x => x.priority);
        //    for (int begin = 0; begin < goals.Count;)
        //    {
        //        int prio = goals[begin].priority;
        //        int end = begin + 1;
        //        while (end < goals.Count && goals[end].priority == prio)
        //            ++end;
        //        var pathfind = new ThetaStar(map, goals.Skip(begin).Take(end - begin).Select(x => (x.x, x.y)), start.x, start.y);
        //        var pathfindResult = pathfind.Execute();
        //        if (pathfindResult >= 0)
        //            return (pathfind, pathfindResult);
        //        begin = end;
        //    }

        //    return (new ThetaStar(map, Enumerable.Empty<(int, int)>(), 0, 0), -1);
        //}
    }
}
