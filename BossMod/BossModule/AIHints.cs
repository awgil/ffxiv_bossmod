using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // information relevant for AI decision making process for a specific player
    public class AIHints
    {
        public class Enemy
        {
            public Actor Actor;
            public int Priority; // <0 means damaging is actually forbidden, 0 is default
            public float TimeToKill;
            public float AttackStrength; // target's predicted HP percent is decreased by this amount (0.05 by default)
            public PartyRolesConfig.Assignment DesignatedTank; // who should tank this enemy (MT by default)
            public WPos DesiredPosition; // tank AI will try to move enemy to this position
            public Angle DesiredRotation; // tank AI will try to rotate enemy to this angle

            public Enemy(Actor actor)
            {
                Actor = actor;
                Priority = actor.InCombat ? 0 : -1;
                TimeToKill = 10000;
                AttackStrength = 0.05f;
                DesignatedTank = PartyRolesConfig.Assignment.MT;
                DesiredPosition = actor.Position;
                DesiredRotation = actor.Rotation;
            }
        }

        public static ArenaBounds DefaultBounds = new ArenaBoundsSquare(new(), 30);

        public ArenaBounds Bounds = DefaultBounds;

        // list of potential targets
        public List<Enemy> PotentialTargets = new();
        public int HighestPotentialTargetPriority = 0;

        // positioning: list of areas that are either forbidden to stand in now or will be in near future
        // AI will try to move in such a way to avoid standing in any forbidden zone after its activation or outside of some restricted zone after its activation, even at the cost of uptime
        public List<(AOEShape shape, WPos origin, Angle rot, DateTime activation)> ForbiddenZones = new();

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
            ForbiddenDirections.Clear();
            PredictedDamage.Clear();
        }

        // fill list of potential targets from world state
        public void FillPotentialTargets(WorldState ws)
        {
            foreach (var actor in ws.Actors.Where(a => a.Type == ActorType.Enemy && a.IsTargetable && !a.IsAlly && !a.IsDead))
            {
                PotentialTargets.Add(new(actor));
            }
        }

        public void UpdatePotentialTargets(Action<Enemy> fn)
        {
            foreach (var enemy in PotentialTargets)
                fn(enemy);
        }

        public void AssignPotentialTargetPriorities(Func<Actor, int> map)
        {
            foreach (var enemy in PotentialTargets)
                enemy.Priority = map(enemy.Actor);
        }

        // normalize all entries after gathering data: sort by priority / activation timestamp
        public void Normalize()
        {
            PotentialTargets.SortByReverse(x => x.Priority);
            HighestPotentialTargetPriority = Math.Max(0, PotentialTargets.FirstOrDefault()?.Priority ?? 0);
            ForbiddenZones.SortBy(e => e.activation);
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
