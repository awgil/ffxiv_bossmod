namespace BossMod.SCH
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public Actor? Fairy;
            public float SwiftcastLeft; // 0 if buff not up, max 10

            public State(float[] cooldowns) : base(cooldowns) { }

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, ..., PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
        }

        public static AID GetBroilAction(State state)
        {
            return state.Unlocked(MinLevel.Broil2)
                ? (state.Unlocked(MinLevel.Broil4) ? AID.Broil4 : state.Unlocked(MinLevel.Broil3) ? AID.Broil3 : AID.Broil2)
                : (state.Unlocked(MinLevel.Broil1) ? AID.Broil1 : AID.Ruin1);
        }

        public static AID GetBioAction(State state)
        {
            return state.Unlocked(MinLevel.Biolysis) ? AID.Biolysis : state.Unlocked(MinLevel.Bio2) ? AID.Bio2 : AID.Bio1;
        }

        public static AID GetArtOfWarAction(State state)
        {
            return state.Unlocked(MinLevel.ArtOfWar2) ? AID.ArtOfWar2 : AID.ArtOfWar1;
        }

        public static AID GetNextBestSTHealGCD(State state)
        {
            return state.Unlocked(MinLevel.Adloquium) && state.CurMP >= 1000 ? AID.Adloquium : AID.Physick;
        }

        public static AID GetNextBestSTDamageGCD(State state, bool moving, bool allowDOT)
        {
            // TODO: priorities change at L38 (ruin2), L46, L54, L64, L72, L82
            if (state.Unlocked(MinLevel.Bio2))
            {
                // L26: bio2 on all targets is more important than ruin
                return allowDOT ? AID.Bio2 : !moving ? AID.Ruin1 : AID.None;
            }
            else if (state.Unlocked(MinLevel.Bio1))
            {
                // L2: bio1 is only used on the move (TODO: it is slightly more potency than ruin on single target, but only if it ticks to the end)
                return !moving ? AID.Ruin1 : allowDOT ? AID.Bio1 : AID.None;
            }
            else
            {
                return !moving ? AID.Ruin1 : AID.None;
            }
        }
    }
}
