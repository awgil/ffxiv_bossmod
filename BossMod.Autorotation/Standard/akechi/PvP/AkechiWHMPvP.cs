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

    public bool IsReady(AID aid) => Cooldown(aid) <= 0.2f;
    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || Player.FindStatus(ClassShared.SID.GuardPvP) != null)
            return;

        var strat = strategy.Option(Track.Targeting).As<TargetingStrategy>();
        var auto = strat == TargetingStrategy.Auto;
        var focus = strat == TargetingStrategy.FocusTargetsTarget;
        var mainTarget = primaryTarget?.Actor;
        var (lineTarget, lineTargets) = GetBestTarget(primaryTarget, 40, LineTargetCheck(40));
        Actor? Retarget(Actor? newTarget) => auto ? newTarget : mainTarget;
        var bestLineTarget = Retarget(lineTarget?.Actor);
        var bestSplashTarget = Retarget(GetBestTarget(primaryTarget, 25, IsSplashTarget).Best?.Actor);
        var bestSeraphStrikeTarget = Retarget(GetBestTarget(primaryTarget, 25, Is10ySplashTarget).Best?.Actor);

        if (auto)
        {
            GetPvPTarget(25, false);
        }
        if (focus)
        {
            GetPvPTarget(25, true);
        }

        if (HasLOS(mainTarget))
        {
            var lb = strategy.Option(Track.LimitBreak).As<LBStrategy>();
            if (DistanceFrom(bestLineTarget, 40f) && World.Party.LimitBreakLevel >= 1 && lb switch
            {
                LBStrategy.Any => lineTargets > 0,
                LBStrategy.Two => lineTargets > 1,
                LBStrategy.Three => lineTargets > 2,
                _ => false
            })
                QueueGCD(AID.AfflatusPurgationPvP, bestLineTarget, GCDPriority.Max);

            var (roleCondition, roleAction, roleTarget) = strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
            {
                RoleActionStrategy.Haelan => (HasStatus(SID.HaelanEquippedPvP) && MP >= 2000 && Player.PendingHPRatio < 0.7f, AID.HaelanPvP, Player),
                RoleActionStrategy.Stoneskin2 => (HasStatus(SID.StoneskinEquippedPvP) && IsReady(AID.StoneskinIIPvP) && (Player.PendingHPRatio < 0.8f || EnemiesTargetingPlayer >= 2), AID.StoneskinIIPvP, Player),
                RoleActionStrategy.Diabrosis => (In25y(bestSeraphStrikeTarget) && HasStatus(SID.DiabrosisEquippedPvP) && IsReady(AID.DiabrosisPvP), AID.DiabrosisPvP, bestSeraphStrikeTarget),
                _ => (false, AID.None, null)
            };
            if (roleCondition)
                QueueGCD(roleAction, roleTarget, GCDPriority.Critical);

            var debuffsUp = Utils.MaxAll(
                StatusDetails(Player, ClassShared.SID.StunPvP, Player.InstanceID, 5).Left,
                StatusDetails(Player, ClassShared.SID.HeavyPvP, Player.InstanceID, 5).Left,
                StatusDetails(Player, ClassShared.SID.BindPvP, Player.InstanceID, 5).Left,
                StatusDetails(Player, ClassShared.SID.SilencePvP, Player.InstanceID, 5).Left,
                StatusDetails(Player, ClassShared.SID.DeepFreezePvP, Player.InstanceID, 5).Left,
                StatusDetails(Player, SID.MiracleOfNaturePvP, Player.InstanceID, 5).Left);

            //self only
            //TODO: add party stuff? 
            if (IsReady(AID.AquaveilPvP) && strategy.Option(Track.Aquaveil).As<AquaveilStrategy>() switch
            {
                AquaveilStrategy.Auto => Player.PendingHPRatio < 1.0f && EnemiesTargetingPlayer >= 2,
                AquaveilStrategy.Two => EnemiesTargetingPlayer >= 2,
                AquaveilStrategy.Three => EnemiesTargetingPlayer >= 3,
                AquaveilStrategy.Four => EnemiesTargetingPlayer >= 4,
                AquaveilStrategy.LessThanFull => Player.PendingHPRatio < 1.0f,
                AquaveilStrategy.LessThan75 => Player.PendingHPRatio < 0.75f,
                AquaveilStrategy.LessThan50 => Player.PendingHPRatio < 0.5f,
                AquaveilStrategy.DebuffOnly => debuffsUp > 0.5f,
                _ => false
            })
                QueueGCD(AID.AquaveilPvP, Player, GCDPriority.VeryHigh + 1);

            var healtarget = strategy.Option(Track.CureTarget).As<CureTargetStrategy>() switch
            {
                CureTargetStrategy.Self => Player,
                CureTargetStrategy.Party => auto ? World.Party.WithoutSlot(excludeNPCs: true).Exclude(Player).Where(a => a.HPMP.CurHP != a.HPMP.MaxHP).OrderBy(a => a.PendingHPRatio).FirstOrDefault() : mainTarget,
                CureTargetStrategy.SelfOrParty => auto ? World.Party.WithoutSlot(excludeNPCs: true).Where(a => a.HPMP.CurHP != a.HPMP.MaxHP).OrderBy(a => a.PendingHPRatio).FirstOrDefault() : mainTarget ?? Player,
                _ => null
            };
            if (DistanceFrom(healtarget, 30f) && (Cooldown(AID.CureIIPvP) < 12.6f || HasStatus(SID.CureIIIReadyPvP)) && strategy.Option(Track.Cure).As<CureStrategy>() switch
            {
                CureStrategy.Eighty => healtarget?.PendingHPRatio < 0.8f && healtarget.HPMP.CurHP != healtarget.HPMP.MaxHP,
                CureStrategy.Seventy => healtarget?.PendingHPRatio < 0.7f && healtarget.HPMP.CurHP != healtarget.HPMP.MaxHP,
                CureStrategy.Sixty => healtarget?.PendingHPRatio < 0.6f && healtarget.HPMP.CurHP != healtarget.HPMP.MaxHP,
                CureStrategy.Fifty => healtarget?.PendingHPRatio < 0.5f && healtarget.HPMP.CurHP != healtarget.HPMP.MaxHP,
                CureStrategy.Fourty => healtarget?.PendingHPRatio < 0.4f && healtarget.HPMP.CurHP != healtarget.HPMP.MaxHP,
                _ => false
            })
                QueueGCD(HasStatus(SID.CureIIIReadyPvP) ? AID.CureIIIPvP : AID.CureIIPvP, healtarget, GCDPriority.VeryHigh);

            if (IsReady(AID.MiracleOfNaturePvP) &&
                In10y(mainTarget) &&
                mainTarget?.NameID == 0 && //doesn't work on NPCs or striking dummies
                mainTarget?.MountId == 0 && //doesn't work on mounted players
                mainTarget?.FindStatus(GNB.SID.RelentlessRushPvP) == null && //doesn't work on Relentless Rush
                mainTarget?.FindStatus(3162) == null && //doesn't work on Honing Dance
                mainTarget?.FindStatus(3039) == null && //don't use on invulnerable DRKs
                mainTarget?.FindStatus(1302) == null && //don't use on invulnerable PLDs
                mainTarget?.FindStatus(1301) == null && mainTarget?.FindStatus(1300) == null && //don't use on any enemies with PLD:Cover
                mainTarget?.FindStatus(1978) == null && //don't use on any tanks with Rampart active
                mainTarget?.FindStatus(ClassShared.SID.GuardPvP) == null && //don't use on any enemies with Guard active
                strategy.Option(Track.MiracleOfNature).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.MiracleOfNaturePvP, mainTarget, GCDPriority.Average);

            if (IsReady(AID.SeraphStrikePvP) && strategy.Option(Track.SeraphStrike).As<SeraphStrategy>() switch
            {
                SeraphStrategy.Five => In5y(bestSeraphStrikeTarget),
                SeraphStrategy.Ten => In10y(bestSeraphStrikeTarget),
                SeraphStrategy.Fifteen => In15y(bestSeraphStrikeTarget),
                SeraphStrategy.Twenty => In20y(bestSeraphStrikeTarget),
                _ => false
            })
                QueueGCD(AID.SeraphStrikePvP, bestSeraphStrikeTarget, GCDPriority.Average);

            if (In25y(mainTarget))
            {
                if (IsReady(AID.AfflatusMiseryPvP) && strategy.Option(Track.AfflatusMisery).As<CommonStrategy>() == CommonStrategy.Allow)
                    QueueGCD(AID.AfflatusMiseryPvP, bestSplashTarget, GCDPriority.Average);

                QueueGCD(HasStatus(SID.SacredSightPvP) ? AID.GlareIVPvP : AID.GlareIIIPvP, HasStatus(SID.SacredSightPvP) ? bestSplashTarget : mainTarget, GCDPriority.Low);
            }
        }
    }
}
