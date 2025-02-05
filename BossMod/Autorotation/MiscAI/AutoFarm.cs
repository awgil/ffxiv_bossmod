namespace BossMod.Autorotation.MiscAI;

public sealed class AutoFarm(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { General, Fate, Specific }
    public enum GeneralStrategy { FightBack, AllowPull, Aggressive, Passive }
    public enum PriorityStrategy { None, Prioritize }

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

        return res;
    }

    // these mobs give fate XP and reward boost and should be prioritized over regular mobs - otherwise it's easy to accidentally complete the fate before killing them and lose the bonus
    public const uint NameForlornMaiden = 6737;
    public const uint NameTheForlorn = 6738;

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

        // first deal with pulling new enemies
        if (allowPulling)
        {
            if (World.Client.ActiveFate.ID != 0 && Player.Level <= Service.LuminaRow<Lumina.Excel.Sheets.Fate>(World.Client.ActiveFate.ID)?.ClassJobLevelMax && strategy.Option(Track.Fate).As<PriorityStrategy>() == PriorityStrategy.Prioritize)
            {
                foreach (var e in Hints.PotentialTargets)
                {
                    if (e.Actor.FateID == World.Client.ActiveFate.ID && e.Priority == AIHints.Enemy.PriorityUndesirable)
                    {
                        var forlorn = e.Actor.NameID is NameForlornMaiden or NameTheForlorn;
                        prioritize(e, forlorn ? 2 : 1);
                    }
                }
            }

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
