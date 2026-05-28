using BossMod.WHM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class WHM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID, WHM.Strategy>(manager, player, PotionType.Mind)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track("Presence of Mind", Action = AID.PresenceOfMind)]
        public Track<OffensiveStrategy> Buffs;
        public Track<AssizeStrategy> Assize;

        [Track(InternalName = "Afflatus Misery")]
        public Track<MiseryStrategy> Misery;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum AssizeStrategy
    {
        [Option("Use if it would hit any target")]
        HitSomething,
        [Option("Use if it would hit all targets")]
        HitEverything,
        [Option("Don't use")]
        None
    }
    public enum MiseryStrategy
    {
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        ASAP,
        [Option("Only use during raid buffs", Targets = ActionTargets.Hostile)]
        BuffedOnly,
        [Option("Don't use")]
        Delay
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan WHM", "White Mage", "Standard rotation (xan)|Healers", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.WHM, Class.CNJ), 100)
            .WithStrategies<Strategy>();
    }

    public uint Lily;
    public uint BloodLily;
    public float NextLily;
    public int SacredSight;

    public float TargetDotLeft;

    public int NumHolyTargets;
    public int NumAssizeTargets;
    public int NumRangedAOETargets;

    private Enemy? BestDotTarget;
    private Enemy? BestRangedAOETarget;

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<WhiteMageGauge>();

        NextLily = 20f - gauge.LilyTimer * 0.001f;
        Lily = gauge.Lily;
        BloodLily = gauge.BloodLily;

        SacredSight = StatusStacks(SID.SacredSight);

        NumHolyTargets = NumNearbyTargets(strategy, 8);
        NumAssizeTargets = NumNearbyTargets(strategy, 15);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, DotLeft, 2);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.Stone))
                PushGCD(AID.Stone, primaryTarget);

            return;
        }

        GoalZoneCombined(strategy, 25, Hints.GoalAOECircle(8), AID.Holy, 3);

        if (!CanFitGCD(TargetDotLeft, 1))
            PushGCD(AID.Aero, BestDotTarget);

        if (BloodLily == 3 && NumRangedAOETargets > 0)
        {
            switch (strategy.Misery.Value)
            {
                case MiseryStrategy.ASAP:
                    PushGCD(AID.AfflatusMisery, ResolveTargetOverride(strategy.Misery) ?? BestRangedAOETarget);
                    break;
                case MiseryStrategy.BuffedOnly:
                    if (RaidBuffsLeft > GCD)
                        PushGCD(AID.AfflatusMisery, ResolveTargetOverride(strategy.Misery) ?? BestRangedAOETarget);
                    break;
            }
        }

        if (SacredSight > 0)
            PushGCD(AID.GlareIV, BestRangedAOETarget);

        if (NumHolyTargets > 2)
            PushGCD(AID.Holy, Player);

        // TODO make a track for this
        if (Unlocked(AID.AfflatusMisery) && Lily == 3)
            PushGCD(AID.AfflatusSolace, Player);

        PushGCD(AID.Stone, primaryTarget);

        if (!Player.InCombat)
            return;

        if (RaidBuffsLeft >= 15 || RaidBuffsIn > 9000)
            PushOGCD(AID.PresenceOfMind, Player);

        switch (strategy.Assize.Value)
        {
            case AssizeStrategy.HitEverything:
                if (NumAssizeTargets == Hints.PriorityTargets.Count())
                    PushOGCD(AID.Assize, Player);
                break;
            case AssizeStrategy.HitSomething:
                if (NumAssizeTargets > 0)
                    PushOGCD(AID.Assize, Player);
                break;
        }

        if (MP <= Player.HPMP.MaxMP * 0.7f)
            PushOGCD(AID.LucidDreaming, Player);
    }

    private float DotLeft(Actor? actor) => actor == null ? float.MaxValue : Utils.MaxAll(
        StatusDetails(actor, SID.Aero, Player.InstanceID).Left,
        StatusDetails(actor, SID.AeroII, Player.InstanceID).Left,
        StatusDetails(actor, SID.Dia, Player.InstanceID).Left
    );
}
