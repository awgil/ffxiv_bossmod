using BossMod.Autorotation;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using ImGuiNET;

namespace BossMod.AI;

sealed class AIManager : IDisposable
{
    private readonly RotationModuleManager _autorot;
    private readonly AIController _controller;
    private readonly AIConfig _config;
    private int MasterSlot => (int)_config.FollowSlot; // non-zero means corresponding player is master
    private readonly UISimpleWindow _ui;
    private WorldState WorldState => _autorot.Bossmods.WorldState;
    private string _aiStatus = "";
    private string _naviStatus = "";

    public float ForceMovementIn => Behaviour?.ForceMovementIn ?? float.MaxValue;
    public AIBehaviour? Behaviour { get; private set; }

    private bool Enabled
    {
        get => _config.Enabled;
        set
        {
            if (_config.Enabled != value)
                _config.Enabled = value;

            if (!value && Behaviour != null)
                SwitchToIdle();
            else if (value && Behaviour == null)
                SwitchToFollow(MasterSlot);
        }
    }

    public AIManager(RotationModuleManager autorot, ActionManagerEx amex, MovementOverride movement)
    {
        _autorot = autorot;
        _controller = new(amex, movement);
        _config = Service.Config.Get<AIConfig>();
        _ui = new("###AI", DrawOverlay, false, new(100, 100), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing)
        {
            RespectCloseHotkey = false,
            ShowCloseButton = false,
            WindowName = $"AI: off###AI",
            TitleBarButtons = [new() { Icon = FontAwesomeIcon.WindowClose, IconOffset = new(1, 1), Click = _ => _config.DrawUI = false }]
        };
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
        Enabled = _config.Enabled;

        if (!WorldState.Party.Members[MasterSlot].IsValid())
            SwitchToIdle();

        if (!_config.Enabled && Behaviour != null)
            SwitchToIdle();

        var player = WorldState.Party.Player();
        var master = WorldState.Party[MasterSlot];

        if (Behaviour != null && player != null && master != null)
        {
            Behaviour.Execute(player, master);
        }
        else
        {
            _controller.Clear();
        }

        _controller.Update(player, _autorot.Hints, WorldState.CurrentTime);
        _aiStatus = $"AI: {(Behaviour != null ? $"on, {$"master={master?.Name}[{(int)_config.FollowSlot + 1}]"}" : "off")}";
        var dist = _controller.NaviTargetPos != null && player != null ? (_controller.NaviTargetPos.Value - player.Position).Length() : 0;
        _naviStatus = $"Navi={_controller.NaviTargetPos?.ToString() ?? "<none>"} (d={dist:f3}, max-cast={MathF.Min(Behaviour?.ForceMovementIn ?? float.MaxValue, 1000):f3})";
        _ui.IsOpen = player != null && _config.DrawUI;
        _ui.WindowName = _config.ShowStatusOnTitlebar ? $"{_aiStatus}, {_naviStatus}###AI" : $"AI###AI";
    }

    private void DrawOverlay()
    {
        if (!_config.ShowStatusOnTitlebar)
        {
            ImGui.TextUnformatted(_aiStatus);
            ImGui.TextUnformatted(_naviStatus);
        }
        Behaviour?.DrawDebug();

        using (var leaderCombo = ImRaii.Combo("Follow", Behaviour == null ? "<idle>" : WorldState.Party[MasterSlot]?.Name ?? "<unknown>"))
        {
            if (leaderCombo)
            {
                if (ImGui.Selectable("<idle>", Behaviour == null))
                {
                    Enabled = false;
                }
                foreach (var (i, p) in WorldState.Party.WithSlot(true))
                {
                    if (ImGui.Selectable(p.Name, MasterSlot == i))
                    {
                        _config.FollowSlot = (AIConfig.Slot)i;
                        _config.Modified.Fire();
                        Enabled = true;
                    }
                }
            }
        }
    }

    public void SwitchToIdle()
    {
        Behaviour?.Dispose();
        Behaviour = null;

        _config.FollowSlot = PartyState.PlayerSlot;
        _config.Modified.Fire();
        _controller.Clear();
    }

    public void SwitchToFollow(int masterSlot)
    {
        SwitchToIdle();
        _config.FollowSlot = (AIConfig.Slot)masterSlot;
        _config.Modified.Fire();
        Behaviour = new AIBehaviour(_controller, _autorot);
    }

    private unsafe int FindPartyMemberSlotFromSender(SeString sender)
    {
        if (sender.Payloads.FirstOrDefault() is not PlayerPayload source)
            return -1;
        var group = GroupManager.Instance()->GetGroup();
        var slot = -1;
        for (int i = 0; i < group->MemberCount; ++i)
        {
            if (group->PartyMembers[i].HomeWorld == source.World.RowId && group->PartyMembers[i].NameString == source.PlayerName)
            {
                slot = i;
                break;
            }
        }
        return slot >= 0 ? Array.FindIndex(WorldState.Party.Members, m => m.ContentId == group->PartyMembers[slot].ContentId) : -1;
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
                Enabled = true;
                break;
            case "off":
                Enabled = false;
                break;
            case "toggle":
                Enabled ^= true;
                break;
            case "follow":
                if (messageData.Length < 2)
                {
                    Service.Log($"[AI] [Follow] Usage: /vbmai follow name");
                    return;
                }

                var masterString = messageData.Length > 2 ? $"{messageData[1]} {messageData[2]}" : messageData[1];
                var masterStringIsSlot = masterString[..4].Equals("slot", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(masterString.Substring(4, 1)) : 0;

                var master = masterStringIsSlot > 0 ? (masterStringIsSlot - 1, WorldState.Party[masterStringIsSlot - 1]) : WorldState.Party.WithSlot().FirstOrDefault(x => x.Item2.Name.Equals(masterString, StringComparison.OrdinalIgnoreCase));

                if (master.Item2 is null)
                {
                    Service.Log($"[AI] [Follow] Error: can't find {masterString} in our party");
                    return;
                }

                _config.FollowSlot = (AIConfig.Slot)master.Item1;
                _config.Modified.Fire();
                Enabled = true;

                break;
            case "ui":
                _config.DrawUI ^= true;
                _config.Modified.Fire();
                break;
            default:
                List<string> list = [];
                list.Add("AIConfig");
                list.AddRange(messageData);

                if (list.Count == 2)
                {
                    //toggle
                    var result = Service.Config.ConsoleCommand(list);
                    if (bool.TryParse(result[0], out var resultBool))
                    {
                        list.Add((!resultBool).ToString());
                        Service.Config.ConsoleCommand(list);
                    }
                    else
                        Service.Log($"[AI] Unknown command: {messageData[0]}");
                }
                else if (list.Count == 3)
                {
                    //set
                    var onOffReplace = list[2].Replace("on", "true", StringComparison.InvariantCultureIgnoreCase).Replace("off", "false", StringComparison.InvariantCultureIgnoreCase);
                    list[2] = onOffReplace;

                    if (Service.Config.ConsoleCommand(list).Count > 0)
                        Service.Log($"[AI] Unknown command: {messageData[0]}");
                }
                else
                    Service.Log($"[AI] Unknown command: {messageData[0]}");

                break;
        }
    }
}
