using BossMod.PLD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class PLD(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player, PotionType.Strength)
{
    public enum Track { Intervene = SharedTrack.Count, HolySpirit, Atonement, Combo }

    public enum HSStrategy
    {
        Standard,
        ForceDM,
        Force,
        Ranged,
        Delay
    }
    public enum AtonementStrategy
    {
        Automatic,
        Force,
        Delay
    }
    public enum DashStrategy
    {
        Automatic,
        GapCloser,
        Delay
    }
    public enum ComboStrategy
    {
        FinishAOE,
        BreakCombo,
        FinishAlways
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan PLD", "Paladin", "Standard rotation (xan)|Tanks", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PLD, Class.GLA), 100);

        def.DefineShared().AddAssociatedActions(AID.FightOrFlight);

        def.Define(Track.Intervene).As<DashStrategy>("Intervene")
            .AddOption(DashStrategy.Automatic, "Use during burst window", minLevel: 66)
            .AddOption(DashStrategy.GapCloser, "Use if outside melee range", minLevel: 66)
            .AddOption(DashStrategy.Delay, "Do not use", minLevel: 66)
            .AddAssociatedActions(AID.Intervene);

        def.Define(Track.HolySpirit).As<HSStrategy>("HS")
            .AddOption(HSStrategy.Standard, "Use during Divine Might only; ASAP in burst, otherwise when out of melee range, or if next GCD will overwrite DM", minLevel: 64)
            .AddOption(HSStrategy.ForceDM, "Use ASAP during next Divine Might proc, regardless of range", minLevel: 64)
            .AddOption(HSStrategy.Force, "Use now, even if in melee range or if DM is not active", minLevel: 64)
            .AddOption(HSStrategy.Ranged, "Always use when out of melee range", minLevel: 64)
            .AddOption(HSStrategy.Delay, "Do not use", minLevel: 64)
            .AddAssociatedActions(AID.HolySpirit);

        def.DefineSimple(Track.Atonement, "Atone", minLevel: 76)
            .AddAssociatedActions(AID.Atonement, AID.Supplication, AID.Sepulchre);

        def.Define(Track.Combo).As<ComboStrategy>("Combo")
            .AddOption(ComboStrategy.FinishAOE, "FinishAOE", "Finish AOE combo, even on 1 target, if it would grant Divine Might")
            .AddOption(ComboStrategy.BreakCombo, "Break", "Break AOE/single-target combo if number of targets changes")
            .AddOption(ComboStrategy.FinishAlways, "FinishAlways", "Finish AOE or single target combo if it would grant Divine Might, even if number of targets changes");

        return def;
    }

    public float FightOrFlight; // max 20
    public float GoringBladeReady; // max 30
    public float DivineMight; // max 30
    public float AtonementReady; // max 30
    public float SupplicationReady; // max 30
    public float SepulchreReady; // max 30
    public float BladeOfHonorReady; // max 30
    public (float Left, int Stacks) Requiescat; // max 30/4 stacks
    public float CCTimer;
    public ushort CCStep;

    public int OathGauge; // 0-100

    public AID ConfiteorCombo;

    public int NumAOETargets;

    private Enemy? BestRangedTarget;

    protected override float GetCastTime(AID aid) => aid switch
    {
        AID.HolyCircle or AID.HolySpirit => DivineMight > GCD || Requiescat.Stacks > 0 ? 0 : base.GetCastTime(aid),
        _ => 0
    };

    public override string DescribeState()
    {
        var gauge = World.Client.GetGauge<PaladinGauge>();
        return $"{{ Oath = {gauge.OathGauge}, C = {gauge.ConfiteorComboStep} / {gauge.ConfiteorComboTimer} }}";
    }

    public enum GCDPriority
    {
        None = 0,
        HS = 100,
        Standard = 500,
        StandardAOE = 510,
        FinishCombo = 550,
        Atonement = 600,
        DMHS = 650,
        AtonementCombo = 700,
        BladeCombo = 750,
        GoringBlade = 800,
        Force = 900
    }

    private bool NextGCDDivineMight => NextGCD is AID.RageOfHalone or AID.RoyalAuthority or AID.Prominence;

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<PaladinGauge>();
        OathGauge = gauge.OathGauge;

        FightOrFlight = StatusLeft(SID.FightOrFlight);
        GoringBladeReady = StatusLeft(SID.GoringBladeReady);
        DivineMight = StatusLeft(SID.DivineMight);
        AtonementReady = StatusLeft(SID.AtonementReady);
        SupplicationReady = StatusLeft(SID.SupplicationReady);
        SepulchreReady = StatusLeft(SID.SepulchreReady);
        BladeOfHonorReady = StatusLeft(SID.BladeOfHonorReady);
        Requiescat = Status(SID.Requiescat);
        ConfiteorCombo = (gauge.ConfiteorComboStep & 0xFF) switch
        {
            0 => StatusLeft(SID.ConfiteorReady) > GCD ? AID.Confiteor : AID.None,
            1 => AID.BladeOfFaith,
            2 => AID.BladeOfTruth,
            3 => AID.BladeOfValor,
            _ => AID.None
        };

        BestRangedTarget = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget).Best;

        NumAOETargets = NumMeleeAOETargets(strategy);

        var shouldAOE = NumAOETargets > 2;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.HolySpirit) + 0.76f)
                PushGCD(AID.HolySpirit, primaryTarget);

            return;
        }

        CalcNextBestOGCD(strategy, primaryTarget);

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.TotalEclipse, 3, maximumActionRange: 20);

        if (ConfiteorCombo != AID.None && MP >= 1000)
            PushGCD(ConfiteorCombo, BestRangedTarget, GCDPriority.BladeCombo);

        if (GoringBladeReady > GCD)
            PushGCD(AID.GoringBlade, primaryTarget, GCDPriority.GoringBlade);

        var comboStrategy = strategy.Option(Track.Combo).As<ComboStrategy>();

        var aoeComboPriority = comboStrategy is ComboStrategy.FinishAOE or ComboStrategy.FinishAlways
            ? GCDPriority.FinishCombo
            : shouldAOE ? GCDPriority.StandardAOE : GCDPriority.None;

        var stComboPriority = comboStrategy == ComboStrategy.FinishAlways ? GCDPriority.FinishCombo : GCDPriority.Standard;

        if (ComboLastMove == AID.TotalEclipse && NumAOETargets > 0)
            PushGCD(AID.Prominence, Player, aoeComboPriority);

        if (shouldAOE)
            PushGCD(AID.TotalEclipse, Player, GCDPriority.StandardAOE);

        if (SepulchreReady > GCD)
            PushGCD(AID.Sepulchre, primaryTarget, GCDPriority.AtonementCombo);

        if (SupplicationReady > GCD)
            PushGCD(AID.Supplication, primaryTarget, GCDPriority.AtonementCombo);

        if (ComboLastMove == AID.RiotBlade)
            PushGCD(AID.RageOfHalone, primaryTarget, stComboPriority);

        if (ComboLastMove == AID.FastBlade)
            PushGCD(AID.RiotBlade, primaryTarget, stComboPriority);

        PushGCD(AID.FastBlade, primaryTarget, GCDPriority.Standard);

        UseHolyCircle();
        UseHolySpirit(strategy, primaryTarget);
        UseAtone(strategy, primaryTarget);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        if (ShouldFoF(strategy, primaryTarget))
            PushOGCD(AID.FightOrFlight, Player);

        if (BladeOfHonorReady > 0)
            PushOGCD(AID.BladeOfHonor, BestRangedTarget);

        if (ReadyIn(AID.FightOrFlight) > 40)
        {
            if (Unlocked(AID.Imperator))
                PushOGCD(AID.Imperator, BestRangedTarget);
            else
                PushOGCD(AID.Requiescat, primaryTarget);
        }

        if (FightOrFlight > 0 || ReadyIn(AID.FightOrFlight) > 15)
        {
            PushOGCD(AID.SpiritsWithin, primaryTarget);

            if (NumAOETargets > 0)
                PushOGCD(AID.CircleOfScorn, Player);
        }

        switch (strategy.Option(Track.Intervene).As<DashStrategy>())
        {
            case DashStrategy.Automatic:
                if (FightOrFlight > 0)
                    PushOGCD(AID.Intervene, primaryTarget);
                break;
            case DashStrategy.GapCloser:
                if (Player.DistanceToHitbox(primaryTarget) > 3)
                    PushOGCD(AID.Intervene, primaryTarget);
                break;
        }
    }

    private void UseHolySpirit(StrategyValues strategy, Enemy? primaryTarget)
    {
        var track = strategy.Option(Track.HolySpirit).As<HSStrategy>();

        if (MP < 1000 || track == HSStrategy.Delay)
            return;

        var requiescat = Requiescat.Left > GCD;
        var divineMight = DivineMight > GCD;
        var fof = FightOrFlight > GCD;

        var useStandard = divineMight && (fof || NextGCDDivineMight) || requiescat;

        var prio = strategy.Option(Track.HolySpirit).As<HSStrategy>() switch
        {
            HSStrategy.Standard => useStandard
                ? GCDPriority.DMHS
                : divineMight ? GCDPriority.HS : GCDPriority.None,
            HSStrategy.ForceDM => divineMight ? GCDPriority.Force : GCDPriority.None,
            HSStrategy.Force => GCDPriority.Force,
            HSStrategy.Ranged => useStandard ? GCDPriority.DMHS : GCDPriority.HS,
            _ => GCDPriority.None
        };

        PushGCD(AID.HolySpirit, primaryTarget, prio);
    }

    private void UseHolyCircle()
    {
        if (MP < 1000 || NumAOETargets < 3)
            return;

        var shouldUse = Requiescat.Left > GCD
            || DivineMight > GCD && (FightOrFlight > GCD || NextGCDDivineMight);

        if (shouldUse)
            PushGCD(AID.HolyCircle, Player, GCDPriority.DMHS);
    }

    private void UseAtone(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (AtonementReady <= GCD)
            return;

        switch (strategy.Simple(Track.Atonement))
        {
            case OffensiveStrategy.Automatic:
                if (FightOrFlight > GCD || NextGCD is AID.RageOfHalone or AID.Prominence)
                    PushGCD(AID.Atonement, primaryTarget, GCDPriority.Atonement);
                break;
            case OffensiveStrategy.Force:
                PushGCD(AID.Atonement, primaryTarget, GCDPriority.Force);
                break;
        }
    }

    private bool ShouldFoF(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (strategy.Simple(SharedTrack.Buffs) == OffensiveStrategy.Delay)
            return false;

        if (!Unlocked(TraitID.DivineMagicMastery1))
            return true;

        // hold FoF until 3rd GCD for opener, otherwise use on cooldown
        return DivineMight > 0 || CombatTimer > 30 || strategy.Simple(SharedTrack.Buffs) == OffensiveStrategy.Force;
    }
}
