using Dalamud.Bindings.ImGui;

using Seg = BossMod.VisibilityPolygon<BossMod.WPos>.Segment;

namespace BossMod.Dev;

internal class ShadowTest : TestWindow
{
    private readonly WPos Origin;
    private readonly List<Seg> Segments;
    private readonly List<Seg> Segments2 = [];
    private readonly MiniArena Canvas;
    private readonly VisibilityPolygon<WPos> Vis = new(VisibilityAdapters.WPos);
    WPos CurrentOrigin;

    public ShadowTest() : base("Shadow test", new(600, 600), ImGuiWindowFlags.None)
    {
        Origin = Utils.LoadResource<WPos>("BossMod.Dev.Shadowcast.origin.json");
        CurrentOrigin = Origin;
        Segments = Utils.LoadResource<List<Seg>>("BossMod.Dev.Shadowcast.segments.json");
        Vis.BreakIntersectionsWithOutput(Segments, Segments2);
        Canvas = new(Service.Config.Get<BossModuleConfig>(), Origin, new ArenaBoundsSquare(30));
    }

    public override void Draw()
    {
        var orig = CurrentOrigin.ToVec2();
        if (ImGui.SliderFloat2("Origin", ref orig, MathF.Min(Origin.X, Origin.Z) - 30, MathF.Max(Origin.X, Origin.Z) + 30))
            CurrentOrigin = new WPos(orig);

        Canvas.Begin(default);
        Canvas.Border(ArenaColor.Border);
        Canvas.AddCircle(CurrentOrigin, 1, 0xFFFF00FF);

        try
        {
            var pts = Vis.Compute(CurrentOrigin, Segments2);
            for (var i = 1; i < pts.Count; i++)
                Canvas.AddLine(pts[i - 1], pts[i], ArenaColor.Danger, 3);
            Canvas.AddLine(pts[^1], pts[0], ArenaColor.Danger, 3);

            foreach (var seg in Segments)
                Canvas.AddLine(seg.P1, seg.P2, ArenaColor.Object);
        }
        catch (ArgumentException ex)
        {
            Service.PluginLog.Warning(ex, "polygon calculation error");
            CurrentOrigin = Origin;
        }

        Canvas.End();
    }
}
