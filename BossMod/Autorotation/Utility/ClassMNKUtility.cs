namespace BossMod.Autorotation;

public sealed class ClassMNKUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Mantra = SharedTrack.Count, RiddleOfEarth, Thunderclap }
    public enum DashStrategy { None, Force, GapClose }
    public float CDleft => World.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining;
    public bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range

    public const float DashMinCD = 0.8f; //Triple-weaving dash is not a good idea, since it might delay gcd for longer than normal anim lock

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MNK.AID.FinalHeaven);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MNK", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "xan", RotationModuleQuality.Excellent, BitMask.Build((int)Class.MNK), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Mantra, "Mantra", "", 100, MNK.AID.Mantra, 15);
        DefineSimpleConfig(res, Track.RiddleOfEarth, "RiddleOfEarth", "RoE", 150, MNK.AID.RiddleOfEarth, 10); // note: secondary effect is 15s hot

        res.Define(Track.Thunderclap).As<DashStrategy>("Thunderclap", "Dash", 20)
            .AddOption(DashStrategy.None, "None", "No use")
            .AddOption(DashStrategy.Force, "Force", "Use ASAP", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 35)
            .AddAssociatedActions(MNK.AID.Thunderclap);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Mantra), MNK.AID.Mantra, Player);
        ExecuteSimple(strategy.Option(Track.RiddleOfEarth), MNK.AID.RiddleOfEarth, Player);

        var dash = strategy.Option(Track.Thunderclap);
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var dashStrategy = strategy.Option(Track.Thunderclap).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, dashTarget))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(MNK.AID.Thunderclap), dashTarget, dash.Priority());
    }

    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.None => false,
        DashStrategy.Force => CDleft >= DashMinCD,
        DashStrategy.GapClose => !InMeleeRange(primaryTarget),
        _ => false,
    };
}
