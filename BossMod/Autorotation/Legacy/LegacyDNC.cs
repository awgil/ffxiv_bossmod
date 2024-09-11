using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.Legacy;

public sealed class LegacyDNC : LegacyModule
{
    public enum Track { AOE, Gauge, Feather, TechStep, StdStep, PauseDuringImprov, AutoPartner }
    public enum AOEStrategy { SingleTarget, AutoOnPrimary }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum ImprovStrategy { Normal, Pause }
    public enum PartnerStrategy { Automatic, Manual }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense
        var res = new RotationModuleDefinition("Legacy DNC", "Old pre-refactoring module", "xan", RotationModuleQuality.WIP, BitMask.Build((int)Class.DNC), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 90)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AutoOnPrimary, "AutoOnPrimary", "Use aoe actions on primary target if profitable");

        res.Define(Track.Gauge).As<OffensiveStrategy>("Gauge", uiPriority: 80)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Feather).As<OffensiveStrategy>("Feather", uiPriority: 70)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.TechStep).As<OffensiveStrategy>("TechStep", uiPriority: 60)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "On cooldown, if there are enemies")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.StdStep).As<OffensiveStrategy>("StdStep", uiPriority: 50)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "On cooldown, if there are enemies")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.PauseDuringImprov).As<ImprovStrategy>("PauseDuringImprov", uiPriority: 40)
            .AddOption(ImprovStrategy.Normal, "Normal")
            .AddOption(ImprovStrategy.Pause, "Pause", "Pause autorotation while Improvisation is active");

        res.Define(Track.AutoPartner).As<PartnerStrategy>("AutoPartner", uiPriority: 30)
            .AddOption(PartnerStrategy.Automatic, "Automatic", "Automatically choose dance partner")
            .AddOption(PartnerStrategy.Manual, "Manual");

        return res;
    }

    // full state needed for determining next action
    public class State(RotationModule module) : CommonState(module)
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

        public bool PauseDuringImprov;
        public bool AutoPartner;

        public int NumDanceTargets; // 15y around self
        public int NumAOETargets; // 5y around self
        public int NumRangedAOETargets; // 5y around target - Saber Dance, Fan3
        public int NumFan4Targets; // 15y/120deg cone
        public int NumStarfallTargets; // 25/4 rect

        public DNC.AID ComboLastMove => (DNC.AID)ComboLastAction;

        public DNC.AID BestStandardStep
        {
            get
            {
                if (StandardStepLeft <= GCD)
                    return DNC.AID.StandardStep;

                return CompletedSteps switch
                {
                    0 => DNC.AID.StandardFinish,
                    1 => DNC.AID.SingleStandardFinish,
                    _ => DNC.AID.DoubleStandardFinish,
                };
            }
        }

        public DNC.AID BestTechStep
        {
            get
            {
                if (FlourishingFinishLeft > GCD && Unlocked(DNC.AID.Tillana))
                    return DNC.AID.Tillana;
                if (TechStepLeft <= GCD)
                    return DNC.AID.TechnicalStep;

                return CompletedSteps switch
                {
                    0 => DNC.AID.TechnicalFinish,
                    1 => DNC.AID.SingleTechnicalFinish,
                    2 => DNC.AID.DoubleTechnicalFinish,
                    3 => DNC.AID.TripleTechnicalFinish,
                    _ => DNC.AID.QuadrupleTechnicalFinish
                };
            }
        }

        public DNC.AID BestImprov => ImprovisationLeft > 0 ? DNC.AID.ImprovisedFinish : DNC.AID.Improvisation;

        public bool Unlocked(DNC.AID aid) => Module.ActionUnlocked(ActionID.MakeSpell(aid));
        public bool Unlocked(DNC.TraitID tid) => Module.TraitUnlocked((uint)tid);

        public override string ToString()
        {
            return $"AOE={NumAOETargets}/Fan3 {NumRangedAOETargets}/Fan4 {NumFan4Targets}/Star {NumStarfallTargets}, Dance={NumDanceTargets}, T={TechFinishLeft:f2}, S={StandardFinishLeft:f2}, C3={SymmetryLeft:f2}, C4={FlowLeft:f2}, Fan3={ThreefoldLeft:f2}, Fan4={FourfoldLeft:f2}, E={Esprit}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
        }
    }

    private readonly State _state;
    private bool _predictedTechFinish; // TODO: find a way to remove that

    private const float FinishDanceWindow = 0.5f;

    public LegacyDNC(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);
        _state.AnimationLockDelay = MathF.Max(0.1f, _state.AnimationLockDelay);

        var gauge = World.Client.GetGauge<DancerGauge>();
        var curStep = (uint)gauge.CurrentStep;

        _state.Feathers = gauge.Feathers;
        _state.IsDancing = gauge.DanceSteps[0] != 0;
        _state.CompletedSteps = gauge.StepIndex;
        _state.NextStep = curStep > 0 ? curStep + 15998 : curStep;
        _state.Esprit = gauge.Esprit;

        _state.StandardStepLeft = StatusLeft(DNC.SID.StandardStep);
        _state.StandardFinishLeft = StatusLeft(DNC.SID.StandardFinish);
        _state.TechStepLeft = StatusLeft(DNC.SID.TechnicalStep);
        _state.TechFinishLeft = StatusLeft(DNC.SID.TechnicalFinish);
        _state.FlourishingFinishLeft = StatusLeft(DNC.SID.FlourishingFinish);
        _state.ImprovisationLeft = StatusLeft(DNC.SID.Improvisation);
        _state.ImprovisedFinishLeft = StatusLeft(DNC.SID.ImprovisedFinish);
        _state.DevilmentLeft = StatusLeft(DNC.SID.Devilment);
        _state.SymmetryLeft = MathF.Max(StatusLeft(DNC.SID.SilkenSymmetry), StatusLeft(DNC.SID.FlourishingSymmetry));
        _state.FlowLeft = MathF.Max(StatusLeft(DNC.SID.SilkenFlow), StatusLeft(DNC.SID.FlourishingFlow));
        _state.FlourishingStarfallLeft = StatusLeft(DNC.SID.FlourishingStarfall);
        _state.ThreefoldLeft = StatusLeft(DNC.SID.ThreefoldFanDance);
        _state.FourfoldLeft = StatusLeft(DNC.SID.FourfoldFanDance);

        var pelo = Player.FindStatus((uint)DNC.SID.Peloton);
        if (pelo != null)
            _state.PelotonLeft = _state.StatusDuration(pelo.Value.ExpireAt);
        else
            _state.PelotonLeft = 0;

        // there seems to be a delay between tech finish use and buff application in full parties - maybe it's a
        // cascading buff that is applied to self last? anyway, the delay can cause the rotation to skip the
        // devilment weave window that occurs right after tech finish since it doesn't think we have tech finish yet
        // TODO: this is not very robust (eg player could die between action and buff application), investigate why StatusDetail doesn't pick it up from pending statuses...
        if (_predictedTechFinish)
        {
            if (_state.TechFinishLeft == 0)
                _state.TechFinishLeft = 1000f;
            else
                _predictedTechFinish = false;
        }

        _state.PauseDuringImprov = strategy.Option(Track.PauseDuringImprov).As<ImprovStrategy>() == ImprovStrategy.Pause;
        _state.AutoPartner = strategy.Option(Track.AutoPartner).As<PartnerStrategy>() == PartnerStrategy.Automatic;

        // TODO: aoe targeting; see how BRD/DRG do aoes
        //if (_state.FlourishingStarfallLeft > _state.GCD && _state.Unlocked(AID.StarfallDance))
        //    return SelectBestTarget(initial, 25, NumStarfallTargets);
        //if (_state.CD(DNC.AID.Devilment) > 0 && _state.FourfoldLeft > _state.AnimationLock)
        //    return SelectBestTarget(initial, 15, NumFan4Targets);
        //// default for saber dance and fan3
        //// TODO: look for enemies we can aoe and move closer?
        //return SelectBestTarget(initial, 25, NumAOETargets); _state.NumDanceTargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 15);
        var aoeStrategy = strategy.Option(Track.AOE).As<AOEStrategy>();
        _state.NumAOETargets = aoeStrategy == AOEStrategy.SingleTarget ? 0 : NumAOETargets(Player);
        _state.NumRangedAOETargets = primaryTarget == null ? 0 : NumAOETargets(primaryTarget);
        _state.NumFan4Targets = primaryTarget == null ? 0 : NumFan4Targets(primaryTarget);
        _state.NumStarfallTargets = primaryTarget == null ? 0 : NumStarfallTargets(primaryTarget);

        // TODO: refactor all that, it's kinda senseless now
        DNC.AID gcd = GetNextBestGCD(strategy);
        PushResult(gcd, primaryTarget);

        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        if (_state.AutoPartner && _state.Unlocked(DNC.AID.ClosedPosition) && StatusLeft(DNC.SID.ClosedPosition) == 0 && _state.CanWeave(DNC.AID.ClosedPosition, 0.6f, deadline) && FindDancePartner() is var partner && partner != null)
        {
            PushResult(ActionID.MakeSpell(DNC.AID.ClosedPosition), partner);
        }
        else
        {
            ActionID ogcd = default;
            if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
                ogcd = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength);
            if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
                ogcd = GetNextBestOGCD(strategy, deadline);
            PushResult(ogcd, primaryTarget);
        }
    }

    //protected override void QueueAIActions()
    //{
    //    if (_state.Unlocked(DNC.AID.HeadGraze))
    //    {
    //        var interruptibleEnemy = Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
    //        SimulateManualActionForAI(ActionID.MakeSpell(DNC.AID.HeadGraze), interruptibleEnemy?.Actor, interruptibleEnemy != null);
    //    }
    //    if (_state.Unlocked(DNC.AID.Peloton))
    //        SimulateManualActionForAI(ActionID.MakeSpell(DNC.AID.Peloton), Player, !Player.InCombat && _state.PelotonLeft < 3 && _strategy.ForceMovementIn == 0);
    //    if (_state.Unlocked(DNC.AID.CuringWaltz))
    //    {
    //        SimulateManualActionForAI(ActionID.MakeSpell(DNC.AID.CuringWaltz), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.75f);
    //    }
    //    if (_state.Unlocked(DNC.AID.SecondWind))
    //    {
    //        SimulateManualActionForAI(ActionID.MakeSpell(DNC.AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
    //    }
    //}

    // TODO: this won't work reliably, since modules can be destroyed and recreated at any time
    //protected override void OnActionSucceeded(ActorCastEvent ev)
    //{
    //    if ((DNC.AID)ev.Action.ID is DNC.AID.TechnicalFinish or DNC.AID.SingleTechnicalFinish or DNC.AID.DoubleTechnicalFinish or DNC.AID.TripleTechnicalFinish or DNC.AID.QuadrupleTechnicalFinish)
    //        _predictedTechFinish = true;
    //}

    public override string DescribeState() => _state.ToString();

    //private Targeting SelectBestTarget(AIHints.Enemy initial, float maxDistanceFromPlayer, Func<Actor, int> prio)
    //{
    //    var newBest = FindBetterTargetBy(initial, maxDistanceFromPlayer, x => prio(x.Actor)).Target;
    //    return new(newBest, newBest.StayAtLongRange ? 25 : 15);
    //}

    private float StatusLeft(DNC.SID status) => _state.StatusDetails(Player, status, Player.InstanceID).Left;

    private Actor? FindDancePartner() => World.Party.WithoutSlot().Exclude(Player).MaxBy(p => p.Class switch
    {
        Class.SAM => 100,
        Class.NIN => 99,
        Class.MNK => 88,
        Class.RPR => 87,
        Class.DRG => 86,
        Class.BLM => 79,
        Class.SMN => 78,
        Class.RDM => 77,
        Class.MCH => 69,
        Class.BRD => 68,
        Class.DNC => 67,
        _ => 1
    });

    private int NumAOETargets(Actor origin) => Hints.NumPriorityTargetsInAOECircle(origin.Position, 5);
    private int NumFan4Targets(Actor primary) => Hints.NumPriorityTargetsInAOECone(Player.Position, 15, (primary.Position - Player.Position).Normalized(), 60.Degrees());
    private int NumStarfallTargets(Actor primary) => Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 25, 4);

    // old DNCRotation
    private DNC.AID GetNextBestGCD(StrategyValues strategy)
    {
        if (ShouldDoNothing())
            return DNC.AID.None;

        if (_state.IsDancing)
        {
            if (_state.NextStep != 0)
                return (DNC.AID)_state.NextStep;

            if (ShouldFinishDance(_state.StandardStepLeft))
                return _state.BestStandardStep;
            if (ShouldFinishDance(_state.TechStepLeft))
                return _state.BestTechStep;

            return DNC.AID.None;
        }

        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining is < 15.5f and > 3.5f && !_state.IsDancing && _state.Unlocked(DNC.AID.StandardStep))
                return DNC.AID.StandardStep;

            return DNC.AID.None;
        }

        if (ShouldTechStep(strategy))
            return DNC.AID.TechnicalStep;

        var shouldStdStep = ShouldStdStep(strategy);

        // priority for cdplan
        if (strategy.Option(Track.StdStep).As<OffensiveStrategy>() == OffensiveStrategy.Force && shouldStdStep)
            return DNC.AID.StandardStep;

        var canStarfall = _state.FlourishingStarfallLeft > _state.GCD && _state.NumStarfallTargets > 0;
        var canFlow = CanFlow(out var flowCombo);
        var canSymmetry = CanSymmetry(out var symmetryCombo);
        var combo2 = _state.NumAOETargets > 1 ? DNC.AID.Bladeshower : DNC.AID.Fountain;
        var haveCombo2 = _state.Unlocked(combo2) && _state.ComboLastMove == (_state.NumAOETargets > 1 ? DNC.AID.Windmill : DNC.AID.Cascade);

        // prevent starfall expiration
        if (canStarfall && _state.FlourishingStarfallLeft <= _state.AttackGCDTime)
            return DNC.AID.StarfallDance;

        // prevent flow expiration
        if (canFlow && _state.FlowLeft <= _state.AttackGCDTime)
            return flowCombo;

        // prevent symmetry expiration
        if (canSymmetry && _state.SymmetryLeft <= _state.AttackGCDTime)
            return symmetryCombo;

        // prevent saber overcap
        if (ShouldSaberDance(strategy, 85))
            return DNC.AID.SaberDance;

        // starfall dance
        if (canStarfall)
            return DNC.AID.StarfallDance;

        // prevent combo2 expiration
        if (haveCombo2 && _state.ComboTimeLeft < _state.AttackGCDTime * 2)
        {
            // use flow first if we have it so combo2 doesn't overwrite proc
            if (canFlow)
                return flowCombo;

            if (_state.ComboTimeLeft < _state.AttackGCDTime)
                return combo2;
        }

        // tillana
        if (_state.FlourishingFinishLeft > _state.GCD && _state.CD(DNC.AID.Devilment) > 0 && _state.NumDanceTargets > 0)
            return DNC.AID.Tillana;

        // buffed saber dance
        if (_state.TechFinishLeft > _state.GCD && ShouldSaberDance(strategy, 50))
            return DNC.AID.SaberDance;

        // unbuffed standard step - combos 3 and 4 are higher priority in raid buff window
        // skip if tech step is around 5s cooldown or lower since std step would delay it
        if (_state.TechFinishLeft == 0 && shouldStdStep && (_state.CD(DNC.AID.TechnicalStep) > _state.GCD + 5 || !_state.Unlocked(DNC.AID.TechnicalStep)))
            return DNC.AID.StandardStep;

        // combo 3
        if (canFlow)
            return flowCombo;
        // combo 4
        if (canSymmetry)
            return symmetryCombo;

        // (possibly buffed) standard step
        if (shouldStdStep)
            return DNC.AID.StandardStep;

        if (haveCombo2)
            return combo2;

        return _state.NumAOETargets > 1 && _state.Unlocked(DNC.AID.Windmill) ? DNC.AID.Windmill
            : _state.TargetingEnemy ? DNC.AID.Cascade
            : DNC.AID.None;
    }

    private ActionID GetNextBestOGCD(StrategyValues strategy, float deadline)
    {
        if (ShouldDoNothing())
            return new();

        if (_state.CountdownRemaining is < 10 and > 2 && _state.NextStep == 0 && _state.PelotonLeft == 0 && _state.Unlocked(DNC.AID.Peloton))
            return ActionID.MakeSpell(DNC.AID.Peloton);

        // only permitted OGCDs while dancing are role actions, shield samba, and curing waltz
        if (_state.IsDancing)
            return new();

        if (_state.TechFinishLeft > _state.GCD && _state.Unlocked(DNC.AID.Devilment) && _state.CanWeave(DNC.AID.Devilment, 0.6f, deadline))
            return ActionID.MakeSpell(DNC.AID.Devilment);

        if (_state.CD(DNC.AID.Devilment) > 55 && _state.CanWeave(DNC.AID.Flourish, 0.6f, deadline))
            return ActionID.MakeSpell(DNC.AID.Flourish);

        if ((_state.TechFinishLeft == 0 || _state.CD(DNC.AID.Devilment) > 0) && _state.ThreefoldLeft > _state.AnimationLock && _state.NumRangedAOETargets > 0)
            return ActionID.MakeSpell(DNC.AID.FanDanceIII);

        var canF1 = ShouldSpendFeathers(strategy);
        var f1ToUse = _state.NumAOETargets > 1 && _state.Unlocked(DNC.AID.FanDanceII) ? ActionID.MakeSpell(DNC.AID.FanDanceII) : ActionID.MakeSpell(DNC.AID.FanDance);

        if (_state.Feathers == 4 && canF1)
            return f1ToUse;

        if (_state.CD(DNC.AID.Devilment) > 0 && _state.FourfoldLeft > _state.AnimationLock && _state.NumFan4Targets > 0)
            return ActionID.MakeSpell(DNC.AID.FanDanceIV);

        if (canF1)
            return f1ToUse;

        return new();
    }

    private bool ShouldTechStep(StrategyValues strategy)
    {
        var techStepStrategy = strategy.Option(Track.TechStep).As<OffensiveStrategy>();
        if (!_state.Unlocked(DNC.AID.TechnicalStep) || _state.CD(DNC.AID.TechnicalStep) > _state.GCD || techStepStrategy == OffensiveStrategy.Delay)
            return false;

        if (techStepStrategy == OffensiveStrategy.Force)
            return true;

        return _state.NumDanceTargets > 0 && _state.StandardFinishLeft > _state.GCD + 5.5;
    }

    private bool ShouldStdStep(StrategyValues strategy)
    {
        var stdStepStrategy = strategy.Option(Track.StdStep).As<OffensiveStrategy>();
        if (!_state.Unlocked(DNC.AID.StandardStep) || _state.CD(DNC.AID.StandardStep) > _state.GCD || stdStepStrategy == OffensiveStrategy.Delay)
            return false;

        if (stdStepStrategy == OffensiveStrategy.Force)
            return true;

        // skip if tech finish would expire before we can cast std finish
        // standard step = 1.5s, step = 2x1s -> 3.5s
        return _state.NumDanceTargets > 0 && (_state.TechFinishLeft == 0 || _state.TechFinishLeft > _state.GCD + 3.5 || !_state.Unlocked(DNC.AID.TechnicalStep));
    }

    private bool ShouldFinishDance(float danceTimeLeft)
    {
        if (_state.NextStep != 0)
            return false;
        if (danceTimeLeft is > 0 and < FinishDanceWindow)
            return true;

        return danceTimeLeft > _state.GCD && _state.NumDanceTargets > 0;
    }

    private bool ShouldSpendFeathers(StrategyValues strategy)
    {
        var featherStrategy = strategy.Option(Track.Feather).As<OffensiveStrategy>();
        if (_state.Feathers == 0 || featherStrategy == OffensiveStrategy.Delay)
            return false;

        if (_state.Feathers == 4 || featherStrategy == OffensiveStrategy.Force)
            return true;

        return _state.TechFinishLeft > _state.AnimationLock;
    }

    private bool ShouldSaberDance(StrategyValues strategy, int minimumEsprit)
    {
        var gaugeStrategy = strategy.Option(Track.Gauge).As<OffensiveStrategy>();
        if (_state.Esprit < 50 || gaugeStrategy == OffensiveStrategy.Delay || !_state.Unlocked(DNC.AID.SaberDance))
            return false;

        if (gaugeStrategy == OffensiveStrategy.Force)
            return true;

        return _state.Esprit >= minimumEsprit && _state.NumRangedAOETargets > 0;
    }

    private bool CanFlow(out DNC.AID action)
    {
        var act = _state.NumAOETargets > 1 ? DNC.AID.Bloodshower : DNC.AID.Fountainfall;
        if (_state.Unlocked(act) && _state.FlowLeft > _state.GCD && HaveTarget())
        {
            action = act;
            return true;
        }

        action = DNC.AID.None;
        return false;
    }

    private bool CanSymmetry(out DNC.AID action)
    {
        var act = _state.NumAOETargets > 1 ? DNC.AID.RisingWindmill : DNC.AID.ReverseCascade;
        if (_state.Unlocked(act) && _state.SymmetryLeft > _state.GCD && HaveTarget())
        {
            action = act;
            return true;
        }

        action = DNC.AID.None;
        return false;
    }

    private bool HaveTarget() => _state.NumAOETargets > 1 || _state.TargetingEnemy;
    private bool ShouldDoNothing() => _state.PauseDuringImprov && _state.ImprovisationLeft > 0;
}
