﻿using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using AID = BossMod.DRG.AID;
using SID = BossMod.DRG.SID;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiDRG(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies

    //Which abilities/strategies we're tracking
    public enum Track
    {
        AOE,                   //Area of Effect actions
        Burst,                 //Burst actions
        Potion,                //Potion usage
        LifeSurge,             //Life Surge ability
        Jump,                  //Jump ability
        DragonfireDive,        //Dragonfire Dive ability
        Geirskogul,            //Geirskogul ability
        Stardiver,             //Stardiver ability
        PiercingTalon,         //Piercing Talon ability
        TrueNorth,             //True North ability
        LanceCharge,           //Lance Charge ability
        BattleLitany,          //Battle Litany ability
        MirageDive,            //Mirage Dive ability
        Nastrond,              //Nastrond ability
        WyrmwindThrust,        //Wyrmwind Thrust ability
        RiseOfTheDragon,       //Rise of the Dragon ability
        Starcross              //Starcross ability
    }

    //Specifying AOE strategy preferences
    public enum AOEStrategy
    {
        AutoTargetHitPrimary,  //Auto-target AOE actions to hit primary target as well
        AutoTargetHitMost,     //Auto-target AOE actions to hit most targets
        AutoOnPrimary,         //Use AOE actions on primary target
        ForceST,               //Force single-target abilities
        Force123ST,            //Force single-target 123 combo
        ForceBuffsST,          //Force single-target buffs combo
        ForceAOE               //Force Area of Effect abilities
    }

    //Burst strategy options
    public enum BurstStrategy
    {
        Automatic,             //Automatic burst under general conditions
        Conserve,              //Conserve burst for later
        UnderRaidBuffs,        //Burst during raid buffs
        UnderPotion            //Burst when a potion is active
    }

    //Potion usage strategies
    public enum PotionStrategy
    {
        Manual,                //Manual potion usage
        AlignWithRaidBuffs,    //Align potion use with raid buffs
        Immediate              //Use potion immediately
    }

    //Life Surge strategy
    public enum SurgeStrategy
    {
        Automatic,             //Automatically use Life Surge
        Force,                 //Force use of Life Surge
        ForceEX,               //Force use of Life Surge (EX)
        Delay                  //Delay use of Life Surge
    }

    //Jump ability strategy
    public enum JumpStrategy
    {
        Automatic,             //Automatically use Jump
        Force,                 //Force use of Jump
        ForceEX,               //Force use of Jump EX)
        ForceEX2,              //Force use of High Jump
        Delay                  //Delay use of Jump
    }

    //Dragonfire Dive strategy
    public enum DragonfireStrategy
    {
        Automatic,             //Automatically use Dragonfire Dive
        Force,                 //Force use of Dragonfire Dive
        ForceEX,               //Force use of Dragonfire Dive (EX))
        Delay                  //Delay use of Dragonfire Dive
    }

    //Geirskogul strategy
    public enum GeirskogulStrategy
    {
        Automatic,             //Automatically use Geirskogul
        Force,                 //Force use of Geirskogul
        ForceEX,               //Force use of Geirskogul (EX)
        Delay                  //Delay use of Geirskogul
    }

    //Stardiver strategy
    public enum StardiverStrategy
    {
        Automatic,             //Automatically use Stardiver
        Force,                 //Force use of Stardiver
        ForceEX,               //Force use of Stardiver (EX)
        Delay                  //Delay use of Stardiver
    }

    //Piercing Talon strategy
    public enum PiercingTalonStrategy
    {
        Forbid,                //Forbid the use of Piercing Talon
        Allow,                 //Use Piercing Talon when appropriate
        Force,                 //Force use of Piercing Talon
    }

    //True North strategy
    public enum TrueNorthStrategy
    {
        Automatic,      //Late-Weave
        ASAP,           //Use ASAP
        Rear,           //Use only when in Rear
        Flank,          //Use only when in Flank
        Force,          //Force
        Delay           //Delay
    }

    //Offensive strategies
    public enum OffensiveStrategy
    {
        Automatic,             //Automatically use offensive abilities
        Force,                 //Force offensive abilities
        Delay                  //Delay offensive abilities
    }

    #endregion

    //Module Definitions
    public static RotationModuleDefinition Definition()
    {
        //Module title & signature
        var res = new RotationModuleDefinition("Akechi DRG", "Standard Rotation Module", "Standard rotation (Akechi)", "Akechi", RotationModuleQuality.Good, BitMask.Build(Class.LNC, Class.DRG), 100);

        #region Custom Strategies
        //Targeting strategy
        res.Define(Track.AOE).As<AOEStrategy>("Combo Option", "AOE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoTargetHitPrimary, "AutoTargetHitPrimary", "Use AOE actions if profitable, select best target that ensures primary target is hit")
            .AddOption(AOEStrategy.AutoTargetHitMost, "AutoTargetHitMost", "Use AOE actions if profitable, select a target that ensures maximal number of targets are hit")
            .AddOption(AOEStrategy.AutoOnPrimary, "AutoOnPrimary", "Use AOE actions on primary target if profitable")
            .AddOption(AOEStrategy.ForceST, "Force ST", "Force Single-Target rotation")
            .AddOption(AOEStrategy.Force123ST, "Only 1-2-3 ST", "Force only ST 1-2-3 rotation (No Buffs)")
            .AddOption(AOEStrategy.ForceBuffsST, "Only 1-4-5 ST", "Force only ST 1-4-5 rotation (Buffs Only)")
            .AddOption(AOEStrategy.ForceAOE, "Force AOE", "Force AOE rotation, even if less than 3 targets");

        //Burst strategy
        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 190)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Use Burst optimally based on situation")
            .AddOption(BurstStrategy.Conserve, "Conserve", "Conserve resources until optimal burst timing")
            .AddOption(BurstStrategy.UnderRaidBuffs, "Under Raid Buffs", "Burst under raid buffs; conserve otherwise (ignores potion usage)")
            .AddOption(BurstStrategy.UnderPotion, "Under Potion", "Burst under potion, conserve otherwise (ignores raid buffs)");

        //Potion strategy
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 180)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use potions automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Align With Raid Buffs", "Use potion in sync with 2-minute raid buffs (e.g., 0/6, 2/8)")
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potion as soon as possible, regardless of any buffs")
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        //Life Surge Strategy
        res.Define(Track.LifeSurge).As<SurgeStrategy>("Life Surge", "L. Surge", uiPriority: 160)
            .AddOption(SurgeStrategy.Automatic, "Automatic", "Use Life Surge normally")
            .AddOption(SurgeStrategy.Force, "Force", "Force Life Surge usage", 40, 5, ActionTargets.Hostile, 6, 87)
            .AddOption(SurgeStrategy.ForceEX, "ForceEX", "Force Life Surge (2 charges)", 40, 5, ActionTargets.Hostile, 88) //2 charges
            .AddOption(SurgeStrategy.Delay, "Delay", "Delay the use of Life Surge", 0, 0, ActionTargets.None, 6)
            .AddAssociatedActions(AID.LifeSurge);

        //Jump strategy
        res.Define(Track.Jump).As<JumpStrategy>("Jump", uiPriority: 110)
            .AddOption(JumpStrategy.Automatic, "Automatic", "Use Jump normally")
            .AddOption(JumpStrategy.Force, "Force Jump", "Force Jump usage", 30, 0, ActionTargets.Self, 30, 67)
            .AddOption(JumpStrategy.ForceEX, "Force Jump (EX)", "Force Jump usage (Grants Dive Ready buff)", 30, 15, ActionTargets.Self, 68, 74)
            .AddOption(JumpStrategy.ForceEX2, "Force High Jump", "Force High Jump usage", 30, 15, ActionTargets.Self, 75)
            .AddOption(JumpStrategy.Delay, "Delay", "Delay Jump usage", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.Jump, AID.HighJump);

        //Dragonfire Dive strategy
        res.Define(Track.DragonfireDive).As<DragonfireStrategy>("Dragonfire Dive", "D.Dive", uiPriority: 150)
            .AddOption(DragonfireStrategy.Automatic, "Automatic", "Use Dragonfire Dive normally")
            .AddOption(DragonfireStrategy.Force, "Force", "Force Dragonfire Dive usage", 120, 0, ActionTargets.Hostile, 50, 91)
            .AddOption(DragonfireStrategy.ForceEX, "ForceEX", "Force Dragonfire Dive (Grants Dragon's Flight)", 120, 30, ActionTargets.Hostile, 92)
            .AddOption(DragonfireStrategy.Delay, "Delay", "Delay Dragonfire Dive usage", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(AID.DragonfireDive);

        //Geirskogul strategy
        res.Define(Track.Geirskogul).As<GeirskogulStrategy>("Geirskogul", "Geirs.", uiPriority: 130)
            .AddOption(GeirskogulStrategy.Automatic, "Automatic", "Use Geirskogul normally")
            .AddOption(GeirskogulStrategy.Force, "Force", "Force Geirskogul usage", 60, 0, ActionTargets.Hostile, 60, 69)
            .AddOption(GeirskogulStrategy.ForceEX, "ForceEX", "Force Geirskogul (Grants Life of the Dragon & 3x Nastrond)", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(GeirskogulStrategy.Delay, "Delay", "Delay Geirskogul usage", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Geirskogul);

        //Stardiver strategy
        res.Define(Track.Stardiver).As<StardiverStrategy>("Stardiver", "S.diver", uiPriority: 140)
            .AddOption(StardiverStrategy.Automatic, "Automatic", "Use Stardiver normally")
            .AddOption(StardiverStrategy.Force, "Force", "Force Stardiver usage", 30, 0, ActionTargets.Hostile, 80, 99)
            .AddOption(StardiverStrategy.ForceEX, "ForceEX", "Force Stardiver (Grants Starcross Ready)", 30, 0, ActionTargets.Hostile, 100)
            .AddOption(StardiverStrategy.Delay, "Delay", "Delay Stardiver usage", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Stardiver);

        //Piercing Talon strategy
        res.Define(Track.PiercingTalon).As<PiercingTalonStrategy>("Piercing Talon", "Talon", uiPriority: 20)
            .AddOption(PiercingTalonStrategy.Forbid, "Forbid", "Forbid use of Piercing Talon")
            .AddOption(PiercingTalonStrategy.Allow, "Allow", "Allow use of Piercing Talon only if already in combat & outside melee range")
            .AddOption(PiercingTalonStrategy.Force, "Force", "Force Piercing Talon usage ASAP (even in melee range)")
            .AddAssociatedActions(AID.PiercingTalon);

        //True North strategy
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
        //Lance Charge strategy
        res.Define(Track.LanceCharge).As<OffensiveStrategy>("Lance Charge", "L.Charge", uiPriority: 165)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Lance Charge normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Lance Charge usage ASAP (even during downtime)", 60, 20, ActionTargets.Self, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Lance Charge usage", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.LanceCharge);

        //Battle Litany strategy
        res.Define(Track.BattleLitany).As<OffensiveStrategy>("Battle Litany", "B.Litany", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Battle Litany normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Battle Litany usage ASAP (even during downtime)", 120, 20, ActionTargets.Self, 52)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Battle Litany usage", 0, 0, ActionTargets.None, 52)
            .AddAssociatedActions(AID.BattleLitany);

        //Mirage Dive strategy
        res.Define(Track.MirageDive).As<OffensiveStrategy>("Mirage Dive", "M.Dive", uiPriority: 105)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Mirage Dive normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Mirage Dive usage", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Mirage Dive usage", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(AID.MirageDive);

        //Nastrond strategy
        res.Define(Track.Nastrond).As<OffensiveStrategy>("Nastrond", "Nast.", uiPriority: 125)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Nastrond normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Nastrond usage", 0, 2, ActionTargets.Hostile, 70)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Nastrond usage", 0, 0, ActionTargets.None, 70)
            .AddAssociatedActions(AID.Nastrond);

        //Wyrmwind Thrust strategy
        res.Define(Track.WyrmwindThrust).As<OffensiveStrategy>("Wyrmwind Thrust", "W.Thrust", uiPriority: 120)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Wyrmwind Thrust normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Wyrmwind Thrust usage ASAP", 0, 10, ActionTargets.Hostile, 90)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Wyrmwind Thrust usage", 0, 0, ActionTargets.None, 90)
            .AddAssociatedActions(AID.WyrmwindThrust);

        //Rise Of The Dragon strategy
        res.Define(Track.RiseOfTheDragon).As<OffensiveStrategy>("Rise Of The Dragon", "RotD", uiPriority: 145)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Rise Of The Dragon normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Rise Of The Dragon usage", 0, 0, ActionTargets.Hostile, 92)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Rise Of The Dragon usage", 0, 0, ActionTargets.None, 92)
            .AddAssociatedActions(AID.RiseOfTheDragon);

        //Starcross strategy
        res.Define(Track.Starcross).As<OffensiveStrategy>("Starcross", "S.cross", uiPriority: 135)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Starcross normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Starcross usage", 0, 0, ActionTargets.Self, 100)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Starcross usage", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(AID.Starcross);

        #endregion

        return res;
    }

    #region Priorities

    //Priorities for Global Cooldowns (GCDs)
    public enum GCDPriority
    {
        None = 0,               //No priority
        Combo123 = 350,         //Priority for the first three combo actions
        NormalGCD = 500,        //Standard priority for normal GCD actions
        ForcedGCD = 900,        //High priority for forced GCD actions
    }

    //Priorities for Off Global Cooldowns (oGCDs)
    public enum OGCDPriority
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

    #region Placeholders for Variables

    private float GCDLength; //Length of the global cooldown
    private float blCD; //Cooldown for Battle Litany
    private float lcLeft; //Time remaining for Lance Charge
    private float lcCD; //Cooldown for Lance Charge
    private float powerLeft; //Time remaining for Power Surge
    private float chaosLeft; //Remaining time for Chaotic Spring DoT

    public float downtimeIn; //Duration of downtime in combat
    private float PotionLeft; //Remaining time for potion effect
    private float RaidBuffsLeft; //Time left for raid buffs
    private float RaidBuffsIn; //Time until next raid buffs are applied
    public float BurstWindowLeft; //Time left in the burst window
    public float BurstWindowIn; //Time until the next burst window starts

    private int focusCount; //Count of Firstmind's Focus gauge

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

    public AID NextGCD; //Next global cooldown action to be used
    private GCDPriority NextGCDPrio; //Priority of the next GCD for cooldown decision making

    #endregion

    #region Module Helpers

    //Check if the desired ability is unlocked
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));

    //Get remaining cooldown time for the specified action
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    //Get the last action used in the combo sequence
    private AID ComboLastMove => (AID)World.Client.ComboState.Action;

    //Check if the target is within melee range (3 yalms)
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 3;

    //Check if the target is within 15 yalms
    private bool In15y(Actor? target) => Player.DistanceToHitbox(target) <= 14.75;

    //Check if the desired action is ready (cooldown < 0.6 seconds)
    private bool ActionReady(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f;

    //Check if the potion should be used before raid buffs expire
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f;

    //Check if status effect is on self
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null;

    //Count number of targets hit by AOE attack
    private int NumTargetsHitByAOE(Actor primary) => Hints.NumPriorityTargetsInAOECone(Player.Position, 10, (primary.Position - Player.Position).Normalized(), 45.Degrees());

    //Check if a target is hit by AOE
    private bool IsHitByAOE(Actor primary, Actor check) => Hints.TargetInAOECone(check, Player.Position, 10, (primary.Position - Player.Position).Normalized(), 45.Degrees());

    //Count number of targets hit by spear attack
    private int NumTargetsHitBySpear(Actor primary) => Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 15, 2);

    //Check if a target is hit by spear
    private bool IsHitBySpear(Actor primary, Actor check) => Hints.TargetInAOERect(check, Player.Position, (primary.Position - Player.Position).Normalized(), 15, 2);

    //Check targeting for AOE strategies
    private (Actor?, int) CheckAOETargeting(AOEStrategy strategy, Actor? primaryTarget, float range, Func<Actor, int> numTargets, Func<Actor, Actor, bool> check) => strategy switch
    {
        AOEStrategy.AutoTargetHitPrimary => FindBetterTargetBy(primaryTarget, range, t => primaryTarget == null || check(t, primaryTarget) ? numTargets(t) : 0),
        AOEStrategy.AutoTargetHitMost => FindBetterTargetBy(primaryTarget, range, numTargets),
        AOEStrategy.AutoOnPrimary => (primaryTarget, primaryTarget != null ? numTargets(primaryTarget) : 0),
        AOEStrategy.ForceAOE => (primaryTarget, int.MaxValue),
        _ => (null, 0)
    };

    //Check which positional Player is on
    private Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
    {
        < -0.7071068f => Positional.Rear,
        < 0.7071068f => Positional.Flank,
        _ => Positional.Front
    };

    //Check if Player is on Rear (back) positional 
    private bool IsOnRear(Actor target) => GetCurrentPositional(target) == Positional.Rear;

    //Check if Player is on Flank (side) positional 
    private bool IsOnFlank(Actor target) => GetCurrentPositional(target) == Positional.Flank;

    #endregion

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        #region Variables

        #region Cooldown-related

        //Gauge Management
        var gauge = World.Client.GetGauge<DragoonGauge>();  //Retrieve Dragoon gauge data
        focusCount = gauge.FirstmindsFocusCount;  //Update focus count from the gauge
        hasLOTD = gauge.LotdTimer > 0;  //Check if Life of the Dragon (LOTD) is active

        //Cooldown Checks
        lcCD = CD(AID.LanceCharge);  //Get cooldown for Lance Charge
        lcLeft = SelfStatusLeft(SID.LanceCharge, 20);  //Get remaining time for Lance Charge effect
        powerLeft = SelfStatusLeft(SID.PowerSurge, 30);  //Get remaining time for Power Surge effect
        chaosLeft = MathF.Max(StatusDetails(primaryTarget, SID.ChaosThrust, Player.InstanceID).Left,
                              StatusDetails(primaryTarget, SID.ChaoticSpring, Player.InstanceID).Left);  //Get max remaining time for Chaos Thrust or Chaotic Spring
        blCD = CD(AID.BattleLitany);  //Get cooldown for Battle Litany
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);  //Calculate rounded global cooldown (GCD) duration
        hasMD = HasEffect(SID.DiveReady);  //Check if Mirage Dive is ready
        hasNastrond = HasEffect(SID.NastrondReady);  //Check if Nastrond is ready
        hasLC = lcCD is >= 40 and <= 60;  //Check if Lance Charge is within cooldown range
        hasBL = blCD is >= 100 and <= 120;  //Check if Battle Litany is within cooldown range
        hasDF = HasEffect(SID.DragonsFlight);  //Check if Dragon's Flight effect is active
        hasSC = HasEffect(SID.StarcrossReady);  //Check if Starcross is ready

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
        canWT = Unlocked(AID.WyrmwindThrust) && ActionReady(AID.WyrmwindThrust);  //minimum condition(s) to execute Wyrmwind Thrust
        canROTD = Unlocked(AID.RiseOfTheDragon) && hasDF;  //minimum condition(s) to execute Rise of the Dragon
        canSC = Unlocked(AID.Starcross) && hasSC;  //minimum condition(s) to execute Starcross

        #endregion

        #region Miscellaneous
        downtimeIn = Manager.Planner?.EstimateTimeToNextDowntime().Item2 ?? float.MaxValue;  //Estimate downtime until next action
        PotionLeft = PotionStatusLeft();  //Get remaining potion status
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);  //Estimate remaining raid buffs
        NextGCD = AID.None;  //Set next GCD ability
        NextGCDPrio = GCDPriority.None;  //Set next GCD priority

        #endregion

        #endregion

        #region AOEStrategy 'Force' Execution

        var AOEStrategy = strategy.Option(Track.AOE).As<AOEStrategy>();  //Retrieve the current AOE strategy

        //Force specific actions based on the AOE strategy selected
        if (AOEStrategy == AOEStrategy.ForceST)  //If forced single target
            QueueGCD(NextFullST(), primaryTarget, GCDPriority.ForcedGCD);  //Queue the next single target action

        if (AOEStrategy == AOEStrategy.Force123ST)  //If forced 123 combo
            QueueGCD(UseOnly123ST(), primaryTarget, GCDPriority.ForcedGCD);  //Queue the 123 combo action

        if (AOEStrategy == AOEStrategy.ForceBuffsST)  //If forced buffs combo
            QueueGCD(UseOnly145ST(), primaryTarget, GCDPriority.ForcedGCD);  //Queue the buffed 145 combo action

        if (AOEStrategy == AOEStrategy.ForceAOE)  //If forced AOE action
            QueueGCD(NextFullAOE(), primaryTarget, GCDPriority.ForcedGCD);  //Queue the next AOE action

        #endregion

        #region Burst Window Strategy
        var burst = strategy.Option(Track.Burst);  //Retrieve the current burst strategy
        var burstStrategy = burst.As<BurstStrategy>();  //Cast to BurstStrategy type
        var hold = burstStrategy == BurstStrategy.Conserve;  //Check if the burst should be conserved

        //Set burst window timings based on the selected burst strategy
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => (RaidBuffsIn, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft)),  //Automatic mode: Set timings based on raid buffs and potion availability
            BurstStrategy.UnderRaidBuffs => (RaidBuffsIn, RaidBuffsLeft),  //Under Raid Buffs: Set timings directly from raid buffs
            BurstStrategy.UnderPotion => (PotionCD, PotionLeft),  //Under Potion: Use potion cooldown and remaining time
            _ => (0, 0)  //Default case: Set timings to zero
        };

        #endregion

        #region Targeting

        //Check if Doom Spike is unlocked; if so, determine the best AOE target and count
        var (AOEBestTarget, AOETargetCount) = Unlocked(AID.DoomSpike)
            ? CheckAOETargeting(AOEStrategy, primaryTarget, 10, NumTargetsHitByAOE, IsHitByAOE)
            : (null, 0);  //Set to null and count 0 if not unlocked

        //Check if Geirskogul is unlocked; if so, determine the best spear target and count
        var (SpearBestTarget, SpearTargetCount) = Unlocked(AID.Geirskogul)
            ? CheckAOETargeting(AOEStrategy, primaryTarget, 15, NumTargetsHitBySpear, IsHitBySpear)
            : (null, 0);  //Set to null and count 0 if not unlocked

        //Determine if using AOE is viable (at least 3 targets hit)
        var useAOE = AOETargetCount >= 3;

        //Determine if using Spear is viable (at least 2 targets hit)
        var useSpear = SpearTargetCount > 1;

        //Select the best target for Spear based on whether it's viable, or default to primary target
        var bestSpeartarget = useSpear ? SpearBestTarget : primaryTarget;

        //Select the best target for AOE based on whether it's viable, or default to primary target
        var bestAOEtarget = useAOE ? AOEBestTarget : primaryTarget;

        #endregion

        #region Combo & Cooldown Execution

        //Combo Action evecution
        QueueGCD(useAOE ? NextFullAOE() : NextFullST(), bestAOEtarget, GCDPriority.Combo123);
        //Execute Lance Charge if available
        var lcStrat = strategy.Option(Track.LanceCharge).As<OffensiveStrategy>();
        if (!hold && ShouldUseLanceCharge(lcStrat, primaryTarget))
            QueueOGCD(AID.LanceCharge, Player, lcStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

        //Execute Battle Litany if available
        var blStrat = strategy.Option(Track.BattleLitany).As<OffensiveStrategy>();
        if (!hold && ShouldUseBattleLitany(blStrat, primaryTarget))
            QueueOGCD(AID.BattleLitany, Player, blStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

        //Execute Life Surge if conditions met
        var lsStrat = strategy.Option(Track.LifeSurge).As<SurgeStrategy>();
        if (!hold && ShouldUseLifeSurge(lsStrat, primaryTarget))
            QueueOGCD(AID.LifeSurge, Player, lsStrat is SurgeStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

        //Execute Jump ability if available
        var jumpStrat = strategy.Option(Track.Jump).As<JumpStrategy>();
        if (!hold && ShouldUseJump(jumpStrat, primaryTarget))
            QueueOGCD(Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump, primaryTarget, jumpStrat == JumpStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Jump);

        //Execute Dragonfire Dive if available
        var ddStrat = strategy.Option(Track.DragonfireDive).As<DragonfireStrategy>();
        if (!hold && ShouldUseDragonfireDive(ddStrat, primaryTarget))
            QueueOGCD(AID.DragonfireDive, primaryTarget, ddStrat is DragonfireStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.DragonfireDive);

        //Execute Geirskogul if available
        var geirskogul = strategy.Option(Track.Geirskogul).As<GeirskogulStrategy>();
        if (!hold && ShouldUseGeirskogul(geirskogul, primaryTarget))
            QueueOGCD(AID.Geirskogul, bestSpeartarget, geirskogul == GeirskogulStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Geirskogul);

        //Execute Mirage Dive if available
        var mirageStrat = strategy.Option(Track.MirageDive).As<OffensiveStrategy>();
        if (!hold && ShouldUseMirageDive(mirageStrat, primaryTarget))
            QueueOGCD(AID.MirageDive, primaryTarget, mirageStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.MirageDive);

        //Execute Nastrond if available
        var nastrondStrat = strategy.Option(Track.Nastrond).As<OffensiveStrategy>();
        if (!hold && ShouldUseNastrond(nastrondStrat, primaryTarget))
            QueueOGCD(AID.Nastrond, bestSpeartarget, nastrondStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Nastrond);

        //Execute Stardiver if available
        var sdStrat = strategy.Option(Track.Stardiver).As<StardiverStrategy>();
        if (!hold && ShouldUseStardiver(sdStrat, primaryTarget))
            QueueOGCD(AID.Stardiver, primaryTarget, sdStrat == StardiverStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Stardiver);

        //Execute Wyrmwind Thrust if available
        var wtStrat = strategy.Option(Track.WyrmwindThrust).As<OffensiveStrategy>();
        if (!hold && ShouldUseWyrmwindThrust(wtStrat, primaryTarget))
            QueueOGCD(AID.WyrmwindThrust, bestSpeartarget, wtStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : HasEffect(SID.LanceCharge) ? OGCDPriority.WyrmwindThrustOpti : OGCDPriority.WyrmwindThrust);

        //Execute Rise of the Dragon if available
        var riseStrat = strategy.Option(Track.RiseOfTheDragon).As<OffensiveStrategy>();
        if (!hold && ShouldUseRiseOfTheDragon(riseStrat, primaryTarget))
            QueueOGCD(AID.RiseOfTheDragon, primaryTarget, riseStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

        //Execute Starcross if available
        var crossStrat = strategy.Option(Track.Starcross).As<OffensiveStrategy>();
        if (!hold && ShouldUseStarcross(crossStrat, primaryTarget))
            QueueOGCD(AID.Starcross, primaryTarget, crossStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Starcross);

        //Execute Piercing Talon if available
        var ptStrat = strategy.Option(Track.PiercingTalon).As<PiercingTalonStrategy>();
        if (ShouldUsePiercingTalon(primaryTarget, ptStrat))
            QueueGCD(AID.PiercingTalon, primaryTarget, ptStrat == PiercingTalonStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.NormalGCD);

        //Execute Potion if available
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.ForcedOGCD, 0, GCD - 0.9f);

        //Execute True North if available
        if (!hold && ShouldUseTrueNorth(strategy.Option(Track.TrueNorth).As<TrueNorthStrategy>(), primaryTarget))
            QueueOGCD(AID.TrueNorth, Player, OGCDPriority.TrueNorth);

        #endregion

        #region AI
        //AI hints for positioning
        var goalST = primaryTarget != null ? Hints.GoalSingleTarget(primaryTarget, 3) : null;
        var goalAOE = primaryTarget != null ? Hints.GoalAOECone(primaryTarget, 10, 45.Degrees()) : null;
        var goal = AOEStrategy switch
        {
            AOEStrategy.ForceST => goalST,
            AOEStrategy.Force123ST => goalST,
            AOEStrategy.ForceBuffsST => goalST,
            AOEStrategy.ForceAOE => goalAOE,
            _ => goalST != null && goalAOE != null ? Hints.GoalCombined(goalST, goalAOE, 2) : goalAOE
        };
        if (goal != null)
            Hints.GoalZones.Add(goal);
        #endregion

    }

    #region Core Execution Helpers

    //Determine the effect application delay for specific abilities
    private float EffectApplicationDelay(AID aid) => aid switch
    {
        AID.ChaoticSpring => 0.45f,         //Chaotic Spring delay
        AID.HighJump => 0.49f,              //High Jump delay
        AID.CoerthanTorment => 0.49f,       //Coerthan Torment delay
        AID.BattleLitany => 0.62f,          //Battle Litany delay
        AID.LanceBarrage => 0.62f,          //Lance Barrage delay
        AID.FangAndClaw => 0.62f,           //Fang and Claw delay
        AID.RaidenThrust => 0.62f,          //Raiden Thrust delay
        AID.Geirskogul => 0.67f,             //Geirskogul delay
        AID.WheelingThrust => 0.67f,         //Wheeling Thrust delay
        AID.HeavensThrust => 0.71f,          //Heavens Thrust delay
        AID.DraconianFury => 0.76f,          //Draconian Fury delay
        AID.Nastrond => 0.76f,               //Nastrond delay
        AID.TrueThrust => 0.76f,             //True Thrust delay
        AID.DragonfireDive => 0.8f,          //Dragonfire Dive delay
        AID.MirageDive => 0.8f,              //Mirage Dive delay
        AID.SonicThrust => 0.8f,             //Sonic Thrust delay
        AID.PiercingTalon => 0.85f,          //Piercing Talon delay
        AID.Starcross => 0.98f,              //Starcross delay
        AID.VorpalThrust => 1.02f,           //Vorpal Thrust delay
        AID.RiseOfTheDragon => 1.16f,        //Rise of the Dragon delay
        AID.WyrmwindThrust => 1.2f,          //Wyrmwind Thrust delay
        AID.DoomSpike => 1.29f,              //Doom Spike delay
        AID.Stardiver => 1.29f,              //Stardiver delay
        AID.SpiralBlow => 1.38f,             //Spiral Blow delay
        AID.Disembowel => 1.65f,             //Disembowel delay
        AID.DragonsongDive => 2.23f,         //Dragonsong Dive delay
        _ => 0                                   //Default case for unknown abilities
    };

    //Queue a global cooldown (GCD) action
    private void QueueGCD(AID aid, Actor? target, GCDPriority prio)
    {
        if (prio != GCDPriority.None)  //Check if priority is not None
        {
            //Calculate delay based on combat status and countdown
            var delay = !Player.InCombat && World.Client.CountdownRemaining > 0
                ? Math.Max(0, World.Client.CountdownRemaining.Value - EffectApplicationDelay(aid))
                : 0;

            //Push action to execute with appropriate priority and delay
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + (int)prio,
                delay: delay);

            //Update next GCD and its priority if the new priority is higher
            if (prio > NextGCDPrio)
            {
                NextGCD = aid;
                NextGCDPrio = prio;
            }
        }
    }

    //Queue an off-global cooldown (oGCD) action
    private void QueueOGCD(AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Low)
    {
        if (prio != OGCDPriority.None)  //Check if priority is not None
        {
            //Calculate delay based on combat status and countdown
            var delay = !Player.InCombat && World.Client.CountdownRemaining > 0
                ? Math.Max(0, World.Client.CountdownRemaining.Value - EffectApplicationDelay(aid))
                : 0;

            //Push action to execute with appropriate base priority and delay
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio,
                delay: delay);
        }
    }

    #endregion

    #region Single-Target Helpers

    //Determines the next skill in the single-target (ST) combo chain based on the last used action.
    private AID NextFullST() => ComboLastMove switch
    {
        //Starting combo with TrueThrust or RaidenThrust
        AID.TrueThrust or AID.RaidenThrust =>
            //If Disembowel is Unlocked and power is low or Chaotic Spring is 0, use Disembowel or SpiralBlow, else VorpalThrust or LanceBarrage
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

        //If no combo active and Draconian Fire buff is up, use RaidenThrust
        _ => HasEffect(SID.DraconianFire) ? AID.RaidenThrust //RaidenThrust if DraconianFire is active
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

        //If Draconian Fire buff is up, use RaidenThrust
        _ => HasEffect(SID.DraconianFire) ? AID.RaidenThrust //RaidenThrust if DraconianFire is active
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

        //If Draconian Fire buff is up, use RaidenThrust
        _ => HasEffect(SID.DraconianFire) ? AID.RaidenThrust //RaidenThrust if DraconianFire is active
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

        //If Draconian Fire buff is up, use DraconianFury
        _ => HasEffect(SID.DraconianFire)
            ? Unlocked(AID.DraconianFury) ? AID.DraconianFury : AID.DoomSpike  //DraconianFury if Unlocked, else DoomSpike
            : AID.DoomSpike,  //No DraconianFire active, default to DoomSpike
    };

    #endregion

    #region Cooldown Helpers

    //Determines when to use Lance Charge
    private bool ShouldUseLanceCharge(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Lance Charge automatically if the player is in combat, the target is valid, the action is ready, and there is power remaining
            Player.InCombat && target != null && canLC && powerLeft > 0,
        OffensiveStrategy.Force => canLC, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Battle Litany
    private bool ShouldUseBattleLitany(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Battle Litany automatically if the player is in combat, the target is valid, the action is ready, and there is power remaining
            Player.InCombat && target != null && canBL && powerLeft > 0,
        OffensiveStrategy.Force => canBL, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Life Surge
    private bool ShouldUseLifeSurge(SurgeStrategy strategy, Actor? target) => strategy switch
    {
        SurgeStrategy.Automatic => Player.InCombat && target != null && canLS && hasLC &&
            !HasEffect(SID.LifeSurge) &&
            (CD(AID.LifeSurge) < 40 || CD(AID.BattleLitany) > 50) &&
            (ComboLastMove is AID.WheelingThrust or AID.FangAndClaw && Unlocked(AID.Drakesbane) ||
            ComboLastMove is AID.VorpalThrust or AID.LanceBarrage && Unlocked(AID.FullThrust)),
        SurgeStrategy.Force => canLS,
        SurgeStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Jump
    private bool ShouldUseJump(JumpStrategy strategy, Actor? target) => strategy switch
    {
        JumpStrategy.Automatic =>
            //Use Jump automatically if the player is in combat, the target is valid, and Lance Charge-related conditions are met
            Player.InCombat && target != null && canJump && (lcLeft > 0 || hasLC || lcCD is < 35 and > 17),
        JumpStrategy.ForceEX => canJump, //Always use in ForceEX strategy
        JumpStrategy.ForceEX2 => canJump, //Always use in ForceEX2 strategy
        JumpStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Dragonfire Dive
    private bool ShouldUseDragonfireDive(DragonfireStrategy strategy, Actor? target) => strategy switch
    {
        DragonfireStrategy.Automatic =>
            //Use Dragonfire Dive automatically if the player is in combat, the target is valid, and both Lance Charge and Battle Litany are active
            Player.InCombat && target != null && In3y(target) && canDD && hasLC && hasBL,
        DragonfireStrategy.Force => canDD, //Always use if forced
        DragonfireStrategy.ForceEX => canDD, //Always use in ForceEX strategy
        DragonfireStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Geirskogul
    private bool ShouldUseGeirskogul(GeirskogulStrategy strategy, Actor? target) => strategy switch
    {
        GeirskogulStrategy.Automatic =>
            //Use Geirskogul automatically if the player is in combat, the action is ready, the target is within 15y, and Lance Charge is active
            Player.InCombat && In15y(target) && canGeirskogul && hasLC,
        GeirskogulStrategy.Force => canGeirskogul, //Always use if forced
        GeirskogulStrategy.ForceEX => canGeirskogul, //Always use if forced
        GeirskogulStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Mirage Dive
    private bool ShouldUseMirageDive(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Mirage Dive automatically if the player is in combat, the target is valid, and Dive Ready effect is active
            Player.InCombat && target != null && canMD,
        OffensiveStrategy.Force => canMD, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Nastrond
    private bool ShouldUseNastrond(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Nastrond automatically if the player is in combat, has Nastrond ready, the target is within 15y, and Lance Charge is active
            Player.InCombat && In15y(target) && canNastrond,
        OffensiveStrategy.Force => canNastrond, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Stardiver
    private bool ShouldUseStardiver(StardiverStrategy strategy, Actor? target) => strategy switch
    {
        StardiverStrategy.Automatic =>
            //Use Stardiver automatically if the player is in combat, the target is valid, the action is ready, and Life of the Dragon (LOTD) is active
            Player.InCombat && target != null && In3y(target) && canSD && hasLOTD,
        StardiverStrategy.Force => canSD, //Always use if forced
        StardiverStrategy.ForceEX => canSD, //Always use if forced
        StardiverStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Wyrmwind Thrust
    private bool ShouldUseWyrmwindThrust(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Wyrmwind Thrust automatically if the player is in combat, the target is within 15y, and focus count is exactly 2
            Player.InCombat && target != null && In15y(target) && canWT && focusCount is 2 && lcCD > GCDLength * 3,
        OffensiveStrategy.Force => canWT, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Rise of the Dragon
    private bool ShouldUseRiseOfTheDragon(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Rise of the Dragon automatically if the player is in combat, the target is valid, and Dragon's Flight effect is active
            Player.InCombat && target != null && canROTD,
        OffensiveStrategy.Force => canROTD, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Starcross
    private bool ShouldUseStarcross(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Starcross automatically if the player is in combat, the target is valid, and Starcross Ready effect is active
            Player.InCombat && target != null && canSC,
        OffensiveStrategy.Force => canSC, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Piercing Talon
    private bool ShouldUsePiercingTalon(Actor? target, PiercingTalonStrategy strategy) => strategy switch
    {
        PiercingTalonStrategy.Forbid => false, //Never use if forbidden
        PiercingTalonStrategy.Allow =>
            //Use Piercing Talon if the target is not within 3y range and already in combat
            Player.InCombat && target != null && !In3y(target),
        PiercingTalonStrategy.Force => true, //Always use if forced
        _ => false
    };

    //Determines when to use a potion based on strategy
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs =>
            //Use potion when Lance Charge and Battle Litany cooldowns align with raid buffs (GCD timing)
            lcCD <= GCD * 3 && blCD <= GCD * 3,
        PotionStrategy.Immediate => true, //Use the potion immediately
        _ => false
    };

    //Determines when to use True North
    //TODO: reconsider this method, it's jank as fuck but it works
    private bool ShouldUseTrueNorth(TrueNorthStrategy strategy, Actor? target) => strategy switch
    {
        TrueNorthStrategy.Automatic =>
            target != null && Player.InCombat &&
            !HasEffect(ClassShared.SID.TrueNorth) &&
            GCD < 1.25f &&
            (!IsOnRear(target) && //Side
            ComboLastMove is AID.Disembowel or AID.SpiralBlow
            or AID.ChaosThrust or AID.ChaoticSpring ||
            !IsOnFlank(target) && //Back
            ComboLastMove is AID.HeavensThrust or AID.FullThrust),
        TrueNorthStrategy.ASAP =>
            target != null && Player.InCombat &&
            !HasEffect(ClassShared.SID.TrueNorth) &&
            (!IsOnRear(target) && //Side
            ComboLastMove is AID.Disembowel or AID.SpiralBlow
            or AID.ChaosThrust or AID.ChaoticSpring ||
            !IsOnFlank(target) && //Back
            ComboLastMove is AID.HeavensThrust or AID.FullThrust),
        TrueNorthStrategy.Flank =>
            target != null && Player.InCombat &&
            !HasEffect(ClassShared.SID.TrueNorth) &&
            GCD < 1.25f &&
            !IsOnFlank(target) && //Back
            ComboLastMove is AID.HeavensThrust or AID.FullThrust,
        TrueNorthStrategy.Rear =>
            target != null && Player.InCombat &&
            !HasEffect(ClassShared.SID.TrueNorth) &&
            GCD < 1.25f &&
            !IsOnRear(target) && //Side
            ComboLastMove is AID.Disembowel or AID.SpiralBlow
            or AID.ChaosThrust or AID.ChaoticSpring,
        TrueNorthStrategy.Force => !HasEffect(SID.TrueNorth),
        TrueNorthStrategy.Delay => false,
        _ => false
    };

    #endregion
}
