// CONTRIB: made by lazylemo, not checked
namespace BossMod.GNB;

public static class Rotation
{
    // full state needed for determining next action
    public class State : CommonRotation.PlayerState
    {
        public int Ammo; // 0 to 100
        public int GunComboStep; // 0 to 2
        public float NoMercyLeft; // 0 if buff not up, max 20
        public bool ReadyToRip; // 0 if buff not up, max 10
        public bool ReadyToTear; // 0 if buff not up, max 10
        public bool ReadyToGouge; // 0 if buff not up, max 10
        public bool ReadyToBlast; // 0 if buff not up, max 10
        public float AuroraLeft; // 0 if buff not up, max 18
        public int NumTargetsHitByAOE;

        public int MaxCartridges;
        // upgrade paths
        public AID BestZone => Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone;
        public AID BestHeart => Unlocked(AID.HeartOfCorundum) ? AID.HeartOfCorundum : AID.HeartOfStone;
        public AID BestContinuation => ReadyToRip ? AID.JugularRip : ReadyToTear ? AID.AbdomenTear : ReadyToGouge ? AID.EyeGouge : ReadyToBlast ? AID.Hypervelocity : AID.Continuation;
        public AID BestGnash => GunComboStep == 1 ? AID.SavageClaw : GunComboStep == 2 ? AID.WickedTalon : AID.GnashingFang;
        public AID ComboLastMove => (AID)ComboLastAction;

        public State(WorldState ws) : base(ws) { }

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"ammo={Ammo}, ReadytoBlast={ReadyToBlast}, ReadytoGouge={ReadyToGouge}, ReadytoRip={ReadyToRip}, ReadytoTear={ReadyToTear}, roughdivide={CD(CDGroup.RoughDivide):f1}, CBT={ComboTimeLeft:f1}, RB={RaidBuffsLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public enum GaugeUse : uint
        {
            Automatic = 0, // spend gauge either under raid buffs or if next downtime is soon (so that next raid buff window won't cover at least 4 GCDs)

            [PropertyDisplay("Spend all gauge ASAP", 0x8000ff00)]
            Spend = 1, // spend all gauge asap, don't bother conserving

            [PropertyDisplay("Use Lightning Shot if outside melee", 0x80c08000)]
            LightningShotIfNotInMelee = 2,

            [PropertyDisplay("Use ST combo if still in ST combo, else use AOE combo", 0x80c0c000)]
            ComboFitBeforeDowntime = 3, // useful on late phases before downtime

            [PropertyDisplay("Use appropriate rotation to reach max gauge before downtime (NEEDS TESTING)", 0x80c0c000)]
            MaxGaugeBeforeDowntime = 4, // useful on late phases before downtime

            [PropertyDisplay("Use combo until second-last step, then spend gauge", 0x80400080)]
            PenultimateComboThenSpend = 5, // useful for ensuring ST extension is used right before long downtime
        }

        public enum PotionUse : uint
        {
            Manual = 0, // potion won't be used automatically

            [PropertyDisplay("Use ASAP, but delay slightly during opener", 0x8000ff00)]
            Immediate = 1,

            [PropertyDisplay("Delay until raidbuffs", 0x8000ffff)]
            DelayUntilRaidBuffs = 2,

            [PropertyDisplay("2min / 8min Potion use", 0x800000ff)]
            Special = 3,

            [PropertyDisplay("Use ASAP", 0x800000ff)]
            Force = 4,
        }

        public enum RoughDivideUse : uint
        {
            Automatic = 0, // always keep one charge reserved, use other charges under raidbuffs or prevent overcapping

            [PropertyDisplay("Forbid automatic use", 0x800000ff)]
            Forbid = 1, // forbid until window end

            [PropertyDisplay("Do not reserve charges: use all charges if under raidbuffs, otherwise use as needed to prevent overcapping", 0x8000ffff)]
            NoReserve = 2, // automatic logic, except without reserved charge

            [PropertyDisplay("Use all charges ASAP", 0x8000ff00)]
            Force = 3, // use all charges immediately, don't wait for raidbuffs

            [PropertyDisplay("Use all charges except one ASAP", 0x80ff0000)]
            ForceReserve = 4, // if 2+ charges, use immediately

            [PropertyDisplay("Reserve 1 charges, trying to prevent overcap", 0x80ffff00)]
            ReserveOne = 5, // use only if about to overcap

            [PropertyDisplay("Use as gapcloser if outside melee range", 0x80ff00ff)]
            UseOutsideMelee = 6, // use immediately if outside melee range
        }

        public enum SpecialAction : uint
        {
            None = 0, // don't use any special actions

            [PropertyDisplay("LB3", 0x8000ff00)]
            LB3 = 1, // use LB3 if available

            [PropertyDisplay("Stance ON", 0x80ff00ff)]
            StanceOn = 2, // use LB3 if available

            [PropertyDisplay("Stance OFF", 0x80c0c000)]
            StanceOff = 3, // use LB3 if available
        }

        public GaugeUse GaugeStrategy; // how are we supposed to handle gauge
        public PotionUse PotionStrategy; // how are we supposed to use potions
        public OffensiveAbilityUse NoMercyUse; // how are we supposed to use IR
        public OffensiveAbilityUse BloodFestUse;
        public OffensiveAbilityUse GnashUse; // how are we supposed to use upheaval
        public OffensiveAbilityUse ZoneUse; // how are we supposed to use upheaval
        public OffensiveAbilityUse BowUse; // how are we supposed to use PR
        public RoughDivideUse RoughDivideStrategy; // how are we supposed to use onslaught
        public SpecialAction SpecialActionUse; // any special actions we want to use
        public bool Aggressive; // if true, we use buffs and stuff at last possible moment; otherwise we make sure to keep at least 1 GCD safety net

        public override string ToString()
        {
            return $"";
        }

        // TODO: these bindings should be done by the framework...
        public void ApplyStrategyOverrides(uint[] overrides)
        {
            if (overrides.Length >= 9)
            {
                GaugeStrategy = (GaugeUse)overrides[0];
                PotionStrategy = (PotionUse)overrides[1];
                NoMercyUse = (OffensiveAbilityUse)overrides[2];
                BloodFestUse = (OffensiveAbilityUse)overrides[3];
                GnashUse = (OffensiveAbilityUse)overrides[4];
                ZoneUse = (OffensiveAbilityUse)overrides[5];
                BowUse = (OffensiveAbilityUse)overrides[6];
                RoughDivideStrategy = (RoughDivideUse)overrides[7];
                SpecialActionUse = (SpecialAction)overrides[8];
            }
            else
            {
                GaugeStrategy = GaugeUse.Automatic;
                PotionStrategy = PotionUse.Manual;
                NoMercyUse = OffensiveAbilityUse.Automatic;
                BloodFestUse = OffensiveAbilityUse.Automatic;
                GnashUse = OffensiveAbilityUse.Automatic;
                ZoneUse = OffensiveAbilityUse.Automatic;
                BowUse = OffensiveAbilityUse.Automatic;
                RoughDivideStrategy = RoughDivideUse.Automatic;
                SpecialActionUse = SpecialAction.None;
            }
        }
    }

    public static int GaugeGainedFromAction(State state, AID action) => action switch
    {
        AID.SolidBarrel or AID.DemonSlaughter => 1,
        AID.Bloodfest => state.Unlocked(TraitID.CartridgeChargeII) ? 3 : 2,
        _ => 0
    };

    public static int GetSTComboLength(AID comboLastMove) => comboLastMove switch
    {
        AID.BrutalShell => 1,
        AID.KeenEdge => 2,
        _ => 3
    };

    public static int GetAOEComboLength(AID comboLastMove) => comboLastMove == AID.DemonSlice ? 1 : 2;
    public static AID GetNextSTComboAction(AID comboLastMove, AID finisher) => comboLastMove switch
    {
        AID.BrutalShell => finisher,
        AID.KeenEdge => AID.BrutalShell,
        _ => AID.KeenEdge
    };

    public static AID GetNextBrutalShellComboAction(AID comboLastMove)
    {
        if (comboLastMove == AID.DemonSlice)
        {
            return AID.DemonSlaughter;
        }
        else if (comboLastMove == AID.BrutalShell)
        {
            return AID.SolidBarrel;
        }
        else if (comboLastMove == AID.KeenEdge)
        {
            return AID.BrutalShell;
        }

        return AID.KeenEdge;
    }

    public static AID GetNextAOEComboAction(AID comboLastMove)
    {
        if (comboLastMove == AID.DemonSlice)
        {
            return AID.DemonSlaughter;
        }
        else if (comboLastMove == AID.BrutalShell)
        {
            return AID.SolidBarrel;
        }
        else if (comboLastMove == AID.KeenEdge)
        {
            return AID.BrutalShell;
        }

        return AID.DemonSlice;
    }

    public static AID GetNextUnlockedComboAction(State state, Strategy strategy, bool aoe)
    {
        if (strategy.GaugeStrategy == Strategy.GaugeUse.ComboFitBeforeDowntime && (strategy.FightEndIn <= state.GCD + 2.5f * GetSTComboLength(state.ComboLastMove)) && state.ComboTimeLeft == 0)
            return AID.DemonSlice;

        if (aoe)
        {
            return state.ComboLastMove switch
            {
                AID.DemonSlice => state.Unlocked(AID.DemonSlaughter) ? AID.DemonSlaughter : AID.DemonSlice,
                AID.BrutalShell => state.Unlocked(AID.SolidBarrel) ? AID.SolidBarrel : AID.KeenEdge,
                AID.KeenEdge => state.Unlocked(AID.BrutalShell) ? AID.BrutalShell : AID.KeenEdge,
                _ => AID.DemonSlice
            };
        }
        else
        {
            return state.ComboLastMove switch
            {
                AID.DemonSlice => state.Unlocked(AID.DemonSlaughter) ? AID.DemonSlaughter : AID.DemonSlice,
                AID.BrutalShell => state.Unlocked(AID.SolidBarrel) ? AID.SolidBarrel : AID.KeenEdge,
                AID.KeenEdge => state.Unlocked(AID.BrutalShell) ? AID.BrutalShell : AID.KeenEdge,
                _ => AID.KeenEdge
            };
        }
    }

    public static AID GetNextAmmoAction(State state, Strategy strategy, bool aoe)
    {
        if (strategy.GaugeStrategy == Strategy.GaugeUse.Spend)
        {
            if (state.CD(CDGroup.GnashingFang) > 9 && state.Ammo >= 1)
                return AID.BurstStrike;
            if (strategy.FightEndIn < 9 && state.Ammo > 0)
                return AID.BurstStrike;
        }

        if (Service.Config.Get<GNBConfig>().Skscheck && state.Ammo == state.MaxCartridges - 1 && state.ComboLastMove == AID.BrutalShell && state.GunComboStep == 0 && state.CD(CDGroup.GnashingFang) < 2.5)
            return AID.SolidBarrel;
        if (!Service.Config.Get<GNBConfig>().Skscheck && state.Ammo == state.MaxCartridges - 1 && state.ComboLastMove == AID.BrutalShell && state.GunComboStep == 0 && state.CD(CDGroup.GnashingFang) < 2.5 && (state.CD(CDGroup.Bloodfest) > 20 && state.Unlocked(AID.Bloodfest)))
            return AID.SolidBarrel;

        if (Service.Config.Get<GNBConfig>().EarlySonicBreak && state.CD(CDGroup.NoMercy) > 40 && state.CD(CDGroup.SonicBreak) < 0.6f)
            return AID.SonicBreak;

        if (state.CD(CDGroup.NoMercy) > 17)
        {
            if (state.GunComboStep == 0 && state.Unlocked(AID.GnashingFang) && state.CD(CDGroup.GnashingFang) < 0.6f && state.Ammo >= 1 && ShouldUseGnash(state, strategy) && state.NumTargetsHitByAOE <= 3)
                return AID.GnashingFang;
        }

        if (state.NoMercyLeft > state.AnimationLock)
        {
            if (state.CD(CDGroup.SonicBreak) < 0.6f && state.Unlocked(AID.SonicBreak))
                return AID.SonicBreak;
            if (state.CD(CDGroup.DoubleDown) < 0.6f && state.Unlocked(AID.DoubleDown) && state.Ammo >= 2 && state.RangeToTarget <= 5)
                return AID.DoubleDown;
            if (!aoe && state.CD(CDGroup.DoubleDown) < state.GCD && state.CD(CDGroup.GnashingFang) > state.GCD && state.Unlocked(AID.DoubleDown) && state.Ammo == 1 && state.CD(CDGroup.Bloodfest) < 1.9)
                return AID.BurstStrike;
            if (aoe && state.CD(CDGroup.DoubleDown) < state.GCD && state.CD(CDGroup.GnashingFang) > state.GCD && state.Unlocked(AID.DoubleDown) && state.Ammo == 1 && state.CD(CDGroup.Bloodfest) < 1.9)
                return AID.FatedCircle;
            if (!aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.CD(CDGroup.DoubleDown) > state.GCD && state.CD(CDGroup.SonicBreak) > state.GCD && state.Unlocked(AID.DoubleDown) && state.GunComboStep == 0)
                return AID.BurstStrike;
            if (!aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.CD(CDGroup.SonicBreak) > state.GCD && !state.Unlocked(AID.DoubleDown) && state.GunComboStep == 0)
                return AID.BurstStrike;
            if (!aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && state.GunComboStep == 0)
                return AID.BurstStrike;
            if (!aoe && state.Ammo >= 1 && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && !state.Unlocked(AID.GnashingFang))
                return AID.BurstStrike;
            if (aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.CD(CDGroup.DoubleDown) > state.GCD && state.CD(CDGroup.SonicBreak) > state.GCD && state.Unlocked(AID.DoubleDown) && state.GunComboStep == 0)
                return AID.FatedCircle;
            if (aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.CD(CDGroup.SonicBreak) > state.GCD && !state.Unlocked(AID.DoubleDown) && state.GunComboStep == 0)
                return AID.FatedCircle;
            if (aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.Unlocked(AID.FatedCircle) && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && state.GunComboStep == 0)
                return AID.FatedCircle;
            if (aoe && state.Ammo >= 1 && state.Unlocked(AID.FatedCircle) && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && !state.Unlocked(AID.GnashingFang))
                return AID.FatedCircle;
        }

        if (state.GunComboStep > 0)
        {
            if (state.GunComboStep == 2)
                return AID.WickedTalon;
            if (state.GunComboStep == 1)
                return AID.SavageClaw;
        }

        if (state.ComboLastMove == AID.BrutalShell && state.Ammo == state.MaxCartridges && state.Unlocked(AID.BurstStrike))
        {
            return AID.BurstStrike;
        }

        if (aoe && state.ComboLastMove == AID.DemonSlice && state.Ammo == state.MaxCartridges && state.Unlocked(AID.FatedCircle))
        {
            return AID.FatedCircle;
        }

        if (strategy.GaugeStrategy == Strategy.GaugeUse.Spend && state.Ammo >= 0)
        {
            if (state.CD(CDGroup.GnashingFang) < 0.6f)
                return AID.GnashingFang;
            return AID.BurstStrike;
        }

        // single-target gauge spender
        return GetNextUnlockedComboAction(state, strategy, aoe);
    }

    public static bool ShouldSpendGauge(State state, Strategy strategy, bool aoe) => strategy.GaugeStrategy switch
    {
        Strategy.GaugeUse.Automatic or Strategy.GaugeUse.LightningShotIfNotInMelee => (state.RaidBuffsLeft > state.GCD || strategy.FightEndIn <= strategy.RaidBuffsIn + 10),
        Strategy.GaugeUse.Spend => true,
        Strategy.GaugeUse.ComboFitBeforeDowntime => strategy.FightEndIn <= state.GCD + 2.5f * ((aoe ? GetAOEComboLength(state.ComboLastMove) : GetSTComboLength(state.ComboLastMove)) - 1),
        Strategy.GaugeUse.PenultimateComboThenSpend => state.ComboLastMove is AID.BrutalShell or AID.DemonSlice,
        _ => true
    };

    public static bool ShouldUsePotion(State state, Strategy strategy) => strategy.PotionStrategy switch
    {
        Strategy.PotionUse.Manual => false,
        Strategy.PotionUse.Immediate => (!Service.Config.Get<GNBConfig>().EarlyNoMercy && state.ComboLastMove == AID.KeenEdge && state.Ammo == 0) || (state.CD(CDGroup.NoMercy) < 5.5 && state.CD(CDGroup.Bloodfest) < 15 && strategy.CombatTimer > 30) || (Service.Config.Get<GNBConfig>().EarlyNoMercy && state.CD(CDGroup.NoMercy) < 5.5 && state.CD(CDGroup.Bloodfest) < 15),
        Strategy.PotionUse.Special => state.ComboLastMove == AID.BrutalShell && state.Ammo == 3 && state.CD(CDGroup.NoMercy) < 3 && state.CD(CDGroup.Bloodfest) < 15,
        Strategy.PotionUse.Force => true,
        _ => false
    };

    public static bool ShouldUseNoMercy(State state, Strategy strategy)
    {
        if (strategy.NoMercyUse == Strategy.OffensiveAbilityUse.Delay)
        {
            return false;
        }
        else if (strategy.NoMercyUse == Strategy.OffensiveAbilityUse.Force)
        {
            return true;
        }
        else
        {
            GNBConfig gnbConfig = Service.Config.Get<GNBConfig>();
            bool isEarlyNoMercy = gnbConfig.EarlyNoMercy;

            bool isGnashingFangReady = state.CD(CDGroup.GnashingFang) < 2.5 && state.Unlocked(AID.GnashingFang);
            bool isSonicBreakReady = state.CD(CDGroup.SonicBreak) < 2.5 && state.Unlocked(AID.SonicBreak);
            bool isDoubleDownReady = state.CD(CDGroup.DoubleDown) < 2.5 && state.Unlocked(AID.DoubleDown);
            bool justusewhenever = !state.Unlocked(AID.BurstStrike) && state.TargetingEnemy && state.RangeToTarget < 5;

            bool shouldUseEarlyNoMercy = state.TargetingEnemy && ((!isEarlyNoMercy && state.ComboLastMove == AID.BrutalShell) || (isEarlyNoMercy && state.ComboLastMove == AID.KeenEdge)) && state.Unlocked(AID.Bloodfest) && strategy.CombatTimer < 10 && (state.Ammo == 0 || state.Ammo == state.MaxCartridges) && ((state.GCD < 0.8 && gnbConfig.Skscheck) || (!gnbConfig.Skscheck));

            bool shouldUseRegularNoMercy = (!gnbConfig.Skscheck
                && (isGnashingFangReady || isSonicBreakReady || isDoubleDownReady)
                && state.TargetingEnemy
                && ((state.Ammo == state.MaxCartridges)
                || (state.Ammo == state.MaxCartridges - 1 && state.ComboLastMove == AID.BrutalShell && state.CD(CDGroup.Bloodfest) > 20)
                || (state.CD(CDGroup.Bloodfest) < 15 && state.Ammo == 1 && state.Unlocked(AID.Bloodfest)))) || shouldUseEarlyNoMercy;

            bool shouldUseSksCheck = (gnbConfig.Skscheck && state.GCD < 0.8
                && (isGnashingFangReady || isSonicBreakReady || isDoubleDownReady)
                && state.TargetingEnemy
                && (state.Ammo == state.MaxCartridges
                || (state.CD(CDGroup.Bloodfest) < 15 && state.Ammo == 1 && state.Unlocked(AID.Bloodfest))
                || (state.Ammo == state.MaxCartridges - 1 && state.ComboLastMove == AID.BrutalShell && state.CD(CDGroup.Bloodfest) > 20 && state.Unlocked(AID.Bloodfest)))) || shouldUseEarlyNoMercy;

            return shouldUseRegularNoMercy || shouldUseSksCheck || justusewhenever;
        }
    }


    public static bool ShouldUseGnash(State state, Strategy strategy) => strategy.GnashUse switch
    {
        Strategy.OffensiveAbilityUse.Delay => false,
        Strategy.OffensiveAbilityUse.Force => true,
        _ => strategy.CombatTimer >= 0 && state.TargetingEnemy && state.Unlocked(AID.GnashingFang) && state.CD(CDGroup.GnashingFang) < 0.6f && state.Ammo >= 1
    };

    public static bool ShouldUseZone(State state, Strategy strategy) => strategy.ZoneUse switch
    {
        Strategy.OffensiveAbilityUse.Delay => false,
        Strategy.OffensiveAbilityUse.Force => true,
        _ => strategy.CombatTimer >= 0 && state.TargetingEnemy && state.Unlocked(AID.SonicBreak) && state.CD(CDGroup.SonicBreak) > state.AnimationLock && state.CD(CDGroup.NoMercy) > 17
    };

    public static bool ShouldUseFest(State state, Strategy strategy)
    {
        if (strategy.BloodFestUse == Strategy.OffensiveAbilityUse.Delay)
        {
            return false;
        }
        else if (strategy.BloodFestUse == Strategy.OffensiveAbilityUse.Force)
        {
            return true;
        }
        else
        {
            GNBConfig gnbConfig = Service.Config.Get<GNBConfig>();
            bool isEarlyNoMercy = gnbConfig.EarlyNoMercy;

            bool shouldUseEarlyNoMercy = state.TargetingEnemy && state.CD(CDGroup.NoMercy) < state.AnimationLock && ((!isEarlyNoMercy && state.ComboLastMove == AID.BrutalShell) || (isEarlyNoMercy && state.ComboLastMove == AID.KeenEdge)) && strategy.CombatTimer < 10 && state.Ammo == 0 && state.Unlocked(AID.Bloodfest);
            bool inNoMercy = state.NoMercyLeft > state.AnimationLock && state.Unlocked(AID.Bloodfest);
            return inNoMercy && state.Ammo == 0;
        }
    }

    public static bool ShouldUseBow(State state, Strategy strategy) => strategy.BowUse switch
    {
        Strategy.OffensiveAbilityUse.Delay => false,
        Strategy.OffensiveAbilityUse.Force => true,
        _ => strategy.CombatTimer >= 0 && state.TargetingEnemy && state.Unlocked(AID.BowShock) && state.CD(CDGroup.SonicBreak) > state.AnimationLock && state.CD(CDGroup.NoMercy) > 40
    };

    public static bool ShouldUseRoughDivide(State state, Strategy strategy)
    {
        bool OnCD = state.CD(CDGroup.NoMercy) > state.AnimationLock && state.CD(CDGroup.GnashingFang) > state.AnimationLock && state.CD(CDGroup.SonicBreak) > state.AnimationLock && state.CD(CDGroup.DoubleDown) > state.AnimationLock;
        switch (strategy.RoughDivideStrategy)
        {
            case Strategy.RoughDivideUse.Forbid:
                return false;
            case Strategy.RoughDivideUse.Force:
                return true;
            case Strategy.RoughDivideUse.ForceReserve:
                return state.CD(CDGroup.RoughDivide) <= state.AnimationLock;
            case Strategy.RoughDivideUse.ReserveOne:
                return state.CD(CDGroup.RoughDivide) <= state.GCD;
            case Strategy.RoughDivideUse.UseOutsideMelee:
                return state.RangeToTarget > 3;
            case Strategy.RoughDivideUse.NoReserve:
                return state.NoMercyLeft > state.AnimationLock && state.CD(CDGroup.RoughDivide) - 30 <= state.GCD;
            default:
                if (strategy.CombatTimer < 0)
                    return false; // don't use out of combat
                if (state.RangeToTarget > 3)
                    return false; // don't use out of melee range to prevent fucking up player's position
                if (strategy.PositionLockIn <= state.AnimationLock)
                    return false; // forbidden due to state flags
                if (OnCD && state.NoMercyLeft > state.AnimationLock)
                    return true; // delay until Gnashing Sonic and Doubledown on CD, even if overcapping charges
                float chargeCapIn = state.CD(CDGroup.RoughDivide);
                if (chargeCapIn < state.GCD + 2.5)
                    return true; // if we won't onslaught now, we risk overcapping charges
                if (strategy.RoughDivideStrategy != Strategy.RoughDivideUse.NoReserve && state.CD(CDGroup.RoughDivide) > 30 + state.AnimationLock)
                    return false; // strategy prevents us from using last charge
                if (state.RaidBuffsLeft > state.AnimationLock)
                    return true; // use now, since we're under raid buffs
                return chargeCapIn <= strategy.RaidBuffsIn; // use if we won't be able to delay until next raid buffs
        }
    }

    public static AID ChooseRotationBasedOnGauge(State state, Strategy strategy, bool aoe)
    {
        int maxGauge = state.MaxCartridges;

        // Calculate remaining gauge needed
        int remainingGauge = maxGauge - state.Ammo;

        // Calculate the time needed to complete each rotation
        float timeForSTRotation;
        float timeForAOERotation;
        float timeForSTRotationForOneCart;
        float timeForAOERotationForOneCart;

        // Calculate the time needed to gain the remaining gauge for each rotation
        timeForSTRotation = state.GCD + remainingGauge * (2.5f * GetSTComboLength(state.ComboLastMove) - 1); // Assuming SolidBarrel adds 1 gauge
        timeForAOERotation = state.GCD + remainingGauge * (2.5f * GetAOEComboLength(state.ComboLastMove) - 1); // Assuming DemonSlice adds 1 gauge

        timeForSTRotationForOneCart = state.GCD + (2.5f * GetSTComboLength(state.ComboLastMove) - 1); // Assuming SolidBarrel adds 1 gauge
        timeForAOERotationForOneCart = state.GCD + (2.5f * GetAOEComboLength(state.ComboLastMove) - 1); // Assuming DemonSlice adds 1 gauge

        // Choose the rotation with the shorter duration
        if (strategy.FightEndIn <= timeForSTRotation)
        {
            // Return the next ST GCD action
            return GetNextBrutalShellComboAction(state.ComboLastMove);
        }
        if (strategy.FightEndIn <= timeForAOERotation)
        {
            // Return the next AOE GCD action
            return GetNextAOEComboAction(state.ComboLastMove);
        }
        // Choose the rotation with the shorter duration
        if (strategy.FightEndIn <= timeForSTRotationForOneCart)
        {
            // Return the next ST GCD action
            return GetNextBrutalShellComboAction(state.ComboLastMove);
        }
        if (strategy.FightEndIn <= timeForAOERotationForOneCart)
        {
            // Return the next AOE GCD action
            return GetNextAOEComboAction(state.ComboLastMove);
        }

        // Default action
        return GetNextUnlockedComboAction(state, strategy, aoe);
    }

    public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
    {
        // prepull
        if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f)
            return AID.None;

        if (strategy.GaugeStrategy == Strategy.GaugeUse.ComboFitBeforeDowntime && (state.GCD + 2.5f * GetSTComboLength(state.ComboLastMove) <= strategy.FightEndIn) && state.NoMercyLeft < state.AnimationLock)
            return GetNextAOEComboAction(state.ComboLastMove);

        if (strategy.GaugeStrategy == Strategy.GaugeUse.LightningShotIfNotInMelee && state.RangeToTarget > 3)
            return AID.LightningShot;

        if (state.ReadyToBlast)
            return state.BestContinuation;
        if (state.ReadyToGouge)
            return state.BestContinuation;
        if (state.ReadyToTear)
            return state.BestContinuation;
        if (state.ReadyToRip)
            return state.BestContinuation;

        if (strategy.GaugeStrategy == Strategy.GaugeUse.PenultimateComboThenSpend && state.ComboLastMove != AID.BrutalShell && state.ComboLastMove != AID.DemonSlice && (state.ComboLastMove != AID.BrutalShell || state.Ammo == state.MaxCartridges) && state.GunComboStep == 0)
            return aoe ? AID.DemonSlice : state.ComboLastMove == AID.KeenEdge ? AID.BrutalShell : AID.KeenEdge;

        if (state.Ammo >= state.MaxCartridges && state.ComboLastMove == AID.BrutalShell)
            return GetNextAmmoAction(state, strategy, aoe);

        if (state.Ammo >= state.MaxCartridges && state.ComboLastMove == AID.DemonSlice)
            return GetNextAmmoAction(state, strategy, aoe);

        if (state.NoMercyLeft > state.AnimationLock)
            return GetNextAmmoAction(state, strategy, aoe);

        if (state.CD(CDGroup.GnashingFang) < 0.6f)
            return GetNextAmmoAction(state, strategy, aoe);

        if (state.GunComboStep > 0)
        {
            if (state.GunComboStep == 2)
                return AID.WickedTalon;
            if (state.GunComboStep == 1)
                return AID.SavageClaw;
        }

        if (strategy.GaugeStrategy == Strategy.GaugeUse.Spend)
            return GetNextAmmoAction(state, strategy, aoe);

        if (strategy.GaugeStrategy == Strategy.GaugeUse.MaxGaugeBeforeDowntime && state.NoMercyLeft < state.AnimationLock)
            return ChooseRotationBasedOnGauge(state, strategy, aoe);

        return GetNextUnlockedComboAction(state, strategy, aoe);
    }

    public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline, bool aoe)
    {
        bool hasContinuation = state.ReadyToBlast || state.ReadyToGouge || state.ReadyToRip || state.ReadyToTear;
        if (strategy.SpecialActionUse == Strategy.SpecialAction.LB3)
            return ActionID.MakeSpell(AID.GunmetalSoul);

        bool wantRoughDivide = state.Unlocked(AID.RoughDivide) && state.TargetingEnemy && ShouldUseRoughDivide(state, strategy);
        if (wantRoughDivide && state.RangeToTarget > 3)
            return ActionID.MakeSpell(AID.RoughDivide);

        if (ShouldUsePotion(state, strategy) && state.CanWeave(state.PotionCD, 1.1f, deadline))
            return CommonDefinitions.IDPotionStr;

        if (state.Unlocked(AID.NoMercy))
        {
            if (ShouldUseNoMercy(state, strategy) && state.CanWeave(CDGroup.NoMercy, 0.6f, deadline))
                return ActionID.MakeSpell(AID.NoMercy);
        }

        if (state.Unlocked(AID.DangerZone) && ShouldUseZone(state, strategy) && state.CanWeave(CDGroup.DangerZone, 0.6f, deadline))
            return ActionID.MakeSpell(state.BestZone);

        if (state.Unlocked(AID.BowShock) && ShouldUseBow(state, strategy) && state.CanWeave(CDGroup.BowShock, 0.6f, deadline))
            return ActionID.MakeSpell(AID.BowShock);

        if (state.ReadyToBlast && state.Unlocked(AID.Hypervelocity))
            return ActionID.MakeSpell(state.BestContinuation);
        if (state.ReadyToGouge && state.Unlocked(AID.Continuation))
            return ActionID.MakeSpell(state.BestContinuation);
        if (state.ReadyToTear && state.Unlocked(AID.Continuation))
            return ActionID.MakeSpell(state.BestContinuation);
        if (state.ReadyToRip && state.Unlocked(AID.Continuation))
            return ActionID.MakeSpell(state.BestContinuation);

        if (state.Unlocked(AID.Bloodfest) && state.CanWeave(CDGroup.Bloodfest, 0.6f, deadline) && ShouldUseFest(state, strategy))
            return ActionID.MakeSpell(AID.Bloodfest);

        if (wantRoughDivide && Service.Config.Get<GNBConfig>().NoMercyRoughDivide && state.CanWeave(state.CD(CDGroup.RoughDivide) - 28.5f, 0.6f, deadline) && state.NoMercyLeft > state.AnimationLock && state.CD(CDGroup.SonicBreak) > 5.5 && state.Unlocked(AID.BurstStrike))
            return ActionID.MakeSpell(AID.RoughDivide);

        if (wantRoughDivide && state.CanWeave(state.CD(CDGroup.RoughDivide), 0.6f, deadline) && state.Unlocked(AID.SonicBreak) && state.CD(CDGroup.SonicBreak) > 5.5)
            return ActionID.MakeSpell(AID.RoughDivide);

        if (wantRoughDivide && state.CanWeave(state.CD(CDGroup.RoughDivide), 0.6f, deadline) && !state.Unlocked(AID.SonicBreak))
            return ActionID.MakeSpell(AID.RoughDivide);

        if (strategy.SpecialActionUse == Strategy.SpecialAction.StanceOn && state.CanWeave(state.CD(CDGroup.RoyalGuard), 0.6f, deadline) && state.GunComboStep == 0 && !state.HaveTankStance)
            return ActionID.MakeSpell(AID.RoyalGuard);

        if (strategy.SpecialActionUse == Strategy.SpecialAction.StanceOff && state.CanWeave(state.CD(CDGroup.ReleaseRoyalGuard), 0.6f, deadline) && state.GunComboStep == 0 && state.HaveTankStance)
            return ActionID.MakeSpell(AID.ReleaseRoyalGuard);

        if (state.CanWeave(state.CD(CDGroup.Aurora) - 60, 0.6f, deadline) && state.Unlocked(AID.Aurora) && state.AuroraLeft < state.GCD && state.CD(CDGroup.NoMercy) > 1 && state.CD(CDGroup.GnashingFang) > 1 && state.CD(CDGroup.SonicBreak) > 1 && state.CD(CDGroup.DoubleDown) > 1)
            return ActionID.MakeSpell(AID.Aurora);

        if (state.CanWeave(state.CD(CDGroup.Aurora) - 60, 0.6f, deadline) && state.Unlocked(AID.Aurora) && !state.Unlocked(AID.DoubleDown) && state.AuroraLeft < state.GCD && state.CD(CDGroup.NoMercy) > 1 && state.CD(CDGroup.GnashingFang) > 1 && state.CD(CDGroup.SonicBreak) > 1)
            return ActionID.MakeSpell(AID.Aurora);

        if (state.CanWeave(state.CD(CDGroup.Aurora) - 60, 0.6f, deadline) && state.Unlocked(AID.Aurora) && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && state.AuroraLeft < state.GCD && state.CD(CDGroup.NoMercy) > 1 && state.CD(CDGroup.GnashingFang) > 1)
            return ActionID.MakeSpell(AID.Aurora);

        if (state.CanWeave(state.CD(CDGroup.Aurora) - 60, 0.6f, deadline) && state.Unlocked(AID.Aurora) && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && !state.Unlocked(AID.GnashingFang) && state.AuroraLeft < state.GCD && state.CD(CDGroup.NoMercy) > 1)
            return ActionID.MakeSpell(AID.Aurora);

        return new();
    }
}
