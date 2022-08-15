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

            // upgrade paths
            public AID BestForbiddenChakra => Unlocked(MinLevel.ForbiddenChakra) ? AID.ForbiddenChakra : AID.SteelPeak;
            public AID BestEnlightenment => Unlocked(MinLevel.Enlightenment) ? AID.Enlightenment : AID.HowlingFist;
            public AID BestShadowOfTheDestroyer => Unlocked(MinLevel.ShadowOfTheDestroyer) ? AID.ShadowOfTheDestroyer : AID.ArmOfTheDestroyer;
            public AID BestRisingPhoenix => Unlocked(MinLevel.RisingPhoenix) ? AID.RisingPhoenix : AID.FlintStrike;
            public AID BestPhantomRush => Unlocked(MinLevel.PhantomRush) ? AID.PhantomRush : AID.TornadoKick;

            public State(float[] cooldowns) : base(cooldowns) { }

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, Chakra={Chakra}, Form={Form}/{FormLeft:f1}, DFist={DisciplinedFistLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public int NumPointBlankAOETargets; // range 5 around self
            public int NumEnlightenmentTargets; // range 10 width 2/4 rect
        }

        public static AID GetOpoOpoFormAction(State state, int numAOETargets)
        {
            // TODO: dragon kick (L50)
            return state.Unlocked(MinLevel.ArmOfTheDestroyer) && numAOETargets >= 3 ? state.BestShadowOfTheDestroyer : AID.Bootshine;
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
            return GetNextComboAction(state, strategy.NumPointBlankAOETargets);
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // 1. potion: TODO

            // 2. steel peek, if have chakra
            if (state.Unlocked(MinLevel.SteelPeak) && state.Chakra == 5 && state.CanWeave(CDGroup.SteelPeak, 0.6f, deadline))
            {
                // L15 Steel Peak is 180p
                // L40 Howling Fist is 100p/target => HF at 2+ targets
                // L54 Forbidden Chakra is 340p => HF at 4+ targets
                // L72 Enlightenment is 170p/target => at 2+ targets
                if (state.Unlocked(MinLevel.Enlightenment))
                    return ActionID.MakeSpell(strategy.NumEnlightenmentTargets >= 2 ? AID.Enlightenment : AID.ForbiddenChakra);
                else if (state.Unlocked(MinLevel.ForbiddenChakra))
                    return ActionID.MakeSpell(strategy.NumEnlightenmentTargets >= 4 ? AID.HowlingFist : AID.ForbiddenChakra);
                else if (state.Unlocked(MinLevel.HowlingFist))
                    return ActionID.MakeSpell(strategy.NumEnlightenmentTargets >= 2 ? AID.HowlingFist : AID.SteelPeak);
                else
                    return ActionID.MakeSpell(AID.SteelPeak);
            }

            // no suitable oGCDs...
            return new();
        }
    }
}
