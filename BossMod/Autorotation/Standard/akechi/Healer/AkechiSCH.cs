using BossMod.SCH;
using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiSCH(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { ST = SharedTrack.Count, Bio, EnergyDrain, ChainStratagem, Aetherflow }
    public enum STOption { Ruin2, Broil }
    public enum BioStrategy { Bio3, Bio6, Bio9, Bio0, Force, Delay }
    public enum EnergyStrategy { Use3, Use2, Use1, Force, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi SCH", "Standard Rotation Module", "Standard rotation (Akechi)|Healer", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);
        res.DefineAOE().AddAssociatedActions(AID.Ruin1, AID.Ruin2, AID.Broil1, AID.Broil2, AID.Broil3, AID.Broil4, AID.ArtOfWar1, AID.ArtOfWar2);
        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionMnd);
        res.Define(Track.ST).As<STOption>("Single Target", "ST", uiPriority: 200)
            .AddOption(STOption.Ruin2, "Use Ruin if single target is forced")
            .AddOption(STOption.Broil, "Use Broil if single target is forced")
            .AddAssociatedActions(AID.Ruin1, AID.Ruin2, AID.Broil1, AID.Broil2, AID.Broil3, AID.Broil4);
        res.Define(Track.Bio).As<BioStrategy>("Damage Over Time", "Bio", uiPriority: 190)
            .AddOption(BioStrategy.Bio3, "Use Bio if target has 3s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio6, "Use Bio if target has 6s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio9, "Use Bio if target has 9s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio0, "Use Bio if target does not have DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Force, "Force use of Bio regardless of DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Delay, "Delay the use of Bio for manual or strategic usage", 0, 0, ActionTargets.Hostile, 2)
            .AddAssociatedActions(AID.Bio1, AID.Bio2, AID.Biolysis);
        res.Define(Track.EnergyDrain).As<EnergyStrategy>("Energy Drain", "E.Drain", uiPriority: 150)
            .AddOption(EnergyStrategy.Use3, "Uses all stacks of Aetherflow for Energy Drain; conserves no stacks for manual usage", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Use2, "Uses 2 stacks of Aetherflow for Energy Drain; conserves 1 stack for manual usage, but consumes any remaining when Aetherflow ability is about to be up", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Use1, "Uses 1 stack of Aetherflow for Energy Drain; conserves 2 stacks for manual usage, but consumes any remaining when Aetherflow ability is about to be up", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Force, "Force use of Energy Drain if any Aetherflow is available", 0, 0, ActionTargets.None, 45)
            .AddOption(EnergyStrategy.Delay, "Delay use of Energy Drain", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.EnergyDrain);
        res.DefineOGCD(Track.ChainStratagem, AID.ChainStratagem, "Chain Stratagem", "C.Strat", uiPriority: 170, 120, 20, ActionTargets.Hostile, 66);
        res.DefineOGCD(Track.Aetherflow, AID.Aetherflow, "Aetherflow", "A.flow", uiPriority: 160, 60, 10, ActionTargets.Self, 45);
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
    private (float CD, int Stacks, bool IsActive) Aetherflow;
    private bool CanAF;
    private bool CanED;
    private bool CanCS;
    private float BioLeft;
    private float CSLeft;
    private bool ShouldUseAOE;
    private Enemy? BestDOTTargets;
    private Enemy? BestDOTTarget;
    #endregion

    #region Module Helpers

    #region DOT
    private static SID[] GetDotStatus() => [SID.Bio1, SID.Bio2, SID.Biolysis];
    private float BioRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 0);
    private bool ShouldUseBio(Actor? target, BioStrategy strategy)
    {
        var normalBio = Player.InCombat && target != null && In25y(target);
        return strategy switch
        {
            BioStrategy.Bio3 => normalBio && BioLeft <= 3,
            BioStrategy.Bio6 => normalBio && BioLeft <= 6,
            BioStrategy.Bio9 => normalBio && BioLeft <= 9,
            BioStrategy.Bio0 => normalBio && BioLeft == 0,
            BioStrategy.Force => true,
            BioStrategy.Delay => false,
            _ => false
        };
    }
    #endregion

    private bool ShouldUseChainStratagem(Actor? target, OGCDStrategy strategy) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanCS && CanWeaveIn && CSLeft == 0 && In25y(target),
        OGCDStrategy.Force => CanCS,
        OGCDStrategy.AnyWeave => CanCS && CanWeaveIn,
        OGCDStrategy.EarlyWeave => CanCS && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => CanCS && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseAetherflow(Actor? target, OGCDStrategy strategy) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanAF && CanWeaveIn,
        OGCDStrategy.Force => CanAF,
        OGCDStrategy.AnyWeave => CanAF && CanWeaveIn,
        OGCDStrategy.EarlyWeave => CanAF && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => CanAF && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseEnergyDrain(Actor? target, EnergyStrategy strategy)
    {
        var normalED = Player.InCombat && target != null && CanED && In25y(target) && CanWeaveIn;
        var needED = Aetherflow.Stacks > 0 && Aetherflow.CD <= 5;
        return strategy switch
        {
            EnergyStrategy.Use3 => normalED,
            EnergyStrategy.Use2 => normalED && ((Aetherflow.Stacks > 1 && Aetherflow.CD > 5) || needED),
            EnergyStrategy.Use1 => normalED && ((Aetherflow.Stacks > 2 && Aetherflow.CD > 5) || needED),
            EnergyStrategy.Force => CanED,
            EnergyStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Potion() switch
    {
        PotionStrategy.AlignWithBuffs => Player.InCombat && CDRemaining(AID.ChainStratagem) <= 4f,
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => true,
        _ => false
    };

    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<ScholarGauge>(); //Retrieve Scholar gauge
        Aetherflow.Stacks = gauge.Aetherflow; //Current Aetherflow stacks
        Aetherflow.IsActive = Aetherflow.Stacks > 0; //Checks if Aetherflow is available
        Aetherflow.CD = CDRemaining(AID.Aetherflow);
        BioLeft = StatusRemaining(BestDOTTargets?.Actor, BestDOT);
        CSLeft = StatusRemaining(BestDOTTargets?.Actor, SID.ChainStratagem);
        CanCS = ActionReady(AID.ChainStratagem); //Chain Stratagem is available
        CanED = Unlocked(AID.EnergyDrain) && Aetherflow.IsActive; //Energy Drain is available
        CanAF = ActionReady(AID.Aetherflow) && !Aetherflow.IsActive; //Aetherflow is available
        ShouldUseAOE = ShouldUseAOECircle(5).OnTwoOrMore; //otherwise, use AOE if 2+ targets would be hit
        (BestDOTTargets, BioLeft) = GetDOTTarget(primaryTarget, BioRemaining, ShouldUseAOECircle(5).OnFourOrMore ? 3 : 4);
        BestDOTTarget = Unlocked(AID.Bio1) ? BestDOTTargets : primaryTarget;

        #region Strategy Definitions
        var AOE = strategy.Option(SharedTrack.AOE);
        var st = strategy.Option(Track.ST); //Single Target track
        var stStrat = st.As<STOption>(); //Single Target strategy

        var Bio = strategy.Option(Track.Bio); //Bio track
        var BioStrategy = Bio.As<BioStrategy>(); //Bio strategy
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
        if (strategy.AutoFinish() || strategy.AutoBreak())
        {
            if (ShouldUseAOE)
                QueueGCD(BestAOE, Player, GCDPriority.Low);
            if (In25y(SingleTargetChoice(primaryTarget?.Actor, AOE)) && (!ShouldUseAOE || IsFirstGCD))
                QueueGCD(IsMoving ? BestRuin : BestST, SingleTargetChoice(primaryTarget?.Actor, AOE), GCDPriority.Low);
        }
        if (strategy.ForceST())
        {
            if (stStrat is STOption.Ruin2)
                QueueGCD(BestRuin, SingleTargetChoice(primaryTarget?.Actor, AOE), GCDPriority.Low);
            if (stStrat is STOption.Broil)
                QueueGCD(BestBroil, SingleTargetChoice(primaryTarget?.Actor, AOE), GCDPriority.Low);
        }
        if (strategy.ForceAOE())
            QueueGCD(BestAOE, Player, GCDPriority.Low);
        if (ShouldUseBio(primaryTarget?.Actor, BioStrategy))
            QueueGCD(BestBio, AOETargetChoice(primaryTarget?.Actor, BestDOTTarget?.Actor, Bio, strategy), GCDPriority.Average);
        #endregion

        #region Cooldowns
        if (HasEffect(SID.ImpactImminent))
            QueueOGCD(AID.BanefulImpaction, SingleTargetChoice(primaryTarget?.Actor, AOE), OGCDPriority.VeryHigh);
        if (ShouldUseChainStratagem(primaryTarget?.Actor, csStrat))
            QueueOGCD(AID.ChainStratagem, SingleTargetChoice(primaryTarget?.Actor, cs), OGCDPrio(csStrat, OGCDPriority.VeryHigh));
        if (ShouldUseAetherflow(primaryTarget?.Actor, afStrat))
            QueueOGCD(AID.Aetherflow, Player, OGCDPrio(afStrat, OGCDPriority.AboveAverage));
        if (ShouldUseEnergyDrain(primaryTarget?.Actor, edStrat))
            QueueOGCD(AID.EnergyDrain, SingleTargetChoice(primaryTarget?.Actor, ed), edStrat is EnergyStrategy.Force ? OGCDPriority.Forced : OGCDPriority.Average);
        if (MP <= 9000 && CanWeaveIn && ActionReady(AID.LucidDreaming))
            QueueOGCD(AID.LucidDreaming, Player, OGCDPriority.Average);
        if (ShouldUsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.Medium);
        #endregion

        #endregion

        #region AI
        AnyGoalZoneCombined(25, Hints.GoalAOECircle(5), AID.ArtOfWar1, Unlocked(AID.Broil1) ? 2 : 1);
        #endregion
    }
}
