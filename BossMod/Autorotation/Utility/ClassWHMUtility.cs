namespace BossMod.Autorotation;

public sealed class ClassWHMUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    //public enum Track { x, x, x, x, x, x, x, Count }
    //public enum xOption { None, x, x }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(WHM.AID.PulseOfLife);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: WHM", "Planner support for utility actions", "Akechi", RotationModuleQuality.WIP, BitMask.Build((int)Class.WHM), 100);
        DefineShared(res, IDLimitBreak3);

        // DefineSimpleConfig(res, Track.x, "x", "", 100, WHM.AID.x, x);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        //ExecuteSimple(strategy.Option(Track.x), WHM.AID.x, Player);
    }
}
