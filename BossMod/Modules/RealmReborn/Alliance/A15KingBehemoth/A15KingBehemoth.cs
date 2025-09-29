namespace BossMod.RealmReborn.Alliance.A15KingBehemoth;

public enum OID : uint
{
    Boss = 0x932,
    Helper = 0x233C,
}

class A15KingBehemothStates : StateMachineBuilder
{
    public A15KingBehemothStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 727, DevOnly = true)]
public class A15KingBehemoth(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
