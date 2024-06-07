using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.SAM;

class Actions : CommonActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private DateTime _lastTsubame;
    private float _tsubameCooldown;
    private readonly ConfigListener<SAMConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _state = new(autorot.WorldState);
        _strategy = new();

        SupportedSpell(AID.Iaijutsu).TransformAction = () => ActionID.MakeSpell(_state.BestIai);
        SupportedSpell(AID.MeikyoShisui).Condition = _ => _state.MeikyoLeft == 0;

        _config = Service.Config.GetAndSubscribe<SAMConfig>(OnConfigModified);
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
        if (_state.Kenki >= 50)
        {
            (var target, var prio) = FindBetterTargetBy(initial, 10, x => NumGurenTargets(x.Actor));
            if (prio >= 3)
                return new(target, 10);
        }

        if (_state.OgiNamikiriLeft > 0 || !_state.Unlocked(AID.Fuko))
        {
            (var target, var prio) = FindBetterTargetBy(initial, 8, x => NumConeTargets(x.Actor));
            if (prio >= 3)
                return new(target, 8);
        }

        return new(
            initial,
            _state.SenCount == 3 ? 6 : 3,
            _strategy.NextPositionalImminent ? _strategy.NextPositional : Positional.Any
        );
    }

    private void OnConfigModified(SAMConfig config)
    {
        SupportedSpell(AID.Hakaze).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.Fuga).PlaceholderForAuto = SupportedSpell(AID.Fuko).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;
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
        if (AutoAction < AutoActionAIFight)
            return default;

        ActionID res = new();
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline - _state.OGCDSlotLength);
        if (!res && _state.CanWeave(deadline)) // second/only ogcd slot
            res = Rotation.GetNextBestOGCD(_state, _strategy, deadline);

        return MakeResult(res, Autorot.PrimaryTarget);
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.SecondWind))
            SimulateManualActionForAI(
                ActionID.MakeSpell(AID.SecondWind),
                Player,
                Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f
            );
        if (_state.Unlocked(AID.Bloodbath))
            SimulateManualActionForAI(
                ActionID.MakeSpell(AID.Bloodbath),
                Player,
                Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8f
            );
        if (_state.Unlocked(AID.MeikyoShisui))
            SimulateManualActionForAI(
                ActionID.MakeSpell(AID.MeikyoShisui),
                Player,
                !_state.HasCombatBuffs
                    && _strategy.CombatTimer > 0
                    && _strategy.CombatTimer < 5
                    && _state.MeikyoLeft == 0
            );
    }

    protected override void UpdateInternalState(int autoAction)
    {
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
        _strategy.ApplyStrategyOverrides(
            Autorot
                .Bossmods.ActiveModule?.PlanExecution
                ?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []
        );

        _strategy.NumAOETargets = autoAction == AutoActionST ? 0 : NumAOETargets();
        _strategy.NumTenkaTargets =
            autoAction == AutoActionST ? 0 : Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 8);
        _strategy.NumOgiTargets = autoAction == AutoActionST ? 0 : NumConeTargets(Autorot.PrimaryTarget);
        _strategy.NumGurenTargets = autoAction == AutoActionST ? 0 : NumGurenTargets(Autorot.PrimaryTarget);

        FillStrategyPositionals(
            _strategy,
            Rotation.GetNextPositional(_state, _strategy),
            _state.TrueNorthLeft > _state.GCD
        );
    }

    private int NumAOETargets() => Autorot.Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    private int NumGurenTargets(Actor? enemy) => enemy == null ? 0 : Autorot.Hints.NumPriorityTargetsInAOERect(Player.Position, (enemy.Position - Player.Position).Normalized(), 10, 4);
    private int NumConeTargets(Actor? enemy) => enemy == null ? 0 : Autorot.Hints.NumPriorityTargetsInAOECone(Player.Position, 8, (enemy.Position - Player.Position).Normalized(), 60.Degrees());

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);

        var newTsubameCooldown = _state.CD(CDGroup.TsubameGaeshi);
        if (newTsubameCooldown > _tsubameCooldown + 10) // eliminate variance, cd increment is 60s
            _lastTsubame = Autorot.WorldState.CurrentTime;

        _tsubameCooldown = newTsubameCooldown;

        var gauge = Service.JobGauges.Get<SAMGauge>();

        _state.HasIceSen = gauge.HasSetsu;
        _state.HasMoonSen = gauge.HasGetsu;
        _state.HasFlowerSen = gauge.HasKa;
        _state.MeditationStacks = gauge.MeditationStacks;
        _state.Kenki = gauge.Kenki;
        _state.Kaeshi = gauge.Kaeshi;
        _state.FukaLeft = StatusDetails(Player, SID.Fuka, Player.InstanceID).Left;
        _state.FugetsuLeft = StatusDetails(Player, SID.Fugetsu, Player.InstanceID).Left;
        _state.TrueNorthLeft = StatusDetails(Player, SID.TrueNorth, Player.InstanceID).Left;
        _state.MeikyoLeft = StatusDetails(Player, SID.MeikyoShisui, Player.InstanceID).Left;
        _state.OgiNamikiriLeft = StatusDetails(Player, SID.OgiNamikiriReady, Player.InstanceID).Left;

        _state.LostExcellenceLeft = MathF.Max(
            StatusDetails(Player, SID.LostExcellence, Player.InstanceID).Left,
            StatusDetails(Player, SID.Memorable, Player.InstanceID).Left
        );
        _state.FoPLeft = StatusDetails(Player, SID.LostFontofPower, Player.InstanceID).Left;
        _state.HsacLeft = StatusDetails(Player, SID.BannerHonoredSacrifice, Player.InstanceID).Left;

        _state.TargetHiganbanaLeft = _strategy.ForbidDOTs
            ? float.MaxValue
            : StatusDetails(Autorot.PrimaryTarget, SID.Higanbana, Player.InstanceID).Left;

        _state.GCDTime = _state.AttackGCDTime;
        _state.LastTsubame =
            _lastTsubame == default
                ? float.MaxValue
                : (float)(Autorot.WorldState.CurrentTime - _lastTsubame).TotalSeconds;

        _state.ClosestPositional = GetClosestPositional();
    }

    private Positional GetClosestPositional()
    {
        var tar = Autorot.PrimaryTarget;
        if (tar == null)
            return Positional.Any;

        return (Player.Position - tar.Position).Normalized().Dot(tar.Rotation.ToDirection()) switch
        {
            < -0.707167f => Positional.Rear,
            < 0.707167f => Positional.Flank,
            _ => Positional.Front
        };
    }
}
