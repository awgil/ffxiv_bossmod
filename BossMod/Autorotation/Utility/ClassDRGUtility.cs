namespace BossMod.Autorotation;

public sealed class ClassDRGUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { WingedGlide = SharedTrack.Count }
    public enum DashStrategy { None, GapClose }
    public bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRG.AID.DragonsongDive);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRG", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DRG), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.WingedGlide).As<DashStrategy>("Winged Glide", "Dash", 20)
            .AddOption(DashStrategy.None, "Automatic", "No use.")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 60, 0, ActionTargets.Hostile, 45)
            .AddAssociatedActions(DRG.AID.WingedGlide);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var dash = strategy.Option(Track.WingedGlide);
        var dashStrategy = strategy.Option(Track.WingedGlide).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, primaryTarget))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.WingedGlide), primaryTarget, dash.Priority());
    }
    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.None => false,
        DashStrategy.GapClose => !InMeleeRange(primaryTarget),
        _ => false,
    };
}
