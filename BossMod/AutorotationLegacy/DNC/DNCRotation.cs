// CONTRIB: made by xan, not checked
namespace BossMod.DNC;

public static class Rotation
{
    public class State(WorldState ws) : CommonRotation.PlayerState(ws)
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

        public AID BestImprov => ImprovisationLeft > 0 ? AID.ImprovisedFinish : AID.Improvisation;

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);

        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"T={TechFinishLeft:f2}, S={StandardFinishLeft:f2}, C3={SymmetryLeft:f2}, C4={FlowLeft:f2}, Fan3={ThreefoldLeft:f2}, Fan4={FourfoldLeft:f2}, E={Esprit}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
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

        public OffensiveAbilityUse StdStepUse; // default: on cooldown, if there are enemies
        public OffensiveAbilityUse TechStepUse; // default: on cooldown, if there are enemies
        public OffensiveAbilityUse FeatherUse;
        public OffensiveAbilityUse GaugeUse;

        public void ApplyStrategyOverrides(uint[] overrides)
        {
            if (overrides.Length >= 4)
            {
                GaugeUse = (OffensiveAbilityUse)overrides[0];
                FeatherUse = (OffensiveAbilityUse)overrides[1];
                TechStepUse = (OffensiveAbilityUse)overrides[2];
                StdStepUse = (OffensiveAbilityUse)overrides[3];
            }
            else
            {
                GaugeUse = OffensiveAbilityUse.Automatic;
                FeatherUse = OffensiveAbilityUse.Automatic;
                TechStepUse = OffensiveAbilityUse.Automatic;
                StdStepUse = OffensiveAbilityUse.Automatic;
            }
        }

        public override string ToString()
        {
            return $"AOE={NumAOETargets}/Fan3 {NumRangedAOETargets}/Fan4 {NumFan4Targets}/Star {NumStarfallTargets}, Dance={NumDanceTargets}";
        }
    }

    const float FinishDanceWindow = 0.5f;

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

        if (strategy.CombatTimer is > -100 and < 0)
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

        if (ShouldTechStep(state, strategy))
            return AID.TechnicalStep;

        var shouldStdStep = ShouldStdStep(state, strategy);

        // priority for cdplan
        if (strategy.StdStepUse == CommonRotation.Strategy.OffensiveAbilityUse.Force && shouldStdStep)
            return AID.StandardStep;

        var canStarfall = state.FlourishingStarfallLeft > state.GCD && strategy.NumStarfallTargets > 0;
        var canFlow = CanFlow(state, strategy, out var flowCombo);
        var canSymmetry = CanSymmetry(state, strategy, out var symmetryCombo);
        var combo2 = strategy.NumAOETargets > 1 ? AID.Bladeshower : AID.Fountain;
        var haveCombo2 =
            state.Unlocked(combo2)
            && state.ComboLastMove == (strategy.NumAOETargets > 1 ? AID.Windmill : AID.Cascade);

        // prevent starfall expiration
        if (canStarfall && state.FlourishingStarfallLeft <= state.AttackGCDTime)
            return AID.StarfallDance;

        // prevent flow expiration
        if (canFlow && state.FlowLeft <= state.AttackGCDTime)
            return flowCombo;

        // prevent symmetry expiration
        if (canSymmetry && state.SymmetryLeft <= state.AttackGCDTime)
            return symmetryCombo;

        // prevent saber overcap
        if (ShouldSaberDance(state, strategy, 85))
            return AID.SaberDance;

        // starfall dance
        if (canStarfall)
            return AID.StarfallDance;

        // prevent combo2 expiration
        if (haveCombo2 && state.ComboTimeLeft < state.AttackGCDTime * 2)
        {
            // use flow first if we have it so combo2 doesn't overwrite proc
            if (canFlow)
                return flowCombo;

            if (state.ComboTimeLeft < state.AttackGCDTime)
                return combo2;
        }

        // tillana
        if (
            state.FlourishingFinishLeft > state.GCD
            && state.CD(CDGroup.Devilment) > 0
            && strategy.NumDanceTargets > 0
        )
            return AID.Tillana;

        // buffed saber dance
        if (state.TechFinishLeft > state.GCD && ShouldSaberDance(state, strategy, 50))
            return AID.SaberDance;

        // unbuffed standard step - combos 3 and 4 are higher priority in raid buff window
        // skip if tech step is around 5s cooldown or lower since std step would delay it
        if (
            state.TechFinishLeft == 0
            && shouldStdStep
            && (state.CD(CDGroup.TechnicalStep) > state.GCD + 5 || !state.Unlocked(AID.TechnicalStep))
        )
            return AID.StandardStep;

        // combo 3
        if (canFlow)
            return flowCombo;
        // combo 4
        if (canSymmetry)
            return symmetryCombo;

        // (possibly buffed) standard step
        if (shouldStdStep)
            return AID.StandardStep;

        if (haveCombo2)
            return combo2;

        return strategy.NumAOETargets > 1 && state.Unlocked(AID.Windmill)
            ? AID.Windmill
            : state.TargetingEnemy
                ? AID.Cascade
                : AID.None;
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

        if (
            state.TechFinishLeft > state.GCD
            && state.Unlocked(AID.Devilment)
            && state.CanWeave(CDGroup.Devilment, 0.6f, deadline)
        )
            return ActionID.MakeSpell(AID.Devilment);

        if (state.CD(CDGroup.Devilment) > 55 && state.CanWeave(CDGroup.Flourish, 0.6f, deadline))
            return ActionID.MakeSpell(AID.Flourish);

        if (
            (state.TechFinishLeft == 0 || state.CD(CDGroup.Devilment) > 0)
            && state.ThreefoldLeft > state.AnimationLock
            && strategy.NumRangedAOETargets > 0
        )
            return ActionID.MakeSpell(AID.FanDanceIII);

        var canF1 = ShouldSpendFeathers(state, strategy);
        var f1ToUse =
            strategy.NumAOETargets > 1 && state.Unlocked(AID.FanDanceII)
                ? ActionID.MakeSpell(AID.FanDanceII)
                : ActionID.MakeSpell(AID.FanDance);

        if (state.Feathers == 4 && canF1)
            return f1ToUse;

        if (
            state.CD(CDGroup.Devilment) > 0
            && state.FourfoldLeft > state.AnimationLock
            && strategy.NumFan4Targets > 0
        )
            return ActionID.MakeSpell(AID.FanDanceIV);

        if (canF1)
            return f1ToUse;

        return new();
    }

    private static bool ShouldTechStep(State state, Strategy strategy)
    {
        if (
            !state.Unlocked(AID.TechnicalStep)
            || state.CD(CDGroup.TechnicalStep) > state.GCD
            || strategy.TechStepUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay
        )
            return false;

        if (strategy.TechStepUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return true;

        return strategy.NumDanceTargets > 0 && state.StandardFinishLeft > state.GCD + 5.5;
    }

    private static bool ShouldStdStep(State state, Strategy strategy)
    {
        if (
            !state.Unlocked(AID.StandardStep)
            || state.CD(CDGroup.StandardStep) > state.GCD
            || strategy.StdStepUse == CommonRotation.Strategy.OffensiveAbilityUse.Delay
        )
            return false;

        if (strategy.StdStepUse == CommonRotation.Strategy.OffensiveAbilityUse.Force)
            return true;

        // skip if tech finish would expire before we can cast std finish
        // standard step = 1.5s, step = 2x1s -> 3.5s
        return strategy.NumDanceTargets > 0
            && (
                state.TechFinishLeft == 0
                || state.TechFinishLeft > state.GCD + 3.5
                || !state.Unlocked(AID.TechnicalStep)
            );
    }

    private static bool ShouldFinishDance(float danceTimeLeft, State state, Strategy strategy)
    {
        if (state.NextStep != 0)
            return false;
        if (danceTimeLeft is > 0 and < FinishDanceWindow)
            return true;

        return danceTimeLeft > state.GCD && strategy.NumDanceTargets > 0;
    }

    private static bool ShouldSpendFeathers(State state, Strategy strategy)
    {
        if (state.Feathers == 0 || strategy.FeatherUse == Strategy.OffensiveAbilityUse.Delay)
            return false;

        if (state.Feathers == 4 || strategy.FeatherUse == Strategy.OffensiveAbilityUse.Force)
            return true;

        return state.TechFinishLeft > state.AnimationLock;
    }

    private static bool ShouldSaberDance(State state, Strategy strategy, int minimumEsprit)
    {
        if (
            state.Esprit < 50
            || strategy.GaugeUse == Strategy.OffensiveAbilityUse.Delay
            || !state.Unlocked(AID.SaberDance)
        )
            return false;

        if (strategy.GaugeUse == Strategy.OffensiveAbilityUse.Force)
            return true;

        return state.Esprit >= minimumEsprit && strategy.NumRangedAOETargets > 0;
    }

    private static bool CanFlow(State state, Strategy strategy, out AID action)
    {
        var act = strategy.NumAOETargets > 1 ? AID.Bloodshower : AID.Fountainfall;
        if (state.Unlocked(act) && state.FlowLeft > state.GCD && HaveTarget(state, strategy))
        {
            action = act;
            return true;
        }

        action = AID.None;
        return false;
    }

    private static bool CanSymmetry(State state, Strategy strategy, out AID action)
    {
        var act = strategy.NumAOETargets > 1 ? AID.RisingWindmill : AID.ReverseCascade;
        if (state.Unlocked(act) && state.SymmetryLeft > state.GCD && HaveTarget(state, strategy))
        {
            action = act;
            return true;
        }

        action = AID.None;
        return false;
    }

    private static bool HaveTarget(State state, Strategy strategy) => strategy.NumAOETargets > 1 || state.TargetingEnemy;
    private static bool ShouldDoNothing(State state, Strategy strategy) => strategy.PauseDuringImprov && state.ImprovisationLeft > 0;
}
