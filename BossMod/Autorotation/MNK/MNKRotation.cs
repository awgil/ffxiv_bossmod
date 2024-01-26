// CONTRIB: made by xan, not checked
using System.Linq;
using Dalamud.Game.ClientState.JobGauge.Enums;

namespace BossMod.MNK
{
    public static class Rotation
    {
        public enum Form { None, OpoOpo, Raptor, Coeurl }

        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public int Chakra; // 0-5
            public BeastChakra[] BeastChakra = [];
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
            public AID BestForbiddenChakra => Unlocked(AID.ForbiddenChakra) ? AID.ForbiddenChakra : AID.SteelPeak;
            public AID BestEnlightenment => Unlocked(AID.Enlightenment) ? AID.Enlightenment : AID.HowlingFist;
            public AID BestShadowOfTheDestroyer => Unlocked(AID.ShadowOfTheDestroyer) ? AID.ShadowOfTheDestroyer : AID.ArmOfTheDestroyer;
            public AID BestRisingPhoenix => Unlocked(AID.RisingPhoenix) ? AID.RisingPhoenix : AID.FlintStrike;
            public AID BestPhantomRush => Unlocked(AID.PhantomRush) ? AID.PhantomRush : AID.TornadoKick;

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

            public State(float[] cooldowns) : base(cooldowns) { }

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

            public bool PreCombatFormShift;

            public enum DashStrategy : uint
            {
                // only use in opener
                Automatic = 0,

                [PropertyDisplay("Forbid")]
                Forbid = 1,

                [PropertyDisplay("Use if outside melee range")]
                GapClose = 2
            }

            public DashStrategy DashUse;

            public enum NadiChoice : uint
            {
                Automatic = 0, // lunar -> solar

                [PropertyDisplay("Lunar", 0xFFDB8BCA)]
                Lunar = 1,

                [PropertyDisplay("Solar", 0xFF8EE6FA)]
                Solar = 2
            }

            public NadiChoice NextNadi;

            public enum FireStrategy : uint
            {
                Automatic = 0, // use on cooldown-ish if something is targetable

                [PropertyDisplay("Don't use")]
                Delay = 1,

                [PropertyDisplay("Force use")]
                Force = 2,

                [PropertyDisplay("Delay until Brotherhood is off cooldown")]
                DelayUntilBrotherhood = 3
            }

            public FireStrategy FireUse;
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
                    DashUse = (DashStrategy)overrides[0];
                    TrueNorthUse = (OffensiveAbilityUse)overrides[1];
                    NextNadi = (NadiChoice)overrides[2];
                    FireUse = (FireStrategy)overrides[3];
                    WindUse = (OffensiveAbilityUse)overrides[4];
                    BrotherhoodUse = (OffensiveAbilityUse)overrides[5];
                    PerfectBalanceUse = (OffensiveAbilityUse)overrides[6];
                    SSSUse = (OffensiveAbilityUse)overrides[7];
                }
                else
                {
                    DashUse = DashStrategy.Automatic;
                    TrueNorthUse = OffensiveAbilityUse.Automatic;
                    NextNadi = NadiChoice.Automatic;
                    FireUse = FireStrategy.Automatic;
                    WindUse = OffensiveAbilityUse.Automatic;
                    BrotherhoodUse = OffensiveAbilityUse.Automatic;
                    PerfectBalanceUse = OffensiveAbilityUse.Automatic;
                    SSSUse = OffensiveAbilityUse.Automatic;
                }
            }
        }

        public static AID GetOpoOpoFormAction(State state, Strategy strategy)
        {
            // TODO: what should we use if form is not up?..
            if (state.Unlocked(AID.ArmOfTheDestroyer) && strategy.NumPointBlankAOETargets >= 3)
                return state.BestShadowOfTheDestroyer;

            if (state.Unlocked(AID.DragonKick) && state.LeadenFistLeft <= state.GCD)
                return AID.DragonKick;

            return AID.Bootshine;
        }

        public static AID GetRaptorFormAction(State state, Strategy strategy)
        {
            // TODO: low level - consider early restart...
            // TODO: better threshold for buff reapplication...
            if (state.Unlocked(AID.FourPointFury) && strategy.NumPointBlankAOETargets >= 3)
                return AID.FourPointFury;

            if (state.Unlocked(AID.TwinSnakes) && state.DisciplinedFistLeft < state.GCD + 7)
                return AID.TwinSnakes;

            return AID.TrueStrike;
        }

        public static AID GetCoeurlFormAction(State state, Strategy strategy)
        {
            // TODO: multidot support...
            // TODO: low level - consider early restart...
            // TODO: better threshold for debuff reapplication...
            if (state.Unlocked(AID.Rockbreaker) && strategy.NumPointBlankAOETargets >= 3)
                return AID.Rockbreaker;

            if (
                !strategy.ForbidDOTs
                && state.Unlocked(AID.Demolish)
                && ShouldRefreshDemolish(state)
            )
                return AID.Demolish;

            return AID.SnapPunch;
        }

        public static AID GetNextComboAction(State state, Strategy strategy)
        {
            var form = GetEffectiveForm(state, strategy);
            if (form == Form.Coeurl)
                return GetCoeurlFormAction(state, strategy);

            if (form == Form.Raptor)
                return GetRaptorFormAction(state, strategy);

            return GetOpoOpoFormAction(state, strategy);
        }

        private static Form GetEffectiveForm(State state, Strategy strategy)
        {
            if (SolarTime(state, strategy))
                return state.BeastCount switch
                {
                    2 => Form.Coeurl,
                    1 => Form.Raptor,
                    _ => Form.OpoOpo
                };

            return state.FormShiftLeft > state.GCD ? Form.OpoOpo : state.Form;
        }

        private static bool SolarTime(State state, Strategy strategy)
        {
            if (state.PerfectBalanceLeft <= state.GCD)
                return false;

            return strategy.NextNadi switch
            {
                Strategy.NadiChoice.Automatic
                    => strategy.CombatTimer < 30
                        ? state.HasLunar && !state.HasSolar
                        : !(state.HasLunar || state.HasSolar),
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
                    (
                        strategy.CombatTimer > -20 && state.FormShiftLeft < 5
                        || strategy.PreCombatFormShift && state.FormShiftLeft < 2
                    ) && state.Unlocked(AID.FormShift)
                )
                    return AID.FormShift;

                if (strategy.CombatTimer > -100)
                    return AID.None;
            }

            if (!state.TargetingEnemy)
            {
                if (state.Chakra < 5 && state.Unlocked(AID.Meditation))
                    return AID.Meditation;

                return AID.None;
            }

            if (state.Unlocked(AID.SixSidedStar) && strategy.SSSUse == Strategy.OffensiveAbilityUse.Force)
                return AID.SixSidedStar;

            if (state.BestBlitz != AID.MasterfulBlitz)
                return state.BestBlitz;

            if (
                strategy.SSSUse == Strategy.OffensiveAbilityUse.Automatic
                && strategy.FightEndIn > state.GCD
                && strategy.FightEndIn < state.GCD + state.AttackGCDTime
                && state.Unlocked(AID.SixSidedStar)
            )
                return AID.SixSidedStar;

            if (state.FormShiftLeft > state.GCD && state.HasSolar && state.HasLunar)
                return AID.TwinSnakes;

            return GetNextComboAction(state, strategy);
        }

        public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
        {
            if (strategy.NumPointBlankAOETargets >= 3)
                return (Positional.Any, false);

            var curForm = GetEffectiveForm(state, strategy);

            var gcdsInAdvance = curForm switch
            {
                Form.Coeurl => 0,
                Form.Raptor => 1,
                _ => 2
            };

            var isCastingGcd = state.AttackGCDTime - 0.500 < state.GCD;
            var formIsPending = state.FormLeft == 1000;
            // the previous form sticks around for about 200ms before being updated. this results in an off-by-one error
            // in the refresh calculation that causes an annoying flickering effect in the positionals predictor.
            // if we know a form swap is imminent, bump the predicted GCD count back.
            // if PB is active, the current "form" is updated instantly since it's based on job gauge instead of a status effect,
            // so skip the adjustment
            if (isCastingGcd && !formIsPending && state.PerfectBalanceLeft == 0)
                gcdsInAdvance -= 1;

            var willDemolish =
                state.Unlocked(AID.Demolish) && ShouldRefreshDemolish(state, gcdsInAdvance);

            return (willDemolish ? Positional.Rear : Positional.Flank, curForm == Form.Coeurl);
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // TODO: potion

            if (strategy.CombatTimer < 0 && strategy.CombatTimer > -100)
            {
                if (
                    strategy.CombatTimer > -0.2
                    && state.RangeToTarget > 3
                    && strategy.DashUse != Strategy.DashStrategy.Forbid
                )
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
                && (
                    // prevent early use in opener
                    state.CD(CDGroup.RiddleOfFire) > 0
                    || strategy.FireUse == Strategy.FireStrategy.Delay
                    || strategy.FireUse == Strategy.FireStrategy.DelayUntilBrotherhood
                )
            )
            {
                // L15 Steel Peak is 180p
                // L40 Howling Fist is 100p/target => HF at 2+ targets
                // L54 Forbidden Chakra is 340p => HF at 4+ targets
                // L72 Enlightenment is 170p/target => at 2+ targets
                if (state.Unlocked(AID.Enlightenment))
                    return ActionID.MakeSpell(strategy.NumEnlightenmentTargets >= 2 ? AID.Enlightenment : AID.ForbiddenChakra);
                else if (state.Unlocked(AID.ForbiddenChakra))
                    return ActionID.MakeSpell(strategy.NumEnlightenmentTargets >= 4 ? AID.HowlingFist : AID.ForbiddenChakra);
                else if (state.Unlocked(AID.HowlingFist))
                    return ActionID.MakeSpell(strategy.NumEnlightenmentTargets >= 2 ? AID.HowlingFist : AID.SteelPeak);
                else
                    return ActionID.MakeSpell(AID.SteelPeak);
            }

            if (
                ShouldUseTrueNorth(state, strategy)
                && state.CanWeave(state.CD(CDGroup.TrueNorth) - 45, 0.6f, deadline)
            )
                return ActionID.MakeSpell(AID.TrueNorth);

            if (ShouldDash(state, strategy, deadline))
                return ActionID.MakeSpell(AID.Thunderclap);

            // no suitable oGCDs...
            return new();
        }

        private static bool ShouldDash(State state, Strategy strategy, float deadline)
        {
            if (
                state.RangeToTarget <= 3
                || !state.CanWeave(state.CD(CDGroup.Thunderclap) - 60, 0.6f, deadline)
                || strategy.DashUse == Strategy.DashStrategy.Forbid
            )
                return false;

            if (strategy.DashUse == Strategy.DashStrategy.GapClose)
                return true;

            // someone early pulled
            if (
                strategy.DashUse == Strategy.DashStrategy.Automatic
                && strategy.CombatTimer > 0
                && strategy.CombatTimer < 3
            )
                return true;

            return false;
        }

        private static bool ShouldUseRoF(State state, Strategy strategy, float deadline)
        {
            if (
                !state.Unlocked(AID.RiddleOfFire)
                || strategy.FireUse == Strategy.FireStrategy.Delay
                || !state.CanWeave(CDGroup.RiddleOfFire, 0.6f, deadline)
            )
                return false;

            if (strategy.FireUse == Strategy.FireStrategy.Force)
                return true;

            if (state.GCD > 0.800)
                return false;

            if (strategy.CombatTimer < 30)
                // use before demolish in opener
                return state.Form == Form.Coeurl;
            else
            {
                return strategy.FireUse == Strategy.FireStrategy.Automatic
                    || state.CD(CDGroup.Brotherhood) < 4;
            }
        }

        private static bool ShouldUseRoW(State state, Strategy strategy, float deadline)
        {
            if (
                !state.Unlocked(AID.RiddleOfWind)
                || strategy.WindUse == Strategy.OffensiveAbilityUse.Delay
                || !state.CanWeave(CDGroup.RiddleOfWind, 0.6f, deadline)
            )
                return false;

            if (strategy.WindUse == Strategy.OffensiveAbilityUse.Force)
                return true;

            // thebalance recommends using RoW like an oGCD dot, so we use on cooldown as long as RoF has been used first
            return state.CD(CDGroup.RiddleOfFire) > 0;
        }

        private static bool ShouldUseBrotherhood(State state, Strategy strategy, float deadline)
        {
            if (
                !state.Unlocked(AID.Brotherhood)
                || strategy.BrotherhoodUse == Strategy.OffensiveAbilityUse.Delay
                || !state.CanWeave(CDGroup.Brotherhood, 0.6f, deadline)
            )
                return false;

            if (strategy.BrotherhoodUse == Strategy.OffensiveAbilityUse.Force)
                return true;

            return strategy.NumPointBlankAOETargets < 3
                && state.FireLeft > state.GCD
                && (
                    // opener
                    state.LeadenFistLeft == 0
                    // even windows: use immediately before blitz
                    || state.BeastCount == 3
                );
        }

        private static bool ShouldUsePB(State state, Strategy strategy, float deadline)
        {
            if (
                state.PerfectBalanceLeft > 0
                || !state.Unlocked(AID.PerfectBalance)
                || !state.CanWeave(state.CD(CDGroup.PerfectBalance) - 40, 0.6f, deadline)
                || strategy.PerfectBalanceUse == Strategy.OffensiveAbilityUse.Delay
            )
                return false;

            if (strategy.PerfectBalanceUse == Strategy.OffensiveAbilityUse.Force)
                return true;

            var haveRof = state.FireLeft > state.GCD || !state.Unlocked(AID.RiddleOfFire);

            // opener. this will end up being skipped for fights with a non burst pre-phase like
            // stygimoloch lord, in which case we treat first brotherhood like any other even window
            if (strategy.CombatTimer < 30)
            {
                if (haveRof && state.LeadenFistLeft == 0)
                    return true;
            }
            else
            {
                // odd windows
                if (haveRof && state.LeadenFistLeft == 0 && state.TargetDemolishLeft > 12)
                    return true;

                // even windows
                // TODO: this should actually check ShouldUseRoF with some expected amount of lead time,
                // but that will require being able to look forward in time on the raidplan too
                if (
                    state.CD(CDGroup.RiddleOfFire) < 5
                    && strategy.FireUse != Strategy.FireStrategy.Delay
                    && state.CD(CDGroup.Brotherhood) < 15
                    && strategy.BrotherhoodUse != CommonRotation.Strategy.OffensiveAbilityUse.Delay
                    && state.LeadenFistLeft == 0
                )
                    return true;
            }

            return false;
        }

        private static bool ShouldUseTrueNorth(State state, Strategy strategy)
        {
            if (
                strategy.TrueNorthUse == Strategy.OffensiveAbilityUse.Delay
                || state.TrueNorthLeft > state.AnimationLock
            )
                return false;
            if (strategy.TrueNorthUse == Strategy.OffensiveAbilityUse.Force)
                return true;

            return strategy.NextPositionalImminent && !strategy.NextPositionalCorrect;
        }

        private static bool ShouldRefreshDemolish(State state, int gcdsInAdvance = 0) =>
            state.TargetDemolishLeft < state.GCD + (state.AttackGCDTime * (3 + gcdsInAdvance));
    }
}
