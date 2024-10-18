using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiDRG(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    //Enum representing different actions tracked for Cooldown Planner execution
    public enum Track
    {
        AoE,                   //Area of Effect actions
        Burst,                 //Burst actions
        Potion,                //Potion usage
        LifeSurge,             //Life Surge ability
        Jump,                  //Jump ability
        DragonfireDive,        //Dragonfire Dive ability
        Geirskogul,            //Geirskogul ability
        Stardiver,             //Stardiver ability
        PiercingTalon,         //Piercing Talon ability
        LanceCharge,           //Lance Charge ability
        BattleLitany,          //Battle Litany ability
        MirageDive,            //Mirage Dive ability
        Nastrond,              //Nastrond ability
        WyrmwindThrust,        //Wyrmwind Thrust ability
        RiseOfTheDragon,       //Rise of the Dragon ability
        Starcross              //Starcross ability
    }

    //specifying AoE strategy preferences
    public enum AOEStrategy
    {
        UseST,                 //Use single-target abilities
        ForceST,               //Force single-target abilities
        Force123ST,            //Force single-target 123 combo
        ForceBuffsST,          //Force single-target buffs combo
        UseAoE,                //Use Area of Effect abilities
        ForceAoE,              //Force Area of Effect abilities
        Auto,                  //Auto-determined strategy
        AutoFinishCombo        //Auto AoE finish combo if conditions met
    }

    //burst strategy options
    public enum BurstStrategy
    {
        Automatic,             //Automatic burst under general conditions
        Conserve,              //Conserve burst for later
        UnderRaidBuffs,        //Burst during raid buffs
        UnderPotion            //Burst when a potion is active
    }

    //potion usage strategies
    public enum PotionStrategy
    {
        Manual,                //Manual potion usage
        AlignWithRaidBuffs,    //Align potion use with raid buffs
        Immediate              //Use potion immediately
    }

    //Piercing Talon strategy
    public enum PiercingTalonStrategy
    {
        OpenerRanged,          //Use Piercing Talon as a ranged opener
        Opener,                //Use Piercing Talon as an opener
        Force,                 //Force use of Piercing Talon
        Ranged,                //Use Piercing Talon for ranged situations
        Forbid                 //Forbid the use of Piercing Talon
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

    //general offensive strategies
    public enum OffensiveStrategy
    {
        Automatic,             //Automatically use offensive abilities
        Force,                 //Force offensive abilities
        Delay                  //Delay offensive abilities
    }

    public static RotationModuleDefinition Definition()
    {
        //Module title & signature
        var res = new RotationModuleDefinition("DRG (Akechi)", "Standard Rotation Module", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.LNC, Class.DRG), 100);

        //Custom strategies
        //Targeting strategy
        res.Define(Track.AoE).As<AOEStrategy>("Combo Option", "AoE", uiPriority: 200)
            .AddOption(AOEStrategy.UseST, "ST", "Use Single-Target rotation")
            .AddOption(AOEStrategy.ForceST, "Force ST", "Force Single-Target rotation")
            .AddOption(AOEStrategy.Force123ST, "Only 1-2-3 ST", "Force only ST 1-2-3 rotation (No Buffs)")
            .AddOption(AOEStrategy.ForceBuffsST, "Only 1-4-5 ST", "Force only ST 1-4-5 rotation (No 1/1.5-2-3)")
            .AddOption(AOEStrategy.UseAoE, "AoE", "Use AoE rotation if 3+ targets are present")
            .AddOption(AOEStrategy.ForceAoE, "Force AoE", "Force AoE rotation, even if less than 3 targets")
            .AddOption(AOEStrategy.Auto, "Automatic", "Use AoE rotation if 3+ targets are present, otherwise use ST rotation")
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto Finish Combo", "Use AoE if 3+ targets, otherwise finish ST combo before switching");

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
            .AddAssociatedActions(DRG.AID.LifeSurge);

        //Jump strategy
        res.Define(Track.Jump).As<JumpStrategy>("Jump", uiPriority: 110)
            .AddOption(JumpStrategy.Automatic, "Automatic", "Use Jump normally")
            .AddOption(JumpStrategy.Force, "Force Jump", "Force Jump usage", 30, 0, ActionTargets.Self, 30, 67)
            .AddOption(JumpStrategy.ForceEX, "Force Jump (EX)", "Force Jump usage (Grants Dive Ready buff)", 30, 15, ActionTargets.Self, 68, 74)
            .AddOption(JumpStrategy.ForceEX2, "Force High Jump", "Force High Jump usage", 30, 15, ActionTargets.Self, 75)
            .AddOption(JumpStrategy.Delay, "Delay", "Delay Jump usage", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(DRG.AID.Jump, DRG.AID.HighJump);

        //Dragonfire Dive strategy
        res.Define(Track.DragonfireDive).As<DragonfireStrategy>("Dragonfire Dive", "D.Dive", uiPriority: 150)
            .AddOption(DragonfireStrategy.Automatic, "Automatic", "Use Dragonfire Dive normally")
            .AddOption(DragonfireStrategy.Force, "Force", "Force Dragonfire Dive usage", 120, 0, ActionTargets.Hostile, 50, 91)
            .AddOption(DragonfireStrategy.ForceEX, "ForceEX", "Force Dragonfire Dive (Grants Dragon's Flight)", 120, 30, ActionTargets.Hostile, 92)
            .AddOption(DragonfireStrategy.Delay, "Delay", "Delay Dragonfire Dive usage", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(DRG.AID.DragonfireDive);

        //Geirskogul strategy
        res.Define(Track.Geirskogul).As<GeirskogulStrategy>("Geirskogul", "Geirs.", uiPriority: 130)
            .AddOption(GeirskogulStrategy.Automatic, "Automatic", "Use Geirskogul normally")
            .AddOption(GeirskogulStrategy.Force, "Force", "Force Geirskogul usage", 60, 0, ActionTargets.Hostile, 60, 69)
            .AddOption(GeirskogulStrategy.ForceEX, "ForceEX", "Force Geirskogul (Grants Life of the Dragon & 3x Nastrond)", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(GeirskogulStrategy.Delay, "Delay", "Delay Geirskogul usage", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(DRG.AID.Geirskogul);

        //Stardiver strategy
        res.Define(Track.Stardiver).As<StardiverStrategy>("Stardiver", "S.diver", uiPriority: 140)
            .AddOption(StardiverStrategy.Automatic, "Automatic", "Use Stardiver normally")
            .AddOption(StardiverStrategy.Force, "Force", "Force Stardiver usage", 30, 0, ActionTargets.Hostile, 80, 99)
            .AddOption(StardiverStrategy.ForceEX, "ForceEX", "Force Stardiver (Grants Starcross Ready)", 30, 0, ActionTargets.Hostile, 100)
            .AddOption(StardiverStrategy.Delay, "Delay", "Delay Stardiver usage", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(DRG.AID.Stardiver);

        //Piercing Talon strategy
        res.Define(Track.PiercingTalon).As<PiercingTalonStrategy>("Piercing Talon", "Talon", uiPriority: 20)
            .AddOption(PiercingTalonStrategy.OpenerRanged, "Opener Ranged", "Use Piercing Talon as the first GCD, only if outside melee range")
            .AddOption(PiercingTalonStrategy.Opener, "Opener", "Use Piercing Talon as the first GCD, regardless of range")
            .AddOption(PiercingTalonStrategy.Force, "Force", "Force Piercing Talon usage ASAP (even in melee range)")
            .AddOption(PiercingTalonStrategy.Ranged, "Ranged", "Use Piercing Talon if outside melee range")
            .AddOption(PiercingTalonStrategy.Forbid, "Forbid", "Do not use Piercing Talon at all")
            .AddAssociatedActions(DRG.AID.PiercingTalon);

        //Offensive Strategies
        //Lance Charge strategy
        res.Define(Track.LanceCharge).As<OffensiveStrategy>("Lance Charge", "L.Charge", uiPriority: 165)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Lance Charge normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Lance Charge usage ASAP (even during downtime)", 60, 20, ActionTargets.Self, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Lance Charge usage", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(DRG.AID.LanceCharge);

        //Battle Litany strategy
        res.Define(Track.BattleLitany).As<OffensiveStrategy>("Battle Litany", "B.Litany", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Battle Litany normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Battle Litany usage ASAP (even during downtime)", 120, 20, ActionTargets.Self, 52)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Battle Litany usage", 0, 0, ActionTargets.None, 52)
            .AddAssociatedActions(DRG.AID.BattleLitany);

        //Mirage Dive strategy
        res.Define(Track.MirageDive).As<OffensiveStrategy>("Mirage Dive", "M.Dive", uiPriority: 105)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Mirage Dive normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Mirage Dive usage", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Mirage Dive usage", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(DRG.AID.MirageDive);

        //Nastrond strategy
        res.Define(Track.Nastrond).As<OffensiveStrategy>("Nastrond", "Nast.", uiPriority: 125)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Nastrond normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Nastrond usage", 0, 2, ActionTargets.Hostile, 70)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Nastrond usage", 0, 0, ActionTargets.None, 70)
            .AddAssociatedActions(DRG.AID.Nastrond);

        //Wyrmwind Thrust strategy
        res.Define(Track.WyrmwindThrust).As<OffensiveStrategy>("Wyrmwind Thrust", "W.Thrust", uiPriority: 120)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Wyrmwind Thrust normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Wyrmwind Thrust usage ASAP", 0, 10, ActionTargets.Hostile, 90)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Wyrmwind Thrust usage", 0, 0, ActionTargets.None, 90)
            .AddAssociatedActions(DRG.AID.WyrmwindThrust);

        //Rise Of The Dragon strategy
        res.Define(Track.RiseOfTheDragon).As<OffensiveStrategy>("Rise Of The Dragon", "RotD", uiPriority: 145)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Rise Of The Dragon normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Rise Of The Dragon usage", 0, 0, ActionTargets.Hostile, 92)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Rise Of The Dragon usage", 0, 0, ActionTargets.None, 92)
            .AddAssociatedActions(DRG.AID.RiseOfTheDragon);

        //Starcross strategy
        res.Define(Track.Starcross).As<OffensiveStrategy>("Starcross", "S.cross", uiPriority: 135)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Starcross normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force Starcross usage", 0, 0, ActionTargets.Self, 100)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay Starcross usage", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(DRG.AID.Starcross);

        return res;
    }

    public enum GCDPriority //Priority for GCDs used
    {
        None = 0,
        Combo123 = 350,
        NormalGCD = 500,
        ForcedGCD = 900,
    }

    public enum OGCDPriority //Priority for oGCDs used
    {
        None = 0,
        //Flexible
        MirageDive = 500,
        Nastrond = 540,
        RiseOfTheDragon = 550,
        Starcross = 550,
        WyrmwindThrust = 590,
        Stardiver = 600,
        //Non-flexible
        Jump = 680,
        DragonfireDive = 690,
        Geirskogul = 700,
        Buffs = 800,
        ForcedOGCD = 900,
    }

    private int focusCount;

    private float GCDLength;
    private float blCD;
    private float lcLeft;
    private float lcCD;
    private float powerLeft;

    public float chaosLeft;
    private float PotionLeft;
    private float RaidBuffsLeft;
    private float RaidBuffsIn;

    public float BurstWindowLeft;
    public float BurstWindowIn;

    private bool hasLOTD;
    private bool hasLC;
    private bool hasBL;
    private bool hasMirage;
    private bool hasFlight;
    private bool hasCross;
    private bool hasNastrond;
    private bool canCharge;
    private bool canLitany;
    private bool canSurge;
    private bool canJump;
    private bool canDragonfire;
    private bool canGeirskogul;
    private bool canMirage;
    private bool canNastrond;
    private bool canStardive;
    private bool canThrust;
    private bool canRise;
    private bool canCross;

    public float downtimeIn;

    public DRG.AID NextGCD; //Next global cooldown action to be used
    private GCDPriority NextGCDPrio; //Priority of the next GCD, used for decision making on cooldowns

    //Check if the desired ability is unlocked
    private bool Unlocked(DRG.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));

    //Get remaining cooldown time for the specified action
    private float CD(DRG.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    //Check if we can fit an additional GCD within the provided deadline
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline;

    //Get the last action used in the combo sequence
    private DRG.AID ComboLastMove => (DRG.AID)World.Client.ComboState.Action;

    //Check if the target is within melee range (3 yalms)
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 3;

    //Check if the target is within 5 yalms
    private bool In15y(Actor? target) => Player.DistanceToHitbox(target) <= 14.75;

    //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool ActionReady(DRG.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f;

    //Check if this is the first GCD in combat
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    //Returns the number of targets hit by AoE within a 5-yalm radius around the player
    private int NumTargetsHitByAoE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    //Checks if the potion should be used before raid buffs expire
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f;

    //Checks if Status effect is on self
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var gauge = World.Client.GetGauge<DragoonGauge>();  //Get Dragoon gauge
        focusCount = gauge.FirstmindsFocusCount;  //Update focus count
        hasLOTD = gauge.LotdTimer > 0;  //Check for LOTD
        lcCD = CD(DRG.AID.LanceCharge);  //Get Lance Charge cooldown
        lcLeft = SelfStatusLeft(DRG.SID.LanceCharge);  //Get remaining Lance Charge status
        powerLeft = SelfStatusLeft(DRG.SID.PowerSurge);  //Get remaining Power Surge status
        blCD = CD(DRG.AID.BattleLitany);  //Get Battle Litany cooldown
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);  //Calculate GCD
        hasMirage = HasEffect(DRG.SID.DiveReady);  //Check if Mirage Dive is ready
        hasNastrond = HasEffect(DRG.SID.NastrondReady);  //Check if Nastrond is ready
        hasLC = lcCD is >= 40 and <= 60;  //Checks for Lance Charge buff
        hasBL = blCD is >= 100 and <= 120;  //Checks for Battle Litany buff
        hasFlight = HasEffect(DRG.SID.DragonsFlight);  //Check if Dragon's Flight is active
        hasCross = HasEffect(DRG.SID.StarcrossReady);  //Check if Starcross is ready

        //Ability readiness checks
        canCharge = Unlocked(DRG.AID.LanceCharge) && ActionReady(DRG.AID.LanceCharge);
        canLitany = Unlocked(DRG.AID.BattleLitany) && ActionReady(DRG.AID.BattleLitany);
        canSurge = Unlocked(DRG.AID.LifeSurge) && CD(DRG.AID.LifeSurge) >= 40;
        canJump = Unlocked(DRG.AID.Jump) && ActionReady(DRG.AID.Jump);
        canDragonfire = Unlocked(DRG.AID.DragonfireDive) && ActionReady(DRG.AID.DragonfireDive);
        canGeirskogul = Unlocked(DRG.AID.Geirskogul) && ActionReady(DRG.AID.Geirskogul);
        canMirage = Unlocked(DRG.AID.MirageDive) && hasMirage;
        canNastrond = Unlocked(DRG.AID.Nastrond) && hasNastrond;
        canStardive = Unlocked(DRG.AID.Stardiver) && ActionReady(DRG.AID.Stardiver);
        canThrust = Unlocked(DRG.AID.WyrmwindThrust) && ActionReady(DRG.AID.WyrmwindThrust);
        canRise = Unlocked(DRG.AID.RiseOfTheDragon) && hasFlight;
        canCross = Unlocked(DRG.AID.Starcross) && hasCross;

        downtimeIn = Manager.Planner?.EstimateTimeToNextDowntime().Item2 ?? float.MaxValue;  //Estimate downtime until next action
        chaosLeft = MathF.Max(
            StatusDetails(primaryTarget, DRG.SID.ChaosThrust, Player.InstanceID).Left,
            StatusDetails(primaryTarget, DRG.SID.ChaoticSpring, Player.InstanceID).Left
        );  //Get remaining time for Chaos Thrust and Chaotic Spring
        PotionLeft = PotionStatusLeft();  //Get remaining potion status
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);  //Estimate remaining raid buffs

        NextGCD = DRG.AID.None;  //Set next GCD ability
        NextGCDPrio = GCDPriority.None;  //Set next GCD priority

        var AOEStrategy = strategy.Option(Track.AoE).As<AOEStrategy>();  //Get AoE strategy
        var AoETargets = AOEStrategy switch  //Determine AoE target count
        {
            AOEStrategy.UseST => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AOEStrategy.ForceST => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AOEStrategy.Force123ST => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AOEStrategy.ForceBuffsST => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AOEStrategy.UseAoE => NumTargetsHitByAoE() > 0 ? 100 : 0,
            AOEStrategy.ForceAoE => NumTargetsHitByAoE() > 0 ? 100 : 0,
            _ => NumTargetsHitByAoE()
        };

        var burst = strategy.Option(Track.Burst);  //Get burst strategy
        var burstStrategy = burst.As<BurstStrategy>();
        var hold = burstStrategy == BurstStrategy.Conserve;  //Check if burst should be conserved

        //Set burst window timings
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => (RaidBuffsIn, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft)),
            BurstStrategy.UnderRaidBuffs => (RaidBuffsIn, RaidBuffsLeft),
            BurstStrategy.UnderPotion => (PotionCD, PotionLeft),
            _ => (0, 0)
        };

        var (comboAction, comboPrio) = ComboActionPriority(AOEStrategy, AoETargets, burstStrategy, burst.Value.ExpireIn);  //Get combo action and priority
        QueueGCD(comboAction, comboAction is DRG.AID.TrueThrust or DRG.AID.RaidenThrust or DRG.AID.DoomSpike or DRG.AID.DraconianFury ? primaryTarget : primaryTarget, comboPrio);

        //Force specific actions
        if (AOEStrategy == AOEStrategy.ForceST)
            QueueGCD(NextFullST(), primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy == AOEStrategy.Force123ST)
            QueueGCD(UseOnly123ST(), primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy == AOEStrategy.ForceBuffsST)
            QueueGCD(UseOnly145ST(), primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy == AOEStrategy.ForceAoE)
            QueueGCD(NextFullAoE(), primaryTarget, GCDPriority.ForcedGCD);

        //Execute Lance Charge if available
        var lcStrat = strategy.Option(Track.LanceCharge).As<OffensiveStrategy>();
        if (!hold && ShouldUseLanceCharge(lcStrat, primaryTarget))
            QueueOGCD(DRG.AID.LanceCharge, Player, lcStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

        //Execute Battle Litany if available
        var blStrat = strategy.Option(Track.BattleLitany).As<OffensiveStrategy>();
        if (!hold && ShouldUseBattleLitany(blStrat, primaryTarget))
            QueueOGCD(DRG.AID.BattleLitany, Player, blStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

        //Execute Life Surge if conditions met
        var lsStrat = strategy.Option(Track.LifeSurge).As<SurgeStrategy>();
        if (!hold && GCD < 1.25f && ShouldUseLifeSurge(lsStrat, primaryTarget))
            QueueOGCD(DRG.AID.LifeSurge, Player, lsStrat is SurgeStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

        //Execute Jump ability if available
        var jumpStrat = strategy.Option(Track.Jump).As<JumpStrategy>();
        if (!hold && ShouldUseJump(jumpStrat, primaryTarget))
            QueueOGCD(Unlocked(DRG.AID.HighJump) ? DRG.AID.HighJump : DRG.AID.Jump, primaryTarget, jumpStrat == JumpStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Jump);

        //Execute Dragonfire Dive if available
        var ddStrat = strategy.Option(Track.DragonfireDive).As<DragonfireStrategy>();
        if (!hold && ShouldUseDragonfireDive(ddStrat, primaryTarget))
            QueueOGCD(DRG.AID.DragonfireDive, primaryTarget, ddStrat is DragonfireStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.DragonfireDive);

        //Execute Geirskogul if available
        var geirskogul = strategy.Option(Track.Geirskogul).As<GeirskogulStrategy>();
        if (!hold && ShouldUseGeirskogul(geirskogul, primaryTarget))
            QueueOGCD(DRG.AID.Geirskogul, primaryTarget, geirskogul == GeirskogulStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Geirskogul);

        //Execute Mirage Dive if available
        var mirageStrat = strategy.Option(Track.MirageDive).As<OffensiveStrategy>();
        if (!hold && ShouldUseMirageDive(mirageStrat, primaryTarget))
            QueueOGCD(DRG.AID.MirageDive, primaryTarget, mirageStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.MirageDive);

        //Execute Nastrond if available
        var nastrondStrat = strategy.Option(Track.Nastrond).As<OffensiveStrategy>();
        if (!hold && ShouldUseNastrond(nastrondStrat, primaryTarget))
            QueueOGCD(DRG.AID.Nastrond, primaryTarget, nastrondStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Nastrond);

        //Execute Stardiver if available
        var sdStrat = strategy.Option(Track.Stardiver).As<StardiverStrategy>();
        if (!hold && ShouldUseStardiver(sdStrat, primaryTarget))
            QueueOGCD(DRG.AID.Stardiver, primaryTarget, sdStrat == StardiverStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Stardiver);

        //Execute Wyrmwind Thrust if available
        var wtStrat = strategy.Option(Track.WyrmwindThrust).As<OffensiveStrategy>();
        if (!hold && ShouldUseWyrmwindThrust(wtStrat, primaryTarget))
            QueueOGCD(DRG.AID.WyrmwindThrust, primaryTarget, wtStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.WyrmwindThrust);

        //Execute Rise of the Dragon if available
        var riseStrat = strategy.Option(Track.RiseOfTheDragon).As<OffensiveStrategy>();
        if (!hold && ShouldUseRiseOfTheDragon(riseStrat, primaryTarget))
            QueueOGCD(DRG.AID.RiseOfTheDragon, primaryTarget, riseStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Buffs);

        //Execute Starcross if available
        var crossStrat = strategy.Option(Track.Starcross).As<OffensiveStrategy>();
        if (!hold && ShouldUseStarcross(crossStrat, primaryTarget))
            QueueOGCD(DRG.AID.Starcross, primaryTarget, crossStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Starcross);

        //Execute Piercing Talon if available
        var ptStrat = strategy.Option(Track.PiercingTalon).As<PiercingTalonStrategy>();
        if (ShouldUsePiercingTalon(primaryTarget, ptStrat))
            QueueGCD(DRG.AID.PiercingTalon, primaryTarget, ptStrat == PiercingTalonStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.NormalGCD);

        //Execute Potion if available
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.ForcedOGCD, 0, GCD - 0.9f);

    }

    //QueueGCD execution
    private void QueueGCD(DRG.AID aid, Actor? target, GCDPriority prio)
    {
        if (prio != GCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + (int)prio);
            if (prio > NextGCDPrio)
            {
                NextGCD = aid;
                NextGCDPrio = prio;
            }
        }
    }

    //QueueOGCD execution
    private void QueueOGCD(DRG.AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Medium)
    {
        if (prio != OGCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
        }
    }

    //Determines the next skill in the single-target (ST) combo chain based on the last used action.
    private DRG.AID NextFullST() => ComboLastMove switch
    {
        //Starting combo with TrueThrust or RaidenThrust
        DRG.AID.TrueThrust or DRG.AID.RaidenThrust =>
            //If Disembowel is unlocked and power is low or Chaotic Spring is 0, use Disembowel or SpiralBlow, else VorpalThrust or LanceBarrage
            (Unlocked(DRG.AID.Disembowel) && (powerLeft <= GCDLength * 6 || chaosLeft <= GCDLength * 4))
            ? (Unlocked(DRG.AID.SpiralBlow) ? DRG.AID.SpiralBlow : DRG.AID.Disembowel)
            : (Unlocked(DRG.AID.LanceBarrage) ? DRG.AID.LanceBarrage : DRG.AID.VorpalThrust),

        //Follow-up after Disembowel or SpiralBlow
        DRG.AID.Disembowel or DRG.AID.SpiralBlow =>
            Unlocked(DRG.AID.ChaoticSpring) ? DRG.AID.ChaoticSpring //Use ChaoticSpring if unlocked
            : Unlocked(DRG.AID.ChaosThrust) ? DRG.AID.ChaosThrust //Use ChaosThrust if unlocked
            : DRG.AID.TrueThrust, //Return to TrueThrust otherwise

        //Follow-up after VorpalThrust or LanceBarrage
        DRG.AID.VorpalThrust or DRG.AID.LanceBarrage =>
            (Unlocked(DRG.AID.HeavensThrust) ? DRG.AID.HeavensThrust //Use HeavensThrust if unlocked
            : Unlocked(DRG.AID.FullThrust) ? DRG.AID.FullThrust //Use FullThrust if unlocked
            : DRG.AID.TrueThrust), //Return to TrueThrust otherwise

        //After FullThrust or HeavensThrust in the combo
        DRG.AID.FullThrust or DRG.AID.HeavensThrust =>
            (Unlocked(DRG.AID.FangAndClaw) ? DRG.AID.FangAndClaw //Use FangAndClaw if unlocked
            : DRG.AID.TrueThrust), //Return to TrueThrust otherwise

        //After ChaosThrust or ChaoticSpring in the combo
        DRG.AID.ChaosThrust or DRG.AID.ChaoticSpring =>
            (Unlocked(DRG.AID.WheelingThrust) ? DRG.AID.WheelingThrust //Use WheelingThrust if unlocked
            : DRG.AID.TrueThrust), //Return to TrueThrust otherwise

        //After WheelingThrust or FangAndClaw in the combo
        DRG.AID.WheelingThrust or DRG.AID.FangAndClaw =>
            (Unlocked(DRG.AID.Drakesbane) ? DRG.AID.Drakesbane //Use Drakesbane if unlocked
            : DRG.AID.TrueThrust), //Return to TrueThrust otherwise

        //If no combo active and Draconian Fire buff is up, use RaidenThrust
        _ => HasEffect(DRG.SID.DraconianFire) ? DRG.AID.RaidenThrust //RaidenThrust if DraconianFire is active
            : DRG.AID.TrueThrust, //No combo, start with TrueThrust
    };

    //Limits the combo sequence to just 1-2-3 ST skills, ignoring other unlocked actions.
    private DRG.AID UseOnly123ST() => ComboLastMove switch
    {
        //Start combo with TrueThrust
        DRG.AID.TrueThrust =>
            (Unlocked(DRG.AID.LanceBarrage) ? DRG.AID.LanceBarrage //LanceBarrage if unlocked
            : Unlocked(DRG.AID.VorpalThrust) ? DRG.AID.VorpalThrust //VorpalThrust otherwise
            : DRG.AID.TrueThrust), //Else return to TrueThrust

        //After VorpalThrust or LanceBarrage
        DRG.AID.VorpalThrust or DRG.AID.LanceBarrage =>
            (Unlocked(DRG.AID.HeavensThrust) ? DRG.AID.HeavensThrust //HeavensThrust if unlocked
            : Unlocked(DRG.AID.FullThrust) ? DRG.AID.FullThrust //FullThrust to end combo
            : DRG.AID.TrueThrust), //Else return to TrueThrust

        //After FullThrust or HeavensThrust
        DRG.AID.FullThrust or DRG.AID.HeavensThrust =>
            (Unlocked(DRG.AID.FangAndClaw) ? DRG.AID.FangAndClaw //FangAndClaw if unlocked
            : DRG.AID.TrueThrust), //Else return to TrueThrust

        //After WheelingThrust or FangAndClaw
        DRG.AID.WheelingThrust or DRG.AID.FangAndClaw =>
            (Unlocked(DRG.AID.Drakesbane) ? DRG.AID.Drakesbane //Drakesbane if unlocked
            : DRG.AID.TrueThrust), //Else return to TrueThrust

        //If Draconian Fire buff is up, use RaidenThrust
        _ => HasEffect(DRG.SID.DraconianFire) ? DRG.AID.RaidenThrust //RaidenThrust if DraconianFire is active
            : DRG.AID.TrueThrust, //No combo, start with TrueThrust
    };

    //Limits the combo sequence to 1-4-5 ST skills, focusing on Disembowel and Chaos/ChaoticSpring.
    private DRG.AID UseOnly145ST() => ComboLastMove switch
    {
        //Start combo with TrueThrust
        DRG.AID.TrueThrust =>
            (Unlocked(DRG.AID.Disembowel) && powerLeft <= GCD * 6)
            ? (Unlocked(DRG.AID.SpiralBlow) ? DRG.AID.SpiralBlow : DRG.AID.Disembowel) //Disembowel/SpiralBlow if unlocked
            : DRG.AID.TrueThrust, //Else return to TrueThrust

        //After Disembowel or SpiralBlow
        DRG.AID.Disembowel or DRG.AID.SpiralBlow =>
            Unlocked(DRG.AID.ChaoticSpring) ? DRG.AID.ChaoticSpring //ChaoticSpring if unlocked
            : (Unlocked(DRG.AID.ChaosThrust) ? DRG.AID.ChaosThrust //ChaosThrust if unlocked
            : DRG.AID.TrueThrust), //Else return to TrueThrust

        //After ChaosThrust or ChaoticSpring
        DRG.AID.ChaosThrust or DRG.AID.ChaoticSpring =>
            (Unlocked(DRG.AID.WheelingThrust) ? DRG.AID.WheelingThrust  //WheelingThrust if unlocked
            : DRG.AID.TrueThrust), //Else return to TrueThrust

        //After WheelingThrust or FangAndClaw
        DRG.AID.WheelingThrust or DRG.AID.FangAndClaw =>
            (Unlocked(DRG.AID.Drakesbane) ? DRG.AID.Drakesbane //Drakesbane if unlocked
            : DRG.AID.TrueThrust), //Else return to TrueThrust

        //If Draconian Fire buff is up, use RaidenThrust
        _ => HasEffect(DRG.SID.DraconianFire) ? DRG.AID.RaidenThrust //RaidenThrust if DraconianFire is active
            : DRG.AID.TrueThrust, //No combo, start with TrueThrust
    };

    //Determines the next action in the AoE combo based on the last action used.
    private DRG.AID NextFullAoE() => ComboLastMove switch
    {
        //Start AoE combo with DoomSpike
        DRG.AID.DoomSpike =>
            (Unlocked(DRG.AID.SonicThrust) ? DRG.AID.SonicThrust : DRG.AID.DoomSpike),  //SonicThrust if unlocked, else DoomSpike

        //Continue AoE combo with SonicThrust
        DRG.AID.SonicThrust =>
            (Unlocked(DRG.AID.CoerthanTorment) ? DRG.AID.CoerthanTorment : DRG.AID.DoomSpike),  //CoerthanTorment if unlocked, else DoomSpike

        //If Draconian Fire buff is up, use DraconianFury
        _ => HasEffect(DRG.SID.DraconianFire)
            ? (Unlocked(DRG.AID.DraconianFury) ? DRG.AID.DraconianFury : DRG.AID.DoomSpike)  //DraconianFury if unlocked, else DoomSpike
            : DRG.AID.DoomSpike,  //No DraconianFire active, default to DoomSpike
    };

    private (DRG.AID, GCDPriority) ComboActionPriority(AOEStrategy aoeStrategy, int AoETargets, BurstStrategy burstStrategy, float burstStrategyExpire)
    {
        //Determine how many combo steps are remaining based on the last action
        var comboStepsRemaining = ComboLastMove switch
        {
            //Single-target combo progression
            DRG.AID.TrueThrust or DRG.AID.HeavensThrust =>
                Unlocked(DRG.AID.Drakesbane) ? 4 :
                (Unlocked(DRG.AID.WheelingThrust) || Unlocked(DRG.AID.FangAndClaw)) ? 3 :
                (Unlocked(DRG.AID.FullThrust) || Unlocked(DRG.AID.HeavensThrust) || Unlocked(DRG.AID.ChaosThrust) || Unlocked(DRG.AID.ChaoticSpring)) ? 2 :
                (Unlocked(DRG.AID.VorpalThrust) || Unlocked(DRG.AID.Disembowel) || Unlocked(DRG.AID.LanceBarrage) || Unlocked(DRG.AID.SpiralBlow)) ? 1 : 0,

            //AoE combo progression
            DRG.AID.DoomSpike or DRG.AID.DraconianFury =>
                Unlocked(DRG.AID.CoerthanTorment) ? 2 : Unlocked(DRG.AID.SonicThrust) ? 1 : 0,
            _ => 0
        };

        //Check if we can fit the remaining GCD in the combo window
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining))
            comboStepsRemaining = 0;

        //Determine if we're in the middle of an AoE combo
        var doingAOECombo = ComboLastMove is DRG.AID.DoomSpike or DRG.AID.DraconianFury;

        //Decide if an AoE action is desirable based on the AoE strategy and number of targets
        var wantAOEAction = Unlocked(DRG.AID.DoomSpike) && aoeStrategy switch
        {
            AOEStrategy.UseST => false,
            AOEStrategy.ForceST => false,
            AOEStrategy.Force123ST => false,
            AOEStrategy.ForceBuffsST => false,
            AOEStrategy.UseAoE => true,
            AOEStrategy.ForceAoE => true,
            AOEStrategy.Auto => AoETargets >= 3,
            AOEStrategy.AutoFinishCombo => comboStepsRemaining > 0 ? doingAOECombo : AoETargets >= 3,
            _ => false
        };

        //Reset combo steps if switching between AoE and single-target combos
        if (comboStepsRemaining > 0 && wantAOEAction != doingAOECombo)
            comboStepsRemaining = 0;

        //Choose the next action based on whether we're focusing on AoE or single-target
        var nextAction = wantAOEAction ? NextFullAoE() : NextFullST();

        //Return combo priority based on the ability to fit GCDs and remaining combo steps
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining, 1))
            return (nextAction, GCDPriority.Combo123);

        return (nextAction, GCDPriority.Combo123); //If no combo steps are remaining or can fit GCD, return normal priority
    }

    //Determines when to use Lance Charge
    private bool ShouldUseLanceCharge(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Lance Charge automatically if the player is in combat, the target is valid, the action is ready, and there is power remaining
            Player.InCombat && target != null && canCharge && powerLeft > 0,
        OffensiveStrategy.Force => true, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Battle Litany
    private bool ShouldUseBattleLitany(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Battle Litany automatically if the player is in combat, the target is valid, the action is ready, and there is power remaining
            Player.InCombat && target != null && canLitany && powerLeft > 0,
        OffensiveStrategy.Force => true, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    private bool ShouldUseLifeSurge(SurgeStrategy strategy, Actor? target) => strategy switch
    {
        SurgeStrategy.Automatic => Player.InCombat && target != null && canSurge &&
            CD(DRG.AID.BattleLitany) > 50 && HasEffect(DRG.SID.LanceCharge) &&
            ((ComboLastMove == DRG.AID.WheelingThrust || ComboLastMove == DRG.AID.FangAndClaw) && Unlocked(DRG.AID.Drakesbane)) ||
            ((ComboLastMove == DRG.AID.VorpalThrust || ComboLastMove == DRG.AID.LanceBarrage) && Unlocked(DRG.AID.FullThrust)),
        SurgeStrategy.Force => true,
        SurgeStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Jump
    private bool ShouldUseJump(JumpStrategy strategy, Actor? target) => strategy switch
    {
        JumpStrategy.Automatic =>
            //Use Jump automatically if the player is in combat, the target is valid, and Lance Charge-related conditions are met
            Player.InCombat && target != null && canJump && (lcLeft > 0 || hasLC || lcCD is < 35 and > 17),
        JumpStrategy.ForceEX => true, //Always use in ForceEX strategy
        JumpStrategy.ForceEX2 => true, //Always use in ForceEX2 strategy
        JumpStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Dragonfire Dive
    private bool ShouldUseDragonfireDive(DragonfireStrategy strategy, Actor? target) => strategy switch
    {
        DragonfireStrategy.Automatic =>
            //Use Dragonfire Dive automatically if the player is in combat, the target is valid, and both Lance Charge and Battle Litany are active
            Player.InCombat && target != null && canDragonfire && hasLC && hasBL,
        DragonfireStrategy.Force => true, //Always use if forced
        DragonfireStrategy.ForceEX => true, //Always use in ForceEX strategy
        DragonfireStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Geirskogul
    private bool ShouldUseGeirskogul(GeirskogulStrategy strategy, Actor? target) => strategy switch
    {
        GeirskogulStrategy.Automatic =>
            //Use Geirskogul automatically if the player is in combat, the action is ready, the target is within 15y, and Lance Charge is active
            Player.InCombat && In15y(target) && canGeirskogul && hasLC,
        GeirskogulStrategy.Force => true, //Always use if forced
        GeirskogulStrategy.ForceEX => true, //Always use if forced
        GeirskogulStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Mirage Dive
    private bool ShouldUseMirageDive(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Mirage Dive automatically if the player is in combat, the target is valid, and Dive Ready effect is active
            Player.InCombat && target != null && canMirage,
        OffensiveStrategy.Force => true, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Nastrond
    private bool ShouldUseNastrond(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Nastrond automatically if the player is in combat, has Nastrond ready, the target is within 15y, and Lance Charge is active
            Player.InCombat && In15y(target) && canNastrond && hasLC,
        OffensiveStrategy.Force => true, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Stardiver
    private bool ShouldUseStardiver(StardiverStrategy strategy, Actor? target) => strategy switch
    {
        StardiverStrategy.Automatic =>
            //Use Stardiver automatically if the player is in combat, the target is valid, the action is ready, and Life of the Dragon (LOTD) is active
            Player.InCombat && target != null && canStardive && hasLOTD,
        StardiverStrategy.Force => true, //Always use if forced
        StardiverStrategy.ForceEX => true, //Always use if forced
        StardiverStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Wyrmwind Thrust
    private bool ShouldUseWyrmwindThrust(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Wyrmwind Thrust automatically if the player is in combat, the target is within 15y, and focus count is exactly 2
            Player.InCombat && target != null && In15y(target) && canThrust && focusCount is 2,
        OffensiveStrategy.Force => true, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Rise of the Dragon
    private bool ShouldUseRiseOfTheDragon(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Rise of the Dragon automatically if the player is in combat, the target is valid, and Dragon's Flight effect is active
            Player.InCombat && target != null && canRise,
        OffensiveStrategy.Force => true, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Starcross
    private bool ShouldUseStarcross(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            //Use Starcross automatically if the player is in combat, the target is valid, and Starcross Ready effect is active
            Player.InCombat && target != null && canCross,
        OffensiveStrategy.Force => true, //Always use if forced
        OffensiveStrategy.Delay => false, //Delay usage if strategy is set to delay
        _ => false
    };

    //Determines when to use Piercing Talon
    private bool ShouldUsePiercingTalon(Actor? target, PiercingTalonStrategy strategy) => strategy switch
    {
        PiercingTalonStrategy.OpenerRanged =>
            //Use Piercing Talon as the first GCD if the target is not within 3y range
            IsFirstGCD() && !In3y(target),
        PiercingTalonStrategy.Opener =>
            //Use Piercing Talon as the first GCD
            IsFirstGCD(),
        PiercingTalonStrategy.Force => true, //Always use if forced
        PiercingTalonStrategy.Ranged =>
            //Use Piercing Talon if the target is not within 3y range
            !In3y(target),
        PiercingTalonStrategy.Forbid => false, //Never use if forbidden
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
}
