namespace BossMod.DNC
{

    public class State(float[] cooldowns): CommonRotation.PlayerState(cooldowns) {}

    public class Strategy : CommonRotation.Strategy {}

    class Actions : CommonActions
    {
        public const int AutoActionST = AutoActionFirstCustom + 0;
        public const int AutoActionAOE = AutoActionFirstCustom + 1;

        private State _state;
        private Strategy _strategy;

        public Actions(Autorotation autorot, Actor player) : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
        {
            _state = new(autorot.Cooldowns);
            _strategy = new();
        }

        public override void Dispose() {}

        public override CommonRotation.PlayerState GetState() => _state;
        public override CommonRotation.Strategy GetStrategy() => _strategy;

        protected override NextAction CalculateAutomaticGCD()
        {
            return default;
        }

        protected override NextAction CalculateAutomaticOGCD(float deadline)
        {
            return default;
        }

        protected override void QueueAIActions()
        {
        }

        protected override void UpdateInternalState(int autoAction)
        {
            FillCommonStrategy(_strategy, CommonDefinitions.IDPotionDex);
            FillCommonPlayerState(_state);
        }
    }
}
