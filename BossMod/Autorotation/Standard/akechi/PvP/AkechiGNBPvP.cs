using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

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

    private OGCDPriority ContinuationPrio
    {
        get
        {
            if (GCD < 0.5f)
                return OGCDPriority.SlightlyHigh + 2000;
            var i = Math.Max(0, (int)((SkSGCDLength - GCD) / 0.5f));
            var a = i * 300;
            return OGCDPriority.Low + a; //every 0.5s = +300 prio
        }
    }
    private void ExecuteCommons(AID action, StrategyValues.OptionRef track, Actor? primaryTarget)
    {
        if (ActionReady(action) && track.As<CommonStrategy>() switch
        {
            CommonStrategy.Buff => HasEffect(SID.NoMercy),
            CommonStrategy.ASAP => true,
            _ => false
        })
            QueueGCD(action, primaryTarget, GCDPriority.High);
    }

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        var GunStep = gauge.AmmoComboStep;
        var hasNM = HasEffect(SID.NoMercyPvP);
        var targetsOk = Hints.NumPriorityTargetsInAOECircle(Player.Position, 6) > 0;
        var mainTarget = primaryTarget?.Actor;
        var rangeOk = Player.DistanceToHitbox(mainTarget) <= 5.99f;

        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || Player.FindStatus(ClassShared.SID.GuardPvP) != null)
            return;

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

        var (roleCondition, roleAction, roleTarget) = strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
        {
            RoleActionStrategy.Rampage => (HasEffect(SID.RampageEquippedPvP) && ActionReady(AID.RampagePvP) && Hints.PriorityTargets.Any(h => h.Actor.IsDeadOrDestroyed && !h.Actor.IsFriendlyNPC && !h.Actor.IsAlly && h.Actor.DistanceToHitbox(Player) <= 10) && In10y(mainTarget), AID.RampagePvP, Player),
            RoleActionStrategy.Rampart => (HasEffect(SID.RampartEquippedPvP) && ActionReady(AID.RampartPvP) && ((PlayerHPP is < 100 and not 0 && EnemiesTargetingSelf(2)) || PlayerHPP is < 50 and not 0), AID.RampartPvP, Player),
            RoleActionStrategy.FullSwing => (HasEffect(SID.FullSwingEquippedPvP) && ActionReady(AID.FullSwingPvP) && In5y(mainTarget), AID.FullSwingPvP, mainTarget),
            _ => (false, AID.None, null)
        };
        if (roleCondition)
            QueueGCD(roleAction, roleTarget, GCDPriority.VeryHigh + 1);

        if (World.Party.LimitBreakLevel >= 1 && rangeOk && targetsOk && strategy.Option(Track.LimitBreak).As<LBStrategy>() == LBStrategy.Allow)
            QueueGCD(AID.RelentlessRushPvP, Player, GCDPriority.VeryHigh + 1);

        if (HasEffect(SID.RelentlessRushPvP) && rangeOk && targetsOk && strategy.Option(Track.TerminalTrigger).As<TriggerStrategy>() switch
        {
            TriggerStrategy.Five => StacksRemaining(mainTarget, SID.RelentlessShrapnelPvP) >= 5,
            TriggerStrategy.Four => StacksRemaining(mainTarget, SID.RelentlessShrapnelPvP) >= 4,
            TriggerStrategy.Three => StacksRemaining(mainTarget, SID.RelentlessShrapnelPvP) >= 3,
            TriggerStrategy.Two => StacksRemaining(mainTarget, SID.RelentlessShrapnelPvP) >= 2,
            TriggerStrategy.One => StacksRemaining(mainTarget, SID.RelentlessShrapnelPvP) >= 1,
            _ => false
        })
            QueueGCD(AID.TerminalTriggerPvP, Player, GCDPriority.VeryHigh);

        if (CDRemaining(AID.RoughDividePvP) < 14.6f && strategy.Option(Track.RoughDivide).As<DivideStrategy>() switch
        {
            DivideStrategy.Auto => !hasNM,
            DivideStrategy.AutoMelee => !hasNM && In5y(mainTarget),
            _ => false
        })
            QueueOGCD(AID.RoughDividePvP, mainTarget, CDRemaining(AID.RoughDividePvP) < 0.6f ? OGCDPriority.High + 2001 : OGCDPriority.High);

        if (In5y(mainTarget) && HasLOS(mainTarget))
        {
            ExecuteCommons(AID.GnashingFangPvP, strategy.Option(Track.GnashingFang), mainTarget);
            ExecuteCommons(AID.FatedCirclePvP, strategy.Option(Track.GnashingFang), mainTarget);

            if (ActionReady(AID.BlastingZonePvP) && strategy.Option(Track.Zone).As<ZoneStrategy>() switch
            {
                ZoneStrategy.Buff => hasNM,
                ZoneStrategy.HalfHPP => HPP(mainTarget) < 50,
                ZoneStrategy.BuffOrHalfHPP => hasNM || HPP(mainTarget) < 50,
                ZoneStrategy.BuffAndHalfHPP => hasNM && HPP(mainTarget) < 50,
                ZoneStrategy.ASAP => true,
                _ => false
            })
                QueueOGCD(AID.BlastingZonePvP, mainTarget, OGCDPriority.High - 1);

            var (gunCondition, gunAction, gunPriority) = gauge.AmmoComboStep switch
            {
                0 => (GunStep == 0, ComboLastMove switch
                {
                    AID.SolidBarrelPvP => AID.BurstStrikePvP,
                    AID.BrutalShellPvP => AID.SolidBarrelPvP,
                    AID.KeenEdgePvP => AID.BrutalShellPvP,
                    _ => AID.KeenEdgePvP,
                }, GCDPriority.Low),
                1 => (GunStep == 1, AID.SavageClawPvP, GCDPriority.BelowAverage),
                2 => (GunStep == 2, AID.WickedTalonPvP, GCDPriority.BelowAverage),
                _ => (false, AID.None, GCDPriority.None)
            };
            if (gunCondition)
                QueueGCD(gunAction, mainTarget, gunPriority);

            foreach (var (status, action) in new[]
            {
                (SID.ReadyToRazePvP,  AID.FatedBrandPvP),
                (SID.ReadyToBlastPvP, AID.HypervelocityPvP),
                (SID.ReadyToRipPvP,   AID.JugularRipPvP),
                (SID.ReadyToTearPvP,  AID.AbdomenTearPvP),
                (SID.ReadyToGougePvP, AID.EyeGougePvP),
            })
            {
                if (HasEffect(status))
                    QueueOGCD(action, mainTarget, ContinuationPrio);
            }
        }
    }
}
