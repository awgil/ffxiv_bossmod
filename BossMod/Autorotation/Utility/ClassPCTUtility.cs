namespace BossMod.Autorotation;

public sealed class ClassPCTUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { TemperaCoat = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(PCT.AID.ChromaticFantasy);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PCT", "Planner support for utility actions", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.PCT), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.TemperaCoat, "Tempora Coat", "T.Coat", 600, PCT.AID.TemperaCoat, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.TemperaCoat), PCT.AID.TemperaCoat, Player);
    }
}
