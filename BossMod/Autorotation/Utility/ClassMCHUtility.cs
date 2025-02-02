namespace BossMod.Autorotation;

public sealed class ClassMCHUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    // Add all MCH tracks to end of list, starting with Tactician
    // SharedTrack.Count here is the "end" of the track list, so we set the first track we want as the "end"
    public enum Track { Tactician = SharedTrack.Count, Dismantle }

    // Add Machinist LB3
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MCH.AID.SatelliteBeam);

    public enum TactOption { None, Use87, Use87IfNotActive, Use88, Use88IfNotActive }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MCH", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Aimsucks", RotationModuleQuality.Excellent, BitMask.Build((int)Class.MCH), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.Tactician).As<TactOption>("Tactician", "Tact", 400)
            .AddOption(TactOption.None, "None", "Do not use automatically")
            .AddOption(TactOption.Use87, "Use", "Use Tactician (120s CD), regardless if equivalent ranged buff is already active", 120, 15, ActionTargets.Self, 56, 87)
            .AddOption(TactOption.Use87IfNotActive, "UseIfNotActive", "Use Tactician (120s CD), unless equivalent ranged buff is already active", 90, 15, ActionTargets.Self, 56, 87)
            .AddOption(TactOption.Use88, "Use88", "Use Tactician (90s CD), regardless if equivalent ranged buff is already active", 90, 15, ActionTargets.Self, 88)
            .AddOption(TactOption.Use88IfNotActive, "Use88IfNotActive", "Use Tactician (90s CD), unless equivalent ranged buff is already active", 90, 15, ActionTargets.Self, 88)
            .AddAssociatedActions(MCH.AID.Tactician);

        DefineSimpleConfig(res, Track.Dismantle, "Dismantle", "Dism", 500, MCH.AID.Dismantle, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Dismantle), MCH.AID.Dismantle, ResolveTargetOverride(strategy.Option(Track.Dismantle).Value) ?? primaryTarget);

        var tact = strategy.Option(Track.Tactician);
        var hasDefensive = Player.FindStatus(BRD.SID.Troubadour) != null || Player.FindStatus(MCH.SID.Tactician) != null || Player.FindStatus(DNC.SID.ShieldSamba) != null;
        if (tact.As<TactOption>() != TactOption.None)
        {
            if (tact.As<TactOption>() is TactOption.Use87 or TactOption.Use88)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(MCH.AID.Tactician), Player, tact.Priority(), tact.Value.ExpireIn);
            if (tact.As<TactOption>() is TactOption.Use87IfNotActive or TactOption.Use88IfNotActive && !hasDefensive)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(MCH.AID.Tactician), Player, tact.Priority(), tact.Value.ExpireIn);
        }
    }
}
