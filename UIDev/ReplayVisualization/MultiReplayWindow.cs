using BossMod;
using ImGuiNET;

namespace UIDev
{
    class MultiReplayWindow : SimpleWindow
    {
        private AnalysisManager _analysis;

        public MultiReplayWindow(string path) : base($"Multiple logs: {path}", new(1200, 800))
        {
            _analysis = new(path);
        }

        public override void Draw()
        {
            _analysis.Draw();
        }
    }
}
