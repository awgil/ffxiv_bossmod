namespace BossMod.Stormblood.Alliance.A34UltimaTheHighSeraph;

public enum OID : uint
{
    Boss = 0x2604,
    Helper = 0x233C,
}

class A34UltimaTheHighSeraphStates : StateMachineBuilder
{
    public A34UltimaTheHighSeraphStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

//[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7657)]
//public class A34UltimaTheHighSeraph(ModuleInit init) : BossModule(init, new(600, -600), new ArenaBoundsSquare(29.5f));

