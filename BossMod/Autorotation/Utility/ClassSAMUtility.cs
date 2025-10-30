namespace BossMod.Autorotation;

public sealed class ClassSAMUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ThirdEye = SharedTrack.Count }
    public enum EyeOption { None, ThirdEye, Tengentsu }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SAM.AID.DoomOfTheLiving);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SAM", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "xan, Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.SAM), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.ThirdEye).As<EyeOption>("ThirdEye", "Eye", 600)
            .AddOption(EyeOption.None, "Do not use automatically")
            .AddOption(EyeOption.ThirdEye, "Use Third Eye", 15, 4, ActionTargets.Self, 6, 81)
            .AddOption(EyeOption.Tengentsu, "Use Tengentsu", 15, 4, ActionTargets.Self, 82)
            .AddAssociatedActions(SAM.AID.ThirdEye, SAM.AID.Tengentsu);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var eye = strategy.Option(Track.ThirdEye);
        var eyeAction = eye.As<EyeOption>() switch
        {
            EyeOption.ThirdEye => SAM.AID.ThirdEye,
            EyeOption.Tengentsu => SAM.AID.Tengentsu,
            _ => default
        };
        if (eyeAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(eyeAction), Player, eye.Priority(), eye.Value.ExpireIn);
    }
}
