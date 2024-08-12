namespace BossMod.Autorotation;

public sealed class ClassSAMUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ThirdEye = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SAM.AID.DoomOfTheLiving);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SAM", "Planner support for utility actions", "xan", RotationModuleQuality.WIP, BitMask.Build((int)Class.SAM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ThirdEye, "ThirdEye", "", 600, SAM.AID.ThirdEye, 4);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.ThirdEye), SAM.AID.ThirdEye, Player);
    }
}
