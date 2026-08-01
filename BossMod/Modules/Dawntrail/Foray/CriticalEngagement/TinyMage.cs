namespace BossMod.Dawntrail.Foray.CriticalEngagement.TinyMage;

public enum OID : uint
{
    Boss = 0x4C6D,
    Helper = 0x233C,
}

class TinyMageStates : StateMachineBuilder
{
    public TinyMageStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14795)]
public class TinyMage(WorldState ws, Actor primary) : CEModule(ws, primary, new(152, 716), new ArenaBoundsCircle(20));

