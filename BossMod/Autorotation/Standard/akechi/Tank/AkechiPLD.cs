using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.PLD;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiPLD(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { AOE, Cooldowns, Potion, Atonement, BladeCombo, Holy, Dash, Ranged, FightOrFlight, Requiescat, SpiritsWithin, CircleOfScorn, GoringBlade, BladeOfHonor }
    public enum AOEStrategy { AutoFinish, AutoBreak, ForceST, ForceAOE }
    public enum CooldownStrategy { Allow, Forbid }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    public enum AtonementStrategy { Automatic, ForceAtonement, ForceSupplication, ForceSepulchre, Delay }
    public enum BladeComboStrategy { Automatic, ForceConfiteor, ForceFaith, ForceTruth, ForceValor, Delay }
    public enum HolyStrategy { Automatic, Spirit, Circle, Delay }
    public enum DashStrategy { Automatic, Force, Force1, GapClose, GapClose1, Delay }
    public enum RangedStrategy { Automatic, OpenerRangedCast, OpenerCast, ForceCast, RangedCast, OpenerRanged, Opener, Force, Ranged, Forbid }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi PLD", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.GLA, (int)Class.PLD), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoFinish, "Auto (Finish Combo)", "Auto-selects best rotation dependant on targets; finishes combo if possible", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoBreak, "Auto (Break Combo)", "Auto-selects best rotation dependant on targets; breaks combo if necessary", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceST, "Use AOE", "Force single-target rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "Force AOE", "Force AOE rotation")
            .AddAssociatedActions(AID.FastBlade, AID.RiotBlade, AID.RageOfHalone, AID.RoyalAuthority, AID.Prominence, AID.TotalEclipse);
        res.Define(Track.Cooldowns).As<CooldownStrategy>("Hold", uiPriority: 190)
            .AddOption(CooldownStrategy.Allow, "Allow", "Allows the use of all cooldowns & buffs; will use them optimally")
            .AddOption(CooldownStrategy.Forbid, "Hold", "Forbids the use of all cooldowns & buffs; will not use any actions with a cooldown timer");
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 90)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use potions automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Align With Raid Buffs", "Align potion usage with raid buffs", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potions immediately when available", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.Atonement).As<AtonementStrategy>("Atonement", "Atones", uiPriority: 155)
            .AddOption(AtonementStrategy.Automatic, "Automatic", "Normal use of Atonement & its combo chain")
            .AddOption(AtonementStrategy.ForceAtonement, "Force Atonement", "Force use of Atonement", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSupplication, "Force Supplication", "Force use of Supplication", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSepulchre, "Force Sepulchre", "Force use of Sepulchre", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.Delay, "Delay", "Delay use of Atonement & its combo chain", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Atonement, AID.Supplication, AID.Sepulchre);
        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blade Combo", "Blades", uiPriority: 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatic", "Normal use of Confiteor & Blades combo chain")
            .AddOption(BladeComboStrategy.ForceConfiteor, "Force", "Force use of Confiteor", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(BladeComboStrategy.ForceFaith, "Force Faith", "Force use of Blade of Faith", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceTruth, "Force Truth", "Force use of Blade of Truth", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceValor, "Force Valor", "Force use of Blade of Valor", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.Delay, "Delay", "Delay use of Confiteor & Blades combo chain", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Confiteor, AID.BladeOfFaith, AID.BladeOfTruth, AID.BladeOfValor);
        res.Define(Track.Holy).As<HolyStrategy>("Holy Spirit / Circle", "Holy S/C", uiPriority: 150)
            .AddOption(HolyStrategy.Automatic, "Automatic", "Automatically choose which Holy action to use based on conditions")
            .AddOption(HolyStrategy.Spirit, "Spirit", "Force use of Holy Spirit", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.Circle, "Circle", "Force use of Holy Circle", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.Delay, "Delay", "Delay use of Holy actions", 0, 0, ActionTargets.None, 64)
            .AddAssociatedActions(AID.HolySpirit, AID.HolyCircle);
        res.Define(Track.Dash).As<DashStrategy>("Intervene", "Dash", uiPriority: 95)
            .AddOption(DashStrategy.Automatic, "Automatic", "Normal use of Intervene")
            .AddOption(DashStrategy.Force, "Force", "Force use of Intervene", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.Force1, "Force (Hold 1)", "Force use of Intervene; Hold one charge for manual usage", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.GapClose, "Gap Close", "Use as gap closer if outside melee range", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.GapClose1, "Gap Close (Hold 1)", "Use as gap closer if outside melee range; Hold one charge for manual usage", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.Delay, "Delay", "Delay use of Intervene", 0, 0, ActionTargets.None, 66)
            .AddAssociatedActions(AID.Intervene);
        res.Define(Track.Ranged).As<RangedStrategy>("Ranged", "Ranged", uiPriority: 100)
            .AddOption(RangedStrategy.Automatic, "Automatic", "Uses Holy Spirit when standing still; Uses Shield Lob if moving")
            .AddOption(RangedStrategy.OpenerRangedCast, "Opener (Cast)", "Use Holy Spirit at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerCast, "Opener", "Use Holy Spirit at the start of combat regardless of range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.ForceCast, "Force Cast", "Force use of Holy Spirit", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.RangedCast, "Ranged Cast", "Use Holy Spirit for ranged attacks", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerRanged, "Opener (Lob)", "Use Shield Lob at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Opener, "Opener", "Use Shield Lob at the start of combat regardless of range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Force, "Force", "Force use of Shield Lob", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Ranged, "Ranged", "Use Shield Lob for ranged attacks", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Forbid, "Forbid", "Prohibit the use of ranged attacks", 0, 0, ActionTargets.Hostile, 15)
            .AddAssociatedActions(AID.ShieldLob, AID.HolySpirit);
        res.DefineOGCD(Track.FightOrFlight, AID.FightOrFlight, "FightOrFlight", "F.Flight", uiPriority: 170, 60, 20, ActionTargets.Self, 2).AddAssociatedActions(AID.FightOrFlight);
        res.DefineOGCD(Track.Requiescat, AID.Requiescat, "Requiescat", "Req.", uiPriority: 165, 60, 30, ActionTargets.Hostile, 68).AddAssociatedActions(AID.Requiescat, AID.Imperator);
        res.DefineOGCD(Track.SpiritsWithin, AID.SpiritsWithin, "SpiritsWithin", "S.Within", uiPriority: 145, 30, 0, ActionTargets.Hostile, 30).AddAssociatedActions(AID.SpiritsWithin, AID.Expiacion);
        res.DefineOGCD(Track.CircleOfScorn, AID.CircleOfScorn, "CircleOfScorn", "C.Scorn", uiPriority: 140, 30, 15, ActionTargets.Self, 50).AddAssociatedActions(AID.CircleOfScorn);
        res.DefineGCD(Track.GoringBlade, AID.GoringBlade, "GoringBlade", "G.Blade", uiPriority: 135, 0, 0, ActionTargets.Hostile, 54).AddAssociatedActions(AID.GoringBlade);
        res.DefineOGCD(Track.BladeOfHonor, AID.BladeOfHonor, "BladeOfHonor", "B.Honor", uiPriority: 130, 0, 0, ActionTargets.Hostile, 100).AddAssociatedActions(AID.BladeOfHonor);

        return res;

    }
    #endregion

    #region Module Variables
    public int Oath; //Current value of the oath gauge
    public int BladeComboStep; //Current step in the Confiteor combo sequence
    public (float Left, bool IsActive) DivineMight; //Conditions for the Divine Might buff
    public (float CD, float Left, bool IsReady, bool IsActive) FightOrFlight; //Conditions for Fight or Flight ability
    public (float CD, bool IsReady) SpiritsWithin; //Conditions for Spirits Within ability
    public (float CD, bool IsReady) CircleOfScorn; //Conditions for Circle of Scorn ability
    public (float CD, float Left, bool IsReady, bool IsActive) GoringBlade; //Conditions for Goring Blade ability
    public (float TotalCD, bool HasCharges, bool IsReady) Intervene; //Conditions for Intervene ability
    public (float CD, float Left, bool IsReady, bool IsActive) Requiescat; //Conditions for Requiescat ability
    public (float Left, bool IsReady, bool IsActive) Atonement; //Conditions for Atonement ability
    public (float Left, bool IsReady, bool IsActive) Supplication; //Conditions for Supplication ability
    public (float Left, bool IsReady, bool IsActive) Sepulchre; //Conditions for Sepulchre ability
    public (float Left, bool HasMP, bool IsReady, bool IsActive) Confiteor; //Conditions for Confiteor ability
    public (float Left, bool IsReady, bool IsActive) BladeOfHonor; //Conditions for Blade of Honor ability
    public (bool HasMP, bool IsReady) HolySpirit; //Conditions for Holy Spirit ability
    public (bool HasMP, bool IsReady) HolyCircle; //Conditions for Holy Circle ability
    public bool ShouldUseAOE; //Check if AOE rotation should be used
    public bool ShouldHoldDMandAC; //Check if Divine Might buff and Atonement combo should be held into Fight or Flight
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestSplashTarget;
    #endregion

    #region Upgrade Paths
    public AID BestSpirits => Unlocked(AID.Expiacion) ? AID.Expiacion : AID.SpiritsWithin;
    public AID BestRequiescat => Unlocked(AID.Imperator) ? AID.Imperator : AID.Requiescat;
    public AID BestHoly => ShouldUseAOE ? BestHolyCircle : AID.HolySpirit;
    public AID BestHolyCircle => HolyCircle.IsReady ? AID.HolyCircle : AID.HolySpirit;
    public AID BestAtonement => Sepulchre.IsReady ? AID.Sepulchre : (Supplication.IsReady ? AID.Supplication : AID.Atonement);
    public AID BestBlade => BladeComboStep == 3 ? AID.BladeOfValor : BladeComboStep == 2 ? AID.BladeOfTruth : BladeComboStep == 1 && Unlocked(AID.BladeOfFaith) ? AID.BladeOfFaith : Unlocked(AID.Confiteor) ? AID.Confiteor : BestHoly;
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.FastBlade or AID.RiotBlade => FullST,
        AID.TotalEclipse => FullAOE,
        AID.RageOfHalone or AID.RoyalAuthority or AID.Prominence or _ => ShouldUseAOE ? FullAOE : FullST,
    };
    private AID AutoBreak => ShouldUseAOE ? FullAOE : FullST;
    private AID FullST => ComboLastMove is AID.RiotBlade ? (Unlocked(AID.RoyalAuthority) ? AID.RoyalAuthority : Unlocked(AID.RageOfHalone) ? AID.RageOfHalone : AID.FastBlade) : Unlocked(AID.RiotBlade) && ComboLastMove is AID.FastBlade ? AID.RiotBlade : AID.FastBlade;
    private AID FullAOE => Unlocked(AID.TotalEclipse) && ComboLastMove is AID.TotalEclipse ? AID.TotalEclipse : AID.Prominence;
    #endregion

    #region Cooldown Helpers
    private bool ShouldUseRangedLob(Actor? target, RangedStrategy strategy) => strategy switch
    {
        RangedStrategy.OpenerRanged => IsFirstGCD() && !In3y(target),
        RangedStrategy.Opener => IsFirstGCD(),
        RangedStrategy.Force => true,
        RangedStrategy.Ranged => !In3y(target),
        RangedStrategy.Forbid => false,
        _ => false
    };
    private bool ShouldUseRangedCast(Actor? target, RangedStrategy strategy) => strategy switch
    {
        RangedStrategy.OpenerRangedCast => IsFirstGCD() && !In3y(target),
        RangedStrategy.OpenerCast => IsFirstGCD(),
        RangedStrategy.ForceCast => true,
        RangedStrategy.RangedCast => !In3y(target),
        _ => false
    };
    private bool ShouldUseFightOrFlight(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && FightOrFlight.IsReady && ((CombatTimer <= 30 && ComboLastMove is AID.RoyalAuthority or AID.RageOfHalone || ShouldUseAOE) || CombatTimer > 30),
        OGCDStrategy.Force => FightOrFlight.IsReady,
        OGCDStrategy.AnyWeave => FightOrFlight.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => FightOrFlight.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => FightOrFlight.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseRequiescat(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && Requiescat.IsReady && FightOrFlight.IsActive,
        OGCDStrategy.Force => Requiescat.IsReady,
        OGCDStrategy.AnyWeave => Requiescat.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => Requiescat.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => Requiescat.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseSpiritsWithin(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && In3y(target) && FightOrFlight.CD is < 57.55f and > 17 && SpiritsWithin.IsReady,
        OGCDStrategy.Force => SpiritsWithin.IsReady,
        OGCDStrategy.AnyWeave => SpiritsWithin.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => SpiritsWithin.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => SpiritsWithin.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseCircleOfScorn(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CircleOfScorn.IsReady && In5y(target) && FightOrFlight.CD is < 57.55f and > 17,
        OGCDStrategy.Force => CircleOfScorn.IsReady,
        OGCDStrategy.AnyWeave => CircleOfScorn.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => CircleOfScorn.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => CircleOfScorn.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBladeOfHonor(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && BladeOfHonor.IsReady,
        OGCDStrategy.Force => BladeOfHonor.IsReady,
        OGCDStrategy.AnyWeave => BladeOfHonor.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => BladeOfHonor.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => BladeOfHonor.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseGoringBlade(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => Player.InCombat && In3y(target) && GoringBlade.IsReady && FightOrFlight.IsActive,
        GCDStrategy.Force => GoringBlade.IsReady,
        GCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBladeCombo(BladeComboStrategy strategy, Actor? target) => strategy switch
    {
        BladeComboStrategy.Automatic => Player.InCombat && target != null && In25y(target) && Requiescat.IsActive && BladeComboStep is 0 or 1 or 2 or 3,
        BladeComboStrategy.ForceConfiteor => Confiteor.IsReady && BladeComboStep is 0,
        BladeComboStrategy.ForceFaith => BladeComboStep is 1,
        BladeComboStrategy.ForceTruth => BladeComboStep is 2,
        BladeComboStrategy.ForceValor => BladeComboStep is 3,
        BladeComboStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseAtonement(AtonementStrategy strategy, Actor? target) => strategy switch
    {
        AtonementStrategy.Automatic => Player.InCombat && target != null && In3y(target) && !ShouldHoldDMandAC && (Atonement.IsReady || Supplication.IsReady || Sepulchre.IsReady),
        AtonementStrategy.ForceAtonement => Atonement.IsReady,
        AtonementStrategy.ForceSupplication => Supplication.IsReady,
        AtonementStrategy.ForceSepulchre => Sepulchre.IsReady,
        AtonementStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseHoly(HolyStrategy strategy, Actor? target) => strategy switch
    {
        HolyStrategy.Automatic => ShouldSpendHoly(HolyStrategy.Automatic, target),
        HolyStrategy.Spirit => HolySpirit.IsReady,
        HolyStrategy.Circle => HolyCircle.IsReady,
        HolyStrategy.Delay => false,
        _ => false
    };
    private bool ShouldSpendHoly(HolyStrategy strategy, Actor? target) => strategy switch
    {
        HolyStrategy.Automatic => Player.InCombat && target != null && (!ShouldUseAOE && In25y(target) || ShouldUseAOE && In5y(target)) && HolySpirit.IsReady && !ShouldHoldDMandAC && DivineMight.IsActive,
        _ => false
    };
    private bool ShouldUseDash(DashStrategy strategy, Actor? target) => strategy switch
    {
        DashStrategy.Automatic => Player.InCombat && target != null && In3y(target) && Intervene.IsReady && FightOrFlight.IsActive,
        DashStrategy.Force => true,
        DashStrategy.Force1 => Intervene.TotalCD < 1f,
        DashStrategy.GapClose => !In3y(target),
        DashStrategy.GapClose1 => Intervene.TotalCD < 1f && !In3y(target),
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => FightOrFlight.CD < 5,
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<PaladinGauge>(); //Retrieve Paladin gauge
        Oath = gauge.OathGauge; //Retrieve current Oath gauge
        BladeComboStep = gauge.ConfiteorComboStep; //Retrieve current step in the Confiteor/Blades combo
        DivineMight.Left = SelfStatusLeft(SID.DivineMight, 30); //Remaining duration of the Divine Might buff
        DivineMight.IsActive = DivineMight.Left > 0; //Check if the Divine Might buff is active
        FightOrFlight.CD = TotalCD(AID.FightOrFlight); //Remaining cooldown for the Fight or Flight ability
        FightOrFlight.Left = SelfStatusLeft(SID.FightOrFlight, 20); //Remaining duration of the Fight or Flight buff
        FightOrFlight.IsActive = FightOrFlight.CD is >= 39.5f and <= 60; //Check if the Fight or Flight buff is active 
        FightOrFlight.IsReady = FightOrFlight.CD < 0.6f; //Fight or Flight ability
        SpiritsWithin.CD = TotalCD(BestSpirits); //Remaining cooldown for the Spirits Within ability
        SpiritsWithin.IsReady = Unlocked(AID.SpiritsWithin) && SpiritsWithin.CD < 0.6f; //Spirits Within ability
        CircleOfScorn.CD = TotalCD(AID.CircleOfScorn); //Remaining cooldown for the Circle of Scorn ability
        CircleOfScorn.IsReady = Unlocked(AID.CircleOfScorn) && CircleOfScorn.CD < 0.6f; //Circle of Scorn ability
        GoringBlade.CD = TotalCD(AID.GoringBlade); //Remaining cooldown for the Goring Blade ability
        GoringBlade.Left = SelfStatusLeft(SID.GoringBladeReady, 30); //Remaining duration of the Goring Blade buff
        GoringBlade.IsActive = GoringBlade.Left > 0; //Check if the Goring Blade buff is active
        GoringBlade.IsReady = Unlocked(AID.GoringBlade) && GoringBlade.IsActive; //Goring Blade ability
        Intervene.TotalCD = TotalCD(AID.Intervene); //Total cooldown for the Intervene ability (60s)
        Intervene.HasCharges = Intervene.TotalCD <= 30f; //Check if the player has charges of Intervene
        Intervene.IsReady = Unlocked(AID.Intervene) && Intervene.HasCharges; //Intervene ability
        Requiescat.CD = TotalCD(BestRequiescat); //Remaining cooldown for the Requiescat ability
        Requiescat.Left = SelfStatusLeft(SID.Requiescat, 30); //Get the current number of Requiescat stacks 
        Requiescat.IsActive = Requiescat.Left > 0; //Check if the Requiescat buff is active
        Requiescat.IsReady = Unlocked(AID.Requiescat) && Requiescat.CD < 0.6f; //Requiescat ability
        Atonement.Left = SelfStatusLeft(SID.AtonementReady, 30); //Remaining duration of the Atonement buff
        Atonement.IsActive = Atonement.Left > 0; //Check if the Atonement buff is active
        Atonement.IsReady = Unlocked(AID.Atonement) && Atonement.IsActive; //Atonement ability
        Supplication.Left = SelfStatusLeft(SID.SupplicationReady, 30); //Remaining duration of the Supplication buff
        Supplication.IsActive = Supplication.Left > 0; //Check if the Supplication buff is active
        Supplication.IsReady = Unlocked(AID.Supplication) && Supplication.IsActive; //Supplication ability
        Sepulchre.Left = SelfStatusLeft(SID.SepulchreReady, 30); //Remaining duration of the Sepulchre buff
        Sepulchre.IsActive = Sepulchre.Left > 0; //Check if the Sepulchre buff is active
        Sepulchre.IsReady = Unlocked(AID.Sepulchre) && Sepulchre.IsActive; //Sepulchre ability
        Confiteor.Left = SelfStatusLeft(SID.ConfiteorReady, 30); //Remaining duration of the Confiteor buff
        Confiteor.IsActive = Confiteor.Left > 0; //Check if the Confiteor buff is active
        Confiteor.IsReady = Unlocked(AID.Confiteor) && Confiteor.IsActive && MP >= 1000; //Confiteor ability
        BladeOfHonor.Left = SelfStatusLeft(SID.BladeOfHonorReady, 30); //Remaining duration of the Blade of Honor buff
        BladeOfHonor.IsActive = BladeOfHonor.Left > 0; //Check if the Blade of Honor buff is active
        BladeOfHonor.IsReady = Unlocked(AID.BladeOfHonor) && BladeOfHonor.IsActive; //Checks if Blade of Honor is ready
        HolySpirit.HasMP = MP >= 1000; //Check if the player has enough MP for Holy Spirit
        HolySpirit.IsReady = Unlocked(AID.HolySpirit) && HolySpirit.HasMP; //Holy Spirit ability
        HolyCircle.HasMP = MP >= 1000; //Check if the player has enough MP for Holy Circle
        HolyCircle.IsReady = Unlocked(AID.HolyCircle) && HolyCircle.HasMP; //Holy Circle ability
        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore; //Check if AOE rotation should be used
        ShouldHoldDMandAC = ComboLastMove is AID.RoyalAuthority ? FightOrFlight.CD < 5 : ComboLastMove is AID.FastBlade ? FightOrFlight.CD < 2.5 : ComboLastMove is AID.RiotBlade && FightOrFlight.CD < GCD;
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.Confiteor) && NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //Retrieves AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //Retrieves AOE strategy
        var cd = strategy.Option(Track.Cooldowns); //Retrieves Cooldowns track
        var cdStrat = cd.As<CooldownStrategy>(); //Retrieves Cooldowns strategy
        var pot = strategy.Option(Track.Potion); //Retrieves Potion track
        var potStrat = pot.As<PotionStrategy>(); //Retrieves Potion strategy
        var fof = strategy.Option(Track.FightOrFlight); //Retrieves Fight or Flight track
        var fofStrat = fof.As<OGCDStrategy>(); //Retrieves Fight or Flight strategy
        var req = strategy.Option(Track.Requiescat); //Retrieves Requiescat track
        var reqStrat = req.As<OGCDStrategy>(); //Retrieves Requiescat strategy
        var atone = strategy.Option(Track.Atonement); //Retrieves Atonement track
        var atoneStrat = atone.As<AtonementStrategy>(); //Retrieves Atonement strategy
        var blade = strategy.Option(Track.BladeCombo); //Retrieves Blade Combo track
        var bladeStrat = blade.As<BladeComboStrategy>(); //Retrieves Blade Combo strategy
        var cos = strategy.Option(Track.CircleOfScorn); //Retrieves Circle of Scorn track
        var cosStrat = cos.As<OGCDStrategy>(); //Retrieves Circle of Scorn strategy
        var sw = strategy.Option(Track.SpiritsWithin); //Retrieves Spirits Within track
        var swStrat = sw.As<OGCDStrategy>(); //Retrieves Spirits Within strategy
        var dash = strategy.Option(Track.Dash); //Retrieves Dash track
        var dashStrat = dash.As<DashStrategy>(); //Retrieves Dash strategy
        var gb = strategy.Option(Track.GoringBlade); //Retrieves Goring Blade track
        var gbStrat = gb.As<GCDStrategy>(); //Retrieves Goring Blade strategy
        var boh = strategy.Option(Track.BladeOfHonor); //Retrieves Blade of Honor track
        var bohStrat = boh.As<OGCDStrategy>(); //Retrieves Blade of Honor strategy
        var holy = strategy.Option(Track.Holy); //Retrieves Holy track
        var holyStrat = holy.As<HolyStrategy>(); //Retrieves Holy strategy
        var ranged = strategy.Option(Track.Ranged); //Retrieves Ranged track
        var rangedStrat = ranged.As<RangedStrategy>(); //Retrieves Ranged strategy
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotation
        if (AOEStrategy == AOEStrategy.AutoFinish)
            QueueGCD(AutoFinish, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Low);
        if (AOEStrategy == AOEStrategy.AutoBreak)
            QueueGCD(AutoBreak, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Low);
        if (AOEStrategy == AOEStrategy.ForceST)
            QueueGCD(FullST, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Low);
        if (AOEStrategy == AOEStrategy.ForceAOE)
            QueueGCD(FullAOE, Player, GCDPriority.Low);
        #endregion

        #region Cooldowns
        if (cdStrat != CooldownStrategy.Forbid)
        {
            if (ShouldUseFightOrFlight(fofStrat, primaryTarget?.Actor))
                QueueOGCD(AID.FightOrFlight, Player, OGCDPrio(fofStrat, OGCDPriority.ExtremelyHigh));
            if (ShouldUseRequiescat(reqStrat, primaryTarget?.Actor))
                QueueOGCD(BestRequiescat, TargetChoice(req) ?? (Unlocked(AID.Imperator) ? BestSplashTarget?.Actor : primaryTarget?.Actor), OGCDPrio(reqStrat, OGCDPriority.VeryHigh));
            if (ShouldUseCircleOfScorn(cosStrat, primaryTarget?.Actor))
                QueueOGCD(AID.CircleOfScorn, Player, OGCDPrio(cosStrat, OGCDPriority.AboveAverage));
            if (ShouldUseSpiritsWithin(swStrat, primaryTarget?.Actor))
                QueueOGCD(BestSpirits, TargetChoice(sw) ?? primaryTarget?.Actor, OGCDPrio(swStrat, OGCDPriority.Average));
            if (ShouldUseDash(dashStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Intervene, TargetChoice(dash) ?? primaryTarget?.Actor, dashStrat is DashStrategy.Force or DashStrategy.Force1 or DashStrategy.GapClose or DashStrategy.GapClose1 ? OGCDPriority.Forced : OGCDPriority.BelowAverage);
            if (ShouldUseBladeOfHonor(bohStrat, primaryTarget?.Actor))
                QueueOGCD(AID.BladeOfHonor, TargetChoice(boh) ?? BestSplashTarget?.Actor, OGCDPrio(bohStrat, OGCDPriority.Low));
            if (ShouldUseGoringBlade(gbStrat, primaryTarget?.Actor))
                QueueGCD(AID.GoringBlade, TargetChoice(gb) ?? primaryTarget?.Actor, GCDPrio(gbStrat, GCDPriority.SlightlyHigh));
        }
        if (ShouldUseBladeCombo(bladeStrat, primaryTarget?.Actor))
        {
            switch (bladeStrat)
            {
                case BladeComboStrategy.Automatic:
                    QueueGCD(BestBlade, TargetChoice(blade) ?? BestSplashTarget?.Actor, GCDPriority.VeryHigh);
                    break;
                case BladeComboStrategy.ForceConfiteor:
                    QueueGCD(AID.Confiteor, TargetChoice(blade) ?? BestSplashTarget?.Actor, GCDPriority.Forced);
                    break;
                case BladeComboStrategy.ForceFaith:
                    QueueGCD(AID.BladeOfFaith, TargetChoice(blade) ?? BestSplashTarget?.Actor, GCDPriority.Forced);
                    break;
                case BladeComboStrategy.ForceTruth:
                    QueueGCD(AID.BladeOfTruth, TargetChoice(blade) ?? BestSplashTarget?.Actor, GCDPriority.Forced);
                    break;
                case BladeComboStrategy.ForceValor:
                    QueueGCD(AID.BladeOfValor, TargetChoice(blade) ?? BestSplashTarget?.Actor, GCDPriority.Forced);
                    break;
            }
        }
        if (ShouldUseAtonement(atoneStrat, primaryTarget?.Actor))
        {
            switch (atoneStrat)
            {
                case AtonementStrategy.Automatic:
                    QueueGCD(BestAtonement, TargetChoice(atone) ?? primaryTarget?.Actor, GCDPriority.SlightlyHigh);
                    break;
                case AtonementStrategy.ForceAtonement:
                    QueueGCD(AID.Atonement, TargetChoice(atone) ?? primaryTarget?.Actor, GCDPriority.Forced);
                    break;
                case AtonementStrategy.ForceSupplication:
                    QueueGCD(AID.Supplication, TargetChoice(atone) ?? primaryTarget?.Actor, GCDPriority.Forced);
                    break;
                case AtonementStrategy.ForceSepulchre:
                    QueueGCD(AID.Sepulchre, TargetChoice(atone) ?? primaryTarget?.Actor, GCDPriority.Forced);
                    break;
            }
        }
        if (ShouldUseHoly(holyStrat, primaryTarget?.Actor))
        {
            var lastsecBurst = fofStrat != OGCDStrategy.Delay && FightOrFlight.Left is <= 2.5f and >= 0.01f && !Supplication.IsActive && !Sepulchre.IsActive;
            switch (holyStrat)
            {
                case HolyStrategy.Automatic:
                    QueueGCD(BestHoly, TargetChoice(holy) ?? primaryTarget?.Actor, lastsecBurst ? GCDPriority.VeryHigh : GCDPriority.AboveAverage);
                    break;
                case HolyStrategy.Spirit:
                    QueueGCD(AID.HolySpirit, TargetChoice(holy) ?? primaryTarget?.Actor, GCDPriority.Forced);
                    break;
                case HolyStrategy.Circle:
                    QueueGCD(BestHolyCircle, Player, GCDPriority.Forced);
                    break;
            }
        }
        if (rangedStrat == RangedStrategy.Automatic && !In3y(TargetChoice(ranged) ?? primaryTarget?.Actor))
            QueueGCD(IsMoving ? AID.ShieldLob : AID.HolySpirit, TargetChoice(ranged) ?? primaryTarget?.Actor, GCDPriority.Low);
        if (ShouldUseRangedLob(primaryTarget?.Actor, rangedStrat))
            QueueGCD(AID.ShieldLob, TargetChoice(ranged) ?? primaryTarget?.Actor, rangedStrat == RangedStrategy.Force ? GCDPriority.Forced : GCDPriority.Low);
        if (ShouldUseRangedCast(primaryTarget?.Actor, rangedStrat))
            QueueGCD(AID.HolySpirit, TargetChoice(ranged) ?? primaryTarget?.Actor, rangedStrat == RangedStrategy.ForceCast ? GCDPriority.Forced : GCDPriority.Low);
        if (ShouldUsePotion(potStrat))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, 0, GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        var goalST = primaryTarget?.Actor != null ? Hints.GoalSingleTarget(primaryTarget!.Actor, 3) : null;
        var goalAOE = Hints.GoalAOECircle(3);
        var goal = strategy.Option(Track.AOE).As<AOEStrategy>() switch
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
