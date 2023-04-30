namespace BossMod.DRG
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public int EyeCount; // 2 max
            public float LifeOfTheDragonLeft; // 20 max
            public float FangAndClawBaredLeft; // 30 max
            public float WheelInMotionLeft; // 30 max
            public float DraconianFireLeft; // 30 max
            public float DiveReadyLeft; // 15 max
            public float PowerSurgeLeft; // 30 max
            public float LanceChargeLeft; // 20 max
            public float RightEyeLeft; // 20 max
            public float TrueNorthLeft; // 10 max
            public float TargetChaosThrustLeft; // 24 max

            // upgrade paths
            public AID BestHeavensThrust => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : AID.FullThrust;
            public AID BestChaoticSpring => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : AID.ChaosThrust;
            public AID BestJump => Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump;
            // proc replacements
            public AID BestGeirskogul => LifeOfTheDragonLeft > AnimationLock ? AID.Nastrond : AID.Geirskogul;
            public AID BestTrueThrust => DraconianFireLeft > GCD ? AID.RaidenThrust : AID.TrueThrust;
            public AID BestDoomSpike => DraconianFireLeft > GCD && Unlocked(AID.DraconianFury) ? AID.DraconianFury : AID.DoomSpike;

            public AID ComboLastMove => (AID)ComboLastAction;

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"LotD={EyeCount}/{LifeOfTheDragonLeft:f3}, ComboEx={FangAndClawBaredLeft:f3}/{WheelInMotionLeft:f3}, DFire={DraconianFireLeft:f3}, Dive={DiveReadyLeft:f3}, RB={RaidBuffsLeft:f3}, PS={PowerSurgeLeft:f3}, LC={LanceChargeLeft:f3}, Eye={RightEyeLeft:f3}, TN={TrueNorthLeft:f3}, CT={TargetChaosThrustLeft:f3}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public int NumAOEGCDTargets; // range 10 width 4 rect
            public bool UseAOERotation;

            public override string ToString()
            {
                return $"AOE={NumAOEGCDTargets}, no-dots={ForbidDOTs}";
            }
        }

        public static bool RefreshDOT(State state, float timeLeft) => timeLeft < state.GCD; // TODO: tweak threshold so that we don't overwrite or miss ticks...

        public static bool UseBuffingCombo(State state, Strategy strategy, bool predict)
        {
            // the selected action will happen in GCD, and it will be the *second* action in the combo
            // note if we're in 'predict' mode (that is, our next action is TT, but we try to predict which branch we'll take), the selected action will happen in second GCD instead
            var secondActionIn = state.GCD + (predict ? 2.5f : 0);
            if (state.Unlocked(AID.ChaosThrust))
            {
                // L50-55: at this point, we have 3-action buff and pure damage combos
                // damage combo (vorpal + full) is 170+250+400 = 820 potency
                // buff combo (disembowel + chaos) is 170+210+260+40*N = 640+40*N potency (assuming positional, -40 otherwise), where N is num ticks
                // dot is 8 ticks (24 sec), assuming 2.5 gcd, we can either do 1:2 rotation (9 gcds = 22.5s => reapplying dot at ~1.5 left) or 1:3 rotation (12 gcds = 30s => dropping a dot for 6 seconds)
                // these seem to be really close (285 vs 284.44 p/gcd); balance recommends 1:3 rotation however
                // we use dot duration rather than buff duration, because it works for two-target scenario

                // L56-57: at this point, we have 3-action buff and 4-action damage combos
                // damage combo (vorpal + full + f&c) is 170+250+400+300 = 1120 potency (asssuming positional)
                // buff combo (disembowel + chaos) is same 640+40*N potency as above
                // most logical is 1:2 rotation (11 gcds = 27.5s => dropping a dot for 3.5s), since 1:1 would clip 3 ticks
                // it also works nicely for 2 targets (25s rotation, meaning almost full dot uptime on both targets)

                // L58-63: at this point, we have 4-action buff and pure damage combos
                // damage combo (vorpal + full + f&c) is same 1120 potency as above
                // buff combo (disembowel + chaos + wheeling) is +300 = 940+40*N potency
                // most logical is 1:2 rotation (12 gcds = 30s => dropping a dot for 6s), since 1:1 would clip 2 ticks

                // L64+: at this point, we have 5-action buff and pure damage combos
                // most logical is 1:1 rotation (10 gcds = 25s => dropping a dot for 1s)

                // if we use buff combo, next dot refresh be second action - that's 2.5s + however many ticks we are ok with overwriting (0 in all cases, for now)
                return !strategy.ForbidDOTs && state.TargetChaosThrustLeft < secondActionIn + 2.5f;
            }
            else if (state.Unlocked(AID.Disembowel))
            {
                // at this point we have 2-action buff combo and 2 or 3-action pure damage combo
                // if we execute pure damage combo, next chance to disembowel will be in GCD-remaining (vorpal) + N (full, if unlocked, then true, then disembowel) gcds
                // we want to avoid dropping power surge buff, so that disembowel is still buffed
                var damageComboLength = state.Unlocked(AID.FullThrust) ? 3 : 2;
                return state.PowerSurgeLeft < secondActionIn + 2.5f * damageComboLength;
            }
            else
            {
                return false; // there is no buff combo yet...
            }
        }

        public static bool UseLifeSurge(State state, Strategy strategy)
        {
            if (strategy.UseAOERotation)
            {
                // for aoe rotation, just use LS on last unlocked combo action
                return state.Unlocked(AID.CoerthanTorment) ? state.ComboLastMove == AID.SonicThrust
                    : state.Unlocked(AID.SonicThrust) ? state.ComboLastMove is AID.DoomSpike or AID.DraconianFury
                    : true;
            }

            // TODO: L64+
            if (state.Unlocked(AID.FullThrust))
            {
                // L26+: our most damaging action is FT, which is the third action in damaging combo
                return state.ComboLastMove == AID.VorpalThrust;
            }
            else
            {
                // L6+: our most damaging action is VT, which is the second action in damaging combo (which is the only combo we have before L18)
                return state.ComboLastMove == AID.TrueThrust && !UseBuffingCombo(state, strategy, false);
            }
        }

        public static (Positional, bool) GetNextPositional(State state, Strategy strategy)
        {
            if (strategy.UseAOERotation)
                return default; // AOE rotation has no positionals

            if (state.Unlocked(AID.FangAndClaw))
            {
                // we have flank positional (fang and claw, 4th in damaging combo) and rear positionals (chaos thrust & wheeling thrust, 3rd and 4th in buffing combo)
                // if our next action is not a positional, then just use next action in the current combo; for True Thrust, predict what branch we'll take using UseBuffingCombo
                if (state.FangAndClawBaredLeft > state.GCD)
                    return (Positional.Flank, true);
                if (state.WheelInMotionLeft > state.GCD)
                    return (Positional.Rear, true);
                var buffingCombo = state.ComboLastMove switch
                {
                    AID.TrueThrust or AID.RaidenThrust => UseBuffingCombo(state, strategy, false),
                    AID.VorpalThrust => false,
                    AID.Disembowel => true,
                    _ => UseBuffingCombo(state, strategy, true)
                };
                return (buffingCombo ? Positional.Rear : Positional.Flank, state.ComboLastMove == AID.Disembowel);
            }
            else if (state.Unlocked(AID.ChaosThrust))
            {
                // the only positional we have at this point is chaos thrust (rear)
                return (Positional.Rear, state.ComboLastMove == AID.Disembowel);
            }
            else
            {
                return default;
            }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (strategy.UseAOERotation)
            {
                return state.ComboLastMove switch
                {
                    AID.DoomSpike or AID.DraconianFury => state.Unlocked(AID.SonicThrust) ? AID.SonicThrust : AID.DoomSpike,
                    AID.SonicThrust => state.Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : AID.DoomSpike,
                    _ => state.BestDoomSpike
                };
            }
            else
            {
                if (state.FangAndClawBaredLeft > state.GCD)
                    return AID.FangAndClaw;
                if (state.WheelInMotionLeft > state.GCD)
                    return AID.WheelingThrust;
                return state.ComboLastMove switch
                {
                    AID.TrueThrust or AID.RaidenThrust => UseBuffingCombo(state, strategy, false) ? AID.Disembowel : state.Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust,
                    AID.VorpalThrust => state.Unlocked(AID.FullThrust) ? state.BestHeavensThrust : AID.TrueThrust,
                    AID.Disembowel => state.Unlocked(AID.ChaosThrust) ? state.BestChaoticSpring : AID.TrueThrust,
                    _ => state.BestTrueThrust
                };
            }
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // life surge on most damaging gcd
            if (state.Unlocked(AID.LifeSurge) && state.CanWeave(state.CD(CDGroup.LifeSurge) - 45, 0.6f, deadline) && UseLifeSurge(state, strategy))
                return ActionID.MakeSpell(AID.LifeSurge);

            // TODO: L90+
            // TODO: better buff conditions, reconsider priorities
            if (state.Unlocked(AID.LanceCharge) && state.CanWeave(CDGroup.LanceCharge, 0.6f, deadline))
                return ActionID.MakeSpell(AID.LanceCharge);
            if (state.Unlocked(AID.DragonSight) && state.CanWeave(CDGroup.DragonSight, 0.6f, deadline))
                return ActionID.MakeSpell(AID.DragonSight);
            if (state.Unlocked(AID.BattleLitany) && state.CanWeave(CDGroup.BattleLitany, 0.6f, deadline))
                return ActionID.MakeSpell(AID.BattleLitany);
            if (state.LifeOfTheDragonLeft > state.AnimationLock && state.CanWeave(CDGroup.Nastrond, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Nastrond);
            if (state.Unlocked(AID.Geirskogul) && state.CanWeave(CDGroup.Geirskogul, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Geirskogul);
            if (state.DiveReadyLeft > state.AnimationLock && state.CanWeave(CDGroup.MirageDive, 0.6f, deadline))
                return ActionID.MakeSpell(AID.MirageDive);

            bool canJump = strategy.PositionLockIn > state.AnimationLock;
            if (canJump && state.Unlocked(AID.Jump) && state.CanWeave(state.Unlocked(AID.HighJump) ? CDGroup.HighJump : CDGroup.Jump, 0.8f, deadline))
                return ActionID.MakeSpell(state.BestJump);
            //if (canJump && state.Unlocked(AID.DragonfireDive) && state.CanWeave(CDGroup.DragonfireDive, 0.8f, deadline))
            //    return ActionID.MakeSpell(AID.DragonfireDive);
            //if (canJump && state.Unlocked(AID.SpineshatterDive) && state.CanWeave(CDGroup.SpineshatterDive, 0.8f, deadline))
            //    return ActionID.MakeSpell(AID.SpineshatterDive);
            if (canJump && state.Unlocked(AID.Stardiver) && state.LifeOfTheDragonLeft > state.AnimationLock && state.CanWeave(CDGroup.Stardiver, 1.5f, deadline))
                return ActionID.MakeSpell(AID.Stardiver);

            // no suitable oGCDs...
            return new();
        }
    }
}
