using BossMod.DRK;
using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiDRK(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Blood = SharedTrack.Count, MP, Carve, DeliriumCombo, Unmend, Delirium, SaltedEarth, SaltAndDarkness, LivingShadow, Shadowbringer, Disesteem }
    public enum BloodStrategy { Automatic, ASAP, OnlyBloodspiller, OnlyQuietus, ForceBloodspiller, ForceQuietus, Conserve, Delay }
    public enum MPStrategy { Optimal, Auto3k, Auto6k, Auto9k, AutoRefresh, Edge3k, Edge6k, Edge9k, EdgeRefresh, Flood3k, Flood6k, Flood9k, FloodRefresh, ForceEdge, ForceFlood, Delay }
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
            .AddOption(BloodStrategy.Automatic, "Automatic", "Automatically use Blood-related abilities optimally")
            .AddOption(BloodStrategy.ASAP, "ASAP", "Automatically use Blood-related abilities ASAP when available", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(BloodStrategy.OnlyBloodspiller, "Only Bloodspiller", "Uses Bloodspiller optimally as Blood spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(BloodStrategy.OnlyQuietus, "Only Quietus", "Uses Quietus optimally as Blood spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(BloodStrategy.ForceBloodspiller, "Force Bloodspiller", "Force use Bloodspiller ASAP when available", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(BloodStrategy.ForceQuietus, "Force Quietus", "Force use Quietus ASAP", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(BloodStrategy.Conserve, "Conserve", "Conserves all Blood-related abilities as much as possible", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(BloodStrategy.Delay, "Delay", "Delay the use of Blood-related abilities", 0, 0, ActionTargets.None, 62)
            .AddAssociatedActions(AID.Bloodspiller, AID.Quietus);
        res.Define(Track.MP).As<MPStrategy>("MP", "MP", uiPriority: 190)
            .AddOption(MPStrategy.Optimal, "Optimal", "Use MP actions optimally; 2 for 1m, 4 (or 5 if Dark Arts is active) for 2m")
            .AddOption(MPStrategy.Auto3k, "Auto 3k", "Automatically decide best MP action to use; Uses when at 3000+ MP", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.Auto6k, "Auto 6k", "Automatically decide best MP action to use; Uses when at 6000+ MP", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.Auto9k, "Auto 9k", "Automatically decide best MP action to use; Uses when at 9000+ MP", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.AutoRefresh, "Auto Refresh", "Automatically decide best MP action to use as Darkside refresher only", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.Edge3k, "Edge 3k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 3000+ MP", 0, 0, ActionTargets.Hostile, 40)
            .AddOption(MPStrategy.Edge6k, "Edge 6k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 6000+ MP", 0, 0, ActionTargets.Hostile, 40)
            .AddOption(MPStrategy.Edge9k, "Edge 9k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 9000+ MP", 0, 0, ActionTargets.Hostile, 40)
            .AddOption(MPStrategy.EdgeRefresh, "Edge Refresh", "Use Edge abilities as Darkside refresher only", 0, 0, ActionTargets.Hostile, 40)
            .AddOption(MPStrategy.Flood3k, "Flood 3k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 3000+ MP", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.Flood6k, "Flood 6k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 6000+ MP", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.Flood9k, "Flood 9k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 9000+ MP", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.FloodRefresh, "Flood Refresh", "Use Flood abilities as Darkside refresher only", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.ForceEdge, "Force Edge", "Force use Edge abilities ASAP if MP is available", 0, 0, ActionTargets.Hostile, 40)
            .AddOption(MPStrategy.ForceFlood, "Force Flood", "Force use Flood abilities ASAP if MP is available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(MPStrategy.Delay, "Delay", "Delay the use of MP actions", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.EdgeOfDarkness, AID.EdgeOfShadow, AID.FloodOfDarkness, AID.FloodOfShadow);
        res.Define(Track.Carve).As<CarveStrategy>("Carve & Spit / Abyssal Drain", "Carve", uiPriority: 160)
            .AddOption(CarveStrategy.Automatic, "Auto", "Automatically decide when to use either Carve and Spit or Abyssal Drain")
            .AddOption(CarveStrategy.OnlyCarve, "Only Carve and Spit", "Automatically use Carve and Spit as optimal spender, regardless of targets", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(CarveStrategy.OnlyDrain, "Only Abysssal Drain", "Automatically use Abyssal Drain as optimal spender, regardless of targets", 0, 0, ActionTargets.Hostile, 56)
            .AddOption(CarveStrategy.ForceCarve, "Force Carve and Spit", "Force use Carve and Spit ASAP when available", 60, 0, ActionTargets.Hostile, 60)
            .AddOption(CarveStrategy.ForceDrain, "Force Abyssal Drain", "Force use Abyssal Drain ASAP when available", 60, 0, ActionTargets.Hostile, 56)
            .AddOption(CarveStrategy.Delay, "Delay", "Delay the use of Carve and Spit", 0, 0, ActionTargets.None, 56)
            .AddAssociatedActions(AID.CarveAndSpit, AID.AbyssalDrain);
        res.Define(Track.DeliriumCombo).As<DeliriumComboStrategy>("Delirium Combo", "Scarlet", uiPriority: 180)
            .AddOption(DeliriumComboStrategy.Automatic, "Auto", "Automatically decide when to use Delirium Combo", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.ScarletDelirum, "Scarlet Delirium", "Force use Scarlet Delirium ASAP when available", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.Comeuppance, "Comeuppance", "Force use Comeuppance ASAP when available", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.Torcleaver, "Torcleaver", "Force use Torcleaver ASAP when available", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.Impalement, "Impalement", "Force use Impalement ASAP when available", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.Delay, "Delay", "Delay use of Scarlet combo", 0, 0, ActionTargets.Hostile, 96)
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

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.SyphonStrike => !Unlocked(AID.Souleater) ? AutoBreak : FullST,
        AID.HardSlash => !Unlocked(AID.SyphonStrike) ? AutoBreak : FullST,
        AID.Unleash => !Unlocked(AID.StalwartSoul) ? AutoBreak : FullAOE,
        AID.Souleater or AID.StalwartSoul or _ => AutoBreak,
    };
    private AID AutoBreak => ShouldUseAOE ? FullAOE : FullST;
    private AID FullST => Unlocked(AID.Souleater) && ComboLastMove is AID.SyphonStrike ? AID.Souleater : Unlocked(AID.SyphonStrike) && ComboLastMove is AID.HardSlash ? AID.SyphonStrike : AID.HardSlash;
    private AID FullAOE => Unlocked(AID.StalwartSoul) && ComboLastMove is AID.Unleash ? AID.StalwartSoul : AID.Unleash;

    #region Upgrade Paths
    private AID BestEdge => Unlocked(AID.EdgeOfShadow) ? AID.EdgeOfShadow : Unlocked(AID.EdgeOfDarkness) ? AID.EdgeOfDarkness : AID.FloodOfDarkness;
    private AID BestFlood => Unlocked(AID.FloodOfShadow) ? AID.FloodOfShadow : AID.FloodOfDarkness;
    private AID BestBloodSpender => ShouldUseAOE && Unlocked(AID.Quietus) ? AID.Quietus : AID.Bloodspiller;
    private AID BestDelirium => Unlocked(AID.Delirium) ? AID.Delirium : AID.BloodWeapon;
    private AID BestCarve => Unlocked(AID.CarveAndSpit) ? AID.CarveAndSpit : AID.AbyssalDrain;
    private SID BestBloodWeapon => Unlocked(AID.ScarletDelirium) ? SID.EnhancedDelirium : Unlocked(AID.Delirium) ? SID.Delirium : SID.BloodWeapon;
    private AID DeliriumCombo => Delirium.Step is 2 ? AID.Torcleaver : Delirium.Step is 1 ? AID.Comeuppance : ShouldUseAOE ? AID.Impalement : Unlocked(AID.ScarletDelirium) && Delirium.IsActive && Delirium.Step is 0 ? AID.ScarletDelirium : BestBloodSpender;
    #endregion

    #endregion

    #region Cooldown Helpers
    private bool ShouldUseMP(MPStrategy strategy)
    {
        if (!CanWeaveIn || !Unlocked(AID.FloodOfDarkness))
            return false;
        if (strategy == MPStrategy.Optimal)
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
        var condition = Player.InCombat && target != null && InMeleeRange(target?.Actor) && minimum && Darkside.IsActive && (RiskingBlood || !InOddWindow(AID.LivingShadow) ? (!CanFitSkSGCD(Delirium.Left, 3) || RaidBuffsLeft > 0f) : minimum || !CanFitSkSGCD(DowntimeIn, 3));
        return strategy switch
        {
            BloodStrategy.Automatic or BloodStrategy.OnlyBloodspiller or BloodStrategy.OnlyQuietus => condition,
            BloodStrategy.ASAP or BloodStrategy.ForceBloodspiller or BloodStrategy.ForceQuietus => minimum,
            BloodStrategy.Conserve => RiskingBlood || Delirium.Left > GCD,
            _ => false
        };
    }
    private bool ShouldUseCarveOrDrain(CarveStrategy strategy, Enemy? target) => strategy switch
    {
        CarveStrategy.Automatic or CarveStrategy.OnlyCarve or CarveStrategy.OnlyDrain => Player.InCombat && target != null && CanWeaveIn && In3y(target?.Actor) && Darkside.IsActive && AbyssalDrain.IsReady && Opener,
        CarveStrategy.ForceCarve => CarveAndSpit.IsReady,
        CarveStrategy.ForceDrain => AbyssalDrain.IsReady,
        CarveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseSaltedEarth(OGCDStrategy strategy, Enemy? target) => SaltedEarth.IsReady && strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && In3y(target?.Actor) && Darkside.IsActive && Opener,
        OGCDStrategy.RaidBuffsOnly => RaidBuffsLeft > 0f && Player.InCombat && target != null && CanWeaveIn && In3y(target?.Actor) && Darkside.IsActive,
        OGCDStrategy.Force => true,
        OGCDStrategy.AnyWeave => CanWeaveIn,
        OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => CanLateWeaveIn,
        _ => false
    };
    private bool ShouldUseSaltAndDarkness(OGCDStrategy strategy, Enemy? target) => SaltedEarth.IsActive && strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target?.Actor != null && CanWeaveIn && CDRemaining(AID.SaltAndDarkness) < 0.6f && (SaltedEarth.Left is <= 5f || RaidBuffsLeft > 0f || !CanFitSkSGCD(DowntimeIn, 3)),
        OGCDStrategy.RaidBuffsOnly => Player.InCombat && target?.Actor != null && CanWeaveIn && CDRemaining(AID.SaltAndDarkness) < 0.6f && RaidBuffsLeft > 0f,
        OGCDStrategy.Force => true,
        OGCDStrategy.AnyWeave => CanWeaveIn,
        OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => CanLateWeaveIn,
        _ => false
    };
    private bool ShouldUseDelirium(OGCDStrategy strategy, Enemy? target) => ShouldUseOGCD(strategy, target?.Actor, Delirium.IsReady, InsideCombatWith(target?.Actor) && Darkside.IsActive && (Unlocked(AID.Delirium) ? Delirium.IsReady : ActionReady(AID.BloodWeapon)) && (Unlocked(AID.LivingShadow) ? Opener : CombatTimer > 0));
    private bool ShouldUseLivingShadow(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && Darkside.IsActive && LivingShadow.IsReady,
        OGCDStrategy.Force => LivingShadow.IsReady,
        OGCDStrategy.AnyWeave => LivingShadow.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => LivingShadow.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => LivingShadow.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay or _ => false
    };
    private bool ShouldUseShadowbringer(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && Darkside.IsActive && Shadowbringer.IsReady && (RaidBuffsLeft > 0f || (!Delirium.IsActive && StatusRemaining(Player, SID.Scorn) is < 20f and not 0f)),
        OGCDStrategy.Force => Shadowbringer.IsReady,
        OGCDStrategy.AnyWeave => Shadowbringer.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => Shadowbringer.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => Shadowbringer.IsReady && CanLateWeaveIn,
        _ => false
    };
    private bool ShouldUseDeliriumCombo(DeliriumComboStrategy strategy, Enemy? target) => Delirium.IsActive && strategy switch
    {
        DeliriumComboStrategy.Automatic => Player.InCombat && target != null && In3y(target?.Actor) && Unlocked(AID.ScarletDelirium) && (!InOddWindow(AID.LivingShadow) ? (RaidBuffsLeft > 0f || !CanFitSkSGCD(Delirium.Left, 3)) : Delirium.IsActive) && Delirium.Step is 0 or 1 or 2,
        DeliriumComboStrategy.ScarletDelirum => Unlocked(AID.ScarletDelirium) && Delirium.Step is 0,
        DeliriumComboStrategy.Comeuppance => Unlocked(AID.Comeuppance) && Delirium.Step is 1,
        DeliriumComboStrategy.Torcleaver => Unlocked(AID.Torcleaver) && Delirium.Step is 2,
        DeliriumComboStrategy.Impalement => Unlocked(AID.Impalement) && Delirium.Step is 0,
        DeliriumComboStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseDisesteem(GCDStrategy strategy, Enemy? target) => strategy switch
    {
        GCDStrategy.Automatic => Player.InCombat && target != null && In10y(target?.Actor) && Darkside.IsActive && Disesteem.IsReady && (RaidBuffsLeft > 0 || StatusRemaining(Player, SID.Scorn) < 10f),
        GCDStrategy.Force => Disesteem.IsReady,
        GCDStrategy.RaidBuffsOnly => RaidBuffsLeft > 0f,
        GCDStrategy.Delay => false,
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
    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Potion() switch
    {
        PotionStrategy.AlignWithBuffs => Player.InCombat && LivingShadow.CD <= 4f,
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<DarkKnightGauge>(); //Retrieve DRK gauge
        Blood = gauge.Blood;
        DarkArts.State = gauge.DarkArtsState; //Retrieve current Dark Arts state
        DarkArts.IsActive = DarkArts.State > 0; //Checks if Dark Arts is active
        Darkside.Timer = gauge.DarksideTimer / 1000f; //Retrieve current Darkside timer
        Darkside.IsActive = Darkside.Timer > 0.1f; //Checks if Darkside is active
        Darkside.NeedsRefresh = Darkside.Timer <= 3; //Checks if Darkside needs to be refreshed
        RiskingBlood = ComboLastMove is AID.SyphonStrike or AID.Unleash && Blood >= 80 || Delirium.CD <= 3 && Blood >= 70; //Checks if we are risking Blood
        RiskingMP = MP >= 10000 || Darkside.NeedsRefresh;
        SaltedEarth.Left = StatusRemaining(Player, SID.SaltedEarth, 15); //Retrieve current Salted Earth time left
        SaltedEarth.CD = CDRemaining(AID.SaltedEarth); //Retrieve current Salted Earth cooldown
        SaltedEarth.IsActive = SaltedEarth.Left > 0.1f; //Checks if Salted Earth is active
        SaltedEarth.IsReady = Unlocked(AID.SaltedEarth) && SaltedEarth.CD < 0.6f; //Salted Earth ability
        AbyssalDrain.CD = CDRemaining(AID.AbyssalDrain); //Retrieve current Abyssal Drain cooldown
        AbyssalDrain.IsReady = Unlocked(AID.AbyssalDrain) && AbyssalDrain.CD < 0.6f; //Abyssal Drain ability
        CarveAndSpit.CD = CDRemaining(AID.CarveAndSpit); //Retrieve current Carve and Spit cooldown
        CarveAndSpit.IsReady = Unlocked(AID.CarveAndSpit) && CarveAndSpit.CD < 0.6f; //Carve and Spit ability
        Disesteem.Left = StatusRemaining(Player, SID.Scorn, 30); //Retrieve current Disesteem time left
        Disesteem.IsActive = Disesteem.Left > 0.1f; //Checks if Disesteem is active
        Disesteem.IsReady = Unlocked(AID.Disesteem) && Disesteem.Left > 0.1f; //Disesteem ability
        Delirium.Step = gauge.DeliriumStep; //Retrieve current Delirium combo step
        Delirium.Left = StatusRemaining(Player, BestBloodWeapon, 15); //Retrieve current Delirium time left
        Delirium.Stacks = StacksRemaining(Player, BestBloodWeapon, 15); //Retrieve current Delirium stacks
        Delirium.CD = CDRemaining(BestDelirium); //Retrieve current Delirium cooldown
        Delirium.IsActive = Delirium.Left > 0.1f; //Checks if Delirium is active
        Delirium.IsReady = Unlocked(BestDelirium) && Delirium.CD < 0.6f; //Delirium ability
        LivingShadow.Timer = gauge.ShadowTimer / 1000f; //Retrieve current Living Shadow timer
        LivingShadow.CD = CDRemaining(AID.LivingShadow); //Retrieve current Living Shadow cooldown
        LivingShadow.IsActive = LivingShadow.Timer > 0; //Checks if Living Shadow is active
        LivingShadow.IsReady = Unlocked(AID.LivingShadow) && LivingShadow.CD < 0.6f; //Living Shadow ability
        Shadowbringer.CDRemaining = CDRemaining(AID.Shadowbringer); //Retrieve current Shadowbringer cooldown
        Shadowbringer.ReadyIn = ReadyIn(AID.Shadowbringer); //Retrieve current Shadowbringer charge cooldown
        Shadowbringer.HasCharges = CDRemaining(AID.Shadowbringer) <= 60; //Checks if Shadowbringer has charges
        Shadowbringer.IsReady = Unlocked(AID.Shadowbringer) && Shadowbringer.HasCharges; //Shadowbringer ability
        Opener = (CombatTimer < 30 && ComboLastMove is AID.Souleater) || CombatTimer >= 30;
        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore;
        (BestAOERectTargets, NumAOERectTargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        BestRectTarget = Unlocked(AID.Shadowbringer) && NumAOERectTargets > 1 ? BestAOERectTargets : primaryTarget;
        var BestHighMPTarget = NumAOERectTargets > 2 ? BestAOERectTargets : primaryTarget;
        var BestLowMPTarget = NumAOERectTargets > 3 ? BestAOERectTargets : primaryTarget;
        BestMPTarget = Unlocked(AID.FloodOfShadow) ? BestHighMPTarget : BestLowMPTarget;

        #region Strategy Definitions
        var mp = strategy.Option(Track.MP);
        var mpStrat = mp.As<MPStrategy>(); //Retrieve MP strategy
        var blood = strategy.Option(Track.Blood);
        var bloodStrat = blood.As<BloodStrategy>(); //Retrieve Blood strategy
        var se = strategy.Option(Track.SaltedEarth);
        var seStrat = se.As<OGCDStrategy>(); //Retrieve Salted Earth strategy
        var cd = strategy.Option(Track.Carve);
        var cdStrat = cd.As<CarveStrategy>(); //Retrieve Carve and Drain strategy
        var deli = strategy.Option(Track.Delirium);
        var deliStrat = deli.As<OGCDStrategy>(); //Retrieve Delirium strategy
        var ls = strategy.Option(Track.LivingShadow);
        var lsStrat = ls.As<OGCDStrategy>(); //Retrieve Living Shadow strategy
        var sb = strategy.Option(Track.Shadowbringer);
        var sbStrat = sb.As<OGCDStrategy>(); //Retrieve Shadowbringer strategy
        var dcombo = strategy.Option(Track.DeliriumCombo);
        var dcomboStrat = dcombo.As<DeliriumComboStrategy>(); //Retrieve Delirium combo strategy
        var de = strategy.Option(Track.Disesteem);
        var deStrat = de.As<GCDStrategy>(); //Retrieve Disesteem strategy
        var unmend = strategy.Option(Track.Unmend);
        var unmendStrat = unmend.As<UnmendStrategy>(); //Retrieve Unmend strategy
        #endregion

        #endregion

        #region Full Rotation Execution
        if (!strategy.HoldEverything())
        {
            #region Standard Rotations
            if (strategy.AutoFinish())
                QueueGCD(AutoFinish, SingleTargetChoice(primaryTarget?.Actor, strategy.Option(SharedTrack.AOE)), GCDPriority.Low);
            if (strategy.AutoBreak())
                QueueGCD(AutoBreak, SingleTargetChoice(primaryTarget?.Actor, strategy.Option(SharedTrack.AOE)), GCDPriority.Low);
            if (strategy.ForceST())
                QueueGCD(FullST, SingleTargetChoice(primaryTarget?.Actor, strategy.Option(SharedTrack.AOE)), GCDPriority.Low);
            if (strategy.ForceAOE())
                QueueGCD(FullAOE, Player, GCDPriority.Low);
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
                    if (ShouldUseCarveOrDrain(cdStrat, primaryTarget))
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
                    case MPStrategy.Optimal:
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
            if (ShouldUseDeliriumCombo(dcomboStrat, primaryTarget))
            {
                switch (dcomboStrat)
                {
                    case DeliriumComboStrategy.Automatic:
                        QueueGCD(DeliriumCombo, SingleTargetChoice(primaryTarget?.Actor, dcombo), GCDPriority.SlightlyHigh);
                        break;
                    case DeliriumComboStrategy.ScarletDelirum:
                        QueueGCD(AID.ScarletDelirium, SingleTargetChoice(primaryTarget?.Actor, dcombo), GCDPriority.Forced);
                        break;
                    case DeliriumComboStrategy.Comeuppance:
                        QueueGCD(AID.Comeuppance, SingleTargetChoice(primaryTarget?.Actor, dcombo), GCDPriority.Forced);
                        break;
                    case DeliriumComboStrategy.Torcleaver:
                        QueueGCD(AID.Torcleaver, SingleTargetChoice(primaryTarget?.Actor, dcombo), GCDPriority.Forced);
                        break;
                    case DeliriumComboStrategy.Impalement:
                        QueueGCD(AID.Impalement, Player, GCDPriority.Forced);
                        break;
                }
            }
            if (ShouldUseSaltAndDarkness(strategy.Option(Track.SaltAndDarkness).As<OGCDStrategy>(), primaryTarget))
                QueueOGCD(AID.SaltAndDarkness, Player, OGCDPriority.AboveAverage);
            if (ShouldUseUnmend(unmendStrat, primaryTarget))
                QueueGCD(AID.Unmend, SingleTargetChoice(primaryTarget?.Actor, unmend), GCDPriority.Low);
            if (ShouldUsePotion(strategy))
                Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Forced, 0, GCD - 0.9f);
            #endregion
        }
        #endregion

        #region AI
        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.Unleash, 3, maximumActionRange: 20);
        #endregion
    }
}
