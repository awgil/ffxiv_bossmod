using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2Dahu
{
    class FeralHowl : Components.Knockback
    {
        public FeralHowl() : base(ActionID.MakeSpell(AID.FeralHowlAOE), true) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            // TODO: not all the wall is safe...
            yield return new(module.Bounds.Center, Math.Max(0.1f, module.Bounds.HalfSize - 0.1f - (actor.Position - module.Bounds.Center).Length()));
        }
    }
}
