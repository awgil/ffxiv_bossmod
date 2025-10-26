namespace BossMod.Autorotation.MiscAI;
public sealed class FateUtils(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Handin }
    public enum Flag { Enabled, Disabled }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("FATE helper", "Utilities for completing FATEs", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, 1);

        res.Define(Track.Handin).As<Flag>("Hand-in")
            .AddOption(Flag.Enabled, "Automatically hand in FATE items at 10+")
            .AddOption(Flag.Disabled, "Do nothing");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Option(Track.Handin).As<Flag>() != Flag.Enabled)
            return;

        if (!Utils.IsPlayerSyncedToFate(World))
            return;

        var fateID = World.Client.ActiveFate.ID;

        var item = Utils.GetFateItem(fateID);
        if (item == 0)
            return;

        var itemsHeld = (int)World.Client.GetItemQuantity(item);

        if (World.Client.ActiveFate.HandInCount < 10 && World.Client.ActiveFate.HandInCount + itemsHeld >= 10)
        {
            var handinNpc = World.Actors.FirstOrDefault(a => a.FateID == fateID && a.IsTargetable && a.IsAlly && a.EventID > 0);
            Hints.InteractWithTarget = handinNpc;
        }
    }
}
