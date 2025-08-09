using BossMod;
using Dalamud.Bindings.ImGui;

namespace UIDev;

abstract class TestWindow : UIWindow
{
    public TestWindow(string name, Vector2 initialSize, ImGuiWindowFlags flags) : base(name, true, initialSize, flags) { }
}
