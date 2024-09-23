namespace BossMod.Autorotation;

public sealed class ClassDRGUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { WingedGlide = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRG.AID.DragonsongDive);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRG", "Planner support for utility actions", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DRG), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.WingedGlide, "Winged Glide", "Glide", 500, DRG.AID.WingedGlide);
        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.WingedGlide), DRG.AID.WingedGlide, primaryTarget);
    }
}
