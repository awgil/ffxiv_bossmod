namespace BossMod;

// when running autorotation, we typically still want the ability to execute actions manually (e.g. if there is no plan available, or if some emergency happens)
// there are some problematic interactions with autorotation that this class solves:
// - typically we want to manually execute oGCDs, and typically we prefer slightly delaying them if it means not losing GCD uptime
// - if after pressing manual oGCD we return to spamming rotation button, we might override manual oGCD with whatever autorotation decides should be cast instead
// our solution is the following:
// - maintain our own queue of manually requested actions, and route user-requested actions here instead of passing directly to the game
// - unlike native action queue, ours supports multiple pending entries
// - our queue distinguishes GCD and oGCD actions; since oGCDs can be delayed, effective 'expiration' time for oGCDs is much larger than native 0.5s
// - trying to queue an oGCD action while it is already queued (double tapping) activates 'emergency mode': all preceeding queued actions are removed and this action is returned even if it would delay GCD
class ManualActionOverride(WorldState ws)
{
    public class Entry(ActionID action, Actor? target, Vector3 targetPos, Angle? facingAngle, ActionDefinition definition, Func<Actor?, bool>? condition, DateTime expireAt)
    {
        public ActionID Action = action;
        public Actor? Target = target;
        public Vector3 TargetPos = targetPos;
        public Angle? FacingAngle = facingAngle;
        public ActionDefinition Definition = definition;
        public Func<Actor?, bool>? Condition = condition;
        public DateTime ExpireAt = expireAt;

        public bool Allowed(Actor player)
        {
            if (Definition.Range > 0)
            {
                var to = Target?.Position ?? new WPos(TargetPos.X, TargetPos.Z);
                var distSq = (to - player.Position).LengthSq();
                var effRange = Definition.Range + player.HitboxRadius + (Target?.HitboxRadius ?? 0);
                if (distSq > effRange * effRange)
                    return false;
            }
            return Condition == null || Condition(Target);
        }

        public bool Expired(DateTime now) => ExpireAt < now || (Target?.IsDestroyed ?? false);
    }

    private readonly List<Entry> _queue = [];
    private bool _emergencyMode;

    public void RemoveExpired()
    {
        if (_emergencyMode && _queue[0].Expired(ws.CurrentTime))
        {
            Service.Log($"[MAO] Emergency {_queue[0].Action} expired");
            _emergencyMode = false;
        }
        _queue.RemoveAll(CheckExpired);
    }

    public void Push(ActionID action, Actor? target, Vector3 targetPos, Angle? facingAngle, ActionDefinition def, Func<Actor?, bool>? condition, bool simulated = false)
    {
        bool isGCD = def.CooldownGroup == CommonDefinitions.GCDGroup;
        float expire = isGCD ? 1.0f : 3.0f;
        if (ws.Client.Cooldowns[def.CooldownGroup].Remaining - expire > def.CooldownAtFirstCharge)
        {
            return;
        }

        var expireAt = ws.CurrentTime.AddSeconds(expire);
        var index = _queue.FindIndex(e => e.Definition.CooldownGroup == def.CooldownGroup);
        if (index < 0)
        {
            Service.Log($"[MAO] Queueing {action} @ {target}");
            _queue.Add(new(action, target, targetPos, facingAngle, def, condition, expireAt));
            return;
        }

        var e = _queue[index];
        if (e.Action != action || e.Target != target)
        {
            Service.Log($"[MAO] Replacing queued {e.Action} with {action} @ {target}");
            _queue.RemoveAt(index);
            _queue.Add(new(action, target, targetPos, facingAngle, def, condition, expireAt));
        }
        else if (isGCD)
        {
            // spamming GCD - just extend expiration time; don't bother moving stuff around, since GCD vs oGCD order doesn't matter
            e.ExpireAt = expireAt;
        }
        else if (!simulated)
        {
            Service.Log($"[MAO] Entering emergency mode for {e.Action}");
            // spamming oGCD - enter emergency mode
            _queue.Clear();
            _queue.Add(new(action, target, targetPos, facingAngle, def, condition, expireAt));
            _emergencyMode = true;
        }
    }

    public void Pop(ActionID action, bool simulated = false)
    {
        var index = _queue.FindIndex(e => e.Action == action);
        if (index >= 0)
        {
            if (!simulated)
                Service.Log($"[MAO] Executed {action}");
            _queue.RemoveAt(index);
        }

        if (_emergencyMode && index == 0)
            _emergencyMode = false;
    }

    // note: this does not check condition/range, assume in emergency mode user knows what he's doing
    public Entry? PeekEmergency() => _emergencyMode ? _queue[0] : null;

    public Entry? PeekGCD()
    {
        var player = ws.Party.Player();
        if (_emergencyMode || player == null)
            return null;
        var e = _queue.Find(e => e.Definition.CooldownGroup == CommonDefinitions.GCDGroup);
        return e != null && e.Allowed(player) ? e : null;
    }

    // deadline is typically gcd minus anim-lock-delay
    public Entry? PeekOGCD(float effAnimLock, float animLockDelay, float deadline)
    {
        var player = ws.Party.Player();
        return !_emergencyMode && player != null ? _queue.Find(e => CheckOGCD(e, player, effAnimLock, animLockDelay, deadline)) : null;
    }

    private bool CheckOGCD(Entry e, Actor player, float effAnimLock, float animLockDelay, float deadline)
    {
        return e.Definition.CooldownGroup != CommonDefinitions.GCDGroup
            && ws.Client.Cooldowns[e.Definition.CooldownGroup].Remaining - effAnimLock <= e.Definition.CooldownAtFirstCharge
            && effAnimLock + e.Definition.AnimationLock + animLockDelay <= deadline
            && e.Allowed(player);
    }

    private bool CheckExpired(Entry e)
    {
        if (e.Expired(ws.CurrentTime))
        {
            Service.Log($"[MAO] Action {e.Action} @ {e.Target} expired");
            return true;
        }
        return false;
    }
}
