using BossMod.Endwalker.Ultimate.DSW1;

namespace BossMod.Endwalker.Ultimate.DSW2;

class P4IntermissionBrightwing(BossModule module) : PureOfHeartBrightwing(module) { }
class P4IntermissionSkyblindBait(BossModule module) : PureOfHeartSkyblindBait(module) { }
class P4IntermissionSkyblind(BossModule module) : PureOfHeartSkyblind(module) { }

class P4Haurchefant(BossModule module) : BossComponent(module)
{
    public bool Appear { get; private set; }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Haurchefant && id == 0x11D3)
            Appear = true;
    }
}
