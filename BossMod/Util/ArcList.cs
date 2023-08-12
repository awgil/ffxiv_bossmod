using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // a disjoint set of circle arcs; useful for e.g. selecting a bunch of safe spots at max melee or arena edge or whatever
    public class ArcList
    {
        public WPos Center;
        public float Radius;
        public DisjointSegmentList Forbidden = new();

        public ArcList(WPos center, float radius)
        {
            Center = center;
            Radius = radius;
        }

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
            var halfWidth = MathF.Acos((oo.LengthSq() + Radius * Radius - radius * radius) / (2 * oo.Length() * Radius));
            ForbidArcByLength(center, halfWidth.Radians());
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

        public IEnumerable<(Angle, Angle)> Allowed(Angle cushion)
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

            var last = Forbidden.Segments.Last();
            if (Forbidden.Segments[0].Min > -MathF.PI)
                yield return (last.Max.Radians() - 360.Degrees(), Forbidden.Segments[0].Min.Radians());

            for (int i = 1; i < Forbidden.Segments.Count; i++)
                yield return (Forbidden.Segments[i - 1].Max.Radians(), Forbidden.Segments[i].Min.Radians());

            if (last.Max < MathF.PI)
                yield return (last.Max.Radians(), Forbidden.Segments[0].Min.Radians() + 360.Degrees());
        }
    }
}
