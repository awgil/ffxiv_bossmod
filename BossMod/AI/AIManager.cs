using Dalamud.Game.Gui.Dtr;
using BossMod.Autorotation;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
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
    private Positional DesiredPositional => _config.DesiredPositional;
    private AIBehaviour? _beh;
    private Preset? _aiPreset;
    private readonly UISimpleWindow _ui;
    private readonly IDtrBarEntry _dtrBarEntry = Service.DtrBar.Get("Bossmod");
    private WorldState WorldState => _autorot.Bossmods.WorldState;
    public float ForceMovementIn => _beh?.ForceMovementIn ?? float.MaxValue;

    public AIManager(RotationModuleManager autorot, ActionManagerEx amex)
    {
        _autorot = autorot;
        _controller = new(amex);
        _config = Service.Config.Get<AIConfig>();
        _ui = new("AI", DrawOverlay, false, new(100, 100), ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing) { RespectCloseHotkey = false };
        Service.ChatGui.ChatMessage += OnChatMessage;
        Service.CommandManager.AddHandler("/vbmai", new Dalamud.Game.Command.CommandInfo(OnCommand) { HelpMessage = "Toggle AI mode" });
    }

    public void Dispose()
    {
        SwitchToIdle();
        _ui.Dispose();
        _dtrBarEntry.Remove();
        Service.ChatGui.ChatMessage -= OnChatMessage;
        Service.CommandManager.RemoveHandler("/vbmai");
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
                {
                    if (!_config.Enabled)
                    {
                        _config.Enabled = true;
                        _config.Modified.Fire();
                    }
                    SwitchToFollow((int)_config.FollowSlot);
                }
            };
        }
    }

    public void Update()
    {
        if (!WorldState.Party.Members[MasterSlot].IsValid())
            SwitchToIdle();

        if (!_config.Enabled && _beh != null)
            SwitchToIdle();

        var player = WorldState.Party.Player();
        var master = WorldState.Party[MasterSlot];
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

    private void DrawOverlay()
    {
        ImGui.TextUnformatted($"AI: {(_beh != null ? "on" : "off")}, master={WorldState.Party[MasterSlot]?.Name}");
        ImGui.TextUnformatted($"Navi={_controller.NaviTargetPos} / {_controller.NaviTargetRot}{(_controller.ForceFacing ? " forced" : "")}");
        _beh?.DrawDebug();

        using (var leaderCombo = ImRaii.Combo("Follow", _beh == null ? "<idle>" : (_config.FollowTarget ? "<target>" : WorldState.Party[MasterSlot]?.Name ?? "<unknown>")))
        {
            if (leaderCombo)
            {
                if (ImGui.Selectable("<idle>", _beh == null))
                {
                    SwitchToIdle();
                }
                if (ImGui.Selectable("<target>", _config.FollowTarget))
                {
                    _config.FollowSlot = 0;
                    _config.FollowTarget = true;
                    _config.Modified.Fire();
                    SwitchToFollow(0);
                }
                foreach (var (i, p) in WorldState.Party.WithSlot(true))
                {
                    if (ImGui.Selectable(p.Name, MasterSlot == i))
                    {
                        _config.FollowSlot = (AIConfig.Slot)i;
                        _config.FollowTarget = false;
                        _config.Modified.Fire();
                        SwitchToFollow(i);
                    }
                }
            }
        }

        using (var positionalCombo = ImRaii.Combo("Positional", $"{DesiredPositional}"))
        {
            if (positionalCombo)
            {
                for (var i = 0; i < 4; i++)
                {
                    if (ImGui.Selectable($"{(Positional)i}", DesiredPositional == (Positional)i))
                    {
                        _config.DesiredPositional = (Positional)i;
                        _config.Modified.Fire();
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

        _config.FollowSlot = PartyState.PlayerSlot;
        _config.Modified.Fire();
        _controller.Clear();
    }

    private void SwitchToFollow(int masterSlot)
    {
        SwitchToIdle();
        _config.FollowSlot = (AIConfig.Slot)masterSlot;
        _config.Modified.Fire();
        _beh = new AIBehaviour(_controller, _autorot, _aiPreset);
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
                SwitchToFollow((int)_config.FollowSlot);
                break;
            case "off":
                SwitchToIdle();
                break;
            case "toggle":
                if (_beh == null)
                    SwitchToFollow((int)_config.FollowSlot);
                else
                    SwitchToIdle();
                break;
            case "follow":
                if (messageData.Length < 2)
                {
                    Service.Log($"[AI] [Follow] Usage: /vbmai follow name");
                    return;
                }

                var masterString = messageData.Length > 2 ? $"{messageData[1]} {messageData[2]}" : messageData[1];
                var master = WorldState.Party.WithSlot().FirstOrDefault(x => x.Item2.Name.Equals(masterString, StringComparison.OrdinalIgnoreCase));

                if (master.Item2 is null)
                {
                    Service.Log($"[AI] [Follow] Error: can't find {masterString} in our party");
                    return;
                }

                _config.FollowSlot = (AIConfig.Slot)master.Item1;
                _config.Modified.Fire();
                SwitchToFollow((int)_config.FollowSlot);

                break;
            default:
                Service.Log($"[AI] Unknown command: {messageData[0]}");
                break;
        }
    }
}
