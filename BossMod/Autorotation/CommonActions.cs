using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    abstract class CommonActions : IDisposable
    {
        public enum Positional { Any, Flank, Rear }
        public enum ActionSource { Automatic, Planned, Manual, ManualEmergency }

        public struct NextAction
        {
            public ActionID Action;
            public ulong TargetID;
            public Vector3 TargetPos;
            public ActionSource Source;
        }

        public class SupportedAction
        {
            public ActionDefinition Definition;
            public bool IsGT;
            public Func<ulong, bool>? Condition;
            public AutoAction PlaceholderForStrategy; // if set, attempting to execute this action would instead initiate auto-strategy
            public Func<ActionID>? TransformAction;
            public Func<ulong, ulong>? TransformTarget;

            public bool Allowed(ulong targetID) => Condition == null || Condition(targetID);

            public SupportedAction(ActionDefinition definition, bool isGT)
            {
                Definition = definition;
                IsGT = isGT;
            }
        }

        public Actor Player { get; init; }
        public Dictionary<ActionID, SupportedAction> SupportedActions { get; init; } = new();
        public Positional PreferredPosition { get; protected set; } // implementation can update this as needed
        protected Autorotation Autorot;
        protected bool AllowGCDDelay; // implementation can set this flag if desired
        private QuestLockCheck _lock;
        private ManualActionOverride _mq;
        private AutoAction _autoStrategy;
        private DateTime _autoStrategyExpire;

        public SupportedAction SupportedSpell<AID>(AID aid) where AID : Enum => SupportedActions[ActionID.MakeSpell(aid)];

        protected unsafe CommonActions(Autorotation autorot, Actor player, QuestLockEntry[] unlockData, Dictionary<ActionID, ActionDefinition> supportedActions)
        {
            Player = player;
            Autorot = autorot;
            foreach (var (aid, def) in supportedActions)
                SupportedActions[aid] = new(def, aid.IsGroundTargeted());
            _lock = new(unlockData);
            _mq = new(autorot.Cooldowns, autorot.WorldState);
        }

        public unsafe bool HaveItemInInventory(uint id)
        {
            return FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(id % 1000000, id >= 1000000, false, false) > 0;
        }

        public float StatusDuration(DateTime expireAt)
        {
            return Math.Max((float)(expireAt - Autorot.WorldState.CurrentTime).TotalSeconds, 0.0f);
        }

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

        public void UpdateAutoStrategy(AutoAction strategy)
        {
            if (_autoStrategy != strategy)
                Service.Log($"[ARCA] Strategy set to {strategy}");
            _autoStrategy = strategy;
            _autoStrategyExpire = Autorot.WorldState.CurrentTime.AddSeconds(1.0f);
        }

        public bool HandleUserActionRequest(ActionID action, ulong targetID, AutorotationConfig.GroundTargetingMode gtMode)
        {
            var supportedAction = SupportedActions.GetValueOrDefault(action);
            if (supportedAction == null)
                return false;

            if (supportedAction.TransformAction != null)
            {
                var adjAction = supportedAction.TransformAction();
                if (adjAction != action)
                {
                    action = adjAction;
                    supportedAction = SupportedActions[adjAction];
                }
            }

            if (supportedAction.PlaceholderForStrategy != AutoAction.None)
            {
                UpdateAutoStrategy(supportedAction.PlaceholderForStrategy);
                return true;
            }

            // this is a manual action
            if (supportedAction.IsGT)
            {
                if (gtMode == AutorotationConfig.GroundTargetingMode.Manual)
                    return false;

                if (gtMode == AutorotationConfig.GroundTargetingMode.AtCursor)
                {
                    var pos = ActionManagerEx.Instance!.GetWorldPosUnderCursor();
                    if (pos == null)
                        return false; // same as manual...
                    _mq.Push(action, 0, pos.Value, supportedAction.Definition, supportedAction.Condition);
                    return true;
                }
            }

            if (supportedAction.TransformTarget != null)
            {
                targetID = supportedAction.TransformTarget(targetID);
            }
            _mq.Push(action, targetID, new(), supportedAction.Definition, supportedAction.Condition);
            return true;
        }

        // delay is 0 if we're getting an action to use, otherwise it can be larger (e.g. if we're showing next-action hint during animation lock)
        public NextAction CalculateNextAction(float effAnimLock, bool moving, float animLockDelay)
        {
            // see if there is any action from manual queue that is to be executed
            bool allowGCDDelay = AllowGCDDelay || _autoStrategy == AutoAction.None;
            var mqAction = _mq.Peek(effAnimLock, animLockDelay, allowGCDDelay);
            if (mqAction.Action)
            {
                return new() { Action = mqAction.Action, TargetID = mqAction.TargetID, TargetPos = mqAction.TargetPos, Source = mqAction.Emergency ? ActionSource.ManualEmergency : ActionSource.Manual };
            }
            if (mqAction.Emergency)
            {
                // we are waiting for emergency action to come off cooldown, do nothing for now
                return new() { Source = ActionSource.ManualEmergency };
            }

            // see if there is anything from cooldown plan to be executed
            var cooldownPlan = Autorot.Bossmods.ActiveModule?.PlanExecution;
            if (cooldownPlan != null)
            {
                // TODO: support non-self targeting
                // TODO: support custom conditions in planner
                var planTargetID = Player.InstanceID;
                var (cpAction, cpTimeLeft) = cooldownPlan.ActiveActions(Autorot.Bossmods.ActiveModule!.StateMachine).Where(x => CanExecutePlannedAction(x.Action, planTargetID, effAnimLock, animLockDelay, allowGCDDelay)).MinBy(x => x.TimeLeft);
                if (cpAction)
                {
                    return new() { Action = cpAction, TargetID = planTargetID, Source = ActionSource.Planned };
                }
            }

            // let module determine best action according to current strategy
            if (_autoStrategy != AutoAction.None && _autoStrategyExpire < Autorot.WorldState.CurrentTime)
            {
                Service.Log("[ARCA] Strategy expired");
                _autoStrategy = AutoAction.None;
            }
            if (_autoStrategy == AutoAction.None)
                return new(); // nothing to do...

            // TODO: reconsider...
            var autoAction = CalculateNextAutomaticAction(_autoStrategy, GetCurrentTargetID(), effAnimLock, moving, animLockDelay);
            if (!autoAction.Action)
                return new();

            var data = SupportedActions[autoAction.Action];
            bool ready = Autorot.Cooldowns[data.Definition.CooldownGroup] - effAnimLock <= data.Definition.CooldownAtFirstCharge;
            return ready ? autoAction : new();
        }

        public void NotifyActionExecuted(ActionID action, ulong target)
        {
            _mq.Pop(action);
        }

        public void DrawOverlay()
        {
            var am = ActionManagerEx.Instance!;
            var effLock = am.AnimationLock + am.CastTimeRemaining;
            var animLockDelay = MathF.Min(MathF.Min(am.AnimationLockDelayMax, am.AnimationLockDelayAverage), 0.1f);
            var next = CalculateNextAction(effLock, false, animLockDelay);
            ImGui.TextUnformatted($"Next: {next.Action} ({next.Source})");
            if (_autoStrategy != AutoAction.None)
                ImGui.TextUnformatted($"Next auto: {CalculateNextAutomaticAction(_autoStrategy, GetCurrentTargetID(), effLock, false, animLockDelay).Action}");
            //ImGui.TextUnformatted(_strategy.ToString());
            //ImGui.TextUnformatted($"Raidbuffs: {_state.RaidBuffsLeft:f2}s left, next in {_strategy.RaidBuffsIn:f2}s");
            //ImGui.TextUnformatted($"Downtime: {_strategy.FightEndIn:f2}s, pos-lock: {_strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={Autorot.Cooldowns[CommonDefinitions.GCDGroup]:f3}, AnimLock={am.AnimationLock + am.CastTimeRemaining:f3}+{animLockDelay:f3}");
        }

        public abstract void Dispose();
        public abstract NextAction CalculateNextAutomaticAction(AutoAction strategy, ulong primaryTargetID, float effAnimLock, bool moving, float animLockDelay);

        // fill common state properties
        protected unsafe void FillCommonPlayerState(CommonRotation.PlayerState s, float effAnimLock, float animLockDelay)
        {
            var am = ActionManagerEx.Instance!;
            var pc = Service.ClientState.LocalPlayer;
            s.Level = _lock.AdjustLevel(pc?.Level ?? 0);
            s.CurMP = pc?.CurrentMp ?? 0;
            s.AnimationLock = effAnimLock;
            s.AnimationLockDelay = animLockDelay;
            s.ComboTimeLeft = am.ComboTimeLeft;
            s.ComboLastAction = am.ComboLastMove;

            foreach (var status in Player.Statuses.Where(s => IsDamageBuff(s.ID)))
            {
                s.RaidBuffsLeft = MathF.Max(s.RaidBuffsLeft, StatusDuration(status.ExpireAt));
            }
            // TODO: also check damage-taken debuffs on target
        }

        // fill common strategy properties
        protected void FillCommonStrategy(CommonRotation.Strategy strategy, ActionID potion)
        {
            strategy.Prepull = !Player.InCombat;
            strategy.FightEndIn = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextDowntime(Autorot.Bossmods.ActiveModule?.StateMachine) ?? 0;
            strategy.RaidBuffsIn = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextVulnerable(Autorot.Bossmods.ActiveModule?.StateMachine) ?? 10000;
            if (Autorot.Bossmods.ActiveModule?.PlanConfig != null) // assumption: if there is no planning support for encounter (meaning it's something trivial, like outdoor boss), don't expect any cooldowns
                strategy.RaidBuffsIn = Math.Min(strategy.RaidBuffsIn, Autorot.Bossmods.RaidCooldowns.NextDamageBuffIn(Autorot.WorldState.CurrentTime));
            strategy.PositionLockIn = Autorot.Config.EnableMovement ? (Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextPositioning(Autorot.Bossmods.ActiveModule?.StateMachine) ?? 10000) : 0;
            strategy.Potion = Autorot.Config.PotionUse;
            if (strategy.Potion != CommonRotation.Strategy.PotionUse.Manual && !HaveItemInInventory(potion.ID)) // don't try to use potions if player doesn't have any
                strategy.Potion = CommonRotation.Strategy.PotionUse.Manual;
        }

        // smart targeting utility: return target (if friendly) or mouseover (if friendly) or null (otherwise)
        protected ulong SmartTargetFriendly(ulong primaryTarget)
        {
            var target = Autorot.WorldState.Actors.Find(primaryTarget);
            if (target?.Type is ActorType.Player or ActorType.Chocobo)
                return primaryTarget;

            target = Autorot.WorldState.Actors.Find(Mouseover.Instance?.Object?.ObjectId ?? 0);
            if (target?.Type is ActorType.Player or ActorType.Chocobo)
                return target.InstanceID;

            return 0;
        }

        // smart targeting utility: return mouseover (if hostile and allowed) or target (otherwise)
        protected ulong SmartTargetHostile(ulong primaryTarget)
        {
            var target = Autorot.WorldState.Actors.Find(Mouseover.Instance?.Object?.ObjectId ?? 0);
            if (target?.Type == ActorType.Enemy && !target.IsAlly)
                return target.InstanceID;

            return primaryTarget;
        }

        // smart targeting utility: return target (if friendly) or mouseover (if friendly) or other tank (if available) or null (otherwise)
        protected ulong SmartTargetCoTank(ulong primaryTarget)
        {
            var target = SmartTargetFriendly(primaryTarget);
            if (target != 0)
                return target;
            return Autorot.WorldState.Party.WithoutSlot().Exclude(Player).FirstOrDefault(a => a.Role == Role.Tank)?.InstanceID ?? 0;
        }

        protected void Log(string message)
        {
            if (Autorot.Config.Logging)
                Service.Log($"[AR] [{GetType().Name}] {message}");
        }

        private unsafe ulong GetCurrentTargetID()
        {
            // this emulates TargetSystem.GetCurrentTargetID, however it correctly preserves bit in high dword (TODO consider just fixing stuff...)
            var targetSystem = FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance();
            if (targetSystem->SoftTarget != null)
                return (ulong)(long)targetSystem->SoftTarget->GetObjectID();
            if (targetSystem->Target != null)
                return (ulong)(long)targetSystem->Target->GetObjectID();
            return GameObject.InvalidGameObjectId;
        }

        private bool CanExecutePlannedAction(ActionID action, ulong targetID, float delay, float animLockDelay, bool allowGCDDelay)
        {
            var data = SupportedActions[action];
            var gcd = Autorot.Cooldowns[CommonDefinitions.GCDGroup] - delay;
            if (data.Definition.CooldownGroup == CommonDefinitions.GCDGroup)
            {
                // TODO: how exactly should we support planned GCDs?..
                return gcd <= 0 && data.Allowed(targetID);
            }
            else
            {
                return (allowGCDDelay || data.Definition.AnimationLock + animLockDelay <= gcd) && data.Allowed(targetID);
            }
        }
    }
}
