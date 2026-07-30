namespace BossMod.Dawntrail.Foray.CriticalEngagement.Abductor;

public enum OID : uint
{
    Boss = 0x4BE1,
    Helper = 0x233C,
}

class AbductorStates : StateMachineBuilder
{
    public AbductorStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14505)]
public class Abductor(WorldState ws, Actor primary) : CEModule(ws, primary, new(-150, -860), new ArenaBoundsCircle(24));
