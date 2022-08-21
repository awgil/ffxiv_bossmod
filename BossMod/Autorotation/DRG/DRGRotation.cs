namespace BossMod.DRG
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public float PowerSurgeLeft; // 30 max

            // upgrade paths
            public AID BestHeavensThrust => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : AID.FullThrust;
            public AID BestChaoticSpring => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : AID.ChaosThrust;
            public AID BestJump => Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump;

            public AID ComboLastMove => (AID)ComboLastAction;

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, PS={PowerSurgeLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public int NumAOEGCDTargets; // range 10 width 4 rect
        }

        public static bool UseLifeSurge(State state)
        {
            if (state.Unlocked(AID.FullThrust))
                return state.ComboLastMove == AID.VorpalThrust;
            else if (state.Unlocked(AID.Disembowel))
                return state.ComboLastMove == AID.TrueThrust && state.PowerSurgeLeft >= state.GCD; // TODO: proper threshold
            else
                return state.ComboLastMove == AID.TrueThrust;
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            // TODO: L45+
            // TODO: better AOE condition
            if (strategy.NumAOEGCDTargets >= 3 && state.PowerSurgeLeft > state.GCD)
            {
                return AID.DoomSpike;
            }
            else
            {
                return state.ComboLastMove switch
                {
                    AID.TrueThrust => state.Unlocked(AID.Disembowel) && state.PowerSurgeLeft < state.GCD + 5 ? AID.Disembowel : state.Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust, // TODO: better threshold (probably depends on combo length?)
                    AID.VorpalThrust => state.Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust,
                    _ => AID.TrueThrust
                };
            }
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // TODO: better buff conditions...
            if (state.Unlocked(AID.LanceCharge) && state.CanWeave(CDGroup.LanceCharge, 0.6f, deadline))
                return ActionID.MakeSpell(AID.LanceCharge);
            if (state.Unlocked(AID.Jump) && state.CanWeave(state.Unlocked(AID.HighJump) ? CDGroup.HighJump : CDGroup.Jump, 0.8f, deadline))
                return ActionID.MakeSpell(state.BestJump);

            // 2. life surge on most damaging gcd (TODO: reconsider condition, it's valid until L26...)
            if (state.Unlocked(AID.LifeSurge) && state.CanWeave(state.CD(CDGroup.LifeSurge) - 45, 0.6f, deadline) && UseLifeSurge(state))
                return ActionID.MakeSpell(AID.LifeSurge);

            // no suitable oGCDs...
            return new();
        }
    }
}
