using System;

namespace BossMod.RPR
{
    public static class Rotation
    {
        public class State : CommonRotation.PlayerState
        {
            public int ShroudGauge;
            public int SoulGauge; 
            public int LemureShroudCount;
            public int VoidShroudCount;
            public float SoulReaverLeft;
            public float SoulSowLeft;
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
            public float HasSoulsow;
            public float TargetDeathDesignLeft;

            public AID Beststalk => EnhancedGallowsLeft > AnimationLock ? AID.UnveiledGallows 
                : EnhancedGibbetLeft > AnimationLock ? AID.UnveiledGibbet
                : EnshroudedLeft > AnimationLock ? AID.LemuresSlice
                : AID.BloodStalk;
            public AID BestGallow => EnshroudedLeft > AnimationLock ? AID.CrossReaping : AID.Gallows;
            public AID BestGibbet => EnshroudedLeft > AnimationLock ? AID.VoidReaping : AID.Gibbet;
            public SID ExpectedShadowofDeath => SID.DeathsDesign;
            public AID ComboLastMove => (AID)ComboLastAction;
            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"shg={ShroudGauge}, Bloodsown={BloodsownCircleLeft} sog={SoulGauge}, RB={RaidBuffsLeft:f1}, DD={TargetDeathDesignLeft:f1}, EGI={EnhancedGibbetLeft:f1}, EGA={EnhancedGallowsLeft:f1}, AC={ArcaneCircleLeft:f1}, ACCD={CD(CDGroup.ArcaneCircle):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
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

                [PropertyDisplay("Spend all gauge ASAP", 0x8000ff00)]
                Spend = 1,

                [PropertyDisplay("Conserve unless under raid buffs", 0x8000ffff)]
                ConserveIfNoBuffs = 2,

                [PropertyDisplay("Conserve as much as possible", 0x800000ff)]
                Conserve = 3,

                [PropertyDisplay("Force extend ST buff, potentially overcapping gauge and/or ST", 0x80ff00ff)]
                ForceExtendDD = 4,

                [PropertyDisplay("Use Harpe or HarvestMoon if outside melee", 0x80c08000)]
                HarpeorHarvestMoonIfNotInMelee = 6,

                [PropertyDisplay("Use combo, unless it can't be finished before downtime and unless gauge and/or ST would overcap", 0x80c0c000)]
                ComboFitBeforeDowntime = 7,

                [PropertyDisplay("Use combo until second-last step, then spend gauge", 0x80400080)]
                PenultimateComboThenSpend = 8,
            }

            public enum BloodstalkUse : uint
            {
                Automatic = 0,

                [PropertyDisplay("Delay", 0x800000ff)]
                Delay = 1,
            }

            public enum TrueNorthUse : uint
            {
                Automatic = 0,

                [PropertyDisplay("Delay", 0x800000ff)]
                Delay = 1,
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
            }

            public enum PotionUse : uint
            {
                Manual = 0, // potion won't be used automatically

                [PropertyDisplay("Use ASAP, but delay slightly during opener", 0x8000ff00)]
                Immediate = 1,

                [PropertyDisplay("Delay until raidbuffs", 0x8000ffff)]
                DelayUntilRaidBuffs = 2,

                [PropertyDisplay("Use ASAP, even if without ST", 0x800000ff)]
                Force = 3,
            }

            public enum SpecialAction : uint
            {
                None = 0, // don't use any special actions

                [PropertyDisplay("LB3", 0x8000ff00)]
                LB3, // use LB3 if available
            }

            public GaugeUse GaugeStrategy; // how are we supposed to handle gauge
            public BloodstalkUse BloodstalkStrategy; // how are we supposed to use bloodstalk
            public EnshroudUse EnshroudStrategy; // how are we supposed to use enshroud
            public GluttonyUse GluttonyStrategy;
            public PotionUse PotionStrategy; // how are we supposed to use potions
            public ArcaneCircleUse ArcaneCircleStrategy;
            public OffensiveAbilityUse CommunioUse;
            public SpecialAction SpecialActionUse;
            public TrueNorthUse TrueNorthStrategy;
            public bool Aggressive;

            public void ApplyStrategyOverrides(uint[] overrides)
            {
                if (overrides.Length >= 8)
                {
                    GaugeStrategy = (GaugeUse)overrides[0];
                    PotionStrategy = (PotionUse)overrides[2];
                    ArcaneCircleStrategy = (ArcaneCircleUse)overrides[3];
                    BloodstalkStrategy = (BloodstalkUse)overrides[4];
                    GluttonyStrategy = (GluttonyUse)overrides[5];
                    EnshroudStrategy = (EnshroudUse)overrides[6];
                    CommunioUse = (OffensiveAbilityUse)overrides[5];
                    SpecialActionUse = (SpecialAction)overrides[7];
                    TrueNorthStrategy = (TrueNorthUse)overrides[8];
                }
                else
                {
                    GaugeStrategy = GaugeUse.Automatic;
                    PotionStrategy = PotionUse.Manual;
                    ArcaneCircleStrategy = ArcaneCircleUse.Automatic;
                    BloodstalkStrategy = BloodstalkUse.Automatic;
                    EnshroudStrategy = EnshroudUse.Automatic;
                    GluttonyStrategy = GluttonyUse.Automatic;
                    CommunioUse = OffensiveAbilityUse.Automatic;
                    SpecialActionUse = SpecialAction.None;
                    TrueNorthStrategy = TrueNorthUse.Automatic;
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
                    AID.InfernalSlice: AID.Slice,
                    AID.Slice => state.Unlocked(AID.WaxingSlice) ? AID.WaxingSlice : AID.Slice,
                    _ => AID.Slice
                };
            }
        }

        public static AID GetNextBSAction(State state, bool aoe)
        {
            if (state.EnhancedGibbetLeft > state.GCD)
                return !aoe ? AID.Gibbet : AID.Gibbet;

            if (state.EnhancedGallowsLeft > state.GCD)
                return !aoe ? AID.Gallows : AID.Gallows;

            if (aoe)
                return AID.Guillotine;


            return !aoe ? AID.Gallows : AID.Gallows;
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

                default:
                    if (!state.TargetingEnemy)
                        return false;
                    if (soulReaver)
                        return false;

                    if (enshrouded)
                        return false;

                        if (state.SoulGauge >= 50 && state.CD(CDGroup.Gluttony) > 25 && !aoe)
                            return true;
                        if (state.SoulGauge == 100 && state.CD(CDGroup.Gluttony) > state.AnimationLock && !aoe)
                            return true;
                  
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

                default:
                    if (!state.TargetingEnemy)
                        return false;
                    if (soulReaver)
                        return false;

                    if (enshrouded)
                        return false;

                    if (state.SoulGauge >= 50 && state.CD(CDGroup.Gluttony) > 25 && aoe)
                        return true;
                    if (state.SoulGauge == 100 && state.CD(CDGroup.Gluttony) > state.AnimationLock && aoe)
                        return true;

                    return false;
            }
        }

        public static bool ShouldUseGluttony(State state, Strategy strategy)
        {
            bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
            bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
            switch (strategy.GluttonyStrategy)
            {
                case Strategy.GluttonyUse.Delay:
                    return false;

                default:
                    if (!state.TargetingEnemy)
                        return false;
                    if (soulReaver)
                        return false;

                    if (enshrouded)
                        return false;

                    if (strategy.CombatTimer > 15 && state.CD(CDGroup.Gluttony) < state.AnimationLock)
                    {
                        if (state.SoulGauge >= 50)
                            return true;
                    }

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
                    if (state.ShroudGauge >= 50 && state.CD(CDGroup.Enshroud) < state.GCD)
                         return true;
                    return false;

                default:
                    if (!state.TargetingEnemy)
                        return false;
                    if (soulReaver)
                        return false;

                    if (enshrouded)
                        return false;
                    if (state.ArcaneCircleLeft > state.AnimationLock && state.ShroudGauge >= 50 && state.CD(CDGroup.Enshroud) < state.GCD)
                        return true;
                    if ((state.CD(CDGroup.ArcaneCircle) < 6 || state.CD(CDGroup.ArcaneCircle) > 60) && state.ShroudGauge >= 50 && state.CD(CDGroup.Enshroud) < state.GCD)
                        return true;

                    return false;
            }
        }

        public static bool ShouldUseArcaneCircle(State state, Strategy strategy)
        {
            bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
            bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
            switch (strategy.ArcaneCircleStrategy)
            {
                case Strategy.ArcaneCircleUse.Delay:
                    return false;

                case Strategy.ArcaneCircleUse.Force:
                    return true;

                default:
                    if (!state.TargetingEnemy)
                        return false;
                    if (soulReaver)
                        return false;

                    if (state.CD(CDGroup.ArcaneCircle) < state.GCD && enshrouded && state.TargetDeathDesignLeft > 30)
                        return true;
                    if (state.CD(CDGroup.ArcaneCircle) < state.GCD && state.ShroudGauge < 50 && !enshrouded)
                        return true;
                    return false;
            }
        }

        public static bool ShouldUsePotion(State state, Strategy strategy) => strategy.PotionStrategy switch
        {
            Strategy.PotionUse.Manual => false,
            Strategy.PotionUse.Immediate => (state.CD(CDGroup.ArcaneCircle) < state.GCD && state.EnshroudedLeft > state.AnimationLock && state.TargetDeathDesignLeft > 30) || (state.CD(CDGroup.ArcaneCircle) > state.GCD && state.CD(CDGroup.SoulSlice) > state.GCD && strategy.CombatTimer < 10),
            Strategy.PotionUse.DelayUntilRaidBuffs => state.ArcaneCircleLeft > 0 && state.RaidBuffsLeft > 0,
            Strategy.PotionUse.Force => true,
            _ => false
        };

        public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
        {
            if (strategy.UseAOERotation)
                return default;

            if (state.Unlocked(AID.Gibbet) && state.SoulReaverLeft > state.GCD && !strategy.UseAOERotation)
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
                    if (state.TrueNorthLeft > state.AnimationLock)
                        return false;
                    if (GetNextPositional(state, strategy).Item2)
                        return true;
                    return false;
            }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
        {
            bool plentifulReady = state.Unlocked(AID.PlentifulHarvest) && state.ImmortalSacrificeLeft > state.AnimationLock;
            bool soulReaver = state.Unlocked(AID.BloodStalk) && state.SoulReaverLeft > state.AnimationLock;
            bool enshrouded = state.Unlocked(AID.Enshroud) && state.EnshroudedLeft > state.AnimationLock;
            // prepull
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < -1.7f)
                return AID.None;

            if (strategy.GaugeStrategy == Strategy.GaugeUse.HarpeorHarvestMoonIfNotInMelee && state.RangeToTarget > 3 && strategy.CombatTimer < 0)
                return AID.Harpe;
            if (strategy.GaugeStrategy == Strategy.GaugeUse.HarpeorHarvestMoonIfNotInMelee && state.SoulSowLeft < state.GCD && state.RangeToTarget > 3 && strategy.CombatTimer > 0)
                return AID.HarvestMoon;
            if (strategy.GaugeStrategy == Strategy.GaugeUse.HarpeorHarvestMoonIfNotInMelee && state.SoulSowLeft > state.GCD && state.RangeToTarget > 3 && strategy.CombatTimer > 0)
                return AID.Harpe;
            if (strategy.GaugeStrategy == Strategy.GaugeUse.ForceExtendDD && state.Unlocked(AID.ShadowofDeath) && !soulReaver)
                return aoe ? AID.WhorlofDeath : AID.ShadowofDeath;
            if (strategy.GaugeStrategy == Strategy.GaugeUse.PenultimateComboThenSpend && state.ComboLastMove != AID.WaxingSlice && state.ComboLastMove != AID.NightmareScythe && (state.ComboLastMove != AID.Slice || state.SoulGauge <= 90) && !soulReaver)
                return aoe ? AID.NightmareScythe : state.ComboLastMove == AID.Slice ? AID.WaxingSlice : AID.Slice;

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

            if ((ShouldUseGluttony(state, strategy) && !enshrouded && state.TargetDeathDesignLeft < state.GCD + 5) || (ShouldUseBloodstalk(state, strategy, aoe) && !enshrouded && state.TargetDeathDesignLeft < state.GCD + 2.5))
                return AID.ShadowofDeath;

            if (enshrouded && !aoe)
            {
                if (state.CD(CDGroup.ArcaneCircle) < 6.5 && state.EnshroudedLeft > 25)
                    return AID.ShadowofDeath;
                if (state.Unlocked(AID.Communio) && state.LemureShroudCount is 1 && state.VoidShroudCount is 0)
                    return AID.Communio;
                if (state.Unlocked(AID.LemuresSlice) && state.VoidShroudCount >= 2)
                    return AID.LemuresSlice;
                if (state.EnhancedVoidReapingLeft > state.AnimationLock)
                    return AID.VoidReaping;
                if (state.EnhancedCrossReapingLeft > state.AnimationLock)
                    return AID.CrossReaping;

                    return AID.CrossReaping;
            }
            if (enshrouded && aoe)
            {
                if (state.CD(CDGroup.ArcaneCircle) < state.GCD + 10)
                    return AID.WhorlofDeath;
                if (state.Unlocked(AID.Communio) && state.LemureShroudCount is 1 && state.VoidShroudCount is 0)
                    return AID.Communio;

                return AID.CrossReaping;
            }
            if (plentifulReady && state.BloodsownCircleLeft < state.GCD && !soulReaver && !enshrouded)
                return AID.PlentifulHarvest;
            
            if (state.SoulReaverLeft > state.GCD)
                return GetNextBSAction(state, aoe);

            if (state.SoulGauge <= 50 && state.CD(CDGroup.SoulSlice) - 30 < state.GCD && !enshrouded && !soulReaver && aoe)
                return AID.SoulScythe;

            if (state.SoulGauge <= 50 && state.CD(CDGroup.SoulSlice) - 30 < state.GCD && !enshrouded && !soulReaver && !aoe)
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
            if (ShouldUseTrueNorth(state, strategy) && state.CanWeave(CDGroup.TrueNorth - 45, 0.6f, deadline) && !aoe)
                return ActionID.MakeSpell(AID.TrueNorth);
            if (ShouldUseEnshroud(state, strategy) && state.CanWeave(CDGroup.Enshroud, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Enshroud);
            if (ShouldUseArcaneCircle(state, strategy) && state.CanWeave(CDGroup.ArcaneCircle, 0.6f, deadline))
                return ActionID.MakeSpell(AID.ArcaneCircle);
            if (state.VoidShroudCount >= 2 && state.CanWeave(CDGroup.LemuresSlice, 0.6f, deadline) && !aoe)
                return ActionID.MakeSpell(AID.LemuresSlice);
            if (state.VoidShroudCount >= 2 && state.CanWeave(CDGroup.LemuresSlice, 0.6f, deadline) && aoe)
                return ActionID.MakeSpell(AID.LemuresScythe);
            if (ShouldUseGluttony(state, strategy) && state.CanWeave(CDGroup.Gluttony, 0.6f, deadline) && !enshrouded && state.TargetDeathDesignLeft > state.GCD + 5)
                return ActionID.MakeSpell(AID.Gluttony);
            if (ShouldUseBloodstalk(state, strategy, aoe) && state.CanWeave(CDGroup.BloodStalk, 0.6f, deadline) && !enshrouded && state.TargetDeathDesignLeft > state.GCD + 2.5)
                return ActionID.MakeSpell(state.Beststalk);
            if (ShouldUseGrimSwathe(state, strategy, aoe) && state.CanWeave(CDGroup.BloodStalk, 0.6f, deadline) && !enshrouded && state.TargetDeathDesignLeft > state.GCD + 2.5)
                return ActionID.MakeSpell(AID.GrimSwathe);

            if (ShouldUsePotion(state, strategy) && state.CanWeave(state.PotionCD, 1.1f, deadline))
                return CommonDefinitions.IDPotionStr;

            return new();
        }
    }
}
