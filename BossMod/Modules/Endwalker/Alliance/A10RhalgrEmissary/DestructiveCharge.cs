using System.Collections.Generic;

namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary
{
    class DestructiveCharge : Components.GenericAOEs
    {
        public List<AOEInstance> AOEs = new();

        private static AOEShapeCone _shape = new(25, 45.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => AOEs;

        public override void OnEventEnvControl(BossModule module, byte index, uint state)
        {
            if (index != 0x25)
                return;
            // 00020001 = anim start
            // 00080004 = -45/+135
            // 00100004 = +45/-135
            var dir = state switch
            {
                0x00080004 => -45.Degrees(),
                0x00100004 => 45.Degrees(),
                _ => default
            };
            if (dir != default)
            {
                AOEs.Add(new(_shape, module.Bounds.Center, dir, module.WorldState.CurrentTime.AddSeconds(16.1f)));
                AOEs.Add(new(_shape, module.Bounds.Center, dir + 180.Degrees(), module.WorldState.CurrentTime.AddSeconds(16.1f)));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.DestructiveChargeAOE)
            {
                ++NumCasts;
                AOEs.Clear();
            }
        }
    }
}
