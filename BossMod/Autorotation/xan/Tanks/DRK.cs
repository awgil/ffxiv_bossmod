using BossMod.DRK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class DRK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("DRK", "Dark Knight", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.DRK), 100);

        def.DefineShared().AddAssociatedActions(AID.LivingShadow);

        return def;
    }

    public DarkKnightGauge Gauge;
    public int BloodWeapon;
    public int Delirium;
    public float SaltedEarth;

    public float Darkside => Gauge.DarksideTimer * 0.001f;
    public int Blood => Gauge.Blood;

    private void CalcNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (Delirium > 0)
            PushGCD(AID.Bloodspiller, primaryTarget);

        if (ComboLastMove == AID.SyphonStrike)
            PushGCD(AID.Souleater, primaryTarget);

        if (ComboLastMove == AID.HardSlash)
            PushGCD(AID.SyphonStrike, primaryTarget);

        PushGCD(AID.HardSlash, primaryTarget);
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        if (Darkside < 20)
            PushOGCD(AID.EdgeOfDarkness, primaryTarget);

        PushOGCD(AID.LivingShadow, Player);

        if (Blood > 0)
            PushOGCD(AID.Delirium, Player);

        if (CD(AID.Delirium) > 0)
        {
            PushOGCD(AID.SaltedEarth, Player);

            PushOGCD(AID.Shadowbringer, primaryTarget);

            PushOGCD(AID.CarveAndSpit, primaryTarget);

            if (SaltedEarth > 0)
                PushOGCD(AID.SaltAndDarkness, Player);
        }
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        Gauge = GetGauge<DarkKnightGauge>();

        BloodWeapon = StatusStacks(SID.BloodWeapon);
        Delirium = StatusStacks(SID.Delirium);
        SaltedEarth = StatusLeft(SID.SaltedEarth);

        CalcNextBestGCD(strategy, primaryTarget);
        CalcNextBestOGCD(strategy, primaryTarget);
    }
}
