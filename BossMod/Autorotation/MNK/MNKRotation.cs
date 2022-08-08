namespace BossMod.MNK
{
    public static class Rotation
    {
        public enum Form { None, OpoOpo, Raptor, Coeurl }

        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public int Chakra; // 0-5
            public Form Form;
            public float FormLeft; // 0 if no form, 30 max
            public float DisciplinedFistLeft; // 15 max
            public float TargetDemolishLeft; // TODO: this shouldn't be here...

            public State(float[] cooldowns) : base(cooldowns) { }

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, Chakra={Chakra}, Form={Form}/{FormLeft:f1}, DFist={DisciplinedFistLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public int NumAOETargets;
        }

        public static AID GetOpoOpoAOEAction(State state) => state.Unlocked(MinLevel.ShadowOfTheDestroyer) ? AID.ShadowOfTheDestroyer : AID.ArmOfTheDestroyer;

        public static AID GetOpoOpoFormAction(State state, int numAOETargets)
        {
            // TODO: dragon kick (L50)
            return state.Unlocked(MinLevel.ArmOfTheDestroyer) && numAOETargets >= 3 ? GetOpoOpoAOEAction(state) : AID.Bootshine;
        }

        public static AID GetRaptorFormAction(State state, int numAOETargets)
        {
            // TODO: low level - consider early restart...
            // TODO: better threshold for buff reapplication...
            return state.Unlocked(MinLevel.FourPointFury) && numAOETargets >= 3 ? AID.FourPointFury : state.Unlocked(MinLevel.TwinSnakes) && state.DisciplinedFistLeft < state.GCD + 7 ? AID.TwinSnakes : AID.TrueStrike;
        }

        public static AID GetCoeurlFormAction(State state, int numAOETargets)
        {
            // TODO: multidot support...
            // TODO: low level - consider early restart...
            // TODO: better threshold for debuff reapplication...
            return state.Unlocked(MinLevel.Rockbreaker) && numAOETargets >= 3 ? AID.Rockbreaker : state.Unlocked(MinLevel.Demolish) && state.TargetDemolishLeft < state.GCD + 3 ? AID.Demolish : AID.SnapPunch;
        }

        public static AID GetNextComboAction(State state, int numAOETargets)
        {
            return state.Form switch
            {
                Form.Coeurl => GetCoeurlFormAction(state, numAOETargets),
                Form.Raptor => GetRaptorFormAction(state, numAOETargets),
                _ => GetOpoOpoFormAction(state, numAOETargets)
            };
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            // TODO: L50+
            return GetNextComboAction(state, strategy.NumAOETargets);
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // 1. potion: TODO

            // 2. steel peek, if have chakra
            if (state.Unlocked(MinLevel.SteelPeak) && state.Chakra == 5 && state.CanWeave(CDGroup.SteelPeak, 0.6f, deadline))
                return ActionID.MakeSpell(AID.SteelPeak);

            // no suitable oGCDs...
            return new();
        }
    }
}
