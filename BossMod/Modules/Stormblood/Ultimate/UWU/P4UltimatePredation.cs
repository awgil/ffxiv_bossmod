using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    // select best safespot for all predation patterns
    class P4UltimatePredation : BossComponent
    {
        public enum State { Inactive, Predicted, First, Second, Done }

        public State CurState { get; private set; }
        private List<WPos> _hints = new();
        private DisjointSegmentList _forbiddenFirst = new();
        private DisjointSegmentList _forbiddenSecond = new();

        private static float _dodgeRadius = 19;
        private static Angle _dodgeCushion = 2.5f.Degrees();

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (movementHints != null)
                foreach (var h in EnumerateHints(actor.Position))
                    movementHints.Add(h);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            //var segs = CurState == State.Second ? _forbiddenSecond : _forbiddenFirst;
            //foreach (var (min, max) in Cushioned(SafeSegments(segs)))
            //{
            //    arena.PathArcTo(module.Bounds.Center, _dodgeRadius, min.Rad, max.Rad);
            //    arena.PathStroke(false, ArenaColor.Safe, 2);
            //}

            foreach (var h in EnumerateHints(pc.Position))
                arena.AddLine(h.from, h.to, h.color);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (CurState == State.Inactive && id == 0x1E43)
            {
                RecalculateHints(module);
                CurState = State.Predicted;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (CurState == State.Predicted && (AID)spell.Action.ID == AID.CrimsonCyclone)
                CurState = State.First;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.CrimsonCyclone:
                    if (CurState == State.First)
                    {
                        CurState = State.Second;
                        if (_hints.Count > 0)
                            _hints.RemoveAt(0);
                    }
                    break;
                case AID.CrimsonCycloneCross:
                    if (CurState == State.Second)
                    {
                        CurState = State.Done;
                        _hints.Clear();
                    }
                    break;
            }
        }

        private IEnumerable<(WPos from, WPos to, uint color)> EnumerateHints(WPos starting)
        {
            uint color = ArenaColor.Safe;
            foreach (var p in _hints)
            {
                yield return (starting, p, color);
                starting = p;
                color = ArenaColor.Danger;
            }
        }

        private void RecalculateHints(BossModule module)
        {
            _forbiddenFirst.Clear();
            _forbiddenSecond.Clear();
            _hints.Clear();

            var castModule = (UWU)module;
            var garuda = castModule.Garuda();
            var titan = castModule.Titan();
            var ifrit = castModule.Ifrit();
            var ultima = castModule.Ultima();
            if (garuda == null || titan == null || ifrit == null || ultima == null)
                return;

            ForbidRect(module, titan.Position, titan.Rotation, 3, _forbiddenFirst);
            ForbidRect(module, titan.Position, titan.Rotation + 45.Degrees(), 3, _forbiddenFirst);
            ForbidRect(module, titan.Position, titan.Rotation - 45.Degrees(), 3, _forbiddenFirst);
            ForbidRect(module, titan.Position, titan.Rotation + 22.5f.Degrees(), 3, _forbiddenSecond);
            ForbidRect(module, titan.Position, titan.Rotation - 22.5f.Degrees(), 3, _forbiddenSecond);
            ForbidRect(module, titan.Position, titan.Rotation + 90.Degrees(), 3, _forbiddenSecond);
            ForbidRect(module, ifrit.Position, ifrit.Rotation, 9, _forbiddenFirst);
            ForbidRect(module, module.Bounds.Center - new WDir(module.Bounds.HalfSize, 0), 90.Degrees(), 5, _forbiddenSecond);
            ForbidRect(module, module.Bounds.Center - new WDir(0, module.Bounds.HalfSize), 0.Degrees(), 5, _forbiddenSecond);
            ForbidCircle(module, garuda.Position, 20, _forbiddenFirst);
            ForbidCircle(module, garuda.Position, 20, _forbiddenSecond);
            ForbidCircle(module, ultima.Position, 14, _forbiddenSecond);

            var safespots = EnumeratePotentialSafespots();
            var (a1, a2) = safespots.MinBy(AngularDistance);
            _hints.Add(GetSafePositionAtAngle(module, a1));
            _hints.Add(GetSafePositionAtAngle(module, a2));

            // simplest way to think about various patterns and safespots is to first look at the titan
            // titan is slightly offset from random cardinal, by half-width of landslide; to simplify things, let's rotate arena to make titan's cardinal 'relative south' and mirror if it is offset to the right
            // then we can always think that titan is S and slightly W (so potential safe spots are N/E/W)
            // landslides then cover following directions (assuming points on border): -85 to -65 degrees (E), 0 to 20 degrees (N), 75 to 105 degrees (W) and 135 to -135 degrees (S)
            // first ifrit charge covers two intercardinals (+- 27 degrees), second charge covers all cardinals (+- 15 degrees)
            // garuda covers two cardinals + ~45 degrees
            // ultima covers single intercardinal (approximately from one cardinal to another)

            //var titanOffset = titan.Position - module.Bounds.Center;
            //var garudaOffset = garuda.Position - module.Bounds.Center;
            //var ifritOffset = ifrit.Position - module.Bounds.Center;
            //var ultimaOffset = ultima.Position - module.Bounds.Center;
            //var titanCardinal = Math.Abs(titanOffset.X) > 10 ? (titanOffset.X > 0 ? 90.Degrees() : -90.Degrees()) : (titanOffset.Z > 0 ? 0.Degrees() : 180.Degrees());
            //var titanCardinalDir = titanCardinal.ToDirection();
            //bool titanMirrored = titanCardinalDir.OrthoL().Dot(titanOffset) > 0;
            //Func<Angle, Angle, Angle> rotate = (start, move) => titanMirrored ? start - move : start + move;

            // note that titan is slightly offset from cardinal, by half-width of landslide - meaning cardinal strictly opposite titan is always on the landslide edge
            // note that ifrit's charge and titan's landslide essentially touch right at the border
            //if (garudaOffset.Dot(titanOffset) > 0)
            //{
            //    // titan near garuda => two cardinals at +- 135 degrees from garuda have safe area nearby
            //    // cardinal opposite titan always has large safe zone for first part, safe zone for second part depends on ultima's position
            //    // TODO: sometimes other cardinal is more comfy...
            //    var safeCardinal = titanCardinal + 180.Degrees();
            //    var safeCardinalDir = safeCardinal.ToDirection();

            //    var dodgeLeft = safeCardinalDir.OrthoL().Dot(titanOffset) > 0;
            //    var firstSafeDir = safeCardinal + (dodgeLeft ? -5.Degrees() : 5.Degrees());
            //    _hints.Add(GetSafePositionAtAngle(module, firstSafeDir));

            //    // ultima covers almost completely one quadrant => check whether it is on the first safe side
            //    if (ultimaOffset.Dot(safeCardinalDir) > 0)
            //    {
            //        bool ultimaLeft = safeCardinalDir.OrthoL().Dot(ultimaOffset) < 0;
            //        if (dodgeLeft == ultimaLeft)
            //            dodgeLeft = !dodgeLeft;
            //    }
            //    // else: ultima is near titan, so don't bother

            //    var secondSafeDir = safeCardinal + (dodgeLeft ? -20.Degrees() : 20.Degrees());
            //    _hints.Add(GetSafePositionAtAngle(module, secondSafeDir));
            //}
            //else
            //{
            //    // titan opposite garuda => safe area is near remaining cardinal
            //    var safeCardinal = titanCardinal + 90.Degrees();
            //    if (safeCardinal.ToDirection().Dot(garudaOffset) > 0)
            //        safeCardinal = titanCardinal - 90.Degrees();

            //    var safeCardinalDir = safeCardinal.ToDirection();
            //    if (safeCardinalDir.Dot(titanOffset) < 0)
            //    {
            //        // titan is offset away from safe cardinal => first safe spot exactly at cardinal, second is away from ultima
            //        _hints.Add(GetSafePositionAtAngle(module, safeCardinal));

            //        bool dodgeLeft = true; // default doesn't matter
            //        if (ultimaOffset.Dot(safeCardinalDir) > 0)
            //        {
            //            bool ultimaLeft = safeCardinalDir.OrthoL().Dot(ultimaOffset) < 0;
            //            if (dodgeLeft == ultimaLeft)
            //                dodgeLeft = !dodgeLeft;
            //        }

            //        var secondSafeDir = safeCardinal + (dodgeLeft ? -20.Degrees() : 20.Degrees());
            //        _hints.Add(GetSafePositionAtAngle(module, secondSafeDir));
            //    }
            //    else
            //    {
            //        // complicated pattern - first dodge ifrit, then dodge ultima
            //        var leftIntercardDir = (safeCardinal - 45.Degrees()).ToDirection();
            //        bool dodgeLeft = Math.Abs(leftIntercardDir.Dot(ifritOffset)) < 10;
            //        bool titanLeft = safeCardinal.Rad > titanCardinal.Rad;
            //        var dodgeAngle = (dodgeLeft ? -1 : 1) * (titanLeft == dodgeLeft ? 20 : 10).Degrees();
            //        _hints.Add(GetSafePositionAtAngle(module, safeCardinal + dodgeAngle));

            //        if (ultimaOffset.Dot(safeCardinalDir) > 0)
            //        {
            //            bool ultimaLeft = safeCardinalDir.OrthoL().Dot(ultimaOffset) < 0;
            //            if (dodgeLeft == ultimaLeft)
            //                dodgeLeft = !dodgeLeft;
            //        }

            //        var secondSafeDir = safeCardinal + (dodgeLeft ? -20.Degrees() : 20.Degrees());
            //        _hints.Add(GetSafePositionAtAngle(module, secondSafeDir));
            //    }
            //}
        }

        private WPos GetSafePositionAtAngle(BossModule module, Angle angle) => module.Bounds.Center + _dodgeRadius * angle.ToDirection();

        private void ForbidRect(BossModule module, WPos origin, Angle dir, float halfWidth, DisjointSegmentList res)
        {
            var direction = dir.ToDirection();
            var o1 = origin + direction.OrthoL() * halfWidth;
            var o2 = origin + direction.OrthoR() * halfWidth;
            var i1 = IntersectLine(module, o1, direction);
            var i2 = IntersectLine(module, o2, direction);
            ForbidArc(res, i1.Item1, i2.Item1);
            ForbidArc(res, i2.Item2, i1.Item2);
        }

        private void ForbidCircle(BossModule module, WPos origin, float radius, DisjointSegmentList res)
        {
            var oo = origin - module.Bounds.Center;
            var center = Angle.FromDirection(oo);
            var halfWidth = MathF.Acos((oo.LengthSq() + _dodgeRadius * _dodgeRadius - radius * radius) / (2 * oo.Length() * _dodgeRadius));
            ForbidArcByLength(res, center, halfWidth.Radians());
        }

        private (Angle, Angle) IntersectLine(BossModule module, WPos origin, WDir dir)
        {
            var oo = origin - module.Bounds.Center;
            // op = oo + dir * t, op^2 = R^2 => dir^2 * t^2 + 2 oo*dir t + oo^2 - R^2 = 0; dir^2 == 1
            var b = 2 * oo.Dot(dir);
            var c = oo.LengthSq() - _dodgeRadius * _dodgeRadius;
            var d = b * b - 4 * c;
            d = d > 0 ? MathF.Sqrt(d) : 0;
            var t1 = (-b - d) * 0.5f;
            var t2 = (-b + d) * 0.5f;
            var op1 = oo + dir * t1;
            var op2 = oo + dir * t2;
            return (Angle.FromDirection(op1), Angle.FromDirection(op2));
        }

        private void ForbidArcByLength(DisjointSegmentList res, Angle center, Angle halfWidth)
        {
            var min = center - halfWidth;
            if (min.Rad < -MathF.PI)
            {
                res.Add(min.Rad + 2 * MathF.PI, MathF.PI);
            }
            var max = center + halfWidth;
            if (max.Rad > MathF.PI)
            {
                res.Add(-MathF.PI, max.Rad - 2 * MathF.PI);
                max = MathF.PI.Radians();
            }
            res.Add(min.Rad, max.Rad);
        }

        private void ForbidArc(DisjointSegmentList res, Angle from, Angle to)
        {
            if (from.Rad < to.Rad)
            {
                res.Add(from.Rad, to.Rad);
            }
            else
            {
                res.Add(-MathF.PI, to.Rad);
                res.Add(from.Rad, MathF.PI);
            }
        }

        private IEnumerable<(Angle, Angle)> SafeSegments(DisjointSegmentList forbidden)
        {
            if (forbidden.Segments.Count == 0)
            {
                yield return (-180.Degrees(), 180.Degrees());
                yield break;
            }

            var last = forbidden.Segments.Last();
            if (forbidden.Segments[0].Min > -MathF.PI)
                yield return (last.Max.Radians() - 360.Degrees(), forbidden.Segments[0].Min.Radians());

            for (int i = 1; i < forbidden.Segments.Count; i++)
                yield return (forbidden.Segments[i - 1].Max.Radians(), forbidden.Segments[i].Min.Radians());

            if (last.Max < MathF.PI)
                yield return (last.Max.Radians(), forbidden.Segments[0].Min.Radians() + 360.Degrees());
        }

        private IEnumerable<(Angle, Angle)> Cushioned(IEnumerable<(Angle, Angle)> segs)
        {
            foreach (var (min, max) in segs)
            {
                var minAdj = min + _dodgeCushion;
                var maxAdj = max - _dodgeCushion;
                if (minAdj.Rad < maxAdj.Rad)
                    yield return (minAdj, maxAdj);
            }
        }

        private IEnumerable<(Angle, Angle)> EnumeratePotentialSafespots()
        {
            var safeFirst = Cushioned(SafeSegments(_forbiddenFirst));
            var safeSecond = Cushioned(SafeSegments(_forbiddenSecond));
            foreach (var (min1, max1) in safeFirst)
            {
                foreach (var (min2, max2) in safeSecond)
                {
                    var intersectMin = MathF.Max(min1.Rad, min2.Rad).Radians();
                    var intersectMax = MathF.Min(max1.Rad, max2.Rad).Radians();
                    if (intersectMin.Rad < intersectMax.Rad)
                    {
                        var midpoint = ((intersectMin + intersectMax) * 0.5f).Normalized();
                        yield return (midpoint, midpoint);
                    }
                    else
                    {
                        yield return (max1.Normalized(), min2.Normalized());
                        yield return (min1.Normalized(), max2.Normalized());
                    }
                }
            }
        }

        private float AngularDistance((Angle, Angle) p)
        {
            var dist = MathF.Abs(p.Item1.Rad - p.Item2.Rad);
            return dist < MathF.PI ? dist : 2 * MathF.PI - dist;
        }
    }
}
