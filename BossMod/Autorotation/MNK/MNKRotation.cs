using System.Linq;
using Dalamud.Game.ClientState.JobGauge.Enums;
using static BossMod.CommonRotation.Strategy;

namespace BossMod.MNK
{
    public static class Rotation
    {
        public enum Form
        {
            None,
            OpoOpo,
            Raptor,
            Coeurl
        }

        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public int Chakra; // 0-5
            public BeastChakra[] BeastChakra;
            public Nadi Nadi;
            public Form Form;
            public float FormLeft; // 0 if no form, 30 max
            public float DisciplinedFistLeft; // 15 max
            public float LeadenFistLeft; // 30 max
            public float TargetDemolishLeft; // TODO: this shouldn't be here...
            public float PerfectBalanceLeft; // 20 max
            public float FormShiftLeft; // 30 max
            public float FireLeft; // 20 max
            public float TrueNorthLeft; // 10 max

            public bool HasLunar => Nadi.HasFlag(Nadi.LUNAR);
            public bool HasSolar => Nadi.HasFlag(Nadi.SOLAR);

            public int BeastCount =>
                BeastChakra.Count(
                    x => x != Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE
                );

            // upgrade paths
            public AID BestForbiddenChakra =>
                Unlocked(AID.ForbiddenChakra) ? AID.ForbiddenChakra : AID.SteelPeak;
            public AID BestEnlightenment =>
                Unlocked(AID.Enlightenment) ? AID.Enlightenment : AID.HowlingFist;
            public AID BestShadowOfTheDestroyer =>
                Unlocked(AID.ShadowOfTheDestroyer)
                    ? AID.ShadowOfTheDestroyer
                    : AID.ArmOfTheDestroyer;
            public AID BestRisingPhoenix =>
                Unlocked(AID.RisingPhoenix) ? AID.RisingPhoenix : AID.FlintStrike;
            public AID BestPhantomRush =>
                Unlocked(AID.PhantomRush) ? AID.PhantomRush : AID.TornadoKick;

            public AID BestBlitz
            {
                get
                {
                    if (BeastCount != 3)
                        return AID.MasterfulBlitz;

                    if (HasLunar && HasSolar)
                        return BestPhantomRush;

                    var bc = BeastChakra;

                    if (bc[0] == bc[1] && bc[1] == bc[2])
                        return AID.ElixirField;
                    if (bc[0] != bc[1] && bc[1] != bc[2] && bc[0] != bc[2])
                        return BestRisingPhoenix;
                    return AID.CelestialRevolution;
                }
            }

            public State(float[] cooldowns)
                : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, N={Nadi}, BC={BeastChakra}, Chakra={Chakra}, Form={Form}/{FormLeft:f1}, DFist={DisciplinedFistLeft:f1}, LFist={LeadenFistLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public int NumPointBlankAOETargets; // range 5 around self
            public int NumEnlightenmentTargets; // range 10 width 2/4 rect

            public enum NadiChoice : uint
            {
                Automatic = 0, // lunar -> solar

                [PropertyDisplay("Lunar", 0xFFDB8BCA)]
                Lunar = 1,

                [PropertyDisplay("Solar", 0xFF8EE6FA)]
                Solar = 2
            }

            public NadiChoice NextNadi;

            public OffensiveAbilityUse FireUse;
            public OffensiveAbilityUse WindUse;
            public OffensiveAbilityUse BrotherhoodUse;
            public OffensiveAbilityUse PerfectBalanceUse;
            public OffensiveAbilityUse SSSUse;
            public OffensiveAbilityUse TrueNorthUse;

            public override string ToString()
            {
                return $"AOE={NumPointBlankAOETargets}/{NumEnlightenmentTargets}, no-dots={ForbidDOTs}";
            }

            public void ApplyStrategyOverrides(uint[] overrides)
            {
                if (overrides.Length >= 7)
                {
                    TrueNorthUse = (OffensiveAbilityUse)overrides[0];
                    NextNadi = (NadiChoice)overrides[1];
                    FireUse = (OffensiveAbilityUse)overrides[2];
                    WindUse = (OffensiveAbilityUse)overrides[3];
                    BrotherhoodUse = (OffensiveAbilityUse)overrides[4];
                    PerfectBalanceUse = (OffensiveAbilityUse)overrides[5];
                    SSSUse = (OffensiveAbilityUse)overrides[6];
                }
                else
                {
                    TrueNorthUse = OffensiveAbilityUse.Automatic;
                    NextNadi = NadiChoice.Automatic;
                    FireUse = OffensiveAbilityUse.Automatic;
                    WindUse = OffensiveAbilityUse.Automatic;
                    BrotherhoodUse = OffensiveAbilityUse.Automatic;
                    PerfectBalanceUse = OffensiveAbilityUse.Automatic;
                    SSSUse = OffensiveAbilityUse.Automatic;
                }
            }
        }

        public static AID GetOpoOpoFormAction(State state, int numAOETargets)
        {
            // TODO: what should we use if form is not up?..
            if (state.Unlocked(AID.ArmOfTheDestroyer) && numAOETargets >= 3)
                return state.BestShadowOfTheDestroyer;

            if (state.Unlocked(AID.DragonKick) && state.LeadenFistLeft <= state.GCD)
                return AID.DragonKick;

            return AID.Bootshine;
        }

        public static AID GetRaptorFormAction(State state, int numAOETargets)
        {
            // TODO: low level - consider early restart...
            // TODO: better threshold for buff reapplication...
            if (state.Unlocked(AID.FourPointFury) && numAOETargets >= 3)
                return AID.FourPointFury;

            if (state.Unlocked(AID.TwinSnakes) && state.DisciplinedFistLeft < state.GCD + 7)
                return AID.TwinSnakes;

            return AID.TrueStrike;
        }

        public static AID GetCoeurlFormAction(State state, int numAOETargets, bool forbidDOTs)
        {
            // TODO: multidot support...
            // TODO: low level - consider early restart...
            // TODO: better threshold for debuff reapplication...
            if (state.Unlocked(AID.Rockbreaker) && numAOETargets >= 3)
                return AID.Rockbreaker;

            if (
                !forbidDOTs
                && state.Unlocked(AID.Demolish)
                && state.TargetDemolishLeft < state.GCD + 3
            )
                return AID.Demolish;

            return AID.SnapPunch;
        }

        public static AID GetNextComboAction(
            State state,
            int numAOETargets,
            bool forbidDOTs,
            Strategy.NadiChoice nextNadi
        )
        {
            var form = GetEffectiveForm(state, nextNadi);
            if (form == Form.Coeurl)
                return GetCoeurlFormAction(state, numAOETargets, forbidDOTs);

            if (form == Form.Raptor)
                return GetRaptorFormAction(state, numAOETargets);

            return GetOpoOpoFormAction(state, numAOETargets);
        }

        private static Form GetEffectiveForm(State state, Strategy.NadiChoice nextNadi)
        {
            if (SolarTime(state, nextNadi))
                return state.BeastCount switch
                {
                    2 => Form.Coeurl,
                    1 => Form.Raptor,
                    _ => Form.OpoOpo
                };

            return state.Form;
        }

        private static bool SolarTime(State state, Strategy.NadiChoice nextNadi)
        {
            if (state.PerfectBalanceLeft <= state.GCD)
                return false;
            return nextNadi switch
            {
                Strategy.NadiChoice.Automatic => state.HasLunar && !state.HasSolar,
                Strategy.NadiChoice.Solar => true,
                _ => false,
            };
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (strategy.CombatTimer < 0)
            {
                if (state.Chakra < 5)
                    return AID.Meditation;

                if (
                    strategy.CombatTimer > -20
                    && state.FormShiftLeft == 0
                    && state.Unlocked(AID.FormShift)
                )
                    return AID.FormShift;

                return AID.None;
            }

            if (state.Unlocked(AID.SixSidedStar) && strategy.SSSUse == OffensiveAbilityUse.Force)
                return AID.SixSidedStar;

            if (state.BestBlitz != AID.MasterfulBlitz)
                return state.BestBlitz;

            if (
                strategy.SSSUse == OffensiveAbilityUse.Automatic
                && strategy.FightEndIn > state.GCD
                && strategy.FightEndIn < state.GCD + 1.95
                && state.Unlocked(AID.SixSidedStar)
            )
                return AID.SixSidedStar;

            // TODO: L52+
            return GetNextComboAction(
                state,
                strategy.NumPointBlankAOETargets,
                strategy.ForbidDOTs,
                strategy.NextNadi
            );
        }

        public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
        {
            if (strategy.NumPointBlankAOETargets >= 3)
                return (Positional.Any, false);

            var gcdsInAdvance = GetEffectiveForm(state, strategy.NextNadi) switch
            {
                Form.Coeurl => 0,
                Form.Raptor => 1,
                _ => 2
            };
            var willDemolish =
                state.Unlocked(AID.Demolish)
                && state.TargetDemolishLeft < state.GCD + 3 + (1.94 * gcdsInAdvance);

            return (willDemolish ? Positional.Rear : Positional.Flank, gcdsInAdvance == 0);
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // TODO: potion

            if (strategy.CombatTimer < 0 && strategy.CombatTimer > -100)
            {
                if (strategy.CombatTimer > -0.2 && state.RangeToTarget > 3)
                    return ActionID.MakeSpell(AID.Thunderclap);

                return new();
            }

            if (ShouldUseRoF(state, strategy, deadline))
                return ActionID.MakeSpell(AID.RiddleOfFire);

            if (ShouldUseBrotherhood(state, strategy, deadline))
                return ActionID.MakeSpell(AID.Brotherhood);

            if (ShouldUsePB(state, strategy, deadline))
                return ActionID.MakeSpell(AID.PerfectBalance);

            if (ShouldUseRoW(state, strategy, deadline))
                return ActionID.MakeSpell(AID.RiddleOfWind);

            // 2. steel peek, if have chakra
            if (
                state.Unlocked(AID.SteelPeak)
                && state.Chakra == 5
                && state.CanWeave(CDGroup.SteelPeak, 0.6f, deadline)
                // prevent early use in opener
                && state.CD(CDGroup.RiddleOfFire) > 0
            )
            {
                // L15 Steel Peak is 180p
                // L40 Howling Fist is 100p/target => HF at 2+ targets
                // L54 Forbidden Chakra is 340p => HF at 4+ targets
                // L72 Enlightenment is 170p/target => at 2+ targets
                if (state.Unlocked(AID.Enlightenment))
                    return ActionID.MakeSpell(
                        strategy.NumEnlightenmentTargets >= 2
                            ? AID.Enlightenment
                            : AID.ForbiddenChakra
                    );
                else if (state.Unlocked(AID.ForbiddenChakra))
                    return ActionID.MakeSpell(
                        strategy.NumEnlightenmentTargets >= 4
                            ? AID.HowlingFist
                            : AID.ForbiddenChakra
                    );
                else if (state.Unlocked(AID.HowlingFist))
                    return ActionID.MakeSpell(
                        strategy.NumEnlightenmentTargets >= 2 ? AID.HowlingFist : AID.SteelPeak
                    );
                else
                    return ActionID.MakeSpell(AID.SteelPeak);
            }

            if (
                ShouldUseTrueNorth(state, strategy)
                && state.CanWeave(state.CD(CDGroup.TrueNorth) - 45, 0.6f, deadline)
                && state.GCD < 0.8
            )
                return ActionID.MakeSpell(AID.TrueNorth);

            // no suitable oGCDs...
            return new();
        }

        private static bool ShouldUseRoF(State state, Strategy strategy, float deadline)
        {
            if (
                !state.Unlocked(AID.RiddleOfFire)
                || strategy.FireUse == OffensiveAbilityUse.Delay
                || !state.CanWeave(CDGroup.RiddleOfFire, 0.6f, deadline)
            )
                return false;

            if (strategy.FireUse == OffensiveAbilityUse.Force)
                return true;

            if (state.GCD > 0.800)
                return false;

            if (strategy.CombatTimer < 30)
                // use before demolish in opener
                return state.Form == Form.Coeurl;
            else
                return state.Form == Form.OpoOpo;
        }

        private static bool ShouldUseRoW(State state, Strategy strategy, float deadline)
        {
            if (
                !state.Unlocked(AID.RiddleOfWind)
                || strategy.WindUse == OffensiveAbilityUse.Delay
                || !state.CanWeave(CDGroup.RiddleOfWind, 0.6f, deadline)
            )
                return false;

            if (strategy.WindUse == OffensiveAbilityUse.Force)
                return true;

            // thebalance recommends using RoW as an oGCD dot, so we use on cooldown as long as RoF has been used first
            return state.CD(CDGroup.RiddleOfFire) > 0;
        }

        private static bool ShouldUseBrotherhood(State state, Strategy strategy, float deadline)
        {
            if (
                !state.Unlocked(AID.Brotherhood)
                || strategy.BrotherhoodUse == OffensiveAbilityUse.Delay
                || !state.CanWeave(CDGroup.Brotherhood, 0.6f, deadline)
            )
                return false;

            if (strategy.BrotherhoodUse == OffensiveAbilityUse.Force)
                return true;

            return strategy.NumPointBlankAOETargets == 0
                && state.FireLeft > state.GCD
                && state.LeadenFistLeft == 0;
        }

        private static bool ShouldUsePB(State state, Strategy strategy, float deadline)
        {
            if (
                state.PerfectBalanceLeft > 0
                || !state.Unlocked(AID.PerfectBalance)
                || !state.CanWeave(state.CD(CDGroup.PerfectBalance) - 40, 0.6f, deadline)
                || strategy.PerfectBalanceUse == OffensiveAbilityUse.Delay
            )
                return false;

            if (strategy.PerfectBalanceUse == OffensiveAbilityUse.Force)
                return true;

            return (state.FireLeft > state.GCD || !state.Unlocked(AID.RiddleOfFire))
                && state.LeadenFistLeft == 0;
        }

        private static bool ShouldUseTrueNorth(State state, Strategy strategy)
        {
            if (
                strategy.TrueNorthUse == OffensiveAbilityUse.Delay
                || state.TrueNorthLeft > state.AnimationLock
            )
                return false;
            if (strategy.TrueNorthUse == OffensiveAbilityUse.Force)
                return true;

            return strategy.NextPositionalImminent && !strategy.NextPositionalCorrect;
        }
    }
}
