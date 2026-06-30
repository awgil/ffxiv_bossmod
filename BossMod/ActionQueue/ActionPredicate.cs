namespace BossMod;

public static class ActionPredicate
{
    public static bool AllowDashToTarget(WorldState ws, Actor player, ActionQueue.Entry action, AIHints hints)
    {
        var cfg = Service.Config.Get<ActionTweaksConfig>();
        var target = action.Target;
        if (target == null || !cfg.DashSafety)
            return true;

        // if there are pending knockbacks, god only knows where we would be sent after using a gapcloser
        // note that once the knockback is actually active and not pending, we can probably cancel it with a dash
        if (player.PendingKnockbacks.Count > 0)
            return false;

        var dist = player.DistanceToHitbox(target);
        var dir = player.AngleTo(target);
        var src = player.Position;

        // facing target (to dash) would make us fail gaze, directional bait, etc
        // TODO: only forbid if dash duration is longer than time to deadline?
        if (hints.ForbiddenDirections.Any(d => dir.AlmostEqual(d.center, d.halfWidth.Rad)))
            return false;

        // TODO: check against action's animation lock duration instead of constant 0.8?
        var (mode, deadline) = hints.ImminentSpecialMode;
        if (mode is AIHints.SpecialMode.Pyretic or AIHints.SpecialMode.PyreticMove && deadline <= ws.FutureTime(0.8f))
            return false;

        return IsDashSafe(src, src + dir.ToDirection() * MathF.Max(0, dist), hints);
    }

    public static bool AllowDashToPosition(WorldState _, Actor player, ActionQueue.Entry action, AIHints hints)
    {
        var cfg = Service.Config.Get<ActionTweaksConfig>();
        if (action.TargetPos == default || !cfg.DashSafety || !cfg.DashSafetyExtra)
            return true;

        if (player.PendingKnockbacks.Count > 0)
            return false;

        return IsDashSafe(player.Position, new WPos(action.TargetPos.XZ()), hints);
    }

    public static ActionDefinition.ConditionDelegate AllowDashFixed(float range, bool backwards = false)
        => (ws, player, act, hints) =>
        {
            var cfg = Service.Config.Get<ActionTweaksConfig>();
            if (!cfg.DashSafety || !cfg.DashSafetyExtra)
                return true;

            if (player.PendingKnockbacks.Count > 0)
                return false;

            var dir = act.FacingAngle ?? player.Rotation;

            var dest = player.Position + dir.ToDirection() * range * (backwards ? -1 : 1);

            return IsDashSafe(player.Position, dest, hints);
        };

    public static ActionDefinition.ConditionDelegate AllowBackdash(float range)
         => (ws, player, act, hints) =>
        {
            var cfg = Service.Config.Get<ActionTweaksConfig>();
            if (act.Target == null || !cfg.DashSafety || !cfg.DashSafetyExtra)
                return true;

            if (player.PendingKnockbacks.Count > 0)
                return false;

            var dir = act.Target.DirectionTo(player).Normalized();

            return IsDashSafe(player.Position, player.Position + dir * range, hints);
        };

    // check if dashing to target will put the player inside a forbidden zone
    public static bool IsDashSafe(WPos from, WPos to, AIHints hints)
    {
        var center = hints.PathfindMapCenter;
        if (!hints.PathfindMapBounds.Contains(to - center))
            return false;

        // if arena is a weird shape, try to ensure player won't dash out of it
        if (from != to && hints.PathfindMapBounds is ArenaBoundsCustom)
        {
            var len = (to - from).Length();
            var distToNearestWall = hints.PathfindMapBounds.IntersectRay(from - center, (to - from).Normalized());
            if (distToNearestWall >= 0 && distToNearestWall < len)
                return false;
        }

        return !hints.ForbiddenZones.Any(d => d.containsFn(to));
    }
}
