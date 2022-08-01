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
            public bool RequestSent;
        }

        private float[] _cooldowns; // assumed to be updated by external code
        private DateTime _now;
        private List<Entry> _queue = new();
        private bool _emergencyMode = false;

        public ManualActionOverride(float[] cooldowns, DateTime now)
        {
            _cooldowns = cooldowns;
            _now = now;
        }

        public void Update(DateTime now)
        {
            _now = now;
            if (_emergencyMode && _queue[0].ExpireAt < now)
                _emergencyMode = false;

            _queue.RemoveAll(e => {
                bool expired = e.ExpireAt < _now;
                if (expired && !e.RequestSent)
                    Service.Log($"[MAO] Request {e.Action} @ {e.TargetID:X} expired");
                return expired;
            });
        }

        public void Queue(ActionID action, ulong target, float animLock, int cooldownGroup, float availableAtCooldown, Func<bool>? condition)
        {
            bool isGCD = cooldownGroup == CommonDefinitions.GCDGroup;
            float expire = isGCD ? 1.0f : 3.0f;
            if (_cooldowns[cooldownGroup] - expire > availableAtCooldown)
            {
                Service.Log($"[MAO] Ignoring {action} @ {target:X}, since it will expire before coming off cooldown");
                return;
            }

            var expireAt = _now.AddSeconds(expire);
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
            else if (e.RequestSent)
            {
                // we're spamming an action, and request was sent recently - just ignore
                Service.Log($"[MAO] Ignoring {e.Action}, since it was cast recently");
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

        public void NotifyRequestSent(ActionID action, ulong target)
        {
            var index = _queue.FindIndex(e => e.Action == action);
            if (index < 0)
                return; // don't care, we didn't queue it

            var e = _queue[index];
            Service.Log($"[MAO] Request sent for {action} @ {target:X}{(target == e.TargetID ? "" : $" instead of planned {e.TargetID:X} !!")}");
            e.RequestSent = true;
            e.ExpireAt = _now.AddSeconds(0.5f); // don't re-queue for some small time, if user is spamming
        }

        // since we're considering next action every frame anyway, we don't bother checking under animation lock - thus we assume that this function is called when current lock is zero
        public (ActionID, ulong) GetAction(float animLockDelay)
        {
            if (_emergencyMode)
                return (_queue[0].Action, _queue[0].TargetID);

            // look for first action that is off cooldown, using which won't delay next gcd
            float gcd = _cooldowns[CommonDefinitions.GCDGroup];
            var index = _queue.FindIndex(e => !e.RequestSent && _cooldowns[e.CooldownGroup] <= e.AvailableAtCooldown && (e.CooldownGroup == CommonDefinitions.GCDGroup || e.AnimLock + animLockDelay < gcd));
            if (index >= 0)
                return (_queue[index].Action, _queue[index].TargetID);

            // nothing found
            return (new(), 0);
        }
    }
}
