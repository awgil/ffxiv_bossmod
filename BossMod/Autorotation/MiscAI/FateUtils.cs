namespace BossMod.Autorotation.MiscAI;

public sealed class FateUtils(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Handin, Collect, Sync }
    public enum Flag { Enabled, Disabled }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("FATE helper", "Utilities for completing FATEs", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, 1);

        res.Define(Track.Handin).As<Flag>("Hand-in")
            .AddOption(Flag.Enabled, "Automatically hand in FATE items at 10+")
            .AddOption(Flag.Disabled, "Do nothing");

        res.Define(Track.Collect).As<Flag>("Collect")
            .AddOption(Flag.Enabled, "Try to collect FATE items instead of engaging in combat")
            .AddOption(Flag.Disabled, "Do nothing");

        res.Define(Track.Sync).As<AIHints.FateSync>("Sync")
            .AddOption(AIHints.FateSync.None, "Do nothing")
            .AddOption(AIHints.FateSync.Enable, "Always enable level sync if possible")
            .AddOption(AIHints.FateSync.Disable, "Always disable level sync if possible");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Hints.WantFateSync = strategy.Option(Track.Sync).As<AIHints.FateSync>();

        if (strategy.Option(Track.Handin).As<Flag>() != Flag.Enabled)
            return;

        if (!Utils.IsPlayerSyncedToFate(World))
            return;

        var fateID = World.Client.ActiveFate.ID;

        var item = Utils.GetFateItem(fateID);
        if (item == 0)
            return;

        var itemsHeld = (int)World.Client.GetItemQuantity(item);
        var itemsTurnin = World.Client.ActiveFate.HandInCount;
        var itemsTotal = itemsTurnin + itemsHeld;

        if (itemsTurnin < 10)
        {
            if (itemsTotal >= 10)
                Hints.InteractWithTarget = World.Actors.Find(World.Client.ActiveFate.ObjectiveNpc);
            else if (strategy.Option(Track.Collect).As<Flag>() == Flag.Enabled && !Player.InCombat)
                Hints.InteractWithTarget = World.Actors.Where(a => a.FateID == fateID && a.IsTargetable && a.Type == ActorType.EventObj).MinBy(Player.DistanceToHitbox);
        }
    }
}
