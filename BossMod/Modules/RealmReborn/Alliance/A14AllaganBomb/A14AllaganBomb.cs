namespace BossMod.RealmReborn.Alliance.A14AllaganBomb;

public enum OID : uint
{
    Boss = 0x967,
    Helper = 0x233C,
}

class A14AllaganBombStates : StateMachineBuilder
{
    public A14AllaganBombStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 1873, DevOnly = true)]
public class A14AllaganBomb(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
