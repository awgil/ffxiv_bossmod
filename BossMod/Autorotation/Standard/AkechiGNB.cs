﻿using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance
//This module currently supports only <=2.47 SkS rotation, as it's the easiest to function & requires way less conditions to operate
//2.5 SkS support will be added later when optimizing this module further

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
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
        DoubleDown,      //Double Down ability tracking
        BurstStrike,     //Burst Strike ability tracking
        FatedCircle,     //Fated Circle ability tracking
        Zone,            //Blasting Zone or Danger Zone tracking
        Bloodfest,       //Bloodfest ability tracking
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
        Automatic,        //Automatically decide when to use Gnashing Fang
        ForceGnash,       //Force the use of Gnashing Fang regardless of conditions
        ForceClaw,        //Force the use of Savage Claw action when in combo
        ForceTalon,       //Force the use of Wicked Talon action when in combo
        Delay             //Delay the use of Gnashing Fang for strategic reasons
    }

    //Defines different offensive strategies that dictate how abilities and resources are used during combat
    public enum OffensiveStrategy
    {
        Automatic,      //Automatically decide when to use offensive abilities
        Force,          //Force the use of offensive abilities regardless of conditions
        Delay           //Delay the use of offensive abilities for strategic reasons
    }

    public static RotationModuleDefinition Definition()
    {
        //Module title & signature
        var res = new RotationModuleDefinition("GNB (Akechi)", "Standard Rotation Module", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.GNB), 100);

        //Custom strategies
        //Targeting strategy
        res.Define(Track.AoE).As<AOEStrategy>("Combo Option", "AoE", uiPriority: 200)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use ST rotation (with overcap protection)")
            .AddOption(AOEStrategy.FocusSingleTarget, "ST", "Use ST rotation (without overcap protection)")
            .AddOption(AOEStrategy.ForceAoE, "AoE", "Use AoE rotation (with overcap protection)")
            .AddOption(AOEStrategy.FocusAoE, "AoE", "Use AoE rotation (without overcap protection)")
            .AddOption(AOEStrategy.Auto, "Auto", "Use AoE rotation if 3+ targets would be hit, otherwise use ST rotation; break combo if necessary")
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto Finish Combo", "Use AoE rotation if 3+ targets would be hit, otherwise use ST rotation; finish combo before switching")
            .AddOption(AOEStrategy.GenerateDowntime, "Generate before Downtime", "Estimates time until disengagement & determines when to use ST or AoE combo to generate carts appropriately before downtime")
            ;

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

        //Offensive Strategies
        //NoMercy strategy
        res.Define(Track.NoMercy).As<OffensiveStrategy>("No Mercy", "N.Mercy", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP (even during downtime)", 60, 20, ActionTargets.Self, 2)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(GNB.AID.NoMercy);

        //SonicBreak strategy
        res.Define(Track.SonicBreak).As<OffensiveStrategy>("Sonic Break", "S.Break", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Sonic Break")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Sonic Break", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Sonic Break", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(GNB.AID.SonicBreak);

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

        //Bloodfest strategy
        res.Define(Track.Bloodfest).As<OffensiveStrategy>("Bloodfest", "Fest", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Bloodfest")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Bloodfest", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Bloodfest", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(GNB.AID.Bloodfest);

        //BowShock strategy
        res.Define(Track.BowShock).As<OffensiveStrategy>("Bow Shock", "B.Shock", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Bow Shock")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Bow Shock", 60, 15, ActionTargets.Self, 62)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Bow Shock", 0, 0, ActionTargets.None, 62)
            .AddAssociatedActions(GNB.AID.BowShock);

        return res;

    }

    public enum GCDPriority //Priority for GCDs used
    {
        None = 0,
        Combo123 = 350,
        NormalBS = 600,
        NormalSB = 670,
        GF23 = 660,
        NormalDD = 680,
        GF1 = 690,
        NormalGCD = 700,
        ForcedLightningShot = 850,
        ForcedSonicBreak = 860,
        ForcedBurstStrike = 870,
        ForcedDoubleDown = 880,
        ForcedGnashing = 890,
        ForcedGCD = 900,
        StopAll = 980,
    }

    public enum OGCDPriority //Priority for oGCDs used
    {
        None = 0,
        Continuation = 510,
        Zone = 540,
        BowShock = 550,
        Continuation1 = 580,
        Bloodfest = 600,
        NoMercy = 850,
        Potion = 900,
        ContinuationNeed = 950,
        StopAll = 980,
    }

    public byte Ammo; //Range: 0-2, 0-3 - current ammo count
    public byte GunComboStep; //0 = Gnashing Fang & Reign of Beasts, 1 = Savage Claw, 2 = Wicked Talon, etc.
    public int MaxCartridges; //Maximum number of cartridges based on player level

    private float GCDLength; //Current GCD length, adjusted by skill speed/haste (2.5s baseline)
    private float bfCD; //Time left on Bloodfest cooldown (120s base)
    private float nmLeft; //Time left on No Mercy buff (20s base)
    private float nmCD; //Time left on No Mercy cooldown (60s base)

    private float PotionLeft; //Time left on potion buff (typically 30s)
    private float RaidBuffsLeft; //Time left on raid-wide buffs (typically 20s-22s)
    private float RaidBuffsIn; //Time until raid-wide buffs are applied again (typically 20s-22s)

    public float BurstWindowLeft; //Time left in current burst window (typically 20s-22s)
    public float BurstWindowIn; //Time until next burst window (typically 20s-22s)

    private bool hasNM; //Checks self for No Mercy buff
    private bool hasBreak; //Checks self for Ready To Break buff
    private bool hasReign; //Checks self for Ready To Reign buff
    private bool hasBlast; //Checks self for Ready To Blast buff
    private bool hasRaze; //Checks self for Ready To Raze buff
    private bool hasRip; //Checks self for Ready To Rip buff
    private bool hasTear; //Checks self for Ready To Tear buff
    private bool hasGouge; //Checks self for Ready To Gouge buff

    public GNB.AID NextGCD; //Next global cooldown action to be used (needed for cartridge management)
    private GCDPriority NextGCDPrio; //Priority of the next GCD, used for decision making on cooldowns

    //Check if the desired ability is unlocked
    private bool Unlocked(GNB.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));

    //Check if the desired trait is unlocked
    private bool Unlocked(GNB.TraitID tid) => TraitUnlocked((uint)tid);

    //Get remaining cooldown time for the specified action
    private float CD(GNB.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    //Check if we can fit an additional GCD within the provided deadline
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline;

    //Get the last action used in the combo sequence
    private GNB.AID ComboLastMove => (GNB.AID)World.Client.ComboState.Action;

    //Check if the target is within melee range (3 yalms)
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 3;

    //Check if the target is within 5 yalms
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.75;

    //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool ActionReady(GNB.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f;

    //Check if this is the first GCD in combat
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    //Returns the number of targets hit by AoE within a 5-yalm radius around the player
    private int NumTargetsHitByAoE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    //Checks if the potion should be used before raid buffs expire
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f;

    //Checks if Status effect is on self
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        //Gauge values
        var gauge = World.Client.GetGauge<GunbreakerGauge>(); //Retrieve Gunbreaker gauge
        Ammo = gauge.Ammo; //Current cartridges
        GunComboStep = gauge.AmmoComboStep; //Combo step for Gnashing Fang or Reign of Beasts
        MaxCartridges = Unlocked(GNB.TraitID.CartridgeChargeII) ? 3 : 2; //Max cartridges based on level

        //Cooldowns and buff timers
        bfCD = CD(GNB.AID.Bloodfest); //Bloodfest cooldown (120s)
        nmCD = CD(GNB.AID.NoMercy); //No Mercy cooldown (60s)
        nmLeft = SelfStatusLeft(GNB.SID.NoMercy); //Remaining time for No Mercy buff (20s)
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on skill speed and haste

        //Buff durations
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)

        //Buff and cooldown checks
        hasBreak = HasEffect(GNB.SID.ReadyToBreak); //Checks for Ready To Break buff
        hasReign = HasEffect(GNB.SID.ReadyToReign); //Checks for Ready To Reign buff
        hasNM = nmCD is >= 40 and <= 60; //Checks if No Mercy is active
        hasBlast = HasEffect(GNB.SID.ReadyToBlast); //Checks for Ready To Blast buff
        hasRaze = HasEffect(GNB.SID.ReadyToRaze); //Checks for Ready To Raze buff
        hasRip = HasEffect(GNB.SID.ReadyToRip); //Checks for Ready To Rip buff
        hasTear = HasEffect(GNB.SID.ReadyToTear); //Checks for Ready To Tear buff
        hasGouge = HasEffect(GNB.SID.ReadyToGouge); //Checks for Ready To Gouge buff

        //Raid buff timings
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);

        //Next GCD action
        NextGCD = GNB.AID.None;
        NextGCDPrio = GCDPriority.None;

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

        //GCD minimal conditions
        var canSonic = hasBreak && Unlocked(GNB.AID.SonicBreak);
        var canDD = Ammo >= 2 && Unlocked(GNB.AID.DoubleDown);
        var canBSlv80 = Ammo >= 1 && Unlocked(GNB.AID.BurstStrike) && Unlocked(GNB.AID.Bloodfest);
        var canBSlv70 = ((Ammo == MaxCartridges && ComboLastMove is GNB.AID.BrutalShell) ||
                         (nmLeft > 0 && Ammo > 0)) &&
                        Unlocked(GNB.AID.BurstStrike) && !Unlocked(GNB.AID.Bloodfest);
        var canGF = Ammo >= 1 && Unlocked(GNB.AID.GnashingFang);
        var canFC = Ammo >= 1 && Unlocked(GNB.AID.FatedCircle);

        //Determine and queue combo action
        var (comboAction, comboPrio) = ComboActionPriority(AOEStrategy, AoETargets, burstStrategy, burst.Value.ExpireIn);
        QueueGCD(comboAction, comboAction is GNB.AID.DemonSlice or GNB.AID.DemonSlaughter ? Player : primaryTarget, comboPrio);

        //Focused actions for AoE strategies
        if (AOEStrategy == AOEStrategy.FocusSingleTarget)
        {
            var action = NextForceSingleTarget();
            QueueGCD(action, primaryTarget, GCDPriority.ForcedGCD);
        }

        if (AOEStrategy == AOEStrategy.FocusAoE)
        {
            var action = NextForceAoE();
            QueueGCD(action, primaryTarget, GCDPriority.ForcedGCD);
        }

        //Estimate time to next downtime
        var downtimeIn = Manager.Planner?.EstimateTimeToNextDowntime().Item2 ?? float.MaxValue;
        var comboStepsRemaining = ComboLastMove switch
        {
            GNB.AID.KeenEdge => Unlocked(GNB.AID.SolidBarrel) ? 2 : Unlocked(GNB.AID.BrutalShell) ? 1 : 0,
            GNB.AID.DemonSlice => Unlocked(GNB.AID.DemonSlaughter) ? 1 : 0,
            _ => 0
        };

        //Generate downtime logic
        if (AOEStrategy == AOEStrategy.GenerateDowntime)
        {
            if (comboStepsRemaining == 0) //Not in any combo
            {
                if ((downtimeIn == GCD * 2 && Ammo == 2) ||
                    (downtimeIn == GCD * 4 && Ammo == 1) ||
                    (downtimeIn == GCD * 6 && Ammo == 0))
                {
                    QueueGCD(GNB.AID.DemonSlice, Player, GCDPriority.ForcedGCD);
                }

                if ((downtimeIn == GCD * 3 && Ammo == 2) ||
                    (downtimeIn == GCD * 5 && Ammo == 1) ||
                    (downtimeIn == GCD * 8 && Ammo == 0) ||
                    (downtimeIn == GCD * 9 && Ammo == 0))
                {
                    QueueGCD(GNB.AID.KeenEdge, primaryTarget, GCDPriority.ForcedGCD);
                }
            }

            if (comboStepsRemaining == 1) //Combo initiated
            {
                if ((downtimeIn == GCD && Ammo == 2) ||
                    (downtimeIn == GCD * 3 && Ammo == 1) ||
                    (downtimeIn == GCD * 5 && Ammo == 0) &&
                    ComboLastMove == GNB.AID.DemonSlice)
                {
                    QueueGCD(GNB.AID.DemonSlaughter, Player, GCDPriority.ForcedGCD);
                }

                if ((downtimeIn == GCD * 2 && Ammo == 2) ||
                    (downtimeIn == GCD * 4 && Ammo == 1) ||
                    (downtimeIn == GCD * 7 && Ammo == 2) ||
                    (downtimeIn == GCD * 8 && Ammo == 2) &&
                    ComboLastMove == GNB.AID.KeenEdge)
                {
                    QueueGCD(GNB.AID.BrutalShell, primaryTarget, GCDPriority.ForcedGCD);
                }
            }

            if (comboStepsRemaining == 2)
            {
                if ((downtimeIn == GCD && (Ammo == 2 || Ammo == 3)) ||
                    (downtimeIn == GCD * 4 && Ammo == 1) ||
                    (downtimeIn == GCD * 7 && Ammo == 0) &&
                    ComboLastMove == GNB.AID.BrutalShell)
                {
                    QueueGCD(GNB.AID.SolidBarrel, primaryTarget, GCDPriority.ForcedGCD);
                }
            }

            if (Ammo == MaxCartridges)
                QueueGCD(NextForceSingleTarget(), primaryTarget, GCDPriority.ForcedGCD);
        }

        //No Mercy execution
        if (!hold && ShouldUseNoMercy(strategy.Option(Track.NoMercy).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.NoMercy, Player, OGCDPriority.NoMercy);

        //Zone execution
        if (!hold && ShouldUseZone(strategy.Option(Track.Zone).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(Unlocked(GNB.AID.BlastingZone) ? GNB.AID.BlastingZone : GNB.AID.DangerZone, primaryTarget, OGCDPriority.Zone);

        //BowShock execution
        if (!hold && ShouldUseBowShock(strategy.Option(Track.BowShock).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.BowShock, primaryTarget, OGCDPriority.BowShock);

        //Bloodfest execution
        if (!hold && ShouldUseBloodfest(strategy.Option(Track.Bloodfest).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.Bloodfest, primaryTarget, OGCDPriority.Bloodfest);

        //Continuation execution
        if (Unlocked(GNB.AID.Continuation))
        {
            if (hasRip)
                QueueOGCD(GNB.AID.JugularRip, primaryTarget, OGCDPriority.ContinuationNeed);
            if (hasTear)
                QueueOGCD(GNB.AID.AbdomenTear, primaryTarget, OGCDPriority.ContinuationNeed);
            if (hasGouge)
                QueueOGCD(GNB.AID.EyeGouge, primaryTarget, OGCDPriority.ContinuationNeed);
            if (hasBlast || ComboLastMove is GNB.AID.BurstStrike)
                QueueOGCD(GNB.AID.Hypervelocity, primaryTarget, OGCDPriority.ContinuationNeed);
            if (hasRaze || ComboLastMove is GNB.AID.FatedCircle)
                QueueOGCD(GNB.AID.FatedBrand, primaryTarget, OGCDPriority.ContinuationNeed);
        }

        //Gnashing Fang execution
        if (!hold && canGF && ShouldUseGnashingFang(strategy.Option(Track.GnashingFang).As<GnashingStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.GnashingFang, primaryTarget, GCDPriority.GF1);

        //Double Down execution
        if (!hold && canDD && ShouldUseDoubleDown(strategy.Option(Track.DoubleDown).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.DoubleDown, primaryTarget, GCDPriority.NormalDD);

        //Gnashing Fang's Combo execution
        if (GunComboStep is 1)
            QueueGCD(GNB.AID.SavageClaw, primaryTarget, GCDPriority.GF23);
        if (GunComboStep is 2)
            QueueGCD(GNB.AID.WickedTalon, primaryTarget, GCDPriority.GF23);

        //Reign Of Beasts execution
        if (hasReign && GunComboStep is 0 && !ActionReady(GNB.AID.DoubleDown))
            QueueGCD(GNB.AID.ReignOfBeasts, primaryTarget, GCDPriority.NormalGCD);

        //Reign Combo execution
        if (GunComboStep is 3)
            QueueGCD(GNB.AID.NobleBlood, primaryTarget, GCDPriority.NormalGCD);
        if (GunComboStep is 4)
            QueueGCD(GNB.AID.LionHeart, primaryTarget, GCDPriority.NormalGCD);

        //Sonic Break execution
        var forceBreak = strategy.Option(Track.SonicBreak).As<OffensiveStrategy>();
        if (canSonic && hasNM)
        {
            if (forceBreak == OffensiveStrategy.Force) //Force without breaking Autorot
            {
                QueueGCD(GNB.AID.SonicBreak, primaryTarget, GCDPriority.ForcedSonicBreak);
            }
            else if (ShouldUseSonicBreak(forceBreak, primaryTarget))
            {
                QueueGCD(GNB.AID.SonicBreak, primaryTarget, GCDPriority.NormalSB);
            }
        }

        //Burst Strike execution
        if (canBSlv80 && ShouldUseBurstStrike(strategy.Option(Track.BurstStrike).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.BurstStrike, primaryTarget, GCDPriority.NormalBS);

        //Fated Circle execution
        if (canFC && ShouldUseFatedCircle(strategy.Option(Track.FatedCircle).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.BurstStrike, primaryTarget, GCDPriority.NormalBS);
        else if (!canFC && canBSlv70)
        {
            var action = UseCorrectBS(AoETargets);
            QueueGCD(action, primaryTarget, GCDPriority.NormalBS);
        }

        //Lightning Shot execution
        if (ShouldUseLightningShot(primaryTarget, strategy.Option(Track.LightningShot).As<LightningShotStrategy>()))
            QueueGCD(GNB.AID.LightningShot, primaryTarget, GCDPriority.ForcedLightningShot);

        //Potion execution
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
    }

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

    private GNB.AID UseCorrectBS(int AoETargets) //Determines whether to use BurstStrike or FatedCircle based on conditions
    {
        //If under No Mercy and FatedCircle is not unlocked, use BurstStrike even in single-target situations
        if (Ammo == MaxCartridges && ComboLastMove is GNB.AID.BrutalShell)
            return Unlocked(GNB.AID.FatedCircle) ? GNB.AID.FatedCircle : GNB.AID.BurstStrike;

        //Optimal AoE usage for specific target counts
        var hasStrike = Unlocked(GNB.AID.BurstStrike);
        var hasCircle = Unlocked(GNB.AID.FatedCircle);

        //For Lv73 or lower (without FatedCircle), prefer BurstStrike on 2+ targets to prevent overcapping
        if (hasStrike && AoETargets >= 2)
            return hasCircle ? GNB.AID.FatedCircle : GNB.AID.BurstStrike;

        //Default to BurstStrike if available
        return hasStrike ? GNB.AID.BurstStrike : GNB.AID.BurstStrike; //This line seems redundant
    }

    private GNB.AID NextComboSingleTarget() => ComboLastMove switch //Determines the next single-target action based on the last action used
    {
        GNB.AID.BrutalShell => Ammo == MaxCartridges ? GNB.AID.BurstStrike : GNB.AID.SolidBarrel,
        GNB.AID.KeenEdge => GNB.AID.BrutalShell,
        _ => GNB.AID.KeenEdge,
    };

    private GNB.AID NextComboAoE() => ComboLastMove switch //Determines the next AoE action based on the last action used
    {
        GNB.AID.DemonSlice => Ammo == MaxCartridges
                              ? (Unlocked(GNB.AID.FatedCircle) ? GNB.AID.FatedCircle : GNB.AID.BurstStrike)
                              : GNB.AID.DemonSlaughter,
        _ => GNB.AID.DemonSlice,
    };

    private GNB.AID NextForceSingleTarget() => ComboLastMove switch
    {
        GNB.AID.BrutalShell => GNB.AID.SolidBarrel,
        GNB.AID.KeenEdge => GNB.AID.BrutalShell,
        _ => GNB.AID.KeenEdge,
    };

    private GNB.AID NextForceAoE() => ComboLastMove switch
    {
        GNB.AID.DemonSlice => GNB.AID.DemonSlaughter,
        _ => GNB.AID.DemonSlice,
    };

    private int AmmoGainedFromAction(GNB.AID action) => action switch //Returns the amount of ammo gained from specific actions
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
                : (Unlocked(GNB.AID.DoubleDown) ? AoETargets >= 3 : AoETargets >= 2),
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
        return (nextAction, riskingAmmo ? GCDPriority.NormalGCD : GCDPriority.Combo123);
    }

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
    //NOTE: Using SkS GNB for now; use No Mercy as soon as full cartridges are available
    private bool ShouldUseNoMercy(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && target != null && ActionReady(GNB.AID.NoMercy) && GCD < 0.9f && (
                (Ammo == 1 && bfCD == 0 && Unlocked(GNB.AID.Bloodfest) && Unlocked(GNB.AID.DoubleDown)) || //Lv90+ Opener
                (Ammo >= 1 && bfCD == 0 && Unlocked(GNB.AID.Bloodfest) && !Unlocked(GNB.AID.DoubleDown)) || //Lv80+ Opener
                (!Unlocked(GNB.AID.Bloodfest) && Ammo >= 1 && ActionReady(GNB.AID.GnashingFang)) || //Lv70 & below
                (Ammo == MaxCartridges) //60s & 120s burst windows
            ),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Bloodfest
    private bool ShouldUseBloodfest(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(GNB.AID.Bloodfest) && Ammo == 0 && hasNM,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Zone
    private bool ShouldUseZone(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && nmCD is < 57.55f and > 17 &&
            ActionReady(Unlocked(GNB.AID.BlastingZone) ? GNB.AID.BlastingZone : GNB.AID.DangerZone),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use BowShock
    private bool ShouldUseBowShock(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(GNB.AID.BowShock) && In5y(target) && nmCD is < 57.55f and > 17,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Sonic Break
    private bool ShouldUseSonicBreak(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && hasNM && hasBreak,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Double Down
    private bool ShouldUseDoubleDown(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(GNB.AID.DoubleDown) && In5y(target) && hasNM && Ammo >= 2,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Gnashing Fang
    private bool ShouldUseGnashingFang(GnashingStrategy strategy, Actor? target)
    {
        return strategy switch
        {
            GnashingStrategy.Automatic =>
                Player.InCombat && Ammo >= 1 && In3y(target) && ActionReady(GNB.AID.GnashingFang) &&
                (nmLeft > 0 || hasNM || nmCD is < 35 and > 17),
            GnashingStrategy.ForceGnash => Player.InCombat && GunComboStep == 0 && Ammo >= 1,
            GnashingStrategy.ForceClaw => Player.InCombat && GunComboStep == 1,
            GnashingStrategy.ForceTalon => Player.InCombat && GunComboStep == 2,
            GnashingStrategy.Delay => false,
            _ => false
        };
    }

    //Determines when to use Burst Strike
    private bool ShouldUseBurstStrike(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) &&
            (
                (Unlocked(GNB.AID.DoubleDown) && hasNM && !ActionReady(GNB.AID.DoubleDown) && GunComboStep == 0 && !hasReign) || //Lv90+
                (!Unlocked(GNB.AID.DoubleDown) && !ActionReady(GNB.AID.GnashingFang) && hasNM && GunComboStep == 0) || //Lv80 & Below
                (ComboLastMove == GNB.AID.BrutalShell && Ammo == MaxCartridges) //Overcap
            ),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Fated Circle
    private bool ShouldUseFatedCircle(OffensiveStrategy strategy, Actor? AoETargets) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(AoETargets) &&
            (
                (hasNM && !ActionReady(GNB.AID.DoubleDown) && Ammo > 0) ||
                (ComboLastMove == GNB.AID.DemonSlice && Ammo == MaxCartridges)
            ),
        OffensiveStrategy.Force => true,
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
                ActionReady(GNB.AID.Bloodfest) || //Opener
                (bfCD < 15 || ActionReady(GNB.AID.Bloodfest)) && Ammo == 3);
    }

    //Determines when to use a potion based on strategy
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs =>
            (IsPotionAlignedWithNM() || (nmCD < 5 && bfCD < 15)),
        PotionStrategy.Immediate => true,
        _ => false
    };

    /* WIP NoMercy options
    private bool ShouldUseNoMercy(OffensiveStrategy strategy, Actor? player)
    {
        var nm = strategy.Option(Track.NoMercy);
        var nmStrat = nm.As<OffensiveStrategy>();
        if (nmStrat == OffensiveStrategy.Delay)
        {
            return false;
        }
        else if (nmStrat == OffensiveStrategy.Force)
        {
            return true;
        }
        else
        {
            bool GFready = CD(GNB.AID.GnashingFang) < GCD && Unlocked(GNB.AID.GnashingFang); 
            bool SBready = nmCD >= 40 && Unlocked(GNB.AID.SonicBreak) && ReadyToBreak;
            bool DDready = CD(GNB.AID.DoubleDown) < GCD && Unlocked(GNB.AID.DoubleDown);
            bool sublv30NM = Player.InCombat && !Unlocked(GNB.AID.BurstStrike);
            bool fastBurstReady = Player.InCombat && Unlocked(GNB.AID.Bloodfest) && (Ammo is 1 || Ammo == MaxCartridges) && GCD < 0.8 && GFready;
            /* bool slowBurstReady = Player.InCombat && Unlocked(GNB.AID.Bloodfest) && (Ammo == 0 || Ammo == MaxCartridges) && GCD < 0.8;

            bool slowNM = //(!gnbConfig.fastNM &&
                (GFready || SBready || DDready)
                && Player.InCombat
                && ((Ammo == MaxCartridges)
                || (Ammo == MaxCartridges - 1 && ComboLastMove == GNB.AID.BrutalShell && bfCD > 20)
                || (bfCD < 15 && Ammo == 1 && Unlocked(GNB.AID.Bloodfest)))) || shouldUseEarlyNoMercy; 

            bool fastNM = //(gnbConfig.fastNM &&
                GCD < 0.8f &&
                (GFready || SBready || DDready)
                && Player.InCombat
                && fastBurstReady;
            return slowNM || fastNM || sublv30NM;
        }
    }*/
}
