using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.GNB;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiGNBPvP(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)

{
    #region Enums: Abilities / Strategies
    public enum Track
    {
        Burst,
        Combo,
        RelentlessRush,
        TerminalTrigger,
        GnashingFang,
        FatedCircle,
        RoughDivide,
        Zone,
        Corundum,
    }

    public enum BurstStrategy
    {
        Automatic,
        Force,
        Hold
    }

    public enum ComboStrategy
    {
        Automatic,
        Force,
        Hold
    }

    public enum RushStrategy
    {
        Automatic,
        Force,
        Hold
    }

    public enum TriggerStrategy
    {
        Automatic,
        Force,
        Hold
    }

    public enum ElixirStrategy
    {
        Automatic,
        Close,
        Mid,
        Far,
        Force,
        Hold
    }

    public enum OffensiveStrategy
    {
        Automatic,
        Force,
        Delay
    }

    #endregion

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB (PvP)", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.GNB), 100, 30);

        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 190)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Use everything optimally")
            .AddOption(BurstStrategy.Force, "Force", "Force everything")
            .AddOption(BurstStrategy.Hold, "Hold", "Hold everything");

        res.Define(Track.Combo).As<ComboStrategy>("Combo", uiPriority: 190)
            .AddOption(ComboStrategy.Automatic, "Automatic", "Use combo optimally")
            .AddOption(ComboStrategy.Force, "Force", "Force combo")
            .AddOption(ComboStrategy.Hold, "Hold", "Hold combo");

        res.Define(Track.RelentlessRush).As<RushStrategy>("Relentless Rush", uiPriority: 190)
            .AddOption(RushStrategy.Automatic, "Automatic", "Use Relentless Rush optimally")
            .AddOption(RushStrategy.Force, "Force", "Force Relentless Rush")
            .AddOption(RushStrategy.Hold, "Hold", "Hold Relentless Rush");

        res.Define(Track.TerminalTrigger).As<TriggerStrategy>("Terminal Trigger", uiPriority: 190)
            .AddOption(TriggerStrategy.Automatic, "Automatic", "Use Terminal Trigger optimally")
            .AddOption(TriggerStrategy.Force, "Force", "Force Terminal Trigger")
            .AddOption(TriggerStrategy.Hold, "Hold", "Hold Terminal Trigger");

        res.Define(Track.GnashingFang).As<OffensiveStrategy>("Gnashing Fang", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.GnashingFangPvP);

        res.Define(Track.FatedCircle).As<OffensiveStrategy>("Fated Circle", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.FatedCirclePvP);

        res.Define(Track.RoughDivide).As<OffensiveStrategy>("Rough Divide", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.RoughDividePvP);

        res.Define(Track.Zone).As<OffensiveStrategy>("Zone", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.BlastingZonePvP);

        res.Define(Track.Corundum).As<OffensiveStrategy>("Corundum", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.HeartOfCorundumPvP);

        return res;
    }

    #region Priorities
    public enum GCDPriority
    {
        None = 0,
        KeenEdge = 325,
        BrutalShell = 320,
        SolidBarrel = 315,
        BurstStrike = 340,
        Combo = 350,
        GnashingFang = 400,
        FatedCircle = 450,
        ForcedGCD = 900,
    }

    public enum OGCDPriority
    {
        None = 0,
        RoughDivide = 400,
        Zone = 420,
        Corundum = 440,
        Continuation = 500,
        LB = 600,
        ForcedOGCD = 900,
    }
    #endregion

    #region Placeholders for Variables
    private float nmLeft;
    private float rdCD;
    private bool hasNM;
    private bool hasBlast;
    private bool hasRaze;
    private bool hasRip;
    private bool hasTear;
    private bool hasGouge;
    private bool canGF;
    private bool canFC;
    private bool canZone;
    private bool canHyper;
    private bool canBrand;
    private bool canRip;
    private bool canTear;
    private bool canGouge;
    public bool LBready;
    public float GFcomboStep;
    public float comboStep;
    public bool inCombo;
    public bool inGF;
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        GetPvPTarget(ref primaryTarget, 3);

        #region Variables
        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        var GunStep = gauge.AmmoComboStep;
        rdCD = TotalCD(AID.RoughDividePvP);
        nmLeft = StatusRemaining(Player, SID.NoMercyPvP, 7);
        hasNM = nmLeft > 0;
        hasBlast = PlayerHasEffect(SID.ReadyToBlastPvP);
        hasRaze = PlayerHasEffect(SID.ReadyToRazePvP);
        hasRip = PlayerHasEffect(SID.ReadyToRipPvP) || GunStep == 1;
        hasTear = PlayerHasEffect(SID.ReadyToTearPvP) || GunStep == 2;
        hasGouge = PlayerHasEffect(SID.ReadyToGougePvP);
        LBready = World.Party.LimitBreakLevel >= 1;
        GFcomboStep = ComboLastMove switch
        {
            AID.WickedTalonPvP => 3,
            AID.SavageClawPvP => 2,
            AID.GnashingFangPvP => 1,
            _ => 0
        };
        comboStep = ComboLastMove switch
        {
            AID.BurstStrikePvP => 0,
            AID.SolidBarrelPvP => 3,
            AID.BrutalShellPvP => 2,
            AID.KeenEdgePvP => 1,
            _ => 0
        };
        inCombo = comboStep > 0;
        inGF = GFcomboStep > 0;

        #region Minimal Requirements
        canGF = IsOffCooldown(AID.GnashingFangPvP);
        canFC = IsOffCooldown(AID.GnashingFangPvP);
        canZone = IsOffCooldown(AID.BlastingZonePvP);
        canHyper = hasBlast && In5y(primaryTarget?.Actor);
        canBrand = hasRaze && In5y(primaryTarget?.Actor);
        canRip = hasRip && In5y(primaryTarget?.Actor);
        canTear = hasTear && In5y(primaryTarget?.Actor);
        canGouge = hasGouge && In5y(primaryTarget?.Actor);
        #endregion
        #endregion

        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        var hold = burstStrategy == BurstStrategy.Hold;

        if (strategy.Option(Track.Combo).As<ComboStrategy>() is ComboStrategy.Force)
            QueueGCD(NextCombo(), primaryTarget?.Actor, GCDPriority.ForcedGCD);

        #region Rotation Execution
        if (!inGF)
            QueueGCD(NextCombo(), primaryTarget?.Actor, GCDPriority.Combo);

        #region OGCDs
        var rdStrat = strategy.Option(Track.RoughDivide).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseRoughDivide(rdStrat, primaryTarget?.Actor))
            QueueOGCD(AID.RoughDividePvP, primaryTarget?.Actor, rdStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.RoughDivide);

        var zoneStrat = strategy.Option(Track.Zone).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseZone(zoneStrat, primaryTarget?.Actor))
            QueueOGCD(AID.BlastingZonePvP, primaryTarget?.Actor, zoneStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Zone);

        if (canRip || GunStep == 1)
            QueueOGCD(AID.JugularRipPvP, primaryTarget?.Actor, OGCDPriority.Continuation);
        if (canTear || GunStep == 2)
            QueueOGCD(AID.AbdomenTearPvP, primaryTarget?.Actor, OGCDPriority.Continuation);
        if (canGouge)
            QueueOGCD(AID.EyeGougePvP, primaryTarget?.Actor, OGCDPriority.Continuation);
        if (canHyper)
            QueueOGCD(AID.HypervelocityPvP, primaryTarget?.Actor, OGCDPriority.Continuation);
        if (canBrand)
            QueueOGCD(AID.FatedBrandPvP, primaryTarget?.Actor, OGCDPriority.Continuation);

        if (TargetHPP(Player) < 50)
            QueueOGCD(AID.HeartOfCorundumPvP, primaryTarget?.Actor, OGCDPriority.Corundum);
        #endregion

        #region GCDs
        var gfStrat = strategy.Option(Track.GnashingFang).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseGnashingFang(gfStrat, primaryTarget?.Actor))
            QueueGCD(AID.GnashingFangPvP, primaryTarget?.Actor, GCDPriority.GnashingFang);
        if (GunStep == 1 && In5y(primaryTarget?.Actor))
            QueueGCD(AID.SavageClawPvP, primaryTarget?.Actor, GCDPriority.GnashingFang);
        if (GunStep == 2 && In5y(primaryTarget?.Actor))
            QueueGCD(AID.WickedTalonPvP, primaryTarget?.Actor, GCDPriority.GnashingFang);

        var fcStrat = strategy.Option(Track.FatedCircle).As<OffensiveStrategy>();
        if (ShouldUseFatedCircle(fcStrat, primaryTarget?.Actor))
            QueueGCD(AID.FatedCirclePvP, primaryTarget?.Actor, fcStrat == OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.FatedCircle);
        #endregion

        #endregion

        var rrStrat = strategy.Option(Track.RelentlessRush).As<RushStrategy>();
        if (ShouldUseRR(rrStrat, primaryTarget?.Actor))
            QueueOGCD(AID.RelentlessRushPvP, Player, rrStrat == RushStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.LB);
        var ttStrat = strategy.Option(Track.TerminalTrigger).As<TriggerStrategy>();
        if (ShouldUseTT(ttStrat, primaryTarget?.Actor) && Hints.NumPriorityTargetsInAOECircle(Player.Position, 5) > 0)
            QueueGCD(AID.TerminalTriggerPvP, Player, ttStrat == TriggerStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.ForcedGCD);
    }

    #region Single-Target Helpers
    private AID NextCombo() => ComboLastMove switch
    {
        AID.SolidBarrelPvP => AID.BurstStrikePvP,
        AID.BrutalShellPvP => AID.SolidBarrelPvP,
        AID.KeenEdgePvP => AID.BrutalShellPvP,
        _ => AID.KeenEdgePvP,
    };
    #endregion

    #region Cooldown Helpers
    private bool ShouldUseRoughDivide(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            target != null &&
            !hasNM && rdCD >= 7 || !OnCooldown(AID.RoughDividePvP),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    private bool ShouldUseZone(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            canZone &&
            hasNM &&
            In5y(target),
        OffensiveStrategy.Force => canZone,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    private bool ShouldUseGnashingFang(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            In5y(target) &&
            hasNM &&
            canGF,
        OffensiveStrategy.Force => canGF,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    private bool ShouldUseFatedCircle(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat &&
            target != null &&
            In5y(target) &&
            hasNM &&
            canFC,
        OffensiveStrategy.Force => canFC,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    private bool ShouldUseRR(RushStrategy strategy, Actor? target) => strategy switch
    {
        RushStrategy.Automatic =>
            target != null &&
            In5y(target) &&
            hasNM &&
            LBready,
        RushStrategy.Force => LBready,
        RushStrategy.Hold => false,
        _ => false
    };

    private bool ShouldUseTT(TriggerStrategy strategy, Actor? target) => strategy switch
    {
        TriggerStrategy.Automatic
            => StacksRemaining(target, SID.RelentlessShrapnelPvP) > 0 && PlayerHasEffect(SID.RelentlessRushPvP),
        TriggerStrategy.Force => PlayerHasEffect(SID.RelentlessRushPvP),
        TriggerStrategy.Hold => false,
        _ => false
    };
    #endregion
}
