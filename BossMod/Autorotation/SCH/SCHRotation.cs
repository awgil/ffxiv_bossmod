namespace BossMod.SCH
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public Actor? Fairy;
            public float SwiftcastLeft; // 0 if buff not up, max 10
            public float TargetBioLeft; // 0 if debuff not up, max 30

            // upgrade paths
            public AID BestBroil => Unlocked(AID.Broil4) ? AID.Broil4 : Unlocked(AID.Broil3) ? AID.Broil3 : Unlocked(AID.Broil2) ? AID.Broil2 : Unlocked(AID.Broil1) ? AID.Broil1 : AID.Ruin1;
            public AID BestBio => Unlocked(AID.Biolysis) ? AID.Biolysis : Unlocked(AID.Bio2) ? AID.Bio2 : AID.Bio1;
            public AID BestArtOfWar => Unlocked(AID.ArtOfWar2) ? AID.ArtOfWar2 : AID.ArtOfWar1;

            // statuses
            public SID ExpectedBio => Unlocked(AID.Biolysis) ? SID.Biolysis : Unlocked(AID.Bio2) ? SID.Bio2 : SID.Bio1;

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, Bio={TargetBioLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public bool Moving;
            public int NumWhisperingDawnTargets; // how many targets would whispering dawn heal (15y around fairy)
            public int NumSuccorTargets; // how many targets would succor heal (15y around self)
        }

        public static bool RefreshDOT(State state, float timeLeft) => timeLeft < state.GCD + 3.0f; // TODO: tweak threshold so that we don't overwrite or miss ticks...

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // TODO: L45+

            // lucid dreaming, if we won't waste mana (TODO: revise mp limit)
            if (state.CurMP <= 8000 && state.Unlocked(AID.LucidDreaming) && state.CanWeave(CDGroup.LucidDreaming, 0.6f, deadline))
                return ActionID.MakeSpell(AID.LucidDreaming);

            // TODO: swiftcast...

            return new();
        }

        public static AID GetNextBestSTHealGCD(State state, Strategy strategy)
        {
            return state.Unlocked(AID.Adloquium) && state.CurMP >= 1000 ? AID.Adloquium : AID.Physick;
        }

        public static AID GetNextBestSTDamageGCD(State state, Strategy strategy)
        {
            // TODO: priorities change at L46, L54, L64, L72, L82
            bool allowRuin = !strategy.Moving || state.SwiftcastLeft > state.GCD;
            if (state.Unlocked(AID.Bio2))
            {
                // L26: bio2 on all targets is more important than ruin
                // L38: cast ruin2 on the move
                return RefreshDOT(state, state.TargetBioLeft) ? AID.Bio2 : allowRuin ? AID.Ruin1 : (state.Unlocked(AID.Ruin2) ? AID.Ruin2 : AID.None);
            }
            else if (state.Unlocked(AID.Bio1))
            {
                // L2: bio1 is only used on the move (TODO: it is slightly more potency than ruin on single target, but only if it ticks to the end)
                return allowRuin ? AID.Ruin1 : RefreshDOT(state, state.TargetBioLeft) ? AID.Bio1 : AID.None;
            }
            else
            {
                return allowRuin ? AID.Ruin1 : AID.None;
            }
        }
    }
}
