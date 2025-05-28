namespace BossMod.Dawntrail.Foray.CriticalEngagement.LionRampant;

public enum OID : uint
{
    Boss = 0x46DC,
    Helper = 0x233C,
}

class LionRampantStates : StateMachineBuilder
{
    public LionRampantStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13809)]
public class LionRampant(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
