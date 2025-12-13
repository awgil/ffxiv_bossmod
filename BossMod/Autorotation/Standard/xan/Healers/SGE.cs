using BossMod.SGE;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class SGE(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID, SGE.Strategy>(manager, player, PotionType.Mind)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track("Phlegma", Actions = [AID.Phlegma, AID.PhlegmaII, AID.PhlegmaIII])]
        public Track<OffensiveStrategy> Buffs;
        public Track<KardiaStrategy> Kardia;
        public Track<DruocholeStrategy> Druochole;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum KardiaStrategy
    {
        [Option("Automatically choose Kardia target")]
        Auto,
        [Option("Don't automatically use Kardia")]
        Manual,
        [Option("Place Kardia on specified ally", Targets = ActionTargets.Party, Context = StrategyContext.Plan)]
        Specific
    }
    public enum DruocholeStrategy
    {
        [Option("Use on a low-HP or otherwise random ally if it's about to overcap")]
        Auto,
        [Option("Don't automatically use")]
        Manual
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan SGE", "Sage", "Standard rotation (xan)|Healers", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SGE), 100).WithStrategies<Strategy>();
    }

    public int Gall;
    public float NextGall;
    public int Sting;
    public bool Eukrasia;
    public float ZoeLeft;

    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumPhlegmaTargets;
    public int NumPneumaTargets;

    public float TargetDotLeft;

    private Enemy? BestPhlegmaTarget; // 6y/5y
    private Enemy? BestRangedAOETarget; // 25y/5y toxikon, psyche
    private Enemy? BestPneumaTarget; // 25y/4y rect

    private Enemy? BestDotTarget;

    protected override float GetCastTime(AID aid) => Eukrasia ? 0 : base.GetCastTime(aid);

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 25);

        var gauge = World.Client.GetGauge<SageGauge>();

        Gall = gauge.Addersgall;
        Sting = gauge.Addersting;
        NextGall = MathF.Max(0, 20f - gauge.AddersgallTimer / 1000f);
        Eukrasia = gauge.EukrasiaActive;

        (BestPhlegmaTarget, NumPhlegmaTargets) = SelectTarget(strategy, primaryTarget, 6, IsSplashTarget);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestPneumaTarget, NumPneumaTargets) = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget);

        NumAOETargets = NumNearbyTargets(strategy, 5);

        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, DotDuration, 2);

        DoGCD(strategy, primaryTarget);
        DoOGCD(strategy, primaryTarget);
    }

    private void DoGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        AutoKardia(strategy);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.Dosis))
                PushGCD(AID.Dosis, primaryTarget);

            return;
        }

        GoalZoneCombined(strategy, 25, Hints.GoalAOECircle(5), AID.Dyskrasia, 2);

        if (!Player.InCombat && Unlocked(AID.Eukrasia) && !Eukrasia)
            PushGCD(AID.Eukrasia, Player);

        if (Unlocked(AID.Eukrasia) && !CanFitGCD(TargetDotLeft, 1))
        {
            if (!Eukrasia)
                PushGCD(AID.Eukrasia, Player);

            PushGCD(AID.Dosis, BestDotTarget);
        }

        if (ReadyIn(AID.Pneuma) <= GCD && NumPneumaTargets > 1)
            PushGCD(AID.Pneuma, BestPneumaTarget);

        if (ShouldPhlegma(strategy))
        {
            if (ReadyIn(AID.Phlegma) <= GCD && primaryTarget is { } t)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(t.Actor, 6));

            PushGCD(AID.Phlegma, BestPhlegmaTarget);
        }

        if (NumRangedAOETargets > 1 && Sting > 0)
            PushGCD(AID.Toxikon, BestPhlegmaTarget);

        if (NumAOETargets > 1)
            PushGCD(AID.Dyskrasia, Player);

        PushGCD(AID.Dosis, primaryTarget);

        if (NumRangedAOETargets > 0 && Sting > 0)
            PushGCD(AID.Toxikon, BestRangedAOETarget);
        if (NumAOETargets > 0)
            PushGCD(AID.Dyskrasia, Player);
    }

    private bool ShouldPhlegma(Strategy strategy)
    {
        if (strategy.Buffs == OffensiveStrategy.Delay)
            return false;

        if (MaxChargesIn(AID.Phlegma) <= GCD || strategy.Buffs == OffensiveStrategy.Force)
            return true;

        if (ReadyIn(AID.Phlegma) > GCD)
            return false;

        return NumPhlegmaTargets > 2 || RaidBuffsLeft > GCD || RaidBuffsIn > 9000;
    }

    private void DoOGCD(Strategy strategy, Enemy? primaryTarget)
    {
        if (!Player.InCombat)
            return;

        if (Gall < 2 && NextGall > 10)
            PushOGCD(AID.Rhizomata, Player);

        if ((Gall == 3 || Gall == 2 && NextGall < 2.5f) && Player.HPMP.CurMP <= 9000 && strategy.Druochole == DruocholeStrategy.Auto)
        {
            var healTarget = World.Party.WithoutSlot(excludeAlliance: true).MinBy(x => (float)x.HPMP.CurHP / x.HPMP.MaxHP);
            PushOGCD(AID.Druochole, healTarget);
        }

        if (MP <= Player.HPMP.MaxMP * 0.7f)
            PushOGCD(AID.LucidDreaming, Player);

        if (NumRangedAOETargets > 0)
            PushOGCD(AID.Psyche, BestRangedAOETarget);
    }

    static readonly SID[] DotStatus = [SID.EukrasianDosis, SID.EukrasianDosisII, SID.EukrasianDosisIII, SID.EukrasianDyskrasia];

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

    private void AutoKardia(in Strategy strategy)
    {
        // early exits
        switch (strategy.Kardia.Value)
        {
            case KardiaStrategy.Manual:
                return;
            case KardiaStrategy.Auto:
                // fast path, assume any target is good enough if Auto is active
                if (Player.FindStatus(SID.Kardia) != null)
                    return;
                break;
        }

        if (FindKardiaTarget(strategy) is not Actor k)
            return;

        if (k.Statuses.Any(s => s.ID == (uint)SID.Kardion && s.SourceID == Player.InstanceID))
            return;

        if (Player.InCombat)
            PushOGCD(AID.Kardia, k);
        else
            PushGCD(AID.Kardia, k);
    }

    private Actor? FindKardiaTarget(in Strategy strategy)
    {
        switch (strategy.Kardia.Value)
        {
            case KardiaStrategy.Specific:
                return ResolveTargetOverride(strategy.Kardia.TrackRaw);
            case KardiaStrategy.Manual:
                return null;
        }

        var total = 0;
        var tanks = 0;
        Actor? tank = null;
        foreach (var actor in World.Party.WithoutSlot(excludeAlliance: true))
        {
            total++;
            if (actor.Class.GetRole() == Role.Tank)
            {
                tanks++;
                tank ??= actor;
            }
        }
        if (total == 1)
            return Player;

        if (tanks == 1)
            return tank;

        return null;
    }
}
