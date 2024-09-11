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
    public float LeewaySeconds; // can be used for finishing casts / slidecasting etc.
    public float TimeToGoal;
    public Map? Map;
    public WPos GoalPos;
    public float GoalRadius;

    public const float ForbiddenZoneCushion = 0; // increase to fatten forbidden zones
    public const float ActivationTimeCushion = 1; // reduce time between now and activation by this value in seconds; increase for more conservativeness

    public static NavigationDecision Build(Context ctx, WorldState ws, AIHints hints, Actor player, WPos? targetPos, float targetRadius, Angle targetRot, Positional positional, float playerSpeed = 6)
    {
        // build a pathfinding map: rasterize all forbidden zones and goals
        hints.Bounds.PathfindMap(ctx.Map, hints.Center);
        if (hints.ForbiddenZones.Count > 0)
            RasterizeForbiddenZones(ctx.Map, hints.ForbiddenZones, ws.CurrentTime, ref ctx.Scratch);
        if (targetPos != null)
            RasterizeGoalZones(ctx.Map, targetPos.Value, targetRadius, targetRot, positional);

        // execute pathfinding
        var goalPos = hints.PathfindingHintDestination ?? targetPos ?? hints.Center;
        var goalRadius = hints.PathfindingHintRadius ?? targetRadius;
        ctx.ThetaStar.Start(ctx.Map, player.Position, goalPos, goalRadius, 1.0f / playerSpeed);
        var bestNodeIndex = ctx.ThetaStar.Execute();
        ref var bestNode = ref ctx.ThetaStar.NodeByIndex(bestNodeIndex);
        return new() { Destination = GetFirstWaypoint(ctx.ThetaStar, ctx.Map, bestNodeIndex), LeewaySeconds = bestNode.PathLeeway, TimeToGoal = bestNode.GScore, Map = ctx.Map, GoalPos = goalPos, GoalRadius = goalRadius };
    }

    public static void RasterizeForbiddenZones(Map map, List<(Func<WPos, float> shapeDistance, DateTime activation)> zones, DateTime current, ref float[] scratch)
    {
        // very slight difference in activation times cause issues for pathfinding - cluster them together
        (Func<WPos, float> shapeDistance, DateTime activation)[] zonesFixed = [.. zones];
        DateTime clusterStart = default, clusterEnd = default, globalStart = current, globalEnd = current.AddSeconds(120);
        foreach (ref var z in zonesFixed.AsSpan())
        {
            if (z.activation < globalStart)
                z.activation = globalStart;
            else if (z.activation > globalEnd)
                z.activation = globalEnd;

            if (z.activation < clusterEnd)
            {
                z.activation = clusterStart;
            }
            else
            {
                clusterStart = z.activation;
                clusterEnd = z.activation.AddSeconds(0.5f);
            }
        }

        // note that a zone can partially intersect a pixel; so what we do is check each corner and set the maxg value of a pixel equal to the minimum of 4 corners
        // to avoid 4x calculations, we do a slightly tricky loop:
        // - outer loop fills row i to with g values corresponding to the 'upper edge' of cell i
        // - inner loop calculates the g value at the left border, then iterates over all right corners and fills minimums of two g values to the cells
        // - second outer loop calculates values at 'bottom' edge and then updates the values of all cells to correspond to the cells rather than edges
        map.MaxG = zonesFixed.Length > 0 ? ActivationToG(zonesFixed[^1].activation, current) : 0;
        if (scratch.Length < map.PixelMaxG.Length)
            scratch = new float[map.PixelMaxG.Length];

        // see Map.EnumeratePixels, note that we care about corners rather than centers
        var dy = map.LocalZDivRes * map.Resolution * map.Resolution;
        var dx = dy.OrthoL();
        var cy = map.Center - map.Width / 2 * dx - map.Height / 2 * dy;

        int iCell = 0;
        for (int y = 0; y < map.Height; ++y)
        {
            var cx = cy;
            var leftG = CalculateMaxG(zonesFixed, map, cx, current);
            for (int x = 0; x < map.Width; ++x)
            {
                cx += dx;
                var rightG = CalculateMaxG(zonesFixed, map, cx, current);
                scratch[iCell++] = Math.Min(leftG, rightG);
                leftG = rightG;
            }
            cy += dy;
        }
        var bleftG = CalculateMaxG(zonesFixed, map, cy, current);
        iCell -= map.Width;
        for (int x = 0; x < map.Width; ++x, ++iCell)
        {
            cy += dx;
            var brightG = CalculateMaxG(zonesFixed, map, cy, current);
            var bottomG = Math.Min(bleftG, brightG);
            var jCell = iCell;
            for (int y = map.Height; y > 0; --y, jCell -= map.Width)
            {
                var topG = scratch[jCell];
                var cellG = map.PixelMaxG[jCell] = Math.Min(Math.Min(topG, bottomG), map.PixelMaxG[jCell]);
                if (cellG != float.MaxValue)
                    map.PixelPriority[jCell] = sbyte.MinValue;
                bottomG = topG;
            }
            bleftG = brightG;
        }
    }

    public static void RasterizeGoalZones(Map map, WPos targetPos, float targetRadius, Angle targetRot, Positional positional)
    {
        var (xc, yc) = map.WorldToGrid(targetPos);
        var halfSize = (int)(targetRadius / map.Resolution) + 2;
        var (x0, y0) = map.ClampToGrid((xc - halfSize, yc - halfSize));
        var (x1, y1) = map.ClampToGrid((xc + halfSize, yc + halfSize));

        // see Map.EnumeratePixels, note that we care about corners rather than centers
        var dy = map.LocalZDivRes * map.Resolution * map.Resolution;
        var dx = dy.OrthoL();
        var cy = map.Center + (x0 - map.Width / 2) * dx + (y0 - map.Height / 2) * dy;

        var targetDir = targetRot.ToDirection();
        for (int y = y0; y <= y1; ++y)
        {
            var cx = cy;
            var leftP = CalculateGoalPriority(targetPos, targetRadius, targetDir, positional, map, cx);
            var iCell = map.GridToIndex(x0, y);
            for (int x = x0; x <= x1; ++x)
            {
                cx += dx;
                var rightP = CalculateGoalPriority(targetPos, targetRadius, targetDir, positional, map, cx);
                map.PixelPriority[iCell++] = Math.Min(leftP, rightP);
                leftP = rightP;
            }
            cy += dy;
        }
        var bleftP = CalculateGoalPriority(targetPos, targetRadius, targetDir, positional, map, cy);
        var bCell = map.GridToIndex(x0, y1);
        for (int x = x0; x <= x1; ++x, ++bCell)
        {
            cy += dx;
            var brightP = CalculateGoalPriority(targetPos, targetRadius, targetDir, positional, map, cy);
            var bottomP = Math.Min(bleftP, brightP);
            var iCell = bCell;
            for (int y = y1; y >= y0; --y, iCell -= map.Width)
            {
                var topP = map.PixelPriority[iCell];
                if (map.PixelMaxG[iCell] == float.MaxValue)
                {
                    var cellP = map.PixelPriority[iCell] = Math.Min(topP, bottomP);
                    map.MaxPriority = Math.Max(map.MaxPriority, cellP);
                }
                else
                {
                    map.PixelPriority[iCell] = sbyte.MinValue;
                }
                bottomP = topP;
            }
            bleftP = brightP;
        }
    }

    private static float ActivationToG(DateTime activation, DateTime current) => MathF.Max(0, (float)(activation - current).TotalSeconds - ActivationTimeCushion);

    private static float CalculateMaxG(Span<(Func<WPos, float> shapeDistance, DateTime activation)> zones, Map map, WPos p, DateTime current)
    {
        foreach (ref var z in zones)
            if (z.shapeDistance(p) < ForbiddenZoneCushion)
                return ActivationToG(z.activation, current);
        return float.MaxValue;
    }

    private static sbyte CalculateGoalPriority(WPos targetPos, float targetRadius, WDir targetDir, Positional positional, Map map, WPos p)
    {
        var offset = p - targetPos;
        var lsq = offset.LengthSq();
        if (lsq > targetRadius * targetRadius)
            return 0;
        if (positional == Positional.Any)
            return 1;

        var front = targetDir.Dot(offset);
        var side = Math.Abs(targetDir.Dot(offset.OrthoL()));
        var inPositional = positional switch
        {
            Positional.Flank => side > Math.Abs(front),
            Positional.Rear => -front > side,
            Positional.Front => front > side, // TODO: reconsider this...
            _ => false
        };
        return (sbyte)(inPositional ? 2 : 1);
    }

    private static WPos? GetFirstWaypoint(ThetaStar pf, Map map, int cell)
    {
        ref var startingNode = ref pf.NodeByIndex(cell);
        if (startingNode.GScore == 0 && startingNode.PathMinG == float.MaxValue)
            return null; // we're already in safe zone

        do
        {
            ref var node = ref pf.NodeByIndex(cell);
            if (pf.NodeByIndex(node.ParentIndex).GScore == 0)
                return pf.CellCenter(cell);
            cell = node.ParentIndex;
        }
        while (true);
    }
}
