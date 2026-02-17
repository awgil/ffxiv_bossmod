using Lumina.Excel;

namespace BossMod;

// Tweak to automatically dismount when trying to use an action and failing due to being mounted.
public sealed class AutoDismountTweak(WorldState ws, ExcelSheet<Lumina.Excel.Sheets.Action> actionsSheet, ExcelSheet<Lumina.Excel.Sheets.Mount> mountsSheet, ActionTweaksConfig config)
{
    public bool AutoDismountEnabled => config.AutoDismount;

    public bool IsMountPreventingAction(ActionID action) => IsMountPreventingAction(ws, actionsSheet, action);
    public static bool IsMountPreventingAction(WorldState ws, ExcelSheet<Lumina.Excel.Sheets.Action> actionsSheet, ActionID action)
    {
        var player = ws.Party.Player();
        if (player == null || player.MountId == 0)
            return false;

        var canUseWhileMounted = action.Type switch
        {
            ActionType.Spell => actionsSheet.TryGetRow(action.ID, out var data) && (data.CanUseWhileMounted || data.SecondaryCostType == 25),
            ActionType.General => action.ID == 20, // dig
            _ => false
        };
        return !canUseWhileMounted;
    }

    public bool AllowDismount()
    {
        var player = ws.Party.Player();
        var mountData = player != null && player.MountId != 0 ? mountsSheet.GetRowOrDefault(player.MountId) : null;
        return mountData != null && mountData.Value.Order >= 0;
    }
}
