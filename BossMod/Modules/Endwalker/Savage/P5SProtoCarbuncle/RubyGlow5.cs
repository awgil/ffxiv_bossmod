using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    class RubyGlow5 : RubyGlowCommon
    {
        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            // TODO: correct explosion time
            foreach (var o in MagicStones)
                yield return (ShapeQuadrant, QuadrantCenter(module, QuadrantForPosition(module, o.Position)), new(), module.WorldState.CurrentTime);
            foreach (var p in ActivePoisonAOEs(module))
                yield return p;
        }
    }
}
