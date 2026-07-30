namespace BossMod.Dawntrail.Foray.CriticalEngagement.Arbatel;

public enum OID : uint
{
    Boss = 0x4BD3,
    Helper = 0x233C,
}

class ArbatelStates : StateMachineBuilder
{
    public ArbatelStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14520)]
public class Arbatel(WorldState ws, Actor primary) : CEModule(ws, primary, new(659, 659), new ArenaBoundsCircle(24.5f));
