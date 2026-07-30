namespace BossMod.Dawntrail.Foray.FATE.Machetaur;

public enum OID : uint
{
    Boss = 0x4C26,
    Helper = 0x233C,
}

class MachetaurStates : StateMachineBuilder
{
    public MachetaurStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

//[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14735)]
//public class Machetaur(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
