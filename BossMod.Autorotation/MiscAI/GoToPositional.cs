namespace BossMod.Autorotation.MiscAI;

public sealed class GoToPositional(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        Positional,
        EdgeBuffer
    }

    public enum EdgeBufferStrategy { None, Small, Medium, Large }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Goes to specified positional", "Module for use with other rotation plugins.", "AI", "erdelf", RotationModuleQuality.Basic, new(~0ul), 1000);

        var track = def.Define(Tracks.Positional).As<Positional>("Positional", "Positional");

        foreach (var positional in Enum.GetValues<Positional>())
        {
            track.AddOption(positional);
        }

        def.Define(Tracks.EdgeBuffer).As<EdgeBufferStrategy>("EdgeBuffer", "Edge buffer", 20)
            .AddOption(EdgeBufferStrategy.None, "Stand at positional edges")
            .AddOption(EdgeBufferStrategy.Small, "Prefer staying 0.5y inside from the edges")
            .AddOption(EdgeBufferStrategy.Medium, "Prefer staying 1.5y inside from the edges")
            .AddOption(EdgeBufferStrategy.Large, "Prefer staying 3y inside from the edges");

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

        var cushion = strategy.Option(Tracks.EdgeBuffer).As<EdgeBufferStrategy>() switch
        {
            EdgeBufferStrategy.Small => 0.5f,
            EdgeBufferStrategy.Medium => 1.5f,
            EdgeBufferStrategy.Large => 3.0f,
            _ => 0f
        };

        Hints.RecommendedPositional = (primaryTarget, positional, true, correct);
        Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, positional, cushion: cushion));
    }
}
