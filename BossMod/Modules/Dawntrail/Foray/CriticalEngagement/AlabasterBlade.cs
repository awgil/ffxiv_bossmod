namespace BossMod.Dawntrail.Foray.CriticalEngagement.AlabasterBlade;

public enum OID : uint
{
    Boss = 0x4BBE,
    Helper = 0x233C,
}

class AlabasterBladeStates : StateMachineBuilder
{
    public AlabasterBladeStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14509)]
public class AlabasterBlade(WorldState ws, Actor primary) : CEModule(ws, primary, new(-519, -641), new ArenaBoundsCircle(24.5f));

