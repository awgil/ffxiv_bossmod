using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace BossMod.AI;

sealed class AIManager : IDisposable
{
    public static AIManager? Instance { get; private set; }
    public readonly Autorotation Autorot;
    public readonly AIController Controller;
    private readonly AIConfig _config;
    private readonly DtrBarEntry _dtrBarEntry;
    private readonly AIManagementWindow _wndAI;
    public int MasterSlot = PartyState.PlayerSlot; // non-zero means corresponding player is master
    public AIBehaviour? Beh;

    public AIManager(Autorotation autorot)
    {
        Instance = this;
        _wndAI = new AIManagementWindow(this);
        Autorot = autorot;
        Controller = new();
        _config = Service.Config.Get<AIConfig>();
        _dtrBarEntry = Service.DtrBar.Get("Bossmod");
        Service.ChatGui.ChatMessage += OnChatMessage;
        Service.CommandManager.AddHandler("/bmrai", new Dalamud.Game.Command.CommandInfo(OnCommand) { HelpMessage = "Toggle AI mode" });
        Service.CommandManager.AddHandler("/vbmai", new Dalamud.Game.Command.CommandInfo(OnCommand) { ShowInHelp = false });
    }

    public void Dispose()
    {
        SwitchToIdle();
        _dtrBarEntry.Dispose();
        _wndAI.Dispose();
        Service.ChatGui.ChatMessage -= OnChatMessage;
        Service.CommandManager.RemoveHandler("/bmrai");
        Service.CommandManager.RemoveHandler("/vbmai");
    }

    public void Update()
    {
        if (Autorot.WorldState.Party.ContentIDs[MasterSlot] == 0 && Autorot.WorldState.Party.ActorIDs[MasterSlot] == 0)
            SwitchToIdle();

        if (!_config.Enabled && Beh != null)
            SwitchToIdle();

        var player = Autorot.WorldState.Party.Player();
        var master = Autorot.WorldState.Party[MasterSlot];
        if (Beh != null && player != null && master != null)
            Beh.Execute(player, master);
        else
            Controller.Clear();
        Controller.Update(player);

        DtrUpdate(Beh);
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

    public void SwitchToIdle()
    {
        Beh?.Dispose();
        Beh = null;

        MasterSlot = PartyState.PlayerSlot;
        Controller.Clear();
        _wndAI.UpdateTitle();
    }

    public void SwitchToFollow(int masterSlot)
    {
        var master = Autorot.WorldState.Party[masterSlot];
        if (master != null)
        {
            SwitchToIdle();
            MasterSlot = masterSlot;
            Beh = new AIBehaviour(Controller, Autorot);
            _wndAI.UpdateTitle();
        }
    }

    private int FindPartyMemberSlotFromSender(SeString sender)
    {
        if (sender.Payloads.FirstOrDefault() is not PlayerPayload source)
            return -1;
        var pm = Service.PartyList.FirstOrDefault(pm => pm.Name.TextValue == source.PlayerName && pm.World.Id == source.World.RowId);
        if (pm != null)
            return Autorot.WorldState.Party.ContentIDs.IndexOf((ulong)pm.ContentId);

        // Check for NPCs (Buddies)
        var buddy = Autorot.WorldState.Party.WithSlot().FirstOrDefault(p => p.Item2.Name.Equals(source.PlayerName, StringComparison.OrdinalIgnoreCase));
        return buddy != default ? buddy.Item1 : -1;
    }

    private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!_config.Enabled || type != XivChatType.Party)
            return;

        var messagePrefix = message.Payloads.FirstOrDefault() as TextPayload;
        if (messagePrefix?.Text == null)
            return;

        var messageText = messagePrefix.Text;
        if (!messageText.StartsWith("bmrai ", StringComparison.OrdinalIgnoreCase) && !messageText.StartsWith("vbmai ", StringComparison.OrdinalIgnoreCase))
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

        var configModified = false;

        switch (messageData[0].ToUpperInvariant())
        {
            case "ON":
                configModified = EnableConfig(true);
                break;
            case "OFF":
                configModified = EnableConfig(false);
                break;
            case "TOGGLE":
                configModified = ToggleConfig();
                break;
            case "TARGETMASTER":
                configModified = ToggleFocusTargetLeader();
                break;
            case "FOLLOW":
                configModified = HandleFollowCommand(messageData);
                break;
            case "UI":
                configModified = ToggleDebugMenu();
                break;
            case "FORBIDACTIONS":
                configModified = ToggleForbidActions();
                break;
            case "FORDBIDMOVEMENT":
                configModified = ToggleForbidMovement();
                break;
            case "FOLLOWOUTOFCOMBAT":
                configModified = ToggleFollowOutOfCombat();
                break;
            case "FOLLOWCOMBAT":
                configModified = ToggleFollowCombat();
                break;
            case "FOLLOWMODULE":
                configModified = ToggleFollowModule();
                break;
            case "FOLLOWTARGET":
                configModified = ToggleFollowTarget(messageData);
                break;
            case "POSITIONAL":
                configModified = HandlePositionalCommand(messageData);
                break;
            default:
                Service.Log($"[AI] Unknown command: {messageData[0]}");
                break;
        }

        if (configModified)
            _config.Modified.Fire();
    }

    private bool EnableConfig(bool isEnabled)
    {
        _config.Enabled = isEnabled;
        if (isEnabled)
            SwitchToFollow(_config.FollowSlot);
        else
            SwitchToIdle();
        return true;
    }

    private bool ToggleConfig()
    {
        _config.Enabled = !_config.Enabled;
        if (Beh == null)
            SwitchToFollow(_config.FollowSlot);
        else
            SwitchToIdle();
        return true;
    }

    private bool ToggleFocusTargetLeader()
    {
        _config.FocusTargetLeader = !_config.FocusTargetLeader;
        return true;
    }

    private bool HandleFollowCommand(string[] messageData)
    {
        if (messageData.Length < 2)
        {
            Service.Log("[AI] Missing follow target.");
            return false;
        }

        if (messageData[1].StartsWith("Slot", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(messageData[1].AsSpan(4), out var slot) && slot >= 1 && slot <= 8)
        {
            SwitchToFollow(slot - 1);
            _config.FollowSlot = slot - 1;
        }
        else
        {
            var memberIndex = FindPartyMemberByName(string.Join(" ", messageData.Skip(1)));
            if (memberIndex >= 0)
            {
                SwitchToFollow(memberIndex);
                _config.FollowSlot = memberIndex;
            }
            else
            {
                Service.Log($"[AI] Unknown party member: {string.Join(" ", messageData.Skip(1))}");
                return false;
            }
        }
        return true;
    }

    private bool ToggleDebugMenu()
    {
        _config.DrawUI = !_config.DrawUI;
        Service.Log($"[AI] AI menu is now {(_config.DrawUI ? "enabled" : "disabled")}");
        return true;
    }

    private bool ToggleForbidActions()
    {
        _config.ForbidActions = !_config.ForbidActions;
        Service.Log($"[AI] Forbid actions is now {(_config.ForbidActions ? "enabled" : "disabled")}");
        return true;
    }

    private bool ToggleForbidMovement()
    {
        _config.ForbidMovement = !_config.ForbidMovement;
        Service.Log($"[AI] Forbid movement is now {(_config.ForbidMovement ? "enabled" : "disabled")}");
        return true;
    }

    private bool ToggleFollowOutOfCombat()
    {
        _config.FollowOutOfCombat = !_config.FollowOutOfCombat;
        Service.Log($"[AI] Follow out of combat is now {(_config.FollowOutOfCombat ? "enabled" : "disabled")}");
        return true;
    }

    private bool ToggleFollowCombat()
    {
        if (_config.FollowDuringCombat)
        {
            _config.FollowDuringCombat = false;
            _config.FollowDuringActiveBossModule = false;
        }
        else
            _config.FollowDuringCombat = true;
        Service.Log($"[AI] Follow during combat is now {(_config.FollowDuringCombat ? "enabled" : "disabled")}");
        Service.Log($"[AI] Follow during active boss module is now {(_config.FollowDuringActiveBossModule ? "enabled" : "disabled")}");
        return true;
    }

    private bool ToggleFollowModule()
    {
        if (_config.FollowDuringActiveBossModule)
            _config.FollowDuringActiveBossModule = false;
        else
        {
            _config.FollowDuringActiveBossModule = true;
            _config.FollowDuringCombat = true;
        }
        Service.Log($"[AI] Follow during active boss module is now {(_config.FollowDuringActiveBossModule ? "enabled" : "disabled")}");
        Service.Log($"[AI] Follow during combat is now {(_config.FollowDuringCombat ? "enabled" : "disabled")}");
        return true;
    }

    private bool ToggleFollowTarget(string[] messageData)
    {
        if (messageData.Length == 1)
            _config.FollowTarget = !_config.FollowTarget;
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowTarget = true;
                    break;
                case "OFF":
                    _config.FollowTarget = false;
                    break;
                default:
                    Service.Log($"[AI] Unknown follow target command: {messageData[1]}");
                    return _config.FollowTarget;
            }
        }
        Service.Log($"[AI] Following targets is now {(_config.FollowTarget ? "enabled" : "disabled")}");
        return _config.FollowTarget;
    }

    private bool HandlePositionalCommand(string[] messageData)
    {
        if (messageData.Length < 2)
        {
            Service.Log("[AI] Missing positional type.");
            return false;
        }
        SetPositional(messageData[1]);
        return true;
    }

    private void SetPositional(string positional)
    {
        switch (positional.ToUpperInvariant())
        {
            case "ANY":
                _config.DesiredPositional = Positional.Any;
                break;
            case "FLANK":
                _config.DesiredPositional = Positional.Flank;
                break;
            case "REAR":
                _config.DesiredPositional = Positional.Rear;
                break;
            case "FRONT":
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
            var member = Autorot.WorldState.Party[i];
            if (member != null && member.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }
}
