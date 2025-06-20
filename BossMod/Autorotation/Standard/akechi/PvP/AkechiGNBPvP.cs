using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiGNBPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, LimitBreak, TerminalTrigger, Corundum, CDs, RoughDivide, Zone }
    public enum TargetingStrategy { Auto, Manual }
    public enum TriggerStrategy { Five, Four, Three, Two, One, Forbid }
    public enum CorundumStrategy { Eighty, Seventy, Sixty, Fifty, Fourty, Thirty, Forbid }
    public enum DivideStrategy { Auto, AutoMelee, Forbid }
    public enum ZoneStrategy { Auto, AutoBuffOnly, HalfHPP, HalfHPPBuffOnly, Forbid }
    public enum CDsStrategy { Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.GNB), 100, 30);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatic", "Automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manual", "Manually select target");
        res.Define(Track.LimitBreak).As<CDsStrategy>("Limit Break", "", 300)
            .AddOption(CDsStrategy.Allow, "Allow", "Allow use of Limit Break automatically when available")
            .AddOption(CDsStrategy.Forbid, "Forbid", "Forbid use of Limit Break entirely")
            .AddAssociatedActions(AID.RelentlessRushPvP);
        res.Define(Track.TerminalTrigger).As<TriggerStrategy>("Terminal Trigger", "", 300)
            .AddOption(TriggerStrategy.Five, "5 Stacks", "Use when 5 stacks of Relentless Shrapnel are available")
            .AddOption(TriggerStrategy.Four, "4 Stacks", "Use when 4+ stacks of Relentless Shrapnel are available")
            .AddOption(TriggerStrategy.Three, "3 Stacks", "Use when 3+ stacks of Relentless Shrapnel are available")
            .AddOption(TriggerStrategy.Two, "2 Stacks", "Use when 2+ stacks of Relentless Shrapnel are available")
            .AddOption(TriggerStrategy.One, "1 Stack", "Use when 1+ stacks of Relentless Shrapnel is available")
            .AddOption(TriggerStrategy.Forbid, "Forbid", "Let Terminal Trigger happen automatically at the end of its duration")
            .AddAssociatedActions(AID.TerminalTriggerPvP);
        res.Define(Track.Corundum).As<CorundumStrategy>("Heart of Corundum", "", 300)
            .AddOption(CorundumStrategy.Eighty, "80% HP", "Use when HP is at or below 80%")
            .AddOption(CorundumStrategy.Seventy, "70% HP", "Use when HP is at or below 70%")
            .AddOption(CorundumStrategy.Sixty, "60% HP", "Use when HP is at or below 60%")
            .AddOption(CorundumStrategy.Fifty, "50% HP", "Use when HP is at or below 50%")
            .AddOption(CorundumStrategy.Fourty, "40% HP", "Use when HP is at or below 40%")
            .AddOption(CorundumStrategy.Thirty, "30% HP", "Use when HP is at or below 30%")
            .AddOption(CorundumStrategy.Forbid, "Forbid", "Forbid use of Heart of Corundum entirely")
            .AddAssociatedActions(AID.HeartOfCorundumPvP);
        res.Define(Track.CDs).As<CDsStrategy>("Cooldowns", "", 300)
            .AddOption(CDsStrategy.Allow, "Allow", "Allow use of cooldowns automatically")
            .AddOption(CDsStrategy.Forbid, "Forbid", "Forbid use of cooldowns entirely");
        res.Define(Track.RoughDivide).As<DivideStrategy>("Rough Divide", "", 300)
            .AddOption(DivideStrategy.Auto, "Automatic", "Use automatically when No Mercy buff is not active")
            .AddOption(DivideStrategy.AutoMelee, "Automatic (Melee Range)", "Use only when in Melee range of target and when No Mercy buff is not active")
            .AddOption(DivideStrategy.Forbid, "Forbid", "Forbid use of Rough Divide entirely")
            .AddAssociatedActions(AID.RoughDividePvP);
        res.Define(Track.Zone).As<ZoneStrategy>("Blasting Zone", "", 300)
            .AddOption(ZoneStrategy.Auto, "Automatic", "Use when No Mercy buff is active or target has less than 50% HP")
            .AddOption(ZoneStrategy.AutoBuffOnly, "Automatic (No Mercy)", "Use only when under No Mercy buff; disregards 2x potency effect")
            .AddOption(ZoneStrategy.HalfHPP, "Half HPP", "Use when target is less than 50% HP; disregards No Mercy buff")
            .AddOption(ZoneStrategy.HalfHPPBuffOnly, "Half HPP (No Mercy", "Use only if under No Mercy buff and when target is less than 50% HP")
            .AddOption(ZoneStrategy.Forbid, "Forbid", "Forbid use of Blasting Zone entirely")
            .AddAssociatedActions(AID.BlastingZonePvP);
        return res;
    }

    private bool HasNM;
    private bool HasBlast;
    private bool HasRaze;
    private bool HasRip;
    private bool HasTear;
    private bool HasGouge;
    public bool LBready;
    public bool inGF;

    private AID PvPCombo => ComboLastMove switch
    {
        AID.SolidBarrelPvP => AID.BurstStrikePvP,
        AID.BrutalShellPvP => AID.SolidBarrelPvP,
        AID.KeenEdgePvP => AID.BrutalShellPvP,
        _ => AID.KeenEdgePvP,
    };
    private bool ShouldUseZone(StrategyValues strategy, Actor? target) => target != null && In5y(target) && ActionReady(AID.BlastingZonePvP) && strategy.Option(Track.Zone).As<ZoneStrategy>() switch
    {
        ZoneStrategy.Auto => HasNM || TargetHPP(target) < 50,
        ZoneStrategy.AutoBuffOnly => HasNM,
        ZoneStrategy.HalfHPP => TargetHPP(target) < 50,
        ZoneStrategy.HalfHPPBuffOnly => HasNM && TargetHPP(target) < 50,
        _ => false
    };
    private bool ShouldUseRoughDivide(StrategyValues strategy, Actor? target) => target != null && CDRemaining(AID.RoughDividePvP) < 14.6f && strategy.Option(Track.RoughDivide).As<DivideStrategy>() switch
    {
        DivideStrategy.Auto => !HasNM,
        DivideStrategy.AutoMelee => !HasNM && In5y(target),
        _ => false
    };
    private bool ShouldUseHOC(StrategyValues strategy) => ActionReady(AID.HeartOfCorundumPvP) && strategy.Option(Track.Corundum).As<CorundumStrategy>() switch
    {
        CorundumStrategy.Eighty => TargetHPP(Player) is <= 80 and not 0,
        CorundumStrategy.Seventy => TargetHPP(Player) is <= 70 and not 0,
        CorundumStrategy.Sixty => TargetHPP(Player) is <= 60 and not 0,
        CorundumStrategy.Fifty => TargetHPP(Player) is <= 50 and not 0,
        CorundumStrategy.Fourty => TargetHPP(Player) is <= 40 and not 0,
        CorundumStrategy.Thirty => TargetHPP(Player) is <= 30 and not 0,
        _ => false
    };
    private bool ShouldUseLB(StrategyValues strategy, Actor? target) => LBready && Player.DistanceToHitbox(target) < 6 && Hints.NumPriorityTargetsInAOECircle(Player.Position, 6) > 0 && strategy.Option(Track.LimitBreak).As<CDsStrategy>() == CDsStrategy.Allow;
    private bool ShouldUseTT(StrategyValues strategy, Actor? target) => Player.DistanceToHitbox(target) < 6 && Hints.NumPriorityTargetsInAOECircle(Player.Position, 6) > 0 && HasEffect(SID.RelentlessRushPvP) && strategy.Option(Track.TerminalTrigger).As<TriggerStrategy>() switch
    {
        TriggerStrategy.Five => StacksRemaining(target, SID.RelentlessShrapnelPvP) >= 5,
        TriggerStrategy.Four => StacksRemaining(target, SID.RelentlessShrapnelPvP) >= 4,
        TriggerStrategy.Three => StacksRemaining(target, SID.RelentlessShrapnelPvP) >= 3,
        TriggerStrategy.Two => StacksRemaining(target, SID.RelentlessShrapnelPvP) >= 2,
        TriggerStrategy.One => StacksRemaining(target, SID.RelentlessShrapnelPvP) >= 1,
        _ => false
    };
    private OGCDPriority ContinuationPrio()
    {
        if (GCD < 0.5f)
            return OGCDPriority.SlightlyHigh + 2000;
        var i = Math.Max(0, (int)((SkSGCDLength - GCD) / 0.5f));
        var a = i * 300;
        return OGCDPriority.Low + a; //every 0.5s = +300 prio
    }

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        var GunStep = gauge.AmmoComboStep;
        HasNM = HasEffect(SID.NoMercyPvP);
        HasBlast = HasEffect(SID.ReadyToBlastPvP);
        HasRaze = HasEffect(SID.ReadyToRazePvP);
        HasRip = HasEffect(SID.ReadyToRipPvP);
        HasTear = HasEffect(SID.ReadyToTearPvP);
        HasGouge = HasEffect(SID.ReadyToGougePvP);
        LBready = World.Party.LimitBreakLevel >= 1;
        inGF = GunStep != 0;
        var hold = strategy.Option(Track.CDs).As<CDsStrategy>() == CDsStrategy.Forbid;
        if (strategy.Option(Track.Targeting).As<TargetingStrategy>() == TargetingStrategy.Auto)
        {
            GetPvPTarget(5);
        }
        if (In5y(PlayerTarget?.Actor))
        {
            if (!inGF)
                QueueGCD(PvPCombo, PlayerTarget?.Actor, GCDPriority.Low);
            if (!hold)
            {
                if (ShouldUseLB(strategy, PlayerTarget?.Actor))
                    QueueOGCD(AID.RelentlessRushPvP, Player, GCDPriority.VeryHigh);
                if (ShouldUseZone(strategy, PlayerTarget?.Actor))
                    QueueOGCD(AID.BlastingZonePvP, PlayerTarget?.Actor, OGCDPriority.High - 1);
                if (HasNM)
                {
                    if (ActionReady(AID.GnashingFangPvP))
                        QueueGCD(AID.GnashingFangPvP, PlayerTarget?.Actor, GCDPriority.High);
                    if (ActionReady(AID.FatedCirclePvP))
                        QueueGCD(AID.FatedCirclePvP, PlayerTarget?.Actor, GCDPriority.Average);
                }
            }
            if (GunStep == 1)
                QueueGCD(AID.SavageClawPvP, PlayerTarget?.Actor, GCDPriority.BelowAverage);
            if (GunStep == 2)
                QueueGCD(AID.WickedTalonPvP, PlayerTarget?.Actor, GCDPriority.BelowAverage);
            if (HasRaze)
                QueueOGCD(AID.FatedBrandPvP, PlayerTarget?.Actor, ContinuationPrio());
            if (HasBlast)
                QueueOGCD(AID.HypervelocityPvP, PlayerTarget?.Actor, ContinuationPrio());
            if (HasRip)
                QueueOGCD(AID.JugularRipPvP, PlayerTarget?.Actor, ContinuationPrio());
            if (HasTear)
                QueueOGCD(AID.AbdomenTearPvP, PlayerTarget?.Actor, ContinuationPrio());
            if (HasGouge)
                QueueOGCD(AID.EyeGougePvP, PlayerTarget?.Actor, ContinuationPrio());
        }
        if (!hold && ShouldUseRoughDivide(strategy, PlayerTarget?.Actor))
            QueueOGCD(AID.RoughDividePvP, PlayerTarget?.Actor, CDRemaining(AID.RoughDividePvP) < 0.6f ? OGCDPriority.High + 2001 : OGCDPriority.High);
        if (ShouldUseHOC(strategy))
            QueueGCD(AID.HeartOfCorundumPvP, Player, GCDPriority.High);
        if (ShouldUseTT(strategy, PlayerTarget?.Actor))
            QueueGCD(AID.TerminalTriggerPvP, Player, GCDPriority.High);
    }
}
