

using System;
using System.Collections.Generic;
using BossMod.ReplayAnalysis;
using Dalamud.Game.ClientState.JobGauge.Enums;
using SharpDX.Direct3D11;

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
            public float GCDTime;
            public bool HasIceSen;
            public bool HasMoonSen;
            public bool HasFlowerSen;

            // for action selection during meikyo if both combo enders are usable.
            // doesn't check whether you're in melee range or not
            public Positional ClosestPositional;

            public int SenCount => (HasIceSen ? 1 : 0) + (HasMoonSen ? 1 : 0) + (HasFlowerSen ? 1 : 0);

            public bool HasCombatBuffs => FukaLeft > GCD && FugetsuLeft > GCD;
            public bool InCombo => ComboLastMove is AID.Fuko or AID.Fuga or AID.Hakaze or AID.Jinpu or AID.Shifu;

            public float NextMeikyoCharge => MathF.Max(0, CD(CDGroup.MeikyoShisui) - (Unlocked(TraitID.EnhancedMeikyoShisui) ? 55 : 0));
            public float NextTsubameCharge => MathF.Max(0, CD(CDGroup.Tsubame) - (Unlocked(TraitID.EnhancedTsubame) ? 60 : 0));

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level);

            public AID ComboLastMove => (AID)ComboLastAction;

            public AID AOEStarter => Unlocked(AID.Fuko) ? AID.Fuko : AID.Fuga;

            public override string ToString()
            {
                var senReadable = new List<string>();
                if (HasIceSen) senReadable.Add("Ice");
                if (HasMoonSen) senReadable.Add("Moon");
                if (HasFlowerSen) senReadable.Add("Flower");

                return $"Sen=[{string.Join(",", senReadable)}], K={Kenki}, M={MeditationStacks}, Kae={Kaeshi}, TCD={CD(CDGroup.Tsubame)}, MCD={CD(CDGroup.MeikyoShisui)}, Fuka={FukaLeft:f3}, Fugetsu={FugetsuLeft:f3}, TN={TrueNorthLeft:f3}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        private static AID NextKaeshi(State state)
        {
            if (state.Kaeshi == Kaeshi.NAMIKIRI) {
                // namikiri is not tied to tsubame cooldown
                return AID.KaeshiNamikiri;
            } else if (state.NextTsubameCharge <= state.GCD) {
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

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f)
                return AID.None;

            // fallback 1: out of range for ogi
            // (enpi is the only gcd that doesn't cancel tsubame)
            if (CanEnpi(state, strategy) && state.RangeToTarget > 8)
                return AID.Enpi;

            // we can't save kaeshi across GCDs, weaponskills break the combo, so always use them even unbuffed
            var k = NextKaeshi(state);
            if (k != AID.None)
                return k;

            // ogi checks
            if (state.OgiNamikiriLeft > 0 &&
                // missed window, panic use
                (state.OgiNamikiriLeft < state.GCDTime ||
                    // buffed up
                    (state.HasCombatBuffs
                    // meikyo charge won't be lost while we use ogi and kaeshi
                    && (state.MeikyoLeft == 0 || state.MeikyoLeft >= (state.GCD + state.GCDTime * 2))
                    // higanbana is already applied (in opener)
                    && !ShouldRefreshHiganbana(state, strategy))))
                        return AID.OgiNamikiri;

            // fallback 2: out of range for iaijutsu
            if (CanEnpi(state, strategy) && state.RangeToTarget > 6)
                return AID.Enpi;

            // midare is always worth it even if unbuffed
            if (state.SenCount == 3)
                return AID.MidareSetsugekka;

            // iaijutsu checks
            if (state.HasCombatBuffs) {
                if (state.SenCount == 1 && state.Unlocked(AID.Higanbana) && ShouldRefreshHiganbana(state, strategy))
                    return AID.Higanbana;

                if (strategy.UseAOERotation && state.SenCount == 2 && state.Unlocked(AID.TenkaGoken))
                    return AID.TenkaGoken;
            }

            // fallback 3: out of range for weaponskills
            if (CanEnpi(state, strategy) && state.RangeToTarget > 3)
                return AID.Enpi;

            if (state.MeikyoLeft > state.GCD) {
                if (strategy.UseAOERotation) {
                    if (!state.HasMoonSen) return AID.Mangetsu;
                    if (!state.HasFlowerSen) return AID.Oka;
                } else
                    return GetMeikyoPositional(state).Action;
            }

            if (state.ComboLastMove == AID.Jinpu && state.Unlocked(AID.Gekko))
                return AID.Gekko;
            if (state.ComboLastMove == AID.Shifu && state.Unlocked(AID.Kasha))
                return AID.Kasha;

            if (state.ComboLastMove == state.AOEStarter) {
                if (state.Unlocked(AID.Oka) && state.FukaLeft <= state.FugetsuLeft)
                    return AID.Oka;
                if (state.Unlocked(AID.Mangetsu) && state.FugetsuLeft <= state.FukaLeft)
                    return AID.Mangetsu;
            }

            if (state.ComboLastMove == AID.Hakaze) {
                var aid = GetHakazeCombo(state);
                if (aid != AID.None) return aid;
            }

            return strategy.UseAOERotation ? state.AOEStarter : AID.Hakaze;
        }

        private static AID GetHakazeCombo(State state)
        {
            // refresh buffs if they are about to expire
            if (state.Unlocked(AID.Shifu) && state.FukaLeft < state.GCDTime * 3)
                return AID.Shifu;
            if (state.Unlocked(AID.Jinpu) && state.FugetsuLeft < state.GCDTime * 3)
                return AID.Jinpu;

            // otherwise, get ice sen so we can use meikyo
            if (state.Unlocked(AID.Yukikaze) && !state.HasIceSen)
                return AID.Yukikaze;

            // if we have ice or are <49, just refresh the buff that runs out first
            if (state.Unlocked(AID.Shifu)
                && !state.HasFlowerSen
                && state.FugetsuLeft >= state.FukaLeft)
                return AID.Shifu;

            if (state.Unlocked(AID.Jinpu) && !state.HasMoonSen)
                return AID.Jinpu;

            return AID.None;
        }

        // range checked at call site rather than here since our different options
        // (ogi, iaijutsu, weaponskills) have different ranges
        private static bool CanEnpi(State state, Strategy strategy)
        {
            return strategy.EnpiStrategy switch
            {
                Strategy.EnpiUse.Automatic => state.Unlocked(AID.Enpi) && state.EnhancedEnpiLeft > state.GCD,
                Strategy.EnpiUse.Ranged => state.Unlocked(AID.Enpi),
                Strategy.EnpiUse.Never or _ => false,
            };
        }

        private static (AID Action, bool Imminent) GetMeikyoPositional(State state) {
            if (!state.HasMoonSen && !state.HasFlowerSen) {
                return state.ClosestPositional switch
                {
                    Positional.Flank => (AID.Kasha, false),
                    Positional.Rear => (AID.Gekko, false),
                    _ => (AID.Kasha, true) // flank is closer
                };;
            }

            if (!state.HasFlowerSen) return (AID.Kasha, true);
            if (!state.HasMoonSen) return (AID.Gekko, true);
            if (!state.HasIceSen) return (AID.Yukikaze, true);

            return (AID.None, false);
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f) {
                if (strategy.CombatTimer > -9 && state.MeikyoLeft == 0)
                    return ActionID.MakeSpell(AID.MeikyoShisui);
                if (strategy.CombatTimer > -5 && state.TrueNorthLeft == 0)
                    return ActionID.MakeSpell(AID.TrueNorth);

                return new();
            }

            if (strategy.DashStrategy == Strategy.DashUse.UseOutsideMelee
                && state.RangeToTarget > 3
                && state.Unlocked(AID.HissatsuGyoten)
                && state.Kenki >= 10
                && state.CanWeave(CDGroup.HissatsuGyoten, 0.6f, deadline))
                return ActionID.MakeSpell(AID.HissatsuGyoten);

            // buffs have expired, but sen is still in gauge - this should usually be avoided if possible
            // but it can happen if a boss goes untargetable and the raidplan doesn't account for it
            // we have to re-apply buffs which will generate these same sen again, so cash it out to prevent overwrite
            if ((state.HasFlowerSen && state.FukaLeft < state.GCD
                    || state.HasMoonSen && state.FugetsuLeft < state.GCD)
                && state.SenCount < 3 // always use midare
                && state.Unlocked(AID.Hagakure)
                && state.CanWeave(CDGroup.Hagakure, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Hagakure);

            if (CanUseMeikyo(state, strategy) && state.CanWeave(state.NextMeikyoCharge, 0.6f, deadline))
                return ActionID.MakeSpell(AID.MeikyoShisui);

            // wait for combat buffs before ikishoten
            // thebalance opener does this, not sure if it's mandatory or what makes it optimal
            // but we follow it to be safe
            if (state.HasCombatBuffs) {
                if (state.Unlocked(AID.Ikishoten)
                    && state.Kenki <= 50 // prevent overcap
                    && state.CanWeave(CDGroup.Ikishoten, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Ikishoten);

                if (state.MeditationStacks == 3 && state.CanWeave(CDGroup.Shoha, 0.6f, deadline))
                    return ActionID.MakeSpell(strategy.UseAOERotation ? AID.Shoha2 : AID.Shoha);
            }

            if (strategy.DashStrategy == Strategy.DashUse.Automatic
                && state.RaidBuffsLeft > state.AnimationLock
                && state.Unlocked(AID.HissatsuGyoten)
                && state.Kenki >= 10
                && (!state.Unlocked(AID.HissatsuSenei) || state.CD(CDGroup.HissatsuSenei) > state.RaidBuffsLeft))
                return ActionID.MakeSpell(AID.HissatsuGyoten);

            if (CanUseKenki(state, strategy)) {
                if (strategy.UseAOERotation) {
                    // 120s cooldown
                    if (state.Unlocked(AID.HissatsuGuren)
                        && state.HasCombatBuffs
                        && state.CanWeave(CDGroup.HissatsuSenei, 0.6f, deadline))
                        return ActionID.MakeSpell(AID.HissatsuGuren);

                    // use on cooldown
                    if (state.Unlocked(AID.HissatsuKyuten) && state.CanWeave(deadline))
                        return ActionID.MakeSpell(AID.HissatsuKyuten);

                    // below level 62 kyuten is not unlocked, use single target instead
                    if (state.Unlocked(AID.HissatsuShinten) && state.CanWeave(deadline))
                        return ActionID.MakeSpell(AID.HissatsuShinten);
                } else {
                    // 120s cooldown
                    if (state.Unlocked(AID.HissatsuGuren)
                        && state.HasCombatBuffs
                        && state.CanWeave(CDGroup.HissatsuSenei, 0.6f, deadline))
                    {
                        // senei is unlocked at 72
                        if (state.Unlocked(AID.HissatsuSenei)) return ActionID.MakeSpell(AID.HissatsuSenei);
                        // otherwise use single target guren
                        return ActionID.MakeSpell(AID.HissatsuGuren);
                    }

                    // use on cooldown
                    if (state.Unlocked(AID.HissatsuShinten) && state.CanWeave(deadline))
                        return ActionID.MakeSpell(AID.HissatsuShinten);
                }
            }

            return new();
        }

        private static bool ShouldRefreshHiganbana(State state, Strategy strategy)
        {
            // higanbana needs to tick for 45 out of 60 seconds to be a dps gain, but it also
            // gives a meditation stack, so use it to get shoha even if the target is dying
            if (strategy.FightEndIn > 0 && strategy.FightEndIn <= 45 + state.GCD)
                return state.MeditationStacks == 2;

            // TODO: use actual GCD length once we have it
            return state.TargetHiganbanaLeft < (state.GCDTime * 3) + state.GCD;
        }

        private static bool CanUseKenki(State state, Strategy strategy)
        {
            var kenkiThreshold = strategy.KenkiStrategy switch
            {
                Strategy.KenkiSpend.All => 25,
                Strategy.KenkiSpend.Most => 35,
                Strategy.KenkiSpend.Never => 0,
                _ => throw new NotImplementedException(),
            };
            if (kenkiThreshold == 0)
                return false;

            // aoe actions build 10 gauge, ST builds 5
            var overcapLimit = strategy.UseAOERotation ? 90 : 95;
            return (state.Kenki >= kenkiThreshold && state.HasCombatBuffs) || state.Kenki >= overcapLimit;
        }

        private static bool CanUseMeikyo(State state, Strategy strategy)
        {
            if (!state.Unlocked(AID.MeikyoShisui)
                // don't overwrite
                || state.MeikyoLeft > 0
                // don't use with a tsubame followup available, wastes a GCD of buff duration
                || state.NextKaeshi != AID.None
                // don't use during combo, wastes the GCD used for the combo starter
                || state.InCombo
            ) return false;

            if (strategy.UseAOERotation)
            {
                // use unless we already have two (or three) sen, in which case it should be delayed for
                // 1-2 gcds depending on if tsubame is off cooldown
                return state.SenCount < 2;
            } else {
                // don't use it if we're about to cast higanbana
                if (state.SenCount == 1 && ShouldRefreshHiganbana(state, strategy)) return false;

                // in ST, meikyo should never be used to generate ice sen, yukikaze is lower potency
                return state.HasIceSen && state.SenCount < 3;
            }
        }

        public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
        {
            if (strategy.UseAOERotation)
                return default;

            if (state.MeikyoLeft > state.GCD)
                return GetMeikyoPositional(state) switch
                {
                    (AID.Gekko, var imminent) => (Positional.Rear, imminent),
                    (AID.Kasha, var imminent) => (Positional.Flank, imminent),
                    _ => default
                };

            if (state.ComboLastMove == AID.Jinpu && state.Unlocked(AID.Gekko))
                return (Positional.Rear, true);

            if (state.ComboLastMove == AID.Shifu && state.Unlocked(AID.Kasha))
                return (Positional.Flank, true);

            if (state.ComboLastMove == AID.Hakaze) {
                var predicted = GetHakazeCombo(state);
                // TODO: DRY
                if (predicted == AID.Jinpu && state.Unlocked(AID.Gekko))
                    return (Positional.Rear, false);

                if (predicted == AID.Shifu && state.Unlocked(AID.Kasha))
                    return (Positional.Flank, false);
            }

            return default;
        }

        public class Strategy : CommonRotation.Strategy
        {
            public enum KenkiSpend : uint
            {
                [PropertyDisplay("Reserve 10 kenki for mobility skills")]
                Most = 0, // reserve 10 for forward and backwards dash abilities
                [PropertyDisplay("Use kenki gauge at 25")]
                All = 1, // or don't
                [PropertyDisplay("Don't auto-use kenki at all")]
                Never = 2
            }

            public KenkiSpend KenkiStrategy;

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
                [PropertyDisplay("Never automatically use")]
                Never = 1,
                [PropertyDisplay("Use as a gap closer if outside melee range")]
                UseOutsideMelee = 2
            }
            public DashUse DashStrategy;

            public bool UseAOERotation;
        }
    }
}
