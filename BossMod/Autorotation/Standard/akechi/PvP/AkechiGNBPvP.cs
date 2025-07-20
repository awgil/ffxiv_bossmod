using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiGNBPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, TerminalTrigger, Corundum, RoughDivide, Zone, GnashingFang, FatedCircle }
    public enum TargetingStrategy { Auto, Manual }
    public enum RoleActionStrategy { Forbid, Rampage, Rampart, FullSwing }
    public enum LBStrategy { Allow, Forbid }
    public enum TriggerStrategy { Five, Four, Three, Two, One, Forbid }
    public enum CorundumStrategy { Auto, Two, Three, Four, Eighty, Seventy, Sixty, Fifty, Fourty, Thirty, Forbid }
    public enum DivideStrategy { Auto, AutoMelee, Forbid }
    public enum ZoneStrategy { Buff, HalfHPP, BuffOrHalfHPP, BuffAndHalfHPP, ASAP, Forbid }
    public enum CommonStrategy { Buff, ASAP, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.GNB), 100, 30);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatic", "Automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manual", "Manually select target");
        res.Define(Track.RoleActions).As<RoleActionStrategy>("Role Actions", "", 300)
            .AddOption(RoleActionStrategy.Forbid, "Forbid", "Do not use any role actions")
            .AddOption(RoleActionStrategy.Rampage, "Rampage", "Use Rampage when available and targets are nearby")
            .AddOption(RoleActionStrategy.Rampart, "Rampart", "Use Rampart when available")
            .AddOption(RoleActionStrategy.FullSwing, "Full Swing", "Use Full Swing when available")
            .AddAssociatedActions(AID.RampagePvP, AID.RampartPvP, AID.FullSwingPvP);
        res.Define(Track.LimitBreak).As<LBStrategy>("Limit Break", "", 300)
            .AddOption(LBStrategy.Allow, "Allow", "Allow use of Limit Break (Relentless Rush) automatically when nearby enemies")
            .AddOption(LBStrategy.Forbid, "Forbid", "Forbid use of Limit Break (Relentless Rush) entirely");
        res.Define(Track.TerminalTrigger).As<TriggerStrategy>("Terminal Trigger", "", 300)
            .AddOption(TriggerStrategy.Five, "5 Stacks", "Use when 5 stacks of Relentless Shrapnel are available on target")
            .AddOption(TriggerStrategy.Four, "4 Stacks", "Use when 4+ stacks of Relentless Shrapnel are available on target")
            .AddOption(TriggerStrategy.Three, "3 Stacks", "Use when 3+ stacks of Relentless Shrapnel are available on target")
            .AddOption(TriggerStrategy.Two, "2 Stacks", "Use when 2+ stacks of Relentless Shrapnel are available on target")
            .AddOption(TriggerStrategy.One, "1 Stack", "Use when 1+ stacks of Relentless Shrapnel is available on target")
            .AddOption(TriggerStrategy.Forbid, "Forbid", "Let Terminal Trigger happen automatically at the end of its duration")
            .AddAssociatedActions(AID.TerminalTriggerPvP);
        res.Define(Track.Corundum).As<CorundumStrategy>("Heart of Corundum", "", 300)
            .AddOption(CorundumStrategy.Auto, "Automatic", "Use automatically when HP is below 80% or two or more enemies are targeting you")
            .AddOption(CorundumStrategy.Two, "2 Targets", "Use when HP is not full and two or more enemies are targeting you")
            .AddOption(CorundumStrategy.Three, "3 Targets", "Use when HP is not full and three or more enemies are targeting you")
            .AddOption(CorundumStrategy.Four, "4 Targets", "Use when HP is not full and four or more enemies are targeting you")
            .AddOption(CorundumStrategy.Eighty, "80% HP", "Use when HP is at or below 80%")
            .AddOption(CorundumStrategy.Seventy, "70% HP", "Use when HP is at or below 70%")
            .AddOption(CorundumStrategy.Sixty, "60% HP", "Use when HP is at or below 60%")
            .AddOption(CorundumStrategy.Fifty, "50% HP", "Use when HP is at or below 50%")
            .AddOption(CorundumStrategy.Fourty, "40% HP", "Use when HP is at or below 40%")
            .AddOption(CorundumStrategy.Thirty, "30% HP", "Use when HP is at or below 30%")
            .AddOption(CorundumStrategy.Forbid, "Forbid", "Forbid use of Heart of Corundum entirely")
            .AddAssociatedActions(AID.HeartOfCorundumPvP);
        res.Define(Track.RoughDivide).As<DivideStrategy>("Rough Divide", "", 300)
            .AddOption(DivideStrategy.Auto, "Automatic", "Use automatically when No Mercy buff is not active")
            .AddOption(DivideStrategy.AutoMelee, "Automatic (Melee Range)", "Use only when in Melee range of target and when No Mercy buff is not active")
            .AddOption(DivideStrategy.Forbid, "Forbid", "Forbid use of Rough Divide entirely")
            .AddAssociatedActions(AID.RoughDividePvP);
        res.Define(Track.Zone).As<ZoneStrategy>("Blasting Zone", "", 300)
            .AddOption(ZoneStrategy.Buff, "Buff", "Use only when under No Mercy buff regardless target HP%")
            .AddOption(ZoneStrategy.HalfHPP, "Half HPP", "Use when target is less than 50% HP regardless of No Mercy buff")
            .AddOption(ZoneStrategy.BuffOrHalfHPP, "Buff or Half HPP", "Use when under No Mercy buff or target is less than 50% HP")
            .AddOption(ZoneStrategy.BuffAndHalfHPP, "Buff and Half HPP", "Use when under No Mercy buff and target is less than 50% HP")
            .AddOption(ZoneStrategy.ASAP, "ASAP", "Use ASAP regardless of No Mercy buff or target HP%")
            .AddOption(ZoneStrategy.Forbid, "Forbid", "Forbid use of Blasting Zone entirely")
            .AddAssociatedActions(AID.BlastingZonePvP);
        res.Define(Track.GnashingFang).As<CommonStrategy>("Gnashing Fang", "", 300)
            .AddOption(CommonStrategy.Buff, "Buff", "Use ASAP when under No Mercy buff")
            .AddOption(CommonStrategy.ASAP, "ASAP", "Use ASAP regardless of No Mercy buff")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Forbid use of Gnashing Fang entirely")
            .AddAssociatedActions(AID.GnashingFangPvP);
        res.Define(Track.FatedCircle).As<CommonStrategy>("Fated Circle", "", 300)
            .AddOption(CommonStrategy.Buff, "Buff", "Use ASAP when under No Mercy buff")
            .AddOption(CommonStrategy.ASAP, "ASAP", "Use ASAP regardless of No Mercy buff")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Forbid use of Fated Circle entirely")
            .AddAssociatedActions(AID.FatedCirclePvP);
        return res;
    }

    private OGCDPriority ContinuationPrio()
    {
        if (GCD < 0.5f)
            return OGCDPriority.SlightlyHigh + 2000;
        var i = Math.Max(0, (int)((SkSGCDLength - GCD) / 0.5f));
        var a = i * 300;
        return OGCDPriority.Low + a; //every 0.5s = +300 prio
    }
    private bool TargetsNearby(float range) => Hints.PriorityTargets.Any(h =>
        !h.Actor.IsDeadOrDestroyed &&
        !h.Actor.IsFriendlyNPC &&
        !h.Actor.IsAlly &&
        h.Actor.DistanceToHitbox(Player) <= range);

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || Player.FindStatus(ClassShared.SID.GuardPvP) != null)
            return;

        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        var GunStep = gauge.AmmoComboStep;
        var hasNM = HasEffect(SID.NoMercyPvP);
        var targetsOk = Hints.NumPriorityTargetsInAOECircle(Player.Position, 6) > 0;
        var rangeOk = Player.DistanceToHitbox(primaryTarget?.Actor) <= 5.99f;
        var role = strategy.Option(Track.RoleActions).As<RoleActionStrategy>();
        if (strategy.Option(Track.Targeting).As<TargetingStrategy>() == TargetingStrategy.Auto)
        {
            GetPvPTarget(5);
        }
        if (ActionReady(AID.HeartOfCorundumPvP) && strategy.Option(Track.Corundum).As<CorundumStrategy>() switch
        {
            CorundumStrategy.Auto => (PlayerHPP is <= 80 and not 0 && EnemiesTargetingSelf(2)) || PlayerHPP is < 50 and not 0,
            CorundumStrategy.Two => PlayerHPP is < 100 and not 0 && EnemiesTargetingSelf(2),
            CorundumStrategy.Three => PlayerHPP is < 100 and not 0 && EnemiesTargetingSelf(3),
            CorundumStrategy.Four => PlayerHPP is < 100 and not 0 && EnemiesTargetingSelf(4),
            CorundumStrategy.Eighty => PlayerHPP is <= 80 and not 0,
            CorundumStrategy.Seventy => PlayerHPP is <= 70 and not 0,
            CorundumStrategy.Sixty => PlayerHPP is <= 60 and not 0,
            CorundumStrategy.Fifty => PlayerHPP is <= 50 and not 0,
            CorundumStrategy.Fourty => PlayerHPP is <= 40 and not 0,
            CorundumStrategy.Thirty => PlayerHPP is <= 30 and not 0,
            _ => false
        })
            QueueGCD(AID.HeartOfCorundumPvP, Player, GCDPriority.Max);
        if (role switch
        {
            RoleActionStrategy.Rampage => HasEffect(SID.RampageEquippedPvP) && ActionReady(AID.RampagePvP) && TargetsNearby(10) && In10y(primaryTarget?.Actor),
            RoleActionStrategy.Rampart => HasEffect(SID.RampartEquippedPvP) && ActionReady(AID.RampartPvP) && ((PlayerHPP is < 100 and not 0 && EnemiesTargetingSelf(2)) || PlayerHPP is < 50 and not 0),
            RoleActionStrategy.FullSwing => HasEffect(SID.FullSwingEquippedPvP) && ActionReady(AID.FullSwingPvP) && In5y(primaryTarget?.Actor),
            _ => false
        })
            QueueGCD(role switch
            {
                RoleActionStrategy.Rampage => AID.RampagePvP,
                RoleActionStrategy.Rampart => AID.RampartPvP,
                RoleActionStrategy.FullSwing => AID.FullSwingPvP,
                _ => AID.None
            },
            role switch
            {
                RoleActionStrategy.Rampage => Player,
                RoleActionStrategy.Rampart => Player,
                RoleActionStrategy.FullSwing => primaryTarget?.Actor,
                _ => null
            }, GCDPriority.VeryHigh + 1);
        if (World.Party.LimitBreakLevel >= 1 && rangeOk && targetsOk && strategy.Option(Track.LimitBreak).As<LBStrategy>() == LBStrategy.Allow)
            QueueGCD(AID.RelentlessRushPvP, Player, GCDPriority.VeryHigh + 1);
        if (HasEffect(SID.RelentlessRushPvP) && rangeOk && targetsOk && strategy.Option(Track.TerminalTrigger).As<TriggerStrategy>() switch
        {
            TriggerStrategy.Five => StacksRemaining(primaryTarget?.Actor, SID.RelentlessShrapnelPvP) >= 5,
            TriggerStrategy.Four => StacksRemaining(primaryTarget?.Actor, SID.RelentlessShrapnelPvP) >= 4,
            TriggerStrategy.Three => StacksRemaining(primaryTarget?.Actor, SID.RelentlessShrapnelPvP) >= 3,
            TriggerStrategy.Two => StacksRemaining(primaryTarget?.Actor, SID.RelentlessShrapnelPvP) >= 2,
            TriggerStrategy.One => StacksRemaining(primaryTarget?.Actor, SID.RelentlessShrapnelPvP) >= 1,
            _ => false
        })
            QueueGCD(AID.TerminalTriggerPvP, Player, GCDPriority.VeryHigh);
        if (CDRemaining(AID.RoughDividePvP) < 14.6f && strategy.Option(Track.RoughDivide).As<DivideStrategy>() switch
        {
            DivideStrategy.Auto => !hasNM,
            DivideStrategy.AutoMelee => !hasNM && In5y(primaryTarget?.Actor),
            _ => false
        })
            QueueOGCD(AID.RoughDividePvP, primaryTarget?.Actor, CDRemaining(AID.RoughDividePvP) < 0.6f ? OGCDPriority.High + 2001 : OGCDPriority.High);
        if (In5y(primaryTarget?.Actor) && HasLOS(primaryTarget?.Actor))
        {
            if (ActionReady(AID.GnashingFangPvP) && strategy.Option(Track.GnashingFang).As<CommonStrategy>() switch
            {
                CommonStrategy.Buff => hasNM,
                CommonStrategy.ASAP => true,
                _ => false
            })
                QueueGCD(AID.GnashingFangPvP, primaryTarget?.Actor, GCDPriority.High);
            if (ActionReady(AID.FatedCirclePvP) && strategy.Option(Track.FatedCircle).As<CommonStrategy>() switch
            {
                CommonStrategy.Buff => hasNM,
                CommonStrategy.ASAP => true,
                _ => false
            })
                QueueGCD(AID.FatedCirclePvP, primaryTarget?.Actor, GCDPriority.Average);
            if (ActionReady(AID.BlastingZonePvP) && strategy.Option(Track.Zone).As<ZoneStrategy>() switch
            {
                ZoneStrategy.Buff => hasNM,
                ZoneStrategy.HalfHPP => HPP(primaryTarget?.Actor) < 50,
                ZoneStrategy.BuffOrHalfHPP => hasNM || HPP(primaryTarget?.Actor) < 50,
                ZoneStrategy.BuffAndHalfHPP => hasNM && HPP(primaryTarget?.Actor) < 50,
                ZoneStrategy.ASAP => true,
                _ => false
            })
                QueueOGCD(AID.BlastingZonePvP, primaryTarget?.Actor, OGCDPriority.High - 1);
            if (GunStep == 1)
                QueueGCD(AID.SavageClawPvP, primaryTarget?.Actor, GCDPriority.BelowAverage);
            if (GunStep == 2)
                QueueGCD(AID.WickedTalonPvP, primaryTarget?.Actor, GCDPriority.BelowAverage);
            if (HasEffect(SID.ReadyToRazePvP))
                QueueOGCD(AID.FatedBrandPvP, primaryTarget?.Actor, ContinuationPrio());
            if (HasEffect(SID.ReadyToBlastPvP))
                QueueOGCD(AID.HypervelocityPvP, primaryTarget?.Actor, ContinuationPrio());
            if (HasEffect(SID.ReadyToRipPvP))
                QueueOGCD(AID.JugularRipPvP, primaryTarget?.Actor, ContinuationPrio());
            if (HasEffect(SID.ReadyToTearPvP))
                QueueOGCD(AID.AbdomenTearPvP, primaryTarget?.Actor, ContinuationPrio());
            if (HasEffect(SID.ReadyToGougePvP))
                QueueOGCD(AID.EyeGougePvP, primaryTarget?.Actor, ContinuationPrio());
            if (GunStep == 0)
                QueueGCD(ComboLastMove switch
                {
                    AID.SolidBarrelPvP => AID.BurstStrikePvP,
                    AID.BrutalShellPvP => AID.SolidBarrelPvP,
                    AID.KeenEdgePvP => AID.BrutalShellPvP,
                    _ => AID.KeenEdgePvP,
                }, primaryTarget?.Actor, GCDPriority.Low);
        }
    }
}
