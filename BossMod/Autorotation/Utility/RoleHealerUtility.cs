namespace BossMod.Autorotation;

public abstract class RoleHealerUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Repose, Esuna, LucidDreaming, Swiftcast, Surecast, Rescue, Count }
    public enum SwiftcastOption { None, Use, UseEx }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 10, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self, 8)
            .AddAssociatedActions(ClassShared.AID.HealingWind, ClassShared.AID.BreathOfTheEarth)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.Repose, "Repose", "", -100, ClassShared.AID.Repose, 30);
        DefineSimpleConfig(def, SharedTrack.Esuna, "Esuna", "", 40, ClassShared.AID.Esuna);
        DefineSimpleConfig(def, SharedTrack.LucidDreaming, "LucidDreaming", "Lucid", 30, ClassShared.AID.LucidDreaming, 21);

        // TODO: combine standard/ex options
        def.Define(SharedTrack.Swiftcast).As<SwiftcastOption>("Swiftcast", "Swift", 20)
            .AddOption(SwiftcastOption.None, "Do not use automatically")
            .AddOption(SwiftcastOption.Use, "Use Swiftcast (10s)", 60, 10, ActionTargets.Self, 22, 93)
            .AddOption(SwiftcastOption.UseEx, "Use Swiftcast (15s)", 40, 10, ActionTargets.Self, 94)
            .AddAssociatedActions(ClassShared.AID.Swiftcast);

        DefineSimpleConfig(def, SharedTrack.Surecast, "Surecast", "", 10, ClassShared.AID.Surecast, 6); // note: secondary effect 15s
        DefineSimpleConfig(def, SharedTrack.Rescue, "Rescue", "", 50, ClassShared.AID.Rescue);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Repose), ClassShared.AID.Repose, primaryTarget, 2.5f); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(SharedTrack.Esuna), ClassShared.AID.Esuna, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.LucidDreaming), ClassShared.AID.LucidDreaming, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Surecast), ClassShared.AID.Surecast, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Rescue), ClassShared.AID.Rescue, primaryTarget);

        var lb = strategy.Option(SharedTrack.LB);
        var lbLevel = LBLevelToExecute(lb.As<LBOption>());
        if (lbLevel > 0)
        {
            var lbAction = lbLevel == 3 ? lb3 : ActionID.MakeSpell(lbLevel == 2 ? ClassShared.AID.BreathOfTheEarth : ClassShared.AID.HealingWind);
            Hints.ActionsToExecute.Push(lbAction, ResolveTargetOverride(lb.Value), ActionQueue.Priority.VeryHigh, lb.Value.ExpireIn, castTime: ActionDefinitions.Instance[lbAction]!.CastTime);
        }

        var swift = strategy.Option(SharedTrack.Swiftcast);
        if (swift.As<SwiftcastOption>() != SwiftcastOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, swift.Priority(), swift.Value.ExpireIn);
    }
}
