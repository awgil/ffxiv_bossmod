using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
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
        AOE,                //Tracks both AOE and single-target rotations
        Cooldowns,          //Tracks Cooldowns damage actions
        Potion,             //Tracks potion usage
        Atonement,          //Tracks Atonement combo actions
        BladeCombo,         //Tracks Blade combo actions
        Holy,               //Tracks Holy actions
        Dash,               //Tracks Intervene
        Ranged,             //Tracks ranged attacks
        FightOrFlight,      //Tracks Fight or Flight
        Requiescat,         //Tracks Requiescat actions
        SpiritsWithin,      //Tracks Spirits Within actions
        CircleOfScorn,      //Tracks Circle of Scorn
        GoringBlade,        //Tracks Goring Blade
        BladeOfHonor,       //Tracks Blade of Honor
    }
    public enum AOEStrategy
    {
        AutoFinishCombo,    //Automatically decide based on targets; finish combo first
        AutoBreakCombo,     //Automatically decide based on targets; break combo if needed
        ForceST,            //Force single-target rotation
        ForceAOE            //Force AOE rotation
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
        Automatic,          //Automatically decide on ranged attacks based on conditions
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
        var res = new RotationModuleDefinition("Akechi PLD",
            "Standard Rotation Module",
            "Standard rotation (Akechi)",
            "Akechi",
            RotationModuleQuality.Good,
            BitMask.Build((int)Class.GLA, (int)Class.PLD),
            100);

        //AOE definitions
        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto (Finish Combo)", "Auto-selects best rotation dependant on targets; Finishes combo first", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoBreakCombo, "Auto (Break Combo)", "Auto-selects best rotation dependant on targets; Breaks combo if needed", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceST, "Use AOE", "Force single-target rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "Force AOE", "Force AOE rotation");
        //Cooldown definitions
        res.Define(Track.Cooldowns).As<CooldownStrategy>("Cooldowns", uiPriority: 190)
            .AddOption(CooldownStrategy.Allow, "Allow", "Allow use of cooldowns")
            .AddOption(CooldownStrategy.Hold, "Hold", "Hold all cooldowns");
        //Potion definitions
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 180)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use potions automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Align With Raid Buffs", "Align potion usage with raid buffs", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potions immediately when available", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        //Atonement Combo definitions
        res.Define(Track.Atonement).As<AtonementStrategy>("Atonement", "Atones", uiPriority: 160)
            .AddOption(AtonementStrategy.Automatic, "Automatic", "Normal use of Atonement & its combo chain")
            .AddOption(AtonementStrategy.ForceAtonement, "Force Atonement", "Force use of Atonement", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSupplication, "Force Supplication", "Force use of Supplication", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSepulchre, "Force Sepulchre", "Force use of Sepulchre", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.Delay, "Delay", "Delay use of Atonement & its combo chain", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Atonement, AID.Supplication, AID.Sepulchre);
        //Blade Combo definitions
        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blade Combo", "Blades", uiPriority: 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatic", "Normal use of Confiteor & Blades combo chain")
            .AddOption(BladeComboStrategy.ForceConfiteor, "Force", "Force use of Confiteor", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(BladeComboStrategy.ForceFaith, "Force Faith", "Force use of Blade of Faith", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceTruth, "Force Truth", "Force use of Blade of Truth", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceValor, "Force Valor", "Force use of Blade of Valor", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.Delay, "Delay", "Delay use of Confiteor & Blades combo chain", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Confiteor, AID.BladeOfFaith, AID.BladeOfTruth, AID.BladeOfValor);
        //Holy action definitions
        res.Define(Track.Holy).As<HolyStrategy>("Holy Spirit / Circle", "Holy S/C", uiPriority: 150)
            .AddOption(HolyStrategy.Automatic, "Automatic", "Automatically choose which Holy action to use based on conditions")
            .AddOption(HolyStrategy.Spirit, "Spirit", "Force use of Holy Spirit", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.Circle, "Circle", "Force use of Holy Circle", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.Delay, "Delay", "Delay use of Holy actions", 0, 0, ActionTargets.None, 64)
            .AddAssociatedActions(AID.HolySpirit, AID.HolyCircle);
        //Intervene definitions
        res.Define(Track.Dash).As<DashStrategy>("Intervene", "Dash", uiPriority: 150)
            .AddOption(DashStrategy.Automatic, "Automatic", "Normal use of Intervene")
            .AddOption(DashStrategy.Force, "Force", "Force use of Intervene", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.Force1, "Force (Hold 1)", "Force use of Intervene; Hold one charge for manual usage", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.GapClose, "Gap Close", "Use as gap closer if outside melee range", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.GapClose1, "Gap Close (Hold 1)", "Use as gap closer if outside melee range; Hold one charge for manual usage", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.Delay, "Delay", "Delay use of Intervene", 0, 0, ActionTargets.None, 66)
            .AddAssociatedActions(AID.Intervene);
        //Ranged attack definitions
        res.Define(Track.Ranged).As<RangedStrategy>("Ranged", "Ranged", uiPriority: 140)
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
        //Fight or Flight definitions
        res.Define(Track.FightOrFlight).As<OGCDStrategy>("Fight or Flight", "F.Flight", uiPriority: 170)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Fight or Flight normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Fight or Flight", 60, 20, ActionTargets.Self, 2)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Fight or Flight in any weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Fight or Flight in the first weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Fight or Flight in the last weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Fight or Flight", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.FightOrFlight);
        //Requiescat definitions
        res.Define(Track.Requiescat).As<OGCDStrategy>("Requiescat", "R.scat", uiPriority: 170)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Requiescat normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Requiescat / Imperator", 60, 30, ActionTargets.Hostile, 68)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Requiescat / Imperator in any weave slot", 60, 30, ActionTargets.Hostile, 68)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Requiescat / Imperator in the first weave slot", 60, 30, ActionTargets.Hostile, 68)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Requiescat / Imperator in the last weave slot", 60, 30, ActionTargets.Hostile, 68)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Requiescat / Imperator", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(AID.Requiescat, AID.Imperator);
        //Spirits Within definitions
        res.Define(Track.SpiritsWithin).As<OGCDStrategy>("Spirits Within", "S.Within", uiPriority: 150)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Spirits Within normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Spirits Within / Expiacion", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Spirits Within / Expiacion in any weave slot", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Spirits Within / Expiacion in the first weave slot", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Spirits Within / Expiacion in the last weave slot", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Spirits Within / Expiacion", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.SpiritsWithin, AID.Expiacion);
        //Circle of Scorn definitions
        res.Define(Track.CircleOfScorn).As<OGCDStrategy>("Circle of Scorn", "C.Scorn", uiPriority: 150)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use Circle of Scorn normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Circle of Scorn ASAP", 30, 15, ActionTargets.Self, 50)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Circle of Scorn in any weave slot", 30, 15, ActionTargets.Self, 50)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Circle of Scorn in the first weave slot", 30, 15, ActionTargets.Self, 50)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Circle of Scorn in the last weave slot", 30, 15, ActionTargets.Self, 50)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Circle of Scorn", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(AID.CircleOfScorn);
        //Goring Blade definitions
        res.Define(Track.GoringBlade).As<GCDStrategy>("Goring Blade", "G.Blade", uiPriority: 150)
            .AddOption(GCDStrategy.Automatic, "Automatic", "Use Goring Blade normally")
            .AddOption(GCDStrategy.Force, "Force", "Force use of Goring Blade ASAP", 0, 0, ActionTargets.Hostile, 54)
            .AddOption(GCDStrategy.Delay, "Delay", "Delay use of Goring Blade", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(AID.GoringBlade);
        //Blade of Honor definitions
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
        Combo123 = 300,
        HolySpirit = 400,
        Atonement = 500,
        GoringBlade = 600,
        Blades = 700,
        ForcedGCD = 900,
    }
    public enum OGCDPriority //Priority for oGCDs used
    {
        None = 0,
        BladeOfHonor = 400,
        Intervene = 450,
        SpiritsWithin = 500,
        CircleOfScorn = 550,
        Requiescat = 600,
        FightOrFlight = 700,
        ForcedOGCD = 900,
        Potion = 910,
    }
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
        ? BestHolyCircle //then use Holy Circle
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
        => BladeComboStep is 3 //if Confiteor combo is at step 3
        ? AID.BladeOfValor //then use Blade of Valor
        : BladeComboStep is 2 //if Confiteor combo is at step 2
        ? AID.BladeOfTruth //then use Blade of Truth
        : BladeComboStep is 1 //if Confiteor combo is at step 1
        && Unlocked(AID.BladeOfFaith)
        ? AID.BladeOfFaith //then use Blade of Faith
        : Unlocked(AID.Confiteor) //if Confiteor is unlocked
        ? AID.Confiteor //otherwise use Confiteor
        : BestHoly;
    #endregion

    #region Module Variables
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
    public (float CD, float Left, bool IsReady, bool IsActive) Requiescat; //Conditions for Requiescat ability
    public (float Left, bool IsReady, bool IsActive) Atonement; //Conditions for Atonement ability
    public (float Left, bool IsReady, bool IsActive) Supplication; //Conditions for Supplication ability
    public (float Left, bool IsReady, bool IsActive) Sepulchre; //Conditions for Sepulchre ability
    public (float Left, bool HasMP, bool IsReady, bool IsActive) Confiteor; //Conditions for Confiteor ability
    public (float Left, bool IsReady, bool IsActive) BladeOfHonor; //Conditions for Blade of Honor ability
    public (bool HasMP, bool IsReady) HolySpirit; //Conditions for Holy Spirit ability
    public (bool HasMP, bool IsReady) HolyCircle; //Conditions for Holy Circle ability
    public uint MP; //Current MP of the player
    public bool ShouldUseAOE; //Check if AOE rotation should be used
    public bool ShouldNormalHolyCircle; //Check if Holy Circle should be used
    public bool ShouldUseDMHolyCircle; //Check if Holy Circle should be used under Divine Might
    public bool ShouldHoldDMandAC; //Check if Divine Might buff and Atonement combo should be held into Fight or Flight
    public AID NextGCD; //The next action to be executed during the global cooldown (for cartridge management)
    public bool canWeaveIn; //Can weave in oGCDs
    public bool canWeaveEarly; //Can early weave oGCDs
    public bool canWeaveLate; //Can late weave oGCDs
    #endregion

    #region Module Helpers
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private AID ComboLastMove => (AID)World.Client.ComboState.Action; //Get the last action used in the combo sequence
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 2.99f; //Check if the target is within ST melee range (3 yalms)
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.99f; //Check if the target is within AOE melee range (5 yalms)
    private bool In25y(Actor? target) => Player.DistanceToHitbox(target) <= 24.99f; //Check if the target is within 25 yalms
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Check if this is the first GCD in combat
    private int TargetsHitByPlayerAOE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //Returns the number of targets hit by AOE within a 5-yalm radius around the player
    public bool HasEffect(SID sid) => SelfStatusLeft(sid) > 0; //Checks if Status effect is on self
    public float CombatTimer => (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds; //Calculates the elapsed time since combat started in seconds
    public Actor? TargetChoice(StrategyValues.OptionRef strategy) => ResolveTargetOverride(strategy.Value); //Resolves the target choice based on the strategy

    //TODO: add better targeting for Blades
    //public Actor? BestSplashTarget()
    #endregion

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        var gauge = World.Client.GetGauge<PaladinGauge>(); //Retrieve Paladin gauge
        Oath = gauge.OathGauge; //Retrieve current Oath gauge
        BladeComboStep = gauge.ConfiteorComboStep; //Retrieve current step in the Confiteor/Blades combo
        MP = Player.HPMP.CurMP; //Current MP of the player
        DivineMight.Left = SelfStatusLeft(SID.DivineMight, 30); //Remaining duration of the Divine Might buff
        DivineMight.IsActive = DivineMight.Left > 0; //Check if the Divine Might buff is active
        FightOrFlight.CD = CD(AID.FightOrFlight); //Remaining cooldown for the Fight or Flight ability
        FightOrFlight.Left = SelfStatusLeft(SID.FightOrFlight, 20); //Remaining duration of the Fight or Flight buff
        FightOrFlight.IsActive = FightOrFlight.CD is >= 39.5f and <= 60; //Check if the Fight or Flight buff is active 
        FightOrFlight.IsReady = FightOrFlight.CD < 0.6f; //Fight or Flight ability
        SpiritsWithin.CD = CD(BestSpirits); //Remaining cooldown for the Spirits Within ability
        SpiritsWithin.IsReady = Unlocked(AID.SpiritsWithin) && SpiritsWithin.CD < 0.6f; //Spirits Within ability
        CircleOfScorn.CD = CD(AID.CircleOfScorn); //Remaining cooldown for the Circle of Scorn ability
        CircleOfScorn.IsReady = Unlocked(AID.CircleOfScorn) && CircleOfScorn.CD < 0.6f; //Circle of Scorn ability
        GoringBlade.CD = CD(AID.GoringBlade); //Remaining cooldown for the Goring Blade ability
        GoringBlade.Left = SelfStatusLeft(SID.GoringBladeReady, 30); //Remaining duration of the Goring Blade buff
        GoringBlade.IsActive = GoringBlade.Left > 0; //Check if the Goring Blade buff is active
        GoringBlade.IsReady = Unlocked(AID.GoringBlade) && GoringBlade.IsActive; //Goring Blade ability
        Intervene.TotalCD = CD(AID.Intervene); //Total cooldown for the Intervene ability (60s)
        Intervene.HasCharges = Intervene.TotalCD <= 30f; //Check if the player has charges of Intervene
        Intervene.IsReady = Unlocked(AID.Intervene) && Intervene.HasCharges; //Intervene ability
        Requiescat.CD = CD(BestRequiescat); //Remaining cooldown for the Requiescat ability
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
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level); //Calculate GCD based on skill speed and haste
        PotionLeft = PotionStatusLeft(); //Remaining duration of the potion buff (typically 30s)
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget); //Estimate remaining and incoming raid buff durations
        NextGCD = AID.None; //Initialize next action as none
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        ShouldUseAOE = TargetsHitByPlayerAOE() > 2; //Check if AOE rotation should be used
        ShouldNormalHolyCircle = !DivineMight.IsActive && TargetsHitByPlayerAOE() > 3; //Check if Holy Circle should be used (very niche)
        ShouldUseDMHolyCircle = DivineMight.IsActive && TargetsHitByPlayerAOE() > 2; //Check if Holy Circle should be used under Divine Might
        ShouldHoldDMandAC = ComboLastMove is AID.RoyalAuthority ? FightOrFlight.CD < 5 : ComboLastMove is AID.FastBlade ? FightOrFlight.CD < 2.5 : ComboLastMove is AID.RiotBlade && FightOrFlight.CD < GCD;

        #region Strategy Options
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

        #region Standard Execution
        if (AOEStrategy == AOEStrategy.AutoBreakCombo) //if Break Combo option is selected
        {
            if (ShouldUseAOE) //if AOE rotation should be used
                QueueGCD(RotationAOE(), //queue the next AOE combo action
                    Player, //on Self (no target needed)
                    GCDPriority.Combo123); //use priority for 123/12 combo actions
            if (!ShouldUseAOE)
                QueueGCD(RotationST(), //queue the next single-target combo action
                    TargetChoice(AOE) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    GCDPriority.Combo123); //use priority for 123/12 combo actions
        }
        if (AOEStrategy == AOEStrategy.AutoFinishCombo) //if Finish Combo option is selected
        {
            QueueGCD(BestRotation(), //queue the next single-target combo action only if combo is finished
                TargetChoice(AOE) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                GCDPriority.Combo123); //use priority for 123/12 combo actions
        }
        #endregion

        #region Cooldowns Execution
        var hold = cdStrat == CooldownStrategy.Hold; //Check if Cooldowns should be held
        if (!hold) //if Cooldowns should not be held
        {
            if (ShouldUseFightOrFlight(fofStrat, primaryTarget)) //if Fight or Flight should be used
                QueueOGCD(AID.FightOrFlight, //queue Fight or Flight
                    Player, //on Self (no target needed)
                    fofStrat is OGCDStrategy.Force //if Force strategy is selected
                    or OGCDStrategy.AnyWeave //or Any Weave strategy is selected
                    or OGCDStrategy.EarlyWeave //or Early Weave strategy is selected
                    or OGCDStrategy.LateWeave //or Late Weave strategy is selected
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.FightOrFlight); //otherwise with normal priority
            if (ShouldUseRequiescat(reqStrat, primaryTarget)) //if Requiescat should be used
                QueueOGCD(BestRequiescat, //queue Requiescat
                    TargetChoice(req) ?? primaryTarget, //with target choice
                    reqStrat is OGCDStrategy.Force //if Force strategy is selected
                    or OGCDStrategy.AnyWeave //or Any Weave strategy is selected
                    or OGCDStrategy.EarlyWeave //or Early Weave strategy is selected
                    or OGCDStrategy.LateWeave //or Late Weave strategy is selected
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.Requiescat); //otherwise with normal priority
            if (bladeStrat == BladeComboStrategy.ForceConfiteor) //if Confiteor should be forced
                QueueGCD(AID.Confiteor, //queue Confiteor
                    TargetChoice(blade) ?? primaryTarget, //with target choice
                    GCDPriority.ForcedGCD); //use priority for forced GCDs
            if (ShouldUseCircleOfScorn(cosStrat, primaryTarget)) //if Circle of Scorn should be used
                QueueOGCD(AID.CircleOfScorn, //queue Circle of Scorn
                    Player, //on Self (no target needed)
                    cosStrat is OGCDStrategy.Force //if Force strategy is selected
                    or OGCDStrategy.AnyWeave //or Any Weave strategy is selected
                    or OGCDStrategy.EarlyWeave //or Early Weave strategy is selected
                    or OGCDStrategy.LateWeave //or Late Weave strategy is selected
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.CircleOfScorn); //otherwise with normal priority
            if (ShouldUseSpiritsWithin(swStrat, primaryTarget)) //if Spirits Within should be used
                QueueOGCD(BestSpirits, //queue Spirits Within
                    TargetChoice(sw) ?? primaryTarget, //with target choice
                    swStrat is OGCDStrategy.Force //if Force strategy is selected
                    or OGCDStrategy.AnyWeave //or Any Weave strategy is selected
                    or OGCDStrategy.EarlyWeave //or Early Weave strategy is selected
                    or OGCDStrategy.LateWeave //or Late Weave strategy is selected
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.SpiritsWithin); //otherwise with normal priority
            if (ShouldUseDash(dashStrat, primaryTarget)) //if Dash should be used
                QueueOGCD(AID.Intervene, //queue Dash
                    TargetChoice(dash) ?? primaryTarget, //with target choice
                    dashStrat is DashStrategy.Force //if Force strategy is selected
                    or DashStrategy.Force1 //or Force1 strategy is selected
                    or DashStrategy.GapClose //or GapClose strategy is selected
                    or DashStrategy.GapClose1 //or GapClose1 strategy is selected
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.Intervene); //otherwise with normal priority
            if (ShouldUseBladeOfHonor(bohStrat, primaryTarget)) //if Blade of Honor should be used
                QueueOGCD(AID.BladeOfHonor, //queue Blade of Honor
                    TargetChoice(boh) ?? primaryTarget, //with target choice
                    bohStrat is OGCDStrategy.Force //if Force strategy is selected
                    or OGCDStrategy.AnyWeave //or Any Weave strategy is selected
                    or OGCDStrategy.EarlyWeave //or Early Weave strategy is selected
                    or OGCDStrategy.LateWeave //or Late Weave strategy is selected
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.BladeOfHonor); //otherwise with normal priority
            if (ShouldUseGoringBlade(gbStrat, primaryTarget)) //if Goring Blade should be used
                QueueGCD(AID.GoringBlade, //queue Goring Blade
                    TargetChoice(gb) ?? primaryTarget, //with target choice
                    gbStrat is GCDStrategy.Force //if Force strategy is selected
                    ? GCDPriority.ForcedGCD //use priority for forced GCDs
                    : GCDPriority.GoringBlade); //otherwise with normal priority
        }
        if (ShouldUseBladeCombo(bladeStrat, primaryTarget)) //if Blade Combo should be used
        {
            if (bladeStrat is BladeComboStrategy.Automatic) //if Automatic strategy is selected
                QueueGCD(BestBlade, //queue the best Blade combo action
                    TargetChoice(blade) ?? primaryTarget, //with target choice
                    GCDPriority.Blades); //use priority for Blade combo actions
            if (bladeStrat is BladeComboStrategy.ForceFaith) //if Force Faith strategy is selected
                QueueGCD(AID.BladeOfFaith, //queue Blade of Faith
                    TargetChoice(blade) ?? primaryTarget, //with target choice
                    GCDPriority.ForcedGCD); //use priority for forced GCDs
            if (bladeStrat is BladeComboStrategy.ForceTruth) //if Force Truth strategy is selected
                QueueGCD(AID.BladeOfTruth, //queue Blade of Truth
                    TargetChoice(blade) ?? primaryTarget, //with target choice
                    GCDPriority.ForcedGCD); //use priority for forced GCDs
            if (bladeStrat is BladeComboStrategy.ForceValor) //if Force Valor strategy is selected
                QueueGCD(AID.BladeOfValor, //queue Blade of Valor
                    TargetChoice(blade) ?? primaryTarget, //with target choice
                    GCDPriority.ForcedGCD); //use priority for forced GCDs
        }
        if (ShouldUseAtonement(atoneStrat, primaryTarget)) //if Atonement should be used
        {
            if (atoneStrat == AtonementStrategy.Automatic) //if Automatic strategy is selected
                QueueGCD(BestAtonement, //queue the best Atonement action
                    TargetChoice(atone) ?? primaryTarget, //with target choice
                    GCDPriority.Atonement); //use priority for Atonement actions
            if (atoneStrat is AtonementStrategy.ForceAtonement) //if Force Atonement strategy is selected
                QueueGCD(AID.Atonement, //queue Atonement
                    TargetChoice(atone) ?? primaryTarget, //with target choice
                    GCDPriority.ForcedGCD); //use priority for forced GCDs
            if (atoneStrat is AtonementStrategy.ForceSupplication) //if Force Supplication strategy is selected
                QueueGCD(AID.Supplication, //queue Supplication
                    TargetChoice(atone) ?? primaryTarget, //with target choice
                    GCDPriority.ForcedGCD); //use priority for forced GCDs
            if (atoneStrat is AtonementStrategy.ForceSepulchre) //if Force Sepulchre strategy is selected
                QueueGCD(AID.Sepulchre, //queue Sepulchre
                    TargetChoice(atone) ?? primaryTarget, //with target choice
                    GCDPriority.ForcedGCD); //use priority for forced GCDs
        }
        if (ShouldUseHoly(holyStrat, primaryTarget)) //if Holy Spirit / Circle should be used
        {
            if (holyStrat == HolyStrategy.Automatic) //if Automatic strategy is selected
                QueueGCD(BestHoly, //queue the best Holy action
                    TargetChoice(holy) ?? primaryTarget, //with target choice
                    FightOrFlight.Left is <= 2.5f and >= 0.01f //if Fight or Flight is active
                    && !Supplication.IsActive //and Supplication is not active
                    && !Sepulchre.IsActive //and Sepulchre is not active
                    ? GCDPriority.GoringBlade //use priority for Goring Blade
                    : GCDPriority.HolySpirit); //otherwise use priority for Holy Spirit
            if (holyStrat == HolyStrategy.Spirit) //if Spirit strategy is selected
                QueueGCD(AID.HolySpirit, //queue Holy Spirit
                    TargetChoice(holy) ?? primaryTarget, //with target choice
                    GCDPriority.ForcedGCD); //use priority for forced GCDs
            if (holyStrat == HolyStrategy.Circle) //if Circle strategy is selected
                QueueGCD(BestHolyCircle, //queue Holy Circle
                    Player, //on Self (no target needed)
                    GCDPriority.ForcedGCD); //use priority
        }
        if (rangedStrat is RangedStrategy.Automatic && //if Automatic strategy is selected
            !In3y(TargetChoice(ranged) ?? primaryTarget)) //and target is not in melee range
            QueueGCD(isMoving ? AID.ShieldLob //queue Shield Lob if moving
                : AID.HolySpirit, //otherwise queue Holy Spirit
                TargetChoice(ranged) ?? primaryTarget, //with target choice
                GCDPriority.Combo123); //use priority for 123/12 combo actions
        if (ShouldUseRangedLob(primaryTarget, rangedStrat)) //if Shield Lob should be used
            QueueGCD(AID.ShieldLob, //queue Shield Lob
                TargetChoice(ranged) ?? primaryTarget, //with target choice
                rangedStrat is RangedStrategy.Force //if Force strategy is selected
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.Combo123); //otherwise use priority for 123/12 combo actions
        if (ShouldUseRangedCast(primaryTarget, rangedStrat)) //if Shield Cast should be used
            QueueGCD(AID.HolySpirit, //queue Holy Spirit
                TargetChoice(ranged) ?? primaryTarget, //with target choice
                rangedStrat is RangedStrategy.ForceCast //if Force Cast strategy is selected
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.Combo123); //otherwise use priority for 123/12 combo actions
        if (ShouldUsePotion(potStrat)) //if Potion should be used
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, //queue the potion action
                Player, //on Self (no target needed)
                ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, //use priority for potions
                0, //no delay
                GCD - 0.9f); //Lateweave
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
        AID.RoyalAuthority => ShouldUseAOE ? RotationAOE() : RotationST(), //If Royal Authority was last used, choose between AOE and ST rotations
        AID.RageOfHalone => ShouldUseAOE ? RotationAOE() : RotationST(), //If Rage of Halone was last used, choose between AOE and ST rotations
        AID.RiotBlade => RotationST(), //If Riot Blade was last used, continue ST rotation
        AID.FastBlade => RotationST(), //If Fast Blade was last used, continue ST rotation
        //AOE
        AID.Prominence => ShouldUseAOE ? RotationAOE() : RotationST(), //If Prominence was last used, choose between AOE and ST rotations
        AID.TotalEclipse => RotationAOE(), //If Total Eclipse was last used, continue AOE rotation
        _ => ShouldUseAOE ? RotationAOE() : RotationST(), //Default to AOE or ST rotation based on conditions
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
            Player.InCombat && //In combat
            target != null && //Target exists
            FightOrFlight.IsReady && //Fight or Flight is ready
            (CombatTimer <= 30 && ComboLastMove is AID.RoyalAuthority or AID.RageOfHalone || //Use within 30s of combat and after Royal Authority or Rage of Halone
             CombatTimer > 30), //Use after 30s of combat
        OGCDStrategy.Force => FightOrFlight.IsReady, //Force Fight or Flight
        OGCDStrategy.AnyWeave => FightOrFlight.IsReady && canWeaveIn, //Force Weave Fight or Flight
        OGCDStrategy.EarlyWeave => FightOrFlight.IsReady && canWeaveEarly, //Force Early Weave Fight or Flight
        OGCDStrategy.LateWeave => FightOrFlight.IsReady && canWeaveLate, //Force Late Weave Fight or Flight
        OGCDStrategy.Delay => false, //Delay Fight or Flight
        _ => false
    };
    private bool ShouldUseRequiescat(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            Requiescat.IsReady && //Requiescat is ready
            FightOrFlight.IsActive, //Fight or Flight is active
        OGCDStrategy.Force => Requiescat.IsReady, //Force Requiescat
        OGCDStrategy.AnyWeave => Requiescat.IsReady && canWeaveIn, //Force Weave Requiescat
        OGCDStrategy.EarlyWeave => Requiescat.IsReady && canWeaveEarly, //Force Early Weave Requiescat
        OGCDStrategy.LateWeave => Requiescat.IsReady && canWeaveLate, //Force Late Weave Requiescat
        OGCDStrategy.Delay => false, //Delay Requiescat
        _ => false
    };
    private bool ShouldUseSpiritsWithin(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In3y(target) && //Target in range
            FightOrFlight.CD is < 57.55f and > 17 && //One use inside FoF, one use outside FoF
            SpiritsWithin.IsReady, //Spirits Within is ready
        OGCDStrategy.Force => SpiritsWithin.IsReady, //Force Spirits Within
        OGCDStrategy.AnyWeave => SpiritsWithin.IsReady && canWeaveIn, //Force Weave Spirits Within
        OGCDStrategy.EarlyWeave => SpiritsWithin.IsReady && canWeaveEarly, //Force Early Weave Spirits Within
        OGCDStrategy.LateWeave => SpiritsWithin.IsReady && canWeaveLate, //Force Late Weave Spirits Within
        OGCDStrategy.Delay => false, //Delay Spirits Within
        _ => false
    };
    private bool ShouldUseCircleOfScorn(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            CircleOfScorn.IsReady && //Circle of Scorn is ready
            In5y(target) && //Target in range
            FightOrFlight.CD is < 57.55f and > 17, //One use inside FoF, one use outside FoF
        OGCDStrategy.Force => CircleOfScorn.IsReady, //Force Circle of Scorn
        OGCDStrategy.AnyWeave => CircleOfScorn.IsReady && canWeaveIn, //Force Weave Circle of Scorn
        OGCDStrategy.EarlyWeave => CircleOfScorn.IsReady && canWeaveEarly, //Force Early Weave Circle of Scorn
        OGCDStrategy.LateWeave => CircleOfScorn.IsReady && canWeaveLate, //Force Late Weave Circle of Scorn
        OGCDStrategy.Delay => false, //Delay Circle of Scorn
        _ => false
    };
    private bool ShouldUseBladeOfHonor(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            BladeOfHonor.IsReady, //Blade of Honor is ready
        OGCDStrategy.Force => BladeOfHonor.IsReady, //Force Blade of Honor
        OGCDStrategy.AnyWeave => BladeOfHonor.IsReady && canWeaveIn, //Force Weave Blade of Honor
        OGCDStrategy.EarlyWeave => BladeOfHonor.IsReady && canWeaveEarly, //Force Early Weave Blade of Honor
        OGCDStrategy.LateWeave => BladeOfHonor.IsReady && canWeaveLate, //Force Late Weave Blade of Honor
        OGCDStrategy.Delay => false, //Delay Blade of Honor
        _ => false
    };
    private bool ShouldUseGoringBlade(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic =>
            Player.InCombat && //In combat
            In3y(target) && //Target in range
            GoringBlade.IsReady && //Goring Blade is ready
            FightOrFlight.IsActive, //Fight or Flight is active
        GCDStrategy.Force => GoringBlade.IsReady, //Force Goring Blade
        GCDStrategy.Delay => false, //Delay Goring Blade
        _ => false
    };
    private bool ShouldUseBladeCombo(BladeComboStrategy strategy, Actor? target) => strategy switch
    {
        BladeComboStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In25y(target) && //Target in range
            Requiescat.IsActive && //Requiescat is active
            BladeComboStep is 0 or 1 or 2 or 3, //Blade Combo conditions are met
        BladeComboStrategy.ForceConfiteor => Confiteor.IsReady && BladeComboStep is 0, //Force Confiteor
        BladeComboStrategy.ForceFaith => BladeComboStep is 1, //Force Blade of Faith
        BladeComboStrategy.ForceTruth => BladeComboStep is 2, //Force Blade of Truth
        BladeComboStrategy.ForceValor => BladeComboStep is 3, //Force Blade of Valor
        BladeComboStrategy.Delay => false, //Delay Blade Combo
        _ => false
    };
    private bool ShouldUseAtonement(AtonementStrategy strategy, Actor? target) => strategy switch
    {
        AtonementStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In3y(target) && //Target in range
            !ShouldHoldDMandAC &&
            (Atonement.IsReady || Supplication.IsReady || Sepulchre.IsReady), //if any of the three are ready
        AtonementStrategy.ForceAtonement => Atonement.IsReady, //Force Atonement
        AtonementStrategy.ForceSupplication => Supplication.IsReady, //Force Supplication
        AtonementStrategy.ForceSepulchre => Sepulchre.IsReady, //Force Sepulchre
        AtonementStrategy.Delay => false, //Delay Atonement
        _ => false
    };
    private bool ShouldUseHoly(HolyStrategy strategy, Actor? target) => strategy switch
    {
        HolyStrategy.Automatic =>
            ShouldUseDMHolyCircle || ShouldNormalHolyCircle //if Holy Circle should be used
            ? ShouldUseHolyCircle(HolyStrategy.Automatic, target) //then use Holy Circle
            : ShouldUseHolySpirit(HolyStrategy.Automatic, target), //otherwise use Holy Spirit
        HolyStrategy.Spirit => HolySpirit.IsReady, //Force Holy Spirit
        HolyStrategy.Circle => HolyCircle.IsReady, //Force Holy Circle
        HolyStrategy.Delay => false, //Delay Holy Spirit
        _ => false
    };
    private bool ShouldUseHolySpirit(HolyStrategy strategy, Actor? target) => strategy switch
    {
        HolyStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In25y(target) && //Target in range
            HolySpirit.IsReady && //can execute Holy Spirit
            !ShouldHoldDMandAC &&
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
            Player.InCombat && //In combat
            target != null && //Target exists
            In3y(target) && //Target in range
            Intervene.IsReady && //can execute Intervene
            FightOrFlight.IsActive, //Fight or Flight is active
        DashStrategy.Force => true, //Force all charges
        DashStrategy.Force1 => Intervene.TotalCD < 1f, //Force 1 charge
        DashStrategy.GapClose => !In3y(target), //Force gap close
        DashStrategy.GapClose1 => Intervene.TotalCD < 1f && !In3y(target), //Force gap close only if at max charges
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => FightOrFlight.CD < 5, //Align potions with buffs
        PotionStrategy.Immediate => true, //Force potion immediately
        _ => false,
    };
    #endregion
}

