using System.Security.Permissions;
using Dalamud.Game.ClientState.JobGauge.Types;
using static BossMod.CommonRotation.Strategy;

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
            public bool UseAOERotation;
            public int NumHostilesInDanceRange;

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
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (ShouldDoNothing(state, strategy))
                return AID.None;

            if (state.IsDancing && state.NextStep != 0)
                return (AID)state.NextStep;

            if (strategy.CombatTimer < 0)
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

            if (ShouldFinishDance(state.StandardStepLeft, state, strategy))
                return state.BestStandardStep;
            if (ShouldFinishDance(state.TechStepLeft, state, strategy))
                return state.BestTechStep;

            if (!state.TargetingEnemy)
                return AID.None;

            if (
                state.DevilmentLeft > state.GCD
                && state.FlourishingStarfallLeft > state.GCD
                && state.Unlocked(AID.StarfallDance)
            )
                return AID.StarfallDance;

            if (state.FlourishingFinishLeft > state.GCD)
                return AID.Tillana;

            if (
                state.StandardFinishLeft > state.GCD + 5.5
                && state.Unlocked(AID.TechnicalStep)
                && state.CD(CDGroup.TechnicalStep) <= state.GCD
            )
                return AID.TechnicalStep;

            if (state.CD(CDGroup.StandardStep) <= state.GCD && state.Unlocked(AID.StandardStep))
                return AID.StandardStep;

            if (ShouldSpendEsprit(state, strategy) && state.Unlocked(AID.SaberDance))
                return AID.SaberDance;

            if (state.FlowLeft > state.GCD)
            {
                if (strategy.UseAOERotation && state.Unlocked(AID.Bloodshower))
                    return AID.Bloodshower;

                // todo: skip this in aoe mode?
                if (state.Unlocked(AID.Fountainfall))
                    return AID.Fountainfall;
            }

            if (state.SymmetryLeft > state.GCD)
            {
                if (strategy.UseAOERotation && state.Unlocked(AID.RisingWindmill))
                    return AID.RisingWindmill;

                // todo: see above
                if (state.Unlocked(AID.ReverseCascade))
                    return AID.ReverseCascade;
            }

            if (state.ComboLastMove == AID.Windmill && state.Unlocked(AID.Bladeshower))
                return AID.Bladeshower;
            if (state.ComboLastMove == AID.Cascade && state.Unlocked(AID.Fountain))
                return AID.Fountain;

            return (strategy.UseAOERotation && state.Unlocked(AID.Windmill))
                ? AID.Windmill
                : AID.Cascade;
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
            if (state.IsDancing || !state.TargetingEnemy)
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

                if (state.FourfoldLeft > state.AnimationLock && state.CanWeave(deadline))
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
            )
                return ActionID.MakeSpell(AID.FanDanceIV);

            if (state.ThreefoldLeft > state.AnimationLock && state.CanWeave(deadline))
                return ActionID.MakeSpell(AID.FanDanceIII);

            if (ShouldSpendFeathers(state, strategy) && state.CanWeave(deadline))
                return ActionID.MakeSpell(AID.FanDance);

            return new();
        }

        private static bool ShouldFinishDance(float danceTimeLeft, State state, Strategy strategy)
        {
            if (state.NextStep != 0)
                return false;
            if (danceTimeLeft < 0.2)
                return true;

            return danceTimeLeft > state.GCD && strategy.NumHostilesInDanceRange > 0;
        }

        private static bool ShouldSpendFeathers(State state, Strategy strategy)
        {
            if (state.Feathers == 0 || strategy.FeatherUse == OffensiveAbilityUse.Delay)
                return false;

            if (state.Feathers == 4 || strategy.FeatherUse == OffensiveAbilityUse.Force)
                return true;

            return state.RaidBuffsLeft > state.AnimationLock;
        }

        private static bool ShouldSpendEsprit(State state, Strategy strategy)
        {
            if (state.Esprit < 50 || strategy.GaugeUse == OffensiveAbilityUse.Delay)
                return false;

            if (state.Esprit >= 90 || strategy.GaugeUse == OffensiveAbilityUse.Force)
                return true;

            return state.RaidBuffsLeft > state.GCD;
        }

        private static bool ShouldDoNothing(State state, Strategy strategy)
        {
            return strategy.PauseDuringImprov && state.ImprovisationLeft > 0;
        }
    }
}
