namespace BossMod.Dawntrail.Foray.CriticalEngagement.Metamorph;

public enum OID : uint
{
    Boss = 0x4C77,
    Helper = 0x233C,
}

class MetamorphStates : StateMachineBuilder
{
    public MetamorphStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14801)]
public class Metamorph(WorldState ws, Actor primary) : CEModule(ws, primary, new(500, -310), new ArenaBoundsCircle(25));
