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
    }
}
