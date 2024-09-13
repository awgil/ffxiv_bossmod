using BossMod.BRD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class BRD(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan BRD", "Bard", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.ARC, Class.BRD), 100);

        def.DefineShared().AddAssociatedActions(AID.RagingStrikes, AID.MagesBallad, AID.ArmysPaeon, AID.WanderersMinuet, AID.BattleVoice, AID.ApexArrow);

        return def;
    }

    public enum Song : byte
    {
        None,
        MagesBallad,
        ArmysPaeon,
        WanderersMinuet
    }

    [Flags]
    public enum CodaSongs : byte
    {
        None = 0,
        MagesBallad = 1,
        ArmysPaeon = 2,
        WanderersMinuet = 4
    }

    public float SongTimer;
    public byte Repertoire;
    public byte Soul;
    public Song CurrentSong;
    public Song PreviousSong;
    public CodaSongs Coda;

    public float HawksEye;
    public float Barrage;
    public float RagingStrikes;
    public float BlastArrow;
    public float BattleVoice;
    public float ResonantArrow;
    public float RadiantEncore;

    public (float Min, float Wind, float Poison) TargetDotLeft;

    public float? NextProc => SongTimer is > 3 and < 45 ? SongTimer - SongTimer % 3f : null;

    public int NumCircleTargets; // 25/5y circle - shadowbite and a bunch of other stuff
    public int NumConeTargets; // 12y/90(?)deg cone - regular aoe gcds
    public int NumLineTargets; // 25y/4y rect - apex arrow and stuff

    private Actor? BestCircleTarget;
    private Actor? BestConeTarget;
    private Actor? BestLineTarget;
    private Actor? BestDotTarget;

    public int Codas => (Coda.HasFlag(CodaSongs.MagesBallad) ? 1 : 0) + (Coda.HasFlag(CodaSongs.ArmysPaeon) ? 1 : 0) + (Coda.HasFlag(CodaSongs.WanderersMinuet) ? 1 : 0);

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<BardGauge>();

        SongTimer = gauge.SongTimer * 0.001f;
        Repertoire = gauge.Repertoire;
        Soul = gauge.SoulVoice;

        CurrentSong = (Song)((byte)gauge.SongFlags & 3);
        PreviousSong = (Song)(((byte)gauge.SongFlags >> 2) & 3);
        Coda = (CodaSongs)((byte)gauge.SongFlags >> 4);

        HawksEye = StatusLeft(SID.HawksEye);
        Barrage = StatusLeft(SID.Barrage);
        RagingStrikes = StatusLeft(SID.RagingStrikes);
        BlastArrow = StatusLeft(SID.BlastArrowReady);
        BattleVoice = StatusLeft(SID.BattleVoice);
        ResonantArrow = StatusLeft(SID.ResonantArrowReady);
        RadiantEncore = StatusLeft(SID.RadiantEncoreReady);

        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, DotDuration, 2);

        (BestCircleTarget, NumCircleTargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestConeTarget, NumConeTargets) = SelectTarget(strategy, primaryTarget, 12,
            (primary, other) => Hints.TargetInAOECone(other, Player.Position, 12, Player.DirectionTo(primary), 45.Degrees()));
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget);

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < EffectApplicationDelay(AID.Stormbite))
                PushGCD(AID.Stormbite, primaryTarget);

            return;
        }

        var ijDelay = EffectApplicationDelay(AID.IronJaws);

        if (CanFitGCD(TargetDotLeft.Min - ijDelay) && !CanFitGCD(TargetDotLeft.Min - ijDelay, 1))
            PushGCD(AID.IronJaws, BestDotTarget);

        if (!CanFitGCD(TargetDotLeft.Wind, 1))
            PushGCD(AID.Windbite, BestDotTarget);

        if (!CanFitGCD(TargetDotLeft.Poison, 1))
            PushGCD(AID.VenomousBite, BestDotTarget);

        if (BlastArrow > GCD)
            PushGCD(AID.BlastArrow, BestLineTarget);

        if (ShouldApexArrow(strategy))
            PushGCD(AID.ApexArrow, BestLineTarget);

        if (RadiantEncore > GCD)
            PushGCD(AID.RadiantEncore, BestCircleTarget);

        if (HawksEye > GCD || Barrage > GCD)
        {
            var aoeGain = Barrage > GCD ? 3 : 2;
            if (NumCircleTargets >= aoeGain)
                PushGCD(AID.WideVolley, BestCircleTarget);

            PushGCD(AID.StraightShot, primaryTarget);
        }

        if (ResonantArrow > GCD)
            PushGCD(AID.ResonantArrow, BestCircleTarget);

        if (NumConeTargets > 1)
            PushGCD(AID.QuickNock, BestConeTarget);

        PushGCD(AID.HeavyShot, primaryTarget);
    }

    private (float Min, float Wind, float Poison) DotDuration(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return (float.MaxValue, float.MaxValue, float.MaxValue);

        var wind = MathF.Max(StatusDetails(primaryTarget, SID.Windbite, Player.InstanceID, 45).Left, StatusDetails(primaryTarget, SID.Stormbite, Player.InstanceID, 45).Left);
        var poison = MathF.Max(StatusDetails(primaryTarget, SID.VenomousBite, Player.InstanceID, 45).Left, StatusDetails(primaryTarget, SID.CausticBite, Player.InstanceID, 45).Left);

        return (MathF.Min(wind, poison), wind, poison);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (SongTimer < 12)
            PushOGCD(AID.WanderersMinuet, Player);

        if (CurrentSong == Song.WanderersMinuet && (Repertoire == 3 || NextProc == null && Repertoire > 0) && NumCircleTargets > 0)
            PushOGCD(AID.PitchPerfect, BestCircleTarget);

        if (SongTimer < 3)
            PushOGCD(AID.MagesBallad, Player);

        if (SongTimer < 3)
            PushOGCD(AID.ArmysPaeon, Player);

        PushOGCD(AID.EmpyrealArrow, primaryTarget);

        if (strategy.BuffsOk())
        {
            PushOGCD(AID.BattleVoice, Player, delay: GCD - 0.8f);

            var canRadiant = BattleVoice > 0 || CombatTimer > 30 && CanWeave(AID.BattleVoice);

            if (canRadiant && Codas > 0)
                PushOGCD(AID.RadiantFinale, Player);

            if (!CanWeave(AID.BattleVoice))
                PushOGCD(AID.RagingStrikes, Player);

            if (!CanWeave(AID.RagingStrikes))
                PushOGCD(AID.Barrage, Player);
        }

        // if raging strikes is active, use all charges
        if (RagingStrikes > 0
            // if max charges within a GCD-ish, use one
            || CanWeave(MaxChargesIn(AID.Bloodletter), 0.6f, 1)
            // if mage's ballad, we might get a cd reset at any moment
            || CurrentSong == Song.MagesBallad && MaxChargesIn(AID.Bloodletter) < 15)
        {
            if (NumCircleTargets > 1)
                PushOGCD(AID.RainOfDeath, BestCircleTarget);

            PushOGCD(AID.Bloodletter, primaryTarget);
        }

        if (!CanWeave(AID.RagingStrikes))
        {
            PushOGCD(AID.Sidewinder, primaryTarget);
            PushOGCD(AID.EmpyrealArrow, primaryTarget);
        }
    }

    private bool ShouldApexArrow(StrategyValues strategy)
    {
        // can't use
        if (Soul < 80)
            return false;

        // buff alignment for 2min
        if (!strategy.BuffsOk())
            return false;

        if (CD(AID.RagingStrikes) > 55)
            return CD(AID.RagingStrikes) < 60 || Soul == 100;

        // use in 2min
        return RagingStrikes > GCD;
    }

    private float EffectApplicationDelay(AID aid) => aid switch
    {
        AID.Sidewinder => 0.55f,
        AID.IronJaws => 0.6f,
        AID.Troubadour => 0.62f,
        AID.WanderersMinuet => 0.63f,
        AID.MagesBallad => 0.63f,
        AID.ArmysPaeon => 0.63f,
        AID.WardensPaean => 0.67f,
        AID.NaturesMinne => 0.7f,
        AID.PitchPerfect => 0.8f,
        AID.ApexArrow => 1.05f,
        AID.EmpyrealArrow => 1.1f,
        AID.CausticBite => 1.3f,
        AID.Stormbite => 1.3f,
        AID.Shadowbite => 1.43f,
        AID.BurstShot => 1.45f,
        AID.RefulgentArrow => 1.45f,
        AID.Bloodletter => 1.6f,
        AID.QuickNock => 1.65f,
        AID.RainOfDeath => 1.65f,
        _ => 0
    };
}
