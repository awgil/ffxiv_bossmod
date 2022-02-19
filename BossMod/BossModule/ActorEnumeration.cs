using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // for raid members, we support both indexed and non-indexed enumeration
    public static class ActorEnumeration
    {
        // build a mask with set bits corresponding to slots in range
        public static ulong Mask(this IEnumerable<(int, WorldState.Actor)> range)
        {
            ulong mask = 0;
            foreach ((var i, _) in range)
                BitVector.SetVector64Bit(ref mask, i);
            return mask;
        }

        // filter range with slot+actor by slot or by actor
        public static IEnumerable<(int, WorldState.Actor)> WhereSlot(this IEnumerable<(int, WorldState.Actor)> range, Func<int, bool> predicate)
        {
            return range.Where(indexActor => predicate(indexActor.Item1));
        }

        public static IEnumerable<(int, WorldState.Actor)> WhereActor(this IEnumerable<(int, WorldState.Actor)> range, Func<WorldState.Actor, bool> predicate)
        {
            return range.Where(indexActor => predicate(indexActor.Item2));
        }

        // exclude specified actor from enumeration
        public static IEnumerable<WorldState.Actor> Exclude(this IEnumerable<WorldState.Actor> range, WorldState.Actor actor)
        {
            return range.Where(x => x != actor);
        }

        public static IEnumerable<(int, WorldState.Actor)> Exclude(this IEnumerable<(int, WorldState.Actor)> range, WorldState.Actor actor)
        {
            return range.WhereActor(x => x != actor);
        }

        public static IEnumerable<(int, WorldState.Actor)> Exclude(this IEnumerable<(int, WorldState.Actor)> range, int slot)
        {
            return range.WhereSlot(i => i != slot);
        }

        // select actors that have their corresponding bit in mask set
        public static IEnumerable<(int, WorldState.Actor)> IncludedInMask(this IEnumerable<(int, WorldState.Actor)> range, ulong mask)
        {
            return range.WhereSlot(i => BitVector.IsVector64BitSet(mask, i));
        }

        // select actors that have their corresponding bit in mask cleared
        public static IEnumerable<(int, WorldState.Actor)> ExcludedFromMask(this IEnumerable<(int, WorldState.Actor)> range, ulong mask)
        {
            return range.WhereSlot(i => !BitVector.IsVector64BitSet(mask, i));
        }

        // select actors in specified radius from specified point
        public static IEnumerable<WorldState.Actor> InRadius(this IEnumerable<WorldState.Actor> range, Vector3 origin, float radius)
        {
            return range.Where(actor => GeometryUtils.PointInCircle(actor.Position - origin, radius));
        }

        public static IEnumerable<(int, WorldState.Actor)> InRadius(this IEnumerable<(int, WorldState.Actor)> range, Vector3 origin, float radius)
        {
            return range.WhereActor(actor => GeometryUtils.PointInCircle(actor.Position - origin, radius));
        }

        // select actors outside specified radius from specified point
        public static IEnumerable<WorldState.Actor> OutOfRadius(this IEnumerable<WorldState.Actor> range, Vector3 origin, float radius)
        {
            return range.Where(actor => !GeometryUtils.PointInCircle(actor.Position - origin, radius));
        }

        public static IEnumerable<(int, WorldState.Actor)> OutOfRadius(this IEnumerable<(int, WorldState.Actor)> range, Vector3 origin, float radius)
        {
            return range.WhereActor(actor => !GeometryUtils.PointInCircle(actor.Position - origin, radius));
        }

        // select actors in specified radius from specified actor, excluding actor itself
        public static IEnumerable<WorldState.Actor> InRadiusExcluding(this IEnumerable<WorldState.Actor> range, WorldState.Actor origin, float radius)
        {
            return range.Exclude(origin).InRadius(origin.Position, radius);
        }

        public static IEnumerable<(int, WorldState.Actor)> InRadiusExcluding(this IEnumerable<(int, WorldState.Actor)> range, WorldState.Actor origin, float radius)
        {
            return range.Exclude(origin).InRadius(origin.Position, radius);
        }

        // select actors that have tether with specific ID
        public static IEnumerable<WorldState.Actor> Tethered<ID>(this IEnumerable<WorldState.Actor> range, ID id) where ID : Enum
        {
            var castID = (uint)(object)id;
            return range.Where(actor => actor.Tether.ID == castID);
        }

        public static IEnumerable<(int, WorldState.Actor)> Tethered<ID>(this IEnumerable<(int, WorldState.Actor)> range, ID id) where ID : Enum
        {
            var castID = (uint)(object)id;
            return range.WhereActor(actor => actor.Tether.ID == castID);
        }

        // sort range by distance from point
        public static IEnumerable<WorldState.Actor> SortedByRange(this IEnumerable<WorldState.Actor> range, Vector3 origin)
        {
            return range
                .Select(actor => (actor, (actor.Position - origin).LengthSquared()))
                .OrderBy(actorDist => actorDist.Item2)
                .Select(actorDist => actorDist.Item1);
        }

        public static IEnumerable<(int, WorldState.Actor)> SortedByRange(this IEnumerable<(int, WorldState.Actor)> range, Vector3 origin)
        {
            return range
                .Select(indexPlayer => (indexPlayer.Item1, indexPlayer.Item2, (indexPlayer.Item2.Position - origin).LengthSquared()))
                .OrderBy(indexPlayerDist => indexPlayerDist.Item3)
                .Select(indexPlayerDist => (indexPlayerDist.Item1, indexPlayerDist.Item2));
        }
    }
}
