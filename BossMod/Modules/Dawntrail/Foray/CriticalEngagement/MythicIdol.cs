namespace BossMod.Dawntrail.Foray.CriticalEngagement.MythicIdol;

public enum OID : uint
{
    Boss = 0x469B,
    Helper = 0x233C,
}

class MythicIdolStates : StateMachineBuilder
{
    public MythicIdolStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13726, DevOnly = true)]
public class MythicIdol(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

