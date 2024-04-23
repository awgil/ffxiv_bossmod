namespace BossMod.AI;

// utility that determines safe orientation based on ai hints
static class AvoidGaze
{
    public static WDir? Update(Actor player, WPos? targetPos, AIHints hints, DateTime deadline)
    {
        if (hints.ForbiddenDirections.Count == 0)
            return null;

        DisjointSegmentList list = new();
        foreach (var d in hints.ForbiddenDirections.Where(d => d.activation <= deadline))
        {
            var center = d.center.Normalized();
            var min = center - d.halfWidth;
            if (min.Rad < -MathF.PI)
            {
                list.Add(min.Rad + 2 * MathF.PI, MathF.PI);
            }
            var max = center + d.halfWidth;
            if (max.Rad > MathF.PI)
            {
                list.Add(-MathF.PI, max.Rad - 2 * MathF.PI);
                max = MathF.PI.Radians();
            }
            list.Add(min.Rad, max.Rad);
        }

        if (!list.Contains(player.Rotation.Rad))
            return null; // all good

        if (targetPos != null)
        {
            var toTarget = Angle.FromDirection(targetPos.Value - player.Position);
            if (!list.Contains(toTarget.Rad))
                return toTarget.ToDirection();
        }

        // select midpoint of largest allowed segment
        float bestWidth = list.Segments[0].Min + 2 * MathF.PI - list.Segments[^1].Max;
        float bestMidpoint = (list.Segments[0].Min + 2 * MathF.PI + list.Segments[^1].Max) / 2;
        for (int i = 1; i < list.Segments.Count; ++i)
        {
            float width = list.Segments[i].Min - list.Segments[i - 1].Max;
            if (width > bestWidth)
            {
                bestWidth = width;
                bestMidpoint = (list.Segments[i].Min + list.Segments[i - 1].Max) / 2;
            }
        }
        return bestMidpoint.Radians().ToDirection();
    }
}
