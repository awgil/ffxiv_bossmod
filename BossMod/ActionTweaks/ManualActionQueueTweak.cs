namespace BossMod;

// Custom queue for manual actions.
// When running autorotation, we typically still want the ability to execute actions manually (e.g. if there is no plan available, or if some emergency happens).
// There are some problematic interactions with autorotation that this tweak solves:
// - typically we want to manually execute oGCDs, and typically we prefer slightly delaying them if it means not losing GCD uptime
// - however, we also want to give an emergency 'use asap' option (not worse than if you were to spam button without autorotation active)
// Tweak uses the following implementation:
// - maintain our own queue of manually requested actions, and route user-requested actions here instead of passing directly to the game
// - unlike native action queue, ours supports multiple pending entries
// - our queue distinguishes GCD and oGCD actions; since oGCDs can be delayed, effective 'expiration' time for oGCDs is much larger than native 0.5s
// - trying to queue an oGCD action while it is already queued (double tapping) activates 'emergency mode': all preceeding queued actions are removed and this action is returned even if it would delay GCD
// - entries from the manual queue are added to the autoqueue every frame with appropriate priorities, and usual logic selects best action to execute
public sealed class ManualActionQueueTweak(WorldState ws, AIHints hints)
{
    private readonly record struct Entry(ActionID Action, Actor? Target, Vector3 TargetPos, Angle? FacingAngle, ActionDefinition Definition, DateTime ExpireAt, float CastTime)
    {
        public readonly bool Expired(DateTime now) => ExpireAt < now || (Target?.IsDestroyed ?? false);
    }

    private readonly ActionTweaksConfig _config = Service.Config.Get<ActionTweaksConfig>();
    private readonly List<Entry> _queue = [];
    private bool _emergencyMode;

    public void RemoveExpired()
    {
        if (_emergencyMode && _queue[0].Expired(ws.CurrentTime))
        {
            Service.Log($"[MAO] Emergency {_queue[0].Action} expired");
            _emergencyMode = false;
        }

        bool checkExpired(Entry e)
        {
            if (e.Expired(ws.CurrentTime))
            {
                Service.Log($"[MAO] Action {e.Action} @ {e.Target} expired");
                return true;
            }
            return false;
        }
        _queue.RemoveAll(checkExpired);
    }

    public void FillQueue(ActionQueue queue)
    {
        if (_emergencyMode)
        {
            ref var entry = ref _queue.Ref(0);
            queue.Push(entry.Action, entry.Target, ActionQueue.Priority.ManualEmergency, 0, 0, 0, entry.TargetPos, entry.FacingAngle, true);
        }
        else
        {
            float expireOrder = 0; // we don't actually care about values, only ordering...
            foreach (ref var e in _queue.AsSpan())
            {
                var prio = ActionQueue.Priority.ManualOGCD;

                if (e.Definition.IsGCD || e.Action.Type is ActionType.Pomander or ActionType.Magicite)
                    prio = ActionQueue.Priority.ManualGCD;

                queue.Push(e.Action, e.Target, prio, expireOrder++, 0, e.CastTime, e.TargetPos, e.FacingAngle, true);
            }
        }
    }

    public bool Enabled => _config.UseManualQueue;

    public bool Push(ActionID action, ulong targetId, float castTime, bool allowTargetOverride, Func<(ulong, Vector3?)> getAreaTarget, Func<ulong> targetNearest)
    {
        if (!Enabled)
            return false; // we don't use queue at all

        var player = ws.Party.Player();
        if (player == null)
            return false; // player is unknown, skip

        var def = ActionDefinitions.Instance[action];
        if (def == null)
            return false; // unknown action, let native queue handle it instead

        bool isGCD = def.IsGCD;
        float expire = isGCD ? 1.0f : 3.0f;
        if (def.ReadyIn(ws.Client.Cooldowns, ws.Client.DutyActions) > expire)
            return false; // don't bother trying to queue something that's on cd

        if (!ResolveTarget(def, player, targetId, getAreaTarget, targetNearest, allowTargetOverride, out var target, out var targetPos))
            return false; // failed to resolve target

        Angle? angleOverride = def.TransformAngle?.Invoke(ws, player, target, hints);

        var expireAt = ws.CurrentTime.AddSeconds(expire);
        var index = _queue.FindIndex(e => e.Definition.MainCooldownGroup == def.MainCooldownGroup); // TODO: what about alt groups and duty actions?..
        if (index < 0)
        {
            Service.Log($"[MAO] Queueing {action} @ {target}");
            _queue.Add(new(action, target, targetPos, angleOverride, def, expireAt, castTime));
            return true;
        }

        ref var e = ref _queue.Ref(index);
        if (e.Action != action || e.Target != target)
        {
            Service.Log($"[MAO] Replacing queued {e.Action} with {action} @ {target}");
            _queue.RemoveAt(index);
            _queue.Add(new(action, target, targetPos, angleOverride, def, expireAt, castTime));
        }
        else
        {
            Service.Log($"[MAO] Entering emergency mode for {e.Action}");
            // spamming oGCD - enter emergency mode
            _queue.Clear();
            _queue.Add(new(action, target, targetPos, angleOverride, def, expireAt, castTime));
            _emergencyMode = true;
        }
        return true;
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

    private bool ResolveTarget(ActionDefinition def, Actor player, ulong targetId, Func<(ulong, Vector3?)> getAreaTarget, Func<ulong> targetNearest, bool allowSmartTarget, out Actor? target, out Vector3 targetPos)
    {
        target = null;
        targetPos = default;

        // ground targeted actions that must target specific objects
        if (def.ID.ID == (uint)BLM.AID.BetweenTheLines)
        {
            var playerLL = ws.Actors.FirstOrDefault(act => act.OwnerID == player.InstanceID && act.OID == 0x179);
            if (playerLL == null)
                return false;

            targetPos = playerLL.PosRot.XYZ();
            return true;
        }

        if (def.ID.ID == (uint)RPR.AID.Regress)
        {
            var playerGate = ws.Actors.FirstOrDefault(act => act.OwnerID == player.InstanceID && act.OID == 0x4C3);
            if (playerGate == null)
                return false;

            targetPos = playerGate.PosRot.XYZ();
            return true;
        }

        if (def.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            // GT actions with range 0 must be cast on player - there are only a few of these (BLM leylines, PCT leylines, PCT PVP limit break)
            if (def.Range == 0)
            {
                targetPos = player.PosRot.XYZ();
                return true;
            }

            // ground-targeted actions have special targeting
            var (gtTarget, gtPos) = getAreaTarget();
            if (gtPos != null)
            {
                // auto cast at cursor
                targetPos = gtPos.Value;
                return true;
            }
            else if (gtTarget is not 0 and not 0xE0000000)
            {
                var t = ws.Actors.Find(gtTarget);
                if (t != null)
                {
                    // auto cast at target's position
                    targetPos = t.PosRot.XYZ();
                    return true;
                }
                return false; // if target isn't found in world, bail
            }
            else
            {
                return false; // manual targeting desired
            }
        }

        if (def.AllowedTargets == ActionTargets.Self)
        {
            // the action can only target player, don't bother with other logic...
            target = player;
            return true;
        }

        target = ws.Actors.Find(targetId);
        if (target == null && targetId is not 0 and not 0xE0000000)
            return false; // target is valid, but not found in world, bail... (TODO this shouldn't be happening really)

        // custom smart-targeting
        if (allowSmartTarget && _config.SmartTargets && def.SmartTarget != null)
            target = def.SmartTarget(ws, player, target, hints);

        // fallback: if requested, use native "target nearest" function to try to find a valid hostile target
        // this conditional ensures we don't get a false positive for holmgang (can target self or hostile) or phantom oracle invuln (can target ally, but not self)
        if (target == null && def.AllowedTargets.HasFlag(ActionTargets.Hostile) && !def.AllowedTargets.HasFlag(ActionTargets.Self))
        {
            target = ws.Actors.Find(targetNearest());
            return true;
        }

        // smart-targeting fallback: cast on self if target is not valid
        var targetInvalid = target == null || !def.AllowedTargets.HasFlag(ActionTargets.Hostile) && !target.IsAlly;
        if (targetInvalid && def.AllowedTargets.HasFlag(ActionTargets.Self))
            target = player;

        return true;
    }
}
