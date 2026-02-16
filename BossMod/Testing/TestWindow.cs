using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;

namespace BossMod.Testing;

abstract class TestWindow(MediatorService mediator, string name, Vector2 initialSize, ImGuiWindowFlags flags) : UIWindow(mediator, name, true, initialSize, flags);
