using System;
using System.Collections.Generic;

namespace BossMod
{
    // note: if arena bounds are changed, new instance is recreated
    // max approx error can change without recreating the instance
    // you can use hash-code to cache clipping results - it will change whenever anything in the instance changes
    public abstract class ArenaBounds
    {
        public WPos Center { get; init; }
        public float HalfSize { get; init; } // largest horizontal/vertical dimension: radius for circle, max of width/height for rect

        // fields below are used for clipping
        public float MaxApproxError { get; private set; }

        private Clip2D _clipper = new();
        public IEnumerable<WPos> ClipPoly => _clipper.ClipPoly;
        public List<(WPos, WPos, WPos)> ClipAndTriangulate(IEnumerable<WPos> poly) => _clipper.ClipAndTriangulate(poly);

        private float _screenHalfSize;
        public float ScreenHalfSize
        {
            get => _screenHalfSize;
            set
            {
                if (_screenHalfSize != value)
                {
                    _screenHalfSize = value;
                    MaxApproxError = CurveApprox.ScreenError / value * HalfSize;
                    _clipper.ClipPoly = BuildClipPoly();
                }
            }
        }

        protected ArenaBounds(WPos center, float halfSize)
        {
            Center = center;
            HalfSize = halfSize;
        }

        public abstract IEnumerable<WPos> BuildClipPoly(float offset = 0); // positive offset increases area, negative decreases
        public abstract bool Contains(WPos p);
        public abstract WDir ClampToBounds(WDir offset, float scale = 1);
        public WPos ClampToBounds(WPos position) => Center + ClampToBounds(position - Center);

        // functions for clipping various shapes to bounds
        public List<(WPos, WPos, WPos)> ClipAndTriangulateCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
        {
            // TODO: think of a better way to do that (analytical clipping?)
            if (innerRadius >= outerRadius || innerRadius < 0 || halfAngle.Rad <= 0)
                return new();

            bool fullCircle = halfAngle.Rad >= MathF.PI;
            bool donut = innerRadius > 0;
            var points = (donut, fullCircle) switch
            {
                (false, false) => CurveApprox.CircleSector(center, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
                (false,  true) => CurveApprox.Circle(center, outerRadius, MaxApproxError),
                ( true, false) => CurveApprox.DonutSector(center, innerRadius, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
                ( true,  true) => CurveApprox.DonutSector(center, innerRadius, outerRadius, 0.0f.Radians(), (2 * MathF.PI).Radians(), MaxApproxError),
            };
            return ClipAndTriangulate(points);
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulateCircle(WPos center, float radius)
        {
            return ClipAndTriangulate(CurveApprox.Circle(center, radius, MaxApproxError));
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulateDonut(WPos center, float innerRadius, float outerRadius)
        {
            if (innerRadius >= outerRadius || innerRadius < 0)
                return new();
            return ClipAndTriangulate(CurveApprox.DonutSector(center, innerRadius, outerRadius, 0.0f.Radians(), (2 * MathF.PI).Radians(), MaxApproxError));
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulateTri(WPos a, WPos b, WPos c)
        {
            return ClipAndTriangulate(new[] { a, b, c });
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulateIsoscelesTri(WPos apex, WDir height, WDir halfBase)
        {
            return ClipAndTriangulateTri(apex, apex + height + halfBase, apex + height - halfBase);
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulateIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height)
        {
            var dir = direction.ToDirection();
            var normal = dir.OrthoL();
            return ClipAndTriangulateIsoscelesTri(apex, height * dir, height * halfAngle.Tan() * normal);
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulateRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth)
        {
            var side = halfWidth * direction.OrthoR();
            var front = origin + lenFront * direction;
            var back = origin - lenBack * direction;
            return ClipAndTriangulate(new[] { front + side, front - side, back - side, back + side });
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulateRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
        {
            return ClipAndTriangulateRect(origin, direction.ToDirection(), lenFront, lenBack, halfWidth);
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulateRect(WPos start, WPos end, float halfWidth)
        {
            var dir = (end - start).Normalized();
            var side = halfWidth * dir.OrthoR();
            return ClipAndTriangulate(new[] { start + side, start - side, end - side, end + side });
        }
    }

    public class ArenaBoundsCircle : ArenaBounds
    {
        public ArenaBoundsCircle(WPos center, float radius) : base(center, radius) { }

        public override IEnumerable<WPos> BuildClipPoly(float offset) => CurveApprox.Circle(Center, HalfSize + offset, MaxApproxError);
        public override bool Contains(WPos position) => position.InCircle(Center, HalfSize);

        public override WDir ClampToBounds(WDir offset, float scale)
        {
            var r = HalfSize * scale;
            if (offset.LengthSq() > r * r)
                offset *= r / offset.Length();
            return offset;
        }
    }

    public class ArenaBoundsSquare : ArenaBounds
    {
        public ArenaBoundsSquare(WPos center, float halfWidth) : base(center, halfWidth) { }

        public override IEnumerable<WPos> BuildClipPoly(float offset)
        {
            var s = HalfSize + offset;
            yield return Center + new WDir( s, -s);
            yield return Center + new WDir( s,  s);
            yield return Center + new WDir(-s,  s);
            yield return Center + new WDir(-s, -s);
        }

        public override bool Contains(WPos position) => WPos.AlmostEqual(position, Center, HalfSize);

        public override WDir ClampToBounds(WDir offset, float scale)
        {
            var wh = HalfSize * scale;
            if (Math.Abs(offset.X) > wh)
                offset *= wh / Math.Abs(offset.X);
            if (Math.Abs(offset.Z) > wh)
                offset *= wh / Math.Abs(offset.Z);
            return offset;
        }
    }
}
