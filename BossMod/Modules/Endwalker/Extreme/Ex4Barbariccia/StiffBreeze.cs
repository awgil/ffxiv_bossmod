using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    class StiffBreeze : Components.GenericAOEs
    {
        private static AOEShape _shape = new AOEShapeCircle(1); // note: actual aoe, if triggered, has radius 2, but we care about triggering radius

        public StiffBreeze() : base(ActionID.MakeSpell(AID.Tousle)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return module.Enemies(OID.StiffBreeze).Select(o => new AOEInstance(_shape, o.Position));
        }
    }
}
