using Dalamud.Bindings.ImGui;

namespace BossMod.Dev;

abstract class TestWindow(string name, Vector2 initialSize, ImGuiWindowFlags flags) : DetachedWindow(name, initialSize, flags);
