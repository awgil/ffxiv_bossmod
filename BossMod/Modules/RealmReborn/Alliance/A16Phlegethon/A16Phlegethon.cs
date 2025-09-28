namespace BossMod.RealmReborn.Alliance.A16Phlegethon;

public enum OID : uint
{
    Boss = 0x938,
    Helper = 0x233C,
}

class A16PhlegethonStates : StateMachineBuilder
{
    public A16PhlegethonStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 732)]
public class A16Phlegethon(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

