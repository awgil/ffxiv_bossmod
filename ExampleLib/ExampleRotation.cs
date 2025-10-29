using BossMod;
using BossMod.Autorotation;

namespace ExampleLib;

public sealed class ExampleRotation(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Sample", "Sample rotation, dynamically loaded!", "Misc", "xan", RotationModuleQuality.Ok, new(~0ul), 1000, 1);
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Hints.ForcedMovement = Player.Rotation.ToDirection().OrthoL().ToVec3();
    }
}
