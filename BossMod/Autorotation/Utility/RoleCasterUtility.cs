namespace BossMod.Autorotation;

public abstract class RoleCasterUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Addle, Sleep, LucidDreaming, Swiftcast, Surecast, Count }
    public enum AddleOption { None, Use, UseEx }
    public enum SwiftcastOption { None, Use, UseEx }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 10, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self, 8)
            .AddAssociatedActions(ClassShared.AID.Skyshard, ClassShared.AID.Starstorm)
            .AddAssociatedAction(lb3);

        // TODO: combine standard/ex options
        // TODO: add 'if-not-active' strategy with configurable min-time-left
        def.Define(SharedTrack.Addle).As<AddleOption>("Addle", "", 250)
            .AddOption(AddleOption.None, "Do not use automatically")
            .AddOption(AddleOption.Use, "Use Addle (10s)", 90, 10, ActionTargets.Hostile, 22, 97)
            .AddOption(AddleOption.UseEx, "Use Addle (15s)", 90, 15, ActionTargets.Hostile, 98)
            .AddAssociatedActions(ClassShared.AID.Addle);

        DefineSimpleConfig(def, SharedTrack.Sleep, "Sleep", "", -10, ClassShared.AID.Sleep);
        DefineSimpleConfig(def, SharedTrack.LucidDreaming, "LucidDreaming", "Lucid D.", 30, ClassShared.AID.LucidDreaming, 21);

        // TODO: combine standard/ex options
        def.Define(SharedTrack.Swiftcast).As<SwiftcastOption>("Swiftcast", "Swiftcast", 20)
            .AddOption(SwiftcastOption.None, "Do not use automatically")
            .AddOption(SwiftcastOption.Use, "Use Swiftcast (10s)", 60, 10, ActionTargets.Self, 22, 93)
            .AddOption(SwiftcastOption.UseEx, "Use Swiftcast (15s)", 40, 10, ActionTargets.Self, 94)
            .AddAssociatedActions(ClassShared.AID.Swiftcast);

        DefineSimpleConfig(def, SharedTrack.Surecast, "Surecast", "", 10, ClassShared.AID.Surecast, 6); // note: secondary effect 15s
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Sleep), ClassShared.AID.Sleep, primaryTarget, 2.5f); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(SharedTrack.LucidDreaming), ClassShared.AID.LucidDreaming, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Surecast), ClassShared.AID.Surecast, Player);

        var lb = strategy.Option(SharedTrack.LB);
        var lbLevel = LBLevelToExecute(lb.As<LBOption>());
        if (lbLevel > 0)
        {
            var lbAction = lbLevel == 3 ? lb3 : ActionID.MakeSpell(lbLevel == 2 ? ClassShared.AID.BreathOfTheEarth : ClassShared.AID.HealingWind);
            Hints.ActionsToExecute.Push(lbAction, ResolveTargetOverride(lb.Value), ActionQueue.Priority.VeryHigh, lb.Value.ExpireIn, castTime: ActionDefinitions.Instance[lbAction]!.CastTime);
        }

        var addle = strategy.Option(SharedTrack.Addle);
        if (addle.As<AddleOption>() != AddleOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Addle), ResolveTargetOverride(strategy.Option(SharedTrack.Addle).Value) ?? primaryTarget, addle.Priority(), addle.Value.ExpireIn);

        var swift = strategy.Option(SharedTrack.Swiftcast);
        if (swift.As<SwiftcastOption>() != SwiftcastOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, swift.Priority(), swift.Value.ExpireIn);
    }
}
