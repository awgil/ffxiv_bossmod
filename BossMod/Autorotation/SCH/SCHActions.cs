using Dalamud.Game.ClientState.JobGauge.Types;
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
        private bool _allowDelayingNextGCD;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
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

        public override CommonRotation.PlayerState GetState() => _state;
        public override CommonRotation.Strategy GetStrategy() => _strategy;

        public override Targeting SelectBetterTarget(AIHints.Enemy initial)
        {
            // TODO: look for good place to cast art of war and move closer...

            // look for target to multidot, if initial target already has dot
            if (_state.Unlocked(AID.Bio1) && !WithoutDOT(initial.Actor))
            {
                var multidotTarget = Autorot.Hints.PriorityTargets.FirstOrDefault(t => t != initial && !t.ForbidDOTs && t.Actor.Position.InCircle(Player.Position, 25) && WithoutDOT(t.Actor));
                if (multidotTarget != null)
                    return new(multidotTarget, multidotTarget.StayAtLongRange ? 25 : 10);
            }

            return new(initial, initial.StayAtLongRange ? 25 : 10);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            base.UpdateInternalState(autoAction);
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionMnd);
            _strategy.NumWhisperingDawnTargets = _state.Fairy != null && _state.Unlocked(AID.WhisperingDawn) ? CountAOEHealTargets(15, _state.Fairy.Position) : 0;
            _strategy.NumSuccorTargets = _state.Unlocked(AID.Succor) ? CountAOEPreshieldTargets(15, Player.Position, (uint)SID.Galvanize, _state.GCD + 2, 10) : 0;
            _strategy.NumArtOfWarTargets = _state.Unlocked(AID.ArtOfWar1) ? Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5) : 0;
            _strategy.BestSTHeal = FindBestSTHealTarget();
        }

        protected override void QueueAIActions()
        {
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            // TODO: rework, implement non-ai...
            if (_state.Unlocked(AID.SummonSelene) && _state.Fairy == null)
                return MakeResult(_config.PreferSelene ? AID.SummonSelene : AID.SummonEos, Player);

            // AI: aoe heals > st heals > esuna > damage
            // i don't really think 'rotation/actions' split is particularly good fit for healers AI...
            // TODO: raise support...
            if (Rotation.CanCast(_state, _strategy, 2) && _strategy.NumSuccorTargets > 2 && _state.CurMP >= 1000)
                return MakeResult(AID.Succor, Player);

            // now check ST heal
            if (Rotation.CanCast(_state, _strategy, 2) && _strategy.BestSTHeal.Target != null && _state.AetherflowStacks == 0 && StatusDetails(_strategy.BestSTHeal.Target, SID.Galvanize, Player.InstanceID).Left <= _state.GCD)
                return MakeResult(Rotation.GetNextBestSTHealGCD(_state, _strategy), _strategy.BestSTHeal.Target);

            // now check esuna
            if (Rotation.CanCast(_state, _strategy, 1))
            {
                var esunaTarget = FindEsunaTarget();
                if (esunaTarget != null)
                    return MakeResult(AID.Esuna, esunaTarget);
            }

            // prepull adlo, if allowed
            var preshieldTarget = _state.Unlocked(AID.Adloquium) ? FindProtectTarget(0.5f) : null;
            if (preshieldTarget != null && Rotation.CanCast(_state, _strategy, 2) && StatusDetails(preshieldTarget, SID.Galvanize, Player.InstanceID).Left <= _state.GCD)
                return MakeResult(AID.Adloquium, preshieldTarget);

            // finally perform damage rotation
            if (_state.CurMP > 3000 && Autorot.PrimaryTarget != null)
                return MakeResult(Rotation.GetNextBestDamageGCD(_state, _strategy), Autorot.PrimaryTarget);

            return new(); // chill
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (AutoAction < AutoActionAIFight)
                return new();

            if (deadline < float.MaxValue && _allowDelayingNextGCD)
                deadline += 0.4f + _state.AnimationLockDelay;

            NextAction res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = GetNextBestOGCD(deadline - _state.OGCDSlotLength);
            if (!res.Action && _state.CanWeave(deadline)) // second/only ogcd slot
                res = GetNextBestOGCD(deadline);
            return res;
        }

        private NextAction GetNextBestOGCD(float deadline)
        {
            // TODO: L52+
            // TODO: fey illumination

            // whispering dawn, if it hits 3+ targets or hits st heal target
            if (_strategy.NumWhisperingDawnTargets > 0 && _state.CanWeave(CDGroup.WhisperingDawn, 0.6f, deadline))
            {
                // TODO: better whispering dawn condition...
                if (_strategy.NumWhisperingDawnTargets > 2 || _strategy.BestSTHeal.Target != null && (_strategy.BestSTHeal.Target.Position - _state.Fairy!.Position).LengthSq() <= 15 * 15)
                    return MakeResult(AID.WhisperingDawn, Player);
            }

            // aetherflow, if no stacks left
            if (_state.AetherflowStacks == 0 && _state.Unlocked(AID.Aetherflow) && _state.CanWeave(CDGroup.Aetherflow, 0.6f, deadline))
                return MakeResult(AID.Aetherflow, Player);

            // lustrate, if want single-target heals
            if (_strategy.BestSTHeal.Target != null && _state.AetherflowStacks > 0 && _state.Unlocked(AID.Lustrate) && _state.CanWeave(CDGroup.Lustrate, 0.6f, deadline))
                return MakeResult(AID.Lustrate, _strategy.BestSTHeal.Target);

            // energy drain, if new aetherflow will come off cd soon (TODO: reconsider...)
            if (Autorot.PrimaryTarget != null && _state.AetherflowStacks > 0 && _state.CD(CDGroup.Aetherflow) <= _state.GCD + _state.AetherflowStacks * 2.5f && _state.Unlocked(AID.EnergyDrain) && _state.CanWeave(CDGroup.EnergyDrain, 0.6f, deadline))
                return MakeResult(AID.EnergyDrain, Autorot.PrimaryTarget);

            // swiftcast, if can't cast any gcd (TODO: current check is not very good...)
            if (deadline >= 10000 && _strategy.ForceMovementIn < 5 && _state.Unlocked(AID.Swiftcast) && _state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline))
                return MakeResult(AID.Swiftcast, Player);

            // lucid dreaming, if we won't waste mana (TODO: revise mp limit)
            if (_state.CurMP <= 7000 && _state.Unlocked(AID.LucidDreaming) && _state.CanWeave(CDGroup.LucidDreaming, 0.6f, deadline))
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
            // note: our GCD heals have cast time 2 +0.1 anim lock => after them we have 0.4 or even slightly smaller window
            // we want to be able to cast at least one oGCD after them, even if it means slightly delaying next GCD
            _allowDelayingNextGCD = ev.Action.Type == ActionType.Spell && (AID)ev.Action.ID is AID.Adloquium or AID.Succor;
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            if (_state.Fairy == null || _state.Fairy.IsDestroyed)
                _state.Fairy = Autorot.WorldState.Actors.FirstOrDefault(a => a.Type == ActorType.Pet && a.OwnerID == Player.InstanceID);

            var gauge = Service.JobGauges.Get<SCHGauge>();
            _state.AetherflowStacks = gauge.Aetherflow;

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
