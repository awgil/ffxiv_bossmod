using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class BLM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan BLM", "Black Mage", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.BLM, Class.THM), 100);

        def.DefineShared().AddAssociatedActions(AID.LeyLines);

        return def;
    }

    public int Element; // -3 (ice) <=> 3 (fire), 0 for none
    public float ElementLeft;
    public float NextPolyglot; // max 30
    public int Hearts; // max 3
    public int Polyglot;
    public int AstralSoul; // max 6
    public bool Paradox;

    public float Triplecast;
    public float Thunderhead;
    public float Firestarter;
    public bool InLeyLines;

    public float TargetThunderLeft;

    public int Fire => Math.Max(0, Element);
    public int Ice => Math.Max(0, -Element);

    public int MaxPolyglot => Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : 1;
    public int MaxHearts => Unlocked(TraitID.UmbralHeart) ? 3 : 0;

    private Actor? BestAOETarget;
    private int NumAOETargets;

    protected override float GetCastTime(AID aid)
    {
        if (Triplecast > GCD)
            return 0;

        var aspect = ActionDefinitions.Instance.Spell(aid)!.Aspect;

        if (aid == AID.Fire3 && Firestarter > GCD
            || aid == AID.Foul && Unlocked(TraitID.EnhancedFoul)
            || aspect == ActionAspect.Thunder && Thunderhead > GCD)
            return 0;

        var castTime = base.GetCastTime(aid);
        if (castTime == 0)
            return 0;

        if (Element == -3 && aspect == ActionAspect.Fire || Element == 3 && aspect == ActionAspect.Ice)
            castTime *= 0.5f;

        return castTime;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 25);

        var gauge = GetGauge<BlackMageGauge>();

        Element = gauge.ElementStance;
        ElementLeft = gauge.ElementTimeRemaining * 0.001f;
        NextPolyglot = gauge.EnochianTimer * 0.001f;
        Hearts = gauge.UmbralHearts;
        Polyglot = gauge.PolyglotStacks;
        Paradox = gauge.ParadoxActive;
        AstralSoul = gauge.AstralSoulStacks;

        Triplecast = StatusLeft(SID.Triplecast);
        Thunderhead = StatusLeft(SID.Thunderhead);
        Firestarter = StatusLeft(SID.Firestarter);
        InLeyLines = Player.FindStatus(SID.CircleOfPower) != null;

        TargetThunderLeft = MaxAll(
            StatusDetails(primaryTarget, SID.Thunder, Player.InstanceID, 24).Left,
            StatusDetails(primaryTarget, SID.ThunderII, Player.InstanceID, 18).Left,
            StatusDetails(primaryTarget, SID.ThunderIII, Player.InstanceID, 27).Left,
            StatusDetails(primaryTarget, SID.ThunderIV, Player.InstanceID, 21).Left,
            StatusDetails(primaryTarget, SID.HighThunder, Player.InstanceID, 30).Left,
            StatusDetails(primaryTarget, SID.HighThunderII, Player.InstanceID, 24).Left
        );

        (BestAOETarget, NumAOETargets) = SelectTargetByHP(strategy, primaryTarget, 25, IsSplashTarget);

        if (World.Client.CountdownRemaining > 0)
        {
            if (World.Client.CountdownRemaining < GetCastTime(AID.Fire3))
                PushGCD(AID.Fire3, primaryTarget);

            return;
        }

        if (primaryTarget == null)
        {
            if (Fire > 0 && Unlocked(AID.Transpose) && Unlocked(AID.UmbralSoul) && CD(AID.Transpose) == 0)
                PushOGCD(AID.Transpose, Player);

            if (Unlocked(AID.UmbralSoul) && Ice > 0 && (Ice < 3 || Hearts < MaxHearts || ElementLeft < 3))
                PushGCD(AID.UmbralSoul, Player);

            return;
        }

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
        else if (!Player.InCombat && MP >= 9600)
            FirePhase(strategy, primaryTarget);
        else
            IcePhase(strategy, primaryTarget);

        if (Polyglot > 0)
        {
            if (NumAOETargets > 2)
                PushGCD(AID.Foul, BestAOETarget);

            PushGCD(AID.Xenoglossy, primaryTarget);
        }
    }

    private void FirePhase(StrategyValues strategy, Actor? primaryTarget)
    {
        if (NumAOETargets > 2 && Unlocked(AID.Flare))
            FirePhaseAOE(strategy);
        else
            FirePhaseST(strategy, primaryTarget);
    }

    private void FirePhaseST(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Thunderhead > GCD && TargetThunderLeft < 5 && ElementLeft > GCDLength + AnimationLockDelay)
            PushGCD(AID.Thunder1, primaryTarget);

        if (Fire < 3 && Unlocked(AID.Fire3))
            PushGCD(AID.Fire3, primaryTarget);

        if (AstralSoul == 6 && ElementLeft > GetCastTime(AID.FlareStar))
            PushGCD(AID.FlareStar, BestAOETarget);

        if (Unlocked(AID.Fire4))
        {
            var minF4Time = MathF.Max(GCDLength, GetCastTime(AID.Fire4) + 0.1f);

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
                }
                // breakpoint at which despair is more damage than f1 despair, because it speeds up next fire phase
                else if (MP <= 2400 && ElementLeft > GetSlidecastEnd(AID.Despair))
                    PushGCD(AID.Despair, primaryTarget);
                // AF3 will last *at least* another two F4s, ok to cast
                // TODO in the case where we have one triplecast stack left, this will end up checking (timer > 2.5 + 2.5) instead of (timer > 2.5 + 3.1) - i think it's ok?
                else if (ElementLeft > NextCastStart + minF4Time * 2)
                {
                    if (Polyglot == MaxPolyglot && NextPolyglot < 5)
                        PushGCD(AID.Xenoglossy, primaryTarget);

                    PushGCD(AID.Fire4, primaryTarget);
                }
                // AF3 will last long enough for us to refresh using Paradox
                // TODO the extra 0.1 is a guesstimate for how long it actually takes instant fire spells to refresh the timer, should look at replays to be sure
                else if (ElementLeft > NextCastStart + minF4Time + 0.1f && Paradox)
                    PushGCD(AID.Fire4, primaryTarget);
                else if (Paradox)
                    PushGCD(AID.Paradox, primaryTarget);
                else if (Firestarter > GCD)
                    PushGCD(AID.Fire3, primaryTarget);
                else
                    PushGCD(AID.Blizzard3, primaryTarget);
            }
        }
        else if (Unlocked(AID.Fire3))
        {
            // TODO this will skip free F3 to switch to UI (so we can transpose F3 for next fire phase)
            // 1. is this a DPS gain 2. does anyone actually care about level 50
            if (MP < 1600)
                PushGCD(AID.Blizzard3, primaryTarget);

            PushGCD(Firestarter > GCD ? AID.Fire3 : AID.Fire1, primaryTarget);
        }
        else if (MP >= 1600)
            PushGCD(AID.Fire1, primaryTarget);
        else
            PushGCD(AID.Blizzard1, primaryTarget);
    }

    private void FirePhaseAOE(StrategyValues strategy)
    {
        if (Thunderhead > GCD && TargetThunderLeft < 5 && ElementLeft > GCDLength + AnimationLockDelay)
            PushGCD(AID.Thunder2, BestAOETarget);

        if (AstralSoul == 6)
            PushGCD(AID.FlareStar, BestAOETarget);

        if (Unlocked(AID.Flare) && MP >= 800 && Fire > 0)
        {
            if (ElementLeft > GetSlidecastEnd(AID.Flare))
                PushGCD(AID.Flare, BestAOETarget);
            else if (Paradox && MP >= 1600)
                PushGCD(AID.Paradox, BestAOETarget);
        }

        TryInstantCast(strategy, BestAOETarget);

        PushGCD(AID.Fire2, BestAOETarget);
    }

    private void IcePhase(StrategyValues strategy, Actor? primaryTarget)
    {
        if (NumAOETargets > 2 && Unlocked(AID.Blizzard2))
            IcePhaseAOE(strategy, primaryTarget);
        else
            IcePhaseST(strategy, primaryTarget);
    }

    private void IcePhaseAOE(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Ice == 0)
            PushGCD(AID.Fire2, BestAOETarget);
        else if (Hearts < MaxHearts && Unlocked(AID.Freeze))
            PushGCD(AID.Freeze, BestAOETarget);

        TryInstantCast(strategy, primaryTarget);
    }

    private void IcePhaseST(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Ice < 3 && Unlocked(AID.Blizzard3))
            PushGCD(AID.Blizzard3, primaryTarget);

        if (MP == 10000 && Unlocked(AID.Fire3))
        {
            var nextGCD = GCD + GCDLength;

            if (ElementLeft > nextGCD && Firestarter > nextGCD && CanWeave(AID.Transpose, 1) && SwiftcastLeft == 0 && Triplecast == 0 && MP == 10000)
                TryInstantCast(strategy, primaryTarget, useFirestarter: false);

            PushGCD(AID.Fire3, primaryTarget);
        }
        else if (Unlocked(AID.Blizzard4))
            PushGCD(AID.Blizzard4, primaryTarget);
        else if (Unlocked(AID.Freeze) && NumAOETargets > 0)
            PushGCD(AID.Freeze, primaryTarget);
        else if (MP == 10000)
            PushGCD(AID.Fire1, primaryTarget);
        else
            PushGCD(AID.Blizzard1, primaryTarget);

    }

    private void Choose(AID st, AID aoe, Actor? primaryTarget, int additionalPrio = 0)
    {
        if (NumAOETargets > 2 && Unlocked(aoe))
            PushGCD(aoe, BestAOETarget, additionalPrio);
        else
            PushGCD(st, primaryTarget, additionalPrio);
    }

    private void TryInstantCast(StrategyValues strategy, Actor? primaryTarget, bool useFirestarter = true, bool useThunderhead = true, bool usePolyglot = true)
    {
        var tp = useThunderhead && Thunderhead > GCD;

        if (tp && TargetThunderLeft < 5)
            Choose(AID.Thunder1, AID.Thunder2, primaryTarget);

        if (usePolyglot && Polyglot > 0)
            Choose(AID.Xenoglossy, AID.Foul, primaryTarget);

        if (tp)
            Choose(AID.Thunder1, AID.Thunder2, primaryTarget, TargetThunderLeft < 5 ? 20 : 0);

        if (useFirestarter && Firestarter > GCD)
            PushGCD(AID.Fire3, primaryTarget);
    }

    private bool ShouldTriplecast(StrategyValues strategy) => Triplecast == 0 && (ShouldUseLeylines(strategy) || InLeyLines);

    private bool ShouldUseLeylines(StrategyValues strategy, int extraGCDs = 0)
        => CanWeave(AID.LeyLines, extraGCDs)
        && ForceMovementIn >= 30
        && strategy.Option(SharedTrack.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;

    private bool ShouldTranspose(StrategyValues strategy)
    {
        if (NumAOETargets > 2)
            return Fire > 0 && MP < 800 || Ice > 0 && Hearts > 0 && MP >= 2400;
        else
            return Firestarter > GCD && Ice > 0 && Hearts == MaxHearts;
    }

    private float MaxAll(float first, params float[] rest)
    {
        foreach (var f in rest)
            first = MathF.Max(f, first);

        return first;
    }
}
