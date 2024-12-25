using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using AID = BossMod.SCH.AID;
using SID = BossMod.SCH.SID;
using TraitID = BossMod.SCH.TraitID;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiSCH(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    //Abilities tracked for Cooldown Planner & Autorotation execution
    public enum Track
    {
        AOE,             //ST&AOE rotations tracking
        DOT,            //DOT abilities tracking
        Potion,          //Potion item tracking
        ChainStratagem,  //Chain Stratagem tracking
        Aetherflow,      //Aetherflow tracking
        EnergyDrain,     //Energy Drain tracking
    }

    //Defines the strategy for using ST/AOE actions based on the current target selection and conditions
    public enum AOEStrategy
    {
        Auto,               //Automatically decide when to use ST or AOE abilities
        Ruin2,              //Force use of Ruin II only 
        Broil,              //Force use of Broil only
        ArtOfWar,           //Force use of Art of War only
    }

    //Defines different strategies for executing burst damage actions based on cooldown and resource availability
    public enum DOTStrategy
    {
        Auto,               //Automatically decide when to use damage-over-time abilities
        Bio,                //Force use of Bio (ST, 30s duration)
        Bio2,               //Force use of Bio II (ST, 30s duration)
        Biolysis,           //Force use of Biolysis (ST, 30s duration)
        BioOpti,            //Force use of Bio (ST, 30s duration) if target does not have DOT effect
        Bio2Opti,           //Force use of Bio II (ST, 30s duration) if target does not have DOT effect
        BiolysisOpti,       //Force use of Biolysis (ST, 30s duration) if target does not have DOT effect
        BanefulImpaction,   //Force use of Baneful Impaction (AOE, 15s duration)
        Forbid,             //Forbids the use of all abilities with a cooldown
    }

    //Defines strategies for potion usage in combat, determining when and how to consume potions based on the situation
    public enum PotionStrategy
    {
        Manual,                //Manual potion usage
        AlignWithRaidBuffs,    //Align potion usage with raid buffs
        Immediate              //Use potions immediately when available
    }

    //Defines different offensive strategies that dictate how abilities and resources are used during combat
    public enum OffensiveStrategy
    {
        Automatic,       //Automatically decide when to use off-global offensive abilities
        Force,           //Force the use of off-global offensive abilities, regardless of weaving conditions
        AnyWeave,        //Force the use of off-global offensive abilities in any next possible weave slot
        EarlyWeave,      //Force the use of off-global offensive abilities in very next FIRST weave slot only
        LateWeave,       //Force the use of off-global offensive abilities in very next LAST weave slot only
        Delay            //Delay the use of offensive abilities for strategic reasons
    }
    #endregion

    //Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi SCH", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)", //Category
            "Akechi", //Contributor
            RotationModuleQuality.WIP, //Quality
            BitMask.Build((int)Class.SCH), //Job
            100); //Level supported

        #region Custom strategies
        res.Define(Track.AOE).As<AOEStrategy>("AOE", "AOE", uiPriority: 200)
            .AddOption(AOEStrategy.Auto, "Auto", "Automatically decide when to use ST or AOE abilities")
            .AddOption(AOEStrategy.Ruin2, "Ruin II", "Force use of Ruin II only (instant cast ST, less DPS)", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.Broil, "Broil", "Force use of Broil only (hardcast ST, more DPS)", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ArtOfWar, "Art of War", "Force use of Art of War only (instant cast AOE)", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(AID.Ruin2, AID.Broil1, AID.Broil2, AID.Broil3, AID.Broil4, AID.ArtOfWar1, AID.ArtOfWar2);
        res.Define(Track.DOT).As<DOTStrategy>("Damage Over Time", "DOTs", uiPriority: 210)
            .AddOption(DOTStrategy.Auto, "Allow", "Automatically decide when to use DoT abilities")
            .AddOption(DOTStrategy.Bio, "Bio", "Force use of Bio (ST, 30s duration)", 0, 30, ActionTargets.Hostile, 2, 26)
            .AddOption(DOTStrategy.Bio2, "Bio II", "Force use of Bio II (ST, 30s duration)", 0, 30, ActionTargets.Hostile, 26, 72)
            .AddOption(DOTStrategy.Biolysis, "Biolysis", "Force use of Biolysis (ST, 30s duration)", 0, 30, ActionTargets.Hostile, 72)
            .AddOption(DOTStrategy.BioOpti, "Bio", "Force use of Bio (ST, 30s duration) if target does not have DOT effect", 0, 30, ActionTargets.Hostile, 2, 26)
            .AddOption(DOTStrategy.Bio2Opti, "Bio II", "Force use of Bio II (ST, 30s duration) if target does not have DOT effect", 0, 30, ActionTargets.Hostile, 26, 72)
            .AddOption(DOTStrategy.BiolysisOpti, "Biolysis", "Force use of Biolysis (ST, 30s duration) if target does not have DOT effect", 0, 30, ActionTargets.Hostile, 72)
            .AddOption(DOTStrategy.BanefulImpaction, "Baneful Impaction", "Force use of Baneful Impaction (AOE, 15s duration)", 0, 15, ActionTargets.Self, 92)
            .AddOption(DOTStrategy.Forbid, "Forbid", "Forbid the use of all DoT abilities", 0, 0, ActionTargets.None)
            .AddAssociatedActions(AID.Bio1, AID.Bio2, AID.Biolysis, AID.BanefulImpaction);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 20)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with No Mercy & Bloodfest together (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        #endregion

        #region Offensive Strategies
        res.Define(Track.ChainStratagem).As<OffensiveStrategy>("Chain Stratagem", "C.Stratagem", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Chain Stratagem")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Chain Stratagem", 120, 20, ActionTargets.Hostile, 66)
            .AddOption(OffensiveStrategy.AnyWeave, "Any Weave", "Force use of Chain Stratagem in any next possible weave slot", 120, 20, ActionTargets.Hostile, 66)
            .AddOption(OffensiveStrategy.EarlyWeave, "Early Weave", "Force use of Chain Stratagem in very next FIRST weave slot only", 120, 20, ActionTargets.Hostile, 66)
            .AddOption(OffensiveStrategy.LateWeave, "Late Weave", "Force use of Chain Stratagem in very next LAST weave slot only", 120, 20, ActionTargets.Hostile, 66)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Chain Stratagem", 0, 0, ActionTargets.None, 66)
            .AddAssociatedActions(AID.ChainStratagem);
        res.Define(Track.Aetherflow).As<OffensiveStrategy>("Aetherflow", "A.flow", uiPriority: 160)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Aetherflow")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Aetherflow", 60, 10, ActionTargets.Self, 45)
            .AddOption(OffensiveStrategy.AnyWeave, "Any Weave", "Force use of Aetherflow in any next possible weave slot", 60, 10, ActionTargets.Self, 45)
            .AddOption(OffensiveStrategy.EarlyWeave, "Early Weave", "Force use of Aetherflow in very next FIRST weave slot only", 60, 10, ActionTargets.Self, 45)
            .AddOption(OffensiveStrategy.LateWeave, "Late Weave", "Force use of Aetherflow in very next LAST weave slot only", 60, 10, ActionTargets.Self, 45)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Aetherflow", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.Aetherflow);
        res.Define(Track.EnergyDrain).As<OffensiveStrategy>("Energy Drain", "E.Drain", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Normal use of Energy Drain")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Energy Drain", 60, 10, ActionTargets.Hostile, 45)
            .AddOption(OffensiveStrategy.AnyWeave, "Any Weave", "Force use of Energy Drain in any next possible weave slot", 60, 10, ActionTargets.Hostile, 45)
            .AddOption(OffensiveStrategy.EarlyWeave, "Early Weave", "Force use of Energy Drain in very next FIRST weave slot only", 60, 10, ActionTargets.Hostile, 45)
            .AddOption(OffensiveStrategy.LateWeave, "Late Weave", "Force use of Energy Drain in very next LAST weave slot only", 60, 10, ActionTargets.Hostile, 45)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Energy Drain", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.EnergyDrain);
        #endregion

        return res;
    }

    #region Priorities
    public enum GCDPriority //priorities for GCDs (higher number = higher priority)
    {
        None = 0,           //default
        Standard = 350,     //combo actions
        ForcedGCD = 900,    //Forced GCDs
    }
    public enum OGCDPriority //priorities for oGCDs (higher number = higher priority)
    {
        None = 0,           //default
        Potion = 900,       //Potion
        ForcedOGCD = 900,   //Forced oGCDs
    }
    #endregion

    #region Placeholders for Variables
    //Cooldown Related
    private bool canAF; //Checks if Aetherflow is completely available
    private bool canED; //Checks if Energy Drain is completely available
    private bool canCS; //Checks if Chain Stratagem is completely available
    private float stratagemLeft; //Time left on Chain Stratagem (15s base)
    private bool canZone; //Checks if Danger / Blasting Zone is completely available
    private bool ShouldUseAOE; //Checks if AOE rotation should be used
    //Misc
    public bool canWeaveIn; //Can weave in oGCDs
    public bool canWeaveEarly; //Can early weave oGCDs
    public bool canWeaveLate; //Can late weave oGCDs
    public bool quarterWeave; //Can last second weave oGCDs
    public float PotionLeft; //Time left on potion buff (30s base)
    public float RaidBuffsLeft; //Time left on raid-wide buffs (typically 20s-22s)
    public float RaidBuffsIn; //Time until raid-wide buffs are applied again (typically 20s-22s)
    public float SpS; //Current GCD length, adjusted by spell speed/haste (2.5s baseline)
    public float BurstWindowLeft; //Time left in current burst window (typically 20s-22s)
    public float BurstWindowIn; //Time until next burst window (typically 20s-22s)
    public AID NextGCD; //Next global cooldown action to be used (needed for cartridge management)
    private GCDPriority NextGCDPrio; //Priority of the next GCD, used for decision making on cooldowns
    #endregion

    #region Module Helpers
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    private bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid); //Check if the desired trait is unlocked
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private AID ComboLastMove => (AID)World.Client.ComboState.Action; //Get the last action used in the combo sequence
    private bool In20y(Actor? target) => Player.DistanceToHitbox(target) <= 19.9f; //Check if the target is within ST range
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.99f; //Check if the target is within AOE range
    private bool ActionReady(AID aid) => Unlocked(aid) && CD(aid) < 0.6f; //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Check if this is the first GCD in combat
    private int TargetsInAOERange() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //Returns the number of targets hit by AOE within a 5-yalm radius around the player
    public bool PlayerHasEffect(SID sid, float duration) => SelfStatusLeft(sid, duration) > 0; //Checks if Status effect is on self

    //TODO: try new things...
    //public bool JustDid(AID aid) => Manager?.LastCast.Data?.IsSpell(aid) ?? false; //Check if the last action used was the desired ability
    //public bool DidWithin(float variance) => (World.CurrentTime - Manager.LastCast.Time).TotalSeconds <= variance; //Check if the last action was used within a certain timeframe
    //public bool JustUsed(AID aid, float variance) => JustDid(aid) && DidWithin(variance); //Check if the last action used was the desired ability & was used within a certain timeframe
    #endregion

    #region Upgrade Paths
    private AID BestBroil //Determine the best Broil to use
        => Unlocked(AID.Broil4) //If Broil IV is unlocked
        ? AID.Broil4 //Use Broil IV
        : Unlocked(AID.Broil3) //Otherwise, if Broil III is unlocked
        ? AID.Broil3 //Use Broil III
        : Unlocked(AID.Broil2) //Otherwise, if Broil II is unlocked
        ? AID.Broil2 //Use Broil II
        : AID.Broil1; //Otherwise, default to Broil I

    private AID BestRuin //Determine the best Ruin to use
        => Unlocked(AID.Ruin2) //Otherwise, if Ruin II is unlocked
        ? AID.Ruin2 //Use Ruin II
        : AID.Ruin1; //Otherwise, default to Ruin I

    private AID BestBio //Determine the best DOT to use
        => Unlocked(AID.Biolysis) //If Biolysis is unlocked
        ? AID.Biolysis //Use Biolysis
        : Unlocked(AID.Bio2) //Otherwise, if Bio II is unlocked
        ? AID.Bio2 //Use Bio II
        : AID.Bio1; //Otherwise, default to Bio I

    private SID BestDOT //Determine the best DOT to use
        => Unlocked(AID.Biolysis) //If Biolysis is unlocked
        ? SID.Biolysis //Use Biolysis
        : Unlocked(AID.Bio2) //Otherwise, if Bio II is unlocked
        ? SID.Bio2 //Use Bio II
        : SID.Bio1; //Otherwise, default to Bio I

    private AID BestST //Determine the best ST to use
        => Unlocked(AID.Broil1) //If Broil I is unlocked
        ? BestBroil //Use the best Broil
        : BestRuin; //Otherwise, default to best Ruin

    private AID BestAOE //Determine the best AOE to use
        => Unlocked(AID.ArtOfWar2) //If Art of War II is unlocked
        ? AID.ArtOfWar2 //Use Art of War II
        : AID.ArtOfWar1; //Otherwise, default to Art of War


    #endregion

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        //Gauge
        var gauge = World.Client.GetGauge<ScholarGauge>(); //Retrieve Scholar gauge
        var seraphLeft = gauge.SeraphTimer; //Current cartridges
        var seraphUp = seraphLeft > 0; //Checks if Seraph is active
        var FairyGauge = gauge.FairyGauge; //Current Fairy Gauge (max: 100)
        var aetherflowStacks = gauge.Aetherflow; //Current Aetherflow stacks (max: 3)
        var hasAetherflow = aetherflowStacks > 0; //Checks if Aetherflow is available
        var hasFairy = gauge.DismissedFairy == 0; //Checks if Fairy is present
        var bioLeft = StatusDetails(primaryTarget, BestDOT, Player.InstanceID).Left;
        stratagemLeft = StatusDetails(primaryTarget, SID.ChainStratagem, Player.InstanceID).Left;
        canCS = ActionReady(AID.ChainStratagem); //ChainStratagem is available
        canED = Unlocked(AID.EnergyDrain) && hasAetherflow; //Energy Drain is available
        canAF = ActionReady(AID.Aetherflow) && !hasAetherflow; //Aetherflow is available
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        SpS = ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on skill speed and haste
        NextGCD = AID.None; //Next global cooldown action to be used
        NextGCDPrio = GCDPriority.None; //Priority of the next GCD, used for decision making on cooldowns
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)
        ShouldUseAOE = TargetsInAOERange() > 1; //otherwise, use AOE if 2+ targets would be hit
        var downtimeIn = Manager.Planner?.EstimateTimeToNextDowntime().Item2 ?? float.MaxValue; //Time until next downtime

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var DOT = strategy.Option(Track.DOT); //DOT track
        var DOTStrategy = DOT.As<DOTStrategy>(); //DOT strategy
        var potion = strategy.Option(Track.Potion).As<PotionStrategy>(); //Potion strategy
        var chainStrategy = strategy.Option(Track.ChainStratagem).As<OffensiveStrategy>(); //Chain Stratagem strategy
        var aetherflow = strategy.Option(Track.Aetherflow).As<OffensiveStrategy>(); //Aetherflow strategy
        var energyDrain = strategy.Option(Track.EnergyDrain).As<OffensiveStrategy>(); //Energy Drain strategy
        #endregion

        #endregion

        #region Force Execution
        if (AOEStrategy is AOEStrategy.Ruin2)
            QueueGCD(BestRuin, ResolveTargetOverride(AOE.Value) ?? primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy is AOEStrategy.Broil)
            QueueGCD(BestBroil, ResolveTargetOverride(AOE.Value) ?? primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy is AOEStrategy.ArtOfWar)
            QueueGCD(BestAOE, Player, GCDPriority.ForcedGCD);
        if (DOTStrategy is DOTStrategy.Bio)
            QueueGCD(AID.Bio1, ResolveTargetOverride(DOT.Value) ?? primaryTarget, GCDPriority.ForcedGCD);
        if (DOTStrategy is DOTStrategy.Bio2)
            QueueGCD(AID.Bio2, ResolveTargetOverride(DOT.Value) ?? primaryTarget, GCDPriority.ForcedGCD);
        if (DOTStrategy is DOTStrategy.Biolysis)
            QueueGCD(AID.Biolysis, ResolveTargetOverride(DOT.Value) ?? primaryTarget, GCDPriority.ForcedGCD);
        if (DOTStrategy is DOTStrategy.BanefulImpaction && PlayerHasEffect(SID.ImpactImminent, 30))
            QueueGCD(AID.BanefulImpaction, Player, GCDPriority.ForcedGCD);
        #endregion

        #region Standard Execution
        if (AOEStrategy == AOEStrategy.Auto)
        {
            if (ShouldUseAOE)
                QueueGCD(BestAOE, Player, GCDPriority.Standard);
            if (!ShouldUseAOE)
                QueueGCD(BestST, ResolveTargetOverride(AOE.Value) ?? primaryTarget, GCDPriority.Standard);
        }
        if (DOTStrategy == DOTStrategy.Auto)
        {
            if (bioLeft <= 3)
                QueueGCD(BestBio, ResolveTargetOverride(DOT.Value) ?? primaryTarget, GCDPriority.Standard);
            if (PlayerHasEffect(SID.ImpactImminent, 30))
                QueueGCD(AID.BanefulImpaction, Player, GCDPriority.ForcedGCD);
        }
        if (ShouldUseChainStratagem(primaryTarget, chainStrategy))
            QueueGCD(AID.ChainStratagem, primaryTarget, GCDPriority.Standard);
        if (ShouldUseAetherflow(primaryTarget, aetherflow))
            QueueGCD(AID.Aetherflow, Player, GCDPriority.Standard);
        if (ShouldUseEnergyDrain(primaryTarget, energyDrain))
            QueueGCD(AID.EnergyDrain, ResolveTargetOverride(strategy.Option(Track.EnergyDrain).Value) ?? primaryTarget, GCDPriority.Standard);
        #endregion
    }

    #region Core Execution Helpers

    public void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueGCD(aid, target, (int)(object)priority, delay);

    public void QueueGCD(AID aid, Actor? target, int priority = 8, float delay = 0)
    {
        var NextGCDPrio = 0;

        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.High, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
        }
    }

    public void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueOGCD(aid, target, (int)(object)priority, delay);

    public void QueueOGCD(AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }

    public bool QueueAction(AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return false;

        if (def.Range != 0 && target == null)
        {
            return false;
        }

        Vector3 targetPos = default;

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


    #region Cooldown Helpers
    private bool ShouldUseChainStratagem(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && target != null && canCS && stratagemLeft == 0,
        OffensiveStrategy.Force => canCS,
        OffensiveStrategy.AnyWeave => canCS && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canCS && canWeaveEarly,
        OffensiveStrategy.LateWeave => canCS && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    private bool ShouldUseAetherflow(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && target != null && canAF,
        OffensiveStrategy.Force => canAF,
        OffensiveStrategy.AnyWeave => canAF && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canAF && canWeaveEarly,
        OffensiveStrategy.LateWeave => canAF && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    private bool ShouldUseEnergyDrain(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && target != null && canED,
        OffensiveStrategy.Force => canED,
        OffensiveStrategy.AnyWeave => canED && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canED && canWeaveEarly,
        OffensiveStrategy.LateWeave => canED && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    #endregion
}
