namespace BossMod.Stormblood.Alliance.A24Yiazmat;

public enum OID : uint
{
    Boss = 0x2253,
    Helper = 0x233C,
}

class A24YiazmatStates : StateMachineBuilder
{
    public A24YiazmatStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000, 10000, "???");
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7070)]
public class A24Yiazmat(ModuleInit init) : BossModule(init, new(800, -400), new ArenaBoundsCircle(29.5f));

