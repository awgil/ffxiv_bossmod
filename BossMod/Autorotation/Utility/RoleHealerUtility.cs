namespace BossMod.Autorotation;

public abstract class RoleHealerUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Repose, Esuna, LucidDreaming, Swiftcast, Surecast, Rescue, Count }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self | ActionTargets.Party)
            .AddAssociatedActions(ClassShared.AID.HealingWind, ClassShared.AID.BreathOfTheEarth)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.Repose, "Repose", "", -100, ClassShared.AID.Repose, 30);
        DefineSimpleConfig(def, SharedTrack.Esuna, "Esuna", "", 100, ClassShared.AID.Esuna);
        DefineSimpleConfig(def, SharedTrack.LucidDreaming, "LucidDreaming", "Lucid", 100, ClassShared.AID.LucidDreaming, 21);
        DefineSimpleConfig(def, SharedTrack.Swiftcast, "Swiftcast", "Swift", 100, ClassShared.AID.Swiftcast, 10);
        DefineSimpleConfig(def, SharedTrack.Surecast, "Surecast", "Anti-KB", 100, ClassShared.AID.Surecast, 6); // note: secondary effect 15s
        DefineSimpleConfig(def, SharedTrack.Rescue, "Rescue", "", 100, ClassShared.AID.Rescue);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Repose), ClassShared.AID.Repose, null);
        ExecuteSimple(strategy.Option(SharedTrack.Esuna), ClassShared.AID.Esuna, null);
        ExecuteSimple(strategy.Option(SharedTrack.LucidDreaming), ClassShared.AID.LucidDreaming, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Swiftcast), ClassShared.AID.Swiftcast, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Surecast), ClassShared.AID.Surecast, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Rescue), ClassShared.AID.Rescue, null);

        var lb = strategy.Option(SharedTrack.LB);
        var lbLevel = LBLevelToExecute(lb.As<LBOption>());
        if (lbLevel > 0)
            Hints.ActionsToExecute.Push(lbLevel == 3 ? lb3 : ActionID.MakeSpell(lbLevel == 2 ? ClassShared.AID.BreathOfTheEarth : ClassShared.AID.HealingWind), ResolveTargetOverride(lb.Value), ActionQueue.Priority.VeryHigh, lb.Value.ExpireIn);
    }
}
