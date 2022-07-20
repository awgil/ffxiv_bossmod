using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // "safe zone" is determined as arena bounds *minus* all forbidden zones *intersecting* all forced safe zones
    public class SafeZone
    {
        public ClipperLib.PolyTree Result { get; private set; }
        private Clip2D _clipper = new();

        public SafeZone(IEnumerable<WPos> bounds)
        {
            Result = _clipper.Union(Enumerable.Repeat(bounds, 1));
        }

        public SafeZone(ArenaBounds bounds) : this(bounds.BuildClipPoly(-1)) { }
        public SafeZone(WPos center) : this(DefaultBounds(center)) { }

        public void ForbidZone(IEnumerable<IEnumerable<WPos>> bounds)
        {
            Result = _clipper.Difference(Result, bounds);
        }
        public void ForbidZone(AOEShape shape, WPos origin, Angle rot) => ForbidZone(shape.Contour(origin, rot, 1, 0.5f));

        //public void RestrictToZone(IEnumerable<IEnumerable<WPos>> bounds)
        //{
        //    Result = _clipper.Intersect - TODO...
        //}

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
