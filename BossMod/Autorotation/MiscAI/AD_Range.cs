namespace BossMod.Autorotation.MiscAI;

public sealed class AD_Range(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition() => new("Misc AI: AD Range", "Module for use by AutoDuty preset.", "erdelf", RotationModuleQuality.Basic, new(~0ul), 1000);

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (primaryTarget != null)
        {
            var radius = 10f;

            var adConfig = Service.PluginInterface.GetIpcSubscriber<string, string>("AutoDuty.GetConfig");

            if (adConfig is { HasFunction: true })
            {
                var func = adConfig.InvokeFunc("MaxDistanceToTargetFloat");
                if (float.TryParse(func, out var newRadius) && newRadius > 0)
                {
                    radius = newRadius;
                }
            }
            Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget.Position, radius+primaryTarget.HitboxRadius, 0.5f));
        }
    }
}
