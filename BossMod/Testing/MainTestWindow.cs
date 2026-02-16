using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;

namespace BossMod.Testing;

class MainTestWindow(MediatorService mediator, IEnumerable<Lazy<TestWindow>> testWindows) : UIWindow(mediator, "Test environment", false, new(600, 600))
{
    public override void Draw()
    {
        foreach (var t in testWindows)
            if (ImGui.Button($"Show {t.GetType().Name}"))
                _ = t.Value;
    }
}
