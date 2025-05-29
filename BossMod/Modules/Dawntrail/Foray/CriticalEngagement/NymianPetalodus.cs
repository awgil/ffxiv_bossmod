namespace BossMod.Dawntrail.Foray.CriticalEngagement.NymianPetalodus;

public enum OID : uint
{
    Boss = 0x4704,
    Helper = 0x233C,
}

class NymianPetalodusStates : StateMachineBuilder
{
    public NymianPetalodusStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13717)]
public class NymianPetalodus(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
