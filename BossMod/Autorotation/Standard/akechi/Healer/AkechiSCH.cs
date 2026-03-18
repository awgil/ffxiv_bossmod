using BossMod.SCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiSCH(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Bio, EnergyDrain, ChainStratagem, Aetherflow }
    public enum AOEStrategy { Auto, AutoNoRuin, ForceST, ForceRuin, ForceBroil, ForceArtOfWar }
    public enum BioStrategy { Bio3, Bio6, Bio9, Bio0, Force, Delay }
    public enum EnergyStrategy { Use3, Use2, Use1, Force, ForceWeave, Delay }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi SCH", "Standard Rotation Module", "Standard rotation (Akechi)|Healer", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);

        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionMnd);

        res.Define(Track.AOE).As<AOEStrategy>("ST/AOE", "Single-Target & AoE Rotations", 300)
            .AddOption(AOEStrategy.Auto, "Automatically use best actions based on targets nearby - Broil if stationary, Ruin if moving, Art of War if enough targets")
            .AddOption(AOEStrategy.AutoNoRuin, "Automatically use best actions based on targets nearby - does not use Ruin")
            .AddOption(AOEStrategy.ForceST, "Force single-target actions, regardless of targets nearby - Broil if stationary, Ruin if moving")
            .AddOption(AOEStrategy.ForceRuin, "Force Ruin only, regardless of targets nearby")
            .AddOption(AOEStrategy.ForceBroil, "Force Broil only, regardless of targets nearby")
            .AddOption(AOEStrategy.ForceArtOfWar, "Force Art of War only, regardless of targets nearby")
            .AddAssociatedActions(AID.Ruin1, AID.Ruin2, AID.Broil1, AID.Broil2, AID.Broil3, AID.Broil4, AID.ArtOfWar1, AID.ArtOfWar2);

        res.Define(Track.Bio).As<BioStrategy>("Bio", "Biolysis (DoT)", 190)
            .AddOption(BioStrategy.Bio3, "Use Biolysis if target has 3s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio6, "Use Biolysis if target has 6s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio9, "Use Biolysis if target has 9s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Bio0, "Use Biolysis if target does not have DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Force, "Force use of Biolysis regardless of DoT effect", 0, 30, ActionTargets.Hostile, 2)
            .AddOption(BioStrategy.Delay, "Delay use of Biolysis", 0, 0, ActionTargets.Hostile, 2)
            .AddAssociatedActions(AID.Bio1, AID.Bio2, AID.Biolysis);

        res.Define(Track.EnergyDrain).As<EnergyStrategy>("E.Drain", "Energy Drain", 150)
            .AddOption(EnergyStrategy.Use3, "Uses all stacks of Aetherflow for Energy Drain; conserves no stacks for manual usage", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Use2, "Uses 2 stacks of Aetherflow for Energy Drain; conserves 1 stack for manual usage, but consumes any remaining when Aetherflow ability is about to be up", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Use1, "Uses 1 stack of Aetherflow for Energy Drain; conserves 2 stacks for manual usage, but consumes any remaining when Aetherflow ability is about to be up", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Force, "Force use of Energy Drain if any Aetherflow is available", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.ForceWeave, "Force use of Energy Drain in next weave if any Aetherflow is available", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(EnergyStrategy.Delay, "Delay use of Energy Drain", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.EnergyDrain);

        res.DefineOGCD(Track.ChainStratagem, AID.ChainStratagem, "C.Strat", "Chain Stratagem", 170, 120, 20, ActionTargets.Hostile, 66);
        res.DefineOGCD(Track.Aetherflow, AID.Aetherflow, "A.flow", "Aetherflow", 160, 60, 10, ActionTargets.Self, 45);

        return res;
    }

    private AID BestBroil => Unlocked(AID.Broil4) ? AID.Broil4 : Unlocked(AID.Broil3) ? AID.Broil3 : Unlocked(AID.Broil2) ? AID.Broil2 : AID.Broil1;
    private AID BestRuin => Unlocked(AID.Ruin2) ? AID.Ruin2 : AID.Ruin1;
    private AID BestBio => Unlocked(AID.Biolysis) ? AID.Biolysis : Unlocked(AID.Bio2) ? AID.Bio2 : AID.Bio1;
    private SID BestDOT => Unlocked(AID.Biolysis) ? SID.Biolysis : Unlocked(AID.Bio2) ? SID.Bio2 : SID.Bio1;
    private AID BestST => Unlocked(AID.Broil1) ? BestBroil : BestRuin;
    private AID BestAOE => Unlocked(AID.ArtOfWar2) ? AID.ArtOfWar2 : AID.ArtOfWar1;

    private static SID[] GetDotStatus() => [SID.Bio1, SID.Bio2, SID.Biolysis];
    private float BioRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 0);

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var gauge = World.Client.GetGauge<ScholarGauge>();
        var mainTarget = primaryTarget?.Actor;
        var aetherflow = gauge.Aetherflow;
        var CanED = Unlocked(AID.EnergyDrain) && aetherflow > 0;
        var CanAF = ActionReady(AID.Aetherflow) && aetherflow == 0;
        var (dotTargets, BioLeft) = GetDOTTarget(primaryTarget, BioRemaining, TargetsInAOECircle(5f, 4) ? 3 : 4);
        var BestDOTTarget = Unlocked(AID.Bio1) ? dotTargets : primaryTarget;
        var dotLeft = StatusRemaining(dotTargets?.Actor, BestDOT);
        var csLeft = StatusRemaining(mainTarget, SID.ChainStratagem);

        var aoe = strategy.Option(Track.AOE);
        var aoeStrat = aoe.As<AOEStrategy>();
        var stTarget = SingleTargetChoice(mainTarget, aoe);
        var wantAOE = aoeStrat == AOEStrategy.ForceArtOfWar || TargetsInAOECircle(5f, 2);
        var bestTarget = wantAOE ? Player : stTarget;
        var (aoeAction, aoeTarget) = aoeStrat switch
        {
            AOEStrategy.Auto => (wantAOE ? BestAOE : (IsMoving ? BestRuin : BestST), bestTarget),
            AOEStrategy.AutoNoRuin => (wantAOE ? BestAOE : BestST, bestTarget),
            AOEStrategy.ForceST => (IsMoving ? BestRuin : BestST, stTarget),
            AOEStrategy.ForceRuin => (BestRuin, stTarget),
            AOEStrategy.ForceBroil => (BestST, stTarget),
            AOEStrategy.ForceArtOfWar => (BestAOE, Player),
            _ => (AID.None, null)
        };
        if (InCombat(aoeTarget) && aoeAction != AID.None)
            QueueGCD(aoeAction, aoeTarget, GCDPriority.Low);

        if (Unlocked(AID.Bio1))
        {
            var b = strategy.Option(Track.Bio);
            var bStrat = b.As<BioStrategy>();
            var bTarget = AOETargetChoice(mainTarget, BestDOTTarget?.Actor, b, strategy);
            var bMinimum = Player.InCombat && bTarget != null && In25y(bTarget);
            if (InCombat(bTarget) && In25y(bTarget) && bStrat switch
            {
                BioStrategy.Bio3 => BioLeft <= 3,
                BioStrategy.Bio6 => BioLeft <= 6,
                BioStrategy.Bio9 => BioLeft <= 9,
                BioStrategy.Bio0 => BioLeft == 0,
                BioStrategy.Force => true,
                _ => false
            })
                QueueGCD(BestBio, bTarget, GCDPriority.Average);
        }

        if (HasEffect(SID.ImpactImminent))
            QueueOGCD(AID.BanefulImpaction, aoeTarget, OGCDPriority.VeryHigh);

        var cs = strategy.Option(Track.ChainStratagem);
        var csStrat = cs.As<OGCDStrategy>();
        var csTarget = SingleTargetChoice(mainTarget, cs);
        if (ShouldUseOGCD(csStrat, csTarget, ActionReady(AID.ChainStratagem), InCombat(csTarget) && CanWeaveIn && !HasEffect(SID.ChainStratagem)))
            QueueOGCD(AID.ChainStratagem, csTarget, OGCDPrio(csStrat, OGCDPriority.High + 1));

        var afStrat = strategy.Option(Track.Aetherflow).As<OGCDStrategy>();
        if (ShouldUseOGCD(afStrat, mainTarget, ActionReady(AID.Aetherflow), InCombat(mainTarget) && CanWeaveIn && aetherflow == 0))
            QueueOGCD(AID.Aetherflow, Player, OGCDPrio(afStrat, OGCDPriority.High));

        if (Unlocked(AID.EnergyDrain))
        {
            var ed = strategy.Option(Track.EnergyDrain);
            var edStrat = ed.As<EnergyStrategy>();
            var edTarget = SingleTargetChoice(mainTarget, ed);
            var normal = aetherflow > 0 && InCombat(edTarget) && In25y(edTarget) && CanWeaveIn;
            var needTimer = Cooldown(AID.Aetherflow) <= 5;
            var need = aetherflow > 0 && needTimer;
            var (edCondition, edPrio) = edStrat switch
            {
                EnergyStrategy.Use3 => (normal, OGCDPriority.Average),
                EnergyStrategy.Use2 => (normal && ((aetherflow > 1 && !needTimer) || need), OGCDPriority.Average),
                EnergyStrategy.Use1 => (normal && ((aetherflow > 2 && !needTimer) || need), OGCDPriority.Average),
                EnergyStrategy.Force => (aetherflow > 0, OGCDPriority.Average + 2000),
                EnergyStrategy.ForceWeave => (aetherflow > 0 && CanWeaveIn, OGCDPriority.Average + 1000),
                _ => (false, OGCDPriority.None)
            };
            if (edCondition)
                QueueOGCD(AID.EnergyDrain, edTarget, edPrio);
        }

        if (ActionReady(AID.LucidDreaming) && MP <= 9000 && CanWeaveIn)
            QueueOGCD(AID.LucidDreaming, Player, OGCDPriority.Average - 1);

        if (strategy.Potion() switch
        {
            PotionStrategy.AlignWithBuffs => Player.InCombat && Cooldown(AID.ChainStratagem) <= 4f,
            PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
            PotionStrategy.Immediate => true,
            _ => false
        })
            QueuePotMND();

        AnyGoalZoneCombined(25, Hints.GoalAOECircle(5), AID.ArtOfWar1, Unlocked(AID.Broil1) ? 2 : 1);
    }
}
