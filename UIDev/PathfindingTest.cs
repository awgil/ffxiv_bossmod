using BossMod;
using BossMod.Pathfinding;
using ImGuiNET;

namespace UIDev;

class PathfindingTest : TestWindow
{
    private MapVisualizer _visu;

    private float _mapResolution = 1;
    private float _mapThreshold = 0.5f;
    private Vector2 _mapCenter;
    private Vector2 _mapHalfSize = new(20, 20);
    private float _mapRotationDeg;

    private Vector2 _startingPos = new(-15, 0);
    private Vector2 _targetPos = new(15, 0);
    private float _targetRadius = 10;
    private float _targetFacingDeg;
    private int _goalPrio = 15;

    private bool _blockCone = true;
    private Vector2 _blockConeCenter = new(0, 1);
    private Vector2 _blockConeRadius = new(0, 10);
    private float _blockConeRotationDeg;
    private float _blockConeHalfAngle = 180;
    private float _blockConeG;

    private bool _blockRect;
    private Vector2 _blockRectCenter = new(0, 0);
    private Vector2 _blockRectLen = new(20, 20);
    private float _blockRectHalfWidth = 20;
    private float _blockRectRotationDeg;
    private float _blockRectG;

    public PathfindingTest() : base("Pathfinding test", new(400, 400), ImGuiWindowFlags.None)
    {
        _visu = RebuildMap();
    }

    public override void Draw()
    {
        _visu.Draw();

        bool rebuild = false;
        if (ImGui.CollapsingHeader("Map setup"))
        {
            rebuild |= ImGui.DragFloat("Resolution", ref _mapResolution, 0.1f, 0.1f, 10, "%.1f", ImGuiSliderFlags.Logarithmic);
            rebuild |= ImGui.DragFloat("Block zone threshold", ref _mapThreshold, 0.1f, -5, 5);
            rebuild |= ImGui.DragFloat2("Center", ref _mapCenter);
            rebuild |= ImGui.DragFloat2("Half-size", ref _mapHalfSize, 1, 0, 30);
            rebuild |= ImGui.DragFloat("Rotation", ref _mapRotationDeg, 5, -180, 180);

            rebuild |= ImGui.DragFloat2("Starting position", ref _startingPos, 1, -30, 30);
            rebuild |= ImGui.DragFloat2("Target position", ref _targetPos, 1, -30, 30);
            rebuild |= ImGui.DragFloat("Target radius", ref _targetRadius, 1, 0, 30);
            rebuild |= ImGui.DragFloat("Target direction", ref _targetFacingDeg, 5, -180, 180);

            rebuild |= ImGui.DragInt("Goal priority", ref _goalPrio, 1, 0, 100);

            rebuild |= ImGui.Checkbox("Block cone", ref _blockCone);
            if (_blockCone)
            {
                rebuild |= ImGui.DragFloat2("Block cone: center", ref _blockConeCenter, 0.25f);
                rebuild |= ImGui.DragFloat2("Block cone: radius", ref _blockConeRadius, 0.25f, 0, 30);
                rebuild |= ImGui.DragFloat("Block cone: direction", ref _blockConeRotationDeg, 5, -180, 180);
                rebuild |= ImGui.DragFloat("Block cone: half-angle", ref _blockConeHalfAngle, 5, 0, 180);
                rebuild |= ImGui.DragFloat("Block cone: max-g", ref _blockConeG, 1, 0, 100);
            }

            rebuild |= ImGui.Checkbox("Block rect", ref _blockRect);
            if (_blockRect)
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

    private MapVisualizer RebuildMap()
    {
        Map map = new(_mapResolution, new(_mapCenter), _mapHalfSize.X, _mapHalfSize.Y, _mapRotationDeg.Degrees());
        if (_blockCone)
            map.BlockPixelsInside(ShapeDistance.DonutSector(new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees()), _blockConeG, _mapThreshold);
        if (_blockRect)
            map.BlockPixelsInside(ShapeDistance.Rect(new(_blockRectCenter), _blockRectRotationDeg.Degrees(), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth), _blockRectG, _mapThreshold);
        map.AddGoal(ShapeDistance.Circle(new(_targetPos), _targetRadius), 0, 0, 10);
        map.AddGoal(ShapeDistance.Cone(new(_targetPos), _targetRadius, _targetFacingDeg.Degrees(), 45.Degrees()), 0, 10, 5);

        var visu = new MapVisualizer(map, _goalPrio, new(_startingPos));

        if (_blockCone)
            visu.Sectors.Add((new(_blockConeCenter), _blockConeRadius.X, _blockConeRadius.Y, _blockConeRotationDeg.Degrees(), _blockConeHalfAngle.Degrees()));
        if (_blockRect)
            visu.Rects.Add((new(_blockRectCenter), _blockRectLen.X, _blockRectLen.Y, _blockRectHalfWidth, _blockRectRotationDeg.Degrees()));

        return visu;
    }
}
