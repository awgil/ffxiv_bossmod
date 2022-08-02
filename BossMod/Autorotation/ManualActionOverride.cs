using System;
using System.Collections.Generic;
using System.Numerics;

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
            public Vector3 TargetPos;
            public float AnimLock;
            public int CooldownGroup;
            public float AvailableAtCooldown;
            public Func<ulong, bool>? Condition;
            public DateTime ExpireAt;

            public bool Allowed => Condition == null || Condition(TargetID);
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

        public void Push(ActionID action, ulong targetID, Vector3 targetPos, ActionDefinition def, Func<ulong, bool>? condition)
        {
            bool isGCD = def.CooldownGroup == CommonDefinitions.GCDGroup;
            float expire = isGCD ? 1.0f : 3.0f;
            if (_cooldowns[def.CooldownGroup] - expire > def.CooldownAtFirstCharge)
            {
                Service.Log($"[MAO] Ignoring {action} @ {targetID:X}, since it will expire before coming off cooldown");
                return;
            }

            var expireAt = _ws.CurrentTime.AddSeconds(expire);
            var index = _queue.FindIndex(e => e.CooldownGroup == def.CooldownGroup);
            if (index < 0)
            {
                Service.Log($"[MAO] Queueing {action} @ {targetID:X}");
                _queue.Add(new() { Action = action, TargetID = targetID, TargetPos = targetPos, AnimLock = def.AnimationLock, CooldownGroup = def.CooldownGroup, AvailableAtCooldown = def.CooldownAtFirstCharge, Condition = condition, ExpireAt = expireAt });
                return;
            }

            var e = _queue[index];
            if (e.Action != action || e.TargetID != targetID)
            {
                Service.Log($"[MAO] Replacing queued {e.Action} with {action} @ {targetID:X}");
                _queue.RemoveAt(index);
                _queue.Add(new() { Action = action, TargetID = targetID, TargetPos = targetPos, AnimLock = def.AnimationLock, CooldownGroup = def.CooldownGroup, AvailableAtCooldown = def.CooldownAtFirstCharge, Condition = condition, ExpireAt = expireAt });
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
                _queue.Add(new() { Action = action, TargetID = targetID, TargetPos = targetPos, AnimLock = def.AnimationLock, CooldownGroup = def.CooldownGroup, AvailableAtCooldown = def.CooldownAtFirstCharge, Condition = condition, ExpireAt = expireAt });
                _emergencyMode = true;
            }
        }

        // note: this will remove expired entries
        public (ActionID Action, ulong TargetID, Vector3 TargetPos, bool Emergency) Peek(float delay, float animLockDelay, bool allowGCDDelay)
        {
            // first remove all expired entries (and if 'emergency' entry is popped, oh well)
            if (_emergencyMode && _queue[0].ExpireAt < _ws.CurrentTime)
            {
                Service.Log($"[MAO] Emergency {_queue[0].Action} expired");
                _emergencyMode = false;
            }
            _queue.RemoveAll(CheckExpired);

            var index = FindIndex(delay, animLockDelay, allowGCDDelay);
            if (index >= 0)
            {
                var e = _queue[index];
                return (e.Action, e.TargetID, e.TargetPos, _emergencyMode);
            }
            else
            {
                return (new(), 0, new(), _emergencyMode);
            }
        }

        public void Pop(ActionID action)
        {
            var index = _queue.FindIndex(e => e.Action == action);
            if (index >= 0)
            {
                Service.Log($"[MAO] Executed {action}");
                _queue.RemoveAt(index);
            }

            if (_emergencyMode && index == 0)
                _emergencyMode = false;
        }

        private int FindIndex(float delay, float animLockDelay, bool allowGCDDelay)
        {
            if (_emergencyMode)
            {
                // in emergency mode, return emergency action if off cd or nothing (and a flag that caller will use to skip looking for lower-priority actions)
                var e = _queue[0];
                return (_cooldowns[e.CooldownGroup] - delay <= e.AvailableAtCooldown) ? 0 : -1;
            }

            // if off gcd, prioritize gcd always
            var gcd = _cooldowns[CommonDefinitions.GCDGroup] - delay;
            if (gcd <= 0)
            {
                var gcdIndex = _queue.FindIndex(e => e.CooldownGroup == CommonDefinitions.GCDGroup && e.Allowed);
                if (gcdIndex >= 0)
                    return gcdIndex;
            }

            // look for available oGCD
            var maxOGCDAnimLock = allowGCDDelay ? float.MaxValue : gcd - animLockDelay;
            if (maxOGCDAnimLock > 0)
            {
                var ogcdIndex = _queue.FindIndex(e => e.CooldownGroup != CommonDefinitions.GCDGroup && e.AnimLock <= maxOGCDAnimLock && (_cooldowns[e.CooldownGroup] - delay <= e.AvailableAtCooldown) && e.Allowed);
                if (ogcdIndex >= 0)
                    return ogcdIndex;
            }

            // nothing found
            return -1;
        }

        private bool CheckExpired(Entry e)
        {
            if (e.ExpireAt < _ws.CurrentTime)
            {
                Service.Log($"[MAO] Action {e.Action} @ {e.TargetID:X} expired");
                return true;
            }
            return false;
        }
    }
}
