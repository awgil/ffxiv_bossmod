using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // information relevant for AI decision making process for a specific player
    public class AIHints
    {
        public struct Enemy
        {
            public Actor Actor;
            public int Priority; // <0 means damaging is actually forbidden, 0 is default
            public float TimeToKill;
        }

        public static ArenaBounds DefaultBounds = new ArenaBoundsSquare(new(), 1000);

        public ArenaBounds Bounds = DefaultBounds;

        // list of potential targets
        public List<Enemy> PotentialTargets = new();
        public int HighestPotentialTargetPriority = 0;

        // positioning: two lists below define areas that player is allowed to be standing in, now or in near future
        // a resulting 'safe zone' is calculated as: arena-bounds INTERSECT union-of(restricted-zones) MINUS union-of(forbidden-zones)
        // AI will try to move in such a way to avoid standing in any forbidden zone after its activation or outside of some restricted zone after its activation, even at the cost of uptime
        public List<(AOEShape shape, WPos origin, Angle rot, DateTime activation)> ForbiddenZones = new();
        public List<(AOEShape shape, WPos origin, Angle rot, DateTime activation)> RestrictedZones = new();

        // imminent forced movements (knockbacks, attracts, etc.)
        public List<(WDir move, DateTime activation)> ForcedMovements = new();

        // positioning: position hint - if set, player will move closer to this position, assuming it is safe and in target's range, without losing uptime
        //public WPos? RecommendedPosition = null;

        // orientation restrictions (e.g. for gaze attacks): a list of forbidden orientation ranges, now or in near future
        // AI will rotate to face allowed orientation at last possible moment, potentially losing uptime
        public List<(Angle center, Angle halfWidth, DateTime activation)> ForbiddenDirections = new();

        // predicted incoming damage (raidwides, tankbusters, etc.)
        // AI will attempt to shield & mitigate
        public List<(BitMask players, DateTime activation)> PredictedDamage = new();

        // clear all stored data
        public void Clear()
        {
            Bounds = DefaultBounds;
            PotentialTargets.Clear();
            ForbiddenZones.Clear();
            RestrictedZones.Clear();
            ForcedMovements.Clear();
            ForbiddenDirections.Clear();
            PredictedDamage.Clear();
        }

        // fill list of potential targets from world state
        public void FillPotentialTargets(WorldState ws)
        {
            foreach (var actor in ws.Actors.Where(a => a.Type == ActorType.Enemy && a.IsTargetable && !a.IsAlly && !a.IsDead))
            {
                PotentialTargets.Add(new() { Actor = actor, Priority = actor.InCombat ? 0 : -1, TimeToKill = 10000 });
            }
        }

        public void AssignPotentialTargetPriorities(Func<Actor, int> map)
        {
            for (int i = 0; i < PotentialTargets.Count; i++)
            {
                var e = PotentialTargets[i];
                e.Priority = map(e.Actor);
                PotentialTargets[i] = e;
            }
        }

        // normalize all entries after gathering data: sort by priority / activation timestamp
        public void Normalize()
        {
            PotentialTargets.SortByReverse(x => x.Priority);
            HighestPotentialTargetPriority = Math.Max(0, PotentialTargets.FirstOrDefault().Priority);
            ForbiddenZones.SortBy(e => e.activation);
            RestrictedZones.SortBy(e => e.activation);
            ForcedMovements.SortBy(e => e.activation);
            ForbiddenDirections.SortBy(e => e.activation);
            PredictedDamage.SortBy(e => e.activation);
        }

        // query utilities
        public IEnumerable<Enemy> PotentialTargetsEnumerable => PotentialTargets;
        public IEnumerable<Enemy> PriorityTargets => PotentialTargets.TakeWhile(e => e.Priority == HighestPotentialTargetPriority);
        public IEnumerable<Actor> PriorityTargetsActors => PriorityTargets.Select(e => e.Actor);
        public IEnumerable<Actor> ForbiddenTargetsActors => PotentialTargetsEnumerable.Reverse().TakeWhile(e => e.Priority < 0).Select(e => e.Actor);

        // TODO: verify how source/target hitboxes are accounted for by various aoe shapes
        public int NumPriorityTargetsInAOE(Func<Actor, bool> pred) => ForbiddenTargetsActors.Any(pred) ? 0 : PriorityTargetsActors.Count(pred);
        public int NumPriorityTargetsInAOECircle(WPos origin, float radius) => NumPriorityTargetsInAOE(a => a.Position.InCircle(origin, radius + a.HitboxRadius));
        public int NumPriorityTargetsInAOECone(WPos origin, float radius, WDir direction, Angle halfAngle) => NumPriorityTargetsInAOE(a => a.Position.InCircleCone(origin, radius + a.HitboxRadius, direction, halfAngle));
        public int NumPriorityTargetsInAOERect(WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0) => NumPriorityTargetsInAOE(a => a.Position.InRect(origin, direction, lenFront + a.HitboxRadius, lenBack, halfWidth));
    }
}
