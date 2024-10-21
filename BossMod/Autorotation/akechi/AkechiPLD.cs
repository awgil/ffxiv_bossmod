using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiPLD(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    //Actions tracked for Cooldown Planner execution
    public enum Track
    {
        AoE,              //Tracks both AoE and single-target actions
        Burst,            //Tracks burst damage actions
        Potion,           //Tracks potion usage
        Atonement,        //Tracks Atonement actions
        BladeCombo,       //Tracks Blade Combo actions
        Dash,             //Tracks the use of Intervene
        Ranged,           //Tracks ranged attacks
        FightOrFlight,    //Tracks Fight or Flight actions
        Requiescat,       //Tracks Requiescat actions
        SpiritsWithin,    //Tracks Spirits Within actions
        CircleOfScorn,    //Tracks Circle of Scorn actions
        GoringBlade,      //Tracks Goring Blade actions
        HolySpirit,       //Tracks Holy Spirit actions
        HolyCircle,       //Tracks Holy Circle actions
        BladeOfHonor,     //Tracks Blade of Honor actions
    }

    //Strategy definitions for AoE usage
    public enum AOEStrategy
    {
        UseST,            //Use single-target actions when appropriate
        ForceST,          //Force the use of single-target actions
        UseAoE,           //Use AoE actions when beneficial
        ForceAoE,         //Force the use of AoE actions
        Auto,             //Automatically decide based on target count; may break combos
        AutoFinishCombo,  //Automatically decide with a preference to finish combos
    }

    //Strategy definitions for burst damage
    public enum BurstStrategy
    {
        Automatic,        //Automatically execute burst actions when conditions are met
        Conserve,         //Conserve MP and cooldowns for strategic usage
        UnderRaidBuffs,   //Execute burst actions when under raid buffs for maximum effect
        UnderPotion,      //Execute burst actions while under potion effects
    }

    //Strategy definitions for potion usage
    public enum PotionStrategy
    {
        Manual,             //Use potions manually based on player discretion
        AlignWithRaidBuffs, //Align potion usage with the timing of raid buffs
        Immediate           //Use potions immediately when available
    }

    //Strategy definitions for Atonement usage
    public enum AtonementStrategy
    {
        Automatic,         //Automatically use Atonement when needed
        ForceAtonement,    //Force the use of Atonement regardless of other actions
        ForceSupplication, //Force use of Supplication
        ForceSepulchre,    //Force use of Sepulchre actions
        Delay              //Delay the use of Atonement for optimal timing
    }

    //Strategy definitions for Blade Combo actions
    public enum BladeComboStrategy
    {
        Automatic,        //Automatically execute Blade Combo when conditions are favorable
        ForceConfiteor,   //Force the use of Confiteor action
        ForceFaith,       //Force the use of Blade of Faith
        ForceTruth,       //Force the use of Blade of Truth
        ForceValor,       //Force the use of Blade of Valor
        Delay             //Delay the use of Confiteor and Blade Combo for timing
    }

    //Strategy definitions for dash actions
    public enum DashStrategy
    {
        Automatic,        //Automatically use Intervene as needed
        Force,            //Force the use of Intervene regardless of other factors
        Conserve1,        //Conserve one use of Intervene for later
        GapClose,         //Use Intervene to close gaps between targets
        Delay             //Delay the use of Intervene for strategic reasons
    }

    //Strategy definitions for ranged attacks
    public enum RangedStrategy
    {
        OpenerRanged,     //Use Shield Lob as part of the opening sequence
        OpenerRangedCast, //Use Holy Spirit as part of the opening sequence
        Opener,           //Use Shield Lob at the start of combat
        OpenerCast,       //Use Holy Spirit at the start of combat
        Force,            //Force Shield Lob when possible
        ForceCast,        //Force Holy Spirit when possible
        Ranged,           //Use Shield Lob for ranged attacks
        RangedCast,       //Use Holy Spirit for ranged attacks
        Forbid            //Prohibit the use of ranged attacks entirely
    }

    //Strategy definitions for offensive actions
    public enum OffensiveStrategy
    {
        Automatic,        //Automatically decide on offensive actions based on conditions
        Force,            //Force the use of offensive actions regardless of context
        Delay             //Delay offensive actions for strategic timing
    }

    public static RotationModuleDefinition Definition()
    {
        //Define the rotation module
        var res = new RotationModuleDefinition("PLD (Akechi)", "Standard Rotation Module", "Standard rotation (Akechi)", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.GLA, (int)Class.PLD), 100);

        //AoE Strategy: Manage AoE versus single-target rotations
        res.Define(Track.AoE).As<AOEStrategy>("AoE", uiPriority: 200)
            .AddOption(AOEStrategy.UseST, "Use ST", "Use single-target rotation")
            .AddOption(AOEStrategy.ForceST, "Use AoE", "Force single-target rotation")
            .AddOption(AOEStrategy.UseAoE, "Force ST", "Use AoE rotation")
            .AddOption(AOEStrategy.ForceAoE, "Force AoE", "Force AoE rotation")
            .AddOption(AOEStrategy.Auto, "Auto", "Choose AoE if 3+ targets; otherwise, use single-target")
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto Finish Combo", "Choose AoE if 3+ targets; otherwise, finish combo if possible");

        //Burst Strategy: Control burst actions based on situational needs
        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 190)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Spend cartridges optimally")
            .AddOption(BurstStrategy.Conserve, "Conserve", "Conserve MP and cooldowns")
            .AddOption(BurstStrategy.UnderRaidBuffs, "Under Raid Buffs", "Spend under raid buffs; conserve otherwise")
            .AddOption(BurstStrategy.UnderPotion, "Under Potion", "Spend under potion; conserve otherwise");

        //Potion Strategy: Manage potion usage
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 180)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use potions automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Align With Raid Buffs", "Align potion usage with raid buffs", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potions immediately when available", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        //Atonement Strategy: Control the use of Atonement abilities
        res.Define(Track.Atonement).As<AtonementStrategy>("Atonement", "Atone", uiPriority: 160)
            .AddOption(AtonementStrategy.Automatic, "Automatic", "Normal use of Atonement & it's combo")
            .AddOption(AtonementStrategy.ForceAtonement, "Force Atonement", "Force use of Atonement", 30, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSupplication, "Force Supplication", "Force use of Supplication", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSepulchre, "Force Sepulchre", "Force use of Sepulchre", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.Delay, "Delay", "Delay use of Atonement", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(PLD.AID.Atonement, PLD.AID.Supplication, PLD.AID.Sepulchre);

        //Blade Combo Strategy: Manage the Blade Combo actions
        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blade Combo", "Blades", uiPriority: 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatic", "Normal use of Confiteor & Blades Combo")
            .AddOption(BladeComboStrategy.ForceConfiteor, "Force", "Force use of Confiteor", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(BladeComboStrategy.ForceFaith, "Force Faith", "Force use of Blade of Faith", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceTruth, "Force Truth", "Force use of Blade of Truth", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceValor, "Force Valor", "Force use of Blade of Valor", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.Delay, "Delay", "Delay use of Confiteor & Blade Combo", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(PLD.AID.BladeOfFaith, PLD.AID.BladeOfTruth, PLD.AID.BladeOfValor);

        //Dash Strategy: Control the use of the Intervene ability
        res.Define(Track.Dash).As<DashStrategy>("Intervene", "Dash", uiPriority: 150)
            .AddOption(DashStrategy.Automatic, "Automatic", "Normal use of Intervene")
            .AddOption(DashStrategy.Force, "Force", "Force use of Intervene", 30, 0, ActionTargets.Hostile, 74)
            .AddOption(DashStrategy.Conserve1, "Conserve 1", "Conserve one use of Intervene for manual usage", 30, 0, ActionTargets.Hostile, 74)
            .AddOption(DashStrategy.GapClose, "Gap Close", "Use as gap closer if outside melee range", 30, 0, ActionTargets.None, 74)
            .AddOption(DashStrategy.Delay, "Delay", "Delay use of Intervene", 30, 0, ActionTargets.None, 74)
            .AddAssociatedActions(PLD.AID.Intervene);

        //Ranged Strategy: Manage ranged attacks when outside melee range
        res.Define(Track.Ranged).As<RangedStrategy>("Ranged", uiPriority: 20)
            .AddOption(RangedStrategy.OpenerRanged, "Opener Ranged", "Use Shield Lob as the first GCD if outside melee range")
            .AddOption(RangedStrategy.OpenerRangedCast, "Opener Ranged Cast", "Use Holy Spirit as the first GCD if outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.Opener, "Opener", "Use Shield Lob as the first GCD regardless of range")
            .AddOption(RangedStrategy.OpenerCast, "Opener Cast", "Use Holy Spirit as the first GCD regardless of range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.Force, "Force", "Always use Shield Lob regardless of conditions")
            .AddOption(RangedStrategy.ForceCast, "Force Cast", "Always use Holy Spirit regardless of conditions", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.Ranged, "Ranged", "Use Shield Lob when outside melee range")
            .AddOption(RangedStrategy.RangedCast, "Ranged Cast", "Use Holy Spirit when outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.Forbid, "Forbid", "Prohibit use of both ranged attacks")
            .AddAssociatedActions(PLD.AID.ShieldLob, PLD.AID.HolySpirit);

        //Fight or Flight Strategy: Manage offensive cooldowns
        res.Define(Track.FightOrFlight).As<OffensiveStrategy>("Fight or Flight", "FoF", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Fight or Flight normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Fight or Flight", 60, 20, ActionTargets.Self, 2)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Fight or Flight", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(PLD.AID.FightOrFlight);

        //Requiescat Strategy: Control the use of Requiescat ability
        res.Define(Track.Requiescat).As<OffensiveStrategy>("Requiescat", "Req", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Requiescat normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Requiescat / Imperator", 60, 20, ActionTargets.Self, 68)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Requiescat / Imperator", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(PLD.AID.Requiescat, PLD.AID.Imperator);

        //Spirits Within Strategy: Manage usage of Spirits Within ability
        res.Define(Track.SpiritsWithin).As<OffensiveStrategy>("Spirits Within", "S.Within", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Spirits Within normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Spirits Within / Expiacion", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Spirits Within / Expiacion", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(PLD.AID.SpiritsWithin, PLD.AID.Expiacion);

        //Circle of Scorn Strategy: Control the use of Circle of Scorn
        res.Define(Track.CircleOfScorn).As<OffensiveStrategy>("Circle of Scorn Strategy", "Circle of Scorn", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Circle of Scorn normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Circle of Scorn ASAP", 60, 15, ActionTargets.Hostile, 50)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Circle of Scorn", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(PLD.AID.CircleOfScorn);

        //Goring Blade Strategy: Manage the Goring Blade action
        res.Define(Track.GoringBlade).As<OffensiveStrategy>("Goring Blade Strategy", "Sonic Break", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Goring Blade normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Goring Blade ASAP", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Goring Blade", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(PLD.AID.GoringBlade);

        //Holy Spirit Strategy: Control usage of Holy Spirit ability
        res.Define(Track.HolySpirit).As<OffensiveStrategy>("Holy Spirit Strategy", "Holy Spirit", uiPriority: 140)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Holy Spirit normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Holy Spirit ASAP", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Holy Spirit", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(PLD.AID.HolySpirit);

        //Holy Circle Strategy: Manage usage of Holy Circle ability
        res.Define(Track.HolyCircle).As<OffensiveStrategy>("Holy Circle Strategy", "Holy Circle", uiPriority: 140)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Holy Circle normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Holy Circle ASAP", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Holy Circle", 0, 0, ActionTargets.None, 72)
            .AddAssociatedActions(PLD.AID.HolyCircle);

        //Blade of Honor Strategy: Manage usage of Blade of Honor ability
        res.Define(Track.BladeOfHonor).As<OffensiveStrategy>("Blade of Honor Strategy", "Blade of Honor", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Blade of Honor normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Blade of Honor ASAP", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Blade of Honor", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(PLD.AID.BladeOfHonor);

        return res;

    }

    public enum GCDPriority //Priority for GCDs used
    {
        None = 0,
        Combo123 = 350,
        NormalGCD = 450,
        HolyCircle = 490,
        HolySpirit = 500,
        Atonement3 = 560,
        Atonement2 = 570,
        Atonement1 = 580,
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

    public int Oath; //Current value of the oath gauge
    public int BladeComboStep; //Current step in the Confiteor combo sequence

    public float GCDLength; //Length of the global cooldown, adjusted by skill speed and haste (baseline: 2.5s)

    public float PotionLeft; //Remaining duration of the potion buff (typically 30s)
    public float RaidBuffsLeft; //Remaining duration of active raid-wide buffs (typically 20s-22s)
    public float RaidBuffsIn; //Time until the next set of raid-wide buffs are applied (typically 20s-22s)

    public float BurstWindowLeft; //Time remaining in the current burst window (typically 20s-22s)
    public float BurstWindowIn; //Time until the next burst window begins (typically 20s-22s)

    //Buff and cooldown management
    public float fofCD; //Cooldown remaining on the Fight or Flight ability
    public float fofLeft; //Time left before Fight or Flight is available again
    public float reqCD; //Cooldown remaining on the Requiescat ability
    public (float Left, int Stacks) req; //Remaining cooldown for Requiescat, along with its stack count
    public uint playerMP; //Current MP (mana points) of the player

    //Buff status indicators
    public bool hasFoF; //Indicates if the Fight or Flight buff is currently active
    public bool hasReq; //Indicates if the Requiescat buff is currently active
    public bool hasMight; //Indicates if the Divine Might buff is currently active
    public bool hasMPforMight; //Checks if there is enough MP to use Holy Spirit with Divine Might
    public bool hasMPforReq; //Checks if there is enough MP to use Holy Spirit under the Requiescat buff

    //Phase and condition monitoring
    public bool isDivineMightExpiring; //Indicates if the Divine Might buff is nearing expiration
    public bool isAtonementExpiring; //Indicates if any Atonement buffs are about to expire

    public PLD.AID NextGCD; //The next action to be executed during the global cooldown (for cartridge management)
    private GCDPriority NextGCDPrio; //Priority of the next global cooldown action for decision-making on cooldowns

    //Check if the desired ability is unlocked
    private bool Unlocked(PLD.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));

    //Get remaining cooldown time for the specified action
    private float CD(PLD.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    //Check if we can fit an additional GCD within the provided deadline
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline;

    //Get the last action used in the combo sequence
    private PLD.AID ComboLastMove => (PLD.AID)World.Client.ComboState.Action;

    //Check if the target is within melee range (3 yalms)
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 3;

    //Check if the target is within 5 yalms
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.75;

    //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool ActionReady(PLD.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f;

    //Check if this is the first GCD in combat
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    //Returns the number of targets hit by AoE within a 5-yalm radius around the player
    private int NumTargetsHitByAoE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    //Checks if the potion should be used before raid buffs expire
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f;

    //Checks if Status effect is on self
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null;

    //Calculates the elapsed time since combat started in seconds
    public float CombatTimer => (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {

        var gauge = World.Client.GetGauge<PaladinGauge>(); //Retrieve current Paladin gauge
        Oath = gauge.OathGauge; //Get the current value of the Oath gauge
        BladeComboStep = gauge.ConfiteorComboStep; //Get the current step in the Confiteor/Blades combo

        //Buff and cooldown management
        fofCD = CD(PLD.AID.FightOrFlight); //Remaining cooldown for the Fight or Flight ability
        fofLeft = SelfStatusLeft(PLD.SID.FightOrFlight); //Remaining duration of the Fight or Flight buff
        reqCD = CD(PLD.AID.Requiescat); //Remaining cooldown for the Requiescat ability
        playerMP = Player.HPMP.CurMP; //Current mana points (MP) of the player

        //Buff status checks
        hasFoF = fofCD is >= 40 and <= 60; //Check if the Fight or Flight buff is active (within cooldown range)
        hasReq = HasEffect(PLD.AID.Requiescat); //Check if the Requiescat buff is active
        hasMight = HasEffect(PLD.SID.DivineMight); //Check if the Divine Might buff is active
        hasMPforMight = playerMP >= 1000; //Check if there is enough MP for Holy Spirit while using Divine Might
        hasMPforReq = playerMP >= 1000 * 3.6; //Check if there is enough MP for Holy Spirit under the Requiescat buff

        //Phase and condition monitoring
        isDivineMightExpiring = SelfStatusLeft(PLD.SID.DivineMight) < 6; //Check if the Divine Might buff is about to expire
        isAtonementExpiring =
          HasEffect(PLD.SID.AtonementReady) && SelfStatusLeft(PLD.SID.AtonementReady) < 6 ||
          HasEffect(PLD.SID.SupplicationReady) && SelfStatusLeft(PLD.SID.SupplicationReady) < 6 ||
          HasEffect(PLD.SID.SepulchreReady) && SelfStatusLeft(PLD.SID.SepulchreReady) < 6; //Check if any Atonement buffs are close to expiring

        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level); //Calculate GCD based on skill speed and haste

        //Buff durations
        PotionLeft = PotionStatusLeft(); //Remaining duration of the potion buff (typically 30s)

        //Raid buff timings
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget); //Estimate remaining and incoming raid buff durations

        //Next global cooldown action
        NextGCD = PLD.AID.None; //Initialize next action as none
        NextGCDPrio = GCDPriority.None; //Set next GCD priority to none

        //Define the AoE strategy and determine the number of targets hit
        var AOEStrategy = strategy.Option(Track.AoE).As<AOEStrategy>();
        var AoETargets = AOEStrategy switch
        {
            AOEStrategy.UseST => NumTargetsHitByAoE() > 0 ? 1 : 0, //Use single-target actions if any AoE targets exist
            AOEStrategy.UseAoE => NumTargetsHitByAoE() > 0 ? 100 : 0, //Use AoE actions if any targets are hit
            _ => NumTargetsHitByAoE() //Default to the number of targets hit by AoE
        };

        //Burst (raid buff) windows typically last 20 seconds every 120 seconds
        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        var hold = burstStrategy == BurstStrategy.Conserve; //Determine if we are conserving cartridges

        //Calculate the burst window based on the current strategy
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => (RaidBuffsIn, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft)), //Automatically calculate based on current buffs and potions
            BurstStrategy.UnderRaidBuffs => (RaidBuffsIn, RaidBuffsLeft), //Under raid buffs, use remaining durations
            BurstStrategy.UnderPotion => (PotionCD, PotionLeft), //Under potion effects, use potion cooldown and remaining duration
            _ => (0, 0), //no burst window
        };

        //Check GCD (Global Cooldown) conditions for various abilities
        var canFoF = Unlocked(PLD.AID.FightOrFlight) && ActionReady(PLD.AID.FightOrFlight); //Fight or Flight ability
        var canReq = Unlocked(PLD.AID.Requiescat) && ActionReady(PLD.AID.Requiescat); //Requiescat ability
        var canScorn = Unlocked(PLD.AID.CircleOfScorn) && ActionReady(PLD.AID.CircleOfScorn); //Circle of Scorn ability
        var canSpirit = Unlocked(PLD.AID.SpiritsWithin) && ActionReady(PLD.AID.SpiritsWithin); //Spirits Within ability
        var canGB = Unlocked(PLD.AID.GoringBlade) && HasEffect(PLD.SID.GoringBladeReady); //Goring Blade ability readiness
        var canHS = Unlocked(PLD.AID.HolySpirit); //Holy Spirit ability
        var canHC = Unlocked(PLD.AID.HolyCircle); //Holy Circle ability
        var canAtone = Unlocked(PLD.AID.Atonement) && HasEffect(PLD.SID.AtonementReady); //Atonement ability readiness
        var canDash = Unlocked(PLD.AID.Intervene) && CD(PLD.AID.Intervene) <= 30; //Intervene ability with cooldown check
        var canConfiteor = Unlocked(PLD.AID.Confiteor) && HasEffect(PLD.SID.ConfiteorReady) && BladeComboStep is 0; //Confiteor ability readiness and combo step check
        var canBlade = Unlocked(PLD.AID.BladeOfValor) && BladeComboStep is not 0; //Blade abilities, only if combo step is not zero
        var canHonor = Unlocked(PLD.AID.BladeOfHonor) && HasEffect(PLD.SID.BladeOfHonorReady); //Blade of Honor ability readiness

        //Determine and queue the next combo action based on AoE strategy
        var (comboAction, comboPrio) = ComboActionPriority(AOEStrategy, AoETargets, burstStrategy, burst.Value.ExpireIn);
        QueueGCD(comboAction, comboAction is PLD.AID.TotalEclipse or PLD.AID.Prominence ? Player : primaryTarget,
            AOEStrategy is AOEStrategy.ForceST or AOEStrategy.ForceAoE ? GCDPriority.ForcedGCD : comboPrio);

        //Execute Fight or Flight if conditions are met
        var fofStrat = strategy.Option(Track.FightOrFlight).As<OffensiveStrategy>();
        if (!hold && canFoF && ShouldUseFightOrFlight(fofStrat, primaryTarget))
            QueueOGCD(PLD.AID.FightOrFlight, Player, fofStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.FightOrFlight);

        //Execute Requiescat if conditions are met
        var reqStrat = strategy.Option(Track.Requiescat).As<OffensiveStrategy>();
        if (!hold && canReq && ShouldUseRequiescat(reqStrat, primaryTarget))
            QueueOGCD(Unlocked(PLD.AID.Imperator) ? PLD.AID.Imperator : PLD.AID.Requiescat, primaryTarget, reqStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Requiescat);

        //Execute Confiteor if conditions are met
        var bladeStrat = strategy.Option(Track.BladeCombo).As<BladeComboStrategy>();
        if (canConfiteor && BladeComboStep is 0 && ShouldUseBladeCombo(bladeStrat, primaryTarget))
            QueueGCD(PLD.AID.Confiteor, primaryTarget, bladeStrat is BladeComboStrategy.ForceConfiteor ? GCDPriority.ForcedGCD : GCDPriority.Confiteor);

        //Execute Circle of Scorn if conditions are met
        var scornStrat = strategy.Option(Track.CircleOfScorn).As<OffensiveStrategy>();
        if (!hold && canScorn && ShouldUseCircleOfScorn(scornStrat, primaryTarget))
            QueueOGCD(PLD.AID.CircleOfScorn, primaryTarget, scornStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.CircleOfScorn);

        //Execute Spirits Within if conditions are met
        var spiritsStrat = strategy.Option(Track.SpiritsWithin).As<OffensiveStrategy>();
        if (!hold && canSpirit && ShouldUseSpiritsWithin(spiritsStrat, primaryTarget))
            QueueOGCD(Unlocked(PLD.AID.Expiacion) ? PLD.AID.Expiacion : PLD.AID.SpiritsWithin, primaryTarget, spiritsStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.SpiritsWithin);

        //Execute Blade Combo actions based on the current combo step
        if (canBlade)
        {
            if (BladeComboStep is 1)
                QueueGCD(PLD.AID.BladeOfFaith, primaryTarget, bladeStrat is BladeComboStrategy.ForceFaith ? GCDPriority.ForcedGCD : GCDPriority.Faith);
            if (BladeComboStep is 2)
                QueueGCD(PLD.AID.BladeOfTruth, primaryTarget, bladeStrat is BladeComboStrategy.ForceTruth ? GCDPriority.ForcedGCD : GCDPriority.Truth);
            if (BladeComboStep is 3)
                QueueGCD(PLD.AID.BladeOfValor, primaryTarget, bladeStrat is BladeComboStrategy.ForceValor ? GCDPriority.ForcedGCD : GCDPriority.Valor);
        }

        //Execute Intervene if conditions are met
        var dashStrat = strategy.Option(Track.Dash).As<DashStrategy>();
        if (canDash && ShouldUseDash(dashStrat, primaryTarget))
            QueueOGCD(PLD.AID.Intervene, primaryTarget, dashStrat is DashStrategy.Force or DashStrategy.GapClose ? OGCDPriority.ForcedOGCD : OGCDPriority.Intervene);

        //Execute Goring Blade if conditions are met
        var gbStrat = strategy.Option(Track.GoringBlade).As<OffensiveStrategy>();
        if (!hold && canGB && ShouldUseGoringBlade(gbStrat, primaryTarget))
            QueueGCD(PLD.AID.GoringBlade, primaryTarget, gbStrat is OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.GoringBlade);

        //Execute Blade of Honor if conditions are met
        var bohStrat = strategy.Option(Track.GoringBlade).As<OffensiveStrategy>();
        if (!hold && canHonor && ShouldUseGoringBlade(bohStrat, primaryTarget))
            QueueOGCD(PLD.AID.BladeOfHonor, primaryTarget, bohStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.BladeOfHonor);

        //Execute Atonement if conditions are met
        var atoneStrat = strategy.Option(Track.Atonement).As<AtonementStrategy>();
        if (!hold && canAtone && ShouldUseAtonement(atoneStrat, primaryTarget))
            QueueGCD(PLD.AID.Atonement, primaryTarget, atoneStrat is AtonementStrategy.ForceAtonement ? GCDPriority.ForcedGCD : GCDPriority.Atonement1);

        //Execute Atonement Combo actions based on readiness
        if (HasEffect(PLD.SID.SupplicationReady))
            QueueGCD(PLD.AID.Supplication, primaryTarget, atoneStrat is AtonementStrategy.ForceSupplication ? GCDPriority.ForcedGCD : GCDPriority.Atonement2);
        if (HasEffect(PLD.SID.SepulchreReady))
            QueueGCD(PLD.AID.Sepulchre, primaryTarget, atoneStrat is AtonementStrategy.ForceSepulchre ? GCDPriority.ForcedGCD : GCDPriority.Atonement3);

        //Execute Holy Spirit if conditions are met
        var hsStrat = strategy.Option(Track.HolySpirit).As<OffensiveStrategy>();
        var hcStrat = strategy.Option(Track.HolyCircle).As<OffensiveStrategy>();
        if (canHS && ShouldUseHolySpirit(hsStrat, primaryTarget))
            QueueGCD(PLD.AID.HolySpirit, primaryTarget, hsStrat is OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.HolySpirit);

        //Execute Holy Circle if conditions are met
        if (canHC && ShouldUseHolyCircle(hcStrat, primaryTarget))
            QueueGCD(PLD.AID.HolyCircle, primaryTarget, hcStrat is OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.HolyCircle);
        if (!canHC && canHS)
            QueueGCD(PLD.AID.HolySpirit, primaryTarget, hsStrat is OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.HolySpirit);

        //Execute ranged skills based on strategy
        var rangedStrat = strategy.Option(Track.Ranged).As<RangedStrategy>();
        if (ShouldUseRangedLob(primaryTarget, rangedStrat))
            QueueGCD(PLD.AID.ShieldLob, primaryTarget, rangedStrat is RangedStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.NormalGCD);
        if (ShouldUseRangedCast(primaryTarget, rangedStrat))
            QueueGCD(PLD.AID.HolySpirit, primaryTarget, rangedStrat is RangedStrategy.ForceCast ? GCDPriority.ForcedGCD : GCDPriority.NormalGCD);

        //Execute potion if conditions are met
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);

    }

    //Method to queue a GCD (Global Cooldown) action
    private void QueueGCD(PLD.AID aid, Actor? target, GCDPriority prio)
    {
        //Check if the priority for this action is valid
        if (prio != GCDPriority.None)
        {
            //Push the action to the execution queue with the specified priority
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + (int)prio);

            //Update the next GCD action and its priority if this one has a higher priority
            if (prio > NextGCDPrio)
            {
                NextGCD = aid; //Set the next GCD action
                NextGCDPrio = prio; //Update the priority for the next GCD
            }
        }
    }

    //Method to queue an OGCD (Off Global Cooldown) action
    private void QueueOGCD(PLD.AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Medium)
    {
        //Check if the priority for this action is valid
        if (prio != OGCDPriority.None)
        {
            //Push the OGCD action to the execution queue with the specified priority
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
        }
    }

    //Method to determine the next single-target action based on the last action used
    private PLD.AID NextComboSingleTarget() => ComboLastMove switch
    {
        PLD.AID.RiotBlade => Unlocked(PLD.AID.RoyalAuthority) ? PLD.AID.RoyalAuthority : PLD.AID.RageOfHalone, //If Riot Blade was last used, choose Royal Authority if available, otherwise Rage of Halone
        PLD.AID.FastBlade => PLD.AID.RiotBlade, //If Fast Blade was last used, go back to Riot Blade
        _ => PLD.AID.FastBlade, //Default to Fast Blade if no recognized last action
    };

    //Method to determine the next AoE action based on the last action used
    private PLD.AID NextComboAoE() => ComboLastMove switch
    {
        PLD.AID.TotalEclipse => PLD.AID.Prominence, //If Total Eclipse was last used, use Prominence next
        _ => PLD.AID.TotalEclipse, //Default to Total Eclipse if no recognized last action
    };

    //Method to determine the next combo action and its priority based on AoE strategy, target count, and burst strategy
    private (PLD.AID, GCDPriority) ComboActionPriority(AOEStrategy aoeStrategy, int AoETargets, BurstStrategy burstStrategy, float burstStrategyExpire)
    {
        //Determine how many combo steps are remaining based on the last action
        var comboStepsRemaining = ComboLastMove switch
        {
            PLD.AID.FastBlade => Unlocked(PLD.AID.RiotBlade) ? 2 : Unlocked(PLD.AID.RoyalAuthority) ? 1 : 0, //Fast Blade allows up to 2 or 1 additional combo actions based on availability
            PLD.AID.TotalEclipse => Unlocked(PLD.AID.Prominence) ? 1 : 0, //Total Eclipse allows 1 more combo action if Prominence is unlocked
            _ => 0 //Default to no combo steps if the last action doesn't match
        };

        //Check if we can fit a GCD based on remaining combo state time
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining))
            comboStepsRemaining = 0; //Reset combo steps if not enough time to complete them

        var doingAOECombo = ComboLastMove == PLD.AID.TotalEclipse; //Track if currently performing an AoE combo

        //Determine if an AoE action is desirable based on target count and strategy
        var wantAOEAction = Unlocked(PLD.AID.TotalEclipse) && aoeStrategy switch
        {
            AOEStrategy.UseST => false, //Explicitly using single-target strategy, so AoE action is not desired
            AOEStrategy.ForceST => false, //Forcing single-target, so AoE action is not desired
            AOEStrategy.UseAoE => true, //Explicitly using AoE strategy, so AoE action is desired
            AOEStrategy.ForceAoE => false, //Forcing AoE action but this is mainly for planning, so AoE not action is desired
            AOEStrategy.Auto => AoETargets >= 3, //Automatically choose AoE if there are 3 or more targets
            AOEStrategy.AutoFinishCombo => comboStepsRemaining > 0
                ? doingAOECombo : AoETargets >= 3, //Continue combo if ongoing, otherwise switch to AoE if targets are sufficient
            _ => false //Default to no AoE action if no matching strategy
        };

        //Reset combo steps if the desired action type doesn't match the current combo type
        if (comboStepsRemaining > 0 && wantAOEAction != doingAOECombo)
            comboStepsRemaining = 0; //Reset if we need to switch from AoE to single-target or vice versa

        //Determine the next action based on whether an AoE or single-target action is desired
        var nextAction = wantAOEAction ? NextComboAoE() : NextComboSingleTarget();

        //Return combo priority based on the ability to fit GCDs and remaining combo steps
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining, 1))
            return (nextAction, GCDPriority.Combo123); //Return with priority if combo steps cannot fit

        //Return normal combo action priority based on action risks
        return (nextAction, GCDPriority.Combo123); //Return the next action and its priority
    }

    //Determines when to use Shield Lob for Ranged purposes
    private bool ShouldUseRangedLob(Actor? target, RangedStrategy strategy) => strategy switch
    {
        RangedStrategy.OpenerRanged => IsFirstGCD() && !In3y(target), //Use if it's the first GCD and target is not within 3y
        RangedStrategy.Opener => IsFirstGCD(), //Use if it's the first GCD
        RangedStrategy.Force => true, //Force
        RangedStrategy.Ranged => !In3y(target), //Use if target is not within 3y
        RangedStrategy.Forbid => false, //Delay usage
        _ => false
    };

    //Determines when to use Holy Spirit for Ranged purposes
    private bool ShouldUseRangedCast(Actor? target, RangedStrategy strategy) => strategy switch
    {
        RangedStrategy.OpenerRangedCast => IsFirstGCD() && !In3y(target), //Use if it's the first GCD and target is not within 3y
        RangedStrategy.OpenerCast => IsFirstGCD(), //Use if it's the first GCD
        RangedStrategy.ForceCast => true, //Force
        RangedStrategy.RangedCast => !In3y(target), //Use if target is not within 3y
        _ => false
    };

    //Determines when to use Fight or Flight
    private bool ShouldUseFightOrFlight(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && target != null && ActionReady(PLD.AID.FightOrFlight) && CombatTimer >= GCDLength * 2 + 0.5f, //Use if in combat, target is valid, action is ready
        OffensiveStrategy.Force => true, //Force
        OffensiveStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Requiescat
    private bool ShouldUseRequiescat(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && target != null && ActionReady(PLD.AID.Requiescat) && hasFoF, //Use if in combat, target is valid, action is ready, and has Fight or Flight
        OffensiveStrategy.Force => true, //Force
        OffensiveStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Spirits Within
    private bool ShouldUseSpiritsWithin(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && fofCD is < 57.55f and > 17 && //Use if in combat, target is in range, and cooldown is within limits
            ActionReady(Unlocked(PLD.AID.Expiacion) ? PLD.AID.Expiacion : PLD.AID.SpiritsWithin), //Action is ready, prioritizing Expiacion if unlocked
        OffensiveStrategy.Force => true, //Force
        OffensiveStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Circle of Scorn
    private bool ShouldUseCircleOfScorn(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(PLD.AID.CircleOfScorn) && In5y(target) && fofCD is < 57.55f and > 17, //Use if in combat, action is ready, target is within 5y, and cooldown is within limits
        OffensiveStrategy.Force => true, //Force
        OffensiveStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Goring Blade
    private bool ShouldUseGoringBlade(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && hasFoF, //Use if in combat, target is in range, and has Fight or Flight active
        OffensiveStrategy.Force => true, //Force
        OffensiveStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Blade Combo
    private bool ShouldUseBladeCombo(BladeComboStrategy strategy, Actor? target) => strategy switch
    {
        BladeComboStrategy.Automatic =>
            Player.InCombat && HasEffect(PLD.SID.ConfiteorReady) && hasFoF && BladeComboStep is 0, //Use if in combat, Confiteor is ready, has Fight or Flight, and it's the first step
        BladeComboStrategy.ForceConfiteor => HasEffect(PLD.SID.ConfiteorReady) && BladeComboStep is 0, //Force use of Confiteor if ready and it's the first step
        BladeComboStrategy.ForceFaith => BladeComboStep is 1, //Force use of Faith if it's the second step
        BladeComboStrategy.ForceTruth => BladeComboStep is 2, //Force use of Truth if it's the third step
        BladeComboStrategy.ForceValor => BladeComboStep is 3, //Force use of Valor if it's the fourth step
        BladeComboStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Atonement
    private bool ShouldUseAtonement(AtonementStrategy strategy, Actor? target) => strategy switch
    {
        AtonementStrategy.Automatic =>
            Player.InCombat && In3y(target) && HasEffect(PLD.SID.AtonementReady), //Use if in combat, target is in range, and Atonement is ready
        AtonementStrategy.ForceAtonement => HasEffect(PLD.SID.AtonementReady), //Force use of Atonement if ready
        AtonementStrategy.ForceSupplication => HasEffect(PLD.SID.SupplicationReady), //Force use of Supplication if ready
        AtonementStrategy.ForceSepulchre => HasEffect(PLD.SID.SepulchreReady), //Force use of Sepulchre if ready
        AtonementStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Holy Spirit
    private bool ShouldUseHolySpirit(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && //Use if in combat
            hasMight && hasMPforMight && !hasReq, //Check if we have Might, sufficient MP for Might, and not under Requiescat
        OffensiveStrategy.Force => true, //Force
        OffensiveStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Holy Circle
    private bool ShouldUseHolyCircle(OffensiveStrategy strategy, Actor? AoETargets) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && //Use if in combat
            hasMight && hasMPforMight && NumTargetsHitByAoE() >= 3 && !hasReq, //Check if we have Might, sufficient MP for Might, hit at least 3 targets, and not under Requiescat
        OffensiveStrategy.Force => true, //Force
        OffensiveStrategy.Delay => false, //Delay usage
        _ => false
    };

    //Determines when to use Dash
    private bool ShouldUseDash(DashStrategy strategy, Actor? target) => strategy switch
    {
        DashStrategy.Automatic =>
            Player.InCombat && target != null && hasFoF, //Use in Fight or Flight
        DashStrategy.Force => true, //Force
        DashStrategy.Conserve1 => CD(PLD.AID.Intervene) > 30, //Use if cooldown is greater than 30 seconds
        DashStrategy.GapClose => !In3y(target), //Use if target is out of range
        _ => false
    };

    //Potion Helpers
    //Determines if potions are aligned with buffs
    private bool IsPotionAlignedWithNM()
    {
        //Use potion before FoF/Req in opener
        //Use for 6m window
        return ActionReady(PLD.AID.FightOrFlight) &&
                (ActionReady(PLD.AID.Requiescat) || //Opener
                reqCD < 15); //Use if Requiescat is on cooldown for less than 15 seconds
    }

    //Determines when to use a potion based on strategy
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs =>
            IsPotionAlignedWithNM() || fofCD < 5 && reqCD < 15, //Align potions with major buffs
        PotionStrategy.Immediate => true, //Force potion immediately
        _ => false,
    };
}

