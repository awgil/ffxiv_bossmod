using BossMod.DRK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiDRK(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Blood = SharedTrack.Count, MP, Carve, DeliriumCombo, Unmend, Delirium, SaltedEarth, SaltAndDarkness, LivingShadow, Shadowbringer, Disesteem }
    public enum BloodStrategy { Automatic, OnlyBloodspiller, OnlyQuietus, ForceBest, ForceBloodspiller, ForceQuietus, Conserve, Delay }
    public enum MPStrategy { Automatic, Auto3k, Auto6k, Auto9k, AutoRefresh, Edge3k, Edge6k, Edge9k, EdgeRefresh, Flood3k, Flood6k, Flood9k, FloodRefresh, ForceEdge, ForceFlood, Delay }
    public enum CarveStrategy { Automatic, OnlyCarve, OnlyDrain, ForceCarve, ForceDrain, Delay }
    public enum DeliriumComboStrategy { Automatic, ScarletDelirum, Comeuppance, Torcleaver, Impalement, Delay }
    public enum UnmendStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRK", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.DRK), 100);
        res.DefineAOE().AddAssociatedActions(AID.HardSlash, AID.SyphonStrike, AID.Souleater, AID.Unleash, AID.StalwartSoul);
        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);

        res.Define(Track.Blood).As<BloodStrategy>("Blood", "Bloodspiller / Quietus", 194)
            .AddOption(BloodStrategy.Automatic, "Automatic", "Automatically use Bloodspiller or Quietus optimally based on targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(BloodStrategy.OnlyBloodspiller, "Only Bloodspiller", "Uses Bloodspiller optimally as Blood spender only, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.OnlyQuietus, "Only Quietus", "Uses Quietus optimally as Blood spender only, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 64)
            .AddOption(BloodStrategy.ForceBest, "ASAP", "Automatically use Bloodspiller or Quietus ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.ForceBloodspiller, "Force Bloodspiller", "Force use Bloodspiller ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.ForceQuietus, "Force Quietus", "Force use Quietus ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 64)
            .AddOption(BloodStrategy.Conserve, "Conserve", "Conserves all Bloodspiller or Quietus as much as possible; only spending when absolutely necessary", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.Delay, "Delay", "Delay the use of Bloodspiller or Quietus", supportedTargets: ActionTargets.None, minLevel: 62)
            .AddAssociatedActions(AID.Bloodspiller, AID.Quietus);

        res.Define(Track.MP).As<MPStrategy>("MP", "Edge / Flood", 193)
            .AddOption(MPStrategy.Automatic, "Optimal", "Automatically use Edge or Flood optimally based on targets nearby; 2 for 1m, 4 (or 5 if Dark Arts is active) for 2m", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Auto3k, "Auto 3k", "Automatically use Edge or Flood optimally based on targets nearby; Uses when 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Auto6k, "Auto 6k", "Automatically use Edge or Flood optimally based on targets nearby; Uses when 6000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Auto9k, "Auto 9k", "Automatically use Edge or Flood optimally based on targets nearby; Uses when 9000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.AutoRefresh, "Auto Refresh", "Automatically use Edge or Flood optimally based on targets nearby; Uses only to refresh Darkside or when almost full on MP", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Edge3k, "Edge 3k", "Use Edge of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby; Uses when 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.Edge6k, "Edge 6k", "Use Edge of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby; Uses when 6000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.Edge9k, "Edge 9k", "Use Edge of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby; Uses when 9000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.EdgeRefresh, "Edge Refresh", "Use Edge of Darkness/Shadow of Shadow as Darkside refresher & MP spender regardless of targets nearby; Uses only to refresh Darkside or when almost full on MP", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.Flood3k, "Flood 3k", "Use Flood of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby; Uses when 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Flood6k, "Flood 6k", "Use Flood of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby; Uses when 6000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Flood9k, "Flood 9k", "Use Flood of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby; Uses when 9000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.FloodRefresh, "Flood Refresh", "Use Flood of Darkness/Shadow as Darkside refresher & MP spender regardless of targets nearby; Uses only to refresh Darkside or when almost full on MP", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.ForceEdge, "Force Edge", "Force use Edge of Darkness/Shadow ASAP if 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 40)
            .AddOption(MPStrategy.ForceFlood, "Force Flood", "Force use Flood of Darkness/Shadow ASAP if 3000+ MP is available", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(MPStrategy.Delay, "Delay", "Delay the use of Edge or Flood of Darkness/Shadow", supportedTargets: ActionTargets.None, minLevel: 40)
            .AddAssociatedActions(AID.EdgeOfDarkness, AID.EdgeOfShadow, AID.FloodOfDarkness, AID.FloodOfShadow);

        res.Define(Track.Carve).As<CarveStrategy>("C&S/AD", "Carve & Spit / Abyssal Drain", 195)
            .AddOption(CarveStrategy.Automatic, "Auto", "Automatically use Carve and Spit or Abyssal Drain based on targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 56)
            .AddOption(CarveStrategy.OnlyCarve, "Only Carve and Spit", "Automatically use Carve and Spit, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 60)
            .AddOption(CarveStrategy.OnlyDrain, "Only Abysssal Drain", "Automatically use Abyssal Drain, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 56)
            .AddOption(CarveStrategy.ForceCarve, "Force Carve and Spit", "Force use Carve and Spit ASAP when available", 60, 0, ActionTargets.Hostile, 60)
            .AddOption(CarveStrategy.ForceDrain, "Force Abyssal Drain", "Force use Abyssal Drain ASAP when available", 60, 0, ActionTargets.Hostile, 56)
            .AddOption(CarveStrategy.Delay, "Delay", "Delay the use of Carve and Spit", supportedTargets: ActionTargets.None, minLevel: 56)
            .AddAssociatedActions(AID.CarveAndSpit, AID.AbyssalDrain);

        res.Define(Track.DeliriumCombo).As<DeliriumComboStrategy>("DeliCombo", "Delirium Combo", 196)
            .AddOption(DeliriumComboStrategy.Automatic, "Auto", "Automatically use Delirium Combo", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.ScarletDelirum, "Scarlet Delirium", "Force use Scarlet Delirium ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Comeuppance, "Comeuppance", "Force use Comeuppance ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Torcleaver, "Torcleaver", "Force use Torcleaver ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Impalement, "Impalement", "Force use Impalement ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Delay, "Delay", "Delay use of Scarlet combo", supportedTargets: ActionTargets.None, minLevel: 96)
            .AddAssociatedActions(AID.ScarletDelirium, AID.Comeuppance, AID.Torcleaver, AID.Impalement);

        res.Define(Track.Unmend).As<UnmendStrategy>("Ranged", "Unmend", 100)
            .AddOption(UnmendStrategy.OpenerFar, "Far (Opener)", "Use Unmend only in pre-pull & out of max-melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.OpenerForce, "Force (Opener)", "Use Unmend only in pre-pull regardless of range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Force, "Force", "Force use Unmend regardless of range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Allow, "Allow", "Allow use of Unmend only when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Forbid, "Forbid", "Forbid use of Unmend")
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
    private (float CDRemaining, float ReadyIn, bool HasCharges, bool IsReady) Shadowbringer;
    private (float Left, bool IsActive, bool IsReady) Disesteem;
    private bool ShouldUseAOE;
    private int NumAOERectTargets;
    private Enemy? BestAOERectTargets;

    private AID BestEdge => Unlocked(AID.EdgeOfShadow) ? AID.EdgeOfShadow : Unlocked(AID.EdgeOfDarkness) ? AID.EdgeOfDarkness : AID.FloodOfDarkness;
    private AID BestFlood => Unlocked(AID.FloodOfShadow) ? AID.FloodOfShadow : AID.FloodOfDarkness;
    private AID BestBloodSpender => ShouldUseAOE && Unlocked(AID.Quietus) ? AID.Quietus : AID.Bloodspiller;
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
        SaltedEarth.CD = CDRemaining(AID.SaltedEarth);
        SaltedEarth.IsActive = SaltedEarth.Left > 0.1f;
        SaltedEarth.IsReady = Unlocked(AID.SaltedEarth) && SaltedEarth.CD < 0.6f;
        AbyssalDrain.CD = CDRemaining(AID.AbyssalDrain);
        AbyssalDrain.IsReady = Unlocked(AID.AbyssalDrain) && AbyssalDrain.CD < 0.6f;
        CarveAndSpit.CD = CDRemaining(AID.CarveAndSpit);
        CarveAndSpit.IsReady = Unlocked(AID.CarveAndSpit) && CarveAndSpit.CD < 0.6f;
        Disesteem.Left = StatusRemaining(Player, SID.Scorn, 30);
        Disesteem.IsActive = Disesteem.Left > 0.1f;
        Disesteem.IsReady = Unlocked(AID.Disesteem) && Disesteem.Left > 0.1f;
        Delirium.Step = gauge.DeliriumStep;
        Delirium.Left = StatusRemaining(Player, BestBloodWeapon, 15);
        Delirium.Stacks = StacksRemaining(Player, BestBloodWeapon, 15);
        Delirium.CD = CDRemaining(BestDelirium);
        Delirium.IsActive = Delirium.Left > 0.1f;
        Delirium.IsReady = Unlocked(BestDelirium) && Delirium.CD < 0.6f;
        LivingShadow.Timer = gauge.ShadowTimer / 1000f;
        LivingShadow.CD = CDRemaining(AID.LivingShadow);
        LivingShadow.IsActive = LivingShadow.Timer > 0;
        LivingShadow.IsReady = Unlocked(AID.LivingShadow) && LivingShadow.CD < 0.6f;
        Shadowbringer.CDRemaining = CDRemaining(AID.Shadowbringer);
        Shadowbringer.ReadyIn = ReadyIn(AID.Shadowbringer);
        Shadowbringer.HasCharges = CDRemaining(AID.Shadowbringer) <= 60;
        Shadowbringer.IsReady = Unlocked(AID.Shadowbringer) && Shadowbringer.HasCharges;
        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore;
        (BestAOERectTargets, NumAOERectTargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);

        if (strategy.HoldEverything())
            return;

        var st = (Unlocked(AID.Souleater) && ComboLastMove is AID.SyphonStrike) ? AID.Souleater : (Unlocked(AID.SyphonStrike) && ComboLastMove is AID.HardSlash) ? AID.SyphonStrike : AID.HardSlash;
        var aoe = (Unlocked(AID.StalwartSoul) && ComboLastMove is AID.Unleash) ? AID.StalwartSoul : AID.Unleash;
        var ab = ShouldUseAOE ? aoe : st;
        var stTarget = SingleTargetChoice(primaryTarget?.Actor, strategy.Option(SharedTrack.AOE));
        if (strategy.AutoFinish())
            QueueGCD(ComboLastMove switch
            {
                AID.SyphonStrike => !Unlocked(AID.Souleater) ? ab : st,
                AID.HardSlash => !Unlocked(AID.SyphonStrike) ? ab : st,
                AID.Unleash => !Unlocked(AID.StalwartSoul) ? ab : aoe,
                AID.Souleater or AID.StalwartSoul or _ => ab,
            }, stTarget, GCDPriority.Low);
        if (strategy.AutoBreak())
            QueueGCD(ab, stTarget, GCDPriority.Low);
        if (strategy.ForceST())
            QueueGCD(st, stTarget, GCDPriority.Low);
        if (strategy.ForceAOE())
            QueueGCD(aoe, Player, GCDPriority.Low);

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
                    if (ShouldUseOGCD(dStrat, primaryTarget?.Actor, Delirium.IsReady, InsideCombatWith(primaryTarget?.Actor) && CanWeaveIn && Darkside.IsActive && (Unlocked(AID.Delirium) ? Delirium.IsReady : ActionReady(AID.BloodWeapon)) && (Unlocked(AID.LivingShadow) ? Opener : CombatTimer > 0)))
                        QueueOGCD(BestDelirium, Player, OGCDPrio(dStrat, OGCDPriority.VeryHigh));

                    var ls = strategy.Option(Track.LivingShadow);
                    var lsStrat = ls.As<OGCDStrategy>();
                    if (ShouldUseOGCD(lsStrat, primaryTarget?.Actor, LivingShadow.IsReady, InsideCombatWith(primaryTarget?.Actor) && CanWeaveIn && Darkside.IsActive))
                        QueueOGCD(AID.LivingShadow, Player, OGCDPrio(lsStrat, OGCDPriority.ExtremelyHigh));
                }

                var se = strategy.Option(Track.SaltedEarth);
                var seStrat = se.As<OGCDStrategy>();
                if (ShouldUseOGCD(seStrat, primaryTarget?.Actor, SaltedEarth.IsReady, InsideCombatWith(primaryTarget?.Actor) && CanWeaveIn && In3y(primaryTarget?.Actor) && Darkside.IsActive && Opener))
                    QueueOGCD(AID.SaltedEarth, Player, OGCDPrio(seStrat, OGCDPriority.AboveAverage));

                var sb = strategy.Option(Track.Shadowbringer);
                var sbStrat = sb.As<OGCDStrategy>();
                var sbTarget = AOETargetChoice(primaryTarget?.Actor, Unlocked(AID.Shadowbringer) && NumAOERectTargets > 1 ? BestAOERectTargets?.Actor : primaryTarget?.Actor, sb, strategy);
                if (ShouldUseOGCD(sbStrat, sbTarget, Shadowbringer.IsReady, InsideCombatWith(sbTarget) && CanWeaveIn && Darkside.IsActive && (RaidBuffsLeft > 0f || (LivingShadow.CD > 80 && Delirium.CD > 40))))
                    QueueOGCD(AID.Shadowbringer, sbTarget, OGCDPrio(sbStrat, OGCDPriority.Average));

                var c = strategy.Option(Track.Carve);
                var cStrat = c.As<CarveStrategy>();
                var cTarget = ShouldUseAOE ? Player : SingleTargetChoice(primaryTarget?.Actor, c);
                var cOptimal = InsideCombatWith(cTarget) && CanWeaveIn && (ShouldUseAOE ? In5y(cTarget) : In3y(cTarget)) && Darkside.IsActive && AbyssalDrain.IsReady && Opener && Delirium.CD >= 40;
                var (cCondition, cAction, cPrio) = cStrat switch
                {
                    CarveStrategy.Automatic => (cOptimal, ShouldUseAOE ? AID.AbyssalDrain : BestCarve, OGCDPriority.BelowAverage),
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
                var blTarget = ShouldUseAOE ? Player : SingleTargetChoice(primaryTarget?.Actor, bl);
                var blMinimum = Unlocked(BestBloodSpender) && (Blood >= 50 || Delirium.IsActive);
                var blOptimal = InsideCombatWith(blTarget) && In3y(blTarget) && blMinimum && Darkside.IsActive && (Delirium.CD > 40 || RiskingBlood || RaidBuffsLeft > GCD);
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
        var deTarget = AOETargetChoice(primaryTarget?.Actor, Unlocked(AID.Disesteem) && NumAOERectTargets > 1 ? BestAOERectTargets?.Actor : primaryTarget?.Actor, de, strategy);
        if (ShouldUseGCD(deStrat, deTarget, Disesteem.IsReady, InsideCombatWith(deTarget) && In10y(deTarget) && Darkside.IsActive && Disesteem.IsReady && (RaidBuffsLeft > 0 || (LivingShadow.CD > 80 && Delirium.CD > 30 && !Delirium.IsActive) || StatusRemaining(Player, SID.Scorn) < 10f)))
            QueueGCD(AID.Disesteem, deTarget, deStrat is GCDStrategy.Force ? GCDPriority.Forced : CombatTimer < 30 ? GCDPriority.VeryHigh : GCDPriority.AboveAverage);

        var mp = strategy.Option(Track.MP);
        var mpStrat = mp.As<MPStrategy>();
        var mp3k = MP >= 3000;
        var mp6k = MP >= 6000;
        var mp9k = MP >= 9000;
        var burst = Delirium.CD >= 40;
        var highAOE = Unlocked(AID.FloodOfShadow) && NumAOERectTargets >= 3;
        var lowAOE = Unlocked(AID.FloodOfDarkness) && NumAOERectTargets >= 4;
        var eosTarget = SingleTargetChoice(primaryTarget?.Actor, mp);
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
        var dcTarget = SingleTargetChoice(primaryTarget?.Actor, dc);
        var dcMinimum = Unlocked(AID.Delirium) && Delirium.IsActive && InsideCombatWith(dcTarget);
        var anyStep = Delirium.Step is 0 or 1 or 2;
        var (dcCondition, dcAction, dcPrio) = dcStrat switch
        {
            DeliriumComboStrategy.Automatic => ((ShouldUseAOE ? In5y(dcTarget) : In3y(dcTarget)) && anyStep,
                ShouldUseAOE && anyStep ? AID.Impalement : Delirium.Step is 2 ? AID.Torcleaver : Delirium.Step is 1 ? AID.Comeuppance : Unlocked(AID.ScarletDelirium) ? AID.ScarletDelirium : BestBloodSpender,
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
        if (seZone != null && ShouldUseOGCD(sdStrat, primaryTarget?.Actor, ActionReady(AID.SaltAndDarkness), InsideCombatWith(primaryTarget?.Actor) && CanWeaveIn && CDRemaining(AID.SaltAndDarkness) < 0.6f &&
            SaltedEarth.IsActive && (SaltedEarth.Left is <= 5f and not 0 || Hints.NumPriorityTargetsInAOECircle(seZone.Position, 5) > 0)))
            QueueOGCD(AID.SaltAndDarkness, Player, OGCDPriority.AboveAverage);

        var u = strategy.Option(Track.Unmend);
        var uStrat = u.As<UnmendStrategy>();
        var uTarget = SingleTargetChoice(primaryTarget?.Actor, u);
        var (uCondition, uPrio) = uStrat switch
        {
            UnmendStrategy.Allow => (InsideCombatWith(uTarget) && !In3y(uTarget), GCDPriority.Low),
            UnmendStrategy.Force => (true, GCDPriority.Low + 1),
            UnmendStrategy.OpenerFar => (!In3y(uTarget) && CountdownRemaining < 0.8f, GCDPriority.Low),
            UnmendStrategy.OpenerForce => (CountdownRemaining < 0.8f, GCDPriority.Low + 1),
            _ => (false, GCDPriority.None)

        };
        if (uCondition)
            QueueGCD(AID.Unmend, uTarget, uPrio);

        var (pCondition, pPrio) = strategy.Potion() switch
        {
            PotionStrategy.AlignWithBuffs => (Player.InCombat && LivingShadow.CD <= 4f, GCDPriority.Max),
            PotionStrategy.AlignWithRaidBuffs => (Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0), GCDPriority.Max),
            PotionStrategy.Immediate => (true, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
        if (pCondition)
            ExecutePotSTR(pPrio);

        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.Unleash, 3, maximumActionRange: 20);
    }
}
