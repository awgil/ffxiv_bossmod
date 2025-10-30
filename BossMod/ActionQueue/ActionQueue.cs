namespace BossMod;

// custom action queue
// the idea is to allow multiple providers (manual input, autorotation, boss modules, etc.) to gather prioritized list of actions
// then next action to use is selected using following logic:
// - select highest priority action
// - if it is still on cooldown, look for the next-best action that can fit without delaying previously selected action
// - repeat the process until no more actions can be found
public sealed class ActionQueue
{
    public readonly record struct Entry(ActionID Action, Actor? Target, float Priority, float Expire, float Delay, float CastTime, Vector3 TargetPos, Angle? FacingAngle, bool Manual);

    // reference priority guidelines
    // values divisible by 1000 are reserved for standard cooldown planner priorities
    // for actions with identical priorities, the 'expiration' field is used to disambiguate (entries expiring earlier are higher effective priority)
    // code should avoid adding several actions with identical priority for consistency; consider using values like 1100 or 1230 (not divisible by 1000, but divisible by 10) to allow user to fine-tune custom priorities
    // note that actions with priority < 0 will never be executed; they can still be added to the queue if it's convenient for the implementation
    public static class Priority
    {
        public const float Minimal = 0; // minimal priority for action to be still considered for execution; do not use directly
        // priorities > Minimal and < VeryLow should be used for ??? (don't know good usecases)
        public const float VeryLow = 1000; // only use this action if there is nothing else to press
        // priorities > VeryLow and < Low should be used for actions that can be safely delayed without affecting dps (eg. ability with charges when there is no risk of overcapping or losing raidbuffs any time soon)
        public const float Low = 2000; // only use this action if it won't delay dps action (eg. delay if there are any ogcds that need to be used)
        // priorities > Low and < Medium should be used for normal ogcds that are part of the rotation
        public const float Medium = 3000; // use this action in first possible ogcd slot, unless there's some hugely important rotational ogcd; you should always have at least 1 slot per gcd to execute Medium actions
        // priorities > Medium and < High should be used for ogcds that can't be delayed (eg. GNB continuation); code should be careful not to queue more than one such action per gcd window
        public const float High = 4000; // use this action asap, unless it would delay gcd (that is - in first possible ogcd slot); careless use could severely affect dps
        // priorities > High and < VeryHigh should be used for gcds, or any other actions that need to delay gcd
        public const float VeryHigh = 5000; // drop everything and use this action asap, delaying gcd if needed; almost guaranteed to severely affect dps
        // priorities > VeryHigh should not be used by general code

        public const float ManualOGCD = 4001; // manually pressed ogcd should be higher priority than any non-gcd, but lower than any gcd
        public const float ManualGCD = 4999; // manually pressed gcd should be higher priority than any gcd; it's still lower priority than VeryHigh, since presumably that action is planned to delay gcd
        public const float ManualEmergency = 9000; // this action should be used asap, because user is spamming it
    }

    public readonly List<Entry> Entries = [];

    public void Clear() => Entries.Clear();
    public void Push(ActionID action, Actor? target, float priority, float expire = float.MaxValue, float delay = 0, float castTime = 0, Vector3 targetPos = default, Angle? facingAngle = null, bool manual = false) => Entries.Add(new(action, target, priority, expire, delay, castTime, targetPos, facingAngle, manual));

    public Entry FindBest(WorldState ws, Actor player, ReadOnlySpan<Cooldown> cooldowns, float animationLock, AIHints hints, float instantAnimLockDelay, bool allowDismount)
    {
        Entries.SortByReverse(e => (e.Priority, -e.Expire));
        Entry best = default;
        float deadline = float.MaxValue; // any candidate we consider, if executed, should allow executing next action by this deadline
        foreach (ref var candidate in Entries.AsSpan())
        {
            if (candidate.Priority < Priority.Minimal)
                break; // this and further actions are something we don't really want to execute (prio < minimal)

            var def = ActionDefinitions.Instance[candidate.Action];
            if (def == null)
            {
                Service.Log($"[ActionQueue] unregistered action {candidate.Action} queued and will be discarded, this is a bug");
                continue;
            }
            if (!def.IsUnlocked(ws, player))
                continue;

            if (candidate.CastTime > hints.MaxCastTime)
                continue; // this cast can't be finished in time, look for something else

            var startDelay = Math.Max(Math.Max(candidate.Delay, animationLock), def.ReadyIn(cooldowns, ws.Client.DutyActions));

            // TODO: adjusted cast time!
            var duration = def.CastTime > 0 ? def.CastTime + def.CastAnimLock : def.InstantAnimLock + instantAnimLockDelay;
            if (startDelay + duration > deadline)
                continue; // this action can't be done in time for higher-priority action, skip

            // we can use this action before deadline it seems
            if (startDelay > 0.05f)
            {
                // the action can't be started right now, so save it as next-best and continue looking
                // note: we specifically *don't* check condition now - it could change before it's ready, and we don't want to delay it
                best = candidate;
                deadline = startDelay;
            }
            else if (CanExecute(ref candidate, def, ws, player, hints, allowDismount))
            {
                // the action can be used right now
                return candidate;
            }
            // else: even though the action is off cooldown, the condition prevents using it - skip it for now, no point waiting forever
        }
        return best;
    }

    private bool CanExecute(ref Entry entry, ActionDefinition? def, WorldState ws, Actor player, AIHints hints, bool allowDismount)
    {
        if (entry.Priority >= Priority.ManualEmergency || def == null)
            return true; // don't make any assumptions

        if (!allowDismount && AutoDismountTweak.IsMountPreventingAction(ws, def.ID))
            return false;

        if (def.ID.Type == ActionType.Item && ws.Client.GetItemQuantity(def.ID.ID) == 0)
            return false;

        if (def.Range > 0)
        {
            var to = entry.Target?.Position ?? new(entry.TargetPos.XZ());
            var distSq = (to - player.Position).LengthSq();
            var effRange = def.Range + player.HitboxRadius + (entry.Target?.HitboxRadius ?? 0);
            if (distSq > effRange * effRange)
                return false;
        }

        return def.ForbidExecute == null || !def.ForbidExecute.Invoke(ws, player, entry, hints);
    }
}
