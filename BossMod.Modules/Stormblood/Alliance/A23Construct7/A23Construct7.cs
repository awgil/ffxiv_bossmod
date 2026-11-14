namespace BossMod.Stormblood.Alliance.A23Construct7;

public enum OID : uint
{
    Boss = 0x225D,
    Helper = 0x233C,
}

class A23Construct7States : StateMachineBuilder
{
    public A23Construct7States(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000, 10000, "???");
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7237)]
public class A23Construct7(ModuleInit init) : BossModule(init, new(800, -141), new ArenaBoundsSquare(29.5f));

