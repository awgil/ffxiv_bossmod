using BossMod.Data;
using BossMod.DRG;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class DRG(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player, PotionType.Strength)
{
    public enum Track { Dive = SharedTrack.Count, Iainuki, Zeninage }

    public enum DiveStrategy
    {
        Allow,
        NoMove,
        NoLock
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan DRG", "Dragoon", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.DRG, Class.LNC), 100);

        def.DefineShared().AddAssociatedActions(AID.BattleLitany, AID.LanceCharge);

        def.Define(Track.Dive).As<DiveStrategy>("Dive")
            .AddOption(DiveStrategy.Allow, "Allow", "Use dives according to standard rotation")
            .AddOption(DiveStrategy.NoMove, "NoMove", "Disallow dive actions that move you to the target")
            .AddOption(DiveStrategy.NoLock, "NoLock", "Disallow dive actions that prevent you from moving (all except Mirage Dive)");

        def.AbilityTrack(Track.Iainuki, "Iainuki", "PSAM: Use Iainuki during burst");
        def.AbilityTrack(Track.Zeninage, "Zeninage", "PSAM: Use Zeninage during burst (costs 10,000 gil)");

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
    public float EnhancedTalon;

    public float TargetDotLeft;

    public int NumAOETargets; // standard combo (10x4 rect)
    public int NumLongAOETargets; // GSK, nastrond (15x4 rect)
    public int NumDiveTargets; // dragonfire, stardiver, etc

    private Enemy? BestAOETarget;
    private Enemy? BestLongAOETarget;
    private Enemy? BestDiveTarget;

    protected override float GetCastTime(AID aid)
    {
        if ((uint)aid == (uint)PhantomID.Iainuki)
            return 1.3f;
        return base.GetCastTime(aid);
    }

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
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
        EnhancedTalon = StatusLeft(SID.EnhancedPiercingTalon);
        TargetDotLeft = MathF.Max(
            StatusDetails(primaryTarget, SID.ChaosThrust, Player.InstanceID).Left,
            StatusDetails(primaryTarget, SID.ChaoticSpring, Player.InstanceID).Left
        );

        (BestAOETarget, NumAOETargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2));
        (BestLongAOETarget, NumLongAOETargets) = SelectTarget(strategy, primaryTarget, 15, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2));
        (BestDiveTarget, NumDiveTargets) = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget);

        var pos = GetPositional(strategy, primaryTarget);
        UpdatePositionals(primaryTarget, ref pos);

        if (primaryTarget == null)
            return;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 0.7f)
                PushGCD(AID.WingedGlide, primaryTarget);

            return;
        }

        GoalZoneCombined(strategy, 3, Hints.GoalAOERect(primaryTarget.Actor, 10, 2), AID.DoomSpike, minAoe: 3, maximumActionRange: 20);

        if (LotD > GCD && PowerSurge > GCD && LanceCharge > GCD && strategy.Enabled(Track.Zeninage) && PhantomReadyIn(PhantomID.Zeninage) <= GCD)
            PushGCD((AID)(uint)PhantomID.Zeninage, primaryTarget, priority: 100);

        if (strategy.Enabled(Track.Iainuki) && PhantomReadyIn(PhantomID.Iainuki) <= GCD && PhantomReadyIn(PhantomID.Zeninage) > GCD)
            PushGCD((AID)(uint)PhantomID.Iainuki, primaryTarget, priority: 90);

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
                    PushGCD(BestActionUnlocked(AID.ChaoticSpring, AID.ChaosThrust), primaryTarget);
                    break;
                case AID.VorpalThrust:
                case AID.LanceBarrage:
                    PushGCD(BestActionUnlocked(AID.HeavensThrust, AID.FullThrust), primaryTarget);
                    break;
                case AID.TrueThrust:
                case AID.RaidenThrust:
                    if (PowerSurge < 10)
                        PushGCD(BestActionUnlocked(AID.SpiralBlow, AID.Disembowel), primaryTarget);
                    PushGCD(BestActionUnlocked(AID.LanceBarrage, AID.VorpalThrust), primaryTarget);
                    break;
            }
        }

        PushGCD(DraconianFire > GCD ? AID.RaidenThrust : AID.TrueThrust, primaryTarget);
        if (EnhancedTalon > GCD)
            PushGCD(AID.PiercingTalon, primaryTarget);

        OGCD(strategy, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat || PowerSurge == 0)
            return;

        var moveOk = MoveOk(strategy);
        var posOk = PosLockOk(strategy);
        var bestSingleTarget = primaryTarget.Priority >= 0 ? primaryTarget : null;

        if (NextPositionalImminent && !NextPositionalCorrect)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.TrueNorth), Player, ActionQueue.Priority.Low - 20, delay: GCD - 0.8f);

        if (strategy.BuffsOk() && PowerSurge > GCD)
        {
            PushOGCD(AID.LanceCharge, Player);
            PushOGCD(AID.BattleLitany, Player);
        }

        // ok to use WT outside of buffs, otherwise we might overcap and waste one
        if (ShouldWT(strategy))
            PushOGCD(AID.WyrmwindThrust, BestLongAOETarget);

        // delay all damaging ogcds until we've used lance charge
        // first one (jump) unlocks at level 30, same as lance charge, so we don't need extra checks
        // TODO check if this is actually a good idea
        if (CanWeave(AID.LanceCharge))
            return;

        if (NastrondReady == 0 && strategy.BuffsOk())
            PushOGCD(AID.Geirskogul, BestLongAOETarget);

        if (DiveReady == 0 && posOk)
            PushOGCD(AID.Jump, bestSingleTarget);

        if (LanceCharge > GCD && ShouldLifeSurge())
            PushOGCD(AID.LifeSurge, Player);

        if (moveOk && strategy.BuffsOk())
            PushOGCD(AID.DragonfireDive, BestDiveTarget);

        if (NastrondReady > 0)
            PushOGCD(AID.Nastrond, BestLongAOETarget);

        if (LotD > AnimLock && moveOk)
        {
            // stardiver: 1.5 + delay
            // regular GCD: 0.6 + delay
            // some conditions like DD haste (and maybe bozja?) can reduce GCD to 2.1s or lower, making stardiver weave impossible
            if (GCDLength > 2.1f + 2 * AnimationLockDelay)
                PushOGCD(AID.Stardiver, BestDiveTarget);
            else if (GCD > 0)
                PushGCD(AID.Stardiver, BestDiveTarget, 3);
        }

        if (StarcrossReady > 0)
            PushOGCD(AID.Starcross, BestDiveTarget);

        if (DragonsFlight > 0)
            PushOGCD(AID.RiseOfTheDragon, BestDiveTarget);

        if (DiveReady > 0)
            PushOGCD(AID.MirageDive, bestSingleTarget);
    }

    private bool ShouldLifeSurge()
    {
        if (LifeSurge > 0)
            return false;

        return NextGCD switch
        {
            // highest potency at max level (full thrust is still highest potency before it gets upgraded)
            AID.CoerthanTorment or AID.Drakesbane or AID.HeavensThrust or AID.FullThrust => true,

            // highest potency before Full Thrust is unlocked at 26
            AID.VorpalThrust => !Unlocked(AID.FullThrust),

            // fallbacks for AOE rotation
            AID.SonicThrust => !Unlocked(AID.CoerthanTorment),
            AID.DoomSpike => !Unlocked(AID.SonicThrust),
            _ => false,
        };
    }

    private bool ShouldWT(StrategyValues strategy)
        => Focus == 2 && (LotD > AnimLock || NextGCD is AID.RaidenThrust or AID.DraconianFury);

    private bool MoveOk(StrategyValues strategy) => strategy.Option(Track.Dive).As<DiveStrategy>() == DiveStrategy.Allow;
    private bool PosLockOk(StrategyValues strategy) => strategy.Option(Track.Dive).As<DiveStrategy>() != DiveStrategy.NoLock;

    private (Positional, bool) GetPositional(StrategyValues strategy, Enemy? primaryTarget)
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
