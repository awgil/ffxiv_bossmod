namespace BossMod.Dawntrail.Foray.CriticalEngagement.PhantomHydra;

public enum OID : uint
{
    Boss = 0x4BC5,
    Helper = 0x233C,
}

class PhantomHydraStates : StateMachineBuilder
{
    public PhantomHydraStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14523)]
public class PhantomHydra(WorldState ws, Actor primary) : CEModule(ws, primary, new(-82, 485), new ArenaBoundsCircle(19.5f));

