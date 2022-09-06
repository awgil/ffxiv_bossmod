using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    class StiffBreeze : Components.GenericAOEs
    {
        private static AOEShape _shape = new AOEShapeCircle(2);

        public StiffBreeze() : base(ActionID.MakeSpell(AID.Tousle)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module)
        {
            return module.Enemies(OID.StiffBreeze).Select(o => (_shape, o.Position, 0.Degrees(), module.WorldState.CurrentTime));
        }
    }
}
