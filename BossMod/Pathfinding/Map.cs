using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Pathfinding
{
    // 'map' used for running pathfinding algorithms
    // this is essentially a square grid representing an arena (or immediate neighbourhood of the player) where we rasterize forbidden/desired zones
    // area covered by each pixel can be in one of the following states:
    // - default: safe to traverse but non-goal
    // - danger: unsafe to traverse after X seconds (X >= 0); instead of X, we store max 'g' value (distance travelled assuming constant speed) for which pixel is still considered unblocked
    // - goal: destination with X priority (X > 0); 'default' is considered a goal with priority 0
    // - goal and danger are mutually exclusive, 'danger' overriding 'goal' state
    // typically we try to find a path to goal with highest priority; if that fails, try lower priorities; if no paths can be found (e.g. we're currently inside an imminent aoe) we find direct path to closest safe pixel
    public class Map
    {
        [Flags]
        public enum Coverage
        {
            None = 0,
            Inside = 1,
            Border = 2,
            Outside = 4
        }

        public struct Pixel
        {
            public float MaxG; // MaxValue if not dangerous
            public int Priority; // >0 if goal
        }

        public float Resolution { get; private init; } // pixel size, in world units
        public int Width { get; private init; } // always even
        public int Height { get; private init; } // always even
        public Pixel[] Pixels { get; private init; }

        public WPos Center { get; private init; } // position of map center in world units
        public Angle Rotation { get; private init; } // rotation relative to world space (=> ToDirection() is equal to direction of local 'height' axis in world space)
        private WDir _localZDivRes { get; init; }

        public float MaxG { get; private set; } // maximal 'maxG' value of all blocked pixels
        public int MaxPriority { get; private set; } // maximal 'priority' value of all blocked pixels

        //public float Speed = 6; // used for converting activation time into max g-value: num world units that player can move per second

        public Pixel this[int x, int y] => x >= 0 && x < Width && y >= 0 && y < Height ? Pixels[y * Width + x] : new() { MaxG = float.MaxValue, Priority = 0 };

        public Map(float resolution, WPos center, float worldHalfWidth, float worldHalfHeight, Angle rotation = new())
        {
            Resolution = resolution;
            Width = 2 * (int)MathF.Ceiling(worldHalfWidth / resolution);
            Height = 2 * (int)MathF.Ceiling(worldHalfHeight / resolution);
            Pixels = new Pixel[Width * Height];
            Array.Fill(Pixels, new Pixel() { MaxG = float.MaxValue, Priority = 0 });

            Center = center;
            Rotation = rotation;
            _localZDivRes = rotation.ToDirection() / Resolution;
        }

        public Vector2 WorldToGridFrac(WPos world)
        {
            var offset = world - Center;
            var x = offset.Dot(_localZDivRes.OrthoL());
            var y = offset.Dot(_localZDivRes);
            return new(Width / 2 + x, Height / 2 + y);
        }

        public (int x, int y) FracToGrid(Vector2 frac) => ((int)MathF.Floor(frac.X), (int)MathF.Floor(frac.Y));
        public (int x, int y) WorldToGrid(WPos world) => FracToGrid(WorldToGridFrac(world));

        public WPos GridToWorld(int gx, int gy, float fx, float fy)
        {
            var rsq = Resolution * Resolution; // since we then multiply by _localZDivRes, end result is same as * res * rotation.ToDir()
            float ax = (gx - Width / 2 + fx) * rsq;
            float az = (gy - Height / 2 + fy) * rsq;
            return Center + ax * _localZDivRes.OrthoL() + az * _localZDivRes;
        }

        public void BlockPixels(IEnumerable<(int x, int y)> pixels, float maxG)
        {
            MaxG = MathF.Max(MaxG, maxG);
            foreach (var (x, y) in pixels)
            {
                ref var pixel = ref Pixels[y * Width + x];
                pixel.MaxG = MathF.Min(pixel.MaxG, maxG);
            }
        }

        public IEnumerable<(int x, int y)> RasterizeCircle(WPos origin, float radius, Coverage coverage)
        {
            var cf = WorldToGridFrac(origin);
            var ci = FracToGrid(cf);
            var lo = FracToGrid(cf - new Vector2(radius, radius) / Resolution);
            var hi = FracToGrid(cf + new Vector2(radius, radius) / Resolution);

            // area outside AABB
            if (coverage.HasFlag(Coverage.Outside))
                foreach (var p in PixelsOutsideAABB(lo.x, hi.x, lo.y, hi.y))
                    yield return p;

            // top-left quadrant: lo.x <= x < c.x, lo.y <= y < c.y
            // other quadrants are similar
            var outerRadiusSq = radius * radius;
            var innerRadiusSq = 0;
            foreach (var p in PixelsInQuadrant(lo.x, ci.x, lo.y, ci.y, true, true, origin, outerRadiusSq, innerRadiusSq, coverage))
                yield return p;
            foreach (var p in PixelsInQuadrant(ci.x + 1, hi.x + 1, lo.y, ci.y, true, false, origin, outerRadiusSq, innerRadiusSq, coverage))
                yield return p;
            foreach (var p in PixelsInQuadrant(lo.x, ci.x, ci.y + 1, hi.y + 1, false, true, origin, outerRadiusSq, innerRadiusSq, coverage))
                yield return p;
            foreach (var p in PixelsInQuadrant(ci.x + 1, hi.x + 1, ci.y + 1, hi.y + 1, false, false, origin, outerRadiusSq, innerRadiusSq, coverage))
                yield return p;

            // pixels sharing x and/or y grid coordinate with origin
            var df = cf - new Vector2(ci.x, ci.y);
            foreach (var p in PixelsInCenterLineX(lo.x, ci.x, ci.y, df.Y, true, origin, outerRadiusSq, innerRadiusSq, coverage))
                yield return p;
            foreach (var p in PixelsInCenterLineX(ci.x + 1, hi.x + 1, ci.y, df.Y, false, origin, outerRadiusSq, innerRadiusSq, coverage))
                yield return p;
            foreach (var p in PixelsInCenterLineY(lo.y, ci.y, ci.x, df.X, true, origin, outerRadiusSq, innerRadiusSq, coverage))
                yield return p;
            foreach (var p in PixelsInCenterLineY(ci.y + 1, hi.y + 1, ci.x, df.X, false, origin, outerRadiusSq, innerRadiusSq, coverage))
                yield return p;

            // center pixel
            var dfi = new Vector2(1, 1) - df;
            df *= df;
            dfi *= dfi;
            var farthest = MathF.Max(df.X, dfi.X) + MathF.Max(df.Y, dfi.Y);
            float closest = 0;
            var cv = farthest < innerRadiusSq || closest >= outerRadiusSq ? Coverage.Outside
                : farthest < outerRadiusSq && closest >= innerRadiusSq ? Coverage.Inside
                : Coverage.Border;
            if (coverage.HasFlag(cv))
                yield return ci;
        }

        //public IEnumerable<(int x, int y)> RasterizeRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
        //{

        //}

        // returns all valid points with xmin <= x < xmax, ymin <= y < ymax
        private IEnumerable<(int x, int y)> PixelsInRange(int xmin, int xmax, int ymin, int ymax)
        {
            xmin = Math.Max(xmin, 0);
            ymin = Math.Max(ymin, 0);
            xmax = Math.Min(xmax, Width);
            ymax = Math.Min(ymax, Height);
            for (int y = ymin; y < ymax; ++y)
                for (int x = xmin; x < xmax; ++x)
                    yield return (x, y);
        }

        // returns all points that have x < xmin OR x > xmax OR y < ymin OR y > ymax
        private IEnumerable<(int x, int y)> PixelsOutsideAABB(int xmin, int xmax, int ymin, int ymax)
        {
            foreach (var p in PixelsInRange(0, Width, 0, ymin))
                yield return p;
            foreach (var p in PixelsInRange(0, xmin, ymin, ymax + 1))
                yield return p;
            foreach (var p in PixelsInRange(xmax + 1, Width, ymin, ymax + 1))
                yield return p;
            foreach (var p in PixelsInRange(0, Width, ymax + 1, Height))
                yield return p;
        }

        private IEnumerable<(int x, int y)> PixelsInQuadrant(int xmin, int xmax, int ymin, int ymax, bool top, bool left, WPos origin, float outerRadiusSq, float innerRadiusSq, Coverage coverage)
        {
            foreach (var p in PixelsInRange(xmin, xmax, ymin, ymax))
            {
                var farthest = (GridToWorld(p.x, p.y, left ? 0 : 1, top ? 0 : 1) - origin).LengthSq();
                var closest = (GridToWorld(p.x, p.y, left ? 1 : 0, top ? 1 : 0) - origin).LengthSq();
                var cv = farthest < innerRadiusSq || closest >= outerRadiusSq ? Coverage.Outside
                    : farthest < outerRadiusSq && closest >= innerRadiusSq ? Coverage.Inside
                    : Coverage.Border;
                if (coverage.HasFlag(cv))
                    yield return p;
            }
        }

        private IEnumerable<(int x, int y)> PixelsInCenterLineX(int xmin, int xmax, int y, float dy, bool left, WPos origin, float outerRadiusSq, float innerRadiusSq, Coverage coverage)
        {
            if (y < 0 || y >= Height)
                yield break;

            var cornerMaxSq = dy > 0.5f ? dy : 1 - dy;
            cornerMaxSq *= cornerMaxSq;
            xmin = Math.Max(xmin, 0);
            xmax = Math.Min(xmax, Width);
            for (int x = xmin; x < xmax; ++x)
            {
                var farthest = (GridToWorld(x, y, left ? 0 : 1, dy) - origin).LengthSq() + cornerMaxSq;
                var closest = (GridToWorld(x, y, left ? 1 : 0, dy) - origin).LengthSq();
                var cv = farthest < innerRadiusSq || closest >= outerRadiusSq ? Coverage.Outside
                    : farthest < outerRadiusSq && closest >= innerRadiusSq ? Coverage.Inside
                    : Coverage.Border;
                if (coverage.HasFlag(cv))
                    yield return (x, y);
            }
        }

        private IEnumerable<(int x, int y)> PixelsInCenterLineY(int ymin, int ymax, int x, float dx, bool top, WPos origin, float outerRadiusSq, float innerRadiusSq, Coverage coverage)
        {
            if (x < 0 || x >= Width)
                yield break;

            var cornerMaxSq = dx > 0.5f ? dx : 1 - dx;
            cornerMaxSq *= cornerMaxSq;
            ymin = Math.Max(ymin, 0);
            ymax = Math.Min(ymax, Height);
            for (int y = ymin; y < ymax; ++y)
            {
                var farthest = (GridToWorld(x, y, dx, top ? 0 : 1) - origin).LengthSq() + cornerMaxSq;
                var closest = (GridToWorld(x, y, dx, top ? 1 : 0) - origin).LengthSq();
                var cv = farthest < innerRadiusSq || closest >= outerRadiusSq ? Coverage.Outside
                    : farthest < outerRadiusSq && closest >= innerRadiusSq ? Coverage.Inside
                    : Coverage.Border;
                if (coverage.HasFlag(cv))
                    yield return (x, y);
            }
        }
    }
}
