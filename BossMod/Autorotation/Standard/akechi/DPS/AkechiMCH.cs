using BossMod.MCH;
using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiMCH(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Opener = SharedTrack.Count, Heat, Battery, Reassemble, Hypercharge, Drill, Wildfire, BarrelStabilizer, AirAnchor, ChainSaw, GaussRound, DoubleCheck, Ricochet, Checkmate, Flamethrower, Excavator, FullMetalField }
    public enum OpenerOption { AirAnchor, Drill, ChainSaw }
    public enum HeatOption { Automatic, OnlyHeatBlast, OnlyAutoCrossbow }
    public enum BatteryStrategy { Automatic, Fifty, Hundred, RaidBuffs, End, Delay }
    public enum ReassembleStrategy { Automatic, Any, HoldOne, Force, ForceWeave, Delay }
    public enum HyperchargeStrategy { Automatic, ASAP, Full, Delay }
    public enum DrillStrategy { Automatic, HoldOne, OnlyDrill, OnlyBioblaster, ForceDrill, ForceBioblaster, Delay }
    public enum WildfireStrategy { Automatic, AlignWithBurst, Force, ForceWeave, End, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi MCH", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.MCH), 100);
        res.DefineAOE().AddAssociatedActions(
            AID.SplitShot, AID.SlugShot, AID.CleanShot,
            AID.HeatedSplitShot, AID.HeatedSlugShot, AID.HeatedCleanShot,
            AID.SpreadShot, AID.Scattergun);
        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionDex);
        res.Define(Track.Opener).As<OpenerOption>("Opener", "Opener", 199)
            .AddOption(OpenerOption.AirAnchor, "Air Anchor", "Use Hot Shot / Air Anchor as first tool in opener", minLevel: 4)
            .AddOption(OpenerOption.Drill, "Drill", "Use Drill as first tool in opener", minLevel: 58)
            .AddOption(OpenerOption.ChainSaw, "Chain Saw", "Use Chain Saw as first tool in opener", minLevel: 90);
        res.Define(Track.Heat).As<HeatOption>("Heat Option", "Heat", 198)
            .AddOption(HeatOption.Automatic, "Automatic", "Automatically use Heat Blast or Auto-Crossbow based on targets nearby")
            .AddOption(HeatOption.OnlyHeatBlast, "Heat Blast", "Only use Heat Blast, regardless of targets", 0, 0, ActionTargets.Hostile, 35)
            .AddOption(HeatOption.OnlyAutoCrossbow, "Auto Crossbow", "Only use Auto Crossbow, regardless of targets", 0, 0, ActionTargets.Hostile, 52)
            .AddAssociatedActions(AID.HeatBlast, AID.AutoCrossbow, AID.BlazingShot);
        res.Define(Track.Battery).As<BatteryStrategy>("Battery", "", 189)
            .AddOption(BatteryStrategy.Automatic, "Automatic", "Use Battery actions when optimal")
            .AddOption(BatteryStrategy.Fifty, "50", "Use Battery actions ASAP when 50+ Battery Gauge is available", minLevel: 40)
            .AddOption(BatteryStrategy.Hundred, "100", "Use Battery actions ASAP when 100 Battery Gauge is available", minLevel: 40)
            .AddOption(BatteryStrategy.RaidBuffs, "Raid Buffs", "Use Battery actions ASAP when raid buffs are active", minLevel: 40)
            .AddOption(BatteryStrategy.End, "End", "Ends Battery action ASAP with Overdrive (assuming it's currently active)", minLevel: 40)
            .AddOption(BatteryStrategy.Delay, "Delay", "Delay use of Battery actions", minLevel: 40)
            .AddAssociatedActions(AID.RookAutoturret, AID.RookOverdrive, AID.AutomatonQueen, AID.QueenOverdrive);
        res.Define(Track.Reassemble).As<ReassembleStrategy>("Reassemble", "R.semble", 184)
            .AddOption(ReassembleStrategy.Automatic, "Automatic", "Use Reassemble when optimal")
            .AddOption(ReassembleStrategy.Any, "Any", "Use Reassemble when any tool is available; uses both charges")
            .AddOption(ReassembleStrategy.HoldOne, "Hold One", "Use Reassemble when any tool is available; holds one charge for manual usage")
            .AddOption(ReassembleStrategy.Force, "Force", "Force use of Reassemble, regardless of weaving", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.ForceWeave, "ForceWeave", "Force use of Reassemble in next possible weave window", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.Delay, "Delay", "Delay use of Reassemble", minLevel: 10)
            .AddAssociatedActions(AID.Reassemble);
        res.Define(Track.Hypercharge).As<HyperchargeStrategy>("Hypercharge", "H.charge", 190)
            .AddOption(HyperchargeStrategy.Automatic, "Automatic", "Use Hypercharge when optimal")
            .AddOption(HyperchargeStrategy.ASAP, "ASAP", "Use Hypercharge ASAP (if any Heat Gauge is available)", 0, 10, ActionTargets.Self, 30)
            .AddOption(HyperchargeStrategy.Full, "Full", "Use Hypercharge when Heat Gauge is full (or about to be)", 0, 10, ActionTargets.Self, 30)
            .AddOption(HyperchargeStrategy.Delay, "Delay", "Delay use of Hypercharge", minLevel: 30)
            .AddAssociatedActions(AID.Hypercharge);
        res.Define(Track.Drill).As<DrillStrategy>("Drill", "", 179)
            .AddOption(DrillStrategy.Automatic, "Automatic", "Automatically use Drill or Bioblaster based on targets nearby; uses both charges")
            .AddOption(DrillStrategy.HoldOne, "Hold One", "Automatically use Drill or Bioblaster based on targets nearby; holds one charge for manual usage")
            .AddOption(DrillStrategy.OnlyDrill, "Only Drill", "Only use Drill, regardless of targets", minLevel: 58)
            .AddOption(DrillStrategy.OnlyBioblaster, "Only Bioblaster", "Only use Bioblaster, regardless of targets", minLevel: 72)
            .AddOption(DrillStrategy.ForceDrill, "Force Drill", "Force use of Drill", 20, 0, ActionTargets.Hostile, 58)
            .AddOption(DrillStrategy.ForceBioblaster, "Force Bioblaster", "Force use of Bioblaster", 20, 15, ActionTargets.Hostile, 72)
            .AddOption(DrillStrategy.Delay, "Delay", "Delay use of Drill", minLevel: 58)
            .AddAssociatedActions(AID.Drill, AID.Bioblaster);
        res.Define(Track.Wildfire).As<WildfireStrategy>("Wildfire", "W.fire", 183)
            .AddOption(WildfireStrategy.Automatic, "Automatic", "Use Wildfire when optimal")
            .AddOption(WildfireStrategy.AlignWithBurst, "AlignWithBurst", "Use when optimal; attempts to keep it aligned with burst windows")
            .AddOption(WildfireStrategy.Force, "Force", "Force use of Wildfire, regardless of weaving", 120, 10, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.ForceWeave, "ForceWeave", "Force use of Wildfire in next possible weave window", 120, 10, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.End, "End", "End Wildfire early with Detonator", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.Delay, "Delay", "Delay use of Wildfire", minLevel: 45)
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
    #endregion

    #region Module Variables
    private int Heat; //0-100
    private int Battery; //0-100
    private bool OverheatActive; //10s duration; 5 stacks
    private bool MinionActive;
    private float RAleft; //5s
    private float HCleft; //30s
    private float WFleft; //10s
    private float EVleft; //30s
    private float FMFleft; //30s
    private float FTleft;
    private float BScd; //120s CD
    private float Drillcd; //20s charge CD, 40s total CD (NOTE: can change depending on speed factors like sks and haste)
    private float AAcd; //40s CD
    private float CScd; //60s CD
    private bool ShouldUseAOE;
    private bool ShouldUseRangedAOE;
    private bool ShouldUseSaw;
    private bool ShouldFlamethrower;
    private bool CanAA;
    private bool CanRA;
    private bool CanHC;
    private bool CanHB;
    private bool CanSummon;
    private bool CanWF;
    private bool CanDrill;
    private bool CanBB;
    private bool CanBS;
    private bool CanCS;
    private bool CanEV;
    private bool CanFMF;
    private bool CanFT;
    private bool Drillsafe;
    private bool AAsafe;
    private bool CSsafe;
    private bool EVsafe;
    private bool FMFsafe;
    private int NumConeTargets;
    private int NumSplashTargets;
    private int NumChainSawTargets;
    private int NumFlamethrowerTargets;
    private int RicoCharges;
    private int GaussCharges;
    private Enemy? BestConeTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestChainSawTargets;
    private Enemy? BestConeTarget;
    private Enemy? BestSplashTarget;
    private Enemy? BestChainSawTarget;
    private Enemy? BestFlamethrowerTarget;
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.SlugShot or AID.HeatedSlugShot or AID.SplitShot or AID.HeatedSplitShot => ST,
        AID.CleanShot or AID.HeatedCleanShot or AID.Scattergun or AID.SpreadShot or _ => AutoBreak,
    };
    private bool BreakCombo => ComboLastMove == AID.HeatedSlugShot ? NumConeTargets > 3 : ComboLastMove == AID.HeatedSplitShot ? NumConeTargets > 2 : NumConeTargets > 1;
    private AID AutoBreak => BreakCombo ? BestSpreadShot : ST;

    #region Upgrade Paths
    private AID ST => ComboLastMove is AID.SlugShot or AID.HeatedSlugShot ? BestCleanShot : ComboLastMove is AID.SplitShot or AID.HeatedSplitShot ? BestSlugShot : BestSplitShot;
    private AID BestDrill => NumConeTargets > 1 && Unlocked(AID.Bioblaster) ? AID.Bioblaster : AID.Drill;
    private AID BestHeat => NumConeTargets > 3 ? AID.AutoCrossbow : BestHeatBlast;

    //NOTE: honestly not sure if these below are even needed but they work & make for cleaner AR window I guess
    private AID BestSpreadShot => Unlocked(AID.Scattergun) ? AID.Scattergun : AID.SpreadShot;
    private AID BestSplitShot => Unlocked(AID.HeatedSplitShot) ? AID.HeatedSplitShot : AID.SplitShot;
    private AID BestSlugShot => Unlocked(AID.HeatedSlugShot) ? AID.HeatedSlugShot : AID.SlugShot;
    private AID BestCleanShot => Unlocked(AID.HeatedCleanShot) ? AID.HeatedCleanShot : AID.CleanShot;
    private AID BestHeatBlast => Unlocked(AID.BlazingShot) ? AID.BlazingShot : Unlocked(AID.HeatBlast) ? AID.HeatBlast : ST;
    private AID BestGauss => Unlocked(AID.DoubleCheck) ? AID.DoubleCheck : AID.GaussRound;
    private AID BestRicochet => Unlocked(AID.Checkmate) ? AID.Checkmate : AID.Ricochet;
    private AID BestAirAnchor => Unlocked(AID.AirAnchor) ? AID.AirAnchor : AID.HotShot;
    private AID BestBattery => Unlocked(AID.AutomatonQueen) ? AID.AutomatonQueen : AID.RookAutoturret;
    #endregion

    #endregion

    #region Cooldown Helpers

    #region Buffs
    private (bool, OGCDPriority) ShouldUseWildfire(WildfireStrategy strategy, Actor? target)
    {
        var condition = InsideCombatWith(target) && CanWF && CanWeaveIn && ((FMFleft > 0 && AAsafe && CSsafe && EVleft == 0) || LastActionUsed(AID.Hypercharge) || OverheatActive);
        return strategy switch
        {
            WildfireStrategy.Automatic => (condition, OGCDPriority.Max),
            WildfireStrategy.AlignWithBurst => (BScd > 90 && condition, OGCDPriority.Max),
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
            OGCDStrategy.Automatic => (CanWeaveIn, OGCDPriority.Max),
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
        var any = Player.InCombat && CanWeaveIn && (AAcd < GCD || CScd < GCD || EVleft > GCD || NextGCD == AID.Drill);
        var risk = CDRemaining(AID.Reassemble) < 15 && any;
        return strategy switch
        {
            ReassembleStrategy.Automatic => (opti || risk, OGCDPriority.High),
            ReassembleStrategy.Any => (any || risk, OGCDPriority.High),
            ReassembleStrategy.HoldOne => (CDRemaining(AID.Reassemble) < 20 && any, OGCDPriority.High),
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
        GCDStrategy.Automatic => (InsideCombatWith(target) && CanAA, GCDPriority.ExtremelyHigh + 10),
        GCDStrategy.Force => (CanAA, GCDPriority.Forced),
        GCDStrategy.Delay or _ => (false, GCDPriority.None),
    };
    private (bool, GCDPriority) ShouldUseDrill(DrillStrategy strategy, Actor? target)
    {
        var st = InsideCombatWith(target) && CanDrill && (ActionReady(AID.Drill) || !OverheatActive);
        var aoe = InsideCombatWith(target) && CanBB && In12y(target) && ActionReady(AID.Bioblaster);
        var prio = CDRemaining(AID.Drill) < GCD ? GCDPriority.ExtremelyHigh + 9 : CanFitSkSGCD(WFleft) && FMFleft == 0 ? GCDPriority.High + 2 : GCDPriority.High;
        return strategy switch
        {
            DrillStrategy.Automatic or DrillStrategy.OnlyDrill or DrillStrategy.OnlyBioblaster => (ShouldUseAOE ? aoe : st, prio),
            DrillStrategy.HoldOne => (CDRemaining(AID.Drill) < GCD && (ShouldUseAOE ? aoe : st), prio),
            DrillStrategy.ForceDrill => (CanDrill, GCDPriority.Forced),
            DrillStrategy.ForceBioblaster => (CanBB, GCDPriority.Forced),
            DrillStrategy.Delay or _ => (false, GCDPriority.None),
        };
    }
    private (bool, GCDPriority) ShouldUseChainSaw(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => (InsideCombatWith(target) && CanCS, GCDPriority.ExtremelyHigh + 2),
        GCDStrategy.Force => (CanCS, GCDPriority.Forced),
        GCDStrategy.Delay or _ => (false, GCDPriority.None),
    };
    private (bool, GCDPriority) ShouldUseExcavator(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => (InsideCombatWith(target) && !OverheatActive && CanEV, GCDPriority.ExtremelyHigh + 5),
        GCDStrategy.Force => (CanEV, GCDPriority.Forced),
        GCDStrategy.Delay or _ => (false, GCDPriority.None),
    };
    private (bool, GCDPriority) ShouldUseFullMetalField(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => (InsideCombatWith(target) && (ComboLastMove is AID.HeatedCleanShot ? OverheatActive : !OverheatActive) && CanFMF, GCDPriority.High + 1),
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
        var off = !Unlocked(AID.Wildfire) || (Unlocked(AID.Wildfire) && (CDRemaining(AID.Wildfire) > 40 || (CDRemaining(AID.Wildfire) <= 2f && FMFleft == 0) || WFleft > 0));
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
            HeatOption.Automatic => InsideCombatWith(target) && (ShouldUseAOE ? In12y(target) : In25y(target)),
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
        var risk = (Battery >= 90 && (CDRemaining(BestAirAnchor) <= GCD || CScd <= GCD || EVleft > GCD)) || (Battery == 100 && ComboLastMove is AID.SlugShot or AID.HeatedSlugShot);
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
    private (bool, OGCDPriority) ShouldUseOGCD(OGCDStrategy strategy, Actor? target, bool unlocked, int charges, bool condition)
    {
        if (!unlocked)
            return (false, OGCDPriority.None);

        var prio = CMDCPriority(charges);

        return strategy switch
        {
            OGCDStrategy.Automatic => (In25y(target) && CanWeaveIn && condition, prio),
            OGCDStrategy.AnyWeave => (CanWeaveIn, prio + 900),
            OGCDStrategy.EarlyWeave => (CanEarlyWeaveIn, prio + 900),
            OGCDStrategy.LateWeave => (CanLateWeaveIn, prio + 900),
            OGCDStrategy.Force => (true, prio + 900),
            _ => (false, OGCDPriority.None),
        };
    }
    private (bool, OGCDPriority) ShouldUseDoubleCheck(OGCDStrategy strategy, Actor? target) => ShouldUseOGCD(strategy, target, Unlocked(AID.GaussRound), GaussCharges,
            WFleft > 0 || RaidBuffsLeft > 0 || OverheatActive || HPP(target) <= 5 || (CDRemaining(AID.Wildfire) > 90 ? CDRemaining(BestGauss) <= 62f : CDRemaining(BestGauss) <= 32f));
    private (bool, OGCDPriority) ShouldUseCheckmate(OGCDStrategy strategy, Actor? target) => ShouldUseOGCD(strategy, target, Unlocked(AID.Ricochet), RicoCharges,
            WFleft > 0 || RaidBuffsLeft > 0 || OverheatActive || HPP(target) <= 5 || (CDRemaining(AID.Wildfire) > 90 ? CDRemaining(BestRicochet) <= 62f : CDRemaining(BestRicochet) <= 32f));
    private OGCDPriority CMDCPriority(int charges) => charges switch
    {
        3 => OGCDPriority.ExtremelyHigh + 1,
        2 => OGCDPriority.Average,
        1 => OGCDPriority.Low,
        _ => OGCDPriority.ExtremelyLow
    };
    private bool ShouldUseFlamethrower(AllowOrForbid strategy, Actor? target)
    {
        if (!CanFT)
            return false;
        return strategy switch
        {
            AllowOrForbid.Allow => InsideCombatWith(target) && ShouldFlamethrower && In12y(target) && AAcd > 10 && target?.FindStatus(SID.Bioblaster) == null && CScd > 10 && EVleft == 0 && FMFleft == 0,
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

    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<MachinistGauge>();
        Heat = gauge.Heat;
        Battery = gauge.Battery;
        OverheatActive = Player.FindStatus(SID.Overheated) != null;
        MinionActive = gauge.SummonTimeRemaining != 0;
        RAleft = StatusRemaining(Player, SID.Reassembled);
        HCleft = StatusRemaining(Player, SID.Hypercharged);
        WFleft = StatusRemaining(Player, SID.WildfirePlayer);
        EVleft = StatusRemaining(Player, SID.ExcavatorReady);
        FMFleft = StatusRemaining(Player, SID.FullMetalMachinist);
        FTleft = StatusRemaining(Player, SID.Flamethrower);
        BScd = CDRemaining(AID.BarrelStabilizer);
        Drillcd = CDRemaining(AID.Drill);
        AAcd = Unlocked(AID.AirAnchor) ? CDRemaining(AID.AirAnchor) : CDRemaining(AID.HotShot);
        CScd = CDRemaining(AID.ChainSaw);
        Drillsafe = !Unlocked(AID.Drill) || (Unlocked(AID.Drill) && Drillcd > 7.6f);
        AAsafe = !Unlocked(AID.AirAnchor) || (Unlocked(AID.AirAnchor) && AAcd > 7.6f);
        CSsafe = !Unlocked(AID.ChainSaw) || (Unlocked(AID.ChainSaw) && CScd > 7.6f);
        EVsafe = !Unlocked(AID.Excavator) || (Unlocked(AID.Excavator) && EVleft == 0);
        FMFsafe = !Unlocked(AID.FullMetalField) || (Unlocked(AID.FullMetalField) && FMFleft == 0);
        CanHC = ActionReady(AID.Hypercharge) && (Heat >= 50 || HCleft > GCD);
        CanHB = Unlocked(AID.HeatBlast) && OverheatActive;
        CanSummon = Unlocked(AID.RookAutoturret) && Battery >= 50 && !MinionActive;
        CanWF = ActionReady(AID.Wildfire);
        CanBS = ActionReady(AID.BarrelStabilizer);
        CanRA = Unlocked(AID.Reassemble) && CDRemaining(AID.Reassemble) <= 57f && !OverheatActive && RAleft == 0;
        CanDrill = Unlocked(AID.Drill) && (Unlocked(TraitID.EnhancedMultiweapon) ? CDRemaining(AID.Drill) < 0.5f + (SkSGCDLength * 8) : CDRemaining(AID.Drill) < 0.5f);
        CanBB = ActionReady(AID.Bioblaster);
        CanAA = ActionReady(BestAirAnchor);
        CanCS = ActionReady(AID.ChainSaw);
        CanEV = Unlocked(AID.Excavator) && EVleft > 0;
        CanFMF = Unlocked(AID.FullMetalField) && FMFleft > 0;
        CanFT = ActionReady(AID.Flamethrower) && !OverheatActive && FTleft == 0 && NumFlamethrowerTargets > 2;
        (BestConeTargets, NumConeTargets) = GetBestTarget(primaryTarget, 12, Is12yConeTarget);
        (BestSplashTargets, NumSplashTargets) = !strategy.ManualTarget() ? GetBestTarget(primaryTarget, 25, IsSplashTarget) : (primaryTarget, 0);
        (BestChainSawTargets, NumChainSawTargets) = !strategy.ManualTarget() ? GetBestTarget(primaryTarget, 25, Is25yRectTarget) : (primaryTarget, 0);
        NumFlamethrowerTargets = Hints.NumPriorityTargetsInAOECone(Player.Position, 12, Player.Rotation.ToDirection(), 45.Degrees());
        ShouldUseAOE = Unlocked(AID.SpreadShot) && (strategy.AutoTarget() ? (NumConeTargets > 1 || strategy.ForceAOE()) : strategy.ManualTarget() ? (NumFlamethrowerTargets > 1 || strategy.ForceAOE()) : primaryTarget?.Actor != null);
        ShouldUseRangedAOE = Unlocked(AID.Ricochet) && NumSplashTargets > 1;
        ShouldUseSaw = Unlocked(AID.ChainSaw) && NumChainSawTargets > 1;
        ShouldFlamethrower = Unlocked(AID.Flamethrower) && NumFlamethrowerTargets > 2;
        BestConeTarget = ShouldUseAOE ? BestConeTargets : primaryTarget;
        BestSplashTarget = ShouldUseRangedAOE ? BestSplashTargets : primaryTarget;
        BestChainSawTarget = ShouldUseSaw ? BestChainSawTargets : primaryTarget;
        BestFlamethrowerTarget = ShouldFlamethrower ? BestConeTarget : primaryTarget;
        RicoCharges = CDRemaining(BestRicochet) <= GCD ? 3 : CDRemaining(BestRicochet) < 30.6f ? 2 : CDRemaining(BestRicochet) < 60.6f ? 1 : 0;
        GaussCharges = CDRemaining(BestGauss) <= GCD ? 3 : CDRemaining(BestGauss) < 30.6f ? 2 : CDRemaining(BestGauss) < 60.6f ? 1 : 0;

        #region Strategy Definitions
        var AOE = strategy.Option(SharedTrack.AOE);
        var AOEStrategy = AOE.As<AOEStrategy>();
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

        if (!strategy.HoldEverything())
        {
            #region Opener / Other
            //Stop all for Flamethrower
            if (StopForFlamethrower && strategy.Option(Track.Flamethrower).As<AllowOrForbid>() != AllowOrForbid.Forbid &&
                ((Unlocked(TraitID.EnhancedMultiweapon) ? ReadyIn(AID.Drill) > 0 : Drillcd > 0) || AAcd > 0 || CScd > 0))
                return;

            if (CountdownRemaining == null || CombatTimer == 0)
            {
                if (!Player.InCombat && In25y(primaryTarget?.Actor))
                {
                    if (RAleft == 0 && ActionReady(AID.Reassemble)) //RA first
                        QueueGCD(AID.Reassemble, Player, GCDPriority.Max);
                    if (RAleft > 0)
                        Opener(openerOpt, primaryTarget?.Actor);
                }
            }
            if (CountdownRemaining > 0)
            {
                if (CountdownRemaining < 5 && RAleft == 0 && ActionReady(AID.Reassemble))
                    QueueGCD(AID.Reassemble, Player, GCDPriority.Max);
                if (ShouldUsePotion(strategy) && CountdownRemaining <= 1.99f)
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.Medium);
                if (CountdownRemaining < 1.15f)
                    Opener(openerOpt, primaryTarget?.Actor);
                if (CountdownRemaining > 0)
                    return;
            }
            if (ShouldUsePotion(strategy))
                Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.Medium);

            #endregion

            #region Standard Rotation
            if (!OverheatActive)
            {
                if (AOEStrategy == AOEStrategy.AutoFinish)
                    QueueGCD(AutoFinish, AOETargetChoice(primaryTarget?.Actor, BestConeTarget?.Actor, AOE, strategy), CombatTimer > 90 && ComboTimer is < 8f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
                if (AOEStrategy == AOEStrategy.AutoBreak)
                    QueueGCD(AutoBreak, AOETargetChoice(primaryTarget?.Actor, BestConeTarget?.Actor, AOE, strategy), CombatTimer > 90 && ComboTimer is < 8f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
                if (AOEStrategy == AOEStrategy.ForceST)
                    QueueGCD(ST, SingleTargetChoice(primaryTarget?.Actor, AOE), CombatTimer > 90 && ComboTimer is < 8f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
                if (AOEStrategy == AOEStrategy.ForceAOE)
                    QueueGCD(BestSpreadShot, AOETargetChoice(primaryTarget?.Actor, BestConeTarget?.Actor, AOE, strategy), CombatTimer > 90 && ComboTimer is < 8f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
            }
            #endregion

            #region Cooldowns
            if (!strategy.HoldAbilities())
            {
                if (!strategy.HoldCDs())
                {
                    if (!strategy.HoldBuffs())
                    {
                        var (wfCondition, wfPrio) = ShouldUseWildfire(wfStrat, primaryTarget?.Actor);
                        if (wfCondition)
                        {
                            if (wfStrat == WildfireStrategy.End)
                                QueueOGCD(AID.Detonator, Player, wfPrio);
                            else
                                QueueOGCD(AID.Wildfire, SingleTargetChoice(primaryTarget?.Actor, wf), wfPrio);
                        }

                        var (bsCondition, bsPrio) = ShouldUseBarrelStabilizer(bsStrat, primaryTarget?.Actor);
                        if (bsCondition)
                            QueueOGCD(AID.BarrelStabilizer, Player, bsPrio);

                        var (raCondition, raPrio) = ShouldUseReassemble(assembleStrat, primaryTarget?.Actor);
                        if (raCondition)
                            QueueOGCD(AID.Reassemble, Player, raPrio);
                    }
                    var (aaCondition, aaPrio) = ShouldUseAirAnchor(aaStrat, primaryTarget?.Actor);
                    if (aaCondition)
                        QueueGCD(BestAirAnchor, SingleTargetChoice(primaryTarget?.Actor, aa) ?? primaryTarget?.Actor, aaPrio);

                    var (csCondition, csPrio) = ShouldUseChainSaw(csStrat, primaryTarget?.Actor);
                    if (csCondition)
                        QueueGCD(AID.ChainSaw, AOETargetChoice(primaryTarget?.Actor, BestChainSawTarget?.Actor, cs, strategy) ?? BestChainSawTarget?.Actor, csPrio);

                    var (drillCondition, drillPrio) = ShouldUseDrill(drillStrat, primaryTarget?.Actor);
                    if (drillCondition)
                    {
                        if (drillStrat is DrillStrategy.Automatic)
                            QueueGCD(BestDrill, ShouldUseAOE ? AOETargetChoice(primaryTarget?.Actor, BestConeTarget?.Actor, drill, strategy) : SingleTargetChoice(primaryTarget?.Actor, drill), drillPrio);
                        if (drillStrat is DrillStrategy.OnlyDrill or DrillStrategy.ForceDrill)
                            QueueGCD(AID.Drill, SingleTargetChoice(primaryTarget?.Actor, drill), drillPrio);
                        if (drillStrat is DrillStrategy.OnlyBioblaster or DrillStrategy.ForceBioblaster)
                            QueueGCD(AID.Bioblaster, AOETargetChoice(primaryTarget?.Actor, BestConeTarget?.Actor, drill, strategy), drillPrio);
                    }

                    var (evCondition, evPrio) = ShouldUseExcavator(evStrat, primaryTarget?.Actor);
                    if (evCondition)
                        QueueGCD(AID.Excavator, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, ev, strategy), evStrat is GCDStrategy.Force ? GCDPriority.Forced : evPrio);

                    var (fmfCondition, fmfPrio) = ShouldUseFullMetalField(fmfStrat, primaryTarget?.Actor);
                    if (fmfCondition)
                        QueueGCD(AID.FullMetalField, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, fmf, strategy), fmfStrat is GCDStrategy.Force ? GCDPriority.Forced : fmfPrio);

                    var (dcCondition, dcPrio) = ShouldUseDoubleCheck(dcStrat, primaryTarget?.Actor);
                    var (grCondition, grPrio) = ShouldUseDoubleCheck(gaussStrat, primaryTarget?.Actor);
                    if (Unlocked(AID.DoubleCheck) ? dcCondition : grCondition)
                        QueueOGCD(BestGauss, AOETargetChoice(primaryTarget?.Actor, Unlocked(AID.DoubleCheck) ? BestSplashTarget?.Actor : primaryTarget?.Actor, Unlocked(AID.DoubleCheck) ? dc : gauss, strategy), Unlocked(AID.DoubleCheck) ? dcPrio : grPrio);
                    var (cmCondition, cmPrio) = ShouldUseCheckmate(cmStrat, primaryTarget?.Actor);
                    var (ricochetCondition, ricoPrio) = ShouldUseCheckmate(ricochetStrat, primaryTarget?.Actor);
                    if (Unlocked(AID.Checkmate) ? cmCondition : ricochetCondition)
                        QueueOGCD(BestRicochet, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, Unlocked(AID.Checkmate) ? cm : ricochet, strategy), Unlocked(AID.Checkmate) ? cmPrio : ricoPrio);

                    if (ShouldUseFlamethrower(ftStrat, primaryTarget?.Actor))
                        QueueGCD(AID.Flamethrower, AOETargetChoice(primaryTarget?.Actor, BestFlamethrowerTarget?.Actor, ft, strategy), ftStrat is AllowOrForbid.Force ? GCDPriority.Forced : GCDPriority.ModeratelyLow);
                }
                if (!strategy.HoldGauge())
                {
                    var (hcCondition, hcPrio) = ShouldUseHypercharge(hcStrat, primaryTarget?.Actor);
                    if (hcCondition)
                        QueueOGCD(AID.Hypercharge, Player, hcPrio);
                    if (ShouldUseBattery(batteryStrat))
                    {
                        if (batteryStrat is BatteryStrategy.Automatic or BatteryStrategy.Fifty or BatteryStrategy.Hundred)
                            QueueOGCD(BestBattery, Player, OGCDPriority.Severe);
                        if (batteryStrat == BatteryStrategy.End)
                            QueueOGCD(Unlocked(AID.QueenOverdrive) ? AID.QueenOverdrive : AID.RookOverdrive, Player, OGCDPriority.Critical);
                    }
                }
                if (ShouldUseHeat(hspOpt, primaryTarget?.Actor))
                {
                    if (hspOpt == HeatOption.Automatic)
                        QueueGCD(BestHeat, AOETargetChoice(primaryTarget?.Actor, primaryTarget?.Actor, hsp, strategy), GCDPriority.High);
                    if (hspOpt == HeatOption.OnlyHeatBlast)
                        QueueGCD(BestHeatBlast, SingleTargetChoice(primaryTarget?.Actor, hsp), GCDPriority.High);
                    if (hspOpt == HeatOption.OnlyAutoCrossbow)
                        QueueGCD(Unlocked(AID.AutoCrossbow) ? AID.AutoCrossbow : BestHeatBlast, AOETargetChoice(primaryTarget?.Actor, BestConeTarget?.Actor, hsp, strategy), GCDPriority.High);
                }
            }
            #endregion
        }
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
