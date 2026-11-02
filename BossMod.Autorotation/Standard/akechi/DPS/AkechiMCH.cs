using BossMod.MCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiMCH(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Opener, Heat, Battery, Reassemble, Hypercharge, Drill, Wildfire, BarrelStabilizer, AirAnchor, ChainSaw, GaussRound, DoubleCheck, Ricochet, Checkmate, Flamethrower, Excavator, FullMetalField }
    public enum AOEStrategy { AutoFinish, AutoBreak, ForceST, ForceAOEFinish, ForceAOEBreak }
    public enum OpenerOption { AirAnchor, Drill, ChainSaw }
    public enum HeatOption { Automatic, OnlyHeatBlast, OnlyAutoCrossbow }
    public enum BatteryStrategy { Automatic, Fifty, Hundred, RaidBuffs, End, Delay }
    public enum ReassembleStrategy { Automatic, Any, HoldOne, Force, ForceWeave, Delay }
    public enum HyperchargeStrategy { Automatic, ASAP, Full, Delay }
    public enum DrillStrategy { Automatic, OnlyDrill, OnlyBioblaster, ForceDrill, ForceBioblaster, Delay }
    public enum WildfireStrategy { Automatic, AlignWithBurst, Force, ForceWeave, End, Delay }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi MCH", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.MCH), 100);

        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionDex);

        res.Define(Track.AOE).As<AOEStrategy>("ST/AOE", "Single-Target & AoE Rotations", 300)
            .AddOption(AOEStrategy.AutoFinish, "Automatically select best rotation based on targets nearby - finishes current combo if possible")
            .AddOption(AOEStrategy.AutoBreak, "Automatically select best rotation based on targets nearby - will break current combo if in one")
            .AddOption(AOEStrategy.ForceST, "Force Single-Target rotation, regardless of targets nearby")
            .AddOption(AOEStrategy.ForceAOEFinish, "Force AoE rotation, regardless of targets nearby - finishes current combo if possible")
            .AddOption(AOEStrategy.ForceAOEBreak, "Force AoE rotation, regardless of targets nearby - will break current combo if in one")
            .AddAssociatedActions(
                AID.SplitShot, AID.SlugShot, AID.CleanShot,
                AID.HeatedSplitShot, AID.HeatedSlugShot, AID.HeatedCleanShot,
                AID.SpreadShot, AID.Scattergun);

        res.Define(Track.Opener).As<OpenerOption>("Opener", "Opener", 199)
            .AddOption(OpenerOption.AirAnchor, "Use Hot Shot / Air Anchor as first tool in opener", minLevel: 4)
            .AddOption(OpenerOption.Drill, "Use Drill as first tool in opener", minLevel: 58)
            .AddOption(OpenerOption.ChainSaw, "Use Chain Saw as first tool in opener", minLevel: 90);

        res.Define(Track.Heat).As<HeatOption>("Heat Option", "Heat", 198)
            .AddOption(HeatOption.Automatic, "Automatically use Heat Blast or Auto-Crossbow based on targets nearby")
            .AddOption(HeatOption.OnlyHeatBlast, "Only use Heat Blast, regardless of targets", 0, 0, ActionTargets.Hostile, 35)
            .AddOption(HeatOption.OnlyAutoCrossbow, "Only use Auto Crossbow, regardless of targets", 0, 0, ActionTargets.Hostile, 52)
            .AddAssociatedActions(AID.HeatBlast, AID.AutoCrossbow, AID.BlazingShot);

        res.Define(Track.Battery).As<BatteryStrategy>("Battery", "", 189)
            .AddOption(BatteryStrategy.Automatic, "Use Battery actions when optimal")
            .AddOption(BatteryStrategy.Fifty, "Use Battery actions ASAP when 50+ Battery Gauge is available", minLevel: 40)
            .AddOption(BatteryStrategy.Hundred, "Use Battery actions ASAP when 100 Battery Gauge is available", minLevel: 40)
            .AddOption(BatteryStrategy.RaidBuffs, "Use Battery actions ASAP when raid buffs are active", minLevel: 40)
            .AddOption(BatteryStrategy.End, "Ends Battery action ASAP with Overdrive (assuming it's currently active)", minLevel: 40)
            .AddOption(BatteryStrategy.Delay, "Delay use of Battery actions", minLevel: 40)
            .AddAssociatedActions(AID.RookAutoturret, AID.RookOverdrive, AID.AutomatonQueen, AID.QueenOverdrive);

        res.Define(Track.Reassemble).As<ReassembleStrategy>("Reassemble", "R.semble", 184)
            .AddOption(ReassembleStrategy.Automatic, "Use Reassemble when optimal")
            .AddOption(ReassembleStrategy.Any, "Use Reassemble when any tool is available; uses both charges")
            .AddOption(ReassembleStrategy.HoldOne, "Use Reassemble when any tool is available; holds one charge for manual usage")
            .AddOption(ReassembleStrategy.Force, "Force use of Reassemble, regardless of weaving", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.ForceWeave, "Force use of Reassemble in next possible weave window", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.Delay, "Delay use of Reassemble", minLevel: 10)
            .AddAssociatedActions(AID.Reassemble);

        res.Define(Track.Hypercharge).As<HyperchargeStrategy>("Hypercharge", "H.charge", 190)
            .AddOption(HyperchargeStrategy.Automatic, "Use Hypercharge when optimal")
            .AddOption(HyperchargeStrategy.ASAP, "Use Hypercharge ASAP (if any Heat Gauge is available)", 0, 10, ActionTargets.Self, 30)
            .AddOption(HyperchargeStrategy.Full, "Use Hypercharge when Heat Gauge is full (or about to be)", 0, 10, ActionTargets.Self, 30)
            .AddOption(HyperchargeStrategy.Delay, "Delay use of Hypercharge", minLevel: 30)
            .AddAssociatedActions(AID.Hypercharge);

        res.Define(Track.Drill).As<DrillStrategy>("Drill", "", 179)
            .AddOption(DrillStrategy.Automatic, "Automatically use Drill or Bioblaster based on targets nearby; uses both charges")
            .AddOption(DrillStrategy.OnlyDrill, "Only use Drill, regardless of targets", minLevel: 58)
            .AddOption(DrillStrategy.OnlyBioblaster, "Only use Bioblaster, regardless of targets", minLevel: 72)
            .AddOption(DrillStrategy.ForceDrill, "Force use of Drill", 20, 0, ActionTargets.Hostile, 58)
            .AddOption(DrillStrategy.ForceBioblaster, "Force use of Bioblaster", 20, 15, ActionTargets.Hostile, 72)
            .AddOption(DrillStrategy.Delay, "Delay use of Drill", minLevel: 58)
            .AddAssociatedActions(AID.Drill, AID.Bioblaster);

        res.Define(Track.Wildfire).As<WildfireStrategy>("Wildfire", "W.fire", 183)
            .AddOption(WildfireStrategy.Automatic, "Use Wildfire when optimal")
            .AddOption(WildfireStrategy.AlignWithBurst, "Use when optimal; attempts to keep it aligned with burst windows")
            .AddOption(WildfireStrategy.Force, "Force use of Wildfire, regardless of weaving", 120, 10, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.ForceWeave, "Force use of Wildfire in next possible weave window", 120, 10, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.End, "End Wildfire early with Detonator", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.Delay, "Delay use of Wildfire", minLevel: 45)
            .AddAssociatedActions(AID.Wildfire, AID.Detonator);

        res.DefineOGCD(Track.BarrelStabilizer, AID.BarrelStabilizer, "Barrel Stabilizer", "B.Stab.", 185, 120, 30, ActionTargets.Self, 66).AddAssociatedActions(AID.BarrelStabilizer);
        res.DefineGCD(Track.AirAnchor, AID.AirAnchor, "Air Anchor", "A.Anchor", 180, 40, 0, ActionTargets.Hostile, 76).AddAssociatedActions(AID.AirAnchor);
        res.DefineGCD(Track.ChainSaw, AID.ChainSaw, "ChainSaw", "C.saw", 178, 60, 30, ActionTargets.Hostile, 90).AddAssociatedActions(AID.ChainSaw);
        res.DefineOGCD(Track.GaussRound, AID.GaussRound, "Gauss Round", "G.Round", 145, 30, 0, ActionTargets.Hostile, 15, 91).AddAssociatedActions(AID.GaussRound);
        res.DefineOGCD(Track.DoubleCheck, AID.DoubleCheck, "Double Check", "D.Check", 144, 30, 0, ActionTargets.Hostile, 92).AddAssociatedActions(AID.DoubleCheck);
        res.DefineOGCD(Track.Ricochet, AID.Ricochet, "Ricochet", "", 141, 30, 0, ActionTargets.Hostile, 50, 91).AddAssociatedActions(AID.Ricochet);
        res.DefineOGCD(Track.Checkmate, AID.Checkmate, "Checkmate", "C.mate", 140, 30, 0, ActionTargets.Hostile, 92).AddAssociatedActions(AID.Checkmate);
        res.DefineAllow(Track.Flamethrower, AID.Flamethrower, "Flamethrower", "F.thrower", -1, 60, 0, ActionTargets.Self, 70).AddAssociatedActions(AID.Flamethrower);
        res.DefineGCD(Track.Excavator, AID.Excavator, "Excavator", "Excav.", 177, 0, 0, ActionTargets.Hostile, 96).AddAssociatedActions(AID.Excavator);
        res.DefineGCD(Track.FullMetalField, AID.FullMetalField, "Full Metal Field", "FM.Field", 176, 0, 0, ActionTargets.Hostile, 100).AddAssociatedActions(AID.FullMetalField);

        return res;
    }

    private MachinistGauge Gauge => World.Client.GetGauge<MachinistGauge>();
    private int Heat => Gauge.Heat;
    private int Battery => Gauge.Battery;
    private bool QueenActive => Gauge.SummonTimeRemaining != 0;
    private bool Overheated => HasStatus(SID.Overheated);
    private bool Reassembled => HasStatus(SID.Reassembled);
    private (float CD, float Left) WF => (Cooldown(AID.Wildfire), Status(SID.WildfirePlayer, 10));
    private float BSCD => Cooldown(AID.BarrelStabilizer);
    private float DrillCD => Cooldown(AID.Drill);
    private float AACD => Cooldown(BestAirAnchor);
    private float CSCD => Cooldown(AID.ChainSaw);
    private float EVLeft => Status(SID.ExcavatorReady, 30);
    private float FMFLeft => Status(SID.FullMetalMachinist, 30);
    private float FTLeft => Status(SID.Flamethrower, 10);
    private bool AnchorAndSawBusy => (!Unlocked(AID.AirAnchor) || AACD > 7.6f) && (!Unlocked(AID.ChainSaw) || CSCD > 7.6f);
    private bool ToolsBusy => AnchorAndSawBusy && (!Unlocked(AID.Drill) || DrillCD > 7.6f) && EVLeft == 0 && FMFLeft == 0;
    private bool PauseForFlamethrower => Service.Config.Get<MCHConfig>().PauseForFlamethrower && FTLeft > 0;

    private AID BestSplitShot => Unlocked(AID.HeatedSplitShot) ? AID.HeatedSplitShot : AID.SplitShot;
    private AID BestSlugShot => Unlocked(AID.HeatedSlugShot) ? AID.HeatedSlugShot : AID.SlugShot;
    private AID BestCleanShot => Unlocked(AID.HeatedCleanShot) ? AID.HeatedCleanShot : AID.CleanShot;
    private AID BestSpreadShot => Unlocked(AID.Scattergun) ? AID.Scattergun : AID.SpreadShot;
    private AID BestAirAnchor => Unlocked(AID.AirAnchor) ? AID.AirAnchor : AID.HotShot;
    private AID BestHeatBlast => Unlocked(AID.BlazingShot) ? AID.BlazingShot : Unlocked(AID.HeatBlast) ? AID.HeatBlast : BestSplitShot;
    private AID BestQueen => Unlocked(AID.AutomatonQueen) ? AID.AutomatonQueen : AID.RookAutoturret;
    private AID BestOverdrive => Unlocked(AID.QueenOverdrive) ? AID.QueenOverdrive : AID.RookOverdrive;

    private bool WantAOE;
    private int ConeTargets;
    private int FlamethrowerTargets;
    private Actor? BestConeTarget;
    private Actor? ConeTarget;
    private Actor? SplashTarget;
    private Actor? SawTarget;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var auto = strategy.AutoTarget();
        var forceAOE = strategy.Option(Track.AOE).As<AOEStrategy>() is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak;
        (var bestCone, ConeTargets) = GetBestTarget(primaryTarget, 12, Is12yConeTarget);
        var (bestSplash, splashTargets) = auto ? GetBestTarget(primaryTarget, 25, IsSplashTarget) : (primaryTarget, 0);
        var (bestSaw, sawTargets) = auto ? GetBestTarget(primaryTarget, 25, Is25yRectTarget) : (primaryTarget, 0);
        FlamethrowerTargets = Hints.NumPriorityTargetsInAOECone(Player.Position, 12, Player.Rotation.ToDirection(), 45.Degrees());
        WantAOE = Unlocked(AID.SpreadShot) && (forceAOE || (auto ? ConeTargets > 2 : FlamethrowerTargets > 1));
        BestConeTarget = bestCone?.Actor;
        ConeTarget = WantAOE ? BestConeTarget : PrimaryTarget;
        SplashTarget = Unlocked(AID.Ricochet) && splashTargets > 1 ? bestSplash?.Actor : PrimaryTarget;
        SawTarget = Unlocked(AID.ChainSaw) && sawTargets > 1 ? bestSaw?.Actor : PrimaryTarget;
        var channeling = PauseForFlamethrower && LastActionUsed(AID.Flamethrower) && strategy.Option(Track.Flamethrower).As<AllowOrForbid>() != AllowOrForbid.Forbid;

        if (strategy.HoldEverything() || channeling)
            return;

        QueueOpener(strategy);
        if (strategy.AllowBuffs())
        {
            QueueWildfire(strategy);
            QueueBarrelStabilizer(strategy);
            QueueReassemble(strategy);
        }
        if (strategy.AllowCDs())
        {
            QueueTool(strategy, Track.AirAnchor, BestAirAnchor, null, ActionReady(BestAirAnchor), true, GCDPriority.ExtremelyHigh + 10);
            QueueTool(strategy, Track.ChainSaw, AID.ChainSaw, SawTarget, ActionReady(AID.ChainSaw), true, GCDPriority.ExtremelyHigh + 2);
            QueueTool(strategy, Track.Excavator, AID.Excavator, SplashTarget, Unlocked(AID.Excavator) && EVLeft > 0, !Overheated, GCDPriority.ExtremelyHigh + 5);
            QueueTool(strategy, Track.FullMetalField, AID.FullMetalField, SplashTarget, Unlocked(AID.FullMetalField) && FMFLeft > 0, EVLeft == 0 && AnchorAndSawBusy, GCDPriority.High + 1);
            QueueDrill(strategy);
            QueueCharges(strategy, AID.GaussRound, AID.DoubleCheck, Track.GaussRound, Track.DoubleCheck, Unlocked(AID.DoubleCheck) ? SplashTarget : PrimaryTarget);
            QueueCharges(strategy, AID.Ricochet, AID.Checkmate, Track.Ricochet, Track.Checkmate, SplashTarget);
            QueueFlamethrower(strategy);
        }
        if (strategy.AllowGauge())
        {
            QueueHypercharge(strategy);
            QueueBattery(strategy);
        }
        if (strategy.AllowAbilities())
        {
            QueueHeat(strategy);
            if (CountdownRemaining is not > 1.99f)
                QueuePotion(strategy, ActionDefinitions.IDPotionDex, BSCD <= 6f, ActionQueue.Priority.Medium);
        }
        QueueCombo(strategy);

        if (primaryTarget != null)
            GoalZoneCombined(strategy, 25, Hints.GoalAOECone(primaryTarget.Actor, 12, 60.Degrees()), AID.SpreadShot, Overheated && Unlocked(AID.AutoCrossbow) ? 4 : 2);
    }

    private AID OpenerTool(StrategyValues strategy) => strategy.Option(Track.Opener).As<OpenerOption>() switch
    {
        OpenerOption.ChainSaw when Unlocked(AID.ChainSaw) => AID.ChainSaw,
        OpenerOption.Drill when Unlocked(AID.Drill) => AID.Drill,
        _ => BestAirAnchor
    };
    private void QueueOpener(StrategyValues strategy)
    {
        var pulling = !Player.InCombat && In25y(PrimaryTarget);
        var toolTime = CountdownRemaining is { } countdown ? countdown < 1.15f : Reassembled || !ActionReady(AID.Reassemble);
        if (pulling && CountdownRemaining is not >= 5f && !Reassembled && ActionReady(AID.Reassemble))
            QueueOGCD(AID.Reassemble, Player, OGCDPriority.Max);
        if (pulling && toolTime)
            QueueGCD(OpenerTool(strategy), PrimaryTarget, GCDPriority.Max);
    }
    private void QueueCombo(StrategyValues strategy)
    {
        var track = strategy.Option(Track.AOE);
        var next = ComboLastMove switch
        {
            AID.SplitShot or AID.HeatedSplitShot => BestSlugShot,
            AID.SlugShot or AID.HeatedSlugShot => BestCleanShot,
            _ => AID.None
        };
        var single = next != AID.None ? next : BestSplitShot;
        var action = track.As<AOEStrategy>() switch
        {
            AOEStrategy.AutoFinish => next != AID.None ? next : WantAOE ? BestSpreadShot : BestSplitShot,
            AOEStrategy.AutoBreak => WantAOE ? BestSpreadShot : single,
            AOEStrategy.ForceST => single,
            AOEStrategy.ForceAOEFinish => next != AID.None ? next : BestSpreadShot,
            AOEStrategy.ForceAOEBreak => BestSpreadShot,
            _ => AID.None
        };
        var target = action == BestSpreadShot ? AOETargetChoice(PrimaryTarget, BestConeTarget, track, strategy) : SingleTargetChoice(PrimaryTarget, track);
        var priority = CombatTimer > 90 && ComboTimer is < 8f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low;
        if (!Overheated && CountdownRemaining is not > 0f)
            QueueGCD(action, target, priority);
    }
    private void QueueWildfire(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Wildfire);
        var wildfire = track.As<WildfireStrategy>();
        var ready = ActionReady(AID.Wildfire);
        var toolsUsed = (CombatTimer < 60 ? FMFLeft > 0 : FMFLeft == 0) && AnchorAndSawBusy && EVLeft == 0;
        var optimal = ready && InCombat(PrimaryTarget) && (toolsUsed || LastActionUsed(AID.Hypercharge) || Overheated);
        var (condition, priority) = wildfire switch
        {
            WildfireStrategy.Automatic => (optimal, ChangePriority(-200, 999)),
            WildfireStrategy.AlignWithBurst => (optimal && BSCD > 90, ChangePriority(-200, 999)),
            WildfireStrategy.Force => (ready, OGCDPriority.Forced),
            WildfireStrategy.ForceWeave => (ready && CanWeaveIn, OGCDPriority.Forced),
            WildfireStrategy.End => (WF.Left > 0, OGCDPriority.Max),
            _ => (false, OGCDPriority.None)
        };
        var detonate = wildfire == WildfireStrategy.End;
        if (condition)
            QueueOGCD(detonate ? AID.Detonator : AID.Wildfire, detonate ? Player : SingleTargetChoice(PrimaryTarget, track), priority);
    }
    private void QueueBarrelStabilizer(StrategyValues strategy)
    {
        var (condition, priority) = strategy.Option(Track.BarrelStabilizer).As<OGCDStrategy>() switch
        {
            OGCDStrategy.Automatic => (InCombat(PrimaryTarget) && CanWeaveIn, OGCDPriority.Max - 1),
            OGCDStrategy.AnyWeave => (CanWeaveIn, OGCDPriority.Forced),
            OGCDStrategy.EarlyWeave => (CanEarlyWeaveIn, OGCDPriority.Forced),
            OGCDStrategy.LateWeave => (CanLateWeaveIn, OGCDPriority.Forced),
            OGCDStrategy.Force => (true, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
        if (condition && ActionReady(AID.BarrelStabilizer))
            QueueOGCD(AID.BarrelStabilizer, Player, priority);
    }
    private void QueueReassemble(StrategyValues strategy)
    {
        var charges = Cooldown(AID.Reassemble);
        var ready = Unlocked(AID.Reassemble) && charges <= 57f && !Overheated && !Reassembled;
        var weave = Player.InCombat && CanWeaveIn;
        var toolNext = AACD <= GCD || CSCD <= GCD || EVLeft > GCD || NextGCD == AID.Drill;
        var optimal = weave && EVLeft > GCD || BSCD is < 110 and > 90 && !Overheated && NextGCD == AID.Drill;
        var (condition, priority) = strategy.Option(Track.Reassemble).As<ReassembleStrategy>() switch
        {
            ReassembleStrategy.Automatic => (optimal || weave && toolNext && charges < 15, OGCDPriority.High),
            ReassembleStrategy.Any => (weave && toolNext, OGCDPriority.High),
            ReassembleStrategy.HoldOne => (weave && toolNext && charges < 20, OGCDPriority.High),
            ReassembleStrategy.Force => (true, OGCDPriority.Forced),
            ReassembleStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
        if (condition && ready)
            QueueOGCD(AID.Reassemble, Player, priority);
    }
    private void QueueTool(StrategyValues strategy, Track track, AID action, Actor? splashTarget, bool ready, bool optimal, GCDPriority priority)
    {
        var option = strategy.Option(track);
        var tool = option.As<GCDStrategy>();
        var target = splashTarget != null ? AOETargetChoice(PrimaryTarget, splashTarget, option, strategy) : SingleTargetChoice(PrimaryTarget, option);
        if (ShouldUseGCD(tool, target, ready, InCombat(PrimaryTarget) && optimal))
            QueueGCD(action, target, tool == GCDStrategy.Force ? GCDPriority.Forced : priority);
    }
    private void QueueDrill(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Drill);
        var drill = track.As<DrillStrategy>();
        var drillReady = Unlocked(AID.Drill) && DrillCD < 0.5f + (Unlocked(TraitID.EnhancedMultiweapon) ? SkSGCDLength * 8 : 0);
        var bioblasterReady = ActionReady(AID.Bioblaster);
        var engaged = InCombat(PrimaryTarget);
        var optimal = WantAOE ? engaged && bioblasterReady && In12y(PrimaryTarget) : engaged && drillReady && (ActionReady(AID.Drill) || !Overheated);
        var automatic = DrillCD <= GCD ? GCDPriority.ExtremelyHigh + 9 : CanFitSkSGCD(WF.Left) && FMFLeft == 0 ? GCDPriority.High + 2 : GCDPriority.High;
        var (condition, priority) = drill switch
        {
            DrillStrategy.Automatic or DrillStrategy.OnlyDrill or DrillStrategy.OnlyBioblaster => (optimal, automatic),
            DrillStrategy.ForceDrill => (drillReady, GCDPriority.Forced),
            DrillStrategy.ForceBioblaster => (bioblasterReady, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
        var action = drill switch
        {
            DrillStrategy.Automatic => ConeTargets > 2 && Unlocked(AID.Bioblaster) ? AID.Bioblaster : AID.Drill,
            DrillStrategy.OnlyBioblaster or DrillStrategy.ForceBioblaster => AID.Bioblaster,
            _ => AID.Drill
        };
        var target = action == AID.Bioblaster ? AOETargetChoice(PrimaryTarget, ConeTarget, track, strategy) : SingleTargetChoice(PrimaryTarget, track);
        if (condition)
            QueueGCD(action, target, priority);
    }
    private void QueueCharges(StrategyValues strategy, AID basic, AID upgraded, Track basicTrack, Track upgradedTrack, Actor? autoTarget)
    {
        var isUpgraded = Unlocked(upgraded);
        var action = isUpgraded ? upgraded : basic;
        var option = strategy.Option(isUpgraded ? upgradedTrack : basicTrack);
        var charges = option.As<OGCDStrategy>();
        var cooldown = Cooldown(action);
        var dying = PrimaryTarget?.PendingHPRatio < 0.05f;
        var window = WF.Left > 0 || RaidBuffsLeft > 0 || Overheated || dying || !Unlocked(AID.Wildfire) || WF.CD > 1f || cooldown <= 0.6f;
        var priority = RaidBuffsLeft > 0 || WF.Left > 0 && Overheated || dying || cooldown <= 0.6f ? OGCDPriority.High
            : (WF.CD > 90 ? cooldown <= 30.6f : cooldown <= 60.6f) ? OGCDPriority.Average
            : OGCDPriority.Low;
        if (ShouldUseOGCD(charges, PrimaryTarget, Unlocked(basic), InCombat(PrimaryTarget) && CanWeaveIn && In25y(PrimaryTarget) && cooldown <= 30.6f && window))
            QueueOGCD(action, AOETargetChoice(PrimaryTarget, autoTarget, option, strategy), OGCDPrio(charges, priority));
    }
    private void QueueFlamethrower(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Flamethrower);
        var worthIt = FlamethrowerTargets >= 3 || FlamethrowerTargets == 2 && RaidBuffsLeft > GCD;
        var (condition, priority) = track.As<AllowOrForbid>() switch
        {
            AllowOrForbid.Allow => (worthIt && InCombat(PrimaryTarget) && In12y(PrimaryTarget), GCDPriority.ModeratelyLow),
            AllowOrForbid.Force => (true, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
        if (condition && ActionReady(AID.Flamethrower) && !Overheated && FTLeft == 0)
            QueueGCD(AID.Flamethrower, AOETargetChoice(PrimaryTarget, ConeTarget, track, strategy), priority);
    }
    private void QueueHypercharge(StrategyValues strategy)
    {
        var ready = ActionReady(AID.Hypercharge) && (Heat >= 50 || Status(SID.Hypercharged) > GCD) && !Reassembled && !Overheated && InCombat(PrimaryTarget);
        var afterOddTools = LastActionUsed(AID.Excavator) || CSCD > 50 && EVLeft == 0;
        var afterEvenTools = LastActionUsed(AID.FullMetalField) || BSCD > 90 && FMFLeft == 0;
        var offWildfire = !Unlocked(AID.Wildfire) || WF.CD > 40 || WF.CD <= 2f && FMFLeft == 0 || WF.Left > 0;
        var toolOnCooldown = Unlocked(BestAirAnchor) && AACD > GCD || Unlocked(AID.ChainSaw) && CSCD > GCD || Unlocked(AID.Drill) && DrillCD > GCD || Unlocked(AID.Excavator) && EVLeft == 0 || Unlocked(AID.FullMetalField) && FMFLeft == 0;
        var overcap = Heat == 100 && toolOnCooldown;
        var comboSafe = CombatTimer <= 30 || ComboTimer == 0 || ComboLastMove is AID.HeatedCleanShot or AID.Scattergun || ComboTimer >= 7.6f;
        var (condition, priority) = strategy.Option(Track.Hypercharge).As<HyperchargeStrategy>() switch
        {
            HyperchargeStrategy.Automatic => (ToolsBusy && comboSafe && (afterOddTools || afterEvenTools || offWildfire || overcap), OGCDPriority.Severe + 1),
            HyperchargeStrategy.ASAP => (true, OGCDPriority.Forced),
            HyperchargeStrategy.Full => (Heat == 100, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
        if (ready && condition)
            QueueOGCD(AID.Hypercharge, Player, priority);
    }
    private void QueueBattery(StrategyValues strategy)
    {
        var battery = strategy.Option(Track.Battery).As<BatteryStrategy>();
        var canSummon = Unlocked(AID.RookAutoturret) && Battery >= 50 && !QueenActive && Player.InCombat;
        var afterExcavator = LastActionUsed(AID.Excavator) || CSCD > 50 && EVLeft == 0;
        var afterAnchor = LastActionUsed(BestAirAnchor) || AACD > 36;
        var opener = CombatTimer < 90 && (CombatTimer < 30 ? afterExcavator : Battery >= 90);
        var overcap = Battery >= 90 && (AACD <= GCD || CSCD <= GCD || EVLeft > GCD) || Battery == 100 && ComboLastMove is AID.SlugShot or AID.HeatedSlugShot;
        var later = CombatTimer >= 90 && (BSCD > 45 && (afterAnchor || afterExcavator) || overcap);
        var condition = battery switch
        {
            BatteryStrategy.Automatic => canSummon && (opener || later),
            BatteryStrategy.Fifty => canSummon,
            BatteryStrategy.Hundred => canSummon && overcap,
            BatteryStrategy.RaidBuffs => canSummon && (RaidBuffsLeft > GCD || RaidBuffsIn < 5),
            BatteryStrategy.End => QueenActive,
            _ => false
        };
        var end = battery == BatteryStrategy.End;
        if (condition)
            QueueOGCD(end ? BestOverdrive : BestQueen, Player, end ? OGCDPriority.Critical : OGCDPriority.Severe);
    }
    private void QueueHeat(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Heat);
        var crossbow = Unlocked(AID.AutoCrossbow) ? AID.AutoCrossbow : BestHeatBlast;
        var autoCrossbow = ConeTargets >= 6 && Unlocked(AID.AutoCrossbow);
        var (condition, action, target) = track.As<HeatOption>() switch
        {
            HeatOption.Automatic => (InCombat(PrimaryTarget) && (WantAOE ? In12y(PrimaryTarget) : In25y(PrimaryTarget)), autoCrossbow ? AID.AutoCrossbow : BestHeatBlast, AOETargetChoice(PrimaryTarget, autoCrossbow ? BestConeTarget : PrimaryTarget, track, strategy)),
            HeatOption.OnlyHeatBlast => (In25y(PrimaryTarget), BestHeatBlast, SingleTargetChoice(PrimaryTarget, track)),
            HeatOption.OnlyAutoCrossbow => (In12y(PrimaryTarget), crossbow, AOETargetChoice(PrimaryTarget, BestConeTarget, track, strategy)),
            _ => (false, AID.None, null)
        };
        if (condition && Overheated && Unlocked(AID.HeatBlast))
            QueueGCD(action, target, GCDPriority.High);
    }
}
