using System.Globalization;

namespace BossMod.Autorotation.MiscAI;

public sealed class StayCloseToTarget(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        Range
    }

    public enum RangeDefinition
    {
        OnHitbox
    }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Stay within range of target", "Module for use by AutoDuty preset.", "AI", "veyn", RotationModuleQuality.Basic, new(~0ul), 1000);

        var configRef = def.Define(Tracks.Range).As<RangeDefinition>("range", "Range to target", renderer: typeof(FakeFloatRenderer));

        configRef.AddOption(RangeDefinition.OnHitbox, "Stay on edge of hitbox (+/- 1 unit)");

        for (float f = 1.1f; f <= 30f; f = MathF.Round(f + 0.1f, 1))
        {
            configRef.AddOption((RangeDefinition)(f * 10f - 10f), internalNameOverride: f.ToString(CultureInfo.InvariantCulture));
        }

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (primaryTarget != null)
        {
            var position = primaryTarget.Position;
            var radius = primaryTarget.HitboxRadius;
            var range = strategy.Option(Tracks.Range);
            if (range.As<RangeDefinition>() == RangeDefinition.OnHitbox)
                Hints.GoalZones.Add(p => p.InDonut(position, radius - 1, radius + 1) ? 0.5f : 0);
            else
                Hints.GoalZones.Add(Hints.GoalSingleTarget(position, (range.Value.Option + 10f) / 10f + primaryTarget.HitboxRadius, 0.5f));
        }
    }
}
