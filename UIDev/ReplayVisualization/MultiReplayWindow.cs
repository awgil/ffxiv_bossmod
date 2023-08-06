using BossMod;
using ImGuiNET;

namespace UIDev
{
    class MultiReplayWindow : SimpleWindow
    {
        private AnalysisManager _analysis;

        public MultiReplayWindow(string path) : base($"Multiple logs: {path}")
        {
            _analysis = new(path);
            Size = new(1200, 800);
            SizeCondition = ImGuiCond.Once;
        }

        public override void Draw()
        {
            _analysis.Draw();
        }
    }
}
