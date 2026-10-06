using BossMod.DRK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiDRK(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Blood, MP, Carve, DeliriumCombo, Unmend, Delirium, SaltedEarth, SaltAndDarkness, LivingShadow, Shadowbringer, Disesteem }
    public enum AOEStrategy { AutoFinish, ForceSTFinish, ForceAOEFinish, AutoBreak, ForceSTBreak, ForceAOEBreak }
    public enum BloodStrategy { Automatic, OnlyBloodspiller, OnlyQuietus, ForceBest, ForceBloodspiller, ForceQuietus, Conserve, Delay }
    public enum MPStrategy { Automatic, Auto3k, Auto6k, Auto9k, AutoRefresh, Edge3k, Edge6k, Edge9k, EdgeRefresh, Flood3k, Flood6k, Flood9k, FloodRefresh, ForceEdge, ForceFlood, Delay }
    public enum CarveStrategy { Automatic, OnlyCarve, OnlyDrain, ForceCarve, ForceDrain, Delay }
    public enum DeliriumComboStrategy { Automatic, ScarletDelirum, Comeuppance, Torcleaver, Impalement, Delay }
    public enum UnmendStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRK", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.DRK), 100);

        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);

        res.Define(Track.AOE).As<AOEStrategy>("ST/AOE", "Single-Target & AoE Rotations", 300)
            .AddOption(AOEStrategy.AutoFinish, "Automatically select best rotation based on targets nearby - finishes current combo if possible")
            .AddOption(AOEStrategy.ForceSTFinish, "Force Single-Target rotation, regardless of targets nearby - finishes current combo if possible")
            .AddOption(AOEStrategy.ForceAOEFinish, "Force AoE rotation, regardless of targets nearby - finishes current combo if possible")
            .AddOption(AOEStrategy.AutoBreak, "Automatically select best rotation based on targets nearby - will break current combo if in one")
            .AddOption(AOEStrategy.ForceSTBreak, "Force Single-Target rotation, regardless of targets nearby - will break current combo if in one")
            .AddOption(AOEStrategy.ForceAOEBreak, "Force AoE rotation, regardless of targets nearby - will break current combo if in one")
            .AddAssociatedActions(AID.HardSlash, AID.SyphonStrike, AID.Souleater, AID.Unleash, AID.StalwartSoul);

        res.Define(Track.Blood).As<BloodStrategy>("Blood", "Bloodspiller / Quietus", 194)
            .AddOption(BloodStrategy.Automatic, "Automatically use Bloodspiller or Quietus optimally based on targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(BloodStrategy.OnlyBloodspiller, "Uses Bloodspiller optimally as Blood spender only, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.OnlyQuietus, "Uses Quietus optimally as Blood spender only, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 64)
            .AddOption(BloodStrategy.ForceBest, "Automatically use Bloodspiller or Quietus ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.ForceBloodspiller, "Force use Bloodspiller ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.ForceQuietus, "Force use Quietus ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 64)
            .AddOption(BloodStrategy.Conserve, "Conserves all Bloodspiller or Quietus as much as possible; only spending when absolutely necessary", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.Delay, "Delay the use of Bloodspiller or Quietus", supportedTargets: ActionTargets.None, minLevel: 62)
            .AddAssociatedActions(AID.Bloodspiller, AID.Quietus);

        res.Define(Track.MP).As<MPStrategy>("MP", "Edge / Flood", 193)
            .AddOption(MPStrategy.Automatic, "Automatically use Edge or Flood optimally based on targets nearby; 2 for 1m, 4 (or 5 if Dark Arts is active) for 2m", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Auto3k, "Automatically use Edge or Flood optimally based on targets nearby - uses when 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Auto6k, "Automatically use Edge or Flood optimally based on targets nearby - uses when 6000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Auto9k, "Automatically use Edge or Flood optimally based on targets nearby - uses when 9000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.AutoRefresh, "Automatically use Edge or Flood optimally based on targets nearby - uses only to refresh Darkside or when almost full on MP", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Edge3k, "Use Edge of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby - uses when 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.Edge6k, "Use Edge of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby - uses when 6000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.Edge9k, "Use Edge of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby - uses when 9000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.EdgeRefresh, "Use Edge of Darkness/Shadow of Shadow as Darkside refresher & MP spender regardless of targets nearby - uses only to refresh Darkside or when almost full on MP", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.Flood3k, "Use Flood of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby - uses when 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Flood6k, "Use Flood of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby - uses when 6000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Flood9k, "Use Flood of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby - uses when 9000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.FloodRefresh, "Use Flood of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby - uses only to refresh Darkside or when almost full on MP", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.ForceEdge, "Force use Edge of Darkness/Shadow ASAP if 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.ForceFlood, "Force use Flood of Darkness/Shadow ASAP if 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Delay, "Delay the use of Edge or Flood of Darkness/Shadow", supportedTargets: ActionTargets.None, minLevel: 40)
            .AddAssociatedActions(AID.EdgeOfDarkness, AID.EdgeOfShadow, AID.FloodOfDarkness, AID.FloodOfShadow);

        res.Define(Track.Carve).As<CarveStrategy>("C&S/AD", "Carve & Spit / Abyssal Drain", 195)
            .AddOption(CarveStrategy.Automatic, "Automatically use Carve and Spit or Abyssal Drain based on targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 56)
            .AddOption(CarveStrategy.OnlyCarve, "Automatically use Carve and Spit, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 60)
            .AddOption(CarveStrategy.OnlyDrain, "Automatically use Abyssal Drain, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 56)
            .AddOption(CarveStrategy.ForceCarve, "Force use Carve and Spit ASAP when available", 60, 0, ActionTargets.Hostile, 60)
            .AddOption(CarveStrategy.ForceDrain, "Force use Abyssal Drain ASAP when available", 60, 0, ActionTargets.Hostile, 56)
            .AddOption(CarveStrategy.Delay, "Delay the use of Carve and Spit", supportedTargets: ActionTargets.None, minLevel: 56)
            .AddAssociatedActions(AID.CarveAndSpit, AID.AbyssalDrain);

        res.Define(Track.DeliriumCombo).As<DeliriumComboStrategy>("DeliCombo", "Delirium Combo", 196)
            .AddOption(DeliriumComboStrategy.Automatic, "Automatically use Delirium Combo", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.ScarletDelirum, "Force use Scarlet Delirium ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Comeuppance, "Force use Comeuppance ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Torcleaver, "Force use Torcleaver ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Impalement, "Force use Impalement ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Delay, "Delay use of Scarlet combo", supportedTargets: ActionTargets.None, minLevel: 96)
            .AddAssociatedActions(AID.ScarletDelirium, AID.Comeuppance, AID.Torcleaver, AID.Impalement);

        res.Define(Track.Unmend).As<UnmendStrategy>("Ranged", "Unmend", 100)
            .AddOption(UnmendStrategy.OpenerFar, "Use Unmend only in pre-pull & out of max-melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.OpenerForce, "Use Unmend only in pre-pull regardless of range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Force, "Force use Unmend regardless of range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Allow, "Allow use of Unmend only when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Forbid, "Forbid use of Unmend")
            .AddAssociatedActions(AID.Unmend);

        res.DefineOGCD(Track.Delirium, AID.Delirium, "Deli", "Delirium", 197, 60, 15, ActionTargets.Self, 35).AddAssociatedActions(AID.BloodWeapon, AID.Delirium);
        res.DefineOGCD(Track.SaltedEarth, AID.SaltedEarth, "SE", "Salted Earth", 196, 90, 15, ActionTargets.Self, 52).AddAssociatedActions(AID.SaltedEarth);
        res.DefineOGCD(Track.SaltAndDarkness, AID.SaltAndDarkness, "S&D", "Salt & Darkness", 190, 20, 0, ActionTargets.Self, 86).AddAssociatedActions(AID.SaltAndDarkness);
        res.DefineOGCD(Track.LivingShadow, AID.LivingShadow, "LS", "Living Shadow", 199, 120, 20, ActionTargets.Self, 80).AddAssociatedActions(AID.LivingShadow);
        res.DefineOGCD(Track.Shadowbringer, AID.Shadowbringer, "SB", "Shadowbringer", 198, 60, 0, ActionTargets.Hostile, 90).AddAssociatedActions(AID.Shadowbringer);
        res.DefineGCD(Track.Disesteem, AID.Disesteem, "Disesteem", "", 150, supportedTargets: ActionTargets.Hostile, minLevel: 100).AddAssociatedActions(AID.Disesteem);

        return res;
    }

    private byte Blood;
    private bool DarkArtsActive;
    private (float Timer, bool IsActive, bool NeedsRefresh) Darkside;
    private (float Left, bool IsActive, bool IsReady) SaltedEarth;
    private (ushort Step, float Left, float CD, bool IsActive, bool IsReady) Delirium;
    private (float CD, bool IsReady) LivingShadow;
    private bool CarveOrDrainReady;
    private bool ShadowbringerReady;
    private bool DisesteemReady;

    private bool WantAOE;
    private bool ForceAOE;
    private int NumAOERectTargets;
    private int NumDrainTargets;
    private Enemy? BestAOERectTargets;
    private Enemy? BestDrainTargets;

    private AID BestEdge => Unlocked(AID.EdgeOfShadow) ? AID.EdgeOfShadow : Unlocked(AID.EdgeOfDarkness) ? AID.EdgeOfDarkness : AID.FloodOfDarkness;
    private AID BestFlood => Unlocked(AID.FloodOfShadow) ? AID.FloodOfShadow : AID.FloodOfDarkness;
    private AID BestBloodSpender => (WantAOE && Unlocked(AID.Quietus)) ? AID.Quietus : AID.Bloodspiller;
    private (AID Action, SID Status) BestDelirium => Unlocked(AID.ScarletDelirium) ? (AID.Delirium, SID.EnhancedDelirium) : Unlocked(AID.Delirium) ? (AID.Delirium, SID.Delirium) : (AID.BloodWeapon, SID.Delirium);
    private AID BestCarve => Unlocked(AID.CarveAndSpit) ? AID.CarveAndSpit : AID.AbyssalDrain;
    private AID STFinishAction => LastComboAction switch
    {
        AID.Unleash => Unlocked(AID.StalwartSoul) ? AID.StalwartSoul : AID.HardSlash,
        AID.SyphonStrike => Unlocked(AID.Souleater) ? AID.Souleater : AID.HardSlash,
        AID.HardSlash => Unlocked(AID.SyphonStrike) ? AID.SyphonStrike : AID.HardSlash,
        _ => AID.HardSlash,
    };
    private AID STBreakAction => LastComboAction switch
    {
        AID.SyphonStrike => Unlocked(AID.Souleater) ? AID.Souleater : AID.HardSlash,
        AID.HardSlash => Unlocked(AID.SyphonStrike) ? AID.SyphonStrike : AID.HardSlash,
        _ => AID.HardSlash,
    };
    private AID AOEFinishAction => LastComboAction switch
    {
        AID.SyphonStrike => Unlocked(AID.Souleater) ? AID.Souleater : AID.Unleash,
        AID.HardSlash => Unlocked(AID.SyphonStrike) ? AID.SyphonStrike : AID.Unleash,
        AID.Unleash => Unlocked(AID.StalwartSoul) ? AID.StalwartSoul : AID.Unleash,
        _ => AID.Unleash,
    };
    private AID AOEBreakAction => LastComboAction is AID.Unleash && Unlocked(AID.StalwartSoul) ? AID.StalwartSoul : AID.Unleash;

    private bool RiskingBlood => (LastComboAction is AID.SyphonStrike or AID.Unleash && Blood >= 80) || (Delirium.CD <= GCD && Blood >= 70);
    private bool RiskingMP => MP >= 10000 || Darkside.NeedsRefresh;
    private bool OpenerReady => !Unlocked(AID.LivingShadow) || (CombatTimer >= 30 || LastComboAction is AID.Souleater);
    private bool InBurst => Delirium.CD >= 40;
    private bool InDeliriumCombo => Delirium.Step is 0 or 1 or 2;
    private Actor? RectAOETarget(Actor? target, AID action) => Unlocked(action) && NumAOERectTargets > 1 ? BestAOERectTargets?.Actor : target;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var mainTarget = primaryTarget?.Actor;
        var gauge = World.Client.GetGauge<DarkKnightGauge>();

        Blood = gauge.Blood;
        DarkArtsActive = gauge.DarkArtsState > 0;

        Darkside.Timer = gauge.DarksideTimer / 1000f;
        Darkside.IsActive = Darkside.Timer > 0.1f;
        Darkside.NeedsRefresh = Darkside.Timer <= 3;

        SaltedEarth.Left = Status(SID.SaltedEarth, 15);
        SaltedEarth.IsActive = SaltedEarth.Left > 0.1f;
        SaltedEarth.IsReady = ActionReady(AID.SaltedEarth);

        Delirium.Step = gauge.DeliriumStep;
        Delirium.Left = Status(BestDelirium.Status, 15);
        Delirium.CD = Cooldown(BestDelirium.Action);
        Delirium.IsActive = Delirium.Left > GCD;
        Delirium.IsReady = ActionReady(BestDelirium.Action);

        LivingShadow.CD = Cooldown(AID.LivingShadow);
        LivingShadow.IsReady = ActionReady(AID.LivingShadow);

        CarveOrDrainReady = ActionReady(AID.AbyssalDrain);
        ShadowbringerReady = Unlocked(AID.Shadowbringer) && Cooldown(AID.Shadowbringer) <= 60;
        DisesteemReady = Unlocked(AID.Disesteem) && Status(SID.Scorn, 30) > 0.1f;

        WantAOE = TargetsInAOECircle(5f, 3) || ForceAOE;
        (BestAOERectTargets, NumAOERectTargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        (BestDrainTargets, NumDrainTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);

        if (strategy.HoldEverything())
            return;

        ForceAOE = strategy.Option(Track.AOE).As<AOEStrategy>() is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak;

        QueueCombo(strategy, mainTarget);

        if (!strategy.HoldAbilities())
        {
            if (!strategy.HoldCDs())
            {
                if (!strategy.HoldBuffs())
                {
                    QueueDelirium(strategy, mainTarget);
                    QueueLivingShadow(strategy, mainTarget);
                }

                QueueSaltedEarth(strategy, mainTarget);
                QueueShadowbringer(strategy, mainTarget);
                QueueCarveOrDrain(strategy, mainTarget);
            }

            if (!strategy.HoldGauge())
                QueueBlood(strategy, mainTarget);
        }

        QueueDisesteem(strategy, mainTarget);
        QueueMP(strategy, mainTarget);
        QueueDeliriumCombo(strategy, mainTarget);
        QueueSaltAndDarkness(strategy, mainTarget);
        QueueUnmend(strategy, mainTarget);
        QueuePotion(strategy);

        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.Unleash, 3, maximumActionRange: 20);
    }

    private void QueueDelirium(StrategyValues strategy, Actor? mainTarget)
    {
        var strat = strategy.Option(Track.Delirium).As<OGCDStrategy>();
        var ready = Unlocked(AID.Delirium) ? Delirium.IsReady : ActionReady(AID.BloodWeapon);
        var condition = Unlocked(AID.LivingShadow) ? OpenerReady : CombatTimer > 0;
        if (ShouldUseOGCD(strat, mainTarget, Delirium.IsReady, InCombat(mainTarget) && CanWeaveIn && Darkside.IsActive && ready && condition))
            QueueOGCD(BestDelirium.Action, Player, OGCDPrio(strat, OGCDPriority.VeryHigh));
    }
    private void QueueLivingShadow(StrategyValues strategy, Actor? mainTarget)
    {
        var strat = strategy.Option(Track.LivingShadow).As<OGCDStrategy>();
        if (ShouldUseOGCD(strat, mainTarget, LivingShadow.IsReady, InCombat(mainTarget) && CanWeaveIn && Darkside.IsActive))
            QueueOGCD(AID.LivingShadow, Player, OGCDPrio(strat, OGCDPriority.ExtremelyHigh));
    }

    private void QueueSaltedEarth(StrategyValues strategy, Actor? mainTarget)
    {
        var strat = strategy.Option(Track.SaltedEarth).As<OGCDStrategy>();
        if (ShouldUseOGCD(strat, mainTarget, SaltedEarth.IsReady, InCombat(mainTarget) && CanWeaveIn && In3y(mainTarget) && Darkside.IsActive && OpenerReady))
            QueueOGCD(AID.SaltedEarth, Player, OGCDPrio(strat, OGCDPriority.AboveAverage));
    }
    private void QueueShadowbringer(StrategyValues strategy, Actor? mainTarget)
    {
        var option = strategy.Option(Track.Shadowbringer);
        var strat = option.As<OGCDStrategy>();
        var target = AOETargetChoice(mainTarget, RectAOETarget(mainTarget, AID.Shadowbringer), option, strategy);
        var condition = RaidBuffsLeft > 0f || (LivingShadow.CD > 80 && Delirium.CD > 40);
        if (ShouldUseOGCD(strat, target, ShadowbringerReady, InCombat(target) && CanWeaveIn && Darkside.IsActive && condition))
            QueueOGCD(AID.Shadowbringer, target, OGCDPrio(strat, OGCDPriority.Average));
    }
    private void QueueCarveOrDrain(StrategyValues strategy, Actor? mainTarget)
    {
        var option = strategy.Option(Track.Carve);
        var strat = option.As<CarveStrategy>();
        var stTarget = SingleTargetChoice(mainTarget, option);
        var aoeTarget = AOETargetChoice(mainTarget, BestDrainTargets?.Actor, option, strategy);
        var wantDrain = NumDrainTargets > 2;
        var optimal = InCombat(mainTarget) && CanWeaveIn && Darkside.IsActive && CarveOrDrainReady && OpenerReady && InBurst;
        var (condition, action, target, prio) = strat switch
        {
            CarveStrategy.Automatic => (optimal, wantDrain ? AID.AbyssalDrain : BestCarve, wantDrain ? aoeTarget : stTarget, OGCDPriority.BelowAverage),
            CarveStrategy.OnlyCarve => (optimal, BestCarve, Unlocked(AID.CarveAndSpit) ? stTarget : aoeTarget, OGCDPriority.BelowAverage),
            CarveStrategy.OnlyDrain => (optimal, AID.AbyssalDrain, aoeTarget, OGCDPriority.BelowAverage),
            CarveStrategy.ForceCarve => (CarveOrDrainReady, BestCarve, Unlocked(AID.CarveAndSpit) ? stTarget : aoeTarget, OGCDPriority.Forced),
            CarveStrategy.ForceDrain => (CarveOrDrainReady, AID.AbyssalDrain, aoeTarget, OGCDPriority.Forced),
            _ => (false, AID.None, null, OGCDPriority.None),
        };
        if (condition)
            QueueOGCD(action, target, prio);
    }
    private void QueueSaltAndDarkness(StrategyValues strategy, Actor? mainTarget)
    {
        var zone = World.Actors.FirstOrDefault(a => a.OID == 0x17C && a.OwnerID == Player.InstanceID);
        if (zone == null)
            return;

        var strat = strategy.Option(Track.SaltAndDarkness).As<OGCDStrategy>();
        var expirationClose = SaltedEarth.Left is <= 5f and not 0;
        var condition = InCombat(mainTarget) && Cooldown(AID.SaltAndDarkness) < 0.6f && SaltedEarth.IsActive && (expirationClose || Hints.NumPriorityTargetsInAOECircle(zone.Position, 5) > 0);
        if (ShouldUseOGCD(strat, mainTarget, ActionReady(AID.SaltAndDarkness), condition))
            QueueOGCD(AID.SaltAndDarkness, Player, OGCDPriority.AboveAverage);
    }

    private void QueueDeliriumCombo(StrategyValues strategy, Actor? mainTarget)
    {
        var option = strategy.Option(Track.DeliriumCombo);
        var strat = option.As<DeliriumComboStrategy>();
        var target = SingleTargetChoice(mainTarget, option);
        var (condition, action, prio) = strat switch
        {
            DeliriumComboStrategy.Automatic => ((WantAOE || In3y(target)) && InDeliriumCombo, UseDeliriumCombo(), Delirium.Left <= 9f ? GCDPriority.Forced : GCDPriority.SlightlyHigh),
            DeliriumComboStrategy.ScarletDelirum => (Delirium.Step is 0, AID.ScarletDelirium, GCDPriority.Forced),
            DeliriumComboStrategy.Comeuppance => (Delirium.Step is 1, AID.Comeuppance, GCDPriority.Forced),
            DeliriumComboStrategy.Torcleaver => (Delirium.Step is 2, AID.Torcleaver, GCDPriority.Forced),
            DeliriumComboStrategy.Impalement => (InDeliriumCombo, Unlocked(AID.Impalement) ? AID.Impalement : AID.Quietus, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.Low)
        };
        if (Unlocked(AID.ScarletDelirium) && Delirium.IsActive && InCombat(target) && condition)
            QueueGCD(action, target, prio);

        AID UseDeliriumCombo()
        {
            if (WantAOE && InDeliriumCombo)
                return AID.Impalement;
            if (Delirium.Step is 2)
                return AID.Torcleaver;
            if (Delirium.Step is 1)
                return AID.Comeuppance;
            return Unlocked(AID.ScarletDelirium) ? AID.ScarletDelirium : BestBloodSpender;
        }
    }
    private void QueueDisesteem(StrategyValues strategy, Actor? mainTarget)
    {
        var option = strategy.Option(Track.Disesteem);
        var strat = option.As<GCDStrategy>();
        var target = AOETargetChoice(mainTarget, RectAOETarget(mainTarget, AID.Disesteem), option, strategy);
        var condition = RaidBuffsLeft > 0 || (LivingShadow.CD > 80 && Delirium.CD > 30 && !Delirium.IsActive) || Status(SID.Scorn) < 10f;
        var prio = strat is GCDStrategy.Force ? GCDPriority.Forced : CombatTimer < 30 ? GCDPriority.VeryHigh : GCDPriority.AboveAverage;
        if (ShouldUseGCD(strat, target, DisesteemReady, InCombat(target) && In10y(target) && Darkside.IsActive && DisesteemReady && condition))
            QueueGCD(AID.Disesteem, target, prio);
    }

    private void QueueBlood(StrategyValues strategy, Actor? mainTarget)
    {
        var option = strategy.Option(Track.Blood);
        var strat = option.As<BloodStrategy>();
        var action = strat switch
        {
            BloodStrategy.Automatic or BloodStrategy.ForceBest or BloodStrategy.Conserve => BestBloodSpender,
            BloodStrategy.OnlyBloodspiller or BloodStrategy.ForceBloodspiller => AID.Bloodspiller,
            BloodStrategy.OnlyQuietus or BloodStrategy.ForceQuietus => AID.Quietus,
            _ => AID.None
        };
        var target = (WantAOE && action == AID.Quietus) ? Player : SingleTargetChoice(mainTarget, option);
        var canSpend = Unlocked(BestBloodSpender) && (Blood >= 50 || Delirium.IsActive);
        var optimal = InCombat(target) && In3y(target) && canSpend && Darkside.IsActive && (Delirium.CD > 40 || RiskingBlood || RaidBuffsLeft > GCD);
        var (condition, prio) = strat switch
        {
            BloodStrategy.Automatic or BloodStrategy.OnlyBloodspiller or BloodStrategy.OnlyQuietus => (optimal, GCDPriority.Average),
            BloodStrategy.ForceBest or BloodStrategy.ForceBloodspiller or BloodStrategy.ForceQuietus => (canSpend, GCDPriority.Forced),
            BloodStrategy.Conserve => (RiskingBlood || Delirium.Left > GCD, GCDPriority.Average),
            _ => (false, GCDPriority.None),
        };

        if (condition)
            QueueGCD(action, target, prio);
    }
    private void QueueMP(StrategyValues strategy, Actor? mainTarget)
    {
        var option = strategy.Option(Track.MP);
        var strat = option.As<MPStrategy>();
        var mp3k = MP >= 3000;
        var mp6k = MP >= 6000;
        var mp9k = MP >= 9000;
        var useFloodOfShadow = Unlocked(AID.FloodOfShadow) && NumAOERectTargets >= 3;
        var useFloodOfDarkness = Unlocked(AID.FloodOfDarkness) && NumAOERectTargets >= 4;
        var target = useFloodOfShadow || useFloodOfDarkness ? BestAOERectTargets?.Actor : SingleTargetChoice(mainTarget, option);
        var autoAction = useFloodOfShadow ? AID.FloodOfShadow : useFloodOfDarkness ? AID.FloodOfDarkness : BestEdge;
        var autoPrio = RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low;
        var (condition, action, prio) = strat switch
        {
            MPStrategy.Automatic => (UseMP(), autoAction, autoPrio),
            MPStrategy.Auto3k => (mp3k || DarkArtsActive, autoAction, autoPrio),
            MPStrategy.Auto6k => (mp6k || DarkArtsActive, autoAction, autoPrio),
            MPStrategy.Auto9k => (mp9k || DarkArtsActive, autoAction, autoPrio),
            MPStrategy.AutoRefresh => (RiskingMP, autoAction, autoPrio),
            MPStrategy.Edge3k => (mp3k, BestEdge, autoPrio),
            MPStrategy.Edge6k => (mp6k, BestEdge, autoPrio),
            MPStrategy.Edge9k => (mp9k, BestEdge, autoPrio),
            MPStrategy.EdgeRefresh => (RiskingMP, BestEdge, autoPrio),
            MPStrategy.Flood3k => (mp3k || DarkArtsActive, BestFlood, autoPrio),
            MPStrategy.Flood6k => (mp6k || DarkArtsActive, BestFlood, autoPrio),
            MPStrategy.Flood9k => (mp9k || DarkArtsActive, BestFlood, autoPrio),
            MPStrategy.FloodRefresh => (RiskingMP, BestFlood, autoPrio),
            MPStrategy.ForceEdge => ((Unlocked(AID.EdgeOfDarkness) && mp3k) || DarkArtsActive, BestEdge, OGCDPriority.Forced),
            MPStrategy.ForceFlood => ((Unlocked(AID.FloodOfDarkness) && mp3k) || DarkArtsActive, BestFlood, OGCDPriority.Forced),
            _ => (false, AID.None, OGCDPriority.None)
        };
        if (condition)
            QueueOGCD(action, target, prio);

        bool UseMP()
        {
            //about to overcap or Darkside is falling off - forget everything and just send
            if (RiskingMP)
                return mp3k || DarkArtsActive;

            //low lvl - just spend whenever possible
            if (!Unlocked(AID.Delirium))
                return mp3k;

            //free Dark Arts during burst (or when it won't delay Delirium's window)
            if (Unlocked(AID.TheBlackestNight) && DarkArtsActive && (InBurst || Delirium.CD >= Darkside.Timer + GCD))
                return mp3k;

            //Burst windows - 2 casts in 1m, 4 in 2m
            if (InBurst && InOddWindow(AID.LivingShadow))
                return MP >= 6000;

            return InBurst && !InOddWindow(AID.LivingShadow) && mp3k;
        }
    }

    private void QueueUnmend(StrategyValues strategy, Actor? mainTarget)
    {
        var option = strategy.Option(Track.Unmend);
        var strat = option.As<UnmendStrategy>();
        var target = SingleTargetChoice(mainTarget, option);
        var (condition, prio) = strat switch
        {
            UnmendStrategy.Allow => (target != null && !In3y(target), GCDPriority.Low),
            UnmendStrategy.Force => (true, GCDPriority.Low + 1),
            UnmendStrategy.OpenerFar => (target != null && !In3y(target) && CountdownRemaining < 0.8f, GCDPriority.Low),
            UnmendStrategy.OpenerForce => (target != null && CountdownRemaining < 0.8f, GCDPriority.Low + 1),
            _ => (false, GCDPriority.None)
        };

        if (condition)
            QueueGCD(AID.Unmend, target, prio);
    }
    private void QueueCombo(StrategyValues strategy, Actor? mainTarget)
    {
        var option = strategy.Option(Track.AOE);
        var strat = option.As<AOEStrategy>();
        var stTarget = SingleTargetChoice(mainTarget, option);
        var (action, aoeIntent) = strat switch
        {
            AOEStrategy.AutoFinish => (WantAOE ? AOEFinishAction : STFinishAction, WantAOE),
            AOEStrategy.ForceSTFinish => (STFinishAction, false),
            AOEStrategy.ForceAOEFinish => (AOEFinishAction, true),
            AOEStrategy.AutoBreak => (WantAOE ? AOEBreakAction : STBreakAction, WantAOE),
            AOEStrategy.ForceSTBreak => (STBreakAction, false),
            AOEStrategy.ForceAOEBreak => (AOEBreakAction, true),
            _ => (AID.None, false)
        };

        var isAOEAction = action is AID.Unleash or AID.StalwartSoul;
        if (aoeIntent && !isAOEAction && !In3y(stTarget))
        {
            action = AOEBreakAction;
            isAOEAction = true;
        }

        var target = isAOEAction ? Player : stTarget;
        var inRange = isAOEAction ? (aoeIntent || (WantAOE ? In5y(target) : In3y(target))) : In3y(stTarget);
        if (target != null && inRange)
            QueueGCD(action, target, GCDPriority.Low);
    }
    private void QueuePotion(StrategyValues strategy)
    {
        var condition = strategy.Potion() switch
        {
            PotionStrategy.AlignWithBuffs => Player.InCombat && LivingShadow.CD <= 4f,
            PotionStrategy.AlignWithRaidBuffs => Player.InCombat && RaidBuffsLeft > 0,
            PotionStrategy.Immediate => true,
            _ => false
        };

        if (condition)
            QueuePotSTR();
    }
}
