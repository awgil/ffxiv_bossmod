namespace BossMod.Autorotation;

// base class that simplifies implementation of tank utility modules, contains shared track definitions
public abstract class RoleTankUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Rampart, LowBlow, Provoke, Interject, Reprisal, Shirk, ArmsLength, Count }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self, 10, 15, 8)
            .AddAssociatedActions(ClassShared.AID.ShieldWall, ClassShared.AID.Stronghold)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.Rampart, "Rampart", "", 500, ClassShared.AID.Rampart, 20);
        DefineSimpleConfig(def, SharedTrack.LowBlow, "LowBlow", "Stun", -100, ClassShared.AID.LowBlow, 5);
        DefineSimpleConfig(def, SharedTrack.Provoke, "Provoke", "", 200, ClassShared.AID.Provoke);
        DefineSimpleConfig(def, SharedTrack.Interject, "Interject", "Interrupt", -50, ClassShared.AID.Interject);
        DefineSimpleConfig(def, SharedTrack.Reprisal, "Reprisal", "", 250, ClassShared.AID.Reprisal, 10);
        DefineSimpleConfig(def, SharedTrack.Shirk, "Shirk", "", 150, ClassShared.AID.Shirk);
        DefineSimpleConfig(def, SharedTrack.ArmsLength, "ArmsLength", "ArmsL", 300, ClassShared.AID.ArmsLength, 6); // note: secondary effect 15s

        // TODO: stance
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Rampart), ClassShared.AID.Rampart, Player);
        ExecuteSimple(strategy.Option(SharedTrack.LowBlow), ClassShared.AID.LowBlow, null);
        ExecuteSimple(strategy.Option(SharedTrack.Provoke), ClassShared.AID.Provoke, null);
        ExecuteSimple(strategy.Option(SharedTrack.Interject), ClassShared.AID.Interject, null);
        ExecuteSimple(strategy.Option(SharedTrack.Reprisal), ClassShared.AID.Reprisal, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Shirk), ClassShared.AID.Shirk, CoTank());
        ExecuteSimple(strategy.Option(SharedTrack.ArmsLength), ClassShared.AID.ArmsLength, Player);

        var lb = LBLevelToExecute(strategy.Option(SharedTrack.LB).As<LBOption>());
        if (lb > 0)
            Hints.ActionsToExecute.Push(lb == 3 ? lb3 : ActionID.MakeSpell(lb == 2 ? ClassShared.AID.Stronghold : ClassShared.AID.ShieldWall), Player, ActionQueue.Priority.VeryHigh);
    }

    protected Actor? CoTank() => World.Party.WithoutSlot().FirstOrDefault(a => a != Player && a.Role == Role.Tank);
}
