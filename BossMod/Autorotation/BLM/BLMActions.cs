using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.BLM;

class Actions : CommonActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private BLMConfig _config;
    private Rotation.State _state;
    private Rotation.Strategy _strategy;
    private DateTime _lastManaTick;
    private uint _prevMP;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _config = Service.Config.Get<BLMConfig>();
        _state = new(autorot.WorldState);
        _strategy = new();
        _prevMP = player.CurMP;

        _config.Modified += OnConfigModified;
        OnConfigModified();
    }

    public override void Dispose()
    {
        _config.Modified -= OnConfigModified;
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
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Manaward), Player, Player.HP.Cur < Player.HP.Max * 0.8f);
    }

    protected override NextAction CalculateAutomaticGCD()
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return new();
        var aid = Rotation.GetNextBestGCD(_state, _strategy);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override NextAction CalculateAutomaticOGCD(float deadline)
    {
        if (Autorot.PrimaryTarget == null || AutoAction < AutoActionAIFight)
            return new();

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
        if (_prevMP < Player.CurMP && !gauge.InAstralFire)
        {
            var expectedTick = Rotation.MPTick(-gauge.UmbralIceStacks);
            if (Player.CurMP - _prevMP == expectedTick)
            {
                _lastManaTick = Autorot.WorldState.CurrentTime;
            }
        }
        _prevMP = Player.CurMP;
        _state.TimeToManaTick = 3 - (_lastManaTick != default ? (float)(Autorot.WorldState.CurrentTime - _lastManaTick).TotalSeconds % 3 : 0);

        _state.ElementalLevel = gauge.InAstralFire ? gauge.AstralFireStacks : -gauge.UmbralIceStacks;
        _state.ElementalLeft = gauge.ElementTimeRemaining * 0.001f;

        _state.SwiftcastLeft = StatusDetails(Player, SID.Swiftcast, Player.InstanceID).Left;
        _state.ThundercloudLeft = StatusDetails(Player, SID.Thundercloud, Player.InstanceID).Left;
        _state.FirestarterLeft = StatusDetails(Player, SID.Firestarter, Player.InstanceID).Left;

        _state.TargetThunderLeft = Math.Max(StatusDetails(Autorot.PrimaryTarget, _state.ExpectedThunder3, Player.InstanceID).Left, StatusDetails(Autorot.PrimaryTarget, SID.Thunder2, Player.InstanceID).Left);
    }

    private void OnConfigModified()
    {
        // placeholders
        SupportedSpell(AID.Blizzard1).PlaceholderForAuto = _config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.Blizzard2).PlaceholderForAuto = _config.FullRotation ? AutoActionAOE : AutoActionNone;

        // smart targets
        SupportedSpell(AID.AetherialManipulation).TransformTarget = _config.MouseoverFriendly ? SmartTargetFriendly : null;
    }

    private int NumTargetsHitByAOE(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOECircle(primary.Position, 5);
}
