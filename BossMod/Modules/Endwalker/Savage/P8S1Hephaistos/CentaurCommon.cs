using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class Footprint : Components.Knockback
    {
        public Footprint() : base(20, ActionID.MakeSpell(AID.Footprint)) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            yield return new(module.PrimaryActor.Position); // TODO: activation
        }
    }
}
