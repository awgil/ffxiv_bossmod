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
        public float[] ScratchG = [];
        public bool[] ScratchD = [];
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

    public const float CushionDepriority = 0.5f; // should be lower than any priority specifically added by rotation modules

    // reduce time between now and activation by this value in seconds; increase for more conservativeness
    public static readonly float ActivationTimeCushion = Service.IsDev
        ? ActorCastInfo.NPCFinishDelay + 0.3f
        : 1;

    public static NavigationDecision Build(Context ctx, DateTime currentTime, AIHints hints, WPos playerPosition, float playerSpeed = 6, float forbiddenZoneCushion = 0)
    {
        var startTime = DateTime.Now;

        hints.InitPathfindMap(ctx.Map);
        if (hints.ForbiddenZones.Count > 0)
            RasterizeForbiddenZones(ctx.Map, hints.ForbiddenZones, currentTime, ref ctx.ScratchG, ref ctx.ScratchD, forbiddenZoneCushion);
        if (hints.GoalZones.Count > 0)
            RasterizeGoalZones(ctx.Map, hints.GoalZones, forbiddenZoneCushion > 0);
        else if (forbiddenZoneCushion > 0)
            AddCushion(ctx.Map);

        var rasterFinish = DateTime.Now;

        // execute pathfinding
        ctx.ThetaStar.Start(ctx.Map, playerPosition, 1.0f / playerSpeed);
        var bestNodeIndex = ctx.ThetaStar.Execute();
        ref var bestNode = ref ctx.ThetaStar.NodeByIndex(bestNodeIndex);
        var waypoints = GetFirstWaypoints(ctx.ThetaStar, ctx.Map, bestNodeIndex, playerPosition);
        var finishTime = DateTime.Now;
        return new NavigationDecision() { Destination = waypoints.first, NextWaypoint = waypoints.second, LeewaySeconds = bestNode.PathLeeway, TimeToGoal = bestNode.GScore, PathfindTime = finishTime - rasterFinish, RasterizeTime = rasterFinish - startTime, TotalTime = finishTime - startTime };
    }

    public static Task<NavigationDecision> BuildAsync(Context ctx, DateTime currentTime, AIHints hints, WPos playerPos, float playerSpeed, float forbiddenZoneCushion)
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

    public static void RasterizeForbiddenZones(Map map, List<(Sdf distance, DateTime activation, ulong source)> zones, DateTime current, ref float[] gScratch, ref bool[] dScratch, float cushion = 0)
    {
        // very slight difference in activation times cause issues for pathfinding - cluster them together
        var zonesFixed = new (Sdf distance, float g)[zones.Count];
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
            zonesFixed[i] = (zones[i].distance, clusterG);
        }

        map.MaxG = clusterG;
        var lenPlus1 = (map.Width + 1) * (map.Height + 1);
        if (gScratch.Length < lenPlus1)
            gScratch = new float[lenPlus1];
        if (dScratch.Length < lenPlus1)
            dScratch = new bool[lenPlus1];

        // TODO: group continuous sdfs with same gscore together
        foreach (var (d, g) in zonesFixed)
        {
            // all gscores <= 0 are equivalent so we use min as a sentinel value
            // note that dScratch doesn't get reset since the cushion effect applies to all zones, not just the most imminent
            Array.Fill(gScratch, float.MinValue);

            RasterizeForbiddenZone(map, d, g, ref gScratch, ref dScratch, cushion);
        }

        // whole grid is blocked, unblock cells with highest gscore so pathfinding produces a reasonable result
        var realMaxG = map.PixelMaxG.Max();
        if (realMaxG < float.MaxValue)
            for (var i = 0; i < map.PixelMaxG.Length; i++)
                if (map.PixelMaxG[i] == realMaxG)
                {
                    map.PixelMaxG[i] = float.MaxValue;
                    map.PixelPriority[i] = 0;
                }
    }

    private static void RasterizeForbiddenZone(Map map, in Sdf sdf, float g, ref float[] gScratch, ref bool[] dScratch, float cushion)
    {
        var discrete = !sdf.IsContinuous;

        var dy = map.LocalZDivRes * map.Resolution * map.Resolution;
        var dx = dy.OrthoL();
        var cy = map.Center - map.Width / 2 * dx - map.Height / 2 * dy;

        for (var y = 0; y <= map.Height; y++)
        {
            for (var x = 0; x <= map.Width; x++)
            {
                var iCell = y * (map.Width + 1) + x;

                // cell is already blocked by a more dangerous zone
                // TODO: this adds a 1-pixel-wide cushion with the current gscore to previously rasterized zones if they happen to be axis aligned
                //if (y < map.Height && x < map.Width && map.PixelMaxG[iCell] < g)
                //    continue;

                // filled by previous iteration
                if (gScratch[iCell] != float.MinValue)
                    continue;

                var point = cy + x * dx + y * dy;

                if (discrete)
                {
                    gScratch[iCell] = sdf.Check(point) ? g : float.MaxValue;
                    continue;
                }

                var distance = sdf.Distance(point);
                var toRowEnd = map.Width - x;

                int distPixels;

                if (distance >= cushion)
                {
                    // TODO optimize; this drastically increases the number of sdf evaluations since they need to be executed for each grid point within the cushion zone
                    distPixels = (int)((distance - cushion) / map.Resolution);
                    Array.Fill(gScratch, float.MaxValue, iCell, Math.Min(distPixels, toRowEnd) + 1);
                }
                else if (cushion > 0 && distance >= 0)
                {
                    dScratch[iCell] = true;
                    gScratch[iCell] = float.MaxValue;
                }
                else
                {
                    distPixels = (int)(distance / map.Resolution);
                    Array.Fill(gScratch, g, iCell, Math.Min(-distPixels, toRowEnd) + 1);
                }
            }
        }

        for (var y = 0; y < map.Height; y++)
            for (var x = 0; x < map.Width; x++)
            {
                var iG = y * (map.Width + 1) + x;
                var iM = y * map.Width + x;

                if (map.PixelMaxG[iM] < float.MaxValue)
                    continue;

                var cellG = map.PixelMaxG[iM] = Math.Min(
                    Math.Min(gScratch[iG], gScratch[iG + 1]),
                    Math.Min(gScratch[iG + map.Width + 1], gScratch[iG + map.Width + 2])
                );
                if (cellG < float.MaxValue)
                    map.PixelPriority[iM] = 0;

                if (dScratch[iG] || dScratch[iG + 1] || dScratch[iG + map.Width + 1] || dScratch[iG + map.Width + 2])
                    map.PixelAvoid[iM] = true;
            }
    }

    public static void RasterizeGoalZones(Map map, List<Func<WPos, float>> goals, bool cushion)
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
                    // TODO: is there a way to track whether this pixel has been previously avoided without creating a whole extra array to store that info?
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

        if (cushion)
            AddCushion(map);
    }

    public static void AddCushion(Map map)
    {
        var pMax = float.MinValue;
        for (var i = 0; i < map.Width * map.Height; i++)
        {
            if (map.PixelPriority[i] >= 0 && map.PixelAvoid[i])
                map.PixelPriority[i] -= CushionDepriority;
            pMax = Math.Max(map.PixelPriority[i], pMax);
        }
        map.MaxPriority = pMax;
    }

    private static float ActivationToG(DateTime activation, DateTime current) => MathF.Max(0, (float)(activation - current).TotalSeconds - ActivationTimeCushion);

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
