namespace BossMod.Autorotation.MiscAI;
public sealed class GoToPositional(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        Positional
    }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Goes to specified positional", "Module for use with other rotation plugins.", "AI", "erdelf", RotationModuleQuality.Basic, new(~0ul), 1000);

        var track = def.Define(Tracks.Positional).As<Positional>("Positional", "Positional");

        foreach (var positional in Enum.GetValues<Positional>())
        {
            track.AddOption(positional, positional.ToString());
        }
        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (!Player.InCombat
            || Player.FindStatus(ClassShared.AID.TrueNorth) != null
            || primaryTarget == null
            || primaryTarget is { Omnidirectional: true }
            || primaryTarget is { TargetID: var t, CastInfo: null, IsStrikingDummy: false } && t == Player.InstanceID)
        {
            return;
        }

        var positional = strategy.Option(Tracks.Positional).As<Positional>();
        if (positional == Positional.Any)
            return;

        //mainly from Basexan.UpdatePositionals
        var correct = positional switch
        {
            Positional.Flank => MathF.Abs(primaryTarget.Rotation.ToDirection().Dot((Player.Position - primaryTarget.Position).Normalized())) < 0.7071067f,
            Positional.Rear => primaryTarget.Rotation.ToDirection().Dot((Player.Position - primaryTarget.Position).Normalized()) < -0.7071068f,
            _ => true
        };

        Hints.RecommendedPositional = (primaryTarget, positional, true, correct);
        Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, positional));
    }
}
