namespace BossMod.Dawntrail.Foray.CriticalEngagement.OccultKnight;

public enum OID : uint
{
    Boss = 0x471E,
    Helper = 0x233C,
}

class OccultKnightStates : StateMachineBuilder
{
    public OccultKnightStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

// TODO: fix state machine
//[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13728)]
// public class OccultKnight(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
