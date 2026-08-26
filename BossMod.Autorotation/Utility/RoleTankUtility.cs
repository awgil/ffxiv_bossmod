namespace BossMod.Autorotation;

// base class that simplifies implementation of tank utility modules, contains shared track definitions
public abstract class RoleTankUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum SharedTrack { Sprint, LB, Rampart, LowBlow, Provoke, Interject, Reprisal, Shirk, ArmsLength, Stance, Count }
    public enum ReprisalOption { None, Use, UseEx }
    public enum StanceOption { None, Apply, Remove }

    protected static void DefineShared(RotationModuleDefinition def, ActionID lb3, ActionID stanceApply, ActionID stanceRemove)
    {
        DefineSimpleConfig(def, SharedTrack.Sprint, "Sprint", "", 100, ClassShared.AID.Sprint, 10);

        DefineLimitBreak(def, SharedTrack.LB, ActionTargets.Self, 10, 15, 8)
            .AddAssociatedActions(ClassShared.AID.ShieldWall, ClassShared.AID.Stronghold)
            .AddAssociatedAction(lb3);

        DefineSimpleConfig(def, SharedTrack.Rampart, "Rampart", "", 500, ClassShared.AID.Rampart, 20);
        DefineSimpleConfig(def, SharedTrack.LowBlow, "LowBlow", "Stun", -100, ClassShared.AID.LowBlow, 5);
        DefineSimpleConfig(def, SharedTrack.Provoke, "Provoke", "", 200, ClassShared.AID.Provoke);
        DefineSimpleConfig(def, SharedTrack.Interject, "Interject", "Interrupt", -50, ClassShared.AID.Interject);

        // TODO: combine standard/ex options
        // TODO: add 'if-not-active' strategy with configurable min-time-left
        def.Define(SharedTrack.Reprisal).As<ReprisalOption>("Reprisal", "", 250)
            .AddOption(ReprisalOption.None, "Do not use automatically")
            .AddOption(ReprisalOption.Use, "Use Reprisal (10s)", 60, 10, ActionTargets.Self, 22, 97)
            .AddOption(ReprisalOption.UseEx, "Use Reprisal (15s)", 60, 15, ActionTargets.Self, 98)
            .AddAssociatedActions(ClassShared.AID.Reprisal);

        DefineSimpleConfig(def, SharedTrack.Shirk, "Shirk", "", 150, ClassShared.AID.Shirk);
        DefineSimpleConfig(def, SharedTrack.ArmsLength, "ArmsLength", "ArmsL", 300, ClassShared.AID.ArmsLength, 6); // note: secondary effect 15s

        def.Define(SharedTrack.Stance).As<StanceOption>("Stance", "", 5)
            .AddOption(StanceOption.None, "Do not touch stance")
            .AddOption(StanceOption.Apply, "Use stance if not already active", 2)
            .AddOption(StanceOption.Remove, "Remove stance if active", 1)
            .AddAssociatedAction(stanceApply)
            .AddAssociatedAction(stanceRemove);
    }

    protected void ExecuteShared(StrategyValues strategy, ActionID lb3, ActionID stanceApply, ActionID stanceRemove, uint stanceStatus, Actor? primaryTarget)
    {
        ExecuteSimple(strategy.Option(SharedTrack.Sprint), ClassShared.AID.Sprint, Player);
        ExecuteSimple(strategy.Option(SharedTrack.Rampart), ClassShared.AID.Rampart, Player);
        ExecuteSimple(strategy.Option(SharedTrack.LowBlow), ClassShared.AID.LowBlow, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Provoke), ClassShared.AID.Provoke, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Interject), ClassShared.AID.Interject, primaryTarget);
        ExecuteSimple(strategy.Option(SharedTrack.Shirk), ClassShared.AID.Shirk, CoTank());
        ExecuteSimple(strategy.Option(SharedTrack.ArmsLength), ClassShared.AID.ArmsLength, Player);

        var stance = strategy.Option(SharedTrack.Stance);
        var stanceOption = stance.As<StanceOption>();
        if (stanceOption != StanceOption.None)
        {
            var haveStance = Player.FindStatus(stanceStatus) != null;
            var wantStance = stanceOption == StanceOption.Apply;
            if (haveStance != wantStance)
                Hints.ActionsToExecute.Push(wantStance ? stanceApply : stanceRemove, Player, stance.Priority(), stance.Value.ExpireIn);
        }

        var reprisal = strategy.Option(SharedTrack.Reprisal);
        if (reprisal.As<ReprisalOption>() != ReprisalOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Reprisal), Player, reprisal.Priority(), reprisal.Value.ExpireIn);

        var lb = strategy.Option(SharedTrack.LB);
        var lbLevel = LBLevelToExecute(lb.As<LBOption>());
        if (lbLevel > 0)
            Hints.ActionsToExecute.Push(lbLevel == 3 ? lb3 : ActionID.MakeSpell(lbLevel == 2 ? ClassShared.AID.Stronghold : ClassShared.AID.ShieldWall), Player, lb.Priority(), lb.Value.ExpireIn);
    }

    protected Actor? CoTank() => World.Party.WithoutSlot().FirstOrDefault(a => a != Player && a.Role == Role.Tank);
}
