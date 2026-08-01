namespace BossMod.Dawntrail.Foray.CriticalEngagement.ConjuredCalofisteri;

public enum OID : uint
{
    Boss = 0x4BB8,
    Helper = 0x233C,
}

class ConjuredCalofisteriStates : StateMachineBuilder
{
    public ConjuredCalofisteriStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14517)]
public class ConjuredCalofisteri(WorldState ws, Actor primary) : CEModule(ws, primary, new(-215, -65), new ArenaBoundsCircle(22));

