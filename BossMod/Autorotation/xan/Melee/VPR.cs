using BossMod.VPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using System.Runtime.InteropServices;

namespace BossMod.Autorotation.xan;

public sealed class VPR(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan VPR", "Viper", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.VPR), 100);

        def.DefineShared().AddAssociatedActions(AID.Reawaken);

        return def;
    }

    public enum TwinType
    {
        None,
        SingleTarget,
        AOE,
        Coil
    }

    public DreadCombo DreadCombo;
    public int Coil; // max 3
    public int Offering; // max 100
    public int Anguine; // 0-4
    public AID CurSerpentsTail; // adjusted during reawaken and after basic combos
    public int TwinStacks; // max 2, granted by using "coil" or "den" gcds
    public TwinType TwinCombo;

    public float Swiftscaled;
    public float Instinct;
    public float FlankstungVenom;
    public float FlanksbaneVenom;
    public float HindstungVenom;
    public float HindsbaneVenom;
    public float HuntersVenom;
    public float SwiftskinsVenom;
    public float FellhuntersVenom;
    public float FellskinsVenom;
    public float GrimhuntersVenom;
    public float GrimskinsVenom;
    public float PoisedForTwinfang;
    public float PoisedForTwinblood;
    public float ReawakenReady;
    public float ReawakenLeft;
    public float HonedReavers;
    public float HonedSteel;

    public int NumAOETargets;
    public int NumRangedAOETargets;

    private Actor? BestRangedAOETarget;
    private Actor? BestGenerationTarget;

    private int CoilMax => Unlocked(TraitID.EnhancedVipersRattle) ? 3 : 2;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<ViperGaugeEx>();
        DreadCombo = gauge.DreadCombo;
        Coil = gauge.RattlingCoilStacks;
        Offering = gauge.SerpentOffering;
        Anguine = gauge.AnguineTribute;

        CurSerpentsTail = (gauge.ComboEx >> 2) switch
        {
            1 => AID.DeathRattle,
            2 => AID.LastLash,
            3 => AID.FirstLegacy,
            4 => AID.SecondLegacy,
            5 => AID.ThirdLegacy,
            6 => AID.FourthLegacy,
            _ => AID.SerpentsTail,
        };
        // this doesn't really matter because the GCDs grant unique statuses, but might as well track regardless
        TwinCombo = (gauge.ComboEx >> 2) switch
        {
            7 => TwinType.SingleTarget,
            8 => TwinType.AOE,
            9 => TwinType.Coil,
            _ => TwinType.None
        };
        TwinStacks = gauge.ComboEx & 3;

        FlanksbaneVenom = StatusLeft(SID.FlanksbaneVenom);
        FlankstungVenom = StatusLeft(SID.FlankstungVenom);
        HindsbaneVenom = StatusLeft(SID.HindsbaneVenom);
        HindstungVenom = StatusLeft(SID.HindstungVenom);
        Swiftscaled = StatusLeft(SID.Swiftscaled);
        Instinct = StatusLeft(SID.HuntersInstinct);
        HuntersVenom = StatusLeft(SID.HuntersVenom);
        SwiftskinsVenom = StatusLeft(SID.SwiftskinsVenom);
        FellhuntersVenom = StatusLeft(SID.FellhuntersVenom);
        FellskinsVenom = StatusLeft(SID.FellskinsVenom);
        GrimhuntersVenom = StatusLeft(SID.GrimhuntersVenom);
        GrimskinsVenom = StatusLeft(SID.GrimskinsVenom);
        PoisedForTwinfang = StatusLeft(SID.PoisedForTwinfang);
        PoisedForTwinblood = StatusLeft(SID.PoisedForTwinblood);
        ReawakenReady = StatusLeft(SID.ReawakenReady);
        ReawakenLeft = StatusLeft(SID.Reawakened);
        HonedReavers = StatusLeft(SID.HonedReavers);
        HonedSteel = StatusLeft(SID.HonedSteel);

        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget);
        BestGenerationTarget = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget).Best;
        NumAOETargets = NumMeleeAOETargets(strategy);

        UpdatePositionals(primaryTarget, GetPositional(strategy), TrueNorthLeft > GCD);

        OGCD(strategy, primaryTarget);

        if (CombatTimer < 1 && Player.DistanceToHitbox(primaryTarget) is > 3 and < 20)
            PushGCD(AID.Slither, primaryTarget);

        if (ShouldReawaken(strategy))
            PushGCD(AID.Reawaken, Player);

        if (DreadCombo == DreadCombo.HuntersCoil)
            PushGCD(AID.SwiftskinsCoil, primaryTarget);

        if (DreadCombo == DreadCombo.SwiftskinsCoil)
            PushGCD(AID.HuntersCoil, primaryTarget);

        if (DreadCombo == DreadCombo.Dreadwinder)
        {
            if (Swiftscaled < Instinct)
                PushGCD(AID.SwiftskinsCoil, primaryTarget);

            PushGCD(AID.HuntersCoil, primaryTarget);
        }

        // if no target, no buff
        if (DreadCombo == DreadCombo.HuntersDen && NumAOETargets > 0)
            PushGCD(AID.SwiftskinsDen, Player);

        if (DreadCombo == DreadCombo.SwiftskinsDen && NumAOETargets > 0)
            PushGCD(AID.HuntersDen, Player);

        if (DreadCombo == DreadCombo.PitOfDread)
        {
            if (Swiftscaled < Instinct)
                PushGCD(AID.SwiftskinsDen, Player);

            PushGCD(AID.HuntersDen, Player);
        }

        if (Anguine > 0)
        {
            var max = Unlocked(TraitID.EnhancedSerpentsLineage) ? 5 : 4;
            PushGCD((max - Anguine) switch
            {
                0 => AID.FirstGeneration,
                1 => AID.SecondGeneration,
                2 => AID.ThirdGeneration,
                3 => AID.FourthGeneration,
                4 => AID.Ouroboros,
                _ => AID.None
            }, BestGenerationTarget);
        }

        if (ShouldCoil(strategy))
            PushGCD(AID.UncoiledFury, BestRangedAOETarget);

        // 123 combos
        // 1. 34606 steel fangs (left)
        //    34607 dread fangs (right)
        //   use right to refresh debuff, otherwise left
        //
        // 2. 34608 hunter (left) damage buff
        //    34609 swiftskin (right) haste buff
        //   pick one based on buff timer, if both are 0 then choose your favorite
        //
        // 3. 34610 flank strike (left) (combos from hunter)
        //    34612 hind strike (left) (combos from swift)
        //    34611 flank fang (right) (combos from hunter)
        //    34613 hind fang (right) (combos from swift)
        //   each action buffs the next one in a loop

        if (NumAOETargets > 2)
        {
            if (ShouldVice(strategy))
                PushGCD(AID.Vicepit, Player);

            if (ComboLastMove is AID.HuntersBite or AID.SwiftskinsBite)
            {
                if (GrimskinsVenom > GCD)
                    PushGCD(AID.BloodiedMaw, Player);

                PushGCD(AID.JaggedMaw, Player);
            }

            if (ComboLastMove is AID.SteelMaw or AID.ReavingMaw)
            {
                if (Instinct < Swiftscaled)
                    PushGCD(AID.HuntersBite, Player);

                PushGCD(AID.SwiftskinsBite, Player);
            }

            if (HonedSteel == 0 && Unlocked(AID.ReavingFangs))
                PushGCD(AID.ReavingMaw, primaryTarget);

            PushGCD(AID.SteelMaw, Player);
        }
        else
        {
            if (ShouldVice(strategy))
                PushGCD(AID.Vicewinder, primaryTarget);

            if (ComboLastMove is AID.HuntersSting)
            {
                if (FlankstungVenom > GCD)
                    PushGCD(AID.FlankstingStrike, primaryTarget);

                PushGCD(AID.FlanksbaneFang, primaryTarget);
            }

            if (ComboLastMove is AID.SwiftskinsSting)
            {
                if (HindstungVenom > GCD)
                    PushGCD(AID.HindstingStrike, primaryTarget);

                PushGCD(AID.HindsbaneFang, primaryTarget);
            }

            if (ComboLastMove is AID.SteelFangs or AID.ReavingFangs)
            {
                if (Instinct < Swiftscaled)
                    PushGCD(AID.HuntersSting, primaryTarget);

                PushGCD(AID.SwiftskinsSting, primaryTarget);
            }

            if (HonedSteel == 0 && Unlocked(AID.ReavingFangs))
                PushGCD(AID.ReavingFangs, primaryTarget);

            PushGCD(AID.SteelFangs, primaryTarget);
        }

        // fallback for out of range
        if (Coil > 0)
            PushGCD(AID.UncoiledFury, BestRangedAOETarget);
    }

    private bool ShouldReawaken(StrategyValues strategy)
    {
        if (!Unlocked(AID.Reawaken) || ReawakenReady == 0 && Offering < 50 || ReawakenLeft > 0 || !strategy.BuffsOk())
            return false;

        if (strategy.Option(SharedTrack.Buffs).As<OffensiveStrategy>() == OffensiveStrategy.Force)
            return true;

        // full reawaken combo is reawaken (2.2) + generation 1-4 (2s each) = 10.2s (scaled by skill speed) (ouroboros not accounted for since we only really care about casting it with the debuff active)
        var baseDuration = 8.2f;
        if (Unlocked(TraitID.EnhancedSerpentsLineage))
            baseDuration += 2;

        var actual = baseDuration * AttackGCDLength / 2.5f;

        if (NumAOETargets == 0 || Instinct < actual || Swiftscaled < actual || DreadCombo > 0)
            return false;

        if (RaidBuffsIn > 9000 || RaidBuffsLeft > 10 || ReawakenReady > GCD)
            return true;

        return Offering == 100 && ComboLastMove is AID.HuntersSting or AID.SwiftskinsSting or AID.HuntersBite or AID.SwiftskinsBite;
    }

    private bool ShouldVice(StrategyValues strategy) => Swiftscaled > GCD && DreadCombo == 0 && NextChargeIn(AID.Vicewinder) <= GCD;

    private bool ShouldCoil(StrategyValues strategy) => Coil > 1 && Swiftscaled > GCD && DreadCombo == 0;

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (CurSerpentsTail != AID.SerpentsTail)
            PushOGCD(CurSerpentsTail, primaryTarget);

        switch (TwinCombo)
        {
            case TwinType.Coil:
                if (PoisedForTwinblood > 0)
                    PushOGCD(AID.UncoiledTwinblood, BestRangedAOETarget);

                if (PoisedForTwinfang > 0)
                    PushOGCD(AID.UncoiledTwinfang, BestRangedAOETarget);
                break;

            case TwinType.AOE:
                if (FellhuntersVenom > 0)
                    PushOGCD(AID.TwinfangThresh, Player);

                if (FellskinsVenom > 0)
                    PushOGCD(AID.TwinbloodThresh, Player);
                break;

            case TwinType.SingleTarget:
                if (HuntersVenom > 0)
                    PushOGCD(AID.TwinfangBite, primaryTarget);

                if (SwiftskinsVenom > 0)
                    PushOGCD(AID.TwinbloodBite, primaryTarget);
                break;
        }

        if (Unlocked(AID.SerpentsIre) && Coil < CoilMax)
            PushOGCD(AID.SerpentsIre, Player);
    }

    private (Positional, bool) GetPositional(StrategyValues strategy)
    {
        (Positional, bool) getmain()
        {
            if (!Unlocked(AID.FlankstingStrike))
                return (Positional.Any, false);

            if (DreadCombo == DreadCombo.Dreadwinder)
                return (Swiftscaled < Instinct ? Positional.Rear : Positional.Flank, true);

            if (DreadCombo == DreadCombo.HuntersCoil)
                return (Positional.Rear, true);

            if (DreadCombo == DreadCombo.SwiftskinsCoil)
                return (Positional.Flank, true);

            return ComboLastMove switch
            {
                AID.HuntersSting => (Positional.Flank, true),
                AID.SwiftskinsSting => (Positional.Rear, true),
                _ => (Swiftscaled < Instinct ? Positional.Rear : Positional.Flank, false)
            };
        }

        var (pos, imm) = getmain();

        if (Anguine > 0 || ShouldReawaken(strategy) || ShouldVice(strategy) || ShouldCoil(strategy))
            imm = false;

        return (pos, imm);
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x11)]
    private struct ViperGaugeEx
    {
        [FieldOffset(0x08)] public byte RattlingCoilStacks;
        [FieldOffset(0x0A)] public byte SerpentOffering;
        [FieldOffset(0x09)] public byte AnguineTribute;
        [FieldOffset(0x0B)] public DreadCombo DreadCombo;
        [FieldOffset(0x10)] public byte ComboEx; // extra combo stuff
    }
}
