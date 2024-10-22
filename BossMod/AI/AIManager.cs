using BossMod.Autorotation;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

namespace BossMod.AI;

sealed class AIManager : IDisposable
{
    private readonly RotationModuleManager _autorot;
    private readonly AIController _controller;

    public readonly AIConfig Config = Service.Config.Get<AIConfig>();
    public AIBehaviour? Behaviour { get; private set; }
    public string AIStatus { get; private set; } = "";
    public string NaviStatus { get; private set; } = "";

    public int MasterSlot => (int)Config.FollowSlot; // non-zero means corresponding player is master
    public WorldState WorldState => _autorot.Bossmods.WorldState;
    public float ForceMovementIn => Behaviour?.ForceMovementIn ?? float.MaxValue;

    // TODO: this is not good, callers should use SwitchToXXX directly
    public bool Enabled
    {
        get => Config.Enabled;
        set
        {
            if (Config.Enabled != value)
                Config.Enabled = value;

            if (!value && Behaviour != null)
                SwitchToIdle();
            else if (value && Behaviour == null)
                SwitchToFollow(MasterSlot);
        }
    }

    public AIManager(RotationModuleManager autorot, ActionManagerEx amex, MovementOverride movement)
    {
        _autorot = autorot;
        _controller = new(autorot.WorldState, amex, movement);
        Service.ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        SwitchToIdle();
        Service.ChatGui.ChatMessage -= OnChatMessage;
    }

    public void Update()
    {
        Enabled = Config.Enabled;

        if (!WorldState.Party.Members[MasterSlot].IsValid())
            SwitchToIdle();

        if (!Config.Enabled && Behaviour != null)
            SwitchToIdle();

        var player = WorldState.Party.Player();
        var master = WorldState.Party[MasterSlot];

        if (Behaviour != null && player != null && master != null && !WorldState.Party.Members[PartyState.PlayerSlot].InCutscene)
        {
            Behaviour.Execute(player, master);
        }
        else
        {
            _controller.Clear();
        }

        _controller.Update(player, _autorot.Hints, WorldState.CurrentTime);
        AIStatus = $"AI: {(Behaviour != null ? $"on, {$"master={master?.Name}[{(int)Config.FollowSlot + 1}]"}" : "off")}";
        var dist = _controller.NaviTargetPos != null && player != null ? (_controller.NaviTargetPos.Value - player.Position).Length() : 0;
        NaviStatus = $"Navi={_controller.NaviTargetPos?.ToString() ?? "<none>"} (d={dist:f3}, max-cast={MathF.Min(Behaviour?.ForceMovementIn ?? float.MaxValue, 1000):f3})";
    }

    public void SwitchToIdle()
    {
        Behaviour?.Dispose();
        Behaviour = null;

        Config.FollowSlot = PartyState.PlayerSlot;
        Config.Modified.Fire();
        _controller.Clear();
    }

    public void SwitchToFollow(int masterSlot)
    {
        SwitchToIdle();
        Config.FollowSlot = (AIConfig.Slot)masterSlot;
        Config.Modified.Fire();
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
        if (!Config.Enabled || type != XivChatType.Party)
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
}
