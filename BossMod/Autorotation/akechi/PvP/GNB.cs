using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AID = BossMod.GNB.AID;
using SID = BossMod.GNB.SID;

namespace BossMod.Autorotation.akechi.PvP;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class GNB(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    //Actions tracked for Cooldown Planner execution
    public enum Track
    {
        Burst,           //Burst tracking
        Combo,           //Solid Barrel combo tracking
        LimitBreak,      //Limit Break tracking
        GnashingFang,    //Gnashing Fang action tracking
        FatedCircle,     //Fated Circle ability tracking
        RoughDivide,     //Rough Divide ability tracking
        Zone,            //Blasting Zone ability tracking
        Corundum,        //Heart of Corundum ability tracking
    }

    public enum BurstStrategy
    {
        Automatic,        //Automatically execute based on conditions
        Force,            //Force burst actions regardless of conditions
        Hold              //Conserve resources and cooldowns
    }

    public enum ComboStrategy
    {
        Automatic,
        Force,
        Hold
    }

    public enum LimitBreakStrategy
    {
        Automatic,
        Force,
        Hold
    }

    public enum ElixirStrategy
    {
        Automatic,
        Close,
        Mid,
        Far,
        Force,
        Hold
    }

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
        var res = new RotationModuleDefinition("GNB (PvP)", "PvP Rotation Module", "Standard rotation (Akechi)", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.GNB), 100);

        #region Custom strategies
        //Burst strategy
        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 190)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Use everything optimally")
            .AddOption(BurstStrategy.Force, "Force", "Force everything")
            .AddOption(BurstStrategy.Hold, "Hold", "Hold everything");
        //Combo strategy
        res.Define(Track.Combo).As<ComboStrategy>("Combo", uiPriority: 190)
            .AddOption(ComboStrategy.Automatic, "Automatic", "Use combo optimally")
            .AddOption(ComboStrategy.Force, "Force", "Force combo")
            .AddOption(ComboStrategy.Hold, "Hold", "Hold combo");
        //Limit Break strategy
        res.Define(Track.LimitBreak).As<LimitBreakStrategy>("Limit Break", uiPriority: 190)
            .AddOption(LimitBreakStrategy.Automatic, "Automatic", "Use Limit Break optimally")
            .AddOption(LimitBreakStrategy.Force, "Force", "Force Limit Break")
            .AddOption(LimitBreakStrategy.Hold, "Hold", "Hold Limit Break");
        #endregion

        #region Offensive Strategies
        res.Define(Track.GnashingFang).As<OffensiveStrategy>("Gnashing Fang", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.GnashingFangPvP);
        res.Define(Track.FatedCircle).As<OffensiveStrategy>("Fated Circle", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.FatedCirclePvP);
        res.Define(Track.RoughDivide).As<OffensiveStrategy>("Rough Divide", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.RoughDividePvP);
        res.Define(Track.Zone).As<OffensiveStrategy>("Zone", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.BlastingZonePvP);
        res.Define(Track.Corundum).As<OffensiveStrategy>("Corundum", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.HeartOfCorundumPvP);
        #endregion

        return res;

    }

    #region Priorities
    //Priority for GCDs used
    public enum GCDPriority
    {
        None = 0,
        KeenEdge = 325,    //1
        BrutalShell = 320, //2
        SolidBarrel = 315, //3
        BurstStrike = 340, //4
        Combo = 350,
        GnashingFang = 400,
        FatedCircle = 450,
        ForcedGCD = 900,
    }
    //Priority for oGCDs used
    public enum OGCDPriority
    {
        None = 0,
        RoughDivide = 400,
        Zone = 420,
        Corundum = 440,
        Continuation = 500,
        LB = 600,
        ForcedOGCD = 900,
    }
    #endregion

    #region Placeholders for Variables
    //Cooldown Related
    private float nmLeft; //Time left on No Mercy buff (20s base)
    private float rdCD; //Time left on No Mercy cooldown (60s base)
    private bool hasNM; //Checks self for No Mercy buff
    private bool hasBlast; //Checks self for Ready To Blast buff
    private bool hasRaze; //Checks self for Ready To Raze buff
    private bool hasRip; //Checks self for Ready To Rip buff
    private bool hasTear; //Checks self for Ready To Tear buff
    private bool hasGouge; //Checks self for Ready To Gouge buff
    private bool can1; //can Keen Edge
    private bool can2; //can Brutal Shell
    private bool can3; //can Solid Barrel
    private bool can4; //can Burst Strike
    private bool canGF; //can Gnashing Fang
    private bool canFC; //can Fated Circle
    private bool canZone; //can Blasting Zone
    private bool canHyper; //can Hypervelocity
    private bool canBrand; //can Fated Brand
    private bool canRip; //can Jugular Rip
    private bool canTear; //can Abdomen Tear
    private bool canGouge; //can Eye Gouge

    //Misc
    public bool LBready; //Checks if Limit Break is ready
    public float GFcomboStep; //Current Gnashing Fang combo step (0-3)
    public float comboStep; //Current combo step (0-4)
    public bool inCombo; //Checks if in combo
    public bool inGF; //Checks if in Gnashing Fang combo
    public float GCDLength; //Current GCD length, adjusted by skill speed/haste (2.5s baseline)
    public AID NextGCD; //Next global cooldown action to be used (needed for cartridge management)
    private GCDPriority NextGCDPrio; //Priority of the next GCD, used for decision making on cooldowns
    #endregion

    #region Module Helpers
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline; //Check if we can fit an additional GCD within the provided deadline
    private AID ComboLastMove => (AID)World.Client.ComboState.Action; //Get the last action used in the combo sequence
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 2.9; //Check if the target is within melee range (3 yalms)
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.9; //Check if the target is within 5 yalms
    private bool In20y(Actor? target) => Player.DistanceToHitbox(target) <= 19.9; //Check if the target is within 20 yalms
    private bool In30y(Actor? target) => Player.DistanceToHitbox(target) <= 29.9; //Check if the target is within 30 yalms
    private bool IsOffCooldown(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f; //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private int NumTargetsHitByAoE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //Returns the number of targets hit by AoE within a 5-yalm radius around the player
    public bool HasEffect(SID sid) => SelfStatusLeft(sid) > 0; //Check if the player has the specified status effect
    public bool TargetHasEffect(SID sid, Actor? target) => StatusDetails(target, sid, Player.InstanceID, 1000).Left > 0; //Check if the target has the specified status effect
    public AID LimitBreak => HasEffect(SID.RelentlessRushPvP) ? AID.TerminalTriggerPvP : AID.RelentlessRushPvP;
    #endregion

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        var gauge = World.Client.GetGauge<GunbreakerGauge>(); //Retrieve Gunbreaker gauge
        var GunStep = gauge.AmmoComboStep;

        rdCD = CD(AID.RoughDividePvP); //Rough Divide cooldown (14s)
        nmLeft = SelfStatusLeft(SID.NoMercyPvP, 7); //Remaining time for No Mercy buff (7s)
        hasNM = nmLeft > 0; //Checks if No Mercy is active
        hasBlast = HasEffect(SID.ReadyToBlastPvP); //Checks for Ready To Blast buff
        hasRaze = HasEffect(SID.ReadyToRazePvP); //Checks for Ready To Raze buff
        hasRip = HasEffect(SID.ReadyToRipPvP) || GunStep == 1; //Checks for Ready To Rip buff
        hasTear = HasEffect(SID.ReadyToTearPvP) || GunStep == 2; //Checks for Ready To Tear buff
        hasGouge = HasEffect(SID.ReadyToGougePvP); //Checks for Ready To Gouge buff
        LBready = World.Party.LimitBreakLevel >= 1; //Checks if Limit Break is ready
        //Misc
        GFcomboStep = ComboLastMove switch
        {
            AID.WickedTalonPvP => 3,
            AID.SavageClawPvP => 2,
            AID.GnashingFangPvP => 1,
            _ => 0
        };
        comboStep = ComboLastMove switch
        {
            AID.BurstStrikePvP => 0,
            AID.SolidBarrelPvP => 3,
            AID.BrutalShellPvP => 2,
            AID.KeenEdgePvP => 1,
            _ => 0
        };
        inCombo = comboStep > 0;
        inGF = GFcomboStep > 0;
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on skill speed and haste
        NextGCD = AID.None; //Next global cooldown action to be used
        NextGCDPrio = GCDPriority.None; //Priority of the next GCD, used for decision making on cooldowns

        #region Minimal Requirements
        can1 = ComboLastMove is AID.BurstStrikePvP; //KeenEdge conditions
        can2 = ComboLastMove is AID.KeenEdgePvP; //BrutalShell conditions
        can3 = ComboLastMove is AID.BrutalShellPvP; //SolidBarrel conditions
        can4 = ComboLastMove is AID.SolidBarrelPvP; //BurstStrike conditions
        canGF = IsOffCooldown(AID.GnashingFangPvP); //GnashingFang conditions
        canFC = IsOffCooldown(AID.GnashingFangPvP); //FatedCircle conditions
        canZone = IsOffCooldown(AID.BlastingZonePvP); //Zone conditions
        canHyper = hasBlast && In5y(primaryTarget); //Hypervelocity conditions
        canBrand = hasRaze && In5y(primaryTarget); //Fated Brand conditions
        canRip = hasRip && In5y(primaryTarget); //Jugular Rip conditions
        canTear = hasTear && In5y(primaryTarget); //Abdomen conditions
        canGouge = hasGouge && In5y(primaryTarget); //Eye Gouge conditions
        #endregion
        #endregion

        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        var hold = burstStrategy == BurstStrategy.Hold;

        if (strategy.Option(Track.Combo).As<ComboStrategy>() == ComboStrategy.Force) //ST (without overcap protection)
            QueueGCD(NextCombo(), primaryTarget, GCDPriority.ForcedGCD);

        #region Rotation Execution
        //Determine and queue combo actions
        if (!hold && !inGF)
            QueueGCD(NextCombo(), primaryTarget, GCDPriority.Combo);

        #region OGCDs
        var rdStrat = strategy.Option(Track.RoughDivide).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseRoughDivide(rdStrat, primaryTarget))
            QueueOGCD(AID.RoughDividePvP, primaryTarget, rdStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.RoughDivide);

        var zoneStrat = strategy.Option(Track.Zone).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseZone(zoneStrat, primaryTarget))
            QueueOGCD(AID.BlastingZonePvP, primaryTarget, zoneStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Zone);

        if (canRip || GunStep == 1)
            QueueOGCD(AID.JugularRipPvP, primaryTarget, OGCDPriority.Continuation);
        if (canTear || GunStep == 1)
            QueueOGCD(AID.AbdomenTearPvP, primaryTarget, OGCDPriority.Continuation);
        if (canGouge)
            QueueOGCD(AID.EyeGougePvP, primaryTarget, OGCDPriority.Continuation);
        if (canHyper)
            QueueOGCD(AID.HypervelocityPvP, primaryTarget, OGCDPriority.Continuation);
        if (canBrand)
            QueueOGCD(AID.FatedBrandPvP, primaryTarget, OGCDPriority.Continuation);
        #endregion

        #region GCDs
        var gfStrat = strategy.Option(Track.GnashingFang).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseGnashingFang(gfStrat, primaryTarget))
            QueueGCD(AID.GnashingFangPvP, primaryTarget, GCDPriority.GnashingFang);
        if (GunStep == 1 && In5y(primaryTarget))
            QueueGCD(AID.SavageClawPvP, primaryTarget, GCDPriority.GnashingFang);
        if (GunStep == 2 && In5y(primaryTarget))
            QueueGCD(AID.WickedTalonPvP, primaryTarget, GCDPriority.GnashingFang);

        var fcStrat = strategy.Option(Track.FatedCircle).As<OffensiveStrategy>();
        if (ShouldUseFatedCircle(fcStrat, primaryTarget))
            QueueGCD(AID.FatedCirclePvP, primaryTarget, fcStrat == OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.FatedCircle);
        #endregion

        #endregion

        //Limit Break execution
        var lbStrat = strategy.Option(Track.LimitBreak).As<LimitBreakStrategy>();
        if (ShouldUseLimitBreak(lbStrat, primaryTarget))
            QueueOGCD(LimitBreak, primaryTarget, lbStrat == LimitBreakStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.LB);
    }

    #region Core Execution Helpers
    //QueueGCD execution
    private void QueueGCD(AID aid, Actor? target, GCDPriority prio)
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
    private void QueueOGCD(AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Medium)
    {
        if (prio != OGCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
        }
    }

    #endregion

    #region Single-Target Helpers
    private AID NextCombo() => ComboLastMove switch //Determines the next single-target action based on the last action used
    {
        AID.SolidBarrelPvP => AID.BurstStrikePvP, //4, defaults back to 1
        AID.BrutalShellPvP => AID.SolidBarrelPvP, //3
        AID.KeenEdgePvP => AID.BrutalShellPvP, //2
        _ => AID.KeenEdgePvP, //1
    };
    #endregion

    #region Cooldown Helpers

    //Determines when to use No Mercy
    private bool ShouldUseRoughDivide(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            target != null &&
            !hasNM || rdCD >= 7 || IsOffCooldown(AID.RoughDividePvP),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Zone
    private bool ShouldUseZone(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            canZone &&
            hasNM &&
            In5y(target),
        OffensiveStrategy.Force => canZone,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Gnashing Fang
    private bool ShouldUseGnashingFang(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            In5y(target) &&
            hasNM &&
            canGF,
        OffensiveStrategy.Force => canGF,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Fated Circle
    private bool ShouldUseFatedCircle(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            In5y(target) &&
            hasNM &&
            canFC,
        OffensiveStrategy.Force => canFC,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    private bool ShouldUseLimitBreak(LimitBreakStrategy strategy, Actor? target) => strategy switch
    {
        LimitBreakStrategy.Automatic =>
            target != null &&
            In5y(target) &&
            hasNM &&
            LBready,
        LimitBreakStrategy.Force => true,
        LimitBreakStrategy.Hold => false,
        _ => false
    };
    #endregion
}
