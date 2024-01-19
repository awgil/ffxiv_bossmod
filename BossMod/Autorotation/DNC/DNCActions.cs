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

        public Actions(Autorotation autorot, Actor player)
            : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _config = Service.Config.Get<DNCConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            SupportedSpell(AID.StandardStep).TransformAction = () =>
                ActionID.MakeSpell(_state.BestStandardStep);
            SupportedSpell(AID.TechnicalStep).TransformAction = () =>
                ActionID.MakeSpell(_state.BestTechStep);
            SupportedSpell(AID.Improvisation).TransformAction = () =>
                ActionID.MakeSpell(_state.BestImprov);

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
            SupportedSpell(AID.Cascade).PlaceholderForAuto = _config.FullRotation
                ? AutoActionST
                : AutoActionNone;
            SupportedSpell(AID.Windmill).PlaceholderForAuto = _config.FullRotation
                ? AutoActionAOE
                : AutoActionNone;

            _strategy.PauseDuringImprov = _config.PauseDuringImprov;
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
                _state.Unlocked(AID.ClosedPosition)
                && StatusDetails(Player, SID.ClosedPosition, Player.InstanceID).Left == 0
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

        protected override void QueueAIActions() { }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionDex);
            _strategy.UseAOERotation = autoAction switch
            {
                AutoActionST => false,
                AutoActionAOE => true,
                AutoActionAIFight => false, // TODO: detect
                _ => false,
            };

            _strategy.NumHostilesInDanceRange = Autorot.Hints.NumPriorityTargetsInAOECircle(
                Player.Position,
                15
            );

            _strategy.ApplyStrategyOverrides(
                Autorot
                    .Bossmods.ActiveModule?.PlanExecution
                    ?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine)
                    ?? new uint[0]
            );
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            var gauge = Service.JobGauges.Get<DNCGauge>();

            _state.Feathers = gauge.Feathers;
            _state.IsDancing = gauge.IsDancing;
            _state.CompletedSteps = gauge.CompletedSteps;
            _state.NextStep =
                (gauge.CompletedSteps == 4 || gauge.NextStep == 15998) ? 0 : gauge.NextStep;
            _state.Esprit = gauge.Esprit;

            _state.StandardStepLeft = StatusLeft(SID.StandardStep);
            _state.StandardFinishLeft = StatusLeft(SID.StandardFinish);
            _state.TechStepLeft = StatusLeft(SID.TechnicalStep);
            _state.TechFinishLeft = StatusLeft(SID.TechnicalFinish);
            _state.FlourishingFinishLeft = StatusLeft(SID.FlourishingFinish);
            _state.ImprovisationLeft = StatusLeft(SID.Improvisation);
            _state.ImprovisedFinishLeft = StatusLeft(SID.ImprovisedFinish);
            _state.DevilmentLeft = StatusLeft(SID.Devilment);
            _state.SymmetryLeft = MathF.Max(
                StatusLeft(SID.SilkenSymmetry),
                StatusLeft(SID.FlourishingSymmetry)
            );
            _state.FlowLeft = MathF.Max(
                StatusLeft(SID.SilkenFlow),
                StatusLeft(SID.FlourishingFlow)
            );
            _state.FlourishingStarfallLeft = StatusLeft(SID.FlourishingStarfall);
            _state.ThreefoldLeft = StatusLeft(SID.ThreefoldFanDance);
            _state.FourfoldLeft = StatusLeft(SID.FourfoldFanDance);
            var pelo = Player.FindStatus((uint)SID.Peloton);
            if (pelo != null)
                _state.PelotonLeft = StatusDuration(pelo.Value.ExpireAt);
            else
                _state.PelotonLeft = 0;
        }

        private float StatusLeft(SID status) =>
            StatusDetails(Player, status, Player.InstanceID).Left;

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
                            Class.SAM => 1.00,
                            Class.NIN => 0.99,
                            Class.MNK => 0.88,
                            Class.RPR => 0.87,
                            Class.DRG => 0.86,
                            Class.BLM => 0.79,
                            Class.SMN => 0.78,
                            Class.RDM => 0.77,
                            Class.MCH => 0.69,
                            Class.BRD => 0.68,
                            Class.DNC => 0.67,
                            _ => 0.01f
                        }
                );
            if (target != null)
            {
                actor = target;
                return true;
            }

            return false;
        }
    }
}
