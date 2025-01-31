using BossMod.Autorotation.xan;

namespace BossMod.Autorotation.MiscAI;
public sealed class AutoEngage(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { QuestBattle, DeepDungeon, EpicEcho }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Misc AI: Auto-engage", "Automatically attack all nearby targets in certain circumstances", "Misc", "xan", RotationModuleQuality.Basic, new(~0ul), 1000);

        def.AbilityTrack(Track.QuestBattle, "Automatically attack solo duty bosses");
        def.AbilityTrack(Track.DeepDungeon, "Automatically attack deep dungeon bosses when solo");
        def.AbilityTrack(Track.EpicEcho, "Automatically attack all targets if the Epic Echo status is present (i.e. when unsynced)");

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Player.InCombat || primaryTarget != null || World.Client.CountdownRemaining > 0)
            return;

        var enabled = false;

        if (strategy.Enabled(Track.QuestBattle))
            enabled |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Quest;

        if (strategy.Enabled(Track.DeepDungeon))
            enabled |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.DeepDungeon && World.Party.WithoutSlot().Count() == 1;

        if (strategy.Enabled(Track.EpicEcho))
            enabled |= Player.Statuses.Any(s => s.ID == 2734);

        if (enabled)
            primaryTarget = Hints.PotentialTargets.Where(t => t.Priority == AIHints.Enemy.PriorityUndesirable).MinBy(p => Player.DistanceToHitbox(p.Actor))?.Actor;
    }
}
