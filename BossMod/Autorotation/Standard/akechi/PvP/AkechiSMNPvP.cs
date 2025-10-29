using BossMod.SMN;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiSMNPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, Placement, RadiantAegis, RuinIV, CrimsonCyclone, MountainBuster, Slipstream, Necrotize }
    public enum TargetingStrategy { Auto, Manual }
    public enum RoleActionStrategy { Forbid, Comet, PhantomDart, Rust }
    public enum LBStrategy { Bahamut, Phoenix, Forbid }
    public enum LBPlacement { Self, Target, Crystal, CrystalOrTarget }
    public enum AegisStrategy { Auto, Two, Three, Four, LessThanFull, LessThan75, LessThan50, Forbid }
    public enum Ruin4Strategy { Early, Late, Forbid }
    public enum CycloneStrategy { Five, Ten, Fifteen, Twenty, Allow, Forbid }
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

        res.Define(Track.LimitBreak).As<LBStrategy>("LB Summon", "", 300)
            .AddOption(LBStrategy.Bahamut, "Bahamut", "Allow use of Bahamut only for Limit Break when available")
            .AddOption(LBStrategy.Phoenix, "Phoenix", "Allow use of Phoenix only for Limit Break when available")
            .AddOption(LBStrategy.Forbid, "Forbid", "Do not use Limit Break")
            .AddAssociatedActions(AID.SummonBahamutPvP, AID.SummonPhoenixPvP);

        res.Define(Track.Placement).As<LBPlacement>("LB Placement", "", 300)
            .AddOption(LBPlacement.Self, "Self", "Place Limit Break summon on self")
            .AddOption(LBPlacement.Target, "Target", "Place Limit Break summon on current target")
            .AddOption(LBPlacement.Crystal, "Crystal", "Place Limit Break summon on crystal only; will hold LB until near the crystal (only works for Crystalline Conflict)")
            .AddOption(LBPlacement.CrystalOrTarget, "Crystal or Target", "Place Limit Break summon on crystal or target if crystal is unavailable (intended for Crystalline Conflict, but works in others too)");

        res.Define(Track.RadiantAegis).As<AegisStrategy>("Radiant Aegis", "", 300)
            .AddOption(AegisStrategy.Auto, "Automatic", "Use Radiant Aegis when HP is less than 75% and two or more targets are targeting you, or when HP is below 33%")
            .AddOption(AegisStrategy.Two, "2 Targets", "Use Radiant Aegis when HP is not full and two or more targets are targeting you")
            .AddOption(AegisStrategy.Three, "3 Targets", "Use Radiant Aegis when HP is not full and three or more targets are targeting you")
            .AddOption(AegisStrategy.Four, "4 Targets", "Use Radiant Aegis when HP is not full and four or more targets are targeting you")
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

        res.Define(Track.CrimsonCyclone).As<CycloneStrategy>("Crimson Cyclone", "", 300)
            .AddOption(CycloneStrategy.Five, "5 yalms", "Use Crimson Cyclone only within 5 yalms of target")
            .AddOption(CycloneStrategy.Ten, "10 yalms", "Use Crimson Cyclone only within 10 yalms of target")
            .AddOption(CycloneStrategy.Fifteen, "15 yalms", "Use Crimson Cyclone only within 15 yalms of target")
            .AddOption(CycloneStrategy.Twenty, "20 yalms", "Use Crimson Cyclone only within 20 yalms of target")
            .AddOption(CycloneStrategy.Allow, "Allow", "Use Crimson Cyclone when available at any range")
            .AddOption(CycloneStrategy.Forbid, "Forbid", "Do not use Crimson Cyclone")
            .AddAssociatedActions(AID.CrimsonCyclonePvP);

        res.Define(Track.MountainBuster).As<CommonStrategy>("Mountain Buster", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Mountain Buster when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Mountain Buster")
            .AddAssociatedActions(AID.MountainBusterPvP);

        res.Define(Track.Slipstream).As<CommonStrategy>("Slipstream", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Slipstream when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Slipstream")
            .AddAssociatedActions(AID.SlipstreamPvP);

        res.Define(Track.Necrotize).As<CommonStrategy>("Necrotize", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Necrotize when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Necrotize")
            .AddAssociatedActions(AID.NecrotizePvP);

        return res;
    }

    public bool IsReady(AID aid) => CDRemaining(aid) <= 0.2f;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || Player.FindStatus(ClassShared.SID.GuardPvP) != null)
            return;

        var (BestConeTargets, NumConeTargets) = GetBestTarget(primaryTarget, 8, Is8yConeTarget);
        var (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        var (BestSlipstreamTargets, NumSlipstreamTargets) = GetBestTarget(primaryTarget, 25, Is10ySplashTarget);
        var BestConeTarget = NumConeTargets > 1 ? BestConeTargets : primaryTarget;
        var BestSplashTarget = NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        var BestSlipstreamTarget = NumSlipstreamTargets > 1 ? BestSlipstreamTargets : primaryTarget;
        var mainTarget = primaryTarget?.Actor;
        var auto = strategy.Option(Track.Targeting).As<TargetingStrategy>() == TargetingStrategy.Auto;
        var BestTarget = auto ? BestSplashTarget?.Actor : mainTarget;

        if (auto)
        {
            GetPvPTarget(25);
        }

        if (In25y(mainTarget) && HasLOS(mainTarget))
        {
            if (World.Party.LimitBreakLevel >= 1)
            {
                var crystal = World.Actors.FirstOrDefault(x => x.OID == 0x3886); //crystal
                var summon = strategy.Option(Track.LimitBreak).As<LBStrategy>() switch
                {
                    LBStrategy.Bahamut => AID.SummonBahamutPvP,
                    LBStrategy.Phoenix => AID.SummonPhoenixPvP,
                    _ => AID.None
                };
                var placement = strategy.Option(Track.Placement).As<LBPlacement>() switch
                {
                    LBPlacement.Self => Player,
                    LBPlacement.Target => mainTarget,
                    LBPlacement.Crystal => crystal,
                    LBPlacement.CrystalOrTarget => crystal ?? mainTarget,
                    _ => null
                };
                QueueGCD(summon, placement, GCDPriority.High + 1);
            }

            var (roleCondition, roleAction, roleTarget) = strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
            {
                RoleActionStrategy.Comet => (HasEffect(SID.CometEquippedPvP) && IsReady(AID.CometPvP) && !IsMoving, AID.CometPvP, auto ? BestSlipstreamTarget?.Actor : mainTarget),
                RoleActionStrategy.PhantomDart => (HasEffect(SID.PhantomDartEquippedPvP) && IsReady(AID.PhantomDartPvP), AID.PhantomDartPvP, mainTarget),
                RoleActionStrategy.Rust => (HasEffect(SID.RustEquippedPvP) && IsReady(AID.RustPvP), AID.RustPvP, mainTarget),
                _ => (false, AID.None, null)
            };
            if (roleCondition)
                QueueGCD(roleAction, roleTarget, GCDPriority.High + 1);

            if (IsReady(AID.BrandOfPurgatoryPvP) && HasEffect(SID.FirebirdTrance))
                QueueGCD(AID.BrandOfPurgatoryPvP, BestTarget, GCDPriority.High);

            if (IsReady(AID.DeathflarePvP) && HasEffect(SID.DreadwyrmTrance))
                QueueGCD(AID.DeathflarePvP, BestTarget, GCDPriority.High);

            if (IsReady(AID.Ruin4PvP) && HasEffect(SID.FurtherRuinPvP))
            {
                var (r4Condition, r4Priority) = strategy.Option(Track.RuinIV).As<Ruin4Strategy>() switch
                {
                    Ruin4Strategy.Early => (true, GCDPriority.High + 1),
                    Ruin4Strategy.Late => (StatusRemaining(Player, SID.FurtherRuinPvP) <= 3f || CDRemaining(AID.NecrotizePvP) <= 1f, GCDPriority.ModeratelyHigh),
                    _ => (false, GCDPriority.None)
                };
                if (r4Condition)
                    QueueGCD(AID.Ruin4PvP, BestTarget, r4Priority);
            }

            if (CDRemaining(AID.NecrotizePvP) < 10.6f && !HasEffect(SID.FurtherRuinPvP) && strategy.Option(Track.Necrotize).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.NecrotizePvP, mainTarget, GCDPriority.SlightlyHigh);

            if (IsReady(AID.CrimsonCyclonePvP) && strategy.Option(Track.CrimsonCyclone).As<CycloneStrategy>() switch
            {
                CycloneStrategy.Five => In5y(BestTarget),
                CycloneStrategy.Ten => In10y(BestTarget),
                CycloneStrategy.Fifteen => In15y(BestTarget),
                CycloneStrategy.Twenty => In20y(BestTarget),
                CycloneStrategy.Allow => true,
                _ => false
            })
                QueueGCD(AID.CrimsonCyclonePvP, BestTarget, GCDPriority.AboveAverage);

            if (HasEffect(SID.CrimsonStrikeReadyPvP))
                QueueGCD(AID.CrimsonStrikePvP, mainTarget, StatusRemaining(Player, SID.FurtherRuinPvP) <= 3f ? GCDPriority.High + 1 : GCDPriority.AboveAverage);

            if (IsReady(AID.MountainBusterPvP) && DistanceFrom(mainTarget, 8f) && strategy.Option(Track.MountainBuster).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.MountainBusterPvP, auto ? BestConeTarget?.Actor : mainTarget, GCDPriority.Average);

            if (IsReady(AID.SlipstreamPvP) && !IsMoving && strategy.Option(Track.Slipstream).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.SlipstreamPvP, BestSlipstreamTarget?.Actor, GCDPriority.BelowAverage);

            if (IsReady(AID.AstralImpulsePvP) && HasEffect(SID.DreadwyrmTrance))
                QueueGCD(AID.AstralImpulsePvP, mainTarget, GCDPriority.SlightlyLow);

            if (IsReady(AID.FountainOfFirePvP) && HasEffect(SID.FirebirdTrance))
                QueueGCD(AID.FountainOfFirePvP, mainTarget, GCDPriority.SlightlyLow);

            if (IsReady(AID.RadiantAegisPvP) && strategy.Option(Track.RadiantAegis).As<AegisStrategy>() switch
            {
                AegisStrategy.Auto => (PlayerHPP is < 75 and not 0 && EnemiesTargetingSelf(2)) || PlayerHPP is < 33 and not 0,
                AegisStrategy.Two => EnemiesTargetingSelf(2) && PlayerHPP is < 100 and not 0,
                AegisStrategy.Three => EnemiesTargetingSelf(3) && PlayerHPP is < 100 and not 0,
                AegisStrategy.Four => EnemiesTargetingSelf(4) && PlayerHPP is < 100 and not 0,
                AegisStrategy.LessThanFull => PlayerHPP is < 100 and not 0,
                AegisStrategy.LessThan75 => PlayerHPP is < 75 and not 0,
                AegisStrategy.LessThan50 => PlayerHPP is < 50 and not 0,
                _ => false
            })
                QueueGCD(AID.RadiantAegisPvP, mainTarget, GCDPriority.Max);

            QueueGCD(AID.Ruin3PvP, mainTarget, GCDPriority.Low);
        }
    }
}
