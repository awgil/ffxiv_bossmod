using System;
using System.Numerics;

namespace BossMod
{
    public static class GeometryUtils
    {
        public static bool ClipLineToNearPlane(ref SharpDX.Vector3 a, ref SharpDX.Vector3 b, SharpDX.Matrix viewProj)
        {
            var n = viewProj.Column3; // near plane
            var an = SharpDX.Vector4.Dot(new(a, 1), n);
            var bn = SharpDX.Vector4.Dot(new(b, 1), n);
            if (an <= 0 && bn <= 0)
                return false;

            if (an < 0 || bn < 0)
            {
                var ab = b - a;
                var abn = SharpDX.Vector3.Dot(ab, new(n.X, n.Y, n.Z));
                var t = -an / abn;
                if (an < 0)
                    a = a + t * ab;
                else
                    b = a + t * ab;
            }
            return true;
        }

        public static Vector3 DirectionToVec3(float direction)
        {
            return new(MathF.Sin(direction), 0, MathF.Cos(direction));
        }

        public static float DirectionFromVec3(Vector3 direction)
        {
            return MathF.Atan2(direction.X, direction.Z);
        }

        // zone checking
        public static bool PointInTri(Vector3 p, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var s = (v2.X - v1.X) * (p.Z - v1.Z) - (v2.Z - v1.Z) * (p.X - v1.X);
            var t = (v3.X - v2.X) * (p.Z - v2.Z) - (v3.Z - v2.Z) * (p.X - v2.X);
            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;
            var d = (v1.X - v3.X) * (p.Z - v3.Z) - (v1.Z - v3.Z) * (p.X - v3.X);
            return d == 0 || (d < 0) == (s + t <= 0);
        }

        public static bool PointInRect(Vector3 offsetFromOrigin, Vector3 direction, float lenFront, float lenBack, float halfWidth)
        {
            var normal = new Vector3(-direction.Z, 0, direction.X);
            var dotDir = Vector3.Dot(offsetFromOrigin, direction);
            var dotNormal = Vector3.Dot(offsetFromOrigin, normal);
            return dotDir >= -lenBack && dotDir <= lenFront && MathF.Abs(dotNormal) <= halfWidth;
        }

        public static bool PointInRect(Vector3 offsetFromOrigin, float direction, float lenFront, float lenBack, float halfWidth)
        {
            return PointInRect(offsetFromOrigin, DirectionToVec3(direction), lenFront, lenBack, halfWidth);
        }

        public static bool PointInRect(Vector3 offsetFromOrigin, Vector3 startToEnd, float halfWidth)
        {
            var len = startToEnd.Length();
            return PointInRect(offsetFromOrigin, startToEnd / len, len, 0, halfWidth);
        }

        public static bool PointInCircle(Vector3 offsetFromOrigin, float radius)
        {
            return offsetFromOrigin.LengthSquared() <= radius * radius;
        }

        public static bool PointInCone(Vector3 offsetFromOrigin, Vector3 direction, float halfAngle)
        {
            return Vector3.Dot(Vector3.Normalize(offsetFromOrigin), direction) >= MathF.Cos(halfAngle);
        }

        public static bool PointInCone(Vector3 offsetFromOrigin, float direction, float halfAngle)
        {
            return PointInCone(offsetFromOrigin, DirectionToVec3(direction), halfAngle);
        }
    }
}
