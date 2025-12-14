namespace BossMod.Autorotation.xan;

public class BozjaAI(RotationModuleManager manager, Actor player) : AIBase<BozjaAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Auto-dispel", InternalName = "Auto-dispel", Action = BozjaHolsterID.LostDispel)]
        public Track<EnabledByDefault> Dispel;
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Bozja AI", "Bozja utilities", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 80).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var dispel = BozjaActionID.GetNormal(BozjaHolsterID.LostDispel);

        if (strategy.Dispel.IsEnabled() && FindDutyActionSlot(dispel) >= 0 && Hints.FindEnemy(primaryTarget)?.ShouldBeDispelled == true && primaryTarget?.PendingDispels.Count == 0)
            Hints.ActionsToExecute.Push(dispel, primaryTarget, ActionQueue.Priority.VeryHigh);
    }
}
