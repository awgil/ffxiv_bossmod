using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Linq;

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
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
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
            SupportedSpell(AID.HeadGraze).Condition = target => target?.CastInfo?.Interruptible ?? false;

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override CommonRotation.PlayerState GetState() => _state;
        public override CommonRotation.Strategy GetStrategy() => _strategy;

        public override Targeting SelectBetterTarget(AIHints.Enemy initial)
        {
            // TODO: min range to better hit clump with cone...
            // TODO: targeting for rain of death
            var bestTarget = initial;
            if (_state.Unlocked(AID.QuickNock))
            {
                var bestAOECount = NumTargetsHitByLadonsbite(initial.Actor);
                foreach (var candidate in Autorot.Hints.PriorityTargets.Where(e => e != initial && e.Actor.Position.InCircle(Player.Position, 12)))
                {
                    var candidateAOECount = NumTargetsHitByLadonsbite(candidate.Actor);
                    if (candidateAOECount > bestAOECount)
                    {
                        bestTarget = candidate;
                        bestAOECount = candidateAOECount;
                    }
                }
            }
            return new(bestTarget, bestTarget.StayAtLongRange ? 25 : 12);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            _strategy.NumLadonsbiteTargets = Autorot.PrimaryTarget != null && autoAction != AutoActionST && _state.Unlocked(AID.QuickNock) ? NumTargetsHitByLadonsbite(Autorot.PrimaryTarget) : 0;
            _strategy.NumRainOfDeathTargets = Autorot.PrimaryTarget != null && autoAction != AutoActionST && _state.Unlocked(AID.RainOfDeath) ? NumTargetsHitByRainOfDeath(Autorot.PrimaryTarget) : 0;
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(AID.HeadGraze))
            {
                var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
                SimulateManualActionForAI(ActionID.MakeSpell(AID.HeadGraze), interruptibleEnemy?.Actor, interruptibleEnemy != null);
            }
            if (_state.Unlocked(AID.SecondWind))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.5f);
            if (_state.Unlocked(AID.WardensPaean))
            {
                var esunableTarget = Autorot.WorldState.Party.WithoutSlot().FirstOrDefault(p => p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)));
                SimulateManualActionForAI(ActionID.MakeSpell(AID.WardensPaean), esunableTarget, esunableTarget != null);
            }
            if (_state.Unlocked(AID.Peloton))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Peloton), Player, !Player.InCombat && _state.PelotonLeft < 3 && _strategy.ForceMovementIn == 0);
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
                return new();
            var aid = Rotation.GetNextBestGCD(_state, _strategy);
            return MakeResult(aid, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
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

            var gauge = Service.JobGauges.Get<BRDGauge>();
            _state.ActiveSong = (Rotation.Song)gauge.Song;
            _state.ActiveSongLeft = gauge.SongTimer * 0.001f;

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

        private int NumTargetsHitByLadonsbite(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOECone(Player.Position, 12, (primary.Position - Player.Position).Normalized(), 45.Degrees());
        private int NumTargetsHitByRainOfDeath(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOECircle(primary.Position, 8);
    }
}
