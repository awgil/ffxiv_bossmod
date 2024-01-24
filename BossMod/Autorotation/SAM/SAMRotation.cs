// CONTRIB: made by xan, not checked
using System.Collections.Generic;
using Dalamud.Game.ClientState.JobGauge.Enums;

// opener -> cooldown -> odd burst -> filler -> cooldown -> even burst -> cooldown
// 0s                    60s                                120s

// opener = gekko -> kasha -> yukikaze -> even burst

// cooldown is 17 GCDs - 2x 8 GCDs for sen, plus one midare cast
// each burst window starts 1 GCD before we gain a tsubame charge
// odd min is 9 GCDs
//   midare -> kaeshi -> moon -> higanbana -> moon -> flower -> hakaze -> ice -> midare
// even is 11 GCDs
//   midare -> kaeshi -> moon -> higanbana -> ogi -> kaeshi -> moon -> flower -> hakaze -> ice -> midare
namespace BossMod.SAM
{
    public static class Rotation
    {
        public class State(float[] cooldowns) : CommonRotation.PlayerState(cooldowns)
        {
            public int MeditationStacks; // 3 max
            public int Kenki; // 100 max, changes by 5
            public Kaeshi Kaeshi; // see SAMGauge.Kaeshi
            public float FukaLeft; // 40 max
            public float FugetsuLeft; // 40 max
            public float TrueNorthLeft; // 10 max
            public float MeikyoLeft; // 15 max
            public float TargetHiganbanaLeft; // 60 max
            public float OgiNamikiriLeft; // 30 max
            public float EnhancedEnpiLeft; // 15 max
            public bool HasIceSen;
            public bool HasMoonSen;
            public bool HasFlowerSen;

            public float GCDTime; // TODO: should be moved to base state
            public float LastTsubame; // can be infinite

            // for action selection during meikyo if both combo enders are usable.
            // doesn't check whether you're in melee range or not
            public Positional ClosestPositional;

            public int SenCount =>
                (HasIceSen ? 1 : 0) + (HasMoonSen ? 1 : 0) + (HasFlowerSen ? 1 : 0);

            public float CastTime => Unlocked(TraitID.EnhancedIaijutsu) ? 1.3f : 1.8f;

            public bool HasCombatBuffs => FukaLeft > GCD && FugetsuLeft > GCD;
            public bool InCombo =>
                ComboTimeLeft > GCD
                && ComboLastMove is AID.Fuko or AID.Fuga or AID.Hakaze or AID.Jinpu or AID.Shifu;

            public float NextMeikyoCharge =>
                CD(CDGroup.MeikyoShisui) - (Unlocked(TraitID.EnhancedMeikyoShisui) ? 55 : 0);
            public float NextTsubameCharge =>
                CD(CDGroup.TsubameGaeshi) - (Unlocked(TraitID.EnhancedTsubame) ? 60 : 0);

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public AID ComboLastMove => (AID)ComboLastAction;

            public AID AOEStarter => Unlocked(AID.Fuko) ? AID.Fuko : AID.Fuga;

            public override string ToString()
            {
                var senReadable = new List<string>();
                if (HasIceSen)
                    senReadable.Add("Ice");
                if (HasMoonSen)
                    senReadable.Add("Moon");
                if (HasFlowerSen)
                    senReadable.Add("Flower");

                return $"Sen=[{string.Join(",", senReadable)}], H={TargetHiganbanaLeft}, K={Kenki}, M={MeditationStacks}, Kae={Kaeshi}, TCD={CD(CDGroup.TsubameGaeshi)}, MCD={CD(CDGroup.MeikyoShisui)}, Fuka={FukaLeft:f3}, Fugetsu={FugetsuLeft:f3}, TN={TrueNorthLeft:f3}, PotCD={PotionCD:f3}, GCDT={GCDTime:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        public class Strategy : CommonRotation.Strategy
        {
            public enum KenkiUse : uint
            {
                [PropertyDisplay(
                    "Spend all kenki when in raid buff window, otherwise prevent overcap"
                )]
                Automatic = 0,

                [PropertyDisplay("Always spend kenki at 25")]
                Force = 1,

                [PropertyDisplay("Always spend kenki at 35, reserving 10 for mobility")]
                ForceDash = 2,

                [PropertyDisplay("Forbid automatic kenki spending")]
                Never = 3
            }

            public KenkiUse KenkiStrategy;

            public enum EnpiUse : uint
            {
                [PropertyDisplay("Use when outside melee range if Enhanced Enpi is active")]
                Automatic = 0,

                [PropertyDisplay("Use when outside melee range, even if unbuffed")]
                Ranged = 1,

                [PropertyDisplay("Never automatically suse")]
                Never = 2
            }

            public EnpiUse EnpiStrategy;

            public enum DashUse : uint
            {
                [PropertyDisplay("Use as a damage skill during raid buffs")]
                Automatic = 0,

                [PropertyDisplay("Never automatically use", 0xff000080)]
                Never = 1,

                [PropertyDisplay("Use as a gap closer if outside melee range", 0xff800080)]
                UseOutsideMelee = 2
            }

            public DashUse DashStrategy;

            public enum MeikyoUse : uint
            {
                [PropertyDisplay("Use after Tsubame during Higanbana refresh")]
                Automatic = 0,

                [PropertyDisplay("Never automatically use", 0xff000080)]
                Never = 1,

                [PropertyDisplay("Force use after the next weaponskill combo ender", 0xff800080)]
                Force = 2,

                [PropertyDisplay("Force use even if a combo is in progress", 0xff008080)]
                ForceBreakCombo = 3
            }

            public MeikyoUse MeikyoStrategy;

            public enum HiganbanaUse : uint
            {
                Automatic = 0,

                [PropertyDisplay("Never use", 0xff000080)]
                Never = 1,

                [PropertyDisplay("Ignore downtime prediction", 0xff008080)]
                Eager = 2,
            }

            public HiganbanaUse HiganbanaStrategy;

            // setting this to Force will make iaijutsu get used during position-lock windows
            public OffensiveAbilityUse IaijutsuUse;

            public OffensiveAbilityUse TrueNorthUse;

            public bool UseAOERotation;

            public void ApplyStrategyOverrides(uint[] overrides)
            {
                if (overrides.Length >= 6)
                {
                    TrueNorthUse = (OffensiveAbilityUse)overrides[0];
                    IaijutsuUse = (OffensiveAbilityUse)overrides[1];
                    HiganbanaStrategy = (HiganbanaUse)overrides[2];
                    MeikyoStrategy = (MeikyoUse)overrides[3];
                    DashStrategy = (DashUse)overrides[4];
                    EnpiStrategy = (EnpiUse)overrides[5];
                    KenkiStrategy = (KenkiUse)overrides[6];
                }
                else
                {
                    TrueNorthUse = OffensiveAbilityUse.Automatic;
                    IaijutsuUse = OffensiveAbilityUse.Automatic;
                    HiganbanaStrategy = HiganbanaUse.Automatic;
                    MeikyoStrategy = MeikyoUse.Automatic;
                    DashStrategy = DashUse.Automatic;
                    EnpiStrategy = EnpiUse.Automatic;
                    KenkiStrategy = KenkiUse.Automatic;
                }
            }
        }

        private static AID ImminentKaeshi(State state)
        {
            if (state.Kaeshi == Kaeshi.NAMIKIRI)
            {
                // namikiri is not tied to tsubame cooldown
                return AID.KaeshiNamikiri;
            }
            else if (state.NextTsubameCharge <= state.GCD)
            {
                // will have tsubame on next gcd
                return state.Kaeshi switch
                {
                    Kaeshi.GOKEN => AID.KaeshiGoken,
                    Kaeshi.SETSUGEKKA => AID.KaeshiSetsugekka,
                    // higanbana is worthless
                    _ => AID.None
                };
            }

            return AID.None;
        }

        private static bool CanCast(State state, Strategy strategy)
        {
            if (strategy.IaijutsuUse == Strategy.OffensiveAbilityUse.Force)
                return true;
            if (strategy.IaijutsuUse == Strategy.OffensiveAbilityUse.Delay)
                return false;

            return strategy.ForceMovementIn >= state.GCD + state.CastTime;
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f)
                return AID.None;

            var canCast = CanCast(state, strategy);

            // fallback 1: out of range for ogi
            if (CanEnpi(state, strategy) && state.RangeToTarget > 8)
                return AID.Enpi;

            var k = ImminentKaeshi(state);
            if (k != AID.None)
                return k;

            if (
                state.OgiNamikiriLeft > 0
                && state.HasCombatBuffs
                && canCast
                && !ShouldRefreshHiganbana(state, strategy)
                && state.SenCount == 0
            )
                return AID.OgiNamikiri;

            // fallback 2: out of range for iaijutsu
            if (CanEnpi(state, strategy) && state.RangeToTarget > 6)
                return AID.Enpi;

            if (state.SenCount == 3 && canCast)
                return AID.MidareSetsugekka;

            if (state.HasCombatBuffs && canCast)
            {
                if (
                    state.SenCount == 1
                    && state.Unlocked(AID.Higanbana)
                    && ShouldRefreshHiganbana(state, strategy)
                )
                    return AID.Higanbana;

                if (
                    strategy.UseAOERotation
                    && state.SenCount == 2
                    && state.Unlocked(AID.TenkaGoken)
                )
                    return AID.TenkaGoken;
            }

            // fallback 3: out of range for weaponskills
            if (CanEnpi(state, strategy) && state.RangeToTarget > 3)
                return AID.Enpi;

            if (state.MeikyoLeft > state.GCD)
            {
                if (strategy.UseAOERotation)
                {
                    if (!state.HasMoonSen)
                        return AID.Mangetsu;
                    if (!state.HasFlowerSen)
                        return AID.Oka;
                }
                else
                    return GetMeikyoPositional(state, strategy).Action;
            }

            if (state.ComboLastMove == AID.Jinpu && state.Unlocked(AID.Gekko))
                return AID.Gekko;
            if (state.ComboLastMove == AID.Shifu && state.Unlocked(AID.Kasha))
                return AID.Kasha;

            if (state.ComboLastMove == state.AOEStarter)
            {
                if (state.Unlocked(AID.Oka) && state.FukaLeft <= state.FugetsuLeft)
                    return AID.Oka;
                if (state.Unlocked(AID.Mangetsu) && state.FugetsuLeft <= state.FukaLeft)
                    return AID.Mangetsu;
            }

            if (state.ComboLastMove == AID.Hakaze)
            {
                var aid = GetHakazeComboAction(state);
                if (aid != AID.None)
                    return aid;
            }

            return strategy.UseAOERotation ? state.AOEStarter : AID.Hakaze;
        }

        // range checked at callsite
        private static bool CanEnpi(State state, Strategy strategy)
        {
            if (strategy.UseAOERotation)
                return false;

            return strategy.EnpiStrategy switch
            {
                Strategy.EnpiUse.Automatic => state.Unlocked(AID.Enpi) && state.EnhancedEnpiLeft > state.GCD,
                Strategy.EnpiUse.Ranged => state.Unlocked(AID.Enpi),
                Strategy.EnpiUse.Never or _ => false,
            };
        }

        private static (AID Action, bool Imminent) GetMeikyoPositional(
            State state,
            Strategy strategy
        )
        {
            if (
                !state.HasMoonSen && !state.HasFlowerSen
                || (state.SenCount == 1 && ShouldRefreshHiganbana(state, strategy))
            )
            {
                if (state.TrueNorthLeft > state.GCD)
                    return (AID.Gekko, false);

                return state.ClosestPositional switch
                {
                    Positional.Flank => (AID.Kasha, false),
                    Positional.Rear => (AID.Gekko, false),
                    _ => (AID.Kasha, true) // flank is closer
                };
                ;
            }

            if (!state.HasMoonSen)
                return (AID.Gekko, true);
            if (!state.HasFlowerSen)
                return (AID.Kasha, true);
            if (!state.HasIceSen)
                return (AID.Yukikaze, true);

            // full on sen but can't cast due to a cdplan fuckup, e.g. midare planned during a forced movement mechanic
            // gotta do something
            return (state.ClosestPositional == Positional.Rear ? AID.Gekko : AID.Kasha, false);
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f)
            {
                if (strategy.CombatTimer > -9 && state.MeikyoLeft == 0)
                    return ActionID.MakeSpell(AID.MeikyoShisui);
                if (
                    strategy.CombatTimer > -5
                    && state.TrueNorthLeft == 0
                    && strategy.TrueNorthUse != Strategy.OffensiveAbilityUse.Delay
                )
                    return ActionID.MakeSpell(AID.TrueNorth);

                return new();
            }

            if (state.MeikyoLeft == 0 && state.CanWeave(state.NextMeikyoCharge, 0.6f, deadline))
            {
                if (strategy.MeikyoStrategy == Strategy.MeikyoUse.Force && !state.InCombo)
                    return ActionID.MakeSpell(AID.MeikyoShisui);
                if (strategy.MeikyoStrategy == Strategy.MeikyoUse.ForceBreakCombo)
                    return ActionID.MakeSpell(AID.MeikyoShisui);
            }

            if (
                ShouldUseTrueNorth(state, strategy)
                && state.CanWeave(state.CD(CDGroup.TrueNorth) - 45, 0.6f, deadline)
                && state.GCD < 0.800
            )
                return ActionID.MakeSpell(AID.TrueNorth);

            if (!state.TargetingEnemy)
                return default;

            if (state.MeikyoLeft == 0 && state.LastTsubame < state.GCDTime * 3)
                return ActionID.MakeSpell(AID.MeikyoShisui);

            if (state.RangeToTarget > 3 && strategy.DashStrategy == Strategy.DashUse.UseOutsideMelee)
                return ActionID.MakeSpell(AID.HissatsuGyoten);

            if (
                state.SenCount == 1
                && state.HasIceSen
                && !state.InCombo
                // cooldown alignment. TODO: adapt for 2.07 SkS, which needs 3 filler GCDs - will have to update GetNextBestGCD also
                && state.NextTsubameCharge > state.GCD + state.GCDTime * 7
                && state.NextTsubameCharge <= state.GCD + state.GCDTime * 9
                && state.CanWeave(CDGroup.Hagakure, 0.6f, deadline)
            )
                return ActionID.MakeSpell(AID.Hagakure);

            if (state.HasCombatBuffs)
            {
                if (state.CanWeave(CDGroup.Ikishoten, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Ikishoten);

                if (
                    CanUseKenki(state, strategy)
                    && state.SenCount == 0
                    && state.Unlocked(AID.HissatsuGuren)
                    && state.CanWeave(CDGroup.HissatsuGuren, 0.6f, deadline)
                )
                {
                    if (strategy.UseAOERotation || !state.Unlocked(AID.HissatsuSenei))
                        return ActionID.MakeSpell(AID.HissatsuGuren);

                    return ActionID.MakeSpell(AID.HissatsuSenei);
                }

                if (
                    CanUseKenki(state, strategy, 10)
                    && state.RaidBuffsLeft > 0
                    && state.CanWeave(CDGroup.HissatsuGyoten, 0.6f, deadline)
                    && (state.CD(CDGroup.HissatsuGuren) > state.GCDTime || state.Kenki >= 35)
                    && state.RangeToTarget <= 3
                    && strategy.DashStrategy != Strategy.DashUse.Never
                )
                    return ActionID.MakeSpell(AID.HissatsuGyoten);
            }

            if (
                CanUseKenki(state, strategy)
                && state.CanWeave(CDGroup.HissatsuShinten, 0.6f, deadline)
                && (state.CD(CDGroup.HissatsuGuren) > state.GCDTime || state.Kenki >= 50)
            )
                return ActionID.MakeSpell(AID.HissatsuShinten);

            if (state.MeditationStacks == 3 && state.CanWeave(deadline))
                return ActionID.MakeSpell(AID.Shoha);

            return new();
        }

        private static bool ShouldUseTrueNorth(State state, Strategy strategy)
        {
            if (state.TrueNorthLeft > state.AnimationLock)
                return false;
            if (strategy.TrueNorthUse == Strategy.OffensiveAbilityUse.Force)
                return true;
            if (!state.TargetingEnemy || strategy.TrueNorthUse == Strategy.OffensiveAbilityUse.Delay)
                return false;

            return strategy.NextPositionalImminent && !strategy.NextPositionalCorrect;
        }

        private static bool ShouldUseBurst(State state, Strategy strategy, float deadline)
        {
            return state.RaidBuffsLeft > deadline
                // fight will end before next window, use everything
                || strategy.RaidBuffsIn > strategy.FightEndIn
                // general combat, no module active. yolo
                || strategy.RaidBuffsIn > 9000;
        }

        private static bool ShouldRefreshHiganbana(
            State state,
            Strategy strategy,
            uint gcdsInAdvance = 0
        )
        {
            if (strategy.HiganbanaStrategy == Strategy.HiganbanaUse.Never || !state.HasCombatBuffs)
                return false;

            // force use to get shoha even if the target is dying, dot overwrite doesn't matter
            if (
                strategy.HiganbanaStrategy != Strategy.HiganbanaUse.Eager
                && strategy.FightEndIn > 0
                && (strategy.FightEndIn - state.GCD) < 45
            )
                return state.MeditationStacks == 2;

            return state.TargetHiganbanaLeft < (5 + state.GCD + state.GCDTime * gcdsInAdvance);
        }

        private static bool CanUseKenki(State state, Strategy strategy, int minCost = 25)
        {
            return strategy.KenkiStrategy switch
            {
                Strategy.KenkiUse.Automatic
                    => state.Kenki >= 90
                        || (
                            state.Kenki >= minCost
                            && ShouldUseBurst(state, strategy, state.AnimationLock)
                        ),
                Strategy.KenkiUse.Force => state.Kenki >= minCost,
                Strategy.KenkiUse.ForceDash => state.Kenki - 10 >= minCost,
                Strategy.KenkiUse.Never or _ => false,
            };
        }

        private static AID GetHakazeComboAction(State state)
        {
            // refresh buffs if they are about to expire
            if (state.Unlocked(AID.Jinpu) && state.FugetsuLeft < state.GCDTime * 2)
                return AID.Jinpu;
            if (state.Unlocked(AID.Shifu) && state.FukaLeft < state.GCDTime * 2)
                return AID.Shifu;

            // lvl 50+, all sen combos are guaranteed to be unlocked here
            if (
                state.Unlocked(AID.Yukikaze)
                && !state.HasIceSen
                &&
                // if we have one sen, and higanbana will drop below 15 after next weaponskill,
                // use non-ice combo: it gives us one extra GCD to let higanbana tick
                (
                    state.SenCount != 1
                    || state.TargetHiganbanaLeft >= state.GCDTime + state.GCD + 15
                )
            )
            {
                return AID.Yukikaze;
            }

            // if not using ice, refresh the buff that runs out first
            if (
                state.Unlocked(AID.Shifu)
                && !state.HasFlowerSen
                && state.FugetsuLeft >= state.FukaLeft
            )
                return AID.Shifu;

            if (state.Unlocked(AID.Jinpu) && !state.HasMoonSen)
                return AID.Jinpu;

            return AID.None;
        }

        public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
        {
            if (strategy.UseAOERotation)
                return default;

            if (state.MeikyoLeft > state.GCD)
                return GetMeikyoPositional(state, strategy) switch
                {
                    (AID.Gekko, var imminent) => (Positional.Rear, imminent),
                    (AID.Kasha, var imminent) => (Positional.Flank, imminent),
                    _ => default
                };

            if (state.ComboLastMove == AID.Jinpu && state.Unlocked(AID.Gekko))
                return (Positional.Rear, true);

            if (state.ComboLastMove == AID.Shifu && state.Unlocked(AID.Kasha))
                return (Positional.Flank, true);

            if (state.ComboLastMove == AID.Hakaze)
            {
                var predicted = GetHakazeComboAction(state);
                // TODO: DRY
                if (predicted == AID.Jinpu && state.Unlocked(AID.Gekko))
                    return (Positional.Rear, false);

                if (predicted == AID.Shifu && state.Unlocked(AID.Kasha))
                    return (Positional.Flank, false);
            }

            return default;
        }
    }
}
