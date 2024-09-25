namespace BossMod.Autorotation;

public sealed class ClassSMNUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { RadiantAegis = SharedTrack.Count }
    public enum AegisStrategy { None, Use }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SMN.AID.Teraflare);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SMN", "Planner support for utility actions", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.SMN), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.RadiantAegis, "Radiant Aegis", "Aegis", 600, SMN.AID.RadiantAegis, 30);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);

        var radi = strategy.Option(Track.RadiantAegis);
        var radiant = radi.As<AegisStrategy>() switch
        {
            AegisStrategy.Use => SMN.AID.RadiantAegis,
            _ => default
        };
        if (radiant != default && SelfStatusLeft(SMN.SID.RadiantAegis) <= 3f)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(radiant), Player, radi.Priority(), radi.Value.ExpireIn);
    }
}
