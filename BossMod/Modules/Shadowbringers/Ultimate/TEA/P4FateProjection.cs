using System;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    // note: fate projection tethers appear before clone actors are spawned, so we're storing id's rather than actors
    class P4FateProjection : BossComponent
    {
        public ulong[] Projections = new ulong[PartyState.MaxPartySize];

        public int ProjectionOwner(ulong proj) => Array.IndexOf(Projections, proj);

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.FateProjection)
            {
                var slot = module.Raid.FindSlot(source.InstanceID);
                if (slot >= 0)
                {
                    Projections[slot] = tether.Target;
                }
            }
        }
    }
}
