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

        res.Define(Track.WingedGlide).As<DashStrategy>("Winged Glide", "W.Glide", 20)
            .AddOption(DashStrategy.None, "No use.")
            .AddOption(DashStrategy.GapClose, "Use as gapcloser if outside melee range", 60, 0, ActionTargets.Hostile, 45)
            .AddOption(DashStrategy.GapCloseHold1, "Use as gapcloser if outside melee range; conserves 1 charge for manual usage", 60, 0, ActionTargets.Hostile, 84)
            .AddAssociatedActions(DRG.AID.WingedGlide);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var dash = strategy.Option(Track.WingedGlide);
        var dashStrategy = strategy.Option(Track.WingedGlide).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var distance = Player.DistanceToHitbox(dashTarget);
        var cd = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRG.AID.WingedGlide)!.MainCooldownGroup].Remaining;
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3 and <= 20 && cd <= 60.5f,
            DashStrategy.GapCloseHold1 => distance is > 3 and <= 20 && cd < 0.6f,
            _ => true,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.WingedGlide), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
