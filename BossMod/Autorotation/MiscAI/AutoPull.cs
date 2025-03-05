using BossMod.Autorotation.xan;

namespace BossMod.Autorotation.MiscAI;

public sealed class AutoPull(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { QuestBattle, DeepDungeon, EpicEcho, Hunt }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Auto-pull", "Automatically attack passive mobs in certain circumstances", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, Order: RotationModuleOrder.HighLevel, CanUseWhileRoleplaying: true);

        def.AbilityTrack(Track.QuestBattle, "Automatically attack solo duty bosses");
        def.AbilityTrack(Track.DeepDungeon, "Automatically attack deep dungeon bosses when solo");
        def.AbilityTrack(Track.EpicEcho, "Automatically attack all targets if the Epic Echo status is present (i.e. when unsynced)");
        def.AbilityTrack(Track.Hunt, "Automatically attack hunt marks once they are below 95% HP");

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Player.InCombat || primaryTarget != null || World.Client.CountdownRemaining != null)
            return;

        var enabled = false;

        if (strategy.Enabled(Track.QuestBattle))
            enabled |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Quest;

        if (strategy.Enabled(Track.DeepDungeon))
            enabled |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.DeepDungeon && World.Party.WithoutSlot().Count() == 1;

        if (strategy.Enabled(Track.EpicEcho))
            enabled |= Player.Statuses.Any(s => s.ID == 2734);

        // TODO set HP threshold lower, or remove entirely? want to avoid getting one guy'd by an early puller
        if (strategy.Enabled(Track.Hunt) && Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Hunt && Bossmods.ActiveModule?.PrimaryActor is Actor p && p.InCombat && p.HPRatio < 0.95f)
        {
            Hints.SetPriority(p, 0);
            primaryTarget = Hints.ForcedTarget = p;
            return;
        }

        if (enabled)
        {
            var bestEnemy = Hints.PotentialTargets.Where(t => t.Priority == AIHints.Enemy.PriorityUndesirable).MinBy(p => Player.DistanceToHitbox(p.Actor));
            if (bestEnemy != null)
            {
                bestEnemy.Priority = 0;
                primaryTarget = Hints.ForcedTarget = bestEnemy.Actor;
            }
        }
    }
}

