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
        // select non-null and optionally alive raid members
        public static IEnumerable<WorldState.Actor> WithoutSlot(this WorldState.Actor?[] raidMembers, bool includeDead = false)
        {
            for (int i = 0; i < raidMembers.Length; ++i)
            {
                var player = raidMembers[i];
                if (player == null)
                    continue;
                if (player.IsDead && !includeDead)
                    continue;
                yield return player;
            }
        }

        public static IEnumerable<(int, WorldState.Actor)> WithSlot(this WorldState.Actor?[] raidMembers, bool includeDead = false)
        {
            for (int i = 0; i < raidMembers.Length; ++i)
            {
                var player = raidMembers[i];
                if (player == null)
                    continue;
                if (player.IsDead && !includeDead)
                    continue;
                yield return (i, player);
            }
        }

        // find a slot index containing specified player (by instance ID); returns -1 if not found
        public static int FindSlot(this WorldState.Actor?[] raidMembers, uint instanceID)
        {
            return instanceID != 0 ? Array.FindIndex(raidMembers, x => x?.InstanceID == instanceID) : -1;
        }

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

        // select actors in specified radius from specified point
        public static IEnumerable<WorldState.Actor> InRadius(this IEnumerable<WorldState.Actor> range, Vector3 origin, float radius)
        {
            return range.Where(actor => GeometryUtils.PointInCircle(actor.Position - origin, radius));
        }

        public static IEnumerable<(int, WorldState.Actor)> InRadius(this IEnumerable<(int, WorldState.Actor)> range, Vector3 origin, float radius)
        {
            return range.WhereActor(actor => GeometryUtils.PointInCircle(actor.Position - origin, radius));
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
