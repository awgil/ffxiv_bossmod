using BossMod.DRK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.ContentFinderConditionInterface.Delegates;

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
    private (byte State, bool IsActive) DarkArts;
    private (float Timer, bool IsActive, bool NeedsRefresh) Darkside;
    private (float Left, float CD, bool IsActive, bool IsReady) SaltedEarth;
    private (float CD, bool IsReady) AbyssalDrain;
    private (float CD, bool IsReady) CarveAndSpit;
    private (ushort Step, float Left, int Stacks, float CD, bool IsActive, bool IsReady) Delirium;
    private (float Timer, float CD, bool IsActive, bool IsReady) LivingShadow;
    private (float Cooldown, float ReadyIn, bool HasCharges, bool IsReady) Shadowbringer;
    private (float Left, bool IsActive, bool IsReady) Disesteem;
    private bool WantAOE;
    private int NumAOERectTargets;
    private Enemy? BestAOERectTargets;
    private bool ForceAOE;

    private AID BestEdge => Unlocked(AID.EdgeOfShadow) ? AID.EdgeOfShadow : Unlocked(AID.EdgeOfDarkness) ? AID.EdgeOfDarkness : AID.FloodOfDarkness;
    private AID BestFlood => Unlocked(AID.FloodOfShadow) ? AID.FloodOfShadow : AID.FloodOfDarkness;
    private AID BestBloodSpender => WantAOE && Unlocked(AID.Quietus) ? AID.Quietus : AID.Bloodspiller;
    private AID BestDelirium => Unlocked(AID.Delirium) ? AID.Delirium : AID.BloodWeapon;
    private AID BestCarve => Unlocked(AID.CarveAndSpit) ? AID.CarveAndSpit : AID.AbyssalDrain;
    private SID BestBloodWeapon => Unlocked(AID.ScarletDelirium) ? SID.EnhancedDelirium : Unlocked(AID.Delirium) ? SID.Delirium : SID.BloodWeapon;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var gauge = World.Client.GetGauge<DarkKnightGauge>();
        Blood = gauge.Blood;
        DarkArts.State = gauge.DarkArtsState;
        DarkArts.IsActive = DarkArts.State > 0;
        Darkside.Timer = gauge.DarksideTimer / 1000f;
        Darkside.IsActive = Darkside.Timer > 0.1f;
        Darkside.NeedsRefresh = Darkside.Timer <= 3;
        SaltedEarth.Left = StatusRemaining(Player, SID.SaltedEarth, 15);
        SaltedEarth.CD = Cooldown(AID.SaltedEarth);
        SaltedEarth.IsActive = SaltedEarth.Left > 0.1f;
        SaltedEarth.IsReady = Unlocked(AID.SaltedEarth) && SaltedEarth.CD < 0.6f;
        AbyssalDrain.CD = Cooldown(AID.AbyssalDrain);
        AbyssalDrain.IsReady = Unlocked(AID.AbyssalDrain) && AbyssalDrain.CD < 0.6f;
        CarveAndSpit.CD = Cooldown(AID.CarveAndSpit);
        CarveAndSpit.IsReady = Unlocked(AID.CarveAndSpit) && CarveAndSpit.CD < 0.6f;
        Disesteem.Left = StatusRemaining(Player, SID.Scorn, 30);
        Disesteem.IsActive = Disesteem.Left > 0.1f;
        Disesteem.IsReady = Unlocked(AID.Disesteem) && Disesteem.Left > 0.1f;
        Delirium.Step = gauge.DeliriumStep;
        Delirium.Left = StatusRemaining(Player, BestBloodWeapon, 15);
        Delirium.Stacks = StacksRemaining(Player, BestBloodWeapon, 15);
        Delirium.CD = Cooldown(BestDelirium);
        Delirium.IsActive = Delirium.Left > 0.1f;
        Delirium.IsReady = Unlocked(BestDelirium) && Delirium.CD < 0.6f;
        LivingShadow.Timer = gauge.ShadowTimer / 1000f;
        LivingShadow.CD = Cooldown(AID.LivingShadow);
        LivingShadow.IsActive = LivingShadow.Timer > 0;
        LivingShadow.IsReady = Unlocked(AID.LivingShadow) && LivingShadow.CD < 0.6f;
        Shadowbringer.Cooldown = Cooldown(AID.Shadowbringer);
        Shadowbringer.ReadyIn = ReadyIn(AID.Shadowbringer);
        Shadowbringer.HasCharges = Cooldown(AID.Shadowbringer) <= 60;
        Shadowbringer.IsReady = Unlocked(AID.Shadowbringer) && Shadowbringer.HasCharges;
        WantAOE = TargetsInAOECircle(5f, 3) || ForceAOE;
        (BestAOERectTargets, NumAOERectTargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        var mainTarget = primaryTarget?.Actor;

        if (strategy.HoldEverything())
            return;

        var aoe = strategy.Option(Track.AOE);
        var aoeStrat = aoe.As<AOEStrategy>();
        ForceAOE = aoeStrat is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak;
        var stFinish = ComboLastMove switch
        {
            AID.Unleash => Unlocked(AID.StalwartSoul) ? AID.StalwartSoul : AID.HardSlash,
            AID.SyphonStrike => Unlocked(AID.Souleater) ? AID.Souleater : AID.HardSlash,
            AID.HardSlash => Unlocked(AID.SyphonStrike) ? AID.SyphonStrike : AID.HardSlash,
            _ => AID.HardSlash,
        };
        var stBreak = ComboLastMove switch
        {
            AID.SyphonStrike => Unlocked(AID.Souleater) ? AID.Souleater : AID.HardSlash,
            AID.HardSlash => Unlocked(AID.SyphonStrike) ? AID.SyphonStrike : AID.HardSlash,
            _ => AID.HardSlash,
        };
        var aoeFinish = ComboLastMove switch
        {
            AID.SyphonStrike => Unlocked(AID.Souleater) ? AID.Souleater : AID.Unleash,
            AID.HardSlash => Unlocked(AID.SyphonStrike) ? AID.SyphonStrike : AID.Unleash,
            AID.Unleash => Unlocked(AID.StalwartSoul) ? AID.StalwartSoul : AID.Unleash,
            _ => AID.Unleash,
        };
        var aoeBreak = (ComboLastMove is AID.Unleash && Unlocked(AID.StalwartSoul)) ? AID.StalwartSoul : AID.Unleash;
        var stTarget = SingleTargetChoice(mainTarget, aoe);
        var autoTarget = WantAOE ? Player : stTarget;
        var (aoeAction, aoeTarget) = aoeStrat switch
        {
            AOEStrategy.AutoFinish => (WantAOE ? aoeFinish : stFinish, autoTarget),
            AOEStrategy.ForceSTFinish => (stFinish, stTarget),
            AOEStrategy.ForceAOEFinish => (aoeFinish, Player),
            AOEStrategy.AutoBreak => (WantAOE ? aoeBreak : stBreak, autoTarget),
            AOEStrategy.ForceSTBreak => (stBreak, stTarget),
            AOEStrategy.ForceAOEBreak => (aoeBreak, Player),
            _ => (AID.None, null)
        };
        if ((WantAOE ? In5y(aoeTarget) : In3y(aoeTarget)) && aoeTarget != null)
            QueueGCD(aoeAction, aoeTarget, GCDPriority.Low);

        var RiskingBlood = (ComboLastMove is AID.SyphonStrike or AID.Unleash && Blood >= 80) || (Delirium.CD <= GCD && Blood >= 70);
        var RiskingMP = MP >= 10000 || Darkside.NeedsRefresh;
        var Opener = (CombatTimer < 30 && ComboLastMove is AID.Souleater) || CombatTimer >= 30;
        if (!strategy.HoldAbilities())
        {
            if (!strategy.HoldCDs())
            {
                if (!strategy.HoldBuffs())
                {
                    var d = strategy.Option(Track.Delirium);
                    var dStrat = d.As<OGCDStrategy>();
                    if (ShouldUseOGCD(dStrat, mainTarget, Delirium.IsReady, InCombat(mainTarget) && CanWeaveIn && Darkside.IsActive && (Unlocked(AID.Delirium) ? Delirium.IsReady : ActionReady(AID.BloodWeapon)) && (Unlocked(AID.LivingShadow) ? Opener : CombatTimer > 0)))
                        QueueOGCD(BestDelirium, Player, OGCDPrio(dStrat, OGCDPriority.VeryHigh));

                    var ls = strategy.Option(Track.LivingShadow);
                    var lsStrat = ls.As<OGCDStrategy>();
                    if (ShouldUseOGCD(lsStrat, mainTarget, LivingShadow.IsReady, InCombat(mainTarget) && CanWeaveIn && Darkside.IsActive))
                        QueueOGCD(AID.LivingShadow, Player, OGCDPrio(lsStrat, OGCDPriority.ExtremelyHigh));
                }

                var se = strategy.Option(Track.SaltedEarth);
                var seStrat = se.As<OGCDStrategy>();
                if (ShouldUseOGCD(seStrat, mainTarget, SaltedEarth.IsReady, InCombat(mainTarget) && CanWeaveIn && In3y(mainTarget) && Darkside.IsActive && Opener))
                    QueueOGCD(AID.SaltedEarth, Player, OGCDPrio(seStrat, OGCDPriority.AboveAverage));

                var sb = strategy.Option(Track.Shadowbringer);
                var sbStrat = sb.As<OGCDStrategy>();
                var sbTarget = AOETargetChoice(mainTarget, Unlocked(AID.Shadowbringer) && NumAOERectTargets > 1 ? BestAOERectTargets?.Actor : mainTarget, sb, strategy);
                if (ShouldUseOGCD(sbStrat, sbTarget, Shadowbringer.IsReady, InCombat(sbTarget) && CanWeaveIn && Darkside.IsActive && (RaidBuffsLeft > 0f || (LivingShadow.CD > 80 && Delirium.CD > 40))))
                    QueueOGCD(AID.Shadowbringer, sbTarget, OGCDPrio(sbStrat, OGCDPriority.Average));

                var c = strategy.Option(Track.Carve);
                var cStrat = c.As<CarveStrategy>();
                var cTarget = WantAOE ? Player : SingleTargetChoice(mainTarget, c);
                var cOptimal = InCombat(cTarget) && CanWeaveIn && (WantAOE ? In5y(cTarget) : In3y(cTarget)) && Darkside.IsActive && AbyssalDrain.IsReady && Opener && Delirium.CD >= 40;
                var (cCondition, cAction, cPrio) = cStrat switch
                {
                    CarveStrategy.Automatic => (cOptimal, WantAOE ? AID.AbyssalDrain : BestCarve, OGCDPriority.BelowAverage),
                    CarveStrategy.OnlyCarve => (cOptimal, BestCarve, OGCDPriority.BelowAverage),
                    CarveStrategy.OnlyDrain => (cOptimal, AID.AbyssalDrain, OGCDPriority.BelowAverage),
                    CarveStrategy.ForceCarve => (CarveAndSpit.IsReady, BestCarve, OGCDPriority.Forced),
                    CarveStrategy.ForceDrain => (AbyssalDrain.IsReady, AID.AbyssalDrain, OGCDPriority.Forced),
                    _ => (false, AID.None, OGCDPriority.None),
                };
                if (cCondition)
                    QueueGCD(cAction, cTarget, cPrio);
            }

            if (!strategy.HoldGauge())
            {
                var bl = strategy.Option(Track.Blood);
                var blStrat = bl.As<BloodStrategy>();
                var blTarget = WantAOE ? Player : SingleTargetChoice(mainTarget, bl);
                var blMinimum = Unlocked(BestBloodSpender) && (Blood >= 50 || Delirium.IsActive);
                var blOptimal = InCombat(blTarget) && In3y(blTarget) && blMinimum && Darkside.IsActive && (Delirium.CD > 40 || RiskingBlood || RaidBuffsLeft > GCD);
                var (blCondition, blAction, blPrio) = blStrat switch
                {
                    BloodStrategy.Automatic => (blOptimal, BestBloodSpender, GCDPriority.Average),
                    BloodStrategy.OnlyBloodspiller => (blOptimal, AID.Bloodspiller, GCDPriority.Average),
                    BloodStrategy.OnlyQuietus => (blOptimal, AID.Quietus, GCDPriority.Average),
                    BloodStrategy.ForceBest => (blMinimum, BestBloodSpender, GCDPriority.Forced),
                    BloodStrategy.ForceBloodspiller => (blMinimum, AID.Bloodspiller, GCDPriority.Forced),
                    BloodStrategy.ForceQuietus => (blMinimum, AID.Quietus, GCDPriority.Forced),
                    BloodStrategy.Conserve => (RiskingBlood || Delirium.Left > GCD, BestBloodSpender, GCDPriority.Average),
                    _ => (false, AID.None, GCDPriority.None),
                };
                if (blCondition)
                    QueueGCD(blAction, blTarget, blPrio);
            }
        }

        var de = strategy.Option(Track.Disesteem);
        var deStrat = de.As<GCDStrategy>();
        var deTarget = AOETargetChoice(mainTarget, Unlocked(AID.Disesteem) && NumAOERectTargets > 1 ? BestAOERectTargets?.Actor : mainTarget, de, strategy);
        if (ShouldUseGCD(deStrat, deTarget, Disesteem.IsReady, InCombat(deTarget) && In10y(deTarget) && Darkside.IsActive && Disesteem.IsReady && (RaidBuffsLeft > 0 || (LivingShadow.CD > 80 && Delirium.CD > 30 && !Delirium.IsActive) || StatusRemaining(Player, SID.Scorn) < 10f)))
            QueueGCD(AID.Disesteem, deTarget, deStrat is GCDStrategy.Force ? GCDPriority.Forced : CombatTimer < 30 ? GCDPriority.VeryHigh : GCDPriority.AboveAverage);

        var mp = strategy.Option(Track.MP);
        var mpStrat = mp.As<MPStrategy>();
        var mp3k = MP >= 3000;
        var mp6k = MP >= 6000;
        var mp9k = MP >= 9000;
        var burst = Delirium.CD >= 40;
        var highAOE = Unlocked(AID.FloodOfShadow) && NumAOERectTargets >= 3;
        var lowAOE = Unlocked(AID.FloodOfDarkness) && NumAOERectTargets >= 4;
        var eosTarget = SingleTargetChoice(mainTarget, mp);
        var mpTarget = highAOE ? BestAOERectTargets?.Actor : lowAOE ? BestAOERectTargets?.Actor : eosTarget;
        var autoAction = highAOE ? AID.FloodOfShadow : lowAOE ? AID.FloodOfDarkness : BestEdge;
        var autoPrio = RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low;
        var (mpCondition, mpAction, mpPrio) = mpStrat switch
        {
            MPStrategy.Automatic =>
                (RiskingMP ? mp3k || DarkArts.IsActive : //drop all conditions and spend if we need it
                !Unlocked(AID.Delirium) ? mp3k : //just use at low level because who cares
                (Unlocked(AID.TheBlackestNight) && DarkArts.IsActive && (burst || Delirium.CD >= Darkside.Timer + GCD)) ? mp3k : //use in burst or if free to spend optimally
                (burst && InOddWindow(AID.LivingShadow)) ? MP >= 6000 : //use in 1m burst; 2 expected
                burst && !InOddWindow(AID.LivingShadow) && mp3k,
                autoAction, autoPrio),
            MPStrategy.Auto3k => (mp3k || DarkArts.IsActive, autoAction, autoPrio),
            MPStrategy.Auto6k => (mp6k || DarkArts.IsActive, autoAction, autoPrio),
            MPStrategy.Auto9k => (mp9k || DarkArts.IsActive, autoAction, autoPrio),
            MPStrategy.AutoRefresh => (RiskingMP, autoAction, autoPrio),
            MPStrategy.Edge3k => (mp3k, BestEdge, autoPrio),
            MPStrategy.Edge6k => (mp6k, BestEdge, autoPrio),
            MPStrategy.Edge9k => (mp9k, BestEdge, autoPrio),
            MPStrategy.EdgeRefresh => (RiskingMP, BestEdge, autoPrio),
            MPStrategy.Flood3k => (mp3k || DarkArts.IsActive, BestFlood, autoPrio),
            MPStrategy.Flood6k => (mp3k || DarkArts.IsActive, BestFlood, autoPrio),
            MPStrategy.Flood9k => (mp9k || DarkArts.IsActive, BestFlood, autoPrio),
            MPStrategy.FloodRefresh => (RiskingMP, BestFlood, autoPrio),
            MPStrategy.ForceEdge => (Unlocked(AID.EdgeOfDarkness) && mp3k || DarkArts.IsActive, BestEdge, OGCDPriority.Forced),
            MPStrategy.ForceFlood => (Unlocked(AID.FloodOfDarkness) && mp3k || DarkArts.IsActive, BestFlood, OGCDPriority.Forced),
            _ => (false, AID.None, OGCDPriority.None)
        };
        if (mpCondition)
            QueueOGCD(mpAction, mpTarget, mpPrio);

        var dc = strategy.Option(Track.DeliriumCombo);
        var dcStrat = dc.As<DeliriumComboStrategy>();
        var dcTarget = SingleTargetChoice(mainTarget, dc);
        var dcMinimum = Unlocked(AID.Delirium) && Delirium.IsActive && InCombat(dcTarget);
        var anyStep = Delirium.Step is 0 or 1 or 2;
        var (dcCondition, dcAction, dcPrio) = dcStrat switch
        {
            DeliriumComboStrategy.Automatic => ((WantAOE ? In5y(dcTarget) : In3y(dcTarget)) && anyStep,
                WantAOE && anyStep ? AID.Impalement : Delirium.Step is 2 ? AID.Torcleaver : Delirium.Step is 1 ? AID.Comeuppance : Unlocked(AID.ScarletDelirium) ? AID.ScarletDelirium : BestBloodSpender,
                Delirium.Left <= 9f ? GCDPriority.Forced : GCDPriority.SlightlyHigh),
            DeliriumComboStrategy.ScarletDelirum => (Delirium.Step is 0, AID.ScarletDelirium, GCDPriority.Forced),
            DeliriumComboStrategy.Comeuppance => (Delirium.Step is 1, AID.Comeuppance, GCDPriority.Forced),
            DeliriumComboStrategy.Torcleaver => (Delirium.Step is 2, AID.Torcleaver, GCDPriority.Forced),
            DeliriumComboStrategy.Impalement => (anyStep, Unlocked(AID.Impalement) ? AID.Impalement : AID.Quietus, GCDPriority.Forced),

            _ => (false, AID.None, GCDPriority.Low)
        };
        if (dcMinimum && dcCondition)
            QueueGCD(dcAction, dcTarget, dcPrio);

        var sd = strategy.Option(Track.SaltAndDarkness);
        var sdStrat = sd.As<OGCDStrategy>();
        var seZone = World.Actors.FirstOrDefault(x => x.OID == 0x17C && x.OwnerID == Player.InstanceID);
        if (seZone != null && ShouldUseOGCD(sdStrat, mainTarget, ActionReady(AID.SaltAndDarkness), InCombat(mainTarget) && CanWeaveIn && Cooldown(AID.SaltAndDarkness) < 0.6f &&
            SaltedEarth.IsActive && (SaltedEarth.Left is <= 5f and not 0 || Hints.NumPriorityTargetsInAOECircle(seZone.Position, 5) > 0)))
            QueueOGCD(AID.SaltAndDarkness, Player, OGCDPriority.AboveAverage);

        var u = strategy.Option(Track.Unmend);
        var uStrat = u.As<UnmendStrategy>();
        var uTarget = SingleTargetChoice(mainTarget, u);
        var (uCondition, uPrio) = uStrat switch
        {
            UnmendStrategy.Allow => (InCombat(uTarget) && !In3y(uTarget), GCDPriority.Low),
            UnmendStrategy.Force => (true, GCDPriority.Low + 1),
            UnmendStrategy.OpenerFar => (!In3y(uTarget) && CountdownRemaining < 0.8f, GCDPriority.Low),
            UnmendStrategy.OpenerForce => (CountdownRemaining < 0.8f, GCDPriority.Low + 1),
            _ => (false, GCDPriority.None)

        };
        if (uCondition)
            QueueGCD(AID.Unmend, uTarget, uPrio);

        if (strategy.Potion() switch
        {
            PotionStrategy.AlignWithBuffs => Player.InCombat && LivingShadow.CD <= 4f,
            PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
            PotionStrategy.Immediate => true,
            _ => false
        })
            QueuePotSTR();

        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.Unleash, 3, maximumActionRange: 20);
    }
}
