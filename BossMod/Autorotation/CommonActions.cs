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

        // all relevant target IDs used for smart target selection
        protected struct Targets
        {
            public ulong MainTarget;
            public ulong MouseoverTarget;
        }

        // 'smart oGCD queueing': when using auto-rotation, pressing manual ogcds is slightly problematic:
        // 1. it might delay next GCD, which is bad for damage
        // 2. it might require awkward spamming, otherwise returning to spamming rotation button might override it with rotational ogcd
        // smart queue solves it: if we press supported oGCD action while under gcd or animation lock, we 'queue' it and return fallback action, which is then passed to normal replace function
        // replace function should then choose best replacement, taking active smart queue entries into account
        // when activated, smart queue accepts an argument specifying its validity time; default value corresponds to manual presses, planned values provide custom activation windows
        // validity time for manual presses is needed to limit the effect of e.g. misclick while ability has long cooldown
        protected class SmartQueue
        {
            public class Entry
            {
                public DateTime Expire;
                public Targets? Targets; // null for planned activation

                public bool Active(DateTime now) => now < Expire;

                public void ActivateManual(DateTime now, Targets targets)
                {
                    var newExpire = now.AddSeconds(3);
                    if (newExpire > Expire)
                        Expire = newExpire;
                    Targets = targets;
                }

                public void ActivatePlanned(DateTime now, float window)
                {
                    var newExpire = now.AddSeconds(window);
                    if (newExpire > Expire)
                        Expire = newExpire;
                    // don't touch targets, if they were manually requested
                }

                public void Deactivate()
                {
                    Expire = new();
                    Targets = null;
                }
            }

            public bool Active; // if active, actions are queued and replacement is returned; otherwise smart-queue is transparent
            public (ActionID, Targets) Replacement; // action + target that will be returned instead of smart-queued action
            public Dictionary<ActionID, Entry> Entries = new(); // key = smart-queueable action, value = expiration timestamp + target
        }

        public ActionID BaseAbility { get; init; } // GCD ability acquired at level 1, that is used as a base for single target rotations
        public Actor Player { get; init; }
        protected Autorotation Autorot;
        private SmartQueue _sq = new();
        private unsafe FFXIVClientStructs.FFXIV.Client.Game.ActionManager* _actionManager = null;

        protected unsafe CommonActions(Autorotation autorot, Actor player, ActionID baseAbility)
        {
            BaseAbility = baseAbility;
            Player = player;
            Autorot = autorot;
            _actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        }

        public unsafe float ActionCooldown(ActionID action)
        {
            var recastGroup = _actionManager->GetRecastGroup((int)action.Type, action.ID);
            var recast = _actionManager->GetRecastGroupDetail(recastGroup);
            if (recast != null)
            {
                return recast->Total - recast->Elapsed;
            }
            else
            {
                Service.Log($"Failed to retrieve recast for {action} (group={recastGroup})");
                return 0;
            }
        }

        public float SpellCooldown<AID>(AID spell) where AID : Enum
        {
            return ActionCooldown(ActionID.MakeSpell(spell));
        }

        public unsafe float PotionCooldown()
        {
            // note: potions have recast group 58; however, for some reason periodically GetRecastGroup for them returns -1...
            var recast = _actionManager->GetRecastGroupDetail(58);
            return recast->Total - recast->Elapsed;
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

        public void CastSucceeded(ActionID actionID, ulong targetID)
        {
            SmartQueueDeactivate(actionID);
            OnCastSucceeded(actionID, targetID);
        }

        public void Update(Actor? target)
        {
            // cooldown planning
            var cooldownPlan = Autorot.Bossmods.ActiveModule?.PlanExecution;
            if (cooldownPlan != null)
            {
                var stateData = cooldownPlan.FindStateData(Autorot.Bossmods.ActiveModule?.StateMachine.ActiveState);
                foreach (var (action, entry) in _sq.Entries)
                {
                    var plan = stateData?.Abilities.GetValueOrDefault(action);
                    if (plan != null)
                    {
                        var progress = Autorot.Bossmods.ActiveModule!.StateMachine.TimeSinceTransitionClamped;
                        var activeWindow = plan.ActivationWindows.FindIndex(w => w.Start <= progress && w.End > progress);
                        if (activeWindow != -1)
                        {
                            entry.ActivatePlanned(Autorot.WorldState.CurrentTime, plan.ActivationWindows[activeWindow].End - progress);
                        }
                    }
                }
            }

            var state = OnUpdate(target);
            _sq.Active = state.GCD > 0 || state.AnimationLock > 0;
        }

        public (ActionID, ulong) ReplaceActionAndTarget(ActionID actionID, ulong targetID)
        {
            var targets = new Targets() { MainTarget = targetID, MouseoverTarget = Mouseover.Instance?.Object?.ObjectId ?? 0 };
            if (Autorot.Config.SmartCooldownQueueing && _sq.Active)
            {
                var e = _sq.Entries.GetValueOrDefault(actionID);
                if (e != null)
                {
                    Log($"Smart-queueing {actionID} @ {targetID:X}");
                    e.ActivateManual(Autorot.WorldState.CurrentTime, targets);
                    (actionID, targets) = _sq.Replacement;
                }
            }
            _sq.Replacement = (actionID, targets); // if we're not smart-queueing, update replacement to last action

            return DoReplaceActionAndTarget(actionID, targets);
        }

        abstract protected void OnCastSucceeded(ActionID actionID, ulong targetID);
        abstract protected CommonRotation.State OnUpdate(Actor? target);
        abstract protected (ActionID, ulong) DoReplaceActionAndTarget(ActionID actionID, Targets targets);
        abstract public AIResult CalculateBestAction(Actor player, Actor? primaryTarget);
        abstract public void DrawOverlay();

        // fill common state properties
        protected void FillCommonState(CommonRotation.State s, Actor? target, ActionID potion)
        {
            var pc = Service.ClientState.LocalPlayer;
            s.Level = pc?.Level ?? 0;
            s.CurMP = pc?.CurrentMp ?? 0;
            s.Moving = Autorot.Moving;
            s.GCD = ActionCooldown(BaseAbility);
            s.AnimationLock = Autorot.AnimLock;
            s.AnimationLockDelay = Autorot.AnimLockDelay;
            s.ComboTimeLeft = Autorot.ComboTimeLeft;
            s.ComboLastAction = Autorot.ComboLastMove;

            foreach (var status in Player.Statuses.Where(s => IsDamageBuff(s.ID)))
            {
                s.RaidBuffsLeft = MathF.Max(s.RaidBuffsLeft, StatusDuration(status.ExpireAt));
            }
            // TODO: also check damage-taken debuffs on target

            s.SprintCD = ActionCooldown(CommonRotation.IDSprint);
            s.PotionCD = PotionCooldown();
        }

        // fill common strategy properties
        protected void FillCommonStrategy(CommonRotation.Strategy strategy, ActionID potion)
        {
            strategy.Prepull = !Player.InCombat;
            strategy.FightEndIn = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextDowntime(Autorot.Bossmods.ActiveModule?.StateMachine) ?? 0;
            strategy.RaidBuffsIn = Autorot.Bossmods.ActiveModule?.PlanConfig != null ? Autorot.Bossmods.RaidCooldowns.NextDamageBuffIn(Autorot.WorldState.CurrentTime) : 10000; // assumption: if there is no planning support for encounter (meaning it's something trivial, like outdoor boss), don't expect any cooldowns
            strategy.PositionLockIn = Autorot.Config.EnableMovement ? (Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextPositioning(Autorot.Bossmods.ActiveModule?.StateMachine) ?? 10000) : 0;
            strategy.Potion = SmartQueueActive(potion) ? CommonRotation.Strategy.PotionUse.Immediate : Autorot.Config.PotionUse;
            if (strategy.Potion != CommonRotation.Strategy.PotionUse.Manual && !HaveItemInInventory(potion.ID)) // don't try to use potions if player doesn't have any
                strategy.Potion = CommonRotation.Strategy.PotionUse.Manual;
            strategy.ExecuteSprint = SmartQueueActive(CommonRotation.IDSprint);
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
    }
}
