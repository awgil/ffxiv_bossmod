using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.BLM;

class Actions : CommonActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private DateTime _lastManaTick;
    private uint _prevMP;
    private readonly ConfigListener<BLMConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _state = new(autorot.WorldState);
        _strategy = new();
        _prevMP = player.HPMP.CurMP;
        _config = Service.Config.GetAndSubscribe<BLMConfig>(OnConfigModified);
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
        // TODO: multidot?..
        var bestTarget = initial;
        if (_state.Unlocked(AID.Blizzard2))
        {
            bestTarget = FindBetterTargetBy(initial, 25, e => NumTargetsHitByAOE(e.Actor)).Target;
        }
        return new(bestTarget, bestTarget.StayAtLongRange ? 25 : 15);
    }

    protected override void UpdateInternalState(int autoAction)
    {
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionInt);
        if (autoAction == AutoActionAIFight)
        {
            _strategy.NumAOETargets = Autorot.PrimaryTarget != null ? NumTargetsHitByAOE(Autorot.PrimaryTarget) : 0;
        }
        else
        {
            _strategy.NumAOETargets = autoAction == AutoActionAOE ? 100 : 0; // TODO: consider making AI-like check
        }
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.Transpose))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Transpose), Player, !Player.InCombat && _state.ElementalLevel > 0 && _state.CurMP < 10000);
        if (_state.Unlocked(AID.Manaward))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Manaward), Player, Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8f);
    }

    protected override ActionQueue.Entry CalculateAutomaticGCD()
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return default;
        var aid = Rotation.GetNextBestGCD(_state, _strategy);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override ActionQueue.Entry CalculateAutomaticOGCD(float deadline)
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return default;

        ActionID res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
        if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);
        return MakeResult(res, Autorot.PrimaryTarget);
    }

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);

        var gauge = Service.JobGauges.Get<BLMGauge>();

        // track mana ticks
        if (_prevMP < Player.HPMP.CurMP && !gauge.InAstralFire)
        {
            var expectedTick = Rotation.MPTick(-gauge.UmbralIceStacks);
            if (Player.HPMP.CurMP - _prevMP == expectedTick)
            {
                _lastManaTick = Autorot.WorldState.CurrentTime;
            }
        }
        _prevMP = Player.HPMP.CurMP;
        _state.TimeToManaTick = 3 - (_lastManaTick != default ? (float)(Autorot.WorldState.CurrentTime - _lastManaTick).TotalSeconds % 3 : 0);

        _state.ElementalLevel = gauge.InAstralFire ? gauge.AstralFireStacks : -gauge.UmbralIceStacks;
        _state.ElementalLeft = gauge.ElementTimeRemaining * 0.001f;

        _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
        _state.ThundercloudLeft = StatusDetails(Player, SID.Thundercloud, Player.InstanceID).Left;
        _state.FirestarterLeft = StatusDetails(Player, SID.Firestarter, Player.InstanceID).Left;

        _state.TargetThunderLeft = Math.Max(StatusDetails(Autorot.PrimaryTarget, _state.ExpectedThunder3, Player.InstanceID).Left, StatusDetails(Autorot.PrimaryTarget, SID.Thunder2, Player.InstanceID).Left);
    }

    private void OnConfigModified(BLMConfig config)
    {
        // placeholders
        SupportedSpell(AID.Blizzard1).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.Blizzard2).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;

        // smart targets
        SupportedSpell(AID.AetherialManipulation).TransformTarget = config.MouseoverFriendly ? SmartTargetFriendly : null;
    }

    private int NumTargetsHitByAOE(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOECircle(primary.Position, 5);
}
