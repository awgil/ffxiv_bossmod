namespace BossMod.RealmReborn.Alliance.A13Thanatos;

public enum OID : uint
{
    Boss = 0x92E,
    Helper = 0x233C,
}

class A13ThanatosStates : StateMachineBuilder
{
    public A13ThanatosStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 710)]
public class A13Thanatos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
