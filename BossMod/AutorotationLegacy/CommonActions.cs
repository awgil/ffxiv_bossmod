using FFXIVGame = FFXIVClientStructs.FFXIV.Client.Game;

namespace BossMod;

abstract class CommonActions(AutorotationLegacy autorot, Actor player, uint[] unlockData) : IDisposable
{
    public const int AutoActionNone = 0;
    public const int AutoActionAIIdle = 1;
    public const int AutoActionAIFight = 2;
    public const int AutoActionFirstCustom = 3;

    public record struct Targeting(AIHints.Enemy Target, float PreferredRange = 3, Positional PreferredPosition = Positional.Any, bool PreferTanking = false);

    public readonly Actor Player = player;
    public int AutoAction { get; private set; }
    public float MaxCastTime { get; private set; }
    protected readonly AutorotationLegacy Autorot = autorot;
    private DateTime _playerCombatStart;
    private DateTime _autoActionExpire;
    private bool _forceExpireAtCountdownCancel;
    private readonly QuestLockCheck _lock = new(unlockData);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }

    // this is called after worldstate update
    public void Update()
    {
        bool wasInCombat = _playerCombatStart != default;
        if (Player.InCombat && !wasInCombat)
        {
            _playerCombatStart = Autorot.WorldState.CurrentTime;
            _forceExpireAtCountdownCancel = false; // once we're in combat, pull has succeeded, so we don't want to cancel actions anymore
        }
        else if (!Player.InCombat && wasInCombat)
        {
            _playerCombatStart = new();
            _autoActionExpire = new(); // immediately expire auto actions, if any
        }

        // prepull expiration logic: if we queue up any action during countdown, and then countdown is cancelled, we don't really want to pull
        if (!Player.InCombat)
        {
            if (!_forceExpireAtCountdownCancel && Autorot.WorldState.Client.CountdownRemaining != null)
            {
                _forceExpireAtCountdownCancel = true;
            }
            else if (_forceExpireAtCountdownCancel && Autorot.WorldState.Client.CountdownRemaining == null)
            {
                _forceExpireAtCountdownCancel = false;
                _autoActionExpire = new();
            }
        }

        if (AutoAction != AutoActionNone && _autoActionExpire < Autorot.WorldState.CurrentTime)
        {
            Log($"Auto action {AutoAction} expired");
            AutoAction = AutoActionNone;
        }

        UpdateInternalState(AutoAction);
    }

    public unsafe bool HaveItemInInventory(uint id) => FFXIVGame.InventoryManager.Instance()->GetInventoryItemCount(id % 1000000, id >= 1000000, false, false) > 0;

    public float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - Autorot.WorldState.CurrentTime).TotalSeconds, 0.0f);

    // this also checks pending statuses
    // note that we check pending statuses first - otherwise we get the same problem with double refresh if we try to refresh early (we find old status even though we have pending one)
    public (float Left, int Stacks) StatusDetails(Actor? actor, uint sid, ulong sourceID, float pendingDuration = 1000)
    {
        if (actor == null)
            return (0, 0);
        var pending = Autorot.WorldState.PendingEffects.PendingStatus(actor.InstanceID, sid, sourceID);
        if (pending != null)
            return (pendingDuration, pending.Value);
        var status = actor.FindStatus(sid, sourceID);
        return status != null ? (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF) : (0, 0);
    }
    public (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);

    // check whether specified status is a damage buff
    // see https://i.redd.it/xrtgpras94881.png
    // TODO: AST card buffs?, enemy debuffs?, single-target buffs (DRG dragon sight, DNC devilment)
    public bool IsDamageBuff(uint statusID) => statusID switch
    {
        49 => true, // medicated
        141 => true, // BRD battle voice
        //638 => true, // NIN trick attack - note that this is a debuff on enemy
        786 => true, // DRG battle litany
        1185 => true, // MNK brotherhood
        //1221 => true, // SCH chain stratagem - note that this is a debuff on enemy
        1297 => true, // RDM embolden
        1822 => true, // DNC technical finish
        1878 => true, // AST divination
        2599 => true, // RPR arcane circle
        2703 => true, // SMN searing light
        2964 => true, // BRD radiant finale
        _ => false
    };

    public void UpdateAutoAction(int autoAction, float maxCastTime, bool isUserRequested)
    {
        var sticky = Autorot.Config.StickyAutoActions && isUserRequested;

        if (AutoAction != autoAction)
        {
            Log($"Auto action set to {autoAction}");
            AutoAction = autoAction;
            _autoActionExpire = sticky ? DateTime.MaxValue : Autorot.WorldState.CurrentTime.AddSeconds(1.0f);
        }
        else if (sticky)
        {
            Log($"Turning off auto action {autoAction}");
            AutoAction = AutoActionNone;
            _autoActionExpire = new();
        }

        MaxCastTime = maxCastTime;
    }

    public void CalculateNextActions(ActionQueue queue)
    {
        // TODO: rework to allow modules to decide stuff themselves
        if (AutoAction != AutoActionNone)
            CalculateAutomaticActions(queue);
        if (AutoAction is > AutoActionNone and < AutoActionFirstCustom)
            QueueAIActions(queue);

        // TODO: move planned action handling outside?..
        var delta = Autorot.Hints.PlannedActions.Count;
        foreach (var a in Autorot.Hints.PlannedActions)
            queue.Push(a.action, a.target, (a.lowPriority ? ActionQueue.Priority.Low : ActionQueue.Priority.Medium) + --delta * ActionQueue.Priority.Delta);
    }

    public void NotifyActionExecuted(in ClientActionRequest request)
    {
        Log($"Exec #{request.SourceSequence} {request.Action} @ {request.TargetID:X} [{GetState()}]");
        Autorot.Bossmods.ActiveModule?.PlanExecution?.NotifyActionExecuted(Autorot.Bossmods.ActiveModule.StateMachine, request.Action);
        OnActionExecuted(in request);
    }

    public void NotifyActionSucceeded(ActorCastEvent ev)
    {
        Log($"Cast #{ev.SourceSequence} {ev.Action} @ {ev.MainTargetID:X} [{GetState()}]");
        OnActionSucceeded(ev);
    }

    public abstract CommonRotation.PlayerState GetState();
    public abstract CommonRotation.Strategy GetStrategy();
    public virtual Targeting SelectBetterTarget(AIHints.Enemy initial) => new(initial);
    public virtual void FillStatusesToCancel(List<(uint statusId, ulong sourceId)> list) { }
    protected abstract void UpdateInternalState(int autoAction);
    protected abstract void QueueAIActions(ActionQueue queue);
    protected abstract void CalculateAutomaticActions(ActionQueue queue);
    protected virtual void OnActionExecuted(in ClientActionRequest request) { }
    protected virtual void OnActionSucceeded(ActorCastEvent ev) { }

    protected void PushResult(ActionQueue queue, ActionID action, Actor? target)
    {
        var data = action ? ActionDefinitions.Instance[action] : null;
        if (data == null)
            return;
        if (data.Range == 0)
            target = Player; // override range-0 actions to always target player
        if (target == null || Autorot.Hints.ForbiddenTargets.FirstOrDefault(e => e.Actor == target)?.Priority == AIHints.Enemy.PriorityForbidFully)
            return; // forbidden
        queue.Push(action, target, (data.IsGCD ? ActionQueue.Priority.High : ActionQueue.Priority.Low) + 500);
    }
    protected void PushResult<AID>(ActionQueue queue, AID aid, Actor? target) where AID : Enum => PushResult(queue, ActionID.MakeSpell(aid), target);

    // fill common state properties
    protected void FillCommonPlayerState(CommonRotation.PlayerState s)
    {
        var vuln = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextVulnerable(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 10000);

        var am = Autorot.ActionManager;
        s.Level = Player.Level;
        s.UnlockProgress = _lock.Progress();
        s.CurMP = Player.HPMP.CurMP;
        s.TargetingEnemy = Autorot.PrimaryTarget != null && Autorot.PrimaryTarget.Type is ActorType.Enemy or ActorType.Part && !Autorot.PrimaryTarget.IsAlly;
        s.RangeToTarget = Autorot.PrimaryTarget != null ? (Autorot.PrimaryTarget.Position - Player.Position).Length() - Autorot.PrimaryTarget.HitboxRadius - Player.HitboxRadius : float.MaxValue;
        s.AnimationLock = am.EffectiveAnimationLock;
        s.AnimationLockDelay = am.AnimationLockDelayEstimate;
        s.ComboTimeLeft = am.ComboTimeLeft;
        s.ComboLastAction = am.ComboLastMove;
        s.LimitBreakLevel = Autorot.WorldState.Party.LimitBreakMax > 0 ? Autorot.WorldState.Party.LimitBreakCur / Autorot.WorldState.Party.LimitBreakMax : 0;

        // all GCD skills share the same base recast time (with some exceptions that aren't relevant here)
        // so we can check Fast Blade (9) and Stone (119) recast timers to get effective sks and sps
        // regardless of current class
        s.AttackGCDTime = FFXIVGame.ActionManager.GetAdjustedRecastTime(FFXIVGame.ActionType.Action, 9) / 1000f;
        s.SpellGCDTime = FFXIVGame.ActionManager.GetAdjustedRecastTime(FFXIVGame.ActionType.Action, 119) / 1000f;

        s.RaidBuffsLeft = vuln.Item1 ? vuln.Item2 : 0;
        foreach (var status in Player.Statuses.Where(s => IsDamageBuff(s.ID)))
        {
            s.RaidBuffsLeft = MathF.Max(s.RaidBuffsLeft, StatusDuration(status.ExpireAt));
        }
        // TODO: also check damage-taken debuffs on target
    }

    // fill common strategy properties
    protected void FillCommonStrategy(CommonRotation.Strategy strategy, ActionID potion)
    {
        var targetEnemy = Autorot.PrimaryTarget != null ? Autorot.Hints.PotentialTargets.Find(e => e.Actor == Autorot.PrimaryTarget) : null;
        var downtime = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextDowntime(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 0);
        var poslock = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextPositioning(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 10000);
        var vuln = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextVulnerable(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 10000);

        strategy.CombatTimer = CombatTimer();
        strategy.ForbidDOTs = targetEnemy?.ForbidDOTs ?? false;
        strategy.ForceMovementIn = MaxCastTime;
        strategy.FightEndIn = downtime.Item1 ? 0 : downtime.Item2;
        strategy.RaidBuffsIn = vuln.Item1 ? 0 : vuln.Item2;
        if (Autorot.Bossmods.ActiveModule?.PlanConfig != null) // assumption: if there is no planning support for encounter (meaning it's something trivial, like outdoor boss), don't expect any cooldowns
            strategy.RaidBuffsIn = Math.Min(strategy.RaidBuffsIn, Autorot.Bossmods.RaidCooldowns.NextDamageBuffIn(Autorot.WorldState.CurrentTime));
        strategy.PositionLockIn = Autorot.Config.EnableMovement && !poslock.Item1 ? poslock.Item2 : 0;
        strategy.NextPositional = Positional.Any;
        strategy.NextPositionalImminent = false;
        strategy.NextPositionalCorrect = true;
    }

    protected void FillStrategyPositionals(CommonRotation.Strategy strategy, (Positional pos, bool imm) positional, bool trueNorth)
    {
        var ignore = trueNorth || (Autorot.PrimaryTarget?.Omnidirectional ?? true);
        strategy.NextPositional = positional.pos;
        strategy.NextPositionalImminent = !ignore && positional.imm;
        strategy.NextPositionalCorrect = ignore || Autorot.PrimaryTarget == null || positional.pos switch
        {
            Positional.Flank => MathF.Abs(Autorot.PrimaryTarget.Rotation.ToDirection().Dot((Player.Position - Autorot.PrimaryTarget.Position).Normalized())) < 0.7071067f,
            Positional.Rear => Autorot.PrimaryTarget.Rotation.ToDirection().Dot((Player.Position - Autorot.PrimaryTarget.Position).Normalized()) < -0.7071068f,
            _ => true
        };
    }

    protected void Log(string message)
    {
        if (Autorot.Config.Logging)
            Service.Log($"[AR] [{GetType()}] {message}");
    }

    private float CombatTimer() => _playerCombatStart != default
            ? (float)(Autorot.WorldState.CurrentTime - _playerCombatStart).TotalSeconds
            : -Math.Max(0.001f, Autorot.WorldState.Client.CountdownRemaining ?? float.MaxValue);

    protected (AIHints.Enemy Target, int Priority) FindBetterTargetBy(AIHints.Enemy initial, float maxDistanceFromPlayer, Func<AIHints.Enemy, int> prioFunc)
    {
        var bestTarget = initial;
        var bestPrio = prioFunc(bestTarget);
        foreach (var enemy in Autorot.Hints.PriorityTargets.Where(x => x != initial && x.Actor.Position.InCircle(Player.Position, maxDistanceFromPlayer + x.Actor.HitboxRadius)))
        {
            var newPrio = prioFunc(enemy);
            if (newPrio > bestPrio)
            {
                bestPrio = newPrio;
                bestTarget = enemy;
            }
        }
        return (bestTarget, bestPrio);
    }
}
