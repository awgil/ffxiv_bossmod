using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    class ParhelicCircle : CommonComponents.CastCounter
    {
        private List<Vector3> _positions = new();

        private static float _triRadius = 8;
        private static float _hexRadius = 17;
        private static AOEShapeCircle _aoeShape = new(6);

        public ParhelicCircle() : base(ActionID.MakeSpell(AID.Incandescence)) { }

        public override void Update(BossModule module)
        {
            if (_positions.Count == 0)
            {
                // there are 10 orbs: 1 in center, 3 in vertices of a triangle with radius=8, 6 in vertices of a hexagon with radius=17
                // note: i'm not sure how exactly orientation is determined, it seems to be related to eventobj rotations...
                var hex = module.Enemies(OID.RefulgenceHexagon).FirstOrDefault();
                var tri = module.Enemies(OID.RefulgenceTriangle).FirstOrDefault();
                if (hex != null && tri != null)
                {
                    var c = module.Arena.WorldCenter;
                    _positions.Add(c);
                    _positions.Add(c + _triRadius * GeometryUtils.DirectionToVec3(tri.Rotation + MathF.PI / 3));
                    _positions.Add(c + _triRadius * GeometryUtils.DirectionToVec3(tri.Rotation + MathF.PI));
                    _positions.Add(c + _triRadius * GeometryUtils.DirectionToVec3(tri.Rotation - MathF.PI / 3));
                    _positions.Add(c + _hexRadius * GeometryUtils.DirectionToVec3(hex.Rotation));
                    _positions.Add(c + _hexRadius * GeometryUtils.DirectionToVec3(hex.Rotation + MathF.PI / 3));
                    _positions.Add(c + _hexRadius * GeometryUtils.DirectionToVec3(hex.Rotation + 2 * MathF.PI / 3));
                    _positions.Add(c + _hexRadius * GeometryUtils.DirectionToVec3(hex.Rotation + MathF.PI));
                    _positions.Add(c + _hexRadius * GeometryUtils.DirectionToVec3(hex.Rotation - 2 * MathF.PI / 3));
                    _positions.Add(c + _hexRadius * GeometryUtils.DirectionToVec3(hex.Rotation - MathF.PI / 3));
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_positions.Any(p => _aoeShape.Check(actor.Position, p, 0)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in _positions)
                _aoeShape.Draw(arena, p, 0);
        }
    }
}
