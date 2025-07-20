using BossMod.DRK;
using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiDRK(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Blood = SharedTrack.Count, MP, Carve, DeliriumCombo, Unmend, Delirium, SaltedEarth, SaltAndDarkness, LivingShadow, Shadowbringer, Disesteem }
    public enum BloodStrategy { Automatic, OnlyBloodspiller, OnlyQuietus, ForceBest, ForceBloodspiller, ForceQuietus, Conserve, Delay }
    public enum MPStrategy { Automatic, Auto3k, Auto6k, Auto9k, AutoRefresh, Edge3k, Edge6k, Edge9k, EdgeRefresh, Flood3k, Flood6k, Flood9k, FloodRefresh, ForceEdge, ForceFlood, Delay }
    public enum CarveStrategy { Automatic, OnlyCarve, OnlyDrain, ForceCarve, ForceDrain, Delay }
    public enum DeliriumComboStrategy { Automatic, ScarletDelirum, Comeuppance, Torcleaver, Impalement, Delay }
    public enum UnmendStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRK", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DRK), 100);
        res.DefineAOE().AddAssociatedActions(AID.HardSlash, AID.SyphonStrike, AID.Souleater, AID.Unleash, AID.StalwartSoul);
        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);
        res.Define(Track.Blood).As<BloodStrategy>("Blood", "Blood", uiPriority: 200)
            .AddOption(BloodStrategy.Automatic, "Automatic", "Automatically use Bloodspiller or Quietus optimally based on targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 30)
            .AddOption(BloodStrategy.OnlyBloodspiller, "Only Bloodspiller", "Uses Bloodspiller optimally as Blood spender only, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.OnlyQuietus, "Only Quietus", "Uses Quietus optimally as Blood spender only, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 64)
            .AddOption(BloodStrategy.ForceBest, "ASAP", "Automatically use Bloodspiller or Quietus ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.ForceBloodspiller, "Force Bloodspiller", "Force use Bloodspiller ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.ForceQuietus, "Force Quietus", "Force use Quietus ASAP when 50+ Blood is available", supportedTargets: ActionTargets.Hostile, minLevel: 64)
            .AddOption(BloodStrategy.Conserve, "Conserve", "Conserves all Bloodspiller or Quietus as much as possible; only spending when absolutely necessary", supportedTargets: ActionTargets.Hostile, minLevel: 62)
            .AddOption(BloodStrategy.Delay, "Delay", "Delay the use of Bloodspiller or Quietus", supportedTargets: ActionTargets.None, minLevel: 62)
            .AddAssociatedActions(AID.Bloodspiller, AID.Quietus);
        res.Define(Track.MP).As<MPStrategy>("MP", "MP", uiPriority: 190)
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
        res.Define(Track.Carve).As<CarveStrategy>("Carve & Spit / Abyssal Drain", "Carve", uiPriority: 160)
            .AddOption(CarveStrategy.Automatic, "Auto", "Automatically use Carve and Spit or Abyssal Drain based on targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 56)
            .AddOption(CarveStrategy.OnlyCarve, "Only Carve and Spit", "Automatically use Carve and Spit, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 60)
            .AddOption(CarveStrategy.OnlyDrain, "Only Abysssal Drain", "Automatically use Abyssal Drain, regardless of targets nearby", supportedTargets: ActionTargets.Hostile, minLevel: 56)
            .AddOption(CarveStrategy.ForceCarve, "Force Carve and Spit", "Force use Carve and Spit ASAP when available", 60, 0, ActionTargets.Hostile, 60)
            .AddOption(CarveStrategy.ForceDrain, "Force Abyssal Drain", "Force use Abyssal Drain ASAP when available", 60, 0, ActionTargets.Hostile, 56)
            .AddOption(CarveStrategy.Delay, "Delay", "Delay the use of Carve and Spit", supportedTargets: ActionTargets.None, minLevel: 56)
            .AddAssociatedActions(AID.CarveAndSpit, AID.AbyssalDrain);
        res.Define(Track.DeliriumCombo).As<DeliriumComboStrategy>("Delirium Combo", "Scarlet", uiPriority: 180)
            .AddOption(DeliriumComboStrategy.Automatic, "Auto", "Automatically use Delirium Combo", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.ScarletDelirum, "Scarlet Delirium", "Force use Scarlet Delirium ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Comeuppance, "Comeuppance", "Force use Comeuppance ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Torcleaver, "Torcleaver", "Force use Torcleaver ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Impalement, "Impalement", "Force use Impalement ASAP when available", supportedTargets: ActionTargets.Hostile, minLevel: 96)
            .AddOption(DeliriumComboStrategy.Delay, "Delay", "Delay use of Scarlet combo", supportedTargets: ActionTargets.None, minLevel: 96)
            .AddAssociatedActions(AID.ScarletDelirium, AID.Comeuppance, AID.Torcleaver, AID.Impalement);
        res.Define(Track.Unmend).As<UnmendStrategy>("Ranged", "Ranged", uiPriority: 30)
            .AddOption(UnmendStrategy.OpenerFar, "Far (Opener)", "Use Unmend only in pre-pull & out of max-melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.OpenerForce, "Force (Opener)", "Use Unmend only in pre-pull regardless of range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Force, "Force", "Force use Unmend regardless of range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Allow, "Allow", "Allow use of Unmend only when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Forbid, "Forbid", "Forbid use of Unmend")
            .AddAssociatedActions(AID.Unmend);
        res.DefineOGCD(Track.Delirium, AID.Delirium, "Delirium", "Deli.", uiPriority: 170, 60, 15, ActionTargets.Self, 35).AddAssociatedActions(AID.BloodWeapon, AID.Delirium);
        res.DefineOGCD(Track.SaltedEarth, AID.SaltedEarth, "Salted Earth", "S.Earth", uiPriority: 140, 90, 15, ActionTargets.Self, 52).AddAssociatedActions(AID.SaltedEarth);
        res.DefineOGCD(Track.SaltAndDarkness, AID.SaltAndDarkness, "Salt & Darkness", "Salt & D.", uiPriority: 135, 20, 0, ActionTargets.Self, 86).AddAssociatedActions(AID.SaltAndDarkness);
        res.DefineOGCD(Track.LivingShadow, AID.LivingShadow, "Living Shadow", "L.Shadow", uiPriority: 175, 120, 20, ActionTargets.Self, 80).AddAssociatedActions(AID.LivingShadow);
        res.DefineOGCD(Track.Shadowbringer, AID.Shadowbringer, "Shadowbringer", "S.bringer", uiPriority: 165, 60, 0, ActionTargets.Hostile, 90).AddAssociatedActions(AID.Shadowbringer);
        res.DefineGCD(Track.Disesteem, AID.Disesteem, "Disesteem", "D.esteem", uiPriority: 150, supportedTargets: ActionTargets.Hostile, minLevel: 100).AddAssociatedActions(AID.Disesteem);
        return res;
    }
    #endregion

    #region Module Variables
    private byte Blood;
    private (byte State, bool IsActive) DarkArts;
    private (float Timer, bool IsActive, bool NeedsRefresh) Darkside;
    private bool RiskingBlood;
    private bool RiskingMP;
    private (float Left, float CD, bool IsActive, bool IsReady) SaltedEarth;
    private (float CD, bool IsReady) AbyssalDrain;
    private (float CD, bool IsReady) CarveAndSpit;
    private (ushort Step, float Left, int Stacks, float CD, bool IsActive, bool IsReady) Delirium;
    private (float Timer, float CD, bool IsActive, bool IsReady) LivingShadow;
    private (float CDRemaining, float ReadyIn, bool HasCharges, bool IsReady) Shadowbringer;
    private (float Left, bool IsActive, bool IsReady) Disesteem;
    private bool Opener;
    private bool ShouldUseAOE;
    private int NumAOERectTargets;
    private Enemy? BestAOERectTargets;
    private Enemy? BestRectTarget;
    private Enemy? BestMPTarget;
    #endregion

    #region Upgrade Paths
    private AID BestEdge => Unlocked(AID.EdgeOfShadow) ? AID.EdgeOfShadow : Unlocked(AID.EdgeOfDarkness) ? AID.EdgeOfDarkness : AID.FloodOfDarkness;
    private AID BestFlood => Unlocked(AID.FloodOfShadow) ? AID.FloodOfShadow : AID.FloodOfDarkness;
    private AID BestBloodSpender => ShouldUseAOE && Unlocked(AID.Quietus) ? AID.Quietus : AID.Bloodspiller;
    private AID BestDelirium => Unlocked(AID.Delirium) ? AID.Delirium : AID.BloodWeapon;
    private AID BestCarve => Unlocked(AID.CarveAndSpit) ? AID.CarveAndSpit : AID.AbyssalDrain;
    private SID BestBloodWeapon => Unlocked(AID.ScarletDelirium) ? SID.EnhancedDelirium : Unlocked(AID.Delirium) ? SID.Delirium : SID.BloodWeapon;
    #endregion

    #region Cooldown Helpers
    private bool ShouldUseMP(MPStrategy strategy)
    {
        if (!CanWeaveIn || !Unlocked(AID.FloodOfDarkness))
            return false;
        if (strategy == MPStrategy.Automatic)
        {
            if (RiskingMP)
                return MP >= 3000 || DarkArts.IsActive;
            if (Unlocked(AID.Delirium))
            {
                //Dark Arts
                if (Unlocked(AID.TheBlackestNight) && DarkArts.IsActive)
                {
                    if (Delirium.CD >= 40 || //if Delirium is active
                       Delirium.CD >= Darkside.Timer + GCD) //if Delirium is down longer than Darkside
                        return MP >= 3000;
                }
                //1m - 2 uses expected
                if (Delirium.CD >= 40 && InOddWindow(AID.LivingShadow))
                    return MP >= 6000;
                //2m - 4 uses expected; 5 with Dark Arts
                if (Delirium.CD >= 40 && !InOddWindow(AID.LivingShadow))
                    return MP >= 3000;
            }
            //if no Delirium, just use it whenever we have more than 3000 MP
            if (!Unlocked(AID.Delirium))
                return MP >= 3000;
        }
        return strategy switch
        {
            MPStrategy.Auto3k or MPStrategy.Edge3k or MPStrategy.Flood3k => MP >= 3000,
            MPStrategy.Auto6k or MPStrategy.Edge6k or MPStrategy.Flood6k => MP >= 6000,
            MPStrategy.Auto9k or MPStrategy.Edge9k or MPStrategy.Flood9k => MP >= 9000,
            MPStrategy.AutoRefresh or MPStrategy.EdgeRefresh or MPStrategy.FloodRefresh => RiskingMP,
            MPStrategy.ForceEdge => Unlocked(AID.EdgeOfDarkness) && MP >= 3000 || DarkArts.IsActive,
            MPStrategy.ForceFlood => Unlocked(AID.FloodOfDarkness) && MP >= 3000 || DarkArts.IsActive,
            _ => false
        };
    }
    private bool ShouldUseBlood(BloodStrategy strategy, Enemy? target)
    {
        var minimum = Unlocked(BestBloodSpender) && (Blood >= 50 || Delirium.IsActive);
        var condition = InsideCombatWith(target?.Actor) && In3y(target?.Actor) && minimum && Darkside.IsActive && (Delirium.CD > 40 || RiskingBlood || RaidBuffsLeft > GCD);
        return strategy switch
        {
            BloodStrategy.Automatic or BloodStrategy.OnlyBloodspiller or BloodStrategy.OnlyQuietus => condition,
            BloodStrategy.ForceBest or BloodStrategy.ForceBloodspiller or BloodStrategy.ForceQuietus => minimum,
            BloodStrategy.Conserve => RiskingBlood || Delirium.Left > GCD,
            _ => false
        };
    }
    private bool ShouldCarveOrDrain(CarveStrategy strategy, Enemy? target) => strategy switch
    {
        CarveStrategy.Automatic or CarveStrategy.OnlyCarve or CarveStrategy.OnlyDrain => InsideCombatWith(target?.Actor) && CanWeaveIn && In3y(target?.Actor) && Darkside.IsActive && AbyssalDrain.IsReady && Opener,
        CarveStrategy.ForceCarve => CarveAndSpit.IsReady,
        CarveStrategy.ForceDrain => AbyssalDrain.IsReady,
        CarveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseSaltedEarth(OGCDStrategy strategy, Enemy? target) => ShouldUseOGCD(strategy, target?.Actor, SaltedEarth.IsReady, InsideCombatWith(target?.Actor) && CanWeaveIn && In3y(target?.Actor) && Darkside.IsActive && Opener);
    private bool ShouldUseSaltAndDarkness(OGCDStrategy strategy, Enemy? target) => ShouldUseOGCD(strategy, target?.Actor, ActionReady(AID.SaltAndDarkness), InsideCombatWith(target?.Actor) && CanWeaveIn && CDRemaining(AID.SaltAndDarkness) < 0.6f && SaltedEarth.IsActive && (SaltedEarth.Left is <= 5f and not 0 || RaidBuffsLeft > 0f || !CanFitSkSGCD(DowntimeIn, 3)));
    private bool ShouldUseDelirium(OGCDStrategy strategy, Enemy? target) => ShouldUseOGCD(strategy, target?.Actor, Delirium.IsReady, InsideCombatWith(target?.Actor) && CanWeaveIn && Darkside.IsActive && (Unlocked(AID.Delirium) ? Delirium.IsReady : ActionReady(AID.BloodWeapon)) && (Unlocked(AID.LivingShadow) ? Opener : CombatTimer > 0));
    private bool ShouldUseLivingShadow(OGCDStrategy strategy, Enemy? target) => ShouldUseOGCD(strategy, target?.Actor, LivingShadow.IsReady, InsideCombatWith(target?.Actor) && CanWeaveIn && Darkside.IsActive);
    private bool ShouldUseShadowbringer(OGCDStrategy strategy, Enemy? target) => ShouldUseOGCD(strategy, target?.Actor, Shadowbringer.IsReady, InsideCombatWith(target?.Actor) && CanWeaveIn && Darkside.IsActive && (RaidBuffsLeft > 0f || (LivingShadow.CD > 80 && Delirium.CD > 40)));
    private bool ShouldUseDisesteem(GCDStrategy strategy, Enemy? target) => ShouldUseGCD(strategy, target?.Actor, Disesteem.IsReady, InsideCombatWith(target?.Actor) && In10y(target?.Actor) && Darkside.IsActive && Disesteem.IsReady && (RaidBuffsLeft > 0 || (LivingShadow.CD > 80 && Delirium.CD > 30 && !Delirium.IsActive) || StatusRemaining(Player, SID.Scorn) < 10f));
    private bool ShouldSpendDeliriumProcs(DeliriumComboStrategy strategy, Enemy? target) => Delirium.IsActive && strategy switch
    {
        DeliriumComboStrategy.Automatic => InsideCombatWith(target?.Actor) && In3y(target?.Actor) && Unlocked(AID.ScarletDelirium) && Delirium.Step is 0 or 1 or 2,
        DeliriumComboStrategy.ScarletDelirum => Unlocked(AID.ScarletDelirium) && Delirium.Step is 0,
        DeliriumComboStrategy.Comeuppance => Unlocked(AID.Comeuppance) && Delirium.Step is 1,
        DeliriumComboStrategy.Torcleaver => Unlocked(AID.Torcleaver) && Delirium.Step is 2,
        DeliriumComboStrategy.Impalement => Unlocked(AID.Impalement) && Delirium.Step is 0,
        DeliriumComboStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseUnmend(UnmendStrategy strategy, Enemy? target) => strategy switch
    {
        UnmendStrategy.OpenerFar => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD && !In3y(target?.Actor),
        UnmendStrategy.OpenerForce => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD,
        UnmendStrategy.Force => true,
        UnmendStrategy.Allow => !In3y(target?.Actor),
        UnmendStrategy.Forbid => false,
        _ => false
    };
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<DarkKnightGauge>();
        Blood = gauge.Blood;
        DarkArts.State = gauge.DarkArtsState;
        DarkArts.IsActive = DarkArts.State > 0;
        Darkside.Timer = gauge.DarksideTimer / 1000f;
        Darkside.IsActive = Darkside.Timer > 0.1f;
        Darkside.NeedsRefresh = Darkside.Timer <= 3;
        RiskingBlood = (ComboLastMove is AID.SyphonStrike or AID.Unleash && Blood >= 80) || (Delirium.CD <= GCD && Blood >= 70);
        RiskingMP = MP >= 10000 || Darkside.NeedsRefresh;
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
        Opener = (CombatTimer < 30 && ComboLastMove is AID.Souleater) || CombatTimer >= 30;
        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore;
        (BestAOERectTargets, NumAOERectTargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        BestRectTarget = Unlocked(AID.Shadowbringer) && NumAOERectTargets > 1 ? BestAOERectTargets : primaryTarget;
        var BestHighMPTarget = NumAOERectTargets > 2 ? BestAOERectTargets : primaryTarget;
        var BestLowMPTarget = NumAOERectTargets > 3 ? BestAOERectTargets : primaryTarget;
        BestMPTarget = Unlocked(AID.FloodOfShadow) ? BestHighMPTarget : BestLowMPTarget;

        #region Strategy Definitions
        var mp = strategy.Option(Track.MP);
        var mpStrat = mp.As<MPStrategy>();
        var blood = strategy.Option(Track.Blood);
        var bloodStrat = blood.As<BloodStrategy>();
        var se = strategy.Option(Track.SaltedEarth);
        var seStrat = se.As<OGCDStrategy>();
        var cd = strategy.Option(Track.Carve);
        var cdStrat = cd.As<CarveStrategy>();
        var deli = strategy.Option(Track.Delirium);
        var deliStrat = deli.As<OGCDStrategy>();
        var ls = strategy.Option(Track.LivingShadow);
        var lsStrat = ls.As<OGCDStrategy>();
        var sb = strategy.Option(Track.Shadowbringer);
        var sbStrat = sb.As<OGCDStrategy>();
        var dcombo = strategy.Option(Track.DeliriumCombo);
        var dcomboStrat = dcombo.As<DeliriumComboStrategy>();
        var de = strategy.Option(Track.Disesteem);
        var deStrat = de.As<GCDStrategy>();
        var unmend = strategy.Option(Track.Unmend);
        var unmendStrat = unmend.As<UnmendStrategy>();
        #endregion

        #endregion

        #region Full Rotation Execution
        if (!strategy.HoldEverything())
        {
            #region Standard Rotations
            var st = (Unlocked(AID.Souleater) && ComboLastMove is AID.SyphonStrike) ? AID.Souleater : (Unlocked(AID.SyphonStrike) && ComboLastMove is AID.HardSlash) ? AID.SyphonStrike : AID.HardSlash;
            var aoe = (Unlocked(AID.StalwartSoul) && ComboLastMove is AID.Unleash) ? AID.StalwartSoul : AID.Unleash;
            var ab = ShouldUseAOE ? aoe : st;
            var af = ComboLastMove switch
            {
                AID.SyphonStrike => !Unlocked(AID.Souleater) ? ab : st,
                AID.HardSlash => !Unlocked(AID.SyphonStrike) ? ab : st,
                AID.Unleash => !Unlocked(AID.StalwartSoul) ? ab : aoe,
                AID.Souleater or AID.StalwartSoul or _ => ab,
            };
            if (strategy.AutoFinish())
                QueueGCD(af, SingleTargetChoice(primaryTarget?.Actor, strategy.Option(SharedTrack.AOE)), GCDPriority.Low);
            if (strategy.AutoBreak())
                QueueGCD(ab, SingleTargetChoice(primaryTarget?.Actor, strategy.Option(SharedTrack.AOE)), GCDPriority.Low);
            if (strategy.ForceST())
                QueueGCD(st, SingleTargetChoice(primaryTarget?.Actor, strategy.Option(SharedTrack.AOE)), GCDPriority.Low);
            if (strategy.ForceAOE())
                QueueGCD(aoe, Player, GCDPriority.Low);
            #endregion

            #region Cooldowns
            if (!strategy.HoldAbilities())
            {
                if (!strategy.HoldCDs())
                {
                    if (!strategy.HoldBuffs())
                    {
                        if (ShouldUseDelirium(deliStrat, primaryTarget))
                            QueueOGCD(BestDelirium, Player, OGCDPrio(deliStrat, OGCDPriority.VeryHigh));
                        if (ShouldUseLivingShadow(lsStrat, primaryTarget))
                            QueueOGCD(AID.LivingShadow, Player, OGCDPrio(lsStrat, OGCDPriority.ExtremelyHigh));
                    }
                    if (ShouldUseSaltedEarth(seStrat, primaryTarget))
                        QueueOGCD(AID.SaltedEarth, Player, OGCDPrio(seStrat, OGCDPriority.AboveAverage));
                    if (ShouldUseShadowbringer(sbStrat, primaryTarget))
                        QueueOGCD(AID.Shadowbringer, AOETargetChoice(primaryTarget?.Actor, BestRectTarget?.Actor, sb, strategy), OGCDPrio(sbStrat, OGCDPriority.Average));
                    if (ShouldCarveOrDrain(cdStrat, primaryTarget))
                    {
                        switch (cdStrat)
                        {
                            case CarveStrategy.Automatic:
                                QueueOGCD(ShouldUseAOE ? AID.AbyssalDrain : BestCarve, SingleTargetChoice(primaryTarget?.Actor, cd), cdStrat is CarveStrategy.ForceCarve or CarveStrategy.ForceDrain ? OGCDPriority.Forced : OGCDPriority.BelowAverage);
                                break;
                            case CarveStrategy.OnlyCarve:
                                QueueOGCD(BestCarve, SingleTargetChoice(primaryTarget?.Actor, cd), cdStrat is CarveStrategy.ForceCarve ? OGCDPriority.Forced : OGCDPriority.BelowAverage);
                                break;
                            case CarveStrategy.OnlyDrain:
                                QueueOGCD(AID.AbyssalDrain, Player, cdStrat is CarveStrategy.ForceDrain ? OGCDPriority.Forced : OGCDPriority.BelowAverage);
                                break;
                        }
                    }
                }
                if (!strategy.HoldGauge() && ShouldUseBlood(bloodStrat, primaryTarget))
                {
                    switch (bloodStrat)
                    {
                        case BloodStrategy.Automatic:
                            QueueGCD(BestBloodSpender, SingleTargetChoice(primaryTarget?.Actor, blood), bloodStrat is BloodStrategy.ForceBloodspiller or BloodStrategy.ForceQuietus ? GCDPriority.Forced : RiskingBlood ? GCDPriority.High : GCDPriority.Average);
                            break;
                        case BloodStrategy.OnlyBloodspiller:
                            QueueGCD(AID.Bloodspiller, SingleTargetChoice(primaryTarget?.Actor, blood), bloodStrat is BloodStrategy.ForceBloodspiller ? GCDPriority.Forced : RiskingBlood ? GCDPriority.High : GCDPriority.Average);
                            break;
                        case BloodStrategy.OnlyQuietus:
                            QueueGCD(AID.Quietus, Unlocked(AID.Quietus) ? Player : SingleTargetChoice(primaryTarget?.Actor, blood), bloodStrat is BloodStrategy.ForceQuietus ? GCDPriority.Forced : RiskingBlood ? GCDPriority.High : GCDPriority.Average);
                            break;
                    }
                }
            }
            if (ShouldUseDisesteem(deStrat, primaryTarget))
                QueueGCD(AID.Disesteem, AOETargetChoice(primaryTarget?.Actor, BestRectTarget?.Actor, de, strategy), deStrat is GCDStrategy.Force ? GCDPriority.Forced : CombatTimer < 30 ? GCDPriority.VeryHigh : GCDPriority.AboveAverage);
            if (ShouldUseMP(mpStrat))
            {
                switch (mpStrat)
                {
                    case MPStrategy.Automatic:
                    case MPStrategy.Auto9k:
                    case MPStrategy.Auto6k:
                    case MPStrategy.Auto3k:
                    case MPStrategy.AutoRefresh:
                        if (NumAOERectTargets >= 3 && Unlocked(AID.FloodOfDarkness))
                            QueueOGCD(BestFlood, AOETargetChoice(primaryTarget?.Actor, BestMPTarget?.Actor, mp, strategy), RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low);
                        if (NumAOERectTargets <= 2 && Unlocked(AID.EdgeOfDarkness))
                            QueueOGCD(BestEdge, SingleTargetChoice(primaryTarget?.Actor, mp), RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low);
                        break;
                    case MPStrategy.Edge9k:
                    case MPStrategy.Edge6k:
                    case MPStrategy.Edge3k:
                    case MPStrategy.EdgeRefresh:
                    case MPStrategy.ForceEdge:
                        QueueOGCD(BestEdge, SingleTargetChoice(primaryTarget?.Actor, mp), RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low);
                        break;
                    case MPStrategy.Flood9k:
                    case MPStrategy.Flood6k:
                    case MPStrategy.Flood3k:
                    case MPStrategy.FloodRefresh:
                    case MPStrategy.ForceFlood:
                        QueueOGCD(BestFlood, AOETargetChoice(primaryTarget?.Actor, BestMPTarget?.Actor, mp, strategy), RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low);
                        break;
                }
            }
            if (ShouldSpendDeliriumProcs(dcomboStrat, primaryTarget))
            {
                switch (dcomboStrat)
                {
                    case DeliriumComboStrategy.Automatic:
                        QueueGCD(Delirium.Step is 2 ? AID.Torcleaver : Delirium.Step is 1 ? AID.Comeuppance : Unlocked(AID.ScarletDelirium) && Delirium.IsActive && Delirium.Step is 0 ? ShouldUseAOE ? AID.Impalement : AID.ScarletDelirium : BestBloodSpender, SingleTargetChoice(primaryTarget?.Actor, dcombo), GCDPriority.SlightlyHigh);
                        break;
                    case DeliriumComboStrategy.ScarletDelirum:
                        QueueGCD(Unlocked(AID.ScarletDelirium) ? AID.ScarletDelirium : AID.Bloodspiller, SingleTargetChoice(primaryTarget?.Actor, dcombo), GCDPriority.Forced);
                        break;
                    case DeliriumComboStrategy.Comeuppance:
                        QueueGCD(Unlocked(AID.Comeuppance) ? AID.Comeuppance : AID.Bloodspiller, SingleTargetChoice(primaryTarget?.Actor, dcombo), GCDPriority.Forced);
                        break;
                    case DeliriumComboStrategy.Torcleaver:
                        QueueGCD(Unlocked(AID.Torcleaver) ? AID.Torcleaver : AID.Bloodspiller, SingleTargetChoice(primaryTarget?.Actor, dcombo), GCDPriority.Forced);
                        break;
                    case DeliriumComboStrategy.Impalement:
                        QueueGCD(Unlocked(AID.Impalement) ? AID.Impalement : AID.Quietus, Player, GCDPriority.Forced);
                        break;
                }
            }
            if (ShouldUseSaltAndDarkness(strategy.Option(Track.SaltAndDarkness).As<OGCDStrategy>(), primaryTarget))
                QueueOGCD(AID.SaltAndDarkness, Player, OGCDPriority.AboveAverage);
            if (ShouldUseUnmend(unmendStrat, primaryTarget))
                QueueGCD(AID.Unmend, SingleTargetChoice(primaryTarget?.Actor, unmend), GCDPriority.Low + 1);
            if (strategy.Potion() switch
            {
                PotionStrategy.AlignWithBuffs => Player.InCombat && LivingShadow.CD <= 4f,
                PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
                PotionStrategy.Immediate => true,
                _ => false
            })
                ExecutePotSTR(strategy.Potion() switch
                {
                    PotionStrategy.AlignWithBuffs => GCDPriority.Max,
                    PotionStrategy.AlignWithRaidBuffs => GCDPriority.Max,
                    PotionStrategy.Immediate => GCDPriority.Forced,
                    _ => GCDPriority.None - 3000
                });
            #endregion
        }
        #endregion

        #region AI
        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.Unleash, 3, maximumActionRange: 20);
        #endregion
    }
}
