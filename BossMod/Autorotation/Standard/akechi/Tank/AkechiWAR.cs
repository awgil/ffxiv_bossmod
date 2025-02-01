using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.WAR;
using System.IO;

namespace BossMod.Autorotation.Standard.akechi.Tank;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiWAR(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Gauge = SharedTrack.Count, SurgingTempest, Infuriate, PrimalRend, Upheaval, Onslaught, Tomahawk, Potion, InnerRelease, PrimalWrath, PrimalRuination }
    public enum GaugeStrategy { Automatic, OnlyST, OnlyAOE, ForceST, ForceAOE, Conserve }
    public enum SurgingTempestStrategy { Automatic, At30s, LastSecond, ForceEye, ForcePath, Delay }
    public enum InfuriateStrategy { Automatic, Force, ForceOvercap, Delay }
    public enum PrimalRendStrategy { Automatic, ASAP, ASAPNotMoving, AfterBF, LastSecond, GapClose, Force, Delay }
    public enum UpheavalStrategy { Automatic, OnlyUpheaval, OnlyOrogeny, ForceUpheaval, ForceOrogeny, Delay }
    public enum OnslaughtStrategy { Automatic, Force, ForceAll, Hold1, Hold2, GapClose, Delay }
    public enum TomahawkStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    #endregion

    #region Module Definitions & Strategies
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi WAR", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.WAR), 100);

        res.DefineShared();
        res.Define(Track.Gauge).As<GaugeStrategy>("Gauge", "Gauge", uiPriority: 200)
            .AddOption(GaugeStrategy.Automatic, "Automatic", "Automatically use Gauge-related abilities optimally", minLevel: 35)
            .AddOption(GaugeStrategy.OnlyST, "Only ST", "Uses Inner Beast / Fell Cleave / Inner Chaos optimally as Beast Gauge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 35)
            .AddOption(GaugeStrategy.OnlyAOE, "Only AOE", "Uses Steel Cyclone / Decimate / Chaotic Cyclone optimally as Beast Gauge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(GaugeStrategy.ForceST, "Force ST", "Force use Inner Beast / Fell Cleave / Inner Chaos", 0, 0, ActionTargets.Hostile, 35)
            .AddOption(GaugeStrategy.ForceAOE, "Force AOE", "Force use Steel Cyclone / Decimate / Chaotic Cyclone", 0, 0, ActionTargets.Hostile, 45)
            .AddOption(GaugeStrategy.Conserve, "Conserve", "Conserves all Gauge-related abilities as much as possible", 0, 0, ActionTargets.None, 35)
            .AddAssociatedActions(AID.InnerBeast, AID.FellCleave, AID.InnerChaos, AID.Decimate, AID.ChaoticCyclone);
        res.Define(Track.SurgingTempest).As<SurgingTempestStrategy>("Surging Tempest", "S.Tempest", uiPriority: 200)
            .AddOption(SurgingTempestStrategy.Automatic, "Auto", "Automatically refreshes Surging Tempest when 8s or less on its duration", minLevel: 50)
            .AddOption(SurgingTempestStrategy.At30s, "At 30s", "Refresh Surging Tempest at less than or equal to 30s", 0, 10, ActionTargets.Hostile, 50)
            .AddOption(SurgingTempestStrategy.LastSecond, "Last Second", "Refresh Surging Tempest at last possible second without dropping it", 0, 10, ActionTargets.Hostile, 50)
            .AddOption(SurgingTempestStrategy.ForceEye, "Force Eye", "Force use Storm's Eye as combo ender", 0, 10, ActionTargets.Hostile, 50)
            .AddOption(SurgingTempestStrategy.ForcePath, "Force Path", "Force use Storm's Path as combo ender, essentially delaying use of Surging Temest ", 0, 10, ActionTargets.Hostile, 26)
            .AddAssociatedActions(AID.StormEye, AID.StormPath);
        res.Define(Track.Infuriate).As<InfuriateStrategy>("Infuriate", "Infuriate", uiPriority: 190)
            .AddOption(InfuriateStrategy.Automatic, "Auto", "Automatically decide when to use Infuriate", minLevel: 50)
            .AddOption(InfuriateStrategy.Force, "Force", "Force use Infuriate ASAP if not under Nascent Chaos effect", 0, 60, ActionTargets.Self, 50)
            .AddOption(InfuriateStrategy.ForceOvercap, "Force Overcap", "Force use Infuriate to prevent overcap on charges if not under Nascent Chaos effect", 0, 60, ActionTargets.Self, 50)
            .AddOption(InfuriateStrategy.Delay, "Delay", "Delay use of Infuriate to prevent overcap on charges", 0, 60, ActionTargets.None, 50)
            .AddAssociatedActions(AID.Infuriate);
        res.Define(Track.PrimalRend).As<PrimalRendStrategy>("Primal Rend", "Primal", uiPriority: 180)
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
            .AddOption(OnslaughtStrategy.ForceAll, "Force All", "Force use Onslaught; holds no charges", 0, 0, ActionTargets.Hostile, 62)
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
        res.DefineOGCD(Track.InnerRelease, "Inner Release", "InnerR.", uiPriority: 170, 60, 15, ActionTargets.Self, 6).AddAssociatedActions(AID.InnerRelease);
        res.DefineOGCD(Track.PrimalWrath, "Primal Wrath", "P.Wrath", uiPriority: 135, 20, 0, ActionTargets.Hostile, 96).AddAssociatedActions(AID.PrimalWrath);
        res.DefineGCD(Track.PrimalRuination, "PrimalRuination", "P.Ruin.", uiPriority: 150, supportedTargets: ActionTargets.Hostile, minLevel: 100).AddAssociatedActions(AID.PrimalRuination);

        return res;
    }
    #endregion

    #region Priorities
    public enum GCDPriority
    {
        None = 0,
        Standard = 100,
        Gauge = 300,
        PrimalRuination = 400,
        PrimalRend = 500,
        NeedTempest = 650,
        NeedGauge = 700,
        Opener = 800,
        ForcedGCD = 900,
    }
    public enum OGCDPriority
    {
        None = 0,
        Standard = 100,
        UpheavalOrOrogeny = 400,
        PrimalWrath = 500,
        Infuriate = 600,
        InnerRelease = 700,
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
    public byte BeastGauge;
    public bool RiskingGauge;
    public bool ForceEye;
    public bool ForcePath;
    public (float Left, bool IsActive, bool NeedsRefresh) SurgingTempest;
    public (float CD, bool IsReady) Upheaval;
    public (float CD, bool IsReady) Orogeny;
    public (float Left, int Stacks) BurgeoningFury;
    public (float Left, bool IsActive, bool IsReady) PrimalRend;
    public (float Left, bool IsActive, bool IsReady) PrimalWrath;
    public (float Left, bool IsActive, bool IsReady) PrimalRuination;
    public (float TotalCD, float ChargeCD, bool HasCharges, bool IsReady) Infuriate;
    public (float Left, int Stacks, float CD, bool IsActive, bool IsReady) InnerRelease;
    public bool ShouldUseAOE;
    public int NumSplashTargets;
    public Enemy? BestSplashTargets;
    public Enemy? BestSplashTarget;
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget) //Executes our actions
    {
        #region Variables

        #region Gauge
        var gauge = World.Client.GetGauge<WarriorGauge>(); //Retrieve WAR gauge
        BeastGauge = gauge.BeastGauge;
        RiskingGauge =
            ComboLastMove is AID.Maim or AID.Overpower or AID.StormEye && BeastGauge >= 90 ||
            ComboLastMove is AID.StormPath or AID.MythrilTempest && BeastGauge >= 90 ||
            ComboLastMove is AID.HeavySwing && BeastGauge == 100 ||
            TargetHPP(primaryTarget?.Actor) <= 5 && BeastGauge >= 50;
        #endregion

        #region Cooldowns
        SurgingTempest.Left = StatusRemaining(Player, SID.SurgingTempest, 60); //Retrieve current SurgingTempest time left
        SurgingTempest.IsActive = SurgingTempest.Left > 0.1f; //Checks if SurgingTempest is active
        SurgingTempest.NeedsRefresh = SurgingTempest.Left <= ((SkSGCDLength * 3) + 0.5f); //Checks if SurgingTempest needs to be refreshed
        Upheaval.CD = TotalCD(AID.Upheaval); //Retrieve current Upheaval cooldown
        Upheaval.IsReady = Unlocked(AID.Upheaval) && Upheaval.CD < 0.6f; //Upheaval ability
        Orogeny.CD = TotalCD(AID.Orogeny); //Retrieve current Orogeny cooldown
        Orogeny.IsReady = Unlocked(AID.Orogeny) && Orogeny.CD < 0.6f; //Orogeny ability
        BurgeoningFury.Left = StatusRemaining(Player, SID.BurgeoningFury, 30); //Retrieve current BurgeoningFury time left
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
        InnerRelease.Left = StatusRemaining(Player, BestBerserk, 15); //Retrieve current InnerRelease time left
        InnerRelease.Stacks = StacksRemaining(Player, BestBerserk, 15); //Retrieve current InnerRelease stacks
        InnerRelease.CD = TotalCD(BestInnerRelease); //Retrieve current InnerRelease cooldown
        InnerRelease.IsActive = InnerRelease.Left > 0.1f; //Checks if InnerRelease is active
        InnerRelease.IsReady = Unlocked(BestInnerRelease) && InnerRelease.CD < 0.6f; //InnerRelease ability
        Infuriate.TotalCD = TotalCD(AID.Infuriate); //Retrieve current Infuriate cooldown
        Infuriate.HasCharges = Infuriate.TotalCD <= 60; //Checks if Infuriate has charges
        Infuriate.IsReady = Unlocked(AID.Infuriate) && Infuriate.HasCharges; //Infuriate ability
        Infuriate.ChargeCD = Infuriate.TotalCD * 0.5f;  // This gives 60s for one charge
        if (Unlocked(TraitID.EnhancedInfuriate))
        {
            if (LastActionUsed(AID.FellCleave) || LastActionUsed(AID.Decimate) || LastActionUsed(AID.InnerChaos) || LastActionUsed(AID.ChaoticCyclone))
            {
                Infuriate.TotalCD -= 5f;  // Reduce by 5 seconds
            }
            // If the cooldown drops to 0, but TotalCD isn't 0, reset ChargeCD to 60
            // Technically, this should mean that charges are not capped, so therefore the timer is still rolling
            if (Infuriate.ChargeCD <= 0 && Infuriate.TotalCD > 0)
            {
                Infuriate.ChargeCD = 60f;  // Reset to 60 seconds for the next charge
            }
        }
        #endregion

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
        var infStrat = inf.As<OGCDStrategy>(); //Retrieve Infuriate strategy
        var prend = strategy.Option(Track.PrimalRend);
        var prendStrat = prend.As<PrimalRendStrategy>(); //Retrieve InnerRelease combo strategy
        var pwrath = strategy.Option(Track.PrimalWrath);
        var pwrathStrat = pwrath.As<OGCDStrategy>(); //Retrieve PrimalWrath strategy       
        var pruin = strategy.Option(Track.PrimalRuination);
        var pruinStrat = pruin.As<GCDStrategy>(); //Retrieve PrimalRuination strategy
        var Tomahawk = strategy.Option(Track.Tomahawk);
        var TomahawkStrat = Tomahawk.As<TomahawkStrategy>(); //Retrieve Tomahawk strategy
        ForceEye = stStrat is SurgingTempestStrategy.ForceEye;
        ForcePath = stStrat is SurgingTempestStrategy.ForcePath;
        #endregion
        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore;
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.PrimalRend) && NumSplashTargets >= 2 ? BestSplashTargets : primaryTarget;
        #endregion

        #region Full Rotation Execution

        #region Standard Rotations
        if (strategy.Automatic())
        {
            QueueGCD(BestRotation(), //queue the next single-target combo action only if combo is finished
                TargetChoice(strategy.Option(SharedTrack.AOE)) //Get target choice
                ?? primaryTarget?.Actor, //if none, pick primary target
                GCDPriority.Standard); //with priority for 123/10 combo actions
        }
        if (strategy.ForceST()) //if Force Single Target option is picked
        {
            QueueGCD(ST(),
                TargetChoice(strategy.Option(SharedTrack.AOE)) //Get target choice
                ?? primaryTarget?.Actor, //if none, pick primary target
                GCDPriority.Standard); //with priority for 123/10 combo actions
        }
        if (strategy.ForceAOE()) //if Force AOE option is picked
        {
            QueueGCD(AOE(),
                Player,
                GCDPriority.Standard); //with priority for 123/10 combo actions
        }
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
                            ? OGCDPriority.ForcedOGCD
                            : OGCDPriority.InnerRelease);
                }
                if (ShouldUseUpheavalOrOrogeny(uoStrat, primaryTarget))
                {
                    if (uoStrat is UpheavalStrategy.Automatic)
                        QueueOGCD(UpheavalOrOrogeny,
                            TargetChoice(uo) ?? primaryTarget?.Actor,
                            uoStrat is UpheavalStrategy.ForceUpheaval
                            or UpheavalStrategy.ForceOrogeny
                            ? OGCDPriority.ForcedOGCD
                            : OGCDPriority.UpheavalOrOrogeny);
                    if (uoStrat is UpheavalStrategy.OnlyUpheaval)
                        QueueOGCD(AID.Upheaval,
                            TargetChoice(uo) ?? primaryTarget?.Actor,
                            uoStrat is UpheavalStrategy.ForceUpheaval
                            ? OGCDPriority.ForcedOGCD
                            : OGCDPriority.UpheavalOrOrogeny);
                    if (uoStrat is UpheavalStrategy.OnlyOrogeny)
                        QueueOGCD(BestOrogeny,
                            TargetChoice(uo) ?? primaryTarget?.Actor,
                            uoStrat is UpheavalStrategy.ForceOrogeny
                            ? OGCDPriority.ForcedOGCD
                            : OGCDPriority.UpheavalOrOrogeny);
                }
                if (ShouldUseInfuriate(infStrat, primaryTarget))
                    QueueOGCD(AID.Infuriate,
                        Player,
                        infStrat is OGCDStrategy.Force
                        or OGCDStrategy.AnyWeave
                        or OGCDStrategy.EarlyWeave
                        or OGCDStrategy.LateWeave
                        ? OGCDPriority.ForcedOGCD
                        : OGCDPriority.Infuriate);

                if (ShouldUsePrimalRend(prendStrat, primaryTarget))
                    QueueGCD(AID.PrimalRuination,
                        TargetChoice(prend) ?? BestSplashTarget?.Actor,
                        prendStrat is PrimalRendStrategy.Force
                        or PrimalRendStrategy.ASAP
                        or PrimalRendStrategy.ASAPNotMoving
                        ? GCDPriority.ForcedGCD
                        : GCDPriority.PrimalRend);

                if (ShouldUsePrimalWrath(pwrathStrat, primaryTarget))
                    QueueGCD(AID.PrimalRuination,
                        TargetChoice(pwrath) ?? BestSplashTarget?.Actor,
                        pwrathStrat is OGCDStrategy.Force
                        or OGCDStrategy.AnyWeave
                        or OGCDStrategy.EarlyWeave
                        or OGCDStrategy.LateWeave
                        ? OGCDPriority.ForcedOGCD
                        : OGCDPriority.PrimalWrath);

                if (ShouldUsePrimalRuination(pruinStrat, primaryTarget))
                    QueueGCD(AID.PrimalRuination,
                        TargetChoice(pruin) ?? BestSplashTarget?.Actor,
                        pruinStrat is GCDStrategy.Force
                        ? GCDPriority.ForcedGCD
                        : GCDPriority.PrimalRuination);
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
                            ? GCDPriority.ForcedGCD
                            : RiskingGauge
                            ? GCDPriority.NeedGauge
                            : GCDPriority.Gauge);
                    if (bgStrat is GaugeStrategy.OnlyST)
                        QueueGCD(AID.FellCleave,
                            TargetChoice(bg) ?? primaryTarget?.Actor,
                            bgStrat is GaugeStrategy.ForceST
                            ? GCDPriority.ForcedGCD
                            : RiskingGauge
                            ? GCDPriority.NeedGauge
                            : GCDPriority.Gauge);
                    if (bgStrat is GaugeStrategy.OnlyAOE)
                        QueueGCD(AID.Decimate,
                            Unlocked(AID.Decimate)
                            ? Player
                            : TargetChoice(bg) ?? primaryTarget?.Actor,
                            bgStrat is GaugeStrategy.ForceAOE
                            ? GCDPriority.ForcedGCD
                            : RiskingGauge
                            ? GCDPriority.NeedGauge
                            : GCDPriority.Gauge);
                }
            }
        }
        if (ShouldUseTomahawk(TomahawkStrat, primaryTarget))
            QueueGCD(AID.Tomahawk,
                TargetChoice(Tomahawk) ?? primaryTarget?.Actor,
                GCDPriority.Standard);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr,
                Player,
                ActionQueue.Priority.VeryHigh + (int)OGCDPriority.ForcedOGCD,
                0,
                GCD - 0.9f);
        #endregion

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
        GaugeStrategy.Automatic => Player.InCombat && target != null && In3y(target?.Actor) &&
            BeastGauge >= 50 && Unlocked(BestFellCleave) && (RiskingGauge || InnerRelease.CD >= 39.5f),
        _ => false
    };
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
        UpheavalStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn && In3y(target?.Actor) &&
            Upheaval.IsReady && (CombatTimer < 30 && ComboLastMove is AID.StormEye || CombatTimer >= 30),
        _ => false
    };
    private bool ShouldUseInnerRelease(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn &&
            InnerRelease.IsReady && (CombatTimer < 30 && ComboLastMove is AID.StormEye || CombatTimer >= 30),
        OGCDStrategy.Force => InnerRelease.IsReady,
        OGCDStrategy.AnyWeave => InnerRelease.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => InnerRelease.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => InnerRelease.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseInfuriate(OGCDStrategy strategy, Enemy? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && target != null && CanWeaveIn &&
            Infuriate.IsReady && !PlayerHasEffect(SID.NascentChaos, 30) && BeastGauge <= 40,
        OGCDStrategy.Force => Infuriate.IsReady,
        OGCDStrategy.AnyWeave => Infuriate.IsReady && CanWeaveIn,
        OGCDStrategy.EarlyWeave => Infuriate.IsReady && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => Infuriate.IsReady && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
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
        GCDStrategy.Automatic => Player.InCombat && target != null && In10y(target?.Actor) &&
            PrimalRuination.IsReady && (CombatTimer < 30 && InnerRelease.IsActive || CombatTimer >= 30 && !InnerRelease.IsActive && InnerRelease.CD > 15),
        GCDStrategy.Force => PrimalRuination.IsReady,
        GCDStrategy.Delay => false,
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
