namespace BossMod.Dawntrail.Foray.CriticalEngagement.CrescentBerserker;

public enum OID : uint
{
    Boss = 0x4703,
    Helper = 0x233C,
}

class CrescentBerserkerStates : StateMachineBuilder
{
    public CrescentBerserkerStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13759)]
public class CrescentBerserker(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));



