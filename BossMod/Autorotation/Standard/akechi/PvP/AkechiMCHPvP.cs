using BossMod.MCH;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiMCHPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, Analysis, Drill, AirAnchor, ChainSaw, Bioblaster, Scattergun, FullMetalField, Wildfire, BishopAutoturret }
    public enum TargetingStrategy { Auto, Manual }
    public enum RoleActionStrategy { Forbid, Dervish, Bravery, EagleEyeShot }
    public enum LBStrategy { ASAP, LessThan70, LessThan60, LessThan50, LessThan40, Forbid }
    public enum AnalysisStrategy { Any, DrillAA, BBCS, Forbid }
    public enum BuffedStrategy { ASAP, BuffOrOvercap, Forbid }
    public enum CommonStrategy { Allow, Forbid }

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
        res.Define(Track.Drill).As<BuffedStrategy>("Drill", "", 300)
            .AddOption(BuffedStrategy.ASAP, "ASAP", "Use Drill as soon as it is available")
            .AddOption(BuffedStrategy.BuffOrOvercap, "Buff or Overcap", "Use Drill when buffed by Analysis or about to overcap on charges")
            .AddOption(BuffedStrategy.Forbid, "Forbid", "Do not use Drill")
            .AddAssociatedActions(AID.DrillPvP);
        res.Define(Track.AirAnchor).As<BuffedStrategy>("Air Anchor", "", 300)
            .AddOption(BuffedStrategy.ASAP, "ASAP", "Use Air Anchor as soon as it is available")
            .AddOption(BuffedStrategy.BuffOrOvercap, "Buff or Overcap", "Use Air Anchor when buffed by Analysis or about to overcap on charges")
            .AddOption(BuffedStrategy.Forbid, "Forbid", "Do not use Air Anchor")
            .AddAssociatedActions(AID.AirAnchorPvP);
        res.Define(Track.ChainSaw).As<BuffedStrategy>("Chain Saw", "", 300)
            .AddOption(BuffedStrategy.ASAP, "ASAP", "Use Chain Saw as soon as it is available")
            .AddOption(BuffedStrategy.BuffOrOvercap, "Buff or Overcap", "Use Chain Saw when buffed by Analysis or about to overcap on charges")
            .AddOption(BuffedStrategy.Forbid, "Forbid", "Do not use Chain Saw")
            .AddAssociatedActions(AID.ChainSawPvP);
        res.Define(Track.Bioblaster).As<BuffedStrategy>("Bioblaster", "", 300)
            .AddOption(BuffedStrategy.ASAP, "ASAP", "Use Bioblaster as soon as it is available")
            .AddOption(BuffedStrategy.BuffOrOvercap, "Buff or Overcap", "Use Bioblaster when buffed by Analysis or about to overcap on charges")
            .AddOption(BuffedStrategy.Forbid, "Forbid", "Do not use Bioblaster")
            .AddAssociatedActions(AID.BioblasterPvP);
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
        return res;
    }

    private bool ShouldAnalyze(StrategyValues strategy) => AnalyzeLeft == 0 && CDRemaining(AID.AnalysisPvP) <= 20.6f && strategy.Option(Track.Analysis).As<AnalysisStrategy>() switch
    {
        AnalysisStrategy.Any => (HasEffect(SID.DrillPrimed) && CDRemaining(AID.DrillPvP) <= 11f) || (HasEffect(SID.BioblasterPrimed) && CDRemaining(AID.BioblasterPvP) <= 11f) || (HasEffect(SID.AirAnchorPrimed) && CDRemaining(AID.AirAnchorPvP) <= 11f) || (HasEffect(SID.ChainSawPrimed) && CDRemaining(AID.ChainSawPvP) <= 11f),
        AnalysisStrategy.DrillAA => (HasEffect(SID.DrillPrimed) && CDRemaining(AID.DrillPvP) <= 11f) || (HasEffect(SID.AirAnchorPrimed) && CDRemaining(AID.AirAnchorPvP) <= 11f),
        AnalysisStrategy.BBCS => (HasEffect(SID.BioblasterPrimed) && CDRemaining(AID.BioblasterPvP) <= 11f) || (HasEffect(SID.ChainSawPrimed) && CDRemaining(AID.ChainSawPvP) <= 11f),
        _ => false
    };
    private bool ShouldUseLB(StrategyValues strategy, Actor? target) => LBready && strategy.Option(Track.LimitBreak).As<LBStrategy>() switch
    {
        LBStrategy.ASAP => true,
        LBStrategy.LessThan70 => TargetHPP(target) < 70,
        LBStrategy.LessThan60 => TargetHPP(target) < 60,
        LBStrategy.LessThan50 => TargetHPP(target) < 50,
        LBStrategy.LessThan40 => TargetHPP(target) < 40,
        LBStrategy.Forbid => false,
        _ => false
    };
    private float AnalyzeLeft => StatusRemaining(Player, SID.AnalysisPvP);
    private bool LBready => World.Party.LimitBreakLevel >= 1;
    private bool IsReady(AID aid) => CDRemaining(aid) <= 0.2f;
    private bool UseTool(StrategyValues strategy, Track index, AID aid) => strategy.Option(index).As<BuffedStrategy>() switch
    {
        BuffedStrategy.ASAP => CDRemaining(aid) <= 10.2f,
        BuffedStrategy.BuffOrOvercap => AnalyzeLeft > 0 || CDRemaining(aid) < 0.6f,
        _ => false
    };

    private int NumConeTargets;
    private int NumLineTargets;
    private int NumSplashTargets;
    private Enemy? BestConeTargets;
    private Enemy? BestLineTargets;
    private Enemy? BestSplashTargets;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        (BestConeTargets, NumConeTargets) = GetBestTarget(PlayerTarget, 12, Is12yConeTarget);
        (BestLineTargets, NumLineTargets) = GetBestTarget(PlayerTarget, 25, Is25yRectTarget);
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(PlayerTarget, 25, IsSplashTarget);
        var BestConeTarget = NumConeTargets > 1 ? BestConeTargets : PlayerTarget;
        var BestLineTarget = NumLineTargets > 1 ? BestLineTargets : PlayerTarget;
        var BestSplashTarget = NumSplashTargets > 1 ? BestSplashTargets : PlayerTarget;
        var auto = strategy.Option(Track.Targeting).As<TargetingStrategy>() == TargetingStrategy.Auto;
        if (auto)
        {
            GetPvPTarget(25);
        }
        if (In25y(PlayerTarget?.Actor))
        {
            if (HasEffect(SID.OverheatedPvP))
            {
                if (IsReady(AID.WildfirePvP) && PlayerTarget?.Actor != null &&
                    strategy.Option(Track.Wildfire).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.WildfirePvP, PlayerTarget?.Actor, GCDPriority.High + 2);

                QueueGCD(AID.BlazingShotPvP, PlayerTarget?.Actor, GCDPriority.Low);
            }
            if (!HasEffect(SID.OverheatedPvP))
            {
                if (strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
                {
                    RoleActionStrategy.Dervish => IsReady(AID.DervishPvP),
                    RoleActionStrategy.Bravery => IsReady(AID.BraveryPvP),
                    RoleActionStrategy.EagleEyeShot => IsReady(AID.EagleEyeShotPvP),
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
                        RoleActionStrategy.EagleEyeShot => PlayerTarget?.Actor,
                        _ => null
                    }, GCDPriority.VeryHigh + 1);
                if (ShouldUseLB(strategy, PlayerTarget?.Actor))
                    QueueGCD(AID.MarksmansSpitePvP, PlayerTarget?.Actor, GCDPriority.VeryHigh);
                if (IsReady(AID.FullMetalFieldPvP) && !HasEffect(SID.OverheatedPvP) && PlayerTarget?.Actor != null &&
                    strategy.Option(Track.FullMetalField).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.FullMetalFieldPvP, auto ? BestSplashTarget?.Actor : PlayerTarget?.Actor, GCDPriority.High + 5);
                if (IsReady(AID.BishopAutoturretPvP) && PlayerTarget?.Actor != null &&
                    strategy.Option(Track.BishopAutoturret).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.BishopAutoturretPvP, auto ? BestSplashTarget?.Actor : PlayerTarget?.Actor, GCDPriority.High + 4);
                if (ShouldAnalyze(strategy) && PlayerTarget?.Actor != null)
                    QueueGCD(AID.AnalysisPvP, Player, GCDPriority.High + 3);
                if (IsReady(AID.ScattergunPvP) && In12y(PlayerTarget?.Actor) && PlayerTarget?.Actor != null &&
                    strategy.Option(Track.Scattergun).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.ScattergunPvP, auto ? BestConeTarget?.Actor : PlayerTarget?.Actor, GCDPriority.High + 1);
                if (CDRemaining(AID.DrillPvP) <= 10.2f && HasEffect(SID.DrillPrimed) &&
                    UseTool(strategy, Track.Drill, AID.DrillPvP))
                    QueueGCD(AID.DrillPvP, PlayerTarget?.Actor, GCDPriority.Average);
                if (CDRemaining(AID.BioblasterPvP) <= 10.2f && HasEffect(SID.BioblasterPrimed) &&
                    UseTool(strategy, Track.Bioblaster, AID.BioblasterPvP))
                    QueueGCD(AID.BioblasterPvP, auto ? BestConeTarget?.Actor : PlayerTarget?.Actor, GCDPriority.Average);
                if (CDRemaining(AID.AirAnchorPvP) <= 10.2f && HasEffect(SID.AirAnchorPrimed) &&
                    UseTool(strategy, Track.AirAnchor, AID.AirAnchorPvP))
                    QueueGCD(AID.AirAnchorPvP, PlayerTarget?.Actor, GCDPriority.Average);
                if (CDRemaining(AID.ChainSawPvP) <= 10.2f && HasEffect(SID.ChainSawPrimed) &&
                    UseTool(strategy, Track.ChainSaw, AID.ChainSawPvP))
                    QueueGCD(AID.ChainSawPvP, auto ? BestLineTarget?.Actor : PlayerTarget?.Actor, GCDPriority.Average);

                QueueGCD(AID.BlastChargePvP, PlayerTarget?.Actor, GCDPriority.Low);
            }
        }
    }
}
