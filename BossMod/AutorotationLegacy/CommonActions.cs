//using FFXIVGame = FFXIVClientStructs.FFXIV.Client.Game;

//namespace BossMod;

//abstract class CommonActions(AutorotationLegacy autorot, Actor player, uint[] unlockData) : IDisposable
//{
//    public const int AutoActionNone = 0;
//    public const int AutoActionAIIdle = 1;
//    public const int AutoActionAIFight = 2;
//    public const int AutoActionFirstCustom = 3;

//    public record struct Targeting(AIHints.Enemy Target, float PreferredRange = 3, Positional PreferredPosition = Positional.Any, bool PreferTanking = false);

//    public readonly Actor Player = player;
//    public int AutoAction { get; private set; }
//    public float MaxCastTime { get; private set; }
//    protected readonly AutorotationLegacy Autorot = autorot;
//    private DateTime _playerCombatStart;
//    private DateTime _autoActionExpire;
//    private bool _forceExpireAtCountdownCancel;
//    private readonly QuestLockCheck _lock = new(unlockData);

//    public void Dispose()
//    {
//        Dispose(true);
//        GC.SuppressFinalize(this);
//    }

//    protected virtual void Dispose(bool disposing) { }

//    // this is called after worldstate update
//    public void Update()
//    {
//        bool wasInCombat = _playerCombatStart != default;
//        if (Player.InCombat && !wasInCombat)
//        {
//            _playerCombatStart = Autorot.WorldState.CurrentTime;
//            _forceExpireAtCountdownCancel = false; // once we're in combat, pull has succeeded, so we don't want to cancel actions anymore
//        }
//        else if (!Player.InCombat && wasInCombat)
//        {
//            _playerCombatStart = new();
//            _autoActionExpire = new(); // immediately expire auto actions, if any
//        }

//        // prepull expiration logic: if we queue up any action during countdown, and then countdown is cancelled, we don't really want to pull
//        if (!Player.InCombat)
//        {
//            if (!_forceExpireAtCountdownCancel && Autorot.WorldState.Client.CountdownRemaining != null)
//            {
//                _forceExpireAtCountdownCancel = true;
//            }
//            else if (_forceExpireAtCountdownCancel && Autorot.WorldState.Client.CountdownRemaining == null)
//            {
//                _forceExpireAtCountdownCancel = false;
//                _autoActionExpire = new();
//            }
//        }

//        if (AutoAction != AutoActionNone && _autoActionExpire < Autorot.WorldState.CurrentTime)
//        {
//            Log($"Auto action {AutoAction} expired");
//            AutoAction = AutoActionNone;
//        }

//        UpdateInternalState(AutoAction);
//    }

//    public unsafe bool HaveItemInInventory(uint id) => FFXIVGame.InventoryManager.Instance()->GetInventoryItemCount(id % 1000000, id >= 1000000, false, false) > 0;

//    public void UpdateAutoAction(int autoAction, float maxCastTime, bool isUserRequested)
//    {
//        var sticky = Autorot.Config.StickyAutoActions && isUserRequested;

//        if (AutoAction != autoAction)
//        {
//            Log($"Auto action set to {autoAction}");
//            AutoAction = autoAction;
//            _autoActionExpire = sticky ? DateTime.MaxValue : Autorot.WorldState.CurrentTime.AddSeconds(1.0f);
//        }
//        else if (sticky)
//        {
//            Log($"Turning off auto action {autoAction}");
//            AutoAction = AutoActionNone;
//            _autoActionExpire = new();
//        }

//        MaxCastTime = maxCastTime;
//    }

//    public void CalculateNextActions(ActionQueue queue)
//    {
//        // TODO: rework to allow modules to decide stuff themselves
//        if (AutoAction != AutoActionNone)
//            CalculateAutomaticActions(queue);
//        if (AutoAction is > AutoActionNone and < AutoActionFirstCustom)
//            QueueAIActions(queue);

//        // TODO: move planned action handling outside?..
//        var delta = Autorot.Hints.PlannedActions.Count;
//        foreach (var a in Autorot.Hints.PlannedActions)
//            queue.Push(a.action, a.target, (a.lowPriority ? ActionQueue.Priority.Low : ActionQueue.Priority.Medium) + --delta * ActionQueue.Priority.Delta);
//    }

//    public virtual Targeting SelectBetterTarget(AIHints.Enemy initial) => new(initial);
//    public virtual void FillStatusesToCancel(List<(uint statusId, ulong sourceId)> list) { }
//    protected abstract void UpdateInternalState(int autoAction);
//    protected abstract void QueueAIActions(ActionQueue queue);
//    protected abstract void CalculateAutomaticActions(ActionQueue queue);
//    protected virtual void OnActionExecuted(in ClientActionRequest request) { }
//    protected virtual void OnActionSucceeded(ActorCastEvent ev) { }

//    protected void FillStrategyPositionals(CommonRotation.Strategy strategy, (Positional pos, bool imm) positional, bool trueNorth)
//    {
//        var ignore = trueNorth || (Autorot.PrimaryTarget?.Omnidirectional ?? true);
//        strategy.NextPositional = positional.pos;
//        strategy.NextPositionalImminent = !ignore && positional.imm;
//        strategy.NextPositionalCorrect = ignore || Autorot.PrimaryTarget == null || positional.pos switch
//        {
//            Positional.Flank => MathF.Abs(Autorot.PrimaryTarget.Rotation.ToDirection().Dot((Player.Position - Autorot.PrimaryTarget.Position).Normalized())) < 0.7071067f,
//            Positional.Rear => Autorot.PrimaryTarget.Rotation.ToDirection().Dot((Player.Position - Autorot.PrimaryTarget.Position).Normalized()) < -0.7071068f,
//            _ => true
//        };
//    }

//    protected void Log(string message)
//    {
//        if (Autorot.Config.Logging)
//            Service.Log($"[AR] [{GetType()}] {message}");
//    }



//    // check whether given actor has tank stance
//    protected static bool HasTankStance(Actor a)
//    {
//        var stanceSID = a.Class switch
//        {
//            Class.WAR => (uint)WAR.SID.Defiance,
//            Class.PLD => (uint)PLD.SID.IronWill,
//            Class.GNB => (uint)GNB.SID.RoyalGuard,
//            _ => 0u
//        };
//        return stanceSID != 0 && a.FindStatus(stanceSID) != null;
//    }
//}
