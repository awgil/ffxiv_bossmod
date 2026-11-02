using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiGNBPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, TerminalTrigger, Corundum, RoughDivide, Zone, GnashingFang, FatedCircle }
    public enum TargetingStrategy { Auto, FocusTargetsTarget, Manual }
    public enum RoleActionStrategy { Forbid, Rampage, Rampart, FullSwing }
    public enum LBStrategy { Allow, Forbid }
    public enum TriggerStrategy { Five, Four, Three, Two, One, Forbid }
    public enum CorundumStrategy { Auto, Two, Three, Four, Eighty, Seventy, Sixty, Fifty, Fourty, Thirty, Forbid }
    public enum DivideStrategy { Auto, AutoMelee, Forbid }
    public enum ZoneStrategy { Buff, HalfHPP, BuffOrHalfHPP, BuffAndHalfHPP, ASAP, Forbid }
    public enum CommonStrategy { Buff, ASAP, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.GNB), 100, 30, PvP: PvPCompatibility.PvPOnly);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatically select best target")
            .AddOption(TargetingStrategy.FocusTargetsTarget, "Automatically target your current Focus Target's target - if no Focus Target or if Focus Target is hostile, then automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manually select target");

        res.Define(Track.RoleActions).As<RoleActionStrategy>("Role Actions", "", 300)
            .AddOption(RoleActionStrategy.Forbid, "Do not use any role actions")
            .AddOption(RoleActionStrategy.Rampage, "Use Rampage when available and targets are nearby")
            .AddOption(RoleActionStrategy.Rampart, "Use Rampart when available")
            .AddOption(RoleActionStrategy.FullSwing, "Use Full Swing when available")
            .AddAssociatedActions(AID.RampagePvP, AID.RampartPvP, AID.FullSwingPvP);

        res.Define(Track.LimitBreak).As<LBStrategy>("Limit Break", "", 300)
            .AddOption(LBStrategy.Allow, "Allow use of Limit Break (Relentless Rush) automatically when nearby enemies")
            .AddOption(LBStrategy.Forbid, "Forbid use of Limit Break (Relentless Rush) entirely");

        res.Define(Track.TerminalTrigger).As<TriggerStrategy>("Terminal Trigger", "", 300)
            .AddOption(TriggerStrategy.Five, "Use when 5 stacks of Relentless Shrapnel are available on target")
            .AddOption(TriggerStrategy.Four, "Use when 4+ stacks of Relentless Shrapnel are available on target")
            .AddOption(TriggerStrategy.Three, "Use when 3+ stacks of Relentless Shrapnel are available on target")
            .AddOption(TriggerStrategy.Two, "Use when 2+ stacks of Relentless Shrapnel are available on target")
            .AddOption(TriggerStrategy.One, "Use when 1+ stacks of Relentless Shrapnel is available on target")
            .AddOption(TriggerStrategy.Forbid, "Let Terminal Trigger happen automatically at the end of its duration")
            .AddAssociatedActions(AID.TerminalTriggerPvP);

        res.Define(Track.Corundum).As<CorundumStrategy>("Heart of Corundum", "", 300)
            .AddOption(CorundumStrategy.Auto, "Use automatically when HP is below 80% or two or more enemies are targeting you")
            .AddOption(CorundumStrategy.Two, "Use when HP is not full and two or more enemies are targeting you")
            .AddOption(CorundumStrategy.Three, "Use when HP is not full and three or more enemies are targeting you")
            .AddOption(CorundumStrategy.Four, "Use when HP is not full and four or more enemies are targeting you")
            .AddOption(CorundumStrategy.Eighty, "Use when HP is at or below 80%")
            .AddOption(CorundumStrategy.Seventy, "Use when HP is at or below 70%")
            .AddOption(CorundumStrategy.Sixty, "Use when HP is at or below 60%")
            .AddOption(CorundumStrategy.Fifty, "Use when HP is at or below 50%")
            .AddOption(CorundumStrategy.Fourty, "Use when HP is at or below 40%")
            .AddOption(CorundumStrategy.Thirty, "Use when HP is at or below 30%")
            .AddOption(CorundumStrategy.Forbid, "Forbid use of Heart of Corundum entirely")
            .AddAssociatedActions(AID.HeartOfCorundumPvP);

        res.Define(Track.RoughDivide).As<DivideStrategy>("Rough Divide", "", 300)
            .AddOption(DivideStrategy.Auto, "Use automatically when No Mercy buff is not active")
            .AddOption(DivideStrategy.AutoMelee, "Use only when in Melee range of target and when No Mercy buff is not active")
            .AddOption(DivideStrategy.Forbid, "Forbid use of Rough Divide entirely")
            .AddAssociatedActions(AID.RoughDividePvP);

        res.Define(Track.Zone).As<ZoneStrategy>("Blasting Zone", "", 300)
            .AddOption(ZoneStrategy.Buff, "Use only when under No Mercy buff regardless target HP%")
            .AddOption(ZoneStrategy.HalfHPP, "Use when target is less than 50% HP regardless of No Mercy buff")
            .AddOption(ZoneStrategy.BuffOrHalfHPP, "Use when under No Mercy buff or target is less than 50% HP")
            .AddOption(ZoneStrategy.BuffAndHalfHPP, "Use when under No Mercy buff and target is less than 50% HP")
            .AddOption(ZoneStrategy.ASAP, "Use ASAP regardless of No Mercy buff or target HP%")
            .AddOption(ZoneStrategy.Forbid, "Forbid use of Blasting Zone entirely")
            .AddAssociatedActions(AID.BlastingZonePvP);

        res.Define(Track.GnashingFang).As<CommonStrategy>("Gnashing Fang", "", 300)
            .AddOption(CommonStrategy.Buff, "Use ASAP when under No Mercy buff")
            .AddOption(CommonStrategy.ASAP, "Use ASAP regardless of No Mercy buff")
            .AddOption(CommonStrategy.Forbid, "Forbid use of Gnashing Fang entirely")
            .AddAssociatedActions(AID.GnashingFangPvP);

        res.Define(Track.FatedCircle).As<CommonStrategy>("Fated Circle", "", 300)
            .AddOption(CommonStrategy.Buff, "Use ASAP when under No Mercy buff")
            .AddOption(CommonStrategy.ASAP, "Use ASAP regardless of No Mercy buff")
            .AddOption(CommonStrategy.Forbid, "Forbid use of Fated Circle entirely")
            .AddAssociatedActions(AID.FatedCirclePvP);

        return res;
    }

    private static readonly (SID Status, AID Action)[] Continuations =
    [
        (SID.ReadyToRazePvP, AID.FatedBrandPvP),
        (SID.ReadyToBlastPvP, AID.HypervelocityPvP),
        (SID.ReadyToRipPvP, AID.JugularRipPvP),
        (SID.ReadyToTearPvP, AID.AbdomenTearPvP),
        (SID.ReadyToGougePvP, AID.EyeGougePvP)
    ];

    private bool HasNoMercy => HasStatus(SID.NoMercyPvP);
    private bool InRushRange => Player.DistanceToHitbox(PrimaryTarget) <= 5.99f && Hints.NumPriorityTargetsInAOECircle(Player.Position, 6) > 0;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || IsGuarded(Player))
            return;

        var targeting = strategy.Option(Track.Targeting).As<TargetingStrategy>();
        if (targeting != TargetingStrategy.Manual)
            GetPvPTarget(5, targeting == TargetingStrategy.FocusTargetsTarget);

        QueueCorundum(strategy);
        QueueRoleAction(strategy);
        QueueLimitBreak(strategy);
        QueueTerminalTrigger(strategy);
        QueueRoughDivide(strategy);

        if (In5y(PrimaryTarget) && HasLOS(PrimaryTarget))
        {
            QueueCommon(AID.GnashingFangPvP, strategy.Option(Track.GnashingFang));
            QueueCommon(AID.FatedCirclePvP, strategy.Option(Track.FatedCircle));
            QueueBlastingZone(strategy);
            QueueCombo();
            QueueContinuations();
        }
    }

    private void QueueCorundum(StrategyValues strategy)
    {
        var condition = strategy.Option(Track.Corundum).As<CorundumStrategy>() switch
        {
            CorundumStrategy.Auto => HPAtOrBelow(0.8f) && EnemiesTargetingPlayer >= 2 || HPBelow(0.5f),
            CorundumStrategy.Two => HPBelow(1f) && EnemiesTargetingPlayer >= 2,
            CorundumStrategy.Three => HPBelow(1f) && EnemiesTargetingPlayer >= 3,
            CorundumStrategy.Four => HPBelow(1f) && EnemiesTargetingPlayer >= 4,
            CorundumStrategy.Eighty => HPAtOrBelow(0.8f),
            CorundumStrategy.Seventy => HPAtOrBelow(0.7f),
            CorundumStrategy.Sixty => HPAtOrBelow(0.6f),
            CorundumStrategy.Fifty => HPAtOrBelow(0.5f),
            CorundumStrategy.Fourty => HPAtOrBelow(0.4f),
            CorundumStrategy.Thirty => HPAtOrBelow(0.3f),
            _ => false
        };
        if (condition && ActionReady(AID.HeartOfCorundumPvP))
            QueueGCD(AID.HeartOfCorundumPvP, Player, GCDPriority.Max);
    }
    private void QueueRoleAction(StrategyValues strategy)
    {
        var (condition, action, target) = strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
        {
            RoleActionStrategy.Rampage => (HasStatus(SID.RampageEquippedPvP) && Hints.NumPriorityTargetsInAOECircle(Player.Position, 10) > 0 && In10y(PrimaryTarget), AID.RampagePvP, Player),
            RoleActionStrategy.Rampart => (HasStatus(SID.RampartEquippedPvP) && (HPBelow(1f) && EnemiesTargetingPlayer >= 2 || HPBelow(0.5f)), AID.RampartPvP, Player),
            RoleActionStrategy.FullSwing => (HasStatus(SID.FullSwingEquippedPvP) && In5y(PrimaryTarget), AID.FullSwingPvP, PrimaryTarget),
            _ => (false, AID.None, null)
        };
        if (condition && ActionReady(action))
            QueueGCD(action, target, GCDPriority.VeryHigh + 1);
    }
    private void QueueLimitBreak(StrategyValues strategy)
    {
        if (World.Party.LimitBreakLevel >= 1 && InRushRange && strategy.Option(Track.LimitBreak).As<LBStrategy>() == LBStrategy.Allow)
            QueueGCD(AID.RelentlessRushPvP, Player, GCDPriority.VeryHigh + 1);
    }
    private void QueueTerminalTrigger(StrategyValues strategy)
    {
        var stacks = Stacks(SID.RelentlessShrapnelPvP, on: PrimaryTarget);
        var condition = strategy.Option(Track.TerminalTrigger).As<TriggerStrategy>() switch
        {
            TriggerStrategy.Five => stacks >= 5,
            TriggerStrategy.Four => stacks >= 4,
            TriggerStrategy.Three => stacks >= 3,
            TriggerStrategy.Two => stacks >= 2,
            TriggerStrategy.One => stacks >= 1,
            _ => false
        };
        if (condition && HasStatus(SID.RelentlessRushPvP) && InRushRange)
            QueueGCD(AID.TerminalTriggerPvP, Player, GCDPriority.VeryHigh);
    }
    private void QueueRoughDivide(StrategyValues strategy)
    {
        var cooldown = Cooldown(AID.RoughDividePvP);
        var condition = strategy.Option(Track.RoughDivide).As<DivideStrategy>() switch
        {
            DivideStrategy.Auto => !HasNoMercy,
            DivideStrategy.AutoMelee => !HasNoMercy && In5y(PrimaryTarget),
            _ => false
        };
        if (condition && cooldown < 14.6f)
            QueueOGCD(AID.RoughDividePvP, PrimaryTarget, cooldown < 0.6f ? OGCDPrio(OGCDStrategy.Force, OGCDPriority.High + 1) : OGCDPriority.High);
    }
    private void QueueCommon(AID action, StrategyValues.OptionRef track)
    {
        var condition = track.As<CommonStrategy>() switch
        {
            CommonStrategy.Buff => HasNoMercy,
            CommonStrategy.ASAP => true,
            _ => false
        };
        if (condition && ActionReady(action))
            QueueGCD(action, PrimaryTarget, GCDPriority.High);
    }
    private void QueueBlastingZone(StrategyValues strategy)
    {
        var halfHP = PrimaryTarget?.PendingHPRatio < 0.5f;
        var condition = strategy.Option(Track.Zone).As<ZoneStrategy>() switch
        {
            ZoneStrategy.Buff => HasNoMercy,
            ZoneStrategy.HalfHPP => halfHP,
            ZoneStrategy.BuffOrHalfHPP => HasNoMercy || halfHP,
            ZoneStrategy.BuffAndHalfHPP => HasNoMercy && halfHP,
            ZoneStrategy.ASAP => true,
            _ => false
        };
        if (condition && ActionReady(AID.BlastingZonePvP))
            QueueOGCD(AID.BlastingZonePvP, PrimaryTarget, OGCDPriority.High - 1);
    }
    private void QueueCombo()
    {
        var (action, priority) = World.Client.GetGauge<GunbreakerGauge>().AmmoComboStep switch
        {
            0 => (ComboLastMove switch
            {
                AID.SolidBarrelPvP => AID.BurstStrikePvP,
                AID.BrutalShellPvP => AID.SolidBarrelPvP,
                AID.KeenEdgePvP => AID.BrutalShellPvP,
                _ => AID.KeenEdgePvP
            }, GCDPriority.Low),
            1 => (AID.SavageClawPvP, GCDPriority.BelowAverage),
            2 => (AID.WickedTalonPvP, GCDPriority.BelowAverage),
            _ => (AID.None, GCDPriority.None)
        };
        QueueGCD(action, PrimaryTarget, priority);
    }
    private void QueueContinuations()
    {
        foreach (var (status, action) in Continuations)
            if (HasStatus(status))
                QueueOGCD(action, PrimaryTarget, ChangePriority(highPrio: (int)OGCDPriority.High));
    }
}
