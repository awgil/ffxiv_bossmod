using System;
using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P1WickedWheel : Components.SelfTargetedAOEs
    {
        public P1WickedWheel() : base(ActionID.MakeSpell(AID.WickedWheel), new AOEShapeCircle(8.7f)) { }
    }

    class P1WickedTornado : Components.GenericAOEs
    {
        private DateTime _activation;

        private static AOEShapeDonut _shape = new(7, 20);

        public P1WickedTornado() : base(ActionID.MakeSpell(AID.WickedTornado)) { }

        public override void Init(BossModule module)
        {
            _activation = module.WorldState.CurrentTime.AddSeconds(2.1f);
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            yield return (_shape, module.PrimaryActor.Position, 0.Degrees(), _activation);
        }
    }
}
