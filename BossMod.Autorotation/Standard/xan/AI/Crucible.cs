using BossMod.BST;

namespace BossMod.Autorotation.xan;

public class CrucibleAI(RotationModuleManager manager, Actor player) : AIBase<CrucibleAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Auto-Snarl", Action = AID.Snarl)]
        public Track<EnabledByDefault> Snarl;

        [Track("Auto-Challenge", Action = AID.Challenge)]
        public Track<EnabledByDefault> Challenge;
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Crucible AI", "Utilities for Crucible runs", "AI (xan)", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.BST), MaxLevel: 50).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var gauge = World.Client.GetGauge<BeastmasterGauge>();

        var havePet = gauge.ActiveBattlehornIndex > 0;

        if (strategy.Snarl.IsEnabled() && havePet && Hints.PotentialTargets.FirstOrDefault(p => p.PreferShirking) is { } e1)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Snarl), e1.Actor, ActionQueue.Priority.Medium, forced: true);

        if (strategy.Challenge.IsEnabled() && Hints.PotentialTargets.FirstOrDefault(p => p.ShouldBeTanked) is { } e2)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Challenge), e2.Actor, ActionQueue.Priority.Medium, forced: true);
    }
}
