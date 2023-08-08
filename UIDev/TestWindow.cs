using BossMod;
using ImGuiNET;

namespace UIDev
{
    abstract class TestWindow : SimpleWindow
    {
        public TestWindow(string name, ImGuiWindowFlags flags) : base(name, null, flags) { }
    }
}
