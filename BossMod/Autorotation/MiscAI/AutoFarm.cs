namespace BossMod.Autorotation.MiscAI;

public sealed class AutoFarm(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { General, Specific, QuestBattle, DeepDungeon, EpicEcho, Hunt }
    public enum GeneralStrategy { FightBack, AllowPull, Aggressive, Passive }
    public enum TargetingStrategy { None, Fate, All }
    public enum Flag { Disabled, Enabled }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition res = new("Automatic targeting", "Collection of utilities to automatically target and pull mobs based on different criteria.", "AI", "veyn", RotationModuleQuality.Basic, new(~0ul), 1000, 1, RotationModuleOrder.HighLevel, CanUseWhileRoleplaying: true);

        res.Define(Track.General).As<GeneralStrategy>("General")
            .AddOption(GeneralStrategy.FightBack, "FightBack", "Automatically engage eligible mobs that are in combat with player, but don't pull new mobs", supportedTargets: ActionTargets.Hostile)
            .AddOption(GeneralStrategy.AllowPull, "AllowPull", "Automatically engage eligible mobs that are in combat with player; if player is not in combat, pull new mobs", supportedTargets: ActionTargets.Hostile)
            .AddOption(GeneralStrategy.Aggressive, "Aggressive", "Aggressively pull eligible mobs that are not yet in combat", supportedTargets: ActionTargets.Hostile)
            .AddOption(GeneralStrategy.Passive, "Passive", "Do nothing");

        res.Define(Track.Specific).As<TargetingStrategy>("Target")
            .AddOption(TargetingStrategy.None, "None", "Don't prioritize any mobs")
            .AddOption(TargetingStrategy.Fate, "FATE", "Prioritize mobs in active fate")
            .AddOption(TargetingStrategy.All, "All", "Prioritize ALL targetable mobs");

        res.Define(Track.QuestBattle).As<Flag>("QuestBattle", "Automatically attack solo duty bosses")
            .AddOption(Flag.Disabled, "Disabled")
            .AddOption(Flag.Enabled, "Enabled");

        res.Define(Track.DeepDungeon).As<Flag>("DD", "Automatically attack deep dungeon bosses if solo")
            .AddOption(Flag.Disabled, "Disabled")
            .AddOption(Flag.Enabled, "Enabled");

        res.Define(Track.EpicEcho).As<Flag>("EE", "Automatically attack all targets in unsynced duties")
            .AddOption(Flag.Disabled, "Disabled")
            .AddOption(Flag.Enabled, "Enabled");

        res.Define(Track.Hunt).As<Flag>("Hunt", "Automatically attack hunt marks once they have been pulled")
            .AddOption(Flag.Disabled, "Disabled")
            .AddOption(Flag.Enabled, "Enabled");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var generalOpt = strategy.Option(Track.General);
        var generalStrategy = generalOpt.As<GeneralStrategy>();
        if (generalStrategy == GeneralStrategy.Passive)
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

        var (allowFate, allowAll) = strategy.Option(Track.Specific).As<TargetingStrategy>() switch
        {
            TargetingStrategy.All => (true, true),
            TargetingStrategy.Fate => (true, false),
            _ => (false, false)
        };

        if (strategy.Option(Track.QuestBattle).As<Flag>() == Flag.Enabled)
            allowAll |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Quest;

        if (strategy.Option(Track.DeepDungeon).As<Flag>() == Flag.Enabled)
            allowAll |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.DeepDungeon && World.Party.WithoutSlot().Count() == 1;

        if (strategy.Option(Track.EpicEcho).As<Flag>() == Flag.Enabled)
            allowAll |= Player.Statuses.Any(s => s.ID == 2734);

        // first deal with pulling new enemies
        if (allowPulling)
        {
            if (allowFate && Utils.IsPlayerSyncedToFate(World))
                foreach (var e in Hints.PotentialTargets.Where(t => t.Actor.FateID == World.Client.ActiveFate.ID))
                {
                    var isForlorn = e.Actor.NameID is 6737 or 6738;
                    prioritize(e, isForlorn ? 2 : 1);
                }

            if (strategy.Option(Track.Hunt).As<Flag>() == Flag.Enabled && Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Hunt && Bossmods.ActiveModule?.PrimaryActor is Actor p && p.InCombat && p.HPRatio < 0.95f)
                prioritize(Hints.FindEnemy(p)!, 1);

            if (allowAll)
                foreach (var h in Hints.PotentialTargets)
                    if (!h.Actor.IsStrikingDummy && h.Priority == AIHints.Enemy.PriorityUndesirable)
                        prioritize(h, 1);
        }

        // we are done with priority changes
        // if we've updated any priorities, we need to re-sort target array
        if (switchTarget != null)
        {
            Hints.PotentialTargets.SortByReverse(x => x.Priority);
            Hints.HighestPotentialTargetPriority = Math.Max(0, Hints.PotentialTargets[0].Priority);
        }

        // if we did not select an enemy to pull, see if we can target something higher-priority than what we have now
        if (switchTarget == null && Player.InCombat)
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
