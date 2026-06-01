using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.AI;

internal sealed class AIWindow : UIWindow
{
    private const string _windowID = "Multibox settings###AI";

    readonly RotationModuleManager _manager;
    readonly EventSubscriptions _subscriptions;
    readonly AIConfig _config = Service.Config.Get<AIConfig>();

    int _masterSlot = -1;

    readonly Preset _pMultibox;

    public AIWindow(RotationModuleManager mgr) : base(_windowID, false, new(100, 100), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing)
    {
        _manager = mgr;
        _pMultibox = mgr.Database.Presets.DefaultPresets.First(p => p.Name == "VBM Multibox");
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() =>
            {
                IsOpen = _config.DrawUI;
                TogglePreset();
                ToggleMove();
                ToggleTarget();
            })
        );
        RespectCloseHotkey = false;
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public override bool DrawConditions() => _manager.WorldState.Party.Player() != null;

    public override void Draw()
    {
        using (var leaderCombo = ImRaii.Combo("Follow", _masterSlot == -1 ? "<disabled>" : $"#{_masterSlot} {_manager.WorldState.Party[_masterSlot]?.Name ?? "<unknown>"}"))
        {
            if (leaderCombo)
            {
                if (ImGui.Selectable("<disabled>", _masterSlot == -1))
                    SetSlot(-1);
                foreach (var (i, p) in _manager.WorldState.Party.WithSlot(true))
                    if (ImGui.Selectable(p.Name, _masterSlot == i))
                        SetSlot(i);
            }
        }

        if (ImGui.Checkbox("Disable movement", ref _config.ForbidMovement))
            _config.Modified.Fire();

        if (ImGui.Checkbox("Disable auto-target", ref _config.ForbidActions))
            _config.Modified.Fire();
    }

    void TogglePreset()
    {
        if (_config.Enabled)
            _manager.Activate(_pMultibox);
        else
            _manager.Deactivate(_pMultibox);
    }

    void ToggleMove()
    {
        var normalMove = _pMultibox.Modules[^1];
        normalMove.TransientSettings.RemoveAll(s => s.Track == 0);
        if (_config.ForbidMovement)
            normalMove.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueTrack() { Option = 0 }));
    }

    void ToggleTarget()
    {
        var autoT = _pMultibox.Modules[0];
        autoT.TransientSettings.RemoveAll(s => s.Track == 0);
        if (_config.ForbidActions)
            autoT.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueTrack() { Option = 1 }));
    }

    public void SetSlot(int slot)
    {
        _masterSlot = slot;
        var mbox = _pMultibox.Modules[1];
        mbox.TransientSettings.RemoveAll(s => s.Track == 0);
        mbox.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueInt() { Value = slot }));
        if (slot == -1)
        {
            _config.Enabled = false;
            _manager.Deactivate(_pMultibox);
        }
        else
        {
            _config.Enabled = true;
            _manager.Activate(_pMultibox);
        }
    }

    public override void OnClose()
    {
        _config.DrawUI = false;
        _config.Modified.Fire();
    }
}
