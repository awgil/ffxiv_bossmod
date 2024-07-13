using BossMod.Autorotation;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.AI;

sealed class AIManager : IDisposable
{
    private readonly RotationModuleManager _autorot;
    private readonly AIController _controller;
    private readonly AIConfig _config;
    private int _masterSlot = PartyState.PlayerSlot; // non-zero means corresponding player is master
    private AIBehaviour? _beh;
    private Preset? _aiPreset;
    private readonly UISimpleWindow _ui;

    private WorldState WorldState => _autorot.Bossmods.WorldState;

    public AIManager(RotationModuleManager autorot)
    {
        _autorot = autorot;
        _controller = new(autorot.ActionManager);
        _config = Service.Config.Get<AIConfig>();
        _ui = new("AI", DrawOverlay, false, new(100, 100), ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing) { RespectCloseHotkey = false };
        Service.ChatGui.ChatMessage += OnChatMessage;
        Service.CommandManager.AddHandler("/vbmai", new Dalamud.Game.Command.CommandInfo(OnCommand) { HelpMessage = "Toggle AI mode" });
    }

    public void Dispose()
    {
        SwitchToIdle();
        _ui.Dispose();
        Service.ChatGui.ChatMessage -= OnChatMessage;
        Service.CommandManager.RemoveHandler("/vbmai");
    }

    public void Update()
    {
        if (WorldState.Party.ActorIDs[_masterSlot] == 0)
            SwitchToIdle();

        if (!_config.Enabled && _beh != null)
            SwitchToIdle();

        var player = WorldState.Party.Player();
        var master = WorldState.Party[_masterSlot];
        if (_beh != null && player != null && master != null)
        {
            _beh.Execute(player, master);
        }
        else
        {
            _controller.Clear();
        }
        _controller.Update(player);

        _ui.IsOpen = _config.Enabled && player != null && _config.DrawUI;
    }

    private void DrawOverlay()
    {
        ImGui.TextUnformatted($"AI: {(_beh != null ? "on" : "off")}, master={WorldState.Party[_masterSlot]?.Name}");
        ImGui.TextUnformatted($"Navi={_controller.NaviTargetPos} / {_controller.NaviTargetRot}{(_controller.ForceFacing ? " forced" : "")}");
        _beh?.DrawDebug();

        using (var leaderCombo = ImRaii.Combo("Leader", _beh == null ? "<idle>" : WorldState.Party[_masterSlot]?.Name ?? "<unknown>"))
        {
            if (leaderCombo)
            {
                if (ImGui.Selectable("<idle>", _beh == null))
                {
                    SwitchToIdle();
                }
                foreach (var (i, p) in WorldState.Party.WithSlot(true))
                {
                    if (ImGui.Selectable(p.Name, _masterSlot == i))
                    {
                        SwitchToFollow(i);
                    }
                }
            }
        }

        using (var presetCombo = ImRaii.Combo("AI preset", _aiPreset?.Name ?? ""))
        {
            if (presetCombo)
            {
                foreach (var p in _autorot.Database.Presets.Presets)
                {
                    if (ImGui.Selectable(p.Name, p == _aiPreset))
                    {
                        _aiPreset = p;
                        if (_beh != null)
                            _beh.AIPreset = p;
                    }
                }
            }
        }
    }

    private void SwitchToIdle()
    {
        _beh?.Dispose();
        _beh = null;

        _masterSlot = PartyState.PlayerSlot;
        _controller.Clear();
        _autorot.Hints.ForceMovementIn = float.MaxValue;
    }

    private void SwitchToFollow(int masterSlot)
    {
        SwitchToIdle();
        _masterSlot = masterSlot;
        _beh = new AIBehaviour(_controller, _autorot, _aiPreset);
    }

    private int FindPartyMemberSlotFromSender(SeString sender)
    {
        if (sender.Payloads.FirstOrDefault() is not PlayerPayload source)
            return -1;
        var pm = Service.PartyList.FirstOrDefault(pm => pm.Name.TextValue == source.PlayerName && pm.World.Id == source.World.RowId);
        return pm != null ? WorldState.Party.ContentIDs.IndexOf((ulong)pm.ContentId) : -1;
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!_config.Enabled || type != XivChatType.Party)
            return;

        var messagePrefix = message.Payloads.FirstOrDefault() as TextPayload;
        if (messagePrefix?.Text == null || !messagePrefix.Text.StartsWith("vbmai ", StringComparison.Ordinal))
            return;

        var messageData = messagePrefix.Text.Split(' ');
        if (messageData.Length < 2)
            return;

        switch (messageData[1])
        {
            case "follow":
                var master = FindPartyMemberSlotFromSender(sender);
                if (master >= 0)
                    SwitchToFollow(master);
                break;
            case "cancel":
                SwitchToIdle();
                break;
            default:
                Service.Log($"[AI] Unknown command: {messageData[1]}");
                break;
        }
    }

    private void OnCommand(string cmd, string message)
    {
        var messageData = message.Split(' ');
        switch (messageData[0])
        {
            case "on":
                SwitchToFollow(PartyState.PlayerSlot);
                break;
            case "off":
                SwitchToIdle();
                break;
            case "toggle":
                if (_beh == null)
                    SwitchToFollow(PartyState.PlayerSlot);
                else
                    SwitchToIdle();
                break;
            default:
                Service.Log($"[AI] Unknown command: {messageData[0]}");
                break;
        }
    }
}
