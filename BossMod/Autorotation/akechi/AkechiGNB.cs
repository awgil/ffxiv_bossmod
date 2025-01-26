using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using AID = BossMod.GNB.AID;
using SID = BossMod.GNB.SID;
using TraitID = BossMod.GNB.TraitID;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    //Abilities tracked for Cooldown Planner & Autorotation execution
    public enum Track
    {
        AOE,                 //ST&AOE rotations tracking
        Cooldowns,           //Cooldown abilities tracking
        Cartridges,          //Cartridge abilities tracking
        Potion,              //Potion item tracking
        LightningShot,       //Ranged ability tracking
        NoMercy,             //No Mercy ability tracking
        SonicBreak,          //Sonic Break ability tracking
        GnashingFang,        //Gnashing Fang abilities tracking
        Reign,               //Reign abilities tracking
        Bloodfest,           //Bloodfest ability tracking
        DoubleDown,          //Double Down ability tracking
        Zone,                //Blasting Zone or Danger Zone ability tracking
        BowShock,            //Bow Shock ability tracking
    }

    //Defines the strategy for using ST/AOE actions based on the current target selection and conditions
    public enum AOEStrategy
    {
        AutoFinishCombo,     //Decide action based on target count but finish current combo if possible
        AutoBreakCombo,      //Decide action based on target count; breaks combo if needed
        ForceSTwithO,        //Force single-target rotation with overcap protection on cartridges
        ForceSTwithoutO,     //Force single-target rotation without overcap protection on cartridges
        ForceAOEwithO,       //Force AOE rotation with overcap protection on cartridges
        ForceAOEwithoutO,    //Force AOE rotation without overcap protection on cartridges
        GenerateDowntime     //Generate cartridges before downtime
    }

    //Defines different strategies for executing burst damage actions based on cooldown and resource availability
    public enum CooldownStrategy
    {
        Automatic,           //Automatically execute based on conditions
        Hold,                //Hold all resources
    }

    //Defines the strategy for using abilities that consume cartridges, allowing for different behaviors based on combat scenarios
    public enum CartridgeStrategy
    {
        Automatic,           //Automatically decide when to use Burst Strike & Fated Circle
        OnlyBS,              //Only use Burst Strike as cartridge spender
        OnlyFC,              //Only use Fated Circle as cartridge spender
        ForceBS,             //Force the use of Burst Strike
        ForceFC,             //Force the use of Fated Circle
        Conserve             //Conserves all cartridge-related abilities as much as possible
    }

    //Defines strategies for potion usage in combat, determining when and how to consume potions based on the situation
    public enum PotionStrategy
    {
        Manual,              //Manual potion usage
        AlignWithRaidBuffs,  //Align potion usage with raid buffs
        Immediate            //Use potions immediately when available
    }

    //Defines strategies for using Lightning Shot during combat based on various conditions
    public enum LightningShotStrategy
    {
        OpenerFar,           //Only use Lightning Shot in pre-pull & out of melee range
        OpenerForce,         //Force use Lightning Shot in pre-pull in any range
        Force,               //Force the use of Lightning Shot in any range
        Allow,               //Allow the use of Lightning Shot when out of melee range
        Forbid               //Prohibit the use of Lightning Shot
    }

    //Defines the strategy for using No Mercy, allowing for different behaviors based on combat scenarios
    public enum NoMercyStrategy
    {
        Automatic,           //Automatically decide when to use No Mercy
        Force,               //Force the use of No Mercy regardless of conditions
        ForceW,              //Force the use of No Mercy in next possible weave slot
        ForceQW,             //Force the use of No Mercy in next last possible second weave slot
        Force1,              //Force the use of No Mercy when 1 cartridge is available
        Force1W,             //Force the use of No Mercy when 1 cartridge is available in next possible weave slot
        Force1QW,            //Force the use of No Mercy when 1 cartridge is available in next last possible second weave slot
        Force2,              //Force the use of No Mercy when 2 cartridges are available
        Force2W,             //Force the use of No Mercy when 2 cartridges are available in next possible weave slot
        Force2QW,            //Force the use of No Mercy when 2 cartridges are available in next last possible second weave slot
        Force3,              //Force the use of No Mercy when 3 cartridges are available
        Force3W,             //Force the use of No Mercy when 3 cartridges are available in next possible weave slot
        Force3QW,            //Force the use of No Mercy when 3 cartridges are available in next last possible second weave slot
        Delay                //Delay the use of No Mercy for strategic reasons
    }

    //Defines the strategy for using Sonic Break, allowing for different behaviors based on combat scenarios
    public enum SonicBreakStrategy
    {
        Automatic,           //Automatically decide when to use Sonic Break
        Force,               //Force the use of Sonic Break regardless of conditions
        Early,               //Force the use of Sonic Break on the first GCD slot inside No Mercy window
        Late,                //Force the use of Sonic Break on the last GCD slot inside No Mercy window
        Delay                //Delay the use of Sonic Break for strategic reasons
    }

    //Defines the strategy for using Gnashing Fang in combos, allowing for different behaviors based on combat scenarios
    public enum GnashingStrategy
    {
        Automatic,           //Automatically decide when to use Gnashing Fang
        ForceGnash,          //Force the use of Gnashing Fang regardless of conditions
        ForceClaw,           //Force the use of Savage Claw action when in combo
        ForceTalon,          //Force the use of Wicked Talon action when in combo
        Delay                //Delay the use of Gnashing Fang for strategic reasons
    }

    //Defines the strategy for using Reign of Beasts & it's combo chain, allowing for different behaviors based on combat scenarios
    public enum ReignStrategy
    {
        Automatic,           //Automatically decide when to use Reign of Beasts
        ForceReign,          //Force the use of Reign of Beasts when available
        ForceNoble,          //Force the use of Noble Blood when in available
        ForceLion,           //Force the use of Lion Heart when in available
        Delay                //Delay the use of Reign combo for strategic reasons
    }

    //Defines the strategy for using Bloodfest, allowing for different behaviors based on combat scenarios
    public enum BloodfestStrategy
    {
        Automatic,           //Automatically decide when to use Bloodfest
        Force,               //Force the use of Bloodfest regardless of conditions
        ForceW,              //Force the use of Bloodfest in next possible weave slot
        Force0,              //Force the use of Bloodfest only when ammo is empty
        Force0W,             //Force the use of Bloodfest only when ammo is empty in next possible weave slot
        Delay                //Delay the use of Sonic Break for strategic reasons
    }

    //Defines different offensive strategies that dictate how abilities and resources are used during combat
    public enum GCDStrategy //Global Cooldown Strategy
    {
        Automatic,           //Automatically decide when to use global offensive abilities
        Force,               //Force the use of global offensive abilities regardless of conditions
        Delay                //Delay the use of global offensive abilities for strategic reasons
    }
    public enum OGCDStrategy //Off-Global Cooldown Strategy
    {
        Automatic,           //Automatically decide when to use off-global offensive abilities
        Force,               //Force the use of off-global offensive abilities, regardless of weaving conditions
        AnyWeave,            //Force the use of off-global offensive abilities in any next possible weave slot
        EarlyWeave,          //Force the use of off-global offensive abilities in very next FIRST weave slot only
        LateWeave,           //Force the use of off-global offensive abilities in very next LAST weave slot only
        Delay                //Delay the use of offensive abilities for strategic reasons
    }
    #endregion

    #region Module Definitions & Strategies
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)", //Category
            "Akechi", //Contributor
            RotationModuleQuality.Good, //Quality
            BitMask.Build((int)Class.GNB), //Job
            100); //Level supported

        #region Custom strategies
        //AOE strategy
        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto (Finish Combo)", "Auto-selects best rotation dependant on targets; Finishes combo first", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoBreakCombo, "Auto (Break Combo)", "Auto-selects best rotation dependant on targets; Breaks combo if needed", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceSTwithO, "Force ST with Overcap", "Force single-target rotation with overcap protection", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceSTwithoutO, "Force ST without Overcap", "Force ST rotation without overcap protection", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOEwithO, "Force AOE with Overcap", "Force AOE rotation with overcap protection")
            .AddOption(AOEStrategy.ForceAOEwithoutO, "Force AOE without Overcap", "Force AOE rotation without overcap protection")
            .AddOption(AOEStrategy.GenerateDowntime, "Generate Downtime", "Generate cartridges before downtime");

        //Cooldowns strategy
        res.Define(Track.Cooldowns).As<CooldownStrategy>("Cooldowns", "CDs", uiPriority: 190)
            .AddOption(CooldownStrategy.Automatic, "Automatic", "Automatically decides when to use cooldowns; will use them optimally")
            .AddOption(CooldownStrategy.Hold, "Hold", "Prohibit use of all cooldown-related abilities; will not use any actions with a cooldown timer");

        //Cartridges strategy
        res.Define(Track.Cartridges).As<CartridgeStrategy>("Cartridges", "Carts", uiPriority: 180)
            .AddOption(CartridgeStrategy.Automatic, "Automatic", "Automatically decide when to use cartridges; uses them optimally")
            .AddOption(CartridgeStrategy.OnlyBS, "Only Burst Strike", "Uses Burst Strike optimally as cartridge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.OnlyFC, "Only Fated Circle", "Uses Fated Circle optimally as cartridge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.ForceBS, "Force Burst Strike", "Force use of Burst Strike; consumes 1 cartridge", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceFC, "Force Fated Circle", "Force use of Fated Circle; consumes 1 cartridge", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.Conserve, "Conserve", "Prohibit use of all cartridge-related abilities; will not use any of these actions listed above")
            .AddAssociatedActions(AID.BurstStrike, AID.FatedCircle);

        //Potion strategy
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 20)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with No Mercy & Bloodfest together (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        //LightningShot strategy
        res.Define(Track.LightningShot).As<LightningShotStrategy>("Lightning Shot", "L.Shot", uiPriority: 30)
            .AddOption(LightningShotStrategy.OpenerFar, "Far (Opener)", "Use Lightning Shot in pre-pull & out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.OpenerForce, "Force (Opener)", "Force use Lightning Shot in pre-pull in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Force, "Force", "Force use Lightning Shot in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Allow, "Allow", "Allow use of Lightning Shot when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Forbid, "Forbid", "Prohibit use of Lightning Shot")
            .AddAssociatedActions(AID.LightningShot);

        //NoMercy strategy
        res.Define(Track.NoMercy).As<NoMercyStrategy>("No Mercy", "N.Mercy", uiPriority: 170)
            .AddOption(NoMercyStrategy.Automatic, "Auto", "Normal use of No Mercy")
            .AddOption(NoMercyStrategy.Force, "Force", "Force use of No Mercy, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceW, "Force (Weave)", "Force use of No Mercy in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceQW, "Force (Q.Weave)", "Force use of No Mercy in next possible last second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1, "Force (1 cart)", "Force use of No Mercy when 1 cartridge is available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1W, "Force (1 cart, Weave)", "Force use of No Mercy when 1 cartridge is available & in next weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1QW, "Force (1 cart, Q.Weave)", "Force use of No Mercy when 1 cartridge is available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2, "Force (2 carts)", "Force use of No Mercy when 2 cartridges are available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2W, "Force (2 carts, Weave)", "Force use of No Mercy when 2 cartridges are available & in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2QW, "Force (2 carts, Q.Weave)", "Force use of No Mercy when 2 cartridges are available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3, "Force (3 carts)", "Force use of No Mercy when 3 cartridges are available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3W, "Force (3 carts, Weave)", "Force use of No Mercy when 3 cartridges are available & in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3QW, "Force (3 carts, Q.Weave)", "Force use of No Mercy when 3 cartridges are available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Delay, "Delay", "Delay use of No Mercy", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.NoMercy);

        //SonicBreak strategy
        res.Define(Track.SonicBreak).As<SonicBreakStrategy>("Sonic Break", "S.Break", uiPriority: 150)
            .AddOption(SonicBreakStrategy.Automatic, "Auto", "Normal use of Sonic Break")
            .AddOption(SonicBreakStrategy.Force, "Force", "Force use of Sonic Break", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Early, "Early Sonic Break", "Uses Sonic Break as the very first GCD when in No Mercy", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Late, "Late Sonic Break", "Uses Sonic Break as the very last GCD when in No Mercy", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Delay, "Delay", "Delay use of Sonic Break", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(AID.SonicBreak);

        //GnashingFang strategy
        res.Define(Track.GnashingFang).As<GnashingStrategy>("Gnashing Fang", "G.Fang", uiPriority: 160)
            .AddOption(GnashingStrategy.Automatic, "Auto", "Normal use of Gnashing Fang")
            .AddOption(GnashingStrategy.ForceGnash, "Force", "Force use of Gnashing Fang (Step 1)", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceClaw, "Force", "Force use of Savage Claw (Step 2)", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceTalon, "Force", "Force use of Wicked Talon (Step 3)", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.Delay, "Delay", "Delay use of Gnashing Fang", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.GnashingFang, AID.SavageClaw, AID.WickedTalon);

        //Reign strategy
        res.Define(Track.Reign).As<ReignStrategy>("Reign of Beasts", "Reign", uiPriority: 160)
            .AddOption(ReignStrategy.Automatic, "Auto", "Normal use of Reign of Beasts")
            .AddOption(ReignStrategy.ForceReign, "Force", "Force use of Reign of Beasts", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.ForceNoble, "Force", "Force use of Noble Blood", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.ForceLion, "Force", "Force use of Lion Heart", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.Delay, "Delay", "Delay use of Reign of Beasts", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(AID.ReignOfBeasts, AID.NobleBlood, AID.LionHeart);

        //Bloodfest strategy
        res.Define(Track.Bloodfest).As<BloodfestStrategy>("Bloodfest", "Fest", uiPriority: 170)
            .AddOption(BloodfestStrategy.Automatic, "Auto", "Normal use of Bloodfest")
            .AddOption(BloodfestStrategy.Force, "Force", "Force use of Bloodfest, regardless of ammo count & weaving", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.ForceW, "Force (Weave)", "Force use of Bloodfest in next possible weave slot, regardless of ammo count", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Force0, "Force (0 cart)", "Force use of Bloodfest only if empty on cartridges", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Force0W, "Force (0 cart, Weave)", "Force use of Bloodfest only if empty on cartridges & in next possible weave slot", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Delay, "Delay", "Delay use of Bloodfest", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Bloodfest);

        #endregion

        #region Offensive Strategies
        //DoubleDown strategy
        res.Define(Track.DoubleDown).As<GCDStrategy>("Double Down", "D.Down", uiPriority: 160)
            .AddOption(GCDStrategy.Automatic, "Auto", "Normal use of Double Down")
            .AddOption(GCDStrategy.Force, "Force", "Force use of Double Down", 60, 0, ActionTargets.Hostile, 90)
            .AddOption(GCDStrategy.Delay, "Delay", "Delay use of Double Down", 0, 0, ActionTargets.None, 90)
            .AddAssociatedActions(AID.DoubleDown);

        //Zone strategy
        res.Define(Track.Zone).As<OGCDStrategy>("Blasting Zone", "Zone", uiPriority: 150)
            .AddOption(OGCDStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OGCDStrategy.Force, "Force", "Force use ASAP", 30, 0, ActionTargets.Hostile, 18)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Use in any next possible weave slot", 30, 0, ActionTargets.Hostile, 18)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Use in very next FIRST weave slot only", 30, 0, ActionTargets.Hostile, 18)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Use in very next LAST weave slot only", 30, 0, ActionTargets.Hostile, 18)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 18)
            .AddAssociatedActions(AID.BlastingZone, AID.DangerZone);

        //BowShock strategy
        res.Define(Track.BowShock).As<OGCDStrategy>("Bow Shock", "B.Shock", uiPriority: 150)
            .AddOption(OGCDStrategy.Automatic, "Auto", "Normal use of Bow Shock")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Bow Shock", 60, 15, ActionTargets.Self, 62)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Bow Shock in any next possible weave slot", 60, 15, ActionTargets.Self, 62)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Bow Shock in very next FIRST weave slot only", 60, 15, ActionTargets.Self, 62)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Bow Shock in very next LAST weave slot only", 60, 15, ActionTargets.Self, 62)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Bow Shock", 0, 0, ActionTargets.None, 62)
            .AddAssociatedActions(AID.BowShock);
        #endregion

        return res;
    }
    #endregion

    #region Priorities
    public enum GCDPriority //priorities for GCDs (higher number = higher priority)
    {
        None = 0,           //default
        Combo123 = 350,     //combo actions
        Gauge = 500,        //cartridge spender actions
        Reign = 525,        //Reign of Beasts
        comboNeed = 550,    //combo actions that need to be used
        GF23 = 575,         //Gnashing combo chain
        SonicBreak = 600,   //Sonic Break
        DoubleDown = 675,   //Double Down
        GF1 = 700,          //Gnashing Fang
        ForcedGCD = 900,    //Forced GCDs
    }
    public enum OGCDPriority //priorities for oGCDs (higher number = higher priority)
    {
        None = 0,           //default
        Continuation = 500, //Continuation procs
        Zone = 550,         //Blasting Zone
        BowShock = 600,     //Bow Shock
        Bloodfest = 700,    //Bloodfest
        NoMercy = 875,      //No Mercy
        Potion = 900,       //Potion
        ForcedOGCD = 900,   //Forced oGCDs
    }
    #endregion

    #region Upgrade Paths
    private AID BestZone //Determine the best Zone to use
        => Unlocked(AID.BlastingZone) //If Blasting Zone is unlocked
        ? AID.BlastingZone //Use Blasting Zone
        : AID.DangerZone; //Otherwise, use Danger Zone
    private AID BestCartSpender //Determine the best cartridge spender to use
        => ShouldUseFC //And we should use Fated Circle because of targets nearby
        ? BestFatedCircle //Use Fated Circle
        : canBS //Otherwise, if Burst Strike is available
        ? AID.BurstStrike //Use Burst Strike
        : NextBestRotation(); //Otherwise, use the next best rotation
    private AID BestFatedCircle //for AOE cart spending Lv30-71
        => Unlocked(AID.FatedCircle) //If Fated Circle is unlocked
        ? AID.FatedCircle //Use Fated Circle
        : AID.BurstStrike; //Otherwise, use Burst Strike
    private AID BestContinuation //Determine the best Continuation to use
        => hasRaze ? AID.FatedBrand //If we have Ready To Raze buff
        : hasBlast ? AID.Hypervelocity //If we have Ready To Blast buff
        : hasGouge ? AID.EyeGouge //If we have Ready To Gouge buff
        : hasTear ? AID.AbdomenTear //If we have Ready To Tear buff
        : hasRip ? AID.JugularRip //If we have Ready To Rip buff
        : AID.Continuation; //Otherwise, default to original hook
    #endregion

    #region Module Variables
    //Gauge
    public byte Ammo; //Range: 0-2 or 0-3 max; this counts current ammo count
    public byte GunComboStep; //0 = Gnashing Fang & Reign of Beasts, 1 = Savage Claw, 2 = Wicked Talon, 4 = NobleBlood, 5 = LionHeart
    public int MaxCartridges; //Maximum number of cartridges based on player level
    //Cooldown Related
    private float bfCD; //Time left on Bloodfest cooldown (120s base)
    private float nmLeft; //Time left on No Mercy buff (20s base)
    private float nmCD; //Time left on No Mercy cooldown (60s base)
    private bool inOdd; //Checks if player is in an odd-minute window
    private bool hasNM; //Checks self for No Mercy buff
    private bool hasBreak; //Checks self for Ready To Break buff
    private bool hasReign; //Checks self for Ready To Reign buff
    private bool hasBlast; //Checks self for Ready To Blast buff
    private bool hasRaze; //Checks self for Ready To Raze buff
    private bool hasRip; //Checks self for Ready To Rip buff
    private bool hasTear; //Checks self for Ready To Tear buff
    private bool hasGouge; //Checks self for Ready To Gouge buff
    private bool canNM; //Checks if No Mercy is completely available
    private bool canBS; //Checks if Burst Strike is completely available
    private bool canGF; //Checks if Gnashing Fang & its combo chain are completely available
    private bool canFC; //Checks if Fated Circle is completely available
    private bool canDD; //Checks if Double Down is completely available
    private bool canBF; //Checks if Bloodfest is completely available
    private bool canZone; //Checks if Danger / Blasting Zone is completely available
    private bool canBreak; //Checks if Sonic Break is completely available
    private bool canBow; //Checks if Bow Shock is completely available 
    private bool canContinue; //Checks if Continuation is completely available 
    private bool canReign; //Checks if Reign of Beasts & its combo chain are completely available
    private bool ShouldUseAOE; //Checks if AOE rotation should be used
    private bool ShouldUseFC; //Checks if Fated Circle should be used
    //Misc
    public bool inCombo; //Checks if player is already in a combo
    public bool canWeaveIn; //Can weave in oGCDs
    public bool canWeaveEarly; //Can early weave oGCDs
    public bool canWeaveLate; //Can late weave oGCDs
    public bool quarterWeave; //Can last second weave oGCDs
    public float PotionLeft; //Time left on potion buff (30s base)
    public float RaidBuffsLeft; //Time left on raid-wide buffs (typically 20s-22s)
    public float RaidBuffsIn; //Time until raid-wide buffs are applied again (typically 20s-22s)
    public float GCDLength; //Current GCD length, adjusted by skill speed/haste (2.5s baseline)
    public float BurstWindowLeft; //Time left in current burst window (typically 20s-22s)
    public float BurstWindowIn; //Time until next burst window (typically 20s-22s)
    public AID NextGCD; //Next global cooldown action to be used (needed for cartridge management)
    #endregion

    #region Module Helpers
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    private bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid); //Check if the desired trait is unlocked
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private AID ComboLastMove => (AID)World.Client.ComboState.Action; //Get the last action used in the combo sequence
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Check if the target is within melee range (3 yalms)
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.75; //Check if the target is within 5 yalms
    private bool ActionReady(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f; //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Check if this is the first GCD in combat
    private int TargetsInAOERange() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //Returns the number of targets hit by AOE within a 5-yalm radius around the player
    public bool HasEffect(SID sid, float duration) => SelfStatusLeft(sid, duration) > 0; //Checks if Status effect is on self
    public bool JustDid(AID aid) => Manager?.LastCast.Data?.IsSpell(aid) ?? false; //Check if the last action used was the desired ability
    public bool DidWithin(float variance) => (World.CurrentTime - Manager.LastCast.Time).TotalSeconds <= variance; //Check if the last action was used within a certain timeframe
    public bool JustUsed(AID aid, float variance) => JustDid(aid) && DidWithin(variance); //Check if the last action used was the desired ability & was used within a certain timeframe
    #endregion

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        //Gauge
        var gauge = World.Client.GetGauge<GunbreakerGauge>(); //Retrieve Gunbreaker gauge
        Ammo = gauge.Ammo; //Current cartridges
        GunComboStep = gauge.AmmoComboStep; //Combo step for Gnashing Fang or Reign of Beasts
        MaxCartridges = Unlocked(TraitID.CartridgeChargeII) ? 3 : 2; //Max cartridges based on level

        //Cooldowns
        bfCD = CD(AID.Bloodfest); //Bloodfest cooldown (120s)
        nmCD = CD(AID.NoMercy); //No Mercy cooldown (60s)
        nmLeft = SelfStatusLeft(SID.NoMercy, 20); //Remaining time for No Mercy buff (20s)
        hasBreak = HasEffect(SID.ReadyToBreak, 30); //Checks for Ready To Break buff
        hasReign = HasEffect(SID.ReadyToReign, 30); //Checks for Ready To Reign buff
        hasNM = nmCD is >= 39.5f and <= 60; //Checks if No Mercy is active
        hasBlast = Unlocked(AID.Hypervelocity) && HasEffect(SID.ReadyToBlast, 10f) && !JustUsed(AID.Hypervelocity, 10f); //Checks for Ready To Blast buff
        hasRaze = Unlocked(AID.FatedBrand) && HasEffect(SID.ReadyToRaze, 10f) && !JustUsed(AID.FatedBrand, 10f); //Checks for Ready To Raze buff
        hasRip = HasEffect(SID.ReadyToRip, 10f) && !JustUsed(AID.JugularRip, 10f); //Checks for Ready To Rip buff
        hasTear = HasEffect(SID.ReadyToTear, 10f) && !JustUsed(AID.AbdomenTear, 10f); //Checks for Ready To Tear buff
        hasGouge = HasEffect(SID.ReadyToGouge, 10f) && !JustUsed(AID.EyeGouge, 10f); //Checks for Ready To Gouge buff

        //GCD & Weaving
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        quarterWeave = GCD < 0.9f; //Can last second weave oGCDs
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on skill speed and haste
        NextGCD = AID.None; //Next global cooldown action to be used

        //Misc
        inOdd = bfCD is <= 90 and >= 30; //Checks if we are in an odd-minute window
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)
        ShouldUseAOE = //Determine if we should use AOE
            Unlocked(TraitID.MeleeMastery) //if Melee Mastery trait unlocked
            ? TargetsInAOERange() > 2 //use AOE if 3+ targets would be hit
            : TargetsInAOERange() > 1; //otherwise, use AOE if 2+ targets would be hit
        ShouldUseFC = TargetsInAOERange() > 1; //Determine if we should use Fated Circle
        var downtimeIn = Manager.Planner?.EstimateTimeToNextDowntime().Item2 ?? float.MaxValue; //Time until next downtime

        #region Minimal Requirements
        //Ammo-relative
        canNM = CD(AID.NoMercy) < 1; //No Mercy conditions
        canBS = Unlocked(AID.BurstStrike) && Ammo > 0; //Burst Strike conditions; -1 Ammo ST
        canGF = Unlocked(AID.GnashingFang) && ActionReady(AID.GnashingFang) && Ammo > 0; //Gnashing Fang conditions; -1 Ammo ST
        canFC = Unlocked(AID.FatedCircle) && Ammo > 0; //Fated Circle conditions; -1 Ammo AOE
        canDD = Unlocked(AID.DoubleDown) && ActionReady(AID.DoubleDown) && Ammo > 0; //Double Down conditions; -1 Ammo AOE
        canBF = Unlocked(AID.Bloodfest) && ActionReady(AID.Bloodfest); //Bloodfest conditions; +all Ammo (must have target)
        //Cooldown-relative
        canZone = Unlocked(AID.DangerZone) && ActionReady(AID.DangerZone); //Zone conditions
        canBreak = hasBreak && Unlocked(AID.SonicBreak); //Sonic Break conditions
        canBow = Unlocked(AID.BowShock) && ActionReady(AID.BowShock); //Bow Shock conditions
        canContinue = Unlocked(AID.Continuation); //Continuation conditions
        canReign = Unlocked(AID.ReignOfBeasts) && hasReign; //Reign of Beasts conditions
        #endregion

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var carts = strategy.Option(Track.Cartridges); //Cartridges track
        var cartStrat = carts.As<CartridgeStrategy>(); //Cartridges strategy
        var nm = strategy.Option(Track.NoMercy); //No Mercy track
        var nmStrat = nm.As<NoMercyStrategy>(); //No Mercy strategy
        var zone = strategy.Option(Track.Zone); //Zone track
        var zoneStrat = zone.As<OGCDStrategy>(); //Zone strategy
        var bow = strategy.Option(Track.BowShock); //Bow Shock track
        var bowStrat = bow.As<OGCDStrategy>(); //Bow Shock strategy
        var bf = strategy.Option(Track.Bloodfest); //Bloodfest track
        var bfStrat = bf.As<BloodfestStrategy>(); //Bloodfest strategy
        var dd = strategy.Option(Track.DoubleDown); //Double Down track
        var ddStrat = dd.As<GCDStrategy>(); //Double Down strategy
        var gf = strategy.Option(Track.GnashingFang); //Gnashing Fang track
        var gfStrat = gf.As<GnashingStrategy>(); //Gnashing Fang strategy
        var reign = strategy.Option(Track.Reign); //Reign of Beasts track
        var reignStrat = reign.As<ReignStrategy>(); //Reign of Beasts strategy
        var sb = strategy.Option(Track.SonicBreak); //Sonic Break track
        var sbStrat = sb.As<SonicBreakStrategy>(); //Sonic Break strategy
        var ls = strategy.Option(Track.LightningShot); //Lightning Shot track
        var lsStrat = ls.As<LightningShotStrategy>(); //Lightning Shot strategy
        var hold = strategy.Option(Track.Cooldowns).As<CooldownStrategy>() == CooldownStrategy.Hold; //Determine if holding resources
        var conserve = cartStrat == CartridgeStrategy.Conserve; //Determine if conserving cartridges
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotations (1-2-3 / 1-2)

        #region Force Execution
        if (AOEStrategy is AOEStrategy.ForceSTwithO) //if Single-target (with overcap protection) option is selected
            QueueGCD(STwithOvercap(), //queue the next single-target combo action with overcap protection
                ResolveTargetOverride(AOE.Value) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                GCDPriority.ForcedGCD); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceSTwithoutO) //if Single-target (without overcap protection) option is selected
            QueueGCD(STwithoutOvercap(), //queue the next single-target combo action without overcap protection
                ResolveTargetOverride(AOE.Value) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                GCDPriority.ForcedGCD); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceAOEwithO) //if AOE (with overcap protection) option is selected
            QueueGCD(AOEwithOvercap(), //queue the next AOE combo action with overcap protection
                Player, //on Self (no target needed)
                GCDPriority.ForcedGCD); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceAOEwithoutO) //if AOE (without overcap protection) option is selected
            QueueGCD(AOEwithoutOvercap(), //queue the next AOE combo action without overcap protection
                Player, //on Self (no target needed)
                GCDPriority.ForcedGCD);  //with priority for forced GCDs
        #endregion

        #region Logic for Cart Generation before Downtime
        //TODO: refactor this
        if (AOEStrategy == AOEStrategy.GenerateDowntime) //if Generate Downtime option is selected
        {
            if (downtimeIn == GCD * 2 && Ammo == 2 || //if 2 GCDs until downtime & has 2 cartridges
                downtimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 cartridge
                downtimeIn == GCD * 6 && Ammo == 0) //if 6 GCDs until downtime & has 0 cartridges
                QueueGCD(AID.DemonSlice, //queue Demon Slice
                    Player, //on Self (no target needed)
                    GCDPriority.ForcedGCD); //with priority for forced GCDs

            if (downtimeIn == GCD * 3 && Ammo == 2 || //if 3 GCDs until downtime & has 2 cartridges
                downtimeIn == GCD * 5 && Ammo == 1 || //if 5 GCDs until downtime & has 1 cartridge
                downtimeIn == GCD * 8 && Ammo == 0 || //if 8 GCDs until downtime & has 0 cartridges
                downtimeIn == GCD * 9 && Ammo == 0) //if 9 GCDs until downtime & has 0 cartridges
                QueueGCD(AID.KeenEdge, //queue Keen Edge
                    primaryTarget, //on the primary target
                    GCDPriority.ForcedGCD); //with priority for forced GCDs

            if (ComboLastMove == AID.DemonSlice && //if last move was Demon Slice
                (downtimeIn == GCD && Ammo == 2 || //if 1 GCD until downtime & has 2 cartridges
                downtimeIn == GCD * 3 && Ammo == 1 || //if 3 GCDs until downtime & has 1 cartridge
                downtimeIn == GCD * 5 && Ammo == 0)) //if 5 GCDs until downtime & has 0 cartridges
                QueueGCD(AID.DemonSlaughter, //queue Demon Slaughter
                    Player, //on Self (no target needed)
                    GCDPriority.ForcedGCD); //with priority for forced GCDs

            if (ComboLastMove == AID.KeenEdge && //if last move was Keen Edge
                (downtimeIn == GCD * 2 && Ammo == 2 || //if 2 GCDs until downtime & has 2 cartridges
                downtimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 cartridge
                downtimeIn == GCD * 7 && Ammo == 2 || //if 7 GCDs until downtime & has 2 cartridges
                downtimeIn == GCD * 8 && Ammo == 2)) //if 8 GCDs until downtime & has 2 cartridges
                QueueGCD(AID.BrutalShell, //queue Brutal Shell
                    primaryTarget, //on the primary target
                    GCDPriority.ForcedGCD); //with priority for forced GCDs

            if (ComboLastMove == AID.BrutalShell) //if last move was Brutal Shell
            {
                if (downtimeIn == GCD && (Ammo == 2 || Ammo == 3) || //if 1 GCD until downtime & has 2 or 3 cartridges
                    downtimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 cartridge
                    downtimeIn == GCD * 7 && Ammo == 0) //if 7 GCDs until downtime & has 0 cartridges
                    QueueGCD(AID.SolidBarrel, //queue Solid Barrel
                        primaryTarget, //on the primary target
                        GCDPriority.ForcedGCD); //with priority for forced GCDs
            }

            if (Ammo == MaxCartridges) //if at max cartridges
                QueueGCD(STwithoutOvercap(), //queue the next single-target combo action without overcap protection to save resources for uptime
                    primaryTarget, //on the primary target
                    GCDPriority.ForcedGCD); //with priority for forced GCDs
        }
        #endregion

        #region Standard Execution
        if (AOEStrategy == AOEStrategy.AutoBreakCombo) //if Break Combo option is selected
        {
            if (ShouldUseAOE) //if AOE rotation should be used
                QueueGCD(AOEwithoutOvercap(), //queue the next AOE combo action
                    Player, //on Self (no target needed)
                    GCDPriority.Combo123); //with priority for 123/12 combo actions
            if (!ShouldUseAOE)
                QueueGCD(STwithoutOvercap(), //queue the next single-target combo action
                    ResolveTargetOverride(AOE.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    GCDPriority.Combo123); //with priority for 123/12 combo actions
        }
        if (AOEStrategy == AOEStrategy.AutoFinishCombo) //if Finish Combo option is selected
        {
            QueueGCD(NextBestRotation(), //queue the next single-target combo action only if combo is finished
                ResolveTargetOverride(AOE.Value) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                GCDPriority.Combo123); //with priority for 123/12 combo actions
        }
        #endregion

        #endregion

        #region OGCDs
        if (!hold) //if not holding cooldowns
        {
            //No Mercy execution
            if (ShouldUseNoMercy(nmStrat, primaryTarget)) //if No Mercy should be used
                QueueOGCD(AID.NoMercy, //queue No Mercy
                    Player, //on Self (no target needed, but desired to not waste)
                    nmStrat is NoMercyStrategy.Force //if strategy option is Force
                    or NoMercyStrategy.ForceW //or Force weave
                    or NoMercyStrategy.ForceQW //or Force last second weave
                    or NoMercyStrategy.Force1 //or Force with 1 cartridge
                    or NoMercyStrategy.Force1W //or Force weave with 1 cartridge
                    or NoMercyStrategy.Force1QW //or Force last second weave with 1 cartridge
                    or NoMercyStrategy.Force2 //or Force with 2 cartridges
                    or NoMercyStrategy.Force2W //or Force weave with 2 cartridges
                    or NoMercyStrategy.Force2QW //or Force last second weave with 2 cartridges
                    or NoMercyStrategy.Force3 //or Force with 3 cartridges
                    or NoMercyStrategy.Force3W //or Force weave with 3 cartridges
                    or NoMercyStrategy.Force3QW //or Force last second weave with 3 cartridges
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.NoMercy); //otherwise, use intended priority

            //Zone execution (Blasting Zone / Danger Zone)
            if (ShouldUseZone(zoneStrat, primaryTarget)) //if Zone should be used
                QueueOGCD(BestZone, //queue the best Zone action
                    ResolveTargetOverride(zone.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    zoneStrat is OGCDStrategy.Force //if strategy option is Force
                    or OGCDStrategy.AnyWeave //or any Weave
                    or OGCDStrategy.EarlyWeave //or Early Weave
                    or OGCDStrategy.LateWeave //or Late Weave
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.Zone); //otherwise, use intended priority

            //Bow Shock execution
            if (ShouldUseBowShock(bowStrat, primaryTarget)) //if Bow Shock should be used
                QueueOGCD(AID.BowShock, //queue Bow Shock
                    Player, //on Self (no target needed)
                    bowStrat is OGCDStrategy.Force //if strategy option is Force
                    or OGCDStrategy.AnyWeave //or Any Weave
                    or OGCDStrategy.EarlyWeave //or Early Weave
                    or OGCDStrategy.LateWeave //or Late Weave
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.BowShock); //otherwise, use intended priority

            //Bloodfest execution
            if (ShouldUseBloodfest(bfStrat, primaryTarget)) //if Bloodfest should be used
                QueueOGCD(AID.Bloodfest, //queue Bloodfest
                    ResolveTargetOverride(bf.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    bfStrat is BloodfestStrategy.Force //if strategy option is Force
                    or BloodfestStrategy.ForceW //or Force weave
                    or BloodfestStrategy.Force0 //or Force with 0 cartridges
                    or BloodfestStrategy.Force0W //or Force weave with 0 cartridges
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.Bloodfest); //otherwise, use intended priority
        }

        //Continuation execution
        if (canContinue && //if Continuation is available
            (hasBlast || //and Ready To Blast buff is active
            hasRaze || //or Ready To Raze buff is active
            hasRip || //or Ready To Rip buff is active
            hasTear || //or Ready To Tear buff is active
            hasGouge)) //or Ready To Gouge buff is active
            QueueOGCD(BestContinuation, //queue the best Continuation action
                primaryTarget, //on the primary target
                canWeaveLate || GCD is 0 //if inside second weave slot & still havent used
                ? OGCDPriority.Continuation + 1201 //force the fuck out of this to prevent loss, any loss is very bad
                : OGCDPriority.Continuation); //otherwise, use intended priority

        #endregion

        #region GCDs
        if (!hold) //if not holding cooldowns
        {
            if (!conserve) //if not conserving cartridges
            {
                //Double Down execution
                if (ShouldUseDoubleDown(ddStrat, primaryTarget)) //if Double Down should be used
                    QueueGCD(AID.DoubleDown, //queue Double Down
                        primaryTarget, //on the primary target
                        ddStrat == GCDStrategy.Force || //or Force Double Down is selected on Double Down strategy
                        Ammo == 1 //or only 1 cartridge is available
                        ? GCDPriority.ForcedGCD //use priority for forced GCDs
                        : GCDPriority.DoubleDown); //otherwise, use intended priority
                //Gnashing Fang execution
                if (ShouldUseGnashingFang(gfStrat, primaryTarget)) //if Gnashing Fang should be used
                    QueueGCD(AID.GnashingFang, //queue Gnashing Fang
                        ResolveTargetOverride(gf.Value) //Get target choice
                        ?? primaryTarget, //if none, choose primary target
                        gfStrat == GnashingStrategy.ForceGnash //or Force Gnashing Fang is selected on Gnashing Fang strategy
                        ? GCDPriority.ForcedGCD //use priority for forced GCDs
                        : GCDPriority.GF1); //otherwise, use intended priority
                //Burst Strike & Fated Circle execution
                if (ShouldUseCartridges(cartStrat, primaryTarget)) //if Cartridges should be used
                {
                    //Optimal targeting & execution for both gauge spenders
                    if (cartStrat == CartridgeStrategy.Automatic) //if Automatic Cartridge strategy is selected
                        QueueGCD(BestCartSpender, //queue the best cartridge spender
                            ResolveTargetOverride(carts.Value) //Get target choice
                            ?? primaryTarget, //if none, choose primary target
                            nmCD < 1 && Ammo == 3 //if No Mercy is imminent and 3 cartridges are available
                            ? GCDPriority.ForcedGCD //use priority for forced GCDs
                            : GCDPriority.Gauge); //otherwise, use priority for gauge actions
                    //Burst Strike forced execution
                    if (cartStrat is CartridgeStrategy.OnlyBS or CartridgeStrategy.ForceBS) //if Burst Strike Cartridge strategies are selected
                        QueueGCD(AID.BurstStrike, //queue Burst Strike
                            ResolveTargetOverride(carts.Value) //Get target choice
                            ?? primaryTarget, //if none, choose primary target
                            GCDPriority.Gauge); //with priority for gauge actions
                    //Fated Circle forced execution
                    if (cartStrat is CartridgeStrategy.ForceFC or CartridgeStrategy.OnlyFC) //if Fated Circle Cartridge strategies are selected
                        QueueGCD(BestFatedCircle, //queue Fated Circle
                            primaryTarget ?? Player, //on Self (no target needed) if Fated Circle, on target if Burst Strike
                            GCDPriority.Gauge); //with priority for gauge actions
                }
            }

            //Sonic Break execution
            if (ShouldUseSonicBreak(sbStrat, primaryTarget)) //if Sonic Break should be used
                QueueGCD(AID.SonicBreak, //queue Sonic Break
                    ResolveTargetOverride(sb.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    sbStrat is SonicBreakStrategy.Force //if strategy option is Force
                    or SonicBreakStrategy.Early //or Early
                    ? GCDPriority.ForcedGCD //use priority for forced GCDs
                    : GCDPriority.SonicBreak); //otherwise, use intended priority
            //Reign of Beasts execution
            if (ShouldUseReign(reignStrat, primaryTarget)) //if Reign of Beasts should be used
                QueueGCD(AID.ReignOfBeasts, //queue Reign of Beasts
                    ResolveTargetOverride(reign.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    reignStrat == ReignStrategy.ForceReign //if Force Reign of Beasts is selected on Reign of Beasts strategy
                    ? GCDPriority.ForcedGCD //use priority for forced GCDs
                    : GCDPriority.Reign); //otherwise, use intended priority
        }

        //Gnashing Fang combo execution
        if (GunComboStep == 1) //if just used Gnashing Fang
            QueueGCD(AID.SavageClaw, //queue Savage Claw
                primaryTarget, //on the primary target
                gfStrat == GnashingStrategy.ForceClaw //if Force Savage Claw is selected on Gnashing Fang strategy
                ? GCDPriority.ForcedGCD //use priority for forced GCDs 
                : GCDPriority.GF23); //otherwise, use priority for Gnashing Fang combo steps
        if (GunComboStep == 2) //if just used Savage Claw
            QueueGCD(AID.WickedTalon, //queue Wicked Talon
                primaryTarget, //on the primary target
                gfStrat == GnashingStrategy.ForceTalon //if Force Wicked Talon is selected on Gnashing Fang strategy
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.GF23); //otherwise, use priority for Gnashing Fang combo steps
        //Reign of Beasts combo execution
        if (GunComboStep == 3) //if just used Wicked Talon
            QueueGCD(AID.NobleBlood, //queue Noble Blood
                primaryTarget, //on the primary target
                reignStrat == ReignStrategy.ForceNoble //if Force Noble Blood is selected on Reign of Beasts strategy
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.Reign); //otherwise, use priority for Reign of Beasts combo steps
        if (GunComboStep == 4) //if just used Noble Blood
            QueueGCD(AID.LionHeart, //queue Lion Heart
                primaryTarget, //on the primary target
                reignStrat == ReignStrategy.ForceLion //if Force Lion Heart is selected on Reign of Beasts strategy
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.Reign); //otherwise, use priority for Reign of Beasts combo steps
        //Lightning Shot execution
        if (ShouldUseLightningShot(primaryTarget, lsStrat)) //if Lightning Shot should be used
            QueueGCD(AID.LightningShot, //queue Lightning Shot
                ResolveTargetOverride(ls.Value) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                lsStrat is LightningShotStrategy.Force //if strategy option is Force
                or LightningShotStrategy.Allow //or Allow
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.Combo123); //otherwise, use priority for standard combo actions
        //Potion execution
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>())) //if Potion should be used
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, //queue the potion
                Player, //on Self (no target needed)
                ActionQueue.Priority.VeryHigh //with very high priority
                + (int)OGCDPriority.Potion, 0, GCD - 0.9f); //and the specified priority
        #endregion

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
    private AID NextBestRotation() => ComboLastMove switch
    {
        //ST
        AID.SolidBarrel => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
        AID.BrutalShell => STwithoutOvercap(),
        AID.KeenEdge => STwithoutOvercap(),
        //AOE
        AID.DemonSlaughter => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
        AID.DemonSlice => AOEwithoutOvercap(),
        _ => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
    };

    #region Single-Target Helpers
    private AID STwithOvercap() => ComboLastMove switch //with Overcap protection
    {
        AID.BrutalShell => Ammo == MaxCartridges ? BestCartSpender : AID.SolidBarrel, //if Brutal Shell is last move, use Solid Barrel regardless of ammo count
        AID.KeenEdge => AID.BrutalShell, //if Keen Edge is last move, use Brutal Shell
        _ => AID.KeenEdge, //start with Keen Edge
    };
    private AID STwithoutOvercap() => ComboLastMove switch //without Overcap protection
    {
        AID.BrutalShell => AID.SolidBarrel, //if Brutal Shell is last move, use Solid Barrel
        AID.KeenEdge => AID.BrutalShell, //if Keen Edge is last move, use Brutal Shell
        _ => AID.KeenEdge, //start with Keen Edge
    };
    #endregion

    #region AOE Helpers
    private AID AOEwithOvercap() => ComboLastMove switch //with Overcap protection
    {
        AID.DemonSlice => Ammo == MaxCartridges ? BestCartSpender : AID.DemonSlaughter, //if full ammo & Demon Slice is last move, use best cartridge spender
        _ => AID.DemonSlice, //start with Demon Slice
    };
    private AID AOEwithoutOvercap() => ComboLastMove switch //without Overcap protection
    {
        AID.DemonSlice => AID.DemonSlaughter, //if not at max ammo, use Demon Slaughter
        _ => AID.DemonSlice, //start with Demon Slice
    };
    #endregion

    #endregion

    #region Cooldown Helpers
    //No Mercy full strategy & conditions
    private bool ShouldUseNoMercy(NoMercyStrategy strategy, Actor? target) => strategy switch
    {
        NoMercyStrategy.Automatic =>
            //Standard conditions
            Player.InCombat && //In combat
            target != null && //Target exists
            canNM && //No Mercy is available
            ((Unlocked(AID.DoubleDown) && //Double Down is unlocked, indicating Lv90 or above
            (inOdd && Ammo >= 2) || //In Odd Window & conditions are met
            (!inOdd && Ammo < 3)) || //In Even Window & conditions are met
            (!Unlocked(AID.DoubleDown) && GCD < 0.9f && //Double Down is not unlocked, so we late weave it
            ((Unlocked(AID.Bloodfest) && //but Bloodfest is, indicating Lv80-89
            Ammo >= 1) || //Ammo is 1 or more
            (!Unlocked(AID.Bloodfest) && canGF) || //Bloodfest is not unlocked but Gnashing Fang is, indicating Lv60-79
            !Unlocked(AID.GnashingFang)))), //Gnashing Fang is not unlocked, indicating Lv59 and below
        NoMercyStrategy.Force => canNM, //Force No Mercy, regardless of correct weaving
        NoMercyStrategy.ForceW => canNM && canWeaveIn, //Force No Mercy into any weave slot 
        NoMercyStrategy.ForceQW => canNM && quarterWeave, //Force No Mercy into last possible second weave slot
        NoMercyStrategy.Force1 => canNM && Ammo == 1, //Force No Mercy if ready and 1 cartridge, regardless of weaving
        NoMercyStrategy.Force1W => canNM && canWeaveIn && Ammo == 1, //Force No Mercy into any weave slot if ready and 1 cartridge
        NoMercyStrategy.Force1QW => canNM && quarterWeave && Ammo == 1, //Force No Mercy into last possible second weave slot if ready and 1 cartridge
        NoMercyStrategy.Force2 => canNM && Ammo == 2, //Force No Mercy if ready and 2 cartridges, regardless of weaving
        NoMercyStrategy.Force2W => canNM && canWeaveIn && Ammo == 2, //Force No Mercy into any weave slot if ready and 2 cartridges
        NoMercyStrategy.Force2QW => canNM && quarterWeave && Ammo == 2, //Force No Mercy into last possible second weave slot if ready and 2 cartridges
        NoMercyStrategy.Force3 => canNM && Ammo == 3, //Force No Mercy if ready and 3 cartridges, regardless of weaving
        NoMercyStrategy.Force3W => canNM && canWeaveIn && Ammo == 3, //Force No Mercy into any weave slot if ready and 3 cartridges
        NoMercyStrategy.Force3QW => canNM && quarterWeave && Ammo == 3, //Force No Mercy into last possible second weave slot if ready and 3 cartridges
        NoMercyStrategy.Delay => false, //Delay No Mercy 
        _ => false
    };

    //Bloodfest full strategy & conditions
    private bool ShouldUseBloodfest(BloodfestStrategy strategy, Actor? target) => strategy switch
    {
        BloodfestStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            canBF && Ammo == 0, //Bloodfest is available and empty on cartridges
        BloodfestStrategy.Force => canBF, //Force Bloodfest, regardless of correct weaving
        BloodfestStrategy.ForceW => canBF && canWeaveIn, //Force Bloodfest into any weave slot
        BloodfestStrategy.Force0 => canBF && Ammo == 0, //Force Bloodfest if ready and 0 cartridges, regardless of weaving
        BloodfestStrategy.Force0W => canBF && Ammo == 0 && canWeaveIn, //Force Bloodfest into any weave slot if ready and 0 cartridges
        BloodfestStrategy.Delay => false, //Delay Bloodfest
        _ => false
    };

    //Zone full strategy & conditions
    private bool ShouldUseZone(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //In combat
            In3y(target) && //Target in melee range
            canZone && //Zone is available
            nmCD is < 57.55f and > 17, //No Mercy is active & not just used within 1GCD or CD is greater than 17s
        OGCDStrategy.Force => canZone, //Force Zone if available
        OGCDStrategy.AnyWeave => canZone && canWeaveIn, //Force Zone into any weave slot
        OGCDStrategy.EarlyWeave => canZone && canWeaveEarly, //Force weave Zone early
        OGCDStrategy.LateWeave => canZone && canWeaveLate, //Force weave Zone late
        OGCDStrategy.Delay => false,
        _ => false
    };

    //Bow Shock full strategy & conditions
    private bool ShouldUseBowShock(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //In combat
            ActionReady(AID.BowShock) && //Bow Shock is available
            In5y(target) && //Target in range
            nmCD is < 57.55f and >= 40, //No Mercy is active, but not just used within 1GCD
        OGCDStrategy.Force => canBow, //Force Bow Shock if available, regardless of weaving
        OGCDStrategy.AnyWeave => canBow && canWeaveIn, //Force Bow Shock into any weave slot
        OGCDStrategy.EarlyWeave => canBow && canWeaveEarly, //Force weave Bow Shock early
        OGCDStrategy.LateWeave => canBow && canWeaveLate, //Force weave Bow Shock late
        OGCDStrategy.Delay => false,
        _ => false
    };

    //Gauge full strategy & conditions
    private bool ShouldUseCartridges(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic =>
            ShouldUseFC //enough targets for optimal use of Fated Circle
            ? ShouldUseFatedCircle(CartridgeStrategy.Automatic, target) //use Fated Circle
            : ShouldUseBurstStrike(CartridgeStrategy.Automatic, target), //otherwise, use Burst Strike
        CartridgeStrategy.OnlyBS => ShouldUseBurstStrike(CartridgeStrategy.Automatic, target), //Optimally use Burst Strike
        CartridgeStrategy.OnlyFC => ShouldUseFatedCircle(CartridgeStrategy.Automatic, target), //Optimally use Fated Circle
        CartridgeStrategy.ForceBS => canBS, //Force Burst Strike
        CartridgeStrategy.ForceFC => canFC, //Force Fated Circle
        CartridgeStrategy.Conserve => false, //Conserve cartridges
        _ => false
    };

    //Double Down full strategy & conditions
    private bool ShouldUseDoubleDown(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In5y(target) && //Target in range
            canDD && //Double Down is available
            hasNM, //No Mercy is active
        GCDStrategy.Force => canDD, //Force Double Down if available
        GCDStrategy.Delay => false,
        _ => false
    };

    //Gnashing Fang & combo chain full strategy & conditions
    private bool ShouldUseGnashingFang(GnashingStrategy strategy, Actor? target) => strategy switch
    {
        GnashingStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In3y(target) && //Target in melee range
            canGF && //Gnashing Fang is available
            (nmLeft > 0 || hasNM || //No Mercy is active
            nmCD is < 35 and > 17), //or greater than 17s on No Mercy CD
        GnashingStrategy.ForceGnash => canGF, //Gnashing Fang is available
        GnashingStrategy.ForceClaw => Player.InCombat && GunComboStep == 1, //Force Savage Claw if available
        GnashingStrategy.ForceTalon => Player.InCombat && GunComboStep == 2, //Force Wicked Talon if available
        GnashingStrategy.Delay => false,
        _ => false
    };

    //Burst Strike & Fated Circle full strategy & conditions
    private bool ShouldUseBurstStrike(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In3y(target) && //Target in melee range
            canBS && //Burst Strike is available
            (hasNM || //No Mercy is active
            (!(bfCD is <= 90 and >= 30) &&
            nmCD < 1 &&
            Ammo == 3)) || //No Mercy is almost ready and full carts
            Ammo == MaxCartridges && ComboLastMove is AID.BrutalShell or AID.DemonSlice, //Full carts and last move was Brutal Shell or Demon Slice
        _ => false
    };

    //Fated Circle full strategy & conditions
    private bool ShouldUseFatedCircle(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In5y(target) && //Target in range
            canFC && //Fated Circle is available
            (hasNM || //No Mercy is active
            (!(bfCD is <= 90 and >= 30) &&
            nmCD < 1 &&
            Ammo == 3)) || //No Mercy is almost ready and full carts
            Ammo == MaxCartridges && ComboLastMove is AID.BrutalShell or AID.DemonSlice, //Full carts and last move was Brutal Shell or Demon Slice
        _ => false
    };

    //Sonic Break full strategy & conditions
    private bool ShouldUseSonicBreak(SonicBreakStrategy strategy, Actor? target) => strategy switch
    {
        SonicBreakStrategy.Automatic =>
            Player.InCombat && //In combat
            In3y(target) && //Target in melee range
            canBreak, //Sonic Break is available
        SonicBreakStrategy.Force => canBreak, //Force Sonic Break
        SonicBreakStrategy.Early => nmCD > 55 || hasBreak, //Use Sonic Break early
        SonicBreakStrategy.Late => nmLeft <= GCDLength, //Use Sonic Break late
        SonicBreakStrategy.Delay => false,
        _ => false
    };

    //Reign of Beasts & combo chain full strategy & conditions
    private bool ShouldUseReign(ReignStrategy strategy, Actor? target) => strategy switch
    {
        ReignStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            canReign && //Reign of Beasts is available
            hasNM && //No Mercy is active
            GunComboStep == 0, //not in GF combo
        ReignStrategy.ForceReign => canReign, //Force Reign of Beasts
        ReignStrategy.ForceNoble => Player.InCombat && GunComboStep == 3, //Force Noble Blood
        ReignStrategy.ForceLion => Player.InCombat && GunComboStep == 4, //Force Lion Heart
        ReignStrategy.Delay => false,
        _ => false
    };

    //Lightning Shot full strategy & conditions
    private bool ShouldUseLightningShot(Actor? target, LightningShotStrategy strategy) => strategy switch
    {
        LightningShotStrategy.OpenerFar =>
            (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && //Prepull or already in combat
            IsFirstGCD() && !In3y(target), //First GCD of fight and target is not in melee range
        LightningShotStrategy.OpenerForce => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD(), //Prepull or already in combat and first GCD of fight
        LightningShotStrategy.Force => true, //Force Lightning Shot, regardless of any cooldowns or GCDs
        LightningShotStrategy.Allow => !In3y(target), //Use Lightning Shot if target is not in melee range
        LightningShotStrategy.Forbid => false, //Do not use Lightning Shot
        _ => false
    };

    //Potion full strategy & conditions
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => //Use potion when buffs are imminent
            nmCD < 5 && //No Mercy is almost ready
            bfCD < 15, //Bloodfest is almost ready
        PotionStrategy.Immediate => true, //Use potion immediately
        _ => false
    };
    #endregion
}
