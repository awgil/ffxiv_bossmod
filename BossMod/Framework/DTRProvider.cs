using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

internal sealed class DTRProvider : IDisposable
{
    private readonly RotationModuleManager _mgr;
    private readonly List<(string, IDtrBarEntry)> _autorotEntries = [];
    private readonly IDtrBarEntry _aiEntry = Service.DtrBar.Get("vbm-ai");
    private readonly IDtrBarEntry _statsEntry = Service.DtrBar.Get("vbm-stats");
    private readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private readonly DtrConfig _dtrConfig = Service.Config.Get<DtrConfig>();
    private bool _wantOpenPopup;
    private string? _popupFor;

    public DTRProvider(RotationModuleManager manager)
    {
        _mgr = manager;

        _aiEntry.Tooltip = "Left click: toggle AI - Right click: toggle window";

        _aiEntry.OnClick = ev =>
        {
            if (ev.ClickType == MouseClickType.Right)
                _aiConfig.DrawUI ^= true;
            else
                _aiConfig.Enabled ^= true;
            _aiConfig.Modified.Fire();
        };
    }

    public void Dispose()
    {
        foreach (var (_, e) in _autorotEntries)
            e.Dispose();
        _aiEntry.Dispose();
        _statsEntry.Dispose();
    }

    public void Update()
    {
        if (_autorotEntries.Count != _dtrConfig.ShowPresets.Count)
        {
            foreach (var (_, e) in _autorotEntries)
                e.Dispose();
            _autorotEntries.Clear();
            foreach (var p in _dtrConfig.ShowPresets)
            {
                var newEntry = Service.DtrBar.Get($"vbm-ar-{p}");
                newEntry.Shown = true;
                newEntry.OnClick = ev => OnPresetClick(ev, p);
                newEntry.Tooltip = $"Left click: toggle preset; right click: open settings menu";

                _autorotEntries.Add((p, newEntry));
            }
        }

        foreach (var (p, e) in _autorotEntries)
            e.Text = $"{p}: {(_mgr.PresetNames.Contains(p) ? "On" : "Off")}";

        _aiEntry.Shown = _dtrConfig.ShowAI;
        _aiEntry.Text = "AI: " + (_aiConfig.Enabled ? "On" : "Off");

        _statsEntry.Shown = _dtrConfig.ShowStats;
        _statsEntry.Text = _mgr.LastPathfindMs > 0
            ? $"Pathfind: {_mgr.LastRasterizeMs:f1}ms (r) {_mgr.LastPathfindMs:f1}ms (p)"
            : $"Pathfind: -";

        if (_wantOpenPopup && _mgr.Player != null)
        {
            ImGui.OpenPopup("vbm_dtr_menu");
            _wantOpenPopup = false;
        }

        using var popup = ImRaii.Popup("vbm_dtr_menu");
        if (popup)
            DrawPopup();
    }

    void OnPresetClick(DtrInteractionEvent ev, string name)
    {
        if (ev.ClickType == MouseClickType.Left)
            ToggleByName(name);

        if (ev.ClickType == MouseClickType.Right)
        {
            _wantOpenPopup = true;
            _popupFor = name;
        }
    }

    void ToggleByName(string name)
    {
        var preset = _mgr.Database.Presets.AllPresets.FirstOrDefault(s => s.Name == name);
        if (preset == null)
            Service.PluginLog.Verbose($"no preset found with name {name}; dtr is stale?");
        else
            _mgr.Toggle(preset);
    }

    void DrawPopup()
    {
        if (_popupFor == null)
        {
            ImGui.CloseCurrentPopup();
            return;
        }

        if (ImGui.MenuItem($"Toggle {_popupFor}", "LMB"))
            ToggleByName(_popupFor);

        ImGui.Separator();

        if (ImGui.MenuItem("Remove entry"))
        {
            _dtrConfig.ShowPresets.Remove(_popupFor);
            _dtrConfig.Modified.Fire();
        }
    }
}
