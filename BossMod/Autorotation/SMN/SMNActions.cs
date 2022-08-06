using Dalamud.Game.ClientState.JobGauge.Types;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.SMN
{
    class Actions : CommonActions
    {
        private SMNConfig _config;
        private bool _aoe;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            PreferredRange = 25;

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

        protected override void UpdateInternalState(AutoAction strategy)
        {
            _aoe = (AutoStrategy & AutoAction.AOEDamage) != 0 && Autorot.PrimaryTarget != null && Autorot.PotentialTargetsInRange(Autorot.PrimaryTarget.Position, 5).Count() >= 3;
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (_strategy.Prepull && _state.Unlocked(MinLevel.SummonCarbuncle) && !_state.PetSummoned)
                return MakeResult(ActionID.MakeSpell(AID.SummonCarbuncle), Player);
            if (Autorot.PrimaryTarget == null)
                return new();
            if ((AutoStrategy & AutoAction.GCDHeal) != 0)
                return MakeResult(ActionID.MakeSpell(AID.Physick), Autorot.PrimaryTarget); // TODO: automatic target selection
            var aid = Rotation.GetNextBestGCD(_state, _strategy, _aoe, (AutoStrategy & AutoAction.NoCast) != 0);
            return aid != AID.None ? MakeResult(ActionID.MakeSpell(aid), Autorot.PrimaryTarget) : new();
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
            // TODO: upgrades

            // self-targeted spells
            SupportedSpell(AID.AstralFlow).TransformTarget = SupportedSpell(AID.SummonCarbuncle).TransformTarget
                = SupportedSpell(AID.Aethercharge).TransformTarget = SupportedSpell(AID.DreadwyrmTrance).TransformTarget
                = SupportedSpell(AID.SearingLight).TransformTarget = SupportedSpell(AID.Swiftcast).TransformTarget = SupportedSpell(AID.LucidDreaming).TransformTarget
                = SupportedSpell(AID.RadiantAegis).TransformTarget = SupportedSpell(AID.Surecast).TransformTarget
                = _ => Player;

            // placeholders
            SupportedSpell(AID.Ruin1).PlaceholderForStrategy = _config.FullRotation ? AutoAction.GCDDamage | AutoAction.OGCDDamage : AutoAction.None;
            SupportedSpell(AID.Outburst).PlaceholderForStrategy = _config.FullRotation ? AutoAction.GCDDamage | AutoAction.OGCDDamage | AutoAction.AOEDamage : AutoAction.None;

            // smart targets
            SupportedSpell(AID.Physick).TransformTarget = _config.MouseoverFriendly ? SmartTargetFriendly : null;
        }

        private AID SmartResurrectAction()
        {
            // 1. swiftcast, if ready and not up yet
            if (_state.Unlocked(MinLevel.Swiftcast) && _state.SwiftcastLeft <= 0 && _state.CD(CDGroup.Swiftcast) <= 0)
                return AID.Swiftcast;

            return AID.Resurrection;
        }
    }
}
