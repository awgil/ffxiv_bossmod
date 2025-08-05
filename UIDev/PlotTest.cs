using BossMod;
using Dalamud.Bindings.ImGui;

namespace UIDev;

class PlotTest : TestWindow
{
    private readonly UIPlot _plot = new();

    public PlotTest() : base("Plot test", new(400, 400), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        _plot.DataMin = new(-180, 0);
        _plot.DataMax = new(180, 60);
        _plot.TickAdvance = new(45, 5);
    }

    public override void Draw()
    {
        _plot.Begin();
        _plot.Point(new(-45, 1), 0xffffffff, () => "first");
        _plot.Point(new(45, 10), 0xff00ff00, () => "second");
        _plot.End();
    }
}
