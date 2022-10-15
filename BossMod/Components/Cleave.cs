using System;
using System.Collections.Generic;

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
        private BitMask _inAOE = new(); // excludes main target

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

        public override void Update(BossModule module)
        {
            _inAOE = new();
            foreach (var (origin, target, angle) in OriginsAndTargets(module))
            {
                _inAOE |= module.Raid.WithSlot().Exclude(target).InShape(Shape, origin.Position, angle).Mask();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_inAOE[slot])
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
            foreach (var (origin, target, angle) in OriginsAndTargets(module))
            {
                Shape.Outline(arena, origin.Position, angle);
            }
        }

        private IEnumerable<(Actor, Actor, Angle)> OriginsAndTargets(BossModule module)
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
