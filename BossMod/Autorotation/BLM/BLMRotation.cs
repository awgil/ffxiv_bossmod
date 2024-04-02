namespace BossMod.BLM;

public static class Rotation
{
    public enum Aspect
    {
        None = 0,
        Ice = 1,
        Fire = 2
    }

    // full state needed for determining next action
    public class State : CommonRotation.PlayerState
    {
        public float TimeToManaTick; // we assume mana tick happens every 3s
        public int ElementalLevel; // -3 (umbral ice 3) to +3 (astral fire 3)
        public float ElementalLeft; // 0 if elemental level is 0, otherwise buff duration, max 15
        public float SwiftcastLeft; // 0 if buff not up, max 10
        public float TriplecastLeft; // max 15
        public float SharpcastLeft; // max 30
        public float ThundercloudLeft;
        public float FirestarterLeft;
        public float TargetThunderLeft; // TODO: this shouldn't be here...
        public float LeyLinesLeft; // max 30
        public bool InLeyLines;
        public bool Paradox;
        public int UmbralHearts; // max 3
        public int Polyglot; // max 2
        public float EnochianTimer;
        public float NextPolyglot => EnochianTimer;
        public int MaxHearts => Unlocked(TraitID.EnhancedFreeze) ? 3 : 0;

        // lowest remaining Flare Star duration on any enemy in range, plus primary target whether they are within 10y or not
        // max is ostensibly 60s, but will be set to float.MaxValue if there are no enemies in range in order to make logic easier
        public float TargetFlareStarLeft;

        // this is an approximation. mana drain ticks can and will be offset arbitrarily from regen ticks. however, actually calculating that is a huge pita
        public float TimeToManaHalfTick => TimeToManaTick > 1.5 ? TimeToManaTick - 1.5f : TimeToManaTick + 1.5f;

        public float InstantCastLeft => MathF.Max(SwiftcastLeft, TriplecastLeft);
        public float FontOfMagicLeft;
        public float MagicBurstLeft;
        public float LucidDreamingLeft;
        public float AutoEtherLeft;

        // upgrade paths
        public AID BestThunder1 => Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1;
        public AID BestThunder2 => Unlocked(AID.Thunder4) ? AID.Thunder4 : AID.Thunder2;

        public AID BestFire1 => Paradox ? AID.Paradox : AID.Fire1;
        public AID BestFire2 => Unlocked(AID.HighFire2) ? AID.HighFire2 : AID.Fire2;
        public AID BestBlizzard2 => Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : AID.Blizzard2;

        // statuses
        public SID ExpectedThunder1 => Unlocked(AID.Thunder3) ? SID.Thunder3 : SID.Thunder1;
        public SID ExpectedThunder2 => Unlocked(AID.Thunder4) ? SID.Thunder4 : SID.Thunder2;

        public AID BestPolySpell => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;

        public State(WorldState ws) : base(ws) { }

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public int GetAdjustedFireCost(int mpCost) =>
            AdjustCost(
                ElementalLevel switch
                {
                    0 => mpCost,
                    > 0 => UmbralHearts > 0 ? mpCost : mpCost * 2,
                    < 0 => 0
                }
            );

        public int GetAdjustedIceCost(int mpCost) =>
            AdjustCost(
                ElementalLevel switch
                {
                    -3 => 0,
                    -2 => mpCost / 2,
                    -1 => mpCost / 4 * 3,
                    _ => mpCost
                }
            );

        public int AdjustCost(int mpCost) => MagicBurstLeft > GCD ? mpCost / 100 * 130 : mpCost;

        // FoM drains MP every half-tick, deducting a random amount between 50% and 150% of 1100 MP
        private const int MAXIMUM_FOM_TICK = 1650;

        // Lucid Dreaming's refresh effect is then applied on top of that (so is Cure II, which is 1000 MP per tick, but i don't know the SID for it)
        public int MPDrainPerHalfTick =>
            (FontOfMagicLeft > 0 ? MAXIMUM_FOM_TICK : 0) - (LucidDreamingLeft > 0 ? 550 : 0);

        public int ExpectedMPAfter(float delay)
        {
            float originalDeadline = delay;
            var expected = (int)CurMP;
            var perTick = (int)MPTick(ElementalLevel);
            var drainPerTick = MPDrainPerHalfTick;

            bool evenTick = TimeToManaTick < TimeToManaHalfTick;
            bool mightEther = AutoEtherLeft > originalDeadline;

            delay -= evenTick ? TimeToManaTick : TimeToManaHalfTick;

            while (delay > 0)
            {
                if (evenTick)
                {
                    expected += perTick;
                }
                else
                {
                    // lucid dreaming applies a mana drain of -550 (in other words, +550 MP every 3 seconds) but
                    // it does not regen mana in astral fire
                    // HOWEVER, it does reduce the mana drain of Font of Magic EVEN DURING astral fire
                    // in other words, we can't gain mana on half ticks during astral fire, but we can lose less
                    if (drainPerTick < 0 && ElementalLevel > 0)
                        drainPerTick = 0;
                    expected -= drainPerTick;
                }

                if (expected < 2000 && mightEther)
                {
                    expected += 5000;
                    mightEther = false; // 50% chance to expire
                }

                delay -= 1.5f;
                evenTick = !evenTick;
            }
            return Math.Min(10000, Math.Max(0, expected));
        }

        public float GetCastTime(AID action)
        {
            if (
                action == AID.Paradox && ElementalLevel < 0
                || InstantCastLeft > GCD
                || action == AID.Fire3 && FirestarterLeft > GCD
                || action.Aspect() == BLM.Aspect.Thunder && ThundercloudLeft > GCD
                || action == AID.Foul && Unlocked(TraitID.EnhancedFoul)
            )
                return 0f;

            var spsFactor = SpellGCDTime / 2.5f;
            var castTime = action.BaseCastTime();
            var aspect = action.Aspect();

            if (
                aspect == BLM.Aspect.Ice && ElementalLevel == 3
                || aspect == BLM.Aspect.Fire && ElementalLevel == -3
            )
                castTime *= 0.5f;

            return castTime * spsFactor;
        }

        // in addition to slidecasting, this is also the time until the provided spell spends MP
        public float GetSlidecastTime(AID action) => MathF.Max(0f, GetCastTime(action) - 0.500f);

        public float GetCastEnd(AID action) => GetCastTime(action) + GCD;

        public float GetSlidecastEnd(AID action) => GetSlidecastTime(action) + GCD;

        public override string ToString()
        {
            return $"MP={CurMP} (tick={TimeToManaTick:f1}), RB={RaidBuffsLeft:f1}, E={EnochianTimer}, Elem={ElementalLevel}/{ElementalLeft:f1}, Thunder={TargetThunderLeft:f1}, TC={ThundercloudLeft:f1}, FS={FirestarterLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public OffensiveAbilityUse TriplecastStrategy;
        public OffensiveAbilityUse LeylinesStrategy;
        public bool UseAOERotation;
        public int NumFlareStarTargets;
        public bool AutoRefresh;
        public bool UseLFS;

        public float ActualFightEndIn => FightEndIn == 0 ? 10000f : FightEndIn;

        public void ApplyStrategyOverrides(uint[] overrides)
        {
            if (overrides.Length >= 2)
            {
                TriplecastStrategy = (OffensiveAbilityUse)overrides[0];
                LeylinesStrategy = (OffensiveAbilityUse)overrides[1];
            }
            else
            {
                TriplecastStrategy = OffensiveAbilityUse.Automatic;
                LeylinesStrategy = OffensiveAbilityUse.Automatic;
            }
        }
    }

    private static bool CanCast(State state, Strategy strategy, float castTime, int mpCost, bool preserveFoM = true)
    {
        var castEndIn = state.GCD + castTime;
        var moveOk = castTime == 0 || strategy.ForceMovementIn > castEndIn;
        var minMP = 0;

        if (mpCost == -1)
            mpCost = Math.Max(800, (int)state.CurMP);

        if (preserveFoM && state.FontOfMagicLeft >= castEndIn)
            minMP = state.MPDrainPerHalfTick + 1;

        return moveOk
            && strategy.ActualFightEndIn > castEndIn
            && state.ExpectedMPAfter(castEndIn) >= mpCost + minMP;
    }

    private static bool CanCast(State state, Strategy strategy, AID action, int mpCost, bool preserveFoM = true) =>
        state.Unlocked(action) && CanCast(state, strategy, state.GetSlidecastTime(action), mpCost, preserveFoM);

    public static uint MPTick(int elementalLevel)
    {
        return elementalLevel switch
        {
            -3 => 6200,
            -2 => 4700,
            -1 => 3200,
            0 => 200,
            _ => 0
        };
    }

    public static AID GetNextBestGCD(State state, Strategy strategy)
    {
        if (strategy.CombatTimer > -100 && strategy.CombatTimer < 0 && state.TargetingEnemy)
        {
            if (strategy.CombatTimer > -state.GetCastTime(AID.Fire3) && state.ElementalLevel == 0)
                return AID.Fire3;

            if (strategy.CombatTimer > -1 && state.TargetThunderLeft == 0)
                return AID.Thunder3;

            return AID.None;
        }

        if (!state.TargetingEnemy)
        {
            if (
                state.ElementalLevel < 0
                && (state.ElementalLeft < 5 || state.UmbralHearts < 3 || state.ElementalLevel > -3)
                && state.Unlocked(AID.UmbralSoul)
                && strategy.AutoRefresh
            )
                return AID.UmbralSoul;
            
            return AID.None;
        }

        // first check if F4 is unlocked and fire timer is running out. all other fire spells refresh the timer
        // TODO: needs some fixing. F4 F1 is 2.8s + 2.5s, F4 Despair is 2.8s + 3s, it is possible to skip this branch
        // with (e.g.) 5.5s remaining on elemental timer, but then after F4 cast we have <1600 MP, can't cast F1,
        // despair is now too slow
        if (
            !strategy.UseAOERotation
            && state.Unlocked(AID.Fire4)
            && state.ElementalLevel == 3
            && state.ElementalLeft
                < state.GCD
                    + MathF.Max(state.SpellGCDTime, state.GetCastTime(AID.Fire4))
                    + state.GetCastTime(AID.Fire1)
        )
        {
            // use despair now if f1 will leave us with too little mana
            if (
                state.ElementalLeft >= state.GetCastEnd(AID.Despair)
                && CanCast(state, strategy, AID.Despair, -1)
                && state.CurMP - 800 < state.GetAdjustedFireCost(800)
            )
                return AID.Despair;

            // otherwise use f1/paradox to refresh
            if (
                state.ElementalLeft >= state.GetCastEnd(state.BestFire1)
                && CanCast(state, strategy, state.BestFire1, state.GetAdjustedFireCost(800))
            )
                return state.BestFire1;

            if (state.ElementalLeft > state.GCD && state.FirestarterLeft > state.GCD)
                return AID.Fire3;

            // out of time, reset to ice
            if (CanCast(state, strategy, AID.Blizzard3, 0))
                return AID.Blizzard3;
        }

        // polyglot overcap
        if (
            state.Polyglot == 2
            && state.NextPolyglot > 0
            && state.NextPolyglot < 10
            && CanCast(state, strategy, AID.Foul, 0)
        )
            return strategy.UseAOERotation ? AID.Foul : AID.Xenoglossy;

        // thunder refresh
        if (state.TargetThunderLeft < 5 && CanCast(state, strategy, state.BestThunder1, state.AdjustCost(400)))
            return strategy.UseAOERotation && state.Unlocked(state.BestThunder2)
                ? state.BestThunder2
                : state.BestThunder1;

        // standard gcd loop
        return state.ElementalLevel > 0 ? GetFireGCD(state, strategy) : GetIceGCD(state, strategy);
    }

    public static AID GetFireGCD(State state, Strategy strategy)
    {
        // intentional gcd clip in opener
        if (
            state.InstantCastLeft == 0
            && strategy.CombatTimer < 60
            && strategy.TriplecastStrategy != CommonRotation.Strategy.OffensiveAbilityUse.Delay
            && state.Unlocked(AID.Triplecast)
            && state.CD(CDGroup.Triplecast) == 0
        )
            return AID.Triplecast;

        if (strategy.UseAOERotation)
        {
            var canFlare = CanCast(state, strategy, AID.Flare, -1);
            // double flare
            if (state.UmbralHearts == 1 && canFlare)
                return AID.Flare;

            if (CanCast(state, strategy, state.BestFire2, state.GetAdjustedFireCost(1500) + 800))
                return state.BestFire2;

            if (canFlare)
                return AID.Flare;
        }
        else
        {
            if (state.ElementalLevel < 3 && CanCast(state, strategy, AID.Fire3, state.GetAdjustedFireCost(2000)))
                return AID.Fire3;

            if (CanCast(state, strategy, AID.Fire4, state.GetAdjustedFireCost(800) + 800))
                return AID.Fire4;

            // before F4 unlock, use firestarter proc for damage
            // (after F4 unlock we save it for fast ice -> fire swap)
            if (!state.Unlocked(AID.Fire4) && state.FirestarterLeft > state.GCD && state.Unlocked(AID.Fire3))
                return AID.Fire3;

            if (CanCast(state, strategy, state.BestFire1, state.GetAdjustedFireCost(800) + 800))
                return state.BestFire1;

            // TODO: swiftcast flare is a dps gain on two targets
            if (CanCast(state, strategy, AID.Despair, -1))
                return AID.Despair;

            // despair isn't unlocked
            if (CanCast(state, strategy, AID.Fire1, state.GetAdjustedFireCost(800)))
                return AID.Fire1;
        }

        // use instant spell for manafont weave
        if (
            state.Polyglot > 0
            && CanUseManafont(state, strategy, state.GCD + state.SpellGCDTime)
            && state.Unlocked(TraitID.EnhancedFoul)
        )
            return strategy.UseAOERotation ? AID.Foul : AID.Xenoglossy;

        // if fight ending, dump resources instead of switching to ice
        // (assuming <800 MP left here, otherwise one of the earlier branches would have been taken)
        if (strategy.ActualFightEndIn < state.GetCastEnd(AID.Blizzard3) + state.SpellGCDTime)
        {
            if (CanPoly(state, strategy, 1))
                return state.BestPolySpell;

            if (state.FirestarterLeft > state.GCD)
                return AID.Fire3;
        }

        // otherwise swap to ice
        if (strategy.UseAOERotation && CanCast(state, strategy, state.BestBlizzard2, 0))
            return state.BestBlizzard2;

        if (CanCast(state, strategy, AID.Blizzard3, 0))
            return AID.Blizzard3;

        if (!state.Paradox && CanCast(state, strategy, AID.Blizzard1, 0))
            return AID.Blizzard1;

        return AID.None;
    }

    public static AID GetIceGCD(State state, Strategy strategy)
    {
        // get max hearts for swap
        if (state.UmbralHearts < 3 && state.Unlocked(TraitID.EnhancedFreeze) && state.ElementalLevel < 0)
        {
            if (strategy.UseAOERotation && CanCast(state, strategy, AID.Freeze, 0))
                return AID.Freeze;

            if (CanCast(state, strategy, AID.Blizzard4, 0))
                return AID.Blizzard4;
        }

        if (CanFlareStar(state, strategy) && state.TargetFlareStarLeft < 5 && state.TargetThunderLeft > 5)
        {
            if (state.ElementalLevel == -3 && state.ElementalLeft > 6)
            {
                if (state.FontOfMagicLeft > state.TimeToManaTick)
                {
                    if (state.CurMP >= 9000 && state.CurMP < 10000)
                        return (AID)BozjaActionID.GetNormal(BozjaHolsterID.LostFlareStar).ID;
                }
                else if (state.DutyActionCD(BozjaActionID.GetNormal(BozjaHolsterID.LostFontOfMagic)) == 0 && CanFoM(state, strategy))
                {
                    if (state.LucidDreamingLeft == 0 && state.CD(CDGroup.LucidDreaming) == 0)
                        return AID.LucidDreaming;

                    return (AID)BozjaActionID.GetNormal(BozjaHolsterID.LostFontOfMagic).ID;
                }
                else if (state.CurMP >= 9000)
                    return (AID)BozjaActionID.GetNormal(BozjaHolsterID.LostFlareStar).ID;
                else if (state.Polyglot > 0)
                    return AID.Xenoglossy;
            }
            else
                return strategy.UseAOERotation ? AID.Blizzard2 : AID.Blizzard3;

            return AID.None;
        }

        // swap if near max mp
        if (strategy.UseAOERotation)
        {
            if (
                CanCast(state, strategy, state.BestFire2, 8200)
                && (!state.Unlocked(TraitID.EnhancedFreeze) || state.UmbralHearts == 3)
            )
                return state.BestFire2;
        }
        else
        {
            if (CanCast(state, strategy, AID.Fire3, 8200))
                return AID.Fire3;

            if (!state.Paradox && CanCast(state, strategy, AID.Fire1, 8200))
                return AID.Fire1;
        }

        if (state.ElementalLeft < 0 && CanPoly(state, strategy, 2))
            return strategy.UseAOERotation ? AID.Foul : state.BestPolySpell;

        if (strategy.UseAOERotation)
        {
            if (CanCast(state, strategy, state.BestBlizzard2, state.GetAdjustedIceCost(800)))
                return state.BestBlizzard2;

            if (CanPoly(state, strategy, 1))
                return AID.Foul;
        }
        else
        {
            if (CanCast(state, strategy, AID.Blizzard3, state.GetAdjustedIceCost(800)) && state.ElementalLevel > -3)
                return AID.Blizzard3;

            // in umbral ice, paradox costs no mp and has no cast time, so no check
            if (state.Paradox)
                return AID.Paradox;

            if (CanPoly(state, strategy, 1))
                return state.BestPolySpell;

            if (CanCast(state, strategy, AID.Blizzard1, state.GetAdjustedIceCost(400)))
                return AID.Blizzard1;
        }

        return AID.None;
    }

    public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
    {
        if (strategy.CombatTimer > -100 && strategy.CombatTimer < 0)
        {
            if (strategy.CombatTimer > -12 && state.SharpcastLeft == 0)
                return ActionID.MakeSpell(AID.Sharpcast);

            return new();
        }

        if (
            !state.TargetingEnemy
            && state.CanWeave(CDGroup.Transpose, 0.6f, deadline)
            && strategy.AutoRefresh
        ) {
            if (state.ElementalLevel > 0)
                return ActionID.MakeSpell(AID.Transpose);

            if (state.ElementalLeft < 5 && state.Unlocked(TraitID.EnhancedEnochian1) && !state.Unlocked(AID.UmbralSoul))
                return ActionID.MakeSpell(AID.Transpose);
        }

        if (
            state.FirestarterLeft > state.GCD
            && state.ElementalLevel < 0
            && state.CurMP >= 9600
            && state.CanWeave(CDGroup.Transpose, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.Transpose);

        if (state.CurMP < 800 && state.ElementalLevel == 3 && CanUseManafont(state, strategy, deadline))
            return ActionID.MakeSpell(AID.Manafont);

        if (state.TriplecastLeft > state.GCD)
        {
            if (
                state.CanWeave(CDGroup.Amplifier, 0.6f, deadline)
                && state.Unlocked(AID.Amplifier)
                && state.Polyglot < 2
                && state.ElementalLevel != 0
            )
                return ActionID.MakeSpell(AID.Amplifier);

            if (
                state.CanWeave(CDGroup.LeyLines, 0.6f, deadline)
                && strategy.LeylinesStrategy != CommonRotation.Strategy.OffensiveAbilityUse.Delay
                // don't place leylines after opener, let the player do it
                && strategy.CombatTimer < 60
            )
                return ActionID.MakeSpell(AID.LeyLines);
        }

        if (state.InLeyLines && state.InstantCastLeft < state.GCD)
        {
            if (state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Swiftcast);

            if (
                state.CanWeave(state.CD(CDGroup.Triplecast) - 60, 0.6f, deadline)
                && (
                    strategy.CombatTimer > 0 && strategy.CombatTimer < 60
                    || strategy.TriplecastStrategy == CommonRotation.Strategy.OffensiveAbilityUse.Force
                )
            )
                return ActionID.MakeSpell(AID.Triplecast);
        }

        // TODO: what we actually want to do is sharpcast *only* if we will refresh thunder using thundercloud, otherwise it will proc on paradox/f1
        if (
            state.ThundercloudLeft > 0
            && state.SharpcastLeft < state.GCD
            && state.Unlocked(AID.Sharpcast)
            && state.CanWeave(state.CD(CDGroup.Sharpcast) - 30, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.Sharpcast);

        return new();
    }

    private static bool CanUseManafont(State state, Strategy strategy, float deadline)
    {
        if (strategy.LeylinesStrategy == CommonRotation.Strategy.OffensiveAbilityUse.Delay)
            return false;

        return state.CanWeave(CDGroup.Manafont, 0.6f, deadline);
    }

    private static bool CanPoly(State state, Strategy strategy, int minStacks = 2) =>
        state.Polyglot >= minStacks && CanCast(state, strategy, state.BestPolySpell, 0);

    private static bool CanFlareStar(State state, Strategy strategy)
    {
        return state.FindDutyActionSlot(BozjaActionID.GetNormal(BozjaHolsterID.LostFlareStar)) >= 0
            && strategy.NumFlareStarTargets > 0
            && state.MagicBurstLeft == 0;
    }

    private static bool CanFoM(State state, Strategy strategy)
    {
        return state.FindDutyActionSlot(BozjaActionID.GetNormal(BozjaHolsterID.LostFontOfMagic)) >= 0
            && strategy.TriplecastStrategy != CommonRotation.Strategy.OffensiveAbilityUse.Delay;
    }
}
