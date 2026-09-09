using BossMod.BST;

namespace BossMod.Autorotation.xan;

public sealed class BST(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, BST.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan BST", "Beastmaster", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.BST), 100).WithStrategies<Strategy>();
    }

    public override void Exec(in Strategy strategy, AIHints.Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        if (ComboLastMove == AID.AxebladeBite)
            PushGCD(AID.Shieldsplitter, primaryTarget);

        if (ComboLastMove == AID.SmashAxe)
            PushGCD(AID.AxebladeBite, primaryTarget);

        PushGCD(AID.SmashAxe, primaryTarget);

        if (PlayerTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, Player, World.Actors, 3));
    }
}
