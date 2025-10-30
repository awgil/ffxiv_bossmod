namespace BossMod.Autorotation;

public sealed class ClassMNKUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Mantra = SharedTrack.Count, RiddleOfEarth, Thunderclap }
    public enum DashStrategy { None, GapClose, GapCloseHold1, GapCloseHold2 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MNK.AID.FinalHeaven);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MNK", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "xan", RotationModuleQuality.Excellent, BitMask.Build((int)Class.MNK), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Mantra, "Mantra", "", 100, MNK.AID.Mantra, 15);
        DefineSimpleConfig(res, Track.RiddleOfEarth, "RiddleOfEarth", "RoE", 150, MNK.AID.RiddleOfEarth, 10); // note: secondary effect is 15s hot

        res.Define(Track.Thunderclap).As<DashStrategy>("Thunderclap", "Dash", 20)
            .AddOption(DashStrategy.None, "No use")
            .AddOption(DashStrategy.GapClose, "Use as gapcloser if outside melee range; uses all charges if needed", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddOption(DashStrategy.GapCloseHold1, "Use as gapcloser if outside melee range; holds 1 charge for manual usage", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddOption(DashStrategy.GapCloseHold2, "Use as gapcloser if outside melee range; holds 2 charges for manual usage", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 84)
            .AddAssociatedActions(MNK.AID.Thunderclap);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Mantra), MNK.AID.Mantra, Player);
        ExecuteSimple(strategy.Option(Track.RiddleOfEarth), MNK.AID.RiddleOfEarth, Player);

        var dash = strategy.Option(Track.Thunderclap);
        var dashStrategy = strategy.Option(Track.Thunderclap).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var distance = Player.DistanceToHitbox(dashTarget);
        var cd = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(MNK.AID.Thunderclap)!.MainCooldownGroup].Remaining;
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3f and <= 20f,
            DashStrategy.GapCloseHold1 => distance is > 3f and <= 20f && cd <= 30.6f,
            DashStrategy.GapCloseHold2 => distance is > 3f and <= 20f && cd <= 0.6f,
            _ => false,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(MNK.AID.Thunderclap), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
