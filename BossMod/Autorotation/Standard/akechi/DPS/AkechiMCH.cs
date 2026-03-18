using BossMod.MCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

//TODO: cleanup this file - it works ok now (i think?), but it's a fucking mess to look at
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

    private int Heat;
    private int Battery;
    private bool OverheatActive;
    private bool MinionActive;
    private bool WantAOE;
    private bool ShouldUseRangedAOE;
    private bool ShouldUseSaw;
    private bool ShouldFlamethrower;
    private int NumConeTargets;
    private int NumSplashTargets;
    private int NumChainSawTargets;
    private int NumFlamethrowerTargets;
    private Enemy? BestConeTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestChainSawTargets;
    private Enemy? BestConeTarget;
    private Enemy? BestSplashTarget;
    private Enemy? BestChainSawTarget;
    private Enemy? BestFlamethrowerTarget;
    private bool ForceAOE;

    public float RAleft => StatusRemaining(Player, SID.Reassembled);
    public float HCleft => StatusRemaining(Player, SID.Hypercharged);
    public float WFleft => StatusRemaining(Player, SID.WildfirePlayer);
    public float EVleft => StatusRemaining(Player, SID.ExcavatorReady);
    public float FMFleft => StatusRemaining(Player, SID.FullMetalMachinist);
    public float FTleft => StatusRemaining(Player, SID.Flamethrower);
    public float BScd => Cooldown(AID.BarrelStabilizer);
    public float Drillcd => Cooldown(AID.Drill);
    public float AAcd => Unlocked(AID.AirAnchor) ? Cooldown(AID.AirAnchor) : Cooldown(AID.HotShot);
    public float CScd => Cooldown(AID.ChainSaw);
    public bool Drillsafe => !Unlocked(AID.Drill) || Drillcd > 7.6f;
    public bool AAsafe => !Unlocked(AID.AirAnchor) || AAcd > 7.6f;
    public bool CSsafe => !Unlocked(AID.ChainSaw) || CScd > 7.6f;
    public bool EVsafe => !Unlocked(AID.Excavator) || EVleft == 0;
    public bool FMFsafe => !Unlocked(AID.FullMetalField) || FMFleft == 0;
    public bool CanHC => ActionReady(AID.Hypercharge) && (Heat >= 50 || HCleft > GCD);
    public bool CanHB => Unlocked(AID.HeatBlast) && OverheatActive;
    public bool CanSummon => Unlocked(AID.RookAutoturret) && Battery >= 50 && !MinionActive;
    public bool CanWF => ActionReady(AID.Wildfire);
    public bool CanBS => ActionReady(AID.BarrelStabilizer);
    public bool CanRA => Unlocked(AID.Reassemble) && Cooldown(AID.Reassemble) <= 57f && !OverheatActive && RAleft == 0;
    public bool CanDrill => Unlocked(AID.Drill) && (Unlocked(TraitID.EnhancedMultiweapon) ? Cooldown(AID.Drill) < 0.5f + (SkSGCDLength * 8) : Cooldown(AID.Drill) < 0.5f);
    public bool CanBB => ActionReady(AID.Bioblaster);
    public bool CanAA => ActionReady(BestAirAnchor);
    public bool CanCS => ActionReady(AID.ChainSaw);
    public bool CanEV => Unlocked(AID.Excavator) && EVleft > 0;
    public bool CanFMF => Unlocked(AID.FullMetalField) && FMFleft > 0;
    public bool CanFT => ActionReady(AID.Flamethrower) && !OverheatActive && FTleft == 0 && NumFlamethrowerTargets > 2;

    private AID AutoFinish => ComboLastMove switch
    {
        AID.SlugShot or AID.HeatedSlugShot => BestCleanShot,
        AID.SplitShot or AID.HeatedSplitShot => BestSlugShot,
        AID.CleanShot or AID.HeatedCleanShot or AID.Scattergun or AID.SpreadShot or _ => AutoBreak,
    };
    private AID ST => ComboLastMove switch
    {
        AID.SlugShot or AID.HeatedSlugShot => BestCleanShot,
        AID.SplitShot or AID.HeatedSplitShot => BestSlugShot,
        AID.CleanShot or AID.HeatedCleanShot or AID.Scattergun or AID.SpreadShot or _ => BestSplitShot,
    };
    private AID AutoBreak => WantAOE ? BestSpreadShot : ST;
    private AID AOEFinish => ComboLastMove switch
    {
        AID.SlugShot or AID.HeatedSlugShot => BestCleanShot,
        AID.SplitShot or AID.HeatedSplitShot => BestSlugShot,
        AID.CleanShot or AID.HeatedCleanShot or AID.Scattergun or AID.SpreadShot or _ => BestSpreadShot,
    };

    //private bool BreakCombo => ComboLastMove == AID.HeatedSlugShot ? NumConeTargets > 3 : ComboLastMove == AID.HeatedSplitShot ? NumConeTargets > 2 : NumConeTargets > 1;

    private AID BestSplitShot => Unlocked(AID.HeatedSplitShot) ? AID.HeatedSplitShot : AID.SplitShot;
    private AID BestSlugShot => Unlocked(AID.HeatedSlugShot) ? AID.HeatedSlugShot : AID.SlugShot;
    private AID BestCleanShot => Unlocked(AID.HeatedCleanShot) ? AID.HeatedCleanShot : AID.CleanShot;
    private AID BestDrill => NumConeTargets > 1 && Unlocked(AID.Bioblaster) ? AID.Bioblaster : AID.Drill;
    private AID BestHeat => NumConeTargets > 3 ? AID.AutoCrossbow : BestHeatBlast;
    private AID BestSpreadShot => Unlocked(AID.Scattergun) ? AID.Scattergun : AID.SpreadShot;
    private AID BestHeatBlast => Unlocked(AID.BlazingShot) ? AID.BlazingShot : Unlocked(AID.HeatBlast) ? AID.HeatBlast : ST;
    private AID BestGauss => Unlocked(AID.DoubleCheck) ? AID.DoubleCheck : AID.GaussRound;
    private AID BestRicochet => Unlocked(AID.Checkmate) ? AID.Checkmate : AID.Ricochet;
    private AID BestAirAnchor => Unlocked(AID.AirAnchor) ? AID.AirAnchor : AID.HotShot;

    #region Buffs

    private (bool, OGCDPriority) ShouldUseWildfire(WildfireStrategy strategy, Actor? target)
    {
        var condition =
            CanWF &&
            InCombat(target) &&
            ((CombatTimer < 60 && FMFleft > 0 && AAsafe && CSsafe && EVleft == 0) || //opener - after all tools except FMF
            (CombatTimer >= 60 && FMFleft == 0 && AAsafe && CSsafe && EVleft == 0) || //2m burst - after all tools
            LastActionUsed(AID.Hypercharge) || //if we just used Hypercharge
            OverheatActive); //last resort - send during Overheat
        return strategy switch
        {
            WildfireStrategy.Automatic => (condition, ChangePriority(-200, 999)),
            WildfireStrategy.AlignWithBurst => (BScd > 90 && condition, ChangePriority(-200, 999)),
            WildfireStrategy.End => (HasEffect(SID.WildfirePlayer), OGCDPriority.Max),
            WildfireStrategy.Force => (CanWF, OGCDPriority.Forced),
            WildfireStrategy.ForceWeave => (CanWF && CanWeaveIn, OGCDPriority.Forced),
            WildfireStrategy.Delay or _ => (false, OGCDPriority.None),
        };
    }
    private (bool, OGCDPriority) ShouldUseBarrelStabilizer(OGCDStrategy strategy, Actor? target)
    {
        if (!CanBS)
            return (false, OGCDPriority.None);
        return strategy switch
        {
            OGCDStrategy.Automatic => (CanWeaveIn, OGCDPriority.Max - 1),
            OGCDStrategy.AnyWeave => (CanWeaveIn, OGCDPriority.Forced),
            OGCDStrategy.EarlyWeave => (CanEarlyWeaveIn, OGCDPriority.Forced),
            OGCDStrategy.LateWeave => (CanLateWeaveIn, OGCDPriority.Forced),
            OGCDStrategy.Force => (true, OGCDPriority.Forced),
            OGCDStrategy.Delay or _ => (false, OGCDPriority.None),
        };
    }
    private (bool, OGCDPriority) ShouldUseReassemble(ReassembleStrategy strategy, Actor? target)
    {
        if (!CanRA)
            return (false, OGCDPriority.None);
        var opti = Player.InCombat && CanWeaveIn && EVleft > GCD || (BScd is < 110 and > 90 && !OverheatActive && NextGCD is AID.Drill);
        var any = Player.InCombat && CanWeaveIn && (AAcd <= GCD || CScd <= GCD || EVleft > GCD || NextGCD == AID.Drill);
        var risk = Cooldown(AID.Reassemble) < 15 && any;
        return strategy switch
        {
            ReassembleStrategy.Automatic => (opti || risk, OGCDPriority.High),
            ReassembleStrategy.Any => (any || risk, OGCDPriority.High),
            ReassembleStrategy.HoldOne => (Cooldown(AID.Reassemble) < 20 && any, OGCDPriority.High),
            ReassembleStrategy.Force or ReassembleStrategy.ForceWeave => (true, OGCDPriority.Forced),
            ReassembleStrategy.Delay or _ => (false, OGCDPriority.None),
        };
    }
    #endregion

    #region Tools
    private void Opener(OpenerOption opt, Actor? target)
    {
        if (CountdownRemaining == null || CombatTimer == 0 || !Player.InCombat)
        {
            var openerAction = opt switch
            {
                OpenerOption.ChainSaw => AID.ChainSaw,
                OpenerOption.Drill => AID.Drill,
                _ => AID.AirAnchor,
            };
            QueueGCD(openerAction, target, GCDPriority.Max);
            return;
        }
        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 1.15f)
            {
                if (opt == OpenerOption.AirAnchor || (opt == OpenerOption.ChainSaw && !Unlocked(AID.ChainSaw)) || (opt == OpenerOption.Drill && !Unlocked(AID.Drill)))
                    QueueGCD(BestAirAnchor, target, GCDPriority.VerySevere);
                if (opt == OpenerOption.Drill)
                    QueueGCD(AID.Drill, target, GCDPriority.VerySevere);
            }

            if (CountdownRemaining < 1.05f && opt == OpenerOption.ChainSaw)
                QueueGCD(AID.ChainSaw, target, GCDPriority.VerySevere);

            return;
        }
    }
    private (bool, GCDPriority) ShouldUseAirAnchor(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => (InCombat(target) && CanAA, GCDPriority.ExtremelyHigh + 10),
        GCDStrategy.Force => (CanAA, GCDPriority.Forced),
        GCDStrategy.Delay or _ => (false, GCDPriority.None),
    };
    private (bool, GCDPriority) ShouldUseDrill(DrillStrategy strategy, Actor? target)
    {
        var st = InCombat(target) && CanDrill && (ActionReady(AID.Drill) || !OverheatActive);
        var aoe = InCombat(target) && CanBB && In12y(target) && ActionReady(AID.Bioblaster);
        var prio = Cooldown(AID.Drill) <= GCD ? GCDPriority.ExtremelyHigh + 9 : CanFitSkSGCD(WFleft) && FMFleft == 0 ? GCDPriority.High + 2 : GCDPriority.High;
        return strategy switch
        {
            DrillStrategy.Automatic or DrillStrategy.OnlyDrill or DrillStrategy.OnlyBioblaster => (WantAOE ? aoe : st, prio),
            DrillStrategy.ForceDrill => (CanDrill, GCDPriority.Forced),
            DrillStrategy.ForceBioblaster => (CanBB, GCDPriority.Forced),
            DrillStrategy.Delay or _ => (false, GCDPriority.None),
        };
    }
    private (bool, GCDPriority) ShouldUseChainSaw(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => (InCombat(target) && CanCS, GCDPriority.ExtremelyHigh + 2),
        GCDStrategy.Force => (CanCS, GCDPriority.Forced),
        GCDStrategy.Delay or _ => (false, GCDPriority.None),
    };
    private (bool, GCDPriority) ShouldUseExcavator(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => (InCombat(target) && !OverheatActive && CanEV, GCDPriority.ExtremelyHigh + 5),
        GCDStrategy.Force => (CanEV, GCDPriority.Forced),
        GCDStrategy.Delay or _ => (false, GCDPriority.None),
    };
    private (bool, GCDPriority) ShouldUseFullMetalField(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => (InCombat(target) && CanFMF && EVleft == 0 && AAsafe && CSsafe, GCDPriority.High + 1),
        GCDStrategy.Force => (CanFMF, GCDPriority.Forced),
        GCDStrategy.Delay or _ => (false, GCDPriority.None),
    };
    #endregion

    #region Gauge
    private (bool, OGCDPriority) ShouldUseHypercharge(HyperchargeStrategy strategy, Actor? target)
    {
        if (!CanHC || RAleft > 0 || OverheatActive || target == null)
            return (false, OGCDPriority.None);
        var ok = AAsafe && CSsafe && Drillsafe && FMFsafe && EVsafe;
        var odd = LastActionUsed(AID.Excavator) || (CScd > 50 && EVleft == 0);
        var even = LastActionUsed(AID.FullMetalField) || (BScd > 90 && FMFleft == 0);
        var off = !Unlocked(AID.Wildfire) || (Unlocked(AID.Wildfire) && (Cooldown(AID.Wildfire) > 40 || (Cooldown(AID.Wildfire) <= 2f && FMFleft == 0) || WFleft > 0));
        var risk = Heat == 100 && ((Unlocked(BestAirAnchor) && AAcd > GCD) || (Unlocked(AID.ChainSaw) && CScd > GCD) || (Unlocked(AID.Drill) && Drillcd > GCD) || (Unlocked(AID.Excavator) && EVleft == 0) || (Unlocked(AID.FullMetalField) && FMFleft == 0));
        var ct = (CombatTimer <= 30 || ComboTimer == 0 || (ComboLastMove is AID.HeatedCleanShot or AID.Scattergun)) ? ComboTimer >= 0 : ComboTimer >= 7.6f;
        return strategy switch
        {
            HyperchargeStrategy.Automatic => ((ok && ct && (odd || even || off || risk)), OGCDPriority.Severe + 1),
            HyperchargeStrategy.ASAP => (true, OGCDPriority.Forced),
            HyperchargeStrategy.Full => (Heat == 100, OGCDPriority.Forced),
            HyperchargeStrategy.Delay or _ => (false, OGCDPriority.None),
        };
    }
    private bool ShouldUseHeat(HeatOption strategy, Actor? target)
    {
        if (!CanHB)
            return false;
        return strategy switch
        {
            HeatOption.Automatic => InCombat(target) && (WantAOE ? In12y(target) : In25y(target)),
            HeatOption.OnlyHeatBlast => In25y(target),
            HeatOption.OnlyAutoCrossbow => In12y(target),
            _ => false,
        };
    }
    private bool ShouldUseBattery(BatteryStrategy strategy)
    {
        var afterEV = LastActionUsed(AID.Excavator) || (CScd > 50 && EVleft == 0);
        var afterAA = LastActionUsed(BestAirAnchor) || AAcd > 36;
        var start = CombatTimer < 90 && (CombatTimer < 30 ? afterEV : Battery >= 90);
        var risk = (Battery >= 90 && (Cooldown(BestAirAnchor) <= GCD || CScd <= GCD || EVleft > GCD)) || (Battery == 100 && ComboLastMove is AID.SlugShot or AID.HeatedSlugShot);
        var rest = CombatTimer >= 90 && ((BScd is > 45 && (afterAA || afterEV)) || risk);
        return strategy switch
        {
            BatteryStrategy.Automatic => CanSummon && (start || rest),
            BatteryStrategy.Fifty => CanSummon,
            BatteryStrategy.Hundred => CanSummon && risk,
            BatteryStrategy.RaidBuffs => CanSummon && (RaidBuffsLeft > GCD || RaidBuffsIn < 5000),
            BatteryStrategy.End => MinionActive,
            BatteryStrategy.Delay or _ => false
        };
    }
    #endregion

    #region Other
    private (bool, OGCDPriority) ShouldUseOGCD(OGCDStrategy strategy, Actor? target, bool unlocked, float cooldown)
    {
        if (!unlocked)
            return (false, OGCDPriority.None);

        var condition =
            CanWeaveIn &&
            In25y(target) &&
            cooldown <= 30.6f &&
            (WFleft > 0 || //Wildfire active
            RaidBuffsLeft > 0 || //raid buffs active
            OverheatActive || //Overheat active
            HPP(target) <= 5 || //target low HP
            ((!Unlocked(AID.Wildfire) || Cooldown(AID.Wildfire) > 1f) || cooldown <= 0.6f) || //hold for Wildfire window if we have less than max charges
            Cooldown(AID.Wildfire) > 90); //spend all in 2m window, else hold 1

        var prio = CMDCPriority(cooldown, target);

        return strategy switch
        {
            OGCDStrategy.Automatic => (condition, prio),
            OGCDStrategy.AnyWeave => (CanWeaveIn, prio + 900),
            OGCDStrategy.EarlyWeave => (CanEarlyWeaveIn, prio + 900),
            OGCDStrategy.LateWeave => (CanLateWeaveIn, prio + 900),
            OGCDStrategy.Force => (true, prio + 900),
            _ => (false, OGCDPriority.None),
        };
    }
    private (bool, OGCDPriority) ShouldUseDoubleCheck(OGCDStrategy strategy, Actor? target) => ShouldUseOGCD(strategy, target, Unlocked(AID.GaussRound), Cooldown(BestGauss));
    private (bool, OGCDPriority) ShouldUseCheckmate(OGCDStrategy strategy, Actor? target) => ShouldUseOGCD(strategy, target, Unlocked(AID.Ricochet), Cooldown(BestRicochet));
    private OGCDPriority CMDCPriority(float cooldown, Actor? target)
    {
        //max prio
        if (RaidBuffsLeft > 0 || //raid buffs active
            (WFleft > 0 && OverheatActive) || //2m active
            HPP(target) <= 3 || //send all before death
            cooldown <= 0.6f) //overcap
            return OGCDPriority.High;

        //high prio
        if (Cooldown(AID.Wildfire) > 90 ? cooldown <= 30.6f : cooldown <= 60.6f)
            return OGCDPriority.Average;

        return OGCDPriority.Low;
    }
    private int CMDCcharges(AID action)
    {
        if (!Unlocked(action))
            return 0;

        if (Unlocked(TraitID.ChargedActionMastery))
        {
            return Cooldown(action) < 0.6f ? 3
                : Cooldown(action) < 30.6f ? 2
                : Cooldown(action) < 60.6f ? 1
                : 0;
        }
        else
        {
            return Cooldown(action) < 0.6f ? 2
                : Cooldown(action) < 30.6f ? 1
                : 0;
        }
    }

    private bool ShouldUseFlamethrower(AllowOrForbid strategy, Actor? target)
    {
        if (!CanFT)
            return false;
        return strategy switch
        {
            AllowOrForbid.Allow => InCombat(target) && ShouldFlamethrower && In12y(target),
            AllowOrForbid.Force => true,
            AllowOrForbid.Forbid or _ => false,
        };
    }
    private bool StopForFlamethrower => Service.Config.Get<MCHConfig>().PauseForFlamethrower && FTleft > 0;
    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Potion() switch
    {
        PotionStrategy.AlignWithBuffs => Player.InCombat && BScd <= 6f,
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<MachinistGauge>();
        Heat = gauge.Heat;
        Battery = gauge.Battery;
        OverheatActive = Player.FindStatus(SID.Overheated) != null;
        MinionActive = gauge.SummonTimeRemaining != 0;
        (BestConeTargets, NumConeTargets) = GetBestTarget(primaryTarget, 12, Is12yConeTarget);
        (BestSplashTargets, NumSplashTargets) = !strategy.ManualTarget() ? GetBestTarget(primaryTarget, 25, IsSplashTarget) : (primaryTarget, 0);
        (BestChainSawTargets, NumChainSawTargets) = !strategy.ManualTarget() ? GetBestTarget(primaryTarget, 25, Is25yRectTarget) : (primaryTarget, 0);
        NumFlamethrowerTargets = Hints.NumPriorityTargetsInAOECone(Player.Position, 12, Player.Rotation.ToDirection(), 45.Degrees());
        var mainTarget = primaryTarget?.Actor;
        WantAOE = Unlocked(AID.SpreadShot) && (strategy.AutoTarget() ? (NumConeTargets > 1 || ForceAOE) : strategy.ManualTarget() ? (NumFlamethrowerTargets > 1 || ForceAOE) : mainTarget != null);
        ShouldUseRangedAOE = Unlocked(AID.Ricochet) && NumSplashTargets > 1;
        ShouldUseSaw = Unlocked(AID.ChainSaw) && NumChainSawTargets > 1;
        ShouldFlamethrower = Unlocked(AID.Flamethrower) && NumFlamethrowerTargets == 2;
        BestConeTarget = WantAOE ? BestConeTargets : primaryTarget;
        BestSplashTarget = ShouldUseRangedAOE ? BestSplashTargets : primaryTarget;
        BestChainSawTarget = ShouldUseSaw ? BestChainSawTargets : primaryTarget;
        BestFlamethrowerTarget = ShouldFlamethrower ? BestConeTarget : primaryTarget;

        #region Strategy Definitions
        var opener = strategy.Option(Track.Opener);
        var openerOpt = opener.As<OpenerOption>();
        var assemble = strategy.Option(Track.Reassemble);
        var assembleStrat = assemble.As<ReassembleStrategy>();
        var gauss = strategy.Option(Track.GaussRound);
        var gaussStrat = gauss.As<OGCDStrategy>();
        var dc = strategy.Option(Track.DoubleCheck);
        var dcStrat = dc.As<OGCDStrategy>();
        var ricochet = strategy.Option(Track.Ricochet);
        var ricochetStrat = ricochet.As<OGCDStrategy>();
        var cm = strategy.Option(Track.Checkmate);
        var cmStrat = cm.As<OGCDStrategy>();
        var ft = strategy.Option(Track.Flamethrower);
        var ftStrat = ft.As<AllowOrForbid>();
        var ev = strategy.Option(Track.Excavator);
        var evStrat = ev.As<GCDStrategy>();
        var fmf = strategy.Option(Track.FullMetalField);
        var fmfStrat = fmf.As<GCDStrategy>();
        var cs = strategy.Option(Track.ChainSaw);
        var csStrat = cs.As<GCDStrategy>();
        var aa = strategy.Option(Track.AirAnchor);
        var aaStrat = aa.As<GCDStrategy>();
        var drill = strategy.Option(Track.Drill);
        var drillStrat = drill.As<DrillStrategy>();
        var hsp = strategy.Option(Track.Heat);
        var hspOpt = hsp.As<HeatOption>();
        var hc = strategy.Option(Track.Hypercharge);
        var hcStrat = hc.As<HyperchargeStrategy>();
        var battery = strategy.Option(Track.Battery);
        var batteryStrat = battery.As<BatteryStrategy>();
        var wf = strategy.Option(Track.Wildfire);
        var wfStrat = wf.As<WildfireStrategy>();
        var bs = strategy.Option(Track.BarrelStabilizer);
        var bsStrat = bs.As<OGCDStrategy>();
        #endregion

        #endregion

        #region Full Rotation Execution

        if (strategy.HoldEverything())
            return;

        #region Opener / Other
        //Stop all for Flamethrower
        if (StopForFlamethrower &&
            strategy.Option(Track.Flamethrower).As<AllowOrForbid>() != AllowOrForbid.Forbid &&
            LastActionUsed(AID.Flamethrower))
            return;

        if (CountdownRemaining == null || CombatTimer == 0)
        {
            if (!Player.InCombat && In25y(mainTarget))
            {
                if (RAleft == 0 && ActionReady(AID.Reassemble)) //RA first
                    QueueGCD(AID.Reassemble, Player, GCDPriority.Max);
                if (RAleft > 0)
                    Opener(openerOpt, mainTarget);
            }
        }
        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 5 && RAleft == 0 && ActionReady(AID.Reassemble))
                QueueGCD(AID.Reassemble, Player, GCDPriority.Max);
            if (ShouldUsePotion(strategy) && CountdownRemaining <= 1.99f)
                Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.Medium);
            if (CountdownRemaining < 1.15f)
                Opener(openerOpt, mainTarget);
            if (CountdownRemaining > 0)
                return;
        }
        if (ShouldUsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.Medium);

        #endregion

        #region Standard Rotation
        if (!OverheatActive)
        {
            var aoe = strategy.Option(Track.AOE);
            var aoeStrat = aoe.As<AOEStrategy>();
            var aoesTarget = AOETargetChoice(mainTarget, BestConeTarget?.Actor, aoe, strategy);
            var stTarget = SingleTargetChoice(mainTarget, aoe);
            var bestTarget = WantAOE ? aoesTarget : stTarget;
            ForceAOE = aoeStrat is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak;
            var (aoeAction, aoeTarget) = aoeStrat switch
            {
                AOEStrategy.AutoFinish => (AutoFinish, bestTarget),
                AOEStrategy.AutoBreak => (AutoBreak, bestTarget),
                AOEStrategy.ForceST => (ST, stTarget),
                AOEStrategy.ForceAOEFinish => (AOEFinish, aoesTarget),
                AOEStrategy.ForceAOEBreak => (BestSpreadShot, aoesTarget),
                _ => (AID.None, null)
            };
            QueueGCD(aoeAction, aoeTarget, CombatTimer > 90 && ComboTimer is < 8f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
        }
        #endregion

        #region Cooldowns
        if (!strategy.HoldAbilities())
        {
            if (!strategy.HoldCDs())
            {
                if (!strategy.HoldBuffs())
                {
                    var (wfCondition, wfPrio) = ShouldUseWildfire(wfStrat, mainTarget);
                    if (wfCondition)
                    {
                        if (wfStrat == WildfireStrategy.End)
                            QueueOGCD(AID.Detonator, Player, wfPrio);
                        else
                            QueueOGCD(AID.Wildfire, SingleTargetChoice(mainTarget, wf), wfPrio);
                    }

                    var (bsCondition, bsPrio) = ShouldUseBarrelStabilizer(bsStrat, mainTarget);
                    if (bsCondition)
                        QueueOGCD(AID.BarrelStabilizer, Player, bsPrio);

                    var (raCondition, raPrio) = ShouldUseReassemble(assembleStrat, mainTarget);
                    if (raCondition)
                        QueueOGCD(AID.Reassemble, Player, raPrio);
                }
                var (aaCondition, aaPrio) = ShouldUseAirAnchor(aaStrat, mainTarget);
                if (aaCondition)
                    QueueGCD(BestAirAnchor, SingleTargetChoice(mainTarget, aa) ?? mainTarget, aaPrio);

                var (csCondition, csPrio) = ShouldUseChainSaw(csStrat, mainTarget);
                if (csCondition)
                    QueueGCD(AID.ChainSaw, AOETargetChoice(mainTarget, BestChainSawTarget?.Actor, cs, strategy) ?? BestChainSawTarget?.Actor, csPrio);

                var (drillCondition, drillPrio) = ShouldUseDrill(drillStrat, mainTarget);
                if (drillCondition)
                {
                    if (drillStrat is DrillStrategy.Automatic)
                        QueueGCD(BestDrill, WantAOE ? AOETargetChoice(mainTarget, BestConeTarget?.Actor, drill, strategy) : SingleTargetChoice(mainTarget, drill), drillPrio);
                    if (drillStrat is DrillStrategy.OnlyDrill or DrillStrategy.ForceDrill)
                        QueueGCD(AID.Drill, SingleTargetChoice(mainTarget, drill), drillPrio);
                    if (drillStrat is DrillStrategy.OnlyBioblaster or DrillStrategy.ForceBioblaster)
                        QueueGCD(AID.Bioblaster, AOETargetChoice(mainTarget, BestConeTarget?.Actor, drill, strategy), drillPrio);
                }

                var (evCondition, evPrio) = ShouldUseExcavator(evStrat, mainTarget);
                if (evCondition)
                    QueueGCD(AID.Excavator, AOETargetChoice(mainTarget, BestSplashTarget?.Actor, ev, strategy), evStrat is GCDStrategy.Force ? GCDPriority.Forced : evPrio);

                var (fmfCondition, fmfPrio) = ShouldUseFullMetalField(fmfStrat, mainTarget);
                if (fmfCondition)
                    QueueGCD(AID.FullMetalField, AOETargetChoice(mainTarget, BestSplashTarget?.Actor, fmf, strategy), fmfStrat is GCDStrategy.Force ? GCDPriority.Forced : fmfPrio);

                var (dcCondition, dcPrio) = ShouldUseDoubleCheck(dcStrat, mainTarget);
                var (grCondition, grPrio) = ShouldUseDoubleCheck(gaussStrat, mainTarget);
                if (Unlocked(AID.DoubleCheck) ? dcCondition : grCondition)
                    QueueOGCD(BestGauss, AOETargetChoice(mainTarget, Unlocked(AID.DoubleCheck) ? BestSplashTarget?.Actor : mainTarget, Unlocked(AID.DoubleCheck) ? dc : gauss, strategy), Unlocked(AID.DoubleCheck) ? dcPrio : grPrio);
                var (cmCondition, cmPrio) = ShouldUseCheckmate(cmStrat, mainTarget);
                var (ricochetCondition, ricoPrio) = ShouldUseCheckmate(ricochetStrat, mainTarget);
                if (Unlocked(AID.Checkmate) ? cmCondition : ricochetCondition)
                    QueueOGCD(BestRicochet, AOETargetChoice(mainTarget, BestSplashTarget?.Actor, Unlocked(AID.Checkmate) ? cm : ricochet, strategy), Unlocked(AID.Checkmate) ? cmPrio : ricoPrio);

                if (ShouldUseFlamethrower(ftStrat, mainTarget))
                    QueueGCD(AID.Flamethrower, AOETargetChoice(mainTarget, BestFlamethrowerTarget?.Actor, ft, strategy), ftStrat is AllowOrForbid.Force ? GCDPriority.Forced : GCDPriority.ModeratelyLow);
            }
            if (!strategy.HoldGauge())
            {
                var (hcCondition, hcPrio) = ShouldUseHypercharge(hcStrat, mainTarget);
                if (hcCondition)
                    QueueOGCD(AID.Hypercharge, Player, hcPrio);
                if (ShouldUseBattery(batteryStrat))
                {
                    if (batteryStrat is BatteryStrategy.Automatic or BatteryStrategy.Fifty or BatteryStrategy.Hundred)
                        QueueOGCD(Unlocked(AID.AutomatonQueen) ? AID.AutomatonQueen : AID.RookAutoturret, Player, OGCDPriority.Severe);

                    if (batteryStrat == BatteryStrategy.End)
                        QueueOGCD(Unlocked(AID.QueenOverdrive) ? AID.QueenOverdrive : AID.RookOverdrive, Player, OGCDPriority.Critical);
                }
            }
            if (ShouldUseHeat(hspOpt, mainTarget))
            {
                if (hspOpt == HeatOption.Automatic)
                    QueueGCD(BestHeat, AOETargetChoice(mainTarget, mainTarget, hsp, strategy), GCDPriority.High);
                if (hspOpt == HeatOption.OnlyHeatBlast)
                    QueueGCD(BestHeatBlast, SingleTargetChoice(mainTarget, hsp), GCDPriority.High);
                if (hspOpt == HeatOption.OnlyAutoCrossbow)
                    QueueGCD(Unlocked(AID.AutoCrossbow) ? AID.AutoCrossbow : BestHeatBlast, AOETargetChoice(mainTarget, BestConeTarget?.Actor, hsp, strategy), GCDPriority.High);
            }
        }
        #endregion
        #endregion

        #region AI
        if (primaryTarget != null)
        {
            var aoebreakpoint = OverheatActive && Unlocked(AID.AutoCrossbow) ? 4 : 2;
            GoalZoneCombined(strategy, 25, Hints.GoalAOECone(primaryTarget.Actor, 12, 60.Degrees()), AID.SpreadShot, aoebreakpoint);
        }
        #endregion
    }
}
