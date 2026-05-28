using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class GNB(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, GNB.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track("Bloodfest", MinLevel = 76, Actions = [AID.Bloodfest, AID.NoMercy])]
        public Track<OffensiveStrategy> Buffs;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan GNB", "Gunbreaker", "Standard rotation (xan)|Tanks", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.GNB), 100).WithStrategies<Strategy>();
    }

    public int Ammo;
    public byte AmmoCombo;
    public float Reign;

    public float SonicBreak;
    public float NoMercy;
    public float Bloodfest;

    public int NumAOETargets;
    public int NumReignTargets;

    private Enemy? BestReignTarget;

    public bool FastGCD => GCDLength <= 2.47f;
    public int MaxAmmo => Bloodfest > GCD ? 6 : Unlocked(TraitID.CartridgeChargeII) ? 3 : 2;

    public enum GCDPriority
    {
        None = 0,
        Filler = 100,
        FillerAOE = 150,
        AmmoOvercap = 200,
        FangCombo = 600,
        ReignCombo = 700,
        StandardComboRefresh = 725,
        SonicBreak = 750,
        DoubleDown = 800,
        FangOvercap = 900,
    }

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        Ammo = gauge.Ammo;
        AmmoCombo = gauge.AmmoComboStep;

        Reign = StatusLeft(SID.ReadyToReign);
        SonicBreak = StatusLeft(SID.ReadyToBreak);
        Bloodfest = StatusLeft(SID.Bloodfest);
        NoMercy = StatusLeft(SID.NoMercy);

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestReignTarget, NumReignTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);

        CalcNextBestOGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
            return;

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.DemonSlice, Unlocked(AID.FatedCircle) && Ammo > 0 ? 2 : 3, maximumActionRange: 20);

        if (MaxChargesIn(AID.GnashingFang) < GCD + GCDLength && AmmoCombo == 0 && Ammo > 0 && (OnCooldown(AID.NoMercy) || CombatTimer > 10))
            PushGCD(AID.GnashingFang, primaryTarget, GCDPriority.FangOvercap);

        if (NoMercy > GCD && GCDReady(AID.GnashingFang) && AmmoCombo == 0 && Ammo > 0)
            PushGCD(AID.GnashingFang, primaryTarget, GCDPriority.FangCombo);

        if (NumAOETargets > 0 && Ammo > 0 && NoMercy > GCD && GCDReady(AID.DoubleDown))
            PushGCD(AID.DoubleDown, Player, GCDPriority.DoubleDown);

        if (SonicBreak > GCD)
            PushGCD(AID.SonicBreak, primaryTarget, GCDPriority.SonicBreak);

        switch (AmmoCombo)
        {
            case 1:
                PushGCD(AID.SavageClaw, primaryTarget, GCDPriority.FangCombo);
                return;
            case 2:
                PushGCD(AID.WickedTalon, primaryTarget, GCDPriority.FangCombo);
                return;
            case 3:
                PushGCD(AID.NobleBlood, BestReignTarget, GCDPriority.ReignCombo);
                return;
            case 4:
                PushGCD(AID.LionHeart, BestReignTarget, GCDPriority.ReignCombo);
                return;
        }

        if (Reign > GCD && AmmoCombo == 0 && OnCooldown(AID.NoMercy))
            PushGCD(AID.ReignOfBeasts, BestReignTarget, GCDPriority.ReignCombo);

        if (NumAOETargets > 2 && Unlocked(AID.DemonSlice))
        {
            if (ComboLastMove == AID.DemonSlice)
                PushGCD(AID.DemonSlaughter, Player, GCDPriority.FillerAOE);

            PushGCD(AID.DemonSlice, Player, GCDPriority.FillerAOE);
        }

        if (ComboLastMove == AID.DemonSlice && NumAOETargets > 0)
            PushGCD(AID.DemonSlaughter, Player, GCDPriority.FillerAOE);

        if (ComboLastMove == AID.BrutalShell)
            PushGCD(AID.SolidBarrel, primaryTarget, GCDPriority.Filler);

        if (ComboLastMove == AID.KeenEdge)
            PushGCD(AID.BrutalShell, primaryTarget, GCDPriority.Filler);

        PushGCD(AID.KeenEdge, primaryTarget, GCDPriority.Filler);

        if (NextGCD is AID.SolidBarrel or AID.DemonSlaughter && Ammo == MaxAmmo)
        {
            if (NumAOETargets > 1)
                PushGCD(AID.FatedCircle, Player, GCDPriority.AmmoOvercap + 1);

            PushGCD(AID.BurstStrike, primaryTarget, GCDPriority.AmmoOvercap);
        }

        if (NextGCD is AID.ReignOfBeasts or AID.GnashingFang
            && AmmoCombo == 0
            && !CanFitGCD(World.Client.ComboState.Remaining, 3))
        {
            var (id, target) = ComboLastMove switch
            {
                AID.BrutalShell => (AID.SolidBarrel, primaryTarget),
                AID.KeenEdge => (AID.BrutalShell, primaryTarget),
                AID.DemonSlice => NumAOETargets > 0 ? (AID.DemonSlaughter, null) : (AID.None, null),
                _ => (AID.None, null)
            };
            if (id != AID.None)
                PushGCD(id, target, GCDPriority.StandardComboRefresh);
        }
    }

    private void CalcNextBestOGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        PushOGCD(Continuation, primaryTarget);

        if (strategy.Buffs != OffensiveStrategy.Delay)
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

    private void UseNoMercy(in Strategy strategy)
    {
        if (FastGCD)
        {
            if (CombatTimer >= 10 || ComboLastMove == AID.BrutalShell || NumAOETargets > 1)
                PushOGCD(AID.NoMercy, Player);
        }
        else if (Ammo > 0)
            PushOGCD(AID.NoMercy, Player, delay: GCD - 0.8f);
    }

    private AID Continuation
    {
        get
        {
            foreach (var s in Player.Statuses)
            {
                switch ((SID)s.ID)
                {
                    case SID.ReadyToBlast:
                        return AID.Hypervelocity;
                    case SID.ReadyToRaze:
                        return AID.FatedBrand;
                    case SID.ReadyToRip:
                        return AID.JugularRip;
                    case SID.ReadyToGouge:
                        return AID.EyeGouge;
                    case SID.ReadyToTear:
                        return AID.AbdomenTear;
                }
            }

            return AID.None;
        }
    }
}
