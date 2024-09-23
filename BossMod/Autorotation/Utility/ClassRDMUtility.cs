﻿namespace BossMod.Autorotation;

public sealed class ClassRDMUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RDM.AID.VermilionScourge);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RDM", "Planner support for utility actions", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.RDM), 100);
        DefineShared(res, IDLimitBreak3);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
    }
}
