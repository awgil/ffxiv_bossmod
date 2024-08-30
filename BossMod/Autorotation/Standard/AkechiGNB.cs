using BossMod.Autorotation.Legacy;
using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.ActorState;
using static BossMod.ClientState;

namespace BossMod.Autorotation;
//Contribution by Akechi, with help provided by Veyn for framework & Xan for assistance. Discord @akechdz or Akechi on Puni.sh for maintenance

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { AoE, Burst, Potion, LightningShot, Cartridge, NoMercy, SonicBreak, GnashingFang, DoubleDown, BurstStrike, FatedCircle, Zone, Bloodfest, BowShock, Trajectory } //What we're tracking
    public enum AoEStrategy { SingleTarget, ForceAoE, Auto } //Targeting strategy
    public enum BurstStrategy { Automatic, SpendCarts, ConserveCarts, UnderRaidBuffs, UnderPotion } //Burst strategy
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate } //Pots strategy
    public enum LightningShotStrategy { OpenerRanged, Opener, Force, Ranged } //RangedUptime strategy
    public enum CartridgeStrategy { Automatic, Delay, ForceDD, ForceGF, ForceBS, ForceFC, ForceAll } //Cartridge usage strategy
    public enum OffensiveStrategy { Automatic, Force, Delay } //CDs strategies
    public enum TrajectoryStrategy { Automatic, Forbid, Force, GapClose } //GapCloser strategy

    public static RotationModuleDefinition Definition()
    {
        //Our module title & signature
        var res = new RotationModuleDefinition("GNB (Akechi)", "Standard rotation module", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.GNB), 100);
        //Targeting strategy
        res.Define(Track.AoE).As<AoEStrategy>("AoE", uiPriority: 90)
            .AddOption(AoEStrategy.SingleTarget, "ST", "Use ST rotation")
            .AddOption(AoEStrategy.ForceAoE, "AoE", "Use AoE rotation")
            .AddOption(AoEStrategy.Auto, "Auto", "Use AoE rotation if 3+ targets would be hit, otherwise use ST rotation; break combo if necessary");
        //Burst Strategy
        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 80)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Spend Carts under potion/raidbuffs, otherwise conserve")
            .AddOption(BurstStrategy.SpendCarts, "SpendCarts", "Spend Carts freely (as if inside burst window)")
            .AddOption(BurstStrategy.ConserveCarts, "ConserveCarts", "Conserve Carts as much as possible (as if outside burst window)")
            .AddOption(BurstStrategy.UnderRaidBuffs, "UnderRaidBuffs", "Spend Carts under raidbuffs, otherwise conserve; ignore potion (useful if potion is delayed)")
            .AddOption(BurstStrategy.UnderPotion, "UnderPotion", "Spend Carts gauge under potion, otherwise conserve; ignore raidbuffs (useful for misaligned potions)");
        //Pots strategy
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 70)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with 2-minute raid buffs (0/6, 2/8, etc)")
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, even if without ST and with IR on cd (0/4:30/9)")
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        //RangedUptime strategy
        res.Define(Track.LightningShot).As<LightningShotStrategy>("LShot", uiPriority: 10)
            .AddOption(LightningShotStrategy.OpenerRanged, "OpenerRanged", "Use as very first GCD and only if outside melee range")
            .AddOption(LightningShotStrategy.Opener, "Opener", "Use as very first GCD regardless of range")
            .AddOption(LightningShotStrategy.Force, "Force", "Force use ASAP (even in melee range)")
            .AddOption(LightningShotStrategy.Ranged, "Ranged", "Use if outside melee range")
            .AddAssociatedActions(GNB.AID.LightningShot);
        //Cartridge usage strategy
        res.Define(Track.Cartridge).As<CartridgeStrategy>("Carts", uiPriority: 50)
            .AddOption(CartridgeStrategy.Automatic, "Automatic", "Use optimally")
            .AddOption(CartridgeStrategy.Delay, "Delay", "Delay")
            .AddOption(CartridgeStrategy.ForceDD, "Force Double Down", "Force use Double Down ASAP (even without No Mercy)")
            .AddOption(CartridgeStrategy.ForceGF, "Force Gnashing Fang", "Force use Gnashing Fang")
            .AddOption(CartridgeStrategy.ForceBS, "Force Burst Strike", "Force use Burst Strike")
            .AddOption(CartridgeStrategy.ForceFC, "Force Fated Circle", "Force use Fated Circle")
            .AddOption(CartridgeStrategy.ForceAll, "Force All", "Force use ASAP (even without No Mercy)")
            .AddAssociatedActions(GNB.AID.DoubleDown, GNB.AID.GnashingFang, GNB.AID.BurstStrike, GNB.AID.FatedCircle);
        //NoMercy strategy
        res.Define(Track.NoMercy).As<OffensiveStrategy>("NM", uiPriority: 60)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP (even during downtime)")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddAssociatedActions(GNB.AID.NoMercy);
        //SonicBreak strategy
        res.Define(Track.SonicBreak).As<OffensiveStrategy>("SB", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Sonic Break")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Sonic Break")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Sonic Break")
            .AddAssociatedActions(GNB.AID.SonicBreak);
        //GnashingFang strategy
        res.Define(Track.GnashingFang).As<OffensiveStrategy>("GF", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Gnashing Fang")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Gnashing Fang")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Gnashing Fang")
            .AddAssociatedActions(GNB.AID.GnashingFang, GNB.AID.SavageClaw, GNB.AID.WickedTalon);
        //DoubleDown strategy
        res.Define(Track.DoubleDown).As<OffensiveStrategy>("DD", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Double Down")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Double Down")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Double Down")
            .AddAssociatedActions(GNB.AID.DoubleDown);
        //BurstStrike strategy
        res.Define(Track.BurstStrike).As<OffensiveStrategy>("BS", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Burst Strike")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Burst Strike")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Burst Strike")
            .AddAssociatedActions(GNB.AID.BurstStrike);
        //FatedCircle strategy
        res.Define(Track.FatedCircle).As<OffensiveStrategy>("FC", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Fated Circle")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Fated Circle")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Fated Circle")
            .AddAssociatedActions(GNB.AID.FatedCircle);
        //Zone strategy
        res.Define(Track.Zone).As<OffensiveStrategy>("Zone", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddAssociatedActions(GNB.AID.BlastingZone, GNB.AID.DangerZone);
        //Bloodfest strategy
        res.Define(Track.Bloodfest).As<OffensiveStrategy>("BF", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Bloodfest")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Bloodfest")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Bloodfest")
            .AddAssociatedActions(GNB.AID.Bloodfest);
        //BowShock strategy
        res.Define(Track.BowShock).As<OffensiveStrategy>("BShock", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Bow Shock")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Bow Shock")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Bow Shock")
            .AddAssociatedActions(GNB.AID.BowShock);
        //Trajectory strategy
        res.Define(Track.Trajectory).As<TrajectoryStrategy>("Dash", uiPriority: 20)
            .AddOption(TrajectoryStrategy.Automatic, "Automatic", "No use")
            .AddOption(TrajectoryStrategy.Forbid, "Forbid", "No use")
            .AddOption(TrajectoryStrategy.Force, "Force", "Use ASAP")
            .AddOption(TrajectoryStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range")
            .AddAssociatedActions(GNB.AID.Trajectory);

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
        GapcloseTrajectory = 980, //note that it uses 'very high' prio
    }

    private int Ammo; //0-2, 0-3
    private int GunComboStep; //0=GnashingFang & ReignOfBeasts, 1=SavageClaw, 2=WickedTalon, 3=NobleBlood, 4=LionHeart
    private int MaxCartridges; //We have MaxCarts for desired level
    private float GCDLength; //2.5s adjusted by sks/haste
    private float BloodfestCD; //120s cooldown
    private float NoMercyLeft; //20s buff
    private float NoMercyCD; //60s cooldown
    public float BreakLeft; //30s buff - we usually want to use this either asap or as late as possible inside No Mercy, so we're going for late usage here in vbm
    private float ReignLeft; //30s buff, but we usually tend to use this the same way every time
    private float BlastLeft; //10s
    private float RazeLeft; //10s
    private float RipLeft; //10s
    private float TearLeft; //10s
    private float GougeLeft; //10s
    private float TrajectoryCD; //30s dash
    private float PotionLeft; //30s buff from Pots
    private float RaidBuffsLeft; //Typically always 20s-22s
    private float RaidBuffsIn; //Typically always 20s-22s
    public float BurstWindowLeft; //Typically always 20s-22s
    public float BurstWindowIn; //Typically always 20s-22s
    private bool ReadyToBreak; //0s if not up, 30s if NoMercy just used
    private bool ReadyToReign; //0s if not up, 30s if Bloodfest just used
    public GNB.AID NextGCD; //This is needed to estimate carts and make a decision on burst
    private GCDPriority NextGCDPrio; //This is needed to estimate priority and make a decision on CDs

    private const float TrajectoryMinGCD = 0.8f; //Triple-weaving Trajectory is not a good idea, since it might delay gcd for longer than normal anim lock

    private bool Unlocked(GNB.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Checks if desired ability is unlocked
    private bool Unlocked(GNB.TraitID tid) => TraitUnlocked((uint)tid); //Checks if desired trait is unlocked
    private float CD(GNB.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Gets cooldown time remaining
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline;
    private GNB.AID ComboLastMove => (GNB.AID)World.Client.ComboState.Action; //Gets our last action used
    private bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range
    private bool ActionReady(GNB.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f; //Checks if desired action is ready
    //public GNB.AID BestContinuation => ReadyToRip ? GNB.AID.JugularRip : ReadyToTear ? GNB.AID.AbdomenTear : ReadyToGouge ? GNB.AID.EyeGouge : ReadyToBlast ? GNB.AID.Hypervelocity : GNB.AID.Continuation; //Gets best action for our Continuation procs

    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Beginning of combat

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving) //Executes our actions
    {
        //GNBGauge ints
        var gauge = GetGauge<GunbreakerGauge>(); //Our gauge
        Ammo = gauge.Ammo; //Our cartridges
        GunComboStep = gauge.AmmoComboStep; //Our combo step relating to GnashingFang or ReignOfBeasts
        MaxCartridges = Unlocked(GNB.TraitID.CartridgeChargeII) ? 3 : 2; //Checks if we are the proper level for how many max cartridges we have
        //Bools
        ReadyToBreak = SelfStatusLeft(GNB.SID.ReadyToBreak) > 0; //Has ReadyToBreak buff
        ReadyToReign = SelfStatusLeft(GNB.SID.ReadyToReign) > 0; //Has ReadyToReign buff
        //Floats - '() > 0' assumes you have the buff, good for using as a "HasBuff" call
        BloodfestCD = CD(GNB.AID.Bloodfest); //120s cooldown
        NoMercyCD = CD(GNB.AID.NoMercy); //60s cooldown
        TrajectoryCD = CD(GNB.AID.Trajectory); //Dash
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
        NoMercyLeft = SelfStatusLeft(GNB.SID.NoMercy); //20s buff
        BreakLeft = SelfStatusLeft(GNB.SID.ReadyToBreak); //30s buff
        ReignLeft = SelfStatusLeft(GNB.SID.ReadyToReign); //30s buff
        BlastLeft = SelfStatusLeft(GNB.SID.ReadyToBlast); //Has ReadyToBlast buff
        RazeLeft = SelfStatusLeft(GNB.SID.ReadyToRaze); //Has ReadyToRaze buff
        RipLeft = SelfStatusLeft(GNB.SID.ReadyToRip); //Has ReadyToRip buff
        TearLeft = SelfStatusLeft(GNB.SID.ReadyToTear); //Has ReadyToTear buff
        GougeLeft = SelfStatusLeft(GNB.SID.ReadyToGouge); //Has ReadyToGouge buff
        BreakLeft = SelfStatusLeft(GNB.SID.ReadyToBreak); //Has ReadyToBreak buff
        PotionLeft = PotionStatusLeft(); //30s buff from Pot
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);
        NextGCD = GNB.AID.None;
        NextGCDPrio = GCDPriority.None;

        var AoEStrategy = strategy.Option(Track.AoE).As<AoEStrategy>();
        var AoETargets = AoEStrategy switch
        {
            AoEStrategy.SingleTarget => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AoEStrategy.ForceAoE => NumTargetsHitByAoE() > 0 ? 100 : 0,
            _ => NumTargetsHitByAoE()
        };

        //Burst (raid buff) windows are normally 20s every 120s (barring downtimes, deaths, etc)
        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => (RaidBuffsIn, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft)),
            BurstStrategy.SpendCarts => (0, float.MaxValue),
            BurstStrategy.ConserveCarts => (0, 0), //'in' is 0, meaning 'raid buffs are imminent, but not yet active, so delay everything'
            BurstStrategy.UnderRaidBuffs => (RaidBuffsIn, RaidBuffsLeft),
            BurstStrategy.UnderPotion => (PotionCD, PotionLeft),
            _ => (0, 0)
        };

        //GCDs
        var canSonic = (ReadyToBreak && Unlocked(GNB.AID.SonicBreak)); //SonicBreak conditions
        //var canReign = (ReadyToReign && Unlocked(GNB.AID.ReignOfBeasts)); //Reign conditions
        var canDD = Ammo >= 2 && Unlocked(GNB.AID.DoubleDown); //DoubleDown minimal conditions
        var canBSlv80 = Ammo >= 1 && Unlocked(GNB.AID.BurstStrike) && Unlocked(GNB.AID.Bloodfest); //BurstStrike minimal conditions
        var canBSlv70 = ((Ammo == MaxCartridges && ComboLastMove is GNB.AID.BrutalShell) || (NoMercyLeft > 0 && Ammo > 0)) && Unlocked(GNB.AID.BurstStrike) && !Unlocked(GNB.AID.Bloodfest); //BurstStrike minimal conditions
        var canGF = Ammo >= 1 && Unlocked(GNB.AID.GnashingFang); //GnashingFang minimal conditions
        var canFC = Ammo >= 1 && Unlocked(GNB.AID.FatedCircle); //FatedCircle minimal conditions

        var (comboAction, comboPrio) = ComboActionPriority(AoEStrategy, AoETargets, burstStrategy, burst.Value.ExpireIn);
        QueueGCD(comboAction, comboAction is GNB.AID.DemonSlice or GNB.AID.DemonSlaughter ? Player : primaryTarget, comboPrio);

        //NoMercy usage
        if (ShouldUseNoMercy(strategy.Option(Track.NoMercy).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.NoMercy, Player, OGCDPriority.NoMercy);
        //Zone usage
        if (ShouldUseZone(strategy.Option(Track.Zone).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.BlastingZone, primaryTarget, OGCDPriority.Zone);
        //BowShock usage
        if (ShouldUseBowShock(strategy.Option(Track.BowShock).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.BowShock, primaryTarget, OGCDPriority.BowShock);
        //Bloodfest usage
        if (ShouldUseBloodfest(strategy.Option(Track.Bloodfest).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(GNB.AID.Bloodfest, primaryTarget, OGCDPriority.Bloodfest);
        //Continuation usage
        if (Unlocked(GNB.AID.Continuation))
        {
            if (RipLeft > 0)
                QueueOGCD(GNB.AID.JugularRip, primaryTarget, OGCDPriority.GapcloseTrajectory);
            if (TearLeft > 0)
                QueueOGCD(GNB.AID.AbdomenTear, primaryTarget, OGCDPriority.GapcloseTrajectory);
            if (GougeLeft > 0)
                QueueOGCD(GNB.AID.EyeGouge, primaryTarget, OGCDPriority.GapcloseTrajectory);
            if (BlastLeft > 0 || ComboLastMove is GNB.AID.BurstStrike)
                QueueOGCD(GNB.AID.Hypervelocity, primaryTarget, OGCDPriority.GapcloseTrajectory);
            if (RazeLeft > 0 || ComboLastMove is GNB.AID.FatedCircle)
                QueueOGCD(GNB.AID.FatedBrand, primaryTarget, OGCDPriority.GapcloseTrajectory);
        }
        //GnashingFang usage
        if (canGF && ShouldUseGnashingFang(strategy.Option(Track.GnashingFang).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.GnashingFang, primaryTarget, GCDPriority.GF1);
        //DoubleDown usage
        if (canDD && ShouldUseDoubleDown(strategy.Option(Track.DoubleDown).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(GNB.AID.DoubleDown, primaryTarget, GCDPriority.NormalDD);
        //Gnashing Combo usage 
        if (GunComboStep is 1)
            QueueGCD(GNB.AID.SavageClaw, primaryTarget, GCDPriority.GF23);
        if (GunComboStep is 2)
            QueueGCD(GNB.AID.WickedTalon, primaryTarget, GCDPriority.GF23);
        //ReignOfBeasts usage
        if (ReadyToReign && GunComboStep is 0 && !ActionReady(GNB.AID.DoubleDown))
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
        //Trajectory usage
        if (Unlocked(GNB.AID.Trajectory))
        {
            var onsStrategy = strategy.Option(Track.Trajectory).As<TrajectoryStrategy>();
            if (ShouldUseTrajectory(onsStrategy, primaryTarget))
            {
                //Special case for use as gapcloser - it has to be very high priority
                var (prio, basePrio) = onsStrategy == TrajectoryStrategy.GapClose
                    ? (OGCDPriority.GapcloseTrajectory, ActionQueue.Priority.High)
                    : (OGCDPriority.Trajectory, TrajectoryCD < GCDLength ? ActionQueue.Priority.VeryLow : ActionQueue.Priority.Low);
                QueueOGCD(GNB.AID.Trajectory, primaryTarget, prio, basePrio);
            }
        }

        //Potion should be used as late as possible in ogcd window, so that if playing at <2.5 gcd, it can cover 13 gcds
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.Low + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
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
        //Sub Lv80 AoE on 2 targets is optimal
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

    private GNB.AID NextComboAoE() => ComboLastMove switch //how we use our AoE 1-2
    {
        GNB.AID.DemonSlice => Ammo == MaxCartridges ? GNB.AID.FatedCircle : GNB.AID.DemonSlaughter,
        _ => GNB.AID.DemonSlice,
    };

    private int AmmoGainedFromAction(GNB.AID action) => action switch //Our ammo gained from certain actions
    {
        GNB.AID.SolidBarrel => 1,
        GNB.AID.DemonSlaughter => 1,
        GNB.AID.Bloodfest => 3,
        _ => 0
    };

    private (GNB.AID, GCDPriority) ComboActionPriority(AoEStrategy AoEStrategy, int AoETargets, BurstStrategy burstStrategy, float burstStrategyExpire)
    {
        var comboStepsRemaining = ComboLastMove switch
        {
            GNB.AID.KeenEdge => Unlocked(GNB.AID.SolidBarrel) ? 2 : Unlocked(GNB.AID.BrutalShell) ? 1 : 0,
            GNB.AID.DemonSlice => Unlocked(GNB.AID.DemonSlaughter) ? 1 : 0,
            _ => 0
        };
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining))
            comboStepsRemaining = 0;
        var doingAoECombo = ComboLastMove == GNB.AID.DemonSlice;

        //AoE combo is profitable at 3+ targets (TODO: this is different at low levels!)
        var wantAoEAction = comboStepsRemaining > 0 ? doingAoECombo : Unlocked(GNB.AID.DemonSlice) && AoETargets >= 3;
        if (comboStepsRemaining > 0 && wantAoEAction != doingAoECombo)
            comboStepsRemaining = 0;

        var nextAction = wantAoEAction ? NextComboAoE() : NextComboSingleTarget();
        var riskingAmmo = Ammo + AmmoGainedFromAction(nextAction) > 3;

        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining, 1))
            return (nextAction, GCDPriority.Combo123);

        //just a normal combo action; delay if overcapping gauge
        return (nextAction, riskingAmmo ? GCDPriority.NormalSB : GCDPriority.Combo123);
    }

    //How we use RangedUptime
    private bool ShouldUseLightningShot(Actor? target, LightningShotStrategy strategy) => strategy switch
    {
        LightningShotStrategy.OpenerRanged => IsFirstGCD() && !InMeleeRange(target),
        LightningShotStrategy.Opener => IsFirstGCD(),
        LightningShotStrategy.Force => true,
        LightningShotStrategy.Ranged => !InMeleeRange(target),
        _ => false
    };

    private bool ShouldUseNoMercy(OffensiveStrategy strategy, Actor? player) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && ActionReady(GNB.AID.NoMercy) && GCD < 0.9f &&
        ((Ammo is 1 && BloodfestCD is 0 && Unlocked(GNB.AID.Bloodfest) && Unlocked(GNB.AID.DoubleDown)) || //Lv90+ Opener
        (Ammo >= 1 && BloodfestCD is 0 && Unlocked(GNB.AID.Bloodfest) && !Unlocked(GNB.AID.DoubleDown)) || //Lv80+ Opener
        (!Unlocked(GNB.AID.Bloodfest) && Ammo == MaxCartridges) || //Lv70 & below
        (Ammo == MaxCartridges)), //60s & 120s burst windows
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //How we use NoMercy 
    //NOTE: using SkS for this, so as soon as we have full carts we use NM
    /*
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
            bool SBready = NoMercyCD >= 40 && Unlocked(GNB.AID.SonicBreak) && ReadyToBreak;
            bool DDready = CD(GNB.AID.DoubleDown) < GCD && Unlocked(GNB.AID.DoubleDown);
            bool sublv30NM = Player.InCombat && !Unlocked(GNB.AID.BurstStrike);
            bool fastBurstReady = Player.InCombat && Unlocked(GNB.AID.Bloodfest) && (Ammo is 1 || Ammo == MaxCartridges) && GCD < 0.8 && GFready;
            /* bool slowBurstReady = Player.InCombat && Unlocked(GNB.AID.Bloodfest) && (Ammo == 0 || Ammo == MaxCartridges) && GCD < 0.8;

            bool slowNM = //(!gnbConfig.fastNM &&
                (GFready || SBready || DDready)
                && Player.InCombat
                && ((Ammo == MaxCartridges)
                || (Ammo == MaxCartridges - 1 && ComboLastMove == GNB.AID.BrutalShell && CD(GNB.AID.Bloodfest) > 20)
                || (CD(GNB.AID.Bloodfest) < 15 && Ammo == 1 && Unlocked(GNB.AID.Bloodfest)))) || shouldUseEarlyNoMercy; 

            bool fastNM = //(gnbConfig.fastNM &&
                GCD < 0.8f &&
                (GFready || SBready || DDready)
                && Player.InCombat
                && fastBurstReady;
            return slowNM || fastNM || sublv30NM;
        }
    }*/

    //How we use Bloodfest
    private bool ShouldUseBloodfest(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && Ammo == 0 && NoMercyCD > 40,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //How we use Zone
    private bool ShouldUseZone(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && InMeleeRange(target) && (NoMercyCD is <= 57.55f and > 17),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //How we use BowShock
    private bool ShouldUseBowShock(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && (NoMercyCD is <= 57.55f and > 40) && InMeleeRange(target),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //How we use our gapcloser
    private bool ShouldUseTrajectory(TrajectoryStrategy strategy, Actor? target) => strategy switch
    {
        TrajectoryStrategy.Automatic => false,
        TrajectoryStrategy.Forbid => false,
        TrajectoryStrategy.Force => GCD >= TrajectoryMinGCD,
        TrajectoryStrategy.GapClose => !InMeleeRange(target),
        _ => false,
    };

    //How we use SonicBreak
    private bool ShouldUseSonicBreak(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && InMeleeRange(target) && NoMercyLeft <= 2.45f,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //How we use DoubleDown
    private bool ShouldUseDoubleDown(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && ActionReady(GNB.AID.DoubleDown) && NoMercyCD > 40 && InMeleeRange(target) && Ammo >= 2,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //How we use GnashingFang
    private bool ShouldUseGnashingFang(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && Ammo >= 1 && (NoMercyLeft > 0 || NoMercyCD is >= 40 and <= 60 || NoMercyCD is < 35 and > 17) && InMeleeRange(target),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //How we use BurstStrike
    private bool ShouldUseBurstStrike(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && InMeleeRange(target) &&
        ((Unlocked(GNB.AID.DoubleDown) && NoMercyCD > 40 && !ActionReady(GNB.AID.DoubleDown) && GunComboStep is 0 && ReignLeft is 0) || //Lv90+
        (!Unlocked(GNB.AID.DoubleDown) && !ActionReady(GNB.AID.GnashingFang) && NoMercyLeft > 0 && GunComboStep is 0) || // Lv80 & Below use
        (ComboLastMove is GNB.AID.BrutalShell && Ammo == MaxCartridges)), //Overcap
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //How we use FatedCircle
    private bool ShouldUseFatedCircle(OffensiveStrategy strategy, Actor? AoETargets) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && ((NoMercyCD > 40 && !ActionReady(GNB.AID.DoubleDown) && Ammo > 0) || (ComboLastMove is GNB.AID.DemonSlice && Ammo == MaxCartridges)) && InMeleeRange(AoETargets),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Pots usage
    private bool IsPotionAlignedWithNM()
    {
        //We use before SolidBarrel in opener
        //We use for 6m window
        if ((Ammo is 1 && ActionReady(GNB.AID.GnashingFang) && ActionReady(GNB.AID.DoubleDown) && ActionReady(GNB.AID.Bloodfest)) || //Opener
            ((BloodfestCD < 15 || ActionReady(GNB.AID.Bloodfest)) && Ammo == 3))
            return true;

        //not aligned
        return false;
    }

    //Pots strategy
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => IsPotionAlignedWithNM() && (RaidBuffsLeft > 0 || RaidBuffsIn < 30),
        PotionStrategy.Immediate => true,
        _ => false
    };
}
