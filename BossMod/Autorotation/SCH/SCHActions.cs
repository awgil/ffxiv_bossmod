using System;
using System.Linq;

namespace BossMod.SCH
{
    // TODO: this is shit, like all healer modules...
    class Actions : HealerActions
    {
        private SCHConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;
        private (Actor? Target, float HPRatio) _bestSTHeal;
        private bool _allowDelayingNextGCD;

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
            base.Dispose();
        }

        public override Targeting SelectBetterTarget(Actor initial)
        {
            // TODO: select target for art of war...

            // look for target to multidot, if initial target already has dot
            if (_state.Unlocked(MinLevel.Bio1) && !WithoutDOT(initial))
            {
                var multidotTarget = Autorot.PotentialTargetsInRangeFromPlayer(25).FirstOrDefault(t => t != initial && WithoutDOT(t));
                if (multidotTarget != null)
                    return new(multidotTarget, 10);
            }

            return new(initial, 10);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            base.UpdateInternalState(autoAction);
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionMnd);
            _strategy.NumWhisperingDawnTargets = _state.Fairy != null && _state.Unlocked(MinLevel.WhisperingDawn) ? CountAOEHealTargets(15, _state.Fairy.Position) : 0;
            _strategy.NumSuccorTargets = _state.Unlocked(MinLevel.Succor) ? CountAOEHealTargets(15, Player.Position) : 0;
            _strategy.Moving = autoAction is AutoActionAIIdleMove or AutoActionAIFightMove;
            _bestSTHeal = FindBestSTHealTarget();
        }

        protected override void QueueAIActions()
        {
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            // TODO: rework, implement non-ai...
            if (_state.Unlocked(MinLevel.SummonFairy) && _state.Fairy == null)
                return MakeResult(_config.PreferSelene ? AID.SummonSelene : AID.SummonEos, Player);

            // AI: aoe heals > st heals > esuna > damage
            // i don't really think 'rotation/actions' split is particularly good fit for healers AI...
            // TODO: raise support...
            bool allowCasts = !_strategy.Moving || _state.SwiftcastLeft > _state.GCD;

            // TODO: L45+
            if (allowCasts && _strategy.NumSuccorTargets > 2 && _state.CurMP >= 1000)
                return MakeResult(AID.Succor, Player);

            // now check ST heal
            if (allowCasts && _bestSTHeal.Target != null)
                return MakeResult(Rotation.GetNextBestSTHealGCD(_state, _strategy), _bestSTHeal.Target);

            // now check esuna
            if (allowCasts)
            {
                var esunaTarget = FindEsunaTarget();
                if (esunaTarget != null)
                    return MakeResult(AID.Esuna, esunaTarget);
            }

            // prepull adlo, if allowed
            var preshieldTarget = _state.Unlocked(MinLevel.Adloquium) ? FindProtectTarget() : null;
            if (preshieldTarget != null && StatusDetails(preshieldTarget, SID.Galvanize, Player.InstanceID).Left <= _state.GCD)
                return MakeResult(AID.Adloquium, preshieldTarget);

            // finally perform damage rotation
            // TODO: art of war
            if (Autorot.PrimaryTarget != null)
                return MakeResult(Rotation.GetNextBestSTDamageGCD(_state, _strategy), Autorot.PrimaryTarget);

            return new(); // chill
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (deadline < float.MaxValue && _allowDelayingNextGCD)
                deadline += 0.4f + _state.AnimationLockDelay;

            NextAction res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = GetNextBestOGCD(deadline - _state.OGCDSlotLength);
            if (!res.Action && _state.CanWeave(deadline)) // second/only ogcd slot
                res = GetNextBestOGCD(deadline - _state.OGCDSlotLength);
            return res;
        }

        private NextAction GetNextBestOGCD(float deadline)
        {
            // TODO: L45+
            // TODO: fey illumination

            // whispering dawn, if it hits 3+ targets or hits st heal target
            if (_strategy.NumWhisperingDawnTargets > 0)
            {
                // TODO: better whispering dawn condition...
                if (_strategy.NumWhisperingDawnTargets > 2 || _bestSTHeal.Target != null && (_bestSTHeal.Target.Position - _state.Fairy!.Position).LengthSq() <= 15 * 15)
                    return MakeResult(AID.WhisperingDawn, Player);
            }

            // swiftcast, if can't cast any gcd
            if (deadline >= 10000 && _strategy.Moving && _state.Unlocked(MinLevel.Swiftcast) && _state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline))
                return MakeResult(AID.Swiftcast, Player);

            // lucid dreaming, if we won't waste mana (TODO: revise mp limit)
            if (_state.CurMP <= 7000 && _state.Unlocked(MinLevel.LucidDreaming) && _state.CanWeave(CDGroup.LucidDreaming, 0.6f, deadline))
                return MakeResult(AID.LucidDreaming, Player);

            return new();
        }

        protected override void OnActionExecuted(ActionID action, Actor? target)
        {
            Log($"Executed {action} @ {target} [{_state}]");
        }

        protected override void OnActionSucceeded(ActorCastEvent ev)
        {
            Log($"Succeeded {ev.Action} @ {ev.MainTargetID:X} [{_state}]");
            // note: our GCD heals have cast time 2 => 1.6 under POM (minus haste), +0.1 anim lock => after them we have 0.3 or even slightly smaller window
            // we want to be able to cast at least one oGCD after them, even if it means slightly delaying next GCD
            _allowDelayingNextGCD = ev.Action.Type == ActionType.Spell && (AID)ev.Action.ID is AID.Adloquium or AID.Succor;
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            if (_state.Fairy == null || _state.Fairy.IsDestroyed)
                _state.Fairy = Autorot.WorldState.Actors.FirstOrDefault(a => a.Type == ActorType.Pet && a.OwnerID == Player.InstanceID);

            //var gauge = Service.JobGauges.Get<SCHGauge>();

            _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
            _state.TargetBioLeft = StatusDetails(Autorot.PrimaryTarget, _state.ExpectedBio, Player.InstanceID).Left;
        }

        private bool WithoutDOT(Actor a) => Rotation.RefreshDOT(_state, StatusDetails(a, _state.ExpectedBio, Player.InstanceID).Left);

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
