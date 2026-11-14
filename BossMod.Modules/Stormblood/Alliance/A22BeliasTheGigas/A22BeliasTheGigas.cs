namespace BossMod.Stormblood.Alliance.A22BeliasTheGigas;

public enum OID : uint
{
    Boss = 0x22A4,
    Helper = 0x233C,
}

class A22BeliasTheGigasStates : StateMachineBuilder
{
    public A22BeliasTheGigasStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7223)]
public class A22BeliasTheGigas(ModuleInit init) : BossModule(init, new(-200, -541), new ArenaBoundsSquare(29.5f));

