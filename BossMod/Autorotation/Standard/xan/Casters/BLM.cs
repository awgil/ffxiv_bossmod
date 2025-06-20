using BossMod.BLM;
using BossMod.Data;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class BLM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player, PotionType.Intelligence)
{
    public enum Track { Scathe = SharedTrack.Buffs, Thunder, Leylines, Triplecast, Iainuki, Zeninage, LLMove }
    public enum ScatheStrategy
    {
        Forbid,
        Allow
    }

    public enum ThunderStrategy
    {
        Automatic,
        Delay,
        Force,
        InstantOnly,
        ForbidInstant
    }

    public enum LeylinesStrategy
    {
        OpenerOnly,
        Delay,
        Force
    }

    public enum TriplecastStrategy
    {
        Automatic,
        Delay,
        Force
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan BLM", "Black Mage", "Standard rotation (xan)|Casters", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.BLM, Class.THM), 100);

        def.DefineSharedTA();

        def.Define(Track.Scathe).As<ScatheStrategy>("Scathe")
            .AddOption(ScatheStrategy.Forbid, "Forbid")
            .AddOption(ScatheStrategy.Allow, "Allow");

        def.Define(Track.Thunder).As<ThunderStrategy>("DoT", "Thunder")
            .AddOption(ThunderStrategy.Automatic, "Automatic", "Automatically refresh on main target according to standard rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(ThunderStrategy.Delay, "Delay", "Don't apply")
            .AddOption(ThunderStrategy.Force, "Force", "Force refresh ASAP", supportedTargets: ActionTargets.Hostile)
            .AddOption(ThunderStrategy.InstantOnly, "InstantOnly", "Allow Thunder if an instant cast is needed, but don't try to maintain uptime", supportedTargets: ActionTargets.Hostile)
            .AddOption(ThunderStrategy.ForbidInstant, "ForbidInstant", "Use only for standard refresh, not as a utility instant cast", supportedTargets: ActionTargets.Hostile);

        def.Define(Track.Leylines).As<LeylinesStrategy>("LL", "Leylines")
            .AddOption(LeylinesStrategy.OpenerOnly, "Opener", "Use Leylines in opener; otherwise do not use automatically")
            .AddOption(LeylinesStrategy.Delay, "Delay", "Do not use")
            .AddOption(LeylinesStrategy.Force, "Force", "Use ASAP", effect: 20, defaultPriority: DefaultOGCDPriority)
            .AddAssociatedActions(AID.LeyLines, AID.Retrace, AID.BetweenTheLines);

        def.Define(Track.Triplecast).As<TriplecastStrategy>("TC", "Triplecast")
            .AddOption(TriplecastStrategy.Automatic, "Automatic", "Use for instant fire/ice swaps, otherwise hold - NOT YET IMPLEMENTED")
            .AddOption(TriplecastStrategy.Delay, "Delay", "Don't use")
            .AddOption(TriplecastStrategy.Force, "Force", "Use ASAP", effect: 15, defaultPriority: DefaultOGCDPriority)
            .AddAssociatedActions(AID.Triplecast);

        def.AbilityTrack(Track.Iainuki, "Iainuki", "PSAM: Use Iainuki on cooldown");
        def.AbilityTrack(Track.Zeninage, "Zeninage", "PSAM: Use Zeninage under raid buffs (costs 10,000 gil)");

        def.AbilityTrack(Track.LLMove, "LLMove", "Allow automatic usage of Leylines while moving")
            .AddAssociatedActions(AID.LeyLines);

        return def;
    }

    public int Element; // -3 (ice) <=> 3 (fire), 0 for none
    public float NextPolyglot; // max 30
    public int Hearts; // max 3
    public int Polyglot;
    public int AstralSoul; // max 6
    public bool Paradox;

    public float TriplecastLeft => Triplecast.Left;
    public (float Left, int Stacks) Triplecast;
    public bool Thunderhead;
    public bool Firestarter;
    public bool InLeyLines;

    public int Fire => Math.Max(0, Element);
    public int Ice => Math.Max(0, -Element);

    public int MaxPolyglot => Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : 1;
    public int MaxHearts => Unlocked(TraitID.UmbralHeart) ? 3 : 0;

    public int NumAOETargets;
    public int NumAOEDotTargets;

    private Enemy? BestAOETarget;
    private Enemy? BestThunderTarget;
    private Enemy? BestAOEThunderTarget;

    public float TargetThunderLeft;

    public const int AOEBreakpoint = 2;

    private float InstantCastLeft => MathF.Max(TriplecastLeft, SwiftcastLeft);

    protected override float GetCastTime(AID aid)
    {
        if (TriplecastLeft > GCD)
            return 0;

        if (aid == AID.Despair && Unlocked(TraitID.EnhancedAstralFire))
            return 0;

        var aspect = ActionDefinitions.Instance.Spell(aid)!.Aspect;

        if (aid == AID.Fire3 && Firestarter
            || aid == AID.Foul && Unlocked(TraitID.EnhancedFoul)
            || aspect == ActionAspect.Thunder && Thunderhead)
            return 0;

        var castTime = base.GetCastTime(aid);
        if (castTime == 0)
            return 0;

        if (Element == -3 && aspect == ActionAspect.Fire || Element == 3 && aspect == ActionAspect.Ice)
            castTime *= 0.5f;

        return castTime;
    }

    private int GetManaCost(AID aid)
    {
        int adjustFire(int cost) => Ice > 0
            ? 0
            : Fire > 0 && Hearts == 0
                ? cost * 2
                : cost;

        return aid switch
        {
            AID.Despair or AID.Flare => 800, // min cost
            AID.Paradox => Fire > 0 ? 1600 : 0, // unaspected, unaffected by elemental gauge
            AID.Scathe => 800, // unaspected
            AID.Fire3 => Firestarter ? 0 : adjustFire(2000),
            AID.Fire1 or AID.Fire4 => adjustFire(800),
            AID.Fire2 or AID.HighFire2 => adjustFire(1500),
            _ => 0
        };
    }

    private readonly float[] EnemyDotTimers = new float[100];

    private float CalculateDotTimer(Actor? t) => t == null || t.PendingDead ? float.MaxValue : Utils.MaxAll(
        StatusDetails(t, SID.Thunder, Player.InstanceID, 24).Left,
        StatusDetails(t, SID.ThunderII, Player.InstanceID, 18).Left,
        StatusDetails(t, SID.ThunderIII, Player.InstanceID, 27).Left,
        StatusDetails(t, SID.ThunderIV, Player.InstanceID, 21).Left,
        StatusDetails(t, SID.HighThunder, Player.InstanceID, 30).Left,
        StatusDetails(t, SID.HighThunderII, Player.InstanceID, 24).Left
    );

    private float GetTargetThunderLeft(Actor? t) => t == null ? float.MaxValue : EnemyDotTimers.BoundSafeAt(t.CharacterSpawnIndex);

    private bool DotExpiring(Actor? t) => DotExpiring(GetTargetThunderLeft(t));
    private bool DotExpiring(float timer) => !CanFitGCD(timer, 1);

    private bool AlmostMaxMP => MP >= Player.HPMP.MaxMP * 0.96f;
    private int MinAstralFireMP => Unlocked(AID.Despair) ? 800 : FireSpellCost;
    private int FireSpellCost => Hearts > 0 ? 800 : 1600;

    public enum GCDPriority
    {
        None = 0,
        InstantMove = 100,
        Standard = 500, // aka F4
        InstantWeave = 600, // if we want to use manafont/transpose ASAP (TODO: or utility actions?)
        DotRefresh = 650, // thunder refresh - could logically be grouped with instant weave
        High = 700, // anything more important than F4 filler - paradox so we don't miss our FS proc, xeno to prevent overcap
        Max = 900, // flare star
    }

    public enum InstantCastPriority
    {
        TP = 0,
        Paradox = 5,
        Firestarter = 10,
        Polyglot = 15,
        Despair = 20
    }

    private static GCDPriority ForMove(InstantCastPriority p) => GCDPriority.InstantMove + (int)p;

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 25);

        var gauge = World.Client.GetGauge<BlackMageGauge>();

        Element = gauge.ElementStance;
        NextPolyglot = gauge.EnochianTimer * 0.001f;
        Hearts = gauge.UmbralHearts;
        Polyglot = gauge.PolyglotStacks;
        Paradox = gauge.ParadoxActive;
        AstralSoul = gauge.AstralSoulStacks;

        Triplecast = Status(SID.Triplecast);
        Thunderhead = Player.FindStatus(SID.Thunderhead) != null;
        Firestarter = Player.FindStatus(SID.Firestarter) != null;
        InLeyLines = Player.FindStatus(SID.CircleOfPower) != null;

        for (var i = 0; i < Hints.Enemies.Length; i++)
            EnemyDotTimers[i] = CalculateDotTimer(Hints.Enemies[i]?.Actor);

        (BestAOETarget, NumAOETargets) = SelectTargetByHP(strategy, primaryTarget, 25, IsSplashTarget);

        var dotTarget = Hints.FindEnemy(ResolveTargetOverride(strategy.Option(Track.Thunder).Value)) ?? primaryTarget;

        if (strategy.Option(Track.Thunder).As<ThunderStrategy>() is ThunderStrategy.Force or ThunderStrategy.Delay)
        {
            BestThunderTarget = dotTarget;
            TargetThunderLeft = GetTargetThunderLeft(dotTarget?.Actor);
        }
        else
        {
            (BestThunderTarget, TargetThunderLeft) = SelectDotTarget(strategy, dotTarget, GetTargetThunderLeft, 2);
        }

        (BestAOEThunderTarget, NumAOEDotTargets) = SelectTarget(strategy, dotTarget, 25, (primary, other) => DotExpiring(other) && Hints.TargetInAOECircle(other, primary.Position, 5));

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.Fire3))
                PushGCD(AID.Fire3, primaryTarget, GCDPriority.Standard);

            return;
        }

        if (primaryTarget == null)
        {
            if (ReadyIn(AID.Transpose) == 0)
            {
                // swap back to ice in downtime if we can't fit in another FS
                if (Fire > 0)
                {
                    var canFS = AstralSoul == 6 || AstralSoul >= 3 && MP >= 800;
                    if (!canFS && Player.HPMP.CurMP < Player.HPMP.MaxMP)
                        PushOGCD(AID.Transpose, Player);
                }

                // at max MP, switch back to AF1 to use firestarter
                if (Ice > 0 && Firestarter && Player.HPMP.CurMP == Player.HPMP.MaxMP)
                    PushOGCD(AID.Transpose, Player);
            }

            if (Unlocked(AID.UmbralSoul) && Ice > 0 && (Ice < 3 || Hearts < MaxHearts || Player.HPMP.CurMP < Player.HPMP.MaxMP))
                PushGCD(AID.UmbralSoul, Player, GCDPriority.Standard);

            return;
        }

        GoalZoneSingle(25);

        if (Player.InCombat && World.Actors.FirstOrDefault(x => x.OID == 0x179 && x.OwnerID == Player.InstanceID) is Actor ll)
            Hints.GoalZones.Add(p => p.InCircle(ll.Position, 3) ? 0.5f : 0);

        if (strategy.Enabled(Track.Zeninage) && RaidBuffsLeft > GCD && PhantomReadyIn(PhantomID.Zeninage) <= GCD)
            PushGCD((AID)PhantomID.Zeninage, primaryTarget, GCDPriority.Max);

        if (strategy.Enabled(Track.Iainuki) && (CombatTimer > 10 || RaidBuffsLeft > GCD))
        {
            var ready = PhantomReadyIn(PhantomID.Iainuki);
            if (ready <= GCD)
                PushGCD((AID)PhantomID.Iainuki, primaryTarget, GCDPriority.Max);

            if (ready <= GCD + GCDLength * 2)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget.Actor, 8));
        }

        if (Polyglot < MaxPolyglot)
            PushOGCD(AID.Amplifier, Player);

        UseLeylines(strategy, primaryTarget);
        UseTriplecastForced(strategy);

        if (MP < MinAstralFireMP && Fire > 0)
            PushOGCD(AID.Manafont, Player);

        if (ShouldTranspose(strategy))
            PushOGCD(AID.Transpose, Player);

        T2(strategy);
        T1(strategy);

        if (Fire > 0)
            FirePhase(strategy, primaryTarget);
        else if (Ice > 0)
            IcePhase(strategy, primaryTarget);
        else if (MP >= 9600)
            FirePhase(strategy, primaryTarget);
        else
            IcePhase(strategy, primaryTarget);

        if (Polyglot > 0)
        {
            var prio = Polyglot < MaxPolyglot || CanFitGCD(NextPolyglot, 1) ? ForMove(InstantCastPriority.Polyglot) : GCDPriority.High;

            if (NumAOETargets >= AOEBreakpoint)
                PushGCD(AID.Foul, BestAOETarget, prio);

            PushGCD(AID.Xenoglossy, primaryTarget, prio);
        }

        if (strategy.Option(Track.Scathe).As<ScatheStrategy>() == ScatheStrategy.Allow)
            PushGCD(AID.Scathe, primaryTarget, GCDPriority.InstantMove - 50);
    }

    private void FirePhase(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (NumAOETargets >= AOEBreakpoint)
        {
            if (Unlocked(TraitID.UmbralHeart))
                FirePhaseAOE(strategy);
            else if (Unlocked(AID.Fire2)) // loll
                FireAOELowLevel(strategy, primaryTarget);
            else
                IceAOELowLevel(strategy, primaryTarget);
        }
        else
            FirePhaseST(strategy, primaryTarget);
    }

    private void FirePhaseST(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Fire < 3)
        {
            if (!Firestarter)
            {
                // generate firestarter via paradox, then use fs to get f3
                // this results in us not getting another free paradox until next manafont
                if (Paradox)
                    PushGCD(AID.Paradox, primaryTarget, GCDPriority.High);
                else if (!CanFitGCD(InstantCastLeft) && Unlocked(AID.Fire3))
                    PushGCD(AID.Swiftcast, Player, GCDPriority.High);
            }

            PushGCD(AID.Fire3, primaryTarget, GCDPriority.High);
        }

        if (AstralSoul == 6)
            PushGCD(AID.FlareStar, BestAOETarget, GCDPriority.Max);

        if (Unlocked(AID.Fire4))
            StandardF4(strategy, primaryTarget);
        else if (Unlocked(AID.Fire3))
        {
            if (Firestarter)
                PushGCD(AID.Fire3, primaryTarget, GCDPriority.Standard);

            PushGCD(AID.Fire1, primaryTarget, GCDPriority.Standard);
            PushGCD(AID.Blizzard3, primaryTarget, GCDPriority.Standard, mpCutoff: FireSpellCost);
        }
        else
        {
            PushGCD(AID.Fire1, primaryTarget, GCDPriority.Standard);
            if (MP < FireSpellCost)
            {
                TryInstantOrTranspose(strategy, primaryTarget);
                PushGCD(AID.Blizzard1, primaryTarget, GCDPriority.Standard, mpCutoff: FireSpellCost);
            }
        }
    }

    private void StandardF4(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Paradox)
        {
            var paraPrio = ForMove(InstantCastPriority.Paradox);

            if (MP < FireSpellCost * 2 && AstralSoul < 5)
                paraPrio = GCDPriority.High;

            // spend this F3 before we get another free one
            if (Firestarter)
                PushGCD(AID.Fire3, primaryTarget, paraPrio + 1);
            PushGCD(AID.Paradox, primaryTarget, paraPrio);
        }

        // force instant cast if we want to manafont or accelerate B3 swap
        if (MP < MinAstralFireMP && (CanWeave(AID.Manafont, 1) || SwiftB3(strategy) && CanWeave(AID.Triplecast, 1)))
            TryInstantCast(strategy, primaryTarget, GCDPriority.InstantWeave);

        // flare acceleration
        if (AstralSoul >= 3)
        {
            var castsNeeded = 6 - AstralSoul;
            var f4MPNeeded = 1600 * castsNeeded;
            var canManafont = CanWeave(AID.Manafont, castsNeeded + 1); // assume we can spend at least one instant cast on manafont weave
            if (MP < f4MPNeeded && !canManafont)
                PushGCD(AID.Flare, BestAOETarget, GCDPriority.High);
        }

        // TODO: BLM doesn't really fit the priority system that well because of the MP cutoff stuff
        PushGCD(AID.Fire4, primaryTarget, GCDPriority.Standard);
        PushGCD(AID.Despair, primaryTarget, GCDPriority.Standard, mpCutoff: FireSpellCost);
        PushGCD(AID.Blizzard3, primaryTarget, GCDPriority.Standard, mpCutoff: MinAstralFireMP);

        // use as instant cast, otherwise save for UI switch
        if (Firestarter)
            PushGCD(AID.Fire3, primaryTarget, ForMove(InstantCastPriority.Firestarter));

    }

    private void FirePhaseAOE(StrategyValues strategy)
    {
        T2(strategy, GCDPriority.InstantMove);

        if (Fire == 0)
            PushGCD(AID.Fire2, BestAOETarget, GCDPriority.Standard);

        if (AstralSoul == 6)
            PushGCD(AID.FlareStar, BestAOETarget, GCDPriority.Max);

        if (Unlocked(AID.Flare) && MP >= 800 && Fire > 0)
            PushGCD(AID.Flare, BestAOETarget, GCDPriority.Standard);

        TryInstantCast(strategy, BestAOETarget, GCDPriority.InstantMove);
    }

    private void FireAOELowLevel(StrategyValues strategy, Enemy? primaryTarget)
    {
        T2(strategy);
        T1(strategy);

        PushGCD(AID.Fire2, BestAOETarget, GCDPriority.Standard);
        PushGCD(AID.Flare, BestAOETarget, GCDPriority.Standard, mpCutoff: 3000);

        if (!Unlocked(TraitID.AspectMastery3) && MP < FireSpellCost)
            TryInstantOrTranspose(strategy, primaryTarget);

        PushGCD(AID.Blizzard2, BestAOETarget, GCDPriority.Standard, mpCutoff: MinAstralFireMP);
    }

    private void IcePhase(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (NumAOETargets >= AOEBreakpoint && Unlocked(AID.Blizzard2))
        {
            if (Unlocked(TraitID.UmbralHeart))
                IcePhaseAOE(strategy, primaryTarget);
            else
                IceAOELowLevel(strategy, primaryTarget);
        }
        else
            IcePhaseST(strategy, primaryTarget);
    }

    private void IcePhaseST(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Ice < 3)
        {
            if (SwiftB3(strategy) && !CanFitGCD(InstantCastLeft))
                PushOGCD(AID.Triplecast, Player);

            PushGCD(AID.Blizzard3, primaryTarget, GCDPriority.Standard);
        }

        if (AlmostMaxMP)
        {
            // recommended to always use ice paradox at max MP even if we don't have FS
            if (Paradox)
                PushGCD(AID.Paradox, primaryTarget, GCDPriority.High);

            if (Firestarter && CanWeave(AID.Transpose, 1) && !CanFitGCD(InstantCastLeft))
                TryInstantCast(strategy, primaryTarget, GCDPriority.InstantWeave, useFirestarter: false);

            PushGCD(AID.Fire3, primaryTarget, GCDPriority.InstantWeave);

            if (!Unlocked(AID.Fire3))
                PushGCD(AID.Fire1, primaryTarget, GCDPriority.Standard);
        }

        PushGCD(AID.Blizzard4, primaryTarget, GCDPriority.Standard);

        if (!Unlocked(AID.Blizzard4))
            PushGCD(AID.Blizzard1, primaryTarget, GCDPriority.Standard);
    }

    private void IcePhaseAOE(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Ice == 0)
        {
            if (AlmostMaxMP)
                PushGCD(AID.Fire2, BestAOETarget, GCDPriority.Standard);

            PushGCD(AID.Blizzard2, BestAOETarget, GCDPriority.Standard);
        }
        else if (Hearts < MaxHearts)
        {
            // freeze gain on 3, everything else gain on 2
            if (NumAOETargets > 2)
                PushGCD(AID.Freeze, BestAOETarget, GCDPriority.Standard);
            else
                PushGCD(AID.Blizzard4, primaryTarget, GCDPriority.Standard);
        }

        if (Paradox)
            PushGCD(AID.Paradox, primaryTarget, ForMove(InstantCastPriority.Paradox));

        TryInstantCast(strategy, primaryTarget, GCDPriority.InstantMove);
    }

    private void IceAOELowLevel(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (AlmostMaxMP)
        {
            if (!Unlocked(TraitID.AspectMastery3))
                TryInstantOrTranspose(strategy, primaryTarget);

            PushGCD(AID.Fire2, BestAOETarget, GCDPriority.Standard);
        }

        if (Ice == 3)
            PushGCD(AID.Freeze, BestAOETarget, GCDPriority.Standard);

        PushGCD(AID.Blizzard2, BestAOETarget, GCDPriority.Standard);
    }

    private static GCDPriority Priomax(GCDPriority g1, GCDPriority g2) => g1 > g2 ? g1 : g2;

    private void T1(StrategyValues strategy, GCDPriority prioForInstant = GCDPriority.InstantMove)
    {
        if (!Thunderhead)
            return;

        var prioStandard = DotExpiring(TargetThunderLeft) ? GCDPriority.DotRefresh : GCDPriority.None;

        var prio = strategy.Option(Track.Thunder).As<ThunderStrategy>() switch
        {
            // use to refresh normally, or use as utility cast
            ThunderStrategy.Automatic => Priomax(prioStandard, prioForInstant),
            // only use to refresh normally
            ThunderStrategy.ForbidInstant => prioStandard,
            // ignore timer, refresh asap
            ThunderStrategy.Force => GCDPriority.Max,
            // only use for utility, ignore timer
            ThunderStrategy.InstantOnly => prioForInstant,
            _ => GCDPriority.None
        };

        PushGCD(AID.Thunder1, BestThunderTarget, prio);
    }

    private void T2(StrategyValues strategy, GCDPriority prioForInstant = GCDPriority.InstantMove)
    {
        if (!Thunderhead)
            return;

        var prioStandard = NumAOEDotTargets >= AOEBreakpoint ? GCDPriority.DotRefresh : GCDPriority.None;
        var prioInstant = NumAOETargets >= AOEBreakpoint ? prioForInstant + (int)InstantCastPriority.TP : GCDPriority.None;

        var prio = strategy.Option(Track.Thunder).As<ThunderStrategy>() switch
        {
            // use to refresh normally, or use as utility cast
            ThunderStrategy.Automatic => Priomax(prioStandard, prioInstant),
            // only use to refresh normally
            ThunderStrategy.ForbidInstant => prioStandard,
            // ignore timer, just check if we have enough AOE targets
            ThunderStrategy.Force => NumAOETargets >= AOEBreakpoint ? GCDPriority.Max : GCDPriority.None,
            // only use for utility, ignore timer
            ThunderStrategy.InstantOnly => prioInstant,
            _ => GCDPriority.None
        };

        PushGCD(AID.Thunder2, BestAOEThunderTarget, prio);
    }

    private void Choose(AID st, AID aoe, Enemy? primaryTarget, GCDPriority prio)
    {
        if (NumAOETargets >= AOEBreakpoint && Unlocked(aoe))
            PushGCD(aoe, BestAOETarget, prio);
        else
            PushGCD(st, primaryTarget, prio);
    }

    private void TryInstantCast(
        StrategyValues strategy,
        Enemy? primaryTarget,
        GCDPriority prioBase,
        bool useFirestarter = true,
        bool useThunderhead = true,
        bool usePolyglot = true
    )
    {
        if (Polyglot > 0 && usePolyglot)
            Choose(AID.Xenoglossy, AID.Foul, primaryTarget, prioBase + (int)InstantCastPriority.Polyglot);

        if (Firestarter && useFirestarter)
            PushGCD(AID.Fire3, primaryTarget, prioBase + (int)InstantCastPriority.Firestarter);

        if (Fire > 0 && Unlocked(TraitID.EnhancedAstralFire))
            PushGCD(AID.Despair, primaryTarget, prioBase + (int)InstantCastPriority.Despair, mpCutoff: FireSpellCost);

        if (useThunderhead)
        {
            T2(strategy, prioBase);
            T1(strategy, prioBase);
        }
    }

    private void TryInstantOrTranspose(StrategyValues strategy, Enemy? primaryTarget, bool useThunderhead = true)
    {
        if (useThunderhead)
        {
            T2(strategy, GCDPriority.InstantWeave);
            T1(strategy, GCDPriority.InstantWeave);
        }

        if (Fire > 0 && MP < 1600)
            PushGCD(AID.Manafont, Player, GCDPriority.Standard);

        if (Element != 0)
            PushGCD(AID.Transpose, Player, GCDPriority.Standard);
    }

    private void UseLeylines(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Player.FindStatus(SID.LeyLines) != null)
            return;

        var opt = strategy.Option(Track.Leylines);
        var prio = opt.As<LeylinesStrategy>() switch
        {
            LeylinesStrategy.OpenerOnly => CombatTimer < 20 ? DefaultOGCDPriority : 0,
            LeylinesStrategy.Force => opt.Priority(),
            _ => 0,
        };

        if (!strategy.Enabled(Track.LLMove) && IsMoving)
            return;

        if (prio > 0)
        {
            PushAction(AID.LeyLines, Player, prio, 0);
            if (CanWeave(AID.LeyLines, 1) && !CanFitGCD(InstantCastLeft))
                TryInstantCast(strategy, primaryTarget, GCDPriority.InstantWeave);
        }
    }

    private void UseTriplecastForced(StrategyValues strategy)
    {
        if (Player.FindStatus(SID.Triplecast) != null)
            return;

        var opt = strategy.Option(Track.Triplecast);
        if (opt.As<TriplecastStrategy>() == TriplecastStrategy.Force)
            PushAction(AID.Triplecast, Player, opt.Priority(), 0);
    }

    private bool SwiftB3(StrategyValues strategy) => strategy.Option(Track.Triplecast).As<TriplecastStrategy>() == TriplecastStrategy.Automatic;

    private bool ShouldTranspose(StrategyValues strategy)
    {
        if (!Unlocked(AID.Fire3))
            return Fire > 0 && MP < 1600 || Ice > 0 && MP > Player.HPMP.MaxMP * 0.9f;

        if (NumAOETargets >= AOEBreakpoint)
            return
                // AF: transpose if we can't flare or flare star
                AstralSoul < 6 && Fire > 0 && MP < 800
                // UI: transpose with at least one heart and enough MP to double flare
                || Ice > 0 && Hearts > 0 && MP >= 2400;

        // fire phase transpose: use TC B3
        if (Fire > 0 && MP < MinAstralFireMP && CanWeave(AID.Triplecast) && SwiftB3(strategy) && AstralSoul < 6)
            return true;

        // ice phase transpose
        var haveInstantFire = Firestarter && Ice > 0 // we have a firestarter
            || Ice == 3 && Unlocked(AID.Paradox) // transpose will give us firestarter
            || InstantCastLeft > GCD; // use tc/sc to cast regular f3

        return Hearts == MaxHearts && AlmostMaxMP && haveInstantFire && !Paradox;
    }

    private void PushGCD(AID aid, Enemy? target, GCDPriority priority, float delay = 0, int mpCutoff = int.MaxValue)
    {
        if (MP >= GetManaCost(aid))
        {
            if (MP >= mpCutoff)
                return;
            base.PushGCD(aid, target, priority, delay);
        }
    }
}
