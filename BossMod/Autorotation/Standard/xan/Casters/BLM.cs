using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class BLM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player, PotionType.Intelligence)
{
    public enum Track { Scathe = SharedTrack.Count, Thunder }
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

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan BLM", "Black Mage", "Standard rotation (xan)|Casters", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.BLM, Class.THM), 100);

        def.DefineShared().AddAssociatedActions(AID.LeyLines);

        def.Define(Track.Scathe).As<ScatheStrategy>("Scathe")
            .AddOption(ScatheStrategy.Forbid, "Forbid")
            .AddOption(ScatheStrategy.Allow, "Allow");

        def.Define(Track.Thunder).As<ThunderStrategy>("DoT")
            .AddOption(ThunderStrategy.Automatic, "Automatic", "Automatically refresh on main target according to standard rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(ThunderStrategy.Delay, "Delay", "Don't apply")
            .AddOption(ThunderStrategy.Force, "Force", "Force refresh ASAP", supportedTargets: ActionTargets.Hostile)
            .AddOption(ThunderStrategy.InstantOnly, "InstantOnly", "Allow Thunder if an instant cast is needed, but don't try to maintain uptime", supportedTargets: ActionTargets.Hostile)
            .AddOption(ThunderStrategy.ForbidInstant, "ForbidInstant", "Use only for standard refresh, not as a utility instant cast", supportedTargets: ActionTargets.Hostile);

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

    private readonly float[] EnemyDotTimers = new float[100];

    private float CalculateDotTimer(Actor? t) => t == null ? float.MaxValue : Utils.MaxAll(
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

    public enum GCDPriority
    {
        None = 0,
        InstantMove = 100,
        SwapSlow = 300, // slowcast elemental refreshes are undesirable
        Despair = 400, // F4 preferred over despair to get stacks for FS
        Standard = 500, // aka F4
        InstantWeave = 600, // if we want to use manafont/transpose ASAP (TODO: or utility actions?)
        DotRefresh = 650, // thunder refresh - can logically be grouped with instant weave
        High = 700, // anything more important than F4 filler - paradox so we don't miss our FS proc, xeno to prevent overcap
        Max = 900, // flare star
    }

    public enum InstantCastPriority
    {
        TP = 0,
        Paradox = 5,
        Firestarter = 10,
        Polyglot = 15
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
            if (Fire > 0 && Unlocked(AID.Transpose) && ReadyIn(AID.Transpose) == 0)
            {
                var canFS = AstralSoul == 6 || AstralSoul >= 3 && MP >= 800;
                if (!canFS && Player.HPMP.CurMP < Player.HPMP.MaxMP)
                    PushOGCD(AID.Transpose, Player, GCDPriority.Standard);
            }

            if (Unlocked(AID.UmbralSoul) && Ice > 0 && (Ice < 3 || Hearts < MaxHearts || Player.HPMP.CurMP < Player.HPMP.MaxMP))
                PushGCD(AID.UmbralSoul, Player, GCDPriority.Standard);

            return;
        }

        GoalZoneSingle(25);

        if (Player.InCombat && World.Actors.FirstOrDefault(x => x.OID == 0x179 && x.OwnerID == Player.InstanceID) is Actor ll)
            Hints.GoalZones.Add(p => p.InCircle(ll.Position, 3) ? 0.5f : 0);

        if (Polyglot < MaxPolyglot)
            PushOGCD(AID.Amplifier, Player);

        if (ShouldTriplecast(strategy))
            PushOGCD(AID.Triplecast, Player);

        if (ShouldUseLeylines(strategy))
            PushOGCD(AID.LeyLines, Player);

        var minUsableMP = Unlocked(AID.Despair) || Hearts > 0 ? 800 : 1600;
        if (MP < minUsableMP && Fire > 0)
            PushOGCD(AID.Manafont, Player);

        if (ShouldTranspose(strategy))
            PushOGCD(AID.Transpose, Player);

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

        if (strategy.Option(Track.Scathe).As<ScatheStrategy>() == ScatheStrategy.Allow && MP >= 800)
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
        T1(strategy);

        if (Fire < 3 && Unlocked(AID.Fire3))
            PushGCD(AID.Fire3, primaryTarget, GCDPriority.Standard);

        if (AstralSoul == 6)
            PushGCD(AID.FlareStar, BestAOETarget, GCDPriority.Max);

        var haveInstant = GetCastTime(AID.Fire4) == 0;

        if (Unlocked(AID.Fire4))
        {
            var f4Cost = Hearts > 0 ? 800 : 1600;

            if (Fire == 3)
            {
                if (ShouldUseLeylines(strategy, 1) && !haveInstant)
                    TryInstantCast(strategy, primaryTarget, GCDPriority.InstantWeave);

                // despair requires 800 MP
                if (MP < 800)
                {
                    if (CanWeave(AID.Manafont, 1))
                        TryInstantCast(strategy, primaryTarget, GCDPriority.InstantWeave);

                    PushGCD(AID.Blizzard3, primaryTarget, GCDPriority.Standard);
                    return;
                }

                if (MP >= f4Cost)
                {
                    // TODO despair used to be optimal at <2400 MP, but now we have to worry about losing an astral soul stack
                    //if (MP < f4Cost + 800 && !CanWeave(AID.Manafont, 1))
                    //    PushGCD(AID.Despair, primaryTarget);

                    PushGCD(AID.Fire4, primaryTarget, GCDPriority.Standard);
                }

                if (Paradox && MP >= 1600)
                {
                    var paraPrio = ForMove(InstantCastPriority.Paradox);

                    if (MP < f4Cost * 2 && AstralSoul < 5)
                        paraPrio = GCDPriority.High;

                    PushGCD(AID.Paradox, primaryTarget, paraPrio);
                }

                // fallback for F4 not being unlocked yet
                if (MP >= 1600)
                    PushGCD(AID.Fire1, primaryTarget, GCDPriority.Standard);

                // only use despair at low MP since predicted cast time might otherwise cause us to end fire phase early
                if (MP < 1600)
                    PushGCD(AID.Despair, primaryTarget, GCDPriority.Despair);

                PushGCD(AID.Blizzard3, primaryTarget, GCDPriority.SwapSlow);

                // use as instant cast, otherwise save for UI switch
                if (Firestarter)
                    PushGCD(AID.Fire3, primaryTarget, ForMove(InstantCastPriority.Firestarter));
            }
        }
        else if (Unlocked(AID.Fire3))
        {
            if (Firestarter)
                PushGCD(AID.Fire3, primaryTarget, GCDPriority.Standard);

            if (MP >= 1600)
                PushGCD(AID.Fire1, primaryTarget, GCDPriority.Standard);
            else
                PushGCD(AID.Blizzard3, primaryTarget, GCDPriority.SwapSlow);
        }
        else if (MP >= 1600)
            PushGCD(AID.Fire1, primaryTarget, GCDPriority.Standard);
        else
        {
            TryInstantOrTranspose(strategy, primaryTarget);
            PushGCD(AID.Blizzard1, primaryTarget, GCDPriority.SwapSlow);
        }
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

        TryInstantCast(strategy, BestAOETarget, GCDPriority.SwapSlow);
    }

    private void FireAOELowLevel(StrategyValues strategy, Enemy? primaryTarget)
    {
        T2(strategy);
        T1(strategy);

        if (MP >= 3000)
            PushGCD(AID.Fire2, BestAOETarget, GCDPriority.Standard);
        else if (MP >= 800 && Unlocked(AID.Flare))
            PushGCD(AID.Flare, BestAOETarget, GCDPriority.Standard);
        else
        {
            if (!Unlocked(TraitID.AspectMastery3))
                TryInstantOrTranspose(strategy, primaryTarget);

            PushGCD(AID.Blizzard2, BestAOETarget, GCDPriority.SwapSlow);
        }
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
        T1(strategy);

        if (Ice < 3 && Unlocked(AID.Blizzard3))
        {
            if (!CanFitGCD(InstantCastLeft))
                PushGCD(AID.Swiftcast, Player, GCDPriority.High);

            PushGCD(AID.Blizzard3, primaryTarget, GCDPriority.Standard);
        }

        if (AlmostMaxMP && Unlocked(AID.Fire3))
        {
            // recommended to always use ice paradox at max MP even if we don't have FS
            if (Paradox)
                PushGCD(AID.Paradox, primaryTarget, GCDPriority.High);

            if (Firestarter && CanWeave(AID.Transpose, 1) && !CanFitGCD(InstantCastLeft))
                TryInstantCast(strategy, primaryTarget, GCDPriority.InstantWeave, useFirestarter: false);

            PushGCD(AID.Fire3, primaryTarget, GCDPriority.Standard);
        }
        else if (Unlocked(AID.Blizzard4))
            PushGCD(AID.Blizzard4, primaryTarget, GCDPriority.Standard);
        else if (AlmostMaxMP)
        {
            TryInstantOrTranspose(strategy, primaryTarget);
            PushGCD(AID.Fire1, primaryTarget, GCDPriority.SwapSlow);
        }
        else
            PushGCD(AID.Blizzard1, primaryTarget, GCDPriority.Standard);

    }

    private void IcePhaseAOE(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Ice == 0)
        {
            if (MP >= Player.HPMP.MaxMP * 0.96f)
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

        TryInstantCast(strategy, primaryTarget, GCDPriority.SwapSlow);
    }

    private void IceAOELowLevel(StrategyValues strategy, Enemy? primaryTarget)
    {
        T2(strategy);
        T1(strategy);

        if (MP >= Player.HPMP.MaxMP * 0.96f)
        {
            if (!Unlocked(TraitID.AspectMastery3))
                TryInstantOrTranspose(strategy, primaryTarget);

            PushGCD(AID.Fire2, BestAOETarget, GCDPriority.Standard);
        }
        else if (Ice == 3)
            PushGCD(AID.Freeze, BestAOETarget, GCDPriority.Standard);
        else
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

    private bool ShouldTriplecast(StrategyValues strategy) => CanWeave(MaxChargesIn(AID.Triplecast), 0.6f); // add strategy track, triplecast is no longer a gain during burst

    private bool ShouldUseLeylines(StrategyValues strategy, int extraGCDs = 0)
        => CanWeave(MaxChargesIn(AID.LeyLines), 0.6f, extraGCDs)
        && strategy.Option(SharedTrack.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;

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
        else
            return Firestarter && Ice > 0 && Hearts == MaxHearts;
    }
}
