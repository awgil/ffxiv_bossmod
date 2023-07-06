using BossMod.Endwalker.Ultimate.DSW1;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P4IntermissionBrightwing : PureOfHeartBrightwing { }
    class P4IntermissionSkyblindBait : PureOfHeartSkyblindBait { }
    class P4IntermissionSkyblind : PureOfHeartSkyblind { }

    class P4Haurchefant : BossComponent
    {
        public bool Appear { get; private set; }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.Haurchefant && id == 0x11D3)
                Appear = true;
        }
    }
}
