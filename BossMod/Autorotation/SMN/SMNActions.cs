using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Linq;

namespace BossMod.SMN
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private SMNConfig _config;
        private bool _aoe;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<SMNConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

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
            // TODO: AOE & multidotting
            return new(initial, 25);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            _aoe = autoAction switch
            {
                AutoActionST => false,
                AutoActionAOE => true, // TODO: consider making AI-like check
                AutoActionAIFight => Autorot.PrimaryTarget != null && Autorot.Hints.NumPriorityTargetsInAOECircle(Autorot.PrimaryTarget.Position, 5) >= 3,
                _ => false, // irrelevant...
            };
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);
        }

        protected override void QueueAIActions()
        {
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (_strategy.Prepull && _state.Unlocked(AID.SummonCarbuncle) && !_state.PetSummoned)
                return MakeResult(AID.SummonCarbuncle, Player);
            //if ((AutoStrategy & AutoAction.GCDHeal) != 0)
            //    return MakeResult(AID.Physick, Autorot.SecondaryTarget); // TODO: automatic target selection

            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
                return new();
            var aid = Rotation.GetNextBestGCD(_state, _strategy, _aoe, _strategy.ForceMovementIn < 5);
            return MakeResult(aid, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
                return new();

            ActionID res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength, _aoe);
            if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline, _aoe);
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

            _state.PetSummoned = Autorot.WorldState.Actors.Any(a => a.Type == ActorType.Pet && a.OwnerID == Player.InstanceID);

            var gauge = Service.JobGauges.Get<SMNGauge>();
            _state.IfritReady = gauge.IsIfritReady;
            _state.TitanReady = gauge.IsTitanReady;
            _state.GarudaReady = gauge.IsGarudaReady;
            _state.Attunement = (Rotation.Attunement)(((int)gauge.AetherFlags >> 2) & 3);
            _state.AttunementStacks = gauge.Attunement;
            _state.AttunementLeft = gauge.AttunmentTimerRemaining * 0.001f;
            _state.SummonLockLeft = gauge.SummonTimerRemaining * 0.001f;
            _state.AetherflowStacks = gauge.AetherflowStacks;

            _state.SwiftcastLeft = 0;
            foreach (var status in Player.Statuses)
            {
                switch ((SID)status.ID)
                {
                    case SID.Swiftcast:
                        _state.SwiftcastLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.Ruin1).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.Outburst).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // smart targets
            SupportedSpell(AID.Physick).TransformTarget = _config.MouseoverFriendly ? SmartTargetFriendly : null;
        }

        private AID SmartResurrectAction()
        {
            // 1. swiftcast, if ready and not up yet
            if (_state.Unlocked(AID.Swiftcast) && _state.SwiftcastLeft <= 0 && _state.CD(CDGroup.Swiftcast) <= 0)
                return AID.Swiftcast;

            return AID.Resurrection;
        }
    }
}
