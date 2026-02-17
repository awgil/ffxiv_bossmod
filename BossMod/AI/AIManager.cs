using BossMod.Autorotation;
using BossMod.Interfaces;
using Dalamud.Plugin.Services;
//using FFXIVClientStructs.FFXIV.Client.Game.Group;

namespace BossMod.AI;

sealed class AIManager(RotationModuleManager autorot, IAmex amex, IMovementOverride movement, ITargetManager targetManager, IObjectTable objectTable, ICondition condition, AIConfig config) : IDisposable
{
    private readonly AIController _controller = new(autorot.WorldState, amex, movement, targetManager, objectTable, condition);

    public readonly AIConfig Config = config;
    public AIBehaviour? Behaviour { get; private set; }
    public string AIStatus { get; private set; } = "";
    public string NaviStatus { get; private set; } = "";

    public int MasterSlot => (int)Config.FollowSlot; // non-zero means corresponding player is master
    public WorldState WorldState => autorot.Bossmods.WorldState;
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

    public void Dispose()
    {
        SwitchToIdle();
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

        _controller.Update(player, autorot.Hints, WorldState.CurrentTime);
        AIStatus = $"AI: {(Behaviour != null ? $"on, {$"master={master?.Name}[{(int)Config.FollowSlot + 1}]"}" : "off")}";
        var dist = _controller.NaviTargetPos != null && player != null ? (_controller.NaviTargetPos.Value - player.Position).Length() : 0;
        NaviStatus = $"Navi={_controller.NaviTargetPos?.ToString() ?? "<none>"} (d={dist:f3}, max-cast={MathF.Min(Behaviour?.ForceMovementIn ?? float.MaxValue, 1000):f3})";
    }

    public void SwitchToIdle()
    {
        Behaviour?.Dispose();
        Behaviour = null;

        Config.FollowSlot = PartyState.PlayerSlot;
        _controller.Clear();
    }

    public void SwitchToFollow(int masterSlot)
    {
        SwitchToIdle();
        Config.FollowSlot = (AIConfig.Slot)masterSlot;
        Behaviour = new AIBehaviour(_controller, autorot, targetManager, Config);
    }
}
