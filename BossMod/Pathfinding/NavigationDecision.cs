using System.Threading.Tasks;

namespace BossMod.Pathfinding;

// utility for selecting player's navigation target
// there are several goals that navigation has to meet, in following rough priority
// 1. stay away from aoes; tricky thing is that sometimes it is ok to temporarily enter aoe, if we're sure we'll exit it in time
// 2. maintain uptime - this is represented by being in specified range of specified target, and not moving to interrupt casts unless needed
// 3. execute positionals - this is strictly less important than points above, we only do that if we can meet other conditions
// 4. be in range of healers - even less important, but still nice to do
public struct NavigationDecision
{
    // context that allows reusing large memory allocations
    public class Context
    {
        public float[] Scratch = [];
        public Map Map = new();
        public ThetaStar ThetaStar = new();
    }

    public WPos? Destination;
    public WPos? NextWaypoint;
    //public float NextTurn; // > 0 if we turn left after reaching first waypoint, < 0 if we turn right, 0 otherwise (no more waypoints)
    public float LeewaySeconds; // can be used for finishing casts / slidecasting etc.
    public float TimeToGoal;

    public TimeSpan RasterizeTime;
    public TimeSpan PathfindTime;
    public TimeSpan TotalTime;

    public const float ForbiddenZoneCushion = 0; // increase to fatten forbidden zones

    // reduce time between now and activation by this value in seconds; increase for more conservativeness
    public static readonly float ActivationTimeCushion = Service.IsDev
        ? ActorCastInfo.NPCFinishDelay + 0.2f
        : 1;

    public static NavigationDecision Build(Context ctx, DateTime currentTime, AIHints hints, WPos playerPosition, float playerSpeed = 6, float forbiddenZoneCushion = ForbiddenZoneCushion)
    {
        var startTime = DateTime.Now;

        hints.InitPathfindMap(ctx.Map);
        if (hints.ForbiddenZones.Count > 0)
            RasterizeForbiddenZones(ctx.Map, hints.ForbiddenZones, currentTime, ref ctx.Scratch);
        if (hints.GoalZones.Count > 0)
            RasterizeGoalZones(ctx.Map, hints.GoalZones);

        if (forbiddenZoneCushion > 0)
            AvoidForbiddenZone(ctx.Map, forbiddenZoneCushion);

        var rasterFinish = DateTime.Now;

        // execute pathfinding
        ctx.ThetaStar.Start(ctx.Map, playerPosition, 1.0f / playerSpeed);
        var bestNodeIndex = ctx.ThetaStar.Execute();
        ref var bestNode = ref ctx.ThetaStar.NodeByIndex(bestNodeIndex);
        var waypoints = GetFirstWaypoints(ctx.ThetaStar, ctx.Map, bestNodeIndex, playerPosition);
        var finishTime = DateTime.Now;
        return new NavigationDecision() { Destination = waypoints.first, NextWaypoint = waypoints.second, LeewaySeconds = bestNode.PathLeeway, TimeToGoal = bestNode.GScore, PathfindTime = finishTime - rasterFinish, RasterizeTime = rasterFinish - startTime, TotalTime = finishTime - startTime };
    }

    public static Task<NavigationDecision> BuildAsync(Context ctx, DateTime currentTime, AIHints hints, WPos playerPos, float playerSpeed = 6, float forbiddenZoneCushion = ForbiddenZoneCushion)
    {
        var hintsCopy = new AIHints()
        {
            PathfindMapBounds = hints.PathfindMapBounds,
            PathfindMapCenter = hints.PathfindMapCenter,
            PathfindMapObstacles = hints.PathfindMapObstacles,
            TemporaryObstacles = [.. hints.TemporaryObstacles],
            Portals = [.. hints.Portals],
            ForbiddenZones = [.. hints.ForbiddenZones],
            GoalZones = [.. hints.GoalZones]
        };
        return Task.Run(() => Build(ctx, currentTime, hintsCopy, playerPos, playerSpeed, forbiddenZoneCushion));
    }

    public static void AvoidForbiddenZone(Map map, float forbiddenZoneCushion)
    {
        int d = (int)(forbiddenZoneCushion / map.Resolution);
        map.MaxPriority = -1;
        foreach (var (x, y, _) in map.EnumeratePixels())
        {
            var cellIndex = map.GridToIndex(x, y);
            if (map.PixelMaxG[cellIndex] == float.MaxValue)
            {
                var neighbourhood = new[]
                {
                    (x + d, y), (x - d, y), (x, y + d), (x, y - d),
                    (x + d, y + d), (x - d, y + d), (x + d, y - d), (x - d, y - d)
                };
                if (neighbourhood.Any(p => map.PixelMaxG[map.GridToIndex(map.ClampToGrid(p))] != float.MaxValue))
                {
                    map.PixelPriority[cellIndex] -= 0.125f;
                }
            }
            map.MaxPriority = Math.Max(map.MaxPriority, map.PixelPriority[cellIndex]);
        }
    }

    public static void RasterizeForbiddenZones(Map map, List<(Func<WPos, bool> containsFn, DateTime activation, ulong source)> zones, DateTime current, ref float[] scratch)
    {
        // very slight difference in activation times cause issues for pathfinding - cluster them together
        var zonesFixed = new (Func<WPos, bool> containsFn, float g)[zones.Count];
        DateTime clusterEnd = default, globalStart = current, globalEnd = current.AddSeconds(120);
        float clusterG = 0;
        for (int i = 0; i < zonesFixed.Length; ++i)
        {
            var activation = zones[i].activation.Clamp(globalStart, globalEnd);
            if (activation > clusterEnd)
            {
                clusterG = ActivationToG(activation, current);
                clusterEnd = activation.AddSeconds(0.5f);
            }
            zonesFixed[i] = (zones[i].containsFn, clusterG);
        }

        // note that a zone can partially intersect a pixel; so what we do is check each corner and set the maxg value of a pixel equal to the minimum of 4 corners
        // to avoid 4x calculations, we do a slightly tricky loop:
        // - outer loop fills row i to with g values corresponding to the 'upper edge' of cell i
        // - inner loop calculates the g value at the left border, then iterates over all right corners and fills minimums of two g values to the cells
        // - second outer loop calculates values at 'bottom' edge and then updates the values of all cells to correspond to the cells rather than edges
        map.MaxG = clusterG;
        if (scratch.Length < map.PixelMaxG.Length)
            scratch = new float[map.PixelMaxG.Length];
        var numBlockedCells = 0;

        // see Map.EnumeratePixels, note that we care about corners rather than centers
        var dy = map.LocalZDivRes * map.Resolution * map.Resolution;
        var dx = dy.OrthoL();
        var cy = map.Center - map.Width / 2 * dx - map.Height / 2 * dy;

        int iCell = 0;
        for (int y = 0; y < map.Height; ++y)
        {
            var cx = cy;
            var leftG = CalculateMaxG(zonesFixed, cx);
            for (int x = 0; x < map.Width; ++x)
            {
                cx += dx;
                var rightG = CalculateMaxG(zonesFixed, cx);
                scratch[iCell++] = Math.Min(leftG, rightG);
                leftG = rightG;
            }
            cy += dy;
        }
        var bleftG = CalculateMaxG(zonesFixed, cy);
        iCell -= map.Width;
        for (int x = 0; x < map.Width; ++x, ++iCell)
        {
            cy += dx;
            var brightG = CalculateMaxG(zonesFixed, cy);
            var bottomG = Math.Min(bleftG, brightG);
            var jCell = iCell;
            for (int y = map.Height; y > 0; --y, jCell -= map.Width)
            {
                var topG = scratch[jCell];
                var cellG = map.PixelMaxG[jCell] = Math.Min(Math.Min(topG, bottomG), map.PixelMaxG[jCell]);
                if (cellG != float.MaxValue)
                {
                    map.PixelPriority[jCell] = float.MinValue; // mark here, in case we don't have goals
                    ++numBlockedCells;
                }
                bottomG = topG;
            }
            bleftG = brightG;
        }

        if (numBlockedCells == map.Width * map.Height)
        {
            // everything is dangerous, clear least dangerous so that pathfinding works reasonably
            // note that max value could be smaller than MaxG, if more dangerous stuff overlaps it
            float realMaxG = 0;
            for (iCell = 0; iCell < numBlockedCells; ++iCell)
                realMaxG = Math.Max(realMaxG, map.PixelMaxG[iCell]);
            for (iCell = 0; iCell < numBlockedCells; ++iCell)
            {
                if (map.PixelMaxG[iCell] == realMaxG)
                {
                    map.PixelMaxG[iCell] = float.MaxValue;
                    map.PixelPriority[iCell] = 0;
                }
            }
        }
    }

    public static void RasterizeGoalZones(Map map, List<Func<WPos, float>> goals)
    {
        // see Map.EnumeratePixels, note that we care about corners rather than centers
        var dy = map.LocalZDivRes * map.Resolution * map.Resolution;
        var dx = dy.OrthoL();
        var cy = map.Center - map.Width / 2 * dx - map.Height / 2 * dy;

        int iCell = 0;
        for (int y = 0; y < map.Height; ++y)
        {
            var cx = cy;
            var leftP = goals.Sum(g => g(cx));
            for (int x = 0; x < map.Width; ++x)
            {
                cx += dx;
                var rightP = goals.Sum(g => g(cx));
                map.PixelPriority[iCell++] = Math.Min(leftP, rightP);
                leftP = rightP;
            }
            cy += dy;
        }
        var bleftP = goals.Sum(g => g(cy));
        iCell -= map.Width;
        for (int x = 0; x < map.Width; ++x, ++iCell)
        {
            cy += dx;
            var brightP = goals.Sum(g => g(cy));
            var bottomP = Math.Min(bleftP, brightP);
            var jCell = iCell;
            for (int y = map.Height; y > 0; --y, jCell -= map.Width)
            {
                var topP = map.PixelPriority[jCell];
                if (map.PixelMaxG[jCell] == float.MaxValue)
                {
                    var cellP = map.PixelPriority[jCell] = Math.Min(topP, bottomP);
                    map.MaxPriority = Math.Max(map.MaxPriority, cellP);
                }
                else
                {
                    map.PixelPriority[jCell] = float.MinValue;
                }
                bottomP = topP;
            }
            bleftP = brightP;
        }
    }

    private static float ActivationToG(DateTime activation, DateTime current) => MathF.Max(0, (float)(activation - current).TotalSeconds - ActivationTimeCushion);

    private static float CalculateMaxG(Span<(Func<WPos, bool> containsFn, float g)> zones, WPos p)
    {
        foreach (ref var z in zones)
            if (z.containsFn(p))
                return z.g;
        return float.MaxValue;
    }

    public static (WPos? first, WPos? second) GetFirstWaypoints(ThetaStar pf, Map map, int cell, WPos startingPos)
    {
        ref var startingNode = ref pf.NodeByIndex(cell);
        if (startingNode.GScore == 0 && startingNode.PathMinG == float.MaxValue)
            return (null, null); // we're already in safe zone

        var nextCell = cell;
        do
        {
            ref var node = ref pf.NodeByIndex(cell);
            if (pf.NodeByIndex(node.ParentIndex).GScore == 0)
            {
                //var dest = pf.CellCenter(cell);
                // if destination coord matches player coord, do not move along that coordinate, this is used for precise positioning
                var destCoord = map.IndexToGrid(cell);
                var playerCoordFrac = map.WorldToGridFrac(startingPos);
                var playerCoord = map.FracToGrid(playerCoordFrac);
                var dest = map.GridToWorld(destCoord.x, destCoord.y, destCoord.x == playerCoord.x ? playerCoordFrac.X - playerCoord.x : 0.5f, destCoord.y == playerCoord.y ? playerCoordFrac.Y - playerCoord.y : 0.5f);

                var next = pf.CellCenter(nextCell);
                return (dest, next);
            }
            nextCell = cell;
            cell = node.ParentIndex;
        }
        while (true);
    }
}
