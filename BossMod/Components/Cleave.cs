using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component for cleaving autoattacks; shows shape outline and warns when anyone other than main target is inside
    public class Cleave : CastCounter
    {
        public AOEShape Shape { get; private init; }
        public uint EnemyOID { get; private init; }
        public bool ActiveForUntargetable { get; private init; }
        public bool ActiveWhileCasting { get; private init; }
        public bool OriginAtTarget { get; private init; }
        public DateTime NextExpected;
        private List<Actor> _enemies = new();

        // enemy OID == 0 means 'primary actor'
        public Cleave(ActionID aid, AOEShape shape, uint enemyOID = 0, bool activeForUntargetable = false, bool originAtTarget = false, bool activeWhileCasting = true) : base(aid)
        {
            Shape = shape;
            EnemyOID = enemyOID;
            ActiveForUntargetable = activeForUntargetable;
            ActiveWhileCasting = activeWhileCasting;
            OriginAtTarget = originAtTarget;
        }

        public override void Init(BossModule module)
        {
            _enemies = module.Enemies(EnemyOID != 0 ? EnemyOID : module.PrimaryActor.OID);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (OriginsAndTargets(module).Any(e => e.target != actor && Shape.Check(actor.Position, e.origin.Position, e.angle)))
            {
                hints.Add("GTFO from cleave!");
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var (origin, target, angle) in OriginsAndTargets(module))
            {
                if (actor != target)
                {
                    hints.AddForbiddenZone(Shape, origin.Position, angle, NextExpected);
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var e in OriginsAndTargets(module))
            {
                Shape.Outline(arena, e.origin.Position, e.angle);
            }
        }

        private IEnumerable<(Actor origin, Actor target, Angle angle)> OriginsAndTargets(BossModule module)
        {
            foreach (var enemy in _enemies)
            {
                if (enemy.IsDead)
                    continue;

                if (!ActiveForUntargetable && !enemy.IsTargetable)
                    continue;

                if (!ActiveWhileCasting && enemy.CastInfo != null)
                    continue;

                var target = module.WorldState.Actors.Find(enemy.TargetID);
                if (target != null)
                {
                    yield return (OriginAtTarget ? target : enemy, target, Angle.FromDirection(target.Position - enemy.Position));
                }
            }
        }
    }
}
