namespace BossMod.Autorotation;

public sealed class ClassDRGUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { WingedGlide = SharedTrack.Count }
    public enum DashStrategy { None, GapClose, GapCloseHold1 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRG.AID.DragonsongDive);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRG", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DRG), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.WingedGlide).As<DashStrategy>("Winged Glide", "Dash", 20)
            .AddOption(DashStrategy.None, "Automatic", "No use.")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use Winged Glide as gapcloser if outside melee range", 60, 0, ActionTargets.Hostile, 45)
            .AddOption(DashStrategy.GapCloseHold1, "GapCloseHold1", "Use Winged Glide as gapcloser if outside melee range; conserves 1 charge for manual usage", 60, 0, ActionTargets.Hostile, 84)
            .AddAssociatedActions(DRG.AID.WingedGlide);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var dash = strategy.Option(Track.WingedGlide);
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget;
        var dashStrategy = strategy.Option(Track.WingedGlide).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, primaryTarget))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.WingedGlide), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.None => false,
        DashStrategy.GapClose => Player.DistanceToHitbox(primaryTarget) is > 3 and <= 20,
        DashStrategy.GapCloseHold1 => Player.DistanceToHitbox(primaryTarget) is > 3 and <= 20 && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRG.AID.WingedGlide)!.MainCooldownGroup].Remaining <= 59.9f,
        _ => false,
    };
}
