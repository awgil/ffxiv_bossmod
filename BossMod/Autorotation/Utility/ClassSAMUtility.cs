namespace BossMod.Autorotation;

public sealed class ClassSAMUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ThirdEye = SharedTrack.Count }
    public enum EyeOption { None, ThirdEye, Tengentsu }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SAM.AID.DoomOfTheLiving);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SAM", "Planner support for utility actions", "xan, Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SAM), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.ThirdEye).As<EyeOption>("ThirdEye", "Eye", 600)
            .AddOption(EyeOption.None, "None", "Do not use automatically")
            .AddOption(EyeOption.ThirdEye, "Use", "Use Third Eye", 15, 4, ActionTargets.Self, 6, 81)
            .AddOption(EyeOption.Tengentsu, "UseEx", "Use Tengentsu", 15, 4, ActionTargets.Self, 82)
            .AddAssociatedActions(SAM.AID.ThirdEye, SAM.AID.Tengentsu);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
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
