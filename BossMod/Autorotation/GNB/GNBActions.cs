using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.GNB;

class Actions : TankActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private GNBConfig _config;
    private bool _aoe;
    private Rotation.State _state;
    private Rotation.Strategy _strategy;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _config = Service.Config.Get<GNBConfig>();
        _state = new(autorot.WorldState);
        _strategy = new();

        // upgrades
        SupportedSpell(AID.RoyalGuard).TransformAction = SupportedSpell(AID.ReleaseRoyalGuard).TransformAction = () => ActionID.MakeSpell(_state.HaveTankStance ? AID.ReleaseRoyalGuard : AID.RoyalGuard);
        SupportedSpell(AID.DangerZone).TransformAction = SupportedSpell(AID.BlastingZone).TransformAction = () => ActionID.MakeSpell(_state.BestZone);
        SupportedSpell(AID.HeartOfStone).TransformAction = SupportedSpell(AID.HeartOfCorundum).TransformAction = () => ActionID.MakeSpell(_state.BestHeart);
        SupportedSpell(AID.Continuation).TransformAction = SupportedSpell(AID.JugularRip).TransformAction = SupportedSpell(AID.AbdomenTear).TransformAction = SupportedSpell(AID.EyeGouge).TransformAction = SupportedSpell(AID.Hypervelocity).TransformAction = () => ActionID.MakeSpell(_state.BestContinuation);
        SupportedSpell(AID.GnashingFang).TransformAction = SupportedSpell(AID.SavageClaw).TransformAction = SupportedSpell(AID.WickedTalon).TransformAction = () => ActionID.MakeSpell(_state.BestGnash);
        SupportedSpell(AID.Continuation).Condition = _ => _state.ReadyToRip ? ActionID.MakeSpell(AID.JugularRip) : ActionID.MakeSpell(AID.None);
        SupportedSpell(AID.Continuation).Condition = _ => _state.ReadyToTear ? ActionID.MakeSpell(AID.AbdomenTear) : ActionID.MakeSpell(AID.None);
        SupportedSpell(AID.Continuation).Condition = _ => _state.ReadyToGouge ? ActionID.MakeSpell(AID.EyeGouge) : ActionID.MakeSpell(AID.None);
        SupportedSpell(AID.Continuation).Condition = _ => _state.ReadyToBlast ? ActionID.MakeSpell(AID.Hypervelocity) : ActionID.MakeSpell(AID.None);

        SupportedSpell(AID.Aurora).Condition = _ => Player.HP.Cur < Player.HP.Max;
        SupportedSpell(AID.Reprisal).Condition = _ => Autorot.Hints.PotentialTargets.Any(e => e.Actor.Position.InCircle(Player.Position, 5 + e.Actor.HitboxRadius)); // TODO: consider checking only target?..
        SupportedSpell(AID.Interject).Condition = target => target?.CastInfo?.Interruptible ?? false;
        SupportedSpell(AID.LightningShot).Condition = _ => !_config.ForbidEarlyLightningShot || _strategy.CombatTimer == float.MinValue || _strategy.CombatTimer >= -0.7f;
        // TODO: SIO - check that raid is in range?..
        // TODO: Provoke - check that not already MT?
        // TODO: Shirk - check that hate is close to MT?..

        _config.Modified += OnConfigModified;
        OnConfigModified();
    }

    public override void Dispose()
    {
        _config.Modified -= OnConfigModified;
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
        _strategy.ApplyStrategyOverrides(Autorot.Bossmods.ActiveModule?.PlanExecution?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? new uint[0]);
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.Interject))
        {
            var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 3 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Interject), interruptibleEnemy?.Actor, interruptibleEnemy != null);
        }
        if (_state.Unlocked(AID.RoyalGuard))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.RoyalGuard), Player, ShouldSwapStance());
        if (_state.Unlocked(AID.Provoke))
        {
            var provokeEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeTanked && e.PreferProvoking && e.Actor.TargetID != Player.InstanceID && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Provoke), provokeEnemy?.Actor, provokeEnemy != null);
        }
    }

    protected override NextAction CalculateAutomaticGCD()
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return new();
        if (AutoAction == AutoActionAIFight && !Autorot.PrimaryTarget.Position.InCircle(Player.Position, 3 + Autorot.PrimaryTarget.HitboxRadius + Player.HitboxRadius) && _state.Unlocked(AID.LightningShot))
            return MakeResult(AID.LightningShot, Autorot.PrimaryTarget); // TODO: reconsider...
        var aid = Rotation.GetNextBestGCD(_state, _strategy, _aoe);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override NextAction CalculateAutomaticOGCD(float deadline)
    {
        if (AutoAction < AutoActionAIFight)
            return new();

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
        _state.HaveTankStance = Player.FindStatus(SID.RoyalGuard) != null;
        if (_state.ComboLastMove == AID.SolidBarrel)
            _state.ComboTimeLeft = 0;

        _state.Ammo = Service.JobGauges.Get<GNBGauge>().Ammo;
        _state.GunComboStep = Service.JobGauges.Get<GNBGauge>().AmmoComboStep;
        _state.MaxCartridges = _state.Unlocked(TraitID.CartridgeChargeII) ? 3 : 2;

        _state.NoMercyLeft = StatusDetails(Player, SID.NoMercy, Player.InstanceID).Left;
        _state.ReadyToRip = Player.FindStatus(SID.ReadyToRip) != null;
        _state.ReadyToTear = Player.FindStatus(SID.ReadyToTear) != null;
        _state.ReadyToGouge = Player.FindStatus(SID.ReadyToGouge) != null;
        _state.ReadyToBlast = Player.FindStatus(SID.ReadyToBlast) != null;
        _state.AuroraLeft = StatusDetails(Player, SID.Aurora, Player.InstanceID).Left;
        _state.NumTargetsHitByAOE = NumTargetsHitByAOE();
    }

    private void OnConfigModified()
    {
        // placeholders
        SupportedSpell(AID.KeenEdge).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.DemonSlice).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

        // combo replacement
        SupportedSpell(AID.BrutalShell).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextBrutalShellComboAction(ComboLastMove)) : null;
        SupportedSpell(AID.SolidBarrel).TransformAction = _config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.SolidBarrel)) : null;
        SupportedSpell(AID.DemonSlaughter).TransformAction = _config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextAOEComboAction(ComboLastMove)) : null;

        // smart targets
        SupportedSpell(AID.HeartOfCorundum).TransformTarget = SupportedSpell(AID.HeartOfStone).TransformTarget = _config.SmartHeartofCorundumShirkTarget ? SmartTargetFriendly : null;
        SupportedSpell(AID.Shirk).TransformTarget = _config.SmartHeartofCorundumShirkTarget ? SmartTargetCoTank : null;
        SupportedSpell(AID.Provoke).TransformTarget = _config.ProvokeMouseover ? SmartTargetHostile : null; // TODO: also interject/low-blow
    }

    private AID ComboLastMove => (AID)ActionManagerEx.Instance!.ComboLastMove;

    private int NumTargetsHitByAOE() => Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
}
