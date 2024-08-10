using BossMod.DRK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class DRK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan DRK", "Dark Knight", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.DRK), 100);

        def.DefineShared().AddAssociatedActions(AID.LivingShadow);

        return def;
    }

    public DarkKnightGauge Gauge;
    public int BloodWeapon;
    public int Delirium;
    public float SaltedEarth;

    public float Darkside => Gauge.DarksideTimer * 0.001f;
    public int Blood => Gauge.Blood;
    public bool DarkArts => Gauge.DarkArtsState == 1;

    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumLineTargets;

    private Actor? BestRangedAOETarget;
    private Actor? BestLineTarget;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        Gauge = GetGauge<DarkKnightGauge>();

        BloodWeapon = StatusStacks(SID.BloodWeapon);
        Delirium = StatusStacks(SID.Delirium);
        SaltedEarth = StatusLeft(SID.SaltedEarth);

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2));

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            return;
        }

        if (Darkside > GCD && ShouldBlood(strategy))
        {
            if (NumAOETargets > 2)
                PushGCD(AID.Quietus, Player);

            PushGCD(AID.Bloodspiller, primaryTarget);
        }

        if (NumAOETargets > 2)
        {
            if (ComboLastMove == AID.Unleash)
                PushGCD(AID.StalwartSoul, Player);

            PushGCD(AID.Unleash, Player);
        }

        if (ComboLastMove == AID.SyphonStrike)
            PushGCD(AID.Souleater, primaryTarget);

        if (ComboLastMove == AID.HardSlash)
            PushGCD(AID.SyphonStrike, primaryTarget);

        PushGCD(AID.HardSlash, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        var mpNeeded = Unlocked(AID.TheBlackestNight) ? 6000 : 3000;

        if (MP >= mpNeeded || DarkArts)
        {
            if (NumLineTargets > 2)
                PushOGCD(AID.FloodOfDarkness, BestLineTarget);

            PushOGCD(AID.EdgeOfDarkness, primaryTarget);
        }

        PushOGCD(AID.LivingShadow, Player);

        if (Blood > 0 || !Unlocked(TraitID.Blackblood))
            PushOGCD(AID.Delirium, Player);

        if (!CanWeave(AID.Delirium))
        {
            if (NumAOETargets > 0)
                PushOGCD(AID.SaltedEarth, Player);

            if (NumLineTargets > 0 && (RaidBuffsLeft > 0 || RaidBuffsIn > 9000))
                PushOGCD(AID.Shadowbringer, BestLineTarget);

            if (NumRangedAOETargets > 2)
                PushOGCD(AID.AbyssalDrain, BestRangedAOETarget);

            PushOGCD(AID.CarveAndSpit, primaryTarget);

            if (SaltedEarth > 0)
                PushOGCD(AID.SaltAndDarkness, Player);
        }
    }

    private bool ShouldBlood(StrategyValues strategy)
    {
        if (Darkside < GCD)
            return false;

        if (Delirium > 0)
            return true;

        if (Blood < 50)
            return false;

        if (RaidBuffsLeft > GCD)
            return true;

        var impendingBlood = ComboLastMove == (NumAOETargets > 2 ? AID.Unleash : AID.Souleater);

        return Blood + (impendingBlood ? 20 : 0) > 100;
    }
}
