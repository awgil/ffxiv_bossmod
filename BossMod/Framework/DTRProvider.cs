using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod;

internal sealed class DTRProvider(RotationModuleManager autorot, AIManager aiManager, IDtrBar dtrBar) : IHostedService
{
    private readonly IDtrBarEntry _autorotationEntry = dtrBar.Get("vbm-autorotation");
    private readonly IDtrBarEntry _aiEntry = dtrBar.Get("vbm-ai");
    private readonly IDtrBarEntry _statsEntry = dtrBar.Get("vbm-stats");
    private readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private bool _wantOpenPopup;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _autorotationEntry.OnClick = _ => _wantOpenPopup = true;
        _aiEntry.Tooltip = "Left Click => Toggle Enabled, Right Click => Toggle DrawUI";

        _aiEntry.OnClick = ev =>
        {
            if (ev.ClickType == MouseClickType.Right)
                _aiConfig.DrawUI ^= true;
            else
                _aiConfig.Enabled ^= true;
            _aiConfig.Modified.Fire();
        };
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _autorotationEntry.Remove();
        _aiEntry.Remove();
        _statsEntry.Remove();
    }

    public void Dispose()
    {
        _autorotationEntry.Remove();
        _aiEntry.Remove();
        _statsEntry.Remove();
    }

    public void Update()
    {
        _autorotationEntry.Shown = autorot.Config.ShowDTR != AutorotationConfig.DtrStatus.None;
        var (icon, name) = autorot.Presets.Count == 0 ? (BitmapFontIcon.SwordSheathed, "Idle") : autorot.IsForceDisabled ? (BitmapFontIcon.SwordSheathed, "Disabled") : (BitmapFontIcon.SwordUnsheathed, string.Join(", ", autorot.PresetNames));
        Payload prefix = autorot.Config.ShowDTR == AutorotationConfig.DtrStatus.TextOnly ? new TextPayload("vbm: ") : new IconPayload(icon);
        _autorotationEntry.Text = new SeString(prefix, new TextPayload(name));

        _aiEntry.Shown = _aiConfig.ShowDTR;
        _aiEntry.Text = "AI: " + (aiManager.Behaviour == null ? "Off" : "On");

        _statsEntry.Shown = autorot.Config.ShowStatsDTR;
        _statsEntry.Text = autorot.LastPathfindMs > 0
            ? $"Pathfind: {autorot.LastRasterizeMs:f1}ms (r) {autorot.LastPathfindMs:f1}ms (p)"
            : $"Pathfind: -";

        if (_wantOpenPopup && autorot.Player != null)
        {
            ImGui.OpenPopup("vbm_dtr_menu");
            _wantOpenPopup = false;
        }

        using var popup = ImRaii.Popup("vbm_dtr_menu");
        if (popup)
            if (UIRotationWindow.DrawRotationSelector(autorot))
                ImGui.CloseCurrentPopup();
    }
}
