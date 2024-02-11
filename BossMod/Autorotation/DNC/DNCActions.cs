using System;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.DNC
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private DNCConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        private bool _predictedTechFinish = false; // TODO: find a way to remove that

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<DNCConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            SupportedSpell(AID.StandardStep).TransformAction = () => ActionID.MakeSpell(_state.BestStandardStep);
            SupportedSpell(AID.TechnicalStep).TransformAction = () => ActionID.MakeSpell(_state.BestTechStep);
            SupportedSpell(AID.Improvisation).TransformAction = () => ActionID.MakeSpell(_state.BestImprov);

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
        }

        public override CommonRotation.PlayerState GetState() => _state;

        public override CommonRotation.Strategy GetStrategy() => _strategy;

        private void OnConfigModified(object? sender, EventArgs args)
        {
            SupportedSpell(AID.Cascade).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.Windmill).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

            _strategy.PauseDuringImprov = _config.PauseDuringImprov;
            _strategy.AutoPartner = _config.AutoPartner;
        }

        protected override NextAction CalculateAutomaticGCD()
        {
            if (AutoAction < AutoActionAIFight)
                return new();

            var aid = Rotation.GetNextBestGCD(_state, _strategy);
            return MakeResult(aid, Autorot.PrimaryTarget);
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            if (AutoAction < AutoActionAIFight)
                return new();

            if (
                _strategy.AutoPartner
                && _state.Unlocked(AID.ClosedPosition)
                && StatusDetails(Player, SID.ClosedPosition, Player.InstanceID).Left == 0
                && _state.CanWeave(CDGroup.Ending, 0.6f, deadline)
                && FindDancePartner(out var partner)
            )
                return MakeResult(ActionID.MakeSpell(AID.ClosedPosition), partner);

            ActionID res = new();
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
            if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
                res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

            return MakeResult(res, Autorot.PrimaryTarget);
        }

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(AID.HeadGraze))
            {
                var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(
                    e =>
                        e.ShouldBeInterrupted
                        && (e.Actor.CastInfo?.Interruptible ?? false)
                        && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius)
                );
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.HeadGraze),
                    interruptibleEnemy?.Actor,
                    interruptibleEnemy != null
                );
            }
            if (_state.Unlocked(AID.Peloton))
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.Peloton),
                    Player,
                    !Player.InCombat && _state.PelotonLeft < 3 && _strategy.ForceMovementIn == 0
                );
            if (_state.Unlocked(AID.CuringWaltz))
            {
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.CuringWaltz),
                    Player,
                    Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.75f
                );
            }
            if (_state.Unlocked(AID.SecondWind))
            {
                SimulateManualActionForAI(
                    ActionID.MakeSpell(AID.SecondWind),
                    Player,
                    Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.5f
                );
            }
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionDex);

            var primaryTarget = Autorot.PrimaryTarget;

            _strategy.NumDanceTargets = Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 15);
            _strategy.NumAOETargets = autoAction == AutoActionST ? 0 : NumAOETargets(Player);
            _strategy.NumRangedAOETargets = primaryTarget == null ? 0 : NumAOETargets(primaryTarget);
            _strategy.NumFan4Targets = primaryTarget == null ? 0 : NumFan4Targets(primaryTarget);
            _strategy.NumStarfallTargets = primaryTarget == null ? 0 : NumStarfallTargets(primaryTarget);

            _strategy.ApplyStrategyOverrides(
                Autorot
                    .Bossmods.ActiveModule?.PlanExecution
                    ?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? new uint[0]
            );
        }

        public override Targeting SelectBetterTarget(AIHints.Enemy initial)
        {
            if (_state.FlourishingStarfallLeft > _state.GCD && _state.Unlocked(AID.StarfallDance))
                return SelectBestTarget(initial, 25, NumStarfallTargets);

            if (_state.CD(CDGroup.Devilment) > 0 && _state.FourfoldLeft > _state.AnimationLock)
                return SelectBestTarget(initial, 15, NumFan4Targets);

            // default for saber dance and fan3
            // TODO: look for enemies we can aoe and move closer?
            return SelectBestTarget(initial, 25, NumAOETargets);
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);
            _state.AnimationLockDelay = MathF.Max(0.1f, _state.AnimationLockDelay);

            var gauge = Service.JobGauges.Get<DNCGauge>();

            _state.Feathers = gauge.Feathers;
            _state.IsDancing = gauge.IsDancing;
            _state.CompletedSteps = gauge.CompletedSteps;
            _state.NextStep = (gauge.CompletedSteps == 4 || gauge.NextStep == 15998) ? 0 : gauge.NextStep;
            _state.Esprit = gauge.Esprit;

            _state.StandardStepLeft = StatusLeft(SID.StandardStep);
            _state.StandardFinishLeft = StatusLeft(SID.StandardFinish);
            _state.TechStepLeft = StatusLeft(SID.TechnicalStep);
            _state.TechFinishLeft = StatusLeft(SID.TechnicalFinish);
            _state.FlourishingFinishLeft = StatusLeft(SID.FlourishingFinish);
            _state.ImprovisationLeft = StatusLeft(SID.Improvisation);
            _state.ImprovisedFinishLeft = StatusLeft(SID.ImprovisedFinish);
            _state.DevilmentLeft = StatusLeft(SID.Devilment);
            _state.SymmetryLeft = MathF.Max(StatusLeft(SID.SilkenSymmetry), StatusLeft(SID.FlourishingSymmetry));
            _state.FlowLeft = MathF.Max(StatusLeft(SID.SilkenFlow), StatusLeft(SID.FlourishingFlow));
            _state.FlourishingStarfallLeft = StatusLeft(SID.FlourishingStarfall);
            _state.ThreefoldLeft = StatusLeft(SID.ThreefoldFanDance);
            _state.FourfoldLeft = StatusLeft(SID.FourfoldFanDance);

            var pelo = Player.FindStatus((uint)SID.Peloton);
            if (pelo != null)
                _state.PelotonLeft = StatusDuration(pelo.Value.ExpireAt);
            else
                _state.PelotonLeft = 0;

            // there seems to be a delay between tech finish use and buff application in full parties - maybe it's a
            // cascading buff that is applied to self last? anyway, the delay can cause the rotation to skip the
            // devilment weave window that occurs right after tech finish since it doesn't think we have tech finish yet
            // TODO: this is not very robust (eg player could die between action and buff application), investigate why StatusDetail doesn't pick it up from pending statuses...
            if (_predictedTechFinish) {
                if (_state.TechFinishLeft == 0)
                    _state.TechFinishLeft = 1000f;
                else
                    _predictedTechFinish = false;
            }
        }

        protected override void OnActionSucceeded(ActorCastEvent ev)
        {
            if (
                ev.Action.ID
                is (uint)AID.TechnicalFinish
                    or (uint)AID.SingleTechnicalFinish
                    or (uint)AID.DoubleTechnicalFinish
                    or (uint)AID.TripleTechnicalFinish
                    or (uint)AID.QuadrupleTechnicalFinish
            )
                _predictedTechFinish = true;
        }

        private Targeting SelectBestTarget(AIHints.Enemy initial, float maxDistanceFromPlayer, Func<Actor, int> prio)
        {
            var newBest = FindBetterTargetBy(initial, maxDistanceFromPlayer, x => prio(x.Actor)).Target;
            return new(newBest, newBest.StayAtLongRange ? 25 : 15);
        }

        private bool FindDancePartner([NotNullWhen(true)] out Actor? actor)
        {
            actor = null;

            var target = Autorot
                .WorldState.Party.WithoutSlot()
                .Exclude(Player)
                .MaxBy(
                    p =>
                        p.Class switch
                        {
                            Class.SAM => 100,
                            Class.NIN => 99,
                            Class.MNK => 88,
                            Class.RPR => 87,
                            Class.DRG => 86,
                            Class.BLM => 79,
                            Class.SMN => 78,
                            Class.RDM => 77,
                            Class.MCH => 69,
                            Class.BRD => 68,
                            Class.DNC => 67,
                            _ => 1
                        }
                );
            if (target != null)
            {
                actor = target;
                return true;
            }

            return false;
        }

        private float StatusLeft(SID status) => StatusDetails(Player, status, Player.InstanceID).Left;

        private int NumAOETargets(Actor origin) => Autorot.Hints.NumPriorityTargetsInAOECircle(origin.Position, 5);

        private int NumFan4Targets(Actor primary) =>
            Autorot.Hints.NumPriorityTargetsInAOECone(
                Player.Position,
                15,
                (primary.Position - Player.Position).Normalized(),
                60.Degrees()
            );

        private int NumStarfallTargets(Actor primary) =>
            Autorot.Hints.NumPriorityTargetsInAOERect(
                Player.Position,
                (primary.Position - Player.Position).Normalized(),
                25,
                4
            );
    }
}
