using Dalamud.Bindings.ImGui;

namespace UIDev.vfgPathfind;

class FloodFillTest : TestWindow
{
    private FloodFillVisualizer _visu;
    private float _spaceRes = 0.5f;
    private float _timeRes = 0.25f;
    private float _aoeLeeway = 0.5f;
    private int _maxDeltaZ = 500;
    private float _timeToBuild;

    public FloodFillTest() : base("vfg floodfill test", new(400, 400), ImGuiWindowFlags.None)
    {
        _visu = RebuildMap();
    }

    public override void Draw()
    {
        _visu.Draw();

        bool rebuild = false;
        ImGui.TextUnformatted($"Time to pathfind: {_timeToBuild:f3}s");
        if (ImGui.CollapsingHeader("Map setup"))
        {
            rebuild |= ImGui.DragFloat("Space Resolution", ref _spaceRes, 0.1f, 0.1f, 10, "%.3f", ImGuiSliderFlags.Logarithmic);
            rebuild |= ImGui.DragFloat("Time Resolution", ref _timeRes, 0.01f, 0.1f, 10, "%.3f", ImGuiSliderFlags.Logarithmic);
            rebuild |= ImGui.DragFloat("AOE leeway", ref _aoeLeeway, 0.01f, 0, 0.5f, "%.3f");
            rebuild |= ImGui.DragInt("Max delta-z", ref _maxDeltaZ, 1, 10, 500);
        }

        if (rebuild)
            _visu = RebuildMap();
    }

    private FloodFillVisualizer RebuildMap()
    {
        var map = new Map(_spaceRes, _timeRes, new(0, 0, 223), 14, 101, 60);

        // columns at entrance
        BlockCircle(map, -4, 292, 1.5f, 0, 60, 100, 0);
        BlockCircle(map, +4, 292, 1.5f, 0, 60, 100, 0);
        BlockRect(map, -10, -3.3f, 285.5f, 293, 0, 60, 100, 0);
        BlockRect(map, +3.3f, +10, 285.5f, 293, 0, 60, 100, 0);

        // central column
        BlockRect(map, -2, 2, 271, 278, 0, 60, 100, 0);
        BlockRect(map, -5.5f, 5.5f, 262.5f, 271, 0, 60, 100, 0);

        // exaflare lane columns
        BlockRect(map, -9.5f, -5.5f, 247.5f, 256, 0, 60, 100, 0);
        BlockRect(map, -2.0f, +2.0f, 247.5f, 256, 0, 60, 100, 0);
        BlockRect(map, +5.5f, +9.5f, 247.5f, 256, 0, 60, 100, 0);
        BlockRect(map, -9.5f, -5.5f, 238.5f, 246.5f, 0, 60, 100, 0);
        BlockRect(map, -2.0f, +2.0f, 238.5f, 246.5f, 0, 60, 100, 0);
        BlockRect(map, +5.5f, +9.5f, 238.5f, 246.5f, 0, 60, 100, 0);

        // rect lane prisms
        BlockPrism(map, -9, 211, 4, 0, 60, 100);
        BlockPrism(map, +9, 211, 4, 0, 60, 100);
        BlockPrism(map, -2.5f, 202, 4, 0, 60, 100);
        BlockPrism(map, +2.5f, 202, 4, 0, 60, 100);
        BlockPrism(map, -9, 194, 4, 0, 60, 100);
        BlockPrism(map, +9, 194, 4, 0, 60, 100);
        BlockPrism(map, 0, 188, 4, 0, 60, 100);

        // stairs center
        BlockTrapezium(map, 1.1f, 151.5f, 3.3f, 139, 0, 60, 100);

        // final corridor
        BlockTrapezium(map, 4.3f, 133.5f, 8.5f, 123.7f, 0, 60, 100);
        BlockCorners(map, 14, 139, 7, 136, 13, 123, 0, 60, 100);

        // aoes
        BlockRotating(map, 1.2f, 6, 267.5f, [-10, 10]);
        BlockDoubleExaflareSequence(map, 9.39f, 251, 11.75f, 243);
        BlockRotating(map, 1.0f, 14.76f, 225.3f, [-10, 0, 10]);
        BlockDoubleExaflareSequence(map, 14.54f, 229.3f, 14.97f, 221.3f);
        BlockRectsSequence(map, -12, -6);
        BlockRectsSequence(map, 12, 6);
        BlockCentralRectsSequence(map);
        BlockSingleExaflareSequence(map, 22.52f, 198.7f, [-12, -4, 4, 12]);
        BlockPairsSequence(map, 5, new(-10, 29.95f, 170), new(-2, 29.95f, 170));
        BlockPairsSequence(map, 5, new(-10, 31.39f, 156), new(-4.34f, 30.81f, 161.66f));
        BlockPairsSequence(map, 5, new(10, 29.95f, 170), new(4.34f, 29.37f, 175.66f));
        BlockPairsSequence(map, 5, new(10, 31.39f, 156), new(2, 31.39f, 156));
        BlockSingleExaflareSequence(map, 33.47f, 145.7f, [-4, 4, 12, -12]);
        BlockPairsSequence(map, 3, new(-6.78f, 35.91f, 136.91f), new(-3.22f, 36.30f, 135.09f));
        BlockPairsSequence(map, 3, new(6.78f, 35.91f, 136.91f), new(3.22f, 36.30f, 135.09f));

        var now = DateTime.Now;
        var visu = new FloodFillVisualizer(map, new(0, 0, 323), 130, _maxDeltaZ);
        _timeToBuild = (float)(DateTime.Now - now).TotalSeconds;

        return visu;
    }

    private void BlockCircle(Map m, float cx, float cz, float radius, float tStart, float tDuration, float tRepeat, float tLeeway)
    {
        var center = new Vector2(cx, cz);
        var vr = new Vector2(radius);
        var rsq = radius * radius;
        m.BlockPixelsInside(center - vr, center + vr, v => (v - center).LengthSquared() <= rsq, tStart, tDuration, tRepeat, tLeeway);
    }

    private void BlockRect(Map m, float xmin, float xmax, float zmin, float zmax, float tStart, float tDuration, float tRepeat, float tLeeway)
    {
        m.BlockPixelsInside(new(xmin, zmin), new(xmax, zmax), _ => true, tStart, tDuration, tRepeat, tLeeway);
    }

    private void BlockPrism(Map m, float cx, float cz, float s, float tStart, float tDuration, float tRepeat)
    {
        var center = new Vector2(cx, cz);
        var vs = new Vector2(s);
        m.BlockPixelsInside(center - vs, center + vs, v => Math.Abs(v.X - cx) + Math.Abs(v.Y - cz) <= s, tStart, tDuration, tRepeat, 0);
    }

    private void BlockTrapezium(Map m, float dx1, float z1, float dx2, float z2, float tStart, float tDuration, float tRepeat)
    {
        float coeff = (dx2 - dx1) / (z2 - z1);
        float cons = dx1 - z1 * coeff;
        m.BlockPixelsInside(new(-dx2, z2), new(dx2, z1), v => Math.Abs(v.X) < cons + coeff * v.Y, tStart, tDuration, tRepeat, 0);
    }

    private void BlockCorners(Map m, float x1, float z1, float x2, float z2, float x3, float z3, float tStart, float tDuration, float tRepeat)
    {
        var a = new Vector2(x1, z1);
        var b = new Vector2(x2, z2);
        var c = new Vector2(x3, z3);
        var ab = b - a;
        var bc = c - b;
        var n1 = new Vector2(ab.Y, -ab.X);
        var n2 = new Vector2(bc.Y, -bc.X);
        m.BlockPixelsInside(new(-x1, z3), new(x1, z1), v => Vector2.Dot(n1, new(Math.Abs(v.X) - x1, v.Y - z1)) < 0 && Vector2.Dot(n2, new(Math.Abs(v.X) - x2, v.Y - z2)) < 0, tStart, tDuration, tRepeat, 0);
    }

    private void BlockRotating(Map m, float repeat, float y, float z, params float[] x)
    {
        float t = 0;
        float seqRepeat = repeat * x.Length;
        foreach (var vx in x)
        {
            BlockCircle(m, vx, z, 5, t, 0.1f, seqRepeat, _aoeLeeway);
            t += repeat;
        }
    }

    private void BlockSingleExaflareSequence(Map m, float y, float z, params float[] x)
    {
        float t = 0;
        float seqRepeat = 1.4f * x.Length + 0.5f;
        foreach (var vx in x)
        {
            BlockCircle(m, vx, z, 6, t, 0.1f, seqRepeat, _aoeLeeway);
            t += vx == x[^1] ? 1.9f : 1.4f;
        }
    }

    private void BlockDoubleExaflareSequence(Map m, float y1, float z1, float y2, float z2)
    {
        float[] x = [-12, -4, 4, 12];
        float t = 0;
        float seqRepeat = 1.4f * 8;
        foreach (var vx in x)
        {
            BlockCircle(m, vx, z1, 6, t, 0.1f, seqRepeat, _aoeLeeway);
            t += 1.4f;
        }
        foreach (var vx in x)
        {
            BlockCircle(m, vx, z2, 6, t, 0.1f, seqRepeat, _aoeLeeway);
            t += 1.4f;
        }
    }

    private void BlockRectsSequence(Map m, float x1, float x2)
    {
        (float y, float z, float d)[] l = [(25.59f, 190.4f, 0.5f), (23.37f, 196.4f, 0.5f), (21.15f, 202.4f, 0.5f), (18.94f, 208.4f, 0.5f), (16.73f, 214.4f, 1.1f)];
        float t = 0;
        float seqRepeat = 3.1f * 2;
        foreach (var e in l)
        {
            BlockRect(m, x1 - 3, x1 + 3, e.z - 3, e.z + 3, t, 0.1f, seqRepeat, _aoeLeeway);
            t += e.d;
        }
        foreach (var e in l)
        {
            BlockRect(m, x2 - 3, x2 + 3, e.z - 3, e.z + 3, t, 0.1f, seqRepeat, _aoeLeeway);
            t += e.d;
        }
    }

    private void BlockCentralRectsSequence(Map m)
    {
        (float y, float z, float d)[] l = [(25.59f, 190.4f, 0.5f), (23.37f, 196.4f, 0.5f), (21.15f, 202.4f, 0.5f), (18.94f, 208.4f, 0.5f), (16.73f, 214.4f, 4.2f)];
        float t = 0;
        float seqRepeat = 3.1f * 2;
        foreach (var e in l)
        {
            BlockRect(m, -3, +3, e.z - 3, e.z + 3, t, 0.1f, seqRepeat, _aoeLeeway);
            t += e.d;
        }
    }

    private void BlockPairsSequence(Map m, float r, Vector3 p1, Vector3 p2)
    {
        BlockCircle(m, p1.X, p1.Z, r, 0.0f, 0.1f, 5, _aoeLeeway);
        BlockCircle(m, p2.X, p2.Z, r, 2.5f, 0.1f, 5, _aoeLeeway);
    }
}
