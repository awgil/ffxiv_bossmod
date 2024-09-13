using BossMod.DNC;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class DNC(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Partner = SharedTrack.Count }
    public enum PartnerStrategy { Automatic, Manual }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan DNC", "Dancer", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.DNC), 100);

        def.DefineShared().AddAssociatedActions(AID.TechnicalStep);

        def.Define(Track.Partner).As<PartnerStrategy>("Partner")
            .AddOption(PartnerStrategy.Automatic, "Auto", "Choose dance partner automatically (based on job aDPS)")
            .AddOption(PartnerStrategy.Manual, "Manual", "Do not choose dance partner automatically");

        return def;
    }

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
    public float LastDanceLeft; // 30s max
    public float FinishingMoveLeft; // 30s max
    public float DanceOfTheDawnLeft; // 30s max

    private Actor? BestFan4Target;
    private Actor? BestRangedAOETarget;
    private Actor? BestStarfallTarget;

    public int NumAOETargets;
    public int NumDanceTargets;

    public int NumFan4Targets;
    public int NumRangedAOETargets;
    public int NumStarfallTargets;

    private const float FinishDanceWindow = 0.5f;

    protected override float GetCastTime(AID aid) => 0;

    private bool HaveTarget(Actor? primaryTarget) => NumAOETargets > 1 || primaryTarget != null;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 25);

        var gauge = World.Client.GetGauge<DancerGauge>();
        var curStep = (uint)gauge.CurrentStep;

        Feathers = gauge.Feathers;
        IsDancing = gauge.DanceSteps[0] != 0;
        CompletedSteps = gauge.StepIndex;
        NextStep = curStep > 0 ? curStep + 15998 : curStep;
        Esprit = gauge.Esprit;

        StandardStepLeft = StatusLeft(SID.StandardStep);
        StandardFinishLeft = StatusLeft(SID.StandardFinish);
        TechStepLeft = StatusLeft(SID.TechnicalStep);
        TechFinishLeft = StatusLeft(SID.TechnicalFinish);
        FlourishingFinishLeft = StatusLeft(SID.FlourishingFinish);
        ImprovisationLeft = StatusLeft(SID.Improvisation);
        ImprovisedFinishLeft = StatusLeft(SID.ImprovisedFinish);
        DevilmentLeft = StatusLeft(SID.Devilment);
        SymmetryLeft = MathF.Max(StatusLeft(SID.SilkenSymmetry), StatusLeft(SID.FlourishingSymmetry));
        FlowLeft = MathF.Max(StatusLeft(SID.SilkenFlow), StatusLeft(SID.FlourishingFlow));
        FlourishingStarfallLeft = StatusLeft(SID.FlourishingStarfall);
        ThreefoldLeft = StatusLeft(SID.ThreefoldFanDance);
        FourfoldLeft = StatusLeft(SID.FourfoldFanDance);
        LastDanceLeft = StatusLeft(SID.LastDanceReady);
        FinishingMoveLeft = StatusLeft(SID.FinishingMoveReady);
        DanceOfTheDawnLeft = StatusLeft(SID.DanceOfTheDawnReady);

        (BestFan4Target, NumFan4Targets) = SelectTarget(strategy, primaryTarget, 15, IsFan4Target);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestStarfallTarget, NumStarfallTargets) = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget);

        NumDanceTargets = NumNearbyTargets(strategy, 15);
        NumAOETargets = NumMeleeAOETargets(strategy);

        if (Unlocked(AID.ClosedPosition)
            && strategy.Option(Track.Partner).As<PartnerStrategy>() == PartnerStrategy.Automatic
            && StatusLeft(SID.ClosedPosition) == 0
            && CD(AID.ClosedPosition) == 0
            && !IsDancing
            && FindDancePartner() is Actor partner)
            PushGCD(AID.ClosedPosition, partner);

        OGCD(strategy, primaryTarget);

        if (IsDancing)
        {
            if (NextStep != 0)
                PushGCD((AID)NextStep, Player);

            if (ShouldFinishDance(StandardStepLeft))
                PushGCD(AID.DoubleStandardFinish, Player);

            if (ShouldFinishDance(TechStepLeft))
                PushGCD(AID.QuadrupleTechnicalFinish, Player);

            return;
        }

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining is > 3.5f and < 15.5f && !IsDancing)
                PushGCD(AID.StandardStep, Player);

            return;
        }

        if (ShouldTechStep(strategy))
            PushGCD(AID.TechnicalStep, Player);

        var shouldStdStep = ShouldStdStep(strategy);

        var canStarfall = FlourishingStarfallLeft > GCD && NumStarfallTargets > 0;
        var canFlow = CanFlow(primaryTarget, out var flowCombo);
        var canSymmetry = CanSymmetry(primaryTarget, out var symmetryCombo);
        var combo2 = NumAOETargets > 1 ? AID.Bladeshower : AID.Fountain;
        var haveCombo2 = Unlocked(combo2) && ComboLastMove == (NumAOETargets > 1 ? AID.Windmill : AID.Cascade);

        if (canStarfall && FlourishingStarfallLeft <= GCDLength)
            PushGCD(AID.StarfallDance, BestStarfallTarget);

        // the targets for these two will be auto fixed if they are AOE actions
        if (canFlow && FlowLeft <= GCDLength)
            PushGCD(flowCombo, primaryTarget);

        if (canSymmetry && SymmetryLeft <= GCDLength)
            PushGCD(symmetryCombo, primaryTarget);

        if (DanceOfTheDawnLeft > GCD && Esprit >= 50)
            PushGCD(AID.DanceOfTheDawn, BestRangedAOETarget);

        if (ShouldSaberDance(strategy, 85))
            PushGCD(AID.SaberDance, BestRangedAOETarget);

        // TODO combine this with above
        if (canStarfall)
            PushGCD(AID.StarfallDance, BestStarfallTarget);

        if (FinishingMoveLeft > GCD && NumDanceTargets > 0)
            PushGCD(AID.FinishingMove, Player);

        if (LastDanceLeft > GCD)
            PushGCD(AID.LastDance, BestRangedAOETarget);

        if (haveCombo2 && !CanFitGCD(World.Client.ComboState.Remaining, 2))
        {
            if (canFlow)
                PushGCD(flowCombo, primaryTarget);

            if (!CanFitGCD(World.Client.ComboState.Remaining, 1))
                PushGCD(combo2, primaryTarget);
        }

        // TODO: this priority is now incorrect
        if (FlourishingFinishLeft > GCD && CD(AID.Devilment) > 0 && NumDanceTargets > 0)
            PushGCD(AID.Tillana, Player);

        if (TechFinishLeft > GCD && ShouldSaberDance(strategy, 50))
            PushGCD(AID.SaberDance, BestRangedAOETarget);

        if (TechFinishLeft == 0 && shouldStdStep && (CD(AID.TechnicalStep) > GCD + 5 || !Unlocked(AID.TechnicalStep)))
            PushGCD(AID.StandardStep, Player);

        if (canFlow)
            PushGCD(flowCombo, primaryTarget);

        if (canSymmetry)
            PushGCD(symmetryCombo, primaryTarget);

        if (shouldStdStep)
            PushGCD(AID.StandardStep, Player);

        if (haveCombo2)
            PushGCD(combo2, primaryTarget);

        if (NumAOETargets > 1 && Unlocked(AID.Windmill))
            PushGCD(AID.Windmill, Player);

        PushGCD(AID.Cascade, primaryTarget);

    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining is > 2 and < 10 && NextStep == 0 && PelotonLeft == 0)
                PushOGCD(AID.Peloton, Player);

            return;
        }

        if (IsDancing)
            return;

        if (TechFinishLeft > GCD)
            PushOGCD(AID.Devilment, Player);

        if (CD(AID.Devilment) > 55)
            PushOGCD(AID.Flourish, Player);

        if ((TechFinishLeft == 0 || CD(AID.Devilment) > 0) && ThreefoldLeft > World.Client.AnimationLock && NumRangedAOETargets > 0)
            PushOGCD(AID.FanDanceIII, BestRangedAOETarget);

        var canF1 = ShouldSpendFeathers(strategy);
        var f1ToUse = NumAOETargets > 1 && Unlocked(AID.FanDanceII) ? AID.FanDanceII : AID.FanDance;

        if (Feathers == 4 && canF1)
            PushOGCD(f1ToUse, primaryTarget);

        if (CD(AID.Devilment) > 0 && FourfoldLeft > World.Client.AnimationLock && NumFan4Targets > 0)
            PushOGCD(AID.FanDanceIV, BestFan4Target);

        if (canF1)
            PushOGCD(f1ToUse, primaryTarget);
    }

    private bool ShouldStdStep(StrategyValues strategy)
    {
        if (!Unlocked(AID.StandardStep) || CD(AID.StandardStep) > GCD)
            return false;

        return NumDanceTargets > 0 &&
            (TechFinishLeft == 0 || TechFinishLeft > GCD + 3.5 || !Unlocked(AID.TechnicalStep));
    }

    private bool ShouldTechStep(StrategyValues strategy)
    {
        if (!strategy.BuffsOk())
            return false;

        const float TechStepDuration = 5.5f;
        const float TechFinishDuration = 20f;

        // standard finish must last for the whole burst window now, since tillana doesn't refresh it
        return NumDanceTargets > 0 && StandardFinishLeft > GCD + TechStepDuration + TechFinishDuration;
    }

    private bool CanFlow(Actor? primaryTarget, out AID action)
    {
        var act = NumAOETargets > 1 ? AID.Bloodshower : AID.Fountainfall;
        if (Unlocked(act) && FlowLeft > GCD && HaveTarget(primaryTarget))
        {
            action = act;
            return true;
        }

        action = AID.None;
        return false;
    }

    private bool CanSymmetry(Actor? primaryTarget, out AID action)
    {
        var act = NumAOETargets > 1 ? AID.RisingWindmill : AID.ReverseCascade;
        if (Unlocked(act) && SymmetryLeft > GCD && HaveTarget(primaryTarget))
        {
            action = act;
            return true;
        }

        action = AID.None;
        return false;
    }

    private bool ShouldFinishDance(float danceTimeLeft)
    {
        if (NextStep != 0)
            return false;
        if (danceTimeLeft is > 0 and < FinishDanceWindow)
            return true;

        return danceTimeLeft > GCD && NumDanceTargets > 0;
    }

    private bool ShouldSaberDance(StrategyValues strategy, int minimumEsprit)
    {
        if (Esprit < 50 || !Unlocked(AID.SaberDance))
            return false;

        return Esprit >= minimumEsprit && NumRangedAOETargets > 0;
    }

    private bool ShouldSpendFeathers(StrategyValues strategy)
    {
        if (Feathers == 0)
            return false;

        if (Feathers == 4 || !Unlocked(AID.TechnicalStep))
            return true;

        return TechFinishLeft > World.Client.AnimationLock;
    }

    private bool IsFan4Target(Actor primary, Actor other) => Hints.TargetInAOECone(other, Player.Position, 15, Player.DirectionTo(primary), 60.Degrees());

    private Actor? FindDancePartner()
    {
        var partner = World.Party.WithoutSlot(partyOnly: true).Exclude(Player).Where(x => Player.DistanceToHitbox(x) <= 30).MaxBy(p => p.Class switch
        {
            Class.SAM => 100,
            Class.NIN or Class.VPR => 99,
            Class.MNK => 88,
            Class.RPR => 87,
            Class.DRG => 86,
            Class.BLM or Class.PCT => 79,
            Class.SMN => 78,
            Class.RDM => 77,
            Class.MCH => 69,
            Class.BRD => 68,
            Class.DNC => 67,
            _ => 1
        });

        if (partner != null)
        {
            // target is in cutscene, we're probably in a raid or something - wait for it to finish
            if (World.Party.Members[World.Party.FindSlot(partner.InstanceID)].InCutscene)
                return null;

            return partner;
        }

        return World.Actors.FirstOrDefault(x => x.Type == ActorType.Chocobo && x.OwnerID == Player.InstanceID);
    }
}
