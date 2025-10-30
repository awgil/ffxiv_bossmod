using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.AI;

internal sealed class AIWindow : UIWindow
{
    private const string _windowID = "###AI";

    private readonly AIManager _manager;
    private readonly EventSubscriptions _subscriptions;

    public AIWindow(AIManager mgr) : base(_windowID, false, new(100, 100), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing)
    {
        _manager = mgr;
        _subscriptions = new
        (
            _manager.Config.Modified.ExecuteAndSubscribe(() => IsOpen = _manager.Config.DrawUI)
        );
        RespectCloseHotkey = false;
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public void SetVisible(bool vis)
    {
        if (_manager.Config.DrawUI != vis)
        {
            _manager.Config.DrawUI = vis;
            _manager.Config.Modified.Fire();
        }
    }

    public override bool DrawConditions() => _manager.WorldState.Party.Player() != null;

    public override void PreDraw()
    {
        var windowName = _manager.Config.ShowStatusOnTitlebar ? _manager.AIStatus : "AI";
        WindowName = windowName + _windowID;
        base.PreDraw();
    }

    public override void Draw()
    {
        if (!_manager.Config.ShowStatusOnTitlebar)
            ImGui.TextUnformatted(_manager.AIStatus);
        ImGui.TextUnformatted(_manager.NaviStatus);
        _manager.Behaviour?.DrawDebug();

        using (var leaderCombo = ImRaii.Combo("Follow", _manager.Behaviour == null ? "<idle>" : _manager.WorldState.Party[_manager.MasterSlot]?.Name ?? "<unknown>"))
        {
            if (leaderCombo)
            {
                if (ImGui.Selectable("<idle>", _manager.Behaviour == null))
                {
                    _manager.Enabled = false;
                }
                foreach (var (i, p) in _manager.WorldState.Party.WithSlot(true))
                {
                    if (ImGui.Selectable(p.Name, _manager.MasterSlot == i))
                    {
                        _manager.Config.FollowSlot = (AIConfig.Slot)i;
                        _manager.Config.Modified.Fire();
                        _manager.Enabled = true;
                    }
                }
            }
        }
    }

    public override void OnClose() => SetVisible(false);
}
