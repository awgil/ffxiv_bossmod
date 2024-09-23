namespace BossMod.Autorotation;

public sealed class ClassBLMUtility(RotationModuleManager manager, Actor player) : RoleMagicalUtility(manager, player)
{
    public enum Track { Manaward = SharedTrack.Count, AetherialManipulation }
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BLM.AID.Meteor);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BLM", "Planner support for utility actions", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.BLM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Manaward, "Manaward", "", 600, BLM.AID.Manaward, 20);
        DefineSimpleConfig(res, Track.AetherialManipulation, "Aetherial Manipulation", "Dash", 500, BLM.AID.AetherialManipulation);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Manaward), BLM.AID.Manaward, Player);
        ExecuteSimple(strategy.Option(Track.AetherialManipulation), BLM.AID.AetherialManipulation, null);
    }
}
