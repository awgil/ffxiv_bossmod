namespace BossMod.Autorotation;

public sealed class ClassDNCUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    public enum Track { CuringWaltz = SharedTrack.Count, ShieldSamba, Improvisation }
    public enum SambaOption { None, Use87, Use87IfNotActive, Use88, Use88IfNotActive }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DNC.AID.CrimsonLotus);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DNC", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "xan", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DNC), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.CuringWaltz, "CuringWaltz", "C.Waltz", 400, DNC.AID.CuringWaltz);

        res.Define(Track.ShieldSamba).As<SambaOption>("Shield Samba", "S.Samba", 500)
            .AddOption(SambaOption.None, "Do not use automatically")
            .AddOption(SambaOption.Use87, "Use Shield Samba (120s CD), regardless if equivalent ranged buff is already active", 120, 15, ActionTargets.Self, 56, 87)
            .AddOption(SambaOption.Use87IfNotActive, "Use Shield Samba  (120s CD), unless equivalent ranged buff is already active", 90, 15, ActionTargets.Self, 56, 87)
            .AddOption(SambaOption.Use88, "Use Shield Samba  (90s CD), regardless if equivalent ranged buff is already active", 90, 15, ActionTargets.Self, 88)
            .AddOption(SambaOption.Use88IfNotActive, "Use Shield Samba  (90s CD), unless equivalent ranged buff is already active", 90, 15, ActionTargets.Self, 88)
            .AddAssociatedActions(DNC.AID.ShieldSamba);

        DefineSimpleConfig(res, Track.Improvisation, "Improvisation", "Improv", 300, DNC.AID.Improvisation, 15);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.CuringWaltz), DNC.AID.CuringWaltz, Player);
        ExecuteSimple(strategy.Option(Track.Improvisation), DNC.AID.Improvisation, Player);

        // TODO: for 'if-not-active' strategy, add configurable min-time-left
        // TODO: combine 87/88 options
        var samba = strategy.Option(Track.ShieldSamba);
        var wantSamba = samba.As<SambaOption>() switch
        {
            SambaOption.Use87 or SambaOption.Use88 => true,
            SambaOption.Use87IfNotActive or SambaOption.Use88IfNotActive => Player.FindStatus(BRD.SID.Troubadour) == null && Player.FindStatus(MCH.SID.Tactician) == null && Player.FindStatus(DNC.SID.ShieldSamba) == null,
            _ => false
        };
        if (wantSamba)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DNC.AID.ShieldSamba), Player, samba.Priority(), samba.Value.ExpireIn);
    }
}
