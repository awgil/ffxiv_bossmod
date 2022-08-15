namespace BossMod.BLM
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public float TimeToManaTick; // we assume mana tick happens every 3s
            public int ElementalLevel; // -3 (umbral ice 3) to +3 (astral fire 3)
            public float ElementalLeft; // 0 if elemental level is 0, otherwise buff duration, max 15
            public float SwiftcastLeft; // 0 if buff not up, max 10
            public float ThundercloudLeft;
            public float FirestarterLeft;
            public float TargetThunderLeft; // TODO: this shouldn't be here...

            // upgrade paths
            public AID BestThunder3 => Unlocked(MinLevel.Thunder3) ? AID.Thunder3 : AID.Thunder1;

            public State(float[] cooldowns) : base(cooldowns) { }

            public override string ToString()
            {
                return $"MP={CurMP} (tick={TimeToManaTick:f1}), RB={RaidBuffsLeft:f1}, Elem={ElementalLevel}/{ElementalLeft:f1}, Thunder={TargetThunderLeft:f1}, TC={ThundercloudLeft:f1}, FS={FirestarterLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public bool AOE;
            public bool Moving;
        }

        public static uint AdjustedFireCost(State state, uint baseCost)
        {
            return state.ElementalLevel switch
            {
                > 0 => baseCost * 2,
                < 0 => 0,
                _ => baseCost
            };
        }

        public static int MPTick(int elementalLevel)
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
            bool allowCasts = !strategy.Moving || state.SwiftcastLeft > state.GCD;
            if (state.Unlocked(MinLevel.Blizzard3))
            {
                // starting from L35, fire/blizzard 2/3 automatically grant 3 fire/ice stacks, so we use them to swap between stances
                if (strategy.AOE)
                {
                    // TODO: revise at L50+ (flare)
                    if (state.ElementalLevel > 0)
                    {
                        // fire phase: F2 until oom > B2
                        if (allowCasts)
                            return state.CurMP >= AdjustedFireCost(state, 1500) ? AID.Fire2 : AID.Blizzard2;
                        if (state.ThundercloudLeft > state.GCD)
                            return AID.Thunder2;
                        return AID.None; // chill...
                    }
                    else if (state.ElementalLevel < 0)
                    {
                        // ice phase: Freeze/B2 if needed for mana tick > T2 if needed to refresh > F2
                        if (state.TargetThunderLeft <= state.GCD && (allowCasts || state.ThundercloudLeft > state.GCD))
                            return AID.Thunder2; // if thunder is about to fall off, refresh before B2

                        bool wantThunder = state.TargetThunderLeft < 10; // TODO: better threshold
                        if (allowCasts)
                        {
                            float minTimeToSwap = state.GCD + (wantThunder ? 2.5f : 0) + 1; // F2 is always hardcasted, time is 3 / 2 = 1.5 minus ~0.5 slidecast
                            var mpTicksAtMinSwap = (int)((3 - state.TimeToManaTick + minTimeToSwap) / 3);
                            var mpAtMinSwap = state.CurMP + mpTicksAtMinSwap * MPTick(state.ElementalLevel);
                            if (mpAtMinSwap < 9600)
                                return state.Unlocked(MinLevel.Freeze) ? AID.Freeze : AID.Blizzard2;
                        }
                        if (state.ThundercloudLeft > state.GCD || wantThunder && allowCasts)
                            return AID.Thunder2;
                        if (state.CurMP >= 9600 && allowCasts)
                            return AID.Fire2;
                        return AID.None; // chill...
                    }
                    else
                    {
                        // opener or just dropped elemental for some reason - just F3
                        // TODO: should we open with dot instead?..
                        if (state.CurMP >= 9600 && allowCasts)
                            return AID.Fire2;
                        else if (allowCasts)
                            return AID.Blizzard2;
                        return AID.None; // chill?..
                    }
                }
                else
                {
                    // TODO: revise at L50+ (flare?..)
                    if (state.ElementalLevel > 0)
                    {
                        // fire phase: F3P > F1 until oom > B3
                        // TODO: Tx[P] now? or delay until ice phase?
                        if (state.FirestarterLeft > state.GCD)
                            return AID.Fire3;
                        if (allowCasts)
                            return state.CurMP >= AdjustedFireCost(state, 800) ? AID.Fire1 : AID.Blizzard3;
                        // TODO: scathe on the move?..
                        if (state.ThundercloudLeft > state.GCD)
                            return state.BestThunder3;
                        return AID.None; // chill...
                    }
                    else if (state.ElementalLevel < 0)
                    {
                        // ice phase: B1 if needed for mana tick > T1/T3 if needed to refresh > F3
                        if (state.TargetThunderLeft <= state.GCD && (allowCasts || state.ThundercloudLeft > state.GCD))
                            return state.BestThunder3; // if thunder is about to fall off, refresh before B1

                        bool wantThunder = state.TargetThunderLeft < 10; // TODO: better threshold
                        if (allowCasts)
                        {
                            float minTimeToSwap = state.GCD + (wantThunder ? 2.5f : 0);
                            if (state.FirestarterLeft < minTimeToSwap)
                                minTimeToSwap += 1.2f; // when hardcasting F3, swap happens around slidecast start; cast time for F3 is 3.5 / 2 = 1.75, action effect happens ~0.5s before cast end
                            var mpTicksAtMinSwap = (int)((3 - state.TimeToManaTick + minTimeToSwap) / 3);
                            var mpAtMinSwap = state.CurMP + mpTicksAtMinSwap * MPTick(state.ElementalLevel);
                            if (mpAtMinSwap < 9800)
                                return AID.Blizzard1;
                        }
                        if (state.ThundercloudLeft > state.GCD || wantThunder && allowCasts)
                            return state.BestThunder3;
                        if (state.CurMP >= 9800 && (allowCasts || state.FirestarterLeft > state.GCD))
                            return AID.Fire3;
                        return AID.None; // chill...
                    }
                    else
                    {
                        // opener or just dropped elemental for some reason - just F3
                        // TODO: should we open with dot instead?..
                        if (state.CurMP >= 9800 && (allowCasts || state.FirestarterLeft > state.GCD))
                            return AID.Fire3;
                        else if (allowCasts)
                            return AID.Blizzard3;
                        return AID.None; // chill?..
                    }
                }
            }
            else
            {
                // before L35, fire/blizzard 1/2 cast under wrong element reset stance - so we use transpose rotation
                if (!allowCasts)
                {
                    // TODO: this is not really correct, we could have thundercloud, but w/e...
                    if (state.Unlocked(MinLevel.Scathe) && state.CurMP >= 800)
                        return AID.Scathe;
                }
                else if (strategy.AOE && state.Unlocked(MinLevel.Blizzard2))
                {
                    if (state.Unlocked(MinLevel.Thunder2) && state.TargetThunderLeft <= state.GCD)
                        return AID.Thunder2;
                    return state.Unlocked(MinLevel.Fire2) ? TransposeRotationGCD(state, AID.Blizzard2, 800, AID.Fire2, 1500) : AID.Blizzard2;
                }
                else
                {
                    if (state.Unlocked(MinLevel.Thunder1) && state.TargetThunderLeft <= state.GCD)
                        return AID.Thunder1;
                    return state.Unlocked(MinLevel.Fire1) ? TransposeRotationGCD(state, AID.Blizzard1, 400, AID.Fire1, 800) : AID.Blizzard1;
                }
            }
            return AID.None;
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            if (state.Unlocked(MinLevel.Blizzard3))
            {
                // L35-Lxx: weave manafont in free slots (TODO: is that right?..)
                if (deadline >= 10000 && strategy.Moving && state.Unlocked(MinLevel.Swiftcast) && state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Swiftcast);
                if (state.Unlocked(MinLevel.Manafont) && state.CanWeave(CDGroup.Manafont, 0.6f, deadline) && state.CurMP <= 7000)
                    return ActionID.MakeSpell(AID.Manafont);
            }
            else
            {
                // before L35, use transpose to swap between elemental states
                // MP thresholds are not especially meaningful (they should work for both ST and AOE), who cares about low level...
                // we could also use manafont, but again who cares
                if (state.Unlocked(MinLevel.Transpose) && state.CanWeave(CDGroup.Transpose, 0.6f, deadline) && (state.ElementalLevel < 0 && state.CurMP >= 9200 || state.ElementalLevel > 0 && state.CurMP < 3600))
                    return ActionID.MakeSpell(AID.Transpose);

                // TODO: swiftcast if moving...
            }

            return new();
        }

        private static AID TransposeRotationGCD(State state, AID iceSpell, int iceCost, AID fireSpell, int fireCost)
        {
            if (state.ElementalLevel < 0)
            {
                // continue blizzard1 spam until full mana, then swap to fire
                // TODO: take mana ticks into account, if it happens before GCD
                if (state.CurMP < 10000 - iceCost)
                    return iceSpell;
                else
                    return state.Unlocked(MinLevel.Transpose) ? AID.None : fireSpell;
            }
            else if (state.ElementalLevel > 0)
            {
                // continue fire1 spam until oom, then swap to ice
                if (state.CurMP >= fireCost * 2 + iceCost * 3 / 4)
                    return fireSpell;
                else
                    return state.Unlocked(MinLevel.Transpose) ? AID.None : iceSpell;
            }
            else
            {
                // dropped buff => fire if have some mana (TODO: better limit), blizzard otherwise
                return state.CurMP < fireCost * 3 + iceCost * 3 / 4 ? iceSpell : fireSpell;
            }
        }
    }
}
