using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // "safe zone" is determined as arena bounds *minus* all forbidden zones *intersecting* all forced safe zones
    public class SafeZone
    {
        public ClipperLib.PolyTree Result { get; private set; }
        private Clip2D _clipper = new();

        public DisjointSegmentList ForbiddenRotations { get; private set; } = new(); // sorted non-intersecting ranges, [-pi, +pi]
        public DateTime ForbiddenRotationsActivation { get; private set; }

        public SafeZone(IEnumerable<WPos> bounds)
        {
            Result = _clipper.Union(Enumerable.Repeat(bounds, 1));
        }

        public SafeZone(ArenaBounds bounds) : this(bounds.BuildClipPoly(-1)) { }
        public SafeZone(WPos center) : this(DefaultBounds(center)) { }

        public void ForbidZone(IEnumerable<IEnumerable<WPos>> bounds, DateTime activateAt, float activeDurationS = 0)
        {
            Result = _clipper.Difference(Result, bounds);
        }
        public void ForbidZone(AOEShape shape, WPos origin, Angle rot, DateTime activateAt, float activeDurationS = 0) => ForbidZone(shape.Contour(origin, rot, 1, 0.5f), activateAt, activeDurationS);

        public void RestrictToZone(IEnumerable<IEnumerable<WPos>> bounds, DateTime activateAt, float activeDurationS = 0)
        {
            Result = _clipper.Intersect(Result, bounds);
        }
        public void RestrictToZone(AOEShape shape, WPos origin, Angle rot, DateTime activateAt, float activeDurationS = 0) => RestrictToZone(shape.Contour(origin, rot, -1, 0.5f), activateAt, activeDurationS);

        public void ForbidDirections(Angle center, Angle halfWidth, DateTime activateAt)
        {
            if (ForbiddenRotationsActivation > activateAt)
            {
                // anything added before will activate later...
                if (ForbiddenRotationsActivation > activateAt.AddSeconds(1))
                    ForbiddenRotations.Clear();
                ForbiddenRotationsActivation = activateAt;
            }
            else if (ForbiddenRotationsActivation != new DateTime() && ForbiddenRotationsActivation.AddSeconds(1) < activateAt)
            {
                // anything added before will activate before...
                return;
            }

            center = center.Normalized();
            var min = center - halfWidth;
            if (min.Rad < -MathF.PI)
            {
                ForbiddenRotations.Add(min.Rad + 2 * MathF.PI, MathF.PI);
            }
            var max = center + halfWidth;
            if (max.Rad > MathF.PI)
            {
                ForbiddenRotations.Add(-MathF.PI, max.Rad - 2 * MathF.PI);
                max = MathF.PI.Radians();
            }
            ForbiddenRotations.Add(min.Rad, max.Rad);
        }

        public static IEnumerable<WPos> DefaultBounds(WPos center)
        {
            var s = 1000;
            yield return center + new WDir( s, -s);
            yield return center + new WDir( s,  s);
            yield return center + new WDir(-s,  s);
            yield return center + new WDir(-s, -s);
        }
    }
}
