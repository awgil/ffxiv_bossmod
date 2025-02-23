namespace BossMod.Autorotation.MiscAI;

public sealed class AutoFarm(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { General, Fate, Specific, Mount }
    public enum GeneralStrategy { FightBack, AllowPull, Aggressive, Passive }
    public enum PriorityStrategy { None, Prioritize }
    public enum MountedStrategy { None, DisableFightBack, DisableAll }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition res = new("Automatic targeting", "Collection of utilities to automatically target and pull mobs based on different criteria.", "AI", "veyn", RotationModuleQuality.Basic, new(~0ul), 1000, 1, RotationModuleOrder.HighLevel, CanUseWhileRoleplaying: true);

        res.Define(Track.General).As<GeneralStrategy>("General")
            .AddOption(GeneralStrategy.FightBack, "FightBack", "Automatically engage any mobs that are in combat with player, but don't pull new mobs", supportedTargets: ActionTargets.Hostile)
            .AddOption(GeneralStrategy.AllowPull, "AllowPull", "Automatically engage any mobs that are in combat with player; if player is not in combat, pull new mobs", supportedTargets: ActionTargets.Hostile)
            .AddOption(GeneralStrategy.Aggressive, "Aggressive", "Aggressively pull all mobs that are not yet in combat", supportedTargets: ActionTargets.Hostile)
            .AddOption(GeneralStrategy.Passive, "Passive", "Do nothing");

        res.Define(Track.Fate).As<PriorityStrategy>("FATE")
            .AddOption(PriorityStrategy.None, "None", "Do not do anything about fate mobs")
            .AddOption(PriorityStrategy.Prioritize, "Prioritize", "Prioritize mobs in active fate");

        res.Define(Track.Specific).As<PriorityStrategy>("Specific")
            .AddOption(PriorityStrategy.None, "None", "Do not do anything special")
            .AddOption(PriorityStrategy.Prioritize, "Prioritize", "Prioritize specific mobs by targeting criterion");

        res.Define(Track.Mount).As<MountedStrategy>("Mount")
            .AddOption(MountedStrategy.None, "None", "Do not do anything special")
            .AddOption(MountedStrategy.DisableFightBack, "NoFightBack", "Do not engage previously uninteresting mobs if they aggro on player")
            .AddOption(MountedStrategy.DisableAll, "NoAll", "Do not engage anything while mounted");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var generalOpt = strategy.Option(Track.General);
        var generalStrategy = generalOpt.As<GeneralStrategy>();
        if (generalStrategy == GeneralStrategy.Passive)
            return;

        var mountStrategy = strategy.Option(Track.Mount).As<MountedStrategy>();
        var mounted = Player.MountId > 0;
        if (mounted && mountStrategy == MountedStrategy.DisableAll)
            return;

        var allowPulling = generalStrategy switch
        {
            GeneralStrategy.AllowPull => !Player.InCombat,
            GeneralStrategy.Aggressive => true,
            _ => false
        };

        Actor? switchTarget = null; // non-null if we bump any priorities
        (int, float) switchTargetKey = (0, float.MinValue); // priority and negated squared distance
        void prioritize(AIHints.Enemy e, int prio)
        {
            e.Priority = prio;

            var key = (prio, -(e.Actor.Position - Player.Position).LengthSq());
            if (key.CompareTo(switchTargetKey) > 0)
            {
                switchTarget = e.Actor;
                switchTargetKey = key;
            }
        }

        // first deal with pulling new enemies
        if (allowPulling)
        {
            var allowFate = Utils.IsPlayerSyncedToFate(World) && strategy.Option(Track.Fate).As<PriorityStrategy>() == PriorityStrategy.Prioritize;
            if (allowFate)
                foreach (var e in Hints.PotentialTargets.Where(t => t.Actor.FateID == World.Client.ActiveFate.ID))
                    prioritize(e, 1);

            var specific = strategy.Option(Track.Specific);
            if (specific.As<PriorityStrategy>() == PriorityStrategy.Prioritize && Hints.FindEnemy(ResolveTargetOverride(specific.Value)) is var target && target != null)
            {
                prioritize(target, 2);
            }
        }

        // we are done with priority changes
        // if we've updated any priorities, we need to re-sort target array
        if (switchTarget != null)
        {
            Hints.PotentialTargets.SortByReverse(x => x.Priority);
            Hints.HighestPotentialTargetPriority = Math.Max(0, Hints.PotentialTargets[0].Priority);
        }

        // if we did not select an enemy to pull, see if we can target something higher-priority than what we have now
        // if mounted, check if the "fight back" strategy is undesired
        var mountNoFightBack = mounted && mountStrategy == MountedStrategy.DisableFightBack;
        if (switchTarget == null && Player.InCombat && !mountNoFightBack)
        {
            var curTargetPrio = Hints.FindEnemy(primaryTarget)?.Priority ?? int.MinValue;
            switchTarget = ResolveTargetOverride(generalOpt.Value) ?? (curTargetPrio < Hints.HighestPotentialTargetPriority ? Hints.PriorityTargets.MinBy(e => (e.Actor.Position - Player.Position).LengthSq())?.Actor : null);
        }

        // if we have target to switch to, do that
        if (switchTarget != null)
        {
            primaryTarget = Hints.ForcedTarget = switchTarget;
        }
    }
}
