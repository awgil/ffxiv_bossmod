using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.SMN;

class Actions : CommonActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private bool _aoe;
    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private readonly ConfigListener<SMNConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _state = new(autorot.WorldState);
        _strategy = new();
        _config = Service.Config.GetAndSubscribe<SMNConfig>(OnConfigModified);
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
        // TODO: AOE & multidotting
        return new(initial, 25);
    }

    protected override void UpdateInternalState(int autoAction)
    {
        _aoe = autoAction switch
        {
            AutoActionST => false,
            AutoActionAOE => true, // TODO: consider making AI-like check
            AutoActionAIFight => Autorot.PrimaryTarget != null && Autorot.Hints.NumPriorityTargetsInAOECircle(Autorot.PrimaryTarget.Position, 5) >= 3,
            _ => false, // irrelevant...
        };
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);
    }

    protected override void QueueAIActions()
    {
    }

    protected override ActionQueue.Entry CalculateAutomaticGCD()
    {
        if (!Player.InCombat && _state.Unlocked(AID.SummonCarbuncle) && !_state.PetSummoned)
            return MakeResult(AID.SummonCarbuncle, Player);
        //if ((AutoStrategy & AutoAction.GCDHeal) != 0)
        //    return MakeResult(AID.Physick, Autorot.SecondaryTarget); // TODO: automatic target selection

        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return default;
        var aid = Rotation.GetNextBestGCD(_state, _strategy, _aoe, _strategy.ForceMovementIn < 5);
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

        _state.PetSummoned = Autorot.WorldState.Actors.Any(a => a.Type == ActorType.Pet && a.OwnerID == Player.InstanceID);

        var gauge = Service.JobGauges.Get<SMNGauge>();
        _state.IfritReady = gauge.IsIfritReady;
        _state.TitanReady = gauge.IsTitanReady;
        _state.GarudaReady = gauge.IsGarudaReady;
        _state.Attunement = (Rotation.Attunement)(((int)gauge.AetherFlags >> 2) & 3);
        _state.AttunementStacks = gauge.Attunement;
        _state.AttunementLeft = gauge.AttunmentTimerRemaining * 0.001f;
        _state.SummonLockLeft = gauge.SummonTimerRemaining * 0.001f;
        _state.AetherflowStacks = gauge.AetherflowStacks;

        _state.SwiftcastLeft = 0;
        foreach (var status in Player.Statuses)
        {
            switch ((SID)status.ID)
            {
                case SID.Swiftcast:
                    _state.SwiftcastLeft = StatusDuration(status.ExpireAt);
                    break;
            }
        }
    }

    private void OnConfigModified(SMNConfig config)
    {
        // placeholders
        SupportedSpell(AID.Ruin1).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.Outburst).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;

        // smart targets
        SupportedSpell(AID.Physick).TransformTarget = config.MouseoverFriendly ? SmartTargetFriendly : null;
    }

    //private AID SmartResurrectAction()
    //{
    //    // 1. swiftcast, if ready and not up yet
    //    if (_state.Unlocked(AID.Swiftcast) && _state.SwiftcastLeft <= 0 && _state.CD(CDGroup.Swiftcast) <= 0)
    //        return AID.Swiftcast;

    //    return AID.Resurrection;
    //}
}
