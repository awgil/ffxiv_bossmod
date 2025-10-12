using BossMod.VPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class VPR(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player, PotionType.Dexterity)
{
    public enum Track { Snap = SharedTrack.Count }
    public enum SnapStrategy
    {
        None,
        Ranged
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan VPR", "Viper", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.VPR), 100);

        def.DefineShared().AddAssociatedActions(AID.Reawaken);
        def.Define(Track.Snap).As<SnapStrategy>("WrithingSnap")
            .AddOption(SnapStrategy.None, "None", "Don't use")
            .AddOption(SnapStrategy.Ranged, "Ranged", "Use when out of melee range, if out of Coil stacks")
            .AddAssociatedActions(AID.WrithingSnap);

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

    private Enemy? BestRangedAOETarget;
    private Enemy? BestGenerationTarget;

    private int CoilMax => Unlocked(TraitID.EnhancedVipersRattle) ? 3 : 2;

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<ViperGauge>();
        DreadCombo = gauge.DreadCombo;
        Coil = gauge.RattlingCoilStacks;
        Offering = gauge.SerpentOffering;
        Anguine = gauge.AnguineTribute;

        CurSerpentsTail = gauge.SerpentCombo switch
        {
            SerpentCombo.DeathRattle => AID.DeathRattle,
            SerpentCombo.LastLash => AID.LastLash,
            SerpentCombo.FirstLegacy => AID.FirstLegacy,
            SerpentCombo.SecondLegacy => AID.SecondLegacy,
            SerpentCombo.ThirdLegacy => AID.ThirdLegacy,
            SerpentCombo.FourthLegacy => AID.FourthLegacy,
            _ => AID.SerpentsTail,
        };
        // this doesn't really matter because the GCDs grant unique statuses, but might as well track regardless
        TwinCombo = (byte)gauge.SerpentCombo switch
        {
            7 => TwinType.SingleTarget,
            8 => TwinType.AOE,
            9 => TwinType.Coil,
            _ => TwinType.None
        };
        TwinStacks = (byte)gauge.SerpentCombo & 3;

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

        if (primaryTarget == null)
            return;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 0.7f)
                PushGCD(AID.Slither, primaryTarget);

            return;
        }

        var aoeBreakpoint = DreadCombo switch
        {
            DreadCombo.Dreadwinder or DreadCombo.HuntersCoil or DreadCombo.SwiftskinsCoil => 50,
            DreadCombo.HuntersDen or DreadCombo.SwiftskinsDen or DreadCombo.PitOfDread => 1,
            _ => Anguine > 0 ? 50 : 3
        };

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

        // fallback 2 for out of range
        if (strategy.Option(Track.Snap).As<SnapStrategy>() == SnapStrategy.Ranged)
            PushGCD(AID.WrithingSnap, primaryTarget);

        var pos = GetPositional(strategy);
        UpdatePositionals(primaryTarget, ref pos);

        OGCD(strategy, primaryTarget);

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.SteelMaw, aoeBreakpoint, 20);
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

    private bool ShouldVice(StrategyValues strategy) => Swiftscaled > GCD && DreadCombo == 0 && ReadyIn(AID.Vicewinder) <= GCD;

    private bool ShouldCoil(StrategyValues strategy) => Coil == CoilMax && Swiftscaled > GCD && DreadCombo == 0;

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
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

        if (NextPositionalImminent && !NextPositionalCorrect)
            PushOGCD(AID.TrueNorth, Player, -10, GCD - 0.8f);
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

            if (DreadCombo is DreadCombo.HuntersDen or DreadCombo.SwiftskinsDen or DreadCombo.PitOfDread)
                return (Positional.Any, false);

            if (NumAOETargets > 2)
                return (Positional.Any, false);

            return ComboLastMove switch
            {
                AID.HuntersSting => (Positional.Flank, true),
                AID.SwiftskinsSting => (Positional.Rear, true),
                _ => (Swiftscaled < Instinct ? Positional.Rear : Positional.Flank, false)
            };
        }

        var (pos, imm) = getmain();

        if (NextGCD is not (AID.FlanksbaneFang or AID.FlankstingStrike or AID.HindsbaneFang or AID.HindstingStrike or AID.HuntersCoil or AID.SwiftskinsCoil))
            imm = false;

        return (pos, imm);
    }
}
