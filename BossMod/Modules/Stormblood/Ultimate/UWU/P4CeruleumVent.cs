using System;
using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P4CeruleumVent : Components.GenericAOEs
    {
        private Actor? _source;
        private DateTime _activation;

        private static AOEShapeCircle _shape = new(14);

        public bool Active => _source != null;

        public P4CeruleumVent() : base(ActionID.MakeSpell(AID.CeruleumVent)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_source != null)
                yield return (_shape, _source.Position, _source.Rotation, _activation);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.UltimaWeapon && id == 0x1E43)
            {
                _source = actor;
                _activation = module.WorldState.CurrentTime.AddSeconds(10.1f);
            }
        }
    }
}
