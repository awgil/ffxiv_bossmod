using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using System;
using System.Runtime.InteropServices;
using static BossMod.ActorCastEvent;
using static BossMod.Autorotation.akechi.AkechiGNB;
using AID = BossMod.PLD.AID;
using SID = BossMod.PLD.SID;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiPLD(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums
    public enum Track
    {
        AoE,                //Tracks both AoE and single-target actions
        Cooldowns,          //Tracks Cooldowns damage actions
        Potion,             //Tracks potion usage
        Atonement,          //Tracks Atonement actions
        BladeCombo,         //Tracks Blade Combo actions
        Holy,               //Tracks Holy actions
        Dash,               //Tracks the use of Intervene
        Ranged,             //Tracks ranged attacks
        FightOrFlight,      //Tracks Fight or Flight actions
        Requiescat,         //Tracks Requiescat actions
        SpiritsWithin,      //Tracks Spirits Within actions
        CircleOfScorn,      //Tracks Circle of Scorn actions
        GoringBlade,        //Tracks Goring Blade actions
        BladeOfHonor,       //Tracks Blade of Honor actions
    }
    public enum AOEStrategy
    {
        AutoFinishCombo,    //Automatically decide based on targets; finish combo first
        AutoBreakCombo,     //Automatically decide based on targets; break combo if needed
        ForceST,            //Force single-target rotation
        ForceAoE            //Force AoE rotation
    }
    public enum CooldownStrategy
    {
        Allow,              //Allow cooldowns when conditions are met
        Hold,               //Hold MP and cooldowns for strategic usage
    }
    public enum PotionStrategy
    {
        Manual,             //Use potions manually based on player discretion
        AlignWithRaidBuffs, //Align potion usage with the timing of raid buffs
        Immediate           //Use potions immediately when available
    }
    public enum AtonementStrategy
    {
        Automatic,          //Automatically use Atonement when needed
        ForceAtonement,     //Force the use of Atonement regardless of other actions
        ForceSupplication,  //Force use of Supplication
        ForceSepulchre,     //Force use of Sepulchre actions
        Delay               //Delay the use of Atonement for optimal timing
    }
    public enum BladeComboStrategy
    {
        Automatic,          //Automatically execute Blade Combo when conditions are favorable
        ForceConfiteor,     //Force the use of Confiteor action
        ForceFaith,         //Force the use of Blade of Faith
        ForceTruth,         //Force the use of Blade of Truth
        ForceValor,         //Force the use of Blade of Valor
        Delay               //Delay the use of Confiteor and Blade Combo for timing
    }
    public enum HolyStrategy
    {
        Automatic,          //Automatically decide on Holy actions based on conditions
        Spirit,             //Force the use of Holy Spirit
        Circle,             //Force the use of Holy Circle
        Delay               //Delay Holy actions for strategic timing
    }
    public enum DashStrategy
    {
        Automatic,          //Automatically use Intervene as needed
        Force,              //Force the use of Intervene regardless of other factors
        Force1,             //Force the use of Intervene; Holds one charge for manual usage
        GapClose,           //Use Intervene to close gaps between targets
        GapClose1,          //Use Intervene to close gaps between targets; Hold one charge of Intervene for manual usage
        Delay               //Delay the use of Intervene for strategic reasons
    }
    public enum RangedStrategy
    {
        OpenerRangedCast,   //Use Holy Spirit at the start of combat only if outside melee range
        OpenerCast,         //Use Holy Spirit at the start of combat regardless of range
        ForceCast,          //Force Holy Spirit when possible
        RangedCast,         //Use Holy Spirit for ranged attacks
        OpenerRanged,       //Use Shield Lob at the start of combat only if outside melee range
        Opener,             //Use Shield Lob at the start of combat regardless of range
        Force,              //Force Shield Lob when possible
        Ranged,             //Use Shield Lob for ranged attacks
        Forbid              //Prohibit the use of all ranged attacks entirely (unless under Divine Might)
    }
    public enum GCDStrategy
    {
        Automatic,          //Automatically decide on GCD actions based on conditions
        Force,              //Force GCD actions regardless of any conditions
        Delay               //Delay GCD actions for strategic reasons
    }
    public enum OGCDStrategy
    {
        Automatic,          //Automatically decide when to use off-global offensive abilities
        Force,              //Force the use of off-global offensive abilities, regardless of weaving conditions
        AnyWeave,           //Force the use of off-global offensive abilities in any next possible weave slot
        EarlyWeave,         //Force the use of off-global offensive abilities in very next FIRST weave slot only
        LateWeave,          //Force the use of off-global offensive abilities in very next LAST weave slot only
        Delay               //Delay the use of offensive abilities for strategic reasons
    }
    #endregion

    #region Module & Strategy Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi PLD", "Standard Rotation Module", "Standard rotation (Akechi)", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.GLA, (int)Class.PLD), 100);

        res.Define(Track.AoE).As<AOEStrategy>("AoE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto (Finish Combo)", "Auto-selects best rotation dependant on targets; Finishes combo first", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoBreakCombo, "Auto (Break Combo)", "Auto-selects best rotation dependant on targets; Breaks combo if needed", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceST, "Use AoE", "Force single-target rotation")
            .AddOption(AOEStrategy.ForceAoE, "Force AoE", "Force AoE rotation");
        res.Define(Track.Cooldowns).As<CooldownStrategy>("Cooldowns", uiPriority: 190)
            .AddOption(CooldownStrategy.Allow, "Allow", "Allow use of cooldowns")
            .AddOption(CooldownStrategy.Hold, "Hold", "Hold all cooldowns");
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 180)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use potions automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Align With Raid Buffs", "Align potion usage with raid buffs", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potions immediately when available", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.Atonement).As<AtonementStrategy>("Atonement", "Atone", uiPriority: 160)
            .AddOption(AtonementStrategy.Automatic, "Automatic", "Normal use of Atonement & it's combo")
            .AddOption(AtonementStrategy.ForceAtonement, "Force Atonement", "Force use of Atonement", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSupplication, "Force Supplication", "Force use of Supplication", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSepulchre, "Force Sepulchre", "Force use of Sepulchre", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.Delay, "Delay", "Delay use of Atonement & its combo chain", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Atonement, AID.Supplication, AID.Sepulchre);
        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blade Combo", "Blades", uiPriority: 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatic", "Normal use of Confiteor & Blades Combo")
            .AddOption(BladeComboStrategy.ForceConfiteor, "Force", "Force use of Confiteor", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(BladeComboStrategy.ForceFaith, "Force Faith", "Force use of Blade of Faith", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceTruth, "Force Truth", "Force use of Blade of Truth", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceValor, "Force Valor", "Force use of Blade of Valor", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.Delay, "Delay", "Delay use of Confiteor & Blade Combo", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Confiteor, AID.BladeOfFaith, AID.BladeOfTruth, AID.BladeOfValor);
        res.Define(Track.Holy).As<HolyStrategy>("Holy Spirit / Circle", "Holy", uiPriority: 150)
            .AddOption(HolyStrategy.Automatic, "Automatic", "Automatically choose which Holy action to use based on conditions")
            .AddOption(HolyStrategy.Spirit, "Spirit", "Force use of Holy Spirit", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.Circle, "Circle", "Force use of Holy Circle", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.Delay, "Delay", "Delay use of Holy actions", 0, 0, ActionTargets.None, 64)
            .AddAssociatedActions(AID.HolySpirit, AID.HolyCircle);
        res.Define(Track.Dash).As<DashStrategy>("Intervene", "Dash", uiPriority: 150)
            .AddOption(DashStrategy.Automatic, "Automatic", "Normal use of Intervene")
            .AddOption(DashStrategy.Force, "Force", "Force use of Intervene", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.Force1, "Force (Hold 1)", "Force use of Intervene; Hold one charge for manual usage", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.GapClose, "Gap Close", "Use as gap closer if outside melee range", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.GapClose1, "Gap Close (Hold 1)", "Use as gap closer if outside melee range; Hold one charge for manual usage", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.Delay, "Delay", "Delay use of Intervene", 0, 0, ActionTargets.None, 66)
            .AddAssociatedActions(AID.Intervene);
        res.Define(Track.Ranged).As<RangedStrategy>("Ranged", "Ranged", uiPriority: 140)
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
        res.Define(Track.FightOrFlight).As<OGCDStrategy>("Fight or Flight", "F.Flight", uiPriority: 170)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Fight or Flight normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Fight or Flight", 60, 20, ActionTargets.Self, 2)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Fight or Flight in any weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Fight or Flight in the first weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Fight or Flight in the last weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Fight or Flight", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.FightOrFlight);
        res.Define(Track.Requiescat).As<OGCDStrategy>("Requiescat", "R.scat", uiPriority: 170)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Requiescat normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Requiescat / Imperator", 60, 20, ActionTargets.Self, 68)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Requiescat / Imperator in any weave slot", 60, 20, ActionTargets.Self, 68)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Requiescat / Imperator in the first weave slot", 60, 20, ActionTargets.Self, 68)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Requiescat / Imperator in the last weave slot", 60, 20, ActionTargets.Self, 68)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Requiescat / Imperator", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(AID.Requiescat, AID.Imperator);
        res.Define(Track.SpiritsWithin).As<OGCDStrategy>("Spirits Within", "S.Within", uiPriority: 150)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Spirits Within normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Spirits Within / Expiacion", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Spirits Within / Expiacion in any weave slot", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Spirits Within / Expiacion in the first weave slot", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Spirits Within / Expiacion in the last weave slot", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Spirits Within / Expiacion", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.SpiritsWithin, AID.Expiacion);
        res.Define(Track.CircleOfScorn).As<OGCDStrategy>("Circle of Scorn", "C.Scorn", uiPriority: 150)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Circle of Scorn normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Circle of Scorn ASAP", 60, 15, ActionTargets.Hostile, 50)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Circle of Scorn in any weave slot", 60, 15, ActionTargets.Hostile, 50)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Circle of Scorn in the first weave slot", 60, 15, ActionTargets.Hostile, 50)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Circle of Scorn in the last weave slot", 60, 15, ActionTargets.Hostile, 50)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Circle of Scorn", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(AID.CircleOfScorn);
        res.Define(Track.GoringBlade).As<GCDStrategy>("Goring Blade", "G.Blade", uiPriority: 150)
            .AddOption(GCDStrategy.Automatic, "Automatic", "Use Goring Blade normally")
            .AddOption(GCDStrategy.Force, "Force", "Force use of Goring Blade ASAP", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(GCDStrategy.Delay, "Delay", "Delay use of Goring Blade", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(AID.GoringBlade);
        res.Define(Track.BladeOfHonor).As<OGCDStrategy>("Blade of Honor", "B.Honor", uiPriority: 150)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Blade of Honor normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Blade of Honor ASAP", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Blade of Honor in any weave slot", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Blade of Honor in the first weave slot", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Blade of Honor in the last weave slot", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Blade of Honor", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(AID.BladeOfHonor);

        return res;

    }
    #endregion

    #region Priorities
    public enum GCDPriority //Priority for GCDs used
    {
        None = 0,
        Combo123 = 350,
        NormalGCD = 450,
        HolyCircle = 490,
        HolySpirit = 500,
        Atonement = 580,
        GoringBlade = 590,
        Valor = 600,
        Truth = 610,
        Faith = 620,
        Confiteor = 650,
        ForcedGCD = 900,
    }
    public enum OGCDPriority //Priority for oGCDs used
    {
        None = 0,
        BladeOfHonor = 520,
        Intervene = 530,
        SpiritsWithin = 540,
        CircleOfScorn = 550,
        Requiescat = 650,
        FightOrFlight = 700,
        ForcedOGCD = 900,
        Potion = 910,
    }
    #endregion

    #region Variables

    public int Oath; //Current value of the oath gauge
    public int BladeComboStep; //Current step in the Confiteor combo sequence
    public float GCDLength; //Length of the global cooldown, adjusted by skill speed and haste (baseline: 2.5s)
    public float PotionLeft; //Remaining duration of the potion buff (typically 30s)
    public float RaidBuffsLeft; //Remaining duration of active raid-wide buffs (typically 20s-22s)
    public float RaidBuffsIn; //Time until the next set of raid-wide buffs are applied (typically 20s-22s)
    public float CooldownsWindowLeft; //Time remaining in the current Cooldowns window (typically 20s-22s)
    public float CooldownsWindowIn; //Time until the next Cooldowns window begins (typically 20s-22s)
    public (float Left, bool IsActive) DivineMight; //Conditions for the Divine Might buff
    public (float CD, float Left, bool IsReady, bool IsActive) FightOrFlight; //Conditions for Fight or Flight ability
    public (float CD, bool IsReady) SpiritsWithin; //Conditions for Spirits Within ability
    public (float CD, bool IsReady) CircleOfScorn; //Conditions for Circle of Scorn ability
    public (float CD, float Left, bool IsReady, bool IsActive) GoringBlade; //Conditions for Goring Blade ability
    public (float TotalCD, bool HasCharges, bool IsReady) Intervene; //Conditions for Intervene ability
    public (float CD, int Stacks, bool IsReady, bool IsActive) Requiescat; //Conditions for Requiescat ability
    public (float Left, bool IsReady, bool IsActive) Atonement; //Conditions for Atonement ability
    public (float Left, bool IsReady, bool IsActive) Supplication; //Conditions for Supplication ability
    public (float Left, bool IsReady, bool IsActive) Sepulchre; //Conditions for Sepulchre ability
    public (float Left, bool HasMP, bool IsReady, bool IsActive) Confiteor; //Conditions for Confiteor ability
    public (float Left, bool IsReady, bool IsActive) BladeOfHonor; //Conditions for Blade of Honor ability
    public (bool HasMP, bool IsReady) HolySpirit; //Conditions for Holy Spirit ability
    public (bool HasMP, bool IsReady) HolyCircle; //Conditions for Holy Circle ability
    public uint MP; //Current MP (mana points) of the player
    public bool ShouldUseAOE; //Check if AOE rotation should be used
    public bool ShouldNormalHolyCircle; //Check if Holy Circle should be used
    public bool ShouldUseDMHolyCircle; //Check if Holy Circle should be used under Divine Might
    public AID NextGCD; //The next action to be executed during the global cooldown (for cartridge management)
    public bool canWeaveIn; //Can weave in oGCDs
    public bool canWeaveEarly; //Can early weave oGCDs
    public bool canWeaveLate; //Can late weave oGCDs
    private delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);
    #endregion

    #region Module Helpers
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private bool IsOffCooldown(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f; //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool ActionReady(AID aid) => Unlocked(aid) && IsOffCooldown(aid); //Check if the desired action is unlocked and off cooldown
    private AID ComboLastMove => (AID)World.Client.ComboState.Action; //Get the last action used in the combo sequence
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 2.99f; //Check if the target is within ST melee range (3 yalms)
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.99f; //Check if the target is within AOE melee range (5 yalms)
    private bool In20y(Actor? target) => Player.DistanceToHitbox(target) <= 19.99f; //Check if the target is within 20 yalms
    private bool In25y(Actor? target) => Player.DistanceToHitbox(target) <= 24.99f; //Check if the target is within 25 yalms
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Check if this is the first GCD in combat
    private int TargetsHitByPlayerAOE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //Returns the number of targets hit by AoE within a 5-yalm radius around the player
    private int TargetsHitByTargetAOE(Actor? target) => Hints.NumPriorityTargetsInAOECircle(target!.Position, 5); //Returns the number of targets hit by AoE within a 5-yalm radius around the target
    private PositionCheck IsSplashTarget => (Actor primary, Actor other) => Hints.TargetInAOECircle(other, primary.Position, 5);
    public bool HasEffect(SID sid) => SelfStatusLeft(sid) > 0; //Checks if Status effect is on self
    public float CombatTimer => (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds; //Calculates the elapsed time since combat started in seconds
    public Actor? TargetChoice(StrategyValues.OptionRef strategy) => ResolveTargetOverride(strategy.Value);
    #endregion

    #region Upgrade Paths
    public AID BestSpirits
        => Unlocked(AID.Expiacion) //if Expiacion is unlocked
        ? AID.Expiacion //then use Expiacion
        : AID.SpiritsWithin; //otherwise use Spirits Within
    public AID BestRequiescat
        => Unlocked(AID.Imperator) //if Imperator is unlocked
        ? AID.Imperator //then use Imperator
        : AID.Requiescat; //otherwise use Requiescat
    public AID BestHoly
        => ShouldUseDMHolyCircle || ShouldNormalHolyCircle //if Holy Circle should be used
        ? AID.HolyCircle //then use Holy Circle
        : AID.HolySpirit; //otherwise use Holy Spirit
    public AID BestHolyCircle //for AOE use from Lv64-Lv71
        => HolyCircle.IsReady //if Holy Circle is ready
        ? AID.HolyCircle //then use Holy Circle
        : AID.HolySpirit; //then use Holy Spirit
    public AID BestAtonement
        => Sepulchre.IsReady //if Sepulchre is ready
        ? AID.Sepulchre //then use Sepulchre
        : Supplication.IsReady //if Supplication is ready
        ? AID.Supplication //then use Supplication
        : AID.Atonement; //otherwise use Atonement
    public AID BestBlade
        => BladeComboStep is 3
        ? AID.BladeOfValor
        : BladeComboStep is 2
        ? AID.BladeOfTruth
        : BladeComboStep is 1
        ? AID.BladeOfFaith
        : AID.Confiteor;
    #endregion
    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        var gauge = World.Client.GetGauge<PaladinGauge>(); //Retrieve current Paladin gauge
        Oath = gauge.OathGauge; //Get the current value of the Oath gauge
        BladeComboStep = gauge.ConfiteorComboStep; //Get the current step in the Confiteor/Blades combo
        MP = Player.HPMP.CurMP; //Current mana points (MP) of the player
        DivineMight.Left = SelfStatusLeft(SID.DivineMight, 30); //Remaining duration of the Divine Might buff
        DivineMight.IsActive = DivineMight.Left > 0; //Check if the Divine Might buff is active
        FightOrFlight.CD = CD(AID.FightOrFlight); //Remaining cooldown for the Fight or Flight ability
        FightOrFlight.Left = SelfStatusLeft(SID.FightOrFlight, 20); //Remaining duration of the Fight or Flight buff
        FightOrFlight.IsActive = FightOrFlight.CD is >= 39.5f and <= 60; //Check if the Fight or Flight buff is active 
        FightOrFlight.IsReady = ActionReady(AID.FightOrFlight); //Fight or Flight ability
        SpiritsWithin.CD = CD(AID.SpiritsWithin); //Remaining cooldown for the Spirits Within ability
        SpiritsWithin.IsReady = Unlocked(AID.SpiritsWithin) && IsOffCooldown(BestSpirits); //Spirits Within ability
        CircleOfScorn.CD = CD(AID.CircleOfScorn); //Remaining cooldown for the Circle of Scorn ability
        CircleOfScorn.IsReady = Unlocked(AID.CircleOfScorn) && IsOffCooldown(AID.CircleOfScorn); //Circle of Scorn ability
        GoringBlade.CD = CD(AID.GoringBlade); //Remaining cooldown for the Goring Blade ability
        GoringBlade.Left = SelfStatusLeft(SID.GoringBladeReady, 30); //Remaining duration of the Goring Blade buff
        GoringBlade.IsActive = GoringBlade.Left > 0; //Check if the Goring Blade buff is active
        GoringBlade.IsReady = Unlocked(AID.GoringBlade) && GoringBlade.IsActive; //Goring Blade ability
        Intervene.TotalCD = CD(AID.Intervene); //Total cooldown for the Intervene ability (60s)
        Intervene.HasCharges = Intervene.TotalCD <= 30f; //Check if the player has charges of Intervene
        Intervene.IsReady = Unlocked(AID.Intervene) && Intervene.HasCharges; //Intervene ability
        Requiescat.CD = CD(BestRequiescat); //Remaining cooldown for the Requiescat ability
        Requiescat.Stacks = SelfStatusDetails(SID.Requiescat).Stacks; //Get the current number of Requiescat stacks 
        Requiescat.IsActive = Requiescat.Stacks > 0; //Check if the Requiescat buff is active
        Requiescat.IsReady = Unlocked(AID.Requiescat) && Requiescat.IsActive; //Requiescat ability
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
        var canBlade = Unlocked(AID.BladeOfValor) && BladeComboStep is not 0; //Blade abilities, only if combo step is not zero
        BladeOfHonor.Left = SelfStatusLeft(SID.BladeOfHonorReady, 30); //Remaining duration of the Blade of Honor buff
        BladeOfHonor.IsActive = BladeOfHonor.Left > 0; //Check if the Blade of Honor buff is active
        BladeOfHonor.IsReady = Unlocked(AID.BladeOfHonor) && IsOffCooldown(AID.BladeOfHonor); //Blade of Honor ability
        HolySpirit.HasMP = MP >= 1000; //Check if the player has enough mana points for Holy Spirit
        HolySpirit.IsReady = Unlocked(AID.HolySpirit) && HolySpirit.HasMP; //Holy Spirit ability
        HolyCircle.HasMP = MP >= 1000; //Check if the player has enough mana points for Holy Circle
        HolyCircle.IsReady = Unlocked(AID.HolyCircle) && HolyCircle.HasMP; //Holy Circle ability
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level); //Calculate GCD based on skill speed and haste
        PotionLeft = PotionStatusLeft(); //Remaining duration of the potion buff (typically 30s)
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget); //Estimate remaining and incoming raid buff durations
        NextGCD = AID.None; //Initialize next action as none
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        ShouldUseAOE = TargetsHitByPlayerAOE() > 2; //Check if AOE rotation should be used
        ShouldNormalHolyCircle = !DivineMight.IsActive && TargetsHitByPlayerAOE() > 3; //Check if Holy Circle should be used
        ShouldUseDMHolyCircle = DivineMight.IsActive && TargetsHitByPlayerAOE() > 2;

        #region Strategy Options
        var AOE = strategy.Option(Track.AoE); //Retrieves AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //Retrieves AOE strategy
        var cd = strategy.Option(Track.Cooldowns); //Retrieves Cooldowns track
        var cdStrat = cd.As<CooldownStrategy>(); //Retrieves Cooldowns strategy
        var pot = strategy.Option(Track.Potion); //Retrieves Potion track
        var potStrat = pot.As<PotionStrategy>(); //Retrieves Potion strategy
        var fof = strategy.Option(Track.FightOrFlight);
        var fofStrat = fof.As<OGCDStrategy>();
        var req = strategy.Option(Track.Requiescat);
        var reqStrat = req.As<OGCDStrategy>();
        var atone = strategy.Option(Track.Atonement); //Retrieves Atonement track
        var atoneStrat = atone.As<AtonementStrategy>(); //Retrieves Atonement strategy
        var blade = strategy.Option(Track.BladeCombo); //Retrieves Blade Combo track
        var bladeStrat = blade.As<BladeComboStrategy>();
        var cos = strategy.Option(Track.CircleOfScorn);
        var cosStrat = cos.As<OGCDStrategy>();
        var sw = strategy.Option(Track.SpiritsWithin);
        var swStrat = sw.As<OGCDStrategy>();
        var dash = strategy.Option(Track.Dash);
        var dashStrat = dash.As<DashStrategy>();
        var gb = strategy.Option(Track.GoringBlade);
        var gbStrat = gb.As<GCDStrategy>();
        var boh = strategy.Option(Track.BladeOfHonor);
        var bohStrat = boh.As<OGCDStrategy>();
        var holy = strategy.Option(Track.Holy);
        var holyStrat = holy.As<HolyStrategy>();
        var ranged = strategy.Option(Track.Ranged);
        var rangedStrat = ranged.As<RangedStrategy>();
        #endregion

        #endregion

        #region Standard Execution
        if (AOEStrategy == AOEStrategy.AutoBreakCombo) //if Break Combo option is selected
        {
            if (ShouldUseAOE) //if AOE rotation should be used
                QueueGCD(RotationAOE(), //queue the next AOE combo action
                    Player, //on Self (no target needed)
                    GCDPriority.Combo123); //with priority for 123/12 combo actions
            if (!ShouldUseAOE)
                QueueGCD(RotationST(), //queue the next single-target combo action
                    TargetChoice(AOE) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    GCDPriority.Combo123); //with priority for 123/12 combo actions
        }
        if (AOEStrategy == AOEStrategy.AutoFinishCombo) //if Finish Combo option is selected
        {
            QueueGCD(BestRotation(), //queue the next single-target combo action only if combo is finished
                TargetChoice(AOE) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                GCDPriority.Combo123); //with priority for 123/12 combo actions
        }
        #endregion

        #region Cooldowns Execution
        var hold = cdStrat == CooldownStrategy.Hold;
        if (!hold)
        {
            if (ShouldUseFightOrFlight(fofStrat, primaryTarget))
                QueueOGCD(AID.FightOrFlight,
                    Player,
                    fofStrat is OGCDStrategy.Force
                    or OGCDStrategy.AnyWeave
                    or OGCDStrategy.EarlyWeave
                    or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD
                    : OGCDPriority.FightOrFlight);
            if (ShouldUseRequiescat(reqStrat, primaryTarget))
                QueueOGCD(BestRequiescat,
                    TargetChoice(req) ?? primaryTarget,
                    reqStrat is OGCDStrategy.Force
                    or OGCDStrategy.AnyWeave
                    or OGCDStrategy.EarlyWeave
                    or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD
                    : OGCDPriority.Requiescat);
            if (bladeStrat == BladeComboStrategy.ForceConfiteor)
                QueueGCD(AID.Confiteor,
                    TargetChoice(blade) ?? primaryTarget,
                    GCDPriority.ForcedGCD);
            if (ShouldUseCircleOfScorn(cosStrat, primaryTarget))
                QueueOGCD(AID.CircleOfScorn,
                    Player,
                    cosStrat is OGCDStrategy.Force
                    or OGCDStrategy.AnyWeave
                    or OGCDStrategy.EarlyWeave
                    or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD
                    : OGCDPriority.CircleOfScorn);
            if (ShouldUseSpiritsWithin(swStrat, primaryTarget))
                QueueOGCD(BestSpirits,
                    TargetChoice(sw) ?? primaryTarget,
                    swStrat is OGCDStrategy.Force
                    or OGCDStrategy.AnyWeave
                    or OGCDStrategy.EarlyWeave
                    or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD
                    : OGCDPriority.SpiritsWithin);
            if (ShouldUseDash(dashStrat, primaryTarget))
                QueueOGCD(AID.Intervene,
                    TargetChoice(dash) ?? primaryTarget,
                    dashStrat is DashStrategy.Force
                    or DashStrategy.Force1
                    or DashStrategy.GapClose
                    or DashStrategy.GapClose1
                    ? OGCDPriority.ForcedOGCD
                    : OGCDPriority.Intervene);
            if (HasEffect(SID.BladeOfHonorReady))
                QueueOGCD(AID.BladeOfHonor,
                    TargetChoice(boh) ?? primaryTarget,
                    bohStrat is OGCDStrategy.Force
                    or OGCDStrategy.AnyWeave
                    or OGCDStrategy.EarlyWeave
                    or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD
                    : OGCDPriority.BladeOfHonor);
            if (ShouldUseGoringBlade(gbStrat, primaryTarget))
                QueueGCD(AID.GoringBlade,
                    TargetChoice(gb) ?? primaryTarget,
                    gbStrat is GCDStrategy.Force
                    ? GCDPriority.ForcedGCD
                    : GCDPriority.GoringBlade);
        }

        if (canBlade &&
            ShouldUseBladeCombo(bladeStrat, primaryTarget))
        {
            if (bladeStrat is BladeComboStrategy.Automatic)
                QueueGCD(BestBlade,
                    TargetChoice(blade) ?? primaryTarget,
                    GCDPriority.Confiteor);
            if (bladeStrat is BladeComboStrategy.ForceFaith)
                QueueGCD(AID.BladeOfFaith,
                    TargetChoice(blade) ?? primaryTarget,
                    GCDPriority.ForcedGCD);
            if (BladeComboStep is 2)
                QueueGCD(AID.BladeOfTruth,
                    TargetChoice(blade) ?? primaryTarget,
                    bladeStrat is BladeComboStrategy.ForceTruth
                    ? GCDPriority.ForcedGCD
                    : GCDPriority.Truth);
            if (BladeComboStep is 3)
                QueueGCD(AID.BladeOfValor,
                    TargetChoice(blade) ?? primaryTarget,
                    bladeStrat is BladeComboStrategy.ForceValor
                    ? GCDPriority.ForcedGCD
                    : GCDPriority.Valor);
        }
        if (ShouldUseAtonement(atoneStrat, primaryTarget))
        {
            if (atoneStrat == AtonementStrategy.Automatic)
                QueueGCD(BestAtonement,
                    TargetChoice(atone) ?? primaryTarget,
                    GCDPriority.Atonement);
            if (atoneStrat is AtonementStrategy.ForceAtonement)
                QueueGCD(AID.Atonement,
                    TargetChoice(atone) ?? primaryTarget,
                    GCDPriority.ForcedGCD);
            if (atoneStrat is AtonementStrategy.ForceSupplication)
                QueueGCD(AID.Supplication,
                    TargetChoice(atone) ?? primaryTarget,
                    GCDPriority.ForcedGCD);
            if (atoneStrat is AtonementStrategy.ForceSepulchre)
                QueueGCD(AID.Sepulchre,
                    TargetChoice(atone) ?? primaryTarget,
                    GCDPriority.ForcedGCD);

        }

        if (ShouldUseHoly(holyStrat, primaryTarget))
        {
            if (holyStrat == HolyStrategy.Automatic)
                QueueGCD(BestHoly, TargetChoice(holy) ?? primaryTarget, GCDPriority.HolySpirit);
            if (holyStrat == HolyStrategy.Spirit)
                QueueGCD(AID.HolySpirit, TargetChoice(holy) ?? primaryTarget, GCDPriority.ForcedGCD);
            if (holyStrat == HolyStrategy.Circle)
                QueueGCD(BestHolyCircle, Player, GCDPriority.ForcedGCD);
        }

        if (ShouldUseRangedLob(primaryTarget, rangedStrat))
            QueueGCD(AID.ShieldLob, TargetChoice(ranged) ?? primaryTarget, rangedStrat is RangedStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.Combo123);
        if (ShouldUseRangedCast(primaryTarget, rangedStrat))
            QueueGCD(AID.HolySpirit, TargetChoice(ranged) ?? primaryTarget, rangedStrat is RangedStrategy.ForceCast ? GCDPriority.ForcedGCD : GCDPriority.Combo123);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
        #endregion
    }

    #region Core Execution Helpers
    public void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum => QueueGCD(aid, target, (int)(object)priority, delay);
    public void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum => QueueOGCD(aid, target, (int)(object)priority, delay);
    public void QueueGCD(AID aid, Actor? target, int priority = 8, float delay = 0)
    {
        var NextGCDPrio = 0;
        if (priority == 0)
            return;
        if (QueueAction(aid, target, ActionQueue.Priority.High + priority, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
        }
    }
    public void QueueOGCD(AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;
        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }
    public bool QueueAction(AID aid, Actor? target, float priority, float delay)
    {
        Vector3 targetPos = default;
        var def = ActionDefinitions.Instance.Spell(aid);
        if ((uint)(object)aid == 0)
            return false;
        if (def == null)
            return false;
        if (def.Range != 0 && target == null)
        {
            return false;
        }
        if (def.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (def.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }
        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, targetPos: targetPos);
        return true;
    }
    #endregion

    #region Rotation Helpers
    private AID RotationST() => ComboLastMove switch
    {
        AID.RiotBlade => Unlocked(AID.RoyalAuthority) ? AID.RoyalAuthority : AID.RageOfHalone, //If Riot Blade was last used, choose Royal Authority if available, otherwise Rage of Halone
        AID.FastBlade => AID.RiotBlade, //If Fast Blade was last used, go back to Riot Blade
        _ => AID.FastBlade, //Default to Fast Blade if no recognized last action
    };
    private AID RotationAOE() => ComboLastMove switch
    {
        AID.TotalEclipse => AID.Prominence, //If Total Eclipse was last used, use Prominence next
        _ => AID.TotalEclipse, //Default to Total Eclipse if no recognized last action
    };
    private AID BestRotation() => ComboLastMove switch
    {
        //ST
        AID.RoyalAuthority => ShouldUseAOE ? RotationAOE() : RotationST(),
        AID.RageOfHalone => ShouldUseAOE ? RotationAOE() : RotationST(),
        AID.RiotBlade => RotationST(),
        AID.FastBlade => RotationST(),
        //AOE
        AID.TotalEclipse => ShouldUseAOE ? RotationAOE() : RotationST(),
        AID.Prominence => RotationAOE(),
        _ => ShouldUseAOE ? RotationAOE() : RotationST(),
    };
    #endregion

    #region Cooldown Helpers
    private bool ShouldUseRangedLob(Actor? target, RangedStrategy strategy) => strategy switch
    {
        RangedStrategy.OpenerRanged => IsFirstGCD() && !In3y(target), //Use if it's the first GCD and target is not within 3y
        RangedStrategy.Opener => IsFirstGCD(), //Use if it's the first GCD
        RangedStrategy.Force => true, //Force
        RangedStrategy.Ranged => !In3y(target), //Use if target is not within 3y
        RangedStrategy.Forbid => false, //Delay usage
        _ => false
    };
    private bool ShouldUseRangedCast(Actor? target, RangedStrategy strategy) => strategy switch
    {
        RangedStrategy.OpenerRangedCast => IsFirstGCD() && !In3y(target), //Use if it's the first GCD and target is not within 3y
        RangedStrategy.OpenerCast => IsFirstGCD(), //Use if it's the first GCD
        RangedStrategy.ForceCast => true, //Force
        RangedStrategy.RangedCast => !In3y(target), //Use if target is not within 3y
        _ => false
    };
    private bool ShouldUseFightOrFlight(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            FightOrFlight.IsReady &&
            CombatTimer >= GCDLength * 2 + 0.5f,
        OGCDStrategy.Force => true,
        OGCDStrategy.AnyWeave => canWeaveIn,
        OGCDStrategy.EarlyWeave => canWeaveEarly,
        OGCDStrategy.LateWeave => canWeaveLate,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseRequiescat(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            Requiescat.IsReady &&
            FightOrFlight.IsActive,
        OGCDStrategy.Force => true,
        OGCDStrategy.AnyWeave => canWeaveIn,
        OGCDStrategy.EarlyWeave => canWeaveEarly,
        OGCDStrategy.LateWeave => canWeaveLate,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseSpiritsWithin(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            In3y(target) &&
            FightOrFlight.CD is < 57.55f and > 17 &&
            SpiritsWithin.IsReady,
        OGCDStrategy.Force => true,
        OGCDStrategy.AnyWeave => canWeaveIn,
        OGCDStrategy.EarlyWeave => canWeaveEarly,
        OGCDStrategy.LateWeave => canWeaveLate,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseCircleOfScorn(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            CircleOfScorn.IsReady &&
            In5y(target) &&
            FightOrFlight.CD is < 57.55f and > 17,
        OGCDStrategy.Force => true,
        OGCDStrategy.AnyWeave => canWeaveIn,
        OGCDStrategy.EarlyWeave => canWeaveEarly,
        OGCDStrategy.LateWeave => canWeaveLate,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseGoringBlade(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic =>
            Player.InCombat &&
            In3y(target) &&
            GoringBlade.IsReady &&
            FightOrFlight.IsActive,
        GCDStrategy.Force => true,
        GCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBladeCombo(BladeComboStrategy strategy, Actor? target) => strategy switch
    {
        BladeComboStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            In25y(target) &&
            Confiteor.IsReady &&
            FightOrFlight.IsActive &&
            BladeComboStep is 0,
        BladeComboStrategy.ForceConfiteor => Confiteor.IsReady && BladeComboStep is 0,
        BladeComboStrategy.ForceFaith => BladeComboStep is 1,
        BladeComboStrategy.ForceTruth => BladeComboStep is 2,
        BladeComboStrategy.ForceValor => BladeComboStep is 3,
        BladeComboStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseAtonement(AtonementStrategy strategy, Actor? target) => strategy switch
    {
        AtonementStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            In3y(target) &&
            Atonement.IsReady || Supplication.IsReady || Sepulchre.IsReady,
        AtonementStrategy.ForceAtonement => Atonement.IsReady,
        AtonementStrategy.ForceSupplication => Supplication.IsReady,
        AtonementStrategy.ForceSepulchre => Sepulchre.IsReady,
        AtonementStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseHoly(HolyStrategy strategy, Actor? target) => strategy switch
    {
        HolyStrategy.Automatic =>
            ShouldUseDMHolyCircle || ShouldNormalHolyCircle
            ? ShouldUseHolyCircle(HolyStrategy.Automatic, target)
            : ShouldUseHolySpirit(HolyStrategy.Automatic, target),
        HolyStrategy.Spirit => HolySpirit.IsReady,
        HolyStrategy.Circle => HolyCircle.IsReady,
        HolyStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseHolySpirit(HolyStrategy strategy, Actor? target) => strategy switch
    {
        HolyStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In25y(target) && //Target in range
            HolyCircle.IsReady && //can execute Holy Circle
            DivineMight.IsActive, //Divine Might is active
        _ => false
    };
    private bool ShouldUseHolyCircle(HolyStrategy strategy, Actor? target) => strategy switch
    {
        HolyStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In5y(target) && //Target in range
            HolyCircle.IsReady && //can execute Holy Circle
            DivineMight.IsActive, //Divine Might is active
        _ => false
    };
    private bool ShouldUseDash(DashStrategy strategy, Actor? target) => strategy switch
    {
        DashStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            FightOrFlight.IsActive,
        DashStrategy.Force => true,
        DashStrategy.Force1 => Intervene.TotalCD < 1f,
        DashStrategy.GapClose => !In3y(target),
        DashStrategy.GapClose1 => Intervene.TotalCD < 1f && !In3y(target),
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs =>  FightOrFlight.CD < 5 && Requiescat.CD < 15, //Align potions with major buffs
        PotionStrategy.Immediate => true, //Force potion immediately
        _ => false,
    };
    #endregion
}

