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
            // TODO: split 'pointless to attack' (eg invulnerable, but fine to hit by aoes) vs 'actually bad to hit' (eg can lead to wipe)
            public const int PriorityForbidAI = -1; // ai is forbidden from attacking this enemy, but player explicitly targeting it is not (e.g. out of combat enemies that we might not want to pull)
            public const int PriorityForbidFully = -2; // attacking this enemy is forbidden both by ai or player (e.g. invulnerable, or attacking/killing might lead to a wipe)

            public Actor Actor;
            public int Priority; // <0 means damaging is actually forbidden, 0 is default
            //public float TimeToKill;
            public float AttackStrength; // target's predicted HP percent is decreased by this amount (0.05 by default)
            public WPos DesiredPosition; // tank AI will try to move enemy to this position
            public Angle DesiredRotation; // tank AI will try to rotate enemy to this angle
            public float TankDistance; // enemy will start moving if distance between hitboxes is bigger than this
            public bool ShouldBeTanked; // tank AI will try to tank this enemy
            public bool PreferProvoking; // tank AI will provoke enemy if not targeted
            public bool ForbidDOTs; // if true, dots on target are forbidden
            public bool ShouldBeInterrupted; // if set and enemy is casting interruptible spell, some ranged/tank will try to interrupt
            public bool StayAtLongRange; // if set, players with ranged attacks don't bother coming closer than max range (TODO: reconsider)
            //public bool PointlessToAttack; // if set, we prefer waiting and doing nothing rather than attacking the enemy (e.g. temporarily invincible)

            public Enemy(Actor actor, bool shouldBeTanked)
            {
                Actor = actor;
                Priority = actor.InCombat ? 0 : PriorityForbidAI;
                AttackStrength = 0.05f;
                ShouldBeTanked = shouldBeTanked;
                DesiredPosition = actor.Position;
                DesiredRotation = actor.Rotation;
                TankDistance = 2;
            }
        }

        public static ArenaBounds DefaultBounds = new ArenaBoundsSquare(new(), 30);

        public ArenaBounds Bounds = DefaultBounds;

        // list of potential targets
        public List<Enemy> PotentialTargets = new();
        public int HighestPotentialTargetPriority = 0;

        // forced target
        public Actor? ForcedTarget;

        // positioning: list of shapes that are either forbidden to stand in now or will be in near future
        // AI will try to move in such a way to avoid standing in any forbidden zone after its activation or outside of some restricted zone after its activation, even at the cost of uptime
        public List<(Func<WPos, float> shapeDistance, DateTime activation)> ForbiddenZones = new();

        // positioning: position hint - if set, player will move closer to this position, assuming it is safe and in target's range, without losing uptime
        //public WPos? RecommendedPosition = null;

        // orientation restrictions (e.g. for gaze attacks): a list of forbidden orientation ranges, now or in near future
        // AI will rotate to face allowed orientation at last possible moment, potentially losing uptime
        public List<(Angle center, Angle halfWidth, DateTime activation)> ForbiddenDirections = new();

        // predicted incoming damage (raidwides, tankbusters, etc.)
        // AI will attempt to shield & mitigate
        public List<(BitMask players, DateTime activation)> PredictedDamage = new();

        // planned actions
        // autorotation will execute them in window-end order, if possible
        public List<(ActionID action, Actor target, float windowEnd, bool lowPriority)> PlannedActions = new();

        // buffs to be canceled asap
        public List<(uint statusId, ulong sourceId)> StatusesToCancel = new();

        // clear all stored data
        public void Clear()
        {
            Bounds = DefaultBounds;
            PotentialTargets.Clear();
            ForcedTarget = null;
            ForbiddenZones.Clear();
            ForbiddenDirections.Clear();
            PredictedDamage.Clear();
            PlannedActions.Clear();
            StatusesToCancel.Clear();
        }

        // fill list of potential targets from world state
        public void FillPotentialTargets(WorldState ws, bool playerIsDefaultTank)
        {
            foreach (var actor in ws.Actors.Where(a => a.Type == ActorType.Enemy && a.IsTargetable && !a.IsAlly && !a.IsDead))
            {
                PotentialTargets.Add(new(actor, playerIsDefaultTank));
            }
        }

        // fill forced target, if any
        public void FillForcedTarget(BossModule? module, WorldState ws, Actor player)
        {
            if (module?.PlanExecution != null)
            {
                var oid = module.PlanExecution.ActiveForcedTarget(module.StateMachine);
                if (oid != null)
                {
                    var targets = oid.Value != 0 ? PotentialTargets.Where(e => e.Actor.OID == oid.Value) : PotentialTargets;
                    var maxPrio = targets.MaxBy(t => t.Priority)?.Priority ?? -1;
                    ForcedTarget = maxPrio >= 0 ? targets.Where(e => e.Priority == maxPrio).MinBy(e => (e.Actor.Position - player.Position).LengthSq())?.Actor : null;
                }
            }
        }

        // fill planned actions based on current state
        public void FillPlannedActions(BossModule? module, int slot, Actor player)
        {
            if (module?.PlanExecution != null)
            {
                // TODO: support custom conditions in planner
                foreach (var a in module.PlanExecution.ActiveActions(module.StateMachine))
                {
                    var target = a.Target.Select(module, slot, player);
                    if (target == null)
                        continue;
                    PlannedActions.Add((a.Action, target, a.TimeLeft, a.LowPriority));
                }
            }
        }

        public void AddForbiddenZone(Func<WPos, float> shapeDistance, DateTime activation = new()) => ForbiddenZones.Add((shapeDistance, activation));
        public void AddForbiddenZone(AOEShape shape, WPos origin, Angle rot = new(), DateTime activation = new()) => ForbiddenZones.Add((shape.Distance(origin, rot), activation));

        // normalize all entries after gathering data: sort by priority / activation timestamp
        public void Normalize()
        {
            PotentialTargets.SortByReverse(x => x.Priority);
            HighestPotentialTargetPriority = Math.Max(0, PotentialTargets.FirstOrDefault()?.Priority ?? 0);
            ForbiddenZones.SortBy(e => e.activation);
            ForbiddenDirections.SortBy(e => e.activation);
            PredictedDamage.SortBy(e => e.activation);
            PlannedActions.SortBy(e => e.windowEnd);
        }

        // query utilities
        public IEnumerable<Enemy> PotentialTargetsEnumerable => PotentialTargets;
        public IEnumerable<Enemy> PriorityTargets => PotentialTargets.TakeWhile(e => e.Priority == HighestPotentialTargetPriority);
        public IEnumerable<Enemy> ForbiddenTargets => PotentialTargetsEnumerable.Reverse().TakeWhile(e => e.Priority < 0);

        // TODO: verify how source/target hitboxes are accounted for by various aoe shapes
        public int NumPriorityTargetsInAOE(Func<Enemy, bool> pred) => ForbiddenTargets.Any(pred) ? 0 : PriorityTargets.Count(pred);
        public int NumPriorityTargetsInAOECircle(WPos origin, float radius) => NumPriorityTargetsInAOE(a => a.Actor.Position.InCircle(origin, radius + a.Actor.HitboxRadius));
        public int NumPriorityTargetsInAOECone(WPos origin, float radius, WDir direction, Angle halfAngle) => NumPriorityTargetsInAOE(a => a.Actor.Position.InCircleCone(origin, radius + a.Actor.HitboxRadius, direction, halfAngle));
        public int NumPriorityTargetsInAOERect(WPos origin, WDir direction, float lenFront, float halfWidth, float lenBack = 0) => NumPriorityTargetsInAOE(a => a.Actor.Position.InRect(origin, direction, lenFront + a.Actor.HitboxRadius, lenBack, halfWidth));
    }
}
