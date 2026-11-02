using BossMod.WHM;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiWHMPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, Cure, CureTarget, Aquaveil, SeraphStrike, MiracleOfNature, AfflatusMisery }
    public enum TargetingStrategy { Auto, FocusTargetsTarget, Manual }
    public enum RoleActionStrategy { Forbid, Haelan, Stoneskin2, Diabrosis }
    public enum LBStrategy { Any, Two, Three, Forbid }
    public enum CureStrategy { Eighty, Seventy, Sixty, Fifty, Fourty, Forbid }
    public enum CureTargetStrategy { Self, Party, SelfOrParty }
    public enum AquaveilStrategy { Auto, Two, Three, Four, LessThanFull, LessThan75, LessThan50, DebuffOnly, Forbid }
    public enum SeraphStrategy { Five, Ten, Fifteen, Twenty, Forbid }
    public enum CommonStrategy { Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi WHM (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.WHM), 100, 30, PvP: PvPCompatibility.PvPOnly);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatically select best target")
            .AddOption(TargetingStrategy.FocusTargetsTarget, "Automatically target your current Focus Target's target - if no Focus Target or if Focus Target is hostile, then automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manually select target");

        res.Define(Track.RoleActions).As<RoleActionStrategy>("Role Actions", "", 300)
            .AddOption(RoleActionStrategy.Forbid, "Do not use any role actions")
            .AddOption(RoleActionStrategy.Haelan, "Use Haelan on self when under 70% HP")
            .AddOption(RoleActionStrategy.Stoneskin2, "Use Stoneskin II when under 80% HP or two enemies are currently targeting you")
            .AddOption(RoleActionStrategy.Diabrosis, "Use Diabrosis on best target when available");

        res.Define(Track.LimitBreak).As<LBStrategy>("Limit Break", "", 300)
            .AddOption(LBStrategy.Any, "Use Limit Break when available")
            .AddOption(LBStrategy.Two, "Use Limit Break when two or more enemies will be hit")
            .AddOption(LBStrategy.Three, "Use Limit Break when three or more enemies will be hit")
            .AddOption(LBStrategy.Forbid, "Do not use Limit Break");

        res.Define(Track.Cure).As<CureStrategy>("Cure", "", 300)
            .AddOption(CureStrategy.Eighty, "Use Cure when HP is below 80%")
            .AddOption(CureStrategy.Seventy, "Use Cure when HP is below 70%")
            .AddOption(CureStrategy.Sixty, "Use Cure when HP is below 60%")
            .AddOption(CureStrategy.Fifty, "Use Cure when HP is below 50%")
            .AddOption(CureStrategy.Fourty, "Use Cure when HP is below 40%")
            .AddOption(CureStrategy.Forbid, "Do not use Cure");

        res.Define(Track.CureTarget).As<CureTargetStrategy>("Cure Target", "", 300)
            .AddOption(CureTargetStrategy.Self, "Use Cure on self only")
            .AddOption(CureTargetStrategy.Party, "Use Cure on party members only")
            .AddOption(CureTargetStrategy.SelfOrParty, "Use Cure on self or party members");

        res.Define(Track.Aquaveil).As<AquaveilStrategy>("Aquaveil", "", 300)
            .AddOption(AquaveilStrategy.Auto, "Use Aquaveil when HP is not full and two or more enemies are targeting you")
            .AddOption(AquaveilStrategy.Two, "Use Aquaveil when two or more enemies are targeting you")
            .AddOption(AquaveilStrategy.Three, "Use Aquaveil when three or more enemies are targeting you")
            .AddOption(AquaveilStrategy.Four, "Use Aquaveil when four or more enemies are targeting you")
            .AddOption(AquaveilStrategy.LessThanFull, "Use Aquaveil when HP is below 100%")
            .AddOption(AquaveilStrategy.LessThan75, "Use Aquaveil when HP is below 75%")
            .AddOption(AquaveilStrategy.LessThan50, "Use Aquaveil when HP is below 50%")
            .AddOption(AquaveilStrategy.DebuffOnly, "Use Aquaveil only when under a cleansible debuff")
            .AddOption(AquaveilStrategy.Forbid, "Do not use Aquaveil");

        res.Define(Track.SeraphStrike).As<SeraphStrategy>("Seraph Strike", "", 300)
            .AddOption(SeraphStrategy.Five, "Use Seraph Strike when target is within 5 yalms")
            .AddOption(SeraphStrategy.Ten, "Use Seraph Strike when target is within 10 yalms")
            .AddOption(SeraphStrategy.Fifteen, "Use Seraph Strike when target is within 15 yalms")
            .AddOption(SeraphStrategy.Twenty, "Use Seraph Strike when target is within 20 yalms")
            .AddOption(SeraphStrategy.Forbid, "Do not use Seraph Strike");

        res.Define(Track.MiracleOfNature).As<CommonStrategy>("Miracle of Nature", "", 300)
            .AddOption(CommonStrategy.Allow, "Use Miracle of Nature when available")
            .AddOption(CommonStrategy.Forbid, "Do not use Miracle of Nature");

        res.Define(Track.AfflatusMisery).As<CommonStrategy>("Afflatus Misery", "", 300)
            .AddOption(CommonStrategy.Allow, "Use Afflatus Misery when available")
            .AddOption(CommonStrategy.Forbid, "Do not use Afflatus Misery");

        return res;
    }

    private static readonly uint[] MiracleBlockers = [(uint)GNB.SID.RelentlessRushPvP, 3162, 3039, 1302, 1301, 1300, 1978, (uint)ClassShared.SID.GuardPvP];

    private bool AutoTarget;
    private (Enemy? Best, int Targets) LineTargets;
    private Actor? SplashTarget;
    private Actor? SeraphTarget;

    private bool IsReady(AID aid) => Cooldown(aid) <= 0.2f;
    private Actor? Retarget(Actor? target) => AutoTarget ? target : PrimaryTarget;
    private float DebuffsLeft => Utils.MaxAll(
        StatusDetails(Player, ClassShared.SID.StunPvP, Player.InstanceID, 5).Left,
        StatusDetails(Player, ClassShared.SID.HeavyPvP, Player.InstanceID, 5).Left,
        StatusDetails(Player, ClassShared.SID.BindPvP, Player.InstanceID, 5).Left,
        StatusDetails(Player, ClassShared.SID.SilencePvP, Player.InstanceID, 5).Left,
        StatusDetails(Player, ClassShared.SID.DeepFreezePvP, Player.InstanceID, 5).Left,
        StatusDetails(Player, SID.MiracleOfNaturePvP, Player.InstanceID, 5).Left);
    private Actor? LowestHPAlly(bool includeSelf)
    {
        Actor? best = null;
        foreach (var ally in World.Party.WithoutSlot(excludeNPCs: true))
            if ((includeSelf || ally != Player) && ally.HPMP.CurHP != ally.HPMP.MaxHP && (best == null || ally.PendingHPRatio < best.PendingHPRatio))
                best = ally;
        return best;
    }
    private bool CanMiracle(Actor? target)
    {
        var blocked = target == null || target.NameID != 0 || target.MountId != 0;
        if (target != null)
            foreach (var status in MiracleBlockers)
                blocked |= target.FindStatus(status) != null;
        return !blocked;
    }

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || IsGuarded(Player))
            return;

        var targeting = strategy.Option(Track.Targeting).As<TargetingStrategy>();
        AutoTarget = targeting == TargetingStrategy.Auto;
        LineTargets = GetBestTarget(primaryTarget, 40, LineTargetCheck(40));
        SplashTarget = Retarget(GetBestTarget(primaryTarget, 25, IsSplashTarget).Best?.Actor);
        SeraphTarget = Retarget(GetBestTarget(primaryTarget, 25, Is10ySplashTarget).Best?.Actor);
        if (targeting != TargetingStrategy.Manual)
            GetPvPTarget(25, targeting == TargetingStrategy.FocusTargetsTarget);
        QueueRoleAction(strategy);
        QueueAquaveil(strategy);
        QueueCure(strategy);
        if (HasLOS(PrimaryTarget))
        {
            QueueLimitBreak(strategy);
            QueueMiracleOfNature(strategy);
            QueueSeraphStrike(strategy);
            QueueAfflatusMisery(strategy);
            QueueGlare();
        }
    }

    private void QueueLimitBreak(StrategyValues strategy)
    {
        var target = Retarget(LineTargets.Best?.Actor);
        var condition = strategy.Option(Track.LimitBreak).As<LBStrategy>() switch
        {
            LBStrategy.Any => LineTargets.Targets > 0,
            LBStrategy.Two => LineTargets.Targets > 1,
            LBStrategy.Three => LineTargets.Targets > 2,
            _ => false
        };
        if (condition && World.Party.LimitBreakLevel >= 1 && DistanceFrom(target, 40f))
            QueueGCD(AID.AfflatusPurgationPvP, target, GCDPriority.Max);
    }
    private void QueueRoleAction(StrategyValues strategy)
    {
        var (condition, action, target) = strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
        {
            RoleActionStrategy.Haelan => (HasStatus(SID.HaelanEquippedPvP) && MP >= 2000 && Player.PendingHPRatio < 0.7f, AID.HaelanPvP, Player),
            RoleActionStrategy.Stoneskin2 => (HasStatus(SID.StoneskinEquippedPvP) && IsReady(AID.StoneskinIIPvP) && (Player.PendingHPRatio < 0.8f || EnemiesTargetingPlayer >= 2), AID.StoneskinIIPvP, Player),
            RoleActionStrategy.Diabrosis => (HasStatus(SID.DiabrosisEquippedPvP) && IsReady(AID.DiabrosisPvP) && In25y(SeraphTarget) && HasLOS(SeraphTarget), AID.DiabrosisPvP, SeraphTarget),
            _ => (false, AID.None, null)
        };
        if (condition)
            QueueGCD(action, target, GCDPriority.Critical);
    }
    private void QueueAquaveil(StrategyValues strategy)
    {
        var condition = strategy.Option(Track.Aquaveil).As<AquaveilStrategy>() switch
        {
            AquaveilStrategy.Auto => Player.PendingHPRatio < 1f && EnemiesTargetingPlayer >= 2,
            AquaveilStrategy.Two => EnemiesTargetingPlayer >= 2,
            AquaveilStrategy.Three => EnemiesTargetingPlayer >= 3,
            AquaveilStrategy.Four => EnemiesTargetingPlayer >= 4,
            AquaveilStrategy.LessThanFull => Player.PendingHPRatio < 1f,
            AquaveilStrategy.LessThan75 => Player.PendingHPRatio < 0.75f,
            AquaveilStrategy.LessThan50 => Player.PendingHPRatio < 0.5f,
            AquaveilStrategy.DebuffOnly => DebuffsLeft > 0.5f,
            _ => false
        };
        if (condition && IsReady(AID.AquaveilPvP))
            QueueGCD(AID.AquaveilPvP, Player, GCDPriority.VeryHigh + 1);
    }
    private void QueueCure(StrategyValues strategy)
    {
        var target = strategy.Option(Track.CureTarget).As<CureTargetStrategy>() switch
        {
            CureTargetStrategy.Self => Player,
            CureTargetStrategy.Party => AutoTarget ? LowestHPAlly(false) : PrimaryTarget,
            CureTargetStrategy.SelfOrParty => AutoTarget ? LowestHPAlly(true) : PrimaryTarget ?? Player,
            _ => null
        };
        var threshold = strategy.Option(Track.Cure).As<CureStrategy>() switch
        {
            CureStrategy.Eighty => 0.8f,
            CureStrategy.Seventy => 0.7f,
            CureStrategy.Sixty => 0.6f,
            CureStrategy.Fifty => 0.5f,
            CureStrategy.Fourty => 0.4f,
            _ => 0f
        };
        var cureIII = HasStatus(SID.CureIIIReadyPvP);
        var condition = target != null && target.PendingHPRatio < threshold && target.HPMP.CurHP != target.HPMP.MaxHP && DistanceFrom(target, 30f);
        if (condition && (Cooldown(AID.CureIIPvP) < 12.6f || cureIII))
            QueueGCD(cureIII ? AID.CureIIIPvP : AID.CureIIPvP, target, GCDPriority.VeryHigh);
    }
    private void QueueMiracleOfNature(StrategyValues strategy)
    {
        if (IsReady(AID.MiracleOfNaturePvP) && In10y(PrimaryTarget) && CanMiracle(PrimaryTarget) && strategy.Option(Track.MiracleOfNature).As<CommonStrategy>() == CommonStrategy.Allow)
            QueueGCD(AID.MiracleOfNaturePvP, PrimaryTarget, GCDPriority.Average);
    }
    private void QueueSeraphStrike(StrategyValues strategy)
    {
        var condition = strategy.Option(Track.SeraphStrike).As<SeraphStrategy>() switch
        {
            SeraphStrategy.Five => In5y(SeraphTarget),
            SeraphStrategy.Ten => In10y(SeraphTarget),
            SeraphStrategy.Fifteen => In15y(SeraphTarget),
            SeraphStrategy.Twenty => In20y(SeraphTarget),
            _ => false
        };
        if (condition && IsReady(AID.SeraphStrikePvP))
            QueueGCD(AID.SeraphStrikePvP, SeraphTarget, GCDPriority.Average);
    }
    private void QueueAfflatusMisery(StrategyValues strategy)
    {
        if (In25y(PrimaryTarget) && IsReady(AID.AfflatusMiseryPvP) && strategy.Option(Track.AfflatusMisery).As<CommonStrategy>() == CommonStrategy.Allow)
            QueueGCD(AID.AfflatusMiseryPvP, SplashTarget, GCDPriority.Average);
    }
    private void QueueGlare()
    {
        var sacredSight = HasStatus(SID.SacredSightPvP);
        if (In25y(PrimaryTarget))
            QueueGCD(sacredSight ? AID.GlareIVPvP : AID.GlareIIIPvP, sacredSight ? SplashTarget : PrimaryTarget, GCDPriority.Low);
    }
}
