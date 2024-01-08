using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.DNC
{
    public static class Rotation
    {
        public class State(float[] cooldowns) : CommonRotation.PlayerState(cooldowns)
        {
            public DNCGauge Gauge { private get; set; }

            public byte Feathers => Gauge.Feathers;
            public bool IsDancing => Gauge.IsDancing;
            public byte CompletedSteps => Gauge.CompletedSteps;
            public uint NextStep => CompletedSteps == 4 || Gauge.NextStep == 15998 ? 0 : Gauge.NextStep;
            public float Esprit => Gauge.Esprit;

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

            public AID ComboLastMove => (AID)ComboLastAction;

            public AID BestStandardStep {
                get {
                    if (StandardStepLeft <= GCD) return AID.StandardStep;

                    return CompletedSteps switch {
                        0 => AID.StandardFinish,
                        1 => AID.SingleStandardFinish,
                        _ => AID.DoubleStandardFinish,
                    };
                }
            }

            public AID BestTechStep {
                get {
                    if (FlourishingFinishLeft > GCD && Unlocked(AID.Tillana)) return AID.Tillana;
                    if (TechStepLeft <= GCD) return AID.TechnicalStep;

                    return CompletedSteps switch {
                        0 => AID.TechnicalFinish,
                        1 => AID.SingleTechnicalFinish,
                        2 => AID.DoubleTechnicalFinish,
                        3 => AID.TripleTechnicalFinish,
                        _ => AID.QuadrupleTechnicalFinish
                    };
                }
            } 

            public AID BestImprov => ImprovisationLeft > 0 ? AID.ImprovisedFinish : AID.Improvisation;

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"F={Feathers}, E={Esprit:f3}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            if (DoNothing(state, strategy)) return AID.None;

            if (state.IsDancing) {
                if (state.NextStep != 0)
                    return (AID)state.NextStep;

                if (state.StandardStepLeft > 0 && (strategy.CombatTimer > 0 || state.StandardStepLeft < 0.5))
                    return state.BestStandardStep;

                if (state.TechStepLeft > 0 && (strategy.CombatTimer > 0 || state.TechStepLeft < 0.5))
                    return state.BestTechStep;

                // we can't use non-step GCDs while dancing
                return AID.None;
            }

            if (strategy.CombatTimer > -100 && strategy.CombatTimer < 0) {
                if (strategy.CombatTimer > -15.5 && strategy.CombatTimer < -3.5 && !state.IsDancing)
                    return AID.StandardStep;

                return AID.None;
            }

            if (state.DevilmentLeft > state.GCD && state.FlourishingStarfallLeft > state.GCD && state.Unlocked(AID.StarfallDance))
                return AID.StarfallDance;

            if (state.FlourishingFinishLeft > state.GCD)
                return AID.Tillana;

            if (state.StandardFinishLeft > state.GCD + 5.5
                && state.Unlocked(AID.TechnicalStep)
                && state.CD(CDGroup.TechnicalStep) <= state.GCD)
                return AID.TechnicalStep;

            if (state.CD(CDGroup.StandardStep) <= state.GCD && state.Unlocked(AID.StandardStep))
                return AID.StandardStep;

            if (SpendEsprit(state, strategy) && state.Unlocked(AID.SaberDance))
                return AID.SaberDance;

            if (state.FlowLeft > state.GCD) return AID.Fountainfall;
            if (state.SymmetryLeft > state.GCD) return AID.ReverseCascade;

            if (state.ComboLastMove == AID.Cascade && state.Unlocked(AID.Fountain)) return AID.Fountain;

            return AID.Cascade;
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // only permitted OGCDs while dancing are role actions, shield samba, and curing waltz
            if (DoNothing(state, strategy) || state.IsDancing)
                return new();

            if (state.RaidBuffsLeft > state.GCD) {
                if (state.Unlocked(AID.Devilment) && state.CanWeave(CDGroup.Devilment, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Devilment);

                if (state.Unlocked(AID.Flourish) && state.CanWeave(CDGroup.Flourish, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.Flourish);

                if (state.FourfoldLeft > state.AnimationLock && state.CanWeave(deadline))
                    return ActionID.MakeSpell(AID.FanDanceIV);
            }

            if (state.CD(CDGroup.Devilment) >= 55 && state.CanWeave(CDGroup.Flourish, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Flourish);

            if (state.CD(CDGroup.Devilment) > 0 && state.FourfoldLeft > state.AnimationLock && state.CanWeave(deadline))
                return ActionID.MakeSpell(AID.FanDanceIV);

            if (state.ThreefoldLeft > state.AnimationLock && state.CanWeave(deadline))
                return ActionID.MakeSpell(AID.FanDanceIII);

            if (SpendFeathers(state, strategy) && state.CanWeave(deadline))
                return ActionID.MakeSpell(AID.FanDance);

            return new();
        }

        private static bool SpendFeathers(State state, Strategy strategy)
        {
            // don't overcap
            if (state.Feathers == 4) return true;

            return state.RaidBuffsLeft > state.AnimationLock && state.Feathers > 0;
        }

        private static bool SpendEsprit(State state, Strategy strategy)
        {
            if (state.Esprit >= 90) return true;

            return state.RaidBuffsLeft > state.GCD && state.Esprit >= 50;
        }

        private static bool DoNothing(State state, Strategy strategy)
        {
            return strategy.PauseDuringImprov && state.ImprovisationLeft > 0;
        }

        public class Strategy : CommonRotation.Strategy
        {
            public bool PauseDuringImprov;
            public bool UseAOERotation;
        }
    }
}
