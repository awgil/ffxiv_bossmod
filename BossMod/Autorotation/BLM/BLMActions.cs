using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.BLM
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private BLMConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        // TODO: this should be done by framework (speculative effect application for actions that we didn't get EffectResult for yet)
        private DateTime _lastThunderSpeculation;
        private List<ulong> _lastThunderTargets = new();

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<BLMConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override Targeting SelectBetterTarget(Actor initial)
        {
            // TODO: select best target for AOE
            return new(initial, 25);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);
            if (autoAction is AutoActionAIFight or AutoActionAIFightMove)
            {
                _strategy.AOE = Autorot.PrimaryTarget != null && Autorot.PotentialTargetsInRange(Autorot.PrimaryTarget.Position, 5).Count() >= 3;
                _strategy.Moving = AutoAction == AutoActionAIFightMove;
            }
            else
            {
                _strategy.AOE = autoAction == AutoActionAOE; // TODO: consider making AI-like check
                _strategy.Moving = false;
            }
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(MinLevel.Manaward))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Manaward), Player, Player.HP.Cur < Player.HP.Max * 0.8f);
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionFirstFight)
                return new();
            var aid = Rotation.GetNextBestGCD(_state, _strategy);
            return MakeResult(aid, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionFirstFight)
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
            // hack
            if (ev.Action.Type == ActionType.Spell && (AID)ev.Action.ID is AID.Thunder1 or AID.Thunder2)
            {
                _lastThunderSpeculation = Autorot.WorldState.CurrentTime.AddMilliseconds(1000);
                _lastThunderTargets.Clear();
                _lastThunderTargets.AddRange(ev.Targets.Select(t => t.ID));
            }
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            var gauge = Service.JobGauges.Get<BLMGauge>();
            _state.ElementalLevel = gauge.InAstralFire ? gauge.AstralFireStacks : -gauge.UmbralIceStacks;
            _state.ElementalLeft = gauge.ElementTimeRemaining * 0.001f;

            _state.TargetThunderLeft = 0;
            if (Autorot.PrimaryTarget != null)
            {
                foreach (var status in Autorot.PrimaryTarget.Statuses)
                {
                    switch ((SID)status.ID)
                    {
                        case SID.Thunder1:
                        case SID.Thunder2:
                            if (status.SourceID == Player.InstanceID)
                                _state.TargetThunderLeft = StatusDuration(status.ExpireAt);
                            break;
                    }
                }
                if (_state.TargetThunderLeft == 0 && _lastThunderSpeculation > Autorot.WorldState.CurrentTime && _lastThunderTargets.Contains(Autorot.PrimaryTarget.InstanceID))
                {
                    _state.TargetThunderLeft = 21;
                }
            }
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.Blizzard1).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.Blizzard2).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // smart targets
            SupportedSpell(AID.AetherialManipulation).TransformTarget = _config.MouseoverFriendly ? SmartTargetFriendly : null;
        }
    }
}
