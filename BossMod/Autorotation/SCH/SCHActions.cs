using System;
using System.Linq;

namespace BossMod.SCH
{
    class Actions : CommonActions
    {
        public struct AIDecisionState
        {
            public Actor? MultidotTarget;
            public Actor? MostDamagedPartyMember;
            public Actor? EsunablePartyMember;
            public bool UseWhisperingDawn;
        }

        private SCHConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;
        private AIDecisionState _aiState = new();

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            PreferredRange = 25;

            _config = Service.Config.Get<SCHConfig>();
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
            UpdateAIState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionMnd);
        }

        protected override void QueueAIActions()
        {
            // TODO: move stuff here...
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (_state.Unlocked(MinLevel.SummonFairy) && _state.Fairy == null)
                return MakeResult(_config.PreferSelene ? AID.SummonSelene : AID.SummonEos, Player);

            // AI actions (TODO: revise at L35)
            if (_aiState.MostDamagedPartyMember != null && _aiState.MostDamagedPartyMember.HP.Cur <= _aiState.MostDamagedPartyMember.HP.Max * 0.5f)
                return MakeResult(Rotation.GetNextBestSTHealGCD(_state), _aiState.MostDamagedPartyMember);
            if (_aiState.EsunablePartyMember != null)
                return MakeResult(AID.Esuna, _aiState.EsunablePartyMember);
            // TODO: prepull adlo on ??? (master? tank?)

            // normal damage actions
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionFirstFight)
                return new();
            var res = Rotation.GetNextBestSTDamageGCD(_state, AutoAction == AutoActionAIFightMove, _aiState.MultidotTarget != null);
            return MakeResult(res, res is AID.Bio1 or AID.Bio2 or AID.Biolysis ? _aiState.MultidotTarget! : Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionFirstFight)
                return new();

            NextAction res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = CalculateAutomaticOGCDInSlot(Autorot.PrimaryTarget, deadline - _state.OGCDSlotLength);
            if (!res.Action && _state.CanWeave(deadline)) // second/only ogcd slot
                res = CalculateAutomaticOGCDInSlot(Autorot.PrimaryTarget, deadline);
            return res;
        }

        private NextAction CalculateAutomaticOGCDInSlot(Actor primaryTarget, float deadline)
        {
            // TODO: L40+
            // whispering dawn, if AI wants to
            if (_aiState.UseWhisperingDawn && _state.Unlocked(MinLevel.WhisperingDawn) && _state.CanWeave(CDGroup.WhisperingDawn, 0.6f, deadline))
                return MakeResult(AID.WhisperingDawn, Player);

            // lucid dreaming, if we won't waste mana (TODO: revise mp limit)
            if (_state.CurMP <= 8000 && _state.Unlocked(MinLevel.LucidDreaming) && _state.CanWeave(CDGroup.LucidDreaming, 0.6f, deadline))
                return MakeResult(AID.LucidDreaming, Player);

            // TODO: swiftcast...

            return new();
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

            if (_state.Fairy?.IsDestroyed ?? false)
                _state.Fairy = null;
            if (_state.Fairy == null)
                _state.Fairy = Autorot.WorldState.Actors.FirstOrDefault(a => a.Type == ActorType.Pet && a.OwnerID == Player.InstanceID);

            //var gauge = Service.JobGauges.Get<SCHGauge>();

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

        private void UpdateAIState()
        {
            if (AutoAction < AutoActionFirstCustom)
            {
                _aiState.MultidotTarget = AutoAction < AutoActionFirstFight ? null : Autorot.PotentialTargetsInRangeFromPlayer(25).FirstOrDefault(WithoutDOT);
                _aiState.MostDamagedPartyMember = Autorot.WorldState.Party.WithoutSlot().MaxBy(p => p.HP.Max - p.HP.Cur);
                _aiState.EsunablePartyMember = Autorot.WorldState.Party.WithoutSlot().FirstOrDefault(p => p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)));

                // TODO: better whispering dawn condition...
                var numWhisperingDawnTargets = _state.Fairy != null && _state.Unlocked(MinLevel.WhisperingDawn) ? Autorot.WorldState.Party.WithoutSlot().Where(p => p.HP.Cur < p.HP.Max).InRadius(_state.Fairy.Position, 15).Count() : 0;
                _aiState.UseWhisperingDawn = numWhisperingDawnTargets > 2;
                if (!_aiState.UseWhisperingDawn && numWhisperingDawnTargets > 0)
                {
                    // also use it if most-damaged has large hp deficit and would be hit
                    var mainHealTarget = _aiState.MostDamagedPartyMember!; // guaranteed to be non-null due to num-targets check
                    _aiState.UseWhisperingDawn = mainHealTarget.HP.Cur < mainHealTarget.HP.Max * 0.8f && (mainHealTarget.Position - _state.Fairy!.Position).LengthSq() <= 15 * 15;
                }
            }
            else
            {
                _aiState = new();
            }
        }

        private bool WithoutDOT(Actor a) => !a.Statuses.Any(s => s.SourceID == Player.InstanceID && (SID)s.ID is SID.Bio1 or SID.Bio2);

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // upgrades
            SupportedSpell(AID.Ruin1).TransformAction = SupportedSpell(AID.Broil1).TransformAction = SupportedSpell(AID.Broil2).TransformAction = SupportedSpell(AID.Broil3).TransformAction = SupportedSpell(AID.Broil4).TransformAction
                = () => ActionID.MakeSpell(Rotation.GetBroilAction(_state));
            SupportedSpell(AID.Bio1).TransformAction = SupportedSpell(AID.Bio2).TransformAction = SupportedSpell(AID.Biolysis).TransformAction = () => ActionID.MakeSpell(Rotation.GetBioAction(_state));
            SupportedSpell(AID.ArtOfWar1).TransformAction = SupportedSpell(AID.ArtOfWar2).TransformAction = () => ActionID.MakeSpell(Rotation.GetArtOfWarAction(_state));

            // self-targeted spells
            SupportedSpell(AID.ArtOfWar1).TransformTarget = SupportedSpell(AID.ArtOfWar2).TransformTarget = SupportedSpell(AID.Succor).TransformTarget
                = SupportedSpell(AID.SummonEos).TransformTarget = SupportedSpell(AID.SummonSelene).TransformTarget
                = SupportedSpell(AID.WhisperingDawn).TransformTarget = SupportedSpell(AID.Indomitability).TransformTarget = SupportedSpell(AID.EmergencyTactics).TransformTarget
                = SupportedSpell(AID.DissolveUnion).TransformTarget = SupportedSpell(AID.FeyBlessing).TransformTarget = SupportedSpell(AID.Consolation).TransformTarget
                = SupportedSpell(AID.LucidDreaming).TransformTarget = SupportedSpell(AID.Swiftcast).TransformTarget = SupportedSpell(AID.FeyIllumination).TransformTarget
                = SupportedSpell(AID.Surecast).TransformTarget = SupportedSpell(AID.Aetherflow).TransformTarget = SupportedSpell(AID.Dissipation).TransformTarget
                = SupportedSpell(AID.Recitation).TransformTarget = SupportedSpell(AID.SummonSeraph).TransformTarget = SupportedSpell(AID.Expedient).TransformTarget
                = _ => Player;

            // placeholders
            //SupportedSpell(AID.Ruin1).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            //SupportedSpell(AID.ArtOfWar1).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // smart targets
            SupportedSpell(AID.Physick).TransformTarget = SupportedSpell(AID.Adloquium).TransformTarget = SupportedSpell(AID.Lustrate).TransformTarget
                = SupportedSpell(AID.DeploymentTactics).TransformTarget = SupportedSpell(AID.Excogitation).TransformTarget = SupportedSpell(AID.Aetherpact).TransformTarget
                = SupportedSpell(AID.Resurrection).TransformTarget = SupportedSpell(AID.Esuna).TransformTarget = SupportedSpell(AID.Rescue).TransformTarget
                = _config.MouseoverFriendly ? SmartTargetFriendly : null;
        }
    }
}
