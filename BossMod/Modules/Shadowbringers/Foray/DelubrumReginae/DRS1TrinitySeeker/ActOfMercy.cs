using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker
{
    class ActOfMercy : Components.GenericAOEs
    {
        private DateTime _activation;
        private static AOEShapeCross _shape = new(50, 4);

        public ActOfMercy() : base(ActionID.MakeSpell(AID.ActOfMercy)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            yield return new(_shape, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
        }

        public override void Init(BossModule module) => _activation = module.WorldState.CurrentTime.AddSeconds(7.6f); // from verdant path cast start
    }
}
