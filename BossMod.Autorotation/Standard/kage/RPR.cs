using BossMod.RPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

// priorities follow WrathCombo's RPR, burst sequencing follows The Balance (opener, double enshroud, post-perfectio), extra options from awgil/ffxiv_bossmod#759
public sealed class KageRPR(RotationModuleManager manager, Actor player) : KageRotation<KageRPR.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Burst (Arcane Circle, Enshroud, Gluttony)", InternalName = "Burst", MinLevel = 72)]
        public Track<OffensiveStrategy> Burst;

        [Track("Arcane Circle", MinLevel = 72, Action = AID.ArcaneCircle)]
        public Track<OffensiveStrategy> ArcaneCircle;

        [Track("Enshroud", MinLevel = 80, Action = AID.Enshroud)]
        public Track<EnshroudStrategy> Enshroud;

        [Track("Communio", MinLevel = 90, Action = AID.Communio)]
        public Track<CommunioStrategy> Communio;

        [Track("Soul gauge (Gluttony, Blood Stalk, Unveiled)", InternalName = "SoulGauge", MinLevel = 50, Actions = [AID.Gluttony, AID.BloodStalk, AID.GrimSwathe])]
        public Track<SoulGaugeStrategy> SoulGauge;

        [Track("Soul Slice / Soul Scythe", InternalName = "Slice", MinLevel = 60, Actions = [AID.SoulSlice, AID.SoulScythe])]
        public Track<SliceStrategy> Slice;

        [Track("Shadow of Death / Whorl of Death", InternalName = "DeathsDesign", MinLevel = 10, Actions = [AID.ShadowofDeath, AID.WhorlofDeath])]
        public Track<DeathsDesignStrategy> DeathsDesign;

        [Track("Plentiful Harvest", MinLevel = 88, Action = AID.PlentifulHarvest)]
        public Track<OffensiveStrategy> Harvest;

        [Track("Perfectio", MinLevel = 100, Action = AID.Perfectio)]
        public Track<PerfectioStrategy> Perfectio;

        [Track("Harvest Moon", MinLevel = 82, Action = AID.HarvestMoon)]
        public Track<HarvestMoonStrategy> HarvestMoon;

        [Track("Harpe", MinLevel = 15, Action = AID.Harpe)]
        public Track<HarpeStrategy> Harpe;

        [Track("Soulsow", MinLevel = 82, Action = AID.Soulsow)]
        public Track<SoulsowStrategy> Soulsow;

        [Track("True North", MinLevel = 50, Action = ClassShared.AID.TrueNorth)]
        public Track<TrueNorthStrategy> TrueNorth;

        [Track("Hell's Ingress", MinLevel = 20, Action = AID.HellsIngress)]
        public Track<IngressStrategy> Ingress;

        [Track("Arcane Crest", MinLevel = 40, Action = AID.ArcaneCrest)]
        public Track<CrestStrategy> Crest;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 90, UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum EnshroudStrategy
    {
        [Option("Double Enshroud in burst, single Enshroud at odd minutes, avoid overcapping Shroud")]
        Automatic,
        [Option("Use ASAP, but don't overwrite Soul Reaver, Executioner or Perfectio")]
        ForceSafe,
        [Option("Only use with Ideal Host; never spend Shroud gauge", MinLevel = 100)]
        IdealHostOnly,
        [Option("Use ASAP")]
        Force,
        [Option("Do not use")]
        Delay
    }

    public enum CommunioStrategy
    {
        [Option("Use on the last Lemure Shroud", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Use at 2 Lemure Shroud left", Targets = ActionTargets.Hostile)]
        At2,
        [Option("Use at 3 Lemure Shroud left", Targets = ActionTargets.Hostile)]
        At3,
        [Option("Use at 4 Lemure Shroud left", Targets = ActionTargets.Hostile)]
        At4,
        [Option("Use ASAP (5 Lemure Shroud)", Targets = ActionTargets.Hostile)]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum SoulGaugeStrategy
    {
        [Option("Gluttony on cooldown, Blood Stalk / Unveiled when Gluttony is far or gauge is full")]
        Automatic,
        [Option("Automatic, but use Gluttony before Plentiful Harvest", MinLevel = 76)]
        GluttonyFirst,
        [Option("Hold Gluttony; Blood Stalk / Unveiled only to avoid overcapping")]
        HoldGluttony,
        [Option("Use Gluttony ASAP, ignoring timers and stacks; no other spenders", Targets = ActionTargets.Hostile, MinLevel = 76)]
        ForceGluttony,
        [Option("Use Blood Stalk / Unveiled ASAP, ignoring timers and stacks; no Gluttony", Targets = ActionTargets.Hostile)]
        ForceStalk,
        [Option("Use Gluttony and Blood Stalk / Unveiled ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Do not use")]
        Delay
    }

    public enum SliceStrategy
    {
        [Option("Use at 50 Soul or less, or to avoid capping charges")]
        Automatic,
        [Option("Pool charges for raid buffs; otherwise only use to avoid capping charges")]
        Pool,
        [Option("Use ASAP, even if it overcaps Soul", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Do not use")]
        Delay
    }

    public enum DeathsDesignStrategy
    {
        [Option("Keep it up and stack ~30s before the burst")]
        Automatic,
        [Option("Automatic; at full Soul with Gluttony coming up, use Shadow of Death instead of Unveiled + Gibbet/Gallows to keep Gluttony aligned", MinLevel = 76)]
        KeepGluttonyAligned,
        [Option("Use Shadow of Death ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Use Whorl of Death ASAP", MinLevel = 35)]
        ForceWhorl,
        [Option("Do not use")]
        Delay
    }

    public enum PerfectioStrategy
    {
        [Option("Use right after Communio", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Hold for melee downtime, use before it expires", Targets = ActionTargets.Hostile)]
        Ranged,
        [Option("Do not use")]
        Delay
    }

    public enum HarvestMoonStrategy
    {
        [Option("Use out of melee range, or when moving at the end of Enshroud", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Automatic, and also use during raid buffs", Targets = ActionTargets.Hostile)]
        Burst,
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Do not use")]
        Delay
    }

    public enum HarpeStrategy
    {
        [Option("Pre-pull, and out of melee range when not moving or Enhanced Harpe is up", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Only with Enhanced Harpe (and pre-pull)", Targets = ActionTargets.Hostile)]
        EnhancedOnly,
        [Option("Do not use")]
        Delay
    }

    public enum SoulsowStrategy
    {
        [Option("Out of combat, and during downtime with no target while standing still")]
        Automatic,
        [Option("Only out of combat")]
        OutOfCombat,
        [Option("Do not use")]
        Delay
    }

    public enum TrueNorthStrategy
    {
        [Option("Use when the next positional is wrong")]
        Automatic,
        [Option("Use when the next positional is wrong, but keep a charge for Gluttony")]
        PoolGluttony,
        [Option("Keep a charge for Gluttony, and spend Soul gauge / Soul Slice while True North is up to cover more positionals")]
        Bundle,
        [Option("Do not use")]
        Delay
    }

    public enum IngressStrategy
    {
        [Option("Do not use")]
        Manual,
        [Option("Dash out of an incoming AoE, to the safe spot closest to the target")]
        Dodge,
        [Option("Dash towards the specified position", Targets = ActionTargets.Area, Context = StrategyContext.Plan)]
        Position
    }

    public enum CrestStrategy
    {
        [Option("Use before raidwides, or when targeted while solo")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use right before Arcane Circle")]
        AlignWithBurst,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Standard opener (Arcane Circle on the 2nd GCD at 100, Wrath's at 90)")]
        Standard,
        [Option("No fixed opener, use the normal priority")]
        None
    }

    private enum Step { ShadowOfDeath, SoulSlice, ArcaneCircle, Gluttony, ExecutionerFirst, ExecutionerSecond, ReaverFirst, ReaverSecond, PlentifulHarvest, Enshroud, VoidReaping, CrossReaping, Sacrificium, LemuresSlice, Communio, Perfectio, Unveiled, GibbetGallows, HarvestMoon, Slice }

    // The Balance lv100 opener (Arcane Circle on the 2nd GCD); potion is weaved by the potion option after Shadow of Death
    private static readonly Step[] Opener100 = [
        Step.ShadowOfDeath, Step.SoulSlice, Step.ArcaneCircle, Step.Gluttony, Step.ExecutionerFirst, Step.ExecutionerSecond, Step.SoulSlice, Step.PlentifulHarvest,
        Step.Enshroud, Step.VoidReaping, Step.Sacrificium, Step.CrossReaping, Step.LemuresSlice, Step.VoidReaping, Step.CrossReaping, Step.LemuresSlice, Step.Communio,
        Step.Perfectio, Step.Unveiled, Step.GibbetGallows, Step.ShadowOfDeath, Step.Slice
    ];

    // WrathCombo lv90 opener
    private static readonly Step[] Opener90 = [
        Step.ShadowOfDeath, Step.ArcaneCircle, Step.SoulSlice, Step.SoulSlice, Step.PlentifulHarvest,
        Step.Enshroud, Step.VoidReaping, Step.CrossReaping, Step.LemuresSlice, Step.VoidReaping, Step.CrossReaping, Step.LemuresSlice, Step.Communio,
        Step.HarvestMoon, Step.Gluttony, Step.ReaverFirst, Step.ReaverSecond, Step.Unveiled, Step.GibbetGallows
    ];

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage RPR", "Reaper", "Standard rotation (Kage)|Melee", "Kagekazu", RotationModuleQuality.Basic, BitMask.Build(Class.RPR), 100).WithStrategies<Strategy>();
    }

    private const float DDRefresh = 6;

    private int Soul;
    private int Shroud;
    private int Lemure;
    private int VoidShroud;
    private float EnshroudLeft;
    private float SoulReaverLeft;
    private int SoulReaverStacks;
    private float ExecutionerLeft;
    private float PerfectioLeft;
    private float ImmortalLeft;
    private float BloodsownLeft;
    private float IdealHostLeft;
    private float OblatioLeft;
    private float CircleLeft;
    private float EnhancedGibbetLeft;
    private float EnhancedGallowsLeft;
    private float EnhancedCrossLeft;
    private float EnhancedHarpeLeft;
    private bool HasSoulsow;
    private float TargetDDLeft;

    private bool AOEMode;
    private bool ConeAOE;
    private bool ForceST;
    private bool InMelee;
    private int NumCircleTargets;
    private int NumConeTargets;
    private Actor? BestConeTarget;
    private Actor? BestLineTarget;
    private Actor? BestSplashTarget;

    private OffensiveStrategy CircleStrat;
    private EnshroudStrategy EnshroudStrat;
    private SoulGaugeStrategy GaugeStrat;
    private bool WantEnshroud;
    private bool WorthBurst;
    private bool AvoidGluttonyDrift;
    private bool SpendUnderTrueNorth;

    private readonly Dictionary<uint, DateTime> LastUsed = [];
    private DateTime LastCastSeen;
    private DateTime CircleReadySince;

    private Step[]? OpenerSteps;
    private int OpenerStep;
    private DateTime OpenerStepStart;
    private bool OpenerChecked;

    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private bool Enshrouded => Lemure > 0;
    private bool Executioner => ExecutionerLeft > GCD;
    private bool SoulReaver => SoulReaverLeft > GCD || Executioner;
    private bool PerfectioReady => PerfectioLeft > GCD && Unlocked(AID.Perfectio);
    private bool HasImmortal => ImmortalLeft > GCD;
    private float CircleIn => CircleStrat == OffensiveStrategy.Delay ? float.MaxValue : ReadyIn(AID.ArcaneCircle);
    private bool InNormalRotation => !Enshrouded && !SoulReaver && !HasImmortal && IdealHostLeft == 0 && !PerfectioReady;
    private bool InPostBurst => JustUsed(AID.Perfectio, GCDLength * 8);
    private bool ShroudOvercapping => Shroud >= 100 && !WantEnshroud;
    private bool ComboExpiring(float gcds) => ComboLeft > 0 && ComboLeft < GCDLength * gcds;
    private bool DDExpiring(float gcds) => TargetDDLeft < GCDLength * gcds;
    private bool JustUsed(AID aid, float within = 3) => LastUsed.TryGetValue((uint)aid, out var t) && (World.CurrentTime - t).TotalSeconds < within;
    private bool GluttonyReady => Soul >= 50 && CanWeave(AID.Gluttony);
    private bool MidCombo(bool aoe) => ComboLeft > GCD && (aoe ? ComboLastMove == AID.SpinningScythe : ComboLastMove is AID.Slice or AID.WaxingSlice);

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 3, strategy.AOE.Value);
        TrackCasts();

        CircleStrat = WithBurst(strategy.Burst.Value, strategy.ArcaneCircle.Value);
        EnshroudStrat = strategy.Burst.Value switch { OffensiveStrategy.Delay => EnshroudStrategy.Delay, OffensiveStrategy.Force => EnshroudStrategy.Force, _ => strategy.Enshroud.Value };
        GaugeStrat = strategy.Burst.Value switch { OffensiveStrategy.Delay => SoulGaugeStrategy.HoldGluttony, OffensiveStrategy.Force => SoulGaugeStrategy.ForceGluttony, _ => strategy.SoulGauge.Value };
        ForceST = strategy.AOE.Value == AOEStrategy.ForceST;

        var gauge = World.Client.GetGauge<ReaperGauge>();
        Soul = gauge.Soul;
        Shroud = gauge.Shroud;
        Lemure = gauge.LemureShroud;
        VoidShroud = gauge.VoidShroud;
        EnshroudLeft = gauge.EnshroudedTimeRemaining * 0.001f;

        (SoulReaverLeft, SoulReaverStacks) = SelfStatusDetails(SID.SoulReaver);
        ExecutionerLeft = SelfStatusLeft(SID.Executioner);
        PerfectioLeft = SelfStatusLeft(SID.PerfectioParata);
        ImmortalLeft = SelfStatusLeft(SID.ImmortalSacrifice);
        BloodsownLeft = SelfStatusLeft(SID.BloodsownCircle);
        IdealHostLeft = SelfStatusLeft(SID.IdealHost);
        OblatioLeft = SelfStatusLeft(SID.Oblatio);
        CircleLeft = SelfStatusLeft(SID.ArcaneCircle);
        EnhancedGibbetLeft = SelfStatusLeft(SID.EnhancedGibbet);
        EnhancedGallowsLeft = SelfStatusLeft(SID.EnhancedGallows);
        EnhancedCrossLeft = SelfStatusLeft(SID.EnhancedCrossReaping);
        EnhancedHarpeLeft = SelfStatusLeft(SID.EnhancedHarpe);
        HasSoulsow = SelfStatusLeft(SID.Soulsow) > 0;
        TargetDDLeft = DDLeft(target);

        var allowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        NumCircleTargets = ForceST ? 0 : Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
        AOEMode = Unlocked(AID.SpinningScythe) && UseAOE(strategy.AOE.Value, NumCircleTargets, 3);
        InMelee = target != null && Player.DistanceToHitbox(target.Actor) <= 3;
        (BestConeTarget, NumConeTargets) = BestAOETarget(target, 8, allowAoE, (c, e) => TargetInAOECone(e, Player.Position, 8, Player.DirectionTo(c), 90.Degrees()));
        ConeAOE = !ForceST && UseAOE(strategy.AOE.Value, NumConeTargets, 3);
        BestLineTarget = BestAOETarget(target, 15, allowAoE, (c, e) => TargetInAOERect(e, Player.Position, Player.DirectionTo(c), 15, 2)).Best;
        BestSplashTarget = BestAOETarget(target, 25, allowAoE, (c, e) => TargetInAOECircle(e, c.Position, 5)).Best;

        AvoidGluttonyDrift = strategy.DeathsDesign.Value == DeathsDesignStrategy.KeepGluttonyAligned && target != null && Soul == 100 && ReadyIn(AID.Gluttony) < 2 && TargetDDLeft < 25 && !SoulReaver && !Enshrouded;
        SpendUnderTrueNorth = strategy.TrueNorth.Value == TrueNorthStrategy.Bundle && SelfStatusLeft(ClassShared.SID.TrueNorth) > GCD + GCDLength;

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            Prepull(strategy, target, countdown);
            return;
        }

        if (!Player.InCombat)
        {
            OpenerChecked = false;
            OpenerSteps = null;
            CircleReadySince = default;

            // if we exit combat while casting, cancel it so we get an instant cast instead
            if (Player.CastInfo?.Action.ID == (uint)AID.Soulsow)
                Hints.ForceCancelCast = true;
            if (!HasSoulsow && strategy.Soulsow.Value != SoulsowStrategy.Delay)
                PushGCD(AID.Soulsow, Player, 95);
        }

        if (target == null)
        {
            if (Player.InCombat && !HasSoulsow && strategy.Soulsow.Value == SoulsowStrategy.Automatic && !isMoving)
                PushGCD(AID.Soulsow, Player, 1, ScaledCastTime(AID.Soulsow));
            UseIngress(strategy, null);
            return;
        }

        WorthBurst = TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 10);
        WantEnshroud = ShouldEnshroud();

        UpdateOpener(strategy, target);
        if (OpenerSteps != null)
            PushOpenerStep(target);
        else
            GCDs(strategy, target, isMoving);

        UpdatePositionals(strategy, target);
        AddGoalZone(target, allowAoE);

        if (Player.InCombat)
        {
            if (OpenerSteps == null)
                OGCDs(strategy, target);
            UseIngress(strategy, target);
        }
    }

    private void Prepull(in Strategy strategy, Enemy? target, float countdown)
    {
        if (!HasSoulsow && strategy.Soulsow.Value != SoulsowStrategy.Delay)
            PushGCD(AID.Soulsow, Player, 10);

        // harpe at -1.7 so it lands on the pull; skip it if we're already in melee range
        if (target != null && strategy.Harpe.Value != HarpeStrategy.Delay && !InMelee && countdown < HarpeCastTime + 0.4f)
            PushGCD(AID.Harpe, target.Actor, 10, HarpeCastTime);
    }

    private float HarpeCastTime => EnhancedHarpeLeft > GCD ? 0 : ScaledCastTime(AID.Harpe);

    #region Opener

    private void TrackCasts()
    {
        var (time, cast) = Manager.LastCast;
        if (cast == null || time == LastCastSeen)
            return;
        LastCastSeen = time;
        LastUsed[cast.Action.ID] = time;
        if (OpenerSteps != null && OpenerStep < OpenerSteps.Length && StepAccepts(OpenerSteps[OpenerStep], (AID)cast.Action.ID))
        {
            ++OpenerStep;
            OpenerStepStart = World.CurrentTime;
        }
    }

    private void UpdateOpener(in Strategy strategy, Enemy target)
    {
        if (!Player.InCombat)
            return;

        if (!OpenerChecked)
        {
            OpenerChecked = true;
            OpenerSteps = strategy.Opener.Value == OpenerStrategy.Standard && InBossFight && CombatTime < 3 && OpenerCooldownsReady()
                ? Player.Level >= 100 ? Opener100 : Player.Level >= 90 ? Opener90 : null
                : null;
            // in melee range we skip harpe, so shadow of death may already be out
            OpenerStep = JustUsed(AID.ShadowofDeath) ? 1 : 0;
            OpenerStepStart = World.CurrentTime;
        }

        // bail out to the normal priority if something goes wrong (target gone, step unusable)
        if (OpenerSteps != null && (OpenerStep >= OpenerSteps.Length || (World.CurrentTime - OpenerStepStart).TotalSeconds > 6))
            OpenerSteps = null;
    }

    private bool OpenerCooldownsReady()
        => MaxChargesIn(AID.SoulSlice) <= GCD && ReadyIn(AID.ArcaneCircle) <= GCD && (Player.Level < 100 || ReadyIn(AID.Gluttony) <= GCD) && VoidShroud == 0 && Soul <= 10;

    private void PushOpenerStep(Enemy target)
    {
        var step = OpenerSteps![OpenerStep];
        if (step == Step.HarvestMoon && !HasSoulsow)
        {
            ++OpenerStep;
            return;
        }

        var aid = StepAction(step, target.Actor);
        if (ActionDefinitions.Instance.Spell(aid) is not { } def)
            return;

        if (def.IsGCD)
            PushGCD(aid, target.Actor, 99, ScaledCastTime(aid));
        else
            // late-weave arcane circle so the buff covers the unveiled gibbet at the end
            PushOGCD(aid, target.Actor, 99, step == Step.ArcaneCircle ? MathF.Max(0, GCD - 1.5f) : 0);
    }

    private AID StepAction(Step step, Actor target) => step switch
    {
        Step.ExecutionerFirst => CurrentPositional(target) == Positional.Rear ? AID.ExecutionersGallows : AID.ExecutionersGibbet,
        Step.ExecutionerSecond => EnhancedGibbetLeft > 0 ? AID.ExecutionersGibbet : AID.ExecutionersGallows,
        Step.ReaverFirst => CurrentPositional(target) == Positional.Rear ? AID.Gallows : AID.Gibbet,
        Step.ReaverSecond => EnhancedGibbetLeft > 0 ? AID.Gibbet : AID.Gallows,
        Step.GibbetGallows => EnhancedGallowsLeft > 0 ? AID.Gallows : AID.Gibbet,
        _ => FixedStepAction(step)
    };

    private static bool StepAccepts(Step step, AID used) => step switch
    {
        Step.ExecutionerFirst or Step.ExecutionerSecond => used is AID.ExecutionersGibbet or AID.ExecutionersGallows,
        Step.ReaverFirst or Step.ReaverSecond or Step.GibbetGallows => used is AID.Gibbet or AID.Gallows,
        Step.Unveiled => used is AID.BloodStalk or AID.UnveiledGibbet or AID.UnveiledGallows,
        Step.LemuresSlice => used is AID.LemuresSlice or AID.LemuresScythe,
        _ => used == FixedStepAction(step)
    };

    // steps that always map to the same action
    private static AID FixedStepAction(Step step) => step switch
    {
        Step.ShadowOfDeath => AID.ShadowofDeath,
        Step.SoulSlice => AID.SoulSlice,
        Step.ArcaneCircle => AID.ArcaneCircle,
        Step.Gluttony => AID.Gluttony,
        Step.PlentifulHarvest => AID.PlentifulHarvest,
        Step.Enshroud => AID.Enshroud,
        Step.VoidReaping => AID.VoidReaping,
        Step.CrossReaping => AID.CrossReaping,
        Step.Sacrificium => AID.Sacrificium,
        Step.LemuresSlice => AID.LemuresSlice,
        Step.Communio => AID.Communio,
        Step.Perfectio => AID.Perfectio,
        Step.Unveiled => AID.BloodStalk, // turns into unveiled gibbet/gallows
        Step.HarvestMoon => AID.HarvestMoon,
        Step.Slice => AID.Slice,
        _ => AID.None
    };

    #endregion

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target, bool isMoving)
    {
        UsePerfectio(strategy, target);
        PostBurstGCD(strategy, target);
        UseDeathsDesign(strategy, target);
        UseReaver(target);
        UseHarvest(strategy, target);
        EnshroudGCDs(strategy, target, isMoving);
        UseSoulSlice(strategy, target);
        UseHarvestMoon(strategy, target);
        if (!InMelee && !SoulReaver)
            RangedFiller(strategy, target, isMoving);
        Combo(target);
    }

    private void UsePerfectio(in Strategy strategy, Enemy target)
    {
        if (!PerfectioReady || strategy.Perfectio.Value == PerfectioStrategy.Delay)
            return;
        var hold = strategy.Perfectio.Value == PerfectioStrategy.Ranged && InMelee && PerfectioLeft > GCD + GCDLength * 2;
        PushGCD(AID.Perfectio, ResolveTarget(strategy.Perfectio) ?? BestSplashTarget ?? target.Actor, hold ? 18 : 90);
    }

    // the balance: after perfectio, continue the combo, then gluttony if we have 50 soul, else soul slice into gluttony
    private bool PostPerfectioCombo => JustUsed(AID.Perfectio, GCDLength + 0.5f) && MidCombo(AOEMode);

    private void PostBurstGCD(in Strategy strategy, Enemy target)
    {
        if (!InPostBurst || SoulReaver || HasImmortal || Unlocked(AID.ShadowofDeath) && TargetDDLeft <= GCD)
            return;

        if (PostPerfectioCombo)
        {
            Combo(target, 85);
            return;
        }

        var slice = AOEMode ? AID.SoulScythe : AID.SoulSlice;
        if (strategy.Slice.Value != SliceStrategy.Delay && GCDReady(slice) && !JustUsed(slice, GCDLength) && Soul < 100
            && (Soul < 50 || ReadyIn(AID.Gluttony) > GCD + GCDLength || SliceCapping(slice)))
            PushGCD(slice, AOEMode ? Player : target.Actor, 85);
    }

    private void UseDeathsDesign(in Strategy strategy, Enemy target)
    {
        switch (strategy.DeathsDesign.Value)
        {
            case DeathsDesignStrategy.Delay:
                return;
            case DeathsDesignStrategy.Force:
                PushGCD(AID.ShadowofDeath, ResolveTarget(strategy.DeathsDesign) ?? target.Actor, 95);
                return;
            case DeathsDesignStrategy.ForceWhorl:
                PushGCD(AID.WhorlofDeath, Player, 95);
                return;
        }

        if (!Unlocked(AID.ShadowofDeath) || target.ForbidDOTs || SoulReaver || PerfectioReady || HasImmortal)
            return;

        if (AvoidGluttonyDrift)
        {
            PushGCD(AID.ShadowofDeath, target.Actor, 80);
            return;
        }

        var dd = TargetDDLeft;
        if (AOEMode && Unlocked(AID.WhorlofDeath))
        {
            if (dd < DDRefresh)
                PushGCD(AID.WhorlofDeath, Player, 80);
            return;
        }

        if (ComboExpiring(3) || JustUsed(AID.ShadowofDeath, GCDLength))
            return;

        bool use;
        if (!InBossFight)
            use = !Enshrouded && dd <= DDRefresh;
        else if (Unlocked(AID.PlentifulHarvest))
            use = !Enshrouded && CircleIn <= 9 + GCDLength * 2 && dd < 30 // stack it before the burst
                || Enshrouded && CircleIn <= GCD + GCDLength && Lemure == 4 && (JustUsed(AID.VoidReaping, 2) || JustUsed(AID.CrossReaping, 2)) // double enshroud: void, shadow of death, arcane circle
                || !Enshrouded && dd <= DDRefresh && (dd == 0 || CircleIn > GCDLength * 8 || CircleIn <= GCD);
        else
            use = dd <= DDRefresh;

        // never let it drop outside of enshroud
        if (use || !Enshrouded && dd < GCD + GCDLength)
            PushGCD(AID.ShadowofDeath, target.Actor, 80);
    }

    private void UseReaver(Enemy target)
    {
        if (!SoulReaver || Enshrouded || ShroudOvercapping || !Unlocked(AID.Gibbet))
            return;

        if (ConeAOE && Unlocked(AID.Guillotine))
            PushGCD(Executioner ? AID.ExecutionersGuillotine : AID.Guillotine, BestConeTarget ?? target.Actor, 76);

        var gibbet = Executioner ? AID.ExecutionersGibbet : AID.Gibbet;
        var gallows = Executioner ? AID.ExecutionersGallows : AID.Gallows;
        var aid = EnhancedGibbetLeft > GCD ? gibbet
            : EnhancedGallowsLeft > GCD ? gallows
            : CurrentPositional(target.Actor) == Positional.Rear ? gallows : gibbet;
        PushGCD(aid, target.Actor, 75);
    }

    private void UseHarvest(in Strategy strategy, Enemy target)
    {
        if (strategy.Harvest.Value == OffensiveStrategy.Delay || Enshrouded || SoulReaver || !HasImmortal)
            return;
        if (strategy.Harvest.Value == OffensiveStrategy.Force || BloodsownLeft <= 1 || JustUsed(AID.Communio))
            PushGCD(AID.PlentifulHarvest, ResolveTarget(strategy.Harvest) ?? BestLineTarget ?? target.Actor, 70);
    }

    private void EnshroudGCDs(in Strategy strategy, Enemy target, bool isMoving)
    {
        if (!Enshrouded)
            return;

        var communioAt = strategy.Communio.Value switch
        {
            CommunioStrategy.Automatic => 1,
            CommunioStrategy.At2 => 2,
            CommunioStrategy.At3 => 3,
            CommunioStrategy.At4 => 4,
            CommunioStrategy.ASAP => 5,
            _ => 0
        };
        if (Lemure <= communioAt)
        {
            PushGCD(AID.Communio, ResolveTarget(strategy.Communio) ?? BestSplashTarget ?? target.Actor, 66, ScaledCastTime(AID.Communio));
            // can't cast communio while moving
            if (isMoving && HasSoulsow && Lemure == 1)
                PushGCD(AID.HarvestMoon, BestSplashTarget ?? target.Actor, 67);
        }

        if (ConeAOE && Unlocked(AID.GrimReaping))
            PushGCD(AID.GrimReaping, BestConeTarget ?? target.Actor, 65);
        else
            PushGCD(EnhancedCrossLeft > GCD ? AID.CrossReaping : AID.VoidReaping, target.Actor, 65);
    }

    private bool SliceCapping(AID slice) => MaxChargesIn(slice) <= GCD + GCDLength * 2;

    private void UseSoulSlice(in Strategy strategy, Enemy target)
    {
        var slice = AOEMode ? AID.SoulScythe : AID.SoulSlice;
        if (strategy.Slice.Value == SliceStrategy.Delay || !GCDReady(slice))
            return;

        var sliceTarget = AOEMode ? Player : ResolveTarget(strategy.Slice) ?? target.Actor;
        if (strategy.Slice.Value == SliceStrategy.Force)
        {
            PushGCD(slice, sliceTarget, 60);
            return;
        }

        if (InPostBurst || !InNormalRotation || ComboExpiring(3))
            return;

        var capping = Soul < 100 && SliceCapping(slice);
        var use = strategy.Slice.Value switch
        {
            SliceStrategy.Pool => Soul <= 50 && (CircleLeft > GCD || RaidBuffsLeft > GCD || SpendUnderTrueNorth) || capping,
            _ => Soul <= 50 || capping
        };
        if (use)
            PushGCD(slice, sliceTarget, 60);
    }

    private void UseHarvestMoon(in Strategy strategy, Enemy target)
    {
        if (!HasSoulsow || SoulReaver)
            return;

        var hmTarget = ResolveTarget(strategy.HarvestMoon) ?? BestSplashTarget ?? target.Actor;
        switch (strategy.HarvestMoon.Value)
        {
            case HarvestMoonStrategy.Force:
                PushGCD(AID.HarvestMoon, hmTarget, 95);
                break;
            case HarvestMoonStrategy.Burst when CircleLeft > GCD || RaidBuffsLeft > GCD:
                PushGCD(AID.HarvestMoon, hmTarget, 55);
                break;
        }
    }

    // wrath's ranged attack: harvest moon, then harpe
    private void RangedFiller(in Strategy strategy, Enemy target, bool isMoving)
    {
        if (Player.DistanceToHitbox(target.Actor) > 25)
            return;

        if (HasSoulsow && strategy.HarvestMoon.Value != HarvestMoonStrategy.Delay)
            PushGCD(AID.HarvestMoon, BestSplashTarget ?? target.Actor, 22);

        var enhanced = EnhancedHarpeLeft > GCD;
        var harpe = strategy.Harpe.Value switch
        {
            HarpeStrategy.Automatic => enhanced || !isMoving,
            HarpeStrategy.EnhancedOnly => enhanced,
            _ => false
        };
        if (harpe)
            PushGCD(AID.Harpe, target.Actor, 18, HarpeCastTime);
    }

    private void Combo(Enemy target, int priority = 10)
    {
        if (AOEMode)
        {
            PushGCD(MidCombo(true) && Unlocked(AID.NightmareScythe) ? AID.NightmareScythe : AID.SpinningScythe, Player, priority);
            return;
        }

        var aid = ComboLeft > GCD && ComboLastMove == AID.Slice && Unlocked(AID.WaxingSlice) ? AID.WaxingSlice
            : ComboLeft > GCD && ComboLastMove == AID.WaxingSlice && Unlocked(AID.InfernalSlice) ? AID.InfernalSlice
            : AID.Slice;
        PushGCD(aid, target.Actor, priority);
    }

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        UseArcaneCircle(strategy, target);

        if (WantEnshroud && (CanWeave(AID.Enshroud) || IdealHostLeft > 0))
            PushOGCD(AID.Enshroud, Player, 68);

        UseGluttony(strategy, target);
        UseBloodStalk(strategy, target);

        if (Enshrouded)
        {
            if (VoidShroud >= 2)
                PushOGCD(ConeAOE && Unlocked(AID.LemuresScythe) ? AID.LemuresScythe : AID.LemuresSlice, ConeAOE ? BestConeTarget ?? target.Actor : target.Actor, 61);

            // keep sacrificium for arcane circle if it comes up during this enshroud
            if (OblatioLeft > 0 && (AOEMode ? Lemure <= 2 : Lemure <= 4) && (CircleLeft > 0 && !JustUsed(AID.ArcaneCircle, 2) || CircleIn > EnshroudLeft))
                PushOGCD(AID.Sacrificium, BestSplashTarget ?? target.Actor, 60);
        }

        if (strategy.Crest.Value == CrestStrategy.Automatic && CanWeave(AID.ArcaneCrest)
            && (RaidwideWithin(3) || !World.Party.WithoutSlot(excludeAlliance: true).Skip(1).Any() && Hints.PotentialTargets.Any(e => e.Actor.TargetID == Player.InstanceID)))
            PushOGCD(AID.ArcaneCrest, Player, 20);
    }

    private void UseArcaneCircle(in Strategy strategy, Enemy target)
    {
        if (CircleStrat == OffensiveStrategy.Delay || !Unlocked(AID.ArcaneCircle))
            return;

        if (ReadyIn(AID.ArcaneCircle) > GCD)
            CircleReadySince = default;
        else if (CircleReadySince == default)
            CircleReadySince = World.CurrentTime;

        if (!CanWeave(AID.ArcaneCircle))
            return;

        // weave it after shadow of death, halfway through the gcd (the balance)
        var sodDone = JustUsed(AID.ShadowofDeath, GCDLength + 0.5f) || target.ForbidDOTs || strategy.DeathsDesign.Value == DeathsDesignStrategy.Delay;
        var overdue = CircleReadySince != default && (World.CurrentTime - CircleReadySince).TotalSeconds > 10;
        var use = CircleStrat == OffensiveStrategy.Force
            || WorthBurst && (AOEMode || sodDone || !Unlocked(AID.Enshroud) || overdue);
        if (use)
            PushOGCD(AID.ArcaneCircle, Player, 70, CircleStrat == OffensiveStrategy.Force ? 0 : MathF.Max(0, GCD - GCDLength * 0.5f));
    }

    private bool ShouldEnshroud()
    {
        if (Enshrouded || EnshroudStrat == EnshroudStrategy.Delay || !Unlocked(AID.Enshroud) || Shroud < 50 && IdealHostLeft == 0)
            return false;

        // let's assume wasting perfectio or soul reaver is a mistake
        var wouldOverwrite = SoulReaver || PerfectioReady;
        switch (EnshroudStrat)
        {
            case EnshroudStrategy.Force:
                return true;
            case EnshroudStrategy.ForceSafe:
                return !wouldOverwrite;
            case EnshroudStrategy.IdealHostOnly:
                return IdealHostLeft > 0 && !wouldOverwrite;
        }

        if (wouldOverwrite || AOEMode && ComboExpiring(6))
            return false;
        if (!Unlocked(AID.PlentifulHarvest) || CircleLeft > 0 || IdealHostLeft > 0)
            return true;
        // double enshroud: arcane circle at 9s, two more gcds, then enshroud
        if (CircleIn <= GCDLength + 1.5f)
            return true;
        if (JustUsed(AID.PlentifulHarvest, 5))
            return true;
        if (DDExpiring(5))
            return false;
        // single enshroud at odd minutes; also when soul is nearly full (no soul gain while enshrouded) or shroud is capped and the burst is far
        return CircleIn is >= 49f and <= 66f || Soul >= 90 || Shroud >= 100 && CircleIn > 15;
    }

    private void UseGluttony(in Strategy strategy, Enemy target)
    {
        if (!GluttonyReady || ShroudOvercapping)
            return;

        var gluttonyTarget = ResolveTarget(strategy.SoulGauge) ?? BestSplashTarget ?? target.Actor;
        switch (GaugeStrat)
        {
            case SoulGaugeStrategy.ForceGluttony:
            case SoulGaugeStrategy.Force:
                PushOGCD(AID.Gluttony, gluttonyTarget, 64);
                return;
            case SoulGaugeStrategy.Automatic:
            case SoulGaugeStrategy.GluttonyFirst:
                break;
            default:
                return;
        }

        // gluttony drifts to the end of the first even minute burst, then goes on cooldown
        var burst = InPostBurst && !PostPerfectioCombo;
        var normal = !Enshrouded && !SoulReaver && IdealHostLeft == 0 && !PerfectioReady && (!HasImmortal || GaugeStrat == SoulGaugeStrategy.GluttonyFirst)
            && !ComboExpiring(3) && !InPostBurst;
        if ((burst || normal) && TargetDDLeft > GCD + GCDLength * 2)
            PushOGCD(AID.Gluttony, gluttonyTarget, 64);
    }

    private void UseBloodStalk(in Strategy strategy, Enemy target)
    {
        if (Soul < 50 || Enshrouded || GaugeStrat is SoulGaugeStrategy.Delay or SoulGaugeStrategy.ForceGluttony)
            return;

        var forced = GaugeStrat is SoulGaugeStrategy.ForceStalk or SoulGaugeStrategy.Force;
        if (!forced)
        {
            if (ShroudOvercapping || !InNormalRotation || ComboExpiring(3) || AvoidGluttonyDrift || TargetDDLeft <= GCD + GCDLength)
                return;
            // after perfectio, keep soul for gluttony
            if (Soul < 100 && InPostBurst && !JustUsed(AID.Gluttony, GCDLength * 8))
                return;

            var gluttonyFar = ReadyIn(AID.Gluttony) > GCDLength * (AOEMode ? 5 : 4);
            var spend = !Unlocked(AID.Gluttony) || Soul == 100
                || GaugeStrat != SoulGaugeStrategy.HoldGluttony && (gluttonyFar || SpendUnderTrueNorth && ReadyIn(AID.Gluttony) > GCDLength * 2);
            if (!spend)
                return;
        }

        var stalkTarget = ResolveTarget(strategy.SoulGauge);
        if (ConeAOE && stalkTarget == null && Unlocked(AID.GrimSwathe))
            PushOGCD(AID.GrimSwathe, BestConeTarget ?? target.Actor, 62);
        else
            PushOGCD(AID.BloodStalk, stalkTarget ?? target.Actor, 62);
    }

    private void UseIngress(in Strategy strategy, Enemy? target)
    {
        var prio = strategy.Ingress.Priority(ActionQueue.Priority.Low + 40);
        switch (strategy.Ingress.Value)
        {
            case IngressStrategy.Dodge:
                if (Player.InCombat && DodgeDirection(target) is Angle dir)
                    PushAction(ActionID.MakeSpell(AID.HellsIngress), Player, prio, facing: dir);
                break;
            case IngressStrategy.Position:
                var dest = ResolveTargetLocation(strategy.Ingress.TrackRaw);
                if (dest != default && !Player.Position.InCircle(dest, 3))
                    PushAction(ActionID.MakeSpell(AID.HellsIngress), Player, prio, facing: Angle.FromDirection(dest - Player.Position));
                break;
        }
    }

    // ingress dashes 15y forward; if we're standing in an imminent aoe, pick a safe landing spot, preferring the one closest to the target
    private Angle? DodgeDirection(Enemy? target)
    {
        var soon = World.FutureTime(2.5f);
        if (!Hints.ForbiddenZones.Any(z => z.activation <= soon && z.shape.Check(Player.Position)))
            return null;

        var later = World.FutureTime(5);
        Angle? best = null;
        var bestDist = float.MaxValue;
        for (var i = 0; i < 16; ++i)
        {
            var dir = (i * 22.5f).Degrees();
            var landing = Player.Position + dir.ToDirection() * 15;
            if (!Hints.PathfindMapBounds.Contains(landing - Hints.PathfindMapCenter) || Hints.ForbiddenZones.Any(z => z.activation <= later && z.shape.Check(landing)))
                continue;

            var dist = target != null ? (landing - target.Actor.Position).Length() : 0;
            if (dist < bestDist)
                (best, bestDist) = (dir, dist);
        }
        return best;
    }

    #endregion

    #region Positionals

    private bool AllowTrueNorth(in Strategy strategy) => strategy.TrueNorth.Value switch
    {
        TrueNorthStrategy.Automatic => true,
        // only spend the last charge if it comes back before gluttony, or if this is the gluttony window
        TrueNorthStrategy.PoolGluttony or TrueNorthStrategy.Bundle => Executioner || SoulReaverStacks >= 2 || MaxChargesIn(ClassShared.AID.TrueNorth) <= GCD || MaxChargesIn(ClassShared.AID.TrueNorth) < ReadyIn(AID.Gluttony),
        _ => false
    };

    private void UpdatePositionals(in Strategy strategy, Enemy target)
    {
        var (pos, imminent) = (Positional.Any, false);
        if (Unlocked(AID.Gibbet) && !ConeAOE && !Enshrouded)
        {
            pos = EnhancedGibbetLeft > 0 ? Positional.Flank
                : EnhancedGallowsLeft > 0 ? Positional.Rear
                : CurrentPositional(target.Actor) == Positional.Rear ? Positional.Rear : Positional.Flank;
            imminent = SoulReaver;
        }
        RecommendPositional(target, pos, imminent, InMelee && AllowTrueNorth(strategy));
    }

    private void AddGoalZone(Enemy target, bool allowAoE)
    {
        var (_, pos, imminent, _) = Hints.RecommendedPositional;
        var single = Hints.GoalSingleTarget(target.Actor, imminent ? pos : Positional.Any, Player, World.Actors, 3);
        Hints.GoalZones.Add(allowAoE && Unlocked(AID.SpinningScythe) ? GoalCombined(single, Hints.GoalAOECircle(5), 3) : single);
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        // the balance: potion in the weave slot before arcane circle
        PotionStrategy.AlignWithBurst => Player.InCombat && InBossFight && (CircleIn <= GCD + GCDLength || CircleLeft > 0 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => Player.InCombat,
        _ => false
    };

    private float DDLeft(Enemy? target)
    {
        if (target == null)
            return 0;
        if (target.ForbidDOTs || !Unlocked(AID.ShadowofDeath))
            return float.MaxValue;
        return StatusDetails(target.Actor, SID.DeathsDesign, Player.InstanceID, 60).Left;
    }

    #endregion
}
