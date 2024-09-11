using BossMod.PLD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class PLD(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan PLD", "Paladin", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PLD, Class.GLA), 100);

        def.DefineShared().AddAssociatedActions(AID.FightOrFlight);

        return def;
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

    private Actor? BestRangedTarget;

    protected override float GetCastTime(AID aid) => aid switch
    {
        AID.HolyCircle or AID.HolySpirit => DivineMight > GCD || Requiescat.Stacks > 0 ? 0 : base.GetCastTime(aid),
        _ => 0
    };

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
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
        ConfiteorCombo = gauge.ConfiteorComboStep switch
        {
            0 => StatusLeft(SID.ConfiteorReady) > GCD ? AID.Confiteor : AID.None,
            1 => AID.BladeOfFaith,
            2 => AID.BladeOfTruth,
            3 => AID.BladeOfValor,
            _ => AID.None
        };

        BestRangedTarget = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget).Best;

        NumAOETargets = NumMeleeAOETargets(strategy);

        CalcNextBestOGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.HolySpirit))
                PushGCD(AID.HolySpirit, BestRangedTarget);

            return;
        }

        if (ConfiteorCombo != AID.None && MP >= 1000)
            PushGCD(ConfiteorCombo, BestRangedTarget);

        // use goring blade even in AOE
        if (GoringBladeReady > GCD)
            PushGCD(AID.GoringBlade, primaryTarget, priority: 50);

        if (NumAOETargets >= 3 && Unlocked(AID.TotalEclipse))
        {
            if ((Requiescat.Left > GCD || DivineMight > GCD && FightOrFlight > GCD) && MP >= 1000)
                PushGCD(AID.HolyCircle, Player);

            if (ComboLastMove == AID.TotalEclipse)
            {
                if (DivineMight > GCD && MP >= 1000)
                    PushGCD(AID.HolyCircle, Player);

                PushGCD(AID.Prominence, Player);
            }

            PushGCD(AID.TotalEclipse, Player);
        }
        else
        {
            // fallback - cast holy spirit if we don't have a melee
            if (DivineMight > GCD && MP >= 1000)
                PushGCD(AID.HolySpirit, primaryTarget, -50);

            if (Requiescat.Left > GCD || DivineMight > GCD && FightOrFlight > GCD)
                PushGCD(AID.HolySpirit, primaryTarget);

            if (AtonementReady > GCD && FightOrFlight > GCD)
                PushGCD(AID.Atonement, primaryTarget);

            if (SepulchreReady > GCD)
                PushGCD(AID.Sepulchre, primaryTarget);

            if (SupplicationReady > GCD)
                PushGCD(AID.Supplication, primaryTarget);

            if (ComboLastMove == AID.RiotBlade)
            {
                if (DivineMight > GCD && MP >= 1000)
                    PushGCD(AID.HolySpirit, primaryTarget);

                if (AtonementReady > GCD)
                    PushGCD(AID.Atonement, primaryTarget);

                PushGCD(AID.RageOfHalone, primaryTarget);
            }

            if (ComboLastMove == AID.FastBlade)
                PushGCD(AID.RiotBlade, primaryTarget);

            PushGCD(AID.FastBlade, primaryTarget);
        }
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (AtonementReady > 0 || Requiescat.Left > 0 || DivineMight > 0)
            PushOGCD(AID.FightOrFlight, Player);

        if (BladeOfHonorReady > 0)
            PushOGCD(AID.BladeOfHonor, BestRangedTarget);

        if (CD(AID.FightOrFlight) > 40)
        {
            if (Unlocked(AID.Imperator))
                PushOGCD(AID.Imperator, BestRangedTarget);
            else
                PushOGCD(AID.Requiescat, primaryTarget);
        }

        if (FightOrFlight > 0 || CD(AID.FightOrFlight) > 15)
        {
            PushOGCD(AID.SpiritsWithin, primaryTarget);

            if (NumAOETargets > 0)
                PushOGCD(AID.CircleOfScorn, Player);
        }

        if (FightOrFlight > 0)
            PushOGCD(AID.Intervene, primaryTarget);
    }
}
