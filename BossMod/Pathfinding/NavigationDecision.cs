using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Pathfinding
{
    // utility for selecting player's navigation target
    // there are several goals that navigation has to meet, in following rough priority
    // 1. stay away from aoes; tricky thing is that sometimes it is ok to temporarily enter aoe, if we're sure we'll exit it in time
    // 2. maintain uptime - this is represented by being in specified range of specified target, and not moving to interrupt casts unless needed
    // 3. execute positionals - this is strictly less important than points above, we only do that if we can meet other conditions
    // 4. be in range of healers - even less important, but still nice to do
    public struct NavigationDecision
    {
        public WPos? Destination;
        public float LeewaySeconds; // can be used for finishing casts / slidecasting etc.
        public float TimeToPositional;
        public Map? Map;
        public int MapGoal;

        public static NavigationDecision Build(WorldState ws, AIHints hints, Actor player, WPos targetPos, float targetRadius, Angle targetRot, Positional positional, float playerSpeed = 6)
        {
            // first check that player is in bounds; otherwise pathfinding won't work properly anyway
            if (!hints.Bounds.Contains(player.Position))
                return new() { Destination = hints.Bounds.ClampToBounds(player.Position), LeewaySeconds = 0, TimeToPositional = float.MaxValue };

            var imminent = ImminentExplosionTime(ws.CurrentTime);
            int numImminentZones = hints.ForbiddenZones.FindIndex(z => z.activation > imminent);
            if (numImminentZones < 0)
                numImminentZones = hints.ForcedMovements.Count;

            // check whether player is inside each forbidden zone
            var inZone = hints.ForbiddenZones.Select(z => z.shape.Check(player.Position, z.origin, z.rot)).ToList();
            if (inZone.Any())
            {
                // we're in forbidden zone => find path to safety (and ideally to uptime zone)
                // if such a path can't be found (that's always the case if we're inside imminent forbidden zone, but can also happen in other cases), try instead to find a path to safety that doesn't enter any other zones that we're not inside
                // first build a map with zones that we're outside of as blockers
                var map = hints.Bounds.BuildMap();
                foreach (var (z, inside) in hints.ForbiddenZones.Zip(inZone))
                    if (!inside)
                        AddBlockerZone(map, imminent, z);

                bool inImminentForbiddenZone = inZone.Take(numImminentZones).Any();
                if (!inImminentForbiddenZone)
                {
                    var map2 = map.Clone();
                    foreach (var (z, inside) in hints.ForbiddenZones.Zip(inZone))
                        if (inside)
                            AddBlockerZone(map2, imminent, z);
                    int maxGoal = AddTargetGoal(map2, targetPos, targetRadius, targetRot, positional, 0);
                    var res = FindPathFromUnsafe(map2, player.Position, 0, maxGoal, playerSpeed);
                    if (res != null)
                        return res.Value;
                }

                // pathfind to any spot outside aoes we're in that doesn't enter new aoes
                foreach (var (z, inside) in hints.ForbiddenZones.Zip(inZone))
                    if (inside)
                        map.AddGoal(map.Rasterize(z.shape.Coverage(map, z.origin, z.rot)), Map.Coverage.Inside | Map.Coverage.Border, 0, -1);
                return FindPathFromImminent(map, player.Position, playerSpeed);
            }

            // we're safe, see if we can improve our position
            if (!player.Position.InCircle(targetPos, targetRadius))
            {
                // we're not in uptime zone, just run to it, avoiding any aoes
                var map = hints.Bounds.BuildMap();
                foreach (var z in hints.ForbiddenZones)
                    AddBlockerZone(map, imminent, z);
                int maxGoal = AddTargetGoal(map, targetPos, targetRadius, targetRot, Positional.Any, 0);
                if (maxGoal != 0)
                {
                    // try to find a path to target
                    var pathfind = new ThetaStar(map, maxGoal, player.Position, 1.0f / playerSpeed);
                    int res = pathfind.Execute();
                    if (res >= 0)
                        return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = float.MaxValue, TimeToPositional = float.MaxValue, Map = map, MapGoal = maxGoal };
                }

                // goal is not reachable, but we can try getting as close to the target as we can until first aoe
                var start = map.WorldToGrid(player.Position);
                var end = map.WorldToGrid(targetPos);
                var best = start;
                foreach (var (x, y) in map.EnumeratePixelsInLine(start.x, start.y, end.x, end.y))
                {
                    if (map[x, y].MaxG != float.MaxValue)
                        break;
                    best = (x, y);
                }
                return new() { Destination = best != start ? map.GridToWorld(best.x, best.y, 0.5f, 0.5f) : null, LeewaySeconds = 0, TimeToPositional = float.MaxValue, Map = map, MapGoal = maxGoal };
            }

            bool inPositional = positional switch
            {
                Positional.Flank => MathF.Abs(targetRot.ToDirection().Dot((targetPos - player.Position).Normalized())) < 0.7071067f,
                Positional.Rear => targetRot.ToDirection().Dot((targetPos - player.Position).Normalized()) < -0.7071068f,
                _ => true
            };
            if (!inPositional)
            {
                // we're in uptime zone, but not in correct quadrant - move there, avoiding all aoes and staying within uptime zone
                var map = hints.Bounds.BuildMap();
                map.BlockPixels(map.RasterizeCircle(targetPos, targetRadius), 0, Map.Coverage.Outside | Map.Coverage.Border);
                foreach (var z in hints.ForbiddenZones)
                    AddBlockerZone(map, imminent, z);
                int maxGoal = AddPositionalGoal(map, targetPos, targetRadius, targetRot, positional, 0);
                if (maxGoal > 0)
                {
                    // try to find a path to quadrant
                    var pathfind = new ThetaStar(map, maxGoal, player.Position, 1.0f / playerSpeed);
                    int res = pathfind.Execute();
                    if (res >= 0)
                        return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = float.MaxValue, TimeToPositional = pathfind.NodeByIndex(res).GScore, Map = map, MapGoal = maxGoal };
                }

                // fail
                return new() { Destination = null, LeewaySeconds = float.MaxValue, TimeToPositional = float.MaxValue, Map = map, MapGoal = maxGoal };
            }

            return new() { Destination = null, LeewaySeconds = float.MaxValue, TimeToPositional = float.MaxValue };
        }

        public static DateTime ImminentExplosionTime(DateTime currentTime) => currentTime.AddSeconds(0.5);

        public static void AddBlockerZone(Map map, DateTime imminent, (AOEShape shape, WPos origin, Angle rot, DateTime activation) zone)
        {
            map.BlockPixels(map.Rasterize(zone.shape.Coverage(map, zone.origin, zone.rot)), MathF.Max(0, (float)(zone.activation - imminent).TotalSeconds), Map.Coverage.Inside | Map.Coverage.Border);
        }

        public static int AddTargetGoal(Map map, WPos targetPos, float targetRadius, Angle targetRot, Positional positional, int minPriority)
        {
            var adjPrio = map.AddGoal(map.RasterizeCircle(targetPos, targetRadius), Map.Coverage.Inside, minPriority, 1);
            if (adjPrio == minPriority)
                return minPriority;
            return AddPositionalGoal(map, targetPos, targetRadius, targetRot, positional, minPriority + 1);
        }

        public static int AddPositionalGoal(Map map, WPos targetPos, float targetRadius, Angle targetRot, Positional positional, int minPriority)
        {
            var adjPrio = minPriority;
            switch (positional)
            {
                case Positional.Flank:
                    var cvl = map.CoverageCone(targetPos, targetRadius, targetRot + 90.Degrees(), 45.Degrees());
                    var cvr = map.CoverageCone(targetPos, targetRadius, targetRot - 90.Degrees(), 45.Degrees());
                    adjPrio = map.AddGoal(map.Rasterize(p => cvl(p) | cvr(p)), Map.Coverage.Inside, minPriority, 1);
                    break;
                case Positional.Rear:
                    adjPrio = map.AddGoal(map.RasterizeCone(targetPos, targetRadius, targetRot + 180.Degrees(), 45.Degrees()), Map.Coverage.Inside, minPriority, 1);
                    break;
            }
            return adjPrio;
        }

        public static NavigationDecision? FindPathFromUnsafe(Map map, WPos startPos, int safeGoal, int maxGoal, float speed = 6)
        {
            if (maxGoal - safeGoal == 2)
            {
                // try finding path to flanking position
                var pathfind = new ThetaStar(map, maxGoal, startPos, 1.0f / speed);
                int res = pathfind.Execute();
                if (res >= 0)
                    return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = pathfind.NodeByIndex(res).PathLeeway, TimeToPositional = pathfind.NodeByIndex(res).GScore, Map = map, MapGoal = maxGoal };
                --maxGoal;
            }

            if (maxGoal - safeGoal == 1)
            {
                // try finding path to uptime position
                var pathfind = new ThetaStar(map, maxGoal, startPos, 1.0f / speed);
                int res = pathfind.Execute();
                if (res >= 0)
                    return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = pathfind.NodeByIndex(res).PathLeeway, TimeToPositional = float.MaxValue, Map = map, MapGoal = maxGoal };
                --maxGoal;
            }

            if (maxGoal - safeGoal == 0)
            {
                // try finding path to any safe spot
                var pathfind = new ThetaStar(map, maxGoal, startPos, 1.0f / speed);
                int res = pathfind.Execute();
                if (res >= 0)
                    return new() { Destination = GetFirstWaypoint(pathfind, res), LeewaySeconds = pathfind.NodeByIndex(res).PathLeeway, TimeToPositional = float.MaxValue, Map = map, MapGoal = maxGoal };
            }

            return null;
        }

        public static NavigationDecision FindPathFromImminent(Map map, WPos startPos, float speed = 6)
        {
            // just run to closest safe spot, if no good path can be found
            var pathfind = new ThetaStar(map, 0, startPos, 1.0f / speed);
            int res = pathfind.Execute();
            var dest = res >= 0 ? GetFirstWaypoint(pathfind, res)
                : map.EnumeratePixels().Where(p => { var px = map[p.x, p.y]; return px.Priority == 0 && px.MaxG == float.MaxValue; }).MinBy(p => (p.center - startPos).LengthSq()).center;
            return new() { Destination = dest, LeewaySeconds = 0, TimeToPositional = float.MaxValue, Map = map };
        }

        public static WPos? GetFirstWaypoint(ThetaStar pf, int cell)
        {
            if (pf.NodeByIndex(cell).GScore == 0)
                return null; // already at dest
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

        public static Map BuildPathfindingMap(AIHints hints, DateTime currentTime)
        {
            var map = hints.Bounds.BuildMap();
            var imminent = ImminentExplosionTime(currentTime);
            foreach (var z in hints.ForbiddenZones)
                AddBlockerZone(map, imminent, z);
            //if (hints.RestrictedZones.Count > 0)
            //{
            //    var min = hints.RestrictedZones[0].activation;
            //    if (min < imminent)
            //        min = imminent;
            //    var union = hints.RestrictedZones.TakeWhile(z => z.activation <= min).Select(z => z.shape.Coverage(map, z.origin, z.rot)).ToList();
            //    Func<WPos, Map.Coverage> unionFunc = p =>
            //    {
            //        Map.Coverage cv = Map.Coverage.None;
            //        foreach (var f in union)
            //            cv |= f(p);
            //        return cv.HasFlag(Map.Coverage.Inside) ? Map.Coverage.Inside : Map.Coverage.Outside;
            //    };
            //    map.BlockPixels(map.Rasterize(unionFunc), (float)(min - imminent).TotalSeconds, Map.Coverage.Outside);
            //}
            return map;
        }
    }
}
