using System;
using System.Collections.Generic;

namespace BossMod
{
    // when running autorotation, we typically still want the ability to execute actions manually (e.g. if there is no plan available, or if some emergency happens)
    // there are some problematic interactions with autorotation that this class solves:
    // - typically we want to manually execute oGCDs, and typically we prefer slightly delaying them if it means not losing GCD uptime
    // - if after pressing manual oGCD we return to spamming rotation button, we might override manual oGCD with whatever autorotation decides should be cast instead
    // our solution is the following:
    // - maintain our own queue of manually requested actions, and route user-requested actions here instead of passing directly to the game
    // - unlike native action queue, ours supports multiple pending entries
    // - our queue distinguishes GCD and oGCD actions; since oGCDs can be delayed, effective 'expiration' time for oGCDs is much larger than native 0.5s
    // - trying to queue an oGCD action while it is already queued (double tapping) activates 'emergency mode': all preceeding queued actions are removed and this action is returned even if it would delay GCD
    class ManualActionOverride
    {
        class Entry
        {
            public ActionID Action;
            public ulong TargetID;
            public float AnimLock;
            public int CooldownGroup;
            public float AvailableAtCooldown;
            public Func<bool>? Condition;
            public DateTime ExpireAt;
        }

        private float[] _cooldowns; // assumed to be updated by external code
        private WorldState _ws; // used to read current time
        private List<Entry> _queue = new();
        private bool _emergencyMode = false;

        public ManualActionOverride(float[] cooldowns, WorldState ws)
        {
            _cooldowns = cooldowns;
            _ws = ws;
        }

        public void Push(ActionID action, ulong target, float animLock, int cooldownGroup, float availableAtCooldown, Func<bool>? condition)
        {
            bool isGCD = cooldownGroup == CommonDefinitions.GCDGroup;
            float expire = isGCD ? 1.0f : 3.0f;
            if (_cooldowns[cooldownGroup] - expire > availableAtCooldown)
            {
                Service.Log($"[MAO] Ignoring {action} @ {target:X}, since it will expire before coming off cooldown");
                return;
            }

            var expireAt = _ws.CurrentTime.AddSeconds(expire);
            var index = _queue.FindIndex(e => e.CooldownGroup == cooldownGroup);
            if (index < 0)
            {
                Service.Log($"[MAO] Queueing {action} @ {target:X}");
                _queue.Add(new() { Action = action, TargetID = target, AnimLock = animLock, CooldownGroup = cooldownGroup, AvailableAtCooldown = availableAtCooldown, Condition = condition, ExpireAt = expireAt });
                return;
            }

            var e = _queue[index];
            if (e.Action != action || e.TargetID != target)
            {
                Service.Log($"[MAO] Replacing queued {e.Action} with {action} @ {target:X}");
                _queue.RemoveAt(index);
                _queue.Add(new() { Action = action, TargetID = target, AnimLock = animLock, CooldownGroup = cooldownGroup, AvailableAtCooldown = availableAtCooldown, Condition = condition, ExpireAt = expireAt });
            }
            else if (isGCD)
            {
                // spamming GCD - just extend expiration time; don't bother moving stuff around, since GCD vs oGCD order doesn't matter
                e.ExpireAt = expireAt;
            }
            else
            {
                Service.Log($"[MAO] Entering emergency mode for {e.Action}");
                // spamming oGCD - enter emergency mode
                _queue.Clear();
                _queue.Add(new() { Action = action, TargetID = target, AnimLock = animLock, CooldownGroup = cooldownGroup, AvailableAtCooldown = availableAtCooldown, Condition = condition, ExpireAt = expireAt });
                _emergencyMode = true;
            }
        }

        // note: any returned action is immediately popped off the queue - we assume caller will execute it
        public (ActionID Action, ulong Target, bool Emergency) Pop(float animLock, float animLockDelay)
        {
            // first remove all expired entries (and if 'emergency' entry is popped, oh well)
            if (_emergencyMode && _queue[0].ExpireAt < _ws.CurrentTime)
            {
                Service.Log($"[MAO] Emergency {_queue[0].Action} expired");
                _emergencyMode = false;
            }
            _queue.RemoveAll(e => e.ExpireAt < _ws.CurrentTime);

            if (_emergencyMode)
            {
                // in emergency mode, return emergency action if off cd or nothing (and a flag that caller will use to skip looking for lower-priority actions)
                var e = _queue[0];
                if (MathF.Max(_cooldowns[e.CooldownGroup] - e.AvailableAtCooldown, animLock) <= Autorotation.EnqueueWindow)
                {
                    // pop off emergency action
                    Service.Log($"[MAO] Executing emergency action {e.Action} @ {e.TargetID:X}");
                    _queue.RemoveAt(0);
                    _emergencyMode = false;
                    return (e.Action, e.TargetID, true);
                }
                else
                {
                    // emergency action is not ready yet, return nothing and keep emergency mode active
                    return (new(), 0, true);
                }
            }

            // look for first action that is off cooldown, using which won't delay next gcd
            float gcd = _cooldowns[CommonDefinitions.GCDGroup];
            var index = _queue.FindIndex(e => CanExecuteAction(e, animLock, animLockDelay));
            if (index >= 0)
            {
                var e = _queue[index];
                Service.Log($"[MAO] Executing queued action {e.Action} @ {e.TargetID:X}");
                _queue.RemoveAt(index);
                return (e.Action, e.TargetID, false);
            }

            // nothing found
            return (new(), 0, false);
        }

        private bool CanExecuteAction(Entry e, float animLock, float animLockDelay)
        {
            var canExecuteIn = MathF.Max(_cooldowns[e.CooldownGroup] - e.AvailableAtCooldown, animLock);
            return canExecuteIn <= Autorotation.EnqueueWindow
                && (e.CooldownGroup == CommonDefinitions.GCDGroup || canExecuteIn + e.AnimLock + animLockDelay < _cooldowns[CommonDefinitions.GCDGroup])
                && (e.Condition == null || e.Condition());
        }
    }
}
