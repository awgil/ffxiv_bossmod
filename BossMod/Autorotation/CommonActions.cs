using Dalamud.Game.ClientState.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    abstract class CommonActions
    {
        // all relevant target IDs used for smart target selection
        protected struct Targets
        {
            public uint MainTarget;
            public uint MouseoverTarget;
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
            var cooldownPlan = Autorot.Bossmods.ActiveModule?.PlanExecution;
            if (cooldownPlan != null)
            {
                var stateData = cooldownPlan.FindStateData(Autorot.Bossmods.ActiveModule?.StateMachine.ActiveState);
                foreach (var (action, entry) in _sq.Entries)
                {
                    var plan = stateData?.Abilities.GetValueOrDefault(action);
                    if (plan != null)
                    {
                        var progress = Autorot.Bossmods.ActiveModule!.StateMachine.TimeSinceTransition;
                        var activeWindow = plan.ActivationWindows.FindIndex(w => w.Start <= progress && w.End > progress);
                        if (activeWindow != -1)
                        {
                            entry.ActivatePlanned(Autorot.Bossmods.WorldState.CurrentTime, plan.ActivationWindows[activeWindow].End - progress);
                        }
                    }
                }
            }

            var state = OnUpdate();
            _sq.Active = state.GCD > 0 || state.AnimationLock > 0;
        }

        public (ActionID, uint) ReplaceActionAndTarget(ActionID actionID, uint targetID)
        {
            var targets = new Targets() { MainTarget = targetID, MouseoverTarget = Mouseover.Instance?.Object?.ObjectId ?? 0 };
            if (Autorot.Config.SmartCooldownQueueing && _sq.Active)
            {
                var e = _sq.Entries.GetValueOrDefault(actionID);
                if (e != null)
                {
                    Log($"Smart-queueing {actionID} @ {targetID:X}");
                    e.ActivateManual(Autorot.Bossmods.WorldState.CurrentTime, targets);
                    (actionID, targets) = _sq.Replacement;
                }
            }
            _sq.Replacement = (actionID, targets); // if we're not smart-queueing, update replacement to last action

            return DoReplaceActionAndTarget(actionID, targets);
        }

        abstract protected void OnCastSucceeded(ActionID actionID);
        abstract protected CommonRotation.State OnUpdate();
        abstract protected (ActionID, uint) DoReplaceActionAndTarget(ActionID actionID, Targets targets);
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
            s.PotionCD = PotionCooldown();
        }

        // fill common strategy properties
        protected void FillCommonStrategy(CommonRotation.Strategy strategy, ActionID potion)
        {
            if (Service.ClientState.LocalPlayer == null)
                return;

            strategy.Prepull = !Service.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat);
            strategy.FightEndIn = Autorot.Bossmods.ActiveModule?.PlanExecution?.EstimateTimeToNextDowntime(Autorot.Bossmods.ActiveModule?.StateMachine) ?? 0;
            strategy.RaidBuffsIn = Autorot.Bossmods.RaidCooldowns.NextDamageBuffIn(Autorot.Bossmods.WorldState.CurrentTime);
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
        protected bool SmartQueueActive(ActionID action) => _sq.Entries.GetValueOrDefault(action)?.Active(Autorot.Bossmods.WorldState.CurrentTime) ?? false;
        protected bool SmartQueueActiveSpell<AID>(AID spell) where AID : Enum => SmartQueueActive(ActionID.MakeSpell(spell));

        // get smart-queue target, if available; otherwise (if smart-queue is inactive or if it was planned without specific target) return fallback (current targets)
        protected Targets SmartQueueTarget(ActionID action, Targets fallback)
        {
            var e = _sq.Entries.GetValueOrDefault(action);
            return e != null && e.Active(Autorot.Bossmods.WorldState.CurrentTime) && e.Targets != null ? e.Targets.Value : fallback;
        }
        protected Targets SmartQueueTargetSpell<AID>(AID spell, Targets fallback) where AID : Enum => SmartQueueTarget(ActionID.MakeSpell(spell), fallback);

        // deactivate smart-queue entry
        protected void SmartQueueDeactivate(ActionID action) => _sq.Entries.GetValueOrDefault(action)?.Deactivate();

        // smart targeting utility: return target (if friendly) or mouseover (if friendly and allowed) or null (otherwise)
        protected Actor? SmartTargetFriendly(Targets targets, bool allowMouseover)
        {
            var target = Autorot.Bossmods.WorldState.Actors.Find(targets.MainTarget);
            if (target?.Type is ActorType.Player or ActorType.Chocobo)
                return target;

            if (allowMouseover)
            {
                target = Autorot.Bossmods.WorldState.Actors.Find(targets.MouseoverTarget);
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
