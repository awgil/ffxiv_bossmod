using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    abstract class CommonActions : IDisposable
    {
        public const int AutoActionNone = 0;
        public const int AutoActionAIIdle = 1;
        public const int AutoActionAIFight = 2;
        public const int AutoActionFirstCustom = 3;

        public enum ActionSource { Automatic, Planned, Manual, Emergency }

        public struct NextAction
        {
            public ActionID Action;
            public Actor? Target;
            public Vector3 TargetPos;
            public ActionSource Source;

            public NextAction(ActionID action, Actor? target, Vector3 targetPos, ActionSource source)
            {
                Action = action;
                Target = target;
                TargetPos = targetPos;
                Source = source;
            }
        }

        public struct Targeting
        {
            public AIHints.Enemy? Target;
            public float PreferredRange;
            public Positional PreferredPosition;
            public bool PreferTanking;

            public Targeting(AIHints.Enemy target, float range = 3, Positional pos = Positional.Any, bool preferTanking = false)
            {
                Target = target;
                PreferredRange = range;
                PreferredPosition = pos;
                PreferTanking = preferTanking;
            }
        }

        public class SupportedAction
        {
            public ActionDefinition Definition;
            public bool IsGT;
            public Func<Actor?, bool>? Condition;
            public int PlaceholderForAuto; // if set, attempting to execute this action would instead initiate auto-strategy
            public Func<ActionID>? TransformAction;
            public Func<Actor?, Actor?>? TransformTarget;

            public SupportedAction(ActionDefinition definition, bool isGT)
            {
                Definition = definition;
                IsGT = isGT;
            }

            public bool Allowed(Actor player, Actor target)
            {
                if (player != target)
                {
                    var distSq = (target.Position - player.Position).LengthSq();
                    var effRange = Definition.Range + player.HitboxRadius + target.HitboxRadius;
                    if (distSq > effRange * effRange)
                        return false;
                }
                return Condition == null || Condition(target);
            }
        }

        public Actor Player { get; init; }
        public Dictionary<ActionID, SupportedAction> SupportedActions { get; init; } = new();
        public int AutoAction { get; private set; }
        public float MaxCastTime { get; private set; }
        protected Autorotation Autorot;
        private DateTime _playerCombatStart;
        private DateTime _autoActionExpire;
        private bool _forceExpireAtCountdownCancel;
        private QuestLockCheck _lock;
        private ManualActionOverride _mq;

        public SupportedAction SupportedSpell<AID>(AID aid) where AID : Enum => SupportedActions[ActionID.MakeSpell(aid)];

        protected CommonActions(Autorotation autorot, Actor player, uint[] unlockData, Dictionary<ActionID, ActionDefinition> supportedActions)
        {
            Player = player;
            Autorot = autorot;
            foreach (var (aid, def) in supportedActions)
            {
                var a = SupportedActions[aid] = new(def, aid.IsGroundTargeted());
                if (def.Range == 0)
                    a.TransformTarget = _ => Player; // by default, all actions with range 0 should always target player
            }
            _lock = new(unlockData);
            _mq = new(autorot.Cooldowns, autorot.WorldState);
        }

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

            _mq.RemoveExpired();
            if (AutoAction != AutoActionNone && _autoActionExpire < Autorot.WorldState.CurrentTime)
            {
                Log($"Auto action {AutoAction} expired");
                AutoAction = AutoActionNone;
            }

            UpdateInternalState(AutoAction);
            if (AutoAction is > AutoActionNone and < AutoActionFirstCustom)
                QueueAIActions();
        }

        public unsafe bool HaveItemInInventory(uint id)
        {
            return FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(id % 1000000, id >= 1000000, false, false) > 0;
        }

        public float StatusDuration(DateTime expireAt)
        {
            return Math.Max((float)(expireAt - Autorot.WorldState.CurrentTime).TotalSeconds, 0.0f);
        }

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
            if (status != null)
                return (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF);
            return (0, 0);
        }
        public (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);

        // check whether specified status is a damage buff
        public bool IsDamageBuff(uint statusID)
        {
            // see https://i.redd.it/xrtgpras94881.png
            // TODO: AST card buffs?, enemy debuffs?, single-target buffs (DRG dragon sight, DNC devilment)
            return statusID switch
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
        }

        public void UpdateAutoAction(int autoAction, float maxCastTime)
        {
            if (AutoAction != autoAction)
            {
                Log($"Auto action set to {autoAction}");
                AutoAction = autoAction;
                _autoActionExpire = Autorot.Config.StickyAutoActions ? DateTime.MaxValue : Autorot.WorldState.CurrentTime.AddSeconds(1.0f);
            }
            else if (Autorot.Config.StickyAutoActions)
            {
                Log($"Turning off auto action {autoAction}");
                AutoAction = AutoActionNone;
                _autoActionExpire = new();
            }

            MaxCastTime = maxCastTime;
        }

        public bool HandleUserActionRequest(ActionID action, Actor? target, Vector3? forcedGTPos = null)
        {
            var supportedAction = SupportedActions.GetValueOrDefault(action);
            if (supportedAction == null)
                return false;

            //UpdateInternalState(AutoAction);
            if (supportedAction.TransformAction != null)
            {
                var adjAction = supportedAction.TransformAction();
                Service.Log($"Transform: {action} -> {adjAction}");
                if (adjAction != action)
                {
                    action = adjAction;
                    supportedAction = SupportedActions[adjAction];
                }
            }

            if (supportedAction.PlaceholderForAuto != AutoActionNone)
            {
                UpdateAutoAction(supportedAction.PlaceholderForAuto, float.MaxValue);
                return true;
            }

            // this is a manual action
            if (supportedAction.IsGT)
            {
                if (forcedGTPos != null)
                {
                    _mq.Push(action, null, forcedGTPos.Value, supportedAction.Definition, supportedAction.Condition);
                    return true;
                }

                if (ActionManagerEx.Instance!.Config.GTMode == ActionManagerConfig.GroundTargetingMode.Manual)
                    return false;

                if (ActionManagerEx.Instance!.Config.GTMode == ActionManagerConfig.GroundTargetingMode.AtCursor)
                {
                    var pos = ActionManagerEx.Instance!.GetWorldPosUnderCursor();
                    if (pos == null)
                        return false; // same as manual...
                    _mq.Push(action, null, pos.Value, supportedAction.Definition, supportedAction.Condition);
                    return true;
                }
            }

            if (supportedAction.TransformTarget != null)
            {
                target = supportedAction.TransformTarget(target);
            }
            _mq.Push(action, target, new(), supportedAction.Definition, supportedAction.Condition);
            return true;
        }

        public NextAction CalculateNextAction()
        {
            // check emergency mode
            var mqEmergency = _mq.PeekEmergency();
            if (mqEmergency != null)
                return new(mqEmergency.Action, mqEmergency.Target, mqEmergency.TargetPos, ActionSource.Emergency);

            var effAnimLock = Autorot.EffAnimLock;
            var animLockDelay = Autorot.AnimLockDelay;

            // see if we have any GCD (queued or automatic)
            var mqGCD = _mq.PeekGCD();
            var nextGCD = mqGCD != null ? new NextAction(mqGCD.Action, mqGCD.Target, mqGCD.TargetPos, ActionSource.Manual) : AutoAction != AutoActionNone ? CalculateAutomaticGCD() : new();
            float ogcdDeadline = nextGCD.Action ? Autorot.Cooldowns[CommonDefinitions.GCDGroup] : float.MaxValue;
            //Log($"{nextGCD.Action} = {ogcdDeadline}");

            // search for any oGCDs that we can execute without delaying GCD
            var mqOGCD = _mq.PeekOGCD(effAnimLock, animLockDelay, ogcdDeadline);
            if (mqOGCD != null)
                return new(mqOGCD.Action, mqOGCD.Target, mqOGCD.TargetPos, ActionSource.Manual);

            // see if there is anything high-priority from cooldown plan to be executed
            var cpActionHigh = Autorot.Hints.PlannedActions.FirstOrDefault(x => !x.lowPriority && CanExecutePlannedAction(x.action, x.target, effAnimLock, animLockDelay, ogcdDeadline));
            if (cpActionHigh.action)
                return new(cpActionHigh.action, cpActionHigh.target, new(), ActionSource.Planned);

            // note: we intentionally don't check that automatic oGCD really does not clip GCD - we provide utilities that allow module checking that, but also allow overriding if needed
            var nextOGCD = AutoAction != AutoActionNone ? CalculateAutomaticOGCD(ogcdDeadline) : new();
            if (nextOGCD.Action)
                return nextOGCD;

            // finally see whether there are any low-priority planned actions
            var cpActionLow = Autorot.Hints.PlannedActions.FirstOrDefault(x => x.lowPriority && CanExecutePlannedAction(x.action, x.target, effAnimLock, animLockDelay, ogcdDeadline));
            if (cpActionLow.action)
                return new(cpActionLow.action, cpActionLow.target, new(), ActionSource.Planned);

            return nextGCD;
        }

        public void NotifyActionExecuted(ClientActionRequest request)
        {
            Log($"Exec #{request.SourceSequence} {request.Action} @ {request.TargetID:X} [{GetState()}]");
            _mq.Pop(request.Action);
            Autorot.Bossmods.ActiveModule?.PlanExecution?.NotifyActionExecuted(Autorot.Bossmods.ActiveModule.StateMachine, request.Action);
            OnActionExecuted(request);
        }

        public void NotifyActionSucceeded(ActorCastEvent ev)
        {
            Log($"Cast #{ev.SourceSequence} {ev.Action} @ {ev.MainTargetID:X} [{GetState()}]");
            OnActionSucceeded(ev);
        }

        public abstract void Dispose();
        public abstract CommonRotation.PlayerState GetState();
        public abstract CommonRotation.Strategy GetStrategy();
        public virtual Targeting SelectBetterTarget(AIHints.Enemy initial) => new(initial);
        protected abstract void UpdateInternalState(int autoAction);
        protected abstract void QueueAIActions();
        protected abstract NextAction CalculateAutomaticGCD();
        protected abstract NextAction CalculateAutomaticOGCD(float deadline);
        protected virtual void OnActionExecuted(ClientActionRequest request) { }
        protected virtual void OnActionSucceeded(ActorCastEvent ev) { }

        protected NextAction MakeResult(ActionID action, Actor? target)
        {
            var data = action ? SupportedActions[action] : null;
            if (data == null)
                return new();
            if (data.Definition.Range == 0)
                target = Player; // override range-0 actions to always target player
            return target != null && data.Allowed(Player, target) ? new(action, target, new(), ActionSource.Automatic) : new();
        }
        protected NextAction MakeResult<AID>(AID aid, Actor? target) where AID : Enum => MakeResult(ActionID.MakeSpell(aid), target);

        protected void SimulateManualActionForAI(ActionID action, Actor? target, bool enable)
        {
            if (enable)
            {
                var data = SupportedActions[action];
                _mq.Push(action, target, new(), data.Definition, data.Condition, true);
            }
            else
            {
                _mq.Pop(action, true);
            }
        }

        // fill common state properties
        protected void FillCommonPlayerState(CommonRotation.PlayerState s)
        {
            var vuln = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextVulnerable(Autorot.Bossmods.ActiveModule.StateMachine) ?? (false, 10000);

            var am = ActionManagerEx.Instance!;
            var pc = Service.ClientState.LocalPlayer;
            s.Level = pc?.Level ?? 0;
            s.UnlockProgress = _lock.Progress();
            s.CurMP = Player.CurMP;
            s.TargetingEnemy = Autorot.PrimaryTarget != null && Autorot.PrimaryTarget.Type is ActorType.Enemy or ActorType.Part && !Autorot.PrimaryTarget.IsAlly;
            s.RangeToTarget = Autorot.PrimaryTarget != null ? (Autorot.PrimaryTarget.Position - Player.Position).Length() - Autorot.PrimaryTarget.HitboxRadius - Player.HitboxRadius : float.MaxValue;
            s.AnimationLock = am.EffectiveAnimationLock;
            s.AnimationLockDelay = am.EffectiveAnimationLockDelay;
            s.ComboTimeLeft = am.ComboTimeLeft;
            s.ComboLastAction = am.ComboLastMove;

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

        // smart targeting utility: return target (if friendly) or mouseover (if friendly) or null (otherwise)
        protected Actor? SmartTargetFriendly(Actor? primaryTarget)
        {
            if (primaryTarget?.Type is ActorType.Player or ActorType.Chocobo)
                return primaryTarget;

            if (Autorot.SecondaryTarget?.Type is ActorType.Player or ActorType.Chocobo)
                return Autorot.SecondaryTarget;

            return null;
        }

        // smart targeting utility: return mouseover (if hostile and allowed) or target (otherwise)
        protected Actor? SmartTargetHostile(Actor? primaryTarget)
        {
            if (Autorot.SecondaryTarget?.Type == ActorType.Enemy && !Autorot.SecondaryTarget.IsAlly)
                return Autorot.SecondaryTarget;

            return primaryTarget;
        }

        // smart targeting utility: return target (if friendly) or mouseover (if friendly) or other tank (if available) or null (otherwise)
        protected Actor? SmartTargetCoTank(Actor? primaryTarget)
        {
            return SmartTargetFriendly(primaryTarget) ?? Autorot.WorldState.Party.WithoutSlot().Exclude(Player).FirstOrDefault(a => a.Role == Role.Tank);
        }

        protected void Log(string message)
        {
            if (Autorot.Config.Logging)
                Service.Log($"[AR] [{GetType()}] {message}");
        }

        private bool CanExecutePlannedAction(ActionID action, Actor target, float effAnimLock, float animLockDelay, float deadline)
        {
            // TODO: planned GCDs?..
            var definition = SupportedActions.GetValueOrDefault(action);
            return definition != null
                && definition.Definition.CooldownGroup != CommonDefinitions.GCDGroup
                && Autorot.Cooldowns[definition.Definition.CooldownGroup] - effAnimLock <= definition.Definition.CooldownAtFirstCharge
                && effAnimLock + definition.Definition.AnimationLock + animLockDelay <= deadline
                && definition.Allowed(Player, target);
        }

        private float CombatTimer()
        {
            if (_playerCombatStart != default)
                return (float)(Autorot.WorldState.CurrentTime - _playerCombatStart).TotalSeconds;
            return -Math.Max(0.001f, Autorot.WorldState.Client.CountdownRemaining ?? float.MaxValue);
        }
    }
}
