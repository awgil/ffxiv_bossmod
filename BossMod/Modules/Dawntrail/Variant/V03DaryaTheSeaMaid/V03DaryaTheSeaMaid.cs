namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

public enum OID : uint
{
    Boss = 0x4A94,
    Helper = 0x233C,
}

class V03DaryaTheSeaMaidStates : StateMachineBuilder
{
    public V03DaryaTheSeaMaidStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14291, DevOnly = true)]
public class V03DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375, 530), new ArenaBoundsSquare(20));
