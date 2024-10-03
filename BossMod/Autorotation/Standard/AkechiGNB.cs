using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance
//This module currently supports only <=2.47 SkS rotation, as it's the easiest to function & requires way less conditions to operate
//2.5 SkS support will be added later when optimizing this module further

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { AoE, Burst, Potion, LightningShot, GnashingFang, NoMercy, SonicBreak, DoubleDown, BurstStrike, FatedCircle, Zone, Bloodfest, BowShock, Trajectory } //What we're tracking for Cooldown Planner usage
    public enum AOEStrategy { SingleTarget, ForceAoE, Auto, AutoFinishCombo } //Targeting strategy
    public enum BurstStrategy { Automatic, SpendCarts, ConserveCarts, UnderRaidBuffs, UnderPotion } //Burst strategy
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate } //Pots strategy
    public enum LightningShotStrategy { OpenerRanged, Opener, Force, Ranged, Forbid } //RangedUptime strategy
    public enum GnashingStrategy { Automatic, ForceGnash, ForceClaw, ForceTalon, Delay } //GnashingFang combo strategy
    public enum OffensiveStrategy { Automatic, Force, Delay } //CDs strategies

    public static RotationModuleDefinition Definition()
    {
        //Our module title & signature
        var res = new RotationModuleDefinition("GNB (Akechi)", "Standard Rotation Module", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.GNB), 100);

        //Our custom strategies
        //Targeting strategy
        res.Define(Track.AoE).As<AOEStrategy>("Combo Option", "AoE", uiPriority: 90)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use ST rotation")
            .AddOption(AOEStrategy.ForceAoE, "AoE", "Use AoE rotation")
            .AddOption(AOEStrategy.Auto, "Auto", "Use AoE rotation if 3+ targets would be hit, otherwise use ST rotation; break combo if necessary")
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto Finish Combo", "Use aoe rotation if 3+ targets would be hit, otherwise use single-target rotation; finish combo route before switching");
        //Burst strategy
        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 80)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Spend Carts under potion/raidbuffs, otherwise conserve")
            .AddOption(BurstStrategy.SpendCarts, "Spend Carts", "Spend Carts freely (as if inside burst window)")
            .AddOption(BurstStrategy.ConserveCarts, "Conserve Carts", "Conserve Carts as much as possible (as if outside burst window)")
            .AddOption(BurstStrategy.UnderRaidBuffs, "Under RaidBuffs", "Spend Carts under raidbuffs, otherwise conserve; ignore potion (useful if potion is delayed)")
            .AddOption(BurstStrategy.UnderPotion, "Under Potion", "Spend Carts gauge under potion, otherwise conserve; ignore raidbuffs (useful for misaligned potions)");
        //Pots strategy
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 70)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with 2-minute raid buffs (0/6, 2/8, etc)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, even if without No Mercy (0/4:30/9)", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        //RangedUptime strategy
        res.Define(Track.LightningShot).As<LightningShotStrategy>("Lightning Shot", "LShot", uiPriority: 20)
            .AddOption(LightningShotStrategy.OpenerRanged, "OpenerRanged", "Use as very first GCD and only if outside melee range")
            .AddOption(LightningShotStrategy.Opener, "Opener", "Use as very first GCD regardless of range")
            .AddOption(LightningShotStrategy.Force, "Force", "Force use ASAP (even in melee range)")
            .AddOption(LightningShotStrategy.Ranged, "Ranged", "Use if outside melee range")
            .AddOption(LightningShotStrategy.Forbid, "Forbid", "Do not use at all")
            .AddAssociatedActions(GNB.AID.LightningShot);
        //GnashingFang strategy
        res.Define(Track.GnashingFang).As<GnashingStrategy>("Gnashing Fang", "GF", uiPriority: 50)
            .AddOption(GnashingStrategy.Automatic, "Auto", "Normal use of Gnashing Fang")
            .AddOption(GnashingStrategy.ForceGnash, "Force", "Force use of Gnashing Fang (Step 1)", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceClaw, "Force", "Force use of Savage Claw (Step 2)", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceTalon, "Force", "Force use of Wicked Talon (Step 3)", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.Delay, "Delay", "Delay use of Gnashing Fang", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(GNB.AID.GnashingFang, GNB.AID.SavageClaw, GNB.AID.WickedTalon);

        //Our OffensiveStrategy Actions
        //NoMercy strategy
        res.Define(Track.NoMercy).As<OffensiveStrategy>("No Mercy", "NM", uiPriority: 60)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP (even during downtime)", 60, 20, ActionTargets.Self, 2)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(GNB.AID.NoMercy);
        //SonicBreak strategy
        res.Define(Track.SonicBreak).As<OffensiveStrategy>("Sonic Break", "SB", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Sonic Break")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Sonic Break", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Sonic Break", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(GNB.AID.SonicBreak);
        //DoubleDown strategy
        res.Define(Track.DoubleDown).As<OffensiveStrategy>("Double Down", "DD", uiPriority: 50)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Double Down")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Double Down", 60, 0, ActionTargets.Hostile, 90)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Double Down", 0, 0, ActionTargets.None, 90)
            .AddAssociatedActions(GNB.AID.DoubleDown);
        //BurstStrike strategy
        res.Define(Track.BurstStrike).As<OffensiveStrategy>("Burst Strike", "BS", uiPriority: 30)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Burst Strike")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Burst Strike", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Burst Strike", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(GNB.AID.BurstStrike);
        //FatedCircle strategy
        res.Define(Track.FatedCircle).As<OffensiveStrategy>("Fated Circle", "FC", uiPriority: 30)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Fated Circle")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Fated Circle", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Fated Circle", 0, 0, ActionTargets.None, 72)
            .AddAssociatedActions(GNB.AID.FatedCircle);
        //Zone strategy
        res.Define(Track.Zone).As<OffensiveStrategy>("Blasting Zone", "Zone", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP", 30, 0, ActionTargets.Hostile, 18)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 18)
            .AddAssociatedActions(GNB.AID.BlastingZone, GNB.AID.DangerZone);
        //Bloodfest strategy
        res.Define(Track.Bloodfest).As<OffensiveStrategy>("Bloodfest", "BF", uiPriority: 60)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Bloodfest")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Bloodfest", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Bloodfest", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(GNB.AID.Bloodfest);
        //BowShock strategy
        res.Define(Track.BowShock).As<OffensiveStrategy>("Bow Shock", "BShock", uiPriority: 40)
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
        NormalSB = 670,
        NormalBS = 600,
        GF23 = 670,
        NormalDD = 680,
        GF1 = 690,
        NormalGCD = 700,
        ForcedLightningShot = 850,
        ForcedSonicBreak = 860,
        ForcedBurstStrike = 870,
        ForcedDoubleDown = 880,
        ForcedGnashing = 890,
        ForcedGCD = 900,
    }

    public enum OGCDPriority //Priority for oGCDs used
    {
        None = 0,
        Trajectory = 500,
        Continuation = 510,
        Zone = 540,
        BowShock = 550,
        Continuation1 = 580,
        Bloodfest = 600,
        NoMercy = 850,
        Potion = 900,
        ContiunuationNeed = 950
    }

    public byte Ammo; // Range: 0-2, 0-3 - current ammo count
    public byte GunComboStep; // 0 = Gnashing Fang & Reign of Beasts, 1 = Savage Claw, 2 = Wicked Talon, etc.
    public int MaxCartridges; // Maximum number of cartridges based on player level

    private float GCDLength; // Current GCD length, adjusted by skill speed/haste (2.5s baseline)
    private float bfCD; // Time left on Bloodfest cooldown (120s base)
    private float nmLeft; // Time left on No Mercy buff (20s base)
    private float nmCD; // Time left on No Mercy cooldown (60s base)
    private float ReignLeft; // Time left on Reign of Beasts buff (30s base)
    private float BlastLeft; // Time left on Blast buff (10s base)
    private float RazeLeft; // Time left on Raze buff (10s base)
    private float RipLeft; // Time left on Rip buff (10s base)
    private float TearLeft; // Time left on Tear buff (10s base)
    private float GougeLeft; // Time left on Eye Gouge buff (10s base)

    private float PotionLeft; // Time left on potion buff (typically 30s)
    private float RaidBuffsLeft; // Time left on raid-wide buffs (typically 20s-22s)
    private float RaidBuffsIn; // Time until raid-wide buffs are applied again (typically 20s-22s)

    public float BurstWindowLeft; // Time left in current burst window (typically 20s-22s)
    public float BurstWindowIn; // Time until next burst window (typically 20s-22s)

    private bool hasNM;
    private bool hasBreak;
    private bool hasReign;

    public GNB.AID NextGCD; // Next global cooldown action to be used (needed for cartridge management)
    private GCDPriority NextGCDPrio; // Priority of the next GCD, used for decision making on cooldowns

    private bool Unlocked(GNB.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Checks if desired ability is unlocked
    private bool Unlocked(GNB.TraitID tid) => TraitUnlocked((uint)tid); //Checks if desired trait is unlocked
    private float CD(GNB.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Gets cooldown time remaining
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline;
    private GNB.AID ComboLastMove => (GNB.AID)World.Client.ComboState.Action; //Gets our last action used
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.75;
    private bool ActionReady(GNB.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f; //Checks if desired action is ready
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Beginning of combat

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving) //Executes our actions
    {
        //GNBGauge ints
        var gauge = World.Client.GetGauge<GunbreakerGauge>(); //Our gauge
        Ammo = gauge.Ammo; //Our cartridges
        GunComboStep = gauge.AmmoComboStep; //Our combo step relating to GnashingFang or ReignOfBeasts
        MaxCartridges = Unlocked(GNB.TraitID.CartridgeChargeII) ? 3 : 2; //Checks if we are the proper level for how many max cartridges we have
        //Bools
        hasBreak = SelfStatusCheck(GNB.SID.ReadyToBreak); // Ready to use Reign of Beasts inside Bloodfest; //Has ReadyToBreak buff
        hasReign = SelfStatusCheck(GNB.SID.ReadyToReign); // Ready to use Reign of Beasts inside Bloodfest; //Has ReadyToReign buff
        hasNM = nmCD is >= 40 and <= 60;
        //Floats - '() > 0' assumes you have the buff, good for using as a "HasBuff" call
        bfCD = CD(GNB.AID.Bloodfest); //120s cooldown
        nmCD = CD(GNB.AID.NoMercy); //60s cooldown
        nmLeft = SelfStatusLeft(GNB.SID.NoMercy); //20s buff
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
        ReignLeft = SelfStatusLeft(GNB.SID.ReadyToReign); //30s buff
        BlastLeft = SelfStatusLeft(GNB.SID.ReadyToBlast); //Has ReadyToBlast buff
        RazeLeft = SelfStatusLeft(GNB.SID.ReadyToRaze); //Has ReadyToRaze buff
        RipLeft = SelfStatusLeft(GNB.SID.ReadyToRip); //Has ReadyToRip buff
        TearLeft = SelfStatusLeft(GNB.SID.ReadyToTear); //Has ReadyToTear buff
        GougeLeft = SelfStatusLeft(GNB.SID.ReadyToGouge); //Has ReadyToGouge buff
        PotionLeft = PotionStatusLeft(); //30s buff from Pot
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);
        NextGCD = GNB.AID.None;
        NextGCDPrio = GCDPriority.None;

        var AOEStrategy = strategy.Option(Track.AoE).As<AOEStrategy>();
        var AoETargets = AOEStrategy switch
        {
            AOEStrategy.SingleTarget => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AOEStrategy.ForceAoE => NumTargetsHitByAoE() > 0 ? 100 : 0,
            _ => NumTargetsHitByAoE()
        };

        //Burst (raid buff) windows are normally 20s every 120s (barring downtimes, deaths, etc)
        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        var hold = burstStrategy == BurstStrategy.ConserveCarts;
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => (RaidBuffsIn, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft)),
            BurstStrategy.UnderRaidBuffs => (RaidBuffsIn, RaidBuffsLeft),
            BurstStrategy.UnderPotion => (PotionCD, PotionLeft),
            _ => (0, 0)
        };

        //GCDs
        var canSonic = hasBreak && Unlocked(GNB.AID.SonicBreak); //SonicBreak conditions
        //var canReign = (ReadyToReign && Unlocked(GNB.AID.ReignOfBeasts)); //Reign conditions
        var canDD = Ammo >= 2 && Unlocked(GNB.AID.DoubleDown); //DoubleDown minimal conditions
        var canBSlv80 = Ammo >= 1 && Unlocked(GNB.AID.BurstStrike) && Unlocked(GNB.AID.Bloodfest); //BurstStrike minimal conditions
        var canBSlv70 = ((Ammo == MaxCartridges && ComboLastMove is GNB.AID.BrutalShell) || (nmLeft > 0 && Ammo > 0)) && Unlocked(GNB.AID.BurstStrike) && !Unlocked(GNB.AID.Bloodfest); //BurstStrike minimal conditions
        var canGF = Ammo >= 1 && Unlocked(GNB.AID.GnashingFang); //GnashingFang minimal conditions
        var canFC = Ammo >= 1 && Unlocked(GNB.AID.FatedCircle); //FatedCircle minimal conditions

        var (comboAction, comboPrio) = ComboActionPriority(AOEStrategy, AoETargets, burstStrategy, burst.Value.ExpireIn);
        QueueGCD(comboAction, comboAction is GNB.AID.DemonSlice or GNB.AID.DemonSlaughter ? Player : primaryTarget, comboPrio);

        //NoMercy usage
        if (!hold && ShouldUseNoMercy(strategy.Option(Track.NoMercy).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.NoMercy, Player, OGCDPriority.NoMercy);
        //Zone usage
        if (!hold && ShouldUseZone(strategy.Option(Track.Zone).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(Unlocked(GNB.AID.BlastingZone) ? GNB.AID.BlastingZone : GNB.AID.DangerZone, primaryTarget, OGCDPriority.Zone);
        //BowShock usage
        if (!hold && ShouldUseBowShock(strategy.Option(Track.BowShock).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.BowShock, primaryTarget, OGCDPriority.BowShock);
        //Bloodfest usage
        if (!hold && ShouldUseBloodfest(strategy.Option(Track.Bloodfest).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.Bloodfest, primaryTarget, OGCDPriority.Bloodfest);
        //Continuation usage
        if (Unlocked(GNB.AID.Continuation))
        {
            if (RipLeft > 0)
                QueueOGCD(GNB.AID.JugularRip, primaryTarget, OGCDPriority.ContiunuationNeed);
            if (TearLeft > 0)
                QueueOGCD(GNB.AID.AbdomenTear, primaryTarget, OGCDPriority.ContiunuationNeed);
            if (GougeLeft > 0)
                QueueOGCD(GNB.AID.EyeGouge, primaryTarget, OGCDPriority.ContiunuationNeed);
            if (BlastLeft > 0 || ComboLastMove is GNB.AID.BurstStrike)
                QueueOGCD(GNB.AID.Hypervelocity, primaryTarget, OGCDPriority.ContiunuationNeed);
            if (RazeLeft > 0 || ComboLastMove is GNB.AID.FatedCircle)
                QueueOGCD(GNB.AID.FatedBrand, primaryTarget, OGCDPriority.ContiunuationNeed);
        }
        //GnashingFang usage
        if (!hold && canGF && ShouldUseGnashingFang(strategy.Option(Track.GnashingFang).As<GnashingStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.GnashingFang, primaryTarget, GCDPriority.GF1);
        //DoubleDown usage
        if (!hold && canDD && ShouldUseDoubleDown(strategy.Option(Track.DoubleDown).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.DoubleDown, primaryTarget, GCDPriority.NormalDD);
        //Gnashing Combo usage 
        if (GunComboStep is 1)
            QueueGCD(GNB.AID.SavageClaw, primaryTarget, GCDPriority.GF23);
        if (GunComboStep is 2)
            QueueGCD(GNB.AID.WickedTalon, primaryTarget, GCDPriority.GF23);
        //ReignOfBeasts usage
        if (hasReign && GunComboStep is 0 && !ActionReady(GNB.AID.DoubleDown))
            QueueGCD(GNB.AID.ReignOfBeasts, primaryTarget, GCDPriority.NormalGCD);
        //Reign Combo usage
        if (GunComboStep is 3)
            QueueGCD(GNB.AID.NobleBlood, primaryTarget, GCDPriority.NormalGCD);
        if (GunComboStep is 4)
            QueueGCD(GNB.AID.LionHeart, primaryTarget, GCDPriority.NormalGCD);
        //SonicBreak usage
        if (canSonic && ShouldUseSonicBreak(strategy.Option(Track.SonicBreak).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.SonicBreak, primaryTarget, GCDPriority.NormalSB);
        //BurstStrike usage
        if (canBSlv80 && ShouldUseBurstStrike(strategy.Option(Track.BurstStrike).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.BurstStrike, primaryTarget, GCDPriority.NormalBS);
        //FatedCircle usage
        if (canFC && ShouldUseFatedCircle(strategy.Option(Track.FatedCircle).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.BurstStrike, primaryTarget, GCDPriority.NormalBS);
        if (!canFC && canBSlv70)
        {
            var action = UseCorrectBS(AoETargets);
            var prio = GCDPriority.NormalBS;
            QueueGCD(action, primaryTarget, prio);
        }
        //LightningShot usage
        if (ShouldUseLightningShot(primaryTarget, strategy.Option(Track.LightningShot).As<LightningShotStrategy>()))
            QueueGCD(GNB.AID.LightningShot, primaryTarget, GCDPriority.ForcedLightningShot);

        //Potion should be used as late as possible in ogcd window, so that if playing at <2.5 gcd, it can cover 13 gcds
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

    private int NumTargetsHitByAoE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //All our AoEs have the same shape
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f;
    private GNB.AID UseCorrectBS(int AoETargets) //BurstStrike & FatedCircle have special conditions when a certain level range due to fated Circle being unlocked at lv74
    {
        //under No Mercy, if FatedCircle is not unlocked yet, we want to use BurstStrike even in non-AoE situations
        if (Ammo == MaxCartridges && ComboLastMove is GNB.AID.BrutalShell)
            return AoETargets < 3 && Unlocked(GNB.AID.FatedCircle) ? GNB.AID.BurstStrike : GNB.AID.FatedCircle;

        //AoE spender is profitable in some situations:
        //Sub Lv90 AoE on 2 targets is optimal
        //Sub Lv73 no FatedCircle
        var hasStrike = Unlocked(GNB.AID.BurstStrike);
        var hasCircle = Unlocked(GNB.AID.FatedCircle);
        if (hasStrike)
        {
            var sub73 = hasStrike && !hasCircle;
            if (sub73 && AoETargets >= 2)
                return hasCircle ? GNB.AID.FatedCircle : GNB.AID.BurstStrike;
        }

        //ST cartridge spender
        return hasStrike ? GNB.AID.BurstStrike : GNB.AID.BurstStrike;
    }

    private GNB.AID NextComboSingleTarget() => ComboLastMove switch //how we use our ST 1-2-3
    {
        GNB.AID.BrutalShell => Ammo == MaxCartridges ? GNB.AID.BurstStrike : GNB.AID.SolidBarrel,
        GNB.AID.KeenEdge => GNB.AID.BrutalShell,
        _ => GNB.AID.KeenEdge,
    };

    private GNB.AID NextComboAoE() => ComboLastMove switch // how we use our AoE 1-2
    {
        GNB.AID.DemonSlice => Ammo == MaxCartridges
                              ? (Unlocked(GNB.AID.FatedCircle) ? GNB.AID.FatedCircle : GNB.AID.BurstStrike)
                              : GNB.AID.DemonSlaughter,
        _ => GNB.AID.DemonSlice,
    };

    private int AmmoGainedFromAction(GNB.AID action) => action switch //Our ammo gained from certain actions
    {
        GNB.AID.SolidBarrel => 1,
        GNB.AID.DemonSlaughter => 1,
        GNB.AID.Bloodfest => 3,
        _ => 0
    };

    private (GNB.AID, GCDPriority) ComboActionPriority(AOEStrategy aoeStrategy, int AoETargets, BurstStrategy burstStrategy, float burstStrategyExpire)
    {
        var comboStepsRemaining = ComboLastMove switch
        {
            GNB.AID.KeenEdge => Unlocked(GNB.AID.SolidBarrel) ? 2 : Unlocked(GNB.AID.BrutalShell) ? 1 : 0,
            GNB.AID.DemonSlice => Unlocked(GNB.AID.DemonSlaughter) ? 1 : 0,
            _ => 0
        };
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining))
            comboStepsRemaining = 0;
        var doingAOECombo = ComboLastMove == GNB.AID.DemonSlice;

        // AOE is profitable on 2 targets when under Lv90
        // Above Lv90 should be 3 targets
        var wantAOEAction = Unlocked(GNB.AID.DemonSlice) && aoeStrategy switch
        {
            AOEStrategy.SingleTarget => false,
            AOEStrategy.ForceAoE => true,
            AOEStrategy.Auto => AoETargets >= 3,
            AOEStrategy.AutoFinishCombo => comboStepsRemaining > 0 ? doingAOECombo : (Unlocked(GNB.AID.DoubleDown) ? AoETargets >= 3 : AoETargets >= 2),
            _ => false
        };
        if (comboStepsRemaining > 0 && wantAOEAction != doingAOECombo)
            comboStepsRemaining = 0;

        var nextAction = wantAOEAction ? NextComboAoE() : NextComboSingleTarget();
        var riskingAmmo = Ammo + AmmoGainedFromAction(nextAction) > 3;

        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining, 1))
            return (nextAction, GCDPriority.Combo123);

        //just a normal combo action; delay if overcapping gauge
        return (nextAction, riskingAmmo ? GCDPriority.NormalGCD : GCDPriority.Combo123);
    }

    // How we use RangedUptime
    private bool ShouldUseLightningShot(Actor? target, LightningShotStrategy strategy) => strategy switch
    {
        LightningShotStrategy.OpenerRanged => IsFirstGCD() && !In3y(target),
        LightningShotStrategy.Opener => IsFirstGCD(),
        LightningShotStrategy.Force => true,
        LightningShotStrategy.Ranged => !In3y(target),
        LightningShotStrategy.Forbid => false,
        _ => false
    };

    // How we use NoMercy
    // NOTE: using SkS GNB for now, so as soon as we have full carts we use NM
    private bool ShouldUseNoMercy(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            (Player.InCombat && target != null) && ActionReady(GNB.AID.NoMercy) && GCD < 0.9f &&
            (
                (Ammo == 1 && bfCD == 0 && Unlocked(GNB.AID.Bloodfest) && Unlocked(GNB.AID.DoubleDown)) || // Lv90+ Opener
                (Ammo >= 1 && bfCD == 0 && Unlocked(GNB.AID.Bloodfest) && !Unlocked(GNB.AID.DoubleDown)) || // Lv80+ Opener
                (!Unlocked(GNB.AID.Bloodfest) && Ammo >= 1 && ActionReady(GNB.AID.GnashingFang)) || // Lv70 & below
                (Ammo == MaxCartridges) // 60s & 120s burst windows
            ),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    // How we use Bloodfest
    private bool ShouldUseBloodfest(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(GNB.AID.Bloodfest) && Ammo == 0 && hasNM,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    // How we use Zone
    private bool ShouldUseZone(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && nmCD is < 57.55f and > 17 &&
            ActionReady(Unlocked(GNB.AID.BlastingZone) ? GNB.AID.BlastingZone : GNB.AID.DangerZone),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    // How we use BowShock
    private bool ShouldUseBowShock(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(GNB.AID.BowShock) && In5y(target) && nmCD is < 57.55f and > 17,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    // How we use SonicBreak
    private bool ShouldUseSonicBreak(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && nmLeft <= 2.45f,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    // How we use DoubleDown
    private bool ShouldUseDoubleDown(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(GNB.AID.DoubleDown) && In5y(target) && hasNM && Ammo >= 2,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    // How we use GnashingFang
    private bool ShouldUseGnashingFang(GnashingStrategy strategy, Actor? target)
    {
        return strategy switch
        {
            GnashingStrategy.Automatic => Player.InCombat && Ammo >= 1 && In3y(target) && ActionReady(GNB.AID.GnashingFang) &&
                (nmLeft > 0 || hasNM || nmCD is < 35 and > 17),
            GnashingStrategy.ForceGnash => Player.InCombat && GunComboStep is 0 && Ammo >= 1,
            GnashingStrategy.ForceClaw => Player.InCombat && GunComboStep is 1,
            GnashingStrategy.ForceTalon => Player.InCombat && GunComboStep is 2,
            GnashingStrategy.Delay => false,
            _ => false
        };
    }

    // How we use BurstStrike
    private bool ShouldUseBurstStrike(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) &&
            (
                (Unlocked(GNB.AID.DoubleDown) && hasNM && !ActionReady(GNB.AID.DoubleDown) && GunComboStep == 0 && ReignLeft == 0) || // Lv90+
                (!Unlocked(GNB.AID.DoubleDown) && !ActionReady(GNB.AID.GnashingFang) && hasNM && GunComboStep == 0) || // Lv80 & Below
                (ComboLastMove == GNB.AID.BrutalShell && Ammo == MaxCartridges) // Overcap
            ),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    // How we use FatedCircle
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

    // Pots usage
    private bool IsPotionAlignedWithNM()
    {
        // We use before SolidBarrel in opener
        // We use for 6m window
        return (Ammo == 1 && ActionReady(GNB.AID.GnashingFang) &&
                ActionReady(GNB.AID.DoubleDown) &&
                ActionReady(GNB.AID.Bloodfest) || // Opener
                (bfCD < 15 || ActionReady(GNB.AID.Bloodfest)) && Ammo == 3);
    }

    // Pots strategy
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
