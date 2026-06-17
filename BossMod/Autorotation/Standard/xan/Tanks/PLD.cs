using BossMod.PLD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class PLD(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, PLD.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track("Fight or Flight", Action = AID.FightOrFlight)]
        public Track<OffensiveStrategy> Buffs;

        [Track("Dash", MinLevel = 66, Action = AID.Intervene)]
        public Track<DashStrategy> Intervene;

        [Track("Holy Spirit", InternalName = "HS", MinLevel = 64, Action = AID.HolySpirit)]
        public Track<HSStrategy> HolySpirit;

        [Track("Atonement", InternalName = "Atone", MinLevel = 76, Actions = [AID.Atonement, AID.Supplication, AID.Sepulchre])]
        public Track<OffensiveStrategy> Atonement;

        public Track<ComboStrategy> Combo;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum HSStrategy
    {
        [Option("Standard usage: use when Divine Might is active; in burst, out of melee range, or to prevent overwriting buff", Targets = ActionTargets.Hostile)]
        Standard,
        [Option("Use ASAP during next Divine Might, regardless of range", Targets = ActionTargets.Hostile)]
        ForceDM,
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Always use when out of melee range", Targets = ActionTargets.Hostile)]
        Ranged,
        [Option("Do not use")]
        Delay
    }
    public enum AtonementStrategy
    {
        [Option("Standard usage: in burst, or to prevent overwriting buff", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Do not use")]
        Delay
    }
    public enum DashStrategy
    {
        [Option("Use both charges in burst", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Use one charge in burst", Targets = ActionTargets.Hostile)]
        HoldOne,
        [Option("Only use when out of melee range", Targets = ActionTargets.Hostile)]
        GapCloser,
        [Option("Do not use")]
        Delay
    }
    public enum ComboStrategy
    {
        [Option("Always finish AOE combo if it would grant Divine Might")]
        FinishAOE,
        [Option("Always finish any combo if it would grant Divine Might")]
        FinishAlways,
        [Option("Allow breaking combo if number of targets changes")]
        BreakCombo
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan PLD", "Paladin", "Standard rotation (xan)|Tanks", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PLD, Class.GLA), 100).WithStrategies<Strategy>();
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

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
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

        var comboStrategy = strategy.Combo.Value;

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

    private void CalcNextBestOGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        if (!Player.InCombat)
            return;

        if (primaryTarget != null)
        {
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
        }

        var dashTarget = ResolveTargetOverride(strategy.Intervene) ?? primaryTarget;

        switch (strategy.Intervene.Value)
        {
            case DashStrategy.Automatic:
                if (FightOrFlight > 0)
                    PushOGCD(AID.Intervene, dashTarget);
                break;
            case DashStrategy.HoldOne:
                if (FightOrFlight > 0 && CanWeave(MaxChargesIn(AID.Intervene), 0.8f))
                    PushOGCD(AID.Intervene, dashTarget);
                break;
            case DashStrategy.GapCloser:
                if (Player.DistanceToHitbox(dashTarget) > 3)
                    PushOGCD(AID.Intervene, dashTarget);
                break;
        }
    }

    private void UseHolySpirit(in Strategy strategy, Enemy? primaryTarget)
    {
        var hs = strategy.HolySpirit;

        if (MP < 1000 || hs == HSStrategy.Delay)
            return;

        var requiescat = Requiescat.Left > GCD;
        var divineMight = DivineMight > GCD;
        var fof = FightOrFlight > GCD;

        var useStandard = divineMight && (fof || NextGCDDivineMight) || requiescat;

        var prio = hs.Value switch
        {
            HSStrategy.Standard => useStandard
                ? GCDPriority.DMHS
                : divineMight ? GCDPriority.HS : GCDPriority.None,
            HSStrategy.ForceDM => divineMight ? GCDPriority.Force : GCDPriority.None,
            HSStrategy.Force => GCDPriority.Force,
            HSStrategy.Ranged => useStandard ? GCDPriority.DMHS : GCDPriority.HS,
            _ => GCDPriority.None
        };

        PushGCD(AID.HolySpirit, ResolveTargetOverride(hs) ?? primaryTarget, prio);
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

    private void UseAtone(in Strategy strategy, Enemy? primaryTarget)
    {
        if (AtonementReady <= GCD)
            return;

        var atone = strategy.Atonement;
        switch (atone.Value)
        {
            case OffensiveStrategy.Automatic:
                if (FightOrFlight > GCD || NextGCD is AID.RageOfHalone or AID.Prominence)
                    PushGCD(AID.Atonement, ResolveTargetOverride(atone) ?? primaryTarget, GCDPriority.Atonement);
                break;
            case OffensiveStrategy.Force:
                PushGCD(AID.Atonement, ResolveTargetOverride(atone) ?? primaryTarget, GCDPriority.Force);
                break;
        }
    }

    private bool ShouldFoF(in Strategy strategy, Enemy? primaryTarget)
    {
        if (strategy.Buffs == OffensiveStrategy.Delay)
            return false;

        if (!Unlocked(TraitID.DivineMagicMastery1))
            return true;

        // hold FoF until 3rd GCD for opener, otherwise use on cooldown
        return DivineMight > 0 || CombatTimer > 30 || strategy.Buffs == OffensiveStrategy.Force;
    }
}
