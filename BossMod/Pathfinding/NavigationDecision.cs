namespace BossMod.Pathfinding;

// utility for selecting player's navigation target
// there are several goals that navigation has to meet, in following rough priority
// 1. stay away from aoes; tricky thing is that sometimes it is ok to temporarily enter aoe, if we're sure we'll exit it in time
// 2. maintain uptime - this is represented by being in specified range of specified target, and not moving to interrupt casts unless needed
// 3. execute positionals - this is strictly less important than points above, we only do that if we can meet other conditions
// 4. be in range of healers - even less important, but still nice to do
public struct NavigationDecision
{
    public enum Decision
    {
        None,
        ImminentToSafe,
        ImminentToClosest,
        UnsafeToPositional,
        UnsafeToUptime,
        UnsafeToSafe,
        SafeToUptime,
        SafeToCloser,
        SafeBlocked,
        UptimeToPositional,
        UptimeBlocked,
        Optimal,
    }

    public WPos? Destination;
    public float LeewaySeconds; // can be used for finishing casts / slidecasting etc.
    public float TimeToGoal;
    public Map? Map;
    public int MapGoal;
    public Decision DecisionType;

    public const float DefaultForbiddenZoneCushion = 0.7071068f;

    public static NavigationDecision Build(WorldState ws, AIHints hints, Actor player, WPos? targetPos, float targetRadius, Angle targetRot, Positional positional, float playerSpeed = 6, float forbiddenZoneCushion = DefaultForbiddenZoneCushion)
    {
        // TODO: skip pathfinding if there are no forbidden zones, just find closest point in circle/cone...

        var imminent = ImminentExplosionTime(ws.CurrentTime);
        int numImminentZones = hints.ForbiddenZones.FindIndex(z => z.activation > imminent);
        if (numImminentZones < 0)
            numImminentZones = hints.ForbiddenZones.Count;

        // check whether player is inside each forbidden zone
        var inZone = hints.ForbiddenZones.Select(f => f.shapeDistance(player.Position) <= forbiddenZoneCushion - 0.1f).ToList(); // we might have a situation where player's cell center is outside, but player is not, yet player is too close to center for navigation to work...
        if (inZone.Any(inside => inside))
        {
            // we're in forbidden zone => find path to safety (and ideally to uptime zone)
            // if such a path can't be found (that's always the case if we're inside imminent forbidden zone, but can also happen in other cases), try instead to find a path to safety that doesn't enter any other zones that we're not inside
            // first build a map with zones that we're outside of as blockers
            var map = hints.Bounds.BuildMap();
            foreach (var (zf, inside) in hints.ForbiddenZones.Zip(inZone))
                if (!inside)
                    AddBlockerZone(map, imminent, zf.activation, zf.shapeDistance, forbiddenZoneCushion);

            bool inImminentForbiddenZone = inZone.Take(numImminentZones).Any(inside => inside);
            if (!inImminentForbiddenZone)
            {
                var map2 = map.Clone();
                foreach (var (zf, inside) in hints.ForbiddenZones.Zip(inZone))
                    if (inside)
                        AddBlockerZone(map2, imminent, zf.activation, zf.shapeDistance, forbiddenZoneCushion);
                int maxGoal = targetPos != null ? AddTargetGoal(map2, targetPos.Value, targetRadius, targetRot, positional, 0) : 0;
                var res = FindPathFromUnsafe(map2, player.Position, 0, maxGoal, targetPos, targetRot, positional, playerSpeed);
                if (res != null)
                    return res.Value;

                // pathfind to any spot outside aoes we're in that doesn't enter new aoes
                foreach (var (zf, inside) in hints.ForbiddenZones.Zip(inZone))
                    if (inside)
                        map.AddGoal(zf.shapeDistance, forbiddenZoneCushion, 0, -1);
                return FindPathFromImminent(map, player.Position, playerSpeed);
            }
            else
            {
                // try to find a path out of imminent aoes that we're in, while remaining in non-imminent aoes that we're already in - it might be worth it...
                foreach (var (zf, inside) in hints.ForbiddenZones.Zip(inZone).Take(numImminentZones))
                    if (inside)
                        map.AddGoal(zf.shapeDistance, forbiddenZoneCushion, 0, -1);
                return FindPathFromImminent(map, player.Position, playerSpeed);
            }
        }

        // we're safe, see if we can improve our position
        if (targetPos != null)
        {
            if (!player.Position.InCircle(targetPos.Value, targetRadius))
            {
                // we're not in uptime zone, just run to it, avoiding any aoes
                var map = hints.Bounds.BuildMap();
                foreach (var (shape, activation) in hints.ForbiddenZones)
                    AddBlockerZone(map, imminent, activation, shape, forbiddenZoneCushion);
                int maxGoal = AddTargetGoal(map, targetPos.Value, targetRadius, targetRot, Positional.Any, 0);
                if (maxGoal != 0)
                {
                    // try to find a path to target
                    var pathfind = new ThetaStar(map, maxGoal, player.Position, 1.0f / playerSpeed);
                    int res = pathfind.Execute();
                    if (res >= 0)
                        return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = float.MaxValue, TimeToGoal = pathfind.NodeByIndex(res).GScore, Map = map, MapGoal = maxGoal, DecisionType = Decision.SafeToUptime };
                }

                // goal is not reachable, but we can try getting as close to the target as we can until first aoe
                var start = map.ClampToGrid(map.WorldToGrid(player.Position));
                var end = map.ClampToGrid(map.WorldToGrid(targetPos.Value));
                var best = start;
                foreach (var (x, y) in map.EnumeratePixelsInLine(start.x, start.y, end.x, end.y))
                {
                    if (map[x, y].MaxG != float.MaxValue)
                        break;
                    best = (x, y);
                }
                if (best != start)
                {
                    var dest = map.GridToWorld(best.x, best.y, 0.5f, 0.5f);
                    return new() { Destination = dest, LeewaySeconds = float.MaxValue, TimeToGoal = (dest - player.Position).Length() / playerSpeed, Map = map, MapGoal = maxGoal, DecisionType = Decision.SafeToCloser };
                }

                return new() { Destination = null, LeewaySeconds = float.MaxValue, TimeToGoal = 0, Map = map, MapGoal = maxGoal, DecisionType = Decision.SafeBlocked };
            }

            bool inPositional = positional switch
            {
                Positional.Flank => MathF.Abs(targetRot.ToDirection().Dot((targetPos.Value - player.Position).Normalized())) < 0.7071067f,
                Positional.Rear => targetRot.ToDirection().Dot((targetPos.Value - player.Position).Normalized()) < -0.7071068f,
                Positional.Front => targetRot.ToDirection().Dot((targetPos.Value - player.Position).Normalized()) > 0.999f, // ~2.5 degrees - assuming max position error of 0.1, this requires us to stay at least at R=~2.25
                _ => true
            };
            if (!inPositional)
            {
                // we're in uptime zone, but not in correct quadrant - move there, avoiding all aoes and staying within uptime zone
                var map = hints.Bounds.BuildMap();
                map.BlockPixelsInside(ShapeDistance.InvertedCircle(targetPos.Value, targetRadius), 0, 0);
                foreach (var (shape, activation) in hints.ForbiddenZones)
                    AddBlockerZone(map, imminent, activation, shape, forbiddenZoneCushion);
                int maxGoal = AddPositionalGoal(map, targetPos.Value, targetRadius, targetRot, positional, 0);
                if (maxGoal > 0)
                {
                    // try to find a path to quadrant
                    var pathfind = new ThetaStar(map, maxGoal, player.Position, 1.0f / playerSpeed);
                    int res = pathfind.Execute();
                    if (res >= 0)
                    {
                        var dest = IncreaseDestinationPrecision(GetFirstWaypoint(pathfind, res), targetPos, targetRot, positional);
                        return new() { Destination = dest, LeewaySeconds = float.MaxValue, TimeToGoal = pathfind.NodeByIndex(res).GScore, Map = map, MapGoal = maxGoal, DecisionType = Decision.UptimeToPositional };
                    }
                }

                // fail
                return new() { Destination = null, LeewaySeconds = float.MaxValue, TimeToGoal = 0, Map = map, MapGoal = maxGoal, DecisionType = Decision.UptimeBlocked };
            }
        }

        return new() { Destination = null, LeewaySeconds = float.MaxValue, TimeToGoal = 0, DecisionType = Decision.Optimal };
    }

    public static DateTime ImminentExplosionTime(DateTime currentTime) => currentTime.AddSeconds(1);

    public static void AddBlockerZone(Map map, DateTime imminent, DateTime activation, Func<WPos, float> shape, float threshold) => map.BlockPixelsInside(shape, MathF.Max(0, (float)(activation - imminent).TotalSeconds), threshold);

    public static int AddTargetGoal(Map map, WPos targetPos, float targetRadius, Angle targetRot, Positional positional, int minPriority)
    {
        var adjPrio = map.AddGoal(ShapeDistance.Circle(targetPos, targetRadius), 0, minPriority, 1);
        if (adjPrio == minPriority)
            return minPriority;
        return AddPositionalGoal(map, targetPos, targetRadius, targetRot, positional, minPriority + 1);
    }

    public static Func<WPos, float> ShapeDistanceFlank(WPos targetPos, Angle targetRot)
    {
        var n1 = (targetRot + 45.Degrees()).ToDirection();
        var n2 = n1.OrthoL();
        return p =>
        {
            var off = p - targetPos;
            var d1 = n1.Dot(off);
            var d2 = n2.Dot(off);
            var dr = Math.Max(d1, d2);
            var dl = Math.Max(-d1, -d2);
            return Math.Min(dr, dl);
        };
    }

    public static Func<WPos, float> ShapeDistanceRear(WPos targetPos, Angle targetRot)
    {
        var n1 = (targetRot - 45.Degrees()).ToDirection();
        var n2 = n1.OrthoL();
        return p =>
        {
            var off = p - targetPos;
            var d1 = n1.Dot(off);
            var d2 = n2.Dot(off);
            return Math.Max(d1, d2);
        };
    }

    public static int AddPositionalGoal(Map map, WPos targetPos, float targetRadius, Angle targetRot, Positional positional, int minPriority)
    {
        var adjPrio = minPriority;
        switch (positional)
        {
            case Positional.Flank:
                adjPrio = map.AddGoal(ShapeDistanceFlank(targetPos, targetRot), 0, minPriority, 1);
                break;
            case Positional.Rear:
                adjPrio = map.AddGoal(ShapeDistanceRear(targetPos, targetRot), 0, minPriority, 1);
                break;
            case Positional.Front:
                Func<int, int, bool> suitable = (x, y) =>
                {
                    if (!map.InBounds(x, y))
                        return false;
                    var pixel = map[x, y];
                    return pixel.Priority >= minPriority && pixel.MaxG == float.MaxValue;
                };

                var dir = targetRot.ToDirection();
                var maxRange = map.WorldToGrid(targetPos + dir * targetRadius);
                if (suitable(maxRange.x, maxRange.y))
                {
                    adjPrio = map.AddGoal(maxRange.x, maxRange.y, 1);
                }
                else if (targetRadius > 3)
                {
                    var minRange = map.WorldToGrid(targetPos + dir * 3);
                    foreach (var p in map.EnumeratePixelsInLine(maxRange.x, maxRange.y, minRange.x, minRange.y))
                    {
                        if (suitable(p.x, p.y))
                        {
                            adjPrio = map.AddGoal(p.x, p.y, 1);
                            break;
                        }
                    }
                }
                break;
        }
        return adjPrio;
    }

    public static NavigationDecision? FindPathFromUnsafe(Map map, WPos startPos, int safeGoal, int maxGoal, WPos? targetPos, Angle targetRot, Positional positional, float speed = 6)
    {
        if (maxGoal - safeGoal == 2)
        {
            // try finding path to flanking position
            var pathfind = new ThetaStar(map, maxGoal, startPos, 1.0f / speed);
            int res = pathfind.Execute();
            if (res >= 0)
            {
                var dest = IncreaseDestinationPrecision(GetFirstWaypoint(pathfind, res), targetPos, targetRot, positional);
                return new() { Destination = dest, LeewaySeconds = pathfind.NodeByIndex(res).PathLeeway, TimeToGoal = pathfind.NodeByIndex(res).GScore, Map = map, MapGoal = maxGoal, DecisionType = Decision.UnsafeToPositional };
            }
            --maxGoal;
        }

        if (maxGoal - safeGoal == 1)
        {
            // try finding path to uptime position
            var pathfind = new ThetaStar(map, maxGoal, startPos, 1.0f / speed);
            int res = pathfind.Execute();
            if (res >= 0)
                return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = pathfind.NodeByIndex(res).PathLeeway, TimeToGoal = pathfind.NodeByIndex(res).GScore, Map = map, MapGoal = maxGoal, DecisionType = Decision.UnsafeToUptime };
            --maxGoal;
        }

        if (maxGoal - safeGoal == 0)
        {
            // try finding path to any safe spot
            var pathfind = new ThetaStar(map, maxGoal, startPos, 1.0f / speed);
            int res = pathfind.Execute();
            if (res >= 0)
                return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = pathfind.NodeByIndex(res).PathLeeway, TimeToGoal = pathfind.NodeByIndex(res).GScore, Map = map, MapGoal = maxGoal, DecisionType = Decision.UnsafeToSafe };
        }

        return null;
    }

    public static NavigationDecision FindPathFromImminent(Map map, WPos startPos, float speed = 6)
    {
        var pathfind = new ThetaStar(map, 0, startPos, 1.0f / speed);
        int res = pathfind.Execute();
        if (res >= 0)
        {
            return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = 0, TimeToGoal = pathfind.NodeByIndex(res).GScore, Map = map, MapGoal = 0, DecisionType = Decision.ImminentToSafe };
        }

        // just run to closest safe spot, if no good path can be found
        var closest = map.EnumeratePixels().Where(p => { var px = map[p.x, p.y]; return px.Priority == 0 && px.MaxG == float.MaxValue; }).MinBy(p => (p.center - startPos).LengthSq()).center;
        return new() { Destination = closest, LeewaySeconds = 0, TimeToGoal = (closest - startPos).Length() / speed, Map = map, DecisionType = Decision.ImminentToClosest };
    }

    public static WPos? GetFirstWaypoint(ThetaStar pf, int cell)
    {
        do
        {
            ref var node = ref pf.NodeByIndex(cell);
            int parent = pf.CellIndex(node.ParentX, node.ParentY);
            if (pf.NodeByIndex(parent).GScore == 0)
                return pf.CellCenter(cell);
            cell = parent;
        }
        while (true);
    }

    public static WPos? IncreaseDestinationPrecision(WPos? dest, WPos? targetPos, Angle targetRot, Positional positional)
    {
        if (dest == null || targetPos == null || positional != Positional.Front)
            return dest;
        var dir = targetRot.ToDirection();
        var adjDest = targetPos.Value + dir * dir.Dot(dest.Value - targetPos.Value);
        return (dest.Value - adjDest).LengthSq() < 1 ? adjDest : dest;
    }
}
