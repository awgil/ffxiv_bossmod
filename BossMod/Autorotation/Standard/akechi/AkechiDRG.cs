using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.DRG;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiDRG(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies

    public enum Track { AOE, Hold, Dives, Potion, LifeSurge, Jump, DragonfireDive, Geirskogul, Stardiver, PiercingTalon, TrueNorth, LanceCharge, BattleLitany, MirageDive, Nastrond, WyrmwindThrust, RiseOfTheDragon, Starcross }
    public enum AOEStrategy { AutoTargetHitPrimary, AutoTargetHitMost, ForceST, Force123ST, ForceBuffsST, ForceAOE }
    public enum HoldStrategy { Allow, Forbid }
    public enum DivesStrategy { AllowMaxMelee, AllowCloseMelee, Allow, Forbid }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    public enum SurgeStrategy { Automatic, Force, ForceWeave, ForceNextOpti, ForceNextOptiWeave, Delay }
    public enum JumpStrategy { Automatic, Force, ForceEX, ForceEX2, ForceWeave, Delay }
    public enum DragonfireStrategy { Automatic, Force, ForceEX, ForceWeave, Delay }
    public enum GeirskogulStrategy { Automatic, Force, ForceEX, ForceWeave, Delay }
    public enum StardiverStrategy { Automatic, Force, ForceEX, ForceWeave, Delay }
    public enum PiercingTalonStrategy { AllowEX, Allow, Force, ForceEX, Forbid }
    public enum TrueNorthStrategy { Automatic, ASAP, Rear, Flank, Force, Delay }
    #endregion

    //Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition(
            "Akechi DRG", //Name
            "Standard Rotation Module", //Type
            "Standard rotation (Akechi)", //Category
            "Akechi", //Contributor of module
            RotationModuleQuality.Good, //Quality
            BitMask.Build(Class.LNC, Class.DRG), //Class and Job
            100); //Max Level supported

        #region Custom Strategies
        res.Define(Track.AOE).As<AOEStrategy>("Combo Option", "AOE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoTargetHitPrimary, "AutoTargetHitPrimary", "Use AOE actions if profitable, select best target that ensures primary target is hit", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoTargetHitMost, "AutoTargetHitMost", "Use AOE actions if profitable, select a target that ensures maximal number of targets are hit", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceST, "Force ST", "Force Single-Target rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.Force123ST, "Only 1-2-3 ST", "Force only ST 1-2-3 rotation (No Buff or DoT)", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceBuffsST, "Only 1-4-5 ST", "Force only ST 1-4-5 rotation (Buff & DoT only)", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "Force AOE", "Force AOE rotation, even if less than 3 targets");
        res.Define(Track.Hold).As<HoldStrategy>("Cooldowns", "CDs", uiPriority: 190)
            .AddOption(HoldStrategy.Allow, "Allow", "Forbid use of all cooldowns & buffs")
            .AddOption(HoldStrategy.Forbid, "Forbid", "Forbid use of all cooldowns & buffs");
        res.Define(Track.Dives).As<DivesStrategy>("Dives", uiPriority: 185)
            .AddOption(DivesStrategy.AllowMaxMelee, "Allow Max Melee", "Allow Jump, Stardiver, & Dragonfire Dive only at max melee range (within 3y)")
            .AddOption(DivesStrategy.AllowCloseMelee, "Allow Close Melee", "Allow Jump, Stardiver, & Dragonfire Dive only at close melee range (within 1y)")
            .AddOption(DivesStrategy.Allow, "Allow", "Allow the use of Jump, Stardiver, & Dragonfire Dive at any distance")
            .AddOption(DivesStrategy.Forbid, "Forbid", "Forbid the use of Jump, Stardiver, & Dragonfire Dive")
            .AddAssociatedActions(AID.Jump, AID.HighJump, AID.DragonfireDive, AID.Stardiver);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 20)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use potions automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Align With Raid Buffs", "Use potion in sync with 2-minute raid buffs (e.g., 0/6, 2/8)")
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potion as soon as possible, regardless of any buffs")
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.LifeSurge).As<SurgeStrategy>("Life Surge", "L. Surge", uiPriority: 160)
            .AddOption(SurgeStrategy.Automatic, "Automatic", "Use Life Surge normally")
            .AddOption(SurgeStrategy.Force, "Force", "Force Life Surge usage", 40, 5, ActionTargets.Hostile, 6)
            .AddOption(SurgeStrategy.ForceWeave, "Force Weave", "Force Life Surge usage inside the next possible weave window", 40, 5, ActionTargets.Hostile, 6)
            .AddOption(SurgeStrategy.ForceNextOpti, "Force Optimally", "Force Life Surge usage in next possible optimal window", 40, 5, ActionTargets.Hostile, 6)
            .AddOption(SurgeStrategy.ForceNextOptiWeave, "Force Weave Optimally", "Force Life Surge optimally inside the next possible weave window", 40, 5, ActionTargets.Hostile, 6)
            .AddOption(SurgeStrategy.Delay, "Delay", "Delay the use of Life Surge", 0, 0, ActionTargets.None, 6)
            .AddAssociatedActions(AID.LifeSurge);
        res.Define(Track.Jump).As<JumpStrategy>("Jump", uiPriority: 110)
            .AddOption(JumpStrategy.Automatic, "Automatic", "Use Jump normally")
            .AddOption(JumpStrategy.Force, "Force Jump", "Force Jump usage", 30, 0, ActionTargets.Self, 30, 67)
            .AddOption(JumpStrategy.ForceEX, "Force Jump (EX)", "Force Jump usage (Grants Dive Ready buff)", 30, 15, ActionTargets.Self, 68, 74)
            .AddOption(JumpStrategy.ForceEX2, "Force High Jump", "Force High Jump usage", 30, 15, ActionTargets.Self, 75)
            .AddOption(JumpStrategy.ForceWeave, "Force Weave", "Force Jump usage inside the next possible weave window", 30, 15, ActionTargets.Hostile, 68)
            .AddOption(JumpStrategy.Delay, "Delay", "Delay Jump usage", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.Jump, AID.HighJump);
        res.Define(Track.DragonfireDive).As<DragonfireStrategy>("Dragonfire Dive", "D.Dive", uiPriority: 150)
            .AddOption(DragonfireStrategy.Automatic, "Automatic", "Use Dragonfire Dive normally")
            .AddOption(DragonfireStrategy.Force, "Force", "Force Dragonfire Dive usage", 120, 0, ActionTargets.Hostile, 50, 91)
            .AddOption(DragonfireStrategy.ForceEX, "ForceEX", "Force Dragonfire Dive (Grants Dragon's Flight)", 120, 30, ActionTargets.Hostile, 92)
            .AddOption(DragonfireStrategy.ForceWeave, "Force Weave", "Force Dragonfire Dive usage inside the next possible weave window", 120, 0, ActionTargets.Hostile, 68)
            .AddOption(DragonfireStrategy.Delay, "Delay", "Delay Dragonfire Dive usage", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(AID.DragonfireDive);
        res.Define(Track.Geirskogul).As<GeirskogulStrategy>("Geirskogul", "Geirs.", uiPriority: 130)
            .AddOption(GeirskogulStrategy.Automatic, "Automatic", "Use Geirskogul normally")
            .AddOption(GeirskogulStrategy.Force, "Force", "Force Geirskogul usage", 60, 0, ActionTargets.Hostile, 60, 69)
            .AddOption(GeirskogulStrategy.ForceEX, "ForceEX", "Force Geirskogul (Grants Life of the Dragon & Nastrond Ready)", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(GeirskogulStrategy.ForceWeave, "Force Weave", "Force Geirskogul usage inside the next possible weave window", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(GeirskogulStrategy.Delay, "Delay", "Delay Geirskogul usage", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Geirskogul);
        res.Define(Track.Stardiver).As<StardiverStrategy>("Stardiver", "S.diver", uiPriority: 140)
            .AddOption(StardiverStrategy.Automatic, "Automatic", "Use Stardiver normally")
            .AddOption(StardiverStrategy.Force, "Force", "Force Stardiver usage", 30, 0, ActionTargets.Hostile, 80, 99)
            .AddOption(StardiverStrategy.ForceEX, "ForceEX", "Force Stardiver (Grants Starcross Ready)", 30, 0, ActionTargets.Hostile, 100)
            .AddOption(StardiverStrategy.ForceWeave, "Force Weave", "Force Stardiver usage inside the next possible weave window", 30, 0, ActionTargets.Hostile, 80)
            .AddOption(StardiverStrategy.Delay, "Delay", "Delay Stardiver usage", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Stardiver);        //Piercing Talon strategy
        res.Define(Track.PiercingTalon).As<PiercingTalonStrategy>("Piercing Talon", "P.Talon", uiPriority: 25)
            .AddOption(PiercingTalonStrategy.AllowEX, "AllowEX", "Allow use of Piercing Talon if already in combat, outside melee range, & is Enhanced")
            .AddOption(PiercingTalonStrategy.Allow, "Allow", "Allow use of Piercing Talon if already in combat & outside melee range")
            .AddOption(PiercingTalonStrategy.Force, "Force", "Force Piercing Talon usage ASAP (even in melee range)")
            .AddOption(PiercingTalonStrategy.ForceEX, "ForceEX", "Force Piercing Talon usage ASAP when Enhanced")
            .AddOption(PiercingTalonStrategy.Forbid, "Forbid", "Forbid use of Piercing Talon")
            .AddAssociatedActions(AID.PiercingTalon);
        res.Define(Track.TrueNorth).As<TrueNorthStrategy>("True North", "T.North", uiPriority: 10)
            .AddOption(TrueNorthStrategy.Automatic, "Automatic", "Late-weaves True North when out of positional")
            .AddOption(TrueNorthStrategy.ASAP, "ASAP", "Use True North as soon as possible when out of positional", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Rear, "Rear", "Use True North for rear positional only", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Flank, "Flank", "Use True North for flank positional only", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Force, "Force", "Force True North usage", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Delay, "Delay", "Delay True North usage", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(ClassShared.AID.TrueNorth);
        #endregion

        #region Offensive Strategies
        res.DefineOGCD(Track.LanceCharge, AID.LanceCharge, "Lance Charge", "L.Charge", uiPriority: 165, 60, 20, ActionTargets.Self, 30);
        res.DefineOGCD(Track.BattleLitany, AID.BattleLitany, "Battle Litany", "B.Litany", uiPriority: 165, 120, 20, ActionTargets.Self, 52);
        res.DefineOGCD(Track.MirageDive, AID.MirageDive, "Mirage Dive", "M.Dive", uiPriority: 105, 0, 0, ActionTargets.Hostile, 68);
        res.DefineOGCD(Track.Nastrond, AID.Nastrond, "Nastrond", "Nast.", uiPriority: 125, 0, 0, ActionTargets.Hostile, 70);
        res.DefineOGCD(Track.WyrmwindThrust, AID.WyrmwindThrust, "Wyrmwind Thrust", "W.Thrust", uiPriority: 120, 0, 10, ActionTargets.Hostile, 90);
        res.DefineOGCD(Track.RiseOfTheDragon, AID.RiseOfTheDragon, "Rise Of The Dragon", "RotD", uiPriority: 145, 0, 0, ActionTargets.Hostile, 92);
        res.DefineOGCD(Track.Starcross, AID.Starcross, "Starcross", "S.cross", uiPriority: 135, 0, 0, ActionTargets.Hostile, 100);
        #endregion

        return res;
    }

    #region Priorities

    public enum GCDPriority //Priorities for Global Cooldowns (GCDs)

    {
        None = 0,               //No priority
        Combo123 = 350,         //Priority for the first three combo actions
        NormalGCD = 500,        //Standard priority for normal GCD actions
        ForcedGCD = 900,        //High priority for forced GCD actions
    }

    public enum OGCDPriority //Priorities for Off Global Cooldowns (oGCDs)

    {
        None = 0,                  //No priority
        //Flexible actions with varying priorities
        MirageDive = 500,          //Priority for Mirage Dive
        Nastrond = 540,            //Priority for Nastrond
        Stardiver = 550,           //Priority for Stardiver
        RiseOfTheDragon = 560,     //Priority for Rise of the Dragon
        Starcross = 560,           //Priority for Starcross
        WyrmwindThrust = 570,      //Priority for Wyrmwind Thrust (normal)
        TrueNorth = 580,           //Priority for True North
        //Non-flexible actions with fixed priorities
        Jump = 660,                //Priority for Jump
        WyrmwindThrustOpti = 670,  //Priority for Wyrmwind Thrust (optimal)
        DragonfireDive = 680,      //Priority for Dragonfire Dive
        Geirskogul = 700,          //Priority for Geirskogul
        Buffs = 800,               //Priority for buffs
        ForcedOGCD = 900,          //High priority for forced oGCD actions
    }

    #endregion

    #region Module Variables
    private bool hasLOTD; //Flag for Life of the Dragon status
    private bool hasLC; //Flag for Lance Charge status
    private bool hasBL; //Flag for Battle Litany status
    private bool hasMD; //Flag for Mirage Dive status
    private bool hasDF; //Flag for Dragon's Flight status
    private bool hasSC; //Flag for Starcross status
    private bool hasNastrond; //Flag for Nastrond status
    private bool canLC; //Ability to use Lance Charge
    private bool canBL; //Ability to use Battle Litany
    private bool canLS; //Ability to use Life Surge
    private bool canJump; //Ability to use Jump
    private bool canDD; //Ability to use Dragonfire Dive
    private bool canGeirskogul; //Ability to use Geirskogul
    private bool canMD; //Ability to use Mirage Dive
    private bool canNastrond; //Ability to use Nastrond
    private bool canSD; //Ability to use Stardiver
    private bool canWT; //Ability to use Wyrmwind Thrust
    private bool canROTD; //Ability to use Rise of the Dragon
    private bool canSC; //Ability to use Starcross
    private float GCDLength; //Length of the global cooldown
    private float blCD; //Cooldown for Battle Litany
    private float lcLeft; //Time remaining for Lance Charge
    private float lcCD; //Cooldown for Lance Charge
    private float powerLeft; //Time remaining for Power Surge
    private float chaosLeft; //Remaining time for Chaotic Spring DoT
    public float downtimeIn; //Duration of downtime in combat
    private int focusCount; //Count of Firstmind's Focus gauge
    public int NumAOETargets;
    public int NumSpearTargets;
    public int NumDiveTargets;
    private Enemy? BestAOETargets;
    private Enemy? BestSpearTargets;
    private Enemy? BestDiveTargets;
    private Enemy? BestAOETarget;
    private Enemy? BestSpearTarget;
    private Enemy? BestDiveTarget;

    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables

        #region Cooldown-related
        //Gauge Management
        var gauge = World.Client.GetGauge<DragoonGauge>();  //Retrieve Dragoon gauge data
        focusCount = gauge.FirstmindsFocusCount;  //Update focus count from the gauge
        hasLOTD = gauge.LotdTimer > 0;  //Check if Life of the Dragon (LOTD) is active
        //Cooldown Checks
        lcCD = TotalCD(AID.LanceCharge);  //Get cooldown for Lance Charge
        lcLeft = SelfStatusLeft(SID.LanceCharge, 20);  //Get remaining time for Lance Charge effect
        powerLeft = SelfStatusLeft(SID.PowerSurge, 30);  //Get remaining time for Power Surge effect
        chaosLeft = MathF.Max(StatusDetails(primaryTarget?.Actor, SID.ChaosThrust, Player.InstanceID).Left,
                              StatusDetails(primaryTarget?.Actor, SID.ChaoticSpring, Player.InstanceID).Left);  //Get max remaining time for Chaos Thrust or Chaotic Spring
        blCD = TotalCD(AID.BattleLitany);  //Get cooldown for Battle Litany
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);  //Calculate rounded global cooldown (GCD) duration
        hasMD = PlayerHasEffect(SID.DiveReady);  //Check if Mirage Dive is ready
        hasNastrond = PlayerHasEffect(SID.NastrondReady);  //Check if Nastrond is ready
        hasLC = lcCD is >= 40 and <= 60;  //Check if Lance Charge is within cooldown range
        hasBL = blCD is >= 100 and <= 120;  //Check if Battle Litany is within cooldown range
        hasDF = PlayerHasEffect(SID.DragonsFlight);  //Check if Dragon's Flight effect is active
        hasSC = PlayerHasEffect(SID.StarcrossReady);  //Check if Starcross is ready
        //Minimum Conditions
        canLC = Unlocked(AID.LanceCharge) && ActionReady(AID.LanceCharge);  //minimum condition(s) to execute Lance Charge
        canBL = Unlocked(AID.BattleLitany) && ActionReady(AID.BattleLitany);  //minimum condition(s) to execute Battle Litany
        canLS = Unlocked(AID.LifeSurge);  //minimum condition(s) to execute Life Surge
        canJump = Unlocked(AID.Jump) && ActionReady(AID.Jump);  //minimum condition(s) to execute Jump
        canDD = Unlocked(AID.DragonfireDive) && ActionReady(AID.DragonfireDive);  //minimum condition(s) to execute Dragonfire Dive
        canGeirskogul = Unlocked(AID.Geirskogul) && ActionReady(AID.Geirskogul);  //minimum condition(s) to execute Geirskogul
        canMD = Unlocked(AID.MirageDive) && hasMD;  //minimum condition(s) to execute Mirage Dive
        canNastrond = Unlocked(AID.Nastrond) && hasNastrond;  //minimum condition(s) to execute Nastrond
        canSD = Unlocked(AID.Stardiver) && ActionReady(AID.Stardiver);  //minimum condition(s) to execute Stardiver
        canWT = Unlocked(AID.WyrmwindThrust) && ActionReady(AID.WyrmwindThrust) && focusCount == 2;  //minimum condition(s) to execute Wyrmwind Thrust
        canROTD = Unlocked(AID.RiseOfTheDragon) && hasDF;  //minimum condition(s) to execute Rise of the Dragon
        canSC = Unlocked(AID.Starcross) && hasSC;  //minimum condition(s) to execute Starcross
        #endregion

        #region Miscellaneous
        var hold = strategy.Option(Track.Hold).As<HoldStrategy>() == HoldStrategy.Forbid;  //Check if the Cooldowns should be held or delayed
        (BestAOETargets, NumAOETargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        (BestSpearTargets, NumSpearTargets) = GetBestTarget(primaryTarget, 15, Is15yRectTarget);
        (BestDiveTargets, NumDiveTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        BestAOETarget = Unlocked(AID.DoomSpike) && NumAOETargets > 2 ? BestAOETargets : primaryTarget;
        BestSpearTarget = Unlocked(AID.Geirskogul) && NumSpearTargets > 1 ? BestSpearTargets : primaryTarget;
        BestDiveTarget = Unlocked(AID.Stardiver) && NumDiveTargets > 1 ? BestDiveTargets : primaryTarget;
        #endregion

        #endregion

        #region AOEStrategy 'Force' Execution
        var AOE = strategy.Option(Track.AOE);
        var AOEStrategy = AOE.As<AOEStrategy>();  //Retrieve the current AOE strategy

        //Force specific actions based on the AOE strategy selected
        if (AOEStrategy == AOEStrategy.ForceST)  //if forced single target
            QueueGCD(NextFullST(), TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ForcedGCD);  //Queue the next single target action
        if (AOEStrategy == AOEStrategy.Force123ST)  //if forced 123 combo
            QueueGCD(UseOnly123ST(), TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ForcedGCD);  //Queue the 123 combo action
        if (AOEStrategy == AOEStrategy.ForceBuffsST)  //if forced buffs combo
            QueueGCD(UseOnly145ST(), TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ForcedGCD);  //Queue the buffed 145 combo action
        if (AOEStrategy == AOEStrategy.ForceAOE)  //if forced AOE action
            QueueGCD(NextFullAOE(), TargetChoice(AOE) ?? (NumAOETargets > 1 ? BestAOETargets?.Actor : primaryTarget?.Actor), GCDPriority.ForcedGCD);  //Queue the next AOE action
        #endregion

        #region Dives Strategy
        //Dive strategy
        var dive = strategy.Option(Track.Dives).As<DivesStrategy>();  //Retrieve the current dive strategy
        var diveStrategy = dive switch
        {
            DivesStrategy.AllowMaxMelee => In3y(primaryTarget?.Actor), //Only allow max melee if target is within 3 yalms
            DivesStrategy.AllowCloseMelee => In0y(primaryTarget?.Actor), //Only allow close melee if target is within 1 yalm
            DivesStrategy.Allow => true, //Always allow dives
            DivesStrategy.Forbid => false, //Never allow dives
            _ => false,
        };
        var maxMelee = dive == DivesStrategy.AllowMaxMelee; //Check if max melee is allowed
        var closeMelee = dive == DivesStrategy.AllowCloseMelee; //Check if close melee is allowed
        var allowed = dive == DivesStrategy.Allow; //Check if dives are allowed
        var forbidden = dive == DivesStrategy.Forbid; //Check if dives are forbidden
        var divesGood = diveStrategy && (maxMelee || closeMelee || allowed) && !forbidden; //Check if dives are good to use
        #endregion

        #region Combo & Cooldown Execution
        //Combo Action evecution
        QueueGCD(NumAOETargets > 2 ? NextFullAOE() : NextFullST(),
            BestAOETarget?.Actor, //on best AOE target
            GCDPriority.Combo123); //set priority to Combo 123
        //Execute Lance Charge if available
        var lcStrat = strategy.Option(Track.LanceCharge).As<OGCDStrategy>(); //Retrieve the Lance Charge strategy
        if (!hold && //if not holding Cooldowns
            ShouldUseLanceCharge(lcStrat, primaryTarget?.Actor)) //if Lance Charge should be used
            QueueOGCD(AID.LanceCharge, //Queue Lance Charge
                Player, //on Self
                lcStrat is OGCDStrategy.Force //if strategy is Force
                or OGCDStrategy.AnyWeave //or Any Weave
                or OGCDStrategy.EarlyWeave //or Early Weave
                or OGCDStrategy.LateWeave //or Late Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Buffs); //otherwise, set priority to Buffs

        //Execute Battle Litany if available
        var blStrat = strategy.Option(Track.BattleLitany).As<OGCDStrategy>(); //Retrieve the Battle Litany strategy
        if (!hold && //if not holding Cooldowns
            ShouldUseBattleLitany(blStrat, primaryTarget?.Actor)) //if Battle Litany should be used
            QueueOGCD(AID.BattleLitany, //Queue Battle Litany
                Player, //on Self
                blStrat is OGCDStrategy.Force //if strategy is Force
                or OGCDStrategy.AnyWeave //or Any Weave
                or OGCDStrategy.EarlyWeave //or Early Weave
                or OGCDStrategy.LateWeave //or Late Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Buffs); //otherwise, set priority to Buffs

        //Execute Life Surge if conditions met
        var lsStrat = strategy.Option(Track.LifeSurge).As<SurgeStrategy>(); //Retrieve the Life Surge strategy
        if (!hold && //if not holding Cooldowns
            ShouldUseLifeSurge(lsStrat, primaryTarget?.Actor)) //if Life Surge should be used
            QueueOGCD(AID.LifeSurge, //Queue Life Surge
                Player, //on Self
                lsStrat is SurgeStrategy.Force //if strategy is Force
                or SurgeStrategy.ForceWeave //or Force Weave
                or SurgeStrategy.ForceNextOpti //or Force Next Optimal Window
                or SurgeStrategy.ForceNextOptiWeave //or Force Next Optimal Weave Window
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Buffs); //otherwise, set priority to Buffs

        //Execute Jump ability if available
        var jump = strategy.Option(Track.Jump); //Retrieve the Jump track
        var jumpStrat = jump.As<JumpStrategy>(); //Retrieve the Jump strategy
        if (!hold && divesGood && //if not holding Cooldowns and dives are good
            ShouldUseJump(jumpStrat, primaryTarget?.Actor)) //if Jump should be used
            QueueOGCD(Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump, //Queue High Jump if unlocked, otherwise Jump
                TargetChoice(jump) //Check target choice
                ?? primaryTarget?.Actor, //if none, choose the primary target
                jumpStrat is JumpStrategy.Force //if strategy is Force
                or JumpStrategy.ForceEX //or Force EX
                or JumpStrategy.ForceEX2 //or Force EX2
                or JumpStrategy.ForceWeave //or Force Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Jump); //otherwise, set priority to Jump

        //Execute Dragonfire Dive if available
        var dd = strategy.Option(Track.DragonfireDive); //Retrieve the Dragonfire Dive track
        var ddStrat = dd.As<DragonfireStrategy>(); //Retrieve the Dragonfire Dive strategy
        if (!hold && divesGood && //if not holding Cooldowns and dives are good
            ShouldUseDragonfireDive(ddStrat, primaryTarget?.Actor)) //if Dragonfire Dive should be used
            QueueOGCD(AID.DragonfireDive, //Queue Dragonfire Dive
                TargetChoice(dd) //Check target choice
                ?? BestDiveTarget?.Actor, //if none, choose the primary target
                ddStrat is DragonfireStrategy.Force //if strategy is Force
                or DragonfireStrategy.ForceWeave //or Force Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.DragonfireDive); //otherwise, set priority to Dragonfire Dive

        //Execute Geirskogul if available
        var geirskogul = strategy.Option(Track.Geirskogul); //Retrieve the Geirskogul track
        var geirskogulStrat = geirskogul.As<GeirskogulStrategy>(); //Retrieve the Geirskogul strategy
        if (!hold && //if not holding Cooldowns
            ShouldUseGeirskogul(geirskogulStrat, primaryTarget?.Actor)) //if Geirskogul should be used
            QueueOGCD(AID.Geirskogul, //Queue Geirskogul
                TargetChoice(geirskogul) //Check target choice
                ?? BestSpearTarget?.Actor, //if none, choose the best spear target
                geirskogulStrat is GeirskogulStrategy.Force //if strategy is Force
                or GeirskogulStrategy.ForceEX //or Force EX
                or GeirskogulStrategy.ForceWeave //or Force Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Geirskogul); //otherwise, set priority to Geirskogul

        //Execute Mirage Dive if available
        var mirage = strategy.Option(Track.MirageDive); //Retrieve the Mirage Dive track
        var mirageStrat = mirage.As<OGCDStrategy>(); //Retrieve the Mirage Dive strategy
        if (!hold && //if not holding Cooldowns
            ShouldUseMirageDive(mirageStrat, primaryTarget?.Actor)) //if Mirage Dive should be used
            QueueOGCD(AID.MirageDive, //Queue Mirage Dive
                TargetChoice(mirage) //Check target choice
                ?? primaryTarget?.Actor, //if none, choose the primary target
                mirageStrat is OGCDStrategy.Force //if strategy is Force
                or OGCDStrategy.AnyWeave //or Any Weave
                or OGCDStrategy.EarlyWeave //or Early Weave
                or OGCDStrategy.LateWeave //or Late Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.MirageDive); //otherwise, set priority to Mirage Dive

        //Execute Nastrond if available
        var nastrond = strategy.Option(Track.Nastrond); //Retrieve the Nastrond track
        var nastrondStrat = nastrond.As<OGCDStrategy>(); //Retrieve the Nastrond strategy
        if (!hold && //if not holding Cooldowns
            ShouldUseNastrond(nastrondStrat, primaryTarget?.Actor)) //if Nastrond should be used
            QueueOGCD(AID.Nastrond, //Queue Nastrond
                TargetChoice(nastrond) //Check target choice
                ?? BestSpearTarget?.Actor, //if none, choose the best spear target
                nastrondStrat is OGCDStrategy.Force //if strategy is Force
                or OGCDStrategy.AnyWeave //or Any Weave
                or OGCDStrategy.EarlyWeave //or Early Weave
                or OGCDStrategy.LateWeave //or Late Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Nastrond); //otherwise, set priority to Nastrond

        //Execute Stardiver if available
        var sd = strategy.Option(Track.Stardiver); //Retrieve the Stardiver track
        var sdStrat = sd.As<StardiverStrategy>(); //Retrieve the Stardiver strategy
        if (!hold && divesGood && //if not holding Cooldowns and dives are good
            ShouldUseStardiver(sdStrat, primaryTarget?.Actor)) //if Stardiver should be used
            QueueOGCD(AID.Stardiver, //Queue Stardiver
                TargetChoice(sd) //Check target choice
                ?? BestDiveTarget?.Actor, //if none, choose the primary target
                sdStrat is StardiverStrategy.Force //on the primary target
                or StardiverStrategy.ForceEX //or Force EX
                or StardiverStrategy.ForceWeave //or Force Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Stardiver); //otherwise, set priority to Stardiver

        //Execute Wyrmwind Thrust if available
        var wt = strategy.Option(Track.WyrmwindThrust); //Retrieve the Wyrmwind Thrust track
        var wtStrat = wt.As<OGCDStrategy>(); //Retrieve the Wyrmwind Thrust strategy
        if (!hold && //if not holding Cooldowns
            ShouldUseWyrmwindThrust(wtStrat, primaryTarget?.Actor)) //if Wyrmwind Thrust should be used
            QueueOGCD(AID.WyrmwindThrust, //Queue Wyrmwind Thrust
                TargetChoice(wt) //Check target choice
                ?? BestSpearTarget?.Actor, //if none, choose the best spear target
                wtStrat is OGCDStrategy.Force //if strategy is Force
                or OGCDStrategy.AnyWeave //or Any Weave
                or OGCDStrategy.EarlyWeave //or Early Weave
                or OGCDStrategy.LateWeave //or Late Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : PlayerHasEffect(SID.LanceCharge) //if Lance Charge is active
                ? OGCDPriority.WyrmwindThrustOpti //set priority to Optimal Wyrmwind Thrust
                : OGCDPriority.WyrmwindThrust); //otherwise, set priority to Wyrmwind Thrust

        //Execute Rise of the Dragon if available
        var rise = strategy.Option(Track.RiseOfTheDragon); //Retrieve the Rise of the Dragon track
        var riseStrat = rise.As<OGCDStrategy>(); //Retrieve the Rise of the Dragon strategy
        if (ShouldUseRiseOfTheDragon(riseStrat, primaryTarget?.Actor)) //if Rise of the Dragon should be used
            QueueOGCD(AID.RiseOfTheDragon, //Queue Rise of the Dragon
                TargetChoice(rise) //Check target choice
                ?? BestDiveTarget?.Actor, //if none, choose the primary target
                riseStrat is OGCDStrategy.Force //if strategy is Force
                or OGCDStrategy.AnyWeave //or Any Weave
                or OGCDStrategy.EarlyWeave //or Early Weave
                or OGCDStrategy.LateWeave //or Late Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Buffs); //otherwise, set priority to Buffs

        //Execute Starcross if available
        var cross = strategy.Option(Track.Starcross); //Retrieve the Starcross track
        var crossStrat = cross.As<OGCDStrategy>(); //Retrieve the Starcross strategy
        if (ShouldUseStarcross(crossStrat, primaryTarget?.Actor)) //if Starcross should be used
            QueueOGCD(AID.Starcross, //Queue Starcross
                TargetChoice(cross) //Check target choice
                ?? BestDiveTarget?.Actor, //if none, choose the best Dive target
                crossStrat is OGCDStrategy.Force //if strategy is Force
                or OGCDStrategy.AnyWeave //or Any Weave
                or OGCDStrategy.EarlyWeave //or Early Weave
                or OGCDStrategy.LateWeave //or Late Weave
                ? OGCDPriority.ForcedOGCD //set priority to Forced oGCD
                : OGCDPriority.Starcross); //otherwise, set priority to Starcross

        //Execute Piercing Talon if available
        var pt = strategy.Option(Track.PiercingTalon); //Retrieve the Piercing Talon track
        var ptStrat = pt.As<PiercingTalonStrategy>(); //Retrieve the Piercing Talon strategy
        if (ShouldUsePiercingTalon(primaryTarget?.Actor, ptStrat)) //if Piercing Talon should be used
            QueueGCD(AID.PiercingTalon, //Queue Piercing Talon
                TargetChoice(pt) //Check target choice
                ?? primaryTarget?.Actor, //if none, choose the primary target
                ptStrat is PiercingTalonStrategy.Force or //if strategy is Force
                PiercingTalonStrategy.ForceEX //or Force EX
                ? GCDPriority.ForcedGCD //set priority to Forced GCD
                : GCDPriority.NormalGCD); //otherwise, set priority to Normal GCD

        //Execute Potion if available
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>())) //if Potion should be used
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, //Queue Potion
                Player, //on Self
                ActionQueue.Priority.VeryHigh + (int)OGCDPriority.ForcedOGCD, 0, GCD - 0.9f); //set priority to Forced oGCD

        //Execute True North if available
        if (!hold && //if not holding Cooldowns
            ShouldUseTrueNorth(strategy.Option(Track.TrueNorth).As<TrueNorthStrategy>(), primaryTarget?.Actor)) //if True North should be used
            QueueOGCD(AID.TrueNorth, //Queue True North
                Player, //on Self
                OGCDPriority.TrueNorth); //set priority to True North
        #endregion

        #region AI
        //AI hints for positioning
        var goalST = primaryTarget?.Actor != null ? Hints.GoalSingleTarget(primaryTarget!.Actor, 3) : null; //Set goal for single target
        var goalAOE = primaryTarget?.Actor != null ? Hints.GoalAOECone(primaryTarget!.Actor, 10, 45.Degrees()) : null; //Set goal for AOE
        var goal = AOEStrategy switch //Set goal based on AOE strategy
        {
            AOEStrategy.ForceST => goalST, //if forced single target
            AOEStrategy.Force123ST => goalST, //if forced 123 combo
            AOEStrategy.ForceBuffsST => goalST, //if forced buffs combo
            AOEStrategy.ForceAOE => goalAOE, //if forced AOE action
            _ => goalST != null && goalAOE != null ? Hints.GoalCombined(goalST, goalAOE, 2) : goalAOE //otherwise, combine goals
        };
        if (goal != null) //if goal is set
            Hints.GoalZones.Add(goal); //add goal to zones
        #endregion
    }

    #region Single-Target Helpers

    //Determines the next skill in the single-target (ST) combo chain based on the last used action.
    private AID NextFullST() => ComboLastMove switch
    {
        //Starting combo with TrueThrust or RaidenThrust
        AID.TrueThrust or AID.RaidenThrust =>
            //if Disembowel is Unlocked and power is low or Chaotic Spring is 0, use Disembowel or SpiralBlow, else VorpalThrust or LanceBarrage
            Unlocked(AID.Disembowel) && (powerLeft <= GCDLength * 6 || chaosLeft <= GCDLength * 4)
            ? Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : AID.Disembowel
            : Unlocked(AID.LanceBarrage) ? AID.LanceBarrage : AID.VorpalThrust,

        //Follow-up after Disembowel or SpiralBlow
        AID.Disembowel or AID.SpiralBlow =>
            Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring //Use ChaoticSpring if Unlocked
            : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust //Use ChaosThrust if Unlocked
            : AID.TrueThrust, //Return to TrueThrust otherwise

        //Follow-up after VorpalThrust or LanceBarrage
        AID.VorpalThrust or AID.LanceBarrage =>
            Unlocked(AID.HeavensThrust) ? AID.HeavensThrust //Use HeavensThrust if Unlocked
            : Unlocked(AID.FullThrust) ? AID.FullThrust //Use FullThrust if Unlocked
            : AID.TrueThrust, //Return to TrueThrust otherwise

        //After FullThrust or HeavensThrust in the combo
        AID.FullThrust or AID.HeavensThrust =>
            Unlocked(AID.FangAndClaw) ? AID.FangAndClaw //Use FangAndClaw if Unlocked
            : AID.TrueThrust, //Return to TrueThrust otherwise

        //After ChaosThrust or ChaoticSpring in the combo
        AID.ChaosThrust or AID.ChaoticSpring =>
            Unlocked(AID.WheelingThrust) ? AID.WheelingThrust //Use WheelingThrust if Unlocked
            : AID.TrueThrust, //Return to TrueThrust otherwise

        //After WheelingThrust or FangAndClaw in the combo
        AID.WheelingThrust or AID.FangAndClaw =>
            Unlocked(AID.Drakesbane) ? AID.Drakesbane //Use Drakesbane if Unlocked
            : AID.TrueThrust, //Return to TrueThrust otherwise

        //if no combo active and Draconian Fire buff is up, use RaidenThrust
        _ => PlayerHasEffect(SID.DraconianFire) ? AID.RaidenThrust //RaidenThrust if DraconianFire is active
            : AID.TrueThrust, //No combo, start with TrueThrust
    };

    //Limits the combo sequence to just 1-2-3 ST skills, ignoring other Unlocked actions.
    private AID UseOnly123ST() => ComboLastMove switch
    {
        //Start combo with TrueThrust
        AID.TrueThrust or AID.RaidenThrust =>
            Unlocked(AID.LanceBarrage) ? AID.LanceBarrage //LanceBarrage if Unlocked
            : Unlocked(AID.VorpalThrust) ? AID.VorpalThrust //VorpalThrust otherwise
            : AID.TrueThrust, //Else return to TrueThrust

        //After VorpalThrust or LanceBarrage
        AID.VorpalThrust or AID.LanceBarrage =>
            Unlocked(AID.HeavensThrust) ? AID.HeavensThrust //HeavensThrust if Unlocked
            : Unlocked(AID.FullThrust) ? AID.FullThrust //FullThrust to end combo
            : AID.TrueThrust, //Else return to TrueThrust

        //After FullThrust or HeavensThrust
        AID.FullThrust or AID.HeavensThrust =>
            Unlocked(AID.FangAndClaw) ? AID.FangAndClaw //FangAndClaw if Unlocked
            : AID.TrueThrust, //Else return to TrueThrust

        //After WheelingThrust or FangAndClaw
        AID.WheelingThrust or AID.FangAndClaw =>
            Unlocked(AID.Drakesbane) ? AID.Drakesbane //Drakesbane if Unlocked
            : AID.TrueThrust, //Else return to TrueThrust

        //if Draconian Fire buff is up, use RaidenThrust
        _ => PlayerHasEffect(SID.DraconianFire) ? AID.RaidenThrust //RaidenThrust if DraconianFire is active
            : AID.TrueThrust, //No combo, start with TrueThrust
    };

    //Limits the combo sequence to 1-4-5 ST skills, focusing on Disembowel and Chaos/ChaoticSpring.
    private AID UseOnly145ST() => ComboLastMove switch
    {
        //Start combo with TrueThrust
        AID.TrueThrust or AID.RaidenThrust =>
            Unlocked(AID.Disembowel)
            ? Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : AID.Disembowel //Disembowel/SpiralBlow if Unlocked
            : AID.TrueThrust, //Else return to TrueThrust

        //After Disembowel or SpiralBlow
        AID.Disembowel or AID.SpiralBlow =>
            Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring //ChaoticSpring if Unlocked
            : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust //ChaosThrust if Unlocked
            : AID.TrueThrust, //Else return to TrueThrust

        //After ChaosThrust or ChaoticSpring
        AID.ChaosThrust or AID.ChaoticSpring =>
            Unlocked(AID.WheelingThrust) ? AID.WheelingThrust  //WheelingThrust if Unlocked
            : AID.TrueThrust, //Else return to TrueThrust

        //After WheelingThrust or FangAndClaw
        AID.WheelingThrust or AID.FangAndClaw =>
            Unlocked(AID.Drakesbane) ? AID.Drakesbane //Drakesbane if Unlocked
            : AID.TrueThrust, //Else return to TrueThrust

        //if Draconian Fire buff is up, use RaidenThrust
        _ => PlayerHasEffect(SID.DraconianFire) ? AID.RaidenThrust //RaidenThrust if DraconianFire is active
            : AID.TrueThrust, //No combo, start with TrueThrust
    };

    #endregion

    #region AOE Helpers

    //Determines the next action in the AOE combo based on the last action used.
    private AID NextFullAOE() => ComboLastMove switch
    {
        //Start AOE combo with DoomSpike
        AID.DoomSpike =>
            Unlocked(AID.SonicThrust) ? AID.SonicThrust : AID.DoomSpike,  //SonicThrust if Unlocked, else DoomSpike

        //Continue AOE combo with SonicThrust
        AID.SonicThrust =>
            Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : AID.DoomSpike,  //CoerthanTorment if Unlocked, else DoomSpike

        //if Draconian Fire buff is up, use DraconianFury
        _ => PlayerHasEffect(SID.DraconianFire)
            ? Unlocked(AID.DraconianFury) ? AID.DraconianFury : AID.DoomSpike  //DraconianFury if Unlocked, else DoomSpike
            : AID.DoomSpike,  //No DraconianFire active, default to DoomSpike
    };

    #endregion

    #region Cooldown Helpers

    #region Buffs
    //Determines when to use Lance Charge
    private bool ShouldUseLanceCharge(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //if in combat
            target != null && //if has target
            canLC && //if Lance Charge is ready
            powerLeft > 0, //if Power Surge is active
        OGCDStrategy.Force => canLC, //Always use if forced
        OGCDStrategy.AnyWeave => canLC && CanWeaveIn, //Always use if any weave
        OGCDStrategy.EarlyWeave => canLC && CanEarlyWeaveIn, //Always use if early weave
        OGCDStrategy.LateWeave => canLC && CanLateWeaveIn, //Always use if late weave
        OGCDStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Battle Litany
    private bool ShouldUseBattleLitany(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //if in combat
            target != null && //if has target
            canBL && //if Battle Litany is ready
            powerLeft > 0, //if Power Surge is active
        OGCDStrategy.Force => canBL, //Always use if forced
        OGCDStrategy.AnyWeave => canBL && CanWeaveIn, //Always use if any weave
        OGCDStrategy.EarlyWeave => canBL && CanEarlyWeaveIn, //Always use if early weave
        OGCDStrategy.LateWeave => canBL && CanLateWeaveIn, //Always use if late weave
        OGCDStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Life Surge
    private bool ShouldUseLifeSurge(SurgeStrategy strategy, Actor? target) => strategy switch
    {
        SurgeStrategy.Automatic =>
            Player.InCombat && //if in combat
            target != null && //if has target
            canLS && //if Life Surge is ready
            hasLC && //if Lance Charge is active
            !PlayerHasEffect(SID.LifeSurge) && //if Life Surge is not already active
            (TotalCD(AID.LifeSurge) < 40 || //if Life Surge cooldown is less than 40s
            TotalCD(AID.BattleLitany) > 50) && //or Battle Litany cooldown is greater than 50s
            (ComboLastMove is AID.WheelingThrust or AID.FangAndClaw && Unlocked(AID.Drakesbane) || //if Wheeling Thrust or Fang and Claw was just used & Drakesbane is Unlocked
            ComboLastMove is AID.VorpalThrust or AID.LanceBarrage && Unlocked(AID.FullThrust)), //or Vorpal Thrust or Lance Barrage was just used & Full Thrust is Unlocked
        SurgeStrategy.Force => canLS, //Always use if forced
        SurgeStrategy.ForceWeave => canLS && CanWeaveIn, //Always use if inside weave window
        SurgeStrategy.ForceNextOpti => canLS && //if Life Surge is ready
            (ComboLastMove is AID.WheelingThrust or AID.FangAndClaw && Unlocked(AID.Drakesbane) || //if Wheeling Thrust or Fang and Claw was just used & Drakesbane is Unlocked
            ComboLastMove is AID.VorpalThrust or AID.LanceBarrage && Unlocked(AID.FullThrust)), //or Vorpal Thrust or Lance Barrage was just used & Full Thrust is Unlocked
        SurgeStrategy.ForceNextOptiWeave => canLS && CanWeaveIn && //Always use if Life Surge is ready and inside weave window
            (ComboLastMove is AID.WheelingThrust or AID.FangAndClaw && Unlocked(AID.Drakesbane) || //if Wheeling Thrust or Fang and Claw was just used & Drakesbane is Unlocked
            ComboLastMove is AID.VorpalThrust or AID.LanceBarrage && Unlocked(AID.FullThrust)), //or Vorpal Thrust or Lance Barrage was just used & Full Thrust is Unlocked
        SurgeStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };
    #endregion

    #region Dives
    //Determines when to use Dragonfire Dive
    private bool ShouldUseDragonfireDive(DragonfireStrategy strategy, Actor? target) => strategy switch
    {
        DragonfireStrategy.Automatic =>
            Player.InCombat && //if in combat
            target != null && //if has target
            In20y(target) && //if within 20 yalms
            canDD && //if Dragonfire Dive is ready
            hasLC && //if Lance Charge is active
            hasBL && //if Battle Litany is active
            hasLOTD, //if Life of the Dragon is active
        DragonfireStrategy.Force => canDD, //Always use if forced
        DragonfireStrategy.ForceEX => canDD, //Always use in ForceEX strategy
        DragonfireStrategy.ForceWeave => canDD && CanWeaveIn, //Always use if inside weave window
        DragonfireStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Jump
    private bool ShouldUseJump(JumpStrategy strategy, Actor? target) => strategy switch
    {
        JumpStrategy.Automatic =>
            //Use Jump automatically if the player is in combat, the target is valid, and Lance Charge-related conditions are met
            Player.InCombat && //if in combat
            target != null && //if has target
            In20y(target) && //if within 20 yalms
            canJump && //if Jump is ready
            (lcLeft > 0 || hasLC || //if Lance Charge is active
            lcCD is < 35 and > 17), //or greater than 17s remaining on Lance Charge cooldown
        JumpStrategy.ForceEX => canJump, //Always use in ForceEX strategy
        JumpStrategy.ForceEX2 => canJump, //Always use in ForceEX2 strategy
        JumpStrategy.ForceWeave => canJump && CanWeaveIn, //Always use if inside weave window
        JumpStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Stardiver
    private bool ShouldUseStardiver(StardiverStrategy strategy, Actor? target) => strategy switch
    {
        StardiverStrategy.Automatic =>
            Player.InCombat && //if in combat
            target != null && //if has target
            In20y(target) && //if within 20 yalms
            canSD && //if Stardiver is ready
            hasLOTD, //if Life of the Dragon is active
        StardiverStrategy.Force => canSD, //Always use if forced
        StardiverStrategy.ForceEX => canSD, //Always use if forced
        StardiverStrategy.ForceWeave => canSD && CanWeaveIn, //Always use if inside weave window
        StardiverStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Mirage Dive
    private bool ShouldUseMirageDive(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //if in combat
            target != null && //if has target
            In20y(target) && //if within 20 yalms
            canMD, //if Mirage Dive is ready
        OGCDStrategy.Force => canMD, //Always use if forced
        OGCDStrategy.AnyWeave => canMD && CanWeaveIn, //Always use if any weave
        OGCDStrategy.EarlyWeave => canMD && CanEarlyWeaveIn, //Always use if early weave
        OGCDStrategy.LateWeave => canMD && CanLateWeaveIn, //Always use if late weave
        OGCDStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };
    #endregion

    #region Spears
    //Determines when to use Geirskogul
    private bool ShouldUseGeirskogul(GeirskogulStrategy strategy, Actor? target) => strategy switch
    {
        GeirskogulStrategy.Automatic =>
            Player.InCombat && //if in combat
            In15y(target) && //if within 15 yalms
            canGeirskogul && //if Geirskogul is ready
            hasLC, //if Lance Charge is active
        GeirskogulStrategy.Force => canGeirskogul, //Always use if forced
        GeirskogulStrategy.ForceEX => canGeirskogul, //Always use if forced
        GeirskogulStrategy.ForceWeave => canGeirskogul && CanWeaveIn, //Always use if inside weave window
        GeirskogulStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Nastrond
    private bool ShouldUseNastrond(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //if in combat
            In15y(target) && //if within 15 yalms
            canNastrond, //if Nastrond is ready
        OGCDStrategy.Force => canNastrond, //Always use if forced
        OGCDStrategy.AnyWeave => canNastrond && CanWeaveIn, //Always use if any weave
        OGCDStrategy.EarlyWeave => canNastrond && CanEarlyWeaveIn, //Always use if early weave
        OGCDStrategy.LateWeave => canNastrond && CanLateWeaveIn, //Always use if late weave
        OGCDStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Wyrmwind Thrust
    private bool ShouldUseWyrmwindThrust(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //if in combat
            target != null && //if has target
            In15y(target) && //if within 15 yalms
            canWT && //if Wyrmwind Thrust is ready
            lcCD > GCDLength * 2, //if Lance Charge is imminent, hold until buff is active
        OGCDStrategy.Force => canWT, //Always use if forced
        OGCDStrategy.AnyWeave => canWT && CanWeaveIn, //Always use if any weave
        OGCDStrategy.EarlyWeave => canWT && CanEarlyWeaveIn, //Always use if early weave
        OGCDStrategy.LateWeave => canWT && CanLateWeaveIn, //Always use if late weave
        OGCDStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };
    #endregion

    //Determines when to use Rise of the Dragon
    private bool ShouldUseRiseOfTheDragon(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //if in combat
            target != null && //if has target
            In20y(target) && //if within 20 yalms
            canROTD, //if Rise of the Dragon is ready
        OGCDStrategy.Force => canROTD, //Always use if forced
        OGCDStrategy.AnyWeave => canROTD && CanWeaveIn, //Always use if any weave
        OGCDStrategy.EarlyWeave => canROTD && CanEarlyWeaveIn, //Always use if early weave
        OGCDStrategy.LateWeave => canROTD && CanLateWeaveIn, //Always use if late weave
        OGCDStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Starcross
    private bool ShouldUseStarcross(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            //Use Starcross automatically if the player is in combat, the target is valid, and Starcross Ready effect is active
            Player.InCombat && //if in combat
            target != null && //if has target
            In20y(target) && //if within 20 yalms
            canSC, //if Starcross is ready
        OGCDStrategy.Force => canSC, //Always use if forced
        OGCDStrategy.AnyWeave => canSC && CanWeaveIn, //Always use if any weave
        OGCDStrategy.EarlyWeave => canSC && CanEarlyWeaveIn, //Always use if early weave
        OGCDStrategy.LateWeave => canSC && CanLateWeaveIn, //Always use if late weave
        OGCDStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Piercing Talon
    private bool ShouldUsePiercingTalon(Actor? target, PiercingTalonStrategy strategy) => strategy switch
    {
        PiercingTalonStrategy.AllowEX =>
            Player.InCombat && //if in combat
            target != null && //if has target
            !In3y(target) && //if not in melee range
            PlayerHasEffect(SID.EnhancedPiercingTalon), //if Enhanced Piercing Talon is active
        PiercingTalonStrategy.Allow =>
            Player.InCombat && //if in combat
            target != null && //if has target
            !In3y(target), //if not in melee range
        PiercingTalonStrategy.Force => true, //Always use if forced
        PiercingTalonStrategy.ForceEX => PlayerHasEffect(SID.EnhancedPiercingTalon), //Use if Enhanced Piercing Talon is active
        PiercingTalonStrategy.Forbid => false, //Never use if forbidden
        _ => false
    };

    //Determines when to use a potion based on strategy
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs =>
            lcCD <= GCD * 2 && //Lance Charge is imminent
            blCD <= GCD * 2, //Battle Litany is imminent
        PotionStrategy.Immediate => true, //Use the potion immediately
        _ => false
    };

    //Determines when to use True North
    //TODO: reconsider this method, it's jank as fuck but it works
    private bool ShouldUseTrueNorth(TrueNorthStrategy strategy, Actor? target) => strategy switch
    {
        TrueNorthStrategy.Automatic =>
            target != null &&
            Player.InCombat &&
            !PlayerHasEffect(SID.TrueNorth) &&
            GCD < 1.25f &&
            (!IsOnRear(target) && //Side
            ComboLastMove is AID.Disembowel or AID.SpiralBlow
            or AID.ChaosThrust or AID.ChaoticSpring ||
            !IsOnFlank(target) && //Back
            ComboLastMove is AID.HeavensThrust or AID.FullThrust),
        TrueNorthStrategy.ASAP =>
            target != null && Player.InCombat &&
            !PlayerHasEffect(SID.TrueNorth) &&
            (!IsOnRear(target) && //Side
            ComboLastMove is AID.Disembowel or AID.SpiralBlow
            or AID.ChaosThrust or AID.ChaoticSpring ||
            !IsOnFlank(target) && //Back
            ComboLastMove is AID.HeavensThrust or AID.FullThrust),
        TrueNorthStrategy.Flank =>
            target != null && Player.InCombat &&
            !PlayerHasEffect(SID.TrueNorth) &&
            GCD < 1.25f &&
            !IsOnFlank(target) && //Back
            ComboLastMove is AID.HeavensThrust or AID.FullThrust,
        TrueNorthStrategy.Rear =>
            target != null && Player.InCombat &&
            !PlayerHasEffect(SID.TrueNorth) &&
            GCD < 1.25f &&
            !IsOnRear(target) && //Side
            ComboLastMove is AID.Disembowel or AID.SpiralBlow
            or AID.ChaosThrust or AID.ChaoticSpring,
        TrueNorthStrategy.Force => !PlayerHasEffect(SID.TrueNorth),
        TrueNorthStrategy.Delay => false,
        _ => false
    };
    #endregion
}
