namespace BossMod.Autorotation.MiscAI;

public sealed class AutoFarm(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { General, Fate, Specific }
    public enum GeneralStrategy { AllowPull, FightBack, Aggressive, Passive }
    public enum PriorityStrategy { None, Prioritize }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition res = new("Misc AI: Automatic farming", "Make sure this is ordered before standard rotation modules!", "Misc", "veyn", RotationModuleQuality.Basic, new(~0ul), 1000);

        res.Define(Track.General).As<GeneralStrategy>("General")
            .AddOption(GeneralStrategy.AllowPull, "AllowPull", "Automatically engage any mobs that are in combat with player; if player is not in combat, pull new mobs")
            .AddOption(GeneralStrategy.FightBack, "FightBack", "Automatically engage any mobs that are in combat with player, but don't pull new mobs")
            .AddOption(GeneralStrategy.Aggressive, "Aggressive", "Aggressively pull all mobs that are not yet in combat")
            .AddOption(GeneralStrategy.Passive, "Passive", "Do nothing");

        res.Define(Track.Fate).As<PriorityStrategy>("FATE")
            .AddOption(PriorityStrategy.None, "None", "Do not do anything about fate mobs")
            .AddOption(PriorityStrategy.Prioritize, "Prioritize", "Prioritize mobs in active fate");

        res.Define(Track.Specific).As<PriorityStrategy>("Specific")
            .AddOption(PriorityStrategy.None, "None", "Do not do anything special")
            .AddOption(PriorityStrategy.Prioritize, "Prioritize", "Prioritize specific mobs by targeting criterion");

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var generalStrategy = strategy.Option(Track.General).As<GeneralStrategy>();
        if (generalStrategy == GeneralStrategy.Passive)
            return;

        var allowPulling = generalStrategy switch
        {
            GeneralStrategy.AllowPull => !Player.InCombat,
            GeneralStrategy.Aggressive => true,
            _ => false
        };

        Actor? closestTargetToSwitchTo = null; // non-null if we bump any priorities
        float closestTargetDistSq = float.MaxValue;
        void prioritize(AIHints.Enemy e, int prio)
        {
            e.Priority = prio;

            var distSq = (e.Actor.Position - Player.Position).LengthSq();
            if (distSq < closestTargetDistSq)
            {
                closestTargetToSwitchTo = e.Actor;
                closestTargetDistSq = distSq;
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
                        prioritize(e, 1);
                    }
                }
            }

            var specific = strategy.Option(Track.Specific);
            if (specific.As<PriorityStrategy>() == PriorityStrategy.Prioritize && Hints.FindEnemy(ResolveTargetOverride(specific.Value)) is var target && target != null)
            {
                prioritize(target, 2);
            }
        }

        // if we're not going to pull anyone, but we are already in combat and not targeting aggroed enemy, find one to target
        if (closestTargetToSwitchTo == null && Player.InCombat && !(primaryTarget?.AggroPlayer ?? false))
        {
            foreach (var e in Hints.PotentialTargets)
            {
                if (e.Actor.AggroPlayer)
                {
                    prioritize(e, 3);
                }
            }
        }

        // if we have target to attack, do that
        if (closestTargetToSwitchTo != null)
        {
            // if we've updated any priorities, we need to re-sort target array
            Hints.PotentialTargets.SortByReverse(x => x.Priority);
            Hints.HighestPotentialTargetPriority = Math.Max(0, Hints.PotentialTargets[0].Priority);
            primaryTarget = Hints.ForcedTarget = closestTargetToSwitchTo;
        }
    }
}
