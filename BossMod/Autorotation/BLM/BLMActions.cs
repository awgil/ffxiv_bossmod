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
        private DateTime _lastManaTick;
        private uint _prevMP;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<BLMConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();
            _prevMP = Service.ClientState.LocalPlayer?.CurrentMp ?? 0;

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
            return new(initial, 15);
        }

        protected override void OnTick()
        {
            // track mana ticks
            var currMP = Service.ClientState.LocalPlayer?.CurrentMp ?? 0;
            if (_prevMP < currMP)
            {
                var gauge = Service.JobGauges.Get<BLMGauge>();
                if (!gauge.InAstralFire)
                {
                    var expectedTick = Rotation.MPTick(-gauge.UmbralIceStacks);
                    if (currMP - _prevMP == expectedTick)
                    {
                        _lastManaTick = Autorot.WorldState.CurrentTime;
                    }
                }
            }
            _prevMP = currMP;
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
            if (_state.Unlocked(MinLevel.Transpose))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Transpose), Player, _strategy.Prepull && _state.ElementalLevel > 0 && _state.CurMP < 10000);
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
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);
            _state.TimeToManaTick = 3 - (_lastManaTick != new DateTime() ? (float)(Autorot.WorldState.CurrentTime - _lastManaTick).TotalSeconds % 3 : 0);

            var gauge = Service.JobGauges.Get<BLMGauge>();
            _state.ElementalLevel = gauge.InAstralFire ? gauge.AstralFireStacks : -gauge.UmbralIceStacks;
            _state.ElementalLeft = gauge.ElementTimeRemaining * 0.001f;

            _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
            _state.ThundercloudLeft = StatusDetails(Player, SID.Thundercloud, Player.InstanceID).Left;
            _state.FirestarterLeft = StatusDetails(Player, SID.Firestarter, Player.InstanceID).Left;

            _state.TargetThunderLeft = Math.Max(StatusDetails(Autorot.PrimaryTarget, SID.Thunder1, Player.InstanceID).Left, StatusDetails(Autorot.PrimaryTarget, SID.Thunder2, Player.InstanceID).Left);
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
