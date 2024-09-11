namespace BossMod.Autorotation;

public sealed class ClassMCHUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    // Add all MCH tracks to end of list, starting with Tactician
    // SharedTrack.Count here is the "end" of the track list, so we set the first track we want as the "end"
    public enum Track { Tactician = SharedTrack.Count, Dismantle }

    // Add Machinist LB3
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MCH.AID.SatelliteBeam);

    public enum TactOption { None, Use87, Use88 }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MCH", "Planner support for utility actions", "Aimsucks", RotationModuleQuality.Ok, BitMask.Build((int)Class.MCH), 100);
        DefineShared(res, IDLimitBreak3);

        // Add track for Tactician: 15s long, 15% player damage received reduction
        res.Define(Track.Tactician).As<TactOption>("Tactician", "Tact", 400)
            .AddOption(TactOption.None, "None", "Do not use automatically")
            .AddOption(TactOption.Use87, "Use", "Use Tactician", 120, 15, ActionTargets.Self, 56, 87)
            .AddOption(TactOption.Use88, "Use88", "Use Tactician", 90, 15, ActionTargets.Self, 88)
            .AddAssociatedActions(MCH.AID.Tactician);

        // Add track for Dismantle: 10s long, 10% target damage reduction
        DefineSimpleConfig(res, Track.Dismantle, "Dismantle", "Dism", 500, MCH.AID.Dismantle, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Dismantle), MCH.AID.Dismantle, Player);

        var tact = strategy.Option(Track.Tactician);
        if (tact.As<TactOption>() != TactOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(MCH.AID.Tactician), Player, tact.Priority(), tact.Value.ExpireIn);
    }
}
