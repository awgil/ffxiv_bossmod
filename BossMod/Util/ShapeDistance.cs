﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // shapes can be defined by distance from point to shape's border; distance is positive for points outside shape and negative for points inside shape
    // union is min, intersection is max
    public static class ShapeDistance
    {
        public static Func<WPos, float> Circle(WPos origin, float radius) => radius <= 0 ? (_ => float.MaxValue) : (p => (p - origin).Length() - radius);

        public static Func<WPos, float> InvertedCircle(WPos origin, float radius) => radius <= 0 ? (_ => float.MinValue) : (p => radius - (p - origin).Length());

        public static Func<WPos, float> Donut(WPos origin, float innerRadius, float outerRadius)
        {
            if (outerRadius <= 0 || innerRadius >= outerRadius)
                return _ => float.MaxValue;
            if (innerRadius <= 0)
                return Circle(origin, outerRadius);
            return p =>
            {
                // intersection of outer circle and inverted inner circle
                var distOrigin = (p - origin).Length();
                var distOuter = distOrigin - outerRadius;
                var distInner = innerRadius - distOrigin;
                return Math.Max(distOuter, distInner);
            };
        }

        public static Func<WPos, float> Cone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
        {
            if (halfAngle.Rad <= 0 || radius <= 0)
                return _ => float.MaxValue;
            if (halfAngle.Rad >= MathF.PI)
                return Circle(origin, radius);
            // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
            // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
            // both normals point outside
            float coneFactor = halfAngle.Rad > MathF.PI / 2 ? -1 : 1;
            var nl = coneFactor * (centerDir + halfAngle).ToDirection().OrthoL();
            var nr = coneFactor * (centerDir - halfAngle).ToDirection().OrthoR();
            return p =>
            {
                var off = p - origin;
                var distOrigin = off.Length();
                var distOuter = distOrigin - radius;
                var distLeft = off.Dot(nl);
                var distRight = off.Dot(nr);
                return Math.Max(distOuter, coneFactor * Math.Max(distLeft, distRight));
            };
        }

        public static Func<WPos, float> DonutSector(WPos origin, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle)
        {
            if (halfAngle.Rad <= 0 || outerRadius <= 0 || innerRadius >= outerRadius)
                return _ => float.MaxValue;
            if (halfAngle.Rad >= MathF.PI)
                return Donut(origin, innerRadius, outerRadius);
            if (innerRadius <= 0)
                return Cone(origin, outerRadius, centerDir, halfAngle);
            float coneFactor = halfAngle.Rad > MathF.PI / 2 ? -1 : 1;
            var nl = coneFactor * (centerDir + halfAngle + 90.Degrees()).ToDirection();
            var nr = coneFactor * (centerDir - halfAngle - 90.Degrees()).ToDirection();
            return p =>
            {
                var off = p - origin;
                var distOrigin = off.Length();
                var distOuter = distOrigin - outerRadius;
                var distInner = innerRadius - distOrigin;
                var distLeft = off.Dot(nl);
                var distRight = off.Dot(nr);
                return Math.Max(Math.Max(distOuter, distInner), coneFactor * Math.Max(distLeft, distRight));
            };
        }

        public static Func<WPos, float> Rect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
        {
            // dir points outside far side
            var normal = dir.OrthoL(); // points outside left side
            return p =>
            {
                var offset = p - origin;
                var distParr = offset.Dot(dir);
                var distOrtho = offset.Dot(normal);
                var distFront = distParr - lenFront;
                var distBack = -distParr - lenBack;
                var distLeft = distOrtho - halfWidth;
                var distRight = -distOrtho - halfWidth;
                return Math.Max(Math.Max(distFront, distBack), Math.Max(distLeft, distRight));
            };
        }
        public static Func<WPos, float> Rect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => Rect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
        public static Func<WPos, float> Rect(WPos from, WPos to, float halfWidth)
        {
            var dir = to - from;
            var l = dir.Length();
            return Rect(from, dir / l, l, 0, halfWidth);
        }

        public static Func<WPos, float> InvertedRect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
        {
            // dir points outside far side
            var normal = dir.OrthoL(); // points outside left side
            return p =>
            {
                var offset = p - origin;
                var distParr = offset.Dot(dir);
                var distOrtho = offset.Dot(normal);
                var distFront = distParr - lenFront;
                var distBack = -distParr - lenBack;
                var distLeft = distOrtho - halfWidth;
                var distRight = -distOrtho - halfWidth;
                return -Math.Max(Math.Max(distFront, distBack), Math.Max(distLeft, distRight));
            };
        }
        public static Func<WPos, float> InvertedRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) => InvertedRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
        public static Func<WPos, float> InvertedRect(WPos from, WPos to, float halfWidth)
        {
            var dir = to - from;
            var l = dir.Length();
            return InvertedRect(from, dir / l, l, 0, halfWidth);
        }

        public static Func<WPos, float> Cross(WPos origin, Angle direction, float length, float halfWidth)
        {
            var dir = direction.ToDirection();
            var normal = dir.OrthoL();
            return p =>
            {
                var offset = p - origin;
                var distParr = offset.Dot(dir);
                var distOrtho = offset.Dot(normal);
                var distPFront = distParr - length;
                var distPBack = -distParr - length;
                var distPLeft = distOrtho - halfWidth;
                var distPRight = -distOrtho - halfWidth;
                var distOFront = distOrtho - length;
                var distOBack = -distOrtho - length;
                var distOLeft = distParr - halfWidth;
                var distORight = -distParr - halfWidth;
                var distP = Math.Max(Math.Max(distPFront, distPBack), Math.Max(distPLeft, distPRight));
                var distO = Math.Max(Math.Max(distOFront, distOBack), Math.Max(distOLeft, distORight));
                return Math.Min(distP, distO);
            };
        }

        // positive offset increases area
        public static Func<WPos, float> ConvexPolygon(IEnumerable<WPos> vertices, bool cw, float offset = 0)
        {
            List<(WPos point, WDir normal)> edges = new();
            Action<WPos, WPos> addEdge = (p1, p2) =>
            {
                if (p1 != p2)
                {
                    var dir = (p2 - p1).Normalized();
                    edges.Add((p1, cw ? dir.OrthoL() : dir.OrthoR()));
                }
            };

            var en = vertices.GetEnumerator();
            if (!en.MoveNext())
                return _ => float.MaxValue; // empty polygon
            var prev = en.Current;
            var first = prev;
            while (en.MoveNext())
            {
                var cur = en.Current;
                addEdge(prev, cur);
                prev = cur;
            }
            addEdge(prev, first);

            return p => edges.Max(e => e.normal.Dot(p - e.point)) - offset;
        }
    }
}
