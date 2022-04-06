using Dalamud.Game.ClientState.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    abstract class CommonActions
    {
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
                public uint Target; // 0 for planned activation

                public bool Active => DateTime.Now < Expire;
                public void Activate(uint target, float expire = 3)
                {
                    var newExpire = DateTime.Now.AddSeconds(expire);
                    if (newExpire > Expire)
                    {
                        Expire = newExpire;
                        Target = target;
                    }
                }
                public void Deactivate() => Expire = new();
            }

            public bool Active; // if active, actions are queued and replacement is returned; otherwise smart-queue is transparent
            public (ActionID, uint) Replacement; // action + target that will be returned instead of smart-queued action
            public Dictionary<ActionID, Entry> Entries = new(); // key = smart-queueable action, value = expiration timestamp + target
        }

        protected Autorotation Autorot;
        private SmartQueue _sq = new();
        private unsafe FFXIVClientStructs.FFXIV.Client.Game.ActionManager* _actionManager = null;

        protected unsafe CommonActions(Autorotation autorot)
        {
            Autorot = autorot;
            _actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        }

        public unsafe float ActionCooldown(ActionID action)
        {
            var type = (FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type;
            var id = action.ID;
            return _actionManager->GetRecastTime(type, id) - _actionManager->GetRecastTimeElapsed(type, id);
        }

        public float SpellCooldown<AID>(AID spell) where AID : Enum
        {
            return ActionCooldown(ActionID.MakeSpell(spell));
        }

        public unsafe bool HaveItemInInventory(uint id)
        {
            return FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(id % 1000000, id >= 1000000, false, false) > 0;
        }

        public float StatusDuration(float dur)
        {
            // note: when buff is applied, it has large negative duration, and it is updated to correct one ~0.6sec later
            return MathF.Abs(dur);
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

        public void CastSucceeded(ActionID actionID)
        {
            SmartQueueDeactivate(actionID);
            OnCastSucceeded(actionID);
        }

        public void Update()
        {
            // cooldown planning
            var activeState = Autorot.Bossmods.ActiveModule?.StateMachine.ActiveState;
            var cooldownPlan = Autorot.Bossmods.ActiveModule?.CurrentCooldownPlan;
            if (cooldownPlan != null && activeState != null)
            {
                foreach (var (action, entry) in _sq.Entries)
                {
                    var plan = cooldownPlan.PlanAbilities.GetValueOrDefault(action.Raw);
                    if (plan != null)
                    {
                        foreach (var e in plan.Where(e => e.StateID == activeState.ID && Autorot.Bossmods.ActiveModule!.StateMachine.TimeSinceTransition >= e.TimeSinceActivation))
                        {
                            var windowLeft = e.WindowLength - (Autorot.Bossmods.ActiveModule!.StateMachine.TimeSinceTransition - e.TimeSinceActivation);
                            if (windowLeft > 0)
                                entry.Activate(0, windowLeft);
                        }
                    }
                }
            }

            var state = OnUpdate();
            _sq.Active = state.GCD > 0 || state.AnimationLock > 0;
        }

        public (ActionID, uint) ReplaceActionAndTarget(ActionID actionID, uint targetID)
        {
            if (Autorot.Config.SmartCooldownQueueing && _sq.Active)
            {
                var e = _sq.Entries.GetValueOrDefault(actionID);
                if (e != null)
                {
                    Log($"Smart-queueing {actionID} @ {targetID:X}");
                    e.Activate(targetID);
                    (actionID, targetID) = _sq.Replacement;
                }
            }
            _sq.Replacement = (actionID, targetID); // if we're not smart-queueing, update replacement to last action

            return DoReplaceActionAndTarget(actionID, targetID);
        }

        abstract protected void OnCastSucceeded(ActionID actionID);
        abstract protected CommonRotation.State OnUpdate();
        abstract protected (ActionID, uint) DoReplaceActionAndTarget(ActionID actionID, uint targetID);
        abstract public void DrawOverlay();

        // fill common state properties
        protected void FillCommonState<AID>(CommonRotation.State s, AID gcdSpell, ActionID potion) where AID : Enum
        {
            if (Service.ClientState.LocalPlayer == null)
                return;

            s.Level = Service.ClientState.LocalPlayer.Level;
            s.CurMP = Service.ClientState.LocalPlayer.CurrentMp;
            s.Moving = Autorot.Moving;
            s.GCD = SpellCooldown(gcdSpell);
            s.AnimationLock = Autorot.AnimLock;
            s.AnimationLockDelay = Autorot.AnimLockDelay;
            s.ComboTimeLeft = Autorot.ComboTimeLeft;
            s.ComboLastAction = Autorot.ComboLastMove;

            foreach (var status in Service.ClientState.LocalPlayer.StatusList.Where(s => IsDamageBuff(s.StatusId)))
            {
                s.RaidBuffsLeft = MathF.Max(s.RaidBuffsLeft, StatusDuration(status.RemainingTime));
            }
            // TODO: also check damage-taken debuffs on target

            s.SprintCD = ActionCooldown(CommonRotation.IDSprint);
            s.PotionCD = ActionCooldown(potion);
        }

        // fill common strategy properties
        protected void FillCommonStrategy(CommonRotation.Strategy strategy, ActionID potion)
        {
            if (Service.ClientState.LocalPlayer == null)
                return;

            strategy.Prepull = !Service.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat);
            strategy.FightEndIn = Autorot.Bossmods.ActiveModule?.StateMachine.EstimateTimeToNextDowntime() ?? 0;
            strategy.RaidBuffsIn = Autorot.Bossmods.RaidCooldowns.NextDamageBuffIn(Autorot.Bossmods.WorldState.CurrentTime);
            strategy.PositionLockIn = Autorot.Config.EnableMovement ? (Autorot.Bossmods.ActiveModule?.StateMachine.EstimateTimeToNextPositioning() ?? 10000) : 0;
            strategy.Potion = SmartQueueActive(potion) ? CommonRotation.Strategy.PotionUse.Immediate : Autorot.Config.PotionUse;
            if (strategy.Potion != CommonRotation.Strategy.PotionUse.Manual && !HaveItemInInventory(potion.ID)) // don't try to use potions if player doesn't have any
                strategy.Potion = CommonRotation.Strategy.PotionUse.Manual;
            strategy.ExecuteSprint = SmartQueueActive(CommonRotation.IDSprint);
        }

        // register new smart-queueable action
        protected void SmartQueueRegister(ActionID action) => _sq.Entries[action] = new();
        protected void SmartQueueRegisterSpell<AID>(AID spell) where AID : Enum => SmartQueueRegister(ActionID.MakeSpell(spell));

        // check whether smart-queueable action is queued
        protected bool SmartQueueActive(ActionID action) => _sq.Entries.GetValueOrDefault(action)?.Active ?? false;
        protected bool SmartQueueActiveSpell<AID>(AID spell) where AID : Enum => SmartQueueActive(ActionID.MakeSpell(spell));

        // get smart-queue target, if available; otherwise (if smart-queue is inactive or if it was planned without specific target) return fallback (current target probably)
        protected uint SmartQueueTarget(ActionID action, uint fallback)
        {
            var e = _sq.Entries.GetValueOrDefault(action);
            return e != null && e.Active && e.Target != 0 ? e.Target : fallback;
        }
        protected uint SmartQueueTargetSpell<AID>(AID spell, uint fallback) where AID : Enum => SmartQueueTarget(ActionID.MakeSpell(spell), fallback);

        // deactivate smart-queue entry
        protected void SmartQueueDeactivate(ActionID action) => _sq.Entries.GetValueOrDefault(action)?.Deactivate();

        // smart targeting utility: return target (if friendly) or mouseover (if friendly and allowed) or null (otherwise)
        protected Actor? SmartTargetFriendly(uint targetID, bool allowMouseover)
        {
            var target = Autorot.Bossmods.WorldState.Actors.Find(targetID);
            if (target?.Type is ActorType.Player or ActorType.Chocobo)
                return target;

            if (allowMouseover)
            {
                target = Autorot.Bossmods.WorldState.Actors.Find(Mouseover.Instance?.Object?.ObjectId ?? 0);
                if (target?.Type is ActorType.Player or ActorType.Chocobo)
                    return target;
            }

            return null;
        }

        protected void Log(string message)
        {
            if (Autorot.Config.Logging)
                Service.Log($"[AR] [{GetType().Name}] {message}");
        }
    }
}
