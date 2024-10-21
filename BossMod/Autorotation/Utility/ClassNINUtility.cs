namespace BossMod.Autorotation;

public sealed class ClassNINUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ShadeShift = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(NIN.AID.Chimatsuri);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: NIN", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.NIN), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ShadeShift, "Shade", "", 400, NIN.AID.ShadeShift, 20);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ShadeShift), NIN.AID.ShadeShift, Player);
    }
}
