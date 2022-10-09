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

            // TODO: L56+ positionals
            var pos = Positional.Any;
            if (_state.TrueNorthLeft <= _state.GCD)
            {
                if (_state.ComboLastMove == AID.Disembowel && _state.Unlocked(AID.ChaosThrust))
                    pos = Positional.Rear;
            }
            return new(adjTarget, 3, pos);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            _strategy.NumAOEGCDTargets = Autorot.PrimaryTarget != null && autoAction != AutoActionST && _state.Unlocked(AID.DoomSpike) ? NumTargetsHitByAOEGCD(Autorot.PrimaryTarget) : 0;
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
            return MakeResult(res, Autorot.PrimaryTarget);
        }

        protected override void OnActionExecuted(ActionID action, Actor? target)
        {
            Log($"Executed {action} @ {target} [{_state}]");
        }

        protected override void OnActionSucceeded(ActorCastEvent ev)
        {
            Log($"Succeeded {ev.Action} @ {ev.MainTargetID:X} [{_state}]");
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            //s.Chakra = Service.JobGauges.Get<DRGGauge>().Chakra;

            _state.PowerSurgeLeft = StatusDetails(Player, SID.PowerSurge, Player.InstanceID).Left;
            _state.LanceChargeLeft = StatusDetails(Player, SID.LanceCharge, Player.InstanceID).Left;
            _state.TrueNorthLeft = StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;

            _state.TargetChaosThrustLeft = StatusDetails(Autorot.PrimaryTarget, SID.ChaosThrust, Player.InstanceID).Left;
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.TrueThrust).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.DoomSpike).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // combo replacement

            // smart targets
        }

        private bool WithoutDOT(Actor a) => Rotation.RefreshDOT(_state, StatusDetails(a, SID.ChaosThrust, Player.InstanceID).Left);
        private int NumTargetsHitByAOEGCD(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 10, 2);
    }
}
