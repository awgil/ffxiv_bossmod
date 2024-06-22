namespace BossMod.Autorotation;

// base class that simplifies implementation of physical melee dps utility modules, contains shared track definitions
public abstract class RoleMeleeUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, SecondWind, LegSweep, Bloodbath, Feint, ArmsLength, Count }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Hostile)
            .AddAssociatedActions(ClassShared.AID.Braver, ClassShared.AID.Bladedance)
            .AddAssociatedAction(lb3);

        // note: true north is a special case, even though it's a role action, it has custom handling by rotational modules
        DefineSimpleConfig(def, SharedTrack.SecondWind, "SecondWind", "", 20, ClassShared.AID.SecondWind);
        DefineSimpleConfig(def, SharedTrack.LegSweep, "LegSweep", "Stun", -150, ClassShared.AID.LegSweep, 3);
        DefineSimpleConfig(def, SharedTrack.Bloodbath, "Bloodbath", "", -50, ClassShared.AID.Bloodbath, 20);
        DefineSimpleConfig(def, SharedTrack.Feint, "Feint", "", 500, ClassShared.AID.Feint, 10);
        DefineSimpleConfig(def, SharedTrack.ArmsLength, "ArmsLength", "ArmsL", 300, ClassShared.AID.ArmsLength, 6); // note: secondary effect 15s
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.SecondWind), ClassShared.AID.SecondWind, Player);
        ExecuteSimple(strategy.Option(SharedTrack.LegSweep), ClassShared.AID.LegSweep, null);
        ExecuteSimple(strategy.Option(SharedTrack.Bloodbath), ClassShared.AID.Bloodbath, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Feint), ClassShared.AID.Feint, null);
        ExecuteSimple(strategy.Option(SharedTrack.ArmsLength), ClassShared.AID.ArmsLength, Player);

        var lb = LBLevelToExecute(strategy.Option(SharedTrack.LB).As<LBOption>());
        if (lb > 0)
            Hints.ActionsToExecute.Push(lb == 3 ? lb3 : ActionID.MakeSpell(lb == 2 ? ClassShared.AID.Bladedance : ClassShared.AID.Braver), null, ActionQueue.Priority.VeryHigh);
    }
}
