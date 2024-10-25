using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance
//This module supports <=2.47 SkS rotation as default (or 'Automatic')
//With user adjustment, 'SlowGNB' or 'FastGNB' usage is achievable

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    //Actions tracked for Cooldown Planner execution
    public enum Track
    {
        AoE,             //ST&AoE actions
        Burst,           //Burst damage actions
        Potion,          //Potion usage tracking
        LightningShot,   //Ranged attack tracking
        GnashingFang,    //Gnashing Fang action tracking
        NoMercy,         //No Mercy ability tracking
        SonicBreak,      //Sonic Break ability tracking
        Bloodfest,       //Bloodfest ability tracking
        DoubleDown,      //Double Down ability tracking
        BurstStrike,     //Burst Strike ability tracking
        FatedCircle,     //Fated Circle ability tracking
        Zone,            //Blasting Zone or Danger Zone tracking
        BowShock,        //Bow Shock ability tracking
    }

    //Defines the strategy for using ST/AoE actions based on the current target selection and conditions
    public enum AOEStrategy
    {
        SingleTarget,       //Force single-target actions without exceeding cartridge cap
        FocusSingleTarget,  //Force single-target actions, regardless of cartridges
        ForceAoE,           //Force AoE actions without exceeding cartridge cap
        FocusAoE,           //Force AoE actions, regardless of cartridges
        Auto,               //Decide action based on target count; may break combo if needed
        AutoFinishCombo,    //Decide action based on target count but finish current combo if possible
        GenerateDowntime,   //Generate cartridges before downtime
    }

    //Defines different strategies for executing burst damage actions based on cooldown and resource availability
    public enum BurstStrategy
    {
        Automatic,          //Automatically execute based on conditions
        ConserveCarts,      //Conserve cartridges for future use
        UnderRaidBuffs,     //Execute during raid buffs for maximum effect
        UnderPotion         //Execute while under the effects of a potion
    }

    //Defines strategies for potion usage in combat, determining when and how to consume potions based on the situation
    public enum PotionStrategy
    {
        Manual,                //Manual potion usage
        AlignWithRaidBuffs,    //Align potion usage with raid buffs
        Immediate              //Use potions immediately when available
    }

    //Defines strategies for using Lightning Shot during combat based on various conditions
    public enum LightningShotStrategy
    {
        OpenerRanged,   //Use Lightning Shot as part of the opener
        Opener,         //Use Lightning Shot at the start of combat
        Force,          //Always use Lightning Shot regardless of conditions
        Ranged,         //Use Lightning Shot when ranged attacks are necessary
        Forbid          //Prohibit the use of Lightning Shot
    }

    //Defines the strategy for using Gnashing Fang in combos, allowing for different behaviors based on combat scenarios
    public enum GnashingStrategy
    {
        Automatic,      //Automatically decide when to use Gnashing Fang
        ForceGnash,     //Force the use of Gnashing Fang regardless of conditions
        ForceClaw,      //Force the use of Savage Claw action when in combo
        ForceTalon,     //Force the use of Wicked Talon action when in combo
        Delay           //Delay the use of Gnashing Fang for strategic reasons
    }

    //Defines the strategy for using No Mercy, allowing for different behaviors based on combat scenarios
    public enum NoMercyStrategy
    {
        Automatic,      //Automatically decide when to use No Mercy
        Force,          //Force the use of No Mercy regardless of conditions
        ForceLW,        //Force the use of No Mercy in next possible 2nd oGCD slot
        Force2,         //Force the use of No Mercy when 2 cartridges are available
        Force2LW,       //Force the use of No Mercy in next possible 2nd oGCD slot & 2 cartridges are available
        Force3,         //Force the use of No Mercy when 3 cartridges are available
        Force3LW,       //Force the use of No Mercy in next possible 2nd oGCD slot & 3 cartridges are available
        Delay           //Delay the use of No Mercy for strategic reasons
    }

    //Defines the strategy for using Sonic Break, allowing for different behaviors based on combat scenarios
    public enum SonicBreakStrategy
    {
        Automatic,      //Automatically decide when to use Sonic Break
        Force,          //Force the use of Sonic Break regardless of conditions
        EarlySB,        //Force the use of Sonic Break on the first GCD slot inside No Mercy window
        LateSB,         //Force the use of Sonic Break on the last GCD slot inside No Mercy window
        Delay           //Delay the use of Sonic Break for strategic reasons
    }

    //Defines the strategy for using Bloodfest, allowing for different behaviors based on combat scenarios
    public enum BloodfestStrategy
    {
        Automatic,      //Automatically decide when to use Bloodfest
        Force,          //Force the use of Bloodfest regardless of conditions
        Force0,         //Force the use of Bloodfest only when ammo is empty
        Delay           //Delay the use of Sonic Break for strategic reasons
    }

    //Defines different offensive strategies that dictate how abilities and resources are used during combat
    public enum OffensiveStrategy
    {
        Automatic,      //Automatically decide when to use offensive abilities
        Force,          //Force the use of offensive abilities regardless of conditions
        Delay           //Delay the use of offensive abilities for strategic reasons
    }
    #endregion

    public static RotationModuleDefinition Definition()
    {
        //Module title & signature
        var res = new RotationModuleDefinition("GNB (Akechi)", "Standard Rotation Module", "Standard rotation (Akechi)", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.GNB), 100);

        #region Custom strategies
        //Targeting strategy
        res.Define(Track.AoE).As<AOEStrategy>("Combo Option", "AoE", uiPriority: 200)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use ST rotation (with overcap protection)")
            .AddOption(AOEStrategy.FocusSingleTarget, "ST", "Use ST rotation (without overcap protection)")
            .AddOption(AOEStrategy.ForceAoE, "AoE", "Use AoE rotation (with overcap protection)")
            .AddOption(AOEStrategy.FocusAoE, "AoE", "Use AoE rotation (without overcap protection)")
            .AddOption(AOEStrategy.Auto, "Auto", "Use AoE rotation if 3+ targets would be hit, otherwise use ST rotation; break combo if necessary")
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto Finish Combo", "Use AoE rotation if 3+ targets would be hit, otherwise use ST rotation; finish combo before switching")
            .AddOption(AOEStrategy.GenerateDowntime, "Generate before Downtime", "Estimates time until disengagement & determines when to use ST or AoE combo to generate carts appropriately before downtime");

        //Burst strategy
        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 190)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Spend cartridges optimally")
            .AddOption(BurstStrategy.ConserveCarts, "Conserve", "Conserve everything (cartridges & GCDs)")
            .AddOption(BurstStrategy.UnderRaidBuffs, "Under RaidBuffs", "Spend under raid buffs, otherwise conserve; ignores potion")
            .AddOption(BurstStrategy.UnderPotion, "Under Potion", "Spend under potion, otherwise conserve; ignores raid buffs");

        //Potion strategy
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 180)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with 2-minute raid buffs (0/6, 2/8, etc)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        //LightningShot strategy
        res.Define(Track.LightningShot).As<LightningShotStrategy>("Lightning Shot", "L.Shot", uiPriority: 20)
            .AddOption(LightningShotStrategy.OpenerRanged, "OpenerRanged", "Use as very first GCD and only if outside melee range")
            .AddOption(LightningShotStrategy.Opener, "Opener", "Use as very first GCD regardless of range")
            .AddOption(LightningShotStrategy.Force, "Force", "Force use ASAP (even in melee range)")
            .AddOption(LightningShotStrategy.Ranged, "Ranged", "Use if outside melee range")
            .AddOption(LightningShotStrategy.Forbid, "Forbid", "Do not use at all")
            .AddAssociatedActions(GNB.AID.LightningShot);

        //GnashingFang strategy
        res.Define(Track.GnashingFang).As<GnashingStrategy>("Gnashing Fang", "G.Fang", uiPriority: 160)
            .AddOption(GnashingStrategy.Automatic, "Auto", "Normal use of Gnashing Fang")
            .AddOption(GnashingStrategy.ForceGnash, "Force", "Force use of Gnashing Fang (Step 1)", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceClaw, "Force", "Force use of Savage Claw (Step 2)", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceTalon, "Force", "Force use of Wicked Talon (Step 3)", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.Delay, "Delay", "Delay use of Gnashing Fang", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(GNB.AID.GnashingFang, GNB.AID.SavageClaw, GNB.AID.WickedTalon);

        //NoMercy strategy
        res.Define(Track.NoMercy).As<NoMercyStrategy>("No Mercy", "N.Mercy", uiPriority: 170)
            .AddOption(NoMercyStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(NoMercyStrategy.Force, "Force", "Force use ASAP (even during downtime)", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceLW, "Force Late-weave", "Uses as soon as in next possible late-weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2, "Force (2+ carts)", "Use as soon as you have 2 (or more) cartridges (SlowGNB)", 60, 20, ActionTargets.Self, 30)
            .AddOption(NoMercyStrategy.Force2LW, "Force Late-weave (2+ carts)", "Use as soon as you have 2+ cartridges & in next possible late-weave slot (FastGNB)", 60, 20, ActionTargets.Self, 30)
            .AddOption(NoMercyStrategy.Force3, "Force (3 carts)", "Use as soon as you have 3 cartridges (SlowGNB)", 60, 20, ActionTargets.Self, 88)
            .AddOption(NoMercyStrategy.Force3LW, "Force Late-weave (3 carts)", "Use as soon as you have 3 cartridges & in next possible late-weave slot (FastGNB)", 60, 20, ActionTargets.Self, 88)
            .AddOption(NoMercyStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(GNB.AID.NoMercy);

        //SonicBreak strategy
        res.Define(Track.SonicBreak).As<SonicBreakStrategy>("Sonic Break", "S.Break", uiPriority: 150)
            .AddOption(SonicBreakStrategy.Automatic, "Auto", "Normal use of Sonic Break")
            .AddOption(SonicBreakStrategy.Force, "Force", "Force use of Sonic Break", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.EarlySB, "Early Sonic Break", "Uses Sonic Break as the very first GCD when in No Mercy", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.LateSB, "Late Sonic Break", "Uses Sonic Break as the very last GCD when in No Mercy", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Delay, "Delay", "Delay use of Sonic Break", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(GNB.AID.SonicBreak);

        //Bloodfest strategy
        res.Define(Track.Bloodfest).As<BloodfestStrategy>("Bloodfest", "Fest", uiPriority: 170)
            .AddOption(BloodfestStrategy.Automatic, "Auto", "Normal use of Bloodfest")
            .AddOption(BloodfestStrategy.Force, "Force", "Force use of Bloodfest, regardless of ammo count", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Force0, "Force (0 cart)", "Force use of Bloodfest as soon as you have 0 carts", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Delay, "Delay", "Delay use of Bloodfest", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(GNB.AID.Bloodfest);
        #endregion

        #region Offensive Strategies
        //DoubleDown strategy
        res.Define(Track.DoubleDown).As<OffensiveStrategy>("Double Down", "D.Down", uiPriority: 160)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Double Down")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Double Down", 60, 0, ActionTargets.Hostile, 90)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Double Down", 0, 0, ActionTargets.None, 90)
            .AddAssociatedActions(GNB.AID.DoubleDown);

        //BurstStrike strategy
        res.Define(Track.BurstStrike).As<OffensiveStrategy>("Burst Strike", "B.Strike", uiPriority: 140)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Burst Strike")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Burst Strike", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Burst Strike", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(GNB.AID.BurstStrike);

        //FatedCircle strategy
        res.Define(Track.FatedCircle).As<OffensiveStrategy>("Fated Circle", "F.Circle", uiPriority: 140)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Fated Circle")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Fated Circle", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Fated Circle", 0, 0, ActionTargets.None, 72)
            .AddAssociatedActions(GNB.AID.FatedCircle);

        //Zone strategy
        res.Define(Track.Zone).As<OffensiveStrategy>("Blasting Zone", "Zone", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP", 30, 0, ActionTargets.Hostile, 18)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 18)
            .AddAssociatedActions(GNB.AID.BlastingZone, GNB.AID.DangerZone);

        //BowShock strategy
        res.Define(Track.BowShock).As<OffensiveStrategy>("Bow Shock", "B.Shock", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Bow Shock")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Bow Shock", 60, 15, ActionTargets.Self, 62)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Bow Shock", 0, 0, ActionTargets.None, 62)
            .AddAssociatedActions(GNB.AID.BowShock);
        #endregion

        return res;

    }

    #region Priorities
    //Priority for GCDs used
    public enum GCDPriority
    {
        None = 0,
        Combo123 = 350,
        FatedCircle = 400,
        BurstStrike = 500,
        Reign = 525,
        comboNeed = 550,
        GF23 = 575,
        SonicBreak = 600,
        DoubleDown = 675,
        GF1 = 700,
        ForcedGCD = 900,
    }
    //Priority for oGCDs used
    public enum OGCDPriority
    {
        None = 0,
        Continuation = 500,
        Zone = 550,
        BowShock = 600,
        Continuation1 = 650,
        Bloodfest = 700,
        ContinuationNeed = 800,
        NoMercy = 875,
        Potion = 900,
        ForcedOGCD = 900,
    }
    #endregion

    #region Placeholders for Variables
    //Gauge
    public byte Ammo; //Range: 0-2 or 0-3 max; this counts current ammo count
    public byte GunComboStep; //0 = Gnashing Fang & Reign of Beasts, 1 = Savage Claw, 2 = Wicked Talon, 4 = NobleBlood, 5 = LionHeart.
    public int MaxCartridges; //Maximum number of cartridges based on player level
    //Cooldown Related
    private float bfCD; //Time left on Bloodfest cooldown (120s base)
    private float nmLeft; //Time left on No Mercy buff (20s base)
    private float nmCD; //Time left on No Mercy cooldown (60s base)
    private bool hasNM; //Checks self for No Mercy buff
    private bool hasBreak; //Checks self for Ready To Break buff
    private bool hasReign; //Checks self for Ready To Reign buff
    private bool hasBlast; //Checks self for Ready To Blast buff
    private bool hasRaze; //Checks self for Ready To Raze buff
    private bool hasRip; //Checks self for Ready To Rip buff
    private bool hasTear; //Checks self for Ready To Tear buff
    private bool hasGouge; //Checks self for Ready To Gouge buff
    private bool canBS;
    private bool canGF;
    private bool canFC;
    private bool canDD;
    private bool canBF;
    private bool canZone;
    private bool canBreak;
    private bool canBow;
    private bool canContinue;
    private bool canReign;

    //Misc
    public float PotionLeft; //Time left on potion buff (30s base)
    public float RaidBuffsLeft; //Time left on raid-wide buffs (typically 20s-22s)
    public float RaidBuffsIn; //Time until raid-wide buffs are applied again (typically 20s-22s)
    public float GCDLength; //Current GCD length, adjusted by skill speed/haste (2.5s baseline)
    public float BurstWindowLeft; //Time left in current burst window (typically 20s-22s)
    public float BurstWindowIn; //Time until next burst window (typically 20s-22s)
    public GNB.AID NextGCD; //Next global cooldown action to be used (needed for cartridge management)
    private GCDPriority NextGCDPrio; //Priority of the next GCD, used for decision making on cooldowns
    #endregion

    #region Module Helpers
    private bool Unlocked(GNB.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    private bool Unlocked(GNB.TraitID tid) => TraitUnlocked((uint)tid); //Check if the desired trait is unlocked
    private float CD(GNB.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline; //Check if we can fit an additional GCD within the provided deadline
    private GNB.AID ComboLastMove => (GNB.AID)World.Client.ComboState.Action; //Get the last action used in the combo sequence
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Check if the target is within melee range (3 yalms)
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.75; //Check if the target is within 5 yalms
    private bool ActionReady(GNB.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f; //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Check if this is the first GCD in combat
    private int NumTargetsHitByAoE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //Returns the number of targets hit by AoE within a 5-yalm radius around the player
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f; //Checks if the potion should be used before raid buffs expire
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null; //Checks if Status effect is on self
    #endregion

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        //Gauge
        var gauge = World.Client.GetGauge<GunbreakerGauge>(); //Retrieve Gunbreaker gauge
        Ammo = gauge.Ammo; //Current cartridges
        GunComboStep = gauge.AmmoComboStep; //Combo step for Gnashing Fang or Reign of Beasts
        MaxCartridges = Unlocked(GNB.TraitID.CartridgeChargeII) ? 3 : 2; //Max cartridges based on level
        //Cooldown Related
        bfCD = CD(GNB.AID.Bloodfest); //Bloodfest cooldown (120s)
        nmCD = CD(GNB.AID.NoMercy); //No Mercy cooldown (60s)
        nmLeft = SelfStatusLeft(GNB.SID.NoMercy, 20); //Remaining time for No Mercy buff (20s)
        hasBreak = HasEffect(GNB.SID.ReadyToBreak); //Checks for Ready To Break buff
        hasReign = HasEffect(GNB.SID.ReadyToReign); //Checks for Ready To Reign buff
        hasNM = nmCD is >= 40 and <= 60; //Checks if No Mercy is active
        hasBlast = HasEffect(GNB.SID.ReadyToBlast); //Checks for Ready To Blast buff
        hasRaze = HasEffect(GNB.SID.ReadyToRaze); //Checks for Ready To Raze buff
        hasRip = HasEffect(GNB.SID.ReadyToRip); //Checks for Ready To Rip buff
        hasTear = HasEffect(GNB.SID.ReadyToTear); //Checks for Ready To Tear buff
        hasGouge = HasEffect(GNB.SID.ReadyToGouge); //Checks for Ready To Gouge buff
        //Misc
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on skill speed and haste
        NextGCD = GNB.AID.None; //Next global cooldown action to be used
        NextGCDPrio = GCDPriority.None; //Priority of the next GCD, used for decision making on cooldowns

        #region Minimal Requirements
        //Ammo-relative
        canBS = Unlocked(GNB.AID.BurstStrike) && Ammo > 0; //BurstStrike conditions; -1 Ammo ST
        canGF = Unlocked(GNB.AID.GnashingFang) && ActionReady(GNB.AID.GnashingFang) && Ammo >= 1; //GnashingFang conditions; -1 Ammo ST
        canFC = Unlocked(GNB.AID.FatedCircle) && Ammo > 0; //FatedCircle conditions; -1 Ammo AOE
        canDD = Unlocked(GNB.AID.DoubleDown) && ActionReady(GNB.AID.DoubleDown) && Ammo >= 2; //DoubleDown conditions; -2 Ammo AOE
        canBF = Unlocked(GNB.AID.Bloodfest) && ActionReady(GNB.AID.Bloodfest); //Bloodfest conditions; +all Ammo (must have target)
        //Cooldown-relative
        canZone = Unlocked(GNB.AID.DangerZone) && ActionReady(GNB.AID.DangerZone); //Zone conditions
        canBreak = hasBreak && Unlocked(GNB.AID.SonicBreak); //SonicBreak conditions
        canBow = Unlocked(GNB.AID.BowShock) && ActionReady(GNB.AID.BowShock); //BowShock conditions
        canContinue = Unlocked(GNB.AID.Continuation); //Continuation conditions
        canReign = Unlocked(GNB.AID.ReignOfBeasts); //ReignOfBeasts conditions
        #endregion

        #endregion

        #region Burst
        //Burst (raid buff) windows typically last 20s every 120s
        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        var hold = burstStrategy == BurstStrategy.ConserveCarts; //Determine if conserving cartridges

        //Calculate the burst window based on the current strategy
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => (RaidBuffsIn, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft)),
            BurstStrategy.UnderRaidBuffs => (RaidBuffsIn, RaidBuffsLeft),
            BurstStrategy.UnderPotion => (PotionCD, PotionLeft),
            _ => (0, 0)
        };
        #endregion

        #region Targeting
        //Define ST/AoE strategy and determine number of targets
        var AOEStrategy = strategy.Option(Track.AoE).As<AOEStrategy>();
        var AoETargets = AOEStrategy switch
        {
            AOEStrategy.SingleTarget => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AOEStrategy.FocusSingleTarget => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AOEStrategy.ForceAoE => NumTargetsHitByAoE() > 0 ? 100 : 0,
            AOEStrategy.FocusAoE => NumTargetsHitByAoE() > 0 ? 100 : 0,
            AOEStrategy.GenerateDowntime => NumTargetsHitByAoE() > 0 ? 100 : 0,
            _ => NumTargetsHitByAoE()
        };
        #endregion

        #region Rotation Strategies
        //Force Options
        if (AOEStrategy == AOEStrategy.FocusSingleTarget) //ST (without overcap protection)
            QueueGCD(NextForceSingleTarget(), primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy == AOEStrategy.FocusAoE) //AoE (without overcap protection)
            QueueGCD(NextForceAoE(), primaryTarget, GCDPriority.ForcedGCD);

        #region Logic for Cart Generation before Downtime
        //Estimate time to next downtime
        var downtimeIn = Manager.Planner?.EstimateTimeToNextDowntime().Item2 ?? float.MaxValue;
        var comboStepsRemaining = ComboLastMove switch
        {
            GNB.AID.KeenEdge => Unlocked(GNB.AID.SolidBarrel) ? 2 : Unlocked(GNB.AID.BrutalShell) ? 1 : 0,
            GNB.AID.DemonSlice => Unlocked(GNB.AID.DemonSlaughter) ? 1 : 0,
            _ => 0
        };

        if (AOEStrategy == AOEStrategy.GenerateDowntime)
        {
            if (comboStepsRemaining == 0) //Not in any combo
            {
                if ((downtimeIn == GCD * 2 && Ammo == 2) ||
                    (downtimeIn == GCD * 4 && Ammo == 1) ||
                    (downtimeIn == GCD * 6 && Ammo == 0))
                    QueueGCD(GNB.AID.DemonSlice, Player, GCDPriority.ForcedGCD);

                if ((downtimeIn == GCD * 3 && Ammo == 2) ||
                    (downtimeIn == GCD * 5 && Ammo == 1) ||
                    (downtimeIn == GCD * 8 && Ammo == 0) ||
                    (downtimeIn == GCD * 9 && Ammo == 0))
                    QueueGCD(GNB.AID.KeenEdge, primaryTarget, GCDPriority.ForcedGCD);
            }

            if (comboStepsRemaining == 1) //Combo initiated
            {
                if (((downtimeIn == GCD && Ammo == 2) ||
                    (downtimeIn == GCD * 3 && Ammo == 1) ||
                    (downtimeIn == GCD * 5 && Ammo == 0)) &&
                    ComboLastMove == GNB.AID.DemonSlice)
                    QueueGCD(GNB.AID.DemonSlaughter, Player, GCDPriority.ForcedGCD);

                if (((downtimeIn == GCD * 2 && Ammo == 2) ||
                    (downtimeIn == GCD * 4 && Ammo == 1) ||
                    (downtimeIn == GCD * 7 && Ammo == 2) ||
                    (downtimeIn == GCD * 8 && Ammo == 2)) &&
                    ComboLastMove == GNB.AID.KeenEdge)
                    QueueGCD(GNB.AID.BrutalShell, primaryTarget, GCDPriority.ForcedGCD);
            }

            if (comboStepsRemaining == 2)
            {
                if (((downtimeIn == GCD && (Ammo == 2 || Ammo == 3)) ||
                    (downtimeIn == GCD * 4 && Ammo == 1) ||
                    (downtimeIn == GCD * 7 && Ammo == 0)) &&
                    ComboLastMove == GNB.AID.BrutalShell)
                    QueueGCD(GNB.AID.SolidBarrel, primaryTarget, GCDPriority.ForcedGCD);
            }

            if (Ammo == MaxCartridges)
                QueueGCD(NextForceSingleTarget(), primaryTarget, GCDPriority.ForcedGCD);
        }
        #endregion

        #endregion

        #region Rotation Execution
        //Determine and queue combo actions
        var (comboAction, comboPrio) = ComboActionPriority(AOEStrategy, AoETargets, burstStrategy, burst.Value.ExpireIn);
        QueueGCD(comboAction, comboAction is GNB.AID.DemonSlice or GNB.AID.DemonSlaughter ? Player : primaryTarget, Ammo == 0 ? GCDPriority.comboNeed : comboPrio);

        #region OGCDs
        //No Mercy execution
        var nmStrat = strategy.Option(Track.NoMercy).As<NoMercyStrategy>();
        if (!hold && ShouldUseNoMercy(nmStrat, primaryTarget))
            QueueOGCD(GNB.AID.NoMercy, Player, nmStrat is NoMercyStrategy.Force or NoMercyStrategy.ForceLW or NoMercyStrategy.Force2 or NoMercyStrategy.Force2LW or NoMercyStrategy.Force3 or NoMercyStrategy.Force3LW ? OGCDPriority.ForcedOGCD : OGCDPriority.NoMercy);

        //Zone execution (Blasting Zone / Danger Zone)
        var zoneAction = Unlocked(GNB.AID.BlastingZone) ? GNB.AID.BlastingZone : GNB.AID.DangerZone;
        var zoneStrat = strategy.Option(Track.Zone).As<OffensiveStrategy>();
        if (!hold && ShouldUseZone(zoneStrat, primaryTarget))
            QueueOGCD(zoneAction, primaryTarget, zoneStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Zone);

        //Bow Shock execution
        var bowStrat = strategy.Option(Track.BowShock).As<OffensiveStrategy>();
        if (!hold && ShouldUseBowShock(bowStrat, primaryTarget))
            QueueOGCD(GNB.AID.BowShock, Player, bowStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.BowShock);

        //Bloodfest execution
        var bfStrat = strategy.Option(Track.Bloodfest).As<BloodfestStrategy>();
        if (!hold && ShouldUseBloodfest(bfStrat, primaryTarget))
            QueueOGCD(GNB.AID.Bloodfest, primaryTarget, bfStrat is BloodfestStrategy.Force or BloodfestStrategy.Force0 ? OGCDPriority.ForcedOGCD : OGCDPriority.Bloodfest);

        //Continuation execution
        if (canContinue)
        {
            if (hasRip)
                QueueOGCD(GNB.AID.JugularRip, primaryTarget, OGCDPriority.ContinuationNeed);
            if (hasTear)
                QueueOGCD(GNB.AID.AbdomenTear, primaryTarget, OGCDPriority.ContinuationNeed);
            if (hasGouge)
                QueueOGCD(GNB.AID.EyeGouge, primaryTarget, OGCDPriority.ContinuationNeed);
            if (hasBlast)
                QueueOGCD(GNB.AID.Hypervelocity, primaryTarget, OGCDPriority.ContinuationNeed);
            if (hasRaze)
                QueueOGCD(GNB.AID.FatedBrand, primaryTarget, OGCDPriority.ContinuationNeed);
        }
        #endregion

        #region GCDs
        //Gnashing Fang execution
        var gfStrat = strategy.Option(Track.GnashingFang).As<GnashingStrategy>();
        if (!hold && ShouldUseGnashingFang(gfStrat, primaryTarget))
            QueueGCD(GNB.AID.GnashingFang, primaryTarget, gfStrat == GnashingStrategy.ForceGnash ? GCDPriority.ForcedGCD : GCDPriority.GF1);

        //Double Down execution
        var ddStrat = strategy.Option(Track.DoubleDown).As<OffensiveStrategy>();
        if (ShouldUseDoubleDown(ddStrat, primaryTarget))
            QueueGCD(GNB.AID.DoubleDown, primaryTarget, ((ddStrat == OffensiveStrategy.Force) || Ammo == 2) ? GCDPriority.ForcedGCD : GCDPriority.DoubleDown);

        //Gnashing Fang Combo execution
        if (GunComboStep == 1)
            QueueGCD(GNB.AID.SavageClaw, primaryTarget, gfStrat == GnashingStrategy.ForceClaw ? GCDPriority.ForcedGCD : GCDPriority.GF23);
        if (GunComboStep == 2)
            QueueGCD(GNB.AID.WickedTalon, primaryTarget, gfStrat == GnashingStrategy.ForceTalon ? GCDPriority.ForcedGCD : GCDPriority.GF23);

        //Reign full combo execution
        if (canReign && hasNM && GunComboStep == 0)
            QueueGCD(GNB.AID.ReignOfBeasts, primaryTarget, GCDPriority.Reign);
        if (GunComboStep == 3)
            QueueGCD(GNB.AID.NobleBlood, primaryTarget, GCDPriority.Reign);
        if (GunComboStep == 4)
            QueueGCD(GNB.AID.LionHeart, primaryTarget, GCDPriority.Reign);

        //Sonic Break execution
        var sbStrat = strategy.Option(Track.SonicBreak).As<SonicBreakStrategy>();
        if (ShouldUseSonicBreak(sbStrat, primaryTarget))
            QueueGCD(GNB.AID.SonicBreak, primaryTarget, sbStrat is SonicBreakStrategy.Force or SonicBreakStrategy.EarlySB ? GCDPriority.ForcedGCD : GCDPriority.SonicBreak);

        //Burst Strike execution
        var strikeStrat = strategy.Option(Track.BurstStrike).As<OffensiveStrategy>();
        if (Unlocked(GNB.AID.BurstStrike) && Unlocked(GNB.AID.Bloodfest) && ShouldUseBurstStrike(strikeStrat, primaryTarget))
            QueueGCD(GNB.AID.BurstStrike, primaryTarget, strikeStrat == OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.BurstStrike);

        //Fated Circle execution
        var fcStrat = strategy.Option(Track.FatedCircle).As<OffensiveStrategy>();
        if (ShouldUseFatedCircle(fcStrat, primaryTarget))
            QueueGCD(GNB.AID.FatedCircle, primaryTarget, fcStrat == OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.FatedCircle);
        if (!canFC && canBS)
        {
            if (Ammo > 0 && ShouldUseBurstStrike(strikeStrat, primaryTarget))
                QueueGCD(GNB.AID.BurstStrike, primaryTarget, GCDPriority.BurstStrike);
        }

        //Lightning Shot execution
        var lsStrat = strategy.Option(Track.LightningShot).As<LightningShotStrategy>();
        if (ShouldUseLightningShot(primaryTarget, strategy.Option(Track.LightningShot).As<LightningShotStrategy>()))
            QueueGCD(GNB.AID.LightningShot, primaryTarget, lsStrat is LightningShotStrategy.Force or LightningShotStrategy.Ranged ? GCDPriority.ForcedGCD : GCDPriority.Combo123);
        #endregion

        #endregion

        //Potion execution
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
    }

    #region Core Execution Helpers
    //QueueGCD execution
    private void QueueGCD(GNB.AID aid, Actor? target, GCDPriority prio)
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
    private void QueueOGCD(GNB.AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Medium)
    {
        if (prio != OGCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
        }
    }

    //Returns the amount of ammo gained from specific actions
    private int AmmoGainedFromAction(GNB.AID action) => action switch
    {
        GNB.AID.SolidBarrel => 1,
        GNB.AID.DemonSlaughter => 1,
        GNB.AID.Bloodfest => 3,
        _ => 0
    };

    private (GNB.AID, GCDPriority) ComboActionPriority(AOEStrategy aoeStrategy, int AoETargets, BurstStrategy burstStrategy, float burstStrategyExpire)
    {
        //Determine how many combo steps are remaining based on the last action
        var comboStepsRemaining = ComboLastMove switch
        {
            GNB.AID.KeenEdge => Unlocked(GNB.AID.SolidBarrel) ? 2 : Unlocked(GNB.AID.BrutalShell) ? 1 : 0,
            GNB.AID.DemonSlice => Unlocked(GNB.AID.DemonSlaughter) ? 1 : 0,
            _ => 0
        };

        //Check if we can fit the GCD based on remaining time
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining))
            comboStepsRemaining = 0;

        var doingAOECombo = ComboLastMove == GNB.AID.DemonSlice;

        //Determine if an AoE action is desirable based on target count and strategy
        var wantAOEAction = Unlocked(GNB.AID.DemonSlice) && aoeStrategy switch
        {
            AOEStrategy.SingleTarget => false,
            AOEStrategy.FocusSingleTarget => false,
            AOEStrategy.ForceAoE => true,
            AOEStrategy.FocusAoE => false,
            AOEStrategy.Auto => AoETargets >= 3,
            AOEStrategy.AutoFinishCombo => comboStepsRemaining > 0
                ? doingAOECombo
                : Unlocked(GNB.AID.Continuation) ? AoETargets >= 3 : AoETargets >= 2,
            AOEStrategy.GenerateDowntime => false,
            _ => false
        };

        //Reset combo steps if the desired action does not match the current combo type
        if (comboStepsRemaining > 0 && wantAOEAction != doingAOECombo)
            comboStepsRemaining = 0;

        var nextAction = wantAOEAction ? NextComboAoE() : NextComboSingleTarget();
        var riskingAmmo = Ammo + AmmoGainedFromAction(nextAction) > 3;

        //Return combo priority based on the ability to fit GCDs and remaining combo steps
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining, 1))
            return (nextAction, GCDPriority.Combo123);

        //Return normal combo action priority based on ammo risks
        return (nextAction, riskingAmmo ? GCDPriority.BurstStrike : GCDPriority.Combo123);
    }

    #endregion

    #region Single-Target Helpers
    private GNB.AID NextComboSingleTarget() => ComboLastMove switch //Determines the next single-target action based on the last action used
    {
        GNB.AID.BrutalShell => Ammo == MaxCartridges ? GNB.AID.BurstStrike : GNB.AID.SolidBarrel,
        GNB.AID.KeenEdge => GNB.AID.BrutalShell,
        _ => GNB.AID.KeenEdge,
    };
    private GNB.AID NextForceSingleTarget() => ComboLastMove switch
    {
        GNB.AID.BrutalShell => GNB.AID.SolidBarrel,
        GNB.AID.KeenEdge => GNB.AID.BrutalShell,
        _ => GNB.AID.KeenEdge,
    };
    #endregion

    #region AoE Helpers
    private GNB.AID NextComboAoE() => ComboLastMove switch //Determines the next AoE action based on the last action used
    {
        GNB.AID.DemonSlice => Ammo == MaxCartridges
                              ? Unlocked(GNB.AID.FatedCircle) ? GNB.AID.FatedCircle : GNB.AID.BurstStrike
                              : GNB.AID.DemonSlaughter,
        _ => GNB.AID.DemonSlice,
    };
    private GNB.AID NextForceAoE() => ComboLastMove switch
    {
        GNB.AID.DemonSlice => GNB.AID.DemonSlaughter,
        _ => GNB.AID.DemonSlice,
    };
    #endregion

    #region Cooldown Helpers

    //Determines when to use Lightning Shot
    private bool ShouldUseLightningShot(Actor? target, LightningShotStrategy strategy) => strategy switch
    {
        LightningShotStrategy.OpenerRanged => IsFirstGCD() && !In3y(target),
        LightningShotStrategy.Opener => IsFirstGCD(),
        LightningShotStrategy.Force => true,
        LightningShotStrategy.Ranged => !In3y(target),
        LightningShotStrategy.Forbid => false,
        _ => false
    };

    //Determines when to use No Mercy
    private bool ShouldUseNoMercy(NoMercyStrategy strategy, Actor? target) => strategy switch
    {
        NoMercyStrategy.Automatic =>
            Player.InCombat && target != null && ActionReady(GNB.AID.NoMercy) && GCD < 0.9f &&
            ((Ammo == 1 && bfCD == 0 && Unlocked(GNB.AID.Bloodfest) && Unlocked(GNB.AID.DoubleDown)) || //Lv90+ Opener
            (Ammo >= 1 && bfCD == 0 && Unlocked(GNB.AID.Bloodfest) && !Unlocked(GNB.AID.DoubleDown)) || //Lv80+ Opener
            (!Unlocked(GNB.AID.Bloodfest) && Ammo >= 1 && ActionReady(GNB.AID.GnashingFang)) || //Lv70 & below
            Ammo == MaxCartridges), //60s & 120s burst windows
        NoMercyStrategy.Force => true,
        NoMercyStrategy.ForceLW => Player.InCombat && GCD < 0.9f,
        NoMercyStrategy.Force2 => Ammo >= 2,
        NoMercyStrategy.Force2LW => Player.InCombat && GCD < 0.9f && Ammo >= 2,
        NoMercyStrategy.Force3 => Ammo == 3,
        NoMercyStrategy.Force3LW => Player.InCombat && GCD < 0.9f && Ammo == 3,
        NoMercyStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Bloodfest
    private bool ShouldUseBloodfest(BloodfestStrategy strategy, Actor? target) => strategy switch
    {
        BloodfestStrategy.Automatic =>
            Player.InCombat && target != null &&
            canBF && Ammo == 0 && hasNM,
        BloodfestStrategy.Force => canBF,
        BloodfestStrategy.Force0 => canBF && Ammo == 0,
        BloodfestStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Zone
    private bool ShouldUseZone(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && nmCD is < 57.55f and > 17 &&
            ActionReady(Unlocked(GNB.AID.BlastingZone) ? GNB.AID.BlastingZone : GNB.AID.DangerZone),
        OffensiveStrategy.Force => canZone,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use BowShock
    private bool ShouldUseBowShock(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(GNB.AID.BowShock) && In5y(target) && nmCD is < 57.55f and > 17,
        OffensiveStrategy.Force => canBow,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Sonic Break
    private bool ShouldUseSonicBreak(SonicBreakStrategy strategy, Actor? target) => strategy switch
    {
        SonicBreakStrategy.Automatic =>
            Player.InCombat && In3y(target) && canBreak,
        SonicBreakStrategy.Force => canBreak,
        SonicBreakStrategy.EarlySB => nmCD is >= 57.5f || hasBreak,
        SonicBreakStrategy.LateSB => nmLeft <= GCDLength,
        SonicBreakStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Double Down
    private bool ShouldUseDoubleDown(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && target != null &&
            In5y(target) && canDD && hasNM,
        OffensiveStrategy.Force => canDD,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Gnashing Fang
    private bool ShouldUseGnashingFang(GnashingStrategy strategy, Actor? target) => strategy switch
    {
        GnashingStrategy.Automatic =>
                Player.InCombat && target != null && In3y(target) && canGF &&
                (nmLeft > 0 || hasNM || nmCD is < 35 and > 17),
        GnashingStrategy.ForceGnash => canGF,
        GnashingStrategy.ForceClaw => Player.InCombat && GunComboStep == 1,
        GnashingStrategy.ForceTalon => Player.InCombat && GunComboStep == 2,
        GnashingStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Burst Strike
    private bool ShouldUseBurstStrike(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && target != null && In3y(target) && canBS &&
            ((Unlocked(GNB.AID.DoubleDown) && hasNM && !ActionReady(GNB.AID.DoubleDown) && GunComboStep == 0 && !hasReign) || //Lv90+
            (!Unlocked(GNB.AID.DoubleDown) && !ActionReady(GNB.AID.GnashingFang) && hasNM && GunComboStep == 0) || //Lv80 & Below
            (ComboLastMove == GNB.AID.BrutalShell && Ammo == MaxCartridges)), //Overcap protection
        OffensiveStrategy.Force => canBS,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Fated Circle
    private bool ShouldUseFatedCircle(OffensiveStrategy strategy, Actor? AoETargets) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && AoETargets != null && In3y(AoETargets) && canFC &&
            ((hasNM && !ActionReady(GNB.AID.DoubleDown)) ||
            (ComboLastMove == GNB.AID.DemonSlice && Ammo == MaxCartridges)),
        OffensiveStrategy.Force => canFC,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines if potions are aligned with No Mercy
    private bool IsPotionAlignedWithNM()
    {
        //Use potion before Solid Barrel in opener
        //Use for 6m window
        return (Ammo == 1 && ActionReady(GNB.AID.GnashingFang) &&
                ActionReady(GNB.AID.DoubleDown) &&
                ActionReady(GNB.AID.Bloodfest)) || //Opener
                (bfCD < 15 || ActionReady(GNB.AID.Bloodfest)) && Ammo == 3;
    }

    //Determines when to use a potion based on strategy
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs =>
            IsPotionAlignedWithNM() || (nmCD < 5 && bfCD < 15),
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion
}
