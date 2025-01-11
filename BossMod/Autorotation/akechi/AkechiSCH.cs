using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using AID = BossMod.SCH.AID;
using SID = BossMod.SCH.SID;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiSCH(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track
    {
        AOE,                 //ST&AOE rotations tracking
        Bio,                 //Bio tracking
        Potion,              //Potion item tracking
        EnergyDrain,         //Energy Drain tracking
        ChainStratagem,      //Chain Stratagem tracking
        Aetherflow,          //Aetherflow tracking
    }
    public enum AOEStrategy
    {
        Auto,                //Automatically decide when to use ST or AOE abilities
        Ruin2,               //Force use of Ruin II only 
        Broil,               //Force use of Broil only
        ArtOfWar,            //Force use of Art of War only
    }
    public enum BioStrategy
    {
        Bio3,                //Force use of Bio if target has 3s or less remaining on DOT effect
        Bio6,                //Force use of Bio if target has 6s or less remaining on DOT effect
        Bio9,                //Force use of Bio if target has 9s or less remaining on DOT effect
        Bio0,                //Force use of Bio if target has does not have DOT effect
        Force,               //Force use of Bio regardless of DOT effect
        Delay                //Delay the use of Bio for manual or strategic usage
    }
    public enum PotionStrategy
    {
        Manual,              //Manual potion usage
        AlignWithRaidBuffs,  //Align potion usage with raid buffs
        Immediate            //Use potions immediately when available
    }
    public enum EnergyStrategy
    {
        Use3,                //Use all stacks of Aetherflow for Energy Drain
        Use2,                //Use 2 stacks of Aetherflow for Energy Drain
        Use1,                //Use 1 stack of Aetherflow for Energy Drain
        Force,               //Force the use of Energy Drain if any Aetherflow is available
        Delay                //Delay the use of Energy Drain
    }
    public enum OffensiveStrategy
    {
        Automatic,           //Automatically decide when to use off-global offensive abilities
        Force,               //Force the use of off-global offensive abilities, regardless of weaving conditions
        AnyWeave,            //Force the use of off-global offensive abilities in any next possible weave slot
        EarlyWeave,          //Force the use of off-global offensive abilities in very next FIRST weave slot only
        LateWeave,           //Force the use of off-global offensive abilities in very next LAST weave slot only
        Delay                //Delay the use of offensive abilities for strategic reasons
    }
    #endregion

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi SCH", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)", //Category
            "Akechi", //Contributor
            RotationModuleQuality.Ok, //Quality
            BitMask.Build((int)Class.SCH), //Job
            100); //Level supported

        #region Custom strategies
        res.Define(Track.AOE).As<AOEStrategy>("AOE", "AOE", uiPriority: 200)
            .AddOption(AOEStrategy.Auto, "Auto", "Automatically decide when to use ST or AOE abilities")
            .AddOption(AOEStrategy.Ruin2, "Ruin II", "Force use of Ruin II only (instant cast ST, less DPS)", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.Broil, "Broil", "Force use of Broil only (hardcast ST, more DPS)", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ArtOfWar, "Art of War", "Force use of Art of War only (instant cast AOE)", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(AID.Ruin2, AID.Broil1, AID.Broil2, AID.Broil3, AID.Broil4, AID.ArtOfWar1, AID.ArtOfWar2);
        res.Define(Track.Bio).As<BioStrategy>("Damage Over Time", "Bio", uiPriority: 190)
            .AddOption(BioStrategy.Bio3, "Bio3", "Use Bio if target has 3s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio6, "Bio6", "Use Bio if target has 6s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio9, "Bio9", "Use Bio if target has 9s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio0, "Bio0", "Use Bio if target does not have DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Force, "Force", "Force use of Bio regardless of DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Delay, "Delay", "Delay the use of Bio for manual or strategic usage", 0, 0, ActionTargets.Hostile, 2)
            .AddAssociatedActions(AID.Bio1, AID.Bio2, AID.Biolysis);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 180)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with No Mercy & Bloodfest together (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.EnergyDrain).As<EnergyStrategy>("Energy Drain", "E.Drain", uiPriority: 150)
            .AddOption(EnergyStrategy.Use3, "UseAll", "Uses all stacks of Aetherflow for Energy Drain; conserves no stacks for manual usage", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Use2, "Use2", "Uses 2 stacks of Aetherflow for Energy Drain; conserves 1 stack for manual usage", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Use1, "Use1", "Uses 1 stack of Aetherflow for Energy Drain; conserves 2 stacks for manual usage", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Force, "Force", "Force use of Energy Drain if any Aetherflow is available", 0, 0, ActionTargets.None, 45)
            .AddOption(EnergyStrategy.Delay, "Delay", "Delay use of Energy Drain", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.EnergyDrain);
        #endregion

        #region Offensive Strategies
        res.Define(Track.ChainStratagem).As<OffensiveStrategy>("Chain Stratagem", "Stratagem", uiPriority: 170)
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
        #endregion

        return res;
    }

    #region Priorities
    public enum GCDPriority //priorities for GCDs (higher number = higher priority)
    {
        None = 0,             //default
        Standard = 300,       //standard abilities
        DOT = 400,            //damage-over-time abilities
        ForcedGCD = 900,      //Forced GCDs
    }
    public enum OGCDPriority //priorities for oGCDs (higher number = higher priority)
    {
        None = 0,             //default
        EnergyDrain = 300,    //Energy Drain
        Aetherflow = 400,     //Aetherflow
        ChainStratagem = 500, //Chain Stratagem
        Potion = 800,         //Potion
        ForcedOGCD = 900,     //Forced oGCDs
    }
    #endregion

    #region Placeholders for Variables
    private (int Stacks, bool IsActive) Aetherflow; //Current Aetherflow stacks (max: 3)
    private bool canAF; //Checks if Aetherflow is completely available
    private bool canED; //Checks if Energy Drain is completely available
    private bool canCS; //Checks if Chain Stratagem is completely available
    private float bioLeft; //Time left on DOT effect (30s base)
    private float stratagemLeft; //Time left on Chain Stratagem (15s base)
    private bool ShouldUseAOE; //Checks if AOE should be used
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
    public AID NextGCD; //Next global cooldown action to be used
    #endregion

    #region Module Helpers
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private bool In25y(Actor? target) => Player.DistanceToHitbox(target) <= 24.99f; //Check if the target is within 25 yalms
    private bool ActionReady(AID aid) => Unlocked(aid) && CD(aid) < 0.6f; //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Check if this is the first GCD in combat
    private int TargetsInAOERange() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //Returns the number of targets hit by AOE within a 5-yalm radius around the player
    public bool PlayerHasEffect(SID sid, float duration) => SelfStatusLeft(sid, duration) > 0; //Checks if Status effect is on self
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
        var gauge = World.Client.GetGauge<ScholarGauge>(); //Retrieve Scholar gauge
        Aetherflow.Stacks = gauge.Aetherflow; //Current Aetherflow stacks
        Aetherflow.IsActive = Aetherflow.Stacks > 0; //Checks if Aetherflow is available
        bioLeft = StatusDetails(primaryTarget, BestDOT, Player.InstanceID).Left;
        stratagemLeft = StatusDetails(primaryTarget, SID.ChainStratagem, Player.InstanceID).Left;
        canCS = ActionReady(AID.ChainStratagem); //Chain Stratagem is available
        canED = Unlocked(AID.EnergyDrain) && Aetherflow.IsActive; //Energy Drain is available
        canAF = ActionReady(AID.Aetherflow) && !Aetherflow.IsActive; //Aetherflow is available
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        SpS = ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on spell speed and haste
        NextGCD = AID.None; //Next global cooldown action to be used
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)
        ShouldUseAOE = TargetsInAOERange() > 1; //otherwise, use AOE if 2+ targets would be hit

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var Bio = strategy.Option(Track.Bio); //Bio track
        var BioStrategy = Bio.As<BioStrategy>(); //Bio strategy
        var potion = strategy.Option(Track.Potion).As<PotionStrategy>(); //Potion strategy
        var cs = strategy.Option(Track.ChainStratagem); //Chain Stratagem track
        var csStrat = cs.As<OffensiveStrategy>(); //Chain Stratagem strategy
        var af = strategy.Option(Track.Aetherflow); //Aetherflow track
        var afStrat = af.As<OffensiveStrategy>(); //Aetherflow strategy
        var ed = strategy.Option(Track.EnergyDrain); //Energy Drain track
        var edStrat = ed.As<EnergyStrategy>(); //Energy Drain strategy
        #endregion

        #endregion

        #region Force Execution
        if (AOEStrategy is AOEStrategy.Ruin2)
            QueueGCD(BestRuin, ResolveTargetOverride(AOE.Value) ?? primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy is AOEStrategy.Broil)
            QueueGCD(BestBroil, ResolveTargetOverride(AOE.Value) ?? primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy is AOEStrategy.ArtOfWar)
            QueueGCD(BestAOE, Player, GCDPriority.ForcedGCD);
        #endregion

        #region Standard Execution
        if (AOEStrategy == AOEStrategy.Auto)
        {
            var STtarget = ResolveTargetOverride(AOE.Value) ?? primaryTarget;
            if (ShouldUseAOE)
                QueueGCD(BestAOE, Player, GCDPriority.Standard);
            if (In25y(STtarget) &&
                (!ShouldUseAOE || IsFirstGCD()))
                QueueGCD(isMoving ? BestRuin : BestST, STtarget, GCDPriority.Standard);
        }
        if (ShouldUseBio(primaryTarget, BioStrategy))
            QueueGCD(BestBio, ResolveTargetOverride(Bio.Value) ?? primaryTarget, GCDPriority.DOT);
        if (PlayerHasEffect(SID.ImpactImminent, 30))
            QueueOGCD(AID.BanefulImpaction, ResolveTargetOverride(Bio.Value) ?? primaryTarget, OGCDPriority.ChainStratagem);
        if (ShouldUseChainStratagem(primaryTarget, csStrat))
            QueueOGCD(AID.ChainStratagem, ResolveTargetOverride(cs.Value) ?? primaryTarget, csStrat is OffensiveStrategy.Force or OffensiveStrategy.AnyWeave or OffensiveStrategy.EarlyWeave or OffensiveStrategy.LateWeave ? OGCDPriority.ForcedOGCD : OGCDPriority.ChainStratagem);
        if (ShouldUseAetherflow(primaryTarget, afStrat))
            QueueOGCD(AID.Aetherflow, Player, afStrat is OffensiveStrategy.Force or OffensiveStrategy.AnyWeave or OffensiveStrategy.EarlyWeave or OffensiveStrategy.LateWeave ? OGCDPriority.ForcedOGCD : OGCDPriority.Aetherflow);
        if (ShouldUseEnergyDrain(primaryTarget, edStrat))
            QueueOGCD(AID.EnergyDrain, ResolveTargetOverride(ed.Value) ?? primaryTarget, edStrat is EnergyStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.EnergyDrain);
        if (Player.HPMP.CurMP <= 9000 && canWeaveIn && ActionReady(AID.LucidDreaming))
            QueueOGCD(AID.LucidDreaming, Player, OGCDPriority.EnergyDrain);
        if (potion is PotionStrategy.AlignWithRaidBuffs && CD(AID.ChainStratagem) < 5 ||
            potion is PotionStrategy.Immediate)
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
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

        if (QueueAction(aid, target, ActionQueue.Priority.High + priority, delay) && priority > NextGCDPrio)
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
    private bool ShouldUseBio(Actor? target, BioStrategy strategy) => strategy switch
    {
        BioStrategy.Bio3 => Player.InCombat && target != null && bioLeft <= 3 && In25y(target),
        BioStrategy.Bio6 => Player.InCombat && target != null && bioLeft <= 6 && In25y(target),
        BioStrategy.Bio9 => Player.InCombat && target != null && bioLeft <= 9 && In25y(target),
        BioStrategy.Bio0 => Player.InCombat && target != null && bioLeft is 0 && In25y(target),
        BioStrategy.Force => true,
        BioStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseChainStratagem(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && target != null && canCS && canWeaveIn && stratagemLeft == 0 && In25y(target),
        OffensiveStrategy.Force => canCS,
        OffensiveStrategy.AnyWeave => canCS && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canCS && canWeaveEarly,
        OffensiveStrategy.LateWeave => canCS && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseAetherflow(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && target != null && canAF && canWeaveIn,
        OffensiveStrategy.Force => canAF,
        OffensiveStrategy.AnyWeave => canAF && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canAF && canWeaveEarly,
        OffensiveStrategy.LateWeave => canAF && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseEnergyDrain(Actor? target, EnergyStrategy strategy) => strategy switch
    {
        EnergyStrategy.Use3 => canED && In25y(target) && canWeaveIn,
        EnergyStrategy.Use2 => canED && Aetherflow.Stacks > 1 && In25y(target) && canWeaveIn,
        EnergyStrategy.Use1 => canED && Aetherflow.Stacks > 2 && In25y(target) && canWeaveIn,
        EnergyStrategy.Force => canED,
        EnergyStrategy.Delay => false,
        _ => false
    };
    #endregion
}
