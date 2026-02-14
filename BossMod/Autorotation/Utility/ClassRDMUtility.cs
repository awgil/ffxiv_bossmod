namespace BossMod.Autorotation;

public sealed class ClassRDMUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { MagickBarrier = SharedTrack.Count }
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RDM.AID.VermilionScourge);

    public static RotationModuleDefinition Definition(ActionDefinitions defs)
    {
        var res = new RotationModuleDefinition("Utility: RDM", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.RDM), 100);
        DefineShared(defs, res, IDLimitBreak3);

        DefineSimpleConfig(defs, res, Track.MagickBarrier, "Magick Barrier", "Barrier", 600, RDM.AID.MagickBarrier, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.MagickBarrier), RDM.AID.MagickBarrier, Player);
    }
}
