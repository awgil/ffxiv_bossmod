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
        if (enemy.Actor.IsForlorn) // forlorns should always be pulled no matter what
            return false;
        if (engagement == Engagement.Normal || engagement == Engagement.AggroOnly && enemy.Actor.AggroPlayer)
            return false;

        if (enemy.Priority >= 0)
            enemy.Priority = AIHints.Enemy.PriorityUndesirable;
        return true;
    }

    // a forlorn is already on us. trash pulls are pointless until it dies, since AI ignores them
    public static bool EngagedWithForlorn(Actor player, AIHints hints)
    {
        foreach (var enemy in hints.PotentialTargets)
        {
            var actor = enemy.Actor;
            if (actor.IsDead || !actor.IsForlorn)
                continue;
            if (actor.AggroPlayer || actor.TargetID == player.InstanceID || actor.InCombat && player.TargetID == actor.InstanceID)
                return true;
        }
        return false;
    }

    // true when the filler aoe on the player or on a mob already in the pack would hit this add, so it doesn't need its own pull
    public static bool InsidePackAOE(Actor player, Actor target, AIHints hints)
    {
        if (InPackRadius(player.Position, target))
            return true;
        foreach (var enemy in hints.PotentialTargets)
        {
            var actor = enemy.Actor;
            if (actor == target || actor.FateID != target.FateID || actor.IsDead)
                continue;
            if ((actor.AggroPlayer || actor.TargetID == player.InstanceID) && InPackRadius(actor.Position, target))
                return true;
        }
        return false;
    }

    // filler gcd aoe radius (for most jobs). an unpulled mob inside of this would be tagged by the next aoe and doesn't need to be manually tagged
    private const float PackAOERadius = 5;
    private static bool InPackRadius(WPos origin, Actor target) => target.Position.InCircle(origin, PackAOERadius + target.HitboxRadius);

    // for when you have a pack of mobs and want to just tag new mobs instead of running towards them
    public static bool HoldingTrashPack(WorldState world, Actor player, AIHints hints, Engagement engagement)
        => engagement == Engagement.Normal && HasAggroedTrash(world, player, hints);

    private static bool HasAggroedTrash(WorldState world, Actor player, AIHints hints)
    {
        if (!Utils.IsPlayerSyncedToFate(world) || EngagedWithForlorn(player, hints))
            return false;
        var fateId = world.Client.ActiveFate.ID;
        if (!Utils.IsNonBossFate(fateId)) // trash in a boss fate doesn't matter
            return false;
        foreach (var enemy in hints.PotentialTargets)
        {
            var actor = enemy.Actor;
            if (actor.FateID != fateId || actor.IsDead || !actor.IsTargetable)
                continue;
            if (actor.AggroPlayer || actor.TargetID == player.InstanceID)
                return true;
        }
        return false;
    }

    public static float TrashPullLeash(RotationModuleManager manager, WorldState world, Actor player, AIHints hints, Actor? primary)
    {
        if (primary == null || CurrentEngagement(manager, world, player) != Engagement.Normal)
            return 0;
        if (!TryGetTagPlan(world, player, hints, out var plan))
            return 0;
        if (player.DistanceToHitbox(plan.Target) <= plan.Range)
            return 0;
        return MathF.Max(0, primary.DistanceToHitbox(plan.Target) - plan.Range) + 2; // 0 = nothing to be tagged
    }

    public static void PullTrashAtRange(WorldState world, Actor player, AIHints hints)
    {
        if (!TryGetTagPlan(world, player, hints, out var plan))
            return;

        var dist = player.DistanceToHitbox(plan.Target);
        // provoke if possible to tag a mob since it's a free ogcd
        if (plan.ProvokeReady && dist <= ActionDefinitions.Instance[ActionID.MakeSpell(ClassShared.AID.Provoke)]!.Range)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Provoke), plan.Target, ActionQueue.Priority.Medium);

        if (dist <= plan.Range)
            return;

        var pullPos = plan.Target.Position;
        var reach = plan.Range + plan.Target.HitboxRadius + player.HitboxRadius;
        var origin = player.Position;
        // get as close to the mob as possible within tag range and let it come to the pack
        hints.GoalZones.Add(p => (p - pullPos).Length() <= reach ? 100 - (p - origin).Length() * 0.05f : 0);
    }

    // aims one ranged gcd at an unpulled mob, unless your normal rot would already hit it
    public static void TagTrash(WorldState world, Actor player, AIHints hints)
    {
        if (!TryGetTagPlan(world, player, hints, out var plan) || plan.ProvokeReady || plan.HoldGcd)
            return;
        if (plan.Target.Visibility != Visibility.Visible)
            return;

        var entries = hints.ActionsToExecute.Entries;
        var bestSt = -1;
        var bestStPrio = float.MinValue;
        var bestStExpire = float.MaxValue;
        var bestGcdPrio = float.MinValue;
        var bestGcdExpire = float.MaxValue;
        var preferred = ActionAspect.None;
        for (var i = 0; i < entries.Count; ++i)
        {
            var entry = entries[i];
            if (ActionDefinitions.Instance[entry.Action] is not { } def || !def.IsGCD || !ConflictsWithTag(def))
                continue;
            var betterGcd = entry.Priority > bestGcdPrio || entry.Priority == bestGcdPrio && entry.Expire < bestGcdExpire;
            if (betterGcd)
            {
                bestGcdPrio = entry.Priority;
                bestGcdExpire = entry.Expire;
                preferred = def.Aspect;
            }
            if (entry.Target is not { IsAlly: false } || !CanTag(world, player, hints, def, plan.Target))
                continue;
            if (!IsHostileSingleTarget(def))
                continue;
            if (entry.Priority > bestStPrio || entry.Priority == bestStPrio && entry.Expire < bestStExpire)
            {
                bestSt = i;
                bestStPrio = entry.Priority;
                bestStExpire = entry.Expire;
            }
        }

        if (bestSt >= 0)
        {
            var entry = entries[bestSt];
            // a melee or aoe gcd would otherwise fire first and leave the add where it is
            var priority = bestGcdPrio > entry.Priority ? bestGcdPrio + 10 : entry.Priority;
            entries[bestSt] = entry with { Target = plan.Target, Priority = priority };
            return;
        }

        if (GetTagAction(world, player, preferred) is not { } tag)
            return;
        var tagDef = ActionDefinitions.Instance[tag];
        if (tagDef == null || !CanTag(world, player, hints, tagDef, plan.Target))
            return;
        if (player.DistanceToHitbox(plan.Target) <= 3) // don't spend a ranged gcd on something standing in melee gcd range
            return;
        var tagPriority = bestGcdPrio > float.MinValue ? MathF.Max(ActionQueue.Priority.High + 500, bestGcdPrio + 10) : ActionQueue.Priority.High + 500;
        hints.ActionsToExecute.Push(tag, plan.Target, tagPriority);
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

    private readonly record struct TagPlan(Actor Target, float Range, bool ProvokeReady, bool HoldGcd);

    // nearest unpulled add that the filler aoe won't already hit, plus how far the tag reaches. false when there is nothing to pull
    private static bool TryGetTagPlan(WorldState world, Actor player, AIHints hints, out TagPlan plan)
    {
        plan = default;

        if (!HasAggroedTrash(world, player, hints))
            return false;
        var fateId = world.Client.ActiveFate.ID;
        if (!Utils.IsNonBossFate(fateId))
            return false;

        var provoke = ActionID.MakeSpell(ClassShared.AID.Provoke);
        var provokeDef = ActionDefinitions.Instance[provoke];
        var provokeReadyIn = provokeDef != null && provokeDef.IsUnlocked(world, player) ? provokeDef.ReadyIn(world.Client.Cooldowns, world.Client.DutyActions) : float.MaxValue;
        var provokeReady = provokeReadyIn <= 0;

        var tag = GetTagAction(world, player, ActionAspect.None);
        var tagDef = tag != null ? ActionDefinitions.Instance[tag.Value] : null;

        // about to come off cooldown: walk to provoke range and keep the gcd on the pack
        float pullRange;
        var holdGcd = false;
        if (provokeReady || provokeReadyIn <= 2.5f)
        {
            pullRange = provokeDef!.Range;
            holdGcd = !provokeReady;
        }
        else if (tagDef != null)
            pullRange = tagDef.Range;
        else
            return false;

        Actor? nearest = null;
        var nearestDist = float.MaxValue;
        var havePack = false;
        foreach (var enemy in hints.PotentialTargets)
        {
            var actor = enemy.Actor;
            if (actor.FateID != fateId || actor.IsDead || !actor.IsTargetable)
                continue;
            if (actor.AggroPlayer || actor.TargetID == player.InstanceID)
            {
                havePack = true;
                continue;
            }
            if (InsidePackAOE(player, actor, hints)) // if it's already nearish, assume your gcd will hit it and trying to range pull it is a waste of time
                continue;
            var dist = player.DistanceToHitbox(actor);
            // an unpulled forlorn is always the target
            var actorForlorn = actor.IsForlorn;
            var nearestForlorn = nearest != null && nearest.IsForlorn;
            if (nearest == null || actorForlorn && !nearestForlorn || actorForlorn == nearestForlorn && dist < nearestDist)
            {
                nearestDist = dist;
                nearest = actor;
            }
        }

        if (!havePack || nearest == null)
            return false;

        plan = new(nearest, pullRange, provokeReady, holdGcd);
        return true;
    }

    private static ActionID? GetTagAction(WorldState world, Actor player, ActionAspect preferred)
    {
        ActionID? best = null;
        var bestLevel = int.MaxValue;
        ActionID? bestAspect = null;
        var bestAspectLevel = int.MaxValue;
        foreach (var def in ActionDefinitions.Instance.Definitions)
        {
            if (!IsRangedTag(def, world, player))
                continue;
            if (def.MinLevel < bestLevel)
            {
                bestLevel = def.MinLevel;
                best = def.ID;
            }
            if (preferred != ActionAspect.None && def.Aspect == preferred && def.MinLevel < bestAspectLevel)
            {
                bestAspectLevel = def.MinLevel;
                bestAspect = def.ID;
            }
        }
        return bestAspect ?? best;
    }

    private static bool IsRangedTag(ActionDefinition def, WorldState world, Actor player)
    {
        // must be a long-ranged single target gcd that doesn't require a proc
        if (def.ID.Type != ActionType.Spell || !def.IsGCD || def.IsRoleAction || def.AllowExecute != null)
            return false;
        if (def.Category is not (ActionCategory.Weaponskill or ActionCategory.Spell))
            return false;
        if (def.Range < 15 || !def.IsUnlocked(world, player))
            return false;
        if (def.AllowedTargets != ActionTargets.Hostile)
            return false;
        return Service.LuminaRow<Lumina.Excel.Sheets.Action>(def.ID.ID) is { IsPvP: false, CastType: 1 };
    }

    private static bool IsHostileSingleTarget(ActionDefinition def)
        => def.ID.Type == ActionType.Spell
            && def.AllowedTargets == ActionTargets.Hostile
            && Service.LuminaRow<Lumina.Excel.Sheets.Action>(def.ID.ID) is { CastType: 1 };

    // damage gcds the tag has to beat: a spell on an enemy, or a point-blank aoe. heals and self buffs are left alone.
    private static bool ConflictsWithTag(ActionDefinition def)
    {
        if (def.ID.Type != ActionType.Spell)
            return false;
        if (def.AllowedTargets == ActionTargets.Hostile)
            return true;
        return def.AllowedTargets == ActionTargets.Self && Service.LuminaRow<Lumina.Excel.Sheets.Action>(def.ID.ID) is { CastType: > 1 };
    }

    // the action would be accepted by the queue on this actor: in line of sight, in range, and not blocked by its execute condition
    private static bool CanTag(WorldState world, Actor player, AIHints hints, ActionDefinition def, Actor target)
    {
        if (target.Visibility != Visibility.Visible && def.RequiresLineOfSight)
            return false;
        var range = def.GetAdjustedRange?.Invoke(world, player) ?? def.Range;
        if (range <= 0 || player.DistanceToHitbox(target) > range)
            return false;
        if (def.AllowExecute == null)
            return true;
        var probe = new ActionQueue.Entry(def.ID, target, 0, 0, 0, 0, default, null, false, false);
        return def.AllowExecute(world, player, probe, hints);
    }

    // pickup paths through mobs, hand-in and defend finish the current fight, otherwise pull normally
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
