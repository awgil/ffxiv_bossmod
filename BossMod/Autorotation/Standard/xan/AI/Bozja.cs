namespace BossMod.Autorotation.xan;

public class BozjaAI(RotationModuleManager manager, Actor player) : AIBase<BozjaAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        // TODO: support BozjaHolsterID in strategy track
        [Track("Auto-dispel", InternalName = "Auto-dispel", Action = 20704 /* BozjaHolsterID.LostDispel */)]
        public Track<EnabledByDefault> Dispel;
    }

    public static RotationModuleDefinition Definition(ActionDefinitions defs)
    {
        return new RotationModuleDefinition("Bozja AI", "Bozja utilities", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 80).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var dispel = Actions.BozjaActions.GetNormal(BozjaHolsterID.LostDispel);

        if (strategy.Dispel.IsEnabled() && FindDutyActionSlot(dispel) >= 0 && Hints.FindEnemy(primaryTarget)?.ShouldBeDispelled == true && primaryTarget?.PendingDispels.Count == 0)
            Hints.ActionsToExecute.Push(dispel, primaryTarget, ActionQueue.Priority.VeryHigh);
    }
}
