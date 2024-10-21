namespace BossMod.Autorotation.xan;

public class DeepDungeonAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Potion }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Deep Dungeon AI", "Utilities for deep dungeon - potion/pomander user", "AI (xan)", "xan", RotationModuleQuality.Basic, new BitMask(~0ul), 100);

        def.AbilityTrack(Track.Potion, "Potion");

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
        if (strategy.Enabled(Track.Potion))
        {
            var potAction = default(ActionID);

            if (PalaceCFCs.Contains(World.CurrentCFCID))
                potAction = ActionDefinitions.IDPotionPalace;

            if (potAction != default && Player.HPMP.CurHP <= Player.HPMP.MaxHP * 0.6f && Player.FindStatus(648) == null && Player.InCombat)
                Hints.ActionsToExecute.Push(potAction, Player, ActionQueue.Priority.Medium);
        }

        foreach (var h in Hints.PriorityTargets)
            if (h.Actor.CastInfo is { Action.ID: 6953 } ci)
                Hints.ForbiddenDirections.Add((Player.AngleTo(h.Actor), 45.Degrees(), World.FutureTime(ci.NPCRemainingTime)));
    }
}
