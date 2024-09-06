namespace BossMod.Autorotation;

public sealed class ClassMNKUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { Mantra = SharedTrack.Count, RiddleOfEarth }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(MNK.AID.FinalHeaven);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: MNK", "Planner support for utility actions", "xan", RotationModuleQuality.WIP, BitMask.Build((int)Class.MNK), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Mantra, "Mantra", "", 100, MNK.AID.Mantra, 15);
        DefineSimpleConfig(res, Track.RiddleOfEarth, "RiddleOfEarth", "RoE", 150, MNK.AID.RiddleOfEarth, 10); // note: secondary effect is 15s hot

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Mantra), MNK.AID.Mantra, Player);
        ExecuteSimple(strategy.Option(Track.RiddleOfEarth), MNK.AID.RiddleOfEarth, Player);
    }
}
