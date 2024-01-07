

using System;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.SAM
{
    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private SAMConfig _config;
        private Rotation.State _state;
        private Rotation.Strategy _strategy;

        public Actions(Autorotation autorot, Actor player) : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            Service.Logger.Debug("{acts}", Definitions.SupportedActions);
            _config = Service.Config.Get<SAMConfig>();
            _state = new(autorot.Cooldowns);
            _strategy = new();

            _config.Modified += OnConfigModified;
            OnConfigModified(null, EventArgs.Empty);
        }

        public override CommonRotation.PlayerState GetState() => _state;
        public override CommonRotation.Strategy GetStrategy() => _strategy;

        public override Targeting SelectBetterTarget(AIHints.Enemy initial)
        {
            // TODO
            // - fuga and ogi namikiri - range 8, angle 120deg
            // - hissatsu: guren - range 10, width (XAxisModifier) 4
            return new(initial);
        }

        private void OnConfigModified(object? sender, EventArgs args)
        {
            SupportedSpell(AID.Hakaze).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
            SupportedSpell(AID.Fuga).PlaceholderForAuto = SupportedSpell(AID.Fuko).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;
            _strategy.KenkiStrategy = _config.UseKenki ?
                (_config.ReserveKenki
                    ? Rotation.Strategy.KenkiSpend.Most
                    : Rotation.Strategy.KenkiSpend.All)
                : Rotation.Strategy.KenkiSpend.Never;
        }

        public override void Dispose()
        {
            _config.Modified -= OnConfigModified;
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

        protected override void QueueAIActions()
        {
            if (_state.Unlocked(AID.SecondWind))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.5f);
            if (_state.Unlocked(AID.Bloodbath))
                SimulateManualActionForAI(ActionID.MakeSpell(AID.Bloodbath), Player, Player.InCombat && Player.HP.Cur < Player.HP.Max * 0.8f);
            // TODO: true north...
        }

        protected override void UpdateInternalState(int autoAction)
        {
            UpdatePlayerState();
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
            _strategy.UseAOERotation = autoAction switch
            {
                AutoActionST => false,
                AutoActionAOE => true,
                AutoActionAIFight => false, // TODO: detect
                _ => false,
            };
            FillStrategyPositionals(_strategy, Rotation.GetNextPositional(_state, _strategy), _state.TrueNorthLeft > _state.GCD);
        }

        private void UpdatePlayerState()
        {
            FillCommonPlayerState(_state);

            var gauge = Service.JobGauges.Get<SAMGauge>();

            _state.HasIceSen = gauge.HasSetsu;
            _state.HasMoonSen = gauge.HasGetsu;
            _state.HasFlowerSen = gauge.HasKa;
            _state.MeditationStacks = gauge.MeditationStacks;
            _state.Kenki = gauge.Kenki;
            _state.Kaeshi = gauge.Kaeshi;
            _state.FukaLeft = StatusDetails(Player, SID.Fuka, Player.InstanceID).Left;
            _state.FugetsuLeft = StatusDetails(Player, SID.Fugetsu, Player.InstanceID).Left;
            _state.TrueNorthLeft = StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;
            _state.MeikyoLeft = StatusDetails(Player, SID.MeikyoShisui, Player.InstanceID).Left;
            _state.OgiNamikiriLeft = StatusDetails(Player, SID.OgiNamikiriReady, Player.InstanceID).Left;
            _state.GCDTime = 2.14f;

            _state.TargetHiganbanaLeft = (_strategy.ForbidDOTs || _strategy.UseAOERotation)
                ? 10000f
                : StatusDetails(Autorot.PrimaryTarget, SID.Higanbana, Player.InstanceID).Left;

            _state.ClosestPositional = GetClosestPositional();
        }

        const float COS45 = 0.70710678118654752f;

        private Positional GetClosestPositional() {
            var tar = Autorot.PrimaryTarget;
            if (tar == null) return Positional.Any;

            return (Player.Position - tar.Position).Normalized().Dot(tar.Rotation.ToDirection()) switch {
                < -COS45 => Positional.Rear,
                < COS45 => Positional.Flank,
                _ => Positional.Front
            };
        }
    }
}