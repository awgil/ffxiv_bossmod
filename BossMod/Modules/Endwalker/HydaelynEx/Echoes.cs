using System;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    class Echoes : CommonComponents.FullPartyStack
    {
        public Echoes() : base(ActionID.MakeSpell(AID.EchoesAOE), 6) { }

        public override void OnEventIcon(BossModule module, uint actorID, uint iconID)
        {
            if (iconID == (uint)IconID.Echoes)
            {
                Target = module.WorldState.Actors.Find(actorID);
            }
        }
    }
}
