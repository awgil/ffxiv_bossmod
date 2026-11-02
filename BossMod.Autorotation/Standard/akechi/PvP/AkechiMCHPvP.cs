using BossMod.MCH;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiMCHPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, Analysis, Tools, Scattergun, FullMetalField, Wildfire, BishopAutoturret, TurretPlacement }
    public enum TargetingStrategy { Auto, FocusTargetsTarget, Manual }
    public enum RoleActionStrategy { Forbid, Dervish, Bravery, EagleEyeShot }
    public enum LBStrategy { ASAP, LessThan70, LessThan60, LessThan50, LessThan40, Forbid }
    public enum AnalysisStrategy { Any, DrillAA, BBCS, Forbid }
    public enum ToolsStrategy { ASAP, BuffOrOvercap, Forbid }
    public enum CommonStrategy { Allow, Forbid }
    public enum TurretPlacement { Self, Target, Crystal, CrystalOrTarget }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi MCH (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.MCH), 100, 30, PvP: PvPCompatibility.PvPOnly);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatically select best target")
            .AddOption(TargetingStrategy.FocusTargetsTarget, "Automatically target your current Focus Target's target - if no Focus Target or if Focus Target is hostile, then automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manually select target");

        res.Define(Track.RoleActions).As<RoleActionStrategy>("Role Actions", "", 300)
            .AddOption(RoleActionStrategy.Forbid, "Do not use any role actions")
            .AddOption(RoleActionStrategy.Dervish, "Use Dervish when available")
            .AddOption(RoleActionStrategy.Bravery, "Use Bravery when available")
            .AddOption(RoleActionStrategy.EagleEyeShot, "Use Eagle Eye Shot when available")
            .AddAssociatedActions(AID.DervishPvP, AID.BraveryPvP, AID.EagleEyeShotPvP);

        res.Define(Track.LimitBreak).As<LBStrategy>("Limit Break", "", 300)
            .AddOption(LBStrategy.ASAP, "Use Limit Break as soon as it is available")
            .AddOption(LBStrategy.LessThan70, "Use Limit Break when target's HP is less than 70%")
            .AddOption(LBStrategy.LessThan60, "Use Limit Break when target's HP is less than 60%")
            .AddOption(LBStrategy.LessThan50, "Use Limit Break when target's HP is less than 50%")
            .AddOption(LBStrategy.LessThan40, "Use Limit Break when target's HP is less than 40%")
            .AddOption(LBStrategy.Forbid, "Forbid use of Limit Break entirely")
            .AddAssociatedActions(AID.MarksmansSpitePvP);

        res.Define(Track.Analysis).As<AnalysisStrategy>("Analysis", "", 300)
            .AddOption(AnalysisStrategy.Any, "Use Analysis to buff any tool")
            .AddOption(AnalysisStrategy.DrillAA, "Use Analysis to buff Drill and Air Anchor only")
            .AddOption(AnalysisStrategy.BBCS, "Use Analysis to buff Bioblaster and ChainSaw only")
            .AddOption(AnalysisStrategy.Forbid, "Do not use Analysis")
            .AddAssociatedActions(AID.AnalysisPvP);

        res.Define(Track.Tools).As<ToolsStrategy>("Tools", "", 300)
            .AddOption(ToolsStrategy.ASAP, "Use Tools as soon as it is available")
            .AddOption(ToolsStrategy.BuffOrOvercap, "Use Tools when buffed by Analysis or about to overcap on charges")
            .AddOption(ToolsStrategy.Forbid, "Do not use Tools")
            .AddAssociatedActions(AID.DrillPvP, AID.BioblasterPvP, AID.AirAnchorPvP, AID.ChainSawPvP);

        res.Define(Track.Scattergun).As<CommonStrategy>("Scattergun", "", 300)
            .AddOption(CommonStrategy.Allow, "Use Scattergun when available")
            .AddOption(CommonStrategy.Forbid, "Do not use Scattergun")
            .AddAssociatedActions(AID.ScattergunPvP);

        res.Define(Track.FullMetalField).As<CommonStrategy>("Full Metal Field", "", 300)
            .AddOption(CommonStrategy.Allow, "Use Full Metal Field when available and not Overheated")
            .AddOption(CommonStrategy.Forbid, "Do not use Full Metal Field")
            .AddAssociatedActions(AID.FullMetalFieldPvP);

        res.Define(Track.Wildfire).As<CommonStrategy>("Wildfire", "", 300)
            .AddOption(CommonStrategy.Allow, "Use Wildfire when available")
            .AddOption(CommonStrategy.Forbid, "Do not use Wildfire")
            .AddAssociatedActions(AID.WildfirePvP);

        res.Define(Track.BishopAutoturret).As<CommonStrategy>("Bishop Autoturret", "", 300)
            .AddOption(CommonStrategy.Allow, "Use Bishop Autoturret on target's location when available")
            .AddOption(CommonStrategy.Forbid, "Do not use Bishop Autoturret")
            .AddAssociatedActions(AID.BishopAutoturretPvP);

        res.Define(Track.TurretPlacement).As<TurretPlacement>("Turret Placement", "", 300)
            .AddOption(TurretPlacement.Self, "Place turret on self")
            .AddOption(TurretPlacement.Target, "Place turret on target's location")
            .AddOption(TurretPlacement.Crystal, "Place turret on crystal only; will hold turret until near the crystal (only works for Crystalline Conflict)")
            .AddOption(TurretPlacement.CrystalOrTarget, "Place turret on crystal or target if crystal is unavailable (intended for Crystalline Conflict, but works in others too)");

        return res;
    }

    private static readonly uint[] SpiteBlockers = [3039, 1302, 1301, 1300, 1978, 1240, (uint)ClassShared.SID.GuardPvP];

    private AID BestTool => HasStatus(SID.ChainSawPrimed) ? AID.ChainSawPvP : HasStatus(SID.AirAnchorPrimed) ? AID.AirAnchorPvP : HasStatus(SID.BioblasterPrimed) ? AID.BioblasterPvP : AID.DrillPvP;
    private bool Overheated => HasStatus(SID.OverheatedPvP);

    private bool AutoTarget;
    private Actor? ConeTarget;

    private bool IsReady(AID aid) => Cooldown(aid) <= 0.2f;
    private bool ToolSoon(SID primed, AID tool) => HasStatus(primed) && Cooldown(tool) <= 11f;
    private Actor? Retarget(Actor? target) => AutoTarget ? target : PrimaryTarget;
    private bool CanSpite(Actor target)
    {
        var blocked = false;
        foreach (var status in SpiteBlockers)
            blocked |= target.FindStatus(status) != null;
        return !blocked && target.NameID is 0 or 541 && DistanceFrom(target, 50);
    }
    private Actor? FirstLivingTarget()
    {
        Actor? found = null;
        foreach (var enemy in Hints.PotentialTargets)
            if (found == null && !enemy.Actor.IsDeadOrDestroyed)
                found = enemy.Actor;
        return found;
    }

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || IsGuarded(Player))
            return;

        var targeting = strategy.Option(Track.Targeting).As<TargetingStrategy>();
        AutoTarget = targeting == TargetingStrategy.Auto;
        ConeTarget = Retarget(GetBestTarget(primaryTarget, 12, Is12yConeTarget).Best?.Actor);
        if (targeting != TargetingStrategy.Manual)
            GetPvPTarget(5, targeting == TargetingStrategy.FocusTargetsTarget);
        if (HasLOS(PrimaryTarget))
        {
            QueueLimitBreak(strategy);
            if (In25y(PrimaryTarget) && Overheated)
            {
                QueueWildfire(strategy);
                QueueGCD(AID.BlazingShotPvP, PrimaryTarget, GCDPriority.Low);
            }
            if (In25y(PrimaryTarget) && !Overheated)
            {
                QueueRoleAction(strategy);
                QueueFullMetalField(strategy, primaryTarget);
                QueueTurret(strategy);
                QueueAnalysis(strategy);
                QueueScattergun(strategy);
                QueueTools(strategy, primaryTarget);
                QueueGCD(AID.BlastChargePvP, PrimaryTarget, GCDPriority.Low);
            }
        }
    }

    private void QueueLimitBreak(StrategyValues strategy)
    {
        var hp = PrimaryTarget?.PendingHPRatio;
        var condition = strategy.Option(Track.LimitBreak).As<LBStrategy>() switch
        {
            LBStrategy.ASAP => true,
            LBStrategy.LessThan70 => hp < 0.7f,
            LBStrategy.LessThan60 => hp < 0.6f,
            LBStrategy.LessThan50 => hp < 0.5f,
            LBStrategy.LessThan40 => hp < 0.4f,
            _ => false
        };
        if (condition && World.Party.LimitBreakLevel >= 1 && PrimaryTarget != null && CanSpite(PrimaryTarget))
            QueueGCD(AID.MarksmansSpitePvP, PrimaryTarget, GCDPriority.Max);
    }
    private void QueueWildfire(StrategyValues strategy)
    {
        if (IsReady(AID.WildfirePvP) && PrimaryTarget != null && strategy.Option(Track.Wildfire).As<CommonStrategy>() == CommonStrategy.Allow)
            QueueGCD(AID.WildfirePvP, PrimaryTarget, GCDPriority.High + 2);
    }
    private void QueueRoleAction(StrategyValues strategy)
    {
        var (condition, action, target) = strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
        {
            RoleActionStrategy.Dervish => (HasStatus(SID.DervishEquippedPvP), AID.DervishPvP, Player),
            RoleActionStrategy.Bravery => (HasStatus(SID.BraveryEquippedPvP), AID.BraveryPvP, Player),
            RoleActionStrategy.EagleEyeShot => (HasStatus(SID.EagleEyeShotEquippedPvP), AID.EagleEyeShotPvP, Retarget(FirstLivingTarget())),
            _ => (false, AID.None, null)
        };
        if (condition && IsReady(action))
            QueueGCD(action, target, GCDPriority.VeryHigh + 1);
    }
    private void QueueFullMetalField(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (IsReady(AID.FullMetalFieldPvP) && PrimaryTarget != null && strategy.Option(Track.FullMetalField).As<CommonStrategy>() == CommonStrategy.Allow)
            QueueGCD(AID.FullMetalFieldPvP, Retarget(GetBestTarget(primaryTarget, 25, IsSplashTarget).Best?.Actor), GCDPriority.High + 5);
    }
    private void QueueTurret(StrategyValues strategy)
    {
        var crystal = FindActor(0x3886);
        var target = strategy.Option(Track.TurretPlacement).As<TurretPlacement>() switch
        {
            TurretPlacement.Self => Player,
            TurretPlacement.Target => PrimaryTarget,
            TurretPlacement.Crystal => crystal,
            TurretPlacement.CrystalOrTarget => crystal ?? PrimaryTarget,
            _ => null
        };
        if (IsReady(AID.BishopAutoturretPvP) && PrimaryTarget != null && strategy.Option(Track.BishopAutoturret).As<CommonStrategy>() == CommonStrategy.Allow)
            QueueGCD(AID.BishopAutoturretPvP, target, GCDPriority.High + 4);
    }
    private void QueueAnalysis(StrategyValues strategy)
    {
        var drill = ToolSoon(SID.DrillPrimed, AID.DrillPvP);
        var anchor = ToolSoon(SID.AirAnchorPrimed, AID.AirAnchorPvP);
        var bioblaster = ToolSoon(SID.BioblasterPrimed, AID.BioblasterPvP);
        var saw = ToolSoon(SID.ChainSawPrimed, AID.ChainSawPvP);
        var condition = strategy.Option(Track.Analysis).As<AnalysisStrategy>() switch
        {
            AnalysisStrategy.Any => drill || anchor || bioblaster || saw,
            AnalysisStrategy.DrillAA => drill || anchor,
            AnalysisStrategy.BBCS => bioblaster || saw,
            _ => false
        };
        if (condition && PrimaryTarget != null && !HasStatus(SID.AnalysisPvP) && Cooldown(AID.AnalysisPvP) <= 20.6f)
            QueueGCD(AID.AnalysisPvP, Player, GCDPriority.High + 3);
    }
    private void QueueScattergun(StrategyValues strategy)
    {
        if (IsReady(AID.ScattergunPvP) && In12y(PrimaryTarget) && strategy.Option(Track.Scattergun).As<CommonStrategy>() == CommonStrategy.Allow)
            QueueGCD(AID.ScattergunPvP, ConeTarget, GCDPriority.High + 1);
    }
    private void QueueTools(StrategyValues strategy, Enemy? primaryTarget)
    {
        var target = HasStatus(SID.ChainSawPrimed) ? Retarget(GetBestTarget(primaryTarget, 25, Is25yRectTarget).Best?.Actor)
            : HasStatus(SID.BioblasterPrimed) ? ConeTarget
            : PrimaryTarget;
        if (Cooldown(BestTool) <= 10.2f && strategy.Option(Track.Tools).As<ToolsStrategy>() != ToolsStrategy.Forbid)
            QueueGCD(BestTool, target, GCDPriority.Average);
    }
}
