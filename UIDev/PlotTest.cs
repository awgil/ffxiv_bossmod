using BossMod;

namespace UIDev
{
    class PlotTest : ITest
    {
        private UIPlot _plot = new();

        public PlotTest()
        {
            _plot.DataMin = new(-180, 0);
            _plot.DataMax = new(180, 60);
            _plot.TickAdvance = new(45, 5);
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            _plot.Begin();
            _plot.Point(new(-45, 1), 0xffffffff, () => "first");
            _plot.Point(new(45, 10), 0xff00ff00, () => "second");
            _plot.End();
        }
    }
}
