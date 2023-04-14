using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P2ApocalypticRay : Components.GenericAOEs
    {
        public Actor? Source { get; private set; }
        private DateTime _activation;

        private AOEShapeCone _shape = new(25.5f, 45.Degrees()); // TODO: verify angle

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (Source != null)
                yield return new(_shape, Source.Position, Source.Rotation, _activation);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ApocalypticRay:
                    Source = caster;
                    _activation = module.WorldState.CurrentTime.AddSeconds(0.6f);
                    break;
                case AID.ApocalypticRayAOE:
                    ++NumCasts;
                    _activation = module.WorldState.CurrentTime.AddSeconds(1.1f);
                    break;
            }
        }
    }
}
