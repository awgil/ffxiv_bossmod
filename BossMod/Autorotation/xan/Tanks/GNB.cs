using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class GNB(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan GNB", "Gunbreaker", "Standard rotation (xan)|Tanks", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.GNB), 100);

        def.DefineShared().AddAssociatedActions(AID.Bloodfest);

        return def;
    }

    public int Ammo;
    public byte AmmoCombo;
    public float Reign;

    public float SonicBreak;
    public bool Continuation;
    public float NoMercy;

    public int NumAOETargets;
    public int NumReignTargets;

    private Actor? BestReignTarget;

    public bool FastGCD => GCDLength <= 2.47f;
    public int MaxAmmo => Unlocked(TraitID.CartridgeChargeII) ? 3 : 2;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        Ammo = gauge.Ammo;
        AmmoCombo = gauge.AmmoComboStep;

        Reign = StatusLeft(SID.ReadyToReign);
        SonicBreak = StatusLeft(SID.ReadyToBreak);
        Continuation = Player.Statuses.Any(s => IsContinuationStatus((SID)s.ID));
        NoMercy = StatusLeft(SID.NoMercy);

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestReignTarget, NumReignTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);

        CalcNextBestOGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
            return;

        GoalZoneCombined(3, Hints.GoalAOECircle(5), Unlocked(AID.FatedCircle) && Ammo > 0 ? 2 : 3);

        if (ReadyIn(AID.NoMercy) > 20 && Ammo > 0)
            PushGCD(AID.GnashingFang, primaryTarget);

        if (NumAOETargets > 0 && Ammo >= 2 && NoMercy > GCD && ReadyIn(AID.DoubleDown) <= GCD)
            PushGCD(AID.DoubleDown, Player);

        if (SonicBreak > GCD)
            PushGCD(AID.SonicBreak, primaryTarget);

        switch (AmmoCombo)
        {
            case 1:
                PushGCD(AID.SavageClaw, primaryTarget);
                return;
            case 2:
                PushGCD(AID.WickedTalon, primaryTarget);
                return;
            case 3:
                PushGCD(AID.NobleBlood, BestReignTarget);
                return;
            case 4:
                PushGCD(AID.LionHeart, BestReignTarget);
                return;
        }

        if (Reign > GCD && OnCooldown(AID.GnashingFang) && OnCooldown(AID.DoubleDown) && SonicBreak == 0)
            PushGCD(AID.ReignOfBeasts, BestReignTarget);

        if (NumAOETargets > 1 && ShouldBust(strategy, AID.BurstStrike))
        {
            PushGCD(AID.FatedCircle, Player);
            PushGCD(AID.BurstStrike, primaryTarget);
        }

        if (NumAOETargets > 2 && Unlocked(AID.DemonSlice))
        {
            if (ComboLastMove == AID.BrutalShell)
                PushGCD(AID.SolidBarrel, primaryTarget);

            if (ComboLastMove == AID.DemonSlice)
                PushGCD(AID.DemonSlaughter, Player);

            PushGCD(AID.DemonSlice, Player);
        }

        if (ShouldBust(strategy, AID.BurstStrike))
            PushGCD(AID.BurstStrike, primaryTarget);

        if (ComboLastMove == AID.DemonSlice && NumAOETargets > 0)
            PushGCD(AID.DemonSlaughter, Player);

        if (ComboLastMove == AID.BrutalShell)
            PushGCD(AID.SolidBarrel, primaryTarget);

        if (ComboLastMove == AID.KeenEdge)
            PushGCD(AID.BrutalShell, primaryTarget);

        PushGCD(AID.KeenEdge, primaryTarget);
    }

    // TODO handle forced 2 cartridge burst
    private bool ShouldBust(StrategyValues strategy, AID spend)
    {
        if (!Unlocked(spend) || Ammo == 0)
            return false;

        if (NoMercy > GCD)
            return ReadyIn(AID.DoubleDown) > NoMercy
                || Ammo == MaxAmmo
                || Ammo == 1 && ReadyIn(AID.DoubleDown) < NoMercy;

        return ComboLastMove is AID.BrutalShell or AID.DemonSlice && Ammo == MaxAmmo;
    }

    private void CalcNextBestOGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (Continuation)
            PushOGCD(AID.Continuation, primaryTarget);

        if (strategy.BuffsOk() && Unlocked(AID.Bloodfest) && Ammo == 0)
            PushOGCD(AID.Bloodfest, primaryTarget);

        UseNoMercy(strategy);

        var usedNM = ReadyIn(AID.NoMercy) > 20;

        if (usedNM)
        {
            PushOGCD(AID.DangerZone, primaryTarget);

            if (NumAOETargets > 0)
                PushOGCD(AID.BowShock, Player);
        }
    }

    private void UseNoMercy(StrategyValues strategy)
    {
        if (FastGCD)
        {
            if (CombatTimer >= 10 || ComboLastMove == AID.BrutalShell || NumAOETargets > 1)
                PushOGCD(AID.NoMercy, Player);
        }
        else if (Ammo > 0)
            PushOGCD(AID.NoMercy, Player, delay: GCD - 0.8f);
    }

    private bool IsContinuationStatus(SID sid) => sid is SID.ReadyToBlast or SID.ReadyToRaze or SID.ReadyToGouge or SID.ReadyToTear or SID.ReadyToRip;
}
