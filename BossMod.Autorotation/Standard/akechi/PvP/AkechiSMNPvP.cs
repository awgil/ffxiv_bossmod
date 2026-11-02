using BossMod.SMN;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiSMNPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, Placement, RadiantAegis, RuinIV, CrimsonCyclone, MountainBuster, Slipstream, Necrotize }
    public enum TargetingStrategy { Auto, FocusTargetsTarget, Manual }
    public enum RoleActionStrategy { Forbid, Comet, PhantomDart, Rust }
    public enum LBStrategy { Bahamut, Phoenix, Forbid }
    public enum LBPlacement { Self, Target, Crystal, CrystalOrTarget }
    public enum AegisStrategy { Auto, Two, Three, Four, LessThanFull, LessThan75, LessThan50, Forbid }
    public enum Ruin4Strategy { Early, Late, Forbid }
    public enum CycloneStrategy { Five, Ten, Fifteen, Twenty, Allow, Forbid }
    public enum BusterStrategy { Allow, HighestHPP, Forbid }
    public enum CommonStrategy { Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi SMN (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.SMN), 100, 30, PvP: PvPCompatibility.PvPOnly);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatically select best target")
            .AddOption(TargetingStrategy.FocusTargetsTarget, "Automatically target your current Focus Target's target - if no Focus Target or if Focus Target is hostile, then automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manually select target");

        res.Define(Track.RoleActions).As<RoleActionStrategy>("Role Actions", "", 300)
            .AddOption(RoleActionStrategy.Forbid, "Do not use any role actions")
            .AddOption(RoleActionStrategy.Comet, "Use Comet when available")
            .AddOption(RoleActionStrategy.PhantomDart, "Use Phantom Dart when available")
            .AddOption(RoleActionStrategy.Rust, "Use Rust when available")
            .AddAssociatedActions(AID.CometPvP, AID.PhantomDartPvP, AID.RustPvP);

        res.Define(Track.LimitBreak).As<LBStrategy>("LB Summon", "", 300)
            .AddOption(LBStrategy.Bahamut, "Allow use of Bahamut only for Limit Break when available")
            .AddOption(LBStrategy.Phoenix, "Allow use of Phoenix only for Limit Break when available")
            .AddOption(LBStrategy.Forbid, "Do not use Limit Break")
            .AddAssociatedActions(AID.SummonBahamutPvP, AID.SummonPhoenixPvP);

        res.Define(Track.Placement).As<LBPlacement>("LB Placement", "", 300)
            .AddOption(LBPlacement.Self, "Place Limit Break summon on self")
            .AddOption(LBPlacement.Target, "Place Limit Break summon on current target")
            .AddOption(LBPlacement.Crystal, "Place Limit Break summon on crystal only; will hold LB until near the crystal (only works for Crystalline Conflict)")
            .AddOption(LBPlacement.CrystalOrTarget, "Place Limit Break summon on crystal or target if crystal is unavailable (intended for Crystalline Conflict, but works in others too)");

        res.Define(Track.RadiantAegis).As<AegisStrategy>("Radiant Aegis", "", 300)
            .AddOption(AegisStrategy.Auto, "Use Radiant Aegis when HP is less than 75% and two or more targets are targeting you, or when HP is below 33%")
            .AddOption(AegisStrategy.Two, "Use Radiant Aegis when HP is not full and two or more targets are targeting you")
            .AddOption(AegisStrategy.Three, "Use Radiant Aegis when HP is not full and three or more targets are targeting you")
            .AddOption(AegisStrategy.Four, "Use Radiant Aegis when HP is not full and four or more targets are targeting you")
            .AddOption(AegisStrategy.LessThanFull, "Use Radiant Aegis when HP is less than 100%")
            .AddOption(AegisStrategy.LessThan75, "Use Radiant Aegis when HP is less than 75%")
            .AddOption(AegisStrategy.LessThan50, "Use Radiant Aegis when HP is less than 50%")
            .AddOption(AegisStrategy.Forbid, "Do not use Radiant Aegis")
            .AddAssociatedActions(AID.RadiantAegisPvP);

        res.Define(Track.RuinIV).As<Ruin4Strategy>("Ruin IV", "", 300)
            .AddOption(Ruin4Strategy.Early, "Use Ruin IV as soon as it is available")
            .AddOption(Ruin4Strategy.Late, "Use Ruin IV as late as possible")
            .AddOption(Ruin4Strategy.Forbid, "Do not use Ruin IV")
            .AddAssociatedActions(AID.Ruin4PvP);

        res.Define(Track.CrimsonCyclone).As<CycloneStrategy>("Crimson Cyclone", "", 300)
            .AddOption(CycloneStrategy.Five, "Use Crimson Cyclone only within 5 yalms of target")
            .AddOption(CycloneStrategy.Ten, "Use Crimson Cyclone only within 10 yalms of target")
            .AddOption(CycloneStrategy.Fifteen, "Use Crimson Cyclone only within 15 yalms of target")
            .AddOption(CycloneStrategy.Twenty, "Use Crimson Cyclone only within 20 yalms of target")
            .AddOption(CycloneStrategy.Allow, "Use Crimson Cyclone when available at any range")
            .AddOption(CycloneStrategy.Forbid, "Do not use Crimson Cyclone")
            .AddAssociatedActions(AID.CrimsonCyclonePvP);

        res.Define(Track.MountainBuster).As<BusterStrategy>("Mountain Buster", "", 300)
            .AddOption(BusterStrategy.Allow, "Use Mountain Buster on the best suitable target nearby when available")
            .AddOption(BusterStrategy.HighestHPP, "Use Mountain Buster on the highest HP% target nearby when available")
            .AddOption(BusterStrategy.Forbid, "Do not use Mountain Buster")
            .AddAssociatedActions(AID.MountainBusterPvP);

        res.Define(Track.Slipstream).As<CommonStrategy>("Slipstream", "", 300)
            .AddOption(CommonStrategy.Allow, "Use Slipstream when available")
            .AddOption(CommonStrategy.Forbid, "Do not use Slipstream")
            .AddAssociatedActions(AID.SlipstreamPvP);

        res.Define(Track.Necrotize).As<CommonStrategy>("Necrotize", "", 300)
            .AddOption(CommonStrategy.Allow, "Use Necrotize when available")
            .AddOption(CommonStrategy.Forbid, "Do not use Necrotize")
            .AddAssociatedActions(AID.NecrotizePvP);

        return res;
    }

    private bool AutoTarget;
    private Actor? SplashTarget;
    private Actor? BigSplashTarget;

    private bool IsReady(AID aid) => Cooldown(aid) <= 0.2f;
    private Actor? Retarget(Actor? target) => AutoTarget ? target : PrimaryTarget;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || IsGuarded(Player))
            return;

        var targeting = strategy.Option(Track.Targeting).As<TargetingStrategy>();
        AutoTarget = targeting == TargetingStrategy.Auto;
        SplashTarget = Retarget(GetBestTarget(primaryTarget, 25, IsSplashTarget).Best?.Actor);
        BigSplashTarget = Retarget(GetBestTarget(primaryTarget, 25, Is10ySplashTarget).Best?.Actor);
        if (targeting != TargetingStrategy.Manual)
            GetPvPTarget(25, targeting == TargetingStrategy.FocusTargetsTarget);
        QueueRadiantAegis(strategy);
        if (In25y(PrimaryTarget) && HasLOS(PrimaryTarget))
        {
            QueueLimitBreak(strategy);
            QueueRoleAction(strategy);
            QueueTrances();
            QueueRuin4(strategy);
            QueueNecrotize(strategy);
            QueueCrimson(strategy);
            QueueMountainBuster(strategy, primaryTarget);
            QueueSlipstream(strategy);
            QueueGCD(AID.Ruin3PvP, PrimaryTarget, GCDPriority.Low);
        }
    }

    private void QueueLimitBreak(StrategyValues strategy)
    {
        var crystal = FindActor(0x3886);
        var summon = strategy.Option(Track.LimitBreak).As<LBStrategy>() switch
        {
            LBStrategy.Bahamut => AID.SummonBahamutPvP,
            LBStrategy.Phoenix => AID.SummonPhoenixPvP,
            _ => AID.None
        };
        var placement = strategy.Option(Track.Placement).As<LBPlacement>() switch
        {
            LBPlacement.Self => Player,
            LBPlacement.Target => PrimaryTarget,
            LBPlacement.Crystal => crystal,
            LBPlacement.CrystalOrTarget => crystal ?? PrimaryTarget,
            _ => null
        };
        if (World.Party.LimitBreakLevel >= 1)
            QueueGCD(summon, placement, GCDPriority.High + 1);
    }
    private void QueueRoleAction(StrategyValues strategy)
    {
        var (condition, action, target) = strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
        {
            RoleActionStrategy.Comet => (HasStatus(SID.CometEquippedPvP) && !IsMoving, AID.CometPvP, BigSplashTarget),
            RoleActionStrategy.PhantomDart => (HasStatus(SID.PhantomDartEquippedPvP), AID.PhantomDartPvP, PrimaryTarget),
            RoleActionStrategy.Rust => (HasStatus(SID.RustEquippedPvP), AID.RustPvP, PrimaryTarget),
            _ => (false, AID.None, null)
        };
        if (condition && IsReady(action))
            QueueGCD(action, target, GCDPriority.High + 1);
    }
    private void QueueTrances()
    {
        var bahamut = HasStatus(SID.DreadwyrmTrance);
        var phoenix = HasStatus(SID.FirebirdTrance);
        if (phoenix && IsReady(AID.BrandOfPurgatoryPvP))
            QueueGCD(AID.BrandOfPurgatoryPvP, SplashTarget, GCDPriority.High);
        if (bahamut && IsReady(AID.DeathflarePvP))
            QueueGCD(AID.DeathflarePvP, SplashTarget, GCDPriority.High);
        if (bahamut && IsReady(AID.AstralImpulsePvP))
            QueueGCD(AID.AstralImpulsePvP, PrimaryTarget, GCDPriority.SlightlyLow);
        if (phoenix && IsReady(AID.FountainOfFirePvP))
            QueueGCD(AID.FountainOfFirePvP, PrimaryTarget, GCDPriority.SlightlyLow);
    }
    private void QueueRuin4(StrategyValues strategy)
    {
        var (condition, priority) = strategy.Option(Track.RuinIV).As<Ruin4Strategy>() switch
        {
            Ruin4Strategy.Early => (true, GCDPriority.High + 1),
            Ruin4Strategy.Late => (Status(SID.FurtherRuinPvP) <= 3f || Cooldown(AID.NecrotizePvP) <= 1f, GCDPriority.ModeratelyHigh),
            _ => (false, GCDPriority.None)
        };
        if (condition && IsReady(AID.Ruin4PvP) && HasStatus(SID.FurtherRuinPvP))
            QueueGCD(AID.Ruin4PvP, SplashTarget, priority);
    }
    private void QueueNecrotize(StrategyValues strategy)
    {
        if (strategy.Option(Track.Necrotize).As<CommonStrategy>() == CommonStrategy.Allow && Cooldown(AID.NecrotizePvP) < 10.6f && !HasStatus(SID.FurtherRuinPvP))
            QueueGCD(AID.NecrotizePvP, PrimaryTarget, GCDPriority.SlightlyHigh);
    }
    private void QueueCrimson(StrategyValues strategy)
    {
        var condition = strategy.Option(Track.CrimsonCyclone).As<CycloneStrategy>() switch
        {
            CycloneStrategy.Five => In5y(SplashTarget),
            CycloneStrategy.Ten => In10y(SplashTarget),
            CycloneStrategy.Fifteen => In15y(SplashTarget),
            CycloneStrategy.Twenty => In20y(SplashTarget),
            CycloneStrategy.Allow => true,
            _ => false
        };
        if (condition && IsReady(AID.CrimsonCyclonePvP))
            QueueGCD(AID.CrimsonCyclonePvP, SplashTarget, GCDPriority.AboveAverage);
        if (HasStatus(SID.CrimsonStrikeReadyPvP))
            QueueGCD(AID.CrimsonStrikePvP, SplashTarget, Status(SID.FurtherRuinPvP) <= 3f ? GCDPriority.High + 1 : GCDPriority.AboveAverage);
    }
    private void QueueMountainBuster(StrategyValues strategy, Enemy? primaryTarget)
    {
        var (target, priority) = strategy.Option(Track.MountainBuster).As<BusterStrategy>() switch
        {
            BusterStrategy.Allow => (Retarget(GetBestTarget(primaryTarget, 8, ConeTargetCheck(8)).Best?.Actor) ?? PrimaryTarget, GCDPriority.Average),
            BusterStrategy.HighestHPP => (HighestHPPvPTarget(8)?.Actor, GCDPriority.High + 1),
            _ => (null, GCDPriority.None)
        };
        if (IsReady(AID.MountainBusterPvP) && DistanceFrom(target, 8f))
            QueueGCD(AID.MountainBusterPvP, target, priority);
    }
    private void QueueSlipstream(StrategyValues strategy)
    {
        if (strategy.Option(Track.Slipstream).As<CommonStrategy>() == CommonStrategy.Allow && IsReady(AID.SlipstreamPvP) && !IsMoving)
            QueueGCD(AID.SlipstreamPvP, BigSplashTarget, GCDPriority.BelowAverage);
    }
    private void QueueRadiantAegis(StrategyValues strategy)
    {
        var condition = strategy.Option(Track.RadiantAegis).As<AegisStrategy>() switch
        {
            AegisStrategy.Auto => HPBelow(0.75f) && EnemiesTargetingPlayer >= 2 || HPBelow(0.33f),
            AegisStrategy.Two => HPBelow(1f) && EnemiesTargetingPlayer >= 2,
            AegisStrategy.Three => HPBelow(1f) && EnemiesTargetingPlayer >= 3,
            AegisStrategy.Four => HPBelow(1f) && EnemiesTargetingPlayer >= 4,
            AegisStrategy.LessThanFull => HPBelow(1f),
            AegisStrategy.LessThan75 => HPBelow(0.75f),
            AegisStrategy.LessThan50 => HPBelow(0.5f),
            _ => false
        };
        if (condition && IsReady(AID.RadiantAegisPvP))
            QueueGCD(AID.RadiantAegisPvP, Player, GCDPriority.Max);
    }
}
