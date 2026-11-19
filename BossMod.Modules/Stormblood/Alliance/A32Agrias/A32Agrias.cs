namespace BossMod.Stormblood.Alliance.A32Agrias;

public enum OID : uint
{
    Boss = 0x261F,
    Helper = 0x233C,
}

class A32AgriasStates : StateMachineBuilder
{
    public A32AgriasStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7931)]
public class A32Agrias(ModuleInit init) : BossModule(init, new(600, -54), new ArenaBoundsCircle(29.5f));

