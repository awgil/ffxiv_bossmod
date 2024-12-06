using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using AID = BossMod.GNB.AID;
using SID = BossMod.GNB.SID;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiGNBPvP(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track
    {
        Burst,
        Combo,
        LimitBreak,
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

    public enum LimitBreakStrategy
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

        #region Custom strategies
        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 190)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Use everything optimally")
            .AddOption(BurstStrategy.Force, "Force", "Force everything")
            .AddOption(BurstStrategy.Hold, "Hold", "Hold everything");

        res.Define(Track.Combo).As<ComboStrategy>("Combo", uiPriority: 190)
            .AddOption(ComboStrategy.Automatic, "Automatic", "Use combo optimally")
            .AddOption(ComboStrategy.Force, "Force", "Force combo")
            .AddOption(ComboStrategy.Hold, "Hold", "Hold combo");

        res.Define(Track.LimitBreak).As<LimitBreakStrategy>("Limit Break", uiPriority: 190)
            .AddOption(LimitBreakStrategy.Automatic, "Automatic", "Use Limit Break optimally")
            .AddOption(LimitBreakStrategy.Force, "Force", "Force Limit Break")
            .AddOption(LimitBreakStrategy.Hold, "Hold", "Hold Limit Break");
        #endregion

        #region Offensive Strategies
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
        #endregion

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
    public float GCDLength;
    public AID NextGCD;
    private GCDPriority NextGCDPrio;
    #endregion

    #region Module Helpers
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;
    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.9;
    private bool IsOffCooldown(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f;
    public bool HasEffect(SID sid) => SelfStatusLeft(sid) > 0;
    public bool TargetHasEffect(SID sid, Actor? target) => StatusDetails(target, sid, Player.InstanceID, 1000).Left > 0;
    public AID LimitBreak => HasEffect(SID.RelentlessRushPvP) ? AID.TerminalTriggerPvP : AID.RelentlessRushPvP;
    #endregion

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        #region Variables
        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        var GunStep = gauge.AmmoComboStep;
        rdCD = CD(AID.RoughDividePvP);
        nmLeft = SelfStatusLeft(SID.NoMercyPvP, 7);
        hasNM = nmLeft > 0;
        hasBlast = HasEffect(SID.ReadyToBlastPvP);
        hasRaze = HasEffect(SID.ReadyToRazePvP);
        hasRip = HasEffect(SID.ReadyToRipPvP) || GunStep == 1;
        hasTear = HasEffect(SID.ReadyToTearPvP) || GunStep == 2;
        hasGouge = HasEffect(SID.ReadyToGougePvP);
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
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
        NextGCD = AID.None;
        NextGCDPrio = GCDPriority.None;

        #region Minimal Requirements
        canGF = IsOffCooldown(AID.GnashingFangPvP);
        canFC = IsOffCooldown(AID.GnashingFangPvP);
        canZone = IsOffCooldown(AID.BlastingZonePvP);
        canHyper = hasBlast && In5y(primaryTarget);
        canBrand = hasRaze && In5y(primaryTarget);
        canRip = hasRip && In5y(primaryTarget);
        canTear = hasTear && In5y(primaryTarget);
        canGouge = hasGouge && In5y(primaryTarget);
        #endregion
        #endregion

        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        var hold = burstStrategy == BurstStrategy.Hold;

        if (strategy.Option(Track.Combo).As<ComboStrategy>() == ComboStrategy.Force)
            QueueGCD(NextCombo(), primaryTarget, GCDPriority.ForcedGCD);

        #region Rotation Execution
        if (!hold && !inGF)
            QueueGCD(NextCombo(), primaryTarget, GCDPriority.Combo);

        #region OGCDs
        var rdStrat = strategy.Option(Track.RoughDivide).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseRoughDivide(rdStrat, primaryTarget))
            QueueOGCD(AID.RoughDividePvP, primaryTarget, rdStrat is OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.RoughDivide);

        var zoneStrat = strategy.Option(Track.Zone).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseZone(zoneStrat, primaryTarget))
            QueueOGCD(AID.BlastingZonePvP, primaryTarget, zoneStrat == OffensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Zone);

        if (canRip || GunStep == 1)
            QueueOGCD(AID.JugularRipPvP, primaryTarget, OGCDPriority.Continuation);
        if (canTear || GunStep == 1)
            QueueOGCD(AID.AbdomenTearPvP, primaryTarget, OGCDPriority.Continuation);
        if (canGouge)
            QueueOGCD(AID.EyeGougePvP, primaryTarget, OGCDPriority.Continuation);
        if (canHyper)
            QueueOGCD(AID.HypervelocityPvP, primaryTarget, OGCDPriority.Continuation);
        if (canBrand)
            QueueOGCD(AID.FatedBrandPvP, primaryTarget, OGCDPriority.Continuation);
        #endregion

        #region GCDs
        var gfStrat = strategy.Option(Track.GnashingFang).As<OffensiveStrategy>();
        if (!hold &&
            ShouldUseGnashingFang(gfStrat, primaryTarget))
            QueueGCD(AID.GnashingFangPvP, primaryTarget, GCDPriority.GnashingFang);
        if (GunStep == 1 && In5y(primaryTarget))
            QueueGCD(AID.SavageClawPvP, primaryTarget, GCDPriority.GnashingFang);
        if (GunStep == 2 && In5y(primaryTarget))
            QueueGCD(AID.WickedTalonPvP, primaryTarget, GCDPriority.GnashingFang);

        var fcStrat = strategy.Option(Track.FatedCircle).As<OffensiveStrategy>();
        if (ShouldUseFatedCircle(fcStrat, primaryTarget))
            QueueGCD(AID.FatedCirclePvP, primaryTarget, fcStrat == OffensiveStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.FatedCircle);
        #endregion

        #endregion

        var lbStrat = strategy.Option(Track.LimitBreak).As<LimitBreakStrategy>();
        if (ShouldUseLimitBreak(lbStrat, primaryTarget))
            QueueOGCD(LimitBreak, primaryTarget, lbStrat == LimitBreakStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.LB);
    }

    #region Core Execution Helpers
    private void QueueGCD(AID aid, Actor? target, GCDPriority prio)
    {
        if (prio != GCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + (int)prio);
            if (prio > NextGCDPrio)
            {
                NextGCD = aid;
                NextGCDPrio = prio;
            }
        }
    }
    private void QueueOGCD(AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Medium)
    {
        if (prio != OGCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
        }
    }
    #endregion

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
            !hasNM || rdCD >= 7 || IsOffCooldown(AID.RoughDividePvP),
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

    private bool ShouldUseLimitBreak(LimitBreakStrategy strategy, Actor? target) => strategy switch
    {
        LimitBreakStrategy.Automatic =>
            target != null &&
            In5y(target) &&
            hasNM &&
            LBready,
        LimitBreakStrategy.Force => true,
        LimitBreakStrategy.Hold => false,
        _ => false
    };
    #endregion
}
