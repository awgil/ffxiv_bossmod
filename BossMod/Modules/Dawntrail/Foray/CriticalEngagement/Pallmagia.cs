namespace BossMod.Dawntrail.Foray.CriticalEngagement.Pallmagia;

public enum OID : uint
{
    Boss = 0x4D8F,
    Helper = 0x233C,
}

class PallmagiaStates : StateMachineBuilder
{
    public PallmagiaStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14714)]
public class Pallmagia(WorldState ws, Actor primary) : CEModule(ws, primary, new(807, -562), new ArenaBoundsCircle(20));

