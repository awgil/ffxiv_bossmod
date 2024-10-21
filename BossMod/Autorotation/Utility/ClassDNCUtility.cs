namespace BossMod.Autorotation;

public sealed class ClassDNCUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    public enum Track { CuringWaltz = SharedTrack.Count, ShieldSamba, Improvisation }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DNC.AID.CrimsonLotus);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DNC", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "xan", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DNC), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.CuringWaltz, "CuringWaltz", "Waltz", 400, DNC.AID.CuringWaltz);
        DefineSimpleConfig(res, Track.ShieldSamba, "ShieldSamba", "Samba", 500, DNC.AID.ShieldSamba, 15);
        DefineSimpleConfig(res, Track.Improvisation, "Improvisation", "Improv", 300, DNC.AID.Improvisation, 15);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.CuringWaltz), DNC.AID.CuringWaltz, Player);
        ExecuteSimple(strategy.Option(Track.ShieldSamba), DNC.AID.ShieldSamba, Player);
        ExecuteSimple(strategy.Option(Track.Improvisation), DNC.AID.Improvisation, Player);
    }
}
