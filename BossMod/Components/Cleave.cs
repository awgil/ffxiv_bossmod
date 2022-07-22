using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component for cleaving autoattacks; shows shape outline and warns when anyone other than main target is inside
    public class Cleave : CastCounter
    {
        public AOEShape Shape { get; private init; }
        public uint EnemyOID { get; private init; }
        private IEnumerable<Actor> _enemies = Enumerable.Empty<Actor>();
        private BitMask _inAOE = new(); // excludes main target

        // enemy OID == 0 means 'primary actor'
        public Cleave(ActionID aid, AOEShape shape, uint enemyOID = 0) : base(aid)
        {
            Shape = shape;
            EnemyOID = enemyOID;
        }

        public override void Init(BossModule module)
        {
            _enemies = EnemyOID != 0 ? module.Enemies(EnemyOID) : Enumerable.Repeat(module.PrimaryActor, 1);
        }

        public override void Update(BossModule module)
        {
            _inAOE = new();
            foreach (var (enemy, target, angle) in EnemiesWithTargets(module))
            {
                _inAOE |= module.Raid.WithSlot().Exclude(target).InShape(Shape, enemy.Position, angle).Mask();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_inAOE[slot])
            {
                hints.Add("GTFO from cleave!");
            }
        }

        public override void UpdateSafeZone(BossModule module, int slot, Actor actor, SafeZone zone)
        {
            foreach (var (enemy, target, angle) in EnemiesWithTargets(module))
            {
                if (actor != target)
                {
                    zone.ForbidZone(Shape, enemy.Position, angle, module.WorldState.CurrentTime, 10000);
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (enemy, target, angle) in EnemiesWithTargets(module))
            {
                Shape.Outline(arena, enemy.Position, angle);
            }
        }

        private IEnumerable<(Actor, Actor, Angle)> EnemiesWithTargets(BossModule module)
        {
            foreach (var enemy in _enemies)
            {
                var target = module.WorldState.Actors.Find(enemy.TargetID);
                if (target != null)
                {
                    yield return (enemy, target, Angle.FromDirection(target.Position - enemy.Position));
                }
            }
        }
    }
}
