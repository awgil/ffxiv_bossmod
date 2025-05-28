using BossMod.WHM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class WHM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player, PotionType.Mind)
{
    public enum Track { Assize = SharedTrack.Count, Misery }
    public enum AssizeStrategy { None, HitSomething, HitEverything }
    public enum MiseryStrategy { ASAP, BuffedOnly, Delay }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan WHM", "White Mage", "Standard rotation (xan)|Healers", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.WHM, Class.CNJ), 100);

        def.DefineShared().AddAssociatedActions(AID.PresenceOfMind);
        def.Define(Track.Assize).As<AssizeStrategy>("Assize")
            .AddOption(AssizeStrategy.None, "None", "Don't automatically use")
            .AddOption(AssizeStrategy.HitSomething, "HitSomething", "Use if it would hit any priority target")
            .AddOption(AssizeStrategy.HitEverything, "HitEverything", "Use if it would hit all priority targets");
        def.Define(Track.Misery).As<MiseryStrategy>("Afflatus Misery")
            .AddOption(MiseryStrategy.ASAP, "ASAP", "Use on best target at 3 Blood Lilies")
            .AddOption(MiseryStrategy.BuffedOnly, "Buffs", "Use during raid buffs")
            .AddOption(MiseryStrategy.Delay, "Delay", "Do not use");

        return def;
    }

    public uint Lily;
    public uint BloodLily;
    public float NextLily;
    public int SacredSight;

    public float TargetDotLeft;

    public int NumHolyTargets;
    public int NumAssizeTargets;
    public int NumMiseryTargets;

    private Enemy? BestDotTarget;
    private Enemy? BestMiseryTarget;

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<WhiteMageGauge>();

        NextLily = 20f - gauge.LilyTimer * 0.001f;
        Lily = gauge.Lily;
        BloodLily = gauge.BloodLily;

        SacredSight = StatusStacks(SID.SacredSight);

        NumHolyTargets = NumNearbyTargets(strategy, 8);
        NumAssizeTargets = NumNearbyTargets(strategy, 15);
        (BestMiseryTarget, NumMiseryTargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
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

        if (BloodLily == 3 && NumMiseryTargets > 0)
        {
            switch (strategy.Option(Track.Misery).As<MiseryStrategy>())
            {
                case MiseryStrategy.ASAP:
                    PushGCD(AID.AfflatusMisery, BestMiseryTarget);
                    break;
                case MiseryStrategy.BuffedOnly:
                    if (RaidBuffsLeft > GCD)
                        PushGCD(AID.AfflatusMisery, BestMiseryTarget);
                    break;
            }
        }

        if (SacredSight > 0)
            PushGCD(AID.GlareIV, BestMiseryTarget);

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

        switch (strategy.Option(Track.Assize).As<AssizeStrategy>())
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
