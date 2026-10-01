using BossMod.BST;

namespace BossMod.Autorotation.xan;

public class CrucibleAI(RotationModuleManager manager, Actor player) : AIBase<CrucibleAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track(Action = AID.Snarl)]
        public Track<EnabledByDefault> Snarl;

        [Track(Action = AID.Challenge)]
        public Track<EnabledByDefault> Challenge;

        [Track(Action = AID.QuellingWave)]
        public Track<EnabledByDefault> Dispel;

        [Track(Action = AID.SoulCrush)]
        public Track<EnabledByDefault> Interrupt;
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

        if (strategy.Dispel.IsEnabled() && Hints.PotentialTargets.FirstOrDefault(p => p.ShouldBeDispelled) is { } d && gauge.Classification == 5)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.QuellingWave), d.Actor, ActionQueue.Priority.VeryHigh);

        if (strategy.Interrupt.IsEnabled() && Hints.PotentialTargets.FirstOrDefault(p => p.ShouldBeInterrupted) is { } i && gauge.Classification == 7)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.SoulCrush), i.Actor, ActionQueue.Priority.Medium, forced: true);
            Hints.GoalZones.Add(AIHints.GoalSingleTarget(i.Actor.Position, i.Actor.HitboxRadius + 3 + Player.HitboxRadius, 5));
        }
    }
}
