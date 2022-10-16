using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class Footprint : Components.KnockbackFromPoints
    {
        public Footprint() : base(20, ActionID.MakeSpell(AID.Footprint)) { }

        public override IEnumerable<WPos> Sources(BossModule module)
        {
            yield return module.PrimaryActor.Position;
        }
    }
}
