using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.BRD
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private BRDConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.QuestsPerLevel, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<BRDConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            // upgrades
            SupportedSpell(AID.HeavyShot).TransformAction = SupportedSpell(AID.BurstShot).TransformAction = () => ActionID.MakeSpell(_state.BestBurstShot);
            SupportedSpell(AID.StraightShot).TransformAction = SupportedSpell(AID.RefulgentArrow).TransformAction = () => ActionID.MakeSpell(_state.BestRefulgentArrow);
            SupportedSpell(AID.VenomousBite).TransformAction = SupportedSpell(AID.CausticBite).TransformAction = () => ActionID.MakeSpell(_state.BestCausticBite);
            SupportedSpell(AID.Windbite).TransformAction = SupportedSpell(AID.Stormbite).TransformAction = () => ActionID.MakeSpell(_state.BestStormbite);
            SupportedSpell(AID.QuickNock).TransformAction = SupportedSpell(AID.Ladonsbite).TransformAction = () => ActionID.MakeSpell(_state.BestLadonsbite);

            SupportedSpell(AID.Peloton).Condition = _ => !Player.InCombat;

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override Targeting SelectBetterTarget(Actor initial)
        {
            // TODO: best target for aoe...
            return new(initial, 12);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            _strategy.AOE = autoAction switch
            {
                AutoActionST => false,
                AutoActionAOE => true,
                _ => Autorot.PrimaryTarget != null && NumTargetsHitByLadonsbite(Autorot.PrimaryTarget) >= 2,
            };
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(MinLevel.SecondWind))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.5f);
            if (_state.Unlocked(MinLevel.WardensPaean))
            {
                var esunableTarget = Autorot.WorldState.Party.WithoutSlot().FirstOrDefault(p => p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)));
                SimulateManualActionForAI(ActionID.MakeSpell(AID.WardensPaean), esunableTarget, esunableTarget != null);
            }
            if (_state.Unlocked(MinLevel.Peloton))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Peloton), Player, !Player.InCombat && _state.PelotonLeft < 3 && AutoAction is AutoActionAIIdleMove or AutoActionAIFightMove);
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

            //s.Chakra = Service.JobGauges.Get<BRDGauge>().Chakra;

            _state.StraightShotLeft = _state.RagingStrikesLeft = _state.BarrageLeft = _state.PelotonLeft = 0;
            foreach (var status in Player.Statuses)
            {
                switch ((SID)status.ID)
                {
                    case SID.StraightShotReady:
                        _state.StraightShotLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.RagingStrikes:
                        _state.RagingStrikesLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.Barrage:
                        _state.BarrageLeft = StatusDuration(status.ExpireAt);
                        break;
                    case SID.Peloton:
                        _state.PelotonLeft = StatusDuration(status.ExpireAt);
                        break;
                }
            }

            _state.TargetCausticLeft = _state.TargetStormbiteLeft = 0;
            if (Autorot.PrimaryTarget != null)
            {
                foreach (var status in Autorot.PrimaryTarget.Statuses)
                {
                    switch ((SID)status.ID)
                    {
                        case SID.VenomousBite:
                        case SID.CausticBite:
                            if (status.SourceID == Player.InstanceID)
                                _state.TargetCausticLeft = StatusDuration(status.ExpireAt);
                            break;
                        case SID.Windbite:
                        case SID.Stormbite:
                            if (status.SourceID == Player.InstanceID)
                                _state.TargetStormbiteLeft = StatusDuration(status.ExpireAt);
                            break;
                    }
                }
            }
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            // placeholders
            SupportedSpell(AID.HeavyShot).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.QuickNock).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            // smart targets
        }

        private int NumTargetsHitByLadonsbite(Actor primary)
        {
            var dir = Angle.FromDirection(primary.Position - Player.Position);
            return 1 + Autorot.PotentialTargets.Count(a => a != primary && a.Position.InCone(Player.Position, dir, 45.Degrees()));
        }
    }
}
