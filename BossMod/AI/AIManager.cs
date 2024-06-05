using Dalamud.Game.Gui.Dtr;
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
    private readonly DtrBarEntry _dtrBarEntry;
    private int _masterSlot = PartyState.PlayerSlot; // non-zero means corresponding player is master
    private AIBehaviour? _beh;
    private readonly UISimpleWindow _ui;

    public AIManager(Autorotation autorot)
    {
        _autorot = autorot;
        _controller = new();
        _config = Service.Config.Get<AIConfig>();
        _ui = new("AI", DrawOverlay, false, new(100, 100), ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing) { RespectCloseHotkey = false };
        _dtrBarEntry = Service.DtrBar.Get("Bossmod");
        Service.ChatGui.ChatMessage += OnChatMessage;
        Service.CommandManager.AddHandler("/bmrai", new Dalamud.Game.Command.CommandInfo(OnCommand) { HelpMessage = "Toggle AI mode" });
        Service.CommandManager.AddHandler("/vbmai", new Dalamud.Game.Command.CommandInfo(OnCommand) { ShowInHelp = false });
    }

    public void Dispose()
    {
        SwitchToIdle();
        _ui.Dispose();
        _dtrBarEntry.Dispose();
        Service.ChatGui.ChatMessage -= OnChatMessage;
        Service.CommandManager.RemoveHandler("/bmrai");
        Service.CommandManager.RemoveHandler("/vbmai");
    }

    public void Update()
    {
        if (_autorot.WorldState.Party.ContentIDs[_masterSlot] == 0 && _autorot.WorldState.Party.ActorIDs[_masterSlot] == 0)
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

        DtrUpdate(_beh);
    }

    public void DtrUpdate(AIBehaviour? behaviour)
    {
        _dtrBarEntry.Shown = _config.ShowDTR;
        if (_dtrBarEntry.Shown)
        {
            var status = behaviour != null ? "On" : "Off";
            _dtrBarEntry.Text = "AI: " + status;
            _dtrBarEntry.OnClick = () =>
            {
                if (behaviour != null)
                    SwitchToIdle();
                else
                    SwitchToFollow(_config.FollowSlot);
            };
        }
    }

    private void DrawOverlay()
    {
        ImGui.TextUnformatted($"AI: {(_beh != null ? "on" : "off")}, master={_autorot.WorldState.Party[_masterSlot]?.Name}");
        ImGui.TextUnformatted($"Navi={_controller.NaviTargetPos} / {_controller.NaviTargetRot}{(_controller.ForceFacing ? " forced" : "")}");
        _beh?.DrawDebug();
        if (ImGui.Button("AI on"))
            SwitchToFollow(_config.FollowSlot);
        ImGui.SameLine();
        if (ImGui.Button("AI off"))
            SwitchToIdle();
        ImGui.Text("Follow party slot");
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

        ImGui.Text("Desired positional");
        ImGui.SameLine();
        var positionalOptions = Enum.GetNames(typeof(Positional));
        var positionalIndex = (int)_config.DesiredPositional;
        if (ImGui.Combo("##DesiredPositional", ref positionalIndex, positionalOptions, positionalOptions.Length))
            _config.DesiredPositional = (Positional)positionalIndex;
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
        var master = _autorot.WorldState.Party[masterSlot];
        if (master != null)
        {
            SwitchToIdle();
            _masterSlot = masterSlot;
            _beh = new AIBehaviour(_controller, _autorot);
        }
    }

    private int FindPartyMemberSlotFromSender(SeString sender)
    {
        if (sender.Payloads.FirstOrDefault() is not PlayerPayload source)
            return -1;
        var pm = Service.PartyList.FirstOrDefault(pm => pm.Name.TextValue == source.PlayerName && pm.World.Id == source.World.RowId);
        if (pm != null)
            return _autorot.WorldState.Party.ContentIDs.IndexOf((ulong)pm.ContentId);

        // Check for NPCs (Buddies)
        var buddy = _autorot.WorldState.Party.WithSlot().FirstOrDefault(p => p.Item2.Name.Equals(source.PlayerName, StringComparison.OrdinalIgnoreCase));
        return buddy != default ? buddy.Item1 : -1;
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
                _config.Enabled = true;
                SwitchToFollow(_config.FollowSlot);
                break;
            case "off":
                _config.Enabled = false;
                SwitchToIdle();
                break;
            case "toggle":
                _config.Enabled = !_config.Enabled;
                if (_beh == null)
                    SwitchToFollow(_config.FollowSlot);
                else
                    SwitchToIdle();
                break;
            case "targetmaster":
                _config.FocusTargetLeader = !_config.FocusTargetLeader;
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
            case "debug":
                _config.DrawUI = !_config.DrawUI;
                Service.Log($"[AI] Debug menu is now {(_config.DrawUI ? "enabled" : "disabled")}");
                break;
            case "forbidactions":
                _config.ForbidActions = !_config.ForbidActions;
                Service.Log($"[AI] Forbid actions is now {(_config.ForbidActions ? "enabled" : "disabled")}");
                break;
            case "forbidmovement":
                _config.ForbidMovement = !_config.ForbidMovement;
                Service.Log($"[AI] Forbid movement is now {(_config.ForbidMovement ? "enabled" : "disabled")}");
                break;
            case "followoutofcombat":
                _config.FollowOutOfCombat = !_config.FollowOutOfCombat;
                Service.Log($"[AI] Forbid movement is now {(_config.FollowOutOfCombat ? "enabled" : "disabled")}");
                break;
            case "followcombat":
                if (_config.FollowDuringCombat)
                {
                    _config.FollowDuringCombat = false;
                    _config.FollowDuringActiveBossModule = false;
                }
                else
                {
                    _config.FollowDuringCombat = true;
                }
                Service.Log($"[AI] Follow during combat is now {(_config.FollowDuringCombat ? "enabled" : "disabled")}");
                Service.Log($"[AI] Follow during active boss module is now {(_config.FollowDuringActiveBossModule ? "enabled" : "disabled")}");
                break;
            case "followmodule":
                if (_config.FollowDuringActiveBossModule)
                {
                    _config.FollowDuringActiveBossModule = false;
                }
                else
                {
                    _config.FollowDuringActiveBossModule = true;
                    _config.FollowDuringCombat = true;
                }
                Service.Log($"[AI] Follow during active boss module is now {(_config.FollowDuringActiveBossModule ? "enabled" : "disabled")}");
                Service.Log($"[AI] Follow during combat is now {(_config.FollowDuringCombat ? "enabled" : "disabled")}");
                break;
            case "followtarget":
                _config.FollowTarget = !_config.FollowTarget;
                Service.Log($"[AI] Following targets is now {(_config.FollowTarget ? "enabled" : "disabled")}");
                break;
            case "positional":
                if (messageData.Length < 2)
                {
                    Service.Log("[AI] Missing positional type.");
                    return;
                }
                SetPositional(messageData[1]);
                break;
            default:
                Service.Log($"[AI] Unknown command: {messageData[0]}");
                break;
        }
    }

    private void SetPositional(string positional)
    {
        switch (positional.ToLower())
        {
            case "any":
                _config.DesiredPositional = Positional.Any;
                break;
            case "flank":
                _config.DesiredPositional = Positional.Flank;
                break;
            case "rear":
                _config.DesiredPositional = Positional.Rear;
                break;
            case "front":
                _config.DesiredPositional = Positional.Front;
                break;
            default:
                Service.Log($"[AI] Unknown positional: {positional}");
                return;
        }
        Service.Log($"[AI] Desired positional set to {_config.DesiredPositional}");
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
