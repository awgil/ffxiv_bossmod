using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    class RubyGlow6 : RubyGlowRecolor
    {
        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (CurRecolorState != RecolorState.BeforeStones && MagicStones.Any())
                yield return (ShapeQuadrant, QuadrantCenter(module, AOEQuadrant), 0.Degrees(), module.WorldState.CurrentTime);
            foreach (var p in ActivePoisonAOEs(module))
                yield return p;
        }
    }
}
