﻿using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace BossMod;

// Tweak to automatically dismount when trying to use an action and failing due to being mounted.
public sealed class AutoDismountTweak(WorldState ws)
{
    private readonly ActionTweaksConfig _config = Service.Config.Get<ActionTweaksConfig>();

    public bool AutoDismountEnabled => _config.AutoDismount;

    public bool IsMountPreventingAction(ActionID action)
    {
        var player = ws.Party.Player();
        if (player == null || player.MountId == 0)
            return false;

        var canUseWhileMounted = action.Type switch
        {
            ActionType.Spell => Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(action.ID) is var data && data != null && (data.Unknown65 || data.SecondaryCostType == 25),
            ActionType.General => action.ID == 20, // dig
            _ => false
        };
        return !canUseWhileMounted;
    }

    public bool AllowDismount()
    {
        var player = ws.Party.Player();
        var mountData = player != null && player.MountId != 0 ? Service.LuminaRow<Lumina.Excel.GeneratedSheets.Mount>(player.MountId) : null;
        return mountData != null && mountData.Order >= 0;
    }
}
