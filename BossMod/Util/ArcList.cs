namespace BossMod;

// a disjoint set of circle arcs; useful for e.g. selecting a bunch of safe spots at max melee or arena edge or whatever
public class ArcList(WPos center, float radius)
{
    public WPos Center = center;
    public float Radius = radius;
    public DisjointSegmentList Forbidden = new();

    public ArcList Clone() => new(Center, Radius)
    {
        Forbidden = Forbidden.Clone()
    };

    public void ForbidArc(Angle from, Angle to)
    {
        if (from.Rad < to.Rad)
        {
            Forbidden.Add(from.Rad, to.Rad);
        }
        else
        {
            Forbidden.Add(-MathF.PI, to.Rad);
            Forbidden.Add(from.Rad, MathF.PI);
        }
    }

    public void ForbidArcByLength(Angle center, Angle halfWidth)
    {
        var min = center - halfWidth;
        if (min.Rad < -MathF.PI)
        {
            Forbidden.Add(min.Rad + 2 * MathF.PI, MathF.PI);
            min = -MathF.PI.Radians();
        }
        var max = center + halfWidth;
        if (max.Rad > MathF.PI)
        {
            Forbidden.Add(-MathF.PI, max.Rad - 2 * MathF.PI);
            max = MathF.PI.Radians();
        }
        Forbidden.Add(min.Rad, max.Rad);
    }

    public void ForbidCircle(WPos origin, float radius)
    {
        var oo = origin - Center;
        var center = Angle.FromDirection(oo);
        var cos = (oo.LengthSq() + Radius * Radius - radius * radius) / (2 * oo.Length() * Radius);
        if (cos is <= 1 and >= -1)
        {
            ForbidArcByLength(center, Angle.Acos(cos));
        }
    }

    public void ForbidInverseCircle(WPos origin, float radius)
    {
        var oo = origin - Center;
        var center = Angle.FromDirection(oo);
        var cos = (oo.LengthSq() + Radius * Radius - radius * radius) / (2 * oo.Length() * Radius);
        if (cos is <= 1 and >= -1)
        {
            ForbidArcByLength((center + 180.Degrees()).Normalized(), Angle.Acos(-cos));
        }
    }

    public void ForbidInfiniteRect(WPos origin, Angle dir, float halfWidth)
    {
        var direction = dir.ToDirection();
        var o1 = origin + direction.OrthoL() * halfWidth;
        var o2 = origin + direction.OrthoR() * halfWidth;
        var i1 = IntersectLine(o1, direction);
        var i2 = IntersectLine(o2, direction);
        ForbidArc(i1.Item1, i2.Item1);
        ForbidArc(i2.Item2, i1.Item2);
    }

    public void ForbidInfiniteCone(WPos origin, Angle dir, Angle halfAngle)
    {
        var min = IntersectLine(origin, (dir - halfAngle).ToDirection()).Item2;
        var max = IntersectLine(origin, (dir + halfAngle).ToDirection()).Item2;
        ForbidArc(min, max);
    }

    public IEnumerable<(Angle min, Angle max)> Allowed(Angle cushion)
    {
        if (Forbidden.Segments.Count == 0)
        {
            yield return (-180.Degrees(), 180.Degrees());
            yield break;
        }

        foreach (var (min, max) in AllowedRaw())
        {
            var minAdj = min + cushion;
            var maxAdj = max - cushion;
            if (minAdj.Rad < maxAdj.Rad)
                yield return (minAdj, maxAdj);
        }
    }

    public (Angle min, Angle max) NextAllowed(Angle dir, bool ccw)
    {
        if (Forbidden.Count == 0)
            return (dir - 180.Degrees(), dir + 180.Degrees()); // everything is allowed

        (Angle, Angle) boundsBefore(int index)
        {
            var min = index == 0 ? Forbidden.Segments[^1].Max.Radians() - 2 * MathF.PI.Radians() : Forbidden.Segments[index - 1].Max.Radians();
            var max = index >= Forbidden.Segments.Count ? Forbidden.Segments[0].Min.Radians() + 2 * MathF.PI.Radians() : Forbidden.Segments[index].Min.Radians();
            return (min, max);
        }

        var forbidden = Forbidden.Intersect(dir.Rad, dir.Rad);
        if (forbidden.count == 0)
        {
            // current direction is not forbidden, find bounds
            // note: since we did not find intersection, the returned bounds are guaranteed to be non-empty
            return boundsBefore(forbidden.first);
        }
        else if (ccw)
        {
            var (min, max) = boundsBefore(forbidden.first + 1);
            return forbidden.first == Forbidden.Segments.Count - 1 && min.Rad >= max.Rad && Forbidden.Count > 1 ? boundsBefore(1) : (min, max);
        }
        else
        {
            var (min, max) = boundsBefore(forbidden.first);
            return forbidden.first == 0 && min.Rad >= max.Rad && Forbidden.Count > 1 ? boundsBefore(forbidden.first - 1) : (min, max);
        }
    }

    private (Angle, Angle) IntersectLine(WPos origin, WDir dir)
    {
        var oo = origin - Center;
        // op = oo + dir * t, op^2 = R^2 => dir^2 * t^2 + 2 oo*dir t + oo^2 - R^2 = 0; dir^2 == 1
        var b = 2 * oo.Dot(dir);
        var c = oo.LengthSq() - Radius * Radius;
        var d = b * b - 4 * c;
        d = d > 0 ? MathF.Sqrt(d) : 0;
        var t1 = (-b - d) * 0.5f;
        var t2 = (-b + d) * 0.5f;
        var op1 = oo + dir * t1;
        var op2 = oo + dir * t2;
        return (Angle.FromDirection(op1), Angle.FromDirection(op2));
    }

    private IEnumerable<(Angle, Angle)> AllowedRaw()
    {
        if (Forbidden.Segments.Count == 0)
        {
            yield return (-180.Degrees(), 180.Degrees());
            yield break;
        }

        var last = Forbidden.Segments[^1];
        if (Forbidden.Segments[0].Min > -MathF.PI)
            yield return (last.Max.Radians() - 360.Degrees(), Forbidden.Segments[0].Min.Radians());

        for (int i = 1; i < Forbidden.Segments.Count; i++)
            yield return (Forbidden.Segments[i - 1].Max.Radians(), Forbidden.Segments[i].Min.Radians());

        if (last.Max < MathF.PI)
            yield return (last.Max.Radians(), Forbidden.Segments[0].Min.Radians() + 360.Degrees());
    }
}
