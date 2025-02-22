using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.WAR;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiWAR(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Gauge = SharedTrack.Count, SurgingTempest, Infuriate, PrimalRend, Upheaval, Onslaught, Tomahawk, Potion, InnerRelease, PrimalWrath, PrimalRuination }
    public enum GaugeStrategy { Automatic, OnlyST, OnlyAOE, ForceST, ForceAOE, Conserve }
    public enum SurgingTempestStrategy { Automatic, At30s, ForceEye, ForcePath, Delay }
    public enum InfuriateStrategy { Automatic, Force, ForceOvercap, Delay }
    public enum PrimalRendStrategy { Automatic, ASAP, ASAPNotMoving, AfterBF, LastSecond, GapClose, Force, Delay }
    public enum UpheavalStrategy { Automatic, OnlyUpheaval, OnlyOrogeny, ForceUpheaval, ForceOrogeny, Delay }
    public enum OnslaughtStrategy { Automatic, Force, Hold0, Hold1, Hold2, GapClose, Delay }
    public enum TomahawkStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi WAR", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)|Tank", //Category
            "Akechi", //Contributor
            RotationModuleQuality.Ok, //Quality
            BitMask.Build(Class.MRD, Class.WAR), //Job
            100); //Level supported

        res.DefineAOE().AddAssociatedActions(AID.HeavySwing, AID.Maim, AID.StormEye, AID.StormPath, AID.Overpower, AID.MythrilTempest);
        res.DefineHold();
        res.Define(Track.Gauge).As<GaugeStrategy>("Gauge", "Gauge", uiPriority: 200)
            .AddOption(GaugeStrategy.Automatic, "Automatic", "Automatically use Gauge-related abilities optimally", minLevel: 35)
            .AddOption(GaugeStrategy.OnlyST, "Only ST", "Uses Inner Beast / Fell Cleave / Inner Chaos optimally as Beast Gauge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 35)
            .AddOption(GaugeStrategy.OnlyAOE, "Only AOE", "Uses Steel Cyclone / Decimate / Chaotic Cyclone optimally as Beast Gauge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(GaugeStrategy.ForceST, "Force ST", "Force use Inner Beast / Fell Cleave / Inner Chaos", 0, 0, ActionTargets.Hostile, 35)
            .AddOption(GaugeStrategy.ForceAOE, "Force AOE", "Force use Steel Cyclone / Decimate / Chaotic Cyclone", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(GaugeStrategy.Conserve, "Conserve", "Conserves all Gauge-related abilities as much as possible", 0, 0, ActionTargets.None, 35)
            .AddAssociatedActions(AID.InnerBeast, AID.FellCleave, AID.InnerChaos, AID.Decimate, AID.ChaoticCyclone);
        res.Define(Track.SurgingTempest).As<SurgingTempestStrategy>("Surging Tempest", "S.Tempest", uiPriority: 200)
            .AddOption(SurgingTempestStrategy.Automatic, "Auto", "Automatically refreshes Surging Tempest when 10s or less on its duration", minLevel: 50)
            .AddOption(SurgingTempestStrategy.At30s, "At 30s", "Refresh Surging Tempest at less than or equal to 30s", 0, 10, ActionTargets.Hostile, 50)
            .AddOption(SurgingTempestStrategy.ForceEye, "Force Eye", "Force use Storm's Eye as combo ender", 0, 10, ActionTargets.Hostile, 50)
            .AddOption(SurgingTempestStrategy.ForcePath, "Force Path", "Force use Storm's Path as combo ender, essentially delaying Surging Tempest ", 0, 10, ActionTargets.Hostile, 26)
            .AddAssociatedActions(AID.StormEye, AID.StormPath);
        res.Define(Track.Infuriate).As<InfuriateStrategy>("Infuriate", "Infuriate", uiPriority: 190)
            .AddOption(InfuriateStrategy.Automatic, "Auto", "Automatically decide when to use Infuriate", minLevel: 50)
            .AddOption(InfuriateStrategy.Force, "Force", "Force use Infuriate ASAP if not under Nascent Chaos effect", 0, 60, ActionTargets.Self, 50)
            .AddOption(InfuriateStrategy.ForceOvercap, "Force Overcap", "Force use Infuriate to prevent overcap on charges if not under Nascent Chaos effect", 0, 60, ActionTargets.Self, 50)
            .AddOption(InfuriateStrategy.Delay, "Delay", "Delay use of Infuriate to prevent overcap on charges", 0, 60, ActionTargets.None, 50)
            .AddAssociatedActions(AID.Infuriate);
        res.Define(Track.PrimalRend).As<PrimalRendStrategy>("Primal Rend", "P.Rend", uiPriority: 180)
            .AddOption(PrimalRendStrategy.Automatic, "Auto", "Automatically decide when to use Primal Rend", minLevel: 90)
            .AddOption(PrimalRendStrategy.ASAP, "ASAP", "Use Primal Rend ASAP after Inner Release", 0, 20, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.ASAPNotMoving, "Force Not Moving", "Use Primal Rend ASAP after Inner Release when not moving", 0, 20, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.AfterBF, "After BF", "Use Primal Rend after consuming all stacks of Burgeoning Fury; if failed to consume all stacks, will use last second", 0, 20, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.LastSecond, "Last Second", "Force use Primal Rend on last second", 0, 20, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.GapClose, "Gap Close", "Use as gapcloser when outside melee range", 0, 20, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.Force, "Force", "Force use Primal Rend", 0, 20, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.Delay, "Delay", "Delay use of Primal Rend", 0, 0, ActionTargets.Hostile, 90)
            .AddAssociatedActions(AID.PrimalRend);
        res.Define(Track.Upheaval).As<UpheavalStrategy>("Upheaval", "Upheaval", uiPriority: 170)
            .AddOption(UpheavalStrategy.Automatic, "Auto", "Automatically decide when to use Upheaval or Orogeny", minLevel: 64)
            .AddOption(UpheavalStrategy.OnlyUpheaval, "Only Upheaval", "Uses Upheaval optimally as optimal spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(UpheavalStrategy.OnlyOrogeny, "Only Orogeny", "Uses Orogeny optimally as optimal spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 86)
            .AddOption(UpheavalStrategy.ForceUpheaval, "Force Upheaval", "Force use Upheaval", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(UpheavalStrategy.ForceOrogeny, "Force Orogeny", "Force use Orogeny", 0, 0, ActionTargets.Hostile, 86)
            .AddOption(UpheavalStrategy.Delay, "Delay", "Delay use of Upheaval", 0, 0, ActionTargets.None, 64)
            .AddAssociatedActions(AID.Upheaval, AID.Orogeny);
        res.Define(Track.Onslaught).As<OnslaughtStrategy>("Onslaught", "Onslaught", uiPriority: 170)
            .AddOption(OnslaughtStrategy.Automatic, "Auto", "Automatically decide when to use Onslaught; 1 for 1 minute, 3 for 2 minute", minLevel: 62)
            .AddOption(OnslaughtStrategy.Force, "Force", "Force use Onslaught", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(OnslaughtStrategy.Hold0, "Force All", "Force use Onslaught; holds no charges", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(OnslaughtStrategy.Hold1, "Hold 1", "Force use Onslaught; holds 1 charge", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(OnslaughtStrategy.Hold2, "Hold 2", "Force use Onslaught; holds 2 charges", 0, 0, ActionTargets.Hostile, 88)
            .AddOption(OnslaughtStrategy.GapClose, "Gap Close", "Use as gapcloser when outside melee range", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(OnslaughtStrategy.Delay, "Delay", "Delay use of Onslaught", 0, 0, ActionTargets.None, 62)
            .AddAssociatedActions(AID.Onslaught);
        res.Define(Track.Tomahawk).As<TomahawkStrategy>("Ranged", "Ranged", uiPriority: 30)
            .AddOption(TomahawkStrategy.OpenerFar, "Far (Opener)", "Use Tomahawk in pre-pull & out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(TomahawkStrategy.OpenerForce, "Force (Opener)", "Force use Tomahawk in pre-pull in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(TomahawkStrategy.Force, "Force", "Force use Tomahawk in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(TomahawkStrategy.Allow, "Allow", "Allow use of Tomahawk when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(TomahawkStrategy.Forbid, "Forbid", "Prohibit use of Tomahawk")
            .AddAssociatedActions(AID.Tomahawk);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 20)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with Inner Release & Infuriate charges to ensure use on 2-minute windows", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.DefineOGCD(Track.InnerRelease, AID.InnerRelease, "Inner Release", "InnerR.", uiPriority: 170, 60, 15, ActionTargets.Self, 6).AddAssociatedActions(AID.InnerRelease);
        res.DefineOGCD(Track.PrimalWrath, AID.PrimalWrath, "Primal Wrath", "P.Wrath", uiPriority: 135, 20, 0, ActionTargets.Hostile, 96).AddAssociatedActions(AID.PrimalWrath);
        res.DefineGCD(Track.PrimalRuination, AID.PrimalRuination, "PrimalRuination", "P.Ruin.", uiPriority: 150, supportedTargets: ActionTargets.Hostile, minLevel: 100).AddAssociatedActions(AID.PrimalRuination);

        return res;
    }
    #endregion

    #region Priorities
    //TODO: I am too lazy to convert this
    private NewGCDPriority FellCleave()
    {
        var ncActive = CanFitSkSGCD(NascentChaos.Left);
        if (ncActive)
        {
            var prExpiringSoon = CanFitSkSGCD(PrimalRend.Left) && !CanFitSkSGCD(PrimalRend.Left, 2);
            if (!CanFitSkSGCD(NascentChaos.Left, prExpiringSoon ? 2 : 1))
                return NewGCDPriority.LastChanceIC;
        }

        var irActive = CanFitSkSGCD(InnerRelease.Left);
        var effectiveIRStacks = InnerRelease.Stacks + (ncActive ? 1 : 0);
        if (irActive && !CanFitSkSGCD(InnerRelease.Left, effectiveIRStacks))
            return NewGCDPriority.LastChanceFC;

        var needFCBeforeInf = ncActive || BeastGauge > 50;
        if (needFCBeforeInf && !CanFitSkSGCD(Infuriate.TotalCD - (Unlocked(TraitID.EnhancedInfuriate) ? 5 : 0) - SkSGCDLength, 1))
            return NewGCDPriority.AvoidOvercapInfuriateNext;

        var numFCBeforeInf = InnerRelease.Stacks + ((ncActive || BeastGauge > 50) ? 1 : 0);
        if (irActive && !CanFitSkSGCD(InnerRelease.Left, numFCBeforeInf + 1) && !CanFitSkSGCD(Infuriate.TotalCD - (Unlocked(TraitID.EnhancedInfuriate) ? 5 : 0) * numFCBeforeInf - SkSGCDLength, numFCBeforeInf))
            return NewGCDPriority.AvoidOvercapInfuriateIR;

        var imminentIRStacks = ncActive ? 4 : 3;
        if (needFCBeforeInf && !CanFitSkSGCD(InnerRelease.CD, 1) && !CanFitSkSGCD(Infuriate.TotalCD - (Unlocked(TraitID.EnhancedInfuriate) ? 5 : 0) * imminentIRStacks - SkSGCDLength, imminentIRStacks))
            return NewGCDPriority.AvoidOvercapInfuriateIR;

        if (CanFitSkSGCD(BurstWindowLeft))
            return irActive ? NewGCDPriority.BuffedIR : NewGCDPriority.BuffedFC;

        if (irActive)
        {
            var maxFillers = (int)((InnerRelease.Left - GCD) / SkSGCDLength) + 1 - effectiveIRStacks;
            var canDelayFC = maxFillers > 0 && !CanFitSkSGCD(BurstWindowIn, maxFillers);
            return canDelayFC ? NewGCDPriority.DelayFC : NewGCDPriority.FlexibleIR;
        }
        else if (ncActive)
        {
            return NascentChaos.Left > BurstWindowIn ? NewGCDPriority.DelayFC : NewGCDPriority.FlexibleFC;
        }
        else
        {
            return NewGCDPriority.DelayFC;
        }
    }
    private enum NewGCDPriority
    {
        None = 0,
        Gauge = 300,
        DelayFC = 390,
        Standard = 400,
        FlexibleFC = 470,
        FlexibleIR = 490,
        PrimalRend = 500,
        PrimalRuination = 500,
        BuffedFC = 550,
        BuffedIR = 570,
        NeedTempest = 650,
        AvoidDropCombo = 660,
        AvoidOvercapInfuriateIR = 670,
        AvoidOvercapInfuriateNext = 680,
        NeedGauge = 700,
        LastChanceFC = 770,
        LastChanceIC = 780,
        Opener = 800,
        ForcedTomahawk = 870,
        ForcedCombo = 880,
        ForcedPR = 890,
        ForcedGCD = 900,
        GapclosePR = 990,
    }
    public enum NewOGCDPriority
    {
        None = 0,
        Standard = 100,
        Onslaught = 500,
        PrimalWrath = 550,
        Infuriate = 570,
        Upheaval = 580,
        InnerRelease = 590,
        Potion = 900,
        Gapclose = 980,
        ForcedOGCD = 1100, //Enough to put it past CDPlanner's "Automatic" priority, which is really only Medium priority
    }
    #endregion

    #region Upgrade Paths
    private AID BestFellCleave => Unlocked(AID.FellCleave) ? AID.FellCleave : AID.InnerBeast;
    private AID BestDecimate => Unlocked(AID.Decimate) ? AID.Decimate : (ShouldUseAOECircle(5).OnFourOrMore || !Unlocked(AID.FellCleave)) ? AID.SteelCyclone : AID.FellCleave;
    private AID BestGaugeSpender => ShouldUseAOE ? BestDecimate : BestFellCleave;
    private AID BestInnerRelease => Unlocked(AID.InnerRelease) ? AID.InnerRelease : AID.Berserk;
    private AID UpheavalOrOrogeny => ShouldUseAOE ? BestOrogeny : AID.Upheaval;
    private AID BestOrogeny => Unlocked(AID.Orogeny) ? AID.Orogeny : AID.Upheaval;
    private SID BestBerserk => Unlocked(AID.InnerRelease) ? SID.InnerRelease : SID.Berserk;
    #endregion

    #region Module Variables
    public float TwoMinuteLeft;
    public float TwoMinuteIn;
    public float BurstWindowLeft;
    public float BurstWindowIn;
    public bool ForceEye;
    public bool ForcePath;
    public bool KeepAt30s;
    public byte BeastGauge;
    public bool ShouldUseAOE;
    public int NumSplashTargets;
    public Enemy? BestSplashTargets;
    public Enemy? BestSplashTarget;
    public (float CD, bool IsReady) Upheaval;
    public (float CD, bool IsReady) Orogeny;
    public (float Left, int Stacks) BurgeoningFury;
    public (float Left, bool IsActive) NascentChaos;
    public (float CD, bool IsReady) Onslaught;
    public (float Left, bool IsActive, bool IsReady) PrimalRend;
    public (float Left, bool IsActive, bool IsReady) PrimalWrath;
    public (float Left, bool IsActive, bool IsReady) PrimalRuination;
    public (float Left, bool IsActive, bool NeedsRefresh, bool KeepAt30s) SurgingTempest;
    public (float TotalCD, float ChargeCD, bool HasCharges, bool IsReady) Infuriate;
    public (float Left, int Stacks, float CD, bool IsActive, bool IsReady) InnerRelease;
    #endregion

    #region Module Helpers
    public bool IsRiskingGauge()
    {
        if (InnerRelease.Stacks > 0)
            return true;
        if (BeastGauge >= 90 && //if 90
            ComboLastMove is AID.Maim) //next is Storm's Path, which overcaps. We need spender here
            return true;
        if (BeastGauge >= 100)
        {
            if (Unlocked(TraitID.MasteringTheBeast) &&
                ComboLastMove is AID.Overpower)
                return true;
            if (ComboLastMove is AID.HeavySwing)
                return true;
        }
        if (NascentChaos.IsActive && //if NC is active
            InnerRelease.CD > 5) //and IR is not imminent
            return true;
        return false;
    }
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables

        #region Strategy Definitions
        var bg = strategy.Option(Track.Gauge);
        var bgStrat = bg.As<GaugeStrategy>(); //Retrieve Gauge strategy
        var st = strategy.Option(Track.SurgingTempest);
        var stStrat = st.As<SurgingTempestStrategy>(); //Retrieve SurgingTempest strategy
        var uo = strategy.Option(Track.Upheaval);
        var uoStrat = uo.As<UpheavalStrategy>(); //Retrieve Upheaval strategy
        var ir = strategy.Option(Track.InnerRelease);
        var irStrat = ir.As<OGCDStrategy>(); //Retrieve InnerRelease strategy
        var inf = strategy.Option(Track.Infuriate);
        var infStrat = inf.As<InfuriateStrategy>(); //Retrieve Infuriate strategy
        var prend = strategy.Option(Track.PrimalRend);
        var prendStrat = prend.As<PrimalRendStrategy>(); //Retrieve InnerRelease combo strategy
        var pwrath = strategy.Option(Track.PrimalWrath);
        var pwrathStrat = pwrath.As<OGCDStrategy>(); //Retrieve PrimalWrath strategy       
        var pruin = strategy.Option(Track.PrimalRuination);
        var pruinStrat = pruin.As<GCDStrategy>(); //Retrieve PrimalRuination strategy
        var ons = strategy.Option(Track.Onslaught);
        var onsStrat = ons.As<OnslaughtStrategy>(); //Retrieve Onslaught strategy
        var Tomahawk = strategy.Option(Track.Tomahawk);
        var TomahawkStrat = Tomahawk.As<TomahawkStrategy>(); //Retrieve Tomahawk strategy
        ForceEye = stStrat is SurgingTempestStrategy.ForceEye;
        ForcePath = stStrat is SurgingTempestStrategy.ForcePath;
        KeepAt30s = stStrat is SurgingTempestStrategy.At30s;
        #endregion

        #region Gauge
        var gauge = World.Client.GetGauge<WarriorGauge>(); //Retrieve WAR gauge
        BeastGauge = gauge.BeastGauge;
        #endregion

        #region Cooldowns
        SurgingTempest.Left = StatusRemaining(Player, SID.SurgingTempest, 60); //Retrieve current SurgingTempest time left
        SurgingTempest.IsActive = SurgingTempest.Left > 0.1f; //Checks if SurgingTempest is active
        SurgingTempest.KeepAt30s = SurgingTempest.Left <= 30; //Checks if SurgingTempest needs to be refreshed once less than 30s
        //TODO: optimize
        SurgingTempest.NeedsRefresh = ShouldRefreshTempest(stStrat); //Checks if SurgingTempest needs to be refreshed, roughly 4 GCDs to refresh it 

        Upheaval.CD = TotalCD(AID.Upheaval); //Retrieve current Upheaval cooldown
        Upheaval.IsReady = Unlocked(AID.Upheaval) && Upheaval.CD < 0.6f; //Upheaval ability

        Orogeny.CD = TotalCD(AID.Orogeny); //Retrieve current Orogeny cooldown
        Orogeny.IsReady = Unlocked(AID.Orogeny) && Orogeny.CD < 0.6f; //Orogeny ability

        BurgeoningFury.Stacks = StacksRemaining(Player, SID.BurgeoningFury, 30); //Retrieve current BurgeoningFury stacks

        PrimalRend.Left = StatusRemaining(Player, SID.PrimalRend, 20); //Retrieve current Primal Rend time left
        PrimalRend.IsActive = PrimalRend.Left > 0.1f; //Checks if Primal Rend is active
        PrimalRend.IsReady = Unlocked(AID.PrimalRend) && PrimalRend.Left > 0.1f; //Primal Rend ability

        PrimalWrath.Left = StatusRemaining(Player, SID.Wrathful, 30); //Retrieve current Primal Wrath time left
        PrimalWrath.IsActive = PrimalWrath.Left > 0.1f; //Checks if Primal Wrath is active
        PrimalWrath.IsReady = Unlocked(AID.PrimalWrath) && PrimalWrath.Left > 0.1f; //Primal Wrath ability

        PrimalRuination.Left = StatusRemaining(Player, SID.PrimalRuinationReady, 20); //Retrieve current Primal Ruination time left
        PrimalRuination.IsActive = PrimalRuination.Left > 0.1f; //Checks if Primal Ruination is active
        PrimalRuination.IsReady = Unlocked(AID.PrimalRuination) && PrimalRuination.Left > 0.1f; //Primal Ruination ability

        InnerRelease.Stacks = StacksRemaining(Player, BestBerserk, 15); //Retrieve current InnerRelease stacks
        InnerRelease.CD = TotalCD(BestInnerRelease); //Retrieve current InnerRelease cooldown
        InnerRelease.IsActive = InnerRelease.Stacks > 0; //Checks if InnerRelease is active
        InnerRelease.IsReady = Unlocked(BestInnerRelease) && InnerRelease.CD < 0.6f; //InnerRelease ability

        NascentChaos.Left = StatusRemaining(Player, SID.NascentChaos, 30);
        NascentChaos.IsActive = NascentChaos.Left > 0.1f;

        Onslaught.CD = TotalCD(AID.Onslaught); //Retrieve current Onslaught cooldown
        Onslaught.IsReady = Unlocked(AID.Onslaught) && Onslaught.CD < 60.6f; //Onslaught ability

        Infuriate.TotalCD = TotalCD(AID.Infuriate); //Retrieve current Infuriate cooldown
        Infuriate.HasCharges = Infuriate.TotalCD <= 60; //Checks if Infuriate has charges
        Infuriate.IsReady = Unlocked(AID.Infuriate) && Infuriate.HasCharges && !PlayerHasEffect(SID.NascentChaos, 30); //Infuriate ability
        Infuriate.ChargeCD = Infuriate.TotalCD * 0.5f;  // This gives 60s for one charge
        if (Unlocked(TraitID.EnhancedInfuriate))
        {
            if (LastActionUsed(AID.FellCleave) || LastActionUsed(AID.Decimate) || LastActionUsed(AID.InnerChaos) || LastActionUsed(AID.ChaoticCyclone))
            {
                Infuriate.TotalCD -= 5f;
            }
            //If the cooldown drops to 0, but TotalCD isn't 0, reset ChargeCD to 60
            //Technically, this should mean that charges are not capped, and therefore the timer is still rolling
            if (Infuriate.ChargeCD <= 0 && Infuriate.TotalCD > 0)
            {
                Infuriate.ChargeCD = 60f;
            }
        }
        #endregion

        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore;

        BurstWindowLeft = (InnerRelease.CD >= 40) ? 1.0f : 0.0f;
        BurstWindowIn = (InnerRelease.CD == 0) ? 1.0f : 0.0f;
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.PrimalRend) && NumSplashTargets >= 2 ? BestSplashTargets : primaryTarget;
        (TwoMinuteLeft, TwoMinuteIn) = EstimateRaidBuffTimings(primaryTarget?.Actor);
        #endregion

        #region Full Rotation Execution

        #region Standard Rotations
        if (strategy.AutoFinish())
            QueueGCD(BestRotation(),
                TargetChoice(strategy.Option(SharedTrack.AOE)) ?? primaryTarget?.Actor,
                IsRiskingGauge() ? NewGCDPriority.Standard - 400 : NewGCDPriority.Standard);
        if (strategy.AutoBreak())
            QueueGCD(ShouldUseAOE ? AOE() : ST(),
                TargetChoice(strategy.Option(SharedTrack.AOE)) ?? primaryTarget?.Actor,
                IsRiskingGauge() ? NewGCDPriority.Standard - 400 : NewGCDPriority.Standard);
        if (strategy.ForceST())
            QueueGCD(ST(),
                TargetChoice(strategy.Option(SharedTrack.AOE)) ?? primaryTarget?.Actor,
                IsRiskingGauge() ? NewGCDPriority.Standard - 400 : NewGCDPriority.ForcedCombo);
        if (strategy.ForceAOE())
            QueueGCD(AOE(),
                Player,
                IsRiskingGauge() ? NewGCDPriority.Standard - 400 : NewGCDPriority.ForcedCombo);
        #endregion

        #region Cooldowns
        if (!strategy.HoldAll()) //if not holding cooldowns
        {
            if (!strategy.HoldCDs()) //if holding cooldowns
            {
                if (!strategy.HoldBuffs())
                {
                    if (ShouldUseInnerRelease(irStrat, primaryTarget))
                        QueueOGCD(BestInnerRelease,
                            Player,
                            irStrat is OGCDStrategy.Force
                            or OGCDStrategy.AnyWeave
                            or OGCDStrategy.EarlyWeave
                            or OGCDStrategy.LateWeave
                            ? NewOGCDPriority.ForcedOGCD
                            : NewOGCDPriority.InnerRelease);
                }
                if (ShouldUseUpheavalOrOrogeny(uoStrat, primaryTarget))
                {
                    if (uoStrat is UpheavalStrategy.Automatic)
                        QueueOGCD(UpheavalOrOrogeny,
                            TargetChoice(uo) ?? primaryTarget?.Actor,
                            uoStrat is UpheavalStrategy.ForceUpheaval
                            or UpheavalStrategy.ForceOrogeny
                            ? NewOGCDPriority.ForcedOGCD
                            : NewOGCDPriority.Upheaval);
                    if (uoStrat is UpheavalStrategy.OnlyUpheaval)
                        QueueOGCD(AID.Upheaval,
                            TargetChoice(uo) ?? primaryTarget?.Actor,
                            uoStrat is UpheavalStrategy.ForceUpheaval
                            ? NewOGCDPriority.ForcedOGCD
                            : NewOGCDPriority.Upheaval);
                    if (uoStrat is UpheavalStrategy.OnlyOrogeny)
                        QueueOGCD(BestOrogeny,
                            TargetChoice(uo) ?? primaryTarget?.Actor,
                            uoStrat is UpheavalStrategy.ForceOrogeny
                            ? NewOGCDPriority.ForcedOGCD
                            : NewOGCDPriority.Upheaval);
                }
                if (ShouldUseInfuriate(infStrat, primaryTarget))
                    QueueOGCD(AID.Infuriate,
                        Player,
                        infStrat is InfuriateStrategy.Force
                        or InfuriateStrategy.ForceOvercap
                        ? NewOGCDPriority.ForcedOGCD
                        : NewOGCDPriority.Infuriate);

                if (ShouldUsePrimalRend(prendStrat, primaryTarget))
                    QueueGCD(AID.PrimalRend,
                        TargetChoice(prend) ?? BestSplashTarget?.Actor,
                        prendStrat is PrimalRendStrategy.Force
                        or PrimalRendStrategy.ASAP
                        or PrimalRendStrategy.ASAPNotMoving
                        ? NewGCDPriority.ForcedGCD
                        : NewGCDPriority.PrimalRend);

                if (ShouldUsePrimalWrath(pwrathStrat, primaryTarget))
                    QueueGCD(AID.PrimalWrath,
                        TargetChoice(pwrath) ?? BestSplashTarget?.Actor,
                        pwrathStrat is OGCDStrategy.Force
                        or OGCDStrategy.AnyWeave
                        or OGCDStrategy.EarlyWeave
                        or OGCDStrategy.LateWeave
                        ? NewOGCDPriority.ForcedOGCD
                        : NewOGCDPriority.PrimalWrath);

                if (ShouldUsePrimalRuination(pruinStrat, primaryTarget))
                    QueueGCD(AID.PrimalRuination,
                        TargetChoice(pruin) ?? BestSplashTarget?.Actor,
                        pruinStrat is GCDStrategy.Force
                        ? NewGCDPriority.ForcedGCD
                        : NewGCDPriority.PrimalRuination);
                if (ShouldUseOnslaught(onsStrat, primaryTarget))
                    QueueOGCD(AID.Onslaught,
                        TargetChoice(ons) ?? primaryTarget?.Actor,
                        onsStrat is OnslaughtStrategy.Force
                        or OnslaughtStrategy.GapClose
                        ? NewOGCDPriority.ForcedOGCD
                        : NewOGCDPriority.Standard);
            }
            if (!strategy.HoldGauge())
            {
                if (ShouldUseGauge(bgStrat, primaryTarget))
                {
                    if (bgStrat is GaugeStrategy.Automatic)
                        QueueGCD(BestGaugeSpender,
                            TargetChoice(bg) ?? primaryTarget?.Actor,
                            bgStrat is GaugeStrategy.ForceST
                            or GaugeStrategy.ForceAOE
                            ? NewGCDPriority.ForcedGCD
                            : FellCleave());
                    if (bgStrat is GaugeStrategy.OnlyST)
                        QueueGCD(AID.FellCleave,
                            TargetChoice(bg) ?? primaryTarget?.Actor,
                            bgStrat is GaugeStrategy.ForceST
                            ? NewGCDPriority.ForcedGCD
                            : IsRiskingGauge()
                            ? NewGCDPriority.NeedGauge
                            : NewGCDPriority.Gauge);
                    if (bgStrat is GaugeStrategy.OnlyAOE)
                        QueueGCD(AID.Decimate,
                            Unlocked(AID.Decimate)
                            ? Player
                            : TargetChoice(bg) ?? primaryTarget?.Actor,
                            bgStrat is GaugeStrategy.ForceAOE
                            ? NewGCDPriority.ForcedGCD
                            : IsRiskingGauge()
                            ? NewGCDPriority.NeedGauge
                            : NewGCDPriority.Gauge);
                }
            }
        }
        if (ShouldUseTomahawk(TomahawkStrat, primaryTarget))
            QueueGCD(AID.Tomahawk,
                TargetChoice(Tomahawk) ?? primaryTarget?.Actor,
                NewGCDPriority.Standard);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr,
                Player,
                ActionQueue.Priority.VeryHigh + (int)NewOGCDPriority.ForcedOGCD,
                0,
                GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        var goalST = primaryTarget?.Actor != null ? Hints.GoalSingleTarget(primaryTarget!.Actor, 3) : null; //Set goal for single target
        var goalAOE = Hints.GoalAOECircle(3); //Set goal for AOE
        var goal = strategy.Option(SharedTrack.AOE).As<AOEStrategy>() switch //Set goal based on AOE strategy
        {
            AOEStrategy.ForceST => goalST, //if forced single target
            AOEStrategy.ForceAOE => goalAOE, //if forced AOE
            _ => goalST != null ? Hints.GoalCombined(goalST, goalAOE, 3) : goalAOE //otherwise, combine goals
        };
        if (goal != null) //if goal is set
            Hints.GoalZones.Add(goal); //add goal to zones
        #endregion
    }

    #region Rotation Helpers
    private AID BestRotation() => ComboLastMove switch
    {
        AID.StormEye => ShouldUseAOE ? AOE() : ST(),
        AID.StormPath => ShouldUseAOE ? AOE() : ST(),
        AID.Maim => ST(),
        AID.HeavySwing => ST(),
        AID.MythrilTempest => ShouldUseAOE ? AOE() : ST(),
        AID.Overpower => AOE(),
        _ => ShouldUseAOE ? AOE() : ST(),
    };
    private AID ST() => ComboLastMove switch
    {
        AID.Maim => ForceEye ? AID.StormEye : ForcePath ? AID.StormPath : SurgingTempest.NeedsRefresh ? AID.StormEye : AID.StormPath,
        AID.HeavySwing => AID.Maim,
        _ => AID.HeavySwing,
    };
    private AID AOE() => ComboLastMove switch
    {
        AID.Overpower => AID.MythrilTempest,
        _ => AID.Overpower,
    };
    private int GaugeGainedFromAction(AID aid) => aid switch
    {
        AID.Maim or AID.StormEye => 10,
        AID.StormPath => 20,
        AID.MythrilTempest => Unlocked(TraitID.MasteringTheBeast) ? 20 : 0,
        _ => 0
    };

    #endregion

    #region Cooldown Helpers
    private bool ShouldUseGauge(GaugeStrategy strategy, Enemy? target) => strategy switch
    {
        GaugeStrategy.Automatic => ShouldSpendGauge(GaugeStrategy.Automatic, target),
        GaugeStrategy.OnlyST => ShouldSpendGauge(GaugeStrategy.Automatic, target),
        GaugeStrategy.OnlyAOE => ShouldSpendGauge(GaugeStrategy.Automatic, target),
        GaugeStrategy.ForceST => Unlocked(AID.FellCleave) && (BeastGauge >= 50 || InnerRelease.IsActive || PlayerHasEffect(SID.NascentChaos, 30)),
        GaugeStrategy.ForceAOE => Unlocked(AID.Decimate) && (BeastGauge >= 50 || InnerRelease.IsActive || PlayerHasEffect(SID.NascentChaos, 30)),
        GaugeStrategy.Conserve => false,
        _ => false
    };
    private bool ShouldSpendGauge(GaugeStrategy strategy, Enemy? target) => strategy switch
    {
        GaugeStrategy.Automatic => Player.InCombat && target != null && In3y(target?.Actor) && Unlocked(BestFellCleave) && SurgingTempest.IsActive && (BeastGauge >= 50 || InnerRelease.Stacks > 0),
        _ => false
    };
    public bool ShouldDumpGauge(Enemy? target) => BeastGauge >= 50 &&
        (TargetHPP(target?.Actor) <= 3 || InnerRelease.CD <= (SkSGCDLength * 2) + 0.5f && !NascentChaos.IsActive);

    public bool ShouldRefreshTempest(SurgingTempestStrategy strategy)
    {
        if (!Unlocked(AID.StormEye) || strategy is SurgingTempestStrategy.Delay || strategy is SurgingTempestStrategy.ForcePath)
            return false;

        if (strategy is SurgingTempestStrategy.Automatic)
            return SurgingTempest.Left <= 10;

        if (strategy is SurgingTempestStrategy.At30s)
            return SurgingTempest.Left <= 30;

        if (strategy is SurgingTempestStrategy.ForceEye)
            return SurgingTempest.Left >= 0;

        return false;
    }
    private bool ShouldUseUpheavalOrOrogeny(UpheavalStrategy strategy, Enemy? target) => strategy switch
    {
        UpheavalStrategy.Automatic => ShouldSpendUpheavalOrOrogeny(UpheavalStrategy.Automatic, target),
        UpheavalStrategy.OnlyUpheaval => ShouldSpendUpheavalOrOrogeny(UpheavalStrategy.Automatic, target),
        UpheavalStrategy.OnlyOrogeny => ShouldSpendUpheavalOrOrogeny(UpheavalStrategy.Automatic, target),
        UpheavalStrategy.ForceUpheaval => Upheaval.IsReady,
        UpheavalStrategy.ForceOrogeny => Orogeny.IsReady,
        UpheavalStrategy.Delay => false,
        _ => false
    };
    private bool ShouldSpendUpheavalOrOrogeny(UpheavalStrategy strategy, Enemy? target) => strategy switch
    {
        UpheavalStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && In3y(target?.Actor) && SurgingTempest.IsActive &&
            Upheaval.IsReady && (CombatTimer < 30 && ComboLastMove is AID.StormEye || CombatTimer >= 30),
        _ => false
    };
    private bool ShouldUseInnerRelease(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && SurgingTempest.IsActive && InnerRelease.IsReady,
        OGCDStrategy.Force => InnerRelease.IsReady,
        OGCDStrategy.AnyWeave => InnerRelease.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => InnerRelease.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => InnerRelease.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseInfuriate(InfuriateStrategy strategy, Enemy? target)
    {
        if (strategy == InfuriateStrategy.Delay || CanFitSkSGCD(NascentChaos.Left))
            return false;
        if (strategy == InfuriateStrategy.Force)
            return true;
        if (strategy == InfuriateStrategy.ForceOvercap && Infuriate.TotalCD <= World.Client.AnimationLock)
            return true;

        if (BeastGauge > 50)
            return false;
        if (target == null)
            return false;

        var irActive = CanFitSkSGCD(InnerRelease.Left);
        if (!Unlocked(AID.InnerRelease))
        {
            if (irActive)
                return true;

            if (!CanFitSkSGCD(Infuriate.TotalCD, 4))
                return true;

            return false;
        }

        var unlockedNC = Unlocked(AID.ChaoticCyclone);
        if (unlockedNC && irActive && !CanFitSkSGCD(InnerRelease.Left, InnerRelease.Stacks))
            return false;

        var maxInfuriateCD = GCD + SkSGCDLength;
        if (BeastGauge + GaugeGainedFromAction(NextGCD) > 50)
        {
            var numFCsToBurnGauge = 1;
            if (irActive)
                numFCsToBurnGauge += InnerRelease.Stacks;
            else if (!CanFitSkSGCD(InnerRelease.CD, 1))
                numFCsToBurnGauge += 3;
            maxInfuriateCD += (SkSGCDLength + (Unlocked(TraitID.EnhancedInfuriate) ? 5 : 0)) * numFCsToBurnGauge;
        }
        if (NextGCD is AID.FellCleave or AID.InnerBeast or AID.SteelCyclone or AID.Decimate)
        {
            maxInfuriateCD += (Unlocked(TraitID.EnhancedInfuriate) ? 5 : 0);
        }
        if (Infuriate.TotalCD < maxInfuriateCD)
            return true;

        if (irActive && unlockedNC)
            return false;

        var (gaugeGained, costedGCDs) = NextGCD switch
        {
            AID.HeavySwing => (20, 3),
            AID.Maim => (20, 2),
            AID.StormEye => (10, 1),
            AID.Overpower => (20, 2),
            AID.MythrilTempest => (20, 1),
            _ => (30, 4)
        };
        if (BeastGauge + gaugeGained + 50 > 100 && !CanFitSkSGCD(SurgingTempest.Left, costedGCDs))
            return false;

        return CanFitSkSGCD(BurstWindowLeft);
    }
    private bool ShouldUsePrimalRend(PrimalRendStrategy strategy, Enemy? target) => strategy switch
    {
        PrimalRendStrategy.Automatic => Player.InCombat && target != null && PrimalRend.IsReady && InnerRelease.Stacks <= 2,
        PrimalRendStrategy.ASAP => PrimalRend.IsReady,
        PrimalRendStrategy.ASAPNotMoving => PrimalRend.IsReady && !IsMoving,
        PrimalRendStrategy.AfterBF => PrimalRend.IsReady && InnerRelease.Stacks == 0 && BurgeoningFury.Stacks == 0,
        PrimalRendStrategy.LastSecond => PrimalRend.Left <= SkSGCDLength + 0.5f,
        PrimalRendStrategy.GapClose => !In3y(target?.Actor),
        PrimalRendStrategy.Force => PrimalRend.IsReady,
        PrimalRendStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUsePrimalWrath(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && PrimalWrath.IsReady,
        OGCDStrategy.Force => PrimalWrath.IsReady,
        OGCDStrategy.AnyWeave => PrimalWrath.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => PrimalWrath.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => PrimalWrath.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUsePrimalRuination(GCDStrategy strategy, Enemy? target) => strategy switch
    {
        GCDStrategy.Automatic => Player.InCombat && target != null && In3y(target?.Actor) && PrimalRuination.IsReady,
        GCDStrategy.Force => PrimalRuination.IsReady,
        GCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseOnslaught(OnslaughtStrategy strategy, Enemy? target) => strategy switch
    {
        OnslaughtStrategy.Automatic => Player.InCombat && target != null && In3y(target?.Actor) && Unlocked(AID.Onslaught) && Onslaught.CD <= 60.5f && InnerRelease.IsActive,
        OnslaughtStrategy.Hold0 => Unlocked(AID.Onslaught) && Onslaught.IsReady,
        OnslaughtStrategy.Hold1 => Unlocked(AID.Onslaught) && Onslaught.CD <= 30.5f,
        OnslaughtStrategy.Hold2 => Unlocked(AID.Onslaught) && Onslaught.CD < 0.6f,
        OnslaughtStrategy.GapClose => !In3y(target?.Actor),
        OnslaughtStrategy.Force => Unlocked(AID.Onslaught) && Onslaught.IsReady,
        OnslaughtStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseTomahawk(TomahawkStrategy strategy, Enemy? target) => strategy switch
    {
        TomahawkStrategy.OpenerFar => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD() && !In3y(target?.Actor),
        TomahawkStrategy.OpenerForce => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD(),
        TomahawkStrategy.Force => true,
        TomahawkStrategy.Allow => !In3y(target?.Actor),
        TomahawkStrategy.Forbid => false,
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => InnerRelease.CD < 5,
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion
}
