namespace BossMod.Autorotation.xan;

public class DeepDungeonAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Potion }

    public enum PotionStrategy
    {
        Disabled,
        Always,
        Boss,
        BossOrHigh
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Deep Dungeon AI", "Utilities for deep dungeon - potion/pomander user", "AI (xan)", "xan", RotationModuleQuality.Basic, new BitMask(~0ul), 100);

        def.Define(Track.Potion).As<PotionStrategy>("Potion")
            .AddOption(PotionStrategy.Disabled, "Do not use")
            .AddOption(PotionStrategy.Always, "Use below 80% HP if status is not present")
            .AddOption(PotionStrategy.Boss, "Use during boss fights")
            .AddOption(PotionStrategy.BossOrHigh, "Use during boss fights, or above floor 100");

        return def;
    }

    private static readonly uint[] PalaceCFCs = [
        174, 175, 176, 177, 178,
        204, 205, 206, 207, 208,
        209, 210, 211, 212, 213,
        214, 215, 216, 217, 218
    ];

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ActionID regenAction = default;
        ActionID potAction = default;

        if (PalaceCFCs.Contains(World.CurrentCFCID))
        {
            regenAction = ActionDefinitions.IDSustainingPotion;
            potAction = ActionDefinitions.IDMaxPotion;
        }

        if (regenAction != default && ShouldPotion(strategy))
            Hints.ActionsToExecute.Push(regenAction, Player, ActionQueue.Priority.Medium);

        if (potAction != default && PredictedHPRatio(Player) <= 0.3f)
            Hints.ActionsToExecute.Push(potAction, Player, ActionQueue.Priority.Medium);

        foreach (var h in Hints.PriorityTargets)
            if (h.Actor.CastInfo is { Action.ID: 6953 } ci)
                Hints.ForbiddenDirections.Add((Player.AngleTo(h.Actor), 45.Degrees(), World.FutureTime(ci.NPCRemainingTime)));
    }

    private bool ShouldPotion(StrategyValues strategy)
    {
        var use = PredictedHPRatio(Player) < 0.8f && Player.FindStatus(648) == null && Player.InCombat;
        return use && strategy.Option(Track.Potion).As<PotionStrategy>() switch
        {
            PotionStrategy.Always => true,
            PotionStrategy.Boss => World.Client.DeepDungeonState.Floor % 10 == 0,
            PotionStrategy.BossOrHigh => World.Client.DeepDungeonState.Floor is var floor && (floor % 10 == 0 || floor > 140),
            _ => false
        };
    }
}
