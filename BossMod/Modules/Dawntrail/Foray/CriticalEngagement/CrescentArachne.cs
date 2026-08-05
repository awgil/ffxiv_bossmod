namespace BossMod.Dawntrail.Foray.CriticalEngagement.CrescentArachne;

public enum OID : uint
{
    Boss = 0x4DFA,
    Helper = 0x233C,
}

class CrescentArachneStates : StateMachineBuilder
{
    public CrescentArachneStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14840)]
public class CrescentArachne(WorldState ws, Actor primary) : CEModule(ws, primary, new(170, -136), new ArenaBoundsCircle(20));

