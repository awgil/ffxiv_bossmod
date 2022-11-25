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
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<BLMConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();
            _prevMP = player.CurMP;

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
            // TODO: multidot?..
            var bestTarget = initial;
            if (_state.Unlocked(AID.Blizzard2))
            {
                var bestAOECount = NumTargetsHitByAOE(initial.Actor);
                foreach (var candidate in Autorot.Hints.PriorityTargets.Where(e => e != initial && e.Actor.Position.InCircle(Player.Position, 25)))
                {
                    var candidateAOECount = NumTargetsHitByAOE(candidate.Actor);
                    if (candidateAOECount > bestAOECount)
                    {
                        bestTarget = candidate;
                        bestAOECount = candidateAOECount;
                    }
                }
            }
            return new(bestTarget, bestTarget.StayAtLongRange ? 25 : 15);
        }

        protected override void OnTick()
        {
            // track mana ticks
            if (_prevMP < Player.CurMP)
            {
                var gauge = Service.JobGauges.Get<BLMGauge>();
                if (!gauge.InAstralFire)
                {
                    var expectedTick = Rotation.MPTick(-gauge.UmbralIceStacks);
                    if (Player.CurMP - _prevMP == expectedTick)
                    {
                        _lastManaTick = Autorot.WorldState.CurrentTime;
                    }
                }
            }
            _prevMP = Player.CurMP;
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);
            if (autoAction == AutoActionAIFight)
            {
                _strategy.NumAOETargets = Autorot.PrimaryTarget != null ? NumTargetsHitByAOE(Autorot.PrimaryTarget) : 0;
            }
            else
            {
                _strategy.NumAOETargets = autoAction == AutoActionAOE ? 100 : 0; // TODO: consider making AI-like check
            }
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(AID.Transpose))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Transpose), Player, !Player.InCombat && _state.ElementalLevel > 0 && _state.CurMP < 10000);
            if (_state.Unlocked(AID.Manaward))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Manaward), Player, Player.HP.Cur < Player.HP.Max * 0.8f);
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
            _state.TimeToManaTick = 3 - (_lastManaTick != new DateTime() ? (float)(Autorot.WorldState.CurrentTime - _lastManaTick).TotalSeconds % 3 : 0);

            var gauge = Service.JobGauges.Get<BLMGauge>();
            _state.ElementalLevel = gauge.InAstralFire ? gauge.AstralFireStacks : -gauge.UmbralIceStacks;
            _state.ElementalLeft = gauge.ElementTimeRemaining * 0.001f;

            _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
            _state.ThundercloudLeft = StatusDetails(Player, SID.Thundercloud, Player.InstanceID).Left;
            _state.FirestarterLeft = StatusDetails(Player, SID.Firestarter, Player.InstanceID).Left;

            _state.TargetThunderLeft = Math.Max(StatusDetails(Autorot.PrimaryTarget, _state.ExpectedThunder3, Player.InstanceID).Left, StatusDetails(Autorot.PrimaryTarget, SID.Thunder2, Player.InstanceID).Left);
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.Blizzard1).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.Blizzard2).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // smart targets
            SupportedSpell(AID.AetherialManipulation).TransformTarget = _config.MouseoverFriendly ? SmartTargetFriendly : null;
        }

        private int NumTargetsHitByAOE(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOECircle(primary.Position, 5);
    }
}
