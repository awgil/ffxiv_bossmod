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

        if (strategy.Option(Track.Chocobo).As<Flag>() == Flag.Enabled && World.CurrentCFCID == 0 && World.Client.GetInventoryItemQuantity(ActionDefinitions.IDMiscItemGreens.ID) > 0 && World.Client.ActiveCompanion is { TimeLeft: < 60, Stabled: false })
            Hints.ActionsToExecute.Push(ActionDefinitions.IDMiscItemGreens, Player, ActionQueue.Priority.VeryHigh);

        var goal = GetGoal(World, Player, strategy);
        if (goal is CollectFateGoal.Pickup or CollectFateGoal.Defend)
        {
            // path to items without pulling. if something already aggroed, finish that fight
            if (primaryTarget == null)
            {
                SetInteract(World.Actors.Where(a => a.FateID == World.Client.ActiveFate.ID && a.IsTargetable && a.Type == ActorType.EventObj).MinBy(Player.DistanceToHitbox));
                return;
            }
        }

        // finish anything we're in combat with before handing in
        if (goal is CollectFateGoal.HandIn && primaryTarget == null)
        {
            SetInteract(World.Actors.Find(World.Client.ActiveFate.ObjectiveNpc));
            return;
        }

        if (Manager.LoSFix is { } los)
        {
            var losDelta = los.Destination - los.Origin;
            var losDist = losDelta.Length();
            var losDir = losDist > 1e-3f ? losDelta / losDist : default;

            // tight spot with high reward to get out of where we're at
            Hints.GoalZones.Add(AIHints.GoalSingleTarget(los.Destination, 0.3f, 120));
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

    public enum Engagement
    {
        /// <summary> Fight whatever autotarget selects </summary>
        Normal,
        /// <summary> Fight whatever is already pulled without pulling more </summary>
        AggroOnly,
        /// <summary> Do not fight anything </summary>
        IgnoreAll
    }

    public static Engagement CurrentEngagement(RotationModuleManager manager, WorldState world, Actor player)
    {
        var engagement = Engagement.Normal;
        foreach (var preset in manager.Presets)
        {
            for (var i = 0; i < preset.Modules.Count; i++)
            {
                if (preset.Modules[i].Type != typeof(FateUtils))
                    continue;
                engagement = MoreRestrictive(engagement, DesiredEngagement(world, player, preset.ActiveStrategyOverrides(i)));
            }
        }

        if (manager.Planner is { Plan: { } plan })
        {
            for (var i = 0; i < plan.Modules.Count; i++)
            {
                if (plan.Modules[i].Type != typeof(FateUtils))
                    continue;
                engagement = MoreRestrictive(engagement, DesiredEngagement(world, player, manager.Planner.ActiveStrategyOverrides(i, world, manager.PlayerSlot)));
            }
        }

        return engagement;
    }

    public static bool ShouldSuppressPull(Engagement engagement, AIHints.Enemy enemy)
    {
        if (engagement == Engagement.Normal || engagement == Engagement.AggroOnly && enemy.Actor.AggroPlayer)
            return false;

        if (enemy.Priority >= 0)
            enemy.Priority = AIHints.Enemy.PriorityUndesirable;
        return true;
    }

    // since nulling ForcedTarget doesn't clear an existing hard target, it's dropped here to prevent autorot from attacking it
    public static unsafe void DropTarget(AIHints hints, ref Actor? primaryTarget)
    {
        primaryTarget = null;
        hints.ForcedTarget = null;
        if (Service.IsMock)
            return;

        var targetSystem = FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance();
        if (targetSystem == null)
            return;
        if (targetSystem->SoftTarget != null)
            targetSystem->SetSoftTarget(null);
        if (targetSystem->Target != null)
            targetSystem->SetHardTarget(null);
    }

    private static Engagement DesiredEngagement(WorldState world, Actor player, StrategyValues strategy) => GetGoal(world, player, strategy) switch
    {
        CollectFateGoal.Pickup => Engagement.IgnoreAll,
        CollectFateGoal.Defend or CollectFateGoal.HandIn => Engagement.AggroOnly,
        _ => Engagement.Normal
    };

    private static Engagement MoreRestrictive(Engagement a, Engagement b) => (Engagement)Math.Max((int)a, (int)b);

    private static CollectFateGoal GetGoal(WorldState world, Actor player, StrategyValues strategy)
    {
        if (strategy.Option(Track.Handin).As<Flag>() != Flag.Enabled)
            return CollectFateGoal.None;

        if (Utils.GetFateItem(world.Client.ActiveFate.ID) is not (not 0 and var itemId))
            return CollectFateGoal.None;

        // already turned in enough, fate is ending, do nothing
        if (world.Client.ActiveFate.HandInCount >= TurnInGoldReq && world.Client.ActiveFate.Progress >= 100)
            return CollectFateGoal.None;

        // until fate is completed, hand in batches of 10; if other people complete the fate, we stop doing stuff
        if (world.Client.GetInventoryItemQuantity(itemId) >= TurnInGoldReq)
            return CollectFateGoal.HandIn;

        if (strategy.Option(Track.Collect).As<Flag>() != Flag.Enabled)
            return CollectFateGoal.None;

        // out of combat, go pick items up; if a mob aggros on the way, fight that and nothing else
        return player.InCombat ? CollectFateGoal.Defend : CollectFateGoal.Pickup;
    }

    private void SetInteract(Actor? target)
    {
        Hints.InteractWithTarget = target;
        // if the auto generated obstacle map is bad, it'll get stuck so force movement regardless
        if (target != null && ShouldForceMovement(target))
            Hints.ForcedMovement = Player.DirectionTo(target).ToVec3(Player.PosRot.Y);
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
        Defend,
    }
}
