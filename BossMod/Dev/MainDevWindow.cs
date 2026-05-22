using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using System.Reflection;

namespace BossMod.Dev;

class MainDevWindow : UIWindow
{
    bool ConfigModified
    {
        get => !RespectCloseHotkey;
        set => RespectCloseHotkey = !value;
    }

    readonly EventSubscription _configChange;
    private readonly IDalamudPluginInterface dalamud;
    readonly List<Type> _devWindows;

    public MainDevWindow(IDalamudPluginInterface dalamud) : base("Dev tools", false, new(600, 600))
    {
        _configChange = Service.Config.Modified.Subscribe(() => ConfigModified = true);
        this.dalamud = dalamud;

        _devWindows = [.. Utils.GetDerivedTypes<TestWindow>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract)];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _configChange.Dispose();
        base.Dispose(disposing);
    }

    public override void Draw()
    {
        using (ImRaii.Disabled(!ConfigModified))
            if (ImGui.Button(ConfigModified ? "Save config (modified)" : "Save config"))
            {
                Service.Config.SaveToFile(dalamud.ConfigFile);
                ConfigModified = false;
            }

        ImGui.Separator();

        foreach (var t in _devWindows)
            if (ImGui.Button($"Show {t}"))
                Activator.CreateInstance(t);
    }
}
