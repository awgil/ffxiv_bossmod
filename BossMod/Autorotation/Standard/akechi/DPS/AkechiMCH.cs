#region Dependencies
using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.MCH;
using static BossMod.ActorCastEvent;
using static BossMod.Autorotation.akechi.AkechiMCH;
#endregion

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiMCH(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Potion = SharedTrack.Count, Tools, Heat, Battery, Reassemble, Hypercharge, Drill, Wildfire, BarrelStabilizer, AirAnchor, ChainSaw, GaussRound, DoubleCheck, Ricochet, Checkmate, Flamethrower, Excavator, FullMetalField }
    public enum PotionStrategy { None, Use, Align }
    public enum ToolOrder { AADrillCS, AACSDrill, DrillAACS, DrillCSAA, CSAADrill, CSDrillAA }
    public enum HeatOption { Automatic, OnlyHeatBlast, OnlyAutoCrossbow }
    public enum BatteryStrategy { Automatic, Fifty, Hundred, End, Delay }
    public enum ReassembleStrategy { Automatic, HoldOne, Force, ForceWeave, Delay }
    public enum HyperchargeStrategy { Automatic, ASAP, Full, Delay }
    public enum DrillStrategy { Automatic, OnlyDrill, OnlyBioblaster, ForceDrill, ForceBioblaster, Delay }
    public enum WildfireStrategy { Automatic, End, Force, ForceWeave, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi MCH", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.MCH), 100);
        res.DefineAOE().AddAssociatedActions(
            AID.SplitShot, AID.SlugShot, AID.CleanShot,
            AID.HeatedSplitShot, AID.HeatedSlugShot, AID.HeatedCleanShot,
            AID.SpreadShot, AID.Scattergun);
        res.DefineHold();
        res.Define(Track.Potion).As<PotionStrategy>("Potion", "", 200)
            .AddOption(PotionStrategy.None, "None", "Do not use Potion")
            .AddOption(PotionStrategy.Use, "Use", "Use Potion when available", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Align, "Align", "Align Potion with raid buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionDex);
        res.Define(Track.Tools).As<ToolOrder>("Tools Order", "Tools", 200)
            .AddOption(ToolOrder.AADrillCS, "AA>Drill>CS", "Air Anchor > Drill > ChainSaw", 0, 0, ActionTargets.None, 70)
            .AddOption(ToolOrder.AACSDrill, "AA>CS>Drill", "Air Anchor > ChainSaw > Drill", 0, 0, ActionTargets.None, 70)
            .AddOption(ToolOrder.DrillAACS, "Drill>AA>CS", "Drill > Air Anchor > ChainSaw", 0, 0, ActionTargets.None, 70)
            .AddOption(ToolOrder.DrillCSAA, "Drill>CS>AA", "Drill > ChainSaw > Air Anchor", 0, 0, ActionTargets.None, 70)
            .AddOption(ToolOrder.CSAADrill, "CS>AA>Drill", "ChainSaw > Air Anchor > Drill", 0, 0, ActionTargets.None, 70)
            .AddOption(ToolOrder.CSDrillAA, "CS>Drill>AA", "ChainSaw > Drill > Air Anchor", 0, 0, ActionTargets.None, 70)
            .AddAssociatedActions(AID.AirAnchor);
        res.Define(Track.Heat).As<HeatOption>("Heat Option", "Heat", 195)
            .AddOption(HeatOption.Automatic, "Automatic", "Automatically use Heat Blast or Auto-Crossbow based on targets nearby")
            .AddOption(HeatOption.OnlyHeatBlast, "Heat Blast", "Only use Heat Blast, regardless of targets", 0, 0, ActionTargets.Hostile, 35)
            .AddOption(HeatOption.OnlyAutoCrossbow, "Auto Crossbow", "Only use Auto Crossbow, regardless of targets", 0, 0, ActionTargets.Hostile, 52)
            .AddAssociatedActions(AID.HeatBlast, AID.AutoCrossbow, AID.BlazingShot);
        res.Define(Track.Battery).As<BatteryStrategy>("Battery", "", 190)
            .AddOption(BatteryStrategy.Automatic, "Automatic", "Use Battery actions when optimal")
            .AddOption(BatteryStrategy.Fifty, "50", "Use Battery actions ASAP when 50+ Battery Gauge is available", 0, 0, ActionTargets.Self, 40)
            .AddOption(BatteryStrategy.Hundred, "100", "Use Battery actions ASAP when 100 Battery Gauge is available", 0, 0, ActionTargets.Self, 40)
            .AddOption(BatteryStrategy.End, "End", "Ends Battery action ASAP with Overdrive (assuming it's currently active)", 0, 0, ActionTargets.Self, 40)
            .AddOption(BatteryStrategy.Delay, "Delay", "Delay use of Battery actions", 0, 0, ActionTargets.None, 40)
            .AddAssociatedActions(AID.RookAutoturret, AID.RookOverdrive, AID.AutomatonQueen, AID.QueenOverdrive);
        res.Define(Track.Reassemble).As<ReassembleStrategy>("Reassemble", "R.semble", 175)
            .AddOption(ReassembleStrategy.Automatic, "Automatic", "Use Reassemble when optimal")
            .AddOption(ReassembleStrategy.HoldOne, "Hold One", "Hold one charge of Reassemble for manual usage", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.Force, "Force", "Force use of Reassemble, regardless of weaving", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.ForceWeave, "ForceWeave", "Force use of Reassemble in next possible weave window", 55, 5, ActionTargets.Self, 10)
            .AddOption(ReassembleStrategy.Delay, "Delay", "Delay use of Reassemble", 0, 0, ActionTargets.None, 10)
            .AddAssociatedActions(AID.Reassemble);
        res.Define(Track.Hypercharge).As<HyperchargeStrategy>("Hypercharge", "H.charge", 200)
            .AddOption(HyperchargeStrategy.Automatic, "Automatic", "Use Hypercharge when optimal")
            .AddOption(HyperchargeStrategy.ASAP, "ASAP", "Use Hypercharge ASAP (if any Heat Gauge is available)", 0, 0, ActionTargets.Self, 30)
            .AddOption(HyperchargeStrategy.Full, "Full", "Use Hypercharge when Heat Gauge is full (or about to be)", 0, 0, ActionTargets.Self, 30)
            .AddOption(HyperchargeStrategy.Delay, "Delay", "Delay use of Hypercharge", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.Hypercharge);
        res.Define(Track.Drill).As<DrillStrategy>("Drill", "", 170)
            .AddOption(DrillStrategy.Automatic, "Automatic", "Automatically use Drill or Bioblaster based on targets nearby")
            .AddOption(DrillStrategy.OnlyDrill, "Only Drill", "Only use Drill, regardless of targets", 20, 0, ActionTargets.Hostile, 58)
            .AddOption(DrillStrategy.OnlyBioblaster, "Only Bioblaster", "Only use Bioblaster, regardless of targets", 20, 15, ActionTargets.Hostile, 72)
            .AddOption(DrillStrategy.ForceDrill, "Force Drill", "Force use of Drill", 20, 0, ActionTargets.Hostile, 58)
            .AddOption(DrillStrategy.ForceBioblaster, "Force Bioblaster", "Force use of Bioblaster", 20, 15, ActionTargets.Hostile, 72)
            .AddOption(DrillStrategy.Delay, "Delay", "Delay use of Drill", 0, 0, ActionTargets.None, 58)
            .AddAssociatedActions(AID.Drill, AID.Bioblaster);
        res.Define(Track.Wildfire).As<WildfireStrategy>("Wildfire", "W.fire", 165)
            .AddOption(WildfireStrategy.Automatic, "Automatic", "Use Wildfire when optimal")
            .AddOption(WildfireStrategy.End, "End", "End Wildfire early with Detonator", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.Force, "Force", "Force use of Wildfire, regardless of weaving", 120, 10, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.ForceWeave, "ForceWeave", "Force use of Wildfire in next possible weave window", 120, 10, ActionTargets.Hostile, 45)
            .AddOption(WildfireStrategy.Delay, "Delay", "Delay use of Wildfire", 0, 0, ActionTargets.None, 45)
            .AddAssociatedActions(AID.Wildfire, AID.Detonator);
        res.DefineOGCD(Track.BarrelStabilizer, AID.BarrelStabilizer, "Barrel Stabilizer", "B.Stab.", 185, 120, 30, ActionTargets.Self, 66).AddAssociatedActions(AID.BarrelStabilizer);
        res.DefineGCD(Track.AirAnchor, AID.AirAnchor, "Air Anchor", "A.Anchor", 165, 40, 0, ActionTargets.Hostile, 76).AddAssociatedActions(AID.AirAnchor);
        res.DefineGCD(Track.ChainSaw, AID.ChainSaw, "ChainSaw", "C.saw", 160, 60, 30, ActionTargets.Hostile, 90).AddAssociatedActions(AID.ChainSaw);
        res.DefineOGCD(Track.GaussRound, AID.GaussRound, "Gauss Round", "G.Round", uiPriority: 145, 30, 0, ActionTargets.Hostile, 15, 91).AddAssociatedActions(AID.GaussRound);
        res.DefineOGCD(Track.DoubleCheck, AID.DoubleCheck, "Double Check", "D.Check", uiPriority: 144, 30, 0, ActionTargets.Hostile, 92).AddAssociatedActions(AID.DoubleCheck);
        res.DefineOGCD(Track.Ricochet, AID.Ricochet, "Ricochet", "", uiPriority: 141, 30, 0, ActionTargets.Hostile, 50, 91).AddAssociatedActions(AID.Ricochet);
        res.DefineOGCD(Track.Checkmate, AID.Checkmate, "Checkmate", "C.mate", uiPriority: 140, 30, 0, ActionTargets.Hostile, 92).AddAssociatedActions(AID.Checkmate);
        res.DefineGCD(Track.Flamethrower, AID.Flamethrower, "Flamethrower", "F.thrower", -1, 60, 0, ActionTargets.Self, 70).AddAssociatedActions(AID.Flamethrower);
        res.DefineGCD(Track.Excavator, AID.Excavator, "Excavator", "Excav.", uiPriority: 150, 0, 0, ActionTargets.Hostile, 96).AddAssociatedActions(AID.Excavator);
        res.DefineGCD(Track.FullMetalField, AID.FullMetalField, "Full Metal Field", "F.M.Field", uiPriority: 155, 0, 0, ActionTargets.Hostile, 100).AddAssociatedActions(AID.FullMetalField);
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
    private float WFcd; //120s CD
    private float Drillcd; //20s charge CD, 40s total CD (NOTE: can change depending on speed factors like sks and haste)
    private float AAcd; //40s CD
    private float CScd; //60s CD
    private bool ShouldUseAOE;
    private bool ShouldUseRangedAOE;
    private bool ShouldUseSaw;
    private bool ShouldFlamethrower;
    private bool CanAA;
    private bool CanRA;
    private bool CanDC;
    private bool CanCM;
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
    private bool AfterDrill;
    private bool AfterAA;
    private bool AfterCS;
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
    private ToolOrder ToolsOpt;
    private GCDStrategy AAstrat;
    private DrillStrategy Drillstrat;
    private GCDStrategy CSstrat;
    #endregion

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

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.SlugShot or AID.HeatedSlugShot or AID.SplitShot or AID.HeatedSplitShot => ST,
        AID.CleanShot or AID.HeatedCleanShot or AID.Scattergun or AID.SpreadShot or _ => AutoBreak,
    };
    private bool BreakCombo => ComboLastMove == AID.HeatedSlugShot ? NumConeTargets > 3 : ComboLastMove == AID.HeatedSplitShot ? NumConeTargets > 2 : NumConeTargets > 1;
    private AID AutoBreak => BreakCombo ? BestSpreadShot : ST;

    //NOTE: for some reason, combo timer does not default to 0 upon executing combo finishers or certain abilities that do not initiate a combo (e.g. Scattergun, Clean Shot)
    //This is a (hopeful) shitty hack for it to ignore combo timer if these moves are executed
    private bool BeforeComboEnds(float seconds)
        => (ComboTimer > seconds && ((ComboLastMove is not AID.CleanShot or AID.HeatedCleanShot or AID.SpreadShot or AID.Scattergun) || !(LastActionUsed(AID.CleanShot) || LastActionUsed(AID.HeatedCleanShot) || LastActionUsed(AID.Scattergun) || LastActionUsed(AID.SpreadShot)))) ||
        (ComboTimer >= 0 && ((ComboLastMove is AID.CleanShot or AID.HeatedCleanShot or AID.SpreadShot or AID.Scattergun) || (LastActionUsed(AID.CleanShot) || LastActionUsed(AID.HeatedCleanShot) || LastActionUsed(AID.Scattergun) || LastActionUsed(AID.SpreadShot))));
    #endregion

    #region Cooldown Helpers

    #region Buffs
    private bool ShouldUseWildfire(WildfireStrategy strategy, Actor? target) => strategy switch
    {
        WildfireStrategy.Automatic => InsideCombatWith(target) && In25y(target) && CanWF && CanLateWeaveIn && ((CombatTimer is > 6 and < 30 && FMFleft > 0 && EVleft == 0 && Drillcd > 21) || (CombatTimer >= 30 && (LastActionUsed(AID.FullMetalField) || LastActionUsed(AID.Hypercharge) || OverheatActive))),
        WildfireStrategy.End => PlayerHasEffect(SID.WildfirePlayer),
        WildfireStrategy.Force => CanWF,
        WildfireStrategy.ForceWeave => CanWF && CanWeaveIn,
        WildfireStrategy.Delay or _ => false,
    };
    private OGCDPriority WFprio(WildfireStrategy strategy)
    {
        if (strategy == WildfireStrategy.Automatic)
        {
            if (CombatTimer < 30 && FMFleft > 0 && EVleft == 0)
                return OGCDPriority.Max;
            if (CombatTimer >= 30 && (LastActionUsed(AID.FullMetalField) || LastActionUsed(AID.Hypercharge) || OverheatActive))
                return OGCDPriority.Critical;
        }
        return strategy switch
        {
            WildfireStrategy.End => OGCDPriority.Max,
            WildfireStrategy.Force or WildfireStrategy.ForceWeave => OGCDPriority.Forced,
            WildfireStrategy.Delay or _ => OGCDPriority.None,
        };
    }
    private bool ShouldUseBarrelStabilizer(OGCDStrategy strategy, Actor? target)
    {
        if (!CanBS)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In25y(target) && CanWeaveIn && TotalCD(BestGauss) >= 0.6f && TotalCD(BestRicochet) >= 0.6f,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Force => true,
            OGCDStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseReassemble(ReassembleStrategy strategy, Actor? target)
    {
        if (!CanRA)
            return false;
        var d = Unlocked(AID.Drill) ? !Unlocked(TraitID.EnhancedMultiweapon) && ChargeCD(AID.Drill) <= 0.6f : Unlocked(AID.CleanShot) ? NextGCD is AID.CleanShot : TotalCD(AID.HotShot) <= 2f;
        var condition = Player.InCombat && CombatTimer >= 5 && CanWeaveIn && BScd is > 50 or < 5 && (AAcd <= 2f || (CombatTimer < 30 ? d : BScd is < 90 and > 50 && d) || CScd <= 2f || CanEV);
        return strategy switch
        {
            ReassembleStrategy.Automatic => condition,
            ReassembleStrategy.HoldOne => TotalCD(AID.Reassemble) < 20 && condition,
            ReassembleStrategy.Force => true,
            ReassembleStrategy.Delay or _ => false,
        };
    }

    #endregion

    #region Tools
    private void Opener(ToolOrder opt, Actor? target)
    {
        if (CanAA && opt is ToolOrder.AADrillCS or ToolOrder.AACSDrill)
            QueueGCD(BestAirAnchor, target, GCDPriority.VerySevere);
        if (CanDrill && opt is ToolOrder.DrillAACS or ToolOrder.DrillCSAA)
            QueueGCD(AID.Drill, target, GCDPriority.VerySevere);
        if (CanCS && opt is ToolOrder.CSAADrill or ToolOrder.CSDrillAA)
            QueueGCD(AID.ChainSaw, target, GCDPriority.VerySevere);
    }
    private GCDPriority BuffedTool()
    {
        if (RAleft > 0)
        {
            if (AAcd <= 2f)
                return GCDPriority.Severe + 5;
            if (CScd <= 2f)
                return GCDPriority.Severe + 3;
            if (EVleft > 0)
                return GCDPriority.Severe + 2;
            if (!Unlocked(TraitID.EnhancedMultiweapon) && ChargeCD(AID.Drill) <= 0.6f && (WFcd > 110 && CombatTimer > 8))
            {
                return CombatTimer < 30 ? GCDPriority.Severe + 4 : GCDPriority.Severe + 1;
            }
        }
        return GCDPriority.None;
    }
    private bool ShouldUseDrill(DrillStrategy strategy, Actor? target)
    {
        var st = InsideCombatWith(target) && CanDrill && (BScd >= 10 || AAcd > 35 || (AAcd < 5 || CScd < 5) || (WFcd > 105 ? BeforeComboEnds(8) : CanDrill));
        var aoe = InsideCombatWith(target) && CanBB && In12y(target) && !TargetHasEffect(target, SID.Bioblaster);
        return strategy switch
        {
            DrillStrategy.Automatic => ShouldUseAOE ? aoe : st,
            DrillStrategy.OnlyDrill => CanDrill && st,
            DrillStrategy.OnlyBioblaster => CanBB && aoe,
            DrillStrategy.ForceDrill => CanDrill,
            DrillStrategy.ForceBioblaster => CanBB,
            DrillStrategy.Delay or _ => false,
        };
    }
    private GCDPriority Drillprio()
    {
        if (Drillcd is < 20.6f and > 0.6f)
            return GCDPriority.AboveAverage;
        return ToolsOpt switch
        {
            ToolOrder.DrillAACS or ToolOrder.DrillCSAA => GCDPriority.Severe + 6,
            ToolOrder.AADrillCS or ToolOrder.CSDrillAA => GCDPriority.Severe + 4,
            ToolOrder.CSAADrill or ToolOrder.AACSDrill => GCDPriority.Severe + 2,
            _ => GCDPriority.None,
        };
    }
    private bool ShouldUseAirAnchor(GCDStrategy strategy, Actor? target)
    {
        if (!CanAA)
            return false;
        return strategy switch
        {
            GCDStrategy.Automatic => InsideCombatWith(target),
            GCDStrategy.Force => true,
            GCDStrategy.Delay or _ => false,
        };
    }
    private GCDPriority AAprio() => ToolsOpt switch
    {
        ToolOrder.AACSDrill or ToolOrder.AADrillCS => GCDPriority.Severe + 6,
        ToolOrder.DrillAACS or ToolOrder.CSAADrill => GCDPriority.Severe + 4,
        ToolOrder.CSDrillAA or ToolOrder.DrillCSAA => GCDPriority.Severe + 2,
        _ => GCDPriority.None,
    };
    private bool ShouldUseChainSaw(GCDStrategy strategy, Actor? target)
    {
        if (!CanCS)
            return false;
        return strategy switch
        {
            GCDStrategy.Automatic => InsideCombatWith(target),
            GCDStrategy.Force => true,
            GCDStrategy.Delay or _ => false,
        };
    }
    private GCDPriority CSprio() => ToolsOpt switch
    {
        ToolOrder.CSAADrill or ToolOrder.CSDrillAA => GCDPriority.Severe + 6,
        ToolOrder.AACSDrill or ToolOrder.DrillCSAA => GCDPriority.Severe + 4,
        ToolOrder.AADrillCS or ToolOrder.DrillAACS => GCDPriority.Severe + 2,
        _ => GCDPriority.None,
    };

    private bool ShouldUseExcavator(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => InsideCombatWith(target) && In25y(target) && CanEV,
        GCDStrategy.Force => CanEV,
        GCDStrategy.Delay or _ => false,
    };
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

    #region Gauge
    private bool ShouldUseHypercharge(HyperchargeStrategy strategy, Actor? target)
    {
        if (!CanHC)
            return false;
        var afterTools = AfterAA && AfterCS && AfterDrill && EVleft == 0 && FMFleft == 0;
        return strategy switch
        {
            HyperchargeStrategy.Automatic => InsideCombatWith(target) && (afterTools && (CombatTimer <= 30 ? ComboTimer >= 0 : BeforeComboEnds(8)) && ((LastActionUsed(AID.Excavator) || (CScd > 50 && EVleft == 0)) || (LastActionUsed(AID.FullMetalField) || BScd > 90) || (Heat >= 75 && (AAcd >= 18 || CScd >= 18)))) || (Heat == 100 && (AAcd > 9 || CScd > 9 || MaxChargesIn(AID.Drill) > 9)),
            HyperchargeStrategy.ASAP => CanWeaveIn,
            HyperchargeStrategy.Full => CanWeaveIn && Heat >= 100,
            HyperchargeStrategy.Delay or _ => false,
        };
    }
    private bool ShouldChooseHeat(HeatOption strategy, Actor? target)
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
        //TODO: this is a pretty shitty hack-around and will most likely need some refactoring later on
        return strategy switch
        {
            BatteryStrategy.Automatic => CanSummon && ((CombatTimer < 90 && (CombatTimer < 30 ? (LastActionUsed(AID.Excavator) || (CScd > 50 && EVleft == 0)) : Battery >= 90)) ||
                    (CombatTimer >= 90 && (BScd is > 50 && ((AAcd > 35 || LastActionUsed(AID.AirAnchor)) || (LastActionUsed(AID.Excavator) || (CScd > 50 && EVleft == 0)))) ||
                    RaidBuffsLeft > 0 || (Battery >= 90 && (TotalCD(BestAirAnchor) <= 2 || CScd <= 2 || EVleft > 0)) || (Battery == 100 && ComboLastMove is AID.SlugShot or AID.HeatedSlugShot))),
            BatteryStrategy.Fifty => CanSummon,
            BatteryStrategy.Hundred => CanSummon && Battery >= 100,
            BatteryStrategy.End => MinionActive,
            _ => false
        };
    }
    #endregion

    #region Other
    private bool ShouldUseCMDC(bool canUse, int charges, OGCDStrategy strategy, Actor? target)
    {
        if (!canUse)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => In25y(target) && CanLateWeaveIn && ((TargetHPP(target) <= 5 || WFleft > 0 || RaidBuffsLeft > 0 || OverheatActive) ? charges > 0 : charges > 1),
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Force => true,
            OGCDStrategy.Delay or _ => false,
        };
    }
    private OGCDPriority CMDCPriority(int charges) => charges switch
    {
        3 => OGCDPriority.VeryCritical,
        2 => OGCDPriority.Average,
        1 => OGCDPriority.Low,
        _ => OGCDPriority.ExtremelyLow
    };
    private bool ShouldUseCheckmate(OGCDStrategy strategy, Actor? target) => ShouldUseCMDC(CanCM, RicoCharges, strategy, target);
    private OGCDPriority CMprio() => CMDCPriority(RicoCharges);
    private bool ShouldUseDoubleCheck(OGCDStrategy strategy, Actor? target) => ShouldUseCMDC(CanDC, GaussCharges, strategy, target);
    private OGCDPriority DCprio() => CMDCPriority(GaussCharges);
    private bool ShouldUseFlamethrower(GCDStrategy strategy, Actor? target)
    {
        if (!CanFT)
            return false;
        return strategy switch
        {
            GCDStrategy.Automatic => InsideCombatWith(target) && ShouldFlamethrower && In12y(target) && AAcd > 10 && !TargetHasEffect(target, SID.Bioblaster) && CScd > 10 && EVleft == 0 && FMFleft == 0,
            GCDStrategy.Force => true,
            GCDStrategy.Delay or _ => false,
        };
    }
    private bool StopForFlamethrower => Service.Config.Get<MCHConfig>().PauseForFlamethrower && FTleft > 0;
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.Use => true,
        PotionStrategy.Align => BScd < 5f,
        _ => false,
    };
    #endregion

    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<MachinistGauge>();
        Heat = gauge.Heat;
        Battery = gauge.Battery;
        OverheatActive = (gauge.TimerActive & 1) != 0;
        MinionActive = (gauge.TimerActive & 2) != 0;
        RAleft = StatusRemaining(Player, SID.Reassembled);
        HCleft = StatusRemaining(Player, SID.Hypercharged);
        WFleft = StatusRemaining(Player, SID.WildfirePlayer);
        EVleft = StatusRemaining(Player, SID.ExcavatorReady);
        FMFleft = StatusRemaining(Player, SID.FullMetalMachinist);
        FTleft = StatusRemaining(Player, SID.Flamethrower);
        BScd = TotalCD(AID.BarrelStabilizer);
        WFcd = TotalCD(AID.Wildfire);
        Drillcd = TotalCD(AID.Drill);
        AAcd = TotalCD(AID.AirAnchor);
        CScd = TotalCD(AID.ChainSaw);
        AfterDrill = !Unlocked(AID.Drill) || (Unlocked(AID.Drill) && Drillcd >= 9);
        AfterAA = !Unlocked(AID.Drill) || (Unlocked(AID.AirAnchor) && AAcd >= 9);
        AfterCS = !Unlocked(AID.Drill) || (Unlocked(AID.ChainSaw) && CScd >= 9);
        CanHC = ActionReady(AID.Hypercharge) && (Heat >= 50 || HCleft > 0) && RAleft == 0 && !OverheatActive;
        CanHB = Unlocked(AID.HeatBlast) && OverheatActive;
        CanDC = Unlocked(BestGauss) && ChargeCD(BestGauss) <= GCD;
        CanCM = Unlocked(BestRicochet) && ChargeCD(BestRicochet) <= GCD;
        CanSummon = Unlocked(AID.RookAutoturret) && Battery >= 50 && !MinionActive;
        CanWF = ActionReady(AID.Wildfire);
        CanBS = ActionReady(AID.BarrelStabilizer);
        CanRA = Unlocked(AID.Reassemble) && ChargeCD(AID.Reassemble) <= GCD && !OverheatActive && RAleft == 0;
        CanDrill = Unlocked(AID.Drill) && ChargeCD(AID.Drill) < 0.6f && !OverheatActive;
        CanBB = Unlocked(AID.Bioblaster) && ChargeCD(AID.Bioblaster) <= GCD && !OverheatActive;
        CanAA = ActionReady(BestAirAnchor) && !OverheatActive;
        CanCS = ActionReady(AID.ChainSaw) && !OverheatActive;
        CanEV = Unlocked(AID.Excavator) && EVleft > 0 && !OverheatActive;
        CanFMF = Unlocked(AID.FullMetalField) && FMFleft > 0 && !OverheatActive;
        CanFT = ActionReady(AID.Flamethrower) && !OverheatActive && FTleft == 0 && NumFlamethrowerTargets > 2;
        (BestConeTargets, NumConeTargets) = GetBestTarget(primaryTarget, 12, Is12yConeTarget);
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        (BestChainSawTargets, NumChainSawTargets) = GetBestTarget(primaryTarget, 25, Is25yRectTarget);
        NumFlamethrowerTargets = Hints.NumPriorityTargetsInAOECone(Player.Position, 12, Player.Rotation.ToDirection(), 45.Degrees());
        ShouldUseAOE = Unlocked(AID.SpreadShot) && NumConeTargets > 1;
        ShouldUseRangedAOE = Unlocked(AID.Ricochet) && NumSplashTargets > 1;
        ShouldUseSaw = Unlocked(AID.ChainSaw) && NumChainSawTargets > 1;
        ShouldFlamethrower = Unlocked(AID.Flamethrower) && NumFlamethrowerTargets > 2;
        BestConeTarget = ShouldUseAOE ? BestConeTargets : primaryTarget;
        BestSplashTarget = ShouldUseRangedAOE ? BestSplashTargets : primaryTarget;
        BestChainSawTarget = ShouldUseSaw ? BestChainSawTargets : primaryTarget;
        BestFlamethrowerTarget = ShouldFlamethrower ? BestConeTarget : primaryTarget;
        RicoCharges = MaxChargesIn(BestRicochet) <= GCD ? 3 : TotalCD(BestRicochet) < 30.6f ? 2 : TotalCD(BestRicochet) < 60.6f ? 1 : 0;
        GaussCharges = MaxChargesIn(BestGauss) <= GCD ? 3 : TotalCD(BestGauss) < 30.6f ? 2 : TotalCD(BestGauss) < 60.6f ? 1 : 0;

        #region Strategy Definitions
        var AOE = strategy.Option(SharedTrack.AOE);
        var AOEStrategy = AOE.As<AOEStrategy>();
        var pot = strategy.Option(Track.Potion);
        var potStrat = pot.As<PotionStrategy>();
        var tools = strategy.Option(Track.Tools);
        ToolsOpt = tools.As<ToolOrder>();
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
        var ftStrat = ft.As<GCDStrategy>();
        var excavator = strategy.Option(Track.Excavator);
        var excavatorStrat = excavator.As<GCDStrategy>();
        var fmf = strategy.Option(Track.FullMetalField);
        var fmfStrat = fmf.As<GCDStrategy>();
        var cs = strategy.Option(Track.ChainSaw);
        CSstrat = cs.As<GCDStrategy>();
        var aa = strategy.Option(Track.AirAnchor);
        AAstrat = aa.As<GCDStrategy>();
        var drill = strategy.Option(Track.Drill);
        Drillstrat = drill.As<DrillStrategy>();
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
            if (StopForFlamethrower &&
                ((Unlocked(TraitID.EnhancedMultiweapon) ? ChargeCD(AID.Drill) > 0 : Drillcd > 0) || AAcd > 0 || CScd > 0))
                return;

            if (CountdownRemaining == null || CombatTimer == 0)
            {
                if (!Player.InCombat && In25y(primaryTarget?.Actor))
                {
                    if (RAleft == 0 && ActionReady(AID.Reassemble)) //RA first
                        QueueGCD(AID.Reassemble, Player, GCDPriority.Max);
                    if (RAleft > 0)
                        Opener(ToolsOpt, primaryTarget?.Actor);
                }
            }
            if (CountdownRemaining > 0)
            {
                if (CountdownRemaining < 5 && RAleft == 0 && ActionReady(AID.Reassemble))
                    QueueGCD(AID.Reassemble, Player);
                if (ShouldUsePotion(potStrat) && CountdownRemaining <= 1.99f)
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.VeryHigh + (int)GCDPriority.VeryCritical);
                if (CountdownRemaining < 1.15f)
                    Opener(ToolsOpt, primaryTarget?.Actor);
                if (CountdownRemaining > 0)
                    return;
            }
            if (ShouldUsePotion(potStrat))
                Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.VeryHigh + (int)GCDPriority.VeryCritical);

            #endregion

            #region Standard Rotation
            if (!OverheatActive)
            {
                if (AOEStrategy == AOEStrategy.AutoFinish)
                    QueueGCD(AutoFinish, TargetChoice(AOE) ?? BestConeTarget?.Actor, CombatTimer > 90 && ComboTimer is <= 2.5f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
                if (AOEStrategy == AOEStrategy.AutoBreak)
                    QueueGCD(AutoBreak, TargetChoice(AOE) ?? BestConeTarget?.Actor, CombatTimer > 90 && ComboTimer is <= 2.5f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
                if (AOEStrategy == AOEStrategy.ForceST)
                    QueueGCD(ST, TargetChoice(AOE) ?? primaryTarget?.Actor, CombatTimer > 90 && ComboTimer is <= 2.5f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
                if (AOEStrategy == AOEStrategy.ForceAOE)
                    QueueGCD(BestSpreadShot, TargetChoice(AOE) ?? BestConeTarget?.Actor, CombatTimer > 90 && ComboTimer is <= 2.5f and not 0 ? GCDPriority.High + 1 : GCDPriority.Low);
            }
            #endregion

            #region Cooldowns
            if (!strategy.HoldAbilities())
            {
                if (!strategy.HoldCDs())
                {
                    if (!strategy.HoldBuffs())
                    {
                        if (ShouldUseWildfire(wfStrat, primaryTarget?.Actor))
                        {
                            if (wfStrat == WildfireStrategy.End)
                                QueueOGCD(AID.Detonator, TargetChoice(wf) ?? primaryTarget?.Actor, OGCDPriority.Max);
                            else
                                QueueOGCD(AID.Wildfire, TargetChoice(wf) ?? primaryTarget?.Actor, WFprio(wfStrat));
                        }
                        if (ShouldUseBarrelStabilizer(bsStrat, primaryTarget?.Actor))
                            QueueOGCD(AID.BarrelStabilizer, TargetChoice(bs) ?? primaryTarget?.Actor, OGCDPrio(bsStrat, OGCDPriority.Severe));
                        if (ShouldUseReassemble(assembleStrat, primaryTarget?.Actor))
                            QueueOGCD(AID.Reassemble, Player, OGCDPriority.Critical);
                    }
                    if (ShouldUseAirAnchor(AAstrat, primaryTarget?.Actor))
                        QueueGCD(BestAirAnchor, primaryTarget?.Actor, AAstrat is GCDStrategy.Force ? GCDPriority.Forced : AAprio());
                    if (ShouldUseChainSaw(CSstrat, primaryTarget?.Actor))
                        QueueGCD(AID.ChainSaw, BestChainSawTarget?.Actor, CSstrat is GCDStrategy.Force ? GCDPriority.Forced : CSprio());
                    if (ShouldUseDrill(Drillstrat, primaryTarget?.Actor))
                    {
                        if (Drillstrat is DrillStrategy.Automatic)
                            QueueGCD(BestDrill, (ShouldUseAOE ? BestConeTarget?.Actor : primaryTarget?.Actor), Drillprio());
                        if (Drillstrat is DrillStrategy.OnlyDrill or DrillStrategy.ForceDrill)
                            QueueGCD(AID.Drill, primaryTarget?.Actor, Drillstrat == DrillStrategy.ForceDrill ? GCDPriority.Forced : Drillprio());
                        if (Drillstrat is DrillStrategy.OnlyBioblaster or DrillStrategy.ForceBioblaster)
                            QueueGCD(AID.Bioblaster, BestConeTarget?.Actor, Drillstrat == DrillStrategy.ForceBioblaster ? GCDPriority.Forced : Drillprio());
                    }
                    if (ShouldUseExcavator(excavatorStrat, primaryTarget?.Actor))
                        QueueGCD(AID.Excavator, TargetChoice(excavator) ?? BestSplashTarget?.Actor, excavatorStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.High);
                    if (ShouldUseFullMetalField(fmfStrat, primaryTarget?.Actor))
                        QueueGCD(AID.FullMetalField, TargetChoice(fmf) ?? BestSplashTarget?.Actor, fmfStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.SlightlyHigh);
                    if (Unlocked(TraitID.DoubleBarrelMastery) ? ShouldUseDoubleCheck(dcStrat, primaryTarget?.Actor) : ShouldUseDoubleCheck(gaussStrat, primaryTarget?.Actor))
                        QueueOGCD(BestGauss, TargetChoice(gauss) ?? (Unlocked(AID.DoubleCheck) ? BestSplashTarget?.Actor : primaryTarget?.Actor), Unlocked(TraitID.DoubleBarrelMastery) ? OGCDPrio(dcStrat, DCprio()) : OGCDPrio(gaussStrat, DCprio()));
                    if (Unlocked(TraitID.DoubleBarrelMastery) ? ShouldUseCheckmate(cmStrat, primaryTarget?.Actor) : ShouldUseCheckmate(ricochetStrat, primaryTarget?.Actor))
                        QueueOGCD(BestRicochet, TargetChoice(ricochet) ?? BestSplashTarget?.Actor, Unlocked(TraitID.DoubleBarrelMastery) ? OGCDPrio(cmStrat, CMprio()) : OGCDPrio(ricochetStrat, CMprio()));
                    if (ShouldUseFlamethrower(ftStrat, primaryTarget?.Actor))
                        QueueGCD(AID.Flamethrower, TargetChoice(ft) ?? BestFlamethrowerTarget?.Actor, ftStrat is GCDStrategy.Force ? GCDPriority.Forced : GCDPriority.ModeratelyLow);
                }
                if (!strategy.HoldGauge())
                {
                    if (ShouldUseHypercharge(hcStrat, primaryTarget?.Actor))
                        QueueOGCD(AID.Hypercharge, TargetChoice(hc) ?? primaryTarget?.Actor, hcStrat == HyperchargeStrategy.ASAP ? OGCDPriority.Forced : (Heat >= 100 || LastActionUsed(AID.Excavator) || LastActionUsed(AID.FullMetalField)) ? OGCDPriority.VerySevere : OGCDPriority.High);
                    if (ShouldUseBattery(batteryStrat))
                    {
                        if (batteryStrat is BatteryStrategy.Automatic or BatteryStrategy.Fifty or BatteryStrategy.Hundred)
                            QueueOGCD(BestBattery, Player, OGCDPriority.Critical);
                        if (batteryStrat == BatteryStrategy.End)
                            QueueOGCD(Unlocked(AID.QueenOverdrive) ? AID.QueenOverdrive : AID.RookOverdrive, Player, OGCDPriority.Critical);
                    }
                }
                if (ShouldChooseHeat(hspOpt, primaryTarget?.Actor))
                {
                    if (hspOpt == HeatOption.Automatic)
                        QueueGCD(BestHeat, TargetChoice(hsp) ?? primaryTarget?.Actor, GCDPriority.High);
                    if (hspOpt == HeatOption.OnlyHeatBlast)
                        QueueGCD(BestHeatBlast, TargetChoice(hsp) ?? primaryTarget?.Actor, GCDPriority.High);
                    if (hspOpt == HeatOption.OnlyAutoCrossbow)
                        QueueGCD(Unlocked(AID.AutoCrossbow) ? AID.AutoCrossbow : BestHeatBlast, TargetChoice(hsp) ?? BestConeTarget?.Actor, GCDPriority.High);
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
