namespace BossMod.Autorotation.MiscAI;

public sealed class FateUtils(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Handin, Collect, Sync, Chocobo }
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

        res.Define(Track.Chocobo).As<Flag>("Chocobo")
            .AddOption(Flag.Enabled, "Resummon chocobo if <60s on timer")
            .AddOption(Flag.Disabled, "Do nothing");

        return res;
    }

    public const int TurnInGoldReq = 10;

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Hints.WantFateSync = strategy.Option(Track.Sync).As<AIHints.FateSync>();

        if (!Utils.IsPlayerSyncedToFate(World))
            return;

        if (strategy.Option(Track.Chocobo).As<Flag>() == Flag.Enabled && World.Client.GetInventoryItemQuantity(ActionDefinitions.IDMiscItemGreens.ID) > 0 && World.Client.ActiveCompanion.TimeLeft < 60)
            Hints.ActionsToExecute.Push(ActionDefinitions.IDMiscItemGreens, Player, ActionQueue.Priority.VeryHigh);

        if (strategy.Option(Track.Handin).As<Flag>() != Flag.Enabled)
            return;

        var fateID = World.Client.ActiveFate.ID;

        var item = Utils.GetFateItem(fateID);
        if (item == 0)
            return;

        // already turned in enough, fate is ending, do nothing
        if (World.Client.ActiveFate.HandInCount >= TurnInGoldReq && World.Client.ActiveFate.Progress >= 100)
            return;

        // until fate is completed, hand in batches of 10; if other people complete the fate, we stop doing stuff
        if (World.Client.GetInventoryItemQuantity(item) >= TurnInGoldReq)
        {
            Hints.InteractWithTarget = World.Actors.Find(World.Client.ActiveFate.ObjectiveNpc);
            return;
        }

        // otherwise, pick up stuff
        if (strategy.Option(Track.Collect).As<Flag>() == Flag.Enabled && !Player.InCombat)
            Hints.InteractWithTarget = World.Actors.Where(a => a.FateID == fateID && a.IsTargetable && a.Type == ActorType.EventObj).MinBy(Player.DistanceToHitbox);
    }
}
