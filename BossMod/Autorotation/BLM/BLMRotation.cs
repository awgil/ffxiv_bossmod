namespace BossMod.BLM
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public int ElementalLevel; // -3 (umbral ice 3) to +3 (astral fire 3)
            public float ElementalLeft; // 0 if elemental level is 0, otherwise buff duration, max 15
            public float TargetThunderLeft; // TODO: this shouldn't be here...

            public State(float[] cooldowns) : base(cooldowns) { }

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, Elem={ElementalLevel}/{ElementalLeft:f1}, Thunder={TargetThunderLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public bool AOE;
            public bool Moving;
            public bool UseManaward;
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

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (strategy.Moving)
            {
                // TODO: not sure about this...
                if (state.Unlocked(MinLevel.Scathe) && state.CurMP >= 800)
                    return AID.Scathe;
            }
            else if (strategy.AOE && state.Unlocked(MinLevel.Blizzard2))
            {
                // TODO: this works until ~L40
                if (state.Unlocked(MinLevel.Thunder2) && state.TargetThunderLeft <= state.GCD)
                    return AID.Thunder2;
                return state.Unlocked(MinLevel.Fire2) ? TransposeRotationGCD(state, AID.Blizzard2, 800, AID.Fire2, 1500) : AID.Blizzard2;
            }
            else
            {
                // TODO: this works until ~L35
                // 1. thunder (TODO: tweak threshold so that we don't overwrite or miss ticks...)
                if (state.Unlocked(MinLevel.Thunder1) && state.TargetThunderLeft <= state.GCD)
                    return AID.Thunder1;
                return state.Unlocked(MinLevel.Fire1) ? TransposeRotationGCD(state, AID.Blizzard1, 400, AID.Fire1, 800) : AID.Blizzard1;
            }
            return AID.None;
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            if (strategy.UseManaward && state.Unlocked(MinLevel.Manaward) && state.CanWeave(CDGroup.Manaward, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Manaward);

            // TODO: this is not really correct...
            if (state.Unlocked(MinLevel.Manafont) && state.CanWeave(CDGroup.Manafont, 0.6f, deadline) && state.CurMP < 5000)
                return ActionID.MakeSpell(AID.Manafont);

            if (state.Unlocked(MinLevel.Transpose) && state.CanWeave(CDGroup.Transpose, 0.6f, deadline) && (state.ElementalLevel < 0 && state.CurMP >= 9200 || state.ElementalLevel > 0 && state.CurMP < 3600))
                return ActionID.MakeSpell(AID.Transpose);

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
