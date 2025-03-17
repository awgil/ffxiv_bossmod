using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.DRK;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiDRK(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Blood = SharedTrack.Count, MP, Carve, DeliriumCombo, Potion, Unmend, Delirium, SaltedEarth, SaltAndDarkness, LivingShadow, Shadowbringer, Disesteem }
    public enum BloodStrategy { Automatic, OnlyBloodspiller, OnlyQuietus, ForceBloodspiller, ForceQuietus, Conserve }
    public enum MPStrategy { Optimal, Auto3k, Auto6k, Auto9k, AutoRefresh, Edge3k, Edge6k, Edge9k, EdgeRefresh, Flood3k, Flood6k, Flood9k, FloodRefresh, Delay }
    public enum CarveStrategy { Automatic, OnlyCarve, OnlyDrain, ForceCarve, ForceDrain, Delay }
    public enum DeliriumComboStrategy { Automatic, ScarletDelirum, Comeuppance, Torcleaver, Impalement, Delay }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    public enum UnmendStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRK", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DRK), 100);

        res.DefineAOE().AddAssociatedActions(AID.HardSlash, AID.SyphonStrike, AID.Souleater, AID.Unleash, AID.StalwartSoul);
        res.DefineHold();
        res.Define(Track.Blood).As<BloodStrategy>("Blood", "Blood", uiPriority: 200)
            .AddOption(BloodStrategy.Automatic, "Automatic", "Automatically use Blood-related abilities optimally")
            .AddOption(BloodStrategy.OnlyBloodspiller, "Only Bloodspiller", "Uses Bloodspiller optimally as Blood spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(BloodStrategy.OnlyQuietus, "Only Quietus", "Uses Quietus optimally as Blood spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(BloodStrategy.ForceBloodspiller, "Force Bloodspiller", "Force use Bloodspiller ASAP", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(BloodStrategy.ForceQuietus, "Force Quietus", "Force use Quietus ASAP", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(BloodStrategy.Conserve, "Conserve", "Conserves all Blood-related abilities as much as possible")
            .AddAssociatedActions(AID.Bloodspiller, AID.Quietus);
        res.Define(Track.MP).As<MPStrategy>("MP", "MP", uiPriority: 190)
            .AddOption(MPStrategy.Optimal, "Optimal", "Use MP actions optimally; 2 for 1 minute, 4 (or 5 if Dark Arts is active) for 2 minutes")
            .AddOption(MPStrategy.Auto3k, "Auto 3k", "Automatically decide best MP action to use; Uses when at 3000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Auto6k, "Auto 6k", "Automatically decide best MP action to use; Uses when at 6000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Auto9k, "Auto 9k", "Automatically decide best MP action to use; Uses when at 9000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.AutoRefresh, "Auto Refresh", "Automatically decide best MP action to use as Darkside refresher only", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Edge3k, "Edge 3k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 3000+ MP", 0, 0, ActionTargets.Self, 40)
            .AddOption(MPStrategy.Edge6k, "Edge 6k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 6000+ MP", 0, 0, ActionTargets.Self, 40)
            .AddOption(MPStrategy.Edge9k, "Edge 9k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 9000+ MP", 0, 0, ActionTargets.Self, 40)
            .AddOption(MPStrategy.EdgeRefresh, "Edge Refresh", "Use Edge abilities as Darkside refresher only", 0, 0, ActionTargets.Self, 40)
            .AddOption(MPStrategy.Flood3k, "Flood 3k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 3000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Flood6k, "Flood 6k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 6000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Flood9k, "Flood 9k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 9000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.FloodRefresh, "Flood Refresh", "Use Flood abilities as Darkside refresher only", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Delay, "Delay", "Delay the use of MP actions for strategic reasons", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.EdgeOfDarkness, AID.EdgeOfShadow, AID.FloodOfDarkness, AID.FloodOfShadow);
        res.Define(Track.Carve).As<CarveStrategy>("Carve & Spit / Abyssal Drain", "Carve", uiPriority: 160)
            .AddOption(CarveStrategy.Automatic, "Auto", "Automatically decide when to use either Carve and Spit or Abyssal Drain")
            .AddOption(CarveStrategy.OnlyCarve, "Only Carve and Spit", "Automatically use Carve and Spit as optimal spender, regardless of targets", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(CarveStrategy.OnlyDrain, "Only Abysssal Drain", "Automatically use Abyssal Drain as optimal spender, regardless of targets", 0, 0, ActionTargets.Hostile, 56)
            .AddOption(CarveStrategy.ForceCarve, "Force Carve and Spit", "Force use Carve and Spit ASAP", 60, 0, ActionTargets.Hostile, 60)
            .AddOption(CarveStrategy.ForceDrain, "Force Abyssal Drain", "Force use Abyssal Drain ASAP", 60, 0, ActionTargets.Hostile, 56)
            .AddOption(CarveStrategy.Delay, "Delay", "Delay the use of Carve and Spit for strategic reasons", 0, 0, ActionTargets.None, 56)
            .AddAssociatedActions(AID.CarveAndSpit, AID.AbyssalDrain);
        res.Define(Track.DeliriumCombo).As<DeliriumComboStrategy>("Delirium Combo", "Scarlet", uiPriority: 180)
            .AddOption(DeliriumComboStrategy.Automatic, "Auto", "Automatically decide when to use Delirium Combo", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.ScarletDelirum, "Scarlet Delirium", "Force use Scarlet Delirium ASAP", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.Comeuppance, "Comeuppance", "Force use Comeuppance ASAP", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.Torcleaver, "Torcleaver", "Force use Torcleaver ASAP", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.Impalement, "Impalement", "Force use Impalement ASAP", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(DeliriumComboStrategy.Delay, "Delay", "Delay use of Scarlet combo for strategic reasons", 0, 0, ActionTargets.Hostile, 96)
            .AddAssociatedActions(AID.ScarletDelirium, AID.Comeuppance, AID.Torcleaver, AID.Impalement);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 20)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with Living Shadow (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.Unmend).As<UnmendStrategy>("Ranged", "Ranged", uiPriority: 30)
            .AddOption(UnmendStrategy.OpenerFar, "Far (Opener)", "Use Unmend in pre-pull & out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.OpenerForce, "Force (Opener)", "Force use Unmend in pre-pull in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Force, "Force", "Force use Unmend in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Allow, "Allow", "Allow use of Unmend when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Forbid, "Forbid", "Prohibit use of Unmend")
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
    public byte Blood;
    public (byte State, bool IsActive) DarkArts;
    public (float Timer, bool IsActive, bool NeedsRefresh) Darkside;
    public bool RiskingBlood;
    public bool RiskingMP;
    public (float Left, float CD, bool IsActive, bool IsReady) SaltedEarth;
    public (float CD, bool IsReady) AbyssalDrain;
    public (float CD, bool IsReady) CarveAndSpit;
    public (ushort Step, float Left, int Stacks, float CD, bool IsActive, bool IsReady) Delirium;
    public (float Timer, float CD, bool IsActive, bool IsReady) LivingShadow;
    public (float TotalCD, float ChargeCD, bool HasCharges, bool IsReady) Shadowbringer;
    public (float Left, bool IsActive, bool IsReady) Disesteem;
    private bool ShouldUseAOE;
    public int NumAOERectTargets;
    public int NumMPRectTargets;
    public Enemy? BestAOERectTargets;
    public Enemy? BestMPRectTargets;
    public Enemy? BestTargetAOERect;
    public Enemy? BestTargetMPRectHigh;
    public Enemy? BestTargetMPRectLow;
    public Enemy? BestTargetMPRect;
    private bool inOdd;
    #endregion

    #region Upgrade Paths
    private AID BestEdge => Unlocked(AID.EdgeOfShadow) ? AID.EdgeOfShadow : Unlocked(AID.EdgeOfDarkness) ? AID.EdgeOfDarkness : AID.FloodOfDarkness;
    private AID BestFlood => !Unlocked(AID.FloodOfShadow) ? AID.FloodOfDarkness : AID.FloodOfShadow;
    private AID BestQuietus => Unlocked(AID.Quietus) ? AID.Quietus : AID.Bloodspiller;
    private AID BestBloodSpender => ShouldUseAOE ? BestQuietus : AID.Bloodspiller;
    private AID BestDelirium => Unlocked(AID.Delirium) ? AID.Delirium : AID.BloodWeapon;
    private AID CarveOrDrain => ShouldUseAOE ? AID.AbyssalDrain : BestCarve;
    private AID BestCarve => Unlocked(AID.CarveAndSpit) ? AID.CarveAndSpit : AID.AbyssalDrain;
    private SID BestBloodWeapon => Unlocked(AID.ScarletDelirium) ? SID.EnhancedDelirium : Unlocked(AID.Delirium) ? SID.Delirium : SID.BloodWeapon;
    private AID DeliriumCombo => Delirium.Step is 2 ? AID.Torcleaver : Delirium.Step is 1 ? AID.Comeuppance : ShouldUseAOE ? AID.Impalement : AID.ScarletDelirium;
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.SyphonStrike or AID.HardSlash => FullST,
        AID.Unleash => FullAOE,
        AID.Souleater or AID.StalwartSoul or _ => ShouldUseAOE ? FullAOE : FullST,
    };
    private AID AutoBreak => ShouldUseAOE ? FullAOE : FullST;
    private AID FullST => Unlocked(AID.Souleater) && ComboLastMove is AID.SyphonStrike ? AID.Souleater : Unlocked(AID.SyphonStrike) && ComboLastMove is AID.HardSlash ? AID.SyphonStrike : AID.HardSlash;
    private AID FullAOE => Unlocked(AID.StalwartSoul) && ComboLastMove is AID.Unleash ? AID.StalwartSoul : Unlocked(AID.Unleash) ? AID.Unleash : FullST;
    #endregion

    #region Cooldown Helpers

    #region MP
    private bool ShouldSpendMP(MPStrategy strategy)
    {
        if (strategy != MPStrategy.Optimal)
            return false;
        if (strategy == MPStrategy.Optimal)
        {
            if (RiskingMP)
                return true;

            if (DarkArts.IsActive)
            {
                if (Delirium.CD >= 40)
                    return true;
                if (Delirium.CD >= Darkside.Timer + GCD)
                    return true;
            }
            //2 uses
            if (Delirium.CD >= 40 && inOdd)
                return MP >= 6000;
            //4 uses (5 with DA)
            if (Delirium.CD >= 40 && !inOdd)
                return MP >= 3000;
        }
        return false;
    }
    private bool ShouldUseMP(MPStrategy strategy) => strategy switch
    {
        MPStrategy.Optimal => ShouldSpendMP(MPStrategy.Optimal),
        MPStrategy.Auto3k => CanWeaveIn && MP >= 3000,
        MPStrategy.Auto6k => CanWeaveIn && MP >= 6000,
        MPStrategy.Auto9k => CanWeaveIn && MP >= 9000,
        MPStrategy.AutoRefresh => CanWeaveIn && RiskingMP,
        MPStrategy.Edge3k => CanWeaveIn && MP >= 3000,
        MPStrategy.Edge6k => CanWeaveIn && MP >= 6000,
        MPStrategy.Edge9k => CanWeaveIn && MP >= 9000,
        MPStrategy.EdgeRefresh => CanWeaveIn && RiskingMP,
        MPStrategy.Flood3k => CanWeaveIn && MP >= 3000,
        MPStrategy.Flood6k => CanWeaveIn && MP >= 6000,
        MPStrategy.Flood9k => CanWeaveIn && MP >= 9000,
        MPStrategy.FloodRefresh => CanWeaveIn && RiskingMP,
        MPStrategy.Delay => false,
        _ => false
    };
    #endregion

    private bool ShouldUseBlood(BloodStrategy strategy, Enemy? target) => strategy switch
    {
        BloodStrategy.Automatic => ShouldSpendBlood(BloodStrategy.Automatic, target),
        BloodStrategy.OnlyBloodspiller => ShouldSpendBlood(BloodStrategy.Automatic, target),
        BloodStrategy.OnlyQuietus => ShouldSpendBlood(BloodStrategy.Automatic, target),
        BloodStrategy.ForceBloodspiller => Unlocked(AID.Bloodspiller) && (Blood >= 50 || Delirium.IsActive),
        BloodStrategy.ForceQuietus => Unlocked(AID.Quietus) && (Blood >= 50 || Delirium.IsActive),
        BloodStrategy.Conserve => false,
        _ => false
    };
    private bool ShouldSpendBlood(BloodStrategy strategy, Enemy? target) => strategy switch
    {
        BloodStrategy.Automatic => Player.InCombat && target != null && In3y(target?.Actor) && Blood >= 50 && Darkside.IsActive && Unlocked(AID.Bloodspiller) && (RiskingBlood || Delirium.CD >= 39.5f),
        _ => false
    };
    private bool ShouldUseSaltedEarth(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && In3y(target?.Actor) && Darkside.IsActive && SaltedEarth.IsReady && (CombatTimer < 30 && ComboLastMove is AID.Souleater || CombatTimer >= 30),
        OGCDStrategy.Force => SaltedEarth.IsReady,
        OGCDStrategy.AnyWeave => SaltedEarth.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => SaltedEarth.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => SaltedEarth.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseSaltAndDarkness(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target?.Actor != null && CanWeaveIn && TotalCD(AID.SaltAndDarkness) < 0.6f && SaltedEarth.IsActive,
        OGCDStrategy.Force => SaltedEarth.IsActive,
        OGCDStrategy.AnyWeave => SaltedEarth.IsActive && CanWeaveIn,
        OGCDStrategy.EarlyWeave => SaltedEarth.IsActive && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => SaltedEarth.IsActive && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseCarveOrDrain(CarveStrategy strategy, Enemy? target) => strategy switch
    {
        CarveStrategy.Automatic => ShouldSpendCarveOrDrain(CarveStrategy.Automatic, target),
        CarveStrategy.OnlyCarve => ShouldSpendCarveOrDrain(CarveStrategy.Automatic, target),
        CarveStrategy.OnlyDrain => ShouldSpendCarveOrDrain(CarveStrategy.Automatic, target),
        CarveStrategy.ForceCarve => CarveAndSpit.IsReady,
        CarveStrategy.ForceDrain => AbyssalDrain.IsReady,
        CarveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldSpendCarveOrDrain(CarveStrategy strategy, Enemy? target) => strategy switch
    {
        CarveStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && In3y(target?.Actor) && Darkside.IsActive && AbyssalDrain.IsReady && (CombatTimer < 30 && ComboLastMove is AID.Souleater || CombatTimer >= 30),
        _ => false
    };
    private bool ShouldUseDelirium(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && Darkside.IsActive && Delirium.IsReady && (CombatTimer < 30 && ComboLastMove is AID.Souleater || CombatTimer >= 30),
        OGCDStrategy.Force => Delirium.IsReady,
        OGCDStrategy.AnyWeave => Delirium.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => Delirium.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => Delirium.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseLivingShadow(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && Darkside.IsActive && LivingShadow.IsReady,
        OGCDStrategy.Force => LivingShadow.IsReady,
        OGCDStrategy.AnyWeave => LivingShadow.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => LivingShadow.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => LivingShadow.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseShadowbringer(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && Darkside.IsActive && Shadowbringer.IsReady && LivingShadow.IsActive && Delirium.IsActive,
        OGCDStrategy.Force => Shadowbringer.IsReady,
        OGCDStrategy.AnyWeave => Shadowbringer.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => Shadowbringer.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => Shadowbringer.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseDeliriumCombo(DeliriumComboStrategy strategy, Enemy? target) => strategy switch
    {
        DeliriumComboStrategy.Automatic => Player.InCombat && target != null && In3y(target?.Actor) && Unlocked(AID.ScarletDelirium) && Delirium.Step is 0 or 1 or 2 && Delirium.IsActive,
        DeliriumComboStrategy.ScarletDelirum => Unlocked(AID.ScarletDelirium) && Delirium.Step is 0 && Delirium.IsActive,
        DeliriumComboStrategy.Comeuppance => Unlocked(AID.Comeuppance) && Delirium.Step is 1 && Delirium.IsActive,
        DeliriumComboStrategy.Torcleaver => Unlocked(AID.Torcleaver) && Delirium.Step is 2 && Delirium.IsActive,
        DeliriumComboStrategy.Impalement => Unlocked(AID.Impalement) && Delirium.Step is 0 && Delirium.IsActive,
        DeliriumComboStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseDisesteem(GCDStrategy strategy, Enemy? target) => strategy switch
    {
        GCDStrategy.Automatic => Player.InCombat && target != null && In10y(target?.Actor) && Darkside.IsActive && Disesteem.IsReady && (CombatTimer < 30 && Delirium.IsActive || CombatTimer >= 30 && !Delirium.IsActive && Delirium.CD > 15),
        GCDStrategy.Force => Disesteem.IsReady,
        GCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseUnmend(UnmendStrategy strategy, Enemy? target) => strategy switch
    {
        UnmendStrategy.OpenerFar => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD() && !In3y(target?.Actor),
        UnmendStrategy.OpenerForce => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD(),
        UnmendStrategy.Force => true,
        UnmendStrategy.Allow => !In3y(target?.Actor),
        UnmendStrategy.Forbid => false,
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => LivingShadow.CD < 5,
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
        SaltedEarth.CD = TotalCD(AID.SaltedEarth); //Retrieve current Salted Earth cooldown
        SaltedEarth.IsActive = SaltedEarth.Left > 0.1f; //Checks if Salted Earth is active
        SaltedEarth.IsReady = Unlocked(AID.SaltedEarth) && SaltedEarth.CD < 0.6f; //Salted Earth ability
        AbyssalDrain.CD = TotalCD(AID.AbyssalDrain); //Retrieve current Abyssal Drain cooldown
        AbyssalDrain.IsReady = Unlocked(AID.AbyssalDrain) && AbyssalDrain.CD < 0.6f; //Abyssal Drain ability
        CarveAndSpit.CD = TotalCD(AID.CarveAndSpit); //Retrieve current Carve and Spit cooldown
        CarveAndSpit.IsReady = Unlocked(AID.CarveAndSpit) && CarveAndSpit.CD < 0.6f; //Carve and Spit ability
        Disesteem.Left = StatusRemaining(Player, SID.Scorn, 30); //Retrieve current Disesteem time left
        Disesteem.IsActive = Disesteem.Left > 0.1f; //Checks if Disesteem is active
        Disesteem.IsReady = Unlocked(AID.Disesteem) && Disesteem.Left > 0.1f; //Disesteem ability
        Delirium.Step = gauge.DeliriumStep; //Retrieve current Delirium combo step
        Delirium.Left = StatusRemaining(Player, BestBloodWeapon, 15); //Retrieve current Delirium time left
        Delirium.Stacks = StacksRemaining(Player, BestBloodWeapon, 15); //Retrieve current Delirium stacks
        Delirium.CD = TotalCD(BestDelirium); //Retrieve current Delirium cooldown
        Delirium.IsActive = Delirium.Left > 0.1f; //Checks if Delirium is active
        Delirium.IsReady = Unlocked(BestDelirium) && Delirium.CD < 0.6f; //Delirium ability
        LivingShadow.Timer = gauge.ShadowTimer / 1000f; //Retrieve current Living Shadow timer
        LivingShadow.CD = TotalCD(AID.LivingShadow); //Retrieve current Living Shadow cooldown
        LivingShadow.IsActive = LivingShadow.Timer > 0; //Checks if Living Shadow is active
        LivingShadow.IsReady = Unlocked(AID.LivingShadow) && LivingShadow.CD < 0.6f; //Living Shadow ability
        Shadowbringer.TotalCD = TotalCD(AID.Shadowbringer); //Retrieve current Shadowbringer cooldown
        Shadowbringer.ChargeCD = ChargeCD(AID.Shadowbringer); //Retrieve current Shadowbringer charge cooldown
        Shadowbringer.HasCharges = TotalCD(AID.Shadowbringer) <= 60; //Checks if Shadowbringer has charges
        Shadowbringer.IsReady = Unlocked(AID.Shadowbringer) && Shadowbringer.HasCharges; //Shadowbringer ability
        inOdd = LivingShadow.CD is < 90 and > 30;
        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore;
        (BestAOERectTargets, NumAOERectTargets) = GetBestTarget(PlayerTarget, 10, Is10yRectTarget);
        BestTargetAOERect = Unlocked(AID.Shadowbringer) && NumAOERectTargets > 1 ? BestAOERectTargets : primaryTarget;
        BestTargetMPRectHigh = Unlocked(AID.FloodOfShadow) && NumAOERectTargets > 2 ? BestAOERectTargets : primaryTarget;
        BestTargetMPRectLow = !Unlocked(AID.FloodOfShadow) && NumAOERectTargets > 3 ? BestAOERectTargets : primaryTarget;
        BestTargetMPRect = Unlocked(AID.FloodOfShadow) ? BestTargetMPRectHigh : BestTargetMPRectLow;

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

        #region Misc
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotations
        if (strategy.AutoFinish())
            QueueGCD(AutoFinish, TargetChoice(strategy.Option(SharedTrack.AOE)) ?? primaryTarget?.Actor, GCDPriority.Low);
        if (strategy.AutoBreak())
            QueueGCD(AutoBreak, TargetChoice(strategy.Option(SharedTrack.AOE)) ?? primaryTarget?.Actor, GCDPriority.Low);
        if (strategy.ForceST())
            QueueGCD(FullST, TargetChoice(strategy.Option(SharedTrack.AOE)) ?? primaryTarget?.Actor, GCDPriority.BelowAverage);
        if (strategy.ForceAOE())
            QueueGCD(FullAOE, Player, GCDPriority.BelowAverage);
        #endregion

        #region Cooldowns
        if (!strategy.HoldAll())
        {
            if (!strategy.HoldCDs())
            {
                if (!strategy.HoldBuffs())
                {
                    if (ShouldUseDelirium(deliStrat, primaryTarget))
                        QueueOGCD(BestDelirium, Player, deliStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave ? OGCDPriority.Forced : OGCDPriority.VeryHigh);
                    if (ShouldUseLivingShadow(lsStrat, primaryTarget))
                        QueueOGCD(AID.LivingShadow, Player, lsStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave ? OGCDPriority.Forced : OGCDPriority.ExtremelyHigh);
                }
                if (ShouldUseSaltedEarth(seStrat, primaryTarget))
                    QueueOGCD(AID.SaltedEarth, Player, seStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave ? OGCDPriority.Forced : OGCDPriority.AboveAverage);
                if (ShouldUseShadowbringer(sbStrat, primaryTarget))
                    QueueOGCD(AID.Shadowbringer, TargetChoice(sb) ?? BestTargetAOERect?.Actor, sbStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave ? OGCDPriority.Forced : OGCDPriority.Average);
                if (ShouldUseCarveOrDrain(cdStrat, primaryTarget))
                {
                    switch (cdStrat)
                    {
                        case CarveStrategy.Automatic:
                            QueueOGCD(CarveOrDrain, TargetChoice(cd) ?? primaryTarget?.Actor, cdStrat is CarveStrategy.ForceCarve or CarveStrategy.ForceDrain ? OGCDPriority.Forced : OGCDPriority.BelowAverage);
                            break;
                        case CarveStrategy.OnlyCarve:
                            QueueOGCD(BestCarve, TargetChoice(cd) ?? primaryTarget?.Actor, cdStrat is CarveStrategy.ForceCarve ? OGCDPriority.Forced : OGCDPriority.BelowAverage);
                            break;
                        case CarveStrategy.OnlyDrain:
                            QueueOGCD(AID.AbyssalDrain, TargetChoice(cd) ?? primaryTarget?.Actor, cdStrat is CarveStrategy.ForceDrain ? OGCDPriority.Forced : OGCDPriority.BelowAverage);
                            break;
                    }
                }
            }
            if (!strategy.HoldGauge() && ShouldUseBlood(bloodStrat, primaryTarget))
            {
                switch (bloodStrat)
                {
                    case BloodStrategy.Automatic:
                        QueueGCD(BestBloodSpender, TargetChoice(blood) ?? primaryTarget?.Actor, bloodStrat is BloodStrategy.ForceBloodspiller or BloodStrategy.ForceQuietus ? GCDPriority.Forced : RiskingBlood ? GCDPriority.High : GCDPriority.Average);
                        break;
                    case BloodStrategy.OnlyBloodspiller:
                        QueueGCD(AID.Bloodspiller, TargetChoice(blood) ?? primaryTarget?.Actor, bloodStrat is BloodStrategy.ForceBloodspiller ? GCDPriority.Forced : RiskingBlood ? GCDPriority.High : GCDPriority.Average);
                        break;
                    case BloodStrategy.OnlyQuietus:
                        QueueGCD(AID.Quietus, Unlocked(AID.Quietus) ? Player : TargetChoice(blood) ?? primaryTarget?.Actor, bloodStrat is BloodStrategy.ForceQuietus ? GCDPriority.Forced : RiskingBlood ? GCDPriority.High : GCDPriority.Average);
                        break;
                }
            }
        }
        if (ShouldUseDisesteem(deStrat, primaryTarget))
            QueueGCD(AID.Disesteem, TargetChoice(de) ?? BestTargetAOERect?.Actor, deStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.AboveAverage);
        if (ShouldUseMP(mpStrat))
        {
            switch (mpStrat)
            {
                case MPStrategy.Optimal:
                case MPStrategy.Auto9k:
                case MPStrategy.Auto6k:
                case MPStrategy.Auto3k:
                case MPStrategy.AutoRefresh:
                    if (NumAOERectTargets >= 3)
                        QueueOGCD(BestFlood, TargetChoice(mp) ?? BestTargetMPRectHigh?.Actor, RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low);
                    if (NumAOERectTargets <= 2)
                        QueueOGCD(BestEdge, TargetChoice(mp) ?? primaryTarget?.Actor, RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low);
                    break;
                case MPStrategy.Edge9k:
                case MPStrategy.Edge6k:
                case MPStrategy.Edge3k:
                case MPStrategy.EdgeRefresh:
                    QueueOGCD(BestEdge, TargetChoice(mp) ?? primaryTarget?.Actor, RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low);
                    break;
                case MPStrategy.Flood9k:
                case MPStrategy.Flood6k:
                case MPStrategy.Flood3k:
                case MPStrategy.FloodRefresh:
                    QueueOGCD(BestFlood, TargetChoice(mp) ?? (NumAOERectTargets > 1 ? BestAOERectTargets?.Actor : primaryTarget?.Actor), RiskingMP ? OGCDPriority.Forced : OGCDPriority.Low);
                    break;
            }
        }
        if (ShouldUseDeliriumCombo(dcomboStrat, primaryTarget))
        {
            switch (dcomboStrat)
            {
                case DeliriumComboStrategy.Automatic:
                    QueueGCD(DeliriumCombo, TargetChoice(dcombo) ?? primaryTarget?.Actor, GCDPriority.SlightlyHigh);
                    break;
                case DeliriumComboStrategy.ScarletDelirum:
                    QueueGCD(AID.ScarletDelirium, TargetChoice(dcombo) ?? primaryTarget?.Actor, GCDPriority.Forced);
                    break;
                case DeliriumComboStrategy.Comeuppance:
                    QueueGCD(AID.Comeuppance, TargetChoice(dcombo) ?? primaryTarget?.Actor, GCDPriority.Forced);
                    break;
                case DeliriumComboStrategy.Torcleaver:
                    QueueGCD(AID.Torcleaver, TargetChoice(dcombo) ?? primaryTarget?.Actor, GCDPriority.Forced);
                    break;
                case DeliriumComboStrategy.Impalement:
                    QueueGCD(AID.Impalement, Player, GCDPriority.Forced);
                    break;
            }
        }
        if (ShouldUseSaltAndDarkness(strategy.Option(Track.SaltAndDarkness).As<OGCDStrategy>(), primaryTarget))
            QueueOGCD(AID.SaltAndDarkness, Player, OGCDPriority.AboveAverage);
        if (ShouldUseUnmend(unmendStrat, primaryTarget))
            QueueGCD(AID.Unmend, TargetChoice(unmend) ?? primaryTarget?.Actor, GCDPriority.Low);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Forced, 0, GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        var goalST = primaryTarget?.Actor != null ? Hints.GoalSingleTarget(primaryTarget!.Actor, 3) : null;
        var goalAOE = Hints.GoalAOECircle(3);
        var goal = strategy.Option(SharedTrack.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.ForceST => goalST,
            AOEStrategy.ForceAOE => goalAOE,
            _ => goalST != null ? Hints.GoalCombined(goalST, goalAOE, 3) : goalAOE
        };
        if (goal != null)
            Hints.GoalZones.Add(goal);
        #endregion
    }
}
