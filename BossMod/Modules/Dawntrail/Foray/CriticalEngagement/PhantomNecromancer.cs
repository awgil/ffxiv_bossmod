namespace BossMod.Dawntrail.Foray.CriticalEngagement.PhantomNecromancer;

public enum OID : uint
{
    Boss = 0x4BC1,
    Helper = 0x233C,
}

class PhantomNecromancerStates : StateMachineBuilder
{
    public PhantomNecromancerStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14512)]
public class PhantomNecromancer(WorldState ws, Actor primary) : CEModule(ws, primary, new(224, -860), new ArenaBoundsSquare(20));
