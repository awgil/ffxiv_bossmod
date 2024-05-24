using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;

namespace BossMod.AI;

sealed class AIManager : IDisposable
{
    private readonly Autorotation _autorot;
    private readonly AIController _controller;
    private readonly AIConfig _config;
    private int _masterSlot = PartyState.PlayerSlot; // non-zero means corresponding player is master
    private AIBehaviour? _beh;
    private readonly UISimpleWindow _ui;

    public AIManager(Autorotation autorot)
    {
        _autorot = autorot;
        _controller = new();
        _config = Service.Config.Get<AIConfig>();
        _ui = new("AI", DrawOverlay, false, new(100, 100), ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing) { RespectCloseHotkey = false };
        Service.ChatGui.ChatMessage += OnChatMessage;
        Service.CommandManager.AddHandler("/bmrai", new Dalamud.Game.Command.CommandInfo(OnCommand) { HelpMessage = "Toggle AI mode" });
        Service.CommandManager.AddHandler("/vbmai", new Dalamud.Game.Command.CommandInfo(OnCommand) { ShowInHelp = false });
    }

    public void Dispose()
    {
        SwitchToIdle();
        _ui.Dispose();
        Service.ChatGui.ChatMessage -= OnChatMessage;
        Service.CommandManager.RemoveHandler("/bmrai");
        Service.CommandManager.RemoveHandler("/vbmai");
    }

    public void Update()
    {
        if (_autorot.WorldState.Party.ContentIDs[_masterSlot] == 0)
            SwitchToIdle();

        if (!_config.Enabled && _beh != null)
            SwitchToIdle();

        var player = _autorot.WorldState.Party.Player();
        var master = _autorot.WorldState.Party[_masterSlot];
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
        ImGui.TextUnformatted($"AI: {(_beh != null ? "on" : "off")}, master={_autorot.WorldState.Party[_masterSlot]?.Name}");
        ImGui.TextUnformatted($"Navi={_controller.NaviTargetPos} / {_controller.NaviTargetRot}{(_controller.ForceFacing ? " forced" : "")}");
        _beh?.DrawDebug();
        if (ImGui.Button("Reset"))
            SwitchToIdle();
        ImGui.SameLine();
        if (ImGui.Button("AI On - Follow selected slot"))
            SwitchToFollow(_config.FollowSlot);
        ImGui.Text("Follow Party Slot");
        ImGui.SameLine();
        var partyMemberNames = new List<string>();
        for (var i = 0; i < 8; i++)
        {
            var member = _autorot.WorldState.Party[i];
            if (member != null)
                partyMemberNames.Add(member.Name);
            else
                partyMemberNames.Add($"Slot {i + 1}");
        }
        var partyMemberNamesArray = partyMemberNames.ToArray();

        ImGui.Combo("##FollowPartySlot", ref _config.FollowSlot, partyMemberNamesArray, partyMemberNamesArray.Length);
    }

    private void SwitchToIdle()
    {
        _beh?.Dispose();
        _beh = null;

        _masterSlot = PartyState.PlayerSlot;
        _controller.Clear();
    }

    private void SwitchToFollow(int masterSlot)
    {
        SwitchToIdle();
        _masterSlot = masterSlot;
        _beh = new AIBehaviour(_controller, _autorot);
    }

    private int FindPartyMemberSlotFromSender(SeString sender)
    {
        if (sender.Payloads.FirstOrDefault() is not PlayerPayload source)
            return -1;
        var pm = Service.PartyList.FirstOrDefault(pm => pm.Name.TextValue == source.PlayerName && pm.World.Id == source.World.RowId);
        return pm != null ? _autorot.WorldState.Party.ContentIDs.IndexOf((ulong)pm.ContentId) : -1;
    }

    private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!_config.Enabled || type != XivChatType.Party)
            return;

        var messagePrefix = message.Payloads.FirstOrDefault() as TextPayload;
        if (messagePrefix?.Text == null || !messagePrefix.Text.StartsWith("bmrai ", StringComparison.Ordinal))
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
        if (messageData.Length == 0)
            return;

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
            case "follow":
                if (messageData.Length < 2)
                {
                    Service.Log("[AI] Missing follow target.");
                    return;
                }

                if (messageData[1].StartsWith("Slot", StringComparison.OrdinalIgnoreCase) && int.TryParse(messageData[1].AsSpan(4), out var slot) && slot >= 1 && slot <= 8)
                    SwitchToFollow(slot - 1);
                else
                {
                    var memberIndex = FindPartyMemberByName(string.Join(" ", messageData.Skip(1)));
                    if (memberIndex >= 0)
                        SwitchToFollow(memberIndex);
                    else
                        Service.Log($"[AI] Unknown party member: {string.Join(" ", messageData.Skip(1))}");
                }
                break;
            default:
                Service.Log($"[AI] Unknown command: {messageData[0]}");
                break;
        }
    }

    private int FindPartyMemberByName(string name)
    {
        for (var i = 0; i < 8; i++)
        {
            var member = _autorot.WorldState.Party[i];
            if (member != null && member.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }
}
