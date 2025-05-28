namespace BossMod.Dawntrail.Foray.CriticalEngagement.TradeTortoise;

public enum OID : uint
{
    Boss = 0x46E7,
    Helper = 0x233C,
}

class TradeTortoiseStates : StateMachineBuilder
{
    public TradeTortoiseStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13695)]
public class TradeTortoise(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
