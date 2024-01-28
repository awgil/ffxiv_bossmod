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

            public int NadiCount => (HasLunar ? 1 : 0) + (HasSolar ? 1 : 0);

            public int BeastCount => BeastChakra.Count(x => x != Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE);

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
                return $"RB={RaidBuffsLeft:f1}, Demo={TargetDemolishLeft:f1}, DF={DisciplinedFistLeft:f1}, Form={Form}/{FormLeft:f1}, LFist={LeadenFistLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
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

            if (!state.Unlocked(AID.TwinSnakes)) return AID.TrueStrike;

            // any non-opo blitz GCD is demo or snakes
            if (state.PerfectBalanceLeft > state.GCD)
                return AID.TwinSnakes;

            var rofIsAligned =
                state.FireLeft > state.GCD
                || ShouldUseRoF(state, strategy, state.GCD + state.AttackGCDTime * 4);

            // during fire windows, if next GCD is demo, force refresh to align loop; we can't use a lunar PB unless
            // DF + demo are close to max duration, since DF only lasts about 7-8 GCDs and a blitz window is 5
            if (rofIsAligned && WillDemolishExpire(state, 4))
                return AID.TwinSnakes;

            // normal refresh
            if (WillDFExpire(state, 3))
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

            // any non-opo blitz GCD is demo or snakes
            if (state.PerfectBalanceLeft > state.GCD)
                return AID.Demolish;

            // normal refresh
            if (!strategy.ForbidDOTs && state.Unlocked(AID.Demolish) && WillDemolishExpire(state, 3))
                return AID.Demolish;

            return AID.SnapPunch;
        }

        public static AID GetNextComboAction(State state, Strategy strategy)
        {
            var form = GetEffectiveForm(state, strategy);
            if (form == Form.Coeurl && state.Unlocked(AID.SnapPunch))
                return GetCoeurlFormAction(state, strategy);

            if (form == Form.Raptor && state.Unlocked(AID.TrueStrike))
                return GetRaptorFormAction(state, strategy);

            return GetOpoOpoFormAction(state, strategy);
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (strategy.CombatTimer < 0)
            {
                if (state.Chakra < 5 && state.Unlocked(AID.Meditation))
                    return AID.Meditation;

                if (
                    strategy.CombatTimer > -20
                    && state.FormShiftLeft < 5
                    && state.Unlocked(AID.FormShift)
                )
                    return AID.FormShift;

                if (strategy.CombatTimer > -100)
                    return AID.None;
            }

            if (!state.TargetingEnemy)
            {
                if (state.Chakra < 5 && state.Unlocked(AID.Meditation))
                    return AID.Meditation;

                if (strategy.PreCombatFormShift && state.FormShiftLeft < 2 && state.Unlocked(AID.FormShift))
                    return AID.FormShift;

                return AID.None;
            }

            if (
                state.Unlocked(AID.SixSidedStar)
                && strategy.SSSUse == Strategy.OffensiveAbilityUse.Force
            )
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

            return GetNextComboAction(state, strategy);
        }

        public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
        {
            if (strategy.NumPointBlankAOETargets >= 3)
                return (Positional.Any, false);

            var curForm = GetEffectiveForm(state, strategy);

            var gcdsUntilCoeurl = curForm switch
            {
                Form.Coeurl => 3,
                Form.Raptor => 4,
                _ => 5
            };

            var isCastingGcd = state.AttackGCDTime - 0.500 < state.GCD;
            var formIsPending = state.FormLeft == 1000;
            // the previous form sticks around for about 200ms before being updated. this results in an off-by-one error
            // in the refresh calculation that causes an annoying flickering effect in the positionals predictor.
            // if we know a form swap is imminent, bump the predicted GCD count back.
            // if PB is active, the current "form" is updated instantly since it's based on job gauge instead of a status effect,
            // so skip the adjustment
            if (isCastingGcd && !formIsPending && state.PerfectBalanceLeft == 0)
                gcdsUntilCoeurl -= 1;

            var willDemolish =
                state.Unlocked(AID.Demolish) && WillDemolishExpire(state, gcdsUntilCoeurl);

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
                    && state.Unlocked(AID.Thunderclap)
                )
                    return ActionID.MakeSpell(AID.Thunderclap);

                return new();
            }

            if (state.GCD <= 0.800f && ShouldUseRoF(state, strategy, deadline))
                return ActionID.MakeSpell(AID.RiddleOfFire);

            if (state.Form == Form.Raptor && ShouldUsePB(state, strategy, deadline))
                return ActionID.MakeSpell(AID.PerfectBalance);

            if (ShouldUseBrotherhood(state, strategy, deadline))
                return ActionID.MakeSpell(AID.Brotherhood);

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
                    || !state.Unlocked(AID.RiddleOfFire)
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

            if (ShouldUseRoW(state, strategy, deadline))
                return ActionID.MakeSpell(AID.RiddleOfWind);

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

        private static Form GetEffectiveForm(State state, Strategy strategy)
        {
            if (NextIsSolar(state, strategy))
            {
                var canCoeurl = true;
                var canOpo = true;
                var canRaptor = true;
                foreach (var chak in state.BeastChakra)
                {
                    if (chak == BeastChakra.COEURL)
                        canCoeurl = false;
                    if (chak == BeastChakra.OPOOPO)
                        canOpo = false;
                    if (chak == BeastChakra.RAPTOR)
                        canRaptor = false;
                }

                if (canCoeurl && WillDemolishExpire(state, state.BeastCount == 2 ? 5 : 2))
                    return Form.Coeurl;

                if (canRaptor && WillDFExpire(state, 3 - state.BeastCount))
                    return Form.Raptor;

                return canOpo ? Form.OpoOpo : canRaptor ? Form.Raptor : Form.Coeurl;
            }

            if (state.FormShiftLeft > state.GCD || state.PerfectBalanceLeft > state.GCD)
                return Form.OpoOpo;

            return state.Form;
        }

        private static bool NextIsSolar(State state, Strategy strategy)
        {
            if (state.PerfectBalanceLeft <= state.GCD)
                return false;

            switch (strategy.NextNadi)
            {
                case Strategy.NadiChoice.Solar:
                    return true;
                case Strategy.NadiChoice.Lunar:
                    return false;
                default:
                    // usually we don't overcap unless forced. however, if nadi are left over due to downtime or
                    // over the course of multiple fights (e.g. delubrum savage), it's probably more dps
                    // to solar for phantom rush than it is to hold until the next buff window
                    if (WillDemolishExpire(state, 5 - state.BeastCount) || WillDFExpire(state, 5 - state.BeastCount))
                        return true;
                    // don't interrupt a solar in process, celestial revolution is always a loss
                    if (state.BeastChakra.Any(x => x != BeastChakra.NONE && x != BeastChakra.OPOOPO))
                        return true;

                    return state.HasLunar && !state.HasSolar;
            }
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

            // prevent early use in opener. DF should otherwise be up permanently excluding downtime
            return state.DisciplinedFistLeft > state.GCD;
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

            // thebalance recommends using RoW like an oGCD dot, so we use on cooldown as long as buffs have been used first
            return state.CD(CDGroup.RiddleOfFire) > 0 && state.CD(CDGroup.Brotherhood) > 0;
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
                    // opener timing mostly important as long as rof is used first, we just want to align with party buffs -
                    // the default opener is bhood after first bootshine
                    state.LeadenFistLeft == 0
                    // later uses can be asap
                    || strategy.CombatTimer > 30
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

            return state.Form == Form.Raptor
                && (
                    ShouldUseRoF(state, strategy, deadline + state.AttackGCDTime * 2)
                    || state.FireLeft > deadline + state.AttackGCDTime * 3
                );
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

        private static bool WillDemolishExpire(State state, int gcds) =>
            WillStatusExpire(state, gcds, state.TargetDemolishLeft);

        private static bool WillDFExpire(State state, int gcds) =>
            WillStatusExpire(state, gcds, state.DisciplinedFistLeft);

        private static bool WillStatusExpire(State state, int gcds, float statusDuration) =>
            statusDuration < state.GCD + (state.AttackGCDTime * gcds);
    }
}
