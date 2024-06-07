using System;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.MNK;

class Actions : CommonActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;
    public const int AutoActionFiller = AutoActionFirstCustom + 2;
    public const int AutoActionSTQOpener = AutoActionFirstCustom + 3;

    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private readonly ConfigListener<MNKConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _state = new(autorot.WorldState);
        _strategy = new();

        // upgrades
        SupportedSpell(AID.SteelPeak).TransformAction = SupportedSpell(AID.ForbiddenChakra).TransformAction = () => ActionID.MakeSpell(_state.BestForbiddenChakra);
        SupportedSpell(AID.HowlingFist).TransformAction = SupportedSpell(AID.Enlightenment).TransformAction = () => ActionID.MakeSpell(_state.BestEnlightenment);
        SupportedSpell(AID.Meditation).TransformAction = () => ActionID.MakeSpell(_state.Chakra == 5 ? _state.BestForbiddenChakra : AID.Meditation);
        SupportedSpell(AID.ArmOfTheDestroyer).TransformAction = SupportedSpell(AID.ShadowOfTheDestroyer).TransformAction = () => ActionID.MakeSpell(_state.BestShadowOfTheDestroyer);
        SupportedSpell(AID.MasterfulBlitz).TransformAction = () => ActionID.MakeSpell(_state.BestBlitz);
        SupportedSpell(AID.PerfectBalance).Condition = _ => _state.PerfectBalanceLeft == 0;

        _config = Service.Config.GetAndSubscribe<MNKConfig>(OnConfigModified);
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
        // TODO: multidotting support...
        var pos = (_state.Form == Rotation.Form.Coeurl ? Rotation.GetCoeurlFormAction(_state, _strategy) : AID.None) switch
        {
            AID.SnapPunch => Positional.Flank,
            AID.Demolish => Positional.Rear,
            _ => Positional.Any
        };
        return new(initial, 3, pos);
    }

    protected override void UpdateInternalState(int autoAction)
    {
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
        _strategy.NumBlitzTargets = NumTargetsHitByBlitz();
        _strategy.ApplyStrategyOverrides(Autorot.Bossmods.ActiveModule?.PlanExecution?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []);
        _strategy.NumPointBlankAOETargets = autoAction == AutoActionST ? 0 : NumTargetsHitByPBAOE();
        _strategy.NumEnlightenmentTargets = Autorot.PrimaryTarget != null && autoAction != AutoActionST && _state.Unlocked(AID.HowlingFist) ? NumTargetsHitByEnlightenment(Autorot.PrimaryTarget) : 0;

        _strategy.UseAOE = _strategy.NumPointBlankAOETargets >= 3;
        _strategy.UseSTQOpener = autoAction == AutoActionSTQOpener;

        if (autoAction == AutoActionFiller)
        {
            _strategy.FireUse = Rotation.Strategy.FireStrategy.Delay;
            _strategy.WindUse = CommonRotation.Strategy.OffensiveAbilityUse.Delay;
            _strategy.BrotherhoodUse = CommonRotation.Strategy.OffensiveAbilityUse.Delay;
            _strategy.PerfectBalanceUse = CommonRotation.Strategy.OffensiveAbilityUse.Delay;
            _strategy.TrueNorthUse = CommonRotation.Strategy.OffensiveAbilityUse.Delay;
        }

        FillStrategyPositionals(_strategy, Rotation.GetNextPositional(_state, _strategy), _state.TrueNorthLeft > _state.GCD);
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.SecondWind))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
        if (_state.Unlocked(AID.Bloodbath))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Bloodbath), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8f);
        if (_state.Unlocked(AID.Meditation))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Meditation), Player, !Player.InCombat && _state.Chakra < 5);
        // TODO: this ends up being super annoying in some cases, maybe reconsider conditions
        // if (_state.Unlocked(AID.FormShift))
        //     SimulateManualActionForAI(ActionID.MakeSpell(AID.FormShift), Player, !Player.InCombat && _state.FormShiftLeft == 0 && _state.PerfectBalanceLeft == 0);
    }

    protected override ActionQueue.Entry CalculateAutomaticGCD()
    {
        if (AutoAction < AutoActionAIFight)
            return default;
        var aid = Rotation.GetNextBestGCD(_state, _strategy);
        return MakeResult(aid, Autorot.PrimaryTarget);
    }

    protected override ActionQueue.Entry CalculateAutomaticOGCD(float deadline)
    {
        if (AutoAction < AutoActionAIFight)
            return default;

        ActionID res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength, deadline);
        if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline, deadline);
        return MakeResult(res, Autorot.PrimaryTarget);
    }

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);

        var gauge = Service.JobGauges.Get<MNKGauge>();
        _state.Chakra = gauge.Chakra;
        _state.BeastChakra = gauge.BeastChakra;
        _state.Nadi = gauge.Nadi;
        _state.BlitzLeft = gauge.BlitzTimeRemaining / 1000f;

        (_state.Form, _state.FormLeft) = DetermineForm();
        _state.DisciplinedFistLeft = StatusDetails(Player, SID.DisciplinedFist, Player.InstanceID).Left;
        _state.LeadenFistLeft = StatusDetails(Player, SID.LeadenFist, Player.InstanceID).Left;
        _state.PerfectBalanceLeft = StatusDetails(Player, SID.PerfectBalance, Player.InstanceID).Left;
        _state.FormShiftLeft = StatusDetails(Player, SID.FormlessFist, Player.InstanceID).Left;
        _state.FireLeft = StatusDetails(Player, SID.RiddleOfFire, Player.InstanceID).Left;
        _state.TrueNorthLeft = StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;

        // these are functionally the same as far as the rotation is concerned
        _state.LostExcellenceLeft = MathF.Max(
            StatusDetails(Player, SID.LostExcellence, Player.InstanceID).Left,
            StatusDetails(Player, SID.Memorable, Player.InstanceID).Left
        );
        _state.FoPLeft = StatusDetails(Player, SID.LostFontofPower, Player.InstanceID).Left;
        _state.HsacLeft = StatusDetails(Player, SID.BannerHonoredSacrifice, Player.InstanceID).Left;

        _state.TargetDemolishLeft = StatusDetails(Autorot.PrimaryTarget, SID.Demolish, Player.InstanceID).Left;
    }

    private (Rotation.Form, float) DetermineForm()
    {
        var s = StatusDetails(Player, SID.OpoOpoForm, Player.InstanceID).Left;
        if (s > 0)
            return (Rotation.Form.OpoOpo, s);
        s = StatusDetails(Player, SID.RaptorForm, Player.InstanceID).Left;
        if (s > 0)
            return (Rotation.Form.Raptor, s);
        s = StatusDetails(Player, SID.CoeurlForm, Player.InstanceID).Left;
        if (s > 0)
            return (Rotation.Form.Coeurl, s);
        return (Rotation.Form.None, 0);
    }

    private void OnConfigModified(MNKConfig config)
    {
        // placeholders
        SupportedSpell(AID.Bootshine).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.ArmOfTheDestroyer).PlaceholderForAuto = SupportedSpell(AID.ShadowOfTheDestroyer).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;
        SupportedSpell(AID.TrueStrike).PlaceholderForAuto = config.FillerRotation ? AutoActionFiller : AutoActionNone;
        SupportedSpell(AID.SnapPunch).PlaceholderForAuto = config.FullRotation ? AutoActionSTQOpener : AutoActionNone;

        // combo replacement
        SupportedSpell(AID.FourPointFury).TransformAction = config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextComboAction(_state, _strategy)) : null;

        SupportedSpell(AID.Thunderclap).Condition = config.PreventCloseDash
            ? ((act) => act == null || !act.Position.InCircle(Player.Position, 3))
            : null;

        SupportedSpell(AID.Thunderclap).TransformTarget = config.SmartThunderclap ? (act) => Autorot.SecondaryTarget ?? act : null;

        // smart targets
    }

    private int NumTargetsHitByBlitz()
    {
        if (_state.BestBlitz is AID.TornadoKick or AID.PhantomRush)
            return Autorot.PrimaryTarget == null ? 0 : Autorot.Hints.NumPriorityTargetsInAOECircle(Autorot.PrimaryTarget.Position, 5);
        return Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    }

    private int NumTargetsHitByPBAOE() => Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    private int NumTargetsHitByEnlightenment(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 10, _state.Unlocked(AID.Enlightenment) ? 2 : 1);
}
