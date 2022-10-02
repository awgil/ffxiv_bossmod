using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    public abstract class AOEShape
    {
        public abstract bool Check(WPos position, WPos origin, Angle rotation);
        public abstract void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE);
        public abstract void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger);
        public abstract IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset = 0, float maxError = 1); // positive offset increases area, negative decreases
        public abstract IEnumerable<(int x, int y, Pathfinding.Map.Coverage cv)> Rasterize(Pathfinding.Map map, WPos origin, Angle rotation);

        public bool Check(WPos position, Actor? origin)
        {
            return origin != null ? Check(position, origin.Position, origin.Rotation) : false;
        }

        public void Draw(MiniArena arena, Actor? origin, uint color = ArenaColor.AOE)
        {
            if (origin != null)
                Draw(arena, origin.Position, origin.Rotation, color);
        }

        public void Outline(MiniArena arena, Actor? origin, uint color = ArenaColor.Danger)
        {
            if (origin != null)
                Outline(arena, origin.Position, origin.Rotation, color);
        }
    }

    public class AOEShapeCone : AOEShape
    {
        public float Radius;
        public Angle DirectionOffset;
        public Angle HalfAngle;

        public AOEShapeCone(float radius, Angle halfAngle, Angle directionOffset = new())
        {
            Radius = radius;
            DirectionOffset = directionOffset;
            HalfAngle = halfAngle;
        }

        public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCircleCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
        public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneCone(origin, 0, Radius, rotation + DirectionOffset, HalfAngle, color);
        public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger) => arena.AddCone(origin, Radius, rotation + DirectionOffset, HalfAngle, color);
        public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
        {
            var centerOffset = offset == 0 ? 0 : offset / HalfAngle.Sin();
            var rot = rotation + DirectionOffset;
            yield return CurveApprox.CircleSector(origin - centerOffset * rot.ToDirection(), Radius + centerOffset + offset, rot - HalfAngle, rot + HalfAngle, maxError);
        }
        public override IEnumerable<(int x, int y, Pathfinding.Map.Coverage cv)> Rasterize(Pathfinding.Map map, WPos origin, Angle rotation) => map.RasterizeCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
    }

    public class AOEShapeCircle : AOEShape
    {
        public float Radius;

        public AOEShapeCircle(float radius)
        {
            Radius = radius;
        }

        public override bool Check(WPos position, WPos origin, Angle rotation = new()) => position.InCircle(origin, Radius);
        public override void Draw(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.AOE) => arena.ZoneCircle(origin, Radius, color);
        public override void Outline(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.Danger) => arena.AddCircle(origin, Radius, color);
        public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
        {
            yield return CurveApprox.Circle(origin, Radius + offset, maxError);
        }
        public override IEnumerable<(int x, int y, Pathfinding.Map.Coverage cv)> Rasterize(Pathfinding.Map map, WPos origin, Angle rotation) => map.RasterizeCircle(origin, Radius);
    }

    public class AOEShapeDonut : AOEShape
    {
        public float InnerRadius;
        public float OuterRadius;

        public AOEShapeDonut(float innerRadius, float outerRadius)
        {
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
        }

        public override bool Check(WPos position, WPos origin, Angle rotation = new()) => position.InDonut(origin, InnerRadius, OuterRadius);
        public override void Draw(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.AOE) => arena.ZoneDonut(origin, InnerRadius, OuterRadius, color);
        public override void Outline(MiniArena arena, WPos origin, Angle rotation = new(), uint color = ArenaColor.Danger)
        {
            arena.AddCircle(origin, InnerRadius, color);
            arena.AddCircle(origin, OuterRadius, color);
        }
        public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
        {
            yield return CurveApprox.Circle(origin, OuterRadius + offset, maxError);
            yield return CurveApprox.Circle(origin, InnerRadius - offset, maxError);
        }
        public override IEnumerable<(int x, int y, Pathfinding.Map.Coverage cv)> Rasterize(Pathfinding.Map map, WPos origin, Angle rotation) => map.RasterizeDonut(origin, InnerRadius, OuterRadius);
    }

    public class AOEShapeDonutSector : AOEShape
    {
        public float InnerRadius;
        public float OuterRadius;
        public Angle DirectionOffset;
        public Angle HalfAngle;

        public AOEShapeDonutSector(float innerRadius, float outerRadius, Angle halfAngle, Angle directionOffset = new())
        {
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            DirectionOffset = directionOffset;
            HalfAngle = halfAngle;
        }

        public override bool Check(WPos position, WPos origin, Angle rotation) => position.InDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
        public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
        public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger) => arena.AddDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
        public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
        {
            var centerOffset = offset == 0 ? 0 : offset / HalfAngle.Sin();
            var rot = rotation + DirectionOffset;
            yield return CurveApprox.DonutSector(origin - centerOffset * rot.ToDirection(), InnerRadius + centerOffset - offset, OuterRadius + centerOffset + offset, rot - HalfAngle, rot + HalfAngle, maxError);
        }
        public override IEnumerable<(int x, int y, Pathfinding.Map.Coverage cv)> Rasterize(Pathfinding.Map map, WPos origin, Angle rotation) => map.RasterizeDonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
    }

    public class AOEShapeRect : AOEShape
    {
        public float LengthFront;
        public float LengthBack;
        public float HalfWidth;
        public Angle DirectionOffset;

        public AOEShapeRect(float lengthFront, float halfWidth, float lengthBack = 0, Angle directionOffset = new())
        {
            LengthFront = lengthFront;
            LengthBack = lengthBack;
            HalfWidth = halfWidth;
            DirectionOffset = directionOffset;
        }

        public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
        public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.ZoneRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth, color);
        public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger)
        {
            var direction = (rotation + DirectionOffset).ToDirection();
            var side = HalfWidth * direction.OrthoR();
            var front = origin + LengthFront * direction;
            var back = origin - LengthBack * direction;
            arena.AddQuad(front + side, front - side, back - side, back + side, color);
        }
        public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
        {
            var direction = (rotation + DirectionOffset).ToDirection();
            var side = (HalfWidth + offset) * direction.OrthoR();
            var front = origin + (LengthFront + offset) * direction;
            var back = origin - (LengthBack + offset) * direction;
            yield return new[] { front + side, front - side, back - side, back + side };
        }
        public override IEnumerable<(int x, int y, Pathfinding.Map.Coverage cv)> Rasterize(Pathfinding.Map map, WPos origin, Angle rotation) => map.RasterizeRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);

        public void SetEndPoint(WPos endpoint, WPos origin, Angle rotation)
        {
            // this is a bit of a hack, but whatever...
            var dir = endpoint - origin;
            LengthFront = dir.Length();
            DirectionOffset = Angle.FromDirection(dir) - rotation;
        }

        public void SetEndPointFromCastLocation(Actor caster)
        {
            if (caster.CastInfo != null)
                SetEndPoint(caster.CastInfo.LocXZ, caster.Position, caster.Rotation);
        }
    }

    public class AOEShapeCross : AOEShape
    {
        public float Length;
        public float HalfWidth;
        public Angle DirectionOffset;

        public AOEShapeCross(float length, float halfWidth, Angle directionOffset = new())
        {
            Length = length;
            HalfWidth = halfWidth;
            DirectionOffset = directionOffset;
        }

        public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, Length, Length, HalfWidth) || position.InRect(origin, rotation + DirectionOffset, HalfWidth, HalfWidth, Length);
        public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.AOE) => arena.Zone(arena.Bounds.ClipAndTriangulate(ContourPoints(origin, rotation)), color);

        public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = ArenaColor.Danger)
        {
            foreach (var p in ContourPoints(origin, rotation))
                arena.PathLineTo(p);
            arena.PathStroke(true, color);
        }

        public override IEnumerable<IEnumerable<WPos>> Contour(WPos origin, Angle rotation, float offset, float maxError)
        {
            yield return ContourPoints(origin, rotation, offset);
        }

        public override IEnumerable<(int x, int y, Pathfinding.Map.Coverage cv)> Rasterize(Pathfinding.Map map, WPos origin, Angle rotation) => map.RasterizeCross(origin, rotation + DirectionOffset, Length, HalfWidth);

        private IEnumerable<WPos> ContourPoints(WPos origin, Angle rotation, float offset = 0)
        {
            var dx = (rotation + DirectionOffset).ToDirection();
            var dy = dx.OrthoL();
            var dx1 = dx * (Length + offset);
            var dx2 = dx * (HalfWidth + offset);
            var dy1 = dy * (Length + offset);
            var dy2 = dy * (HalfWidth + offset);
            yield return origin + dx1 - dy2;
            yield return origin + dx2 - dy2;
            yield return origin + dx2 - dy1;
            yield return origin - dx2 - dy1;
            yield return origin - dx2 - dy2;
            yield return origin - dx1 - dy2;
            yield return origin - dx1 + dy2;
            yield return origin - dx2 + dy2;
            yield return origin - dx2 + dy1;
            yield return origin + dx2 + dy1;
            yield return origin + dx2 + dy2;
            yield return origin + dx1 + dy2;
        }
    }
}
