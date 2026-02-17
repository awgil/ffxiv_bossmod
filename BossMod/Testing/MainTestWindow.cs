using BossMod.Services;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;

namespace BossMod.Testing;

abstract class TestWindow(string name, Vector2 initialSize, ImGuiWindowFlags flags) : UIWindow(name, true, initialSize, flags);

class MainTestWindow : UIWindow
{
    private readonly MediatorService mediator;
    private readonly IEnumerable<TestWindow> testWindows;

    public MainTestWindow(MediatorService mediator, IEnumerable<TestWindow> testWindows) : base("Test environment", false, new(600, 600))
    {
        this.mediator = mediator;
        this.testWindows = testWindows;
        IsOpen = true;
    }

    public override void Draw()
    {
        foreach (var t in testWindows)
            if (ImGui.Button($"Show {t.GetType().Name}"))
                mediator.Publish(new CreateWindowMessage(t));
    }
}
