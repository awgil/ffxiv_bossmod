using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;

namespace BossMod.Dev;

class MainDevWindow : WindowBase, IDisposable
{
    bool ConfigModified
    {
        get => !RespectCloseHotkey;
        set => RespectCloseHotkey = !value;
    }

    readonly EventSubscription _configChange;
    private readonly IDalamudPluginInterface dalamud;
    private readonly PackLoader packs;
    private readonly IEnumerable<TestWindow> testWindows;
    private readonly MediatorService mediator;

    public MainDevWindow(IDalamudPluginInterface dalamud, PackLoader packs, IEnumerable<TestWindow> testWindows, MediatorService mediator) : base("Dev tools", new(600, 600))
    {
        _configChange = Service.Config.Modified.Subscribe(() => ConfigModified = true);
        this.dalamud = dalamud;
        this.packs = packs;
        this.testWindows = testWindows;
        this.mediator = mediator;

        IsOpen = true;
    }

    void IDisposable.Dispose()
    {
        _configChange.Dispose();
    }

    public override void Draw()
    {
        using (ImRaii.Disabled(!ConfigModified))
            if (ImGui.Button(ConfigModified ? "Save config (modified)" : "Save config"))
            {
                Service.Config.SaveToFile(dalamud.ConfigFile);
                ConfigModified = false;
            }

        foreach (var p in packs.Loaded)
            ImGui.Text($"Loaded assembly: {p.FullName}");

        ImGui.Separator();

        foreach (var t in testWindows)
            if (ImGui.Button($"Show {t.GetType()}"))
            {
                t.IsOpen = true;
                mediator.Publish(new CreateWindow(t));
            }
    }
}
