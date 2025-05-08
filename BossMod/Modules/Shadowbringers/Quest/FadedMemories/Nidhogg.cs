namespace BossMod.Shadowbringers.Quest.FadedMemories;

class HighJump(BossModule module) : Components.StandardAOEs(module, AID.HighJump, new AOEShapeCircle(8));
class Geirskogul(BossModule module) : Components.StandardAOEs(module, AID.Geirskogul, new AOEShapeRect(62, 4));

class NidhoggStates : StateMachineBuilder
{
    public NidhoggStates(BossModule module) : base(module)
    {
        TrivialPhase().ActivateOnEnter<HighJump>().ActivateOnEnter<Geirskogul>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 3458, PrimaryActorOID = (uint)OID.Nidhogg)]
public class Nidhogg(WorldState ws, Actor primary) : BossModule(ws, primary, new(-242, 436.5f), new ArenaBoundsCircle(20));
