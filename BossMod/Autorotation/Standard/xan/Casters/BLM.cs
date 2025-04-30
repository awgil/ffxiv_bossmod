using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class BLM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
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
                PushGCD(AID.Fire3, primaryTarget);

            return;
        }

        if (primaryTarget == null)
        {
            if (Fire > 0 && Unlocked(AID.Transpose) && ReadyIn(AID.Transpose) == 0)
            {
                var canFS = AstralSoul == 6 || AstralSoul >= 3 && MP >= 800;
                if (!canFS)
                    PushOGCD(AID.Transpose, Player);
            }

            if (Unlocked(AID.UmbralSoul) && Ice > 0 && (Ice < 3 || Hearts < MaxHearts))
                PushGCD(AID.UmbralSoul, Player);

            return;
        }

        GoalZoneSingle(25);

        if (Player.InCombat && World.Actors.FirstOrDefault(x => x.OID == 0x179 && x.OwnerID == Player.InstanceID) is Actor ll)
            Hints.GoalZones.Add(p => p.InCircle(ll.Position, 3) ? 0.5f : 0);

        if (Unlocked(AID.Swiftcast))
            PushOGCD(AID.Swiftcast, Player);

        if (Unlocked(AID.Amplifier) && Polyglot < MaxPolyglot)
            PushOGCD(AID.Amplifier, Player);

        if (ShouldTriplecast(strategy))
            PushOGCD(AID.Triplecast, Player);

        if (ShouldUseLeylines(strategy))
            PushOGCD(AID.LeyLines, Player);

        if (Unlocked(AID.Manafont) && MP < 800 && Fire > 0)
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
            if (NumAOETargets >= AOEBreakpoint)
                PushGCD(AID.Foul, BestAOETarget);

            PushGCD(AID.Xenoglossy, primaryTarget);
        }

        if (strategy.Option(Track.Scathe).As<ScatheStrategy>() == ScatheStrategy.Allow && MP >= 800)
            PushGCD(AID.Scathe, primaryTarget);
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
            PushGCD(AID.Fire3, primaryTarget);

        if (AstralSoul == 6)
            PushGCD(AID.FlareStar, BestAOETarget);

        if (Unlocked(AID.Fire4))
        {
            var f4Cost = Hearts > 0 ? 800 : 1600;

            if (Fire == 3)
            {
                if (ShouldUseLeylines(strategy, 1) && GetCastTime(AID.Fire4) > 0)
                    TryInstantCast(strategy, primaryTarget);

                // despair requires 800 MP
                if (MP < 800)
                {
                    if (CanWeave(AID.Manafont, 1))
                        TryInstantCast(strategy, primaryTarget);

                    PushGCD(AID.Blizzard3, primaryTarget);
                    return;
                }

                if (MP >= f4Cost)
                {
                    if (MP < f4Cost * 2 && Paradox)
                        PushGCD(AID.Paradox, primaryTarget);

                    PushGCD(AID.Fire4, primaryTarget);
                }

                if (Paradox && MP >= 1600)
                    PushGCD(AID.Paradox, primaryTarget);

                // fallback for F4 not being unlocked yet
                if (MP >= 1600)
                    PushGCD(AID.Fire1, primaryTarget);

                // only use despair at low MP since predicted cast time might otherwise cause us to end fire phase early
                if (MP < 1600)
                    PushGCD(AID.Despair, primaryTarget);

                PushGCD(AID.Blizzard3, primaryTarget);

                // use as instant cast, otherwise save for UI switch
                if (Firestarter)
                    PushGCD(AID.Fire3, primaryTarget);
            }
        }
        else if (Unlocked(AID.Fire3))
        {
            // TODO this will skip free F3 to switch to UI (so we can transpose F3 for next fire phase)
            // 1. is this a DPS gain 2. does anyone actually care about level 50
            if (MP < 1600)
            {
                // may get skipped - B3 is unlocked via quest, not level
                PushGCD(AID.Blizzard3, primaryTarget);

                TryInstantOrTranspose(strategy, primaryTarget);
            }

            PushGCD(Firestarter ? AID.Fire3 : AID.Fire1, primaryTarget);
        }
        else if (MP >= 1600)
            PushGCD(AID.Fire1, primaryTarget);
        else
        {
            TryInstantOrTranspose(strategy, primaryTarget);
            PushGCD(AID.Blizzard1, primaryTarget);
        }
    }

    private void FirePhaseAOE(StrategyValues strategy)
    {
        T2(strategy);

        if (Fire == 0)
            PushGCD(AID.Fire2, BestAOETarget);

        if (AstralSoul == 6)
            PushGCD(AID.FlareStar, BestAOETarget);

        if (Unlocked(AID.Flare) && MP >= 800 && Fire > 0)
            PushGCD(AID.Flare, BestAOETarget);

        TryInstantCast(strategy, BestAOETarget);
    }

    private void FireAOELowLevel(StrategyValues strategy, Enemy? primaryTarget)
    {
        T2(strategy);
        T1(strategy);

        if (MP >= 3000)
            PushGCD(AID.Fire2, BestAOETarget);
        else if (MP >= 800 && Unlocked(AID.Flare))
            PushGCD(AID.Flare, BestAOETarget);
        else
        {
            if (!Unlocked(TraitID.AspectMastery3))
                TryInstantOrTranspose(strategy, primaryTarget);

            PushGCD(AID.Blizzard2, BestAOETarget);
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

        if (Paradox)
            PushGCD(AID.Paradox, primaryTarget);

        if (Ice < 3 && Unlocked(AID.Blizzard3))
            PushGCD(AID.Blizzard3, primaryTarget);

        if (MP >= 10000 && Unlocked(AID.Fire3))
        {
            if (Firestarter && CanWeave(AID.Transpose, 1) && SwiftcastLeft == 0 && TriplecastLeft == 0)
                TryInstantCast(strategy, primaryTarget, useFirestarter: false);

            PushGCD(AID.Fire3, primaryTarget);
        }
        else if (Unlocked(AID.Blizzard4))
            PushGCD(AID.Blizzard4, primaryTarget);
        else if (MP >= 10000)
        {
            TryInstantOrTranspose(strategy, primaryTarget);
            PushGCD(AID.Fire1, primaryTarget);
        }
        else
            PushGCD(AID.Blizzard1, primaryTarget);

    }

    private void IcePhaseAOE(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Ice == 0)
        {
            if (MP >= 9600)
                PushGCD(AID.Fire2, BestAOETarget);

            PushGCD(AID.Blizzard2, BestAOETarget);
        }
        else if (Hearts < MaxHearts)
        {
            // freeze gain on 3, everything else gain on 2
            if (NumAOETargets > 2)
                PushGCD(AID.Freeze, BestAOETarget);
            else
                PushGCD(AID.Blizzard4, primaryTarget);
        }

        TryInstantCast(strategy, primaryTarget);
    }

    private void IceAOELowLevel(StrategyValues strategy, Enemy? primaryTarget)
    {
        T2(strategy);
        T1(strategy);

        if (MP >= 9600)
        {
            if (!Unlocked(TraitID.AspectMastery3))
                TryInstantOrTranspose(strategy, primaryTarget);

            PushGCD(AID.Fire2, BestAOETarget);
        }
        else if (Ice == 3)
            PushGCD(AID.Freeze, BestAOETarget);
        else
            PushGCD(AID.Blizzard2, BestAOETarget);
    }

    private void T1(StrategyValues strategy, bool useForInstant = false)
    {
        // TODO we can also use thunder proc to weave manafont which grants thunderhead

        var wantStandard = DotExpiring(TargetThunderLeft);
        var wantInstant = useForInstant;

        var canUse = Thunderhead && strategy.Option(Track.Thunder).As<ThunderStrategy>() switch
        {
            // use to refresh normally, or use as utility cast
            ThunderStrategy.Automatic => wantStandard || wantInstant,
            // only use to refresh normally
            ThunderStrategy.ForbidInstant => wantStandard,
            // ignore timer, refresh asap
            ThunderStrategy.Force => true,
            // only use for utility, ignore timer
            ThunderStrategy.InstantOnly => wantInstant,
            _ => false
        };

        if (canUse)
            PushGCD(AID.Thunder1, BestThunderTarget);
    }

    private void T2(StrategyValues strategy, bool useForInstant = false)
    {
        var wantStandard = NumAOEDotTargets >= AOEBreakpoint;
        var wantInstant = useForInstant && NumAOETargets >= AOEBreakpoint;

        var canUse = Thunderhead && strategy.Option(Track.Thunder).As<ThunderStrategy>() switch
        {
            // use to refresh normally, or use as utility cast
            ThunderStrategy.Automatic => wantStandard || wantInstant,
            // only use to refresh normally
            ThunderStrategy.ForbidInstant => wantStandard,
            // ignore timer, just check if we have enough AOE targets
            ThunderStrategy.Force => NumAOETargets >= AOEBreakpoint,
            // only use for utility, ignore timer
            ThunderStrategy.InstantOnly => wantInstant,
            _ => false
        };

        if (canUse)
            PushGCD(AID.Thunder2, BestAOEThunderTarget);
    }

    private void Choose(AID st, AID aoe, Enemy? primaryTarget, int additionalPrio = 0)
    {
        if (NumAOETargets >= AOEBreakpoint && Unlocked(aoe))
            PushGCD(aoe, BestAOETarget, additionalPrio + 1);
        else
            PushGCD(st, primaryTarget, additionalPrio + 1);
    }

    private void TryInstantCast(StrategyValues strategy, Enemy? primaryTarget, bool useFirestarter = true, bool useThunderhead = true, bool usePolyglot = true)
    {
        if (usePolyglot && Polyglot > 0)
            Choose(AID.Xenoglossy, AID.Foul, primaryTarget);

        if (useFirestarter && Firestarter)
            PushGCD(AID.Fire3, primaryTarget);

        if (useThunderhead)
        {
            T2(strategy, true);
            T1(strategy, true);
        }
    }

    private void TryInstantOrTranspose(StrategyValues strategy, Enemy? primaryTarget, bool useThunderhead = true)
    {
        if (useThunderhead)
        {
            T2(strategy, true);
            T1(strategy, true);
        }

        if (Fire > 0 && MP < 1600)
            PushGCD(AID.Manafont, Player);

        if (Element != 0)
            PushGCD(AID.Transpose, Player);
    }

    private bool ShouldTriplecast(StrategyValues strategy) => CanWeave(MaxChargesIn(AID.Triplecast), 0.6f); // add strategy track, triplecast is no longer a gain during burst

    private bool ShouldUseLeylines(StrategyValues strategy, int extraGCDs = 0)
        => CanWeave(MaxChargesIn(AID.LeyLines), 0.6f, extraGCDs)
        && strategy.Option(SharedTrack.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;

    private bool ShouldTranspose(StrategyValues strategy)
    {
        if (!Unlocked(AID.Fire3))
            return Fire > 0 && MP < 1600 || Ice > 0 && MP > 9000;

        if (NumAOETargets >= AOEBreakpoint)
            return Fire > 0 && MP < 800 || Ice > 0 && Hearts > 0 && MP >= 2400;
        else
            return Firestarter && Ice > 0 && Hearts == MaxHearts;
    }
}
