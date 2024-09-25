namespace BossMod.Autorotation;

public sealed class ClassVPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Slither = SharedTrack.Count }
    public enum DashStrategy { None, Force, GapClose } //GapCloser strategy
    public float CDleft => World.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining;
    public bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range

    public const float DashMinCD = 0.8f; //Triple-weaving dash is not a good idea, since it might delay gcd for longer than normal anim lock
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(VPR.AID.WorldSwallower);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: VPR", "Planner support for utility actions", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.VPR), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.Slither).As<DashStrategy>("Slither", "", 20)
            .AddOption(DashStrategy.None, "None", "No use")
            .AddOption(DashStrategy.Force, "Force", "Use ASAP", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 40)
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 40)
            .AddAssociatedActions(VPR.AID.Slither);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var dash = strategy.Option(Track.Slither);
        var dashStrategy = strategy.Option(Track.Slither).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, null))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(VPR.AID.Slither), null, dash.Priority());
    }

    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.None => false,
        DashStrategy.Force => CDleft >= DashMinCD,
        DashStrategy.GapClose => !InMeleeRange(primaryTarget),
        _ => false,
    };
}
