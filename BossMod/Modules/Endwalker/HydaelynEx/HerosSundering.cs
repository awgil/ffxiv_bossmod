using System;
using System.Linq;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    class HerosSundering : Component
    {
        private Actor? _target;
        private ulong _otherHit;

        private static AOEShapeCone _aoeShape = new(40, MathF.PI / 4);

        public override void Init(BossModule module)
        {
            _target = module.WorldState.Actors.Find(module.PrimaryActor.CastInfo?.TargetID ?? 0);
            if (_target == null)
                Service.Log($"[HydaelynEx] Failed to determine target for heros sundering: {module.PrimaryActor.CastInfo?.TargetID:X}");
        }

        public override void Update(BossModule module)
        {
            _otherHit = 0;
            if (_target != null)
            {
                var dir = GeometryUtils.DirectionFromVec3(_target.Position - module.PrimaryActor.Position);
                _otherHit = module.Raid.WithSlot().Exclude(_target).InShape(_aoeShape, module.PrimaryActor.Position, dir).Mask();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_target == null)
                return;

            if (actor == _target)
            {
                if (_otherHit != 0)
                    hints.Add("Turn boss away from raid!");
            }
            else
            {
                if (BitVector.IsVector64BitSet(_otherHit, slot))
                    hints.Add("GTFO from tankbuster aoe!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_target != null)
                _aoeShape.Draw(arena, module.PrimaryActor.Position, GeometryUtils.DirectionFromVec3(_target.Position - module.PrimaryActor.Position));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (pc == _target)
            {
                foreach (var (slot, player) in module.Raid.WithSlot().Exclude(_target))
                    arena.Actor(player, BitVector.IsVector64BitSet(_otherHit, slot) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
            }
            else
            {
                arena.Actor(_target, arena.ColorDanger);
            }
        }
    }
}
