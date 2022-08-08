using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Linq;

namespace BossMod.WHM
{
    // TODO: this is shit, like all healer modules...
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;
        public const int AutoActionSTHeal = AutoActionFirstCustom + 2;
        public const int AutoActionAOEHeal = AutoActionFirstCustom + 3;

        private WHMConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<WHMConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            // upgrades
            SupportedSpell(AID.Stone1).TransformAction = SupportedSpell(AID.Stone2).TransformAction = SupportedSpell(AID.Stone3).TransformAction = SupportedSpell(AID.Stone4).TransformAction
                = SupportedSpell(AID.Glare1).TransformAction = SupportedSpell(AID.Glare3).TransformAction = () => ActionID.MakeSpell(_state.BestGlare);
            SupportedSpell(AID.Aero1).TransformAction = SupportedSpell(AID.Aero2).TransformAction = SupportedSpell(AID.Dia).TransformAction = () => ActionID.MakeSpell(_state.BestDia);
            SupportedSpell(AID.Holy1).TransformAction = SupportedSpell(AID.Holy3).TransformAction = () => ActionID.MakeSpell(_state.BestHoly);

            SupportedSpell(AID.DivineBenison).Condition = target => target != null && target.FindStatus(SID.DivineBenison, Player.InstanceID) == null; // check whether potential divine benison target doesn't already have it applied

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
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionMnd);
            _strategy.NumAssizeMedica1Targets = CountAOEHealTargets(15, Player.Position);
            _strategy.NumRaptureMedica2Targets = CountAOEHealTargets(20, Player.Position);
            _strategy.NumCure3Targets = SmartCure3Target().Item2;
            _strategy.EnableAssize = AllowAssize(); // note: should be plannable...
            _strategy.AllowReplacingHealWithMisery = _config.NeverOvercapBloodLilies && Autorot.PrimaryTarget?.Type == ActorType.Enemy;
            _strategy.Moving = false;
            if (autoAction < AutoActionFirstCustom)
            {
                _strategy.HealTarget = Autorot.WorldState.Party.WithoutSlot().MaxBy(p => p.HP.Max - p.HP.Cur);
                if (_strategy.HealTarget != null && _strategy.HealTarget.HP.Cur > _strategy.HealTarget.HP.Max * 0.7f)
                    _strategy.HealTarget = null;
                if (_strategy.HealTarget != null) // TODO: this aoe/st heal selection is not very good...
                    _strategy.AOE = _strategy.NumAssizeMedica1Targets > 2 || _strategy.NumRaptureMedica2Targets > 2 || _strategy.NumCure3Targets > 2;
                else
                    _strategy.AOE = Autorot.PotentialTargetsInRangeFromPlayer(8).Count() >= 3;
                _strategy.Moving = autoAction is AutoActionAIIdleMove or AutoActionAIFightMove;
            }
            else if (autoAction is AutoActionST or AutoActionAOE)
            {
                _strategy.HealTarget = null;
                _strategy.AOE = autoAction == AutoActionAOE;
            }
            else
            {
                _strategy.HealTarget = (_config.MouseoverFriendly ? SmartTargetFriendly(Autorot.PrimaryTarget) : Autorot.PrimaryTarget) ?? Player;
                _strategy.AOE = autoAction == AutoActionAOEHeal;
            }
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(MinLevel.Esuna))
            {
                var esunableTarget = _strategy.HealTarget != null ? null : Autorot.WorldState.Party.WithoutSlot().FirstOrDefault(p => p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)));
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Esuna), esunableTarget, esunableTarget != null);
            }
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            // heals
            if (_strategy.HealTarget != null)
                return MakeResult(Rotation.GetNextBestGCD(_state, _strategy), _strategy.HealTarget);

            // normal damage actions
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionFirstFight)
                return new();
            var res = Rotation.GetNextBestGCD(_state, _strategy);
            return MakeResult(res, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (_strategy.HealTarget == null && (Autorot.PrimaryTarget == null || AutoAction < AutoActionFirstFight))
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

            var gauge = Service.JobGauges.Get<WHMGauge>();
            _state.NormalLilies = gauge.Lily;
            _state.BloodLilies = gauge.BloodLily;
            _state.NextLilyIn = 30 - gauge.LilyTimer * 0.001f;

            _state.SwiftcastLeft = _state.ThinAirLeft = _state.FreecureLeft = _state.MedicaLeft = 0;
            foreach (var status in Player.Statuses)
            {
                switch ((SID)status.ID)
                {
                    case SID.Swiftcast:
                        _state.SwiftcastLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.ThinAir:
                        _state.ThinAirLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.Freecure:
                        _state.FreecureLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.Medica2:
                        if (status.SourceID == Player.InstanceID)
                            _state.MedicaLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }

            _state.TargetDiaLeft = 0;
            if (Autorot.PrimaryTarget != null)
            {
                foreach (var status in Autorot.PrimaryTarget.Statuses)
                {
                    switch ((SID)status.ID)
                    {
                        case SID.Aero1:
                        case SID.Aero2:
                        case SID.Dia:
                            if (status.SourceID == Player.InstanceID)
                                _state.TargetDiaLeft = StatusDuration(status.ExpireAt);
                            break;
                    }
                }
            }
        }

        private bool WithoutDOT(Actor a) => !a.Statuses.Any(s => s.SourceID == Player.InstanceID && (SID)s.ID is SID.Aero1 or SID.Aero2 or SID.Dia);

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.Stone1).PlaceholderForAuto = SupportedSpell(AID.Stone2).PlaceholderForAuto = SupportedSpell(AID.Stone3).PlaceholderForAuto = SupportedSpell(AID.Stone4).PlaceholderForAuto
                = SupportedSpell(AID.Glare1).PlaceholderForAuto = SupportedSpell(AID.Glare3).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.Holy1).PlaceholderForAuto = SupportedSpell(AID.Holy3).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;
            SupportedSpell(AID.Cure1).PlaceholderForAuto = _config.FullRotation ? AutoActionSTHeal : AutoActionNone;
            SupportedSpell(AID.Medica1).PlaceholderForAuto = _config.FullRotation ? AutoActionAOEHeal : AutoActionNone;

            // raise replacement
            SupportedSpell(AID.Raise).TransformAction = _config.SwiftFreeRaise ? () => ActionID.MakeSpell(SmartRaiseAction()) : null;

            // smart targets
            SupportedSpell(AID.Cure1).TransformTarget = SupportedSpell(AID.Cure2).TransformTarget = SupportedSpell(AID.Regen).TransformTarget
                = SupportedSpell(AID.AfflatusSolace).TransformTarget = SupportedSpell(AID.Raise).TransformTarget = SupportedSpell(AID.Esuna).TransformTarget
                = SupportedSpell(AID.Rescue).TransformTarget = SupportedSpell(AID.DivineBenison).TransformTarget = SupportedSpell(AID.Tetragrammaton).TransformTarget
                = SupportedSpell(AID.Benediction).TransformTarget = SupportedSpell(AID.Aquaveil).TransformTarget
                = _config.MouseoverFriendly ? SmartTargetFriendly : null;
            SupportedSpell(AID.Cure3).TransformTarget = _config.SmartCure3Target ? _ => SmartCure3Target().Item1 : _config.MouseoverFriendly ? SmartTargetFriendly : null;
        }

        private AID SmartRaiseAction()
        {
            // 1. swiftcast, if ready and not up yet
            if (_state.Unlocked(MinLevel.Swiftcast) && _state.SwiftcastLeft <= 0 && _state.CD(CDGroup.Swiftcast) <= 0)
                return AID.Swiftcast;

            // 2. thin air, if ready and not up yet
            if (_state.Unlocked(MinLevel.ThinAir) && _state.ThinAirLeft <= 0 && _state.CD(CDGroup.ThinAir) <= 60)
                return AID.ThinAir;

            return AID.Raise;
        }

        private int CountAOEHealTargets(float radius, WPos center)
        {
            var rsq = radius * radius;
            return Autorot.WorldState.Party.WithoutSlot().Count(o => o.HP.Cur < o.HP.Max && (o.Position - center).LengthSq() <= rsq);
        }

        // select best target for cure3, such that most people are hit
        private (Actor?, int) SmartCure3Target()
        {
            var rsq = 30 * 30;
            return Autorot.WorldState.Party.WithoutSlot().Select(o => (o, (o.Position - Player.Position).LengthSq() <= rsq ? CountAOEHealTargets(6, o.Position) : -1)).MaxBy(oc => oc.Item2);
        }

        // check whether any targetable enemies are in assize range
        private bool AllowAssize()
        {
            return Autorot.PotentialTargetsInRangeFromPlayer(15).Any();
        }
    }
}
