using System;
using System.Linq;

namespace BossMod.SCH
{
    // TODO: this is shit, like all healer modules...
    class Actions : CommonActions
    {
        private SCHConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<SCHConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            // upgrades
            SupportedSpell(AID.Ruin1).TransformAction = SupportedSpell(AID.Broil1).TransformAction = SupportedSpell(AID.Broil2).TransformAction = SupportedSpell(AID.Broil3).TransformAction = SupportedSpell(AID.Broil4).TransformAction = () => ActionID.MakeSpell(_state.BestBroil);
            SupportedSpell(AID.Bio1).TransformAction = SupportedSpell(AID.Bio2).TransformAction = SupportedSpell(AID.Biolysis).TransformAction = () => ActionID.MakeSpell(_state.BestBio);
            SupportedSpell(AID.ArtOfWar1).TransformAction = SupportedSpell(AID.ArtOfWar2).TransformAction = () => ActionID.MakeSpell(_state.BestArtOfWar);

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override Targeting SelectBetterTarget(Actor initial)
        {
            // TODO: select target for art of war...

            // look for target to multidot, if initial target already has dot
            if (_state.Unlocked(MinLevel.Bio1) && !WithoutDOT(initial))
            {
                var multidotTarget = Autorot.PotentialTargetsInRangeFromPlayer(25).FirstOrDefault(t => t != initial && WithoutDOT(t));
                if (multidotTarget != null)
                    return new(multidotTarget, 25);
            }

            return new(initial, 25);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionMnd);
            if (autoAction < AutoActionFirstCustom)
            {
                _strategy.HealTarget = Autorot.WorldState.Party.WithoutSlot().MaxBy(p => p.HP.Max - p.HP.Cur);
                if (_strategy.HealTarget != null && _strategy.HealTarget.HP.Cur > _strategy.HealTarget.HP.Max * 0.5f)
                    _strategy.HealTarget = null;
                _strategy.Moving = autoAction is AutoActionAIIdleMove or AutoActionAIFightMove;
            }
            else
            {
                _strategy.HealTarget = null;
                _strategy.Moving = false;
            }
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(MinLevel.Esuna))
            {
                var esunableTarget = _strategy.HealTarget != null ? null : Autorot.WorldState.Party.WithoutSlot().FirstOrDefault(p => p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)));
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Esuna), esunableTarget, esunableTarget != null);
            }
            if (_state.Unlocked(MinLevel.WhisperingDawn))
            {
                // TODO: better whispering dawn condition...
                var numWhisperingDawnTargets = _state.Fairy != null ? Autorot.WorldState.Party.WithoutSlot().Where(p => p.HP.Cur < p.HP.Max).InRadius(_state.Fairy.Position, 15).Count() : 0;
                bool useWhisperingDawn = numWhisperingDawnTargets > 2;
                if (!useWhisperingDawn && numWhisperingDawnTargets > 0)
                {
                    // also use it if most-damaged has large hp deficit and would be hit
                    var mainHealTarget = Autorot.WorldState.Party.WithoutSlot().MaxBy(p => p.HP.Max - p.HP.Cur)!; // guaranteed to be non-null due to num-targets check
                    useWhisperingDawn = mainHealTarget.HP.Cur < mainHealTarget.HP.Max * 0.8f && (mainHealTarget.Position - _state.Fairy!.Position).LengthSq() <= 15 * 15;
                }
                SimulateManualActionForAI(ActionID.MakeSpell(AID.WhisperingDawn), Player, useWhisperingDawn);
            }
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (_state.Unlocked(MinLevel.SummonFairy) && _state.Fairy == null)
                return MakeResult(_config.PreferSelene ? AID.SummonSelene : AID.SummonEos, Player);

            // AI actions (TODO: revise at L35)
            if (_strategy.HealTarget != null)
                return MakeResult(Rotation.GetNextBestSTHealGCD(_state, _strategy), _strategy.HealTarget);
            // TODO: prepull adlo on ??? (master? tank?)

            // normal damage actions
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionFirstFight)
                return new();
            var res = Rotation.GetNextBestSTDamageGCD(_state, _strategy);
            return MakeResult(res, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionFirstFight)
                return new();

            NextAction res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = MakeResult(Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength), _strategy.HealTarget ?? Autorot.PrimaryTarget!);
            if (!res.Action && _state.CanWeave(deadline)) // second/only ogcd slot
                res = MakeResult(Rotation.GetNextBestOGCD(_state, _strategy, deadline), _strategy.HealTarget ?? Autorot.PrimaryTarget!);
            return res;
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

            _state.TargetBioLeft = 0;
            if (Autorot.PrimaryTarget != null)
            {
                foreach (var status in Autorot.PrimaryTarget.Statuses)
                {
                    switch ((SID)status.ID)
                    {
                        case SID.Bio1:
                        case SID.Bio2:
                        case SID.Biolysis:
                            if (status.SourceID == Player.InstanceID)
                                _state.TargetBioLeft = StatusDuration(status.ExpireAt);
                            break;
                    }
                }
            }
        }

        private bool WithoutDOT(Actor a)
        {
            var dot = a.Statuses.FirstOrDefault(s => s.SourceID == Player.InstanceID && (SID)s.ID is SID.Bio1 or SID.Bio2 or SID.Biolysis);
            return dot.ID == 0 || Rotation.RefreshDOT(_state, StatusDuration(dot.ExpireAt));
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
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
