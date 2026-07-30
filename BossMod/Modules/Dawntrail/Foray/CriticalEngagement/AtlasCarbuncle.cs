namespace BossMod.Dawntrail.Foray.CriticalEngagement.AtlasCarbuncle;

public enum OID : uint
{
    Boss = 0x4C4F,
    Helper = 0x233C,
}

class AtlasCarbuncleStates : StateMachineBuilder
{
    public AtlasCarbuncleStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14791)]
public class AtlasCarbuncle(WorldState ws, Actor primary) : CEModule(ws, primary, new(238, 352), new ArenaBoundsSquare(20));
