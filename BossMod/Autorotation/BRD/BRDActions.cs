using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.BRD;

class Actions : CommonActions
{
    public const int AutoActionST = AutoActionFirstCustom + 0;
    public const int AutoActionAOE = AutoActionFirstCustom + 1;

    private readonly Rotation.State _state;
    private readonly Rotation.Strategy _strategy;
    private readonly ConfigListener<BRDConfig> _config;

    public Actions(Autorotation autorot, Actor player)
        : base(autorot, player, Definitions.UnlockQuests, Definitions.SupportedActions)
    {
        _state = new(autorot.WorldState);
        _strategy = new();

        // upgrades
        SupportedSpell(AID.HeavyShot).TransformAction = SupportedSpell(AID.BurstShot).TransformAction = () => ActionID.MakeSpell(_state.BestBurstShot);
        SupportedSpell(AID.StraightShot).TransformAction = SupportedSpell(AID.RefulgentArrow).TransformAction = () => ActionID.MakeSpell(_state.BestRefulgentArrow);
        SupportedSpell(AID.VenomousBite).TransformAction = SupportedSpell(AID.CausticBite).TransformAction = () => ActionID.MakeSpell(_state.BestCausticBite);
        SupportedSpell(AID.Windbite).TransformAction = SupportedSpell(AID.Stormbite).TransformAction = () => ActionID.MakeSpell(_state.BestStormbite);
        SupportedSpell(AID.QuickNock).TransformAction = SupportedSpell(AID.Ladonsbite).TransformAction = () => ActionID.MakeSpell(_state.BestLadonsbite);
        // button replacement
        SupportedSpell(AID.WanderersMinuet).TransformAction = SupportedSpell(AID.PitchPerfect).TransformAction = () => ActionID.MakeSpell(_state.ActiveSong == Rotation.Song.WanderersMinuet ? AID.PitchPerfect : AID.WanderersMinuet);

        SupportedSpell(AID.Peloton).Condition = _ => !Player.InCombat;
        SupportedSpell(AID.HeadGraze).Condition = target => target?.CastInfo?.Interruptible ?? false;

        _config = Service.Config.GetAndSubscribe<BRDConfig>(OnConfigModified);
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
        // TODO: min range to better hit clump with cone...
        // TODO: targeting for rain of death
        var bestTarget = initial;
        if (_state.Unlocked(AID.QuickNock))
        {
            bestTarget = FindBetterTargetBy(bestTarget, 12, e => NumTargetsHitByLadonsbite(e.Actor)).Target;
        }
        return new(bestTarget, bestTarget.StayAtLongRange ? 25 : 12);
    }

    protected override void UpdateInternalState(int autoAction)
    {
        UpdatePlayerState();
        FillCommonStrategy(_strategy, CommonDefinitions.IDPotionStr);
        _strategy.ApplyStrategyOverrides(Autorot.Bossmods.ActiveModule?.PlanExecution?.ActiveStrategyOverrides(Autorot.Bossmods.ActiveModule.StateMachine) ?? []);
        _strategy.NumLadonsbiteTargets = Autorot.PrimaryTarget != null && autoAction != AutoActionST && _state.Unlocked(AID.QuickNock) ? NumTargetsHitByLadonsbite(Autorot.PrimaryTarget) : 0;
        _strategy.NumRainOfDeathTargets = Autorot.PrimaryTarget != null && autoAction != AutoActionST && _state.Unlocked(AID.RainOfDeath) ? NumTargetsHitByRainOfDeath(Autorot.PrimaryTarget) : 0;
    }

    protected override void QueueAIActions()
    {
        if (_state.Unlocked(AID.HeadGraze))
        {
            var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
            SimulateManualActionForAI(ActionID.MakeSpell(AID.HeadGraze), interruptibleEnemy?.Actor, interruptibleEnemy != null);
        }
        if (_state.Unlocked(AID.SecondWind))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
        if (_state.Unlocked(AID.WardensPaean))
        {
            var esunableTarget = FindEsunableTarget();
            SimulateManualActionForAI(ActionID.MakeSpell(AID.WardensPaean), esunableTarget, esunableTarget != null);
        }
        if (_state.Unlocked(AID.Peloton))
            SimulateManualActionForAI(ActionID.MakeSpell(AID.Peloton), Player, !Player.InCombat && _state.PelotonLeft < 3 && _strategy.ForceMovementIn == 0);
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

    private void UpdatePlayerState()
    {
        FillCommonPlayerState(_state);
        if (_state.AnimationLockDelay < 0.1f)
            _state.AnimationLockDelay = 0.1f; // TODO: reconsider; we generally don't want triple weaves or extra-late proc weaves

        var gauge = Service.JobGauges.Get<BRDGauge>();
        _state.ActiveSong = (Rotation.Song)gauge.Song;
        _state.ActiveSongLeft = gauge.SongTimer * 0.001f;
        _state.Repertoire = gauge.Repertoire;
        _state.SoulVoice = gauge.SoulVoice;
        _state.NumCoda = gauge.Coda.Count(c => c != default);

        _state.StraightShotLeft = StatusDetails(Player, SID.StraightShotReady, Player.InstanceID, 30).Left;
        _state.BlastArrowLeft = StatusDetails(Player, SID.BlastArrowReady, Player.InstanceID, 10).Left;
        _state.ShadowbiteLeft = StatusDetails(Player, SID.ShadowbiteReady, Player.InstanceID, 30).Left;
        _state.RagingStrikesLeft = StatusDetails(Player, SID.RagingStrikes, Player.InstanceID, 20).Left;
        _state.BattleVoiceLeft = StatusDetails(Player, SID.BattleVoice, Player.InstanceID, 15).Left;
        _state.RadiantFinaleLeft = StatusDetails(Player, SID.RadiantFinale, Player.InstanceID, 15).Left;
        _state.ArmysMuseLeft = StatusDetails(Player, SID.ArmysMuse, Player.InstanceID, 10).Left;
        _state.BarrageLeft = StatusDetails(Player, SID.Barrage, Player.InstanceID, 10).Left;
        _state.PelotonLeft = StatusDetails(Player, SID.Peloton, Player.InstanceID, 30).Left;

        _state.TargetCausticLeft = StatusDetails(Autorot.PrimaryTarget, _state.ExpectedCaustic, Player.InstanceID, 45).Left;
        _state.TargetStormbiteLeft = StatusDetails(Autorot.PrimaryTarget, _state.ExpectedStormbite, Player.InstanceID, 45).Left;
    }

    private void OnConfigModified(BRDConfig config)
    {
        // placeholders
        SupportedSpell(AID.HeavyShot).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.BurstShot).PlaceholderForAuto = config.FullRotation ? AutoActionST : AutoActionNone;
        SupportedSpell(AID.QuickNock).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;
        SupportedSpell(AID.Ladonsbite).PlaceholderForAuto = config.FullRotation ? AutoActionAOE : AutoActionNone;

        // smart targets
        SupportedSpell(AID.WardensPaean).TransformTarget = config.SmartWardensPaeanTarget ? SmartTargetEsunable : null;
    }

    private int NumTargetsHitByLadonsbite(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOECone(Player.Position, 12, (primary.Position - Player.Position).Normalized(), 45.Degrees());
    private int NumTargetsHitByRainOfDeath(Actor primary) => Autorot.Hints.NumPriorityTargetsInAOECircle(primary.Position, 8);

    // smart targeting utility: return target (if friendly) or mouseover (if friendly) or first esunable party member (if available) or self (otherwise)
    private Actor? FindEsunableTarget() => Autorot.WorldState.Party.WithoutSlot().FirstOrDefault(p => p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)));
    private Actor? SmartTargetEsunable(Actor? primaryTarget) => SmartTargetFriendly(primaryTarget) ?? FindEsunableTarget() ?? Player;
}
