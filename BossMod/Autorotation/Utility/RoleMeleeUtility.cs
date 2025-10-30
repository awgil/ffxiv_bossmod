namespace BossMod.Autorotation;

// base class that simplifies implementation of physical melee dps utility modules, contains shared track definitions
public abstract class RoleMeleeUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, SecondWind, LegSweep, Bloodbath, Feint, ArmsLength, Count }
    public enum FeintOption { None, Use, UseEx }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Hostile)
            .AddAssociatedActions(ClassShared.AID.Braver, ClassShared.AID.Bladedance)
            .AddAssociatedAction(lb3);

        // note: true north is a special case, even though it's a role action, it has custom handling by rotational modules
        DefineSimpleConfig(def, SharedTrack.SecondWind, "SecondWind", "S.Wind", 20, ClassShared.AID.SecondWind);
        DefineSimpleConfig(def, SharedTrack.LegSweep, "LegSweep", "Stun", -150, ClassShared.AID.LegSweep, 3);
        DefineSimpleConfig(def, SharedTrack.Bloodbath, "Bloodbath", "", -50, ClassShared.AID.Bloodbath, 20);

        // TODO: combine standard/ex options
        // TODO: add 'if-not-active' strategy with configurable min-time-left
        def.Define(SharedTrack.Feint).As<FeintOption>("Feint", "", 250)
            .AddOption(FeintOption.None, "Do not use automatically")
            .AddOption(FeintOption.Use, "Use Feint (10s)", 90, 10, ActionTargets.Hostile, 22, 97)
            .AddOption(FeintOption.UseEx, "Use Feint (15s)", 90, 15, ActionTargets.Hostile, 98)
            .AddAssociatedActions(ClassShared.AID.Feint);

        DefineSimpleConfig(def, SharedTrack.ArmsLength, "ArmsLength", "ArmsL", 300, ClassShared.AID.ArmsLength, 6); // note: secondary effect 15s
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.SecondWind), ClassShared.AID.SecondWind, Player);
        ExecuteSimple(strategy.Option(SharedTrack.LegSweep), ClassShared.AID.LegSweep, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Bloodbath), ClassShared.AID.Bloodbath, Player);
        ExecuteSimple(strategy.Option(SharedTrack.ArmsLength), ClassShared.AID.ArmsLength, Player);

        var lb = strategy.Option(SharedTrack.LB);
        var lbLevel = LBLevelToExecute(lb.As<LBOption>());
        if (lbLevel > 0)
        {
            var lbAction = lbLevel == 3 ? lb3 : ActionID.MakeSpell(lbLevel == 2 ? ClassShared.AID.Bladedance : ClassShared.AID.Braver);
            Hints.ActionsToExecute.Push(lbAction, ResolveTargetOverride(lb.Value) ?? primaryTarget, ActionQueue.Priority.VeryHigh, lb.Value.ExpireIn, castTime: ActionDefinitions.Instance[lbAction]!.CastTime);
        }

        var feint = strategy.Option(SharedTrack.Feint);
        if (feint.As<FeintOption>() != FeintOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Feint), ResolveTargetOverride(feint.Value) ?? primaryTarget, feint.Priority(), feint.Value.ExpireIn);

    }
}
