using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.RPR;

class Actions : CommonActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private bool _aoe;
    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private readonly ConfigListener<RPRConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _state = new(autorot.WorldState);
        _strategy = new();

        // upgrades
        SupportedSpell(AID.BloodStalk).TransformAction = SupportedSpell(AID.UnveiledGallows).TransformAction = SupportedSpell(AID.UnveiledGibbet).TransformAction = () => ActionID.MakeSpell(_state.Beststalk);
        SupportedSpell(AID.Gibbet).TransformAction = SupportedSpell(AID.VoidReaping).TransformAction = () => ActionID.MakeSpell(_state.BestGibbet);
        SupportedSpell(AID.Gallows).TransformAction = SupportedSpell(AID.CrossReaping).TransformAction = () => ActionID.MakeSpell(_state.BestGallow);
        SupportedSpell(AID.SoulSow).TransformAction = SupportedSpell(AID.HarvestMoon).TransformAction = () => ActionID.MakeSpell(_state.BestSow);

        SupportedSpell(AID.LegSweep).Condition = target => target?.CastInfo?.Interruptible ?? false;

        _config = Service.Config.GetAndSubscribe<RPRConfig>(OnConfigModified);
    }

    protected override void Dispose(bool disposing)
    {
        _config.Dispose();
        base.Dispose(disposing);
    }

    public override CommonRotation.PlayerState GetState() => _state;
    public override CommonRotation.Strategy GetStrategy() => _strategy;

    public override Targeting SelectBetterTarget(AIHints.Enemy initial)
    {
        // targeting for multidot
        var adjTarget = initial;
        if (_state.Unlocked(AID.WhorlofDeath) && !WithoutDOT(initial.Actor))
        {
            var multidotTarget = Autorot.Hints.PriorityTargets.FirstOrDefault(e => e != initial && !e.ForbidDOTs && e.Actor.Position.InCircle(Player.Position, 5) && WithoutDOT(e.Actor));
            if (multidotTarget != null)
                adjTarget = multidotTarget;
        }

        var pos = _strategy.NextPositionalImminent ? _strategy.NextPositional : Positional.Any; // TODO: move to common code
        return new(adjTarget, 3, pos);
    }

    protected override void UpdateInternalState(int autoAction)
    {
        _aoe = autoAction switch
        {
            AutoActionST => false,
            AutoActionAOE => true, // TODO: consider making AI-like check
            AutoActionAIFight => NumTargetsHitByAOEGCD() >= 3,
            _ => false, // irrelevant...
        };
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
        _strategy.ApplyStrategyOverrides(Autorot.Bossmods.ActiveModule?.PlanExecution?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []);
        FillStrategyPositionals(_strategy, Rotation.GetNextPositional(_state, _strategy), _state.TrueNorthLeft > _state.GCD);
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.LegSweep))
        {
            var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.LegSweep), interruptibleEnemy?.Actor, interruptibleEnemy != null);
        }
        if (_state.Unlocked(AID.SecondWind))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
        if (_state.Unlocked(AID.Bloodbath))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Bloodbath), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8f);
        // TODO: true north...
    }

    protected override ActionQueue.Entry CalculateAutomaticGCD()
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return default;
        var aid = Rotation.GetNextBestGCD(_state, _strategy, _aoe);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override ActionQueue.Entry CalculateAutomaticOGCD(float deadline)
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
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
        _state.HasSoulsow = Player.FindStatus(SID.Soulsow) != null;

        var gauge = Service.JobGauges.Get<RPRGauge>();
        _state.LemureShroudCount = gauge.LemureShroud;
        _state.VoidShroudCount = gauge.VoidShroud;
        _state.ShroudGauge = gauge.Shroud;
        _state.SoulGauge = gauge.Soul;
        if (_state.ComboLastMove == AID.InfernalSlice)
            _state.ComboTimeLeft = 0;

        _state.SoulReaverLeft = StatusDetails(Player, SID.SoulReaver, Player.InstanceID).Left;
        _state.ImmortalSacrificeLeft = StatusDetails(Player, SID.ImmortalSacrifice, Player.InstanceID).Left;
        _state.ArcaneCircleLeft = StatusDetails(Player, SID.ArcaneCircle, Player.InstanceID).Left;
        _state.EnhancedGibbetLeft = StatusDetails(Player, SID.EnhancedGibbet, Player.InstanceID).Left;
        _state.EnhancedGallowsLeft = StatusDetails(Player, SID.EnhancedGallows, Player.InstanceID).Left;
        _state.EnhancedVoidReapingLeft = StatusDetails(Player, SID.EnhancedVoidReaping, Player.InstanceID).Left;
        _state.EnhancedCrossReapingLeft = StatusDetails(Player, SID.EnhancedCrossReaping, Player.InstanceID).Left;
        _state.EnhancedHarpeLeft = StatusDetails(Player, SID.EnhancedHarpe, Player.InstanceID).Left;
        _state.EnshroudedLeft = StatusDetails(Player, SID.Enshrouded, Player.InstanceID).Left;
        _state.TrueNorthLeft = StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;
        _state.BloodsownCircleLeft = StatusDetails(Player, SID.BloodsownCircle, Player.InstanceID).Left;
        _state.CircleofSacrificeLeft = StatusDetails(Player, SID.CircleofSacrifice, Player.InstanceID).Left;

        _state.TargetDeathDesignLeft = StatusDetails(Autorot.PrimaryTarget, _state.ExpectedShadowofDeath, Player.InstanceID).Left;
    }

    protected override void OnActionSucceeded(ActorCastEvent ev)
    {
        _state.lastActionisSoD = ev.Action.Type == ActionType.Spell && (AID)ev.Action.ID is AID.ShadowofDeath or AID.WhorlofDeath;
    }

    private void OnConfigModified(RPRConfig config)
    {
        // placeholders
        SupportedSpell(AID.Slice).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.SpinningScythe).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;

        SupportedSpell(AID.Harpe).Condition = config.ForbidEarlyHarpe ? _ => _strategy.CombatTimer is float.MinValue or >= -1.7f : null;
    }

    private bool WithoutDOT(Actor a) => Rotation.RefreshDOT(_state, StatusDetails(a, SID.DeathsDesign, Player.InstanceID).Left);
    private int NumTargetsHitByAOEGCD() => Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
}
