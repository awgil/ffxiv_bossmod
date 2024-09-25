namespace BossMod.Autorotation;

public sealed class ClassBLMUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { Manaward = SharedTrack.Count, AetherialManipulation }
    public enum DashStrategy { None, Force }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BLM.AID.Meteor);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BLM", "Planner support for utility actions", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.BLM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Manaward, "Manaward", "", 600, BLM.AID.Manaward, 20);

        res.Define(Track.AetherialManipulation).As<DashStrategy>("Dash", "", 20)
            .AddOption(DashStrategy.None, "None", "No use.")
            .AddOption(DashStrategy.Force, "Force", "Use ASAP", 10, 0, ActionTargets.Party, 50)
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
        DashStrategy.None => false,
        DashStrategy.Force => true,
        _ => false,
    };
}
