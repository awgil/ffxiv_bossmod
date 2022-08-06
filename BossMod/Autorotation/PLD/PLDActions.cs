using System;
using System.Linq;

namespace BossMod.PLD
{
    class Actions : CommonActions
    {
        private PLDConfig _config;
        private bool _aoe;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<PLDConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            SupportedSpell(AID.Reprisal).Condition = _ => Autorot.PotentialTargetsInRangeFromPlayer(5).Any(); // TODO: consider checking only target?..
            SupportedSpell(AID.Interject).Condition = target => target?.CastInfo?.Interruptible ?? false;

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        protected override void UpdateInternalState(AutoAction strategy)
        {
            _aoe = (AutoStrategy & AutoAction.AOEDamage) != 0 && Autorot.PotentialTargetsInRangeFromPlayer(5).Count() >= 2;
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (Autorot.PrimaryTarget == null)
                return new();
            var aid = Rotation.GetNextBestGCD(_state, _strategy, _aoe);
            return MakeResult(ActionID.MakeSpell(aid), Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (Autorot.PrimaryTarget == null)
                return new();

            ActionID res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength, _aoe);
            if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline, _aoe);
            return res ? MakeResult(res, Autorot.PrimaryTarget) : new();
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

            //s.Gauge = Service.JobGauges.Get<PLDGauge>().OathGauge;

            _state.FightOrFlightLeft = 0;
            foreach (var status in Player.Statuses)
            {
                switch ((SID)status.ID)
                {
                    case SID.FightOrFlight:
                        _state.FightOrFlightLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // TODO: upgrades

            // self-targeted spells
            SupportedSpell(AID.TotalEclipse).TransformTarget = SupportedSpell(AID.Prominence).TransformTarget = SupportedSpell(AID.HolyCircle).TransformTarget
                = SupportedSpell(AID.CircleOfScorn).TransformTarget
                = SupportedSpell(AID.FightOrFlight).TransformTarget
                = SupportedSpell(AID.Rampart).TransformTarget = SupportedSpell(AID.Sheltron).TransformTarget = SupportedSpell(AID.Sentinel).TransformTarget
                = SupportedSpell(AID.HolySheltron).TransformTarget = SupportedSpell(AID.HallowedGround).TransformTarget = SupportedSpell(AID.Reprisal).TransformTarget
                = SupportedSpell(AID.PassageOfArms).TransformTarget = SupportedSpell(AID.DivineVeil).TransformTarget
                = SupportedSpell(AID.ArmsLength).TransformTarget = SupportedSpell(AID.IronWill).TransformTarget
                = _ => Player;

            // placeholders
            SupportedSpell(AID.FastBlade).PlaceholderForStrategy = _config.FullRotation ? AutoAction.GCDDamage | AutoAction.OGCDDamage : AutoAction.None;
            SupportedSpell(AID.TotalEclipse).PlaceholderForStrategy = _config.FullRotation ? AutoAction.GCDDamage | AutoAction.OGCDDamage | AutoAction.AOEDamage : AutoAction.None;

            // combo replacement
            SupportedSpell(AID.RiotBlade).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextRiotBladeComboAction(ComboLastMove)) : null;
            SupportedSpell(AID.RageOfHalone).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.RageOfHalone)) : null;
            SupportedSpell(AID.GoringBlade).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.GoringBlade)) : null;

            // smart targets
            SupportedSpell(AID.Shirk).TransformTarget = _config.SmartShirkTarget ? SmartTargetCoTank : null;
            SupportedSpell(AID.Provoke).TransformTarget = _config.ProvokeMouseover ? SmartTargetHostile : null; // TODO: also interject/low-blow
        }

        private AID ComboLastMove => (AID)ActionManagerEx.Instance!.ComboLastMove;
    }
}
