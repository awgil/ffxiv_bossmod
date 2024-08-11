using BossMod.AI;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Autorotation;
internal sealed class DTR : IDisposable
{
    private readonly RotationModuleManager _mgr;
    private readonly AIManager _ai;
    private readonly IDtrBarEntry _autorotationEntry = Service.DtrBar.Get("vbm-autorotation");
    private readonly IDtrBarEntry _aiEntry = Service.DtrBar.Get("vbm-ai");
    private readonly AutorotationConfig _arConfig = Service.Config.Get<AutorotationConfig>();
    private readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private readonly DTRPopup _popup;

    public DTR(RotationModuleManager manager, AIManager ai)
    {
        _mgr = manager;
        _ai = ai;
        _popup = new(_mgr) { IsOpen = true };

        _autorotationEntry.OnClick = () => _popup.Show();
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
                _ai.SwitchToIdle();
        };
    }

    public void Dispose()
    {
        _autorotationEntry.Remove();
        _aiEntry.Remove();
        _popup.Dispose();
    }

    public void Update()
    {
        _autorotationEntry.Shown = _arConfig.ShowDTR != AutorotationConfig.DtrStatus.None;
        var (icon, current) = _mgr.Preset?.Name switch
        {
            "" => (BitmapFontIcon.SwordSheathed, "Disabled"),
            null => (BitmapFontIcon.SwordSheathed, "Idle"),
            var x => (BitmapFontIcon.SwordUnsheathed, x)
        };
        Payload prefix = _arConfig.ShowDTR == AutorotationConfig.DtrStatus.TextOnly ? new TextPayload("vbm: ") : new IconPayload(icon);
        _autorotationEntry.Text = new SeString(prefix, new TextPayload(current));

        _aiEntry.Shown = _aiConfig.ShowDTR;
        _aiEntry.Text = "AI: " + (_ai.Behaviour == null ? "Off" : "On");
    }
}

internal class DTRPopup(RotationModuleManager manager) : UIWindow("###vbm_dtr", false, new(100, 100), ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoResize)
{
    private bool _open;
    public void Show()
    {
        if (manager.Player == null)
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
        if (manager.Player == null)
            return;

        using (ImRaii.PushColor(ImGuiCol.Button, 0xff000080, manager.Preset == RotationModuleManager.ForceDisable))
        {
            if (ImGui.Button("X"))
            {
                manager.Preset = manager.Preset == RotationModuleManager.ForceDisable ? null : RotationModuleManager.ForceDisable;
            }
        }

        foreach (var p in manager.Database.Presets.PresetsForClass(manager.Player.Class))
        {
            ImGui.SameLine();
            using var col = ImRaii.PushColor(ImGuiCol.Button, 0xff008080, manager.Preset == p);
            if (ImGui.Button(p.Name))
            {
                manager.Preset = manager.Preset == p ? null : p;
                ImGui.CloseCurrentPopup();
            }
        }
    }
}
