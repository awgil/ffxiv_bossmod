using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Linq;

namespace BossMod.MNK
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private MNKConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<MNKConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            // upgrades
            SupportedSpell(AID.SteelPeak).TransformAction = SupportedSpell(AID.ForbiddenChakra).TransformAction = () => ActionID.MakeSpell(_state.BestForbiddenChakra);
            SupportedSpell(AID.HowlingFist).TransformAction = SupportedSpell(AID.Enlightenment).TransformAction = () => ActionID.MakeSpell(_state.BestEnlightenment);
            SupportedSpell(AID.ArmOfTheDestroyer).TransformAction = SupportedSpell(AID.ShadowOfTheDestroyer).TransformAction = () => ActionID.MakeSpell(_state.BestShadowOfTheDestroyer);
            SupportedSpell(AID.FlintStrike).TransformAction = SupportedSpell(AID.RisingPhoenix).TransformAction = () => ActionID.MakeSpell(_state.BestRisingPhoenix);
            SupportedSpell(AID.TornadoKick).TransformAction = SupportedSpell(AID.PhantomRush).TransformAction = () => ActionID.MakeSpell(_state.BestPhantomRush);

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override Targeting SelectBetterTarget(Actor initial)
        {
            // TODO: multidotting support...
            var pos = (_state.Form == Rotation.Form.Coeurl ? Rotation.GetCoeurlFormAction(_state, _strategy.NumAOETargets) : AID.None) switch
            {
                AID.SnapPunch => Positional.Flank,
                AID.Demolish => Positional.Rear,
                _ => Positional.Any
            };
            return new(initial, 3, pos);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            _strategy.NumAOETargets = autoAction == AutoActionST ? 0 : Autorot.PotentialTargetsInRangeFromPlayer(5).Count();
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(MinLevel.SteelPeak))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Meditation), Player, _strategy.Prepull && _state.Chakra < 5);
            if (_state.Unlocked(MinLevel.SecondWind))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.HP.Cur < Player.HP.Max * 0.5f);
            if (_state.Unlocked(MinLevel.Bloodbath))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Bloodbath), Player, Player.HP.Cur < Player.HP.Max * 0.8f);
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

            _state.Chakra = Service.JobGauges.Get<MNKGauge>().Chakra;

            _state.Form = Rotation.Form.None;
            _state.FormLeft = _state.DisciplinedFistLeft = 0;
            foreach (var status in Player.Statuses)
            {
                switch ((SID)status.ID)
                {
                    case SID.OpoOpoForm:
                        _state.Form = Rotation.Form.OpoOpo;
                        _state.FormLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.RaptorForm:
                        _state.Form = Rotation.Form.Raptor;
                        _state.FormLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.CoeurlForm:
                        _state.Form = Rotation.Form.Coeurl;
                        _state.FormLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.DisciplinedFist:
                        _state.DisciplinedFistLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }

            var demolish = Autorot.PrimaryTarget?.FindStatus(SID.Demolish, Player.InstanceID);
            _state.TargetDemolishLeft = demolish != null ? StatusDuration(demolish.Value.ExpireAt) : 0;
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.Bootshine).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.ArmOfTheDestroyer).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // combo replacement
            SupportedSpell(AID.FourPointFury).TransformAction = _config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextComboAction(_state, 100)) : null;

            // smart targets
        }
    }
}
