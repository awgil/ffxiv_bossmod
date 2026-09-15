namespace BossMod.Global.Crucible.PasDeSeul;

public enum OID : uint
{
    Boss = 0x4B90,
    Helper = 0x233C,
}

class PasDeSeulStates : StateMachineBuilder
{
    public PasDeSeulStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14541)]
public class PasDeSeul(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, -420), new ArenaBoundsRect(20, 24));

