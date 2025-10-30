using BossMod;
using Dalamud.Bindings.ImGui;

namespace UIDev;

abstract class TestWindow(string name, Vector2 initialSize, ImGuiWindowFlags flags) : UIWindow(name, true, initialSize, flags)
{
}
