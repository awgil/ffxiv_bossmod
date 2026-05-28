namespace BossMod.Autorotation.MiscAI;

public sealed class FateUtils(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public override bool WantsLoSFix => true;

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

        if (strategy.Option(Track.Chocobo).As<Flag>() == Flag.Enabled && World.Client.GetInventoryItemQuantity(ActionDefinitions.IDMiscItemGreens.ID) > 0 && World.Client.ActiveCompanion is { TimeLeft: < 60, Stabled: false })
            Hints.ActionsToExecute.Push(ActionDefinitions.IDMiscItemGreens, Player, ActionQueue.Priority.VeryHigh);

        var goal = GetGoal(strategy);
        if (goal is CollectFateGoal.HandIn)
        {
            var target = World.Actors.Find(World.Client.ActiveFate.ObjectiveNpc);
            Hints.InteractWithTarget = target;
            // if the auto generated obstacle map is bad, it'll get stuck so force movement regardless
            if (target != null && ShouldForceMovement(target))
                Hints.ForcedMovement = Player.DirectionTo(target).ToVec3(Player.PosRot.Y);
            return;
        }
        else if (goal is CollectFateGoal.Pickup)
        {
            var target = World.Actors.Where(a => a.FateID == World.Client.ActiveFate.ID && a.IsTargetable && a.Type == ActorType.EventObj).MinBy(Player.DistanceToHitbox);
            Hints.InteractWithTarget = target;
            if (target != null && ShouldForceMovement(target))
                Hints.ForcedMovement = Player.DirectionTo(target).ToVec3(Player.PosRot.Y);
            return;
        }

        if (Manager.LoSFix is { } los)
        {
            var losDelta = los.Destination - los.Origin;
            var losDist = losDelta.Length();
            var losDir = losDist > 1e-3f ? losDelta / losDist : default;

            // tight spot with high reward to get out of where we're at
            Hints.GoalZones.Add(Hints.GoalSingleTarget(los.Destination, 0.3f, 120));
            // add a penalty to current position to actually encourage moving out of it
            Hints.GoalZones.Add(p => p.InCircle(los.Origin, 1.0f) ? -25 : 0);
            // more encouragement for going towards the destination, but need to cap it or else it'll just keep going
            Hints.GoalZones.Add(p =>
            {
                var progress = WDir.Dot(p - los.Origin, losDir);
                if (progress <= 0)
                    return 0;

                var cappedProgress = MathF.Min(progress, losDist);
                var overshoot = MathF.Max(0, progress - losDist);
                return cappedProgress * 8 - overshoot * 16;
            });
        }
    }

    private CollectFateGoal GetGoal(StrategyValues strategy)
    {
        if (strategy.Option(Track.Handin).As<Flag>() != Flag.Enabled)
            return CollectFateGoal.None;

        if (Utils.GetFateItem(World.Client.ActiveFate.ID) is not (not 0 and var itemId))
            return CollectFateGoal.None;

        // already turned in enough, fate is ending, do nothing
        if (World.Client.ActiveFate.HandInCount >= TurnInGoldReq && World.Client.ActiveFate.Progress >= 100)
            return CollectFateGoal.None;

        // until fate is completed, hand in batches of 10; if other people complete the fate, we stop doing stuff
        if (World.Client.GetInventoryItemQuantity(itemId) >= TurnInGoldReq)
            return CollectFateGoal.HandIn;

        // pick up stuff
        return strategy.Option(Track.Collect).As<Flag>() == Flag.Enabled && !Player.InCombat ? CollectFateGoal.Pickup : CollectFateGoal.None;
    }

    // no path = force movement anyway
    private bool ShouldForceMovement(Actor target)
        => Hints.PathfindMapObstacles.Bitmap != null
            && (!Hints.PathfindMapBounds.Contains(target.Position - Hints.PathfindMapCenter) || !Hints.PathfindMapObstacles.HasObstacleMapLineOfSight(Hints.PathfindMapCenter, Player.Position, target.Position));

    private enum CollectFateGoal
    {
        None,
        HandIn,
        Pickup,
    }
}
