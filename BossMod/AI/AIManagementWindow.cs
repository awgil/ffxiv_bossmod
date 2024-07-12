using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.AI;

sealed class AIManagementWindow : UIWindow
{
    private readonly AIConfig _config;
    private readonly AIManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private const string _title = $"AI: off{_windowID}";
    private const string _windowID = "###AI debug window";

    public AIManagementWindow(AIManager manager) : base(_windowID, false, new(100, 100))
    {
        WindowName = _title;
        _config = Service.Config.Get<AIConfig>();
        _manager = manager;
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() => IsOpen = _config.DrawUI)
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
        if (_config.DrawUI != vis)
        {
            _config.DrawUI = vis;
            _config.Modified.Fire();
        }
    }

    public override void Draw()
    {
        ImGui.TextUnformatted($"Navi={_manager.Controller.NaviTargetPos} / {_manager.Controller.NaviTargetRot}{(_manager.Controller.ForceFacing ? " forced" : "")}");
        _manager.Beh?.DrawDebug();
        ImGui.Text("Follow party slot");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250);
        ImGui.SetNextWindowSizeConstraints(new Vector2(0, 0), new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50));
        using (var leaderCombo = ImRaii.Combo("##Leader", _manager.Beh == null ? "<idle>" : _manager.WorldState.Party[_manager.MasterSlot]?.Name ?? "<unknown>"))
        {
            if (leaderCombo)
            {
                if (ImGui.Selectable("<idle>", _manager.Beh == null))
                {
                    _manager.SwitchToIdle();
                }
                foreach (var (i, p) in _manager.WorldState.Party.WithSlot(true))
                {
                    if (ImGui.Selectable(p.Name, _manager.MasterSlot == i))
                    {
                        _config.Enabled = true;
                        _manager.SwitchToFollow(i);
                        _config.FollowSlot = i;
                        _config.Modified.Fire();
                    }
                }
            }
        }
        ImGui.Text("Desired positional");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        var positionalOptions = Enum.GetNames(typeof(Positional));
        var positionalIndex = (int)_config.DesiredPositional;
        if (ImGui.Combo("##DesiredPositional", ref positionalIndex, positionalOptions, positionalOptions.Length))
        {
            _config.DesiredPositional = (Positional)positionalIndex;
            _config.Modified.Fire();
        }
        ImGui.Text("Autorotation AI preset");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250);
        ImGui.SetNextWindowSizeConstraints(new Vector2(0, 0), new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50));
        using var presetCombo = ImRaii.Combo("##AI preset", _manager.AiPreset?.Name ?? "");
        if (presetCombo)
        {
            foreach (var p in _manager.Autorot.Database.Presets.Presets)
            {
                if (ImGui.Selectable(p.Name, p == _manager.AiPreset))
                {
                    _manager.AiPreset = p;
                    if (_manager.Beh != null)
                        _manager.Beh.AIPreset = p;
                }
            }
        }
    }

    public override void OnClose() => SetVisible(false);

    public void UpdateTitle()
    {
        var masterSlot = _manager?.MasterSlot ?? -1;
        var masterName = _manager?.Autorot?.WorldState?.Party[masterSlot]?.Name ?? "unknown";
        var masterSlotNumber = masterSlot != -1 ? (masterSlot + 1).ToString() : "N/A";

        WindowName = $"AI: {(_manager?.Beh != null ? "on" : "off")}, master={masterName}[{masterSlotNumber}]{_windowID}";
    }
}
