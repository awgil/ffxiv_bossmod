namespace BossMod.Autorotation;

public sealed class ClassBLMUtility(RotationModuleManager manager, Actor player) : RoleMagicalUtility(manager, player)
{
    public enum Track { Manaward = SharedTrack.Count, AetherialManipulation }
    public enum DashStrategy { Automatic, Force, GapClose } //GapCloser strategy
    public float CDleft => World.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining;
    public bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range

    public const float DashMinCD = 0.8f; //Triple-weaving dash is not a good idea, since it might delay gcd for longer than normal anim lock

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BLM.AID.Meteor);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BLM", "Planner support for utility actions", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.BLM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Manaward, "Manaward", "", 600, BLM.AID.Manaward, 20);

        res.Define(Track.AetherialManipulation).As<DashStrategy>("Dash", uiPriority: 20)
            .AddOption(DashStrategy.Automatic, "Automatic", "No use")
            .AddOption(DashStrategy.Force, "Force", "Use ASAP")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range")
            .AddAssociatedActions(BLM.AID.AetherialManipulation);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Manaward), BLM.AID.Manaward, Player);

        var dash = strategy.Option(Track.AetherialManipulation);
        var dashStrategy = strategy.Option(Track.AetherialManipulation).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, null))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BLM.AID.AetherialManipulation), null, dash.Priority());
    }
    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.Automatic => false,
        DashStrategy.Force => CDleft >= DashMinCD,
        DashStrategy.GapClose => !InMeleeRange(primaryTarget),
        _ => false,
    };
}
