namespace BossMod.Autorotation;

// base class that simplifies implementation of physical ranged dps utility modules, contains shared track definitions
public abstract class RoleRangedUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, LegGraze, SecondWind, FootGraze, HeadGraze, ArmsLength, Count }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Hostile)
            .AddAssociatedActions(ClassShared.AID.BigShot, ClassShared.AID.Desperado)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.LegGraze, "LegGraze", "Slow", -100, ClassShared.AID.LegGraze, 10);
        DefineSimpleConfig(def, SharedTrack.SecondWind, "SecondWind", "", 20, ClassShared.AID.SecondWind);
        DefineSimpleConfig(def, SharedTrack.FootGraze, "FootGraze", "Bind", -150, ClassShared.AID.FootGraze, 10);
        DefineSimpleConfig(def, SharedTrack.HeadGraze, "HeadGraze", "Interrupt", -50, ClassShared.AID.HeadGraze);
        DefineSimpleConfig(def, SharedTrack.ArmsLength, "ArmsLength", "ArmsL", 300, ClassShared.AID.ArmsLength, 6); // note: secondary effect 15s
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.LegGraze), ClassShared.AID.LegGraze, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.SecondWind), ClassShared.AID.SecondWind, Player);
        ExecuteSimple(strategy.Option(SharedTrack.FootGraze), ClassShared.AID.FootGraze, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.HeadGraze), ClassShared.AID.HeadGraze, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.ArmsLength), ClassShared.AID.ArmsLength, Player);

        var lb = strategy.Option(SharedTrack.LB);
        var lbLevel = LBLevelToExecute(lb.As<LBOption>());
        if (lbLevel > 0)
            Hints.ActionsToExecute.Push(lbLevel == 3 ? lb3 : ActionID.MakeSpell(lbLevel == 2 ? ClassShared.AID.Desperado : ClassShared.AID.BigShot), ResolveTargetOverride(lb.Value), ActionQueue.Priority.VeryHigh, lb.Value.ExpireIn);
    }
}
