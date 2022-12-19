using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P4Freefire : Components.GenericAOEs
    {
        private List<Actor> _casters = new();
        private DateTime _activation;

        private static AOEShape _shape = new AOEShapeCircle(15); // TODO: verify falloff

        public P4Freefire() : base(ActionID.MakeSpell(AID.FreefireIntermission)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _casters.Select(c => new AOEInstance(_shape, c.Position, 0.Degrees(), _activation));
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.Helper && id == 0x0449)
            {
                _casters.Add(actor);
                _activation = module.WorldState.CurrentTime.AddSeconds(5.9f);
            }
        }
    }
}
