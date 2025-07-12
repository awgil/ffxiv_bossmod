using BossMod.SMN;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiSMNPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, RadiantAegis, RuinIV, MountainBuster, Slipstream, CrimsonCyclone, Necrotize }
    public enum TargetingStrategy { Auto, Manual }
    public enum RoleActionStrategy { Forbid, Comet, PhantomDart, Rust }
    public enum LBStrategy { Bahamut, Phoenix, Forbid }
    public enum AegisStrategy { LessThanFull, LessThan75, LessThan50, Forbid }
    public enum Ruin4Strategy { Early, Late, Forbid }
    public enum CommonStrategy { Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi SMN (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.SMN), 100, 30);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatic", "Automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manual", "Manually select target");
        res.Define(Track.RoleActions).As<RoleActionStrategy>("Role Actions", "", 300)
            .AddOption(RoleActionStrategy.Forbid, "Forbid", "Do not use any role actions")
            .AddOption(RoleActionStrategy.Comet, "Comet", "Use Comet when available")
            .AddOption(RoleActionStrategy.PhantomDart, "Phantom Dart", "Use Phantom Dart when available")
            .AddOption(RoleActionStrategy.Rust, "Rust", "Use Rust when available")
            .AddAssociatedActions(AID.CometPvP, AID.PhantomDartPvP, AID.RustPvP);
        res.Define(Track.LimitBreak).As<LBStrategy>("Limit Break", "", 300)
            .AddOption(LBStrategy.Bahamut, "Bahamut", "Allow use of Bahamut only for Limit Break when available")
            .AddOption(LBStrategy.Phoenix, "Phoenix", "Allow use of Phoenix only for Limit Break when available")
            .AddOption(LBStrategy.Forbid, "Forbid", "Forbid use of Limit Break entirely")
            .AddAssociatedActions(AID.SummonBahamutPvP, AID.SummonPhoenixPvP);
        res.Define(Track.RadiantAegis).As<AegisStrategy>("Radiant Aegis", "", 300)
            .AddOption(AegisStrategy.LessThanFull, "Less than Full", "Use Radiant Aegis when HP is less than 100%")
            .AddOption(AegisStrategy.LessThan75, "Less than 75%", "Use Radiant Aegis when HP is less than 75%")
            .AddOption(AegisStrategy.LessThan50, "Less than 50%", "Use Radiant Aegis when HP is less than 50%")
            .AddOption(AegisStrategy.Forbid, "Forbid", "Do not use Radiant Aegis")
            .AddAssociatedActions(AID.RadiantAegisPvP);
        res.Define(Track.RuinIV).As<Ruin4Strategy>("Ruin IV", "", 300)
            .AddOption(Ruin4Strategy.Early, "Early", "Use Ruin IV as soon as it is available")
            .AddOption(Ruin4Strategy.Late, "Late", "Use Ruin IV as late as possible")
            .AddOption(Ruin4Strategy.Forbid, "Forbid", "Do not use Ruin IV")
            .AddAssociatedActions(AID.Ruin4PvP);
        res.Define(Track.MountainBuster).As<CommonStrategy>("Mountain Buster", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Mountain Buster when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Mountain Buster")
            .AddAssociatedActions(AID.MountainBusterPvP);
        res.Define(Track.Slipstream).As<CommonStrategy>("Slipstream", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Slipstream when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Slipstream")
            .AddAssociatedActions(AID.SlipstreamPvP);
        res.Define(Track.CrimsonCyclone).As<CommonStrategy>("Crimson Cyclone", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Crimson Cyclone when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Crimson Cyclone")
            .AddAssociatedActions(AID.CrimsonCyclonePvP);
        res.Define(Track.Necrotize).As<CommonStrategy>("Necrotize", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Necrotize when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Necrotize")
            .AddAssociatedActions(AID.NecrotizePvP);
        return res;
    }

    public bool IsReady(AID aid) => CDRemaining(aid) <= 0.2f;
    public int NumConeTargets;
    private int NumSplashTargets;
    private Enemy? BestConeTargets;
    private Enemy? BestSplashTargets;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        (BestConeTargets, NumConeTargets) = GetBestTarget(primaryTarget, 8, Is8yConeTarget);
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        var LBready = World.Party.LimitBreakLevel >= 1;
        var BestConeTarget = NumConeTargets > 1 ? BestConeTargets : primaryTarget;
        var BestSplashTarget = NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        var auto = strategy.Option(Track.Targeting).As<TargetingStrategy>() == TargetingStrategy.Auto;
        if (auto)
        {
            GetPvPTarget(25);
        }
        if (In25y(primaryTarget?.Actor) && !HasEffect(SID.GuardPvP) && HasLOS(primaryTarget?.Actor))
        {
            if (LBready)
            {
                var lb = strategy.Option(Track.LimitBreak).As<LBStrategy>();
                if (lb == LBStrategy.Bahamut)
                    QueueGCD(AID.SummonBahamutPvP, primaryTarget?.Actor, GCDPriority.High + 1);
                if (lb == LBStrategy.Phoenix)
                    QueueGCD(AID.SummonPhoenixPvP, Player, GCDPriority.High + 1);
            }
            var role = strategy.Option(Track.RoleActions).As<RoleActionStrategy>();
            if (role switch
            {
                RoleActionStrategy.Comet => HasEffect(SID.CometEquippedPvP) && IsReady(AID.CometPvP) && !IsMoving,
                RoleActionStrategy.PhantomDart => HasEffect(SID.PhantomDartEquippedPvP) && IsReady(AID.PhantomDartPvP),
                RoleActionStrategy.Rust => HasEffect(SID.RustEquippedPvP) && IsReady(AID.RustPvP),
                _ => false
            })
                QueueGCD(role switch
                {
                    RoleActionStrategy.Comet => AID.CometPvP,
                    RoleActionStrategy.PhantomDart => AID.PhantomDartPvP,
                    RoleActionStrategy.Rust => AID.RustPvP,
                    _ => AID.None
                },
                role switch
                {
                    RoleActionStrategy.Comet => auto ? BestSplashTarget?.Actor : primaryTarget?.Actor,
                    RoleActionStrategy.PhantomDart => primaryTarget?.Actor,
                    RoleActionStrategy.Rust => auto ? BestSplashTarget?.Actor : primaryTarget?.Actor,
                    _ => null
                }, GCDPriority.VeryHigh + 1);
            if (IsReady(AID.BrandOfPurgatoryPvP) && HasEffect(SID.FirebirdTrance))
                QueueGCD(AID.BrandOfPurgatoryPvP, auto ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.High);
            if (IsReady(AID.DeathflarePvP) && HasEffect(SID.DreadwyrmTrance))
                QueueGCD(AID.DeathflarePvP, auto ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.High);
            if (IsReady(AID.Ruin4PvP) && HasEffect(SID.FurtherRuinPvP))
            {
                var r4 = strategy.Option(Track.RuinIV).As<Ruin4Strategy>();
                if (r4 == Ruin4Strategy.Late &&
                    (StatusRemaining(Player, SID.FurtherRuinPvP) <= 3f || CDRemaining(AID.NecrotizePvP) <= 1f))
                    QueueGCD(AID.Ruin4PvP, auto ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.High + 1);
                if (r4 == Ruin4Strategy.Early)
                    QueueGCD(AID.Ruin4PvP, auto ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.ModeratelyHigh);
            }
            if (CDRemaining(AID.NecrotizePvP) < 10.6f && !HasEffect(SID.FurtherRuinPvP) &&
                strategy.Option(Track.Necrotize).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.NecrotizePvP, primaryTarget?.Actor, GCDPriority.SlightlyHigh);
            if (IsReady(AID.CrimsonCyclonePvP) && strategy.Option(Track.CrimsonCyclone).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.CrimsonCyclonePvP, auto ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.AboveAverage);
            if (IsReady(AID.CrimsonStrikePvP) && HasEffect(SID.CrimsonStrikeReadyPvP))
                QueueGCD(AID.CrimsonStrikePvP, primaryTarget?.Actor, StatusRemaining(Player, SID.FurtherRuinPvP) <= 3f ? GCDPriority.High + 1 : GCDPriority.AboveAverage);
            if (IsReady(AID.MountainBusterPvP) && InRange(primaryTarget?.Actor, 8f) &&
                strategy.Option(Track.MountainBuster).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.MountainBusterPvP, auto ? BestConeTarget?.Actor : primaryTarget?.Actor, GCDPriority.Average);
            if (IsReady(AID.SlipstreamPvP) && !IsMoving &&
                strategy.Option(Track.Slipstream).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.SlipstreamPvP, auto ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.BelowAverage);
            if (IsReady(AID.AstralImpulsePvP) && HasEffect(SID.DreadwyrmTrance))
                QueueGCD(AID.AstralImpulsePvP, primaryTarget?.Actor, GCDPriority.SlightlyLow);
            if (IsReady(AID.FountainOfFirePvP) && HasEffect(SID.FirebirdTrance))
                QueueGCD(AID.FountainOfFirePvP, primaryTarget?.Actor, GCDPriority.SlightlyLow);
            QueueGCD(AID.Ruin3PvP, primaryTarget?.Actor, GCDPriority.Low);
        }
        if (IsReady(AID.RadiantAegisPvP) && strategy.Option(Track.RadiantAegis).As<AegisStrategy>() switch
        {
            AegisStrategy.LessThanFull => PlayerHPP < 100,
            AegisStrategy.LessThan75 => PlayerHPP < 75,
            AegisStrategy.LessThan50 => PlayerHPP < 50,
            _ => false
        })
            QueueGCD(AID.RadiantAegisPvP, Player, GCDPriority.VeryHigh);
    }
}
