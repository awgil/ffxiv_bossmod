using System;

namespace BossMod.Endwalker.Extreme.Ex1Zodiark
{
    using static BossModule;

    class Styx : CommonComponents.FullPartyStack
    {
        public Styx() : base(ActionID.MakeSpell(AID.StyxAOE), 5) { }

        public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            if (iconID == (uint)IconID.Styx)
            {
                Target = module.WorldState.Actors.Find(actorID);
            }
        }
    }
}
