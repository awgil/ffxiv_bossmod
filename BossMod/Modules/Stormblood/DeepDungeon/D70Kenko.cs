namespace BossMod.Stormblood.DeepDungeon.D70Kenko;

public enum OID : uint
{
    Boss = 0x23EB,
    Helper = 0x233C,
}

class KenkoStates : StateMachineBuilder
{
    public KenkoStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 546, NameID = 7489)]
public class Kenko(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

