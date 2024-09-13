using BossMod.SCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class SCH(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SCH", "Scholar", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SCH, Class.ACN), 100);

        def.DefineShared().AddAssociatedActions(AID.ChainStratagem, AID.Dissipation);

        return def;
    }

    public int Aetherflow;
    public int FairyGauge;
    public float SeraphTimer;
    public bool FairyGone;

    public float ImpactImminent;
    public float TargetDotLeft;
    public int NumAOETargets;
    public int NumRangedAOETargets;

    private Actor? Eos;
    private Actor? BestDotTarget;
    private Actor? BestRangedAOETarget;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<ScholarGauge>();
        Aetherflow = gauge.Aetherflow;
        FairyGauge = gauge.FairyGauge;
        SeraphTimer = gauge.SeraphTimer * 0.001f;
        FairyGone = gauge.DismissedFairy > 0;

        Eos = World.Actors.FirstOrDefault(x => x.Type == ActorType.Pet && x.OwnerID == Player.InstanceID);

        ImpactImminent = StatusLeft(SID.ImpactImminent);

        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, DotDuration, 2);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        NumAOETargets = NumMeleeAOETargets(strategy);

        if (Eos == null && !FairyGone)
            PushGCD(AID.SummonEos, Player);

        OGCD(strategy, primaryTarget);

        if (primaryTarget == null)
            return;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining <= GetCastTime(AID.Broil1))
                PushGCD(AID.Broil1, primaryTarget);

            return;
        }

        if (!CanFitGCD(TargetDotLeft, 1))
            PushGCD(AID.Bio1, BestDotTarget);

        if (RaidBuffsLeft > 0 && !CanFitGCD(RaidBuffsLeft, 1))
            PushGCD(AID.Bio1, BestDotTarget);

        var needAOETargets = Unlocked(AID.Broil1) ? 2 : 1;

        if (NumAOETargets >= needAOETargets)
        {
            if (needAOETargets == 1)
                Hints.RecommendedRangeToTarget = 5f;

            PushGCD(AID.ArtOfWar1, Player);
        }

        PushGCD(AID.Ruin1, primaryTarget);

        // instant cast - fallback for movement
        PushGCD(AID.Ruin2, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        if (strategy.BuffsOk())
        {
            if (Eos != null)
                PushOGCD(AID.Dissipation, Player);

            PushOGCD(AID.ChainStratagem, primaryTarget);
        }

        if (Aetherflow == 0)
            PushOGCD(AID.Aetherflow, Player);

        if (Aetherflow > 0 && CanWeave(AID.Aetherflow, Aetherflow))
            PushOGCD(AID.EnergyDrain, primaryTarget);

        if (ImpactImminent > 0)
            PushOGCD(AID.BanefulImpaction, BestRangedAOETarget);

        if (MP <= 7000)
            PushOGCD(AID.LucidDreaming, Player);
    }

    static readonly SID[] DotStatus = [SID.Bio1, SID.Bio2, SID.Biolysis];

    private float DotDuration(Actor? x)
    {
        if (x == null)
            return float.MaxValue;

        foreach (var stat in DotStatus)
        {
            var dur = StatusDetails(x, (uint)stat, Player.InstanceID).Left;
            if (dur > 0)
                return dur;
        }

        return 0;
    }
}
