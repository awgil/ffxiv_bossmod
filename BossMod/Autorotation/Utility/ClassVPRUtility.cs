namespace BossMod.Autorotation;

public sealed class ClassVPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Slither = SharedTrack.Count }
    public enum DashStrategy { None, GapClose, GapCloseHold1, GapCloseHold2 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(VPR.AID.WorldSwallower);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: VPR", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.VPR), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.Slither).As<DashStrategy>("Slither", "", 20)
            .AddOption(DashStrategy.None, "No use")
            .AddOption(DashStrategy.GapClose, "Use as gapcloser if outside melee range; uses all charges if needed", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddOption(DashStrategy.GapCloseHold1, "Use as gapcloser if outside melee range; holds 1 charge for manual usage", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddOption(DashStrategy.GapCloseHold2, "Use as gapcloser if outside melee range; holds 2 charges for manual usage", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 84)
            .AddAssociatedActions(VPR.AID.Slither);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var dash = strategy.Option(Track.Slither);
        var dashStrategy = strategy.Option(Track.Slither).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var distance = Player.DistanceToHitbox(dashTarget);
        var cd = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(VPR.AID.Slither)!.MainCooldownGroup].Remaining;
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3f and <= 20f,
            DashStrategy.GapCloseHold1 => distance is > 3f and <= 20f && cd <= 30.6f,
            DashStrategy.GapCloseHold2 => distance is > 3f and <= 20f && cd <= 0.6f,
            _ => false,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(VPR.AID.Slither), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
