using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.WAR;

class Actions : TankActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private bool _aoe;
    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private readonly ConfigListener<WARConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _state = new(autorot.WorldState);
        _strategy = new();

        // upgrades
        SupportedSpell(AID.Defiance).TransformAction = SupportedSpell(AID.ReleaseDefiance).TransformAction = () => ActionID.MakeSpell(_state.HaveTankStance ? AID.ReleaseDefiance : AID.Defiance);
        SupportedSpell(AID.InnerBeast).TransformAction = SupportedSpell(AID.FellCleave).TransformAction = SupportedSpell(AID.InnerChaos).TransformAction = () => ActionID.MakeSpell(_state.BestFellCleave);
        SupportedSpell(AID.SteelCyclone).TransformAction = SupportedSpell(AID.Decimate).TransformAction = SupportedSpell(AID.ChaoticCyclone).TransformAction = () => ActionID.MakeSpell(_state.BestDecimate);
        SupportedSpell(AID.Berserk).TransformAction = SupportedSpell(AID.InnerRelease).TransformAction = () => ActionID.MakeSpell(_state.BestInnerRelease);
        SupportedSpell(AID.RawIntuition).TransformAction = SupportedSpell(AID.Bloodwhetting).TransformAction = () => ActionID.MakeSpell(_state.BestBloodwhetting);

        SupportedSpell(AID.Equilibrium).Condition = _ => Player.HPMP.CurHP < Player.HPMP.MaxHP;
        SupportedSpell(AID.Reprisal).Condition = _ => Autorot.Hints.PotentialTargets.Any(e => e.Actor.Position.InCircle(Player.Position, 5 + e.Actor.HitboxRadius)); // TODO: consider checking only target?..
        SupportedSpell(AID.Interject).Condition = target => target?.CastInfo?.Interruptible ?? false;
        // TODO: SIO - check that raid is in range?..
        // TODO: Provoke - check that not already MT?
        // TODO: Shirk - check that hate is close to MT?..

        _config = Service.Config.GetAndSubscribe<WARConfig>(OnConfigModified);
    }

    protected override void Dispose(bool disposing)
    {
        _config.Dispose();
        base.Dispose(disposing);
    }

    public override CommonRotation.PlayerState GetState() => _state;
    public override CommonRotation.Strategy GetStrategy() => _strategy;

    protected override void UpdateInternalState(int autoAction)
    {
        base.UpdateInternalState(autoAction);
        _aoe = autoAction switch
        {
            AutoActionST => false,
            AutoActionAOE => true, // TODO: consider making AI-like check
            AutoActionAIFight => NumTargetsHitByAOE() >= 3,
            _ => false, // irrelevant...
        };
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
        _strategy.ApplyStrategyOverrides(Autorot.Bossmods.ActiveModule?.PlanExecution?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []);
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.Interject))
        {
            var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 3 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Interject), interruptibleEnemy?.Actor, interruptibleEnemy != null);
        }
        if (_state.Unlocked(AID.Defiance))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Defiance), Player, ShouldSwapStance());
        if (_state.Unlocked(AID.Provoke))
        {
            var provokeEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeTanked && e.PreferProvoking && e.Actor.TargetID != Player.InstanceID && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Provoke), provokeEnemy?.Actor, provokeEnemy != null);
        }
    }

    protected override ActionQueue.Entry CalculateAutomaticGCD()
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return default;
        if (AutoAction == AutoActionAIFight && !Autorot.PrimaryTarget.Position.InCircle(Player.Position, 3 + Autorot.PrimaryTarget.HitboxRadius + Player.HitboxRadius) && _state.Unlocked(AID.Tomahawk))
            return MakeResult(AID.Tomahawk, Autorot.PrimaryTarget); // TODO: reconsider...
        var aid = Rotation.GetNextBestGCD(_state, _strategy, _aoe);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override ActionQueue.Entry CalculateAutomaticOGCD(float deadline)
    {
        if (AutoAction < AutoActionAIFight)
            return default;

        ActionID res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength, _aoe);
        if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline, _aoe);
        return MakeResult(res, Autorot.PrimaryTarget);
    }

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);
        _state.HaveTankStance = Player.FindStatus(SID.Defiance) != null;

        _state.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;

        _state.SurgingTempestLeft = _state.NascentChaosLeft = _state.PrimalRendLeft = _state.InnerReleaseLeft = 0;
        _state.InnerReleaseStacks = 0;
        foreach (var status in Player.Statuses)
        {
            switch ((SID)status.ID)
            {
                case SID.SurgingTempest:
                    _state.SurgingTempestLeft = StatusDuration(status.ExpireAt);
                    break;
                case SID.NascentChaos:
                    _state.NascentChaosLeft = StatusDuration(status.ExpireAt);
                    break;
                case SID.Berserk:
                case SID.InnerRelease:
                    _state.InnerReleaseLeft = StatusDuration(status.ExpireAt);
                    _state.InnerReleaseStacks = status.Extra & 0xFF;
                    break;
                case SID.PrimalRend:
                    _state.PrimalRendLeft = StatusDuration(status.ExpireAt);
                    break;
            }
        }
    }

    private void OnConfigModified(WARConfig config)
    {
        // placeholders
        SupportedSpell(AID.HeavySwing).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.Overpower).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;

        // combo replacement
        SupportedSpell(AID.Maim).TransformAction = config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextMaimComboAction(ComboLastMove)) : null;
        SupportedSpell(AID.StormEye).TransformAction = config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.StormEye)) : null;
        SupportedSpell(AID.StormPath).TransformAction = config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.StormPath)) : null;
        SupportedSpell(AID.MythrilTempest).TransformAction = config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextAOEComboAction(ComboLastMove)) : null;

        // smart targets
        SupportedSpell(AID.NascentFlash).TransformTarget = config.SmartNascentFlashShirkTarget ? SmartTargetCoTank : null;
        SupportedSpell(AID.Shirk).TransformTarget = config.SmartNascentFlashShirkTarget ? SmartTargetCoTank : null;
        SupportedSpell(AID.Provoke).TransformTarget = config.ProvokeMouseover ? SmartTargetHostile : null; // TODO: also interject/low-blow
        SupportedSpell(AID.Holmgang).TransformTarget = config.HolmgangSelf ? _ => Player : null; // TODO: otherwise smarttarget hostile or self...

        SupportedSpell(AID.Tomahawk).Condition = config.ForbidEarlyTomahawk ? _ => _strategy.CombatTimer is float.MinValue or >= -0.7f : null;
        _strategy.OnslaughtHeadroom = config.OnslaughtHeadroom;
    }

    private AID ComboLastMove => (AID)ActionManagerEx.Instance!.ComboLastMove;

    private int NumTargetsHitByAOE() => Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
}
