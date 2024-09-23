namespace BossMod.Autorotation;

public sealed class ClassSMNUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SMN.AID.Teraflare);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SMN", "Planner support for utility actions", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SMN), 100);
        DefineShared(res, IDLimitBreak3);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
    }
}
