namespace BossMod.Autorotation.MiscAI;

public sealed class StayCloseToTarget(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition() => new("Misc AI: Stay within 10m of target", "Module for use by AutoDuty preset.", "AI Behaviours", "veyn", RotationModuleQuality.Basic, new(~0ul), 1000);

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (primaryTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget.Position, 10, 0.5f));
    }
}
