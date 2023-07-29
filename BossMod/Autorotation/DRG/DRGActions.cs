using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Linq;

namespace BossMod.DRG
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private DRGConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<DRGConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            // upgrades
            SupportedSpell(AID.FullThrust).TransformAction = SupportedSpell(AID.HeavensThrust).TransformAction = () => ActionID.MakeSpell(_state.BestHeavensThrust);
            SupportedSpell(AID.ChaosThrust).TransformAction = SupportedSpell(AID.ChaoticSpring).TransformAction = () => ActionID.MakeSpell(_state.BestChaoticSpring);
            SupportedSpell(AID.Jump).TransformAction = SupportedSpell(AID.HighJump).TransformAction = () => ActionID.MakeSpell(_state.BestJump);
            SupportedSpell(AID.TrueThrust).TransformAction = SupportedSpell(AID.RaidenThrust).TransformAction = () => ActionID.MakeSpell(_state.BestTrueThrust);
            SupportedSpell(AID.DoomSpike).TransformAction = SupportedSpell(AID.DraconianFury).TransformAction = () => ActionID.MakeSpell(_state.BestDoomSpike);
            SupportedSpell(AID.Geirskogul).TransformAction = SupportedSpell(AID.Nastrond).TransformAction = () => ActionID.MakeSpell(_state.BestGeirskogul);

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override CommonRotation.PlayerState GetState() => _state;
        public override CommonRotation.Strategy GetStrategy() => _strategy;

        public override Targeting SelectBetterTarget(AIHints.Enemy initial)
        {
            // targeting for aoe
            if (_state.Unlocked(AID.DoomSpike))
            {
                var bestAOETarget = initial;
                var bestAOECount = NumTargetsHitByAOEGCD(initial.Actor);
                foreach (var candidate in Autorot.Hints.PriorityTargets.Where(e => e != initial && e.Actor.Position.InCircle(Player.Position, 10)))
                {
                    var candidateAOECount = NumTargetsHitByAOEGCD(candidate.Actor);
                    if (candidateAOECount > bestAOECount)
                    {
                        bestAOETarget = candidate;
                        bestAOECount = candidateAOECount;
                    }
                }

                if (bestAOECount >= 3)
                    return new(bestAOETarget, 3);
            }

            // targeting for multidot
            var adjTarget = initial;
            if (_state.Unlocked(AID.ChaosThrust) && !WithoutDOT(initial.Actor))
            {
                var multidotTarget = Autorot.Hints.PriorityTargets.FirstOrDefault(e => e != initial && !e.ForbidDOTs && e.Actor.Position.InCircle(Player.Position, 5) && WithoutDOT(e.Actor));
                if (multidotTarget != null)
                    adjTarget = multidotTarget;
            }

            var pos = _strategy.NextPositionalImminent ? _strategy.NextPositional : Positional.Any; // TODO: move to common code
            return new(adjTarget, 3, pos);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            _strategy.NumAOEGCDTargets = Autorot.PrimaryTarget != null && autoAction != AutoActionST && _state.Unlocked(AID.DoomSpike) ? NumTargetsHitByAOEGCD(Autorot.PrimaryTarget) : 0;
            _strategy.UseAOERotation = autoAction switch
            {
                AutoActionST => false,
                AutoActionAOE => true, // TODO: consider making AI-like check
                AutoActionAIFight => _strategy.NumAOEGCDTargets >= 3 && (_state.Unlocked(AID.SonicThrust) || _state.PowerSurgeLeft > _state.GCD), // TODO: better AOE condition
                _ => false, // irrelevant...
            };
            FillStrategyPositionals(_strategy, Rotation.GetNextPositional(_state, _strategy), _state.TrueNorthLeft > _state.GCD);
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(AID.SecondWind))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.5f);
            if (_state.Unlocked(AID.Bloodbath))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Bloodbath), Player, Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.8f);
            // TODO: true north...
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
                return new();
            var aid = Rotation.GetNextBestGCD(_state, _strategy);
            return MakeResult(aid, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
                return new();

            ActionID res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
            if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

            var target = res == ActionID.MakeSpell(AID.DragonSight) ? FindBestDragonSightTarget() : Autorot.PrimaryTarget;
            return MakeResult(res, target);
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            var gauge = Service.JobGauges.Get<DRGGauge>();
            _state.EyeCount = gauge.EyeCount;
            _state.LifeOfTheDragonLeft = gauge.IsLOTDActive ? gauge.LOTDTimer * 0.001f : 0;

            _state.FangAndClawBaredLeft = StatusDetails(Player, SID.FangAndClawBared, Player.InstanceID).Left;
            _state.WheelInMotionLeft = StatusDetails(Player, SID.WheelInMotion, Player.InstanceID).Left;
            _state.DraconianFireLeft = StatusDetails(Player, SID.DraconianFire, Player.InstanceID).Left;
            _state.DiveReadyLeft = StatusDetails(Player, SID.DiveReady, Player.InstanceID).Left;
            _state.PowerSurgeLeft = StatusDetails(Player, SID.PowerSurge, Player.InstanceID).Left;
            _state.LanceChargeLeft = StatusDetails(Player, SID.LanceCharge, Player.InstanceID).Left;
            _state.RightEyeLeft = StatusDetails(Player, SID.RightEye, Player.InstanceID).Left;
            _state.TrueNorthLeft = StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;

            _state.TargetChaosThrustLeft = StatusDetails(Autorot.PrimaryTarget, _state.ExpectedChaoticSpring, Player.InstanceID).Left;
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.TrueThrust).PlaceholderForAuto = SupportedSpell(AID.RaidenThrust).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.DoomSpike).PlaceholderForAuto = SupportedSpell(AID.DraconianFury).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // combo replacement

            // smart targets
            SupportedSpell(AID.DragonSight).TransformTarget = _config.SmartDragonSightTarget ? SmartTargetDragonSight : null;
        }

        private bool WithoutDOT(Actor a) => Rotation.RefreshDOT(_state, StatusDetails(a, SID.ChaosThrust, Player.InstanceID).Left);
        private int NumTargetsHitByAOEGCD(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 10, 2);

        // smart targeting utility: return target (if friendly) or mouseover (if friendly) or other tank (if available) or null (otherwise)
        private Actor? SmartTargetDragonSight(Actor? primaryTarget) => SmartTargetFriendly(primaryTarget) ?? FindBestDragonSightTarget();
        private Actor FindBestDragonSightTarget()
        {
            // TODO: allow designating specific player as target in config
            var bestPartyMember = Autorot.WorldState.Party.WithoutSlot().Exclude(Player).MaxBy(p => p.Class switch
            {
                Class.SAM => 1.00f,
                Class.NIN => 0.99f,
                Class.RPR => 0.89f,
                Class.MNK => 0.88f,
                Class.DRG => 0.88f,
                Class.DNC => 0.86f,
                Class.BRD => 0.86f,
                Class.BLM => 0.82f,
                Class.RDM => 0.77f,
                Class.GNB => 0.68f,
                Class.DRK => 0.67f,
                Class.MCH => 0.67f,
                Class.SMN => 0.66f,
                _ => 0.01f
            });
            return bestPartyMember ?? Player;
        }
    }
}
