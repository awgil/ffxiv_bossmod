using BossMod.Autorotation;
using BossMod.Autorotation.MiscAI;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.AI;

internal sealed class AIWindow : UIWindow
{
    private const string _windowID = "Multibox settings###AI";

    readonly RotationModuleManager _manager;
    readonly EventSubscriptions _subscriptions;
    readonly AIConfig _config = Service.Config.Get<AIConfig>();

    int _masterSlot;

    readonly Preset _pMultibox;
    readonly Preset _pVbmai;
    bool _usingAIFallback;

    public AIWindow(RotationModuleManager mgr) : base(_windowID, false, new(100, 100), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing)
    {
        _manager = mgr;
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() => IsOpen = _config.DrawUI)
        );
        RespectCloseHotkey = false;
        _pVbmai = mgr.Database.Presets.DefaultPresets.First(p => p.Name == "VBM AI");
        _pMultibox = mgr.Database.Presets.DefaultPresets.First(p => p.Name == "VBM Multibox");
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public override bool DrawConditions() => _manager.WorldState.Party.Player() != null;

    public override void Draw()
    {
        using var leaderCombo = ImRaii.Combo("Follow", _masterSlot == 0 ? "<disabled>" : $"#{_masterSlot} {_manager.WorldState.Party[_masterSlot]?.Name ?? "<unknown>"}");
        if (leaderCombo)
        {
            if (ImGui.Selectable("<disabled>", _masterSlot == 0))
                SetSlot(0);
            foreach (var (i, p) in _manager.WorldState.Party.WithSlot(true).Skip(1))
                if (ImGui.Selectable(p.Name, _masterSlot == i))
                    SetSlot(i);
        }
    }

    public void SetSlot(int slot)
    {
        _masterSlot = slot;
        if (slot == 0)
        {
            _manager.Deactivate(_pMultibox);
            if (_usingAIFallback)
                _manager.Deactivate(_pVbmai);
        }
        else
        {
            var mmod = _pMultibox.Modules[0];
            mmod.TransientSettings.RemoveAll(s => s.Track == 0);
            mmod.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueInt() { Value = slot }));
            _manager.Activate(_pMultibox);

            if (!_manager.Presets.Any(p => p.Modules.Any(m => m.Type == typeof(NormalMovement))))
            {
                _manager.Activate(_pVbmai);
                _usingAIFallback = true;
            }
        }
    }

    public override void OnClose()
    {
        _config.DrawUI = false;
        _config.Modified.Fire();
    }
}
