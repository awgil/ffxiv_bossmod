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

            public State(float[] cooldowns) : base(cooldowns) { }

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, Chakra={Chakra}, Form={Form}/{FormLeft:f1}, DFist={DisciplinedFistLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public bool AOE;
        }

        public static AID GetNextAOEComboAction(State state)
        {
            if (!state.Unlocked(MinLevel.Rockbreaker))
                return AID.ArmOfTheDestroyer;

            return state.Form switch
            {
                Form.Coeurl => AID.Rockbreaker,
                Form.Raptor => state.Unlocked(MinLevel.FourPointFury) ? AID.FourPointFury : AID.TwinSnakes,
                _ => state.Unlocked(MinLevel.ShadowOfTheDestroyer) ? AID.ShadowOfTheDestroyer : AID.ArmOfTheDestroyer,
            };
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (strategy.AOE && state.Unlocked(MinLevel.ArmOfTheDestroyer))
            {
                // TODO: this is not right...
                return AID.ArmOfTheDestroyer;
            }
            else
            {
                // TODO: L30+
                return state.Form switch
                {
                    Form.Coeurl => AID.SnapPunch,
                    Form.Raptor => state.Unlocked(MinLevel.TwinSnakes) && state.DisciplinedFistLeft < 7 ? AID.TwinSnakes : AID.TrueStrike, // TODO: better threshold for debuff reapplication
                    _ => AID.Bootshine
                };
            }
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // 2. steel peek, if have chakra
            if (state.Unlocked(MinLevel.SteelPeak) && state.Chakra == 5 && state.CanWeave(CDGroup.SteelPeak, 0.6f, deadline))
                return ActionID.MakeSpell(AID.SteelPeak);

            // no suitable oGCDs...
            return new();
        }
    }
}
