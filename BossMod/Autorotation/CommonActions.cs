using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    abstract class CommonActions
    {
        public enum Positional { Any, Flank, Rear }

        // result of determining best 'AI' behaviour
        public struct AIResult
        {
            public ActionID Action;
            public Actor? Target;
            //public WPos? TargetPos; // for ground-targeted
            //public WPos PositionHint; // position where player should aim to be
            public Positional Positional;
        }

        private class SupportedAction
        {
            public ActionDefinition Definition;
            public Func<bool>? Condition;

            public SupportedAction(ActionDefinition definition)
            {
                Definition = definition;
            }
        }

        public Actor Player { get; init; }
        protected Autorotation Autorot;
        private Dictionary<ActionID, SupportedAction> _supportedActions = new();
        private ManualActionOverride _mq;

        protected unsafe CommonActions(Autorotation autorot, Actor player, Dictionary<ActionID, ActionDefinition> supportedActions)
        {
            Player = player;
            Autorot = autorot;
            foreach (var (aid, def) in supportedActions)
                _supportedActions[aid] = new(def);
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

        public (ActionID, ulong) CalculateNextAction(Actor? target, bool moving, float animLockDelay)
        {
            //var am = ActionManagerEx.Instance!;
            //if ()
            //{
            //    // we're under animation lock or are casting - just do nothing for now
            //    return (new(), 0);
            //}

            // see if there is any action from manual queue that is to be executed
            var mqAction = _mq.Pop(animLockDelay);
            if (mqAction.Action)
            {
                return (mqAction.Action, mqAction.Target);
            }
            if (mqAction.Emergency)
            {
                // we are waiting for emergency action to come off cooldown, do nothing for now
                return (new(), 0);
            }

            // see if there is anything from cooldown plan to be executed
            var cooldownPlan = Autorot.Bossmods.ActiveModule?.PlanExecution;
            if (cooldownPlan != null)
            {
                // TODO: support non-self targeting
                // TODO: support custom conditions in planner
                var (cpAction, cpTimeLeft) = cooldownPlan.ActiveActions(Autorot.Bossmods.ActiveModule!.StateMachine).Where(x => CanExecutePlannedAction(x.Action, animLockDelay)).MinBy(x => x.TimeLeft);
                if (cpAction)
                {
                    return (cpAction, Player.InstanceID);
                }
            }

            // finally, let module determine best action
            return CalculateNextAutomaticAction(target, moving);
        }

        public (ActionID, ulong) ReplaceActionAndTarget(ActionID actionID, ulong targetID, bool forced)
        {
            var targets = new Targets() { MainTarget = targetID, MouseoverTarget = Mouseover.Instance?.Object?.ObjectId ?? 0 };
            if (!forced)
            {
                if (Autorot.Config.SmartCooldownQueueing && _sq.Active)
                {
                    var e = _sq.Entries.GetValueOrDefault(actionID);
                    if (e != null)
                    {
                        Log($"Smart-queueing {actionID} @ {targets.MainTarget:X} / {targets.MouseoverTarget:X}");
                        e.ActivateManual(Autorot.WorldState.CurrentTime, targets);
                        (actionID, targets) = _sq.Replacement;
                    }
                }
                _sq.Replacement = (actionID, targets); // if we're not smart-queueing, update replacement to last action
            }

            return DoReplaceActionAndTarget(actionID, targets);
        }

        abstract protected (ActionID, ulong) CalculateNextAutomaticAction(Actor? target, bool moving);

        abstract protected (ActionID, ulong) DoReplaceActionAndTarget(ActionID actionID, Targets targets);
        abstract public AIResult CalculateBestAction(Actor player, Actor? primaryTarget, bool moving);
        abstract public void DrawOverlay();

        // fill common state properties
        protected unsafe void FillCommonPlayerState(CommonRotation.PlayerState s, Actor? target, ActionID potion)
        {
            var am = ActionManagerEx.Instance!;
            var pc = Service.ClientState.LocalPlayer;
            s.Level = pc?.Level ?? 0;
            s.CurMP = pc?.CurrentMp ?? 0;
            s.AnimationLock = am.AnimationLock;
            s.AnimationLockDelay = am.AnimationLockDelayAverage;
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
            strategy.Potion = SmartQueueActive(potion) ? CommonRotation.Strategy.PotionUse.Immediate : Autorot.Config.PotionUse;
            if (strategy.Potion != CommonRotation.Strategy.PotionUse.Manual && !HaveItemInInventory(potion.ID)) // don't try to use potions if player doesn't have any
                strategy.Potion = CommonRotation.Strategy.PotionUse.Manual;
            strategy.ExecuteSprint = SmartQueueActive(CommonDefinitions.IDSprint);
        }

        // register new smart-queueable action
        protected void SmartQueueRegister(ActionID action) => _sq.Entries[action] = new();
        protected void SmartQueueRegisterSpell<AID>(AID spell) where AID : Enum => SmartQueueRegister(ActionID.MakeSpell(spell));

        // check whether smart-queueable action is queued
        protected bool SmartQueueActive(ActionID action) => _sq.Entries.GetValueOrDefault(action)?.Active(Autorot.WorldState.CurrentTime) ?? false;
        protected bool SmartQueueActiveSpell<AID>(AID spell) where AID : Enum => SmartQueueActive(ActionID.MakeSpell(spell));

        // get smart-queue target, if available; otherwise (if smart-queue is inactive or if it was planned without specific target) return fallback (current targets)
        protected Targets SmartQueueTarget(ActionID action, Targets fallback)
        {
            var e = _sq.Entries.GetValueOrDefault(action);
            return e != null && e.Active(Autorot.WorldState.CurrentTime) && e.Targets != null ? e.Targets.Value : fallback;
        }
        protected Targets SmartQueueTargetSpell<AID>(AID spell, Targets fallback) where AID : Enum => SmartQueueTarget(ActionID.MakeSpell(spell), fallback);

        // deactivate smart-queue entry
        protected void SmartQueueDeactivate(ActionID action) => _sq.Entries.GetValueOrDefault(action)?.Deactivate();

        // smart targeting utility: return target (if friendly) or mouseover (if friendly and allowed) or null (otherwise)
        protected Actor? SmartTargetFriendly(Targets targets, bool allowMouseover)
        {
            var target = Autorot.WorldState.Actors.Find(targets.MainTarget);
            if (target?.Type is ActorType.Player or ActorType.Chocobo)
                return target;

            if (allowMouseover)
            {
                target = Autorot.WorldState.Actors.Find(targets.MouseoverTarget);
                if (target?.Type is ActorType.Player or ActorType.Chocobo)
                    return target;
            }

            return null;
        }

        // smart targeting utility: return mouseover (if hostile and allowed) or target (otherwise)
        protected Actor? SmartTargetHostile(ActionID action, Targets targets, bool allowMouseover)
        {
            targets = SmartQueueTarget(action, targets);
            if (allowMouseover)
            {
                var target = Autorot.WorldState.Actors.Find(targets.MouseoverTarget);
                if (target?.Type == ActorType.Enemy && !target.IsAlly)
                    return target;
            }

            return Autorot.WorldState.Actors.Find(targets.MainTarget);
        }

        // smart targeting utility: return target (if friendly) or mouseover (if friendly and allowed) or other tank (if available and allowed) or null (otherwise)
        protected Actor? SmartTargetCoTank(ActionID action, Targets targets, bool allow)
        {
            targets = SmartQueueTarget(action, targets);
            var target = SmartTargetFriendly(targets, allow);
            if (target != null)
                return target;

            if (allow)
            {
                target = Autorot.WorldState.Party.WithoutSlot().Exclude(Player).FirstOrDefault(a => a.Role == Role.Tank);
                if (target != null)
                    return target;
            }

            // can't find good target, deactivate smart-queue entry to prevent silly spam
            Log($"Smart-target failed, removing from queue");
            SmartQueueDeactivate(action);
            return null;
        }

        protected void Log(string message)
        {
            if (Autorot.Config.Logging)
                Service.Log($"[AR] [{GetType().Name}] {message}");
        }

        private bool CanExecutePlannedAction(ActionID action, float animLockDelay)
        {
            var data = _supportedActions[action];
            return Autorot.Cooldowns[data.Definition.CooldownGroup] <= data.Definition.CooldownAtFirstCharge
                && (data.Definition.CooldownGroup == CommonDefinitions.GCDGroup || data.Definition.AnimationLock + animLockDelay < Autorot.Cooldowns[CommonDefinitions.GCDGroup])
                && (data.Condition == null || data.Condition());
        }
    }
}
