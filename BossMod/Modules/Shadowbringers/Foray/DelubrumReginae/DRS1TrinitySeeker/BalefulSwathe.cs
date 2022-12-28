using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker
{
    class BalefulSwathe : Components.GenericAOEs
    {
        private DateTime _activation;
        private static AOEShapeRect _shape = new(50, 50, -5);

        public BalefulSwathe() : base(ActionID.MakeSpell(AID.BalefulSwathe)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            yield return new(_shape, module.PrimaryActor.Position, module.PrimaryActor.Rotation + 90.Degrees(), _activation);
            yield return new(_shape, module.PrimaryActor.Position, module.PrimaryActor.Rotation - 90.Degrees(), _activation);
        }

        public override void Init(BossModule module) => _activation = module.WorldState.CurrentTime.AddSeconds(7.6f); // from verdant path cast start
    }
}
