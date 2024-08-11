using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod;

internal sealed class DTRProvider : IDisposable
{
    private readonly RotationModuleManager _mgr;
    private readonly AIManager _ai;
    private readonly IDtrBarEntry _autorotationEntry = Service.DtrBar.Get("vbm-autorotation");
    private readonly IDtrBarEntry _aiEntry = Service.DtrBar.Get("vbm-ai");
    private readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private bool _wantOpenPopup;

    public DTRProvider(RotationModuleManager manager, AIManager ai)
    {
        _mgr = manager;
        _ai = ai;

        _autorotationEntry.OnClick = () => _wantOpenPopup = true;
        _aiEntry.OnClick = () =>
        {
            if (_ai.Behaviour == null)
            {
                if (!_aiConfig.Enabled)
                {
                    _aiConfig.Enabled = true;
                    _aiConfig.Modified.Fire();
                }
                _ai.SwitchToFollow((int)_aiConfig.FollowSlot);
            }
            else
            {
                _ai.SwitchToIdle();
            }
        };
    }

    public void Dispose()
    {
        _autorotationEntry.Remove();
        _aiEntry.Remove();
    }

    public void Update()
    {
        _autorotationEntry.Shown = _mgr.Config.ShowDTR != AutorotationConfig.DtrStatus.None;
        var (icon, name) = _mgr.Preset == null ? (BitmapFontIcon.SwordSheathed, "Idle") : _mgr.Preset == RotationModuleManager.ForceDisable ? (BitmapFontIcon.SwordSheathed, "Disabled") : (BitmapFontIcon.SwordUnsheathed, _mgr.Preset.Name);
        Payload prefix = _mgr.Config.ShowDTR == AutorotationConfig.DtrStatus.TextOnly ? new TextPayload("vbm: ") : new IconPayload(icon);
        _autorotationEntry.Text = new SeString(prefix, new TextPayload(name));

        _aiEntry.Shown = _aiConfig.ShowDTR;
        _aiEntry.Text = "AI: " + (_ai.Behaviour == null ? "Off" : "On");

        if (_wantOpenPopup && _mgr.Player != null)
        {
            ImGui.OpenPopup("vbm_dtr_menu");
            _wantOpenPopup = false;
        }

        using (var popup = ImRaii.Popup("vbm_dtr_menu"))
            if (popup)
                if (UIRotationWindow.DrawRotationSelector(_mgr))
                    ImGui.CloseCurrentPopup();
    }
}
