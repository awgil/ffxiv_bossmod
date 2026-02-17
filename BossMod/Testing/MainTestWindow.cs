using Autofac;
using BossMod.Services;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using System.Reflection;

namespace BossMod.Testing;

abstract class TestWindow(string name, Vector2 initialSize, ImGuiWindowFlags flags) : UIWindow(name, true, initialSize, flags);

class MainTestWindow : UIWindow
{
    private readonly MediatorService mediator;
    private readonly IComponentContext scope;

    public MainTestWindow(MediatorService mediator, IComponentContext scope) : base("Test environment", false, new(600, 600))
    {
        this.mediator = mediator;
        this.scope = scope;
        IsOpen = true;
    }

    public override void Draw()
    {
        foreach (var t in Utils.GetDerivedTypes<TestWindow>(Assembly.GetExecutingAssembly()))
            if (ImGui.Button($"Show {t.Name}"))
                mediator.Publish(new CreateWindowMessage((scope.Resolve(t) as TestWindow)!));
    }
}
