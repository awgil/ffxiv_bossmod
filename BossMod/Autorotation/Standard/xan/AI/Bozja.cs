namespace BossMod.Autorotation.xan;

public class BozjaAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Dispel }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Bozja AI", "Bozja utilities", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 80);

        def.AbilityTrack(Track.Dispel, "Auto-dispel").AddAssociatedAction(BozjaActionID.GetNormal(BozjaHolsterID.LostDispel));

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Enabled(Track.Dispel) && HaveAction(BozjaHolsterID.LostDispel) && Hints.FindEnemy(primaryTarget)?.ShouldBeDispelled == true && primaryTarget?.PendingDispels.Count == 0)
            Hints.ActionsToExecute.Push(BozjaActionID.GetNormal(BozjaHolsterID.LostDispel), primaryTarget, ActionQueue.Priority.VeryHigh);
    }

    private bool HaveAction(BozjaHolsterID id) => World.Client.DutyActions.Any(d => d.Action == BozjaActionID.GetNormal(id));
}
