namespace BossMod.Autorotation;

public sealed class ClassRPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ArcaneCrest = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RPR.AID.TheEnd);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RPR", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.RPR), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ArcaneCrest, "Crest", "", 600, RPR.AID.ArcaneCrest, 5);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ArcaneCrest), RPR.AID.ArcaneCrest, Player);
    }
}
