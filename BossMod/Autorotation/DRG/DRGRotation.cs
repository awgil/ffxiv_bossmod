namespace BossMod.DRG
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public float PowerSurgeLeft; // 30 max
            public float LanceChargeLeft; // 20 max
            public float TrueNorthLeft; // 10 max
            public float TargetChaosThrustLeft; // 24 max

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
                return $"RB={RaidBuffsLeft:f1}, PS={PowerSurgeLeft:f1}, LC={LanceChargeLeft:f1}, TN={TrueNorthLeft:f1}, CT={TargetChaosThrustLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public int NumAOEGCDTargets; // range 10 width 4 rect

            public override string ToString()
            {
                return $"AOE={NumAOEGCDTargets}, no-dots={ForbidDOTs}";
            }
        }

        public static bool RefreshDOT(State state, float timeLeft) => timeLeft < state.GCD + 3.0f; // TODO: tweak threshold so that we don't overwrite or miss ticks...

        public static bool UseLifeSurge(State state)
        {
            if (state.Unlocked(AID.FullThrust))
                return state.ComboLastMove == AID.VorpalThrust;
            else if (state.Unlocked(AID.Disembowel))
                return state.ComboLastMove == AID.TrueThrust && state.PowerSurgeLeft >= state.GCD; // TODO: proper threshold
            else
                return state.ComboLastMove == AID.TrueThrust;
        }

        public static bool UseBuffingCombo(State state, Strategy strategy)
        {
            // the selected action will happen in GCD, and it will be the *second* action in the combo
            // TODO: L56+
            if (state.Unlocked(AID.ChaosThrust))
            {
                // at this point, we have 3-action buff and pure damage combos
                // damage combo (vorpal + full) is 230+250+400 = 880 potency
                // buff combo (disembowel + chaos) is 230+210+260+40*N = 700+40*N potency (assuming positional, -40 otherwise), where N is num ticks
                // dot is 8 ticks (24 sec), assuming 2.5 gcd, we can either do 1:2 rotation (9 gcds = 22.5s => reapplying dot at ~1.5 left) or 1:3 rotation (12 gcds = 30s => dropping a dot for 6 seconds)
                // these seem to be really close (~305 p/gcd average)?.. TODO decide which is better
                // we use dot duration rather than buff duration, because it works for multi-target scenario (refreshing buff is only 40p loss)
                // if we use buff combo, next dot refresh be second action - that's 2.5s, plus we allow overwriting last tick ... (TODO!!!)
                return !strategy.ForbidDOTs && RefreshDOT(state, state.TargetChaosThrustLeft - 5);
            }
            else if (state.Unlocked(AID.Disembowel))
            {
                // at this point we have 2-action buff combo and 2 or 3-action pure damage combo
                // if we execute pure damage combo, next chance to disembowel will be in GCD-remaining (vorpal) + N (full, if unlocked, then true, then disembowel) gcds
                // we want to avoid dropping power surge buff, so that disembowel is still buffed
                var damageComboLength = state.Unlocked(AID.FullThrust) ? 3 : 2;
                return state.PowerSurgeLeft < state.GCD + 2.5f * damageComboLength;
            }
            else
            {
                return false; // there is no buff combo yet...
            }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            // TODO: L56+
            // TODO: better AOE condition
            if (strategy.NumAOEGCDTargets >= 3 && state.PowerSurgeLeft > state.GCD)
            {
                return AID.DoomSpike;
            }
            else
            {
                return state.ComboLastMove switch
                {
                    AID.TrueThrust => UseBuffingCombo(state, strategy) ? AID.Disembowel : state.Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust, // TODO: better threshold (probably depends on combo length?)
                    AID.VorpalThrust => state.Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust,
                    AID.Disembowel => state.Unlocked(AID.ChaosThrust) ? AID.ChaosThrust : AID.TrueThrust,
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
            if (state.Unlocked(AID.DragonfireDive) && state.CanWeave(CDGroup.DragonfireDive, 0.8f, deadline))
                return ActionID.MakeSpell(AID.DragonfireDive);
            if (state.Unlocked(AID.SpineshatterDive) && state.CanWeave(CDGroup.SpineshatterDive, 0.8f, deadline))
                return ActionID.MakeSpell(AID.SpineshatterDive);

            // 2. life surge on most damaging gcd (TODO: reconsider condition, it's valid until L26...)
            if (state.Unlocked(AID.LifeSurge) && state.CanWeave(state.CD(CDGroup.LifeSurge) - 45, 0.6f, deadline) && UseLifeSurge(state))
                return ActionID.MakeSpell(AID.LifeSurge);

            // no suitable oGCDs...
            return new();
        }
    }
}
