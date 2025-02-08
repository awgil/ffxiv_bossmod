using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.SCH;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiSCH(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
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

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi SCH", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)|Healer", //Category
            "Akechi", //Contributor
            RotationModuleQuality.Ok, //Quality
            BitMask.Build((int)Class.SCH), //Job
            100); //Level supported

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

        return res;
    }
    #endregion

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

    #region Upgrade Paths
    private AID BestBroil => Unlocked(AID.Broil4) ? AID.Broil4 : Unlocked(AID.Broil3) ? AID.Broil3 : Unlocked(AID.Broil2) ? AID.Broil2 : AID.Broil1;
    private AID BestRuin => Unlocked(AID.Ruin2) ? AID.Ruin2 : AID.Ruin1;
    private AID BestBio => Unlocked(AID.Biolysis) ? AID.Biolysis : Unlocked(AID.Bio2) ? AID.Bio2 : AID.Bio1;
    private SID BestDOT => Unlocked(AID.Biolysis) ? SID.Biolysis : Unlocked(AID.Bio2) ? SID.Bio2 : SID.Bio1;
    private AID BestST => Unlocked(AID.Broil1) ? BestBroil : BestRuin;
    private AID BestAOE => Unlocked(AID.ArtOfWar2) ? AID.ArtOfWar2 : AID.ArtOfWar1;
    #endregion

    #region Module Variables
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
    public float BurstWindowLeft; //Time left in current burst window (typically 20s-22s)
    public float BurstWindowIn; //Time until next burst window (typically 20s-22s)
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<ScholarGauge>(); //Retrieve Scholar gauge
        Aetherflow.Stacks = gauge.Aetherflow; //Current Aetherflow stacks
        Aetherflow.IsActive = Aetherflow.Stacks > 0; //Checks if Aetherflow is available
        bioLeft = StatusDetails(primaryTarget?.Actor, BestDOT, Player.InstanceID).Left;
        stratagemLeft = StatusDetails(primaryTarget?.Actor, SID.ChainStratagem, Player.InstanceID).Left;
        canCS = ActionReady(AID.ChainStratagem); //Chain Stratagem is available
        canED = Unlocked(AID.EnergyDrain) && Aetherflow.IsActive; //Energy Drain is available
        canAF = ActionReady(AID.Aetherflow) && !Aetherflow.IsActive; //Aetherflow is available
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        NextGCD = AID.None; //Next global cooldown action to be used
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)
        ShouldUseAOE = ShouldUseAOECircle(5).OnTwoOrMore; //otherwise, use AOE if 2+ targets would be hit

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

        #region Full Rotation Execution

        #region Standard Rotation
        if (AOEStrategy == AOEStrategy.Auto)
        {
            if (ShouldUseAOE)
                QueueGCD(BestAOE, Player, GCDPriority.Standard);
            if (In25y(TargetChoice(AOE) ?? primaryTarget?.Actor) &&
                (!ShouldUseAOE || IsFirstGCD()))
                QueueGCD(IsMoving ? BestRuin : BestST, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Standard);
        }
        if (AOEStrategy is AOEStrategy.Ruin2)
            QueueGCD(BestRuin, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ForcedGCD);
        if (AOEStrategy is AOEStrategy.Broil)
            QueueGCD(BestBroil, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ForcedGCD);
        if (AOEStrategy is AOEStrategy.ArtOfWar)
            QueueGCD(BestAOE, Player, GCDPriority.ForcedGCD);
        if (ShouldUseBio(primaryTarget?.Actor, BioStrategy))
            QueueGCD(BestBio, TargetChoice(Bio) ?? primaryTarget?.Actor, GCDPriority.DOT);
        #endregion

        #region Cooldowns
        if (PlayerHasEffect(SID.ImpactImminent, 30))
            QueueOGCD(AID.BanefulImpaction, TargetChoice(Bio) ?? primaryTarget?.Actor, OGCDPriority.ChainStratagem);
        if (ShouldUseChainStratagem(primaryTarget?.Actor, csStrat))
            QueueOGCD(AID.ChainStratagem,
                TargetChoice(cs) ?? primaryTarget?.Actor,
                csStrat is OffensiveStrategy.Force or OffensiveStrategy.AnyWeave or OffensiveStrategy.EarlyWeave or OffensiveStrategy.LateWeave
                ? OGCDPriority.ForcedOGCD : OGCDPriority.ChainStratagem);
        if (ShouldUseAetherflow(primaryTarget?.Actor, afStrat))
            QueueOGCD(AID.Aetherflow,
                Player,
                afStrat is OffensiveStrategy.Force or OffensiveStrategy.AnyWeave or OffensiveStrategy.EarlyWeave or OffensiveStrategy.LateWeave
                ? OGCDPriority.ForcedOGCD : OGCDPriority.Aetherflow);
        if (ShouldUseEnergyDrain(primaryTarget?.Actor, edStrat))
            QueueOGCD(AID.EnergyDrain,
                TargetChoice(ed) ?? primaryTarget?.Actor,
                edStrat is EnergyStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.EnergyDrain);
        if (Player.HPMP.CurMP <= 9000 && canWeaveIn && ActionReady(AID.LucidDreaming))
            QueueOGCD(AID.LucidDreaming, Player, OGCDPriority.EnergyDrain);
        if (potion is PotionStrategy.AlignWithRaidBuffs && TotalCD(AID.ChainStratagem) < 5 ||
            potion is PotionStrategy.Immediate)
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        GoalZoneSingle(25);
        #endregion
    }

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
