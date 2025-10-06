using BossMod.WHM;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiWHMPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Targeting, RoleActions, LimitBreak, Cure, CureTarget, Aquaveil, SeraphStrike, MiracleOfNature, AfflatusMisery }
    public enum TargetingStrategy { Auto, Manual }
    public enum RoleActionStrategy { Forbid, Haelan, Stoneskin2, Diabrosis }
    public enum LBStrategy { Any, Two, Three, Forbid }
    public enum CureStrategy { Eighty, Seventy, Sixty, Fifty, Fourty, Forbid }
    public enum CureTargetStrategy { Self, Party, SelfOrParty }
    public enum AquaveilStrategy { Auto, Two, Three, Four, LessThanFull, LessThan75, LessThan50, DebuffOnly, Forbid }
    public enum SeraphStrategy { Twenty, Fifteen, Ten, Five, Forbid }
    public enum CommonStrategy { Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi WHM (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.WHM), 100, 30);
        res.Define(Track.Targeting).As<TargetingStrategy>("Targeting", "", 300)
            .AddOption(TargetingStrategy.Auto, "Automatic", "Automatically select best target")
            .AddOption(TargetingStrategy.Manual, "Manual", "Manually select target");

        res.Define(Track.RoleActions).As<RoleActionStrategy>("Role Actions", "", 300)
            .AddOption(RoleActionStrategy.Forbid, "Forbid", "Do not use any role actions")
            .AddOption(RoleActionStrategy.Haelan, "Haelan", "Use Haelan when available")
            .AddOption(RoleActionStrategy.Stoneskin2, "Stoneskin2", "Use Stoneskin II when available")
            .AddOption(RoleActionStrategy.Diabrosis, "Diabrosis", "Use Diabrosis when available");

        res.Define(Track.LimitBreak).As<LBStrategy>("Limit Break", "", 300)
            .AddOption(LBStrategy.Any, "Any", "Use Limit Break when available")
            .AddOption(LBStrategy.Two, "Two", "Use Limit Break when two or more enemies will be hit")
            .AddOption(LBStrategy.Three, "Three", "Use Limit Break when three or more enemies will be hit")
            .AddOption(LBStrategy.Forbid, "Forbid", "Do not use Limit Break");

        res.Define(Track.Cure).As<CureStrategy>("Cure", "", 300)
            .AddOption(CureStrategy.Eighty, "80%", "Use Cure when HP is below 80%")
            .AddOption(CureStrategy.Seventy, "70%", "Use Cure when HP is below 70%")
            .AddOption(CureStrategy.Sixty, "60%", "Use Cure when HP is below 60%")
            .AddOption(CureStrategy.Fifty, "50%", "Use Cure when HP is below 50%")
            .AddOption(CureStrategy.Fourty, "40%", "Use Cure when HP is below 40%")
            .AddOption(CureStrategy.Forbid, "Forbid", "Do not use Cure");

        res.Define(Track.CureTarget).As<CureTargetStrategy>("Cure Target", "", 300)
            .AddOption(CureTargetStrategy.Self, "Self", "Use Cure on self only")
            .AddOption(CureTargetStrategy.Party, "Party", "Use Cure on party members only")
            .AddOption(CureTargetStrategy.SelfOrParty, "Self or Party", "Use Cure on self or party members");

        res.Define(Track.Aquaveil).As<AquaveilStrategy>("Aquaveil", "", 300)
            .AddOption(AquaveilStrategy.Auto, "Auto", "Use Aquaveil when HP is not full and two or more enemies are targeting you")
            .AddOption(AquaveilStrategy.Two, "Two", "Use Aquaveil when two or more enemies are targeting you")
            .AddOption(AquaveilStrategy.Three, "Three", "Use Aquaveil when three or more enemies are targeting you")
            .AddOption(AquaveilStrategy.Four, "Four", "Use Aquaveil when four or more enemies are targeting you")
            .AddOption(AquaveilStrategy.LessThanFull, "Less than Full", "Use Aquaveil when HP is below 100%")
            .AddOption(AquaveilStrategy.LessThan75, "Less than 75%", "Use Aquaveil when HP is below 75%")
            .AddOption(AquaveilStrategy.LessThan50, "Less than 50%", "Use Aquaveil when HP is below 50%")
            .AddOption(AquaveilStrategy.DebuffOnly, "Debuff Only", "Use Aquaveil only when under a cleansible debuff")
            .AddOption(AquaveilStrategy.Forbid, "Forbid", "Do not use Aquaveil");

        res.Define(Track.SeraphStrike).As<SeraphStrategy>("Seraph Strike", "", 300)
            .AddOption(SeraphStrategy.Twenty, "20y", "Use Seraph Strike when target is within 20 yalms")
            .AddOption(SeraphStrategy.Fifteen, "15y", "Use Seraph Strike when target is within 15 yalms")
            .AddOption(SeraphStrategy.Ten, "10y", "Use Seraph Strike when target is within 10 yalms")
            .AddOption(SeraphStrategy.Five, "5y", "Use Seraph Strike when target is within 5 yalms")
            .AddOption(SeraphStrategy.Forbid, "Forbid", "Do not use Seraph Strike");

        res.Define(Track.MiracleOfNature).As<CommonStrategy>("Miracle of Nature", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Miracle of Nature when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Miracle of Nature");

        res.Define(Track.AfflatusMisery).As<CommonStrategy>("Afflatus Misery", "", 300)
            .AddOption(CommonStrategy.Allow, "Allow", "Use Afflatus Misery when available")
            .AddOption(CommonStrategy.Forbid, "Forbid", "Do not use Afflatus Misery");

        return res;
    }

    public bool IsReady(AID aid) => CDRemaining(aid) <= 0.2f;
    public float DebuffsLeft(Actor? target) => Utils.MaxAll(
        StatusDetails(target, ClassShared.SID.StunPvP, Player.InstanceID, 5).Left,
        StatusDetails(target, ClassShared.SID.HeavyPvP, Player.InstanceID, 5).Left,
        StatusDetails(target, ClassShared.SID.BindPvP, Player.InstanceID, 5).Left,
        StatusDetails(target, ClassShared.SID.SilencePvP, Player.InstanceID, 5).Left,
        StatusDetails(target, ClassShared.SID.DeepFreezePvP, Player.InstanceID, 5).Left,
        StatusDetails(target, SID.MiracleOfNaturePvP, Player.InstanceID, 5).Left);

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var (BestLineTargets, NumLineTargets) = GetBestTarget(primaryTarget, 40, Is40yRectTarget);
        var (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        var (BestSeraphStrikeTargets, NumSeraphStrikeTargets) = GetBestTarget(primaryTarget, 25, Is10ySplashTarget);
        var BestLineTarget = NumLineTargets > 1 ? BestLineTargets : primaryTarget;
        var BestSplashTarget = NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        var BestSeraphStrikeTarget = NumSeraphStrikeTargets > 1 ? BestSeraphStrikeTargets : primaryTarget;
        var mainTarget = primaryTarget?.Actor;
        var auto = strategy.Option(Track.Targeting).As<TargetingStrategy>() == TargetingStrategy.Auto;

        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || Player.FindStatus(ClassShared.SID.GuardPvP) != null)
            return;

        if (auto)
        {
            GetPvPTarget(25);
        }

        if (HasLOS(mainTarget))
        {
            var lb = strategy.Option(Track.LimitBreak).As<LBStrategy>();
            var bestLBtarget = lb switch
            {
                LBStrategy.Any => NumLineTargets >= 1 ? BestLineTargets : primaryTarget,
                LBStrategy.Two => NumLineTargets >= 2 ? BestLineTargets : primaryTarget,
                LBStrategy.Three => NumLineTargets >= 3 ? BestLineTargets : primaryTarget,
                _ => primaryTarget
            };
            if (World.Party.LimitBreakLevel >= 1 && lb switch
            {
                LBStrategy.Any => NumLineTargets >= 1,
                LBStrategy.Two => NumLineTargets >= 2,
                LBStrategy.Three => NumLineTargets >= 3,
                _ => false
            })
                QueueGCD(AID.AfflatusPurgationPvP, auto ? bestLBtarget?.Actor : mainTarget, GCDPriority.Max);

            var bestSStarget = auto ? BestSeraphStrikeTarget?.Actor : mainTarget;
            var (roleCondition, roleAction, roleTarget) = strategy.Option(Track.RoleActions).As<RoleActionStrategy>() switch
            {
                RoleActionStrategy.Haelan => (HasEffect(SID.HaelanEquippedPvP) && MP >= 2500 && PlayerHPP < 60, AID.HaelanPvP, Player),
                RoleActionStrategy.Stoneskin2 => (HasEffect(SID.StoneskinEquippedPvP) && IsReady(AID.StoneskinIIPvP) && EnemiesTargetingSelf(2), AID.StoneskinIIPvP, Player),
                RoleActionStrategy.Diabrosis => (HasEffect(SID.DiabrosisEquippedPvP) && IsReady(AID.DiabrosisPvP), AID.DiabrosisPvP, bestSStarget),
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
            if (IsReady(AID.AquaveilPvP) && strategy.Option(Track.Aquaveil).As<AquaveilStrategy>() switch
            {
                AquaveilStrategy.Auto => PlayerHPP is < 100 and not 0 && EnemiesTargetingSelf(2),
                AquaveilStrategy.Two => EnemiesTargetingSelf(2),
                AquaveilStrategy.Three => EnemiesTargetingSelf(3),
                AquaveilStrategy.Four => EnemiesTargetingSelf(4),
                AquaveilStrategy.LessThanFull => PlayerHPP is < 100 and not 0,
                AquaveilStrategy.LessThan75 => PlayerHPP is < 75 and not 0,
                AquaveilStrategy.LessThan50 => PlayerHPP is < 50 and not 0,
                AquaveilStrategy.DebuffOnly => debuffsUp > 0,
                _ => false
            })
                QueueGCD(AID.AquaveilPvP, Player, GCDPriority.VeryHigh + 1);

            var healtarget = strategy.Option(Track.CureTarget).As<CureTargetStrategy>() switch
            {
                CureTargetStrategy.Self => Player,
                CureTargetStrategy.Party => auto ? World.Party.WithoutSlot(excludeNPCs: true).Exclude(Player).Where(a => a.HPMP.CurHP != a.HPMP.MaxHP).OrderBy(a => (float)a.HPMP.CurHP / a.HPMP.MaxHP).FirstOrDefault() : mainTarget ?? Player,
                CureTargetStrategy.SelfOrParty => auto ? World.Party.WithoutSlot(excludeNPCs: true).Where(a => a.HPMP.CurHP != a.HPMP.MaxHP).OrderBy(a => (float)a.HPMP.CurHP / a.HPMP.MaxHP).FirstOrDefault() : mainTarget ?? Player,
                _ => null
            };
            if ((CDRemaining(AID.CureIIPvP) < 12.6f || HasEffect(SID.CureIIIReadyPvP)) && strategy.Option(Track.Cure).As<CureStrategy>() switch
            {
                CureStrategy.Eighty => HPP(healtarget) is < 80 and not 0,
                CureStrategy.Seventy => HPP(healtarget) is < 70 and not 0,
                CureStrategy.Sixty => HPP(healtarget) is < 60 and not 0,
                CureStrategy.Fifty => HPP(healtarget) is < 50 and not 0,
                CureStrategy.Fourty => HPP(healtarget) is < 40 and not 0,
                _ => false
            })
                QueueGCD(HasEffect(SID.CureIIIReadyPvP) ? AID.CureIIIPvP : AID.CureIIPvP, healtarget, GCDPriority.VeryHigh);

            if (IsReady(AID.AfflatusMiseryPvP) && strategy.Option(Track.AfflatusMisery).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.AfflatusMiseryPvP, auto ? BestSplashTarget?.Actor : mainTarget, GCDPriority.Average);

            if (IsReady(AID.MiracleOfNaturePvP) &&
                mainTarget!.NameID == 0 && //doesn't work on NPCs or striking dummies
                mainTarget.MountId == 0 && //doesn't work on mounted players
                mainTarget.FindStatus(GNB.SID.RelentlessRushPvP) == null && //doesn't work on Relentless Rush
                mainTarget.FindStatus(3162) == null && //doesn't work on Honing Dance
                mainTarget.FindStatus(3039) == null && //don't use on invulnerable DRKs
                mainTarget.FindStatus(1302) == null && //don't use on invulnerable PLDs
                mainTarget.FindStatus(1301) == null && mainTarget.FindStatus(1300) == null && //don't use on any enemies with PLD:Cover
                mainTarget.FindStatus(1978) == null && //don't use on any tanks with Rampart active
                mainTarget.FindStatus(ClassShared.SID.GuardPvP) == null && //don't use on any enemies with Guard active
                strategy.Option(Track.MiracleOfNature).As<CommonStrategy>() == CommonStrategy.Allow)
                QueueGCD(AID.MiracleOfNaturePvP, mainTarget, GCDPriority.Average);

            if (IsReady(AID.SeraphStrikePvP) && strategy.Option(Track.SeraphStrike).As<SeraphStrategy>() switch
            {
                SeraphStrategy.Twenty => In20y(bestSStarget),
                SeraphStrategy.Fifteen => In15y(bestSStarget),
                SeraphStrategy.Ten => In10y(bestSStarget),
                SeraphStrategy.Five => In5y(bestSStarget),
                _ => false
            })
                QueueGCD(AID.SeraphStrikePvP, bestSStarget, GCDPriority.Average);

            QueueGCD(HasEffect(SID.SacredSightPvP) ? AID.GlareIVPvP : AID.GlareIIIPvP, auto && HasEffect(SID.SacredSightPvP) ? BestSplashTarget?.Actor : mainTarget, GCDPriority.Low);
        }
    }
}
