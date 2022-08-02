using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Linq;

namespace BossMod.WAR
{
    class Actions : CommonActions
    {
        private WARConfig _config;
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
            SupportedSpell(AID.Reprisal).Condition = _ => AllowReprisal();
            SupportedSpell(AID.Interject).Condition = AllowInterject;
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

        public override NextAction CalculateNextAutomaticAction(AutoAction strategy, ulong primaryTargetID, float effAnimLock, bool moving, float animLockDelay)
        {
            UpdatePlayerState(effAnimLock, animLockDelay);
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            return new() { Action = Rotation.GetNextBestAction(_state, _strategy, (strategy & AutoAction.AOEDamage) != 0), TargetID = primaryTargetID };
        }

        private void UpdatePlayerState(float effAnimLock, float animLockDelay)
        {
            FillCommonPlayerState(_state, effAnimLock, animLockDelay);

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
            SupportedSpell(AID.HeavySwing).PlaceholderForStrategy = _config.FullRotation ? AutoAction.GCDDamage | AutoAction.OGCDDamage : AutoAction.None;
            SupportedSpell(AID.Overpower).PlaceholderForStrategy = _config.FullRotation ? AutoAction.GCDDamage | AutoAction.OGCDDamage | AutoAction.AOEDamage : AutoAction.None;
            SupportedSpell(AID.Maim).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextMaimComboAction(ComboLastMove)) : null;
            SupportedSpell(AID.StormEye).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.StormEye)) : null;
            SupportedSpell(AID.StormPath).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.StormPath)) : null;
            SupportedSpell(AID.MythrilTempest).TransformAction = _config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextAOEComboAction(ComboLastMove)) : null;
            SupportedSpell(AID.NascentFlash).TransformTarget = _config.SmartNascentFlashShirkTarget ? SmartTargetCoTank : null;
            SupportedSpell(AID.Shirk).TransformTarget = _config.SmartNascentFlashShirkTarget ? SmartTargetCoTank : null;
            SupportedSpell(AID.Provoke).TransformTarget = _config.ProvokeMouseover ? SmartTargetHostile : null;
            SupportedSpell(AID.Holmgang).TransformTarget = _config.HolmgangSelf ? _ => Player.InstanceID : null;
        }

        private AID ComboLastMove => (AID)ActionManagerEx.Instance!.ComboLastMove;

        // check whether any targetable enemies are in reprisal range (TODO: consider checking only target?..)
        private bool AllowReprisal()
        {
            return Autorot.PotentialTargetsInRangeFromPlayer(5).Any();
        }

        private bool AllowInterject(ulong targetID)
        {
            return Autorot.WorldState.Actors.Find(targetID)?.CastInfo?.Interruptible ?? false;
        }
    }
}
