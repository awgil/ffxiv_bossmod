using BossMod.MCH;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiMCHPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, Analysis, Tools, Scattergun, FullMetalField, Wildfire, BishopAutoturret, TurretPlacement }
    public enum TargetingStrategy { Auto, Manual }
    public enum RoleActionStrategy { Forbid, Dervish, Bravery, EagleEyeShot }
    public enum LBStrategy { ASAP, LessThan70, LessThan60, LessThan50, LessThan40, Forbid }
    public enum AnalysisStrategy { Any, DrillAA, BBCS, Forbid }
    public enum ToolsStrategy { ASAP, BuffOrOvercap, Forbid }
    public enum CommonStrategy { Allow, Forbid }
    public enum TurretPlacement { Self, Target, Crystal, CrystalOrTarget }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi MCH (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.MCH), 100, 30);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatic", "Automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manual", "Manually select target");
        res.Define(Track.RoleActions).As<RoleActionStrategy>("Role Actions", "", 300)
            .AddOption(RoleActionStrategy.Forbid, "Forbid", "Do not use any role actions")
            .AddOption(RoleActionStrategy.Dervish, "Dervish", "Use Dervish when available")
            .AddOption(RoleActionStrategy.Bravery, "Bravery", "Use Bravery when available")
            .AddOption(RoleActionStrategy.EagleEyeShot, "Eagle Eye Shot", "Use Eagle Eye Shot when available")
            .AddAssociatedActions(AID.DervishPvP, AID.BraveryPvP, AID.EagleEyeShotPvP);
        res.Define(Track.LimitBreak).As<LBStrategy>("Limit Break", "", 300)
            .AddOption(LBStrategy.ASAP, "ASAP", "Use Limit Break as soon as it is available")
            .AddOption(LBStrategy.LessThan70, "Less than 70%", "Use Limit Break when target's HP is less than 70%")
            .AddOption(LBStrategy.LessThan60, "Less than 60%", "Use Limit Break when target's HP is less than 60%")
            .AddOption(LBStrategy.LessThan50, "Less than 50%", "Use Limit Break when target's HP is less than 50%")
            .AddOption(LBStrategy.LessThan40, "Less than 40%", "Use Limit Break when target's HP is less than 40%")
            .AddOption(LBStrategy.Forbid, "Forbid", "Forbid use of Limit Break entirely")
            .AddAssociatedActions(AID.MarksmansSpitePvP);
        res.Define(Track.Analysis).As<AnalysisStrategy>("Analysis", "", 300)
            .AddOption(AnalysisStrategy.Any, "Any", "Use Analysis to buff any tool")
            .AddOption(AnalysisStrategy.DrillAA, "Drill", "Use Analysis to buff Drill and Air Anchor only")
            .AddOption(AnalysisStrategy.BBCS, "Bioblaster", "Use Analysis to buff Bioblaster and ChainSaw only")
            .AddOption(AnalysisStrategy.Forbid, "Forbid", "Do not use Analysis")
            .AddAssociatedActions(AID.AnalysisPvP);
        res.Define(Track.Tools).As<ToolsStrategy>("Tools", "", 300)
            .AddOption(ToolsStrategy.ASAP, "ASAP", "Use Tools as soon as it is available")
            .AddOption(ToolsStrategy.BuffOrOvercap, "Buff or Overcap", "Use Tools when buffed by Analysis or about to overcap on charges")
            .AddOption(ToolsStrategy.Forbid, "Forbid", "Do not use Tools")
            .AddAssociatedActions(AID.DrillPvP, AID.BioblasterPvP, AID.AirAnchorPvP, AID.ChainSawPvP);
        res.Define(Track.Scattergun).As<CommonStrategy>("Scattergun", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Scattergun when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Scattergun")
            .AddAssociatedActions(AID.ScattergunPvP);
        res.Define(Track.FullMetalField).As<CommonStrategy>("Full Metal Field", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Full Metal Field when available and not Overheated")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Full Metal Field")
            .AddAssociatedActions(AID.FullMetalFieldPvP);
        res.Define(Track.Wildfire).As<CommonStrategy>("Wildfire", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Wildfire when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Wildfire")
            .AddAssociatedActions(AID.WildfirePvP);
        res.Define(Track.BishopAutoturret).As<CommonStrategy>("Bishop Autoturret", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Bishop Autoturret on target's location when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Bishop Autoturret")
            .AddAssociatedActions(AID.BishopAutoturretPvP);
        res.Define(Track.TurretPlacement).As<TurretPlacement>("Turret Placement", "", 300)
            .AddOption(TurretPlacement.Self, "Self", "Place turret on self")
            .AddOption(TurretPlacement.Target, "Target", "Place turret on target's location")
            .AddOption(TurretPlacement.Crystal, "Crystal", "Place turret on crystal only; will hold turret until near the crystal (only works for Crystalline Conflict)")
            .AddOption(TurretPlacement.CrystalOrTarget, "Crystal or Target", "Place turret on crystal or target if crystal is unavailable (intended for Crystalline Conflict, but works in others too)");
        return res;
    }

    private int NumConeTargets;
    private int NumLineTargets;
    private int NumSplashTargets;
    private Enemy? BestConeTargets;
    private Enemy? BestLineTargets;
    private Enemy? BestSplashTargets;

    private float AnalyzeLeft => StatusRemaining(Player, SID.AnalysisPvP);
    private bool LBready => World.Party.LimitBreakLevel >= 1;
    private AID BestTool => HasEffect(SID.ChainSawPrimed) ? AID.ChainSawPvP : HasEffect(SID.AirAnchorPrimed) ? AID.AirAnchorPvP : HasEffect(SID.BioblasterPrimed) ? AID.BioblasterPvP : AID.DrillPvP;
    private bool IsReady(AID aid) => CDRemaining(aid) <= 0.2f;
    private bool ShouldAnalyze(StrategyValues strategy) => AnalyzeLeft == 0 && CDRemaining(AID.AnalysisPvP) <= 20.6f && strategy.Option(Track.Analysis).As<AnalysisStrategy>() switch
    {
        AnalysisStrategy.Any => (HasEffect(SID.DrillPrimed) && CDRemaining(AID.DrillPvP) <= 11f) || (HasEffect(SID.BioblasterPrimed) && CDRemaining(AID.BioblasterPvP) <= 11f) || (HasEffect(SID.AirAnchorPrimed) && CDRemaining(AID.AirAnchorPvP) <= 11f) || (HasEffect(SID.ChainSawPrimed) && CDRemaining(AID.ChainSawPvP) <= 11f),
        AnalysisStrategy.DrillAA => (HasEffect(SID.DrillPrimed) && CDRemaining(AID.DrillPvP) <= 11f) || (HasEffect(SID.AirAnchorPrimed) && CDRemaining(AID.AirAnchorPvP) <= 11f),
        AnalysisStrategy.BBCS => (HasEffect(SID.BioblasterPrimed) && CDRemaining(AID.BioblasterPvP) <= 11f) || (HasEffect(SID.ChainSawPrimed) && CDRemaining(AID.ChainSawPvP) <= 11f),
        _ => false
    };
    private bool ShouldUseLB(StrategyValues strategy, Actor? target) => LBready && target != null &&
        target.NameID is 0 or 541 && //guaranteed enemy player (or striking dummy)
        target.FindStatus(3039) == null && //no DRK invuln active - no point in using if invulnerable
        target.FindStatus(1302) == null && //no PLD invuln active - no point in using if invulnerable
        target.FindStatus(1301) == null && target.FindStatus(1300) == null && //no PLD Cover active - no point in using if resistant
        target.FindStatus(1978) == null && //no Rampart active - no point in using if resistant
        target.FindStatus(1240) == null && //no SAM buff active - using this results in us receiving a debuff if they just so happen to survive
        target.FindStatus(ClassShared.SID.GuardPvP) == null && //no Guard active - no point in using if resistant
        strategy.Option(Track.LimitBreak).As<LBStrategy>() switch
        {
            LBStrategy.ASAP => true,
            LBStrategy.LessThan70 => HPP(target) <= 70,
            LBStrategy.LessThan60 => HPP(target) <= 60,
            LBStrategy.LessThan50 => HPP(target) <= 50,
            LBStrategy.LessThan40 => HPP(target) <= 40,
            LBStrategy.Forbid => false,
            _ => false
        };

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || Player.FindStatus(ClassShared.SID.GuardPvP) != null)
            return;

        (BestConeTargets, NumConeTargets) = GetBestTarget(primaryTarget, 12, Is12yConeTarget);
        (BestLineTargets, NumLineTargets) = GetBestTarget(primaryTarget, 25, Is25yRectTarget);
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        var BestConeTarget = NumConeTargets > 1 ? BestConeTargets : primaryTarget;
        var BestLineTarget = NumLineTargets > 1 ? BestLineTargets : primaryTarget;
        var BestSplashTarget = NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        var BestTarget = HasEffect(SID.ChainSawPrimed) ? BestLineTarget : HasEffect(SID.Bioblaster) ? BestConeTarget : primaryTarget;
        var auto = strategy.Option(Track.Targeting).As<TargetingStrategy>() == TargetingStrategy.Auto;
        if (auto)
        {
            GetPvPTarget(25);
        }
        if (In25y(primaryTarget?.Actor) && HasLOS(primaryTarget?.Actor))
        {
            if (HasEffect(SID.OverheatedPvP))
            {
                if (IsReady(AID.WildfirePvP) && primaryTarget?.Actor != null &&
                    strategy.Option(Track.Wildfire).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.WildfirePvP, primaryTarget?.Actor, GCDPriority.High + 2);

                QueueGCD(AID.BlazingShotPvP, primaryTarget?.Actor, GCDPriority.Low);
            }
            if (!HasEffect(SID.OverheatedPvP))
            {
                if (strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
                {
                    RoleActionStrategy.Dervish => HasEffect(SID.DervishEquippedPvP) && IsReady(AID.DervishPvP),
                    RoleActionStrategy.Bravery => HasEffect(SID.BraveryEquippedPvP) && IsReady(AID.BraveryPvP),
                    RoleActionStrategy.EagleEyeShot => HasEffect(SID.EagleEyeShotEquippedPvP) && IsReady(AID.EagleEyeShotPvP),
                    _ => false
                })
                    QueueGCD(strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
                    {
                        RoleActionStrategy.Dervish => AID.DervishPvP,
                        RoleActionStrategy.Bravery => AID.BraveryPvP,
                        RoleActionStrategy.EagleEyeShot => AID.EagleEyeShotPvP,
                        _ => AID.None
                    },
                    strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
                    {
                        RoleActionStrategy.Dervish => Player,
                        RoleActionStrategy.Bravery => Player,
                        RoleActionStrategy.EagleEyeShot => primaryTarget?.Actor,
                        _ => null
                    }, GCDPriority.VeryHigh + 1);
                if (ShouldUseLB(strategy, primaryTarget?.Actor))
                    QueueGCD(AID.MarksmansSpitePvP, primaryTarget?.Actor, GCDPriority.VeryHigh);
                if (IsReady(AID.FullMetalFieldPvP) && !HasEffect(SID.OverheatedPvP) && primaryTarget?.Actor != null &&
                    strategy.Option(Track.FullMetalField).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.FullMetalFieldPvP, auto ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.High + 5);
                var crystal = World.Actors.FirstOrDefault(x => x.OID == 0x3886); //crystal
                if (IsReady(AID.BishopAutoturretPvP) && primaryTarget?.Actor != null &&
                    strategy.Option(Track.BishopAutoturret).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.BishopAutoturretPvP, strategy.Option(Track.TurretPlacement).As<TurretPlacement>() switch
                    {
                        TurretPlacement.Self => Player,
                        TurretPlacement.Target => primaryTarget?.Actor,
                        TurretPlacement.Crystal => crystal,
                        TurretPlacement.CrystalOrTarget => crystal ?? primaryTarget?.Actor,
                        _ => null
                    }, GCDPriority.High + 4);
                if (ShouldAnalyze(strategy) && primaryTarget?.Actor != null)
                    QueueGCD(AID.AnalysisPvP, Player, GCDPriority.High + 3);
                if (IsReady(AID.ScattergunPvP) && In12y(primaryTarget?.Actor) && primaryTarget?.Actor != null &&
                    strategy.Option(Track.Scattergun).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.ScattergunPvP, auto ? BestConeTarget?.Actor : primaryTarget?.Actor, GCDPriority.High + 1);
                if (CDRemaining(BestTool) <= 10.2f && strategy.Option(Track.Tools).As<ToolsStrategy>() != ToolsStrategy.Forbid)
                    QueueGCD(BestTool, auto ? BestTarget?.Actor : primaryTarget?.Actor, GCDPriority.Average);

                QueueGCD(AID.BlastChargePvP, primaryTarget?.Actor, GCDPriority.Low);
            }
        }
    }
}
