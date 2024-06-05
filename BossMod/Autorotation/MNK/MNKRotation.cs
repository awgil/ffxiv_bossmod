// CONTRIB: made by xan, not checked
using Dalamud.Game.ClientState.JobGauge.Enums;

namespace BossMod.MNK;

public static class Rotation
{
    public enum Form { None, OpoOpo, Raptor, Coeurl }

    private const float SSSApplicationDelay = 0.62f;

    // make configurable? idk? only rotation devs would care about this
    public static readonly bool Debug;

    // full state needed for determining next action
    public class State(WorldState ws) : CommonRotation.PlayerState(ws)
    {
        public int Chakra; // 0-5
        public BeastChakra[] BeastChakra = [];
        public Nadi Nadi;
        public Form Form;
        public float BlitzLeft; // 20 max
        public float FormLeft; // 0 if no form, 30 max
        public float DisciplinedFistLeft; // 15 max
        public float LeadenFistLeft; // 30 max
        public float TargetDemolishLeft; // TODO: this shouldn't be here...
        public float PerfectBalanceLeft; // 20 max
        public float FormShiftLeft; // 30 max
        public float FireLeft; // 20 max
        public float TrueNorthLeft; // 10 max
        public float LostExcellenceLeft; // 60(?) max
        public float FoPLeft; // 30 max
        public float HsacLeft; // 15 max

        public bool HasLunar => Nadi.HasFlag(Nadi.LUNAR);
        public bool HasSolar => Nadi.HasFlag(Nadi.SOLAR);
        public bool HasBothNadi => HasLunar && HasSolar;

        public bool CanFormShift => Unlocked(AID.FormShift) && PerfectBalanceLeft == 0;

        public int BeastCount => BeastChakra.Count(x => x != Dalamud.Game.ClientState.JobGauge.Enums.BeastChakra.NONE);

        public bool ForcedLunar => BeastCount > 1 && BeastChakra[0] == BeastChakra[1] && !HasBothNadi;
        public bool ForcedSolar => BeastCount > 1 && BeastChakra[0] != BeastChakra[1] && !HasBothNadi;

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

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"RB={RaidBuffsLeft:f1}, Demo={TargetDemolishLeft:f1}, DF={DisciplinedFistLeft:f1}, Blitz={BlitzLeft:f1}, Form={Form}/{FormLeft:f1}, LFist={LeadenFistLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public int NumBlitzTargets; // 5y around self
        public int NumPointBlankAOETargets; // 5y around self
        public int NumEnlightenmentTargets; // 10y/4y rect

        public bool UseAOE;

        public bool UseSTQOpener;

        public enum FormShiftStrategy : uint
        {
            [PropertyDisplay("Use if there are no targets in range")]
            Automatic = 0,
            [PropertyDisplay("Do not use")]
            Delay = 1
        }

        public FormShiftStrategy FormShiftUse;

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
            Solar = 2,
            [PropertyDisplay("Lunar (downtime)", 0xA0DB8BCA)]
            LunarDowntime = 3,
            [PropertyDisplay("Solar (downtime)", 0xA08EE6FA)]
            SolarDowntime = 4
        }

        public NadiChoice NextNadi;

        public enum FormChoice : uint
        {
            Automatic = 0,
            [PropertyDisplay("Opo-Opo", 0xFF3B34DA)]
            Opo = 1,
            [PropertyDisplay("Raptor", 0xFF38D17B)]
            Raptor = 2,
            [PropertyDisplay("Coeurl", 0xFFA264D7)]
            Coeurl = 3,
        }

        public FormChoice FormShiftForm;

        public enum FireStrategy : uint
        {
            Automatic = 0, // use on cooldown-ish if something is targetable

            [PropertyDisplay("Don't use")]
            Delay = 1,

            [PropertyDisplay("Force use")]
            Force = 2,

            [PropertyDisplay("Delay until Brotherhood is off cooldown")]
            DelayUntilBrotherhood = 3,

            [PropertyDisplay("Delay until 1 Beast Chakra is opened")]
            DelayBeast1 = 4,

            [PropertyDisplay("Delay until 2 Beast Chakra are opened")]
            DelayBeast2 = 5,

            [PropertyDisplay("Delay until 3 Beast Chakra are opened")]
            DelayBeast3 = 6
        }

        public FireStrategy FireUse;

        public enum BlitzStrategy : uint
        {
            // use when available
            Automatic = 0,
            [PropertyDisplay("Delay")]
            Delay = 1,
            [PropertyDisplay("Delay until at least two targets are in range")]
            DelayUntilMultiTarget = 2,
        }
        public BlitzStrategy BlitzUse;

        public enum DragonKickStrategy : uint
        {
            // standard rotation, use in opo-opo form to proc leaden fist
            Automatic = 0,
            [PropertyDisplay("Replace all GCDs unless Leaden Fist is active or Disciplined Fist will expire")]
            Filler = 1,
        }

        public OffensiveAbilityUse WindUse;
        public OffensiveAbilityUse BrotherhoodUse;
        public OffensiveAbilityUse TFCUse;
        public OffensiveAbilityUse MeditationUse;
        public OffensiveAbilityUse PerfectBalanceUse;
        public FormChoice PBForm1;
        public FormChoice PBForm2;
        public FormChoice PBForm3;
        public OffensiveAbilityUse SSSUse;
        public OffensiveAbilityUse TrueNorthUse;
        public OffensiveAbilityUse DisciplinedFistUse;
        public OffensiveAbilityUse DemolishUse;
        public DragonKickStrategy DragonKickUse;
        public OffensiveAbilityUse PotionUse;

        public float ActualFightEndIn => FightEndIn == 0 ? 10000f : FightEndIn;

        public override string ToString()
        {
            return $"AOE={NumPointBlankAOETargets}/{NumEnlightenmentTargets}, no-dots={ForbidDOTs}";
        }

        public void ApplyStrategyOverrides(uint[] overrides)
        {
            if (overrides.Length >= 20)
            {
                DashUse = (DashStrategy)overrides[0];
                TrueNorthUse = (OffensiveAbilityUse)overrides[1];
                DisciplinedFistUse = (OffensiveAbilityUse)overrides[2];
                DemolishUse = (OffensiveAbilityUse)overrides[3];
                NextNadi = (NadiChoice)overrides[4];
                FireUse = (FireStrategy)overrides[5];
                WindUse = (OffensiveAbilityUse)overrides[6];
                BrotherhoodUse = (OffensiveAbilityUse)overrides[7];
                TFCUse = (OffensiveAbilityUse)overrides[8];
                MeditationUse = (OffensiveAbilityUse)overrides[9];
                PerfectBalanceUse = (OffensiveAbilityUse)overrides[10];
                PBForm1 = (FormChoice)overrides[11];
                PBForm2 = (FormChoice)overrides[12];
                PBForm3 = (FormChoice)overrides[13];
                FormShiftUse = (FormShiftStrategy)overrides[14];
                FormShiftForm = (FormChoice)overrides[15];
                BlitzUse = (BlitzStrategy)overrides[16];
                DragonKickUse = (DragonKickStrategy)overrides[17];
                SSSUse = (OffensiveAbilityUse)overrides[18];
                PotionUse = (OffensiveAbilityUse)overrides[19];
            }
            else
            {
                DashUse = DashStrategy.Automatic;
                TrueNorthUse = OffensiveAbilityUse.Automatic;
                DisciplinedFistUse = OffensiveAbilityUse.Automatic;
                DemolishUse = OffensiveAbilityUse.Automatic;
                NextNadi = NadiChoice.Automatic;
                FireUse = FireStrategy.Automatic;
                WindUse = OffensiveAbilityUse.Automatic;
                BrotherhoodUse = OffensiveAbilityUse.Automatic;
                TFCUse = OffensiveAbilityUse.Automatic;
                MeditationUse = OffensiveAbilityUse.Automatic;
                PerfectBalanceUse = OffensiveAbilityUse.Automatic;
                PBForm1 = FormChoice.Automatic;
                PBForm2 = FormChoice.Automatic;
                PBForm3 = FormChoice.Automatic;
                FormShiftUse = FormShiftStrategy.Automatic;
                FormShiftForm = FormChoice.Automatic;
                BlitzUse = BlitzStrategy.Automatic;
                DragonKickUse = DragonKickStrategy.Automatic;
                SSSUse = OffensiveAbilityUse.Automatic;
                PotionUse = OffensiveAbilityUse.Automatic;
            }
        }
    }

    public static AID GetOpoOpoFormAction(State state, Strategy strategy)
    {
        // TODO: what should we use if form is not up?..
        if (state.Unlocked(AID.ArmOfTheDestroyer) && strategy.UseAOE)
            return state.BestShadowOfTheDestroyer;

        if (state.Unlocked(AID.DragonKick) && state.LeadenFistLeft <= state.GCD)
            return AID.DragonKick;

        return AID.Bootshine;
    }

    public static AID GetRaptorFormAction(State state, Strategy strategy)
    {
        // TODO: low level - consider early restart...
        // TODO: better threshold for buff reapplication...
        if (state.Unlocked(AID.FourPointFury) && strategy.UseAOE)
            return AID.FourPointFury;

        if (!state.Unlocked(AID.TwinSnakes))
            return AID.TrueStrike;

        // TODO: this is kind of a hack
        if (state.FireLeft > state.GCD && state.PerfectBalanceLeft > state.GCD)
            return AID.TwinSnakes;

        var rofIsAligned =
            state.FireLeft > state.GCD || ShouldUseRoF(state, strategy, state.GCD + state.AttackGCDTime * 4);

        // during fire windows, if next GCD is demo, force refresh to align loop; we can't use a lunar PB unless
        // DF + demo are close to max duration, since DF only lasts about 7-8 GCDs and a blitz window is 5
        if (rofIsAligned && NeedDemolishRefresh(state, strategy, 4))
            return AID.TwinSnakes;

        // force refresh if we anticipate another PB use in this buff window
        if (
            state.FireLeft >= state.GCD + state.AttackGCDTime * 3 &&
            state.CanWeave(state.CD(CDGroup.PerfectBalance) - 40, 0.6f, state.GCD + state.AttackGCDTime) &&
            state.PerfectBalanceLeft == 0 &&
            state.HasSolar
        )
            return AID.TwinSnakes;

        // normal refresh
        if (NeedDFRefresh(state, strategy, 3))
            return AID.TwinSnakes;

        return AID.TrueStrike;
    }

    public static AID GetCoeurlFormAction(State state, Strategy strategy)
    {
        // TODO: multidot support...
        // TODO: low level - consider early restart...
        // TODO: better threshold for debuff reapplication...
        if (state.Unlocked(AID.Rockbreaker) && strategy.UseAOE)
            return AID.Rockbreaker;

        // normal refresh
        if (!strategy.ForbidDOTs && state.Unlocked(AID.Demolish) && NeedDemolishRefresh(state, strategy, 3))
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
        // tradeoff here between always using meditation + form shift when not in combat ("optimal") versus only using
        // them during countdowns (mostly optimal).
        // the tradeoff is that "not in combat" includes the scenario of manually targeting an enemy you want to attack,
        // even if they're already in melee range, which incurs an annoying 3s delay
        // maybe AI mode should separately handle the out of combat form shift + meditate usage?
        if (strategy.CombatTimer is < 0 and > -100)
        {
            if (state.Chakra < 5 && state.Unlocked(AID.Meditation))
                return AID.Meditation;

            if (
                strategy.FormShiftUse == Strategy.FormShiftStrategy.Automatic
                && state.FormShiftLeft < 3
                && state.CanFormShift
            )
                return AID.FormShift;

            if (strategy.CombatTimer > -10)
            {
                // form shift on countdown. TODO: ignore Never here? don't think there's ever any reason not to use it on countdown
                if (
                    strategy.FormShiftUse == Strategy.FormShiftStrategy.Automatic
                    && strategy.CombatTimer < -9
                    && state.FormShiftLeft < 15
                    && state.Unlocked(AID.FormShift)
                )
                    return AID.FormShift;

                return AID.None;
            }
        }

        if (!HaveTarget(state, strategy))
        {
            if (state.Chakra < 5 && state.Unlocked(AID.Meditation) && strategy.MeditationUse != CommonRotation.Strategy.OffensiveAbilityUse.Delay)
                return AID.Meditation;

            if (strategy.FormShiftUse == Strategy.FormShiftStrategy.Automatic && state.CanFormShift && state.FormShiftLeft < 3)
                return AID.FormShift;

            if (strategy.NextNadi == Strategy.NadiChoice.LunarDowntime && state.BeastCount < 3 && state.PerfectBalanceLeft > 0)
                return AID.ShadowOfTheDestroyer;

            if (strategy.NextNadi == Strategy.NadiChoice.SolarDowntime && state.PerfectBalanceLeft > 0)
                return state.BeastCount switch
                {
                    0 => AID.ShadowOfTheDestroyer,
                    1 => AID.FourPointFury,
                    2 => AID.Rockbreaker,
                    _ => AID.None
                };

            return AID.None;
        }

        if (state.RangeToTarget > 3 && strategy.DashUse == Strategy.DashStrategy.GapClose && state.CD(CDGroup.Thunderclap) <= 60 && state.Unlocked(AID.Thunderclap))
            return AID.Thunderclap;

        if (state.Unlocked(AID.SixSidedStar) && strategy.SSSUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return AID.SixSidedStar;

        if (strategy.UseSTQOpener && state.LostExcellenceLeft > 0 && state.FoPLeft == 0)
            return AID.SixSidedStar;

        if (state.BestBlitz != AID.MasterfulBlitz && strategy.NumBlitzTargets > 0 && ShouldBlitz(state, strategy))
            return state.BestBlitz;

        // TODO: calculate optimal DK spam before SSS
        if (
            strategy.SSSUse == CommonRotation.Strategy.OffensiveAbilityUse.Automatic
            && strategy.ActualFightEndIn < state.GCD + state.AttackGCDTime + SSSApplicationDelay
            && state.Unlocked(AID.SixSidedStar)
        )
            return AID.SixSidedStar;

        if (state.Unlocked(AID.DragonKick) && ShouldDKSpam(state, strategy))
            return AID.DragonKick;

        return GetNextComboAction(state, strategy);
    }

    public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
    {
        if (strategy.UseAOE)
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

        var willDemolish = state.Unlocked(AID.Demolish) && NeedDemolishRefresh(state, strategy, gcdsUntilCoeurl);

        return (willDemolish ? Positional.Rear : Positional.Flank, curForm == Form.Coeurl);
    }

    public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline, float finalOGCDDeadline)
    {
        // TODO: potion

        if (strategy.CombatTimer is < 0 and > -100)
        {
            if (
                strategy.CombatTimer > -0.2
                && state.RangeToTarget > 3
                && strategy.DashUse != Strategy.DashStrategy.Forbid
                && state.Unlocked(AID.Thunderclap)
            )
                return ActionID.MakeSpell(AID.Thunderclap);

            if (strategy.PotionUse == CommonRotation.Strategy.OffensiveAbilityUse.Force && state.CanWeave(state.PotionCD, 1.1f, deadline))
                return CommonDefinitions.IDPotionStr;

            return new();
        }

        if (strategy.UseSTQOpener && HaveTarget(state, strategy))
        {
            var hsac = BozjaActionID.GetNormal(BozjaHolsterID.BannerHonoredSacrifice);
            var fop = BozjaActionID.GetNormal(BozjaHolsterID.LostFontOfPower);
            var ex = BozjaActionID.GetNormal(BozjaHolsterID.LostExcellence);

            var hsacInBag = state.BozjaHolster[(int)BozjaHolsterID.BannerHonoredSacrifice] > 0;
            var hsacSlot = state.FindDutyActionSlot(hsac, fop);
            var exSlot = state.FindDutyActionSlot(ex, fop);

            if (state.LostExcellenceLeft > 0)
            {
                if (state.HsacLeft > 0)
                {
                    if (state.FoPLeft > 0)
                    {
                        if (state.CanWeave(state.PotionCD, 0.6f, deadline))
                            return CommonDefinitions.IDPotionStr;
                    }

                    if (state.CanWeave(state.DutyActionCD(fop), 0.6f, deadline))
                        return fop;
                }

                if (state.CanWeave(state.DutyActionCD(hsac), 0.6f, deadline))
                    return hsac;

                if (hsacSlot < 0)
                    return ActionID.MakeBozjaHolster(BozjaHolsterID.BannerHonoredSacrifice, exSlot);
            }

            if (state.Form == Form.Raptor && hsacInBag && exSlot >= 0 && state.CanWeave(state.DutyActionCD(ex), 0.6f, deadline))
                return ex;
        }

        if (state.GCD <= 0.800f && ShouldUseRoF(state, strategy, deadline))
        {
            // this is checked separately here because other functions (notably ShouldUsePB) make decisions
            // based on whether RoF is expected to be off cooldown by a given time
            var shouldRoFDelayed = strategy.FireUse switch
            {
                Strategy.FireStrategy.DelayBeast1 => state.BeastCount >= 1,
                Strategy.FireStrategy.DelayBeast2 => state.BeastCount >= 2,
                Strategy.FireStrategy.DelayBeast3 => state.BeastCount == 3,
                _ => true
            };
            if (shouldRoFDelayed)
                return ActionID.MakeSpell(AID.RiddleOfFire);
        }

        if (strategy.PotionUse == CommonRotation.Strategy.OffensiveAbilityUse.Force && state.CanWeave(state.PotionCD, 1.1f, deadline))
            return CommonDefinitions.IDPotionStr;

        if (ShouldUseBrotherhood(state, strategy, deadline))
            return ActionID.MakeSpell(AID.Brotherhood);

        if (ShouldUsePB(state, strategy, deadline))
            return ActionID.MakeSpell(AID.PerfectBalance);

        // 2. steel peek, if have chakra
        if (ShouldUseTFC(state, strategy, deadline))
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

        if (ShouldUseTrueNorth(state, strategy, finalOGCDDeadline) && state.CanWeave(state.CD(CDGroup.TrueNorth) - 45, 0.6f, deadline))
            return ActionID.MakeSpell(AID.TrueNorth);

        if (ShouldDash(state, strategy))
            return ActionID.MakeSpell(AID.Thunderclap);

        // no suitable oGCDs...
        return new();
    }

    private static Form GetEffectiveForm(State state, Strategy strategy)
    {
        if (state.PerfectBalanceLeft > state.GCD)
        {
            Strategy.FormChoice[] formOverrides = [strategy.PBForm1, strategy.PBForm2, strategy.PBForm3];
            switch (formOverrides[state.BeastCount])
            {
                case Strategy.FormChoice.Opo:
                    return Form.OpoOpo;
                case Strategy.FormChoice.Coeurl:
                    return Form.Coeurl;
                case Strategy.FormChoice.Raptor:
                    return Form.Raptor;
                default:
                    break;
            }

            bool canCoeurl, canRaptor, canOpo;

            var nextNadi = strategy.NextNadi;
            // if a blitz is already in progress, finish it even if buffs would fall off in the process, since celestial revolution is always a mistake
            var forcedLunar = nextNadi == Strategy.NadiChoice.Lunar || state.ForcedLunar;
            var forcedSolar = nextNadi == Strategy.NadiChoice.Solar || state.ForcedSolar;
            canCoeurl = !forcedLunar;
            canRaptor = !forcedLunar;
            canOpo = true;

            if (!state.HasBothNadi)
                foreach (var chak in state.BeastChakra)
                {
                    canCoeurl &= chak != BeastChakra.COEURL;
                    canRaptor &= chak != BeastChakra.RAPTOR;
                    if (forcedSolar)
                        canOpo &= chak != BeastChakra.OPOOPO;
                }

            // big pile of conditionals to check whether this is a forced solar (buffs are running out).
            // odd windows are planned out such that buffed demo was used right before perfect balance, so this
            // block only applies to even windows
            // see ShouldUsePB for more context
            if (canCoeurl && canRaptor)
            {
                if (state.DisciplinedFistLeft == 0)
                    return Form.Raptor;
                if (NeedDemolishRefresh(state, strategy, 2))
                    return Form.Coeurl;
                if (NeedDFRefresh(state, strategy, 2))
                    return Form.Raptor;
            }
            else if (canCoeurl)
            {
                if (state.BeastCount == 1 && NeedDemolishRefresh(state, strategy, 1))
                    return Form.Coeurl;
                else if (state.BeastCount == 2 && NeedDemolishRefresh(state, strategy, 5))
                    return Form.Coeurl;
            }
            else if (canRaptor)
            {
                if (state.BeastCount == 1 && NeedDFRefresh(state, strategy, 1))
                    return Form.Raptor;
                else if (state.BeastCount == 2 && NeedDFRefresh(state, strategy, 4))
                    return Form.Raptor;
            }

            // PB is used preemptively in two cases
            // 1. odd windows (both nadi, NOT COVERED HERE): 1-2 GCDs before RoF so that phantom rush happens in the
            //    buff window, followed by immediate natural demolish refresh
            // 2. BH2 (no nadi, HERE): 1-3 GCDs before RoF (at the latest, it will be used right before Rising Phoenix).
            //    this window consists of Solar -> natural demolish -> Lunar;
            //      if we do lunar first, it's possible for all 3 opo GCDs to miss the RoF window;
            //      if we try to delay both lunar/solar until RoF is up, like the standard opener (which is just BH3),
            //      pre-PB demolish will fall off for multiple GCDs;
            //      so early non-demo solar is the only way to prevent clipping

            // TODO: full demo is more potency than any single gcd, so we should use opo before demo if a refresh is imminent
            var isBH2 = state.FireLeft == 0 && (forcedSolar || !state.HasSolar) && state.Unlocked(AID.RiddleOfFire);
            if (isBH2)
                return canRaptor ? Form.Raptor : canCoeurl ? Form.Coeurl : Form.OpoOpo;

            return canOpo ? Form.OpoOpo : canCoeurl ? Form.Coeurl : Form.Raptor;
        }

        if (state.FormShiftLeft > state.GCD)
        {
            switch (strategy.FormShiftForm)
            {
                case Strategy.FormChoice.Automatic:
                    break;
                case Strategy.FormChoice.Coeurl:
                    return Form.Coeurl;
                case Strategy.FormChoice.Raptor:
                    return Form.Raptor;
                default:
                    return Form.OpoOpo;
            }

            if (NeedDemolishRefresh(state, strategy, 2) && state.DisciplinedFistLeft > state.GCD)
                return Form.Coeurl;

            return Form.OpoOpo;
        }

        return state.Form;
    }

    private static bool ShouldBlitz(State state, Strategy strategy)
        => state.DisciplinedFistLeft > state.GCD &&
        strategy.BlitzUse switch
        {
            Strategy.BlitzStrategy.Delay => false,
            Strategy.BlitzStrategy.DelayUntilMultiTarget => strategy.NumBlitzTargets > 1 || state.BlitzLeft < state.AttackGCDTime,
            _ => true,
        };

    private static bool ShouldDKSpam(State state, Strategy strategy)
        => strategy.DragonKickUse switch
        {
            Strategy.DragonKickStrategy.Filler => state.LeadenFistLeft == 0 && state.DisciplinedFistLeft > state.GCD,
            _ => false,
        };

    private static bool ShouldDash(State state, Strategy strategy)
    {
        if (!state.Unlocked(AID.Thunderclap) || state.CD(CDGroup.Thunderclap) > 60)
            return false;

        return strategy.DashUse switch
        {
            Strategy.DashStrategy.Automatic => strategy.CombatTimer > 0 && strategy.CombatTimer < 1 && state.RangeToTarget > 3,
            Strategy.DashStrategy.Forbid => false,
            Strategy.DashStrategy.GapClose => state.RangeToTarget > 3,
            _ => false,
        };
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

        if (!HaveTarget(state, strategy) || strategy.ActualFightEndIn < 20)
            return false;

        // prevent early use in standard opener
        return state.DisciplinedFistLeft > state.GCD;
    }

    private static bool ShouldUseRoW(State state, Strategy strategy, float deadline)
    {
        if (
            !state.Unlocked(AID.RiddleOfWind)
            || strategy.WindUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay
            || !state.CanWeave(CDGroup.RiddleOfWind, 0.6f, deadline)
        )
            return false;

        if (strategy.WindUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return true;

        if (!HaveTarget(state, strategy) || strategy.ActualFightEndIn < 15)
            return false;

        // thebalance recommends using RoW like an oGCD dot, so we use on cooldown as long as buffs have been used first
        return state.CD(CDGroup.RiddleOfFire) > 0 && state.CD(CDGroup.Brotherhood) > 0;
    }

    private static bool ShouldUseBrotherhood(State state, Strategy strategy, float deadline)
    {
        if (
            !state.Unlocked(AID.Brotherhood)
            || strategy.BrotherhoodUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay
            || !state.CanWeave(CDGroup.Brotherhood, 0.6f, deadline)
        )
            return false;

        if (strategy.BrotherhoodUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return true;

        if (!HaveTarget(state, strategy) || strategy.ActualFightEndIn < 15)
            return false;

        // opener timing mostly important as long as rof is used first, we just want to align with party buffs -
        // the default opener is bhood after first bootshine
        // later uses can be asap
        return !strategy.UseAOE && state.CD(CDGroup.RiddleOfFire) > 0 && (state.LeadenFistLeft == 0 || strategy.CombatTimer > 30);
    }

    private static bool ShouldUsePB(State state, Strategy strategy, float deadline)
    {
        if (
            state.PerfectBalanceLeft > 0
            || !state.Unlocked(AID.PerfectBalance)
            || !state.CanWeave(state.CD(CDGroup.PerfectBalance) - 40, 0.6f, deadline)
            || strategy.PerfectBalanceUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay
        )
            return LogWhy(false, "PB", $"PBLeft = {state.PerfectBalanceLeft}, cd = {state.CD(CDGroup.PerfectBalance)}");

        if (strategy.PerfectBalanceUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return LogWhy(true, "PB", "forced");

        if (!HaveTarget(state, strategy) || strategy.ActualFightEndIn < state.GCD + state.AttackGCDTime * 3)
            return LogWhy(false, "PB", $"target={HaveTarget(state, strategy)}, fight end={strategy.ActualFightEndIn}");

        // with enough haste/low enough GCD (< 1.6, currently exclusive to bozja), double lunar is possible without dropping buffs
        // via lunar -> opo -> snakes -> pb -> lunar
        // this is the only time PB use is not directly after an opo GCD
        if (state.Form == Form.Coeurl && state.FireLeft > deadline + state.AttackGCDTime * 3)
            return LogWhy(
                !NeedDFRefresh(state, strategy, 5) && !NeedDemolishRefresh(state, strategy, 3),
                "PB",
                $"nonstandard (coeurl) lunar, DF={state.DisciplinedFistLeft}, Demo={state.TargetDemolishLeft}"
            );

        if (state.Form != Form.Raptor)
            return LogWhy(false, "PB", "not in raptor");

        // bh1 and bh3 even windows where RoF is used no earlier than 2 GCDs before this; also odd windows where
        // natural demolish happens during RoF
        // before level 68 (RoF unlock) we have nothing to plan our blitzes around, so just use PB whenever it's off cooldown
        // as long as buffs won't fall off
        // TODO: before level 60 (blitz unlock) PB is just a free opo GCD generator so use it right after DF + demo
        if (ShouldUseRoF(state, strategy, deadline) || state.FireLeft > deadline + state.AttackGCDTime * 3 || !state.Unlocked(AID.RiddleOfFire))
        {
            if (!CanSolar(state, strategy))
            {
                return LogWhy(
                    !NeedDFRefresh(state, strategy, 5) && !NeedDemolishRefresh(state, strategy, 6),
                    "PB",
                    $"BH1 (RoF active or imminent), solar unavailable, DF={state.DisciplinedFistLeft}, Demo={state.TargetDemolishLeft}"
                );
            }

            // see haste note above; delay standard even window PB2 in favor of double lunar
            if (NeedDFRefresh(state, strategy, 3) && !NeedDemolishRefresh(state, strategy, 4))
                return LogWhy(false, "PB", $"BH1 (RoF active or imminent), DF expiring = {state.DisciplinedFistLeft}");

            return LogWhy(true, "PB", "BH1 (RoF active or imminent)");
        }

        // odd windows where natural demolish happens before RoF, at most 3 GCDs prior - raptor GCD is forced to
        // be twin snakes if this is the case, so we don't need to check DF timer
        if (!CanSolar(state, strategy) && ShouldUseRoF(state, strategy, state.GCD + state.AttackGCDTime))
            return LogWhy(
                !NeedDemolishRefresh(state, strategy, 7),
                "PB",
                $"odd window, solar unavailable, RoF imminent, demo = {state.TargetDemolishLeft}"
            );

        // bhood 2 window: natural demolish happens in the middle of RoF. it's possible that only the blitz itself
        // gets the RoF buff, so BH2 consists of
        // 1. PB -> "weak" non-OPO gcds until RoF is active
        // 2. RoF -> RP
        // 3. opo, DF, demolish
        // 4. PB -> lunar
        if (
            CanSolar(state, strategy)
            && !ShouldUseRoF(state, strategy, deadline)
            && ShouldUseRoF(state, strategy, deadline + state.AttackGCDTime * 3)
        )
            return LogWhy(!NeedDemolishRefresh(state, strategy, 7), "PB", $"BH2 (early unbuffed solar), demo = {state.TargetDemolishLeft}");

        // forced solar (cdplan or because we would otherwise overcap lunar)
        // (we are guaranteed to be in raptor form due to conditional above)
        if ((strategy.NextNadi == Strategy.NadiChoice.Solar || state.HasLunar && !state.HasSolar) && state.CD(CDGroup.RiddleOfFire) == 0)
            return LogWhy(true, "PB", "Solar forced");

        return LogWhy(false, "PB", "fallback");
    }

    private static bool ShouldUseTrueNorth(State state, Strategy strategy, float lastOgcdDeadline)
    {
        if (
            strategy.TrueNorthUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay
            || state.TrueNorthLeft > state.AnimationLock
        )
            return false;
        if (strategy.TrueNorthUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return true;
        if (!HaveTarget(state, strategy))
            return false;

        var positionalIsWrong = strategy.NextPositionalImminent && !strategy.NextPositionalCorrect;

        // always late weave true north if possible (it's annoying for it to be used immediately)
        // but prioritize Riddle of Fire over it
        if (ShouldUseRoF(state, strategy, lastOgcdDeadline))
            return positionalIsWrong;
        else
            return positionalIsWrong && state.GCD <= 0.800;
    }

    private static bool ShouldUseTFC(State state, Strategy strategy, float deadline)
    {
        if (
            !state.Unlocked(AID.SteelPeak)
            || state.Chakra < 5
            || strategy.TFCUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay
            || !state.CanWeave(CDGroup.SteelPeak, 0.6f, deadline)
        )
            return false;

        if (strategy.TFCUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return true;

        // prevent early use in opener
        return state.CD(CDGroup.RiddleOfFire) > 0 || !state.Unlocked(AID.RiddleOfFire);
    }

    // UseAOE is only true if enemies are in range
    public static bool HaveTarget(State state, Strategy strategy) => state.TargetingEnemy || strategy.UseAOE;

    private static bool NeedDemolishRefresh(State state, Strategy strategy, int gcds)
    {
        // don't care
        if (strategy.UseAOE)
            return false;

        if (strategy.DemolishUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return true;

        if (strategy.DemolishUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay)
            return false;

        if (WillStatusExpire(state, gcds, state.TargetDemolishLeft))
            // snap is 280 (if flank) potency
            // demo is 310 (if rear) potency after 3 ticks: 100 + 70 * 3
            // TODO: this should actually be calculating from the time when we expect to refresh demolish, rather than naively adding duration to the current one, but it probably works for most purposes?
            return true; // strategy.ActualFightEndIn > state.TargetDemolishLeft + 9;

        return false;
    }

    private static bool NeedDFRefresh(State state, Strategy strategy, int gcds)
    {
        if (strategy.DisciplinedFistUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return true;

        if (strategy.DisciplinedFistUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay)
            return false;

        return WillStatusExpire(state, gcds, state.DisciplinedFistLeft);
    }

    private static bool WillStatusExpire(State state, int gcds, float statusDuration)
        => statusDuration < state.GCD + state.AttackGCDTime * gcds;

    private static bool CanSolar(State state, Strategy strategy) => strategy.NextNadi switch
    {
        Strategy.NadiChoice.Solar => true,
        Strategy.NadiChoice.Lunar => false,
        _ => !state.HasSolar
    };

    private static T LogWhy<T>(T value, string tag, string message)
    {
        if (Debug)
            Service.Log($"[{tag}] {value}: {message}");
        return value;
    }
}
