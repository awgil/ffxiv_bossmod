using BossMod.Autorotation.xan;

namespace BossMod.Autorotation.MiscAI;

public sealed class AutoTarget(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { General, Retarget, QuestBattle, DeepDungeon, EpicEcho, Hunt, FATE, Everything, CollectFATE }
    public enum GeneralStrategy { Aggressive, Passive }
    public enum RetargetStrategy { NoTarget, Hostiles, Always, Never }
    public enum Flag { Disabled, Enabled }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition res = new("Automatic targeting", "Collection of utilities to automatically target and pull mobs based on different criteria.", "AI", "veyn", RotationModuleQuality.Basic, new(~0ul), 1000, 1, RotationModuleOrder.HighLevel, CanUseWhileRoleplaying: true);

        res.Define(Track.General).As<GeneralStrategy>("General")
            .AddOption(GeneralStrategy.Aggressive, "Automatically prioritize targets", supportedTargets: ActionTargets.Hostile)
            .AddOption(GeneralStrategy.Passive, "Do nothing");

        res.Define(Track.Retarget).As<RetargetStrategy>("Retarget")
            .AddOption(RetargetStrategy.NoTarget, "Only switch target if player has no target")
            .AddOption(RetargetStrategy.Hostiles, "Only switch target if player is not targeting an ally")
            .AddOption(RetargetStrategy.Always, "Always switch target to the highest priority enemy")
            .AddOption(RetargetStrategy.Never, "Never switch target; only apply priority changes to enemies");

        res.Define(Track.QuestBattle).As<Flag>("QuestBattle", "Prioritize bosses in quest battles", renderer: typeof(DefaultOffRenderer))
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.DeepDungeon).As<Flag>("DD", "Prioritize deep dungeon bosses (solo only)", renderer: typeof(DefaultOffRenderer))
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.EpicEcho).As<Flag>("EE", "Prioritize all targets in unsynced duties", renderer: typeof(DefaultOffRenderer))
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.Hunt).As<Flag>("Hunt", "Prioritize hunt marks once they have been pulled", renderer: typeof(DefaultOffRenderer))
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.FATE).As<Flag>("FATE", "Prioritize mobs in the current FATE", renderer: typeof(DefaultOffRenderer))
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.Everything).As<Flag>("Everything", "Prioritize EVERYTHING", renderer: typeof(DefaultOffRenderer))
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.CollectFATE).As<Flag>("CollectFATE", "Ignore passive mobs in hand-in FATEs", renderer: typeof(DefaultOffRenderer))
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var generalOpt = strategy.Option(Track.General);
        var generalStrategy = generalOpt.As<GeneralStrategy>();
        if (generalStrategy == GeneralStrategy.Passive)
            return;

        Actor? bestTarget = null; // non-null if we bump any priorities
        (int, float) bestTargetKey = (0, float.MinValue); // priority and negated squared distance
        void prioritize(AIHints.Enemy e, int prio)
        {
            e.Priority = prio;

            var key = (e.Priority, -(e.Actor.Position - Player.Position).LengthSq());
            if (key.CompareTo(bestTargetKey) > 0)
            {
                bestTarget = e.Actor;
                bestTargetKey = key;
            }
        }

        var allowAll = strategy.Option(Track.Everything).As<Flag>() == Flag.Enabled;

        if (strategy.Option(Track.QuestBattle).As<Flag>() == Flag.Enabled)
            allowAll |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Quest;

        if (strategy.Option(Track.DeepDungeon).As<Flag>() == Flag.Enabled)
            allowAll |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.DeepDungeon && World.Party.WithoutSlot().Count() == 1;

        if (strategy.Option(Track.EpicEcho).As<Flag>() == Flag.Enabled)
            allowAll |= Player.Statuses.Any(s => s.ID == 2734);

        ulong huntTarget = 0;

        if (strategy.Option(Track.Hunt).As<Flag>() == Flag.Enabled && Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Hunt && Bossmods.ActiveModule?.PrimaryActor is { InCombat: true, HPRatio: < 0.95f, InstanceID: var i })
            huntTarget = i;

        var targetFates = strategy.Option(Track.FATE).As<Flag>() == Flag.Enabled && Utils.IsPlayerSyncedToFate(World);
        var skipFate = false;

        var handinCount = 0;

        if (targetFates)
        {
            var handinId = Utils.GetFateItem(World.Client.ActiveFate.ID);
            if (handinId > 0)
            {
                handinCount = World.Client.ActiveFate.HandInCount + (int)World.Client.GetItemQuantity(handinId);
                skipFate = strategy.Option(Track.CollectFATE).As<Flag>() == Flag.Enabled;
            }
        }

        // first deal with pulling new enemies
        foreach (var target in Hints.PotentialTargets)
        {
            if (target.Actor.InstanceID == huntTarget)
            {
                prioritize(target, 0);
                continue;
            }

            if (allowAll && !target.Actor.IsStrikingDummy && target.Priority == AIHints.Enemy.PriorityUndesirable)
            {
                prioritize(target, 0);
                continue;
            }

            if (targetFates && target.Actor.FateID == World.Client.ActiveFate.ID)
            {
                if (target.Actor.NameID is 6737 or 6738)
                {
                    prioritize(target, 1);
                    continue;
                }
                if (handinCount < 10 && !skipFate)
                {
                    prioritize(target, 0);
                    continue;
                }
            }

            // add all other targets to potential targets list (e.g. if modules modify out-of-combat mob priority)
            if (target.Priority >= 0)
                prioritize(target, target.Priority);
        }

        // prioritizer yielded no results meaning there are no targets to pick, do nothing
        if (bestTarget == null)
            return;

        Hints.PotentialTargets.SortByReverse(x => x.Priority);
        Hints.HighestPotentialTargetPriority = Math.Max(0, Hints.PotentialTargets[0].Priority);

        var retargetStrategy = strategy.Option(Track.Retarget).As<RetargetStrategy>();
        if (retargetStrategy == RetargetStrategy.Never)
            return;

        var currentTarget = World.Actors.Find(Player.TargetID);

        var changeTarget = retargetStrategy switch
        {
            RetargetStrategy.Hostiles => currentTarget == null || !currentTarget.IsAlly,
            RetargetStrategy.NoTarget => currentTarget == null,
            _ => true
        };

        // if we have target to switch to, do that
        if (changeTarget)
            primaryTarget = Hints.ForcedTarget = bestTarget;
    }
}
