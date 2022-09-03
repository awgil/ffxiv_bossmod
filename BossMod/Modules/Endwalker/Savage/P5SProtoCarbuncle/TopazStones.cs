using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    // TODO: consider using eobjanim's to activate/deactivate stones instead
    class TopazStones : Components.GenericAOEs
    {
        private static AOEShapeRect _shapeRay = new(1, 7.5f, 14);
        private static AOEShapeCircle _shapePoison = new(13);

        public TopazStones() : base(ActionID.MakeSpell(AID.RubyReflection1AOE)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module)
        {
            // TODO: correct explosion time
            foreach (var o in module.Enemies(OID.TopazStoneAny))
            {
                var offset = (o.Position - module.Bounds.Center).Abs();
                var isRay = MathF.Min(offset.X, offset.Z) <= 2; // 'rays' are at +-1 along some axis, poisons are at +-(4,4) or (11,11)
                yield return (isRay ? _shapeRay : _shapePoison, o.Position, o.Rotation, module.WorldState.CurrentTime);
            }
        }
    }
}
