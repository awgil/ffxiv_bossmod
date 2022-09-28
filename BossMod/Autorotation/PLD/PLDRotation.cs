namespace BossMod.PLD
{
    // note: correct up to ~L30
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public float FightOrFlightLeft; // 0 if buff not up, max 25

            public AID ComboLastMove => (AID)ComboLastAction;

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, FF={FightOrFlightLeft:f1}/{CD(CDGroup.FightOrFlight):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public override string ToString()
            {
                return $"";
            }
        }

        public static AID GetNextRiotBladeComboAction(AID comboLastMove)
        {
            return comboLastMove == AID.FastBlade ? AID.RiotBlade : AID.FastBlade;
        }

        public static AID GetNextSTComboAction(AID comboLastMove, AID finisher)
        {
            return comboLastMove switch
            {
                AID.RiotBlade => finisher,
                AID.FastBlade => AID.RiotBlade,
                _ => AID.FastBlade
            };
        }

        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
        {
            if (aoe)
            {
                if (state.Unlocked(AID.Prominence) && state.ComboLastMove == AID.TotalEclipse)
                    return AID.Prominence;
                else
                    return AID.TotalEclipse;
            }
            else
            {
                if (state.Unlocked(AID.RageOfHalone) && state.ComboLastMove == AID.RiotBlade)
                    return AID.RageOfHalone;
                else if (state.Unlocked(AID.RiotBlade) && state.ComboLastMove == AID.FastBlade)
                    return AID.RiotBlade;
                else
                    return AID.FastBlade;
            }
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline, bool aoe)
        {
            // 1. potion - TODO

            // 2. fight or flight, if off gcd and late-weaving, after first combo action
            if (state.Unlocked(AID.FightOrFlight) && state.ComboLastMove == (aoe ? AID.TotalEclipse : AID.FastBlade) && state.CanWeave(CDGroup.FightOrFlight, 0.6f, deadline) && state.GCD <= 1.0f)
                return ActionID.MakeSpell(AID.FightOrFlight);

            // 3. spirits within/circle of scorn, delayed until FoF if it's about to be off cooldown (TODO: think more about delay condition...)
            if (state.Unlocked(AID.SpiritsWithin) && state.CanWeave(CDGroup.SpiritsWithin, 0.6f, deadline) && (state.FightOrFlightLeft > 0 || state.CD(CDGroup.FightOrFlight) > 15))
                return ActionID.MakeSpell(AID.SpiritsWithin);
            if (state.Unlocked(AID.CircleOfScorn) && state.CanWeave(CDGroup.CircleOfScorn, 0.6f, deadline) && (state.FightOrFlightLeft > 0 || state.CD(CDGroup.FightOrFlight) > 15))
                return ActionID.MakeSpell(AID.CircleOfScorn);

            // no suitable oGCDs...
            return new();
        }
    }
}
