namespace BossMod.BRD
{
    public static class Rotation
    {
        public enum Song { None, MagesBallad, ArmysPaeon, WanderersMinuet }

        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public Song ActiveSong;
            public float ActiveSongLeft; // 45 max
            public float StraightShotLeft;
            public float RagingStrikesLeft;
            public float BarrageLeft;
            public float PelotonLeft; // 30 max
            public float TargetCausticLeft;
            public float TargetStormbiteLeft;

            // upgrade paths
            public AID BestBurstShot => Unlocked(AID.BurstShot) ? AID.BurstShot : AID.HeavyShot;
            public AID BestRefulgentArrow => Unlocked(AID.RefulgentArrow) ? AID.RefulgentArrow : AID.StraightShot;
            public AID BestCausticBite => Unlocked(AID.CausticBite) ? AID.CausticBite : AID.VenomousBite;
            public AID BestStormbite => Unlocked(AID.Stormbite) ? AID.Stormbite : AID.Windbite;
            public AID BestLadonsbite => Unlocked(AID.Ladonsbite) ? AID.Ladonsbite : AID.QuickNock;

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"S={ActiveSong}/{ActiveSongLeft:f1}, RB={RaidBuffsLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public int NumLadonsbiteTargets; // range 12 90-degree cone
            public int NumRainOfDeathTargets; // range 8 circle around target

            public override string ToString()
            {
                return $"AOE={NumRainOfDeathTargets}/{NumLadonsbiteTargets}, no-dots={ForbidDOTs}";
            }
        }

        public static bool RefreshDOT(State state, float timeLeft) => timeLeft < state.GCD + 3.0f; // TODO: tweak threshold so that we don't overwrite or miss ticks...

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            // TODO: L56+
            if (strategy.NumLadonsbiteTargets >= 2 && state.Unlocked(AID.QuickNock))
            {
                return state.BestLadonsbite;
            }
            else
            {
                // 1. dots
                if (!strategy.ForbidDOTs && state.Unlocked(AID.Windbite) && RefreshDOT(state, state.TargetStormbiteLeft))
                    return state.BestStormbite;
                if (!strategy.ForbidDOTs && state.Unlocked(AID.VenomousBite) && RefreshDOT(state, state.TargetCausticLeft))
                    return state.BestCausticBite;

                // 2. straight shot if possible
                if (state.StraightShotLeft > state.GCD)
                    return state.BestRefulgentArrow;

                // 3. heavy shot
                return state.BestBurstShot;
            }
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // TODO: this should be improved... correct for low levels
            if (state.ActiveSong == Song.None && state.Unlocked(AID.MagesBallad) && state.CanWeave(CDGroup.MagesBallad, 0.6f, deadline))
                return ActionID.MakeSpell(AID.MagesBallad);
            if (state.ActiveSong == Song.None && state.Unlocked(AID.ArmysPaeon) && state.CanWeave(CDGroup.ArmysPaeon, 0.6f, deadline))
                return ActionID.MakeSpell(AID.ArmysPaeon);

            if (state.Unlocked(AID.RagingStrikes) && state.CanWeave(CDGroup.RagingStrikes, 0.6f, deadline))
                return ActionID.MakeSpell(AID.RagingStrikes);
            if (state.Unlocked(AID.BattleVoice) && state.CanWeave(CDGroup.BattleVoice, 0.6f, deadline))
                return ActionID.MakeSpell(AID.BattleVoice);
            if (strategy.NumLadonsbiteTargets < 2 && state.StraightShotLeft <= state.GCD && state.Unlocked(AID.Barrage) && state.CanWeave(CDGroup.Barrage, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Barrage);
            if (state.Unlocked(AID.Bloodletter) && state.CanWeave(state.CD(CDGroup.Bloodletter) - 60, 0.6f, deadline))
                return ActionID.MakeSpell(strategy.NumRainOfDeathTargets >= 2 ? AID.RainOfDeath : AID.Bloodletter);

            // no suitable oGCDs...
            return new();
        }
    }
}
