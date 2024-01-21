﻿using System;
using System.Linq;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.SGE
{
    // TODO: this is shit, like all healer modules...
    class Actions : HealerActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private SGEConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<SGEConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

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

        private Actor? _raiseTarget;
        private Actor? _kardiaTarget;

        protected override void UpdateInternalState(int autoAction)
        {
            base.UpdateInternalState(autoAction);
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionMnd);
            _strategy.NumDyskrasiaTargets =
                autoAction == AutoActionST
                    ? 0
                    : Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5f);
            _strategy.NumToxikonTargets =
                (autoAction == AutoActionST || Autorot.PrimaryTarget == null)
                    ? 0
                    : Autorot.Hints.NumPriorityTargetsInAOECircle(
                        Autorot.PrimaryTarget.Position,
                        5f
                    );
        }

        protected override void QueueAIActions() { }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (_config.AutoEsuna && Rotation.CanCast(_state, _strategy, 1))
            {
                var esunaTarget = FindEsunaTarget();
                if (esunaTarget != null)
                    return MakeResult(AID.Esuna, esunaTarget);
            }

            if (
                _raiseTarget != null
                && _state.SwiftcastLeft > _state.GCD
                && _state.Unlocked(AID.Egeiro)
            )
                return MakeResult(AID.Egeiro, _raiseTarget);

            return MakeResult(Rotation.GetNextBestGCD(_state, _strategy), Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (AutoAction < AutoActionAIFight)
                return new();

            NextAction res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = GetNextBestOGCD(deadline - _state.OGCDSlotLength);
            if (!res.Action && _state.CanWeave(deadline)) // second/only ogcd slot
                res = GetNextBestOGCD(deadline);

            return res;
        }

        private NextAction GetNextBestOGCD(float deadline)
        {
            if (
                _kardiaTarget != null
                && Player.FindStatus((uint)SID.Kardia) == null
                && _state.Unlocked(AID.Kardia)
                && _state.CanWeave(CDGroup.Kardia, 0.6f, deadline)
            )
                return MakeResult(ActionID.MakeSpell(AID.Kardia), _kardiaTarget);

            if (
                _raiseTarget != null
                && _state.SwiftcastLeft == 0
                && _state.CanWeave(CDGroup.Swiftcast, 0.6f, deadline)
            )
                return MakeResult(ActionID.MakeSpell(AID.Swiftcast), Player);

            var ogcd = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

            if (
                !ogcd
                && _config.PreventGallOvercap
                && (_state.Gall == 3 || (_state.Gall == 2 && _state.NextGall < 2.5))
                && _state.CurMP <= 9000
            )
                return MakeResult(
                    ActionID.MakeSpell(AID.Druochole),
                    FindBestSTHealTarget(1).Target ?? Player
                );

            return MakeResult(ogcd, Autorot.PrimaryTarget);
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            var gauge = Service.JobGauges.Get<SGEGauge>();
            _state.Gall = gauge.Addersgall;
            _state.Sting = gauge.Addersting;
            _state.NextGall = MathF.Max(0, 20f - (gauge.AddersgallTimer / 1000f));
            _state.Eukrasia = gauge.Eukrasia;

            _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
            _state.TargetDotLeft = StatusDetails(
                Autorot.PrimaryTarget,
                _state.ExpectedEudosis,
                Player.InstanceID
            ).Left;

            _raiseTarget = _config.AutoRaise ? FindRaiseTarget() : null;
            _kardiaTarget = _config.AutoKardia ? FindKardiaTarget() : null;
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.Dosis).PlaceholderForAuto = _config.FullRotation
                ? AutoActionST
                : AutoActionNone;
            SupportedSpell(AID.Dyskrasia).PlaceholderForAuto = _config.FullRotation
                ? AutoActionAOE
                : AutoActionNone;

            // smart targets
            SupportedSpell(AID.Diagnosis).TransformTarget =
                SupportedSpell(AID.Druochole).TransformTarget =
                SupportedSpell(AID.Kardia).TransformTarget =
                SupportedSpell(AID.Taurochole).TransformTarget =
                SupportedSpell(AID.Krasis).TransformTarget =
                SupportedSpell(AID.Haima).TransformTarget =
                SupportedSpell(AID.Esuna).TransformTarget =
                SupportedSpell(AID.Rescue).TransformTarget =
                    _config.MouseoverFriendly ? SmartTargetFriendly : null;

            SupportedSpell(AID.Icarus).TransformTarget = _config.MouseoverIcarus
                ? (act) => Autorot.SecondaryTarget ?? act
                : null;
        }

        private Actor? FindRaiseTarget()
        {
            if (!_config.AutoRaise)
                return null;

            var party = Autorot.WorldState.Party.WithoutSlot(includeDead: true, partyOnly: true);
            // if all tanks dead, raise tank, otherwise h -> t -> d
            var tanks = party.Where(x => x.Class.GetRole() == Role.Tank);
            if (tanks.Any() && tanks.All(x => x.IsDead))
                return tanks.First();

            return party
                .Where(x => CanBeRaised(x) && RangeToTarget(x) <= 30)
                .MaxBy(
                    x =>
                        x.Class.GetRole() == Role.Healer
                            ? 10
                            : x.Class.GetRole() == Role.Tank
                                ? 9
                                : x.Class is Class.RDM or Class.SMN
                                    ? 8
                                    : 7
                );
        }

        private Actor? FindKardiaTarget()
        {
            if (!_config.AutoKardia)
                return null;

            var party = Autorot.WorldState.Party.WithoutSlot(partyOnly: true);

            if (party.Count(x => x.Type == ActorType.Player) == 1)
                return Player;

            var tanks = party.Where(x => x.Class.GetRole() == Role.Tank);
            if (tanks.Count() == 1)
                return tanks.First();

            return null;
        }

        private float RangeToTarget(Actor a1)
        {
            return (a1.Position - Player.Position).Length() - a1.HitboxRadius - Player.HitboxRadius;
        }
    }
}
