using BossMod.DRG;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class DRG(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Dive = SharedTrack.Count }

    public enum DiveStrategy
    {
        Allow,
        NoMove,
        NoLock
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan DRG", "Dragoon", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.DRG, Class.LNC), 100);

        def.DefineShared().AddAssociatedActions(AID.BattleLitany, AID.LanceCharge);

        def.Define(Track.Dive).As<DiveStrategy>("Dive")
            .AddOption(DiveStrategy.Allow, "Allow", "Use dives according to standard rotation")
            .AddOption(DiveStrategy.NoMove, "NoMove", "Disallow dive actions that move you to the target")
            .AddOption(DiveStrategy.NoLock, "NoLock", "Disallow dive actions that prevent you from moving (all except Mirage Dive)");

        return def;
    }

    public int Eyes;
    public int Focus;
    public float LotD;
    public float PowerSurge;
    public float LanceCharge;
    public float DiveReady;
    public float NastrondReady;
    public float LifeSurge;
    public float DraconianFire;
    public float DragonsFlight;
    public float StarcrossReady;

    public float TargetDotLeft;

    public int NumAOETargets; // standard combo (10x4 rect)
    public int NumLongAOETargets; // GSK, nastrond (15x4 rect)
    public int NumDiveTargets; // dragonfire, stardiver, etc

    private Actor? BestAOETarget;
    private Actor? BestLongAOETarget;
    private Actor? BestDiveTarget;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<DragoonGauge>();

        Eyes = gauge.EyeCount;
        Focus = gauge.FirstmindsFocusCount;
        LotD = gauge.LotdTimer * 0.001f;

        PowerSurge = StatusLeft(SID.PowerSurge);
        DiveReady = StatusLeft(SID.DiveReady);
        NastrondReady = StatusLeft(SID.NastrondReady);
        LifeSurge = StatusLeft(SID.LifeSurge);
        LanceCharge = StatusLeft(SID.LanceCharge);
        DraconianFire = StatusLeft(SID.DraconianFire);
        DragonsFlight = StatusLeft(SID.DragonsFlight);
        StarcrossReady = StatusLeft(SID.StarcrossReady);
        TargetDotLeft = MathF.Max(
            StatusDetails(primaryTarget, SID.ChaosThrust, Player.InstanceID).Left,
            StatusDetails(primaryTarget, SID.ChaoticSpring, Player.InstanceID).Left
        );

        (BestAOETarget, NumAOETargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2));
        (BestLongAOETarget, NumLongAOETargets) = SelectTarget(strategy, primaryTarget, 15, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2));
        (BestDiveTarget, NumDiveTargets) = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget);

        UpdatePositionals(primaryTarget, GetPositional(strategy, primaryTarget), TrueNorthLeft > GCD);

        OGCD(strategy, primaryTarget);

        if (primaryTarget == null)
            return;

        if (NumAOETargets > 2)
        {
            switch (ComboLastMove)
            {
                case AID.SonicThrust:
                    PushGCD(AID.CoerthanTorment, BestAOETarget);
                    break;
                case AID.DoomSpike:
                case AID.DraconianFury:
                    PushGCD(AID.SonicThrust, BestAOETarget);
                    break;
            }

            // lol
            if (!Unlocked(AID.SonicThrust) && PowerSurge < GCD)
            {
                if (ComboLastMove == AID.TrueThrust)
                    PushGCD(AID.Disembowel, primaryTarget);

                PushGCD(AID.TrueThrust, primaryTarget);
            }

            PushGCD(DraconianFire > GCD ? AID.DraconianFury : AID.DoomSpike, BestAOETarget);
        }
        else
        {
            switch (ComboLastMove)
            {
                case AID.WheelingThrust:
                case AID.FangAndClaw:
                    PushGCD(AID.Drakesbane, primaryTarget);
                    break;
                case AID.ChaosThrust:
                case AID.ChaoticSpring:
                    PushGCD(AID.WheelingThrust, primaryTarget);
                    break;
                case AID.FullThrust:
                case AID.HeavensThrust:
                    PushGCD(AID.FangAndClaw, primaryTarget);
                    break;
                case AID.Disembowel:
                case AID.SpiralBlow:
                    PushGCD(AID.ChaosThrust, primaryTarget);
                    break;
                case AID.VorpalThrust:
                case AID.LanceBarrage:
                    PushGCD(AID.FullThrust, primaryTarget);
                    break;
                case AID.TrueThrust:
                case AID.RaidenThrust:
                    if (PowerSurge < 10)
                        PushGCD(AID.Disembowel, primaryTarget);
                    PushGCD(AID.VorpalThrust, primaryTarget);
                    break;
            }
        }

        PushGCD(DraconianFire > GCD ? AID.RaidenThrust : AID.TrueThrust, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat || PowerSurge == 0)
            return;

        var moveOk = MoveOk(strategy);
        var posOk = PosLockOk(strategy);

        if (NextPositionalImminent && !NextPositionalCorrect)
            PushOGCD(AID.TrueNorth, Player, priority: -20, delay: GCD - 0.8f);

        if (strategy.BuffsOk())
        {
            PushOGCD(AID.LanceCharge, Player);
            PushOGCD(AID.BattleLitany, Player);
        }

        // delay all damaging ogcds until we've used lance charge
        // first one (jump) unlocks at level 30, same as lance charge, so we don't need extra checks
        // TODO check if this is actually a good idea
        if (CD(AID.LanceCharge) == 0)
            return;

        if (LanceCharge > GCD && ShouldLifeSurge())
            PushOGCD(AID.LifeSurge, Player);

        if (StarcrossReady > 0)
            // TODO we *technically* should select a specific target for starcross because it's a 3y range 5y radius circle...
            // but it's always gonna get used immediately after stardiver and we'll be melee range...so fuck it
            PushOGCD(AID.Starcross, primaryTarget);

        if (LotD > 0 && moveOk)
            PushOGCD(AID.Stardiver, BestDiveTarget);

        if (NastrondReady == 0)
            PushOGCD(AID.Geirskogul, BestLongAOETarget);

        if (DiveReady == 0 && posOk)
            PushOGCD(AID.Jump, primaryTarget);

        if (moveOk)
            PushOGCD(AID.DragonfireDive, BestDiveTarget);

        if (NastrondReady > 0)
            PushOGCD(AID.Nastrond, BestLongAOETarget);

        if (DragonsFlight > 0)
            PushOGCD(AID.RiseOfTheDragon, BestDiveTarget);

        if (DiveReady > 0)
            PushOGCD(AID.MirageDive, primaryTarget);

        if (Focus == 2)
            PushOGCD(AID.WyrmwindThrust, BestLongAOETarget);
    }

    private bool ShouldLifeSurge()
    {
        if (LifeSurge > 0)
            return false;

        if (NumAOETargets > 2 && Unlocked(AID.DoomSpike))
        {
            // coerthan torment is always our strongest aoe GCD (draconian fury just gives eyeball)
            if (Unlocked(AID.CoerthanTorment))
                return ComboLastMove == AID.SonicThrust;

            if (Unlocked(AID.SonicThrust))
                return ComboLastMove == AID.DoomSpike;

            // doom spike is our only AOE skill at this level, always use
            return true;
        }
        else
        {
            // 440 potency, level 64
            if (Unlocked(AID.Drakesbane))
            {
                var ok = ComboLastMove is AID.WheelingThrust or AID.FangAndClaw;

                // also 440 potency, level 86
                if (Unlocked(AID.HeavensThrust))
                    ok |= ComboLastMove is AID.VorpalThrust or AID.LanceBarrage;

                return ok;
            }

            // 380 potency, level 26
            if (Unlocked(AID.FullThrust))
                return ComboLastMove is AID.VorpalThrust;

            // below level 26, strongest GCD is vorpal thrust
            return ComboLastMove is AID.TrueThrust;
        }
    }

    private bool MoveOk(StrategyValues strategy) => strategy.Option(Track.Dive).As<DiveStrategy>() == DiveStrategy.Allow;
    private bool PosLockOk(StrategyValues strategy) => strategy.Option(Track.Dive).As<DiveStrategy>() != DiveStrategy.NoLock;

    private (Positional, bool) GetPositional(StrategyValues strategy, Actor? primaryTarget)
    {
        // no positional
        if (NumAOETargets > 2 && Unlocked(AID.DoomSpike) || !Unlocked(AID.ChaosThrust) || primaryTarget == null)
            return (Positional.Any, false);

        if (!Unlocked(AID.FangAndClaw))
            return (Positional.Rear, ComboLastMove == AID.Disembowel);

        (Positional, bool) predictNext(int gcdsBeforeTrueThrust)
        {
            var buffsUp = CanFitGCD(TargetDotLeft, gcdsBeforeTrueThrust + 3) && CanFitGCD(PowerSurge, gcdsBeforeTrueThrust + 2);
            return (buffsUp ? Positional.Flank : Positional.Rear, false);
        }

        return ComboLastMove switch
        {
            AID.ChaosThrust => Unlocked(AID.WheelingThrust) ? (Positional.Rear, true) : predictNext(0),
            AID.ChaoticSpring => (Positional.Rear, true), // wheeling thrust is unlocked
            AID.Disembowel or AID.SpiralBlow => (Positional.Rear, true),
            AID.TrueThrust or AID.RaidenThrust => predictNext(-1),
            AID.VorpalThrust or AID.LanceBarrage => (Positional.Flank, false),
            AID.HeavensThrust or AID.FullThrust => (Positional.Flank, true),
            AID.WheelingThrust or AID.FangAndClaw => predictNext(Unlocked(AID.Drakesbane) ? 1 : 0),
            // last action is AOE, or nothing, or drakesbane - loop reset
            _ => predictNext(0)
        };
    }
}
