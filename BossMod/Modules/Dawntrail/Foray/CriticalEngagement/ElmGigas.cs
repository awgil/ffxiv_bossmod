namespace BossMod.Dawntrail.Foray.CriticalEngagement.ElmGigas;

public enum OID : uint
{
    Boss = 0x4BD9,
    Helper = 0x233C,
}

class ElmGigasStates : StateMachineBuilder
{
    public ElmGigasStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14508)]
public class ElmGigas(WorldState ws, Actor primary) : CEModule(ws, primary, new(-390, 700), new ArenaBoundsCircle(29.5f));
