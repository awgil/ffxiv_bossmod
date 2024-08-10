using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Autorotation;
internal class DTR : IDisposable
{
    private readonly RotationModuleManager _mgr;
    private readonly IDtrBarEntry _dtrBarEntry = Service.DtrBar.Get("vbm");
    private readonly AutorotationConfig _config = Service.Config.Get<AutorotationConfig>();
    private readonly DTRPopup _popup;

    public DTR(RotationModuleManager manager)
    {
        _mgr = manager;
        _popup = new(_mgr) { IsOpen = true };

        _dtrBarEntry.OnClick = () => _popup.Show();
    }

    public void Dispose()
    {
        _dtrBarEntry.Remove();
    }

    public void Update()
    {
        _dtrBarEntry.Shown = _config.ShowDTR != AutorotationConfig.DtrStatus.None;
        var (icon, current) = _mgr.Preset?.Name switch
        {
            "" => (BitmapFontIcon.SwordSheathed, "Disabled"),
            null => (BitmapFontIcon.SwordSheathed, "Idle"),
            var x => (BitmapFontIcon.SwordUnsheathed, x)
        };
        Payload prefix = _config.ShowDTR == AutorotationConfig.DtrStatus.TextOnly ? new TextPayload("vbm: ") : new IconPayload(icon);
        _dtrBarEntry.Text = new SeString(prefix, new TextPayload(current));
    }
}

internal class DTRPopup(RotationModuleManager _mgr) : UIWindow("###vbm_dtr", false, new(100, 100), ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoResize)
{
    private bool _open;
    public void Show()
    {
        if (_mgr.Player == null)
            return;

        _open = true;
    }

    public override void Draw()
    {
        if (ImGui.BeginPopup("vbm_dtr_menu"))
        {
            DrawSelector();
            ImGui.EndPopup();
        }
        if (_open)
        {
            _open = false;
            ImGui.OpenPopup("vbm_dtr_menu");
        }
    }

    void DrawSelector()
    {
        if (_mgr.Player == null)
            return;

        using (ImRaii.PushColor(ImGuiCol.Button, 0xff000080, _mgr.Preset == RotationModuleManager.ForceDisable))
        {
            if (ImGui.Button("X"))
            {
                _mgr.Preset = _mgr.Preset == RotationModuleManager.ForceDisable ? null : RotationModuleManager.ForceDisable;
            }
        }

        foreach (var p in _mgr.Database.Presets.PresetsForClass(_mgr.Player.Class))
        {
            ImGui.SameLine();
            using var col = ImRaii.PushColor(ImGuiCol.Button, 0xff008080, _mgr.Preset == p);
            if (ImGui.Button(p.Name))
            {
                _mgr.Preset = _mgr.Preset == p ? null : p;
                ImGui.CloseCurrentPopup();
            }
        }
    }
}
