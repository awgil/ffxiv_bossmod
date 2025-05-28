namespace BossMod.Dawntrail.Foray.CriticalEngagement.CommandUrn;

public enum OID : uint
{
    Boss = 0x46E1,
    Helper = 0x233C,
}

class CommandUrnStates : StateMachineBuilder
{
    public CommandUrnStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13814)]
public class CommandUrn(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
