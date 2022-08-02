using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Linq;

namespace BossMod.WAR
{
    class Actions : CommonActions
    {
        private WARConfig _config;
        private bool _aoe;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<WARConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new()
            {
                FirstChargeIn = 0.01f, // by default, always preserve 1 onslaught charge
                SecondChargeIn = 10000, // ... but don't preserve second
            };

            SupportedSpell(AID.Equilibrium).Condition = _ => Player.HP.Cur < Player.HP.Max;
            SupportedSpell(AID.Reprisal).Condition = _ => Autorot.PotentialTargetsInRangeFromPlayer(5).Any(); // TODO: consider checking only target?..
            SupportedSpell(AID.Interject).Condition = target => target?.CastInfo?.Interruptible ?? false;
            // TODO: SIO - check that raid is in range?..
            // TODO: Provoke - check that not already MT?
            // TODO: Shirk - check that hate is close to MT?..

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        protected override void UpdateInternalState(AutoAction strategy)
        {
            _aoe = (AutoStrategy & AutoAction.AOEDamage) != 0; // TODO: take potential targets in account instead...
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

            // TODO: refactor... at very least replace window-end with deadline (difference is how lock-delay is accounted)
            ActionID res = new();
            if (_state.CanDoubleWeave) // first ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, _state.DoubleWeaveWindowEnd, _aoe);
            if (!res && _state.CanSingleWeave) // second/only ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, _state.GCD, _aoe);
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

            _state.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;

            _state.SurgingTempestLeft = _state.NascentChaosLeft = _state.PrimalRendLeft = _state.InnerReleaseLeft = 0;
            _state.InnerReleaseStacks = 0;
            foreach (var status in Player.Statuses)
            {
                switch ((SID)status.ID)
                {
                    case SID.SurgingTempest:
                        _state.SurgingTempestLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.NascentChaos:
                        _state.NascentChaosLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.Berserk:
                    case SID.InnerRelease:
                        _state.InnerReleaseLeft = StatusDuration(status.ExpireAt);
                        _state.InnerReleaseStacks = status.Extra & 0xFF;
                        break;
                    case SID.PrimalRend:
                        _state.PrimalRendLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // IB/FC/IC is a single button, as is SteelCyclone/Decimate/ChaoticCyclone, as is Berserk/IR, as is RawIntuition/Bloodwhetting
            SupportedSpell(AID.InnerBeast).TransformAction = SupportedSpell(AID.FellCleave).TransformAction = SupportedSpell(AID.InnerChaos).TransformAction = () => ActionID.MakeSpell(Rotation.GetFCAction(_state));
            SupportedSpell(AID.SteelCyclone).TransformAction = SupportedSpell(AID.Decimate).TransformAction = SupportedSpell(AID.ChaoticCyclone).TransformAction = () => ActionID.MakeSpell(Rotation.GetDecimateAction(_state));
            SupportedSpell(AID.Berserk).TransformAction = SupportedSpell(AID.Berserk).TransformAction = () => ActionID.MakeSpell(_state.Unlocked(MinLevel.InnerRelease) ? AID.InnerRelease : AID.Berserk);
            SupportedSpell(AID.RawIntuition).TransformAction = SupportedSpell(AID.Bloodwhetting).TransformAction = () => ActionID.MakeSpell(_state.Unlocked(MinLevel.Bloodwhetting) ? AID.Bloodwhetting : AID.RawIntuition);

            // self-targeted spells
            SupportedSpell(AID.Overpower).TransformTarget = SupportedSpell(AID.MythrilTempest).TransformTarget
                = SupportedSpell(AID.SteelCyclone).TransformTarget = SupportedSpell(AID.Decimate).TransformTarget = SupportedSpell(AID.ChaoticCyclone).TransformTarget
                = SupportedSpell(AID.Infuriate).TransformTarget = SupportedSpell(AID.Orogeny).TransformTarget
                = SupportedSpell(AID.Berserk).TransformTarget = SupportedSpell(AID.InnerRelease).TransformTarget
                = SupportedSpell(AID.Rampart).TransformTarget = SupportedSpell(AID.Vengeance).TransformTarget = SupportedSpell(AID.ThrillOfBattle).TransformTarget
                = SupportedSpell(AID.Equilibrium).TransformTarget = SupportedSpell(AID.Reprisal).TransformTarget = SupportedSpell(AID.ShakeItOff).TransformTarget
                = SupportedSpell(AID.RawIntuition).TransformTarget = SupportedSpell(AID.Bloodwhetting).TransformTarget
                = SupportedSpell(AID.ArmsLength).TransformTarget = SupportedSpell(AID.Defiance).TransformTarget
                = _ => Player;

            // placeholders
            SupportedSpell(AID.HeavySwing).PlaceholderForStrategy = _config.FullRotation ? AutoAction.GCDDamage | AutoAction.OGCDDamage : AutoAction.None;
            SupportedSpell(AID.Overpower).PlaceholderForStrategy = _config.FullRotation ? AutoAction.GCDDamage | AutoAction.OGCDDamage | AutoAction.AOEDamage : AutoAction.None;

            // combo replacement
            SupportedSpell(AID.Maim).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextMaimComboAction(ComboLastMove)) : null;
            SupportedSpell(AID.StormEye).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.StormEye)) : null;
            SupportedSpell(AID.StormPath).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.StormPath)) : null;
            SupportedSpell(AID.MythrilTempest).TransformAction = _config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextAOEComboAction(ComboLastMove)) : null;

            // smart targets
            SupportedSpell(AID.NascentFlash).TransformTarget = _config.SmartNascentFlashShirkTarget ? SmartTargetCoTank : null;
            SupportedSpell(AID.Shirk).TransformTarget = _config.SmartNascentFlashShirkTarget ? SmartTargetCoTank : null;
            SupportedSpell(AID.Provoke).TransformTarget = _config.ProvokeMouseover ? SmartTargetHostile : null; // TODO: also interject/low-blow
            SupportedSpell(AID.Holmgang).TransformTarget = _config.HolmgangSelf ? _ => Player : null; // TODO: otherwise smarttarget hostile or self...
        }

        private AID ComboLastMove => (AID)ActionManagerEx.Instance!.ComboLastMove;
    }
}
