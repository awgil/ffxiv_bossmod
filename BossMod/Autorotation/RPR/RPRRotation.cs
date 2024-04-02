// CONTRIB: made by lazylemo, not checked
namespace BossMod.RPR;

public static class Rotation
{
    public class State : CommonRotation.PlayerState
    {
        public int ShroudGauge;
        public int SoulGauge;
        public int LemureShroudCount;
        public int VoidShroudCount;
        public float SoulReaverLeft;
        public float EnhancedGibbetLeft;
        public float EnhancedGallowsLeft;
        public float EnhancedVoidReapingLeft;
        public float EnhancedCrossReapingLeft;
        public float BloodsownCircleLeft;
        public float EnshroudedLeft;
        public float ArcaneCircleLeft;
        public float ImmortalSacrificeLeft;
        public float TrueNorthLeft;
        public float EnhancedHarpeLeft;
        public bool HasSoulsow;
        public float TargetDeathDesignLeft;
        public float CircleofSacrificeLeft;
        public bool lastActionisSoD;

        public AID Beststalk => EnhancedGallowsLeft > AnimationLock ? AID.UnveiledGallows
            : EnhancedGibbetLeft > AnimationLock ? AID.UnveiledGibbet
            : EnshroudedLeft > AnimationLock ? AID.LemuresSlice
            : AID.BloodStalk;
        public AID BestGallow => EnshroudedLeft > AnimationLock ? AID.CrossReaping : AID.Gallows;
        public AID BestGibbet => EnshroudedLeft > AnimationLock ? AID.VoidReaping : AID.Gibbet;
        public AID BestSow => HasSoulsow ? AID.HarvestMoon : AID.SoulSow;
        public SID ExpectedShadowofDeath => SID.DeathsDesign;
        public AID ComboLastMove => (AID)ComboLastAction;
        public State(WorldState ws) : base(ws) { }

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"shg={ShroudGauge}, Bloodsown={BloodsownCircleLeft} sog={SoulGauge}, RB={RaidBuffsLeft:f1}, DD={TargetDeathDesignLeft:f1}, EGI={EnhancedGibbetLeft:f1}, EGA={EnhancedGallowsLeft:f1}, CircleofSac={CircleofSacrificeLeft} SoulSlice={CD(CDGroup.SoulSlice)}, Enshroud={CD(CDGroup.Enshroud)}, AC={ArcaneCircleLeft}, ACCD={CD(CDGroup.ArcaneCircle):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public int NumAOEGCDTargets;
        public bool UseAOERotation;

        public override string ToString()
        {
            return $"AOE={NumAOEGCDTargets}, no-dots={ForbidDOTs}";
        }

        public enum GaugeUse : uint
        {
            Automatic = 0,

            [PropertyDisplay("Force extend DD Target Debuff, potentially overcapping DD", 0x80ff00ff)]
            ForceExtendDD = 1,

            [PropertyDisplay("Use Harpe or HarvestMoon if outside melee", 0x80c08000)]
            HarpeorHarvestMoonIfNotInMelee = 2,

            [PropertyDisplay("Use only HarvestMoon if outside melee", 0x80c08000)]
            HarvestMoonIfNotInMelee = 3,

            [PropertyDisplay("Force Harvest Moon", 0x80c08000)]
            ForceHarvestMoon = 4,

            [PropertyDisplay("Use combo, unless it can't be finished before downtime", 0x80c0c000)]
            ComboFitBeforeDowntime = 5,
        }

        public enum BloodstalkUse : uint
        {
            Automatic = 0,

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1,
            [PropertyDisplay("Force", 0x8000ff00)]
            Force = 2,
        }

        public enum TrueNorthUse : uint
        {
            Automatic = 0,

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1,
            [PropertyDisplay("Force", 0x8000ff00)]
            Force = 2,
        }

        public enum EnshroudUse : uint
        {
            Automatic = 0,

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1,

            [PropertyDisplay("Force", 0x8000ff00)]
            Force = 2,
        }

        public enum ArcaneCircleUse : uint
        {
            Automatic = 0,

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1,

            [PropertyDisplay("Force", 0x8000ff00)]
            Force = 2,
        }

        public enum GluttonyUse : uint
        {
            Automatic = 0,

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1,
            [PropertyDisplay("Force", 0x8000ff00)]
            Force = 2,
        }

        public enum PotionUse : uint
        {
            Manual = 0, // potion won't be used automatically

            [PropertyDisplay("Opener", 0x8000ff00)]
            Opener = 1,

            [PropertyDisplay("2+ minute windows", 0x8000ffff)]
            Burst = 2,

            [PropertyDisplay("Force", 0x800000ff)]
            Force = 3,

            [PropertyDisplay("Special (Needs testing)", 0x80400080)]
            Special = 4,
        }

        public enum SpecialAction : uint
        {
            None = 0, // don't use any special actions
        }

        public GaugeUse GaugeStrategy; // how are we supposed to handle gauge
        public BloodstalkUse BloodstalkStrategy; // how are we supposed to use bloodstalk
        public OffensiveAbilityUse SoulSliceStrategy;
        public EnshroudUse EnshroudStrategy; // how are we supposed to use enshroud
        public OffensiveAbilityUse ArcaneCircleStrategy;
        public GluttonyUse GluttonyStrategy;
        public PotionUse PotionStrategy; // how are we supposed to use potions
        //public OffensiveAbilityUse CommunioUse;
        public SpecialAction SpecialActionUse;
        public TrueNorthUse TrueNorthStrategy;
        public bool Aggressive;

        public void ApplyStrategyOverrides(uint[] overrides)
        {
            if (overrides.Length >= 9)
            {
                GaugeStrategy = (GaugeUse)overrides[0];
                BloodstalkStrategy = (BloodstalkUse)overrides[1];
                SoulSliceStrategy = (OffensiveAbilityUse)overrides[2];
                TrueNorthStrategy = (TrueNorthUse)overrides[3];
                EnshroudStrategy = (EnshroudUse)overrides[4];
                ArcaneCircleStrategy = (OffensiveAbilityUse)overrides[5];
                GluttonyStrategy = (GluttonyUse)overrides[6];
                PotionStrategy = (PotionUse)overrides[7];
                SpecialActionUse = (SpecialAction)overrides[8];
            }
            else
            {
                GaugeStrategy = GaugeUse.Automatic;
                BloodstalkStrategy = BloodstalkUse.Automatic;
                SoulSliceStrategy = OffensiveAbilityUse.Automatic;
                TrueNorthStrategy = TrueNorthUse.Automatic;
                EnshroudStrategy = EnshroudUse.Automatic;
                ArcaneCircleStrategy = OffensiveAbilityUse.Automatic;
                GluttonyStrategy = GluttonyUse.Automatic;
                PotionStrategy = PotionUse.Manual;
                SpecialActionUse = SpecialAction.None;
            }
        }
    }

    public static int SoulGaugeGainedFromAction(State state, AID action) => action switch
    {
        AID.Slice or AID.WaxingSlice or AID.InfernalSlice => 10,
        AID.SoulSlice => 50,
        AID.SoulScythe => 50,
        AID.SpinningScythe or AID.NightmareScythe => 10,
        _ => 0
    };

    public static int ShroudGaugeGainedFromAction(State state, AID action) => action switch
    {
        AID.Gibbet or AID.Gallows or AID.Guillotine => 10,
        AID.PlentifulHarvest => 50,
        _ => 0
    };

    public static AID GetNextSTComboAction(AID comboLastMove, AID finisher) => comboLastMove switch
    {
        AID.WaxingSlice => finisher,
        AID.Slice => AID.WaxingSlice,
        _ => AID.Slice
    };

    public static int GetSTComboLength(AID comboLastMove) => comboLastMove switch
    {
        AID.WaxingSlice => 1,
        AID.Slice => 2,
        _ => 3
    };

    public static int GetAOEComboLength(AID comboLastMove) => comboLastMove == AID.SpinningScythe ? 1 : 2;

    public static AID GetNextMaimComboAction(AID comboLastMove) => comboLastMove == AID.Slice ? AID.WaxingSlice : AID.Slice;

    public static AID GetNextAOEComboAction(AID comboLastMove) => comboLastMove == AID.SpinningScythe ? AID.NightmareScythe : AID.SpinningScythe;

    public static AID GetNextUnlockedComboAction(State state, bool aoe)
    {
        if (aoe && state.Unlocked(AID.SpinningScythe))
        {
            return state.Unlocked(AID.NightmareScythe) && state.ComboLastMove == AID.SpinningScythe ? AID.NightmareScythe : AID.SpinningScythe;
        }
        else
        {
            return state.ComboLastMove switch
            {
                AID.WaxingSlice => state.Unlocked(AID.InfernalSlice) ?
                AID.InfernalSlice : AID.Slice,
                AID.Slice => state.Unlocked(AID.WaxingSlice) ? AID.WaxingSlice : AID.Slice,
                _ => AID.Slice
            };
        }
    }

    public static AID GetNextBSAction(State state, bool aoe)
    {
        if (!aoe)
        {
            if (state.EnhancedGibbetLeft > state.GCD)
                return AID.Gibbet;

            if (state.EnhancedGallowsLeft > state.GCD)
                return AID.Gallows;
        }

        if (aoe)
            return AID.Guillotine;


        return AID.Gallows;
    }

    public static bool RefreshDOT(State state, float timeLeft) => timeLeft < state.GCD;


    public static bool ShouldUseBloodstalk(State state, Strategy strategy, bool aoe)
    {
        bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
        bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
        switch (strategy.BloodstalkStrategy)
        {
            case Strategy.BloodstalkUse.Delay:
                return false;
            case Strategy.BloodstalkUse.Force:
                if (state.SoulGauge >= 50)
                    return true;
                return false;

            default:
                if (!state.TargetingEnemy)
                    return false;
                if (soulReaver)
                    return false;

                if (enshrouded)
                    return false;

                if (ShouldUseEnshroud(state, strategy) && state.CD(CDGroup.Enshroud) < state.GCD)
                    return false;

                if (state.SoulGauge >= 50 && state.CD(CDGroup.Gluttony) > 28 && !aoe && (state.ComboTimeLeft > 2.5 || state.ComboTimeLeft == 0) && state.ShroudGauge <= 90 && state.CD(CDGroup.ArcaneCircle) > 9)
                    return true;

                if (state.SoulGauge == 100 && state.CD(CDGroup.Gluttony) > state.AnimationLock && !aoe && (state.ComboTimeLeft > 2.5 || state.ComboTimeLeft == 0) && state.ShroudGauge <= 90 && state.ImmortalSacrificeLeft < state.AnimationLock)
                    return true;

                if (state.SoulGauge >= 50 && !aoe && (state.ComboTimeLeft > 2.5 || state.ComboTimeLeft == 0) && state.ImmortalSacrificeLeft > state.AnimationLock && state.BloodsownCircleLeft > 4.8f && (state.CD(CDGroup.SoulSlice) > 30 || state.CD(CDGroup.SoulSlice) < 60) && state.ShroudGauge <= 40)
                    return true;

                if ((state.CD(CDGroup.ArcaneCircle) < 9 || state.CD(CDGroup.ArcaneCircle) > 60) && state.ShroudGauge >= 50 && (state.ComboTimeLeft > 11 || state.ComboTimeLeft == 0))
                    return false;
                return false;
        }
    }

    public static bool ShouldUseGrimSwathe(State state, Strategy strategy, bool aoe)
    {
        bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
        bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
        switch (strategy.BloodstalkStrategy)
        {
            case Strategy.BloodstalkUse.Delay:
                return false;
            case Strategy.BloodstalkUse.Force:
                if (state.SoulGauge >= 50)
                    return true;
                return false;

            default:
                if (!state.TargetingEnemy)
                    return false;
                if (soulReaver)
                    return false;

                if (enshrouded)
                    return false;

                if (state.SoulGauge >= 50 && state.CD(CDGroup.Gluttony) > 28 && aoe && state.ShroudGauge <= 90)
                    return true;

                if (state.SoulGauge == 100 && state.CD(CDGroup.Gluttony) > state.AnimationLock && aoe && state.ShroudGauge <= 90)
                    return true;

                return false;
        }
    }

    public static bool ShouldUseGluttony(State state, Strategy strategy)
    {
        bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
        bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
        bool plentifulReady = state.Unlocked(AID.PlentifulHarvest) && ((state.ImmortalSacrificeLeft > state.AnimationLock) || (state.CircleofSacrificeLeft > state.AnimationLock));
        switch (strategy.GluttonyStrategy)
        {
            case Strategy.GluttonyUse.Delay:
                return false;
            case Strategy.GluttonyUse.Force:
                if (state.SoulGauge >= 50)
                    return true;
                return false;

            default:
                if (!state.TargetingEnemy)
                    return false;
                if (soulReaver)
                    return false;

                if (enshrouded)
                    return false;

                if (!state.Unlocked(AID.Gluttony))
                    return false;

                if (ShouldUseEnshroud(state, strategy))
                    return false;

                if ((!plentifulReady || (plentifulReady && state.BloodsownCircleLeft > 4.8f)) && (state.ComboTimeLeft > 5 || state.ComboTimeLeft == 0) && state.SoulGauge >= 50 && state.ShroudGauge <= 80 && state.TargetDeathDesignLeft > 0)
                    return true;

                return false;
        }
    }

    public static bool ShouldUseEnshroud(State state, Strategy strategy)
    {
        bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
        bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
        switch (strategy.EnshroudStrategy)
        {
            case Strategy.EnshroudUse.Delay:
                return false;

            case Strategy.EnshroudUse.Force:
                if (state.ShroudGauge >= 50)
                    return true;
                return false;

            default:
                if (!state.TargetingEnemy)
                    return false;
                if (soulReaver)
                    return false;
                if (!state.Unlocked(AID.Enshroud))
                    return false;

                if (enshrouded)
                    return false;
                if (state.ArcaneCircleLeft > state.AnimationLock && state.ShroudGauge >= 50 && (state.ComboTimeLeft > 11 || state.ComboTimeLeft == 0) && state.CD(CDGroup.Enshroud) < state.GCD)
                    return true;
                if ((state.CD(CDGroup.ArcaneCircle) < 9 || state.CD(CDGroup.ArcaneCircle) > 60) && state.ShroudGauge >= 50 && (state.ComboTimeLeft > 11 || state.ComboTimeLeft == 0) && state.CD(CDGroup.Enshroud) < state.GCD)
                    return true;

                return false;
        }
    }

    public static bool ShouldUseArcaneCircle(State state, Strategy strategy)
    {
        bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
        bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;

        if (strategy.ArcaneCircleStrategy == Strategy.OffensiveAbilityUse.Delay)
            return false;

        else if (strategy.ArcaneCircleStrategy == Strategy.OffensiveAbilityUse.Force)
            return true;

        else
        {
            if (!state.TargetingEnemy)
                return false;
            if (soulReaver)
                return false;

            if (enshrouded && state.LemureShroudCount is 3 && state.TargetDeathDesignLeft > 30)
                return true;
            if (state.ShroudGauge < 50 && !enshrouded && state.TargetDeathDesignLeft > 0)
                return true;
            return false;
        }
    }

    public static bool ShouldUsePotion(State state, Strategy strategy) => strategy.PotionStrategy switch
    {
        Strategy.PotionUse.Manual => false,
        Strategy.PotionUse.Opener => state.CD(CDGroup.ArcaneCircle) > state.GCD && state.CD(CDGroup.SoulSlice) > 0,
        Strategy.PotionUse.Burst => state.CD(CDGroup.ArcaneCircle) < 9 && state.lastActionisSoD && state.TargetDeathDesignLeft > 28,
        Strategy.PotionUse.Force => true,
        _ => false
    };

    public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
    {
        if (strategy.UseAOERotation)
            return default;

        if (state.Unlocked(AID.Gibbet) && !strategy.UseAOERotation)
        {
            if (state.EnhancedGibbetLeft > state.GCD)
                return (Positional.Flank, true);
            if (state.EnhancedGallowsLeft > state.GCD)
                return (Positional.Rear, true);

            return (Positional.Rear, true);
        }
        else
        {
            return default;
        }
    }

    public static bool ShouldUseTrueNorth(State state, Strategy strategy)
    {
        switch (strategy.TrueNorthStrategy)
        {
            case Strategy.TrueNorthUse.Delay:
                return false;

            default:
                if (!state.TargetingEnemy)
                    return false;
                if (state.TrueNorthLeft > state.AnimationLock)
                    return false;
                if (GetNextPositional(state, strategy).Item2 && strategy.NextPositionalCorrect && state.SoulReaverLeft > state.AnimationLock)
                    return false;
                if (GetNextPositional(state, strategy).Item2 && !strategy.NextPositionalCorrect && state.SoulReaverLeft > state.AnimationLock)
                    return true;
                if (GetNextPositional(state, strategy).Item2 && !strategy.NextPositionalCorrect && ShouldUseGluttony(state, strategy) && state.CD(CDGroup.Gluttony) < 2.5)
                    return true;
                return false;
        }
    }

    public static bool ShouldUseSoulSlice(State state, Strategy strategy, bool aoe)
    {
        bool plentifulReady = state.Unlocked(AID.PlentifulHarvest) && state.ImmortalSacrificeLeft > state.AnimationLock && state.CircleofSacrificeLeft < state.GCD;
        bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
        bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
        switch (strategy.SoulSliceStrategy)
        {
            case Strategy.OffensiveAbilityUse.Delay:
                return false;

            case Strategy.OffensiveAbilityUse.Force:
                return true;

            default:
                if (!state.TargetingEnemy)
                    return false;
                if (state.SoulGauge <= 50 && state.CD(CDGroup.SoulSlice) - 30 < state.GCD && (state.ComboTimeLeft > 5 || state.ComboTimeLeft == 0 || (state.ArcaneCircleLeft > state.AnimationLock && state.ComboTimeLeft > 11)) && state.CD(CDGroup.ArcaneCircle) > 11.5f)
                    return true;
                if (enshrouded)
                    return false;
                if (soulReaver)
                    return false;
                if (state.ArcaneCircleLeft > state.AnimationLock && state.ComboTimeLeft < 11)
                    return false;
                return false;
        }
    }

    public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
    {
        bool plentifulReady = state.Unlocked(AID.PlentifulHarvest) && state.ImmortalSacrificeLeft > state.AnimationLock && state.CircleofSacrificeLeft < state.GCD;
        bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
        bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
        // prepull
        if (strategy.CombatTimer > -100 && strategy.CombatTimer < -4.2f && !state.HasSoulsow)
            return AID.SoulSow;
        if (strategy.CombatTimer > -100 && strategy.CombatTimer < -1.6f)
            return AID.None;
        if (strategy.CombatTimer > -1.6f && strategy.CombatTimer < 0f)
            return AID.Harpe;

        if (strategy.GaugeStrategy == Strategy.GaugeUse.HarvestMoonIfNotInMelee && state.HasSoulsow && state.RangeToTarget > 3 && strategy.CombatTimer > 0)
            return AID.HarvestMoon;
        if (strategy.GaugeStrategy == Strategy.GaugeUse.ForceHarvestMoon && state.HasSoulsow)
            return AID.HarvestMoon;
        if (strategy.GaugeStrategy == Strategy.GaugeUse.HarpeorHarvestMoonIfNotInMelee && state.RangeToTarget > 3 && strategy.CombatTimer < 0)
            return AID.Harpe;
        if (strategy.GaugeStrategy == Strategy.GaugeUse.HarpeorHarvestMoonIfNotInMelee && state.HasSoulsow && state.RangeToTarget > 3 && strategy.CombatTimer > 0)
            return AID.HarvestMoon;
        if (strategy.GaugeStrategy == Strategy.GaugeUse.HarpeorHarvestMoonIfNotInMelee && !state.HasSoulsow && state.RangeToTarget > 3 && strategy.CombatTimer > 0)
            return AID.Harpe;
        if (strategy.GaugeStrategy == Strategy.GaugeUse.ForceExtendDD && state.Unlocked(AID.ShadowofDeath) && !soulReaver)
            return aoe ? AID.WhorlofDeath : AID.ShadowofDeath;

        if (strategy.PotionStrategy == Strategy.PotionUse.Special && state.HasSoulsow && (state.CD(CDGroup.ArcaneCircle) < 11.5 || state.CD(CDGroup.ArcaneCircle) > 115))
        {
            if (state.CD(CDGroup.ArcaneCircle) < 11.5f && state.TargetDeathDesignLeft < 30)
                return AID.ShadowofDeath;
            if (state.ComboTimeLeft != 0 || state.ComboTimeLeft == 0 && !enshrouded && !soulReaver)
                return GetNextUnlockedComboAction(state, aoe);
            if (state.LemureShroudCount is 3 && !state.lastActionisSoD && state.PotionCD < 1)
                return AID.ShadowofDeath;
            if (state.LemureShroudCount is 1)
                return AID.HarvestMoon;
            if (enshrouded && !aoe)
            {
                if (state.Unlocked(AID.Communio) && state.LemureShroudCount is 1 && state.VoidShroudCount is 0)
                    return AID.Communio;
                if (state.EnhancedVoidReapingLeft > state.AnimationLock)
                    return AID.VoidReaping;
                if (state.EnhancedCrossReapingLeft > state.AnimationLock)
                    return AID.CrossReaping;

                return AID.CrossReaping;
            }

            return GetNextUnlockedComboAction(state, aoe);
        }

        if (!aoe)
        {
            if (state.Unlocked(AID.ShadowofDeath) && state.TargetDeathDesignLeft <= state.GCD + 2.5f && !soulReaver)
                return AID.ShadowofDeath;
        }
        else
        {
            if (state.Unlocked(AID.WhorlofDeath) && state.TargetDeathDesignLeft <= state.GCD + 2.5f && !soulReaver)
                return AID.WhorlofDeath;
        }

        if (plentifulReady && state.BloodsownCircleLeft < 1 && !soulReaver && !enshrouded && (state.ComboTimeLeft > 2.5 || state.ComboTimeLeft == 0))
            return AID.PlentifulHarvest;

        if ((state.CD(CDGroup.Gluttony) < 7.5 && state.Unlocked(AID.Gluttony) && !enshrouded && !soulReaver && state.TargetDeathDesignLeft < 10) || (state.CD(CDGroup.Gluttony) > 25 && state.Unlocked(AID.Gluttony) && state.SoulGauge >= 50 && !soulReaver && !enshrouded && state.TargetDeathDesignLeft < 7.5))
            return AID.ShadowofDeath;

        if (enshrouded && !aoe)
        {
            if ((state.LemureShroudCount == 4 || state.LemureShroudCount == 3) && (!state.lastActionisSoD || strategy.PotionStrategy == Strategy.PotionUse.Burst && !state.lastActionisSoD && state.PotionCD < 1) && state.CD(CDGroup.ArcaneCircle) < 9)
                return AID.ShadowofDeath;
            if (state.Unlocked(AID.Communio) && state.LemureShroudCount is 1 && state.VoidShroudCount is 0)
                return AID.Communio;
            if (state.Unlocked(AID.LemuresSlice) && state.VoidShroudCount >= 2 && state.CD(CDGroup.ArcaneCircle) > 10)
                return AID.LemuresSlice;
            if (state.EnhancedVoidReapingLeft > state.AnimationLock)
                return AID.VoidReaping;
            if (state.EnhancedCrossReapingLeft > state.AnimationLock)
                return AID.CrossReaping;

            return AID.CrossReaping;
        }

        if (enshrouded && aoe)
        {
            if (state.CD(CDGroup.ArcaneCircle) < 6)
                return AID.WhorlofDeath;
            if (state.Unlocked(AID.Communio) && state.LemureShroudCount is 1 && state.VoidShroudCount is 0)
                return AID.Communio;

            return AID.GrimReaping;
        }

        if (state.SoulReaverLeft > state.GCD)
            return GetNextBSAction(state, aoe);

        if (ShouldUseSoulSlice(state, strategy, aoe) && aoe)
            return AID.SoulScythe;

        if (ShouldUseSoulSlice(state, strategy, aoe) && !aoe)
            return AID.SoulSlice;

        return GetNextUnlockedComboAction(state, aoe);
    }

    public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline, bool aoe)
    {
        bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
        bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
        var (positional, shouldUsePositional) = GetNextPositional(state, strategy);
        //if (strategy.ArcaneCircleStrategy == Strategy.ArcaneCircleUse.Delay)
        //    return ActionID.MakeSpell(AID.Enshroud);
        if (strategy.PotionStrategy == Strategy.PotionUse.Special && state.HasSoulsow)
        {
            if (state.CD(CDGroup.ArcaneCircle) < 7.5f && state.ShroudGauge >= 50 && state.CanWeave(CDGroup.Enshroud, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Enshroud);
            if (state.LemureShroudCount is 3 && state.CanWeave(state.PotionCD, 1.1f, deadline) && state.lastActionisSoD)
                return CommonDefinitions.IDPotionStr;
            if (state.LemureShroudCount is 2 && state.CanWeave(CDGroup.ArcaneCircle, 0.6f, deadline))
                return ActionID.MakeSpell(AID.ArcaneCircle);
            if (state.CD(CDGroup.ArcaneCircle) > 11 && state.VoidShroudCount >= 2 && state.CanWeave(CDGroup.LemuresSlice, 0.6f, deadline))
                return ActionID.MakeSpell(AID.LemuresSlice);
        }

        if (ShouldUsePotion(state, strategy) && state.CanWeave(state.PotionCD, 1.1f, deadline))
            return CommonDefinitions.IDPotionStr;
        if (ShouldUseTrueNorth(state, strategy) && state.CanWeave(CDGroup.TrueNorth - 45, 0.6f, deadline) && !aoe && state.GCD < 0.8)
            return ActionID.MakeSpell(AID.TrueNorth);
        if (ShouldUseEnshroud(state, strategy) && state.Unlocked(AID.Enshroud) && state.CanWeave(CDGroup.Enshroud, 0.6f, deadline))
            return ActionID.MakeSpell(AID.Enshroud);
        if (ShouldUseArcaneCircle(state, strategy) && state.Unlocked(AID.ArcaneCircle) && state.CanWeave(CDGroup.ArcaneCircle, 0.6f, deadline))
            return ActionID.MakeSpell(AID.ArcaneCircle);
        if (state.VoidShroudCount >= 2 && state.CanWeave(CDGroup.LemuresSlice, 0.6f, deadline) && !aoe && state.CD(CDGroup.ArcaneCircle) > 10)
            return ActionID.MakeSpell(AID.LemuresSlice);
        if (state.VoidShroudCount >= 2 && state.CanWeave(CDGroup.LemuresSlice, 0.6f, deadline) && aoe && state.CD(CDGroup.ArcaneCircle) > 10)
            return ActionID.MakeSpell(AID.LemuresScythe);
        if (ShouldUseGluttony(state, strategy) && state.Unlocked(AID.Gluttony) && state.CanWeave(CDGroup.Gluttony, 0.6f, deadline) && !enshrouded && state.TargetDeathDesignLeft > 5)
            return ActionID.MakeSpell(AID.Gluttony);
        if (ShouldUseBloodstalk(state, strategy, aoe) && state.Unlocked(AID.BloodStalk) && state.CanWeave(CDGroup.BloodStalk, 0.6f, deadline) && !enshrouded && state.TargetDeathDesignLeft > 2.5)
            return ActionID.MakeSpell(state.Beststalk);
        if (ShouldUseGrimSwathe(state, strategy, aoe) && state.Unlocked(AID.GrimSwathe) && state.CanWeave(CDGroup.BloodStalk, 0.6f, deadline) && !enshrouded && state.TargetDeathDesignLeft > 2.5)
            return ActionID.MakeSpell(AID.GrimSwathe);

        return new();
    }
}
