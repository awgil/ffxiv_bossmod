using BossMod.Autorotation;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.AI;

internal sealed class AIWindow : UIWindow, IMediatorSubscriber
{
    private const string _windowID = "Multibox settings###AI";

    public MediatorService MediatorService { get; }

    readonly RotationModuleManager _manager;
    readonly EventSubscriptions _subscriptions;
    readonly AIConfig _config = Service.Config.Get<AIConfig>();

    int _masterSlot = -1;

    readonly Preset _pMultibox;

    public AIWindow(RotationModuleManager mgr, MediatorService mediator) : base(_windowID, false, new(100, 100), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing)
    {
        MediatorService = mediator;
        _manager = mgr;
        _pMultibox = mgr.Database.Presets.DefaultPresets.First(p => p.Name == "VBM Multibox");
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() =>
            {
                IsOpen = _config.DrawUI;
                ToggleMove();
                ToggleTarget();
            })
        );
        RespectCloseHotkey = false;

        mediator.Subscribe(this, (SetFollowSlot s) => SetSlot(s.Slot));
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        MediatorService.UnsubscribeAll(this);
        base.Dispose(disposing);
    }

    public override bool DrawConditions() => _manager.WorldState.Party.Player() != null;

    public override void Draw()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("Follow");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(180 * ImGuiHelpers.GlobalScale);
        using (var leaderCombo = ImRaii.Combo("###follow", _masterSlot == -1 ? "<disabled>" : $"#{_masterSlot} {_manager.WorldState.Party[_masterSlot]?.Name ?? "<unknown>"}"))
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

        if (DrawSwitch("Movement", "move", !_config.ForbidMovement))
        {
            _config.ForbidMovement ^= true;
            _config.Modified.Fire();
        }
        ImGui.SameLine();
        if (DrawSwitch("Auto-target", "target", !_config.ForbidActions))
        {
            _config.ForbidActions ^= true;
            _config.Modified.Fire();
        }

        DrawStatus();
    }

    private static bool DrawSwitch(string label, string id, bool on)
        => UIMisc.ToggleButton($"{label}: {(on ? "on" : "off")}###{id}", true, on ? UIMisc.ColorActive : 0xff000080);

    private void DrawStatus()
    {
        var player = _manager.Player;
        if (player == null)
            return;

        List<(string text, uint color)> status = [];
        if (!_config.Enabled)
            status.Add(("Not following", ImGui.GetColorU32(ImGuiCol.TextDisabled)));
        else if (_masterSlot >= 0 && _manager.WorldState.Party[_masterSlot] is { } master && master != player)
            status.Add(($"Following {master.Name} ({(master.Position - player.Position).Length():f0}y)", UIMisc.ColorInfo));

        var hints = _manager.Hints;
        var now = _manager.WorldState.CurrentTime;
        if (hints.ForbiddenZones.Any(z => z.activation <= now.AddSeconds(1) && z.shape.Check(player.Position)))
            status.Add(("In danger", UIMisc.ColorDanger));
        else if (hints.ForbiddenZones.Count > 0)
            status.Add(($"Avoiding {hints.ForbiddenZones.Count} AOE{(hints.ForbiddenZones.Count > 1 ? "s" : "")}", UIMisc.ColorWarning));
        if (hints.ForcedMovement != null)
            status.Add(("Forced movement", UIMisc.ColorWarning));
        if (hints.GoalZones.Count > 0)
            status.Add(($"{hints.GoalZones.Count} goal zone{(hints.GoalZones.Count > 1 ? "s" : "")}", UIMisc.ColorGood));

        if (status.Count == 0)
        {
            ImGui.TextDisabled("Idle");
            return;
        }
        for (var i = 0; i < status.Count; ++i)
        {
            if (i > 0)
                UIMisc.SameLineOrWrap(ImGui.CalcTextSize(status[i].text).X + 10 * ImGuiHelpers.GlobalScale);
            UIMisc.Badge(status[i].text, status[i].color);
        }
    }

    private void ToggleMove()
    {
        var normalMove = _pMultibox.Modules[^1];
        normalMove.TransientSettings.RemoveAll(s => s.Track == 0);
        if (_config.ForbidMovement)
            normalMove.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueTrack() { Option = 0 }));
    }

    private void ToggleTarget()
    {
        var autoT = _pMultibox.Modules[0];
        autoT.TransientSettings.RemoveAll(s => s.Track == 0);
        if (_config.ForbidActions)
            autoT.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueTrack() { Option = 1 }));
    }

    private void SetSlot(int slot)
    {
        _masterSlot = slot;
        var mbox = _pMultibox.Modules[1];
        mbox.TransientSettings.RemoveAll(s => s.Track == 0);
        mbox.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueInt() { Value = slot }));

        _config.Enabled = slot >= 0;
        _config.Modified.Fire();
    }

    public override void OnClose()
    {
        _config.DrawUI = false;
        _config.Modified.Fire();
    }
}
