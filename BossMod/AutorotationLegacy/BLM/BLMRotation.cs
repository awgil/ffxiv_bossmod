namespace BossMod.BLM;

public static class Rotation
{
    // full state needed for determining next action
    public class State(WorldState ws) : CommonRotation.PlayerState(ws)
    {
        public float TimeToManaTick; // we assume mana tick happens every 3s
        public int ElementalLevel; // -3 (umbral ice 3) to +3 (astral fire 3)
        public float ElementalLeft; // 0 if elemental level is 0, otherwise buff duration, max 15
        public float SwiftcastLeft; // 0 if buff not up, max 10
        public float ThundercloudLeft;
        public float FirestarterLeft;
        public float TargetThunderLeft; // TODO: this shouldn't be here...

        // upgrade paths
        public AID BestThunder3 => Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1;

        // statuses
        public SID ExpectedThunder3 => Unlocked(AID.Thunder3) ? SID.Thunder3 : SID.Thunder1;

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"MP={CurMP} (tick={TimeToManaTick:f1}), RB={RaidBuffsLeft:f1}, Elem={ElementalLevel}/{ElementalLeft:f1}, Thunder={TargetThunderLeft:f1}, TC={ThundercloudLeft:f1}, FS={FirestarterLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public int NumAOETargets;

        public override string ToString()
        {
            return $"AOE={NumAOETargets}, no-dots={ForbidDOTs}, movement-in={ForceMovementIn:f3}";
        }
    }

    public static bool CanCast(State state, Strategy strategy, float castTime) => state.SwiftcastLeft > state.GCD || strategy.ForceMovementIn >= state.GCD + castTime;

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
        if (state.Unlocked(AID.Blizzard3))
        {
            // starting from L35, fire/blizzard 2/3 automatically grant 3 fire/ice stacks, so we use them to swap between stances
            if (strategy.NumAOETargets >= 3)
            {
                // TODO: revise at L58+
                if (state.ElementalLevel > 0)
                {
                    // fire phase: F2 until oom > Flare > B2
                    bool flareUnlocked = state.Unlocked(AID.Flare);
                    if (CanCast(state, strategy, flareUnlocked ? 4 : 3)) // TODO: flare is 4s cast time, B2 is 1.5s if at 3 stacks, other spells are 3s
                    {
                        if (flareUnlocked)
                            return state.CurMP > AdjustedFireCost(state, 1500) ? AID.Fire2 : state.CurMP > 0 ? AID.Flare : AID.Blizzard2;
                        else
                            return state.CurMP >= AdjustedFireCost(state, 1500) ? AID.Fire2 : AID.Blizzard2;
                    }
                    if (!strategy.ForbidDOTs && state.ThundercloudLeft > state.GCD)
                        return AID.Thunder2;
                    return AID.None; // chill...
                }
                else if (state.ElementalLevel < 0)
                {
                    // ice phase: Freeze/B2 if needed for mana tick > T2 if needed to refresh > F2
                    if (!strategy.ForbidDOTs && state.TargetThunderLeft <= state.GCD && (state.ThundercloudLeft > state.GCD || CanCast(state, strategy, 2.5f)))
                        return AID.Thunder2; // if thunder is about to fall off, refresh before B2

                    bool wantThunder = !strategy.ForbidDOTs && state.TargetThunderLeft < 10; // TODO: better threshold
                    if (CanCast(state, strategy, 3))
                    {
                        float minTimeToSwap = state.GCD + (wantThunder ? 2.5f : 0) + 1; // F2 is always hardcasted, time is 3 / 2 = 1.5 minus ~0.5 slidecast
                        var mpTicksAtMinSwap = (int)((3 - state.TimeToManaTick + minTimeToSwap) / 3);
                        var mpAtMinSwap = state.CurMP + mpTicksAtMinSwap * MPTick(state.ElementalLevel);
                        if (mpAtMinSwap < 9600)
                            return state.Unlocked(AID.Freeze) ? AID.Freeze : AID.Blizzard2;
                    }
                    if (!strategy.ForbidDOTs && state.ThundercloudLeft > state.GCD || wantThunder && CanCast(state, strategy, 2.5f))
                        return AID.Thunder2;
                    if (state.CurMP >= 9600 && CanCast(state, strategy, state.ElementalLevel == -3 ? 1.5f : 3))
                        return AID.Fire2;
                    return AID.None; // chill...
                }
                else
                {
                    // opener or just dropped elemental for some reason - just F3
                    // TODO: should we open with dot instead?..
                    if (CanCast(state, strategy, 2.5f))
                        return state.CurMP >= 9600 ? AID.Fire2 : AID.Blizzard2;
                    return AID.None; // chill?..
                }
            }
            else
            {
                // TODO: revise at L58+
                if (state.ElementalLevel > 0)
                {
                    // fire phase: F3P > F1 until oom > B3
                    // TODO: Tx[P] now? or delay until ice phase?
                    if (state.FirestarterLeft > state.GCD)
                        return AID.Fire3;
                    if (CanCast(state, strategy, 2.5f)) // TODO: B3 is 3.5, but since we typically have 3 fire stacks, it's actually 1.75
                        return state.CurMP >= AdjustedFireCost(state, 800) ? AID.Fire1 : AID.Blizzard3;
                    // TODO: scathe on the move?..
                    if (!strategy.ForbidDOTs && state.ThundercloudLeft > state.GCD)
                        return state.BestThunder3;
                    return AID.None; // chill...
                }
                else if (state.ElementalLevel < 0)
                {
                    // ice phase: B1 if needed for mana tick > T1/T3 if needed to refresh > F3
                    if (!strategy.ForbidDOTs && state.TargetThunderLeft <= state.GCD && (state.ThundercloudLeft > state.GCD || CanCast(state, strategy, 2.5f)))
                        return state.BestThunder3; // if thunder is about to fall off, refresh before B1

                    bool wantThunder = !strategy.ForbidDOTs && state.TargetThunderLeft < 10; // TODO: better threshold
                    if (CanCast(state, strategy, 2.5f))
                    {
                        float minTimeToSwap = state.GCD + (wantThunder ? 2.5f : 0);
                        if (state.FirestarterLeft < minTimeToSwap)
                            minTimeToSwap += 1.2f; // when hardcasting F3, swap happens around slidecast start; cast time for F3 is 3.5 / 2 = 1.75, action effect happens ~0.5s before cast end
                        var mpTicksAtMinSwap = (int)((3 - state.TimeToManaTick + minTimeToSwap) / 3);
                        var mpAtMinSwap = state.CurMP + mpTicksAtMinSwap * MPTick(state.ElementalLevel);
                        if (mpAtMinSwap < 9800)
                            return AID.Blizzard1;
                    }
                    if (!strategy.ForbidDOTs && state.ThundercloudLeft > state.GCD || wantThunder && CanCast(state, strategy, 2.5f))
                        return state.BestThunder3;
                    if (state.CurMP >= 9800 && (state.FirestarterLeft > state.GCD || CanCast(state, strategy, state.ElementalLeft == -3 ? 1.75f : 3.5f)))
                        return AID.Fire3;
                    return AID.None; // chill...
                }
                else
                {
                    // opener or just dropped elemental for some reason - just F3
                    // TODO: should we open with dot instead?..
                    if (state.CurMP >= 9800 && (state.FirestarterLeft > state.GCD || CanCast(state, strategy, 3.5f)))
                        return AID.Fire3;
                    else if (CanCast(state, strategy, 3.5f))
                        return AID.Blizzard3;
                    return AID.None; // chill?..
                }
            }
        }
        else
        {
            // before L35, fire/blizzard 1/2 cast under wrong element reset stance - so we use transpose rotation
            if (!CanCast(state, strategy, 3)) // TODO: B2/F2 are 3s, B1/F1 are 2.5s
            {
                // TODO: this is not really correct, we could have thundercloud, but w/e...
                if (state.Unlocked(AID.Scathe) && state.CurMP >= 800)
                    return AID.Scathe;
            }
            else if (strategy.NumAOETargets >= 3 && state.Unlocked(AID.Blizzard2))
            {
                if (!strategy.ForbidDOTs && state.Unlocked(AID.Thunder2) && state.TargetThunderLeft <= state.GCD)
                    return AID.Thunder2;
                return state.Unlocked(AID.Fire2) ? TransposeRotationGCD(state, AID.Blizzard2, 800, AID.Fire2, 1500) : AID.Blizzard2;
            }
            else
            {
                if (!strategy.ForbidDOTs && state.Unlocked(AID.Thunder1) && state.TargetThunderLeft <= state.GCD)
                    return AID.Thunder1;
                return state.Unlocked(AID.Fire1) ? TransposeRotationGCD(state, AID.Blizzard1, 400, AID.Fire1, 800) : AID.Blizzard1;
            }
        }
        return AID.None;
    }

    public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
    {
        if (state.Unlocked(AID.Blizzard3))
        {
            // L35-Lxx: weave manafont in free slots (TODO: is that right?..)
            if (deadline >= 10000 && strategy.ForceMovementIn < 5 && state.Unlocked(AID.Swiftcast) && state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline)) // TODO: better swiftcast condition...
                return ActionID.MakeSpell(AID.Swiftcast);
            if (state.Unlocked(AID.Manafont) && state.CanWeave(CDGroup.Manafont, 0.6f, deadline) && state.CurMP <= 7000)
                return ActionID.MakeSpell(AID.Manafont);
        }
        else
        {
            // before L35, use transpose to swap between elemental states
            // MP thresholds are not especially meaningful (they should work for both ST and AOE), who cares about low level...
            // we could also use manafont, but again who cares
            if (state.Unlocked(AID.Transpose) && state.CanWeave(CDGroup.Transpose, 0.6f, deadline) && (state.ElementalLevel < 0 && state.CurMP >= 9200 || state.ElementalLevel > 0 && state.CurMP < 3600))
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
                return state.Unlocked(AID.Transpose) ? AID.None : fireSpell;
        }
        else if (state.ElementalLevel > 0)
        {
            // continue fire1 spam until oom, then swap to ice
            if (state.CurMP >= fireCost * 2 + iceCost * 3 / 4)
                return fireSpell;
            else
                return state.Unlocked(AID.Transpose) ? AID.None : iceSpell;
        }
        else
        {
            // dropped buff => fire if have some mana (TODO: better limit), blizzard otherwise
            return state.CurMP < fireCost * 3 + iceCost * 3 / 4 ? iceSpell : fireSpell;
        }
    }
}
