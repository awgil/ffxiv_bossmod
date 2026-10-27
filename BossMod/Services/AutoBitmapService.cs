using BossMod.Pathfinding;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal sealed class AutoBitmapService(
    IDalamudPluginInterface dalamud,
    ICondition condition,
    IClientState clientState,
    IFramework framework,
    WorldState world,
    ObstacleMapManager obstacles,
    IPluginLog logger
) : IHostedService
{
    private readonly DeveloperConfig config = Service.Config.Get<DeveloperConfig>();
    public bool Paused;
    private Exception? LastException;

    private readonly ICallGateSubscriber<bool> isReady = dalamud.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
    private readonly ICallGateSubscriber<Vector3, float, bool, bool> isPointOnMesh = dalamud.GetIpcSubscriber<Vector3, float, bool, bool>("vnavmesh.Query.Mesh.IsPointOnMesh");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        framework.Update += Update;
        clientState.ZoneInit += ZoneInit;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        framework.Update -= Update;
        clientState.ZoneInit -= ZoneInit;
    }

    void Update(IFramework framework)
    {
        if (Paused || world.Party.Player() is not { } player || !config.AutoBitmaps)
            return;

        unsafe
        {
            // vnav isn't reliable in housing zones and there's not really a reason to have bitmaps there anyway
            // TODO isn't there a oneliner for this?
            if (FFXIVClientStructs.FFXIV.Client.Game.GameMain.Instance()->CurrentTerritoryIntendedUseId is FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse.HousingIndoor or FFXIVClientStructs.FFXIV.Client.Enums.TerritoryIntendedUse.HousingOutdoor)
                return;
        }

        // scorched earth approach: if player is moving vertically, assume we can't trust their position
        // this covers jumping and clientpaths, as well as some annoying edge cases that don't show up anywhere else: walking off the boat in ihuykatumu (during which IsJumping() returns false), walking off of any wall-less arena (which puts you into condition 47, not Jumping), etc
        if (player.PosRot.Y != player.PrevPosRot.Y)
            return;

        // try to do nothing if player is in any state that isn't "standing on the ground"
        if (condition.Any(ConditionFlag.BetweenAreas, ConditionFlag.BetweenAreas51, ConditionFlag.OccupiedInCutSceneEvent, ConditionFlag.OccupiedInQuestEvent, ConditionFlag.InFlight, ConditionFlag.Diving, ConditionFlag.Jumping, (ConditionFlag)47, ConditionFlag.DutyRecorderPlayback))
            return;

        var insertAtFront = false;

        var (entry, data) = obstacles.Find(player.PosRot.XYZ());
        if (entry != null && data != null)
        {
            var poff = ((player.Position - entry.Origin) / data.PixelSize).Floor();
            var px = (int)poff.X;
            var py = (int)poff.Z;

            var playerDeepInObstacle = px >= 0 && py >= 0 && px < data.Width && py < data.Height && data[px, py] && (px == 0 || data[px - 1, py]) && (py == 0 || data[px, py - 1]) && (px == (data.Width - 1) || data[px + 1, py]) && (py == (data.Height - 1) || data[px, py + 1]);

            if (playerDeepInObstacle)
                // we are probably standing on an isolated landmass that is within the current map's bounding box
                // we should try to generate a new map from player's current position and make it higher priority (assumption is that this area has a smaller bounding box)
                insertAtFront = true;
            else
                // standing in normal part of map, everything is fine
                return;
        }

        try
        {
            if (!isReady.InvokeFunc())
                return;
        }
        catch (IpcNotReadyError ex)
        {
            logger.Warning(ex, "vnav is not loaded, pausing");
            Paused = true;
            LastException = ex;
            return;
        }

        try
        {
            // if player is standing on a little rock or at the edge of accessible ground, we want to avoid generating a bitmap every frame which just overwrites an existing one
            if (!isPointOnMesh.InvokeFunc(player.PosRot.XYZ(), 2, true))
                return;
        }
        catch (IpcNotReadyError ex)
        {
            logger.Warning(ex, "vnav is too old, must be >=1.2.3.8 to support auto generation");
            Paused = true;
            LastException = ex;
            return;
        }

        try
        {
            obstacles.GenerateMap(player.PosRot.XYZ(), 2048, true, insertAtFront);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Unable to generate an obstacle map, pausing");
            Paused = true;
            LastException = ex;
        }
    }

    private void ZoneInit(ZoneInitEventArgs obj)
    {
        Paused = false;
    }
}
