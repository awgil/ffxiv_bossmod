using BossMod.RDM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class RDM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID, RDM.Strategy>(manager, player, PotionType.Intelligence)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track(Actions = [AID.Embolden, AID.Manafication])]
        public Track<OffensiveStrategy> Buffs;

        [Track(Targets = ActionTargets.Hostile, MinLevel = 100)]
        public Track<OffensiveStrategy> Prefulgence;

        [Track(Targets = ActionTargets.Hostile)]
        public Track<MeleeStrategy> Melee;

        public Track<ComboStrategy> Combo;

        [Track("Corps-a-corps", Targets = ActionTargets.Hostile)]
        public Track<EnabledByDefault> Dash;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum MeleeStrategy
    {
        [PropertyDisplay("Use at full mana, if needed for movement, or during burst")]
        Automatic,
        [PropertyDisplay("Do not use")]
        Delay,
        [PropertyDisplay("Use ASAP, break combos if necessary")]
        Force
    }

    public enum ComboStrategy
    {
        [PropertyDisplay("Always complete combo (hold if target moves out of range)")]
        Preserve,
        [PropertyDisplay("Always complete combo; use Reprise as fallback if enough mana (NOT IMPLEMENTED, this is a placeholder option)")]
        Reprise,
        [PropertyDisplay("Break combo if target is out of range")]
        Break
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan RDM", "Red Mage", "Standard rotation (xan)|Casters", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.RDM), 100).WithStrategies<Strategy>();
    }

    public uint BlackMana;
    public uint WhiteMana;
    public uint Stacks;

    public float ManaficLeft;
    public float EmboldenLeft;
    public float DualcastLeft;
    public float AccelLeft;
    public float VerfireReady;
    public float VerstoneReady;
    public int SwordplayStacks;
    public float ThornedFlourish;
    public float GrandImpactReady;
    public float PrefulgenceReady;

    public uint LowestMana => Math.Min(BlackMana, WhiteMana);

    public int NumAOETargets;
    public int NumConeTargets;
    public int NumLineTargets;

    private Enemy? BestAOETarget;
    private Enemy? BestConeTarget;
    private Enemy? BestLineTarget;

    private bool InRangedCombo
        => Stacks == 3
        || ComboLastMove is AID.Verflare or AID.Verholy && Unlocked(AID.Scorch)
        || ComboLastMove is AID.Scorch && Unlocked(AID.Resolution);

    private bool InMeleeCombo
        => ComboLastMove == AID.Riposte && Unlocked(AID.Zwerchhau)
        || ComboLastMove == AID.Zwerchhau && Unlocked(AID.Redoublement)
        || ComboLastMove is AID.EnchantedMoulinetDeux or AID.EnchantedMoulinet;

    private bool InCombo => InMeleeCombo || InRangedCombo;

    private bool PostOpener => Player.InCombat && (OnCooldown(AID.Embolden) || CombatTimer > 5);

    private static readonly AID[] Accelerated = [
        AID.Verthunder,
        AID.VerthunderIII,
        AID.Veraero,
        AID.VeraeroIII,
        AID.Scatter,
        AID.Impact
    ];

    protected override float GetCastTime(AID aid)
    {
        if (DualcastLeft > GCD)
            return 0;

        if (AccelLeft > GCD && Accelerated.Contains(aid))
            return 0;

        return base.GetCastTime(aid);
    }

    public enum GCDPriority : int
    {
        None = 0,
        MeleeMove = 100,
        Standard = 200,
        Proc = 250, // wind/fire
        StandardAOE = 275,
        GI = 300,
        MeleeStart = 400,
        Instant = 500, // long casts
        InstantAOE = 525,
        Combo = 600,
        Force = 700,
    }

    public enum OGCDPriority : int
    {
        None = 0,
        Fleche = 400,
        Manafic = 450,
        Pref = 475,
        Embolden = 500,
    }

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<RedMageGauge>();
        BlackMana = gauge.BlackMana;
        WhiteMana = gauge.WhiteMana;
        Stacks = gauge.ManaStacks;

        ManaficLeft = StatusLeft(SID.Manafication);
        EmboldenLeft = StatusLeft(SID.EmboldenSelf, 20);
        DualcastLeft = StatusLeft(SID.Dualcast);
        AccelLeft = StatusLeft(SID.Acceleration);
        VerfireReady = StatusLeft(SID.VerfireReady);
        VerstoneReady = StatusLeft(SID.VerstoneReady);
        SwordplayStacks = StatusStacks(SID.MagickedSwordplay);
        ThornedFlourish = StatusLeft(SID.ThornedFlourish);
        GrandImpactReady = StatusLeft(SID.GrandImpactReady);
        PrefulgenceReady = StatusLeft(SID.PrefulgenceReady);

        (BestAOETarget, NumAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget);
        (BestConeTarget, NumConeTargets) = SelectTarget(strategy, primaryTarget, 8, (primary, other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees()));

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.Veraero))
                PushGCD(AID.Veraero, primaryTarget);

            return;
        }

        var comboMana = Unlocked(AID.Redoublement) ? 50
            : Unlocked(AID.Zwerchhau) ? 35
            : 20;

        if (primaryTarget is { } tar && ManaficLeft <= GCD && (SwordplayStacks > 0 || LowestMana >= comboMana || InMeleeCombo))
            Hints.GoalZones.Add(Hints.GoalSingleTarget(tar.Actor, 3));

        GoalZoneSingle(25);

        DoGCD(strategy, primaryTarget, comboMana);
        OGCD(strategy, primaryTarget);
    }

    void DoGCD(in Strategy strategy, Enemy? primaryTarget, int comboMana)
    {
        // combo continuations
        if (ComboLastMove is AID.Scorch)
            PushGCD(AID.Resolution, BestLineTarget, GCDPriority.Combo);

        if (ComboLastMove is AID.Verflare or AID.Verholy)
            PushGCD(AID.Scorch, BestAOETarget, GCDPriority.Combo);

        if (Stacks == 3)
            PushGCD(BlackMana > WhiteMana ? AID.Verholy : AID.Verflare, BestAOETarget, GCDPriority.Combo);

        if (ComboLastMove == AID.Zwerchhau && Unlocked(AID.Redoublement))
            PushGCD(AID.Redoublement, primaryTarget, GCDPriority.Combo);

        if (ComboLastMove == AID.Riposte && Unlocked(AID.Zwerchhau))
            PushGCD(AID.Zwerchhau, primaryTarget, GCDPriority.Combo);

        if (ComboLastMove == AID.EnchantedMoulinetDeux)
            PushGCD(AID.EnchantedMoulinetTrois, BestConeTarget, GCDPriority.Combo);

        if (ComboLastMove == AID.EnchantedMoulinet)
            PushGCD(AID.EnchantedMoulinetDeux, BestConeTarget, GCDPriority.Combo);

        if (strategy.Combo != ComboStrategy.Break && InCombo)
            return;

        if (GrandImpactReady > GCD)
        {
            if (!CanFitGCD(GrandImpactReady, 1)) // expiring soon
                PushGCD(AID.GrandImpact, BestAOETarget, GCDPriority.Combo);

            if (PostOpener)
                PushGCD(AID.GrandImpact, BestAOETarget, GCDPriority.GI);
        }

        if (AccelLeft > GCD || DualcastLeft > GCD || SwiftcastLeft > GCD)
        {
            if (NumAOETargets > 2)
                PushGCD(AID.Scatter, BestAOETarget, GCDPriority.InstantAOE);

            if (BlackMana > WhiteMana && CombatTimer > 6)
                PushGCD(AID.Veraero, primaryTarget, GCDPriority.Instant);

            PushGCD(AID.Verthunder, primaryTarget, GCDPriority.Instant);
        }

        UseMelee(strategy, primaryTarget, comboMana);

        if (NumAOETargets > 2)
        {
            if (BlackMana > WhiteMana)
                PushGCD(AID.VeraeroII, BestAOETarget, GCDPriority.StandardAOE);

            PushGCD(AID.VerthunderII, BestAOETarget, GCDPriority.StandardAOE);
        }

        if (VerfireReady > GCD + 1.5f)
            PushGCD(AID.Verfire, primaryTarget, GCDPriority.Proc);

        if (VerstoneReady > GCD + 1.5f)
            PushGCD(AID.Verstone, primaryTarget, GCDPriority.Proc);

        PushGCD(AID.Jolt, primaryTarget, GCDPriority.Standard);
    }

    void UseMelee(in Strategy strategy, Enemy? primaryTarget, int comboMana)
    {
        void doit(GCDPriority p, in Strategy strategy, Enemy? primaryTarget)
        {
            if (NumConeTargets > 2 && Player.DistanceToHitbox(BestConeTarget) <= 8)
                PushGCD(AID.EnchantedMoulinet, ResolveTargetOverride(strategy.Melee) ?? BestConeTarget, p);

            if (Player.DistanceToHitbox(primaryTarget) <= 3 || ManaficLeft > GCD)
                PushGCD(AID.Riposte, ResolveTargetOverride(strategy.Melee) ?? primaryTarget, p);
        }

        if (
            // disabled by strategy
            strategy.Melee == MeleeStrategy.Delay
            // insufficient resources
            || SwordplayStacks <= GCD && LowestMana < comboMana
            // already mid combo, using riposte again would break it
            || InCombo
        )
            return;

        if (strategy.Melee == MeleeStrategy.Force)
        {
            doit(GCDPriority.Force, strategy, primaryTarget);
            return;
        }

        if (DualcastLeft > GCD || SwiftcastLeft > GCD)
            return;

        if (
            // overcap incoming (TODO: maybe ignore this condition if buffs are imminent)
            LowestMana >= 89
            // start combo early for buff window
            || CanWeave(AID.Embolden, extraFixedDelay: 5.2f) && strategy.Buffs != OffensiveStrategy.Delay
            // full combo is 12.7s
            || EmboldenLeft > GCD + 12.7f
        )
            doit(GCDPriority.MeleeStart, strategy, primaryTarget);

        doit(GCDPriority.MeleeMove, strategy, primaryTarget);
    }

    private void OGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        UseEmbolden(strategy);

        if (!Player.InCombat || primaryTarget == null)
            return;

        if (CombatTimer < 10)
            PushOGCD(AID.Swiftcast, Player, OGCDPriority.Pref);

        if (!InCombo && PostOpener)
            PushOGCD(AID.Manafication, Player, OGCDPriority.Manafic);

        if (GetCastTime(NextGCD) > 0)
            PushOGCD(AID.Swiftcast, Player);

        PushOGCD(AID.Fleche, primaryTarget, OGCDPriority.Fleche);

        if (CanWeave(MaxChargesIn(AID.Acceleration), 0.6f, 2))
            PushOGCD(AID.Acceleration, Player);

        PushOGCD(AID.ContreSixte, BestAOETarget);
        PushOGCD(AID.Engagement, primaryTarget);

        if (strategy.Dash.IsEnabled())
            PushOGCD(AID.CorpsACorps, ResolveTargetOverride(strategy.Dash) ?? primaryTarget);

        if (ThornedFlourish > AnimLock)
            PushOGCD(AID.ViceOfThorns, BestAOETarget);

        UsePrefulgence(strategy);

        if (CanFitGCD(RaidBuffsLeft, 1) && AccelLeft <= GCD && GrandImpactReady <= GCD)
            PushOGCD(AID.Acceleration, Player);

        if (MP <= Player.HPMP.MaxMP * 0.7f)
            PushOGCD(AID.LucidDreaming, Player);
    }

    void UseEmbolden(in Strategy strategy)
    {
        if (strategy.Buffs == OffensiveStrategy.Force)
        {
            PushOGCD(AID.Embolden, Player, OGCDPriority.Embolden);
            return;
        }

        if (strategy.Buffs == OffensiveStrategy.Delay)
            return;

        if (Player.InCombat && CombatTimer > 5)
            PushOGCD(AID.Embolden, Player, OGCDPriority.Embolden);
    }

    void UsePrefulgence(in Strategy strategy)
    {
        if (PrefulgenceReady <= AnimLock)
            return;

        var shouldUse = strategy.Prefulgence.Value switch
        {
            OffensiveStrategy.Automatic => EmboldenLeft > AnimLock || !CanFitGCD(PrefulgenceReady, 1) || NumAOETargets > 1,
            OffensiveStrategy.Force => true,
            _ => false
        };

        if (shouldUse)
            PushOGCD(AID.Prefulgence, ResolveTargetOverride(strategy.Prefulgence) ?? BestAOETarget, OGCDPriority.Pref);
    }
}
