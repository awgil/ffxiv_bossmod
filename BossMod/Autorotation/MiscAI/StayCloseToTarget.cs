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
        RotationModuleDefinition def = new("Misc AI: Stay within range of target", "Module for use by AutoDuty preset.", "AI", "veyn", RotationModuleQuality.Basic, new(~0ul), 1000, CanUseWhileRoleplaying: true, PvP: PvPCompatibility.Any);

        var configRef = def.Define(Tracks.Range).As<RangeDefinition>("range", "Range to target", renderer: typeof(FakeFloatRenderer));

        configRef.AddOption(RangeDefinition.OnHitbox, "Stay on edge of hitbox (+/- 1 unit)");

        for (var f = 1.1f; f <= 30f; f = MathF.Round(f + 0.1f, 1))
        {
            configRef.AddOption((RangeDefinition)(f * 10f - 10f), internalNameOverride: f.ToString(CultureInfo.InvariantCulture));
        }

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (primaryTarget != null)
        {
            var position = primaryTarget.Position;
            var radius = primaryTarget.HitboxRadius;
            var range = strategy.Option(Tracks.Range);
            if (range.As<RangeDefinition>() == RangeDefinition.OnHitbox)
            {
                Hints.GoalZones.Add(p => p.InDonut(position, radius - 1f, radius + 1f) ? 0.5f : default);
            }
            else
            {
                Hints.GoalZones.Add(AIHints.GoalSingleTarget(position, (range.Value.Option + 10f) / 10f + radius, 0.5f));
            }
        }
    }
}
