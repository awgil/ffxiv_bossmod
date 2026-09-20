using BossMod.BST;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public class CrucibleAI(RotationModuleManager manager, Actor player) : AIBase<CrucibleAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Auto-Snarl", Action = AID.Snarl)]
        public Track<EnabledByDefault> Snarl;
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Crucible AI", "Utilities for Crucible runs", "AI (xan)", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.BST), MaxLevel: 50).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var gauge = World.Client.GetGauge<BeastmasterGauge>();

        var havePet = gauge.ActiveBattlehornIndex > 0;

        if (strategy.Snarl.IsEnabled() && havePet && Hints.PotentialTargets.FirstOrDefault(p => p.PreferShirking) is { } enemy)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Snarl), enemy.Actor, ActionQueue.Priority.Medium, forced: true); // bypass Forbidden priority for parrying enemy
    }
}
