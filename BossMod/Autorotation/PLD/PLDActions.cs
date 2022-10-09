using System;
using System.Linq;

namespace BossMod.PLD
{
    class Actions : TankActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private PLDConfig _config;
        private bool _aoe;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<PLDConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            SupportedSpell(AID.Reprisal).Condition = _ => Autorot.Hints.PotentialTargets.Any(e => e.Actor.Position.InCircle(Player.Position, 5 + e.Actor.HitboxRadius)); // TODO: consider checking only target?..
            SupportedSpell(AID.Interject).Condition = target => target?.CastInfo?.Interruptible ?? false;

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override CommonRotation.PlayerState GetState() => _state;
        public override CommonRotation.Strategy GetStrategy() => _strategy;

        protected override void UpdateInternalState(int autoAction)
        {
            base.UpdateInternalState(autoAction);
            _aoe = autoAction switch
            {
                AutoActionST => false,
                AutoActionAOE => true, // TODO: consider making AI-like check
                AutoActionAIFight => NumTargetsHitByAOE() >= 3,
                _ => false, // irrelevant...
            };
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(AID.Interject))
            {
                var interruptibleEnemy = Autorot.Hints.PotentialTargets.FirstOrDefault(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 3 + e.Actor.HitboxRadius + Player.HitboxRadius));
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Interject), interruptibleEnemy?.Actor, interruptibleEnemy != null);
            }
            if (_state.Unlocked(AID.IronWill))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.IronWill), Player, ShouldSwapStance());
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
                return new();
            if (AutoAction == AutoActionAIFight && !Autorot.PrimaryTarget.Position.InCircle(Player.Position, 3 + Autorot.PrimaryTarget.HitboxRadius + Player.HitboxRadius) && _state.Unlocked(AID.ShieldLob))
                return MakeResult(AID.ShieldLob, Autorot.PrimaryTarget); // TODO: reconsider...
            var aid = Rotation.GetNextBestGCD(_state, _strategy, _aoe);
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
            base.OnActionExecuted(action, target);
            Log($"Executed {action} @ {target} [{_state}]");
        }

        protected override void OnActionSucceeded(ActorCastEvent ev)
        {
            Log($"Succeeded {ev.Action} @ {ev.MainTargetID:X} [{_state}]");
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            //s.Gauge = Service.JobGauges.Get<PLDGauge>().OathGauge;

            _state.FightOrFlightLeft = StatusDetails(Player, SID.FightOrFlight, Player.InstanceID).Left;
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.FastBlade).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.TotalEclipse).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // combo replacement
            SupportedSpell(AID.RiotBlade).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextRiotBladeComboAction(ComboLastMove)) : null;
            SupportedSpell(AID.RageOfHalone).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.RageOfHalone)) : null;
            SupportedSpell(AID.GoringBlade).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.GoringBlade)) : null;

            // smart targets
            SupportedSpell(AID.Shirk).TransformTarget = _config.SmartShirkTarget ? SmartTargetCoTank : null;
            SupportedSpell(AID.Provoke).TransformTarget = _config.ProvokeMouseover ? SmartTargetHostile : null; // TODO: also interject/low-blow
        }

        private AID ComboLastMove => (AID)ActionManagerEx.Instance!.ComboLastMove;

        private int NumTargetsHitByAOE() => Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    }
}
