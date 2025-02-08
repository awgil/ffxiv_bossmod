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

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition(
            "Akechi DRG", //Name
            "Standard Rotation Module", //Type
            "Standard rotation (Akechi)|DPS", //Category
            "Akechi", //Contributor of module
            RotationModuleQuality.Good, //Quality
            BitMask.Build(Class.LNC, Class.DRG), //Class and Job
            100); //Max Level supported

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
            .AddAssociatedActions(AID.Stardiver);
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
        res.DefineOGCD(Track.LanceCharge, AID.LanceCharge, "Lance Charge", "L.Charge", uiPriority: 165, 60, 20, ActionTargets.Self, 30);
        res.DefineOGCD(Track.BattleLitany, AID.BattleLitany, "Battle Litany", "B.Litany", uiPriority: 165, 120, 20, ActionTargets.Self, 52);
        res.DefineOGCD(Track.MirageDive, AID.MirageDive, "Mirage Dive", "M.Dive", uiPriority: 105, 0, 0, ActionTargets.Hostile, 68);
        res.DefineOGCD(Track.Nastrond, AID.Nastrond, "Nastrond", "Nast.", uiPriority: 125, 0, 0, ActionTargets.Hostile, 70);
        res.DefineOGCD(Track.WyrmwindThrust, AID.WyrmwindThrust, "Wyrmwind Thrust", "W.Thrust", uiPriority: 120, 0, 10, ActionTargets.Hostile, 90);
        res.DefineOGCD(Track.RiseOfTheDragon, AID.RiseOfTheDragon, "Rise Of The Dragon", "RotD", uiPriority: 145, 0, 0, ActionTargets.Hostile, 92);
        res.DefineOGCD(Track.Starcross, AID.Starcross, "Starcross", "S.cross", uiPriority: 135, 0, 0, ActionTargets.Hostile, 100);

        return res;
    }
    #endregion

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
        var gauge = World.Client.GetGauge<DragoonGauge>();
        focusCount = gauge.FirstmindsFocusCount;
        hasLOTD = gauge.LotdTimer > 0;
        lcCD = TotalCD(AID.LanceCharge);
        lcLeft = SelfStatusLeft(SID.LanceCharge, 20);
        powerLeft = SelfStatusLeft(SID.PowerSurge, 30);
        chaosLeft = MathF.Max(StatusDetails(primaryTarget?.Actor, SID.ChaosThrust, Player.InstanceID).Left, StatusDetails(primaryTarget?.Actor, SID.ChaoticSpring, Player.InstanceID).Left);
        blCD = TotalCD(AID.BattleLitany);
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
        hasMD = PlayerHasEffect(SID.DiveReady);
        hasNastrond = PlayerHasEffect(SID.NastrondReady);
        hasLC = lcCD is >= 40 and <= 60;
        hasBL = blCD is >= 100 and <= 120;
        hasDF = PlayerHasEffect(SID.DragonsFlight);
        hasSC = PlayerHasEffect(SID.StarcrossReady);
        canLC = Unlocked(AID.LanceCharge) && ActionReady(AID.LanceCharge);
        canBL = Unlocked(AID.BattleLitany) && ActionReady(AID.BattleLitany);
        canLS = Unlocked(AID.LifeSurge);
        canJump = Unlocked(AID.Jump) && ActionReady(AID.Jump);
        canDD = Unlocked(AID.DragonfireDive) && ActionReady(AID.DragonfireDive);
        canGeirskogul = Unlocked(AID.Geirskogul) && ActionReady(AID.Geirskogul);
        canMD = Unlocked(AID.MirageDive) && hasMD;
        canNastrond = Unlocked(AID.Nastrond) && hasNastrond;
        canSD = Unlocked(AID.Stardiver) && ActionReady(AID.Stardiver);
        canWT = Unlocked(AID.WyrmwindThrust) && ActionReady(AID.WyrmwindThrust) && focusCount == 2;
        canROTD = Unlocked(AID.RiseOfTheDragon) && hasDF;
        canSC = Unlocked(AID.Starcross) && hasSC;
        (BestAOETargets, NumAOETargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        (BestSpearTargets, NumSpearTargets) = GetBestTarget(primaryTarget, 15, Is15yRectTarget);
        (BestDiveTargets, NumDiveTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        BestAOETarget = Unlocked(AID.DoomSpike) && NumAOETargets > 2 ? BestAOETargets : primaryTarget;
        BestSpearTarget = Unlocked(AID.Geirskogul) && NumSpearTargets > 1 ? BestSpearTargets : primaryTarget;
        BestDiveTarget = Unlocked(AID.Stardiver) && NumDiveTargets > 1 ? BestDiveTargets : primaryTarget;

        #region Strategy Definitions
        var hold = strategy.Option(Track.Hold).As<HoldStrategy>() == HoldStrategy.Forbid;
        var AOE = strategy.Option(Track.AOE);
        var AOEStrategy = AOE.As<AOEStrategy>();
        var dive = strategy.Option(Track.Dives).As<DivesStrategy>();
        var lc = strategy.Option(Track.LanceCharge);
        var lcStrat = lc.As<OGCDStrategy>();
        var bl = strategy.Option(Track.BattleLitany);
        var blStrat = bl.As<OGCDStrategy>();
        var ls = strategy.Option(Track.LifeSurge);
        var lsStrat = ls.As<SurgeStrategy>();
        var jump = strategy.Option(Track.Jump);
        var jumpStrat = jump.As<JumpStrategy>();
        var dd = strategy.Option(Track.DragonfireDive);
        var ddStrat = dd.As<DragonfireStrategy>();
        var geirskogul = strategy.Option(Track.Geirskogul);
        var geirskogulStrat = geirskogul.As<GeirskogulStrategy>();
        var sd = strategy.Option(Track.Stardiver);
        var sdStrat = sd.As<StardiverStrategy>();
        var wt = strategy.Option(Track.WyrmwindThrust);
        var wtStrat = wt.As<OGCDStrategy>();
        var rotd = strategy.Option(Track.RiseOfTheDragon);
        var rotdStrat = rotd.As<OGCDStrategy>();
        var sc = strategy.Option(Track.Starcross);
        var scStrat = sc.As<OGCDStrategy>();
        var md = strategy.Option(Track.MirageDive);
        var mdStrat = md.As<OGCDStrategy>();
        var nastrond = strategy.Option(Track.Nastrond);
        var nastrondStrat = nastrond.As<OGCDStrategy>();
        var pt = strategy.Option(Track.PiercingTalon);
        var ptStrat = pt.As<PiercingTalonStrategy>();
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Dives
        //Dive strategy
        var diveStrategy = dive switch
        {
            DivesStrategy.AllowMaxMelee => In3y(primaryTarget?.Actor), //Only allow max melee if target is within 3 yalms
            DivesStrategy.AllowCloseMelee => In0y(primaryTarget?.Actor), //Only allow close melee if target is within 1 yalm
            DivesStrategy.Allow => In20y(primaryTarget?.Actor), //Always allow dives
            DivesStrategy.Forbid => false, //Never allow dives
            _ => false,
        };
        var maxMelee = dive == DivesStrategy.AllowMaxMelee; //Check if max melee is allowed
        var closeMelee = dive == DivesStrategy.AllowCloseMelee; //Check if close melee is allowed
        var allowed = dive == DivesStrategy.Allow; //Check if dives are allowed
        var forbidden = dive == DivesStrategy.Forbid; //Check if dives are forbidden
        var divesGood = diveStrategy && (maxMelee || closeMelee || allowed) && !forbidden; //Check if dives are good to use
        #endregion

        #region Standard Rotations
        //Force specific actions based on the AOE strategy selected
        if (AOEStrategy == AOEStrategy.ForceST)  //if forced single target
            QueueGCD(NextFullST(), TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ForcedGCD);  //Queue the next single target action
        if (AOEStrategy == AOEStrategy.Force123ST)  //if forced 123 combo
            QueueGCD(UseOnly123ST(), TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ForcedGCD);  //Queue the 123 combo action
        if (AOEStrategy == AOEStrategy.ForceBuffsST)  //if forced buffs combo
            QueueGCD(UseOnly145ST(), TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ForcedGCD);  //Queue the buffed 145 combo action
        if (AOEStrategy == AOEStrategy.ForceAOE)  //if forced AOE action
            QueueGCD(NextFullAOE(), TargetChoice(AOE) ?? (NumAOETargets > 1 ? BestAOETargets?.Actor : primaryTarget?.Actor), GCDPriority.ForcedGCD);  //Queue the next AOE action
        //Combo Action evecution
        QueueGCD(NumAOETargets > 2 ? NextFullAOE() : NextFullST(),
            BestAOETarget?.Actor,
            GCDPriority.Combo123);
        #endregion

        #region Cooldowns

        if (!hold)
        {
            if (ShouldUseLanceCharge(lcStrat, primaryTarget?.Actor))
                QueueOGCD(AID.LanceCharge, Player,
                    lcStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

            if (ShouldUseBattleLitany(blStrat, primaryTarget?.Actor))
                QueueOGCD(AID.BattleLitany, Player,
                    blStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

            if (ShouldUseLifeSurge(lsStrat, primaryTarget?.Actor))
                QueueOGCD(AID.LifeSurge, Player,
                    lsStrat is SurgeStrategy.Force or SurgeStrategy.ForceWeave or SurgeStrategy.ForceNextOpti or SurgeStrategy.ForceNextOptiWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

            if (divesGood && ShouldUseJump(jumpStrat, primaryTarget?.Actor))
                QueueOGCD(Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump,
                    TargetChoice(jump) ?? primaryTarget?.Actor,
                    jumpStrat is JumpStrategy.Force or JumpStrategy.ForceEX or JumpStrategy.ForceEX2 or JumpStrategy.ForceWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Jump);

            if (divesGood && ShouldUseDragonfireDive(ddStrat, primaryTarget?.Actor))
                QueueOGCD(AID.DragonfireDive,
                    TargetChoice(dd) ?? BestDiveTarget?.Actor,
                    ddStrat is DragonfireStrategy.Force or DragonfireStrategy.ForceWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.DragonfireDive);

            if (ShouldUseGeirskogul(geirskogulStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Geirskogul,
                    TargetChoice(geirskogul) ?? BestSpearTarget?.Actor,
                    geirskogulStrat is GeirskogulStrategy.Force or GeirskogulStrategy.ForceEX or GeirskogulStrategy.ForceWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Geirskogul);

            if (ShouldUseMirageDive(mdStrat, primaryTarget?.Actor))
                QueueOGCD(AID.MirageDive,
                    TargetChoice(md) ?? primaryTarget?.Actor,
                    mdStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.MirageDive);

            if (ShouldUseNastrond(nastrondStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Nastrond,
                    TargetChoice(nastrond) ?? BestSpearTarget?.Actor,
                    nastrondStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Nastrond);

            if (divesGood && ShouldUseStardiver(sdStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Stardiver,
                    TargetChoice(sd) ?? BestDiveTarget?.Actor,
                    sdStrat is StardiverStrategy.Force or StardiverStrategy.ForceEX or StardiverStrategy.ForceWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Stardiver);

            if (ShouldUseWyrmwindThrust(wtStrat, primaryTarget?.Actor))
                QueueOGCD(AID.WyrmwindThrust,
                    TargetChoice(wt) ?? BestSpearTarget?.Actor,
                    wtStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : PlayerHasEffect(SID.LanceCharge)
                    ? OGCDPriority.WyrmwindThrustOpti : OGCDPriority.WyrmwindThrust);

            if (ShouldUseRiseOfTheDragon(rotdStrat, primaryTarget?.Actor))
                QueueOGCD(AID.RiseOfTheDragon,
                    TargetChoice(rotd) ?? BestDiveTarget?.Actor,
                    rotdStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

            if (ShouldUseStarcross(scStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Starcross,
                    TargetChoice(sc) ?? BestDiveTarget?.Actor,
                    scStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Starcross);

            if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
                Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr,
                    Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.ForcedOGCD, 0, GCD - 0.9f);

            if (ShouldUseTrueNorth(strategy.Option(Track.TrueNorth).As<TrueNorthStrategy>(), primaryTarget?.Actor))
                QueueOGCD(AID.TrueNorth, Player, OGCDPriority.TrueNorth);
        }

        if (ShouldUsePiercingTalon(primaryTarget?.Actor, ptStrat))
            QueueGCD(AID.PiercingTalon,
                TargetChoice(pt) ?? primaryTarget?.Actor,
                ptStrat is PiercingTalonStrategy.Force or PiercingTalonStrategy.ForceEX
                ? GCDPriority.ForcedGCD : GCDPriority.NormalGCD);
        #endregion

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

    #region Rotation Helpers

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

    #endregion

    #region Cooldown Helpers

    #region Buffs
    private bool ShouldUseLanceCharge(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && canLC && powerLeft > 0,
        OGCDStrategy.Force => canLC,
        OGCDStrategy.AnyWeave => canLC && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canLC && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canLC && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBattleLitany(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && canBL && powerLeft > 0,
        OGCDStrategy.Force => canBL,
        OGCDStrategy.AnyWeave => canBL && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canBL && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canBL && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseLifeSurge(SurgeStrategy strategy, Actor? target) => strategy switch
    {
        SurgeStrategy.Automatic => Player.InCombat && target != null && canLS && hasLC && !PlayerHasEffect(SID.LifeSurge) &&
            (TotalCD(AID.LifeSurge) < 40 || TotalCD(AID.BattleLitany) > 50) &&
            (ComboLastMove is AID.WheelingThrust or AID.FangAndClaw && Unlocked(AID.Drakesbane) ||
            ComboLastMove is AID.VorpalThrust or AID.LanceBarrage && Unlocked(AID.FullThrust)),
        SurgeStrategy.Force => canLS,
        SurgeStrategy.ForceWeave => canLS && CanWeaveIn,
        SurgeStrategy.ForceNextOpti => canLS &&
            (ComboLastMove is AID.WheelingThrust or AID.FangAndClaw && Unlocked(AID.Drakesbane) ||
            ComboLastMove is AID.VorpalThrust or AID.LanceBarrage && Unlocked(AID.FullThrust)),
        SurgeStrategy.ForceNextOptiWeave => canLS && CanWeaveIn &&
            (ComboLastMove is AID.WheelingThrust or AID.FangAndClaw && Unlocked(AID.Drakesbane) ||
            ComboLastMove is AID.VorpalThrust or AID.LanceBarrage && Unlocked(AID.FullThrust)),
        SurgeStrategy.Delay => false,
        _ => false
    };
    #endregion

    #region Dives
    private bool ShouldUseDragonfireDive(DragonfireStrategy strategy, Actor? target) => strategy switch
    {
        DragonfireStrategy.Automatic => Player.InCombat && target != null && In20y(target) && canDD && hasLC && hasBL && hasLOTD,
        DragonfireStrategy.Force => canDD,
        DragonfireStrategy.ForceEX => canDD,
        DragonfireStrategy.ForceWeave => canDD && CanWeaveIn,
        DragonfireStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseJump(JumpStrategy strategy, Actor? target) => strategy switch
    {
        JumpStrategy.Automatic => Player.InCombat && target != null && In20y(target) && canJump && (lcLeft > 0 || hasLC || lcCD is < 35 and > 17),
        JumpStrategy.ForceEX => canJump,
        JumpStrategy.ForceEX2 => canJump,
        JumpStrategy.ForceWeave => canJump && CanWeaveIn,
        JumpStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseStardiver(StardiverStrategy strategy, Actor? target) => strategy switch
    {
        StardiverStrategy.Automatic => Player.InCombat && target != null && In20y(target) && canSD && hasLOTD,
        StardiverStrategy.Force => canSD,
        StardiverStrategy.ForceEX => canSD,
        StardiverStrategy.ForceWeave => canSD && CanWeaveIn,
        StardiverStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseMirageDive(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && In20y(target) && canMD,
        OGCDStrategy.Force => canMD,
        OGCDStrategy.AnyWeave => canMD && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canMD && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canMD && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    #endregion

    #region Spears
    private bool ShouldUseGeirskogul(GeirskogulStrategy strategy, Actor? target) => strategy switch
    {
        GeirskogulStrategy.Automatic => Player.InCombat && In15y(target) && canGeirskogul && hasLC,
        GeirskogulStrategy.Force => canGeirskogul,
        GeirskogulStrategy.ForceEX => canGeirskogul,
        GeirskogulStrategy.ForceWeave => canGeirskogul && CanWeaveIn,
        GeirskogulStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseNastrond(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && In15y(target) && canNastrond,
        OGCDStrategy.Force => canNastrond,
        OGCDStrategy.AnyWeave => canNastrond && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canNastrond && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canNastrond && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseWyrmwindThrust(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && In15y(target) && canWT && lcCD > GCDLength * 2,
        OGCDStrategy.Force => canWT,
        OGCDStrategy.AnyWeave => canWT && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canWT && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canWT && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    #endregion

    private bool ShouldUseRiseOfTheDragon(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && In20y(target) && canROTD,
        OGCDStrategy.Force => canROTD,
        OGCDStrategy.AnyWeave => canROTD && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canROTD && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canROTD && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseStarcross(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && In20y(target) && canSC,
        OGCDStrategy.Force => canSC,
        OGCDStrategy.AnyWeave => canSC && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canSC && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canSC && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUsePiercingTalon(Actor? target, PiercingTalonStrategy strategy) => strategy switch
    {
        PiercingTalonStrategy.AllowEX => Player.InCombat && target != null && !In3y(target) && PlayerHasEffect(SID.EnhancedPiercingTalon),
        PiercingTalonStrategy.Allow => Player.InCombat && target != null && !In3y(target),
        PiercingTalonStrategy.Force => true,
        PiercingTalonStrategy.ForceEX => PlayerHasEffect(SID.EnhancedPiercingTalon),
        PiercingTalonStrategy.Forbid => false,
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => lcCD <= GCD * 2 && blCD <= GCD * 2,
        PotionStrategy.Immediate => true,
        _ => false
    };
    private bool ShouldUseTrueNorth(TrueNorthStrategy strategy, Actor? target) => strategy switch
    {
        TrueNorthStrategy.Automatic => target != null && Player.InCombat && !PlayerHasEffect(SID.TrueNorth) && GCD < 1.25f &&
            (!IsOnRear(target) && ComboLastMove is AID.Disembowel or AID.SpiralBlow or AID.ChaosThrust or AID.ChaoticSpring ||
            !IsOnFlank(target) && ComboLastMove is AID.HeavensThrust or AID.FullThrust),
        TrueNorthStrategy.ASAP => target != null && Player.InCombat && !PlayerHasEffect(SID.TrueNorth) &&
            (!IsOnRear(target) && ComboLastMove is AID.Disembowel or AID.SpiralBlow or AID.ChaosThrust or AID.ChaoticSpring ||
            !IsOnFlank(target) && ComboLastMove is AID.HeavensThrust or AID.FullThrust),
        TrueNorthStrategy.Flank => target != null && Player.InCombat && !PlayerHasEffect(SID.TrueNorth) && GCD < 1.25f &&
            !IsOnFlank(target) && ComboLastMove is AID.HeavensThrust or AID.FullThrust,
        TrueNorthStrategy.Rear => target != null && Player.InCombat && !PlayerHasEffect(SID.TrueNorth) && GCD < 1.25f &&
            !IsOnRear(target) && ComboLastMove is AID.Disembowel or AID.SpiralBlow or AID.ChaosThrust or AID.ChaoticSpring,
        TrueNorthStrategy.Force => !PlayerHasEffect(SID.TrueNorth),
        TrueNorthStrategy.Delay => false,
        _ => false
    };
    #endregion
}
