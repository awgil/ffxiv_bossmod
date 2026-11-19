namespace BossMod.Stormblood.Alliance.A33DarkCrusader;

public enum OID : uint
{
    Boss = 0x25E3,
    Helper = 0x233C,
}

class A33DarkCrusaderStates : StateMachineBuilder
{
    public A33DarkCrusaderStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

//[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7950)]
//public class A33DarkCrusader(ModuleInit init) : BossModule(init, new(-625.29f, -431.055f), new ArenaBoundsCircle(19.5f));

