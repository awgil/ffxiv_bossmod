namespace BossMod.Autorotation;

public sealed class ClassSMNUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { RadiantAegis = SharedTrack.Count }
    public enum AegisStrategy { None, Use }
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null; //Checks if Status effect is on self

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SMN.AID.Teraflare);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SMN", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.SMN), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.RadiantAegis).As<AegisStrategy>("Radiant Aegis", "Aegis", 20)
            .AddOption(AegisStrategy.None, "None", "No use")
            .AddOption(AegisStrategy.Use, "Use", "Use Radiant Aegis", 60, 30, ActionTargets.Self, 2);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);

        var radi = strategy.Option(Track.RadiantAegis);
        var hasAegis = HasEffect(SMN.SID.RadiantAegis);
        if (radi.As<AegisStrategy>() != AegisStrategy.None && !hasAegis)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SMN.AID.RadiantAegis), primaryTarget, radi.Priority(), radi.Value.ExpireIn);
    }
}
