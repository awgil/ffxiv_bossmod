using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod;

internal sealed class DtrService(RotationModuleManager manager, IDtrBar dtrBar, IUiBuilder uiBuilder) : IHostedService, IDisposable
{
    private readonly List<(string, IDtrBarEntry)> _autorotEntries = [];
    private readonly IDtrBarEntry _arEntry = dtrBar.Get("vbm-ar");
    private readonly IDtrBarEntry _aiEntry = dtrBar.Get("vbm-ai");
    private readonly IDtrBarEntry _statsEntry = dtrBar.Get("vbm-stats");
    private readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private readonly DtrConfig _dtrConfig = Service.Config.Get<DtrConfig>();
    private bool _wantOpenPopup;
    private string? _popupFor;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _aiEntry.Tooltip = "Left click: toggle AI - Right click: toggle window";
        _aiEntry.OnClick = ev =>
        {
            if (ev.ClickType == MouseClickType.Right)
                _aiConfig.DrawUI ^= true;
            else
                _aiConfig.Enabled ^= true;
            _aiConfig.Modified.Fire();
        };

        _arEntry.Tooltip = "Left click: open preset selector; right click: disable all";
        _arEntry.OnClick = OnAutorotClick;

        uiBuilder.Draw += Update;

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        uiBuilder.Draw -= Update;

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var (_, e) in _autorotEntries)
            e.Dispose();
        _aiEntry.Dispose();
        _arEntry.Dispose();
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
            e.Text = $"{p}: {(manager.PresetNames.Contains(p, StringComparison.Ordinal) ? "On" : "Off")}";

        _arEntry.Shown = _dtrConfig.ShowAutorot != DtrConfig.DtrStatus.None;
        var (icon, name) = manager.Presets.Count == 0 ? (BitmapFontIcon.SwordSheathed, "Idle") : manager.IsForceDisabled ? (BitmapFontIcon.SwordSheathed, "Disabled") : (BitmapFontIcon.SwordUnsheathed, $"{manager.Presets.Count} active");
        Payload prefix = _dtrConfig.ShowAutorot == DtrConfig.DtrStatus.TextOnly ? new TextPayload("vbm: ") : new IconPayload(icon);
        _arEntry.Text = new SeString(prefix, new TextPayload(name));

        _aiEntry.Shown = _dtrConfig.ShowAI;
        _aiEntry.Text = "AI: " + (_aiConfig.Enabled ? "On" : "Off");

        _statsEntry.Shown = _dtrConfig.ShowStats;
        _statsEntry.Text = manager.LastPathfindMs > 0
            ? $"Pathfind: {manager.LastRasterizeMs:f1}ms (r) {manager.LastPathfindMs:f1}ms (p)"
            : $"Pathfind: -";

        if (_wantOpenPopup && manager.Player != null)
        {
            ImGui.OpenPopup("vbm_dtr_menu");
            _wantOpenPopup = false;
        }

        using var popup = ImRaii.Popup("vbm_dtr_menu");
        if (popup)
            DrawPopup();
    }

    void OnAutorotClick(DtrInteractionEvent ev)
    {
        if (ev.ClickType == MouseClickType.Left)
        {
            _popupFor = null;
            _wantOpenPopup = true;
        }

        if (ev.ClickType == MouseClickType.Right)
            manager.Clear();
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
        var preset = manager.Database.Presets.AllPresets.FirstOrDefault(s => s.Name == name);
        if (preset == null)
            Service.PluginLog.Verbose($"no preset found with name {name}; dtr is stale?");
        else
            manager.Toggle(preset);
    }

    void DrawPopup()
    {
        if (_popupFor == null)
        {
            UIRotationWindow.DrawRotationSelector(manager);
            ImGui.Separator();
            if (ImGui.MenuItem("Remove from info bar"))
            {
                _dtrConfig.ShowAutorot = DtrConfig.DtrStatus.None;
                _dtrConfig.Modified.Fire();
            }
            return;
        }

        if (ImGui.MenuItem($"Toggle {_popupFor}", "LMB"))
            ToggleByName(_popupFor);

        ImGui.Separator();

        if (ImGui.MenuItem("Open in preset editor"))
            Service.Mediator.Publish(new OpenPresetEditor(_popupFor));

        if (ImGui.MenuItem("Remove from info bar"))
        {
            _dtrConfig.ShowPresets.Remove(_popupFor);
            _dtrConfig.Modified.Fire();
        }
    }
}
