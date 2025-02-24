using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.SCH;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiSCH(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { AOE, Bio, Potion, EnergyDrain, ChainStratagem, Aetherflow }
    public enum AOEStrategy { Auto, Ruin2, Broil, ArtOfWar }
    public enum BioStrategy { Bio3, Bio6, Bio9, Bio0, Force, Delay }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    public enum EnergyStrategy { Use3, Use2, Use1, Force, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi SCH", "Standard Rotation Module", "Standard rotation (Akechi)|Healer", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);

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
            .AddOption(EnergyStrategy.Use2, "Use2", "Uses 2 stacks of Aetherflow for Energy Drain; conserves 1 stack for manual usage, but consumes any remaining when Aetherflow ability is about to be up", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Use1, "Use1", "Uses 1 stack of Aetherflow for Energy Drain; conserves 2 stacks for manual usage, but consumes any remaining when Aetherflow ability is about to be up", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Force, "Force", "Force use of Energy Drain if any Aetherflow is available", 0, 0, ActionTargets.None, 45)
            .AddOption(EnergyStrategy.Delay, "Delay", "Delay use of Energy Drain", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.EnergyDrain);
        res.Define(Track.ChainStratagem).As<OGCDStrategy>("Chain Stratagem", "Stratagem", uiPriority: 170)
            .AddOption(OGCDStrategy.Automatic, "Auto", "Normal use of Chain Stratagem")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Chain Stratagem", 120, 20, ActionTargets.Hostile, 66)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Chain Stratagem in any next possible weave slot", 120, 20, ActionTargets.Hostile, 66)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Chain Stratagem in very next FIRST weave slot only", 120, 20, ActionTargets.Hostile, 66)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Chain Stratagem in very next LAST weave slot only", 120, 20, ActionTargets.Hostile, 66)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Chain Stratagem", 0, 0, ActionTargets.None, 66)
            .AddAssociatedActions(AID.ChainStratagem);
        res.Define(Track.Aetherflow).As<OGCDStrategy>("Aetherflow", "A.flow", uiPriority: 160)
            .AddOption(OGCDStrategy.Automatic, "Auto", "Normal use of Aetherflow")
            .AddOption(OGCDStrategy.Force, "Force", "Force use of Aetherflow", 60, 10, ActionTargets.Self, 45)
            .AddOption(OGCDStrategy.AnyWeave, "Any Weave", "Force use of Aetherflow in any next possible weave slot", 60, 10, ActionTargets.Self, 45)
            .AddOption(OGCDStrategy.EarlyWeave, "Early Weave", "Force use of Aetherflow in very next FIRST weave slot only", 60, 10, ActionTargets.Self, 45)
            .AddOption(OGCDStrategy.LateWeave, "Late Weave", "Force use of Aetherflow in very next LAST weave slot only", 60, 10, ActionTargets.Self, 45)
            .AddOption(OGCDStrategy.Delay, "Delay", "Delay use of Aetherflow", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.Aetherflow);

        return res;
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
    private (float CD, int Stacks, bool IsActive) Aetherflow; //Current Aetherflow stacks (max: 3)
    private bool canAF; //Checks if Aetherflow is completely available
    private bool canED; //Checks if Energy Drain is completely available
    private bool canCS; //Checks if Chain Stratagem is completely available
    private float bioLeft; //Time left on DOT effect (30s base)
    private float stratagemLeft; //Time left on Chain Stratagem (15s base)
    private bool ShouldUseAOE; //Checks if AOE should be used
    private Enemy? BestDOTTargets;
    private Enemy? BestDOTTarget;
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<ScholarGauge>(); //Retrieve Scholar gauge
        Aetherflow.Stacks = gauge.Aetherflow; //Current Aetherflow stacks
        Aetherflow.IsActive = Aetherflow.Stacks > 0; //Checks if Aetherflow is available
        Aetherflow.CD = TotalCD(AID.Aetherflow);
        bioLeft = StatusRemaining(BestDOTTargets?.Actor, BestDOT);
        stratagemLeft = StatusRemaining(BestDOTTargets?.Actor, SID.ChainStratagem);
        canCS = ActionReady(AID.ChainStratagem); //Chain Stratagem is available
        canED = Unlocked(AID.EnergyDrain) && Aetherflow.IsActive; //Energy Drain is available
        canAF = ActionReady(AID.Aetherflow) && !Aetherflow.IsActive; //Aetherflow is available
        ShouldUseAOE = ShouldUseAOECircle(5).OnTwoOrMore; //otherwise, use AOE if 2+ targets would be hit
        (BestDOTTargets, bioLeft) = GetDOTTarget(primaryTarget, BioRemaining, ShouldUseAOECircle(5).OnFourOrMore ? 3 : 4);
        BestDOTTarget = Unlocked(AID.Bio1) ? BestDOTTargets : primaryTarget;

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var Bio = strategy.Option(Track.Bio); //Bio track
        var BioStrategy = Bio.As<BioStrategy>(); //Bio strategy
        var potion = strategy.Option(Track.Potion).As<PotionStrategy>(); //Potion strategy
        var cs = strategy.Option(Track.ChainStratagem); //Chain Stratagem track
        var csStrat = cs.As<OGCDStrategy>(); //Chain Stratagem strategy
        var af = strategy.Option(Track.Aetherflow); //Aetherflow track
        var afStrat = af.As<OGCDStrategy>(); //Aetherflow strategy
        var ed = strategy.Option(Track.EnergyDrain); //Energy Drain track
        var edStrat = ed.As<EnergyStrategy>(); //Energy Drain strategy
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotation
        if (AOEStrategy == AOEStrategy.Auto)
        {
            if (ShouldUseAOE)
                QueueGCD(BestAOE, Player, GCDPriority.Low);
            if (In25y(TargetChoice(AOE) ?? primaryTarget?.Actor) && (!ShouldUseAOE || IsFirstGCD()))
                QueueGCD(IsMoving ? BestRuin : BestST, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Low);
        }
        if (AOEStrategy is AOEStrategy.Ruin2)
            QueueGCD(BestRuin, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Forced);
        if (AOEStrategy is AOEStrategy.Broil)
            QueueGCD(BestBroil, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Forced);
        if (AOEStrategy is AOEStrategy.ArtOfWar)
            QueueGCD(BestAOE, Player, GCDPriority.Forced);
        if (ShouldUseBio(primaryTarget?.Actor, BioStrategy))
            QueueGCD(BestBio, TargetChoice(Bio) ?? BestDOTTarget?.Actor, GCDPriority.Average);
        #endregion

        #region Cooldowns
        if (PlayerHasEffect(SID.ImpactImminent, 30))
            QueueOGCD(AID.BanefulImpaction, TargetChoice(Bio) ?? primaryTarget?.Actor, OGCDPriority.VeryHigh);
        if (ShouldUseChainStratagem(primaryTarget?.Actor, csStrat))
            QueueOGCD(AID.ChainStratagem, TargetChoice(cs) ?? primaryTarget?.Actor, OGCDPrio(csStrat, OGCDPriority.VeryHigh));
        if (ShouldUseAetherflow(primaryTarget?.Actor, afStrat))
            QueueOGCD(AID.Aetherflow, Player, OGCDPrio(afStrat, OGCDPriority.AboveAverage));
        if (ShouldUseEnergyDrain(primaryTarget?.Actor, edStrat))
            QueueOGCD(AID.EnergyDrain, TargetChoice(ed) ?? primaryTarget?.Actor, edStrat is EnergyStrategy.Force ? OGCDPriority.Forced : OGCDPriority.Average);
        if (MP <= 9000 && CanWeaveIn && ActionReady(AID.LucidDreaming))
            QueueOGCD(AID.LucidDreaming, Player, OGCDPriority.Average);
        if ((potion is PotionStrategy.AlignWithRaidBuffs && TotalCD(AID.ChainStratagem) < 5) || potion is PotionStrategy.Immediate)
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, 0, GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        AnyGoalZoneCombined(25, Hints.GoalAOECircle(5), AID.ArtOfWar1, Unlocked(AID.Broil1) ? 2 : 1);
        #endregion
    }

    #region Cooldown Helpers

    #region DOT
    private static SID[] GetDotStatus() => [SID.Bio1, SID.Bio2, SID.Biolysis];
    private float BioRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 0);
    private bool ShouldUseBio(Actor? target, BioStrategy strategy)
    {
        var normalBio = Player.InCombat && target != null && In25y(target);
        return strategy switch
        {
            BioStrategy.Bio3 => normalBio && bioLeft <= 3,
            BioStrategy.Bio6 => normalBio && bioLeft <= 6,
            BioStrategy.Bio9 => normalBio && bioLeft <= 9,
            BioStrategy.Bio0 => normalBio && bioLeft == 0,
            BioStrategy.Force => true,
            BioStrategy.Delay => false,
            _ => false
        };
    }
    #endregion

    private bool ShouldUseChainStratagem(Actor? target, OGCDStrategy strategy) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && canCS && CanWeaveIn && stratagemLeft == 0 && In25y(target),
        OGCDStrategy.Force => canCS,
        OGCDStrategy.AnyWeave => canCS && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canCS && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canCS && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseAetherflow(Actor? target, OGCDStrategy strategy) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && canAF && CanWeaveIn,
        OGCDStrategy.Force => canAF,
        OGCDStrategy.AnyWeave => canAF && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canAF && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canAF && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseEnergyDrain(Actor? target, EnergyStrategy strategy)
    {
        var normalED = Player.InCombat && target != null && canED && In25y(target) && CanWeaveIn;
        var needED = Aetherflow.Stacks > 0 && Aetherflow.CD <= 5;
        return strategy switch
        {
            EnergyStrategy.Use3 => normalED,
            EnergyStrategy.Use2 => normalED && ((Aetherflow.Stacks > 1 && Aetherflow.CD > 5) || needED),
            EnergyStrategy.Use1 => normalED && ((Aetherflow.Stacks > 2 && Aetherflow.CD > 5) || needED),
            EnergyStrategy.Force => canED,
            EnergyStrategy.Delay => false,
            _ => false
        };
    }
    #endregion
}
