// CONTRIB: contributions by lazylemo, needs final pass
namespace BossMod.DRG;

public static class Rotation
{
    // full state needed for determining next action
    public class State : CommonRotation.PlayerState
    {
        public int FirstmindFocusCount; // 2 max
        public int EyeCount; // 2 max
        public float LifeOfTheDragonLeft; // 20 max
        public float FangAndClawBaredLeft; // 30 max
        public float WheelInMotionLeft; // 30 max
        public float DraconianFireLeft; // 30 max
        public float DiveReadyLeft; // 15 max
        public float PowerSurgeLeft; // 30 max
        public float LanceChargeLeft; // 20 max
        public float RightEyeLeft; // 20 max
        public float LifeSurgeLeft; // 5 max
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

        // statuses
        public SID ExpectedChaoticSpring => Unlocked(AID.ChaoticSpring) ? SID.ChaoticSpring : SID.ChaosThrust;

        public AID ComboLastMove => (AID)ComboLastAction;

        public State(WorldState ws) : base(ws) { }

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"FF={FirstmindFocusCount}, LotD={EyeCount}/{LifeOfTheDragonLeft:f3}, ComboEx={FangAndClawBaredLeft:f3}/{WheelInMotionLeft:f3}, DFire={DraconianFireLeft:f3}, Dive={DiveReadyLeft:f3}, RB={RaidBuffsLeft:f3}, PS={PowerSurgeLeft:f3}, LC={LanceChargeLeft:f3}, Eye={RightEyeLeft:f3}, TN={TrueNorthLeft:f3}, CT={TargetChaosThrustLeft:f3}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}, SCD={CD(CDGroup.Stardiver)}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public enum TrueNorthUse : uint
        {
            Automatic = 0,

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1,
            [PropertyDisplay("Force", 0x8000ff00)]
            Force = 2,
        }
        public enum SpineShatteruse : uint
        {
            Automatic = 0, // always keep one charge reserved, use other charges under raidbuffs or prevent overcapping

            [PropertyDisplay("Forbid automatic use", 0x800000ff)]
            Forbid = 1, // forbid until window end

            [PropertyDisplay("Use all charges ASAP", 0x8000ff00)]
            Force = 2, // use all charges immediately, don't wait for raidbuffs

            [PropertyDisplay("Use all charges except one ASAP", 0x80ff0000)]
            ForceReserve = 3, // if 2+ charges, use immediately

            [PropertyDisplay("Use as gapcloser if outside melee range", 0x80ff00ff)]
            UseOutsideMelee = 4, // use immediately if outside melee range
        }

        public TrueNorthUse TrueNorthStrategy;
        public SpineShatteruse SpineShatterStrategy; // how are we supposed to use spineshatter dive
        public int NumAOEGCDTargets; // range 10 width 4 rect
        public bool UseAOERotation;

        public override string ToString()
        {
            return $"";
        }

        public void ApplyStrategyOverrides(uint[] overrides)
        {
            if (overrides.Length >= 8)
            {
                TrueNorthStrategy = (TrueNorthUse)overrides[0];
                SpineShatterStrategy = (SpineShatteruse)overrides[1];

            }
            else
            {
                TrueNorthStrategy = TrueNorthUse.Automatic;
                SpineShatterStrategy = SpineShatteruse.Automatic;
            }
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
        if (state.LifeSurgeLeft > state.GCD)
            return false;

        if (state.Unlocked(TraitID.EnhancedLifeSurge))
        {
            float chargeCapIn = state.CD(CDGroup.LifeSurge) - (state.Unlocked(TraitID.EnhancedLifeSurge) ? 0 : 40);
            if (state.RightEyeLeft > state.GCD
                && state.LanceChargeLeft > state.GCD
                && ((state.WheelInMotionLeft > state.GCD) || (state.FangAndClawBaredLeft > state.GCD))
                && chargeCapIn < state.GCD + 2.5
                && state.CD(CDGroup.BattleLitany) > 0
                && state.CD(CDGroup.Geirskogul) > 0)
                return true;

            if (state.ComboLastMove == AID.VorpalThrust
                && chargeCapIn < state.GCD + 2.5
                && state.CD(CDGroup.BattleLitany) < 100
                && state.CD(CDGroup.DragonSight) < 100
                && state.CD(CDGroup.LanceCharge) < 40
                && state.CD(CDGroup.Geirskogul) > 0)
                return true;
        }

        if (strategy.UseAOERotation)
        {
            // for aoe rotation, just use LS on last unlocked combo action
            return state.Unlocked(AID.CoerthanTorment) ? state.ComboLastMove == AID.SonicThrust
                : state.Unlocked(AID.SonicThrust) ? state.ComboLastMove is AID.DoomSpike or AID.DraconianFury
                : true;
        }

        if (state.Unlocked(AID.FullThrust) && state.Unlocked(TraitID.EnhancedLifeSurge) && state.LanceChargeLeft > state.GCD)
        {
            // L26+: our most damaging action is FT, which is the third action in damaging combo
            return state.ComboLastMove == AID.VorpalThrust;
        }

        // TODO: L64+
        if (state.Unlocked(AID.FullThrust) && !state.Unlocked(TraitID.EnhancedLifeSurge))
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

    public static bool UseSpineShatterDive(State state, Strategy strategy)
    {
        switch (strategy.SpineShatterStrategy)
        {
            case Strategy.SpineShatteruse.Forbid:
                return false;
            case Strategy.SpineShatteruse.Force:
                return true;
            case Strategy.SpineShatteruse.ForceReserve:
                return state.CD(CDGroup.SpineshatterDive) <= 60 + state.AnimationLock;
            case Strategy.SpineShatteruse.UseOutsideMelee:
                return state.RangeToTarget > 3;
            default:
                if (strategy.CombatTimer < 0 || strategy.PositionLockIn <= state.AnimationLock)
                    return false; // Combat or Position restrictions

                if (state.Unlocked(TraitID.EnhancedSpineshatterDive))
                {
                    // Enhanced Spineshatter Dive logic
                    if (state.RightEyeLeft > state.AnimationLock)
                        return true; // Use all charges under RightEyeLeft buff
                }
                else
                {
                    // Regular Spineshatter Dive logic
                    if (state.LanceChargeLeft > state.AnimationLock)
                        return true; // Use when there is Lance Charge left
                }

                return false; // Default: Don't use in other cases
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

    public static bool ShouldUseTrueNorth(State state, Strategy strategy)
    {
        switch (strategy.TrueNorthStrategy)
        {
            case Strategy.TrueNorthUse.Delay:
                return false;

            default:
                if (!state.TargetingEnemy)
                    return false;
                if (state.TrueNorthLeft > state.AnimationLock || state.RightEyeLeft > state.AnimationLock)
                    return false;
                if (GetNextPositional(state, strategy).Item2 && strategy.NextPositionalCorrect)
                    return false;
                if (GetNextPositional(state, strategy).Item2 && !strategy.NextPositionalCorrect)
                    return true;
                return false;
        }
    }

    public static bool ShouldUseGeirskogul(State state, Strategy strategy)
    {
        if (state.EyeCount == 2 && state.CD(CDGroup.LanceCharge) < 40)
            return false;
        if (state.EyeCount == 2 && state.LanceChargeLeft > state.AnimationLock)
            return true;
        if (state.EyeCount == 1 && state.LanceChargeLeft > state.AnimationLock)
        {
            if (state.DiveReadyLeft > state.AnimationLock && state.CD(CDGroup.HighJump) > 10)
                return false;
        }
        if (state.EyeCount != 2 && state.CD(CDGroup.LanceCharge) < 40)
        {
                return true;
        }
        if (state.EyeCount == 0)
        {
            return true;
        }
        return true;
    }   

    public static bool ShouldUseWyrmWindThrust(State state, Strategy strategy)
    {
        bool nextGCDisRaiden = state.DraconianFireLeft > state.AnimationLock && (state.ComboLastMove == AID.WheelingThrust || state.ComboLastMove == AID.FangAndClaw) && state.WheelInMotionLeft < state.AnimationLock && state.FangAndClawBaredLeft < state.AnimationLock;
        if (state.FirstmindFocusCount >= 2 && state.CD(CDGroup.LanceCharge) > 10)
            return true;
        if (state.FirstmindFocusCount >= 2 && state.LanceChargeLeft > state.AnimationLock)
            return true;
        if (state.FirstmindFocusCount >= 2 && state.CD(CDGroup.LanceCharge) < 10 && !nextGCDisRaiden)
            return false;
        return false;
    }
    
    public static AID GetNextBestGCD(State state, Strategy strategy)
    {
        // prepull
        if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f)
            return AID.None;

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
        bool canJump = strategy.PositionLockIn > state.AnimationLock;
        bool wantSpineShatter = state.Unlocked(AID.SpineshatterDive) && state.TargetingEnemy && UseSpineShatterDive(state, strategy);


        if (state.PowerSurgeLeft > state.GCD)
        {
            if (state.Unlocked(AID.LanceCharge) && state.CanWeave(CDGroup.LanceCharge, 0.6f, deadline - state.OGCDSlotLength) && ((state.CD(CDGroup.DragonSight) < state.GCD) || (state.CD(CDGroup.DragonSight) < 65) && (state.CD(CDGroup.DragonSight) > 55)))
                return ActionID.MakeSpell(AID.LanceCharge);
            if (state.Unlocked(AID.DragonSight) && state.CanWeave(CDGroup.DragonSight, 0.6f, deadline) && state.CD(CDGroup.BattleLitany) < state.GCD + 2.5)
                return ActionID.MakeSpell(AID.DragonSight);
            if (state.Unlocked(AID.BattleLitany) && state.CanWeave(CDGroup.BattleLitany, 0.6f, deadline))
                return ActionID.MakeSpell(AID.BattleLitany);
            // life surge on most damaging gcd
            if (state.Unlocked(AID.LifeSurge) && state.CanWeave(state.CD(CDGroup.LifeSurge) - 45, 0.6f, deadline) && UseLifeSurge(state, strategy))
                return ActionID.MakeSpell(AID.LifeSurge);

            // TODO: better buff conditions, reconsider priorities
            if (state.LifeOfTheDragonLeft > state.AnimationLock && state.CanWeave(CDGroup.Nastrond, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Nastrond);

            if (state.CD(CDGroup.LanceCharge) > 5 && state.CD(CDGroup.DragonSight) > 5 && state.CD(CDGroup.BattleLitany) > 5)
            {
                if (state.CanWeave(CDGroup.WyrmwindThrust, 0.6f, deadline) && ShouldUseWyrmWindThrust(state, strategy) && (GetNextBestGCD(state, strategy) == AID.DraconianFury || GetNextBestGCD(state, strategy) == AID.RaidenThrust))
                    return ActionID.MakeSpell(AID.WyrmwindThrust);
                if (state.Unlocked(AID.Geirskogul) && state.CanWeave(CDGroup.Geirskogul, 0.6f, deadline) && ShouldUseGeirskogul(state, strategy))
                    return ActionID.MakeSpell(AID.Geirskogul);
                if (canJump && state.Unlocked(AID.Jump) && state.CanWeave(state.Unlocked(AID.HighJump) ? CDGroup.HighJump : CDGroup.Jump, 0.8f, deadline))
                    return ActionID.MakeSpell(state.BestJump);
                if (state.DiveReadyLeft > state.AnimationLock && state.CanWeave(CDGroup.MirageDive, 0.6f, deadline) && state.EyeCount == 1 && state.CD(CDGroup.Geirskogul) < state.AnimationLock && state.LanceChargeLeft > state.AnimationLock)
                    return ActionID.MakeSpell(AID.MirageDive);
                if (canJump && state.Unlocked(AID.DragonfireDive) && state.CanWeave(CDGroup.DragonfireDive, 0.8f, deadline))
                    return ActionID.MakeSpell(AID.DragonfireDive);
                if (wantSpineShatter && state.CanWeave(state.CD(CDGroup.SpineshatterDive), 0.8f, deadline))
                    return ActionID.MakeSpell(AID.SpineshatterDive);
                if (canJump && state.Unlocked(AID.Stardiver) && state.LifeOfTheDragonLeft > state.AnimationLock && state.CanWeave(CDGroup.Stardiver, 1.5f, deadline))
                    return ActionID.MakeSpell(AID.Stardiver);
                if (state.CanWeave(CDGroup.WyrmwindThrust, 0.6f, deadline) && ShouldUseWyrmWindThrust(state, strategy))
                    return ActionID.MakeSpell(AID.WyrmwindThrust);
                if (wantSpineShatter && state.RangeToTarget > 3)
                    return ActionID.MakeSpell(AID.SpineshatterDive);
                //if (wantSpineShatter && state.LifeOfTheDragonLeft < state.AnimationLock && state.CanWeave(state.CD(CDGroup.SpineshatterDive) - 60, 0.8f, deadline))
                //    return ActionID.MakeSpell(AID.SpineshatterDive);
                //if (wantSpineShatter && state.LifeOfTheDragonLeft > state.AnimationLock && state.CD(CDGroup.Stardiver) > 0 && state.CanWeave(state.CD(CDGroup.SpineshatterDive) - 60, 0.8f, deadline))
                //    return ActionID.MakeSpell(AID.SpineshatterDive);
                if (wantSpineShatter && state.CanWeave(state.CD(CDGroup.SpineshatterDive) - 60, 0.8f, deadline))
                    return ActionID.MakeSpell(AID.SpineshatterDive);
                //if (state.DiveReadyLeft > state.AnimationLock && state.CanWeave(CDGroup.MirageDive, 0.6f, deadline) && state.EyeCount != 2 && state.LifeOfTheDragonLeft > state.AnimationLock && state.CD(CDGroup.Stardiver) > 0)
                //    return ActionID.MakeSpell(AID.MirageDive);
                //if (state.DiveReadyLeft > state.AnimationLock && state.CanWeave(CDGroup.MirageDive, 0.6f, deadline) && state.EyeCount != 2 && state.LifeOfTheDragonLeft < state.AnimationLock)
                //    return ActionID.MakeSpell(AID.MirageDive);
                if (state.DiveReadyLeft > state.AnimationLock && state.CanWeave(CDGroup.MirageDive, 0.6f, deadline) && (state.EyeCount != 2 || state.DiveReadyLeft < state.GCD))
                    return ActionID.MakeSpell(AID.MirageDive);
            }
        }

        if (ShouldUseTrueNorth(state, strategy) && state.CanWeave(CDGroup.TrueNorth - 45, 0.6f, deadline) && !strategy.UseAOERotation && state.GCD < 0.8)
            return ActionID.MakeSpell(AID.TrueNorth);

        // no suitable oGCDs...
        return new();
    }
}
