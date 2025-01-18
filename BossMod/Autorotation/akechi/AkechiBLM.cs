using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using AID = BossMod.BLM.AID;
using SID = BossMod.BLM.SID;
using TraitID = BossMod.BLM.TraitID;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiBLM(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track
    {
        AOE,                 //ST&AOE rotations tracking
        Movement,            //Movement strategy tracking
        Thunder,             //Thunder tracking
        Polyglot,            //Polyglot tracking
        Manafont,            //Manafont tracking
        Triplecast,          //Triplecast tracking
        LeyLines,            //Ley Lines tracking
        Potion,              //Potion item tracking
        TPUS,                //Transpose&UmbralSoul combo tracking
        Casting,              //Cast while Moving option tracking
        Transpose,           //Transpose tracking
        Amplifier,           //Amplifier tracking
        Retrace,             //Retrace tracking
        BTL,                 //Between the Lines tracking
    }
    public enum AOEStrategy
    {
        Auto,                //Automatically decide when to use ST or AOE rotation based on targets nearby
        ForceST,             //Force ST rotation only
        ForceAOE,            //Force AOE rotation only
    }
    public enum MovementStrategy
    {
        Allow,               //Allow the use of all abilities for movement, regardless of any setting or condition set by the user in other options
        OnlyGCDs,            //Only use instant cast GCDs for movement (Polyglots->Firestarter->Thunder->Scathe if nothing left), regardless of any setting or condition set by the user in other options
        OnlyOGCDs,           //Only use OGCDs for movement, (Swiftcast->Triplecast) regardless of any setting or condition set by the user in other options
        OnlyScathe,          //Only use Scathe for movement
        Forbid               //Forbid the use of any abilities for movement
    }
    public enum ThunderStrategy
    {
        Thunder3,            //Force use of Thunder if target has 3s or less remaining on DOT effect
        Thunder6,            //Force use of Thunder if target has 6s or less remaining on DOT effect
        Thunder9,            //Force use of Thunder if target has 9s or less remaining on DOT effect
        Thunder0,            //Force use of Thunder if target has does not have DOT effect
        Force,               //Force use of Thunder regardless of DOT effect
        Delay                //Delay the use of Thunder for manual or strategic usage
    }
    public enum PolyglotStrategy
    {
        AutoSpendAll,        //Spend all Polyglots as soon as possible
        AutoHold1,           //Spend 2 Polyglots; holds one for manual usage
        AutoHold2,           //Spend 1 Polyglot; holds two for manual usage
        AutoHold3,           //Holds all Polyglots for as long as possible
        XenoSpendAll,        //Use Xenoglossy as optimal spender, regardless of targets nearby; spends all Polyglots
        XenoHold1,           //Use Xenoglossy as optimal spender, regardless of targets nearby; holds one Polyglot for manual usage
        XenoHold2,           //Use Xenoglossy as optimal spender, regardless of targets nearby; holds two Polyglots for manual usage
        XenoHold3,           //Holds all Polyglots for as long as possible
        FoulSpendAll,        //Use Foul as optimal spender, regardless of targets nearby
        FoulHold1,           //Use Foul as optimal spender, regardless of targets nearby; holds one Polyglot for manual usage
        FoulHold2,           //Use Foul as optimal spender, regardless of targets nearby; holds two Polyglots for manual usage
        FoulHold3,           //Holds all Polyglots for as long as possible
        ForceXeno,           //Force use of Xenoglossy
        ForceFoul,           //Force use of Foul
        Delay                //Delay the use of Polyglot abilities for manual or strategic usage
    }
    public enum ManafontStrategy
    {
        Automatic,           //Automatically decide when to use Manafont
        Force,               //Force the use of Manafont (180s CD), regardless of weaving conditions
        ForceWeave,          //Force the use of Manafont (180s CD) in any next possible weave slot
        ForceEX,             //Force the use of Manafont (100s CD), regardless of weaving conditions
        ForceWeaveEX,        //Force the use of Manafont (100s CD) in any next possible weave slot
        Delay                //Delay the use of Manafont for strategic reasons
    }
    public enum TriplecastStrategy
    {
        Automatic,           //Automatically decide when to use Triplecast
        Force,               //Force the use of Triplecast; use all charges
        Force1,              //Force the use of Triplecast; holds one charge for manual usage
        ForceWeave,          //Force the use of Triplecast in any next possible weave slot
        ForceWeave1,         //Force the use of Triplecast in any next possible weave slot; holds one charge for manual usage
        Delay                //Delay the use of Triplecast
    }
    public enum LeyLinesStrategy
    {
        Automatic,           //Automatically decide when to use Ley Lines
        Force,               //Force the use of Ley Lines, regardless of weaving conditions
        Force1,              //Force the use of Ley Lines; holds one charge for manual usage
        ForceWeave,          //Force the use of Ley Lines in any next possible weave slot
        ForceWeave1,         //Force the use of Ley Lines in any next possible weave slot; holds one charge for manual usage
        Delay                //Delay the use of Ley Lines
    }
    public enum PotionStrategy
    {
        Manual,              //Manual potion usage
        AlignWithRaidBuffs,  //Align potion usage with raid buffs
        Immediate            //Use potions immediately when available
    }
    public enum TPUSStrategy
    {
        Allow,               //Allow Transpose & Umbral Soul combo whenever available
        OOConly,             //Only use Transpose & Umbral Soul combo when fully out of combat
        Forbid               //Forbid Transpose & Umbral Soul combo
    }
    public enum CastingOption
    {
        Allow,               //Allow casting while moving
        Forbid               //Forbid casting while moving
    }
    public enum OffensiveStrategy
    {
        Automatic,           //Automatically decide when to use off-global offensive abilities
        Force,               //Force the use of off-global offensive abilities, regardless of weaving conditions
        AnyWeave,            //Force the use of off-global offensive abilities in any next possible weave slot
        EarlyWeave,          //Force the use of off-global offensive abilities in very next FIRST weave slot only
        LateWeave,           //Force the use of off-global offensive abilities in very next LAST weave slot only
        Delay                //Delay the use of offensive abilities for strategic reasons
    }
    #endregion

    #region Module & Strategy Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi BLM", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)", //Category
            "Akechi", //Contributor
            RotationModuleQuality.Basic, //Quality
            BitMask.Build(Class.THM, Class.BLM), //Job
            100); //Level supported

        #region Custom strategies
        res.Define(Track.AOE).As<AOEStrategy>("AOE", "AOE", uiPriority: 200)
            .AddOption(AOEStrategy.Auto, "Auto", "Automatically decide when to use ST or AOE abilities")
            .AddOption(AOEStrategy.ForceST, "Force ST", "Force use of ST abilities only", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "Force AOE", "Force use of AOE abilities only", supportedTargets: ActionTargets.Hostile);
        res.Define(Track.Movement).As<MovementStrategy>("Movement", uiPriority: 195)
            .AddOption(MovementStrategy.Allow, "Allow", "Allow the use of all appropriate abilities for movement")
            .AddOption(MovementStrategy.OnlyGCDs, "OnlyGCDs", "Only use instant cast GCDs for movement; Polyglots->Firestarter->Thunder->Scathe if nothing left")
            .AddOption(MovementStrategy.OnlyOGCDs, "OnlyOGCDs", "Only use OGCDs for movement; Swiftcast->Triplecast")
            .AddOption(MovementStrategy.OnlyScathe, "OnlyScathe", "Only use Scathe for movement")
            .AddOption(MovementStrategy.Forbid, "Forbid", "Forbid the use of any abilities for movement");
        res.Define(Track.Thunder).As<ThunderStrategy>("Thunder", "DOT", uiPriority: 190)
            .AddOption(ThunderStrategy.Thunder3, "Thunder3", "Use Thunder if target has 3s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Thunder6, "Thunder6", "Use Thunder if target has 6s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Thunder9, "Thunder9", "Use Thunder if target has 9s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Thunder0, "Thunder0", "Use Thunder if target does not have DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Force, "Force", "Force use of Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Delay, "Delay", "Delay the use of Thunder for manual or strategic usage", 0, 0, ActionTargets.Hostile, 6)
            .AddAssociatedActions(AID.Thunder1, AID.Thunder2, AID.Thunder3, AID.Thunder4, AID.HighThunder, AID.HighThunder2);
        res.Define(Track.Polyglot).As<PolyglotStrategy>("Polyglot", "Polyglot", uiPriority: 180)
            .AddOption(PolyglotStrategy.AutoSpendAll, "AutoSpendAll", "Spend all Polyglots as soon as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold1, "AutoHold1", "Spend 2 Polyglots; holds one for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold2, "AutoHold2", "Spend 1 Polyglot; holds two for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold3, "AutoHold3", "Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.XenoSpendAll, "XenoSpendAll", "Use Xenoglossy as optimal spender, regardless of targets nearby; spends all Polyglots", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold1, "XenoHold1", "Use Xenoglossy as optimal spender, regardless of targets nearby; holds one Polyglot for manual usage", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold2, "XenoHold2", "Use Xenoglossy as optimal spender, regardless of targets nearby; holds two Polyglots for manual usage", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold3, "XenoHold3", "Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.FoulSpendAll, "FoulSpendAll", "Use Foul as optimal spender, regardless of targets nearby", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold1, "FoulHold1", "Use Foul as optimal spender, regardless of targets nearby; holds one Polyglot for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold2, "FoulHold2", "Use Foul as optimal spender, regardless of targets nearby; holds two Polyglots for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold3, "FoulHold3", "Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.ForceXeno, "Force Xenoglossy", "Force use of Xenoglossy", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.ForceFoul, "Force Foul", "Force use of Foul", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.Delay, "Delay", "Delay the use of Polyglot abilities for manual or strategic usage", 0, 0, ActionTargets.Hostile, 70)
            .AddAssociatedActions(AID.Xenoglossy, AID.Foul);
        res.Define(Track.Manafont).As<ManafontStrategy>("Manafont", "M.font", uiPriority: 165)
            .AddOption(ManafontStrategy.Automatic, "Auto", "Automatically decide when to use Manafont", 0, 0, ActionTargets.Self, 30)
            .AddOption(ManafontStrategy.Force, "Force", "Force the use of Manafont (180s CD), regardless of weaving conditions", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(ManafontStrategy.ForceWeave, "ForceWeave", "Force the use of Manafont (180s CD) in any next possible weave slot", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(ManafontStrategy.ForceEX, "ForceEX", "Force the use of Manafont (100s CD), regardless of weaving conditions", 100, 0, ActionTargets.Self, 84)
            .AddOption(ManafontStrategy.ForceWeaveEX, "ForceWeaveEX", "Force the use of Manafont (100s CD) in any next possible weave slot", 100, 0, ActionTargets.Self, 84)
            .AddOption(ManafontStrategy.Delay, "Delay", "Delay the use of Manafont for strategic reasons", 0, 0, ActionTargets.Self, 30)
            .AddAssociatedActions(AID.Manafont);
        res.Define(Track.Triplecast).As<TriplecastStrategy>("T.cast", uiPriority: 170)
            .AddOption(TriplecastStrategy.Automatic, "Auto", "Use any charges available during Ley Lines window or every 2 minutes (NOTE: does not take into account charge overcap, will wait for 2 minute windows to spend both)", 0, 0, ActionTargets.Self, 66)
            .AddOption(TriplecastStrategy.Force, "Force", "Force the use of Triplecast; uses all charges", 60, 0, ActionTargets.Self, 66)
            .AddOption(TriplecastStrategy.Force1, "Force1", "Force the use of Triplecast; holds one charge for manual usage", 60, 0, ActionTargets.Self, 66)
            .AddOption(TriplecastStrategy.ForceWeave, "ForceWeave", "Force the use of Triplecast in any next possible weave slot", 60, 0, ActionTargets.Self, 66)
            .AddOption(TriplecastStrategy.ForceWeave1, "ForceWeave1", "Force the use of Triplecast in any next possible weave slot; holds one charge for manual usage", 60, 0, ActionTargets.Self, 66)
            .AddOption(TriplecastStrategy.Delay, "Delay", "Delay the use of Triplecast", 60, 0, ActionTargets.Self, 66)
            .AddAssociatedActions(AID.Triplecast);
        res.Define(Track.LeyLines).As<LeyLinesStrategy>("L.Lines", uiPriority: 170)
            .AddOption(LeyLinesStrategy.Automatic, "Auto", "Automatically decide when to use Ley Lines", 0, 0, ActionTargets.Self, 52)
            .AddOption(LeyLinesStrategy.Force, "Force", "Force the use of Ley Lines, regardless of weaving conditions", 120, 0, ActionTargets.Self, 52)
            .AddOption(LeyLinesStrategy.Force1, "Force1", "Force the use of Ley Lines; holds one charge for manual usage", 120, 0, ActionTargets.Self, 52)
            .AddOption(LeyLinesStrategy.ForceWeave, "ForceWeave", "Force the use of Ley Lines in any next possible weave slot", 120, 0, ActionTargets.Self, 52)
            .AddOption(LeyLinesStrategy.ForceWeave1, "ForceWeave1", "Force the use of Ley Lines in any next possible weave slot; holds one charge for manual usage", 120, 0, ActionTargets.Self, 52)
            .AddOption(LeyLinesStrategy.Delay, "Delay", "Delay the use of Ley Lines", 120, 0, ActionTargets.Self, 52)
            .AddAssociatedActions(AID.LeyLines);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 160)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with buffs (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionInt);
        res.Define(Track.TPUS).As<TPUSStrategy>("Transpose & Umbral Soul", "TP/US", uiPriority: 160)
            .AddOption(TPUSStrategy.Allow, "Allow", "Allow Transpose & Umbral Soul combo when out of combat or no targetable enemy is nearby", 0, 0, ActionTargets.Self, 35)
            .AddOption(TPUSStrategy.OOConly, "OOConly", "Only use Transpose & Umbral Soul combo when fully out of combat", 0, 0, ActionTargets.Self, 35)
            .AddOption(TPUSStrategy.Forbid, "Forbid", "Forbid Transpose & Umbral Soul combo", 0, 0, ActionTargets.Self, 35)
            .AddAssociatedActions(AID.Transpose, AID.UmbralSoul);
        res.Define(Track.Casting).As<CastingOption>("Casting", uiPriority: 155)
            .AddOption(CastingOption.Allow, "Allow", "Allow casting while Casting")
            .AddOption(CastingOption.Forbid, "Forbid", "Forbid casting while moving");
        #endregion

        #region Offensive Strategies
        res.Define(Track.Transpose).As<OffensiveStrategy>("Transpose", uiPriority: 125)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Automatically decide when to use Transpose", 0, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Force, "Force", "Force the use of Transpose, regardless of weaving conditions", 5, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.AnyWeave, "AnyWeave", "Force the use of Transpose in any next possible weave slot", 5, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.EarlyWeave, "EarlyWeave", "Force the use of Transpose in very next FIRST weave slot only", 5, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.LateWeave, "LateWeave", "Force the use of Transpose in very next LAST weave slot only", 0, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay the use of Transpose", 0, 0, ActionTargets.Self, 4)
            .AddAssociatedActions(AID.Transpose);
        res.Define(Track.Amplifier).As<OffensiveStrategy>("Amplifier", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Automatically decide when to use Amplifier", 0, 0, ActionTargets.Self, 86)
            .AddOption(OffensiveStrategy.Force, "Force", "Force the use of Amplifier, regardless of weaving conditions", 120, 0, ActionTargets.Self, 86)
            .AddOption(OffensiveStrategy.AnyWeave, "AnyWeave", "Force the use of Amplifier in any next possible weave slot", 120, 0, ActionTargets.Self, 86)
            .AddOption(OffensiveStrategy.EarlyWeave, "EarlyWeave", "Force the use of Amplifier in very next FIRST weave slot only", 120, 0, ActionTargets.Self, 86)
            .AddOption(OffensiveStrategy.LateWeave, "LateWeave", "Force the use of Amplifier in very next LAST weave slot only", 120, 0, ActionTargets.Self, 86)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay the use of Amplifier", 0, 0, ActionTargets.Self, 86)
            .AddAssociatedActions(AID.Amplifier);
        res.Define(Track.Retrace).As<OffensiveStrategy>("Retrace", uiPriority: 155)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Automatically decide when to use Retrace", 0, 0, ActionTargets.Self, 96)
            .AddOption(OffensiveStrategy.Force, "Force", "Force the use of Retrace, regardless of weaving conditions", 40, 0, ActionTargets.Self, 96)
            .AddOption(OffensiveStrategy.AnyWeave, "AnyWeave", "Force the use of Retrace in any next possible weave slot", 40, 0, ActionTargets.Self, 96)
            .AddOption(OffensiveStrategy.EarlyWeave, "EarlyWeave", "Force the use of Retrace in very next FIRST weave slot only", 40, 0, ActionTargets.Self, 96)
            .AddOption(OffensiveStrategy.LateWeave, "LateWeave", "Force the use of Retrace in very next LAST weave slot only", 40, 0, ActionTargets.Self, 96)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay the use of Retrace", 0, 0, ActionTargets.Self, 96)
            .AddAssociatedActions(AID.Retrace);
        res.Define(Track.BTL).As<OffensiveStrategy>("Between The Lines", "BTL", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Automatically decide when to use Between The Lines", 0, 0, ActionTargets.Self, 62)
            .AddOption(OffensiveStrategy.Force, "Force", "Force the use of Between The Lines, regardless of weaving conditions", 3, 0, ActionTargets.Self, 62)
            .AddOption(OffensiveStrategy.AnyWeave, "AnyWeave", "Force the use of Between The Lines in any next possible weave slot", 3, 0, ActionTargets.Self, 62)
            .AddOption(OffensiveStrategy.EarlyWeave, "EarlyWeave", "Force the use of Between The Lines in very next FIRST weave slot only", 3, 0, ActionTargets.Self, 62)
            .AddOption(OffensiveStrategy.LateWeave, "LateWeave", "Force the use of Between The Lines in very next LAST weave slot only", 3, 0, ActionTargets.Self, 62)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay the use of Between The Lines", 0, 0, ActionTargets.Self, 62)
            .AddAssociatedActions(AID.BetweenTheLines);

        #endregion

        return res;
    }
    #endregion

    #region Priorities
    //TODO: Fix this shit later, looks crazy
    public enum GCDPriority //priorities for GCDs (higher number = higher priority)
    {
        None = 0,             //default
        Step1 = 100,          //Step 1
        Step2 = 110,          //Step 2
        Step3 = 120,          //Step 3
        Step4 = 130,          //Step 4
        Step5 = 140,          //Step 5
        Step6 = 150,          //Step 6
        Step7 = 160,          //Step 7
        Step8 = 170,          //Step 8
        Step9 = 180,          //Step 9
        Step10 = 190,         //Step 10
        Standard = 300,       //standard abilities
        DOT = 350,            //damage-over-time abilities
        FlareStar = 375,      //Flare Star
        Despair = 400,        //Despair
        F3P = 450,            //Fire III proc
        NeedB3 = 460,         //Need to use Blizzard III
        Polyglot = 475,       //Polyglots
        Paradox = 500,        //Paradox
        NeedDOT = 600,        //Need to apply DOTs
        NeedF3P = 625,        //Need to use Fire III proc
        NeedDespair = 640,    //Need to use Despair
        NeedPolyglot = 650,   //Need to use Polyglots
        Moving3 = 700,        //Moving (3rd priority)
        Moving2 = 710,        //Moving (2nd priority)
        Moving1 = 720,        //Moving (1st priority)
        ForcedGCD = 900,      //Forced GCDs
        BlockAll = 2000,      //Block all GCDs
    }
    public enum OGCDPriority //priorities for oGCDs (higher number = higher priority)
    {
        None = 0,             //default
        Transpose = 400,      //Transpose
        Manafont = 450,       //Manafont
        LeyLines = 500,       //Ley Lines
        Amplifier = 550,      //Amplifier
        Triplecast = 600,     //Triplecast
        Potion = 800,         //Potion
        ForcedOGCD = 900,     //Forced oGCDs
    }
    #endregion

    #region Upgrade Paths
    private AID BestThunderST
        => Unlocked(AID.HighThunder) ? AID.HighThunder
        : Unlocked(AID.Thunder3) ? AID.Thunder3
        : AID.Thunder1;
    private AID BestThunderAOE
        => Unlocked(AID.HighThunder2) ? AID.HighThunder2
        : Unlocked(AID.Thunder4) ? AID.Thunder4
        : AID.Thunder2;
    private AID BestThunder
        => ShouldUseAOE ? BestThunderAOE : BestThunderST;
    private AID BestPolyglot
        => ShouldUseAOE ? AID.Foul : BestXenoglossy;
    private AID BestXenoglossy
        => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;
    #endregion

    #region Placeholders for Variables
    private uint MP; //Current MP
    private bool NoStance; //No stance
    private bool InAstralFire; //In Astral Fire
    private bool InUmbralIce; //In Umbral Ice
    private sbyte ElementStance; //Elemental Stance
    private byte Polyglots; //Polyglot Stacks
    private int MaxPolyglots; // 
    private byte UmbralHearts; //Umbral Hearts
    private int MaxUmbralHearts; //Max Umbral Hearts
    private int UmbralStacks; //Umbral Ice Stacks
    private int AstralStacks; //Astral Fire Stacks
    private int AstralSoulStacks; //Stacks for Flare Star (Lv100)
    private int EnochianTimer; //Enochian timer
    private float ElementTimer; //Time remaining on Enochian
    private bool ParadoxActive; //Paradox is active
    private bool canFoul; //Can use Foul
    private bool canXeno; //Can use Xenoglossy
    private bool canParadox; //Can use Paradox
    private bool canLL; //Can use Ley Lines
    private bool canAmp; //Can use Amplifier
    private bool canTC; //Can use Triplecast
    private bool canMF; //Can use Manafont
    private bool canRetrace; //Can use Retrace
    private bool canBTL; //Can use Between the Lines
    private bool hasThunderhead; //Has Thunderhead buff
    private float ThunderLeft; //Time left on DOT effect (30s base)
    public bool canWeaveIn; //Can weave in oGCDs
    public bool canWeaveEarly; //Can early weave oGCDs
    public bool canWeaveLate; //Can late weave oGCDs
    public float SpS; //Current GCD length, adjusted by spell speed/haste (2.5s baseline)
    public AID NextGCD; //Next global cooldown action to be used
    #endregion

    #region Module Helpers
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    private bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid); //Check if the desired trait is unlocked
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private bool In25y(Actor? target) => Player.DistanceToHitbox(target) <= 24.99f; //Check if the target is within 25 yalms
    private bool ActionReady(AID aid) => Unlocked(aid) && CD(aid) < 0.6f; //Check if the desired action is unlocked and is ready (cooldown less than 0.6 seconds)
    private bool PlayerHasEffect(SID sid, float duration) => SelfStatusLeft(sid, duration) > GCD; //Checks if Status effect is on self
    public float GetActualCastTime(AID aid) => ActionDefinitions.Instance.Spell(aid)!.CastTime * SpS / 2.5f;
    public float GetCastTime(AID aid)
    {
        var aspect = ActionDefinitions.Instance.Spell(aid)!.Aspect;
        var castTime = GetActualCastTime(aid);
        if (PlayerHasEffect(SID.Triplecast, 15f) || PlayerHasEffect(SID.Swiftcast, 10f))
            return 0f;
        if (aid == AID.Fire3 && PlayerHasEffect(SID.Firestarter, 30f)
            || aid == AID.Foul && Unlocked(TraitID.EnhancedFoul)
            || aspect == ActionAspect.Thunder && PlayerHasEffect(SID.Thunderhead, 30f)
            || aid == AID.Despair && Unlocked(TraitID.EnhancedAstralFire))
            return 0;
        if (castTime == 0)
            return 0;
        if (ElementStance == -3 && aspect == ActionAspect.Fire ||
            ElementStance == 3 && aspect == ActionAspect.Ice)
            castTime *= 0.5f;
        return castTime;
    }
    private bool JustUsed(AID aid, float variance)
    {
        var used = Manager.LastCast.Data?.IsSpell(aid) == true;
        var within = (World.CurrentTime - Manager.LastCast.Time).TotalSeconds <= variance;
        return used && within;
    }

    #region Targeting
    private int TargetsInRange() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 25); //Returns the number of targets hit by AOE within a 25-yalm radius around the player
    private Actor? TargetChoice(StrategyValues.OptionRef strategy) => ResolveTargetOverride(strategy.Value); //Resolves the target choice based on the strategy
    private Actor? FindBestSplashTarget()
    {
        float splashPriorityFunc(Actor actor)
        {
            var distanceToPlayer = actor.DistanceToHitbox(Player);
            if (distanceToPlayer <= 24f)
            {
                var targetsInSplashRadius = 0;
                foreach (var enemy in Hints.PriorityTargets)
                {
                    var targetActor = enemy.Actor;
                    if (targetActor != actor && targetActor.Position.InCircle(actor.Position, 5f))
                    {
                        targetsInSplashRadius++;
                    }
                }
                return targetsInSplashRadius;
            }
            return float.MinValue;
        }

        var (bestTarget, bestPrio) = FindBetterTargetBy(null, 25f, splashPriorityFunc);

        return bestTarget;
    }
    private Actor? BestAOETarget => FindBestSplashTarget(); // Find the best target for splash attack
    private bool ShouldUseAOE
    {
        get
        {
            // Check if there's a valid target for the AoE attack
            var bestTarget = BestAOETarget;

            // If there is a best target and it has a significant number of other targets in its splash radius, we can use AoE
            if (bestTarget != null)
            {
                // We can define a threshold to require a minimum number of targets within the splash radius to make AoE worthwhile
                var minimumTargetsForAOE = 2; // Example: At least 2 other enemies within the 5-yard splash radius
                float splashPriorityFunc(Actor actor)
                {
                    var distanceToPlayer = actor.DistanceToHitbox(Player);
                    if (distanceToPlayer <= 24f)
                    {
                        var targetsInSplashRadius = 0;
                        foreach (var enemy in Hints.PriorityTargets)
                        {
                            var targetActor = enemy.Actor;
                            if (targetActor != actor && targetActor.Position.InCircle(actor.Position, 5f))
                            {
                                targetsInSplashRadius++;
                            }
                        }
                        return targetsInSplashRadius;
                    }
                    return float.MinValue;
                }

                var (_, bestPrio) = FindBetterTargetBy(null, 25f, splashPriorityFunc);

                return bestPrio >= minimumTargetsForAOE;
            }

            return false;
        }
    }
    #endregion

    #endregion

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        var gauge = World.Client.GetGauge<BlackMageGauge>(); //Retrieve BLM gauge
        NoStance = ElementStance is 0; //No stance
        ElementStance = gauge.ElementStance; //Elemental Stance
        InAstralFire = ElementStance is 1 or 2 or 3; //In Astral Fire
        InUmbralIce = ElementStance is -1 or -2 or -3; //In Umbral Ice
        Polyglots = gauge.PolyglotStacks; //Polyglot Stacks
        UmbralHearts = gauge.UmbralHearts; //Umbral Hearts
        MaxUmbralHearts = Unlocked(TraitID.UmbralHeart) ? 3 : 0;
        UmbralStacks = gauge.UmbralStacks; //Umbral Ice Stacks
        AstralStacks = gauge.AstralStacks; //Astral Fire Stacks
        AstralSoulStacks = gauge.AstralSoulStacks; //Stacks for Flare Star (Lv100)
        ParadoxActive = gauge.ParadoxActive; //Paradox is active
        EnochianTimer = gauge.EnochianTimer; //Enochian timer
        ElementTimer = gauge.ElementTimeRemaining / 1000f; //Time remaining on current element
        MaxPolyglots = Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : 1;
        canFoul = Unlocked(AID.Foul) && Polyglots > 0; //Can use Foul
        canXeno = Unlocked(AID.Xenoglossy) && Polyglots > 0; //Can use Xenoglossy
        canParadox = Unlocked(AID.Paradox) && ParadoxActive; //Can use Paradox
        canLL = Unlocked(AID.LeyLines) && CD(AID.LeyLines) <= 120 && SelfStatusLeft(SID.LeyLines, 30) == 0; //Can use Ley Lines
        canAmp = ActionReady(AID.Amplifier); //Can use Amplifier
        canTC = Unlocked(AID.Triplecast) && CD(AID.Triplecast) <= 60 && SelfStatusLeft(SID.Triplecast) == 0; //Can use Triplecast
        canMF = ActionReady(AID.Manafont); //Can use Manafont
        canRetrace = ActionReady(AID.Retrace) && PlayerHasEffect(SID.LeyLines, 30); //Can use Retrace
        canBTL = ActionReady(AID.BetweenTheLines) && PlayerHasEffect(SID.LeyLines, 30); //Can use Between the Lines
        hasThunderhead = PlayerHasEffect(SID.Thunderhead, 30); //Has Thunderhead buff
        ThunderLeft = Utils.MaxAll( //Time left on DOT effect
            StatusDetails(primaryTarget, SID.Thunder, Player.InstanceID, 24).Left,
            StatusDetails(primaryTarget, SID.ThunderII, Player.InstanceID, 18).Left,
            StatusDetails(primaryTarget, SID.ThunderIII, Player.InstanceID, 27).Left,
            StatusDetails(primaryTarget, SID.ThunderIV, Player.InstanceID, 21).Left,
            StatusDetails(primaryTarget, SID.HighThunder, Player.InstanceID, 30).Left,
            StatusDetails(primaryTarget, SID.HighThunderII, Player.InstanceID, 24).Left);
        MP = Player.HPMP.CurMP; //Current MP
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        SpS = ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on spell speed and haste
        NextGCD = AID.None; //Next global cooldown action to be used

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var movementStrat = strategy.Option(Track.Movement).As<MovementStrategy>();
        var thunder = strategy.Option(Track.Thunder); //Thunder track
        var thunderStrat = thunder.As<ThunderStrategy>(); //Thunder strategy
        var polyglot = strategy.Option(Track.Polyglot); //Polyglot track
        var polyglotStrat = polyglot.As<PolyglotStrategy>(); //Polyglot strategy
        var mf = strategy.Option(Track.Manafont); //Manafont track
        var mfStrat = mf.As<ManafontStrategy>(); //Manafont strategy
        var tc = strategy.Option(Track.Triplecast); //Triplecast track
        var tcStrat = tc.As<TriplecastStrategy>(); //Triplecast strategy
        var ll = strategy.Option(Track.LeyLines); //Ley Lines track
        var llStrat = ll.As<LeyLinesStrategy>(); //Ley Lines strategy
        var amp = strategy.Option(Track.Amplifier); //Amplifier track
        var ampStrat = amp.As<OffensiveStrategy>(); //Amplifier strategy
        var retrace = strategy.Option(Track.Retrace); //Retrace track
        var retraceStrat = retrace.As<OffensiveStrategy>(); //Retrace strategy
        var btl = strategy.Option(Track.BTL); //Between the Lines track
        var btlStrat = btl.As<OffensiveStrategy>(); //Between the Lines strategy
        var potionStrat = strategy.Option(Track.Potion).As<PotionStrategy>(); //Potion strategy
        var tpusStrat = strategy.Option(Track.TPUS).As<TPUSStrategy>(); //Transpose/Umbral Soul strategy
        var movingOption = strategy.Option(Track.Casting).As<CastingOption>(); //Casting while moving strategy
        #endregion

        #endregion

        #region Rotation Execution

        #region ST / AOE
        if (movingOption is CastingOption.Allow ||
            movingOption is CastingOption.Forbid &&
            (!isMoving || //if not moving
            (PlayerHasEffect(SID.Swiftcast, 10) || //or has Swiftcast
            PlayerHasEffect(SID.Triplecast, 15) || //or has Triplecast
            (canParadox && ElementTimer < (SpS * 3) && MP >= 1600 || canParadox && JustUsed(AID.Blizzard4, 5)) || //or can use Paradox
            SelfStatusLeft(SID.Firestarter, 30) is < 25 and not 0 || //or can use F3P
            (Unlocked(TraitID.EnhancedAstralFire) && MP is < 1600 and not 0)))) //instant cast Despair 
        {
            if (AOEStrategy is AOEStrategy.Auto)
                BestRotation(TargetChoice(AOE) ?? BestAOETarget ?? primaryTarget);
            if (AOEStrategy is AOEStrategy.ForceST)
                BestST(TargetChoice(AOE) ?? primaryTarget);
            if (AOEStrategy is AOEStrategy.ForceAOE)
                BestAOE(TargetChoice(AOE) ?? BestAOETarget ?? primaryTarget);
        }
        #endregion

        #region Movement
        if (Player.InCombat &&
            primaryTarget != null &&
            isMoving)
        {
            if (movementStrat is MovementStrategy.Allow)
            {
                //GCDs
                if (!PlayerHasEffect(SID.Swiftcast, 10) ||
                    !PlayerHasEffect(SID.Triplecast, 15))
                    QueueGCD(
                        Unlocked(TraitID.EnhancedPolyglot) && Polyglots > 0 ? BestPolyglot
                        : PlayerHasEffect(SID.Firestarter, 30) ? AID.Fire3
                        : hasThunderhead ? BestThunder
                        : AID.Scathe,
                        Polyglots > 0 ? TargetChoice(polyglot) ?? BestAOETarget ?? primaryTarget
                        : PlayerHasEffect(SID.Firestarter, 30) ? TargetChoice(AOE) ?? primaryTarget
                        : hasThunderhead ? TargetChoice(thunder) ?? BestAOETarget ?? primaryTarget
                        : primaryTarget,
                        GCDPriority.Moving1);
                //OGCDs
                if (ActionReady(AID.Swiftcast) &&
                    !PlayerHasEffect(SID.Triplecast, 15))
                    QueueOGCD(AID.Swiftcast, Player, GCDPriority.Moving2);
                if (Unlocked(AID.Triplecast) &&
                    CD(AID.Triplecast) <= 60 &&
                    !PlayerHasEffect(SID.Triplecast, 15) &&
                    !PlayerHasEffect(SID.Swiftcast, 10))
                    QueueOGCD(AID.Triplecast, Player, GCDPriority.Moving3);
            }
            if (movementStrat is MovementStrategy.OnlyGCDs)
            {
                //GCDs
                if (!PlayerHasEffect(SID.Swiftcast, 10) ||
                    !PlayerHasEffect(SID.Triplecast, 15))
                    QueueGCD(
                        Unlocked(TraitID.EnhancedPolyglot) && Polyglots > 0 ? BestPolyglot
                        : PlayerHasEffect(SID.Firestarter, 30) ? AID.Fire3
                        : hasThunderhead ? BestThunder
                        : AID.Scathe,
                        Polyglots > 0 ? TargetChoice(polyglot) ?? BestAOETarget ?? primaryTarget
                        : PlayerHasEffect(SID.Firestarter, 30) ? TargetChoice(AOE) ?? primaryTarget
                        : hasThunderhead ? TargetChoice(thunder) ?? BestAOETarget ?? primaryTarget
                        : primaryTarget,
                        GCDPriority.Moving1);
            }
            if (movementStrat is MovementStrategy.OnlyOGCDs)
            {
                //OGCDs
                if (ActionReady(AID.Swiftcast) &&
                    !PlayerHasEffect(SID.Triplecast, 15))
                    QueueOGCD(AID.Swiftcast, Player, GCDPriority.Moving2);
                if (canTC &&
                    !PlayerHasEffect(SID.Swiftcast, 10))
                    QueueOGCD(AID.Triplecast, Player, GCDPriority.Moving3);
            }
            if (movementStrat is MovementStrategy.OnlyScathe)
            {
                if (MP >= 800)
                    QueueGCD(AID.Scathe, primaryTarget, GCDPriority.Moving1);
            }
        }
        #endregion

        #region Out of combat
        if (tpusStrat != TPUSStrategy.Forbid)
        {
            if (Unlocked(AID.Transpose))
            {
                if (!Unlocked(AID.UmbralSoul))
                {
                    if (primaryTarget == null &&
                        (tpusStrat == TPUSStrategy.Allow && (!Player.InCombat || Player.InCombat && TargetsInRange() is 0)) ||
                        (tpusStrat == TPUSStrategy.OOConly && !Player.InCombat))
                    {
                        if (CD(AID.Transpose) < 0.6f &&
                            (InAstralFire || InUmbralIce))
                            QueueOGCD(AID.Transpose, Player, OGCDPriority.Transpose);
                    }
                }
                if (Unlocked(AID.UmbralSoul))
                {
                    if (primaryTarget == null &&
                        (tpusStrat == TPUSStrategy.Allow && (!Player.InCombat || Player.InCombat && TargetsInRange() is 0)) ||
                        (tpusStrat == TPUSStrategy.OOConly && !Player.InCombat))
                    {
                        if (InAstralFire)
                            QueueOGCD(AID.Transpose, Player, OGCDPriority.Transpose);
                        if (InUmbralIce &&
                            (ElementTimer <= 14 || UmbralStacks < 3 || UmbralHearts != MaxUmbralHearts))
                            QueueGCD(AID.UmbralSoul, Player, GCDPriority.Standard);
                    }
                }
            }
        }
        #endregion

        //Thunder
        if (ShouldUseThunder(primaryTarget, thunderStrat)) //if Thunder should be used based on strategy
        {
            if (AOEStrategy is AOEStrategy.Auto)
                QueueGCD(BestThunder,
                    TargetChoice(thunder) ?? BestAOETarget ?? primaryTarget,
                    ThunderLeft < 3 ? GCDPriority.NeedDOT :
                    GCDPriority.DOT);
            if (AOEStrategy is AOEStrategy.ForceST)
                QueueGCD(BestThunderST,
                    TargetChoice(thunder) ?? primaryTarget,
                    ThunderLeft < 3 ? GCDPriority.NeedDOT :
                    GCDPriority.DOT);
            if (AOEStrategy is AOEStrategy.ForceAOE)
                QueueGCD(BestThunderAOE,
                    TargetChoice(thunder) ?? BestAOETarget ?? primaryTarget,
                    ThunderLeft < 3 ? GCDPriority.NeedDOT :
                    GCDPriority.DOT);
        }
        //Polyglots
        if (ShouldUsePolyglot(primaryTarget, polyglotStrat)) //if Polyglot should be used based on strategy
        {
            if (polyglotStrat is PolyglotStrategy.AutoSpendAll
                or PolyglotStrategy.AutoHold1
                or PolyglotStrategy.AutoHold2
                or PolyglotStrategy.AutoHold3)
                QueueGCD(BestPolyglot,
                    TargetChoice(polyglot) ?? BestAOETarget ?? primaryTarget,
                    polyglotStrat is PolyglotStrategy.ForceXeno ? GCDPriority.ForcedGCD
                    : Polyglots == MaxPolyglots && EnochianTimer < 5000 ? GCDPriority.NeedPolyglot
                    : GCDPriority.Paradox);
            if (polyglotStrat is PolyglotStrategy.XenoSpendAll
                or PolyglotStrategy.XenoHold1
                or PolyglotStrategy.XenoHold2
                or PolyglotStrategy.XenoHold3)
                QueueGCD(BestXenoglossy,
                    TargetChoice(polyglot) ?? primaryTarget,
                    polyglotStrat is PolyglotStrategy.ForceXeno ? GCDPriority.ForcedGCD
                    : Polyglots == MaxPolyglots && EnochianTimer < 5000 ? GCDPriority.NeedPolyglot
                    : GCDPriority.Paradox);
            if (polyglotStrat is PolyglotStrategy.FoulSpendAll
                or PolyglotStrategy.FoulHold1
                or PolyglotStrategy.FoulHold2
                or PolyglotStrategy.FoulHold3)
                QueueGCD(AID.Foul, TargetChoice(polyglot) ?? BestAOETarget ?? primaryTarget, polyglotStrat is PolyglotStrategy.ForceFoul ? GCDPriority.ForcedGCD : Polyglots == MaxPolyglots && EnochianTimer < 5000 ? GCDPriority.NeedPolyglot : GCDPriority.Paradox); //Queue Foul
        }
        //LeyLines
        if (ShouldUseLeyLines(primaryTarget, llStrat))
            QueueOGCD(AID.LeyLines,
                Player,
                llStrat is LeyLinesStrategy.Force
                or LeyLinesStrategy.Force1
                or LeyLinesStrategy.ForceWeave1
                or LeyLinesStrategy.ForceWeave1
                ? OGCDPriority.ForcedOGCD
                : OGCDPriority.LeyLines);
        //Triplecast
        if (ShouldUseTriplecast(primaryTarget, tcStrat))
            QueueOGCD(AID.Triplecast,
                Player,
                tcStrat is TriplecastStrategy.Force
                or TriplecastStrategy.Force1
                or TriplecastStrategy.ForceWeave
                or TriplecastStrategy.ForceWeave1
                ? OGCDPriority.ForcedOGCD
                : OGCDPriority.Triplecast);
        //Amplifier
        if (ShouldUseAmplifier(primaryTarget, ampStrat))
            QueueOGCD(AID.Amplifier,
                Player,
                ampStrat is OffensiveStrategy.Force
                or OffensiveStrategy.AnyWeave
                or OffensiveStrategy.EarlyWeave
                or OffensiveStrategy.LateWeave
                ? OGCDPriority.ForcedOGCD
                : OGCDPriority.Amplifier);
        //Manafont
        if (ShouldUseManafont(primaryTarget, mfStrat))
            QueueOGCD(AID.Manafont,
                Player,
                mfStrat is ManafontStrategy.Force
                or ManafontStrategy.ForceWeave
                or ManafontStrategy.ForceEX
                or ManafontStrategy.ForceWeaveEX
                ? OGCDPriority.ForcedOGCD
                : OGCDPriority.Manafont);
        //Retrace
        if (ShouldUseRetrace(retraceStrat))
            QueueOGCD(AID.Retrace,
                Player,
                OGCDPriority.ForcedOGCD);
        //Between the Lines
        if (ShouldUseBTL(btlStrat))
            QueueOGCD(AID.BetweenTheLines,
                Player,
                OGCDPriority.ForcedOGCD);
        //Potion
        if (potionStrat is PotionStrategy.AlignWithRaidBuffs && CD(AID.LeyLines) < 5 ||
            potionStrat is PotionStrategy.Immediate)
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionInt, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
        #endregion
    }

    #region Core Execution Helpers
    public void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueGCD(aid, target, (int)(object)priority, delay);

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

    public void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueOGCD(aid, target, (int)(object)priority, delay);

    public void QueueOGCD(AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }

    public bool QueueAction(AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return false;

        if (def.Range != 0 && target == null)
        {
            return false;
        }

        Vector3 targetPos = default;

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
    private void BestRotation(Actor? target) //Best rotation based on targets nearby
    {
        if (ShouldUseAOE)
        {
            BestAOE(target);
        }
        if (!ShouldUseAOE)
        {
            BestST(target);
        }
    }
    #endregion

    #region Single-Target Helpers
    private void STLv1toLv34(Actor? target) //Level 1-34 single-target rotation
    {
        //Fire
        if (Unlocked(AID.Fire1) && //if Fire is unlocked
            NoStance && MP >= 800 || //if no stance is active and MP is 800 or more
            InAstralFire && MP >= 1600) //or if Astral Fire is active and MP is 1600 or more
            QueueGCD(AID.Fire1, target, GCDPriority.Standard); //Queue Fire
        //Ice
        //TODO: Fix Blizzard I still casting once after at 10000MP due to MP tick not counting fast enough before next cast
        if (InUmbralIce && MP < 9500) //if Umbral Ice is active and MP is not max
            QueueGCD(AID.Blizzard1, target, GCDPriority.Standard); //Queue Blizzard
        //Transpose 
        if (ActionReady(AID.Transpose) && //if Transpose is unlocked & off cooldown
            InAstralFire && MP < 1600 || //if Astral Fire is active and MP is less than 1600
            InUmbralIce && MP == 10000) //or if Umbral Ice is active and MP is max
            QueueOGCD(AID.Transpose, Player, OGCDPriority.Transpose); //Queue Transpose
    }
    private void STLv35toLv59(Actor? target) //Level 35-59 single-target rotation
    {
        if (NoStance) //if no stance is active
        {
            if (Unlocked(AID.Blizzard3)) //if Blizzard III is unlocked
            {
                if (MP >= 10000) //if no stance is active and MP is max (opener)
                    QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                if (MP < 10000 && Player.InCombat) //or if in combat and no stance is active and MP is less than max (died or stopped attacking)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3); //Queue Swiftcast->Blizzard III
                    else
                        QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                }
            }
        }
        if (InUmbralIce) //if Umbral Ice is active
        {
            //Step 1 - max stacks in UI
            if (JustUsed(AID.Blizzard3, 5)) //if Blizzard III was just used
            {
                if (!Unlocked(AID.Blizzard4) && UmbralStacks == 3) //if Blizzard IV is not unlocked and Umbral Ice stacks are max
                    QueueGCD(AID.Blizzard1, target, GCDPriority.Step2); //Queue Blizzard I
                if (Unlocked(AID.Blizzard4) && UmbralHearts != MaxUmbralHearts) //if Blizzard IV is unlocked and Umbral Hearts are not max
                    QueueGCD(AID.Blizzard4, target, GCDPriority.Step2); //Queue Blizzard IV
            }
            //Step 2 - swap from UI to AF
            if (Unlocked(AID.Fire3) && //if Fire III is unlocked
                JustUsed(AID.Blizzard1, 5) && //and Blizzard I was just used
                MP < 10000 && //and MP is less than max
                Unlocked(TraitID.UmbralHeart) ? UmbralHearts == MaxUmbralHearts : UmbralHearts == 0) //and Umbral Hearts are max if unlocked, or 0 if not
                QueueGCD(AID.Fire3, target, JustUsed(AID.Blizzard1, 5) ? GCDPriority.Step10 : GCDPriority.Step1); //Queue Fire III, increase priority if Blizzard I was just used
        }
        if (InAstralFire) //if Astral Fire is active
        {
            //Step 1 - Fire 1
            if (MP >= 1600) //if MP is 1600 or more
                QueueGCD(AID.Fire1, target, GCDPriority.Step3); //Queue Fire I
            //Step 2B - F3P 
            if (SelfStatusLeft(SID.Firestarter, 30) is < 25 and not 0 && //if Firestarter buff is active and not 0
                AstralStacks == 3) //and Umbral Hearts are 0
                QueueGCD(AID.Fire3, target, GCDPriority.Step10); //Queue Fire III (AF3 F3P)
            //Step 3 - swap from AF to UI
            if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
                MP < 1600) //and MP is less than 400
                QueueGCD(AID.Blizzard3, target, GCDPriority.Step1); //Queue Blizzard III
        }
    }
    private void STLv60toLv71(Actor? target) //Level 60-71 single-target rotation
    {
        if (NoStance) //if no stance is active
        {
            if (Unlocked(AID.Blizzard3)) //if Blizzard III is unlocked
            {
                if (MP >= 10000) //if no stance is active and MP is max (opener)
                    QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                if (MP < 10000 && Player.InCombat) //or if in combat and no stance is active and MP is less than max (died or stopped attacking)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3); //Queue Swiftcast->Blizzard III
                    else
                        QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                }
            }
        }
        if (InUmbralIce) //if Umbral Ice is active
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.Blizzard4) && //if Blizzard IV is unlocked
                JustUsed(AID.Blizzard3, 5) || UmbralHearts != MaxUmbralHearts) //and Blizzard III was just used or Umbral Hearts are not max
                QueueGCD(AID.Blizzard4, target, GCDPriority.Step2); //Queue Blizzard IV
            //Step 2 - swap from UI to AF
            if (Unlocked(AID.Fire3) && //if Fire III is unlocked
                UmbralHearts == MaxUmbralHearts) //and Umbral Hearts are max
                QueueGCD(AID.Fire3, target, GCDPriority.Step1); //Queue Fire III
        }
        if (InAstralFire) //if Astral Fire is active
        {
            //Step 1-3, 5-7 - Fire IV
            if (MP >= 1600) //and MP is 1600 or more
                QueueGCD(AID.Fire4, target, GCDPriority.Step5); //Queue Fire IV
            //Step 4A - Fire 1
            if (ElementTimer <= (SpS * 3) && //if time remaining on current element is less than 3x GCDs
                MP >= 4000) //and MP is 4000 or more
                QueueGCD(AID.Fire1, target, ElementTimer <= 5 && MP >= 4000 ? GCDPriority.Paradox : GCDPriority.Step4); //Queue Fire I, increase priority if less than 3s left on element
            //Step 4B - F3P 
            if (SelfStatusLeft(SID.Firestarter, 30) is < 25 and not 0 && //if Firestarter buff is active and not 0
                AstralStacks == 3) //and Umbral Hearts are 0
                QueueGCD(AID.Fire3, target, GCDPriority.Step10); //Queue Fire III (AF3 F3P)
            //Step 8 - swap from AF to UI
            if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
                MP < 1600) //and MP is less than 400
                QueueGCD(AID.Blizzard3, target, GCDPriority.Step1); //Queue Blizzard III
        }
    }
    private void STLv72toLv89(Actor? target) //Level 72-89 single-target rotation
    {
        if (NoStance) //if no stance is active
        {
            if (Unlocked(AID.Blizzard3)) //if Blizzard III is unlocked
            {
                if (MP >= 10000) //if no stance is active and MP is max (opener)
                    QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                if (MP < 10000 && Player.InCombat) //or if in combat and no stance is active and MP is less than max (died or stopped attacking)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3); //Queue Swiftcast->Blizzard III
                    else
                        QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                }
            }
        }
        if (InUmbralIce) //if Umbral Ice is active
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.Blizzard4) && //if Blizzard IV is unlocked
                JustUsed(AID.Blizzard3, 5) || UmbralHearts != MaxUmbralHearts) //and Blizzard III was just used or Umbral Hearts are not max
                QueueGCD(AID.Blizzard4, target, GCDPriority.Step2); //Queue Blizzard IV
            //Step 2 - swap from UI to AF
            if (Unlocked(AID.Fire3) && //if Fire III is unlocked
                UmbralHearts == MaxUmbralHearts) //and Umbral Hearts are max
                QueueGCD(AID.Fire3, target, GCDPriority.Step1); //Queue Fire III
        }
        if (InAstralFire) //if Astral Fire is active
        {
            //Step 1-3, 5-7 - Fire IV
            if (MP >= 1600) //and MP is 1600 or more
                QueueGCD(AID.Fire4, target, GCDPriority.Step5); //Queue Fire IV
            //Step 4A - Fire 1
            if (ElementTimer <= (SpS * 3) && //if time remaining on current element is less than 3x GCDs
                MP >= 4000) //and MP is 4000 or more
                QueueGCD(AID.Fire1, target, ElementTimer <= 5 && MP >= 4000 ? GCDPriority.Paradox : GCDPriority.Step4); //Queue Fire I, increase priority if less than 3s left on element
            //Step 4B - F3P 
            if (SelfStatusLeft(SID.Firestarter, 30) is < 25 and not 0 && //if Firestarter buff is active and not 0
                AstralStacks == 3) //and Umbral Hearts are 0
                QueueGCD(AID.Fire3, target, GCDPriority.Step10); //Queue Fire III (AF3 F3P)
            //Step 8 - Despair 
            if (MP is < 1600 and not 0 && //if MP is less than 1600 and not 0
                Unlocked(AID.Despair)) //and Despair is unlocked
            {
                if (ActionReady(AID.Swiftcast) && ElementTimer < GetCastTime(AID.Despair))
                    QueueGCD(AID.Swiftcast, target, GCDPriority.Step2); //Queue Swiftcast->Despair
                else
                    QueueGCD(AID.Despair, target, GCDPriority.Step2); //Queue Despair
            }
            //Step 9 - swap from AF to UI 
            if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
                MP <= 400) //and MP is less than 400
                QueueGCD(AID.Blizzard3, target, GCDPriority.Step1); //Queue Blizzard III
        }
    }
    private void STLv90toLv99(Actor? target) //Level 90-99 single-target rotation
    {
        if (NoStance) //if no stance is active
        {
            if (Unlocked(AID.Blizzard3)) //if Blizzard III is unlocked
            {
                if (MP >= 10000) //if no stance is active and MP is max (opener)
                    QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                if (MP < 10000 && Player.InCombat) //or if in combat and no stance is active and MP is less than max (died or stopped attacking)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3); //Queue Swiftcast->Blizzard III
                    else
                        QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                }
            }
        }
        if (InUmbralIce) //if Umbral Ice is active
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.Blizzard4) && //if Blizzard IV is unlocked
                JustUsed(AID.Blizzard3, 5) || UmbralHearts != MaxUmbralHearts) //and Blizzard III was just used or Umbral Hearts are not max
                QueueGCD(AID.Blizzard4, target, GCDPriority.Step3); //Queue Blizzard IV
            //Step 2 - Ice Paradox
            if (canParadox && //if Paradox is unlocked and Paradox is active
                JustUsed(AID.Blizzard4, 5)) //and Blizzard IV was just used
                QueueGCD(AID.Paradox, target, GCDPriority.Step2); //Queue Paradox
            //Step 2 - swap from UI to AF
            if (Unlocked(AID.Fire3) && //if Fire III is unlocked
                UmbralHearts == MaxUmbralHearts) //and Umbral Hearts are max
                QueueGCD(AID.Fire3, target, GCDPriority.Step1); //Queue Fire III
        }
        if (InAstralFire) //if Astral Fire is active
        {
            //Step 1-4, 6 & 7 - Fire IV
            if (MP >= 1600) //and MP is 1600 or more
                QueueGCD(AID.Fire4, target, GCDPriority.Step5); //Queue Fire IV
            //Step 5A - Paradox
            if (canParadox && //if Paradox is unlocked and Paradox is active
                ElementTimer < (SpS * 3) && //and time remaining on current element is less than 3x GCDs
                MP >= 1600) //and MP is 1600 or more
                QueueGCD(AID.Paradox, target, ElementTimer <= 3 ? GCDPriority.Paradox : GCDPriority.Step4); //Queue Paradox, increase priority if less than 3s left on element
            //Step 4B - F3P 
            if (SelfStatusLeft(SID.Firestarter, 30) is < 25 and not 0 && //if Firestarter buff is active and not 0
                AstralStacks == 3) //and Umbral Hearts are 0
                QueueGCD(AID.Fire3, target, GCDPriority.Step10); //Queue Fire III (AF3 F3P)
            //Step 8 - Despair 
            if (MP is < 1600 and not 0 && //if MP is less than 1600 and not 0
                Unlocked(AID.Despair)) //and Despair is unlocked
            {
                if (ActionReady(AID.Swiftcast) && ElementTimer < GetCastTime(AID.Despair))
                    QueueGCD(AID.Swiftcast, target, GCDPriority.Step2); //Queue Swiftcast->Despair
                else
                    QueueGCD(AID.Despair, target, GCDPriority.Step2); //Queue Despair
            }
            //Step 9 - swap from AF to UI
            if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
                MP <= 400) //and MP is less than 400
                QueueGCD(AID.Blizzard3, target, GCDPriority.Step1); //Queue Blizzard III
        }
    }
    private void STLv100(Actor? target) //Level 100 single-target rotation
    {
        if (NoStance) //if no stance is active
        {
            if (Unlocked(AID.Blizzard3)) //if Blizzard III is unlocked
            {
                if (MP >= 10000) //if no stance is active and MP is max (opener)
                    QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                if (MP < 10000 && Player.InCombat) //or if in combat and no stance is active and MP is less than max (died or stopped attacking)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3); //Queue Swiftcast->Blizzard III
                    else
                        QueueGCD(AID.Blizzard3, target, GCDPriority.NeedB3); //Queue Blizzard III
                }
            }
        }
        if (InUmbralIce) //if Umbral Ice is active
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.Blizzard4) && //if Blizzard IV is unlocked
                JustUsed(AID.Blizzard3, 5) || UmbralHearts != MaxUmbralHearts) //and Blizzard III was just used or Umbral Hearts are not max
                QueueGCD(AID.Blizzard4, target, GCDPriority.Step3); //Queue Blizzard IV
            //Step 2 - Ice Paradox
            if (canParadox && //if Paradox is unlocked and Paradox is active
                JustUsed(AID.Blizzard4, 5)) //and Blizzard IV was just used
                QueueGCD(AID.Paradox, target, GCDPriority.Step2); //Queue Paradox
            //Step 2 - swap from UI to AF
            if (Unlocked(AID.Fire3) && //if Fire III is unlocked
                UmbralHearts == MaxUmbralHearts) //and Umbral Hearts are max
                QueueGCD(AID.Fire3, target, GCDPriority.Step1); //Queue Fire III
        }
        if (InAstralFire) //if Astral Fire is active
        {
            //Step 1-4, 6 & 7 - Fire IV
            if (AstralSoulStacks != 6 && //and Astral Soul stacks are not max
                MP >= 1600) //and MP is 1600 or more
                QueueGCD(AID.Fire4, target, GCDPriority.Step6); //Queue Fire IV
            //Step 5A - Paradox
            if (ParadoxActive && //if Paradox is active
                ElementTimer < (SpS * 3) && //and time remaining on current element is less than 3x GCDs
                MP >= 1600) //and MP is 1600 or more
                QueueGCD(AID.Paradox, target, ElementTimer <= 3 ? GCDPriority.Paradox : GCDPriority.Step5); //Queue Paradox, increase priority if less than 3s left on element
            //Step 4B - F3P 
            if (SelfStatusLeft(SID.Firestarter, 30) is < 25 and not 0 && //if Firestarter buff is active and not 0
                AstralStacks == 3) //and Umbral Hearts are 0
                QueueGCD(AID.Fire3, target, GCDPriority.Step10); //Queue Fire III (AF3 F3P)
            //Step 8 - Despair
            if (MP is < 1600 and not 0 && //if MP is less than 1600 and not 0
                Unlocked(AID.Despair)) //and Despair is unlocked
                QueueGCD(AID.Despair, target, GCDPriority.Step3); //Queue Despair
            //Step 9 - Flare Star
            if (AstralSoulStacks == 6) //if Astral Soul stacks are max
            {
                if (JustUsed(AID.Despair, 5f) && ActionReady(AID.Swiftcast))
                    QueueGCD(AID.Swiftcast, Player, GCDPriority.Step2); //Queue Swiftcast->Flare Star
                QueueGCD(AID.FlareStar, target, GCDPriority.Step2); //Queue Flare Star
            }
            //Step 10A - skip Flare Star if we cant use it (cryge)
            if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
                MP <= 400 && //and MP is less than 400
                AstralSoulStacks is < 6 and > 0) //and Astral Soul stacks are less than 6 but greater than 0
                QueueGCD(AID.Blizzard3, target, GCDPriority.Step1); //Queue Blizzard III
            //Step 10B - swap from AF to UI
            if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
                MP <= 400 && //and MP is less than 400
                AstralSoulStacks == 0) //and Astral Soul stacks are 0
                QueueGCD(AID.Blizzard3, target, GCDPriority.Step1); //Queue Blizzard III
        }
    }
    private void BestST(Actor? target) //Single-target rotation based on level
    {
        if (Player.Level is >= 1 and <= 34)
        {
            STLv1toLv34(target);
        }
        if (Player.Level is >= 35 and <= 59)
        {
            STLv35toLv59(target);
        }
        if (Player.Level is >= 60 and <= 71)
        {
            STLv60toLv71(target);
        }
        if (Player.Level is >= 72 and <= 89)
        {
            STLv72toLv89(target);
        }
        if (Player.Level is >= 90 and <= 99)
        {
            STLv90toLv99(target);
        }
        if (Player.Level is 100)
        {
            STLv100(target);
        }
    }
    #endregion

    #region AOE Helpers
    private void AOELv12toLv34(Actor? target) //Level 12-34 AOE rotation
    {
        if (NoStance)
        {
            if (Unlocked(AID.Blizzard2))
            {
                if (MP >= 10000)
                    QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                if (MP < 10000 && Player.InCombat)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3);
                    else
                        QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                }
            }
        }
        //Fire
        if (Unlocked(AID.Fire2) && //if Fire is unlocked
            InAstralFire && MP >= 3000) //or if Astral Fire is active and MP is 1600 or more
            QueueGCD(AID.Fire2, target, GCDPriority.Standard); //Queue Fire II
        //Ice
        //TODO: MP tick is not fast enough before next cast, this will cause an extra unnecessary cast
        if (InUmbralIce &&
            MP <= 9600)
            QueueGCD(AID.Blizzard2, target, GCDPriority.Standard); //Queue Blizzard II
        //Transpose 
        if (ActionReady(AID.Transpose) && //if Transpose is unlocked & off cooldown
            (InAstralFire && MP < 3000 || //if Astral Fire is active and MP is less than 1600
            InUmbralIce && MP > 9600)) //or if Umbral Ice is active and MP is max
            QueueOGCD(AID.Transpose, Player, OGCDPriority.Transpose); //Queue Transpose
    }
    private void AOELv35toLv39(Actor? target) //Level 35-39 AOE rotation
    {
        if (NoStance)
        {
            if (Unlocked(AID.Blizzard2))
            {
                if (MP >= 10000)
                    QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                if (MP < 10000 && Player.InCombat)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3);
                    else
                        QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                }
            }
        }
        if (InUmbralIce)
        {
            //Step 1 - max stacks in UI
            //TODO: MP tick is not fast enough before next cast, this will cause an extra unnecessary cast
            if (Unlocked(AID.Blizzard2) &&
                MP < 9600)
                QueueGCD(AID.Blizzard2, target, GCDPriority.Step2);
            //Step 2 - swap from UI to AF
            if (Unlocked(AID.Fire2) &&
                MP >= 9600 &&
                UmbralStacks == 3)
                QueueGCD(AID.Fire2, target, GCDPriority.Step1);
        }
        if (InAstralFire)
        {
            if (MP >= 3000)
                QueueGCD(AID.Fire2, target, GCDPriority.Step2);
            if (Unlocked(AID.Blizzard2) &&
                MP < 3000)
                QueueGCD(AID.Blizzard2, target, GCDPriority.Step1);
        }
    }
    private void AOELv40toLv49(Actor? target) //Level 40-49 AOE rotation
    {
        if (NoStance)
        {
            if (Unlocked(AID.Blizzard2))
            {
                if (MP >= 10000)
                    QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                if (MP < 10000 && Player.InCombat)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3);
                    else
                        QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                }
            }
        }
        if (InUmbralIce)
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.Blizzard2) &&
                UmbralStacks < 3)
                QueueGCD(AID.Blizzard2, target, GCDPriority.Step3);
            //Step 2 - Freeze
            if (Unlocked(AID.Freeze) && !JustUsed(AID.Freeze, 5f) &&
                (JustUsed(AID.Blizzard2, 5) || MP < 10000))
                QueueGCD(AID.Freeze, target, GCDPriority.Step2);
            //Step 3 - swap from UI to AF
            if (Unlocked(AID.Fire2) &&
                MP >= 10000 &&
                UmbralStacks == 3)
                QueueGCD(AID.Fire2, target, GCDPriority.Step1);
        }
        if (InAstralFire)
        {
            if (MP >= 3000)
                QueueGCD(AID.Fire2, target, GCDPriority.Step2);
            if (Unlocked(AID.Blizzard2) &&
                MP < 3000)
                QueueGCD(AID.Blizzard2, target, GCDPriority.Step1);
        }
    }
    private void AOELv50toLv57(Actor? target) //Level 50-57 AOE rotation
    {
        if (NoStance)
        {
            if (Unlocked(AID.Blizzard2))
            {
                if (MP >= 10000)
                    QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                if (MP < 10000 && Player.InCombat)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3);
                    else
                        QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                }
            }
        }
        if (InUmbralIce)
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.Blizzard2) &&
                UmbralStacks < 3)
                QueueGCD(AID.Blizzard2, target, GCDPriority.Step3);
            //Step 2 - Freeze
            if (Unlocked(AID.Freeze) && !JustUsed(AID.Freeze, 5f) &&
                (JustUsed(AID.Blizzard2, 5) || MP < 10000))
                QueueGCD(AID.Freeze, target, GCDPriority.Step2);
            //Step 3 - swap from UI to AF
            if (Unlocked(AID.Fire2) &&
                MP >= 10000 &&
                UmbralStacks == 3)
                QueueGCD(AID.Fire2, target, GCDPriority.Step1);
        }
        if (InAstralFire)
        {
            //Step 1 - spam Fire 2
            if (MP >= 3000)
                QueueGCD(AID.Fire2, target, GCDPriority.Step3);
            //Step 2 - Flare
            if (Unlocked(AID.Flare) &&
                MP < 3000)
                QueueGCD(AID.Flare, target, GCDPriority.Step2);
            //Step 3 - swap from AF to UI
            if (Unlocked(AID.Blizzard2) &&
                (!Unlocked(AID.Flare) && MP < 3000) || //do your job quests, fool
                (Unlocked(AID.Flare) && MP < 400))
                QueueGCD(AID.Blizzard2, target, MP < 400 ? GCDPriority.Step10 : GCDPriority.Step1);
        }
    }
    private void AOELv58toLv81(Actor? target) //Level 58-81 AOE rotation
    {
        if (NoStance)
        {
            if (Unlocked(AID.Blizzard2))
            {
                if (MP >= 10000)
                    QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                if (MP < 10000 && Player.InCombat)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3);
                    else
                        QueueGCD(AID.Blizzard2, target, GCDPriority.NeedB3);
                }
            }
        }
        if (InUmbralIce)
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.Blizzard2) &&
                UmbralStacks < 3)
                QueueGCD(AID.Blizzard2, target, GCDPriority.Step3);
            //Step 2 - Freeze
            if (Unlocked(AID.Freeze) && !JustUsed(AID.Freeze, 5f) &&
                (JustUsed(AID.Blizzard2, 5) || MP < 10000))
                QueueGCD(AID.Freeze, target, GCDPriority.Step2);
            //Step 3 - swap from UI to AF
            if (Unlocked(AID.Fire2) &&
                MP >= 10000 &&
                UmbralStacks == 3)
                QueueGCD(AID.Fire2, target, GCDPriority.Step1);
        }
        if (InAstralFire)
        {
            //Step 1 - spam Fire 2
            if (UmbralHearts > 1)
                QueueGCD(AID.Fire2, target, GCDPriority.Step4);
            //Step 2 - Flare
            if (Unlocked(AID.Flare))
            {
                //first cast
                if (UmbralHearts == 1)
                    QueueGCD(AID.Flare, target, GCDPriority.Step3);
                //second cast
                if (UmbralHearts == 0 &&
                    MP >= 800)
                    QueueGCD(AID.Flare, target, GCDPriority.Step2);
            }
            //Step 3 - swap from AF to UI
            if (Unlocked(AID.Blizzard2) &&
                MP < 400)
                QueueGCD(AID.Blizzard2, target, GCDPriority.Step1);
        }
    }
    private void AOELv82toLv99(Actor? target) //Level 82-99 AOE rotation
    {
        if (NoStance)
        {
            if (Unlocked(AID.HighBlizzard2))
            {
                if (MP >= 10000)
                    QueueGCD(AID.HighBlizzard2, target, GCDPriority.NeedB3);
                if (MP < 10000 && Player.InCombat)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3);
                    else
                        QueueGCD(AID.HighBlizzard2, target, GCDPriority.NeedB3);
                }
            }
        }
        if (InUmbralIce)
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.HighBlizzard2) &&
                UmbralStacks < 3)
                QueueGCD(AID.HighBlizzard2, target, GCDPriority.Step3);
            //Step 2 - Freeze
            if (Unlocked(AID.Freeze) && !JustUsed(AID.Freeze, 5f) &&
                (JustUsed(AID.HighBlizzard2, 5) || MP < 10000))
                QueueGCD(AID.Freeze, target, GCDPriority.Step2);
            //Step 3 - swap from UI to AF
            if (Unlocked(AID.HighFire2) &&
                MP >= 10000 &&
                UmbralStacks == 3)
                QueueGCD(AID.HighFire2, target, GCDPriority.Step1);
        }
        if (InAstralFire)
        {
            //Step 1 - spam Fire 2
            if (MP > 5500)
                QueueGCD(AID.HighFire2, target, GCDPriority.Step4);
            //Step 2 - Flare
            if (Unlocked(AID.Flare))
            {
                //first cast
                if (UmbralHearts == 1)
                    QueueGCD(AID.Flare, target, GCDPriority.Step3);
                //second cast
                if (UmbralHearts == 0 &&
                    MP >= 800)
                    QueueGCD(AID.Flare, target, GCDPriority.Step2);
            }
            //Step 3 - swap from AF to UI
            if (Unlocked(AID.HighBlizzard2) &&
                MP < 400)
                QueueGCD(AID.HighBlizzard2, target, GCDPriority.Step1);
        }
    }
    private void AOELv100(Actor? target) //Level 100 AOE rotation
    {
        if (NoStance)
        {
            if (Unlocked(AID.HighBlizzard2))
            {
                if (MP >= 10000)
                    QueueGCD(AID.HighBlizzard2, target, GCDPriority.NeedB3);
                if (MP < 10000 && Player.InCombat)
                {
                    if (ActionReady(AID.Swiftcast))
                        QueueGCD(AID.Swiftcast, target, GCDPriority.NeedB3);
                    else
                        QueueGCD(AID.HighBlizzard2, target, GCDPriority.NeedB3);
                }
            }
        }
        if (InUmbralIce)
        {
            //Step 1 - max stacks in UI
            if (Unlocked(AID.HighBlizzard2) &&
                UmbralStacks < 3)
                QueueGCD(AID.HighBlizzard2, target, GCDPriority.Step3);
            //Step 2 - Freeze
            if (Unlocked(AID.Freeze) && !JustUsed(AID.Freeze, 5f) &&
                (JustUsed(AID.HighBlizzard2, 5) || MP < 10000))
                QueueGCD(AID.Freeze, target, GCDPriority.Step2);
            //Step 3 - swap from UI to AF
            if (Unlocked(AID.HighFire2) &&
                MP >= 10000 &&
                UmbralStacks == 3)
                QueueGCD(AID.HighFire2, target, GCDPriority.Step1);
        }
        if (InAstralFire)
        {
            //Step 1 - Flare
            if (Unlocked(AID.Flare))
            {
                //first cast
                if (UmbralHearts == 1)
                    QueueGCD(AID.Flare, target, GCDPriority.Step3);
                //second cast
                if (UmbralHearts == 0 &&
                    MP >= 800)
                    QueueGCD(AID.Flare, target, GCDPriority.Step2);
            }
            //Step 2 - Flare Star
            if (AstralSoulStacks == 6) //if Astral Soul stacks are max
                QueueGCD(AID.FlareStar, target, GCDPriority.Step2); //Queue Flare Star
            //Step 3 - swap from AF to UI
            if (Unlocked(AID.HighBlizzard2) &&
                MP < 400)
                QueueGCD(AID.HighBlizzard2, target, GCDPriority.Step1);
        }
    }
    private void BestAOE(Actor? target) //AOE rotation based on level
    {
        if (In25y(target))
        {
            if (Player.Level is >= 12 and <= 34)
            {
                AOELv12toLv34(target);
            }
            if (Player.Level is >= 35 and <= 39)
            {
                AOELv35toLv39(target);
            }
            if (Player.Level is >= 40 and <= 49)
            {
                AOELv40toLv49(target);
            }
            if (Player.Level is >= 50 and <= 57)
            {
                AOELv50toLv57(target);
            }
            if (Player.Level is >= 58 and <= 81)
            {
                AOELv58toLv81(target);
            }
            if (Player.Level is >= 82 and <= 99)
            {
                AOELv82toLv99(target);
            }
            if (Player.Level is 100)
            {
                AOELv100(target);
            }
        }
    }
    #endregion

    #region Cooldown Helpers
    private bool ShouldUseThunder(Actor? target, ThunderStrategy strategy) => strategy switch
    {
        ThunderStrategy.Thunder3 => Player.InCombat && target != null && hasThunderhead && ThunderLeft <= 3 && In25y(target),
        ThunderStrategy.Thunder6 => Player.InCombat && target != null && hasThunderhead && ThunderLeft <= 6 && In25y(target),
        ThunderStrategy.Thunder9 => Player.InCombat && target != null && hasThunderhead && ThunderLeft <= 9 && In25y(target),
        ThunderStrategy.Thunder0 => Player.InCombat && target != null && hasThunderhead && ThunderLeft is 0 && In25y(target),
        ThunderStrategy.Force => hasThunderhead,
        ThunderStrategy.Delay => false,
        _ => false
    };
    private bool ShouldSpendPolyglot(Actor? target, PolyglotStrategy strategy) => strategy switch
    {
        PolyglotStrategy.AutoSpendAll
            => Player.InCombat &&
            target != null &&
            Polyglots > 0 && //Spend 3
            (((CD(AID.Triplecast) < 5 || CD(AID.Triplecast) == 0 || (CD(AID.Triplecast) >= 59 && CD(AID.Triplecast) <= 65)) && PlayerHasEffect(SID.LeyLines, 30)) || //Triplecast prep
            (CD(AID.LeyLines) < 5 || CD(AID.LeyLines) == 0 || CD(AID.LeyLines) <= 125 && CD(AID.LeyLines) >= 119) || //Ley Lines prep
            CD(AID.Amplifier) < 0.6f || //Amplifier prep
            (CD(AID.Manafont) < 0.6f && MP < 1600)), //Manafont prep
        PolyglotStrategy.AutoHold1
            => Player.InCombat &&
            target != null &&
            Polyglots > 1 && //Spend 2
            (((CD(AID.Triplecast) < 5 || CD(AID.Triplecast) == 0 || (CD(AID.Triplecast) >= 59 && CD(AID.Triplecast) <= 65)) && PlayerHasEffect(SID.LeyLines, 30)) || //Triplecast prep
            (CD(AID.LeyLines) < 5 || CD(AID.LeyLines) == 0 || CD(AID.LeyLines) <= 125 && CD(AID.LeyLines) >= 119) || //Ley Lines prep
            CD(AID.Amplifier) < 0.6f || //Amplifier prep
            (CD(AID.Manafont) < 0.6f && MP < 1600)), //Manafont prep
        PolyglotStrategy.AutoHold2
            => Player.InCombat &&
            target != null &&
            Polyglots > 2 && //Spend 1
            (((CD(AID.Triplecast) < 5 || (CD(AID.Triplecast) <= 60 && CD(AID.Triplecast) >= 65)) && PlayerHasEffect(SID.LeyLines, 30)) || //Triplecast prep
            (CD(AID.LeyLines) < 5 || CD(AID.LeyLines) <= 120 && CD(AID.LeyLines) >= 110 || //Ley Lines prep
            CD(AID.Amplifier) < 0.6f || //Amplifier prep
            (CD(AID.Manafont) < 0.6f && MP < 1600))), //Manafont prep
        PolyglotStrategy.AutoHold3
            => Player.InCombat &&
            target != null &&
            Polyglots == MaxPolyglots && //if max Polyglots
            EnochianTimer <= 10000f, //Enochian is 5s away from adding a Polyglot
        _ => false
    };
    private bool ShouldUsePolyglot(Actor? target, PolyglotStrategy strategy) => strategy switch
    {
        PolyglotStrategy.AutoSpendAll => ShouldSpendPolyglot(target, PolyglotStrategy.AutoSpendAll),
        PolyglotStrategy.AutoHold1 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold1),
        PolyglotStrategy.AutoHold2 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold2),
        PolyglotStrategy.AutoHold3 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold3),
        PolyglotStrategy.XenoSpendAll => ShouldSpendPolyglot(target, PolyglotStrategy.AutoSpendAll),
        PolyglotStrategy.XenoHold1 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold1),
        PolyglotStrategy.XenoHold2 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold2),
        PolyglotStrategy.XenoHold3 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold3),
        PolyglotStrategy.FoulSpendAll => ShouldSpendPolyglot(target, PolyglotStrategy.AutoSpendAll),
        PolyglotStrategy.FoulHold1 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold1),
        PolyglotStrategy.FoulHold2 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold2),
        PolyglotStrategy.FoulHold3 => ShouldSpendPolyglot(target, PolyglotStrategy.AutoHold3),
        PolyglotStrategy.ForceXeno => canXeno,
        PolyglotStrategy.ForceFoul => canFoul,
        PolyglotStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseLeyLines(Actor? target, LeyLinesStrategy strategy) => strategy switch
    {
        LeyLinesStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canLL &&
        canWeaveIn,
        LeyLinesStrategy.Force => Player.InCombat && canLL,
        LeyLinesStrategy.Force1 => Player.InCombat && canLL && CD(AID.LeyLines) < (SpS * 2),
        LeyLinesStrategy.ForceWeave => Player.InCombat && canLL && canWeaveIn,
        LeyLinesStrategy.ForceWeave1 => Player.InCombat && canLL && canWeaveIn && CD(AID.LeyLines) < (SpS * 2),
        LeyLinesStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseAmplifier(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canAmp &&
        canWeaveIn &&
        Polyglots != MaxPolyglots,
        OffensiveStrategy.Force => canAmp,
        OffensiveStrategy.AnyWeave => canAmp && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canAmp && canWeaveEarly,
        OffensiveStrategy.LateWeave => canAmp && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseRetrace(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => false,
        OffensiveStrategy.Force => canRetrace,
        OffensiveStrategy.AnyWeave => canRetrace && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canRetrace && canWeaveEarly,
        OffensiveStrategy.LateWeave => canRetrace && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false

    };
    private bool ShouldUseBTL(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => false,
        OffensiveStrategy.Force => canBTL,
        OffensiveStrategy.AnyWeave => canBTL && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canBTL && canWeaveEarly,
        OffensiveStrategy.LateWeave => canBTL && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false

    };
    private bool ShouldUseManafont(Actor? target, ManafontStrategy strategy) => strategy switch
    {
        ManafontStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canMF &&
        InAstralFire &&
        (JustUsed(BestXenoglossy, 5) && MP < 1600),
        ManafontStrategy.Force => canMF,
        ManafontStrategy.ForceWeave => canMF && canWeaveIn,
        ManafontStrategy.ForceEX => canMF,
        ManafontStrategy.ForceWeaveEX => canMF && canWeaveIn,
        ManafontStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseTriplecast(Actor? target, TriplecastStrategy strategy) => strategy switch
    {
        TriplecastStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canTC &&
        canWeaveIn &&
        InAstralFire &&
        PlayerHasEffect(SID.LeyLines, 30),
        TriplecastStrategy.Force => Player.InCombat && canTC,
        TriplecastStrategy.Force1 => Player.InCombat && canTC && CD(AID.Triplecast) < (SpS * 2),
        TriplecastStrategy.ForceWeave => Player.InCombat && canTC && canWeaveIn,
        TriplecastStrategy.ForceWeave1 => Player.InCombat && canTC && canWeaveIn && CD(AID.Triplecast) < (SpS * 2),
        TriplecastStrategy.Delay => false,
        _ => false
    };
    #endregion
}
