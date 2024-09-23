namespace BossMod.Autorotation;

public sealed class ClassBLMUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BLM.AID.Meteor);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BLM", "Planner support for utility actions", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.BLM), 100);
        DefineShared(res, IDLimitBreak3);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
    }
}
