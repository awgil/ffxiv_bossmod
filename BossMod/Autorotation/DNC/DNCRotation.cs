// CONTRIB: made by xan, not checked
namespace BossMod.DNC
{
    public static class Rotation
    {
        public class State(float[] cooldowns) : CommonRotation.PlayerState(cooldowns)
        {
            public byte Feathers;
            public bool IsDancing;
            public byte CompletedSteps;
            public uint NextStep;
            public byte Esprit;

            public float StandardStepLeft; // 15s max
            public float StandardFinishLeft; // 60s max
            public float TechStepLeft; // 15s max
            public float TechFinishLeft; // 20s max
            public float FlourishingFinishLeft; // 30s max, granted by tech step
            public float ImprovisationLeft; // 15s max
            public float ImprovisedFinishLeft; // 30s max
            public float DevilmentLeft; // 20s max
            public float SymmetryLeft; // 30s max
            public float FlowLeft; // 30s max
            public float FlourishingStarfallLeft; // 20s max
            public float ThreefoldLeft; // 30s max
            public float FourfoldLeft; // 30s max
            public float PelotonLeft;

            public AID ComboLastMove => (AID)ComboLastAction;

            public AID BestStandardStep
            {
                get
                {
                    if (StandardStepLeft <= GCD)
                        return AID.StandardStep;

                    return CompletedSteps switch
                    {
                        0 => AID.StandardFinish,
                        1 => AID.SingleStandardFinish,
                        _ => AID.DoubleStandardFinish,
                    };
                }
            }

            public AID BestTechStep
            {
                get
                {
                    if (FlourishingFinishLeft > GCD && Unlocked(AID.Tillana))
                        return AID.Tillana;
                    if (TechStepLeft <= GCD)
                        return AID.TechnicalStep;

                    return CompletedSteps switch
                    {
                        0 => AID.TechnicalFinish,
                        1 => AID.SingleTechnicalFinish,
                        2 => AID.DoubleTechnicalFinish,
                        3 => AID.TripleTechnicalFinish,
                        _ => AID.QuadrupleTechnicalFinish
                    };
                }
            }

            public AID BestImprov =>
                ImprovisationLeft > 0 ? AID.ImprovisedFinish : AID.Improvisation;

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                var steps = IsDancing ? CompletedSteps : 0;
                return $"F={Feathers}, E={Esprit}, S={steps}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        public class Strategy : CommonRotation.Strategy
        {
            public bool PauseDuringImprov;
            public bool AutoPartner;

            public int NumDanceTargets; // 15y around self
            public int NumAOETargets; // 5y around self
            public int NumRangedAOETargets; // 5y around target - Saber Dance, Fan3
            public int NumFan4Targets; // 15y/120deg cone
            public int NumStarfallTargets; // 25/4 rect

            public OffensiveAbilityUse FeatherUse;
            public OffensiveAbilityUse GaugeUse;

            public void ApplyStrategyOverrides(uint[] overrides)
            {
                if (overrides.Length >= 2)
                {
                    GaugeUse = (OffensiveAbilityUse)overrides[0];
                    FeatherUse = (OffensiveAbilityUse)overrides[1];
                }
                else
                {
                    GaugeUse = OffensiveAbilityUse.Automatic;
                    FeatherUse = OffensiveAbilityUse.Automatic;
                }
            }

            public override string ToString()
            {
                return $"AOE={NumAOETargets}/Fan3 {NumRangedAOETargets}/Fan4 {NumFan4Targets}/Star {NumStarfallTargets}, Dance={NumDanceTargets}";
            }
        }

        const float FINISH_DANCE_WINDOW = 0.5f;

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (ShouldDoNothing(state, strategy))
                return AID.None;

            if (state.IsDancing)
            {
                if (state.NextStep != 0)
                    return (AID)state.NextStep;

                if (ShouldFinishDance(state.StandardStepLeft, state, strategy))
                    return state.BestStandardStep;
                if (ShouldFinishDance(state.TechStepLeft, state, strategy))
                    return state.BestTechStep;

                return AID.None;
            }

            if (strategy.CombatTimer > -100 && strategy.CombatTimer < 0)
            {
                if (
                    strategy.CombatTimer > -15.5
                    && strategy.CombatTimer < -3.5
                    && !state.IsDancing
                    && state.Unlocked(AID.StandardStep)
                )
                    return AID.StandardStep;

                return AID.None;
            }

            if (
                state.DevilmentLeft > state.GCD
                && state.FlourishingStarfallLeft > state.GCD
                && state.Unlocked(AID.StarfallDance)
                && strategy.NumStarfallTargets > 0
            )
                return AID.StarfallDance;

            if (
                state.FlourishingFinishLeft > state.GCD
                && state.CD(CDGroup.Devilment) > 0
                && strategy.NumDanceTargets > 0
            )
                return AID.Tillana;

            if (
                state.StandardFinishLeft > state.GCD + 5.5
                && state.Unlocked(AID.TechnicalStep)
                && state.CD(CDGroup.TechnicalStep) <= state.GCD
                && strategy.NumDanceTargets > 0
            )
                return AID.TechnicalStep;

            if (
                state.CD(CDGroup.StandardStep) <= state.GCD
                && state.Unlocked(AID.StandardStep)
                && strategy.NumDanceTargets > 0
            )
                return AID.StandardStep;

            if (
                ShouldSpendEsprit(state, strategy)
                && state.Unlocked(AID.SaberDance)
                && strategy.NumRangedAOETargets > 0
            )
                return AID.SaberDance;

            if (state.FlowLeft > state.GCD)
            {
                // bloodshower > fountainfall on 2 targets
                if (strategy.NumAOETargets > 1 && state.Unlocked(AID.Bloodshower))
                    return AID.Bloodshower;

                if (state.Unlocked(AID.Fountainfall) && state.TargetingEnemy)
                    return AID.Fountainfall;
            }

            if (state.SymmetryLeft > state.GCD)
            {
                // rising windmill == reverse cascade on 2 targets
                if (strategy.NumAOETargets > 1 && state.Unlocked(AID.RisingWindmill))
                    return AID.RisingWindmill;

                if (state.Unlocked(AID.ReverseCascade) && state.TargetingEnemy)
                    return AID.ReverseCascade;
            }

            if (
                state.ComboLastMove == AID.Windmill
                && state.Unlocked(AID.Bladeshower)
                // bladeshower (140) is higher potency on 2 targets (280) than cascade (220)
                && strategy.NumAOETargets > 1
            )
                return AID.Bladeshower;

            // windmill is higher potency on 3 targets (100x3) than cascade (220) or fountain (280)
            if (strategy.NumAOETargets > 2 && state.Unlocked(AID.Windmill))
                return AID.Windmill;

            if (!state.TargetingEnemy) return AID.None;

            if (
                state.ComboLastMove == AID.Cascade
                && state.Unlocked(AID.Fountain)
            )
                return AID.Fountain;

            return AID.Cascade;
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            if (ShouldDoNothing(state, strategy))
                return new();

            if (
                strategy.CombatTimer > -10
                && strategy.CombatTimer < -2
                && state.NextStep == 0
                && state.PelotonLeft == 0
                && state.Unlocked(AID.Peloton)
            )
                return ActionID.MakeSpell(AID.Peloton);

            // only permitted OGCDs while dancing are role actions, shield samba, and curing waltz
            if (state.IsDancing)
                return new();

            if (state.RaidBuffsLeft > state.GCD)
            {
                if (
                    state.Unlocked(AID.Devilment)
                    && state.CanWeave(CDGroup.Devilment, 0.6f, deadline)
                )
                    return ActionID.MakeSpell(AID.Devilment);

                if (
                    state.Unlocked(AID.Flourish) && state.CanWeave(CDGroup.Flourish, 0.6f, deadline)
                )
                    return ActionID.MakeSpell(AID.Flourish);

                if (
                    state.FourfoldLeft > state.AnimationLock
                    && state.CanWeave(deadline)
                    && strategy.NumFan4Targets > 0
                )
                    return ActionID.MakeSpell(AID.FanDanceIV);
            }

            if (
                state.CD(CDGroup.Devilment) >= 55
                && state.CanWeave(CDGroup.Flourish, 0.6f, deadline)
            )
                return ActionID.MakeSpell(AID.Flourish);

            if (
                state.CD(CDGroup.Devilment) > 0
                && state.FourfoldLeft > state.AnimationLock
                && state.CanWeave(deadline)
                && strategy.NumFan4Targets > 0
            )
                return ActionID.MakeSpell(AID.FanDanceIV);

            if (
                state.ThreefoldLeft > state.AnimationLock
                && state.CanWeave(deadline)
                && strategy.NumRangedAOETargets > 0
            )
                return ActionID.MakeSpell(AID.FanDanceIII);

            if (ShouldSpendFeathers(state, strategy) && state.CanWeave(deadline))
                return strategy.NumAOETargets > 1 && state.Unlocked(AID.FanDanceII)
                    ? ActionID.MakeSpell(AID.FanDanceII)
                    : ActionID.MakeSpell(AID.FanDance);

            return new();
        }

        private static bool ShouldFinishDance(float danceTimeLeft, State state, Strategy strategy)
        {
            if (state.NextStep != 0)
                return false;
            if (danceTimeLeft > 0 && danceTimeLeft < FINISH_DANCE_WINDOW)
                return true;

            return danceTimeLeft > state.GCD && strategy.NumDanceTargets > 0;
        }

        private static bool ShouldSpendFeathers(State state, Strategy strategy)
        {
            if (state.Feathers == 0 || strategy.FeatherUse == Strategy.OffensiveAbilityUse.Delay)
                return false;

            if (state.Feathers == 4 || strategy.FeatherUse == Strategy.OffensiveAbilityUse.Force)
                return true;

            return state.RaidBuffsLeft > state.AnimationLock;
        }

        private static bool ShouldSpendEsprit(State state, Strategy strategy)
        {
            if (state.Esprit < 50 || strategy.GaugeUse == Strategy.OffensiveAbilityUse.Delay)
                return false;

            if (state.Esprit >= 90 || strategy.GaugeUse == Strategy.OffensiveAbilityUse.Force)
                return true;

            return state.RaidBuffsLeft > state.GCD;
        }

        private static bool ShouldDoNothing(State state, Strategy strategy)
        {
            return strategy.PauseDuringImprov && state.ImprovisationLeft > 0;
        }
    }
}
