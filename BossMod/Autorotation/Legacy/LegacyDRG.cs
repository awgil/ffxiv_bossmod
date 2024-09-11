using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.Legacy;

public sealed class LegacyDRG : LegacyModule
{
    public enum Track { AOE, TrueNorth, SpineshatterDive }
    public enum AOEStrategy { SingleTarget, AutoTargetHitPrimary, AutoTargetHitMost, AutoOnPrimary, ForceAOE }
    public enum TrueNorthStrategy { Automatic, Delay, Force }
    public enum SpineshatterStrategy { Automatic, Forbid, Force, ForceReserve, UseOutsideMelee }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense
        var res = new RotationModuleDefinition("Legacy DRG", "Old pre-refactoring module", "veyn, lazylemo", RotationModuleQuality.WIP, BitMask.Build((int)Class.DRG), 100);

        // TODO: 'force' flavour for continuing vs breaking combo?
        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 90)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AutoTargetHitPrimary, "AutoTargetHitPrimary", "Use aoe actions if profitable select best target that ensures primary target is hit")
            .AddOption(AOEStrategy.AutoTargetHitMost, "AutoTargetHitMost", "Use aoe actions if profitable select a target that ensures maximal number of targets are hit")
            .AddOption(AOEStrategy.AutoOnPrimary, "AutoOnPrimary", "Use aoe actions on primary target if profitable")
            .AddOption(AOEStrategy.ForceAOE, "AOE", "Use aoe rotation on primary target even if it's less total damage than single-target");

        res.Define(Track.TrueNorth).As<TrueNorthStrategy>("TrueN", uiPriority: 80)
            .AddOption(TrueNorthStrategy.Automatic, "Automatic")
            .AddOption(TrueNorthStrategy.Delay, "Delay")
            .AddOption(TrueNorthStrategy.Force, "Force")
            .AddAssociatedActions(DRG.AID.TrueNorth);

        //res.Define(Track.SpineshatterDive).As<SpineshatterStrategy>("SpineShatter", "SSDive", uiPriority: 70)
        //    .AddOption(SpineshatterStrategy.Automatic, "Automatic", "Always keep one charge reserved, use other charges under raidbuffs or prevent overcapping")
        //    .AddOption(SpineshatterStrategy.Forbid, "Forbid", "Forbid automatic use")
        //    .AddOption(SpineshatterStrategy.Force, "Force", "Use all charges ASAP")
        //    .AddOption(SpineshatterStrategy.ForceReserve, "ForceReserve", "Use all charges except one ASAP")
        //    .AddOption(SpineshatterStrategy.UseOutsideMelee, "UseOutsideMelee", "Use as gapcloser if outside melee range")
        //    .AddAssociatedActions(DRG.AID.SpineshatterDive);

        return res;
    }

    // full state needed for determining next action
    public class State(RotationModule module) : CommonState(module)
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
        public Actor? BestAOEGCDTarget;
        public int NumAOEGCDTargets; // range 10 width 4 rect
        public bool UseAOERotation;

        // upgrade paths
        public DRG.AID BestHeavensThrust => Unlocked(DRG.AID.HeavensThrust) ? DRG.AID.HeavensThrust : DRG.AID.FullThrust;
        public DRG.AID BestChaoticSpring => Unlocked(DRG.AID.ChaoticSpring) ? DRG.AID.ChaoticSpring : DRG.AID.ChaosThrust;
        public DRG.AID BestJump => Unlocked(DRG.AID.HighJump) ? DRG.AID.HighJump : DRG.AID.Jump;
        // proc replacements
        public DRG.AID BestGeirskogul => LifeOfTheDragonLeft > AnimationLock ? DRG.AID.Nastrond : DRG.AID.Geirskogul;
        public DRG.AID BestTrueThrust => DraconianFireLeft > GCD ? DRG.AID.RaidenThrust : DRG.AID.TrueThrust;
        public DRG.AID BestDoomSpike => DraconianFireLeft > GCD && Unlocked(DRG.AID.DraconianFury) ? DRG.AID.DraconianFury : DRG.AID.DoomSpike;

        // statuses
        public DRG.SID ExpectedChaoticSpring => Unlocked(DRG.AID.ChaoticSpring) ? DRG.SID.ChaoticSpring : DRG.SID.ChaosThrust;

        public DRG.AID ComboLastMove => (DRG.AID)ComboLastAction;

        public bool Unlocked(DRG.AID aid) => Module.ActionUnlocked(ActionID.MakeSpell(aid));
        public bool Unlocked(DRG.TraitID tid) => Module.TraitUnlocked((uint)tid);

        public override string ToString()
        {
            return $"FF={FirstmindFocusCount}, LotD={EyeCount}/{LifeOfTheDragonLeft:f3}, ComboEx={FangAndClawBaredLeft:f3}/{WheelInMotionLeft:f3}, DFire={DraconianFireLeft:f3}, Dive={DiveReadyLeft:f3}, RB={RaidBuffsLeft:f3}, PS={PowerSurgeLeft:f3}, LC={LanceChargeLeft:f3}, Eye={RightEyeLeft:f3}, TN={TrueNorthLeft:f3}, CT={TargetChaosThrustLeft:f3}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}, SCD={CD(DRG.AID.Stardiver)}";
        }
    }

    private readonly State _state;

    public LegacyDRG(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = World.Client.GetGauge<DragoonGauge>();
        _state.FirstmindFocusCount = gauge.FirstmindsFocusCount;
        _state.EyeCount = gauge.EyeCount;
        _state.LifeOfTheDragonLeft = gauge.LotdState != 0 ? gauge.LotdTimer * 0.001f : 0;

        _state.FangAndClawBaredLeft = _state.StatusDetails(Player, DRG.SID.FangAndClawBared, Player.InstanceID).Left;
        _state.WheelInMotionLeft = _state.StatusDetails(Player, DRG.SID.WheelInMotion, Player.InstanceID).Left;
        _state.DraconianFireLeft = _state.StatusDetails(Player, DRG.SID.DraconianFire, Player.InstanceID).Left;
        _state.DiveReadyLeft = _state.StatusDetails(Player, DRG.SID.DiveReady, Player.InstanceID).Left;
        _state.PowerSurgeLeft = _state.StatusDetails(Player, DRG.SID.PowerSurge, Player.InstanceID).Left;
        _state.LanceChargeLeft = _state.StatusDetails(Player, DRG.SID.LanceCharge, Player.InstanceID).Left;
        _state.RightEyeLeft = _state.StatusDetails(Player, DRG.SID.RightEye, Player.InstanceID).Left;
        _state.TrueNorthLeft = _state.StatusDetails(Player, DRG.SID.TrueNorth, Player.InstanceID).Left;
        _state.LifeSurgeLeft = _state.StatusDetails(Player, DRG.SID.LifeSurge, Player.InstanceID).Left;

        // TODO: multidot support
        //var adjTarget = initial;
        //if (_state.Unlocked(AID.ChaosThrust) && !WithoutDOT(initial.Actor))
        //{
        //    var multidotTarget = Autorot.Hints.PriorityTargets.FirstOrDefault(e => e != initial && !e.ForbidDOTs && e.Actor.Position.InCircle(Player.Position, 5) && WithoutDOT(e.Actor));
        //    if (multidotTarget != null)
        //        adjTarget = multidotTarget;
        //}
        _state.TargetChaosThrustLeft = _state.StatusDetails(primaryTarget, _state.ExpectedChaoticSpring, Player.InstanceID).Left;

        // TODO: auto select aoe target for ogcds
        var aoeStrategy = strategy.Option(Track.AOE).As<AOEStrategy>();
        (_state.BestAOEGCDTarget, _state.NumAOEGCDTargets) = _state.Unlocked(DRG.AID.DoomSpike) ? CheckAOETargeting(aoeStrategy, primaryTarget, 10, NumTargetsHitByAOEGCD, IsHitByAOEGCD) : (null, 0);
        _state.UseAOERotation = _state.NumAOEGCDTargets >= 3 && (_state.Unlocked(DRG.AID.SonicThrust) || _state.PowerSurgeLeft > _state.GCD); // TODO: better AOE condition

        _state.UpdatePositionals(primaryTarget, GetNextPositional(), _state.TrueNorthLeft > _state.GCD || _state.RightEyeLeft > _state.GCD);

        // TODO: refactor all that, it's kinda senseless now
        DRG.AID gcd = GetNextBestGCD(strategy);
        PushResult(gcd, gcd is DRG.AID.DoomSpike or DRG.AID.DraconianFury or DRG.AID.SonicThrust or DRG.AID.CoerthanTorment ? _state.BestAOEGCDTarget : primaryTarget);

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        //if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
        //    ogcd = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline, gcd);
        PushResult(ogcd, /*ogcd == ActionID.MakeSpell(DRG.AID.DragonSight) ? DRG.Definitions.FindBestDragonSightTarget(World, Player) :*/ primaryTarget);
    }

    //protected override void QueueAIActions()
    //{
    //    if (_state.Unlocked(AID.SecondWind))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
    //    if (_state.Unlocked(AID.Bloodbath))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.Bloodbath), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8f);
    //}

    public override string DescribeState() => _state.ToString();

    private int NumTargetsHitByAOEGCD(Actor primary) => Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 10, 2);
    private bool IsHitByAOEGCD(Actor primary, Actor check) => Hints.TargetInAOERect(check, Player.Position, (primary.Position - Player.Position).Normalized(), 10, 2);

    private (Actor?, int) CheckAOETargeting(AOEStrategy strategy, Actor? primaryTarget, float range, Func<Actor, int> numTargets, Func<Actor, Actor, bool> check) => strategy switch
    {
        AOEStrategy.AutoTargetHitPrimary => FindBetterTargetBy(primaryTarget, range, t => primaryTarget == null || check(t, primaryTarget) ? numTargets(t) : 0),
        AOEStrategy.AutoTargetHitMost => FindBetterTargetBy(primaryTarget, range, numTargets),
        AOEStrategy.AutoOnPrimary => (primaryTarget, primaryTarget != null ? numTargets(primaryTarget) : 0),
        AOEStrategy.ForceAOE => (primaryTarget, int.MaxValue),
        _ => (null, 0)
    };

    // old DRGDRotation
    //private bool WithoutDOT(Actor a) => Rotation.RefreshDOT(_state, StatusDetails(a, SID.ChaosThrust, Player.InstanceID).Left);
    //private bool RefreshDOT(float timeLeft) => timeLeft < _state.GCD; // TODO: tweak threshold so that we don't overwrite or miss ticks...

    private bool UseBuffingCombo(bool predict)
    {
        // the selected action will happen in GCD, and it will be the *second* action in the combo
        // note if we're in 'predict' mode (that is, our next action is TT, but we try to predict which branch we'll take), the selected action will happen in second GCD instead
        var secondActionIn = _state.GCD + (predict ? 2.5f : 0);
        if (_state.Unlocked(DRG.AID.ChaosThrust))
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
            return !_state.ForbidDOTs && _state.TargetChaosThrustLeft < secondActionIn + 2.5f;
        }
        else if (_state.Unlocked(DRG.AID.Disembowel))
        {
            // at this point we have 2-action buff combo and 2 or 3-action pure damage combo
            // if we execute pure damage combo, next chance to disembowel will be in GCD-remaining (vorpal) + N (full, if unlocked, then true, then disembowel) gcds
            // we want to avoid dropping power surge buff, so that disembowel is still buffed
            var damageComboLength = _state.Unlocked(DRG.AID.FullThrust) ? 3 : 2;
            return _state.PowerSurgeLeft < secondActionIn + 2.5f * damageComboLength;
        }
        else
        {
            return false; // there is no buff combo yet...
        }
    }

    private bool UseLifeSurge()
    {
        if (_state.LifeSurgeLeft > _state.GCD)
            return false;

        if (_state.Unlocked(DRG.TraitID.EnhancedLifeSurge))
        {
            float chargeCapIn = _state.CD(DRG.AID.LifeSurge) - (_state.Unlocked(DRG.TraitID.EnhancedLifeSurge) ? 0 : 40);
            if (_state.RightEyeLeft > _state.GCD
                && _state.LanceChargeLeft > _state.GCD
                && ((_state.WheelInMotionLeft > _state.GCD) || (_state.FangAndClawBaredLeft > _state.GCD))
                && chargeCapIn < _state.GCD + 2.5
                && _state.CD(DRG.AID.BattleLitany) > 0
                && _state.CD(DRG.AID.Geirskogul) > 0)
                return true;

            if (_state.ComboLastMove == DRG.AID.VorpalThrust
                && chargeCapIn < _state.GCD + 2.5
                && _state.CD(DRG.AID.BattleLitany) < 100
                //&& _state.CD(DRG.AID.DragonSight) < 100
                && _state.CD(DRG.AID.LanceCharge) < 40
                && _state.CD(DRG.AID.Geirskogul) > 0)
                return true;
        }

        if (_state.UseAOERotation)
        {
            // for aoe rotation, just use LS on last unlocked combo action
            return _state.Unlocked(DRG.AID.CoerthanTorment) ? _state.ComboLastMove == DRG.AID.SonicThrust
                : !_state.Unlocked(DRG.AID.SonicThrust) || _state.ComboLastMove is DRG.AID.DoomSpike or DRG.AID.DraconianFury;
        }

        if (_state.Unlocked(DRG.AID.FullThrust) && _state.Unlocked(DRG.TraitID.EnhancedLifeSurge) && _state.LanceChargeLeft > _state.GCD)
        {
            // L26+: our most damaging action is FT, which is the third action in damaging combo
            return _state.ComboLastMove == DRG.AID.VorpalThrust;
        }

        // TODO: L64+
        if (_state.Unlocked(DRG.AID.FullThrust) && !_state.Unlocked(DRG.TraitID.EnhancedLifeSurge))
        {
            // L26+: our most damaging action is FT, which is the third action in damaging combo
            return _state.ComboLastMove == DRG.AID.VorpalThrust;
        }
        else
        {
            // L6+: our most damaging action is VT, which is the second action in damaging combo (which is the only combo we have before L18)
            return _state.ComboLastMove == DRG.AID.TrueThrust && !UseBuffingCombo(false);
        }
    }

    /*
    private bool UseSpineShatterDive(SpineshatterStrategy strategy)
    {
        switch (strategy)
        {
            case SpineshatterStrategy.Forbid:
                return false;
            case SpineshatterStrategy.Force:
                return true;
            case SpineshatterStrategy.ForceReserve:
                return _state.CD(DRG.AID.SpineshatterDive) <= 60 + _state.AnimationLock;
            case SpineshatterStrategy.UseOutsideMelee:
                return _state.RangeToTarget > 3;
            default:
                if (_state.CountdownRemaining > 0 || _state.PositionLockIn <= _state.AnimationLock)
                    return false; // Combat or Position restrictions

                if (_state.Unlocked(DRG.TraitID.EnhancedSpineshatterDive))
                {
                    // Enhanced Spineshatter Dive logic
                    if (_state.RightEyeLeft > _state.AnimationLock)
                        return true; // Use all charges under RightEyeLeft buff
                }
                else
                {
                    // Regular Spineshatter Dive logic
                    if (_state.LanceChargeLeft > _state.AnimationLock)
                        return true; // Use when there is Lance Charge left
                }

                return false; // Default: Don't use in other cases
        }
    }
    */

    private (Positional, bool) GetNextPositional()
    {
        if (_state.UseAOERotation)
            return default; // AOE rotation has no positionals

        if (_state.Unlocked(DRG.AID.FangAndClaw))
        {
            // we have flank positional (fang and claw, 4th in damaging combo) and rear positionals (chaos thrust & wheeling thrust, 3rd and 4th in buffing combo)
            // if our next action is not a positional, then just use next action in the current combo; for True Thrust, predict what branch we'll take using UseBuffingCombo
            if (_state.FangAndClawBaredLeft > _state.GCD)
                return (Positional.Flank, true);
            if (_state.WheelInMotionLeft > _state.GCD)
                return (Positional.Rear, true);
            var buffingCombo = _state.ComboLastMove switch
            {
                DRG.AID.TrueThrust or DRG.AID.RaidenThrust => UseBuffingCombo(false),
                DRG.AID.VorpalThrust => false,
                DRG.AID.Disembowel => true,
                _ => UseBuffingCombo(true)
            };
            return (buffingCombo ? Positional.Rear : Positional.Flank, _state.ComboLastMove == DRG.AID.Disembowel);
        }
        else if (_state.Unlocked(DRG.AID.ChaosThrust))
        {
            // the only positional we have at this point is chaos thrust (rear)
            return (Positional.Rear, _state.ComboLastMove == DRG.AID.Disembowel);
        }
        else
        {
            return default;
        }
    }

    private bool ShouldUseTrueNorth(TrueNorthStrategy strategy)
    {
        switch (strategy)
        {
            case TrueNorthStrategy.Delay:
                return false;
            case TrueNorthStrategy.Force:
                return true;

            default:
                if (!_state.TargetingEnemy)
                    return false;
                if (_state.TrueNorthLeft > _state.AnimationLock || _state.RightEyeLeft > _state.AnimationLock)
                    return false;
                if (GetNextPositional().Item2 && _state.NextPositionalCorrect)
                    return false;
                if (GetNextPositional().Item2 && !_state.NextPositionalCorrect)
                    return true;
                return false;
        }
    }

    private bool ShouldUseGeirskogul()
    {
        if (_state.EyeCount == 2 && _state.CD(DRG.AID.LanceCharge) < 40)
            return false;
        if (_state.EyeCount == 2 && _state.LanceChargeLeft > _state.AnimationLock)
            return true;
        if (_state.EyeCount == 1 && _state.LanceChargeLeft > _state.AnimationLock)
        {
            if (_state.DiveReadyLeft > _state.AnimationLock && _state.CD(DRG.AID.HighJump) > 10)
                return false;
        }
        if (_state.EyeCount != 2 && _state.CD(DRG.AID.LanceCharge) < 40)
        {
            return true;
        }
        if (_state.EyeCount == 0)
        {
            return true;
        }
        return true;
    }

    private bool ShouldUseWyrmWindThrust()
    {
        bool nextGCDisRaiden = _state.DraconianFireLeft > _state.AnimationLock && (_state.ComboLastMove == DRG.AID.WheelingThrust || _state.ComboLastMove == DRG.AID.FangAndClaw) && _state.WheelInMotionLeft < _state.AnimationLock && _state.FangAndClawBaredLeft < _state.AnimationLock;
        if (_state.FirstmindFocusCount >= 2 && _state.CD(DRG.AID.LanceCharge) > 10)
            return true;
        if (_state.FirstmindFocusCount >= 2 && _state.LanceChargeLeft > _state.AnimationLock)
            return true;
        if (_state.FirstmindFocusCount >= 2 && _state.CD(DRG.AID.LanceCharge) < 10 && !nextGCDisRaiden)
            return false;
        return false;
    }

    private DRG.AID GetNextBestGCD(StrategyValues strategy)
    {
        // prepull or no target
        if (!_state.TargetingEnemy || _state.CountdownRemaining > 0.7f)
            return DRG.AID.None;

        if (_state.UseAOERotation)
        {
            return _state.ComboLastMove switch
            {
                DRG.AID.DoomSpike or DRG.AID.DraconianFury => _state.Unlocked(DRG.AID.SonicThrust) ? DRG.AID.SonicThrust : DRG.AID.DoomSpike,
                DRG.AID.SonicThrust => _state.Unlocked(DRG.AID.CoerthanTorment) ? DRG.AID.CoerthanTorment : DRG.AID.DoomSpike,
                _ => _state.BestDoomSpike
            };
        }
        else
        {
            if (_state.FangAndClawBaredLeft > _state.GCD)
                return DRG.AID.FangAndClaw;
            if (_state.WheelInMotionLeft > _state.GCD)
                return DRG.AID.WheelingThrust;
            return _state.ComboLastMove switch
            {
                DRG.AID.TrueThrust or DRG.AID.RaidenThrust => UseBuffingCombo(false) ? DRG.AID.Disembowel : _state.Unlocked(DRG.AID.VorpalThrust) ? DRG.AID.VorpalThrust : DRG.AID.TrueThrust,
                DRG.AID.VorpalThrust => _state.Unlocked(DRG.AID.FullThrust) ? _state.BestHeavensThrust : DRG.AID.TrueThrust,
                DRG.AID.Disembowel => _state.Unlocked(DRG.AID.ChaosThrust) ? _state.BestChaoticSpring : DRG.AID.TrueThrust,
                _ => _state.BestTrueThrust
            };
        }
    }

    private ActionID GetNextBestOGCD(StrategyValues strategy, float deadline, DRG.AID nextBestGCD)
    {
        if (!_state.TargetingEnemy)
            return default;

        bool canJump = _state.PositionLockIn > _state.AnimationLock;
        // bool wantSpineShatter = _state.Unlocked(DRG.AID.SpineshatterDive) && _state.TargetingEnemy && UseSpineShatterDive(strategy.Option(Track.SpineshatterDive).As<SpineshatterStrategy>());

        if (_state.PowerSurgeLeft > _state.GCD)
        {
            if (_state.Unlocked(DRG.AID.LanceCharge) && _state.CanWeave(DRG.AID.LanceCharge, 0.6f, deadline - _state.OGCDSlotLength) /*&& ((_state.CD(DRG.AID.DragonSight) < _state.GCD) || (_state.CD(DRG.AID.DragonSight) < 65) && (_state.CD(DRG.AID.DragonSight) > 55))*/)
                return ActionID.MakeSpell(DRG.AID.LanceCharge);
            //if (_state.Unlocked(DRG.AID.DragonSight) && _state.CanWeave(DRG.AID.DragonSight, 0.6f, deadline) && _state.CD(DRG.AID.BattleLitany) < _state.GCD + 2.5)
            //    return ActionID.MakeSpell(DRG.AID.DragonSight);
            if (_state.Unlocked(DRG.AID.BattleLitany) && _state.CanWeave(DRG.AID.BattleLitany, 0.6f, deadline))
                return ActionID.MakeSpell(DRG.AID.BattleLitany);
            // life surge on most damaging gcd
            if (_state.Unlocked(DRG.AID.LifeSurge) && _state.CanWeave(_state.CD(DRG.AID.LifeSurge) - 45, 0.6f, deadline) && UseLifeSurge())
                return ActionID.MakeSpell(DRG.AID.LifeSurge);

            // TODO: better buff conditions, reconsider priorities
            if (_state.LifeOfTheDragonLeft > _state.AnimationLock && _state.CanWeave(DRG.AID.Nastrond, 0.6f, deadline))
                return ActionID.MakeSpell(DRG.AID.Nastrond);

            if (_state.CD(DRG.AID.LanceCharge) > 5 && /*_state.CD(DRG.AID.DragonSight) > 5 &&*/ _state.CD(DRG.AID.BattleLitany) > 5)
            {
                if (_state.CanWeave(DRG.AID.WyrmwindThrust, 0.6f, deadline) && ShouldUseWyrmWindThrust() && (nextBestGCD is DRG.AID.DraconianFury or DRG.AID.RaidenThrust))
                    return ActionID.MakeSpell(DRG.AID.WyrmwindThrust);
                if (_state.Unlocked(DRG.AID.Geirskogul) && _state.CanWeave(DRG.AID.Geirskogul, 0.6f, deadline) && ShouldUseGeirskogul())
                    return ActionID.MakeSpell(DRG.AID.Geirskogul);
                if (canJump && _state.Unlocked(DRG.AID.Jump) && _state.CanWeave(_state.Unlocked(DRG.AID.HighJump) ? DRG.AID.HighJump : DRG.AID.Jump, 0.8f, deadline))
                    return ActionID.MakeSpell(_state.BestJump);
                if (_state.DiveReadyLeft > _state.AnimationLock && _state.CanWeave(DRG.AID.MirageDive, 0.6f, deadline) && _state.EyeCount == 1 && _state.CD(DRG.AID.Geirskogul) < _state.AnimationLock && _state.LanceChargeLeft > _state.AnimationLock)
                    return ActionID.MakeSpell(DRG.AID.MirageDive);
                if (canJump && _state.Unlocked(DRG.AID.DragonfireDive) && _state.CanWeave(DRG.AID.DragonfireDive, 0.8f, deadline))
                    return ActionID.MakeSpell(DRG.AID.DragonfireDive);
                //if (wantSpineShatter && _state.CanWeave(_state.CD(DRG.AID.SpineshatterDive), 0.8f, deadline))
                //    return ActionID.MakeSpell(DRG.AID.SpineshatterDive);
                if (canJump && _state.Unlocked(DRG.AID.Stardiver) && _state.LifeOfTheDragonLeft > _state.AnimationLock && _state.CanWeave(DRG.AID.Stardiver, 1.5f, deadline))
                    return ActionID.MakeSpell(DRG.AID.Stardiver);
                if (_state.CanWeave(DRG.AID.WyrmwindThrust, 0.6f, deadline) && ShouldUseWyrmWindThrust())
                    return ActionID.MakeSpell(DRG.AID.WyrmwindThrust);
                //if (wantSpineShatter && _state.RangeToTarget > 3)
                //    return ActionID.MakeSpell(DRG.AID.SpineshatterDive);
                //if (wantSpineShatter && _state.LifeOfTheDragonLeft < _state.AnimationLock && _state.CanWeave(_state.CD(DRG.AID.SpineshatterDive) - 60, 0.8f, deadline))
                //    return ActionID.MakeSpell(DRG.AID.SpineshatterDive);
                //if (wantSpineShatter && _state.LifeOfTheDragonLeft > _state.AnimationLock && _state.CD(DRG.AID.Stardiver) > 0 && _state.CanWeave(_state.CD(DRG.AID.SpineshatterDive) - 60, 0.8f, deadline))
                //    return ActionID.MakeSpell(DRG.AID.SpineshatterDive);
                //if (wantSpineShatter && _state.CanWeave(_state.CD(DRG.AID.SpineshatterDive) - 60, 0.8f, deadline))
                //    return ActionID.MakeSpell(DRG.AID.SpineshatterDive);
                //if (_state.DiveReadyLeft > _state.AnimationLock && _state.CanWeave(DRG.AID.MirageDive, 0.6f, deadline) && _state.EyeCount != 2 && _state.LifeOfTheDragonLeft > _state.AnimationLock && _state.CD(DRG.AID.Stardiver) > 0)
                //    return ActionID.MakeSpell(DRG.AID.MirageDive);
                //if (_state.DiveReadyLeft > _state.AnimationLock && _state.CanWeave(DRG.AID.MirageDive, 0.6f, deadline) && _state.EyeCount != 2 && _state.LifeOfTheDragonLeft < _state.AnimationLock)
                //    return ActionID.MakeSpell(DRG.AID.MirageDive);
                if (_state.DiveReadyLeft > _state.AnimationLock && _state.CanWeave(DRG.AID.MirageDive, 0.6f, deadline) && (_state.EyeCount != 2 || _state.DiveReadyLeft < _state.GCD))
                    return ActionID.MakeSpell(DRG.AID.MirageDive);
            }
        }

        if (ShouldUseTrueNorth(strategy.Option(Track.TrueNorth).As<TrueNorthStrategy>()) && _state.CanWeave(DRG.AID.TrueNorth - 45, 0.6f, deadline) && !_state.UseAOERotation && _state.GCD < 0.8)
            return ActionID.MakeSpell(DRG.AID.TrueNorth);

        // no suitable oGCDs...
        return new();
    }
}
