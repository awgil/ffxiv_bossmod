using System.Globalization;

namespace BossMod.Autorotation.MiscAI;

public sealed class StayAwayFromTarget(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        Distance
    }

    public enum DistanceDefinition
    {
    }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Stay away from target", "Preset module to maintain distance from target and avoid dodging to the center pixel.", "AI", "Erisen", RotationModuleQuality.Basic, new(~0ul), 100);

        var configRef = def.Define(Tracks.Distance).As<DistanceDefinition>("Distance");

        for (float f = 0; f <= 5f; f = MathF.Round(f + 0.1f, 1))
        {
            configRef.AddOption((DistanceDefinition)(f * 10f), f.ToString(CultureInfo.InvariantCulture));
        }

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (primaryTarget != null)
        {
            var realDistance = strategy.Option(Tracks.Distance).Value.Option / 10f + primaryTarget.HitboxRadius;
            var sqDist = 2 * realDistance * realDistance; // 2x to make .5 prio
            var tagetPosition = primaryTarget.Position;
            // No hard cut because this can conflict with stay close to target.
            // by assigning a smaller scaling amount to the middle it will still attempt to get near the edge.
            Hints.GoalZones.Add(p => MathF.Min(.5f, MathF.Round((tagetPosition - p).LengthSq() / sqDist, 1)));
        }
    }
}
