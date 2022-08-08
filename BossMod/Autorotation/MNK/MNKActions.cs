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

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            _strategy.AOE = autoAction switch
            {
                AutoActionST => false,
                AutoActionAOE => true, // TODO: consider making AI-like check
                AutoActionAIFight or AutoActionAIFightMove => Autorot.PotentialTargetsInRangeFromPlayer(5).Count() >= 3,
                _ => false, // irrelevant...
            };

            PreferredPosition = _state.Form != Rotation.Form.Coeurl || _strategy.AOE ? Positional.Any : Positional.Flank; // TODO: demolish support...
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(MinLevel.SteelPeak))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Meditation), Player, _strategy.Prepull && _state.Chakra < 5);
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
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // upgrades
            SupportedSpell(AID.SteelPeak).TransformAction = SupportedSpell(AID.ForbiddenChakra).TransformAction = () => ActionID.MakeSpell(_state.Unlocked(MinLevel.ForbiddenChakra) ? AID.ForbiddenChakra : AID.SteelPeak);
            SupportedSpell(AID.HowlingFist).TransformAction = SupportedSpell(AID.Enlightenment).TransformAction = () => ActionID.MakeSpell(_state.Unlocked(MinLevel.Enlightenment) ? AID.Enlightenment : AID.HowlingFist);
            SupportedSpell(AID.ArmOfTheDestroyer).TransformAction = SupportedSpell(AID.ShadowOfTheDestroyer).TransformAction = () => ActionID.MakeSpell(_state.Unlocked(MinLevel.ShadowOfTheDestroyer) ? AID.ShadowOfTheDestroyer : AID.ArmOfTheDestroyer);
            SupportedSpell(AID.FlintStrike).TransformAction = SupportedSpell(AID.RisingPhoenix).TransformAction = () => ActionID.MakeSpell(_state.Unlocked(MinLevel.RisingPhoenix) ? AID.RisingPhoenix : AID.FlintStrike);
            SupportedSpell(AID.TornadoKick).TransformAction = SupportedSpell(AID.PhantomRush).TransformAction = () => ActionID.MakeSpell(_state.Unlocked(MinLevel.PhantomRush) ? AID.PhantomRush : AID.TornadoKick);

            // self-targeted spells
            SupportedSpell(AID.ArmOfTheDestroyer).TransformTarget = SupportedSpell(AID.ShadowOfTheDestroyer).TransformTarget = SupportedSpell(AID.FourPointFury).TransformTarget = SupportedSpell(AID.Rockbreaker).TransformTarget
                = SupportedSpell(AID.MasterfulBlitz).TransformTarget = SupportedSpell(AID.ElixirField).TransformTarget = SupportedSpell(AID.FlintStrike).TransformTarget = SupportedSpell(AID.RisingPhoenix).TransformTarget
                = SupportedSpell(AID.PerfectBalance).TransformTarget = SupportedSpell(AID.RiddleOfFire).TransformTarget = SupportedSpell(AID.Brotherhood).TransformTarget = SupportedSpell(AID.RiddleOfWind).TransformTarget
                = SupportedSpell(AID.SecondWind).TransformTarget = SupportedSpell(AID.Mantra).TransformTarget = SupportedSpell(AID.RiddleOfEarth).TransformTarget
                = SupportedSpell(AID.Bloodbath).TransformTarget = SupportedSpell(AID.ArmsLength).TransformTarget
                = SupportedSpell(AID.Meditation).TransformTarget = SupportedSpell(AID.TrueNorth).TransformTarget
                = SupportedSpell(AID.FormShift).TransformTarget = SupportedSpell(AID.Anatman).TransformTarget
                = _ => Player;

            // placeholders
            SupportedSpell(AID.Bootshine).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.ArmOfTheDestroyer).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // combo replacement
            SupportedSpell(AID.FourPointFury).TransformAction = _config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextAOEComboAction(_state)) : null;

            // smart targets
        }
    }
}
