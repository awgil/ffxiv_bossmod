using System.Buffers;
using System.Runtime.Intrinsics;
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

    // factors to account for:
    // 1. ping (of course)
    // 2. snapshot delay (probably 300ms for NPC casts, see NPCFinishDelay)
    // 3. unknown (variable) amount of delay between movement packet being sent and server deciding to update our position; can be bypassed (kind of) by forcing a position update through various means
    //  - jumping (has its own issues; player gets stuck in the air for a bit, you can't steer while jumping, etc)
    //  - sending position update packet (not an option, can't be done in the vanilla client, so it doesn't match our philosophy)
    public static float CushionFromRTT(float rtt) => rtt + 0.5f + ActorCastInfo.NPCFinishDelay;

    public static NavigationDecision Build(Context ctx, DateTime currentTime, AIHints hints, WPos playerPosition, float playerSpeed, float rtt, float spaceCushion, bool useOldRasterizer = false)
    {
        var startTime = DateTime.Now;

        hints.InitPathfindMap(ctx.Map);
        if (hints.ForbiddenZones.Count > 0)
        {
            if (useOldRasterizer)
                RasterizeForbiddenZonesOld(ctx.Map, hints.ForbiddenZones, currentTime, rtt, spaceCushion);
            else
                RasterizeForbiddenZones(ctx.Map, hints.ForbiddenZones, currentTime, rtt, spaceCushion);
        }

        var emergencyMode = false;
        var (pX, pZ) = ctx.Map.WorldToGrid(playerPosition);
        if (pX >= 0 && pX < ctx.Map.Width && pZ >= 0 && pZ < ctx.Map.Height)
            emergencyMode = ctx.Map.PixelMaxG[ctx.Map.GridToIndex(pX, pZ)] == 0;

        if (hints.GoalZones.Count > 0 && !emergencyMode)
            RasterizeGoalZones(ctx.Map, hints.GoalZones);

        var rasterFinish = DateTime.Now;

        // execute pathfinding
        ctx.ThetaStar.Start(ctx.Map, playerPosition, 1.0f / playerSpeed);
        var bestNodeIndex = ctx.ThetaStar.Execute();
        ref var bestNode = ref ctx.ThetaStar.NodeByIndex(bestNodeIndex);
        var waypoints = GetFirstWaypoints(ctx.ThetaStar, ctx.Map, bestNodeIndex, playerPosition);
        var finishTime = DateTime.Now;
        return new NavigationDecision() { Destination = waypoints.first, NextWaypoint = waypoints.second, LeewaySeconds = bestNode.PathLeeway, TimeToGoal = bestNode.GScore, PathfindTime = finishTime - rasterFinish, RasterizeTime = rasterFinish - startTime, TotalTime = finishTime - startTime };
    }

    public static Task<NavigationDecision> BuildAsync(Context ctx, DateTime currentTime, AIHints hints, WPos playerPos, float playerSpeed, float rtt, float spaceCushion)
    {
        var hintsCopy = new AIHints()
        {
            PathfindMapBounds = hints.PathfindMapBounds,
            PathfindMapCenter = hints.PathfindMapCenter,
            PathfindMapObstacles = hints.PathfindMapObstacles,
            TemporaryObstacles = [.. hints.TemporaryObstacles],
            Portals = [.. hints.Portals],
            ForbiddenZones = [.. hints.ForbiddenZones],
            GoalZones = [.. hints.GoalZones],
        };
        return Task.Run(() => Build(ctx, currentTime, hintsCopy, playerPos, playerSpeed, rtt, spaceCushion));
    }

    public static void RasterizeForbiddenZones(Map map, List<(Sdf distance, DateTime activation, ulong source)> zones, DateTime current, float rtt, float cushion, List<float>? producedScratch = null)
    {
        // very slight difference in activation times cause issues for pathfinding - cluster them together
        var zonesFixed = new (Sdf distance, float g)[zones.Count];
        DateTime clusterEnd = default, globalStart = current, globalEnd = current.AddSeconds(120);
        float clusterG = 0;
        var i = 0;
        foreach (var zone in zones)
        {
            var activation = zone.activation.Clamp(globalStart, globalEnd);
            if (activation > clusterEnd)
            {
                clusterG = ActivationToG(activation, current, rtt);
                clusterEnd = activation.AddSeconds(0.5f);
            }
            zonesFixed[i++] = (zone.distance, clusterG);
        }

        map.MaxG = clusterG;

        RasterizeRaw(map, zonesFixed, cushion, producedScratch);
    }

    public static void RasterizeRaw(Map map, (Sdf sdf, float g)[] zones, float cushion, List<float>? producedScratch = null)
    {
        var rowLength = ((map.Width + 1) / VFloat.Count + 1) * VFloat.Count;
        var dim = rowLength * (map.Height + 1);

        var gScratch = ArrayPool<float>.Shared.Rent(dim);
        var dScratch = ArrayPool<int>.Shared.Rent(dim);

        var dy = map.LocalZDivRes * map.Resolution * map.Resolution;
        var dx = dy.OrthoL();
        var cy = map.Center - map.Width / 2 * dx - map.Height / 2 * dy;

        Parallel.For(0, map.Height + 1, y =>
        {
            var vz = VV.Create(cy.Z + y * dy.Z);

            for (var x = 0; x <= map.Width; x += VFloat.Count)
            {
                var iCell = y * rowLength + x;

                var (g, d) = CalculateMaxGV(zones, new(VV.CreateSequence(cy.X + x * dx.X, dx.X), vz), cushion);

                g.CopyTo(gScratch, iCell);
                d.CopyTo(dScratch, iCell);
            }
        });

        var maxG = map.MaxPriority = float.MinValue;

        // set each cell to the min of its corners
        // track max cell priority seen (if most of the map is blocked and the user has requested a pathfinding cushion, best unblocked pixel might be e.g. -0.5 instead of 0)
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var iG = y * rowLength + x;
                var iM = y * map.Width + x;

                var cellG = map.PixelMaxG[iM] = Math.Min(
                    map.PixelMaxG[iM],
                    Math.Min(
                        Math.Min(gScratch[iG], gScratch[iG + 1]),
                        Math.Min(gScratch[iG + rowLength], gScratch[iG + rowLength + 1])
                    )
                );
                maxG = MathF.Max(maxG, cellG);

                if (cellG < float.MaxValue)
                    map.PixelPriority[iM] = float.MinValue;
                else if ((dScratch[iG] | dScratch[iG + 1] | dScratch[iG + rowLength] | dScratch[iG + rowLength + 1]) != 0)
                    map.PixelPriority[iM] = -CushionDepriority;

                map.MaxPriority = Math.Max(map.PixelPriority[iM], map.MaxPriority);
            }
        }

        // whole grid is blocked, unblock cells with highest gscore so pathfinding produces a reasonable result
        if (maxG < float.MaxValue)
        {
            for (var j = 0; j < map.Width * map.Height; j++)
                if (map.PixelMaxG[j] == maxG)
                {
                    map.PixelMaxG[j] = float.MaxValue;
                    map.PixelPriority[j] = 0;
                }
        }

        producedScratch?.AddRange(gScratch);

        ArrayPool<float>.Shared.Return(gScratch);
        ArrayPool<int>.Shared.Return(dScratch);
    }

    public static void RasterizeGoalZones(Map map, List<GoalFn> goals)
    {
        var rowLength = ((map.Width + 1) / VFloat.Count + 1) * VFloat.Count;
        var dim = rowLength * (map.Height + 1);

        var gScratch = ArrayPool<float>.Shared.Rent(dim);

        var dy = map.LocalZDivRes * map.Resolution * map.Resolution;
        var dx = dy.OrthoL();
        var cy = map.Center - map.Width / 2 * dx - map.Height / 2 * dy;

        Parallel.For(0, map.Height + 1, y =>
        {
            var vz = VV.Create(cy.Z + y * dy.Z);

            for (var x = 0; x <= map.Width; x += VFloat.Count)
            {
                var iCell = y * rowLength + x;

                var score = VFloat.Zero;
                foreach (var goal in goals)
                    score += goal(new(VV.CreateSequence(cy.X + x * dx.X, dx.X), vz));

                score.CopyTo(gScratch, iCell);
            }
        });

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var iG = y * rowLength + x;
                var iM = y * map.Width + x;

                if (map.PixelMaxG[iM] == float.MaxValue)
                {
                    var cellP = map.PixelPriority[iM] = map.PixelPriority[iM] + Math.Min(
                        Math.Min(gScratch[iG], gScratch[iG + 1]),
                        Math.Min(gScratch[iG + rowLength], gScratch[iG + rowLength + 1])
                    );
                    map.MaxPriority = MathF.Max(map.MaxPriority, cellP);
                }
                else
                    map.PixelPriority[iM] = float.MinValue;
            }
        }

        ArrayPool<float>.Shared.Return(gScratch);
    }

    public static void RasterizeForbiddenZonesOld(Map map, List<(Sdf distance, DateTime activation, ulong source)> zones, DateTime current, float rtt, float cushion)
    {
        // very slight difference in activation times cause issues for pathfinding - cluster them together
        var zonesFixed = new (Sdf distance, float g)[zones.Count];
        DateTime clusterEnd = default, globalStart = current, globalEnd = current.AddSeconds(120);
        float clusterG = 0;
        for (var i = 0; i < zonesFixed.Length; ++i)
        {
            var activation = zones[i].activation.Clamp(globalStart, globalEnd);
            if (activation > clusterEnd)
            {
                clusterG = ActivationToG(activation, current, rtt);
                clusterEnd = activation.AddSeconds(0.5f);
            }
            zonesFixed[i] = (zones[i].distance, clusterG);
        }

        // note that a zone can partially intersect a pixel; so what we do is check each corner and set the maxg value of a pixel equal to the minimum of 4 corners
        // to avoid 4x calculations, we do a slightly tricky loop:
        // - outer loop fills row i to with g values corresponding to the 'upper edge' of cell i
        // - inner loop calculates the g value at the left border, then iterates over all right corners and fills minimums of two g values to the cells
        // - second outer loop calculates values at 'bottom' edge and then updates the values of all cells to correspond to the cells rather than edges
        map.MaxG = clusterG;
        var gScratch = new float[map.PixelMaxG.Length];
        var dScratch = new uint[map.PixelMaxG.Length];

        var numBlockedCells = 0;

        // see Map.EnumeratePixels, note that we care about corners rather than centers
        var dy = map.LocalZDivRes * map.Resolution * map.Resolution;
        var dx = dy.OrthoL();
        var cy = map.Center - map.Width / 2 * dx - map.Height / 2 * dy;

        var iCell = 0;
        for (var y = 0; y < map.Height; ++y)
        {
            var cx = cy;
            var (leftG, leftD) = CalculateMaxG(-1, zonesFixed, cx, cushion);
            for (var x = 0; x < map.Width; ++x)
            {
                cx += dx;
                var (rightG, rightD) = CalculateMaxG(-1, zonesFixed, cx, cushion);
                dScratch[iCell] = leftD || rightD ? 1u : 0;
                gScratch[iCell++] = Math.Min(leftG, rightG);
                leftD = rightD;
                leftG = rightG;
            }
            cy += dy;
        }
        var (bleftG, bleftD) = CalculateMaxG(-1, zonesFixed, cy, cushion);
        iCell -= map.Width;
        for (var x = 0; x < map.Width; ++x, ++iCell)
        {
            cy += dx;
            var (brightG, brightD) = CalculateMaxG(-1, zonesFixed, cy, cushion);
            var bottomD = bleftD || brightD;
            var bottomG = Math.Min(bleftG, brightG);
            var jCell = iCell;
            for (var y = map.Height; y > 0; --y, jCell -= map.Width)
            {
                var topG = gScratch[jCell];
                var topD = dScratch[jCell];
                var cellG = map.PixelMaxG[jCell] = Math.Min(Math.Min(topG, bottomG), map.PixelMaxG[jCell]);
                if (cellG != float.MaxValue)
                {
                    map.PixelPriority[jCell] = float.MinValue;
                    ++numBlockedCells;
                }
                else if (bottomD || topD > 0)
                    map.PixelPriority[jCell] -= CushionDepriority;
                bottomD = topD > 0;
                bottomG = topG;
            }
            bleftD = brightD;
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

    private static (float G, bool D) CalculateMaxG(int cell, (Sdf d, float g)[] zones, WPos p, float cushion)
    {
        var g = float.MaxValue;
        var d = false;

        foreach (var z in zones)
        {
            var dist = z.d.Type == Sdf.SdfType.Indexed
                ? z.d.CheckIndexed(p, cell) ? float.MinValue : float.MaxValue
                : z.d.Distance(p);
            if (!float.IsFinite(dist))
                throw new InvalidOperationException($"distance function returned invalid value {dist}");
            if (dist < 0)
            {
                g = z.g;
                break;
            }

            if (dist < cushion)
                d = true;
        }
        return (g, d);
    }

    private static (VFloat G, VBool D) CalculateMaxGV((Sdf d, float g)[] zones, WVec p, float cushion)
    {
        var g = VV.Create(float.MaxValue);
        var d = VBool.Zero;
        var c0 = VV.Create(cushion);

        foreach (var z in zones)
        {
            var dist = z.d.Distance(p);
            g = VV.Min(g, VV.ConditionalSelect(dist < VFloat.Zero, VV.Create(z.g), VV.Create(float.MaxValue)));
            d |= dist < c0;
        }

        return (g, d);
    }

    private static float ActivationToG(DateTime activation, DateTime current, float rtt) => MathF.Max(0, (float)(activation - current).TotalSeconds - CushionFromRTT(rtt));

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
