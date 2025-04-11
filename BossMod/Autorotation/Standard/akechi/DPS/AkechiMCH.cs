using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.MCH;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiMCH(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track
    {
        Potion = SharedTrack.Count,
        Hypercharge, HeatSpender, Battery, Reassemble, Drill, Wildfire,
        BarrelStabilizer, AirAnchor, Chainsaw, GaussRound, Ricochet, Flamethrower, Excavator, FullMetalField
    }
    public enum PotionStrategy { None, Use, Align }
    public enum HyperchargeStrategy { Automatic, ASAP, Full, Delay }
    public enum HeatSpenderStrategy { Automatic, OnlyHeatBlast, OnlyAutoCrossbow }
    public enum BatteryStrategy { Automatic, Fifty, Hundred, RaidBuffs, End, Delay }
    public enum ReassembleStrategy { Automatic, HoldOne, Force, ForceWeave, Delay }
    public enum WildfireStrategy { Automatic, End, Force, ForceWeave, Delay }
    public enum DrillStrategy { Automatic, OnlyDrill, OnlyBioblaster, ForceDrill, ForceBioblaster, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi MCH", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.MCH), 100);
        res.DefineAOE().AddAssociatedActions(
            AID.SplitShot, AID.SlugShot, AID.CleanShot,
            AID.HeatedSplitShot, AID.HeatedSlugShot, AID.HeatedCleanShot,
            AID.SpreadShot, AID.Scattergun);
        res.DefineHold();
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 200)
            .AddOption(PotionStrategy.None, "None", "Do not use Potion")
            .AddOption(PotionStrategy.Use, "Use", "Use Potion when available", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Align, "Align", "Align Potion with raid buffs", 270, 30, ActionTargets.Self);
        res.Define(Track.Hypercharge).As<HyperchargeStrategy>("HC", uiPriority: 200)
            .AddOption(HyperchargeStrategy.Automatic, "Automatic", "Use Heat actions when optimal")
            .AddOption(HyperchargeStrategy.ASAP, "ASAP", "Use Heat actions ASAP (if any Heat Gauge is available)", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(HyperchargeStrategy.Full, "Full", "Use Heat actions when Heat Gauge is full (or about to be)", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(HyperchargeStrategy.Delay, "Delay", "Delay use of Heat actions", 0, 0, ActionTargets.None, 30);
        res.Define(Track.HeatSpender).As<HeatSpenderStrategy>("Heat Spender", uiPriority: 200)
            .AddOption(HeatSpenderStrategy.Automatic, "Automatic", "Automatically choose best optimal Heat actions")
            .AddOption(HeatSpenderStrategy.OnlyHeatBlast, "Heat Blast", "Only use Heat Blast, regardless of targets", 0, 0, ActionTargets.Hostile, 35)
            .AddOption(HeatSpenderStrategy.OnlyAutoCrossbow, "Auto Crossbow", "Only use Auto Crossbow, regardless of targets", 0, 0, ActionTargets.Hostile, 52)
            .AddAssociatedActions(AID.HeatBlast, AID.AutoCrossbow, AID.BlazingShot);
        res.Define(Track.Battery).As<BatteryStrategy>("Battery", uiPriority: 200)
            .AddOption(BatteryStrategy.Automatic, "Automatic", "Use Battery actions when optimal")
            .AddOption(BatteryStrategy.Fifty, "50", "Use Battery actions ASAP when 50 or more Battery Gauge is available", 0, 0, ActionTargets.Self, 40)
            .AddOption(BatteryStrategy.Hundred, "100", "Use Battery actions ASAP when 100 Battery Gauge is available", 0, 0, ActionTargets.Self, 40)
            .AddOption(BatteryStrategy.RaidBuffs, "Raid Buffs", "Use Battery actions when raid buffs are active", 0, 0, ActionTargets.Self, 40)
            .AddOption(BatteryStrategy.End, "End", "Ends Battery action ASAP with Overdrive (assuming it's currently active)", 0, 0, ActionTargets.Self, 40)
            .AddOption(BatteryStrategy.Delay, "Delay", "Delay use of Battery actions", 0, 0, ActionTargets.None, 40);
        res.Define(Track.Reassemble).As<ReassembleStrategy>("Reassemble", uiPriority: 200)
            .AddOption(ReassembleStrategy.Automatic, "Automatic", "Use Reassemble when optimal")
            .AddOption(ReassembleStrategy.HoldOne, "Hold One", "Hold one charge of Reassemble for manual usage", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.Force, "Force", "Force use of Reassemble, regardless of weaving", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.ForceWeave, "ForceWeave", "Force use of Reassemble in next possible weave window", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.Delay, "Delay", "Delay use of Reassemble", 0, 0, ActionTargets.None, 10)
            .AddAssociatedActions(AID.Reassemble);
        res.Define(Track.Drill).As<DrillStrategy>("Drill", uiPriority: 200)
            .AddOption(DrillStrategy.Automatic, "Automatic", "Use Drill when optimal")
            .AddOption(DrillStrategy.OnlyDrill, "Only Drill", "Only use Drill, regardless of targets", 20, 0, ActionTargets.Hostile, 58)
            .AddOption(DrillStrategy.OnlyBioblaster, "Only Bioblaster", "Only use Bioblaster, regardless of targets", 20, 0, ActionTargets.Hostile, 58)
            .AddOption(DrillStrategy.ForceDrill, "Force Drill", "Force use of Drill", 20, 15, ActionTargets.Hostile, 72)
            .AddOption(DrillStrategy.ForceBioblaster, "Force Bioblaster", "Force use of Bioblaster", 20, 15, ActionTargets.Hostile, 72)
            .AddOption(DrillStrategy.Delay, "Delay", "Delay use of Drill", 0, 0, ActionTargets.None, 58)
            .AddAssociatedActions(AID.Drill, AID.Bioblaster);
        res.Define(Track.Wildfire).As<WildfireStrategy>("Wildfire", uiPriority: 200)
            .AddOption(WildfireStrategy.Automatic, "Automatic", "Use Wildfire when optimal")
            .AddOption(WildfireStrategy.End, "End", "End Wildfire early with Detonator", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.Force, "Force", "Force use of Wildfire, regardless of weaving", 120, 10, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.ForceWeave, "ForceWeave", "Force use of Wildfire in next possible weave window", 120, 10, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.Delay, "Delay", "Delay use of Wildfire", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.Wildfire);
        res.DefineOGCD(Track.BarrelStabilizer, AID.BarrelStabilizer, "Barrel Stabilizer", "B.Stab.", uiPriority: 200, 120, 30, ActionTargets.Hostile, 66).AddAssociatedActions(AID.BarrelStabilizer);
        res.DefineGCD(Track.AirAnchor, AID.AirAnchor, "Air Anchor", "A.Anchor", uiPriority: 200, 40, 0, ActionTargets.Hostile, 30).AddAssociatedActions(AID.AirAnchor);
        res.DefineGCD(Track.Chainsaw, AID.ChainSaw, "Chainsaw", "C.saw", uiPriority: 200, 60, 30, ActionTargets.Hostile, 30).AddAssociatedActions(AID.ChainSaw);
        res.DefineOGCD(Track.GaussRound, AID.GaussRound, "Gauss Round", "G.Round", uiPriority: 200, 30, 0, ActionTargets.Hostile, 15).AddAssociatedActions(AID.GaussRound);
        res.DefineOGCD(Track.Ricochet, AID.Ricochet, "Ricochet", "Rico", uiPriority: 200, 30, 0, ActionTargets.Hostile, 50).AddAssociatedActions(AID.Ricochet);
        res.DefineGCD(Track.Flamethrower, AID.Flamethrower, "Flamethrower", "F.thrower", uiPriority: 200, 0, 0, ActionTargets.Hostile, 70).AddAssociatedActions(AID.Flamethrower);
        res.DefineGCD(Track.Excavator, AID.Excavator, "Excavator", "Excav.", uiPriority: 200, 0, 0, ActionTargets.Hostile, 96).AddAssociatedActions(AID.Excavator);
        res.DefineGCD(Track.FullMetalField, AID.FullMetalField, "Full Metal Field", "F.M.Field", uiPriority: 200, 0, 0, ActionTargets.Hostile, 100).AddAssociatedActions(AID.FullMetalField);
        return res;
    }
    #endregion

    #region Module Variables
    public int Heat; // max 100
    public int Battery; // max 100
    public bool BatteryCapped;
    public float OverheatLeft; // max 10s
    public bool OverheatActive;
    public bool MinionActive;
    public float ReassembleLeft; // max 5s
    public float WildfireLeft; // max 10s
    public float HyperchargedLeft; // max 30s
    public float ExcavatorLeft; // max 30s
    public float FMFLeft; // max 30s
    public bool InFlamethrower;
    public bool ShouldUseAOE;
    public bool ShouldUseRangedAOE;
    public bool ShouldUseSaw;
    public bool ShouldFlamethrower;
    public bool CanAA;
    public bool CanReassemble;
    public bool CanGR;
    public bool CanRicochet;
    public bool CanHC;
    public bool CanHB;
    public bool CanSummon;
    public bool CanWF;
    public bool CanDrill;
    public bool CanBS;
    public bool CanChainsaw;
    public bool CanExcavate;
    public bool CanFMF;
    public bool AfterDrill;
    public bool AfterAirAnchor;
    public bool AfterChainsaw;
    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumSawTargets;
    public int NumFlamethrowerTargets;
    public Enemy? BestAOETargets;
    public Enemy? BestRangedAOETargets;
    public Enemy? BestChainsawTargets;
    public Enemy? BestAOETarget;
    public Enemy? BestRangedAOETarget;
    public Enemy? BestChainsawTarget;
    public Enemy? BestFlamethrowerTarget;

    private bool IsPausedForFlamethrower => Service.Config.Get<MCHConfig>().PauseForFlamethrower && InFlamethrower;
    #endregion

    #region Upgrade Paths
    public AID ST
        => ComboLastMove == BestSlugShot ? BestCleanShot : //3
        ComboLastMove == BestSplitShot ? BestSlugShot : //2
        BestSplitShot; //1
    public AID BestSpreadShot => Unlocked(AID.Scattergun) ? AID.Scattergun : AID.SpreadShot;
    public AID BestSplitShot => Unlocked(AID.HeatedSplitShot) ? AID.HeatedSplitShot : AID.SplitShot;
    public AID BestSlugShot => Unlocked(AID.HeatedSlugShot) ? AID.HeatedSlugShot : AID.SlugShot;
    public AID BestCleanShot => Unlocked(AID.HeatedCleanShot) ? AID.HeatedCleanShot : AID.CleanShot;
    public AID BestHeatBlast => Unlocked(AID.BlazingShot) ? AID.BlazingShot : Unlocked(AID.HeatBlast) ? AID.HeatBlast : ST;
    public AID BestDrill => NumAOETargets > 1 && Unlocked(AID.Bioblaster) ? AID.Bioblaster : AID.Drill;
    public AID BestAirAnchor => Unlocked(AID.AirAnchor) ? AID.AirAnchor : AID.HotShot;
    public AID BestGauss => Unlocked(AID.DoubleCheck) ? AID.DoubleCheck : AID.GaussRound;
    public AID BestRicochet => Unlocked(AID.Checkmate) ? AID.Checkmate : AID.Ricochet;
    public AID BestHeatSpender => NumAOETargets > 3 ? AID.AutoCrossbow : BestHeatBlast;
    public AID BestBattery => Unlocked(AID.AutomatonQueen) ? AID.AutomatonQueen : AID.RookAutoturret;
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.SlugShot or AID.HeatedSlugShot or AID.SplitShot or AID.HeatedSplitShot => ST,
        AID.CleanShot or AID.HeatedCleanShot or AID.Scattergun or AID.SpreadShot or _ => AutoBreak,
    };
    private AID AutoBreak => ShouldUseAOE ? BestSpreadShot : ST;
    #endregion

    #region Cooldown Helpers

    #region Buffs
    private bool ShouldUseWildfire(WildfireStrategy strategy, Actor? target) => strategy switch
    {
        WildfireStrategy.Automatic => InsideCombatWith(target) && In25y(target) && CanWF && LastActionUsed(AID.Hypercharge),
        WildfireStrategy.End => PlayerHasEffect(SID.WildfirePlayer),
        WildfireStrategy.Force => CanWF,
        WildfireStrategy.ForceWeave => CanWF && CanWeaveIn,
        WildfireStrategy.Delay or _ => false,
    };
    private bool ShouldUseBarrelStabilizer(OGCDStrategy strategy, Actor? target)
    {
        if (!CanBS)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In25y(target),
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Force => true,
            OGCDStrategy.Delay or _ => false,
        };
    }
    private bool Assemble(AID aid) => aid is AID.Drill or AID.AirAnchor or AID.ChainSaw or AID.Excavator;
    private bool ShouldUseReassemble(ReassembleStrategy strategy)
    {
        if (!CanReassemble)
            return false;

        if (strategy == ReassembleStrategy.Automatic ||
            strategy == ReassembleStrategy.HoldOne && TotalCD(AID.Reassemble) < 20)
        {
            Assemble(NextGCD);
            return NextGCD switch
            {
                AID.SpreadShot or AID.Scattergun or AID.AutoCrossbow => true,
                AID.CleanShot => !Unlocked(AID.Drill),
                AID.HotShot => !Unlocked(AID.CleanShot),
                _ => false
            };
        }
        return strategy switch
        {
            ReassembleStrategy.Force => !PlayerHasEffect(SID.Reassembled),
            ReassembleStrategy.Delay or _ => false,
        };
    }
    #endregion

    #region Tools
    private bool ShouldUseDrill(DrillStrategy strategy, Actor? target)
    {
        if (!CanDrill)
            return false;
        return strategy switch
        {
            DrillStrategy.Automatic => InsideCombatWith(target) && (ShouldUseAOE ? In12y(target) : In25y(target)),
            DrillStrategy.OnlyDrill => true,
            DrillStrategy.OnlyBioblaster => true,
            DrillStrategy.ForceDrill => true,
            DrillStrategy.ForceBioblaster => true,
            DrillStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseAirAnchor(GCDStrategy strategy, Actor? target)
    {
        if (!CanAA)
            return false;
        return strategy switch
        {
            GCDStrategy.Automatic => InsideCombatWith(target) && In25y(target),
            GCDStrategy.Force => true,
            GCDStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseChainsaw(GCDStrategy strategy, Actor? target)
    {
        if (!CanChainsaw)
            return false;
        return strategy switch
        {
            GCDStrategy.Automatic => InsideCombatWith(target) && In25y(target),
            GCDStrategy.Force => true,
            GCDStrategy.Delay or _ => false,
        };
    }

    private bool ShouldUseExcavator(GCDStrategy strategy, Actor? target)
    {
        if (!CanExcavate)
            return false;
        return strategy switch
        {
            GCDStrategy.Automatic => InsideCombatWith(target) && In25y(target),
            GCDStrategy.Force => true,
            GCDStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseFullMetalField(GCDStrategy strategy, Actor? target)
    {
        if (!CanFMF)
            return false;
        return strategy switch
        {
            GCDStrategy.Automatic => InsideCombatWith(target) && In25y(target),
            GCDStrategy.Force => true,
            GCDStrategy.Delay or _ => false,
        };
    }

    #endregion

    #region Heat
    private bool ShouldUseHypercharge(HyperchargeStrategy strategy, Actor? target)
    {
        if (!CanHC)
            return false;

        if (strategy == HyperchargeStrategy.Automatic)
        {
            // Ensures Hypercharge is double weaved with WF
            if (Unlocked(AID.FullMetalField) && LastActionUsed(AID.FullMetalField) &&
                (TotalCD(AID.Wildfire) < GCD || ActionReady(AID.Wildfire)) ||
                !Unlocked(AID.FullMetalField) && ActionReady(AID.Wildfire) ||
                !Unlocked(AID.Wildfire))
                return true;

            // Only Hypercharge when tools are on cooldown
            if (AfterAirAnchor && AfterChainsaw && AfterDrill &&
                (!Unlocked(AID.Wildfire) ||
                (Unlocked(AID.Wildfire) &&
                (TotalCD(AID.Wildfire) > 40 ||
                IsOffCooldown(AID.Wildfire) && !PlayerHasEffect(SID.FullMetalMachinist)))))
                return true;
        }

        return strategy switch
        {
            HyperchargeStrategy.ASAP => CanWeaveIn,
            HyperchargeStrategy.Full => CanWeaveIn && Heat >= 100,
            HyperchargeStrategy.Delay or _ => false,
        };
    }
    private bool ShouldChooseHeat(HeatSpenderStrategy strategy, Actor? target)
    {
        if (!CanHB)
            return false;
        return strategy switch
        {
            HeatSpenderStrategy.Automatic => InsideCombatWith(target) && (ShouldUseAOE ? In12y(target) : In25y(target)),
            HeatSpenderStrategy.OnlyHeatBlast => true,
            HeatSpenderStrategy.OnlyAutoCrossbow => true,
            _ => false,
        };
    }
    #endregion

    #region Battery
    #endregion

    private bool ShouldUseGaussRound(OGCDStrategy strategy, Actor? target)
    {
        if (!CanGR)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In25y(target),
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Force => true,
            OGCDStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseRicochet(OGCDStrategy strategy, Actor? target)
    {
        if (!CanRicochet)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In25y(target),
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Force => true,
            OGCDStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseFlamethrower(GCDStrategy strategy, Actor? target)
    {
        if (!CanHC)
            return false;
        return strategy switch
        {
            GCDStrategy.Automatic => InsideCombatWith(target) && In25y(target),
            GCDStrategy.Force => true,
            GCDStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUsePotion(PotionStrategy strategy)
    {
        return strategy switch
        {
            PotionStrategy.Use => true,
            PotionStrategy.Align => TotalCD(AID.BarrelStabilizer) < 5f,
            _ => false,
        };
    }
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<MachinistGauge>();
        Heat = gauge.Heat;
        Battery = gauge.Battery;
        BatteryCapped = gauge.Battery >= 100;
        OverheatLeft = gauge.OverheatTimeRemaining / 1000f;
        OverheatActive = (gauge.TimerActive & 1) != 0;
        MinionActive = (gauge.TimerActive & 2) != 0;
        ReassembleLeft = StatusRemaining(Player, SID.Reassembled);
        WildfireLeft = StatusRemaining(Player, SID.WildfirePlayer);
        HyperchargedLeft = StatusRemaining(Player, SID.Hypercharged);
        ExcavatorLeft = StatusRemaining(Player, SID.ExcavatorReady);
        FMFLeft = StatusRemaining(Player, SID.FullMetalMachinist);
        InFlamethrower = StatusRemaining(Player, SID.Flamethrower) > 0;
        AfterDrill = !CanDrill || (CanDrill && ((!Unlocked(TraitID.EnhancedMultiweapon) && TotalCD(AID.Drill) >= 9) || (Unlocked(TraitID.EnhancedMultiweapon) && TotalCD(AID.Drill) >= 9)));
        AfterAirAnchor = !CanAA || (CanAA && TotalCD(BestAirAnchor) >= 9);
        AfterChainsaw = !CanChainsaw || (CanChainsaw && TotalCD(AID.ChainSaw) >= 9);
        (BestAOETargets, NumAOETargets) = GetBestTarget(primaryTarget, 12, Is12yConeTarget);
        (BestRangedAOETargets, NumRangedAOETargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        (BestChainsawTargets, NumSawTargets) = GetBestTarget(primaryTarget, 25, Is25yRectTarget);
        NumFlamethrowerTargets = Hints.NumPriorityTargetsInAOECone(Player.Position, 12, Player.Rotation.ToDirection(), 45.Degrees());
        ShouldUseAOE = Unlocked(AID.SpreadShot) && NumAOETargets > 1;
        ShouldUseRangedAOE = Unlocked(AID.Ricochet) && NumRangedAOETargets > 1;
        ShouldUseSaw = Unlocked(AID.ChainSaw) && NumSawTargets > 1;
        ShouldFlamethrower = Unlocked(AID.Flamethrower) && NumFlamethrowerTargets > 2;
        BestAOETarget = ShouldUseAOE ? BestAOETargets : primaryTarget;
        BestRangedAOETarget = ShouldUseRangedAOE ? BestRangedAOETargets : primaryTarget;
        BestChainsawTarget = ShouldUseSaw ? BestChainsawTargets : primaryTarget;
        BestFlamethrowerTarget = ShouldFlamethrower ? BestAOETarget : primaryTarget;

        CanHC = ActionReady(AID.Hypercharge) && Heat >= 50 && !OverheatActive;
        CanHB = Unlocked(AID.HeatBlast) && OverheatActive;
        CanGR = Unlocked(BestGauss) && ChargeCD(BestGauss) < 0.6f;
        CanRicochet = Unlocked(BestRicochet) && ChargeCD(BestRicochet) < 0.6f;

        CanSummon = Unlocked(AID.RookAutoturret) && Battery >= 90;

        CanWF = ActionReady(AID.Wildfire);
        CanBS = ActionReady(AID.BarrelStabilizer);
        CanReassemble = Unlocked(AID.Reassemble) && ChargeCD(AID.Reassemble) < 0.6f && !OverheatActive;

        CanDrill = Unlocked(AID.Drill) && ChargeCD(AID.Drill) < 0.6f && !OverheatActive;
        CanAA = ActionReady(BestAirAnchor) && !OverheatActive;
        CanChainsaw = ActionReady(AID.ChainSaw) && !OverheatActive;
        CanExcavate = Unlocked(AID.Excavator) && PlayerHasEffect(SID.ExcavatorReady) && !OverheatActive;
        CanFMF = Unlocked(AID.FullMetalField) && PlayerHasEffect(SID.FullMetalMachinist) && !OverheatActive;

        #region Strategy Definitions
        var AOE = strategy.Option(SharedTrack.AOE); //Retrieves AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //Retrieves AOE strategy
        var pot = strategy.Option(Track.Potion); //Retrieves Potion track
        var potStrat = pot.As<PotionStrategy>(); //Retrieves Potion strategy
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotation
        if (AOEStrategy == AOEStrategy.AutoFinish)
            QueueGCD(AutoFinish, TargetChoice(AOE) ?? BestAOETarget?.Actor, GCDPriority.Low);
        if (AOEStrategy == AOEStrategy.AutoBreak)
            QueueGCD(AutoBreak, TargetChoice(AOE) ?? BestAOETarget?.Actor, GCDPriority.Low);
        if (AOEStrategy == AOEStrategy.ForceST)
            QueueGCD(ST, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Low);
        if (AOEStrategy == AOEStrategy.ForceAOE)
            QueueGCD(BestSpreadShot, TargetChoice(AOE) ?? BestAOETarget?.Actor, GCDPriority.Low);
        #endregion

        #region Cooldowns
        if (!strategy.HoldAll())
        {
            if (!strategy.HoldCDs())
            {
                if (!strategy.HoldBuffs())
                {
                    //Wildfire & Detonator
                    var wf = strategy.Option(Track.Wildfire);
                    var wfStrat = wf.As<WildfireStrategy>();
                    if (ShouldUseWildfire(wfStrat, primaryTarget?.Actor))
                    {
                        if (wfStrat == WildfireStrategy.End)
                            QueueOGCD(AID.Detonator, TargetChoice(wf) ?? primaryTarget?.Actor, OGCDPriority.Max);
                        else
                            QueueOGCD(AID.Wildfire, TargetChoice(wf) ?? primaryTarget?.Actor, wfStrat is WildfireStrategy.Force or WildfireStrategy.ForceWeave ? OGCDPriority.Forced : OGCDPriority.VeryHigh);
                    }

                    //Barrel Stabilizer
                    var bs = strategy.Option(Track.BarrelStabilizer);
                    var bsStrat = bs.As<OGCDStrategy>();
                    if (ShouldUseBarrelStabilizer(bsStrat, primaryTarget?.Actor))
                        QueueOGCD(AID.BarrelStabilizer, TargetChoice(bs) ?? primaryTarget?.Actor, OGCDPrio(bsStrat, OGCDPriority.Average));

                    //Reassemble
                    var assemble = strategy.Option(Track.Reassemble);
                    var assembleStrat = assemble.As<ReassembleStrategy>();
                    if (ShouldUseReassemble(assembleStrat))
                        QueueOGCD(AID.Reassemble, Player, OGCDPriority.VeryHigh);
                }

                //Gauss Round & Double Check
                var gauss = strategy.Option(Track.GaussRound);
                var gaussStrat = gauss.As<OGCDStrategy>();
                if (ShouldUseGaussRound(gaussStrat, primaryTarget?.Actor))
                    QueueOGCD(BestGauss, TargetChoice(gauss) ?? (Unlocked(AID.DoubleCheck) ? BestRangedAOETarget?.Actor : primaryTarget?.Actor), OGCDPrio(gaussStrat, GaussAndRico(BestGauss)));

                //Ricochet & Checkmate
                var ricochet = strategy.Option(Track.Ricochet);
                var ricochetStrat = ricochet.As<OGCDStrategy>();
                if (ShouldUseRicochet(ricochetStrat, primaryTarget?.Actor))
                    QueueOGCD(BestRicochet, TargetChoice(ricochet) ?? BestRangedAOETarget?.Actor, OGCDPrio(ricochetStrat, GaussAndRico(BestRicochet)));

                //Drill & Bioblaster
                var drill = strategy.Option(Track.Drill);
                var drillStrat = drill.As<DrillStrategy>();
                if (ShouldUseDrill(drillStrat, primaryTarget?.Actor))
                {
                    if (drillStrat is DrillStrategy.Automatic)
                        QueueGCD(BestDrill, TargetChoice(drill) ?? BestAOETarget?.Actor, GCDPriority.High);
                    if (drillStrat is DrillStrategy.OnlyDrill or DrillStrategy.ForceDrill)
                        QueueGCD(AID.Drill, TargetChoice(drill) ?? BestAOETarget?.Actor, drillStrat == DrillStrategy.ForceDrill ? GCDPriority.Forced : GCDPriority.High);
                    if (drillStrat is DrillStrategy.OnlyBioblaster or DrillStrategy.ForceBioblaster)
                        QueueGCD(AID.Bioblaster, TargetChoice(drill) ?? BestAOETarget?.Actor, drillStrat == DrillStrategy.ForceBioblaster ? GCDPriority.Forced : GCDPriority.High);
                }

                //Flamethrower
                var ft = strategy.Option(Track.Flamethrower);
                var ftStrat = ft.As<GCDStrategy>();
                if (ShouldUseFlamethrower(ftStrat, primaryTarget?.Actor))
                    QueueGCD(AID.Flamethrower, TargetChoice(ft) ?? BestFlamethrowerTarget?.Actor, ftStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.ModeratelyLow);

                //Excavator
                var excavator = strategy.Option(Track.Excavator);
                var excavatorStrat = excavator.As<GCDStrategy>();
                if (ShouldUseExcavator(excavatorStrat, primaryTarget?.Actor))
                    QueueGCD(AID.Excavator, TargetChoice(excavator) ?? BestRangedAOETarget?.Actor, excavatorStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.High);

                //Full Metal Field
                var fmf = strategy.Option(Track.FullMetalField);
                var fmfStrat = fmf.As<GCDStrategy>();
                if (ShouldUseFullMetalField(fmfStrat, primaryTarget?.Actor))
                    QueueGCD(AID.FullMetalField, TargetChoice(fmf) ?? BestRangedAOETarget?.Actor, fmfStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.High);

                //Chainsaw
                var cs = strategy.Option(Track.Chainsaw);
                var csStrat = cs.As<GCDStrategy>();
                if (ShouldUseChainsaw(csStrat, primaryTarget?.Actor))
                    QueueGCD(AID.ChainSaw, TargetChoice(cs) ?? BestChainsawTarget?.Actor, csStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.ExtremelyHigh);

                //Hot Shot & Air Anchor
                var aa = strategy.Option(Track.AirAnchor);
                var aaStrat = aa.As<GCDStrategy>();
                if (ShouldUseAirAnchor(aaStrat, primaryTarget?.Actor))
                    QueueGCD(BestAirAnchor, TargetChoice(aa) ?? BestAOETarget?.Actor, aaStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.VeryHigh);

                //Hypercharge
                var hc = strategy.Option(Track.Hypercharge);
                var hcStrat = hc.As<HyperchargeStrategy>();
                if (ShouldUseHypercharge(hcStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.Hypercharge, TargetChoice(hc) ?? primaryTarget?.Actor, hcStrat == HyperchargeStrategy.ASAP ? OGCDPriority.Forced : Heat >= 100 ? OGCDPriority.Critical : OGCDPriority.High);

                //Heat Blast, Blazing Shot, & Auto-Crossbow
                var hsp = strategy.Option(Track.HeatSpender);
                var hspStrat = hsp.As<HeatSpenderStrategy>();
                if (ShouldChooseHeat(hspStrat, primaryTarget?.Actor))
                {
                    if (hspStrat == HeatSpenderStrategy.Automatic)
                        QueueGCD(BestHeatSpender, TargetChoice(hsp) ?? BestAOETarget?.Actor, GCDPriority.High);
                    if (hspStrat == HeatSpenderStrategy.OnlyHeatBlast)
                        QueueGCD(BestHeatBlast, TargetChoice(hsp) ?? BestAOETarget?.Actor, GCDPriority.High);
                    if (hspStrat == HeatSpenderStrategy.OnlyAutoCrossbow)
                        QueueGCD(Unlocked(AID.AutoCrossbow) ? AID.AutoCrossbow : BestHeatBlast, TargetChoice(hsp) ?? BestAOETarget?.Actor, GCDPriority.High);
                }
            }
        }
        if (ShouldUsePotion(potStrat))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, 0, GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        if (primaryTarget != null)
        {
            var aoebreakpoint = OverheatActive && Unlocked(AID.AutoCrossbow) ? 4 : 3;
            GoalZoneCombined(strategy, 25, Hints.GoalAOECone(primaryTarget.Actor, 12, 60.Degrees()), AID.SpreadShot, aoebreakpoint);
        }
        #endregion
    }

    public OGCDPriority GaussAndRico(AID aid)
    {
        var maxCharges = Unlocked(TraitID.ChargedActionMastery) ? 3 : 2;
        var chargesAvailable = Math.Clamp(maxCharges - (int)Math.Ceiling(TotalCD(aid) / 30), 0, maxCharges);

        return chargesAvailable switch
        {
            3 => OGCDPriority.High,
            2 => OGCDPriority.Average,
            1 => OGCDPriority.Low,
            _ => OGCDPriority.ExtremelyLow
        };
    }
}
